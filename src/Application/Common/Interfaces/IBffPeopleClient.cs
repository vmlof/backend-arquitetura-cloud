using GestaoRH.Application.Features.Bff.Common;
using System.Text.Json.Nodes;

namespace GestaoRH.Application.Common.Interfaces;

public interface IBffPeopleClient
{
    Task<IReadOnlyCollection<PersonSummaryDto>> ListAsync(CancellationToken cancellationToken = default);
    Task<PersonDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<JsonNode?> CreateAsync(JsonObject payload, CancellationToken cancellationToken = default);
    Task<JsonNode?> UpdateAsync(int id, JsonObject payload, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);
}
