# Projeto PJBL — Arquitetura Completa

## Objetivo

Desenvolver e demonstrar uma aplicação distribuída utilizando microservices, BFF, microfrontend e Azure Functions, aplicando padrões modernos como:

- Clean Architecture
- Vertical Slice
- Event-Driven Architecture
- API Gateway + BFF + Microservice + Database Service + Serverless
- Integração com múltiplos bancos

---

## 🧱 Arquitetura da Solução

A solução será composta pelos seguintes componentes:

### 🔹 1. Microfrontend

- Interface do usuário (SPA)
- Pode ser feito com:
    - React ou Angular
- Responsável por consumir o BFF (não acessa microservices diretamente)

---

### 🔹 2. BFF (Backend for Frontend) — Node.js

- Implementado com Node.js preferencialmente (ex: Express ou NestJS)
- Responsabilidades:
    - Agregação de dados
    - Proxy de requisições (CRUD)
    - Comunicação com:
        - Microservices
        - Function (HTTP Trigger)

#### 📌 Endpoint obrigatório

- `GET /aggregated-data`
    - Consome:
        - Microserviço 1
        - Microserviço 2
        - Azure Function
    - Retorna tudo em um único response

---

### 🔹 3. Microserviço 1 (MongoDB)

- Banco: MongoDB Atlas (Free Tier)
- Responsável por um domínio (Documento)
- CRUD completo
- Swagger documentado

---

### 🔹 4. Microserviço 2 (Azure SQL)

- Banco: Azure SQL Database (Free 1 DTU)
- Responsável por outro domínio (People)
- CRUD completo
- Swagger documentado

---

### 🔹 5. Azure Function

- Tipo: HTTP Trigger
- Pode simular:
    - Cálculo
    - Enriquecimento de dados
- Exposta via endpoint HTTP

---

### 🔹 6. API Gateway (opcional, mas exigido na entrega)

- Pode usar:
    - AWS Gateway
- Responsável por:
    - Roteamento
    - Segurança
    - Centralização de entrada

---

## 🔄 Fluxo da Aplicação

1. Usuário acessa o Microfrontend
2. Front chama o BFF
3. BFF:
    - Consulta Microserviço Mongo
    - Consulta Microserviço SQL
    - Consulta Azure Function
4. BFF agrega tudo
5. Retorna um único JSON

---

## 🐳 Dockerização

Publicar no Docker Hub para cada serviço (microservices + BFF):

```bash
docker build -t <registry>/<repo>/<imagem>:<tag> .
docker login <registry>
docker push <registry>/<repo>/<imagem>:<tag>
```

#### 📌 Exemplo

```bash
docker build -t dockerhubuser/pjbl/bff:v1 .
docker push dockerhubuser/pjbl/bff:v1
```

---

## 🧪 Requisitos de Código

### ✔ Clean Architecture

Separação em camadas:

- Domain
- Application
- Infrastructure
- API

### ✔ Vertical Slice

Organização por feature (ex: `CreateOrder`, `GetOrder`)

### ✔ Testes

- Testes unitários
- Testes de arquitetura

---

## 📂 Repositórios (GitHub)

Publicar **tudo** em repositórios públicos no GitHub:

- Microfrontend
- BFF
- Microserviço Mongo
- Microserviço SQL
- Azure Function

---

## 📄 README.md (em cada projeto)

Deve conter:

- Descrição da arquitetura
- Tecnologias utilizadas
- Como rodar localmente
- Nome dos alunos

---

## 🧾 Documentação (Entrega em PDF da Entrega 3)

### 📘 ARC42 deve conter

Links para:

- GitHub (todos os projetos)
- Docker Hub (imagens)
- Swagger:
    - BFF
    - Microservices
- Frontend
- API Gateway
- Vídeo no YouTube

---

## 🧩 Outros artefatos obrigatórios

### ✔ C4 Model atualizado

- Contexto
- Containers
- Componentes

### ✔ Canvas

### ✔ Prints

- Swagger BFF
- Swagger Microservices
- Frontend funcionando
- API Gateway

---

## 🎥 Vídeo (YouTube)

- Plataforma: YouTube
- Tipo: Não listado (somente link)
- **Todos do grupo devem falar**, deixar explícito no vídeo

Deve demonstrar:

- Frontend funcionando
- Chamadas passando pelo BFF
- CRUD completo
- Integração com:
    - MongoDB
    - SQL Server
    - Function
- Fluxo completo da arquitetura