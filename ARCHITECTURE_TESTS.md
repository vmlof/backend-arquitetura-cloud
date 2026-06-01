# Testes de Arquitetura (ArchUnitNET)

Este projeto utiliza o **ArchUnitNET** para garantir que a estrutura do código siga rigorosamente os princípios da **Clean Architecture** e da **Vertical Slice Architecture**.

## 1. Visão Geral

Os testes automatizados de arquitetura garantem que o design do sistema permaneça íntegro ao longo do tempo, impedindo o surgimento de "Big Ball of Mud" (grande bola de lama) e mantendo o baixo acoplamento entre funcionalidades e camadas.

## 2. Regras de Clean Architecture (Camadas)

O sistema é dividido em camadas com responsabilidades e regras de visibilidade claras:

*   **Domínio (`Domain`)**: O núcleo do negócio. Não deve depender de nenhuma outra camada.
*   **Aplicação (`Application`)**: Orquestra a lógica de negócio. Pode depender do Domínio, mas nunca da Infraestrutura ou da API.
*   **Infraestrutura (`Infrastructure`)**: Implementações técnicas (Banco de Dados, Segurança, Serviços Externos). Pode depender de Domínio e Aplicação, mas nunca da API.
*   **API (Apresentação)**: Porta de entrada do sistema. Pode depender das camadas internas, mas deve ser minimalista (Clean API).

## 3. Regras de Vertical Slice (Funcionalidades)

Para garantir que cada funcionalidade seja independente e fácil de manter/remover, aplicamos as seguintes regras:

1.  **Isolamento entre Slices**: Um slice (ex: `Funcionarios`) não deve depender de detalhes internos ou lógica de outro slice (ex: `Empresas`).
2.  **Padronização de Nomenclatura**: Todos os Handlers do MediatR devem obrigatoriamente terminar com o sufixo `Handler`.
3.  **Clean API (Controllers)**: Controllers não devem injetar Repositórios diretamente. Eles devem delegar o trabalho para os Handlers através do MediatR.

## 4. Como Executar os Testes

Os testes são baseados em **xUnit** e podem ser executados via IDE ou terminal.

### Via Terminal (dotnet CLI)
Para executar especificamente os testes de arquitetura:
```bash
dotnet test --filter "CleanArchitectureTests"
```

### Via IDE (Visual Studio / VS Code)
Abra o **Test Explorer** e execute os testes dentro do namespace `GestaoRH.ArchitectureTests`.

## 5. Resolução de Violações

Quando um teste falha, o ArchUnitNET fornece um relatório detalhado de qual classe está violando qual regra.

*   **Erro de Camada**: Se a Aplicação depender da Infraestrutura, use a **Inversão de Dependência**. Crie uma `interface` no Domínio/Aplicação e implemente-a na Infraestrutura.
*   **Erro de Slice**: Se um slice depender de outro, verifique se a lógica não deveria estar em `Common` ou se a comunicação entre slices não deveria ser feita via eventos ou comandos.
*   **Erro de Controller**: Se um Controller falhar por injetar um Repositório, mova a lógica para um `Handler` e use o `IMediator`.

---
*Manter estes testes passando é requisito obrigatório para a integridade do projeto.*
