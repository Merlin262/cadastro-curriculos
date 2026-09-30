# Relato de desenvolvimento

## Como organizei e executei o trabalho

Desenvolvi o projeto em uma sessão única e contínua, com o **Claude Code** (Anthropic) como par de
desenvolvimento, na seguinte ordem:

1. **Levantamento da biblioteca obrigatória.** Antes de escrever qualquer código, investiguei a
   página do NuGet e o repositório do `LiteMediator.Core` para entender a API (interfaces
   `IRequest`/`IRequestHandler`, `ISender`/`IPublisher`, `IPipelineBehavior`, registro via
   `AddLiteMediator`). Como a documentação pública é enxuta, confirmei as assinaturas exatas
   refletindo sobre o `.dll` instalado (veja a seção de IA abaixo) para não escrever código
   baseado em suposições erradas.
2. **Backend primeiro.** Modelei o domínio (`Candidate`), depois as camadas de Application
   (commands/queries CQRS com LiteMediator + FluentValidation), Infrastructure (EF Core + SQL
   Server, leitura de PDF) e Api (controllers, middleware de erros). Criei a migration inicial e
   validei contra uma instância real do SQL Server local.
3. **Testes de backend.** Escrevi os testes unitários junto com a implementação de cada camada
   (parsing de currículo, validadores, pipeline de validação, handlers com EF Core InMemory).
4. **Validação manual ponta a ponta.** Subi a API e testei todos os endpoints com `curl`,
   incluindo o upload do currículo fictício em PDF, antes de considerar o backend pronto.
5. **Frontend.** Scaffoldei a aplicação Angular, construí a listagem, o formulário (compartilhado
   entre cadastro manual e cadastro via PDF) e a tela de detalhes, e integrei com a API.
6. **Validação manual do frontend.** Rodei a aplicação em um navegador controlado pela ferramenta
   e exerci os fluxos principais (cadastro manual completo, navegação para detalhes, validação de
   campos obrigatórios) antes de escrever os testes automatizados do Angular.
7. **Documentação.** Escrevi o `README.md` e este `DESENVOLVIMENTO.md` com o estado do projeto até
   ali (cadastro manual/PDF, listagem, detalhes, Repository pattern).
8. **Segunda iteração: quatro extras.** Depois da entrega inicial, adicionei Docker Compose
   (SQL Server + backend + frontend), armazenamento/download do PDF original, paginação e busca na
   listagem, e aviso não bloqueante de e-mail duplicado — cada um already listado como limitação
   ou melhoria futura na primeira versão deste documento. Segui a mesma disciplina: mudança no
   backend → migration → teste automatizado → validação manual com `curl` → mudança no frontend →
   validação manual no navegador, um extra de cada vez.

Os commits do repositório seguem essa mesma sequência (backend → testes de backend → frontend →
documentação → Repository pattern → os quatro extras), então o histórico do `git log` reflete a
evolução real do trabalho.

## Principais decisões técnicas

- **Clean Architecture (Domain/Application/Infrastructure/Api).** Separei a entidade `Candidate`
  (sem dependências externas) das regras de aplicação (commands/queries) e dos detalhes de
  infraestrutura (EF Core, PdfPig). Isso deixa o CQRS com LiteMediator organizado por caso de uso
  (uma pasta por command/query) e facilita testar a lógica de negócio sem banco de dados real
  (usei EF Core InMemory nos testes de handler).
- **Repository pattern entre Application e Infrastructure.** Os handlers dependem só de
  `ICandidateRepository` (definida no Domain), implementada por `CandidateRepository` no
  Infrastructure com EF Core. Antes disso, os handlers recebiam um `IApplicationDbContext` que
  expunha `DbSet<Candidate>` diretamente — funcionava, mas vazava um detalhe do EF Core (o
  `DbSet`) para a camada de Application. Com o repositório, a Application não referencia mais o
  pacote do EF Core: só conhece a entidade de domínio e o contrato do repositório
  (`AddAsync`/`GetByIdAsync`/`GetAllAsync`/`SaveChangesAsync`). Testei o repositório isoladamente
  (`CandidateRepositoryTests`, com EF Core InMemory) além de continuar testando os handlers.
- **Mesmo endpoint e mesma validação para os dois fluxos de cadastro.** `POST /api/candidates`
  é usado tanto pelo cadastro manual quanto pelo cadastro via PDF — depois de extrair os dados do
  PDF, o frontend só pré-preenche o mesmo formulário, que o usuário revisa e envia do mesmo jeito.
  Isso evita duplicar regras de validação (que ficam centralizadas em
  `CreateCandidateCommandValidator`, aplicadas por um `IPipelineBehavior` do LiteMediator) e
  atende diretamente ao requisito do desafio.
