using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;
using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace GestaoRH.ArchitectureTests
{
    public class CleanArchitectureTests
    {
        private static readonly Architecture Architecture = new ArchLoader()
            .LoadAssemblies(typeof(GestaoRH.Domain.Entities.Funcionario).Assembly)
            .Build();

        // ========================================================================
        // DEFINIÇÃO DAS CAMADAS (CLEAN ARCHITECTURE)
        // ========================================================================
        
        private readonly IObjectProvider<IType> DomainLayer = Types()
            .That().ResideInNamespace("GestaoRH.Domain")
            .Or().HaveFullNameContaining("GestaoRH.Domain.")
            .As("Camada de Domínio");

        private readonly IObjectProvider<IType> ApplicationLayer = Types()
            .That().ResideInNamespace("GestaoRH.Application")
            .Or().HaveFullNameContaining("GestaoRH.Application.")
            .As("Camada de Aplicação");

        private readonly IObjectProvider<IType> InfrastructureLayer = Types()
            .That().ResideInNamespace("GestaoRH.Infrastructure")
            .Or().HaveFullNameContaining("GestaoRH.Infrastructure.")
            .As("Camada de Infraestrutura");

        private readonly IObjectProvider<IType> ApiLayer = Types()
            .That().ResideInNamespace("GestaoRH.API")
            .Or().HaveFullNameContaining("GestaoRH.API.")
            .As("Camada API");

        // ========================================================================
        // TESTES DE CLEAN ARCHITECTURE (DEPENDÊNCIAS ENTRE CAMADAS)
        // ========================================================================

        [Fact]
        public void DomainShouldNotDependOnOtherLayers()
        {
            Types().That().Are(DomainLayer)
                .Should().NotDependOnAny(ApplicationLayer)
                .AndShould().NotDependOnAny(InfrastructureLayer)
                .AndShould().NotDependOnAny(ApiLayer)
                .Because("o Domínio é o núcleo e deve ser independente.")
                .Check(Architecture);
        }

        [Fact]
        public void ApplicationShouldNotDependOnInfrastructureOrApi()
        {
            Types().That().Are(ApplicationLayer)
                .Should().NotDependOnAny(InfrastructureLayer)
                .AndShould().NotDependOnAny(ApiLayer)
                .Because("a camada de Aplicação deve ser independente de detalhes de infraestrutura e apresentação.")
                .Check(Architecture);
        }

        [Fact]
        public void InfrastructureShouldNotDependOnApi()
        {
            Types().That().Are(InfrastructureLayer)
                .Should().NotDependOnAny(ApiLayer)
                .Because("a camada de Infraestrutura não deve conhecer a camada de apresentação.")
                .Check(Architecture);
        }

        // ========================================================================
        // TESTES DE VERTICAL SLICE (ISOLAMENTO E PADRONIZAÇÃO)
        // ========================================================================

        [Fact]
        public void SlicesShouldBeIndependent()
        {
            // Regra para o slice de Funcionários
            var funcionariosSlice = Types().That().ResideInNamespace("GestaoRH.Application.Features.Funcionarios")
                .Or().HaveFullNameContaining("GestaoRH.Application.Features.Funcionarios.");
            
            // Outros slices são aqueles em Features que NÃO são Funcionarios
            var outrosSlicesEmFeatures = Types().That().ResideInNamespace("GestaoRH.Application.Features")
                .Or().HaveFullNameContaining("GestaoRH.Application.Features.")
                .And().AreNot(funcionariosSlice);

            Types().That().Are(funcionariosSlice)
                .Should().NotDependOnAny(outrosSlicesEmFeatures)
                .Because("cada Vertical Slice deve ser autossuficiente e isolado de outras funcionalidades.")
                .Check(Architecture);
        }

        [Fact]
        public void HandlersShouldFollowNamingConvention()
        {
            // Todos os Handlers do MediatR devem terminar com o sufixo 'Handler'
            Classes().That().ResideInNamespace("GestaoRH.Application.Features")
                .Or().HaveFullNameContaining("GestaoRH.Application.Features.")
                .And().HaveNameContaining("Handler")
                .Should().HaveNameEndingWith("Handler")
                .Because("padronização de nomenclatura facilita a descoberta de código.")
                .Check(Architecture);
        }

        [Fact]
        public void ControllersShouldNotDependOnRepositoriesDirectly()
        {
            var repositories = Interfaces().That().HaveNameEndingWith("Repository");

            Classes().That().Are(ApiLayer)
                .Should().NotDependOnAny(repositories)
                .Because("Controllers devem delegar o trabalho para os Handlers via MediatR.")
                .Check(Architecture);
        }

        [Fact]
        public void EntitiesShouldResideInDomain()
        {
            Classes().That().ResideInNamespace("GestaoRH.Domain.Entities")
                .Or().HaveFullNameContaining("GestaoRH.Domain.Entities.")
                .Should().Be(DomainLayer)
                .Because("entidades representam o domínio do negócio.")
                .Check(Architecture);
        }
    }
}
