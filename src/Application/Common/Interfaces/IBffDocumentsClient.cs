using GestaoRH.Application.Features.Bff.Common;
using System.Text.Json.Nodes;

namespace GestaoRH.Application.Common.Interfaces;

public interface IBffDocumentsClient
{
    Task<IReadOnlyCollection<DocumentSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<DocumentDetailDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<JsonNode?> CreateAsync(JsonObject payload, CancellationToken cancellationToken = default);
    Task<JsonNode?> UpdateAsync(string id, JsonObject payload, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
