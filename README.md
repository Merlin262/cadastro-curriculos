# Cadastro de Currículos

Aplicação para cadastro e consulta de candidatos, desenvolvida como desafio técnico. Permite
cadastro manual e cadastro a partir de um PDF de currículo (o backend extrai o texto e tenta
identificar nome, e-mail e telefone para pré-preencher o formulário).

Veja também [`DESENVOLVIMENTO.md`](DESENVOLVIMENTO.md) para o relato do processo de
desenvolvimento, decisões técnicas e uso de IA.

## Tecnologias e versões

| Camada | Tecnologia | Versão |
|---|---|---|
| Backend | ASP.NET Core Web API (.NET) | .NET 8.0 |
| Padrão Mediator/CQRS | [LiteMediator.Core](https://www.nuget.org/packages/LiteMediator.Core) / LiteMediator.Extensions.DependencyInjection | 0.1.2 |
| Validação | FluentValidation | 12.1.1 |
| ORM / Banco | Entity Framework Core + SQL Server | EF Core 8.0.11 |
| Leitura de PDF | [PdfPig](https://www.nuget.org/packages/PdfPig) | 0.1.16 |
| Testes backend | xUnit + EF Core InMemory | .NET 8.0 |
| Frontend | Angular (standalone components) | 21.2 |
| UI | Angular Material | 21.2 |
| Testes frontend | Vitest (via `@angular/build:unit-test`) | — |
| Banco de dados | SQL Server | 2019+ (LocalDB também funciona) |

> ⚠️ **Atenção ao instalar pacotes .NET manualmente:** durante o desenvolvimento identificamos que
> o pacote `UglyToad.PdfPig` no NuGet.org **não é o pacote oficial** do projeto PdfPig — é um
> pacote de outro dono (`grinay`) com uma versão `1.7.0-custom-5` propositalmente numerada para
> parecer mais nova que qualquer versão legítima. O pacote oficial é **`PdfPig`** (donos `BobLd`,
> `EliotJones`, `PdfPig`, https://github.com/UglyToad/PdfPig). Este repositório já usa o pacote
> correto; documentamos isso aqui para quem for reproduzir o setup manualmente.

## Estrutura do repositório

```
backend/            Solução .NET (Clean Architecture: Domain/Application/Infrastructure/Api)
  src/
    CadastroCurriculos.Domain          Entidade Candidate (sem dependências externas)
    CadastroCurriculos.Application     Commands/Queries (CQRS via LiteMediator), validação, DTOs
    CadastroCurriculos.Infrastructure  EF Core (SQL Server), leitura de PDF (PdfPig), migrations
    CadastroCurriculos.Api             Controllers, middleware de erros, Program.cs
  tests/
    CadastroCurriculos.Tests           Testes unitários (xUnit)
frontend/            Aplicação Angular (standalone + Angular Material)
samples/
  curriculo-exemplo-joao-silva.pdf     Currículo fictício para testar a importação
```

## Pré-requisitos

- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) e npm
- SQL Server (Developer/Express, LocalDB, ou uma instância acessível) — não incluso no repositório

## Configurando a conexão com o SQL Server

A connection string fica em `backend/src/CadastroCurriculos.Api/appsettings.json`, na chave
`ConnectionStrings:DefaultConnection`. O valor já commitado **não contém credenciais reais** — usa
autenticação integrada do Windows contra uma instância local:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CadastroCurriculosDb;Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False"
}
```

Se você usa SQL Server com usuário/senha (ex.: em Linux/Docker), sobrescreva localmente — **nunca
commite credenciais reais**. A forma mais simples é definir a variável de ambiente (funciona sem
editar nenhum arquivo, inclusive em CI):

```bash
export ConnectionStrings__DefaultConnection="Server=localhost;Database=CadastroCurriculosDb;User Id=sa;Password=SUA_SENHA_AQUI;TrustServerCertificate=True"
```

Ou crie `backend/src/CadastroCurriculos.Api/appsettings.Local.json` (já ignorado pelo git) com o
mesmo formato do `appsettings.json`.

## Criando a estrutura do banco (migrations)

O projeto já inclui a migration inicial (`InitialCreate`) que cria a tabela `Candidates`. Duas
formas de aplicá-la:

**Opção 1 — automática:** basta rodar a API (próxima seção). O `Program.cs` chama
`dbContext.Database.Migrate()` na inicialização, então a primeira execução já cria o banco e a
tabela.

**Opção 2 — manual**, via CLI do EF Core (útil para preparar o banco antes de rodar a API, ou para
rodar em pipelines):

```bash
cd backend
dotnet tool restore
dotnet ef database update --project src/CadastroCurriculos.Infrastructure --startup-project src/CadastroCurriculos.Api
```

## Executando o backend

```bash
cd backend
dotnet restore
dotnet run --project src/CadastroCurriculos.Api
```

A API sobe por padrão em `http://localhost:5044` (ver
`backend/src/CadastroCurriculos.Api/Properties/launchSettings.json`), com Swagger em
`http://localhost:5044/swagger` no ambiente de desenvolvimento.

## Executando o frontend

```bash
cd frontend
npm install
npm start
```

A aplicação sobe em `http://localhost:4200` e já está configurada para chamar a API em
`http://localhost:5044/api` (`frontend/src/environments/environment.ts`). Ajuste esse arquivo se a
API estiver rodando em outra porta/host.

## Rodando os testes

**Backend** (33 testes: parsing de currículo, validadores, pipeline de validação, handlers com EF
Core InMemory):

```bash
cd backend
dotnet test
```

**Frontend** (12 testes: contrato HTTP do `CandidatesService` e validações do formulário/arquivo):

```bash
cd frontend
npm test
```

## Testando a importação de PDF

Use o currículo fictício em [`samples/curriculo-exemplo-joao-silva.pdf`](samples/curriculo-exemplo-joao-silva.pdf)
na tela de novo cadastro ("Enviar currículo em PDF"). Ele foi montado para que nome, e-mail e
telefone apareçam nas primeiras linhas do documento, layout que a heurística de extração
reconhece bem — veja as limitações conhecidas no `DESENVOLVIMENTO.md`.

## Principais endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/candidates` | Lista os candidatos cadastrados |
| `GET` | `/api/candidates/{id}` | Detalhes de um candidato |
| `POST` | `/api/candidates` | Cadastra um candidato (usado pelo fluxo manual e pelo fluxo com PDF) |
| `POST` | `/api/candidates/extract-resume` | Recebe um PDF (`multipart/form-data`, campo `file`) e retorna nome/e-mail/telefone identificados, sem salvar nada |
