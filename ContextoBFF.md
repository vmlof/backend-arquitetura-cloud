# Contexto da Etapa 2 - BFF

Este arquivo registra o contexto completo do que foi feito no item 2 da atividade para manter continuidade nas proximas conversas e nas proximas entregas.

## Objetivo desta etapa

Transformar o backend atual em uma base de **BFF (Backend for Frontend)** alinhada com o PJBL da disciplina, preservando o uso de **C# / ASP.NET Core** e reaproveitando a estrutura arquitetural que ja existia no projeto.

A meta desta etapa foi atender especificamente o item:

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
- deixar os microservicos reais para as proximas etapas

## Relacao com a Etapa 1

Na etapa 1, o microfrontend foi preparado para consumir somente o BFF.

Os contratos esperados pelo frontend eram:

- `GET /aggregated-data`
- `GET /people`
- `GET /people/:id`
- `GET /documents`
- `GET /documents/:id`

Nesta etapa, o backend foi adaptado para entregar essa base de contrato, sem conectar ainda nos microservicos reais.

## O que foi implementado nesta atualizacao

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

Mesmo em modo mock, o fluxo ja representa o papel do BFF.

### 4. Clients downstream

Foram criados clientes dedicados para os servicos que o BFF vai consumir futuramente:

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

### 6. Configuracao para mocks e integracao futura

Foi adicionada a secao:

- `DownstreamServices`

com campos para:

- `UseMocks`
- `PeopleBaseUrl`
- `DocumentsBaseUrl`
- `FunctionBaseUrl`
- `FunctionSummaryPath`

Nesta etapa, o projeto ficou configurado com:

- `UseMocks = true`

Isso permite demonstrar o BFF mesmo antes dos microservicos e da Azure Function estarem prontos.

### 7. CORS adaptado para o frontend

O CORS foi ampliado para aceitar origens locais usuais do frontend em desenvolvimento, especialmente as portas comuns da shell e dos remotes.

Isso facilita a integracao futura entre:

- shell
- microfrontends
- BFF local

### 8. Documentacao atualizada

O `README.md` foi reescrito para refletir o novo papel do projeto como BFF e explicar:

- objetivo
- endpoints
- configuracao
- forma de execucao
- evolucao futura

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
- com proxy CRUD pronto em estrutura
- com clients desacoplados para servicos externos
- com modo mock para demonstracao

Em outras palavras:

- o BFF ja existe conceitualmente e tecnicamente
- a integracao real ainda nao existe
- a arquitetura ja esta pronta para receber os microservicos nas proximas etapas

## O que ainda NAO foi feito nesta etapa

Esses pontos ficaram para as proximas fases:

- conectar `PeopleBffClient` ao microservico Mongo real
- conectar `DocumentsBffClient` ao microservico SQL real
- conectar `FunctionBffClient` a Azure Function real
- alinhar payloads reais de entrada e saida
- decidir autenticacao final do BFF no fluxo da entrega
- integrar API Gateway na frente do BFF
- publicar imagem Docker do BFF

## Como esta previsto que vai ficar depois

Quando os itens 3 e 4 estiverem prontos, o fluxo esperado sera:

1. usuario acessa a shell do microfrontend
2. shell chama o BFF
3. BFF chama o microservico de pessoas/produtos no Mongo
4. BFF chama o microservico de documentos/pedidos no SQL
5. BFF chama a Azure Function para enriquecimento ou calculo
6. BFF agrega a resposta
7. frontend recebe um unico JSON pronto para uso

## Contrato evolutivo esperado

Hoje o BFF esta pronto para entregar:

- agregacao da overview
- acesso a people
- acesso a documents

Depois, ele deve evoluir para:

- autenticar o frontend
- repassar token ou contexto para os microservicos
- aplicar regras de composicao especificas da interface
- reduzir payloads desnecessarios
- adaptar respostas conforme o tipo de cliente, se necessario

## Validacao realizada

Foi validado nesta etapa:

- `dotnet build`
- `dotnet test`

Resultado:

- build com sucesso
- testes de arquitetura aprovados

Observacoes encontradas:

- existe um warning de versao entre `AutoMapper` e `AutoMapper.Extensions.Microsoft.DependencyInjection`
- existe tambem um warning relacionado ao `Microsoft.NET.Test.Sdk` no mesmo projeto principal

Esses pontos nao bloquearam a entrega da etapa 2, mas podem ser limpos depois.

## Premissas para as proximas conversas

Para manter consistencia nas proximas etapas, considerar sempre:

1. Este repositorio agora representa o **BFF** da solucao.
2. O frontend deve continuar consumindo somente o BFF.
3. Nao devemos ligar o frontend diretamente aos microservicos.
4. Os endpoints publicos principais do frontend devem ser preservados:
   - `/aggregated-data`
   - `/people`
   - `/documents`
5. A integracao real com microservicos deve entrar por `HttpClient` e interfaces ja criadas.
6. O modo mock deve ser usado enquanto os servicos reais nao estiverem prontos.
7. A arquitetura academica precisa continuar explicavel em termos de:
   - BFF
   - Clean Architecture
   - Vertical Slice
   - agregacao
   - proxy

## Sugestao natural de continuidade

A sequencia mais natural a partir daqui e:

1. definir o dominio real do microservico 1
2. implementar o microservico 1 no MongoDB
3. alinhar o contrato do BFF com esse microservico
4. definir o dominio real do microservico 2
5. implementar o microservico 2 no Azure SQL
6. alinhar o contrato do BFF com esse microservico
7. criar a Azure Function
8. trocar `UseMocks` para `false`
9. integrar tudo no endpoint `/aggregated-data`

## Arquivos mais importantes para retomar rapido

Se precisarmos retomar rapido a etapa 2 depois, olhar primeiro:

- `src/API/Program.cs`
- `src/API/Controllers/AggregatedDataController.cs`
- `src/API/Controllers/PeopleController.cs`
- `src/API/Controllers/DocumentsBffController.cs`
- `src/Application/Features/Bff/Common/BffDtos.cs`
- `src/Application/Features/Bff/AggregatedData/GetAggregatedData/GetAggregatedDataHandler.cs`
- `src/Infrastructure/Clients/PeopleBffClient.cs`
- `src/Infrastructure/Clients/DocumentsBffClient.cs`
- `src/Infrastructure/Clients/FunctionBffClient.cs`
- `appsettings.json`
- `README.md`
- `ContextoBFF.md`

## Observacao final

Nesta etapa, o mais importante nao foi conectar tudo de fato, e sim posicionar corretamente o projeto no papel de BFF dentro da arquitetura distribuida da disciplina.

Com isso, a base ficou pronta para evoluir sem retrabalho nas proximas fases, mantendo coerencia com o microfrontend da etapa 1 e com os futuros microservicos das etapas 3 e 4.
