# Diretrizes para Commits de Refatoração (Clean Architecture & Vertical Slices)

Este guia orienta sobre como estruturar a mensagem de commit ao finalizar a migração da arquitetura do projeto.

---

## 1. Padrão Recomendado: Conventional Commits

Recomenda-se o uso do padrão **Conventional Commits** para garantir um histórico de commits legível e automatizável.

### Estrutura da Mensagem
```txt
<tipo>(<escopo>): <descrição curta em minúsculas>

[corpo detalhado explicando o "porquê" e principais mudanças]

[rodapé com links de tarefas ou informações de breaking changes, se aplicável]
```

---

## 2. Sugestão de Commit para esta Refatoração

### Opção 1: Commit Unificado (Recomendado se tudo for enviado de uma vez)

*   **Tipo**: `refactor` (refatoração de código que não altera o comportamento final)
*   **Escopo**: `architecture`
*   **Mensagem**:

```txt
refactor(architecture): migrar estrutura para Clean Architecture e Vertical Slices

- Reestruturação de pastas em camadas sob a pasta 'src/':
  - Domain (Entidades e Interfaces de Repositório)
  - Application (Common Services, DTOs, Mappings e Feature Slices)
  - Infrastructure (Implementação de Dados/Dapper, PDF e Segurança)
  - API (Controllers baseados em MediatR e Middlewares)
- Migração do módulo Funcionario para Vertical Slices usando MediatR.
- Desacoplamento de validações utilizando FluentValidation.
- Centralização de mapeamento de dados com AutoMapper.
- Introdução de testes arquiteturais automatizados usando ArchUnitNET.
- Remoção das pastas legadas do diretório raiz.
```

### Opção 2: Commits Incrementais (Se preferir dividir as alterações em commits menores)

1.  **Mudança Estrutural (Mover arquivos e pastas)**
    ```txt
    refactor(architecture): reorganizar arquivos no padrão de pastas Clean Architecture
    ```
2.  **Lógica do Funcionario (MediatR & Slices)**
    ```txt
    feat(funcionario): implementar vertical slices para cadastrar, atualizar, login e desativar
    ```
3.  **Testes de Arquitetura**
    ```txt
    test(architecture): adicionar testes automatizados com ArchUnitNET para validação de camadas
    ```
4.  **Remoção de Código Legado**
    ```txt
    chore(cleanup): remover controllers, serviços e repositórios legados do diretório raiz
    ```

---

## 3. Boas Práticas

*   **Evite commits gigantes sem descrição**: Sempre detalhe os diretórios criados/removidos.
*   **Valide o build antes de commitar**: Sempre rode `dotnet build` e `dotnet test` para garantir que o commit não quebre a build.
*   **Mensagens no Imperativo**: Prefira descrever a ação de forma direta (ex: `migrar` em vez de `migrado`, `implementar` em vez de `implementei`).
