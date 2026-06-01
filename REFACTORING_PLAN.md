# Plano de Refatoração: Vertical Slice + Clean Architecture + Clean Code

Este documento detalha a estratégia para transformar a API GestaoRH em um sistema seguindo padrões modernos de arquitetura e desenvolvimento, garantindo manutenibilidade, testabilidade e escalabilidade.

## 1. Visão Geral da Mudança

Atualmente, o projeto utiliza uma arquitetura **N-Tier (Camadas)** tradicional:
- **Controllers**: Lidam com HTTP e chamam Services.
- **Services**: Contêm lógica de negócio, validações e mapeamentos.
- **Repositories**: Lidam com o acesso a dados usando Dapper.
- **Models/DTOs**: Compartilhados globalmente.

### Nova Abordagem: Clean Vertical Slices
Combinaremos o melhor de duas abordagens:
1. **Vertical Slice Architecture**: Organizaremos o código por **Funcionalidade (Feature)** em vez de camada técnica. Cada funcionalidade (ex: "Cadastrar Funcionário") terá seu próprio "slice" contendo Request, Response e Logic.
2. **Clean Architecture**: Dentro de cada slice e na base do projeto, respeitaremos a separação de interesses e a regra de dependência (o núcleo não conhece a infraestrutura).
3. **Clean Code**: Aplicaremos princípios como SRP (Responsabilidade Única), nomes significativos, e remoção de código repetitivo através de middlewares e padrões de design.

---

## 2. Estrutura de Pastas Proposta

```text
GestaoRH-API/
├── src/
│   ├── Domain/                 # Núcleo (Sem dependências externas)
│   │   ├── Entities/           # Entidades de Negócio
│   │   ├── Exceptions/         # Exceções customizadas (DomainException)
│   │   └── Interfaces/         # Abstrações (IRepository, IUnitOfWork)
│   ├── Application/            # Lógica Transversal
│   │   ├── Common/             # Comportamentos compartilhados (Behaviors)
│   │   └── Features/           # VERTICAL SLICES
│   │       ├── Funcionarios/
│   │       │   ├── Commands/
│   │       │   │   ├── CadastrarFuncionario/
│   │       │   │   │   ├── CadastrarFuncionarioCommand.cs
│   │       │   │   │   ├── CadastrarFuncionarioHandler.cs
│   │       │   │   │   └── CadastrarFuncionarioValidator.cs
│   │       │   ├── Queries/
│   │       │   │   ├── ListarFuncionarios/
│   │       │   │   └── ObterFuncionarioPorId/
│   │       │   └── DTOs/
│   │       └── Empresas/
│   ├── Infrastructure/         # Implementações Técnicas
│   │   ├── Data/               # Repositórios (Dapper), Contexto
│   │   ├── Services/           # Serviços externos (PdfService, JwtService)
│   │   └── Security/           # Autenticação/Autorização
│   └── API/                    # Apresentação
│       ├── Controllers/        # Controllers minimalistas (delegam para MediatR)
│       ├── Middlewares/        # Tratamento global de erros
│       └── Program.cs          # Configuração da Injeção de Dependência
```

---

## 3. Pilares da Implementação

### A. Vertical Slice com MediatR
Substituiremos os Services "monolíticos" (ex: `FuncionarioService`) por **Handlers** específicos usando MediatR.
- **Vantagem**: Menos acoplamento. Alterar a lógica de cadastro não afeta a lógica de listagem.
- **Clean Code**: Cada classe terá uma única razão para mudar.

### B. Validação com FluentValidation
Removeremos os `if (string.IsNullOrWhiteSpace...)` dos Services e usaremos validadores declarativos.
- **Exemplo**: `CadastrarFuncionarioValidator` garantirá que os dados estão corretos antes de chegar ao Handler.

### C. Clean Code nos Controllers
Os controllers serão "burros". Eles apenas recebem a requisição e enviam para o MediatR.
```csharp
[HttpPost]
public async Task<IActionResult> Cadastrar(CadastrarFuncionarioCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(ObterPorId), new { id = result.Id }, result);
}
```

### D. Tratamento Global de Erros (Middleware)
Removeremos os blocos `try-catch` repetitivos dos controllers. Um middleware capturará exceções conhecidas (ex: `ValidationException`, `NotFoundException`) e retornará o status code correto automaticamente.

### E. Mapeamento Automático
Usaremos **AutoMapper** ou **Mapster** para converter Entidades em DTOs, removendo os métodos estáticos `ToResponse` que poluem as classes de negócio.

---

## 4. Roadmap de Execução

1.  **Fundação**:
    - Instalação de pacotes: `MediatR`, `FluentValidation`, `AutoMapper`.
    - Criação da estrutura de pastas `Domain`, `Application`, `Infrastructure`.
    - Implementação do `ExceptionHandlingMiddleware`.
2.  **Migração do Domínio**:
    - Mover entidades para `Domain/Entities`.
    - Criar interfaces de repositório em `Domain/Interfaces`.
3.  **Refatoração por Slices (Exemplo: Funcionário)**:
    - Criar `CadastrarFuncionarioCommand` e `Handler`.
    - Mover lógica do `FuncionarioService.Cadastrar` para o Handler.
    - Criar Validator.
    - Atualizar `FuncionarioController` para usar `IMediator`.
4.  **Limpeza**:
    - Remover `FuncionarioService` após migrar todos os métodos para Handlers.
    - Ajustar Repositórios na camada de `Infrastructure`.

---

## 5. Benefícios Esperados

- **Facilidade de Teste**: Cada Handler pode ser testado isoladamente.
- **Baixo Acoplamento**: Mudanças em um setor do sistema não quebram outros.
- **Código Expressivo**: A intenção de cada classe (Command/Query) fica clara pelo nome.
- **Padronização**: Todos os endpoints seguirão o mesmo fluxo de validação e resposta.
