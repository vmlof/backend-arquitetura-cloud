using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using GestaoRH.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GestaoRH.Infrastructure.Clients;

public class DocumentsBffClient : IBffDocumentsClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly DownstreamServicesOptions _options;

    public DocumentsBffClient(HttpClient httpClient, IOptions<DownstreamServicesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<DocumentSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UseDocumentsMocks)
        {
            return
            [
                new("101", "Contrato de Admissao", "admission", "Ana Souza", "pending"),
                new("102", "Termo de Ferias", "vacation", "Carlos Lima", "signed"),
                new("103", "Atestado Medico", "medical", "Marina Costa", "review")
            ];
        }

        EnsureConfigured(_options.DocumentsBaseUrl, "DocumentsBaseUrl");
        var response = await _httpClient.GetFromJsonAsync<IReadOnlyCollection<DocumentsServiceDocumentDto>>("", JsonOptions, cancellationToken)
            ?? [];

        return response.Select(MapToSummary).ToList();
    }

    public async Task<DocumentDetailDto?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UseDocumentsMocks)
        {
            var documents = await ListAsync(cancellationToken);
            return documents
                .Where(x => x.Id == id)
                .Select(x => new DocumentDetailDto(
                    x.Id,
                    x.Title,
                    x.Category,
                    x.Owner,
                    x.Status,
                    DateTimeOffset.UtcNow.AddMinutes(-30),
                    "mock-documents-service"))
                .FirstOrDefault();
        }

        EnsureConfigured(_options.DocumentsBaseUrl, "DocumentsBaseUrl");
        var response = await _httpClient.GetFromJsonAsync<DocumentsServiceDocumentDto>($"{id}", JsonOptions, cancellationToken);
        return response is null ? null : MapToDetail(response);
    }

    public Task<JsonNode?> CreateAsync(JsonObject payload, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UseDocumentsMocks)
        {
            payload["id"] = 999;
            payload["source"] = "mock-documents-service";
            payload["createdAtUtc"] = DateTimeOffset.UtcNow;
            return Task.FromResult<JsonNode?>(payload);
        }

        return SendAsync(HttpMethod.Post, "", payload, cancellationToken);
    }

    public Task<JsonNode?> UpdateAsync(string id, JsonObject payload, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UseDocumentsMocks)
        {
            payload["id"] = id;
            payload["source"] = "mock-documents-service";
            payload["updatedAtUtc"] = DateTimeOffset.UtcNow;
            return Task.FromResult<JsonNode?>(payload);
        }

        return SendAsync(HttpMethod.Put, $"{id}", payload, cancellationToken);
    }

    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UseDocumentsMocks)
        {
            await Task.CompletedTask;
            return;
        }

        EnsureConfigured(_options.DocumentsBaseUrl, "DocumentsBaseUrl");
        using var response = await _httpClient.DeleteAsync($"{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<JsonNode?> SendAsync(HttpMethod method, string path, JsonObject payload, CancellationToken cancellationToken)
    {
        EnsureConfigured(_options.DocumentsBaseUrl, "DocumentsBaseUrl");
        using var request = new HttpRequestMessage(method, path)
        {
            Content = new StringContent(payload.ToJsonString(), Encoding.UTF8, "application/json")
        };
        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return JsonNode.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
    }

    private static void EnsureConfigured(string? url, string settingName)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            throw new InvalidOperationException($"Configure DownstreamServices:{settingName} para usar o BFF sem mocks.");
        }
    }

    private static DocumentSummaryDto MapToSummary(DocumentsServiceDocumentDto document)
    {
        return new DocumentSummaryDto(
            document.Id,
            document.Title,
            document.Category,
            document.OwnerName,
            document.Status);
    }

    private static DocumentDetailDto MapToDetail(DocumentsServiceDocumentDto document)
    {
        return new DocumentDetailDto(
            document.Id,
            document.Title,
            document.Category,
            document.OwnerName,
            document.Status,
            document.UpdatedAtUtc,
            "documents-mongodb-service");
    }

    private sealed record DocumentsServiceDocumentDto(
        string Id,
        string Title,
        string Category,
        string OwnerId,
        string OwnerName,
        string Status,
        string Content,
        IReadOnlyCollection<string> Tags,
        DateTimeOffset CreatedAtUtc,
        DateTimeOffset UpdatedAtUtc);
}
