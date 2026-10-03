# Relato de desenvolvimento

## Como organizei e executei o trabalho

1. **Backend primeiro.** Modelei o domínio (`Candidate`), depois as camadas de Application
   (commands/queries CQRS com LiteMediator + FluentValidation), Infrastructure (EF Core + SQL
   Server, leitura de PDF) e Api (controllers, middleware de erros). Criei a migration inicial e
   validei contra uma instância real do SQL Server local.
2. **Testes de backend.** Escrevi os testes unitários junto com a implementação de cada camada
   (parsing de currículo, validadores, pipeline de validação, handlers com EF Core InMemory).
3. **Validação manual ponta a ponta.** Subi a API e testei todos os endpoints com `curl`,
   incluindo o upload do currículo fictício em PDF, antes de considerar o backend pronto.
4. **Frontend.** Scaffoldei a aplicação Angular, construí a listagem, o formulário (compartilhado
   entre cadastro manual e cadastro via PDF) e a tela de detalhes, e integrei com a API.
5. **Validação manual do frontend.** Rodei a aplicação em um navegador e exerci os fluxos
   principais (cadastro manual completo, navegação para detalhes, validação de
   campos obrigatórios).
7. **Docker Compose** Adicionei Docker Compose (SQL Server + backend + frontend).
8. **Documentação.** Escrevi o `README.md` e este `DESENVOLVIMENTO.md`.

## Principais decisões técnicas

- **Clean Architecture (Domain/Application/Infrastructure/Api).** Separei a entidade `Candidate`
  (sem dependências externas) das regras de aplicação (commands/queries) e dos detalhes de
  infraestrutura (EF Core, PdfPig). Isso deixa o CQRS com LiteMediator (minha biblioteca) organizado por caso de uso
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
- **Busca com `Contains` (vira `LIKE '%termo%'` no SQL Server), sem exigir correspondência exata.**
  Simples e cobre o caso de uso (encontrar por parte do nome ou e-mail); não tratei caracteres
  curinga do LIKE (`%`, `_`) digitados pelo usuário como literais, o que é uma limitação aceitável
  para uma busca informal deste tipo.

## Ferramentas de IA utilizadas

- **Claude Code** (Anthropic), modelo **Claude Sonnet 5**, usado como par de desenvolvimento
  — leitura e escrita de código, execução de comandos (`dotnet`, `ng`,
  `curl`) e redação desta documentação.

## Em quais etapas a IA ajudou (com exemplos)

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

## O que precisei corrigir, adaptar ou descartar

- **Extração de texto por linha em vez de `Page.Text`**, conforme descrito acima — o primeiro
  teste manual contra um PDF real expôs o problema, que não apareceria só lendo a documentação.

## Tempo aproximado dedicado

O desenvolvimento foi feito com o Claude Code em duas sessões contínuas: a primeira entrega
(arquitetura, cadastro manual/PDF, listagem, detalhes, Repository pattern) levou o equivalente a
**3 a 4 horas**; os quatro extras (Docker Compose, download de PDF, paginação/busca, aviso de
e-mail duplicado) mais **1 a 2 horas**, incluindo escrever/ajustar os testes novos e validar cada
um manualmente antes de seguir para o próximo. Total aproximado: **4,5 a 6 horas**.

## Dificuldades, limitações e melhorias com mais tempo

- **PDFs digitalizados (imagem escaneada) não são suportados.** Não há OCR; um PDF sem texto
  selecionável retorna todos os campos vazios com um aviso claro, e o cadastro manual continua
  funcionando normalmente.
- **Sem testes end-to-end (Cypress/Playwright).** A cobertura atual é unitária nos dois lados; um
  teste E2E dos dois fluxos de cadastro completos (formulário → API → banco → listagem →
  detalhes) seria o próximo passo natural.
- **CI não roda o `docker compose build`** — só `dotnet test`/`ng test` e os respectivos builds.
  Adicionar um terceiro job de sanidade do Docker seria natural com mais tempo.
