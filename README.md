# GestaoRH BFF

Backend for Frontend em **ASP.NET Core / C#** para a entrega do item 2 do PJBL de Arquitetura.

Este projeto foi adaptado para atuar como a camada intermediaria entre o microfrontend e os servicos de backend. Nesta etapa ele ja entrega a estrutura de **BFF**, com endpoints de agregacao e proxy, mantendo a base existente de **Clean Architecture** e **Vertical Slice**.

## Papel do projeto na arquitetura

O BFF e o unico backend consumido pelo frontend. Ele concentra:

- agregacao de dados para a tela principal
- proxy de chamadas de `people`
- proxy de chamadas de `documents`
- ponto de integracao com Azure Function
- isolamento do frontend em relacao aos microservicos reais

## Endpoints BFF desta etapa

Os endpoints abaixo ficam expostos sem o prefixo `/api`, porque sao os contratos esperados pelo microfrontend:

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

## Como a integracao esta preparada

Nesta fase o BFF roda em dois modos:

- `UseMocks = true`
  - responde com dados mockados
  - permite demonstrar o BFF antes dos microservicos existirem
- `UseMocks = false`
  - passa a usar `HttpClient`
  - encaminha chamadas para os servicos configurados em `DownstreamServices`

Configuracao atual em [appsettings.json](/C:/Users/pcesa/OneDrive/PUC-%20BES/6%20Semestre/Arquitetura/backend-arquitetura-cloud/appsettings.json:1):

```json
"DownstreamServices": {
  "UseMocks": true,
  "PeopleBaseUrl": "http://localhost:5101/",
  "DocumentsBaseUrl": "http://localhost:5102/",
  "FunctionBaseUrl": "http://localhost:7071/",
  "FunctionSummaryPath": "api/enrichment-summary"
}
```

Quando os microservicos e a Azure Function estiverem prontos, basta:

1. alterar `UseMocks` para `false`
2. apontar as URLs corretas
3. alinhar os contratos dos payloads de `people` e `documents`

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

### Execucao

```bash
dotnet restore
dotnet run
```

Documentacao OpenAPI:

- JSON: `http://localhost:5000/openapi/v1.json`
- Scalar: `http://localhost:5000/scalar`

## Observacoes importantes para a entrega

- Este projeto agora representa o **BFF** da solucao.
- A conexao real com os microservicos sera feita nas proximas etapas.
- A parte de Azure Function aqui ja esta preparada por configuracao e cliente dedicado.
- A API antiga do projeto ainda existe no repositorio, mas o foco academico desta etapa passa a ser o contrato BFF acima.

## Alunos

Preencher com os nomes do grupo.
