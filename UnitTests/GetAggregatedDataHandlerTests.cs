using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.AggregatedData.GetAggregatedData;
using GestaoRH.Application.Features.Bff.Common;
using NSubstitute;
using Xunit;

namespace GestaoRH.UnitTests;

public class GetAggregatedDataHandlerTests
{
    private readonly IBffPeopleClient _peopleClient;
    private readonly IBffDocumentsClient _documentsClient;
    private readonly IBffFunctionClient _functionClient;
    private readonly GetAggregatedDataHandler _handler;

    public GetAggregatedDataHandlerTests()
    {
        _peopleClient = Substitute.For<IBffPeopleClient>();
        _documentsClient = Substitute.For<IBffDocumentsClient>();
        _functionClient = Substitute.For<IBffFunctionClient>();
        
        _handler = new GetAggregatedDataHandler(_peopleClient, _documentsClient, _functionClient);
    }

    [Fact]
    public async Task Handle_ShouldReturnAggregatedDataResponseDto_WhenClientsReturnData()
    {
        // Arrange
        var cancellationToken = CancellationToken.None;

        var mockPeople = new List<PersonSummaryDto>
        {
            new(1, "Ana Souza", "Analista RH", "People", "ana.souza@empresa.com", "active"),
            new(2, "Carlos Lima", "Tech Recruiter", "People", "carlos.lima@empresa.com", "active")
        };

        var mockDocuments = new List<DocumentSummaryDto>
        {
            new("101", "Contrato de Experiencia", "admission", "Ana Souza", "pending")
        };

        var mockFunctionData = new EnrichmentSummaryDto(
            "Resumo enriquecido",
            mockPeople.Count,
            mockDocuments.Count,
            DateTimeOffset.UtcNow,
            "azure-function"
        );

        _peopleClient.ListAsync(cancellationToken).Returns(mockPeople);
        _documentsClient.ListAsync(cancellationToken).Returns(mockDocuments);
        _functionClient.GetSummaryAsync(mockPeople.Count, mockDocuments.Count, cancellationToken).Returns(mockFunctionData);

        var query = new GetAggregatedDataQuery();

        // Act
        var result = await _handler.Handle(query, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.People.Count);
        Assert.Equal(1, result.Documents.Count);
        Assert.Equal("Resumo enriquecido", result.FunctionData.Message);
        Assert.Equal(2, result.FunctionData.TotalPeople);
        Assert.Equal(1, result.FunctionData.TotalDocuments);
        Assert.Equal("web", result.Client);
        
        await _peopleClient.Received(1).ListAsync(cancellationToken);
        await _documentsClient.Received(1).ListAsync(cancellationToken);
        await _functionClient.Received(1).GetSummaryAsync(2, 1, cancellationToken);
    }
}
