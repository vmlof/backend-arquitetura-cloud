using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using GestaoRH.Infrastructure.Configuration;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json;

namespace GestaoRH.Infrastructure.Clients;

public class FunctionBffClient : IBffFunctionClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly DownstreamServicesOptions _options;

    public FunctionBffClient(HttpClient httpClient, IOptions<DownstreamServicesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<EnrichmentSummaryDto> GetSummaryAsync(int peopleCount, int documentsCount, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks)
        {
            return new EnrichmentSummaryDto(
                "Resumo enriquecido pelo BFF em modo mock.",
                peopleCount,
                documentsCount,
                DateTimeOffset.UtcNow,
                "mock-azure-function");
        }

        if (string.IsNullOrWhiteSpace(_options.FunctionBaseUrl))
        {
            throw new InvalidOperationException("Configure DownstreamServices:FunctionBaseUrl para usar o BFF sem mocks.");
        }

        var path = QueryHelpers.AddQueryString(
            _options.FunctionSummaryPath,
            new Dictionary<string, string?>
            {
                ["peopleCount"] = peopleCount.ToString(),
                ["documentsCount"] = documentsCount.ToString()
            });

        return await _httpClient.GetFromJsonAsync<EnrichmentSummaryDto>(path, JsonOptions, cancellationToken)
            ?? new EnrichmentSummaryDto(
                "Azure Function nao retornou payload.",
                peopleCount,
                documentsCount,
                DateTimeOffset.UtcNow,
                "azure-function");
    }
}
