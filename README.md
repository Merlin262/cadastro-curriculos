# Cadastro de Currículos

[![CI](https://github.com/Merlin262/cadastro-curriculos/actions/workflows/ci.yml/badge.svg)](https://github.com/Merlin262/cadastro-curriculos/actions/workflows/ci.yml)

Aplicação para cadastro e consulta de candidatos, desenvolvida como desafio técnico. Permite
cadastro manual e cadastro a partir de um PDF de currículo (o backend extrai o texto e tenta
identificar nome, e-mail e telefone para pré-preencher o formulário), com listagem paginada e
pesquisável, aviso (não bloqueante) de e-mail já cadastrado, e download do PDF original a partir
da tela de detalhes.

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
| Orquestração local | Docker Compose | opcional — ver seção dedicada abaixo |

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
    CadastroCurriculos.Domain          Entidade Candidate e ICandidateRepository (sem dependências externas)
    CadastroCurriculos.Application     Commands/Queries (CQRS via LiteMediator), validação, DTOs
    CadastroCurriculos.Infrastructure  CandidateRepository (EF Core/SQL Server), leitura de PDF (PdfPig), migrations
    CadastroCurriculos.Api             Controllers, middleware de erros, Program.cs
  tests/
    CadastroCurriculos.Tests           Testes unitários (xUnit)
frontend/            Aplicação Angular (standalone + Angular Material)
samples/
  curriculo-exemplo-joao-silva.pdf     Currículo fictício para testar a importação
docker-compose.yml    Orquestra SQL Server + backend + frontend (ver seção abaixo)
```

## Rodando tudo com Docker Compose (forma mais rápida)

Não precisa instalar .NET, Node nem SQL Server na máquina — só Docker.

```bash
cp .env.example .env   # opcional: edite a senha do SQL Server antes de continuar
docker compose up --build
```

- Frontend: http://localhost:4200
- API: http://localhost:5044 (Swagger em http://localhost:5044/swagger)
- SQL Server: localhost:1433 (usuário `sa`, senha definida em `.env`)

O backend aplica as migrations automaticamente assim que o SQL Server do container fica saudável
(há um healthcheck + retry para isso — a primeira subida pode levar ~30s a mais enquanto o SQL
Server inicializa). Para derrubar tudo: `docker compose down` (adicione `-v` para também apagar o
volume do banco).

## Pré-requisitos (rodando sem Docker)

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

**Backend** (53 testes: parsing de currículo, validadores, pipeline de validação, repositório e
handlers com EF Core InMemory):

```bash
cd backend
dotnet test
```

**Frontend** (23 testes: contrato HTTP do `CandidatesService`, validações do formulário/arquivo,
aviso de e-mail duplicado, busca/paginação da listagem):

```bash
cd frontend
npm test
```

Os dois suítes também rodam automaticamente a cada `push`/`pull request` via GitHub Actions
(`.github/workflows/ci.yml`) — veja o badge no topo deste README ou a aba *Actions* do repositório.

## Testando a importação de PDF

Use o currículo fictício em [`samples/curriculo-exemplo-joao-silva.pdf`](samples/curriculo-exemplo-joao-silva.pdf)
na tela de novo cadastro ("Enviar currículo em PDF"). Ele foi montado para que nome, e-mail e
telefone apareçam nas primeiras linhas do documento, layout que a heurística de extração
reconhece bem — veja as limitações conhecidas no `DESENVOLVIMENTO.md`.

## Principais endpoints da API

| Método | Rota | Descrição |
|---|---|---|
| `GET` | `/api/candidates?search=&page=&pageSize=` | Lista paginada de candidatos, com busca opcional por nome/e-mail |
| `GET` | `/api/candidates/{id}` | Detalhes de um candidato |
| `POST` | `/api/candidates` | Cadastra um candidato (`multipart/form-data`; usado pelo fluxo manual e pelo fluxo com PDF; aceita um `resumeFile` opcional para guardar o PDF original) |
| `GET` | `/api/candidates/{id}/resume` | Baixa o PDF original do candidato, quando houver um armazenado (404 caso contrário) |
| `GET` | `/api/candidates/check-email?email=` | Verificação leve e não bloqueante de e-mail já cadastrado (usada pelo formulário) |
| `POST` | `/api/candidates/extract-resume` | Recebe um PDF (`multipart/form-data`, campo `file`) e retorna nome/e-mail/telefone identificados, sem salvar nada |
