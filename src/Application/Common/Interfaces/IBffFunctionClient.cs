using GestaoRH.Application.Features.Bff.Common;

namespace GestaoRH.Application.Common.Interfaces;

public interface IBffFunctionClient
{
    Task<EnrichmentSummaryDto> GetSummaryAsync(int peopleCount, int documentsCount, CancellationToken cancellationToken = default);
}
