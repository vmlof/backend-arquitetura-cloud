# Contexto da Etapa 2 - BFF

Este arquivo registra o contexto completo do que foi feito no item 2 da atividade e as evolucoes posteriores dessa camada, para manter continuidade nas proximas conversas e nas proximas entregas.

## Objetivo desta etapa

Transformar o backend atual em uma base de **BFF (Backend for Frontend)** alinhada com o PJBL da disciplina, preservando o uso de **C# / ASP.NET Core** e reaproveitando a estrutura arquitetural que ja existia no projeto.

A meta inicial desta etapa foi atender especificamente o item:

`2. BFF (Backend for Frontend)`

Requisitos principais considerados:

- backend especializado para o frontend
- agregacao de dados
- proxy de requisicoes CRUD
- ponto de comunicacao com microservicos
- ponto de comunicacao com Azure Function
- compatibilidade com a arquitetura ja adotada no projeto

## Decisao arquitetural adotada

Embora o enunciado cite Node.js como sugestao, foi mantida a stack em **C# com ASP.NET Core**, porque:

- o projeto ja estava nessa stack
- a base ja tinha Clean Architecture e Vertical Slice
- a troca de linguagem geraria retrabalho sem ganho academico real
- o conceito de BFF nao depende de Node.js, e sim do papel arquitetural da aplicacao

Com isso, a decisao foi:

- manter o backend em C#
- reposicionar o projeto como **BFF da solucao**
- expor endpoints orientados ao frontend
- desacoplar o frontend dos microservicos
- evoluir gradualmente a integracao real com os servicos externos

## Relacao com a Etapa 1

Na etapa 1, o microfrontend foi preparado para consumir somente o BFF.

Os contratos esperados pelo frontend eram:

- `GET /aggregated-data`
- `GET /people`
- `GET /people/:id`
- `GET /documents`
- `GET /documents/:id`

O BFF foi implementado justamente para ser a unica porta de entrada do frontend.

## O que foi implementado originalmente no BFF

### 1. Novo papel do projeto

O repositorio deixou de ser tratado apenas como uma API de dominio de RH e passou a ser tratado como o **BFF da arquitetura completa**.

Isso significa que agora ele deve:

- servir o microfrontend
- agregar dados de varios servicos
- evitar acesso direto do frontend aos microservicos
- centralizar contratos HTTP consumidos pela shell e pelos remotes

### 2. Endpoints principais do BFF

Foram criados os endpoints publicos do BFF, sem prefixo `/api`, porque esse e o formato esperado pelo frontend:

- `GET /aggregated-data`
- `GET /people`
- `GET /people/{id}`
- `POST /people`
- `PUT /people/{id}`
- `DELETE /people/{id}`
- `GET /documents`
- `GET /documents/{id}`
- `POST /documents`
- `PUT /documents/{id}`
- `DELETE /documents/{id}`

### 3. Agregacao de dados

Foi criada a base do endpoint:

- `GET /aggregated-data`

Ele foi implementado para:

- consultar o client de pessoas
- consultar o client de documentos
- consultar o client da function
- montar uma unica resposta para o frontend

### 4. Clients downstream

Foram criados clientes dedicados para os servicos que o BFF consome ou consumira:

- `PeopleBffClient`
- `DocumentsBffClient`
- `FunctionBffClient`

Esses clients ficaram na infraestrutura e foram acessados por interfaces na aplicacao.

Objetivo:

- manter desacoplamento
- permitir trocar mocks por endpoints reais depois
- preservar Clean Architecture

### 5. Contratos e slices do BFF

Foi criada uma area especifica para o BFF dentro da aplicacao:

- `Application/Features/Bff/AggregatedData`
- `Application/Features/Bff/People`
- `Application/Features/Bff/Documents`

Tambem foram criados DTOs e interfaces para os contratos do BFF.

Isso garante:

- organizacao por feature
- aderencia a Vertical Slice
- separacao clara entre contratos do BFF e regras dos dominios antigos

## Evolucao posterior: integracao com Microservico 1 MongoDB

