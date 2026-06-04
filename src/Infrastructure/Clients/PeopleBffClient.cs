using GestaoRH.Application.Common.Interfaces;
using GestaoRH.Application.Features.Bff.Common;
using GestaoRH.Infrastructure.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GestaoRH.Infrastructure.Clients;

public class PeopleBffClient : IBffPeopleClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _httpClient;
    private readonly DownstreamServicesOptions _options;

    public PeopleBffClient(HttpClient httpClient, IOptions<DownstreamServicesOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyCollection<PersonSummaryDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UsePeopleMocks)
        {
            return
            [
                new(1, "Ana Souza", "Analista RH", "People", "ana.souza@bff.local", "active"),
                new(2, "Carlos Lima", "Tech Recruiter", "People", "carlos.lima@bff.local", "active"),
                new(3, "Marina Costa", "BP RH", "Operations", "marina.costa@bff.local", "inactive")
            ];
        }

        EnsureConfigured(_options.PeopleBaseUrl, "PeopleBaseUrl");
        return await _httpClient.GetFromJsonAsync<IReadOnlyCollection<PersonSummaryDto>>("", JsonOptions, cancellationToken)
            ?? [];
    }

    public async Task<PersonDetailDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UsePeopleMocks)
        {
            var people = await ListAsync(cancellationToken);
            return people
                .Where(x => x.Id == id)
                .Select(x => new PersonDetailDto(
                    x.Id,
                    x.Name,
                    x.Role,
                    x.Department,
                    x.Email,
                    x.Status,
                    "Responsável pelo departamento de " + x.Department + ".",
                    DateTimeOffset.UtcNow.AddMinutes(-15),
                    "mock-people-service"))
                .FirstOrDefault();
        }

        EnsureConfigured(_options.PeopleBaseUrl, "PeopleBaseUrl");
        return await _httpClient.GetFromJsonAsync<PersonDetailDto>($"{id}", JsonOptions, cancellationToken);
    }

    public Task<JsonNode?> CreateAsync(JsonObject payload, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UsePeopleMocks)
        {
            payload["id"] = 999;
            payload["source"] = "mock-people-service";
            payload["createdAtUtc"] = DateTimeOffset.UtcNow;
            return Task.FromResult<JsonNode?>(payload);
        }

        return SendAsync(HttpMethod.Post, "", payload, cancellationToken);
    }

    public Task<JsonNode?> UpdateAsync(int id, JsonObject payload, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UsePeopleMocks)
        {
            payload["id"] = id;
            payload["source"] = "mock-people-service";
            payload["updatedAtUtc"] = DateTimeOffset.UtcNow;
            return Task.FromResult<JsonNode?>(payload);
        }

        return SendAsync(HttpMethod.Put, $"{id}", payload, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        if (_options.UseMocks || _options.UsePeopleMocks)
        {
            await Task.CompletedTask;
            return;
        }

        EnsureConfigured(_options.PeopleBaseUrl, "PeopleBaseUrl");
        using var response = await _httpClient.DeleteAsync($"{id}", cancellationToken);
        response.EnsureSuccessStatusCode();
    }

    private async Task<JsonNode?> SendAsync(HttpMethod method, string path, JsonObject payload, CancellationToken cancellationToken)
    {
        EnsureConfigured(_options.PeopleBaseUrl, "PeopleBaseUrl");
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
}