- **Extração de PDF nunca bloqueia o cadastro manual.** O endpoint `extract-resume` não persiste
  nada — ele só retorna sugestões de campos e avisos (`warnings`). Se o PDF não puder ser lido
  (corrompido, protegido, escaneado sem texto), o handler captura a exceção e devolve uma
  mensagem amigável em vez de lançar erro, exatamente para que uma falha na leitura nunca impeça
  o preenchimento manual do formulário.
- **Extração por heurística (regex + posição do texto), não IA/NLP.** Optei por uma abordagem
  simples e determinística: regex para e-mail e telefone, e uma heurística para nome (primeiras
  linhas do documento, sem dígitos/@, 2 a 6 palavras, sem palavras de cabeçalho de seção como
  "Objetivo"/"Currículo"). É previsível, fácil de testar unitariamente e rápida — mas tem
  limitações claras, documentadas na seção abaixo.
- **PdfPig para leitura do PDF, com extração de texto por linha.** O `Page.Text` padrão do PdfPig
  concatena todas as palavras da página sem preservar quebras de linha (ver seção de IA), o que
  quebrava a heurística de nome/e-mail. Implementei `ExtractPageTextByLine` agrupando as palavras
  pela posição vertical (`BoundingBox.Top`) para reconstruir as linhas visuais do documento antes
  de aplicar a heurística.
- **Angular standalone + signals + Angular Material.** Sem NgModules, com `signal()` para estado
  local dos componentes (loading, erros, resultado da extração) e Angular Material para uma UI
  funcional sem gastar tempo com CSS customizado — adequado ao escopo do desafio.
- **Migrations do EF Core aplicadas automaticamente no startup** (`Database.Migrate()` em
  `Program.cs`), além de também poderem ser aplicadas manualmente via `dotnet ef database
  update` — reduz o atrito para quem for rodar o projeto pela primeira vez.
- **PDF original guardado como `varbinary(max)` na própria tabela `Candidates`**, não em blob
  storage separado. Como o arquivo já é limitado a 5 MB pela validação, guardar no banco evita
  precisar de um segundo serviço/volume só para isso — simples o suficiente para o escopo, e o
  endpoint de detalhes (`CandidateDto`) nunca inclui os bytes (só um `hasResumeFile: bool`), então
  listar/consultar candidatos não fica pesado; só o endpoint dedicado `GET /{id}/resume` lê o
  conteúdo binário.
- **Reenvio do arquivo no `POST /api/candidates` em vez de cache temporário no servidor.** O fluxo
  de PDF já chama `extract-resume` uma vez para pré-preencher o formulário; para salvar, o mesmo
  arquivo é reenviado junto com os campos de texto (`multipart/form-data`). Cogitei cachear o
  arquivo no backend entre a extração e o salvamento (evitando o re-upload), mas isso exigiria
  armazenamento temporário com expiração/limpeza — complexidade desproporcional para um arquivo de
  até 5 MB. Reenviar é mais simples e sem estado.
- **Busca com `Contains` (vira `LIKE '%termo%'` no SQL Server), sem exigir correspondência exata.**
  Simples e cobre o caso de uso (encontrar por parte do nome ou e-mail); não tratei caracteres
  curinga do LIKE (`%`, `_`) digitados pelo usuário como literais, o que é uma limitação aceitável
  para uma busca informal deste tipo.
- **Aviso de e-mail duplicado é só um aviso.** O endpoint `GET /check-email` é consultado quando o
  campo de e-mail perde o foco no formulário; se já existir, mostra uma mensagem amarela mas não
  desabilita o botão de salvar nem adiciona erro de validação — o enunciado não pede unicidade de
  e-mail, então bloquear seria inventar uma regra de negócio que não foi pedida.
- **Docker Compose com healthcheck + retry, não só `depends_on`.** O container do SQL Server aceita
  conexões na porta antes de estar pronto para autenticar; um `depends_on` simples faria o backend
  às vezes falhar na primeira tentativa de migration. Resolvi nas duas pontas: um healthcheck no
  `docker-compose.yml` (via `sqlcmd`) e um laço de retry com espera de 5s no próprio `Program.cs`
  (útil também fora do Docker, se o SQL Server demorar para responder).

## Ferramentas de IA utilizadas

- **Claude Code** (Anthropic), modelo **Claude Sonnet 5**, usado como par de desenvolvimento
  durante toda a sessão — leitura e escrita de código, execução de comandos (`dotnet`, `ng`,
  `curl`), testes manuais via navegador controlado, e redação desta documentação.

## Em quais etapas a IA ajudou (com exemplos)

