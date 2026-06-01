# GestaoRH-API

Backend do Sistema de Gestão de Documentos de RH.  
Stack: **.NET 9 · Dapper · PostgreSQL · JWT · BCrypt**

---

## Pré-requisitos

| Ferramenta | Versão mínima | Download |
|---|---|---|
| .NET SDK | 9.0 | https://dotnet.microsoft.com/download |
| PostgreSQL | 14+ | https://www.postgresql.org/download |

---

## Configuração rápida

### 1. Clone / abra o projeto
```
cd C:\Users\pcesa\OneDrive\PUC-BES\6 Semestre\ProjetoSemestre\GestaoRH-API
```

### 2. Crie o banco de dados no PostgreSQL
```sql
CREATE DATABASE "GestaoRHDB";
```

### 3. Rode o script inicial
Abra o arquivo `Scripts/001_create_empresa.sql` no pgAdmin ou psql e execute.

### 4. Configure a connection string
Edite `appsettings.json` (ou `appsettings.Development.json`) e ajuste:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GestaoRHDB;Username=postgres;Password=SUA_SENHA"
  },
  "Jwt": {
    "SecretKey": "coloque-uma-chave-secreta-longa-aqui-minimo-64-caracteres"
  }
}
```

### 5. Restaure os pacotes e rode
```
dotnet restore
dotnet run
```

A API sobe em `http://localhost:5000`.  
A UI de testes (Scalar) fica em `http://localhost:5000/scalar`.

---

## Endpoints — Empresa

| Método | Rota | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/api/empresa/cadastrar` | ❌ | Cadastra nova empresa |
| POST | `/api/empresa/login` | ❌ | Login, retorna JWT |
| GET | `/api/empresa` | ✅ | Lista empresas |
| GET | `/api/empresa/{id}` | ✅ | Busca empresa por ID |
| PUT | `/api/empresa/{id}` | ✅ | Atualiza dados |
| DELETE | `/api/empresa/{id}` | ✅ | Desativa empresa |

### Exemplo de cadastro
```json
POST /api/empresa/cadastrar
{
  "cnpj": "12.345.678/0001-99",
  "razaoSocial": "Empresa Exemplo Ltda",
  "endereco": "Rua das Flores, 123 - Curitiba/PR",
  "telefone": "(41) 99999-9999",
  "logoBase64": null,
  "responsavelNome": "João",
  "responsavelSobrenome": "Silva",
  "senha": "minhasenha123"
}
```

### Exemplo de login
```json
POST /api/empresa/login
{
  "cnpj": "12.345.678/0001-99",
  "senha": "minhasenha123"
}
```
Retorna `{ empresa: {...}, jwt: "eyJ..." }`.  
Use o JWT no header das próximas requisições: `Authorization: Bearer <token>`

---

## Estrutura de pastas

```
GestaoRH-API/
├── Controllers/         # Recebe requests HTTP
├── Middlewares/         # Auth.cs — validação JWT
├── Models/
│   ├── Empresa.cs
│   └── DTOs/            # DTOs de entrada/saída
├── Repositories/        # Acesso ao banco (Dapper)
│   ├── IUnitOfWork.cs
│   ├── UnitOfWork.cs
│   ├── IEmpresaRepository.cs
│   └── EmpresaRepository.cs
├── Scripts/             # SQL de criação de tabelas
├── Services/            # Regras de negócio
├── Utils/               # Jwt.cs
├── Program.cs
└── appsettings.json
```

---

## Próximos módulos previstos

- `Funcionario` — cadastro, grupos, setores
- `Documento` — templates, versões, metadados
- `Upload` — envio de atestados/comprovantes
- `Aprovacao` — fluxo Enviado → Aprovado/Rejeitado
- `AssinaturaEletronica` — PDF com evidências
- `Notificacao` — email/push
- `AuditoriaLog` — rastreabilidade completa
