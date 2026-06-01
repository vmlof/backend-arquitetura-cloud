# Relatório de Refatoração: GestaoRH-API

Este documento detalha as mudanças arquiteturais e de código implementadas para alinhar o projeto aos padrões de **Vertical Slice Architecture**, **Clean Architecture** e **Clean Code**.

## 1. Nova Estrutura de Pastas (Clean Architecture)

O projeto foi reestruturado para separar interesses e seguir a regra de dependência:

- **src/Domain/**: Contém o núcleo do negócio.
    - `Entities/`: Modelos de dados puros (Funcionario, Empresa, etc).
    - `Interfaces/`: Abstrações (IRepository, IUnitOfWork).
- **src/Application/**: Lógica de aplicação e transversal.
    - `Features/`: **VERTICAL SLICES** (Organizado por funcionalidade).
    - `Common/`: DTOs compartilhados, Mappings (AutoMapper) e Services transversais.
- **src/Infrastructure/**: Implementações técnicas.
    - `Data/`: Repositórios Dapper e UnitOfWork.
    - `Services/`: Serviços externos (PdfService).
    - `Security/`: Lógica de JWT e Criptografia.
- **src/API/**: Camada de entrada (Apresentação).
    - `Controllers/`: Controllers minimalistas que usam MediatR.
    - `Middlewares/`: Tratamento global de erros e Autenticação.

## 2. Implementação de Vertical Slices (Módulo Funcionario)

A lógica que antes residia no `FuncionarioService` foi decomposta em "slices" independentes usando **MediatR**:

### Commands (Escrita)
- `CadastrarFuncionario`: Handler responsável pela criação, geração de senha temporária e commit.
- `AtualizarFuncionario`: Atualização de dados cadastrais.
- `TrocarSenha`: Lógica de validação de senha atual e atualização de hash.
- `Login`: Autenticação e geração de Token JWT.
- `DesativarFuncionario`: Desativação lógica do registro.

### Queries (Leitura)
- `ListarFuncionarios`: Listagem geral (com filtro opcional de ativos).
- `ListarFuncionariosPorSetor`: Filtro por departamento.
- `ObterFuncionarioPorId`: Detalhes de um funcionário específico.

## 3. Clean Code e Padrões Adotados

### A. Global Exception Handling
Implementado o `ExceptionHandlingMiddleware`. 
- **Mudança**: Removidos blocos `try-catch` repetitivos dos controllers.
- **Resultado**: O middleware captura exceções como `ValidationException`, `KeyNotFoundException` e `UnauthorizedAccessException`, retornando o Status Code HTTP correto automaticamente.

### B. Validação Desacoplada (FluentValidation)
- **Mudança**: As validações de `if (string.IsNullOrEmpty)` foram movidas dos Handlers para classes `Validator`.
- **Exemplo**: `CadastrarFuncionarioValidator` garante a integridade dos dados antes da lógica de negócio ser executada.

### C. Controllers "Burros"
- **Mudança**: O `FuncionarioController` agora recebe apenas o `IMediator`.
- **Antes**: Injetava múltiplos serviços e repositórios.
- **Agora**: `return await _mediator.Send(command);`

### D. Mapeamento com AutoMapper
- **Mudança**: Removidos métodos estáticos como `ToResponse` das entidades/serviços.
- **Resultado**: Uso de `MappingProfile` centralizado para conversão entre Entidades e DTOs.

## 4. Tecnologias Adicionadas

- **MediatR**: Orquestração de comandos e consultas.
- **FluentValidation**: Validação fluente e desacoplada.
- **AutoMapper**: Mapeamento automático de objetos.

## 5. Status da Compilação
- O projeto foi verificado via `dotnet build` e está compilando sem erros na nova estrutura.
- O arquivo `FuncionarioService.cs` foi descontinuado e removido, servindo como modelo para a migração dos demais serviços.

---
**Data**: 11 de Maio de 2026  
**Status**: Fase 1 (Fundação e Módulo Funcionario) Concluída.
