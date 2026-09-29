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
7. **Documentação.** Por último, escrevi o `README.md` e este `DESENVOLVIMENTO.md`.

Os commits do repositório seguem essa mesma sequência (backend → testes de backend → frontend →
documentação), então o histórico do `git log` reflete a evolução real do trabalho.

## Principais decisões técnicas

- **Clean Architecture (Domain/Application/Infrastructure/Api).** Separei a entidade `Candidate`
  (sem dependências externas) das regras de aplicação (commands/queries) e dos detalhes de
  infraestrutura (EF Core, PdfPig). Isso deixa o CQRS com LiteMediator organizado por caso de uso
  (uma pasta por command/query) e facilita testar a lógica de negócio sem banco de dados real
  (usei EF Core InMemory nos testes de handler).
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

## Como verifiquei se a solução estava correta

- **Backend:** `dotnet build` (sem erros/warnings) e `dotnet test` — 33 testes passando
  (parsing de currículo, validadores, pipeline de validação, handlers com EF Core InMemory).
- **Banco de dados real:** apliquei a migration contra uma instância local do SQL Server
  (`dotnet ef database update`) e conferi o `CREATE TABLE`/índices gerados.
- **API ponta a ponta com `curl`:** cadastro manual, listagem, detalhe por id (incluindo 404 para
  id inexistente), extração de PDF com o currículo fictício de teste (nome/e-mail/telefone
  corretamente identificados), rejeição de arquivo inválido (extensão errada e conteúdo que não é
  PDF de verdade), e erros de validação (campos obrigatórios, e-mail malformado) com a mensagem
  clara devolvida pela API.
- **Frontend:** `ng build` (build de produção sem erros), `ng test` — 12 testes passando
  (contrato HTTP do `CandidatesService`, validação de campos obrigatórios/e-mail, validação de
  arquivo PDF por extensão e tamanho) — e uma sessão manual no navegador cobrindo o fluxo completo
  de cadastro manual (preenchimento → validação de campo obrigatório ao tentar salvar vazio →
  preenchimento correto → salvar → redirecionamento para a tela de detalhes → conferência dos
  dados exibidos → volta para a listagem confirmando o novo registro).

## Tempo aproximado dedicado

O desenvolvimento foi feito em uma única sessão contínua com o Claude Code, cobrindo desde o
levantamento da biblioteca obrigatória até a documentação final. Estimo o equivalente a
**3 a 4 horas** de trabalho, considerando o escopo (duas camadas completas, testes nos dois lados,
uma migration real aplicada e verificada, e validação manual ponta a ponta em vez de apenas
"parece que compila").

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
- **O arquivo PDF em si não é armazenado**, só o nome do arquivo é guardado como metadado de
  origem. Com mais tempo, guardaria o PDF (coluna `varbinary`/blob storage) para permitir
  reabrir/baixar o currículo original a partir da tela de detalhes.
- **Sem paginação/busca na listagem.** Adequado para o volume de um teste técnico; para uso real
  seria necessário paginação, busca por nome/e-mail e filtro por área de interesse.
- **Sem testes end-to-end (Cypress/Playwright).** A cobertura atual é unitária nos dois lados; um
  teste E2E dos dois fluxos de cadastro completos (formulário → API → banco → listagem →
  detalhes) seria o próximo passo natural.
- **Sem pipeline de CI configurado** (ex.: GitHub Actions rodando `dotnet test` e `ng test` a cada
  push) — faria parte de uma entrega para produção.
- **Heurística de telefone assume formato brasileiro.** Funcionaria mal para currículos com
  números de outros países; seria necessário generalizar o regex ou detectar o idioma/localidade
  do documento.