Depois da criacao do microservico 1, foi feita a primeira integracao real do BFF com um servico externo da arquitetura.

### Dominio integrado

O primeiro dominio integrado foi:

- `Documents`

### Tipo de microservico integrado

Esse dominio passou a ser atendido por:

- microservico independente
- banco MongoDB Atlas
- CRUD real

### O que mudou no BFF nessa evolucao

O BFF deixou de usar mock para `documents` e passou a consumir o microservico real na porta local:

- `http://localhost:5102/api/documents/`

Foram feitos os seguintes ajustes:

- `DocumentsBffClient` passou a consumir o endpoint real do microservico
- o contrato de `documents` foi ajustado para aceitar `id` como `string`
- o controller `DocumentsBffController` foi ajustado para rotas com `id` string
- o BFF passou a mapear o contrato real do microservico para o formato que o frontend espera

### Estado atual dos downstreams

Hoje, o estado do BFF e:

- `people`: ainda em mock
- `documents`: integrado ao microservico real MongoDB
- `function`: ainda em mock

## Configuracao atual

Foi mantida a secao:

- `DownstreamServices`

Mas ela evoluiu para um modelo mais granular.

Configuracao conceitual atual:

```json
"DownstreamServices": {
  "UseMocks": false,
  "UsePeopleMocks": true,
  "UseDocumentsMocks": false,
  "UseFunctionMocks": true,
  "PeopleBaseUrl": "http://localhost:5101/",
  "DocumentsBaseUrl": "http://localhost:5102/api/documents/",
  "FunctionBaseUrl": "http://localhost:7071/",
  "FunctionSummaryPath": "api/enrichment-summary"
}
```

Isso permite:

- usar integracao real para `documents`
- manter `people` em mock
- manter `function` em mock
- continuar evoluindo sem bloquear a arquitetura toda

## CORS adaptado para o frontend

O CORS foi ampliado para aceitar origens locais usuais do frontend em desenvolvimento, especialmente as portas comuns da shell e dos remotes.

Isso facilita a integracao entre:

- shell
- microfrontends
- BFF local

