namespace GestaoRH.Application.Features.Bff.Common;

public record PersonSummaryDto(
    int Id,
    string Name,
    string Role,
    string Department,
    string Email,
    string Status);

public record PersonDetailDto(
    int Id,
    string Name,
    string Role,
    string Department,
    string Email,
    string Status,
    DateTimeOffset LastUpdatedAtUtc,
    string Source);

public record DocumentSummaryDto(
    int Id,
    string Title,
    string Category,
    string Owner,
    string Status);

public record DocumentDetailDto(
    int Id,
    string Title,
    string Category,
    string Owner,
    string Status,
    DateTimeOffset LastUpdatedAtUtc,
    string Source);

public record EnrichmentSummaryDto(
    string Message,
    int TotalPeople,
    int TotalDocuments,
    DateTimeOffset GeneratedAtUtc,
    string Source);

public record AggregatedDataResponseDto(
    IReadOnlyCollection<PersonSummaryDto> People,
    IReadOnlyCollection<DocumentSummaryDto> Documents,
    EnrichmentSummaryDto FunctionData,
    DateTimeOffset AggregatedAtUtc,
    string Client);
