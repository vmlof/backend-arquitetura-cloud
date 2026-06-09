# GestaoRH BFF

Backend for Frontend em **ASP.NET Core / C#** para a arquitetura distribuida do PJBL.

Este projeto atua como a camada intermediaria oficial entre o microfrontend e os servicos de backend. Ele centraliza contratos HTTP voltados ao frontend, agrega respostas e isola os clientes das mudancas internas dos microservicos.

## Papel do projeto na arquitetura

O BFF e o unico backend consumido pelo frontend. Ele concentra:

- agregacao de dados para a tela principal
- proxy de chamadas de `people`
- proxy de chamadas de `documents`
- integracao com Azure Function
- adaptacao de contratos entre frontend e microservicos

## Endpoints BFF

Os endpoints abaixo ficam expostos sem o prefixo `/api`, porque esse e o formato esperado pelo microfrontend:

| Metodo | Rota | Objetivo |
|---|---|---|
| GET | `/aggregated-data` | Agrega dados de pessoas, documentos e function |
| GET | `/people` | Lista pessoas para o frontend |
| GET | `/people/{id}` | Detalhe de pessoa |
| POST | `/people` | Proxy de criacao |
| PUT | `/people/{id}` | Proxy de atualizacao |
| DELETE | `/people/{id}` | Proxy de exclusao |
| GET | `/documents` | Lista documentos para o frontend |
| GET | `/documents/{id}` | Detalhe de documento |
| POST | `/documents` | Proxy de criacao |
| PUT | `/documents/{id}` | Proxy de atualizacao |
| DELETE | `/documents/{id}` | Proxy de exclusao |

## Estado atual da integracao

Neste momento, o BFF esta preparado para operar com os tres downstreams reais:

- `people`
  - integrado ao Microservico 2 real
  - servico externo em Azure SQL
- `documents`
  - integrado ao Microservico 1 real
  - servico externo em MongoDB Atlas
- `function`
  - integrado ao projeto Azure Function da etapa 5
  - integracao pronta no codigo do BFF
  - validacao em runtime ainda pendente

Isso significa que o endpoint `GET /aggregated-data` esta configurado para compor:

- `people` real
- `documents` real
- `function` real

## Configuracao atual

Configuracao atual em [appsettings.json](/C:/Users/pcesa/OneDrive/PUC-%20BES/6%20Semestre/Arquitetura/backend-arquitetura-cloud/appsettings.json:1):

```json
"DownstreamServices": {
  "UseMocks": false,
  "UsePeopleMocks": false,
  "UseDocumentsMocks": false,
  "UseFunctionMocks": false,
  "PeopleBaseUrl": "http://localhost:5096/api/people/",
  "DocumentsBaseUrl": "http://localhost:5102/api/documents/",
  "FunctionBaseUrl": "http://localhost:7071/",
  "FunctionSummaryPath": "api/enrichment-summary"
}
```

### Significado da configuracao

- `UseMocks = false`
  - desliga o modo totalmente mockado
- `UsePeopleMocks = false`
  - faz o BFF consumir o microservico real de `people`
- `UseDocumentsMocks = false`
  - faz o BFF consumir o microservico real de `documents`
- `UseFunctionMocks = false`
  - faz o BFF consumir a Azure Function real

## Downstreams integrados

### Microservico 1 - Documents

- tecnologia: ASP.NET Core
- banco: MongoDB Atlas
- URL esperada no BFF:
  - `http://localhost:5102/api/documents/`

### Microservico 2 - People

- tecnologia: ASP.NET Core
- banco: Azure SQL Database
- URL esperada no BFF:
  - `http://localhost:5096/api/people/`

### Azure Function - Enrichment Summary

- tecnologia: Azure Functions .NET 8 isolated
- rota esperada no BFF:
  - `http://localhost:7071/api/enrichment-summary`

URL de publicacao criada no Azure:

- `https://func-pjbl-arquitetura-dpeye5c5cyd9gthe.centralus-01.azurewebsites.net`

Quando a Function for publicada e validada, a URL local pode ser trocada pela URL publicada.

## Estrutura arquitetural relevante

```text
src
|-- API
|   |-- Controllers
|   |   |-- AggregatedDataController.cs
|   |   |-- PeopleController.cs
|   |   `-- DocumentsBffController.cs
|-- Application
|   |-- Common
|   |   `-- Interfaces
|   `-- Features
|       `-- Bff
|           |-- AggregatedData
|           |-- People
|           `-- Documents
|-- Infrastructure
|   |-- Clients
|   |   |-- PeopleBffClient.cs
|   |   |-- DocumentsBffClient.cs
|   |   `-- FunctionBffClient.cs
|   `-- Configuration
|       `-- DownstreamServicesOptions.cs
```

## Padroes aplicados

- Clean Architecture
  - `API`
  - `Application`
  - `Infrastructure`
  - `Domain`
- Vertical Slice
  - slices separados para agregacao, people e documents
- BFF
  - endpoints orientados ao frontend
  - composicao de respostas
  - proxy para servicos downstream

## Como rodar localmente

### Pre-requisitos

- .NET SDK 9
- Microservico `Documents` rodando
- Microservico `People` rodando
- Azure Function rodando localmente em `http://localhost:7071/`

### Execucao

```bash
dotnet restore
dotnet run
```

Documentacao OpenAPI:

- JSON: `http://localhost:5000/openapi/v1.json`
- Scalar: `http://localhost:5000/scalar`

## Fluxo atual da solucao

1. o frontend chama o BFF
2. o BFF consulta `people` no microservico SQL
3. o BFF consulta `documents` no microservico Mongo
4. o BFF consulta a Azure Function
5. o frontend recebe um contrato unificado

## Observacoes importantes para a entrega

- Este projeto representa o **BFF** da solucao.
- O frontend continua sem falar diretamente com microservicos.
- O contrato de `documents` no BFF foi ajustado para `id` string, por causa do MongoDB.
- O contrato de `people` continua com `id` inteiro, conforme o microservico SQL.
- A integracao da Azure Function esta pronta no codigo e na configuracao do BFF.
- A validacao da Azure Function em runtime ainda precisa ser confirmada em ambiente com Azure Functions Core Tools.

## Alunos

Preencher com os nomes do grupo.