## Estrutura criada ou destacada nesta etapa

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
|   |       |-- IBffPeopleClient.cs
|   |       |-- IBffDocumentsClient.cs
|   |       `-- IBffFunctionClient.cs
|   `-- Features
|       `-- Bff
|           |-- AggregatedData
|           |-- People
|           |-- Documents
|           `-- Common
|-- Infrastructure
|   |-- Clients
|   |   |-- PeopleBffClient.cs
|   |   |-- DocumentsBffClient.cs
|   |   `-- FunctionBffClient.cs
|   `-- Configuration
|       `-- DownstreamServicesOptions.cs
|-- appsettings.json
|-- README.md
`-- ContextoBFF.md
```

## Como o BFF ficou neste momento

Neste ponto do projeto, o BFF ficou assim:

- implementado em C#
- com Clean Architecture preservada
- com Vertical Slice preservado
- com endpoints voltados ao frontend
- com agregacao pronta
- com proxy CRUD pronto
- com clients desacoplados para servicos externos
- com integracao real no dominio `documents`
- com mocks ainda ativos para `people` e `function`

Em outras palavras:

- o BFF ja existe conceitualmente e tecnicamente
- a primeira integracao real com microservico ja existe
- a arquitetura esta parcialmente real e parcialmente mockada
- isso permite validar o fluxo `frontend -> BFF -> microservico 1`

## Fluxo funcional atual

Hoje o fluxo que ja pode ser demonstrado e:

1. usuario acessa o microfrontend
2. frontend chama o BFF
3. BFF responde `people` por mock
4. BFF chama o microservico `Documents`
5. microservico consulta o MongoDB Atlas
6. BFF devolve os dados para o frontend

No caso do endpoint agregado:

- `people` vem de mock
- `documents` vem do microservico real MongoDB
- `function` vem de mock

## O que ainda NAO foi feito

Esses pontos continuam para as proximas fases:

- implementar o microservico real de `people`
- conectar `PeopleBffClient` ao servico real
- implementar a Azure Function real
- conectar `FunctionBffClient`
- tornar `GET /aggregated-data` totalmente real
- decidir autenticacao final do BFF no fluxo da entrega
- integrar API Gateway na frente do BFF
- publicar imagem Docker do BFF

## Como esta previsto que vai ficar depois

Quando os itens 3 e 4 estiverem completos, o fluxo esperado sera:

1. usuario acessa a shell do microfrontend
2. shell chama o BFF
3. BFF chama o microservico de `people`
4. BFF chama o microservico de `documents`
5. BFF chama a Azure Function para enriquecimento ou calculo
6. BFF agrega a resposta
7. frontend recebe um unico JSON pronto para uso

## Contrato evolutivo esperado

Hoje o BFF esta pronto para entregar:

- agregacao da overview
- acesso a people
- acesso a documents

Mas com o seguinte estado:

- `documents` real
- `people` mock
- `function` mock

Depois, ele deve evoluir para:

- autenticar o frontend
- repassar token ou contexto para os microservicos
- aplicar regras de composicao especificas da interface
- reduzir payloads desnecessarios
- adaptar respostas conforme o tipo de cliente, se necessario

## Validacao realizada

Ao longo da evolucao do BFF, foi validado:

- `dotnet build`
- `dotnet test`

Resultado atual:

- build com sucesso
- testes de arquitetura aprovados
- BFF compilando com a integracao do microservico `documents`

## Premissas para as proximas conversas

Para manter consistencia nas proximas etapas, considerar sempre:

1. Este repositorio representa o **BFF** da solucao.
2. O frontend deve continuar consumindo somente o BFF.
3. Nao devemos ligar o frontend diretamente aos microservicos.
4. Os endpoints publicos principais do frontend devem ser preservados:
   - `/aggregated-data`
   - `/people`
   - `/documents`
5. A integracao real com microservicos deve entrar por `HttpClient` e interfaces ja criadas.
6. Hoje o dominio `documents` ja esta real.
7. Hoje `people` e `function` ainda usam mock.
8. A arquitetura academica precisa continuar explicavel em termos de:
   - BFF
   - Clean Architecture
   - Vertical Slice
   - agregacao
   - proxy
   - integracao progressiva com microservicos

## Sugestao natural de continuidade

A sequencia mais natural a partir daqui e:

1. implementar o microservico 2
2. definir o dominio real de `people`
3. integrar `PeopleBffClient` ao novo servico
4. criar a Azure Function
5. integrar a function ao BFF
6. deixar `GET /aggregated-data` totalmente real
7. depois pensar em API Gateway e publicacao

## Arquivos mais importantes para retomar rapido

Se precisarmos retomar rapido a etapa do BFF depois, olhar primeiro:

- `src/API/Program.cs`
- `src/API/Controllers/AggregatedDataController.cs`
- `src/API/Controllers/PeopleController.cs`
- `src/API/Controllers/DocumentsBffController.cs`
- `src/Application/Features/Bff/Common/BffDtos.cs`
- `src/Application/Features/Bff/AggregatedData/GetAggregatedData/GetAggregatedDataHandler.cs`
- `src/Infrastructure/Clients/PeopleBffClient.cs`
- `src/Infrastructure/Clients/DocumentsBffClient.cs`
- `src/Infrastructure/Clients/FunctionBffClient.cs`
- `src/Infrastructure/Configuration/DownstreamServicesOptions.cs`
- `appsettings.json`
- `README.md`
- `ContextoBFF.md`

## Observacao final

Neste momento, o BFF ja deixou de ser apenas uma estrutura preparada para o futuro e passou a participar de uma integracao real com o microservico 1.

Isso significa que a arquitetura ja possui uma base concreta e demonstravel do fluxo:

- `frontend -> BFF -> microservico -> MongoDB Atlas`

Com isso, a proxima etapa natural e repetir essa evolucao para o microservico 2, mantendo o mesmo principio de desacoplamento e evolucao incremental da arquitetura distribuida.