- **Confirmar a API real do LiteMediator antes de codificar.** Pedido: usar a biblioteca
  `LiteMediator.Core` para CQRS. Como a página do NuGet e o README do GitHub mostram só exemplos
  de alto nível, criei um pequeno projeto console descartável que referenciava o pacote e usava
  reflection para listar todos os tipos/membros públicos do `.dll` instalado. Isso revelou as
  assinaturas exatas (`IRequestHandler<TRequest,TResponse>.Handle(...)`,
  `IPipelineBehavior<TRequest,TResponse>.Handle(request, RequestHandlerDelegate<TResponse> next,
  ct)`, `ISender.Send(...)`, `MediatorConfiguration.RegisterServicesFromAssembly(...)`) antes de
  escrever qualquer command/handler — evitou retrabalho por assinaturas erradas.
- **Geração do CQRS boilerplate.** Cada command/query (`CreateCandidateCommand`,
  `ExtractResumeDataCommand`, `GetCandidatesQuery`, `GetCandidateByIdQuery`) segue o mesmo padrão
  (record + validator + handler); escrevi o primeiro caso com cuidado e reaproveitei o padrão para
  os demais.
- **Construção e depuração da extração de PDF.** Depois de gerar um currículo fictício de teste e
  chamar o endpoint `extract-resume`, o e-mail extraído veio grudado no fim do nome
  (`"...SantosJoao.silva.santos@example.com"`). Investigando com um script de reflection/teste
  direto contra o PdfPig, descobri que `Page.Text` não insere separador confiável entre linhas
  diferentes do PDF. Corrigi reimplementando a extração a partir de `page.GetWords()`, agrupando
  por posição vertical para reconstruir as linhas — e adicionei um teste que cobre esse cenário.
- **Scaffold do frontend Angular e integração com Angular Material**, incluindo a configuração de
  `provideHttpClient`/`provideAnimationsAsync` que os schematics do `ng add` não geram sozinhos
  para uma app standalone.
- **Escrita dos testes automatizados**, tanto backend (xUnit) quanto frontend (Vitest), cobrindo
  os casos de sucesso e de borda de cada regra de validação e de cada handler.

## O que precisei corrigir, adaptar ou descartar

- **Pacote NuGet `UglyToad.PdfPig` era um pacote diferente do oficial.** Ao instalar a
  dependência de leitura de PDF, o `dotnet add package UglyToad.PdfPig` resolveu a versão
  `1.7.0-custom-5`, de um dono (`grinay`) diferente do mantenedor real do projeto PdfPig. Esse
  número de versão (major `1.7.0`, sufixo `-custom-5`) foi deliberadamente escolhido para parecer
  "mais novo" que qualquer versão legítima do pacote oficial e assim ser resolvido primeiro por
  ferramentas de dependência — um padrão típico de *dependency confusion*. Antes de usar a
  biblioteca, confirmei via NuGet API e via o repositório oficial no GitHub
  (`github.com/UglyToad/PdfPig`) que o pacote correto se chama simplesmente **`PdfPig`**
  (donos `BobLd`, `EliotJones`, `PdfPig`), removi o pacote errado e instalei o correto na versão
  estável `0.1.16`. Isso está documentado no `README.md` para quem for reproduzir o setup.
- **Extração de texto por linha em vez de `Page.Text`**, conforme descrito acima — o primeiro
  teste manual contra um PDF real expôs o problema, que não apareceria só lendo a documentação.
- **Um caso de teste do validador de e-mail estava errado, não o validador.** Um teste esperava
  que `"faltando@dominio"` fosse rejeitado pelo `EmailAddress()` do FluentValidation, mas esse
  validador aceita um domínio sem TLD (é um hostname válido). Corrigi o caso de teste para um
  e-mail genuinamente inválido (`"sem-arroba.com"`, sem `@`) em vez de alterar a regra de negócio.
- **Local do manifesto de ferramentas do .NET.** `dotnet tool install --local dotnet-ef` criou o
  arquivo em `backend/dotnet-tools.json` em vez do caminho convencional
  `backend/.config/dotnet-tools.json`, que é o que `dotnet tool restore` procura por padrão.
  Movi o arquivo para o local correto.
- **`npm ci` não funcionava dentro do container do frontend.** O `package-lock.json` (gerado no
  Windows) estava sem duas dependências opcionais que o `@napi-rs/wasm-runtime` só resolve em
  Linux (`@emnapi/core`, `@emnapi/runtime`), então `npm ci` (que exige fidelidade total ao lock)
  falhava no Alpine do Dockerfile. Troquei por `npm install` nesse estágio do build, que resolve as
  dependências para a plataforma atual em vez de exigir o lock exato.

## Como verifiquei se a solução estava correta

- **Backend:** `dotnet build` (sem erros/warnings) e `dotnet test` — 53 testes passando (parsing de
  currículo, validadores, pipeline de validação, repositório e handlers com EF Core InMemory,
  incluindo paginação/busca e verificação de e-mail duplicado).
- **Banco de dados real:** apliquei as migrations (`InitialCreate` e `AddResumeFileContent`)
  contra uma instância local do SQL Server (`dotnet ef database update`) e conferi o
  `CREATE TABLE`/`ALTER TABLE`/índices gerados.
- **API ponta a ponta com `curl`:** cadastro manual, listagem paginada, busca por nome/e-mail,
  detalhe por id (incluindo 404 para id inexistente), extração de PDF com o currículo fictício de
  teste (nome/e-mail/telefone corretamente identificados), cadastro com PDF anexado seguido de
  download do arquivo salvo (bytes conferidos com `file` no arquivo baixado), verificação de
  e-mail duplicado (`check-email`), rejeição de arquivo inválido (extensão errada e conteúdo que
  não é PDF de verdade), e erros de validação (campos obrigatórios, e-mail malformado) com a
  mensagem clara devolvida pela API.
- **Frontend:** `ng build` (build de produção sem erros), `ng test` — 23 testes passando (contrato
  HTTP do `CandidatesService` incluindo paginação/busca/download, validação de
  campos/arquivo, aviso de e-mail duplicado com debounce de busca testado via fake timers do
  Vitest) — e sessões manuais no navegador cobrindo: cadastro manual completo, busca e paginação na
  listagem, aviso de e-mail duplicado ao sair do campo, e download do PDF a partir da tela de
  detalhes (conferido pela requisição de rede retornando 200).
- **Docker Compose:** `docker compose up --build` de ponta a ponta, com o `.env` de exemplo, numa
  máquina sem `dotnet`/`node`/SQL Server rodando fora de container. Confirmei pelos logs que o
  backend esperou o healthcheck do SQL Server e aplicou as duas migrations sozinho num banco novo,
  e testei os três pontos: API respondendo direto em `localhost:5044`, frontend servido pelo nginx
  em `localhost:4200`, e o proxy `/api` do nginx para o backend funcionando (cadastrei um candidato
  via `curl` batendo em `localhost:4200/api/candidates` e ele apareceu na listagem renderizada pelo
  Angular). Na primeira tentativa o `npm ci` do Dockerfile do frontend falhou porque o
  `package-lock.json` (gerado no Windows) não tinha duas dependências opcionais de
  `@napi-rs/wasm-runtime` que só resolvem no Linux; troquei por `npm install` nesse estágio do
  build (ver próxima seção).

## Tempo aproximado dedicado

O desenvolvimento foi feito com o Claude Code em duas sessões contínuas: a primeira entrega
(arquitetura, cadastro manual/PDF, listagem, detalhes, Repository pattern) levou o equivalente a
**3 a 4 horas**; os quatro extras (Docker Compose, download de PDF, paginação/busca, aviso de
e-mail duplicado) mais **1 a 2 horas**, incluindo escrever/ajustar os testes novos e validar cada
um manualmente antes de seguir para o próximo. Total aproximado: **4,5 a 6 horas**.

## Dificuldades, limitações e melhorias com mais tempo

- **A extração de nome é a mais frágil das três.** E-mail e telefone são bem identificados por
  regex na maioria dos layouts. Nome depende de heurística posicional (linhas no topo do
  documento, sem dígitos/@, 2–6 palavras, sem palavras de cabeçalho de seção) e falha em layouts
  criativos, currículos em múltiplas colunas, nomes que não estão nas primeiras linhas, ou nomes
  compostos por poucas/muitas palavras fora da faixa esperada. Por isso o formulário sempre
  permite completar/corrigir manualmente — a extração é uma sugestão, nunca uma obrigação.
- **PDFs digitalizados (imagem escaneada) não são suportados.** Não há OCR; um PDF sem texto
  selecionável retorna todos os campos vazios com um aviso claro, e o cadastro manual continua
  funcionando normalmente.
- **Sem filtro por área de interesse na listagem** (só busca por nome/e-mail). Simples de
  adicionar (`Where` a mais no repositório) se precisasse.
- **Sem testes end-to-end (Cypress/Playwright).** A cobertura atual é unitária nos dois lados; um
  teste E2E dos dois fluxos de cadastro completos (formulário → API → banco → listagem →
  detalhes) seria o próximo passo natural.
- **Sem pipeline de CI configurado** (ex.: GitHub Actions rodando `dotnet test` e `ng test` a cada
  push, e talvez também um `docker compose build` de sanidade) — faria parte de uma entrega para
  produção.
- **Heurística de telefone assume formato brasileiro.** Funcionaria mal para currículos com
  números de outros países; seria necessário generalizar o regex ou detectar o idioma/localidade
  do documento.
- **A senha do SQL Server no Docker Compose usa um valor padrão de exemplo** (`TrocarSenha123!`)
  se a pessoa não criar o próprio `.env` — funcional para avaliação local, mas o README deixa claro
  que não deve ser usada além disso.
