namespace GestaoRH.Infrastructure.Configuration;

public class DownstreamServicesOptions
{
    public const string SectionName = "DownstreamServices";

    public bool UseMocks { get; set; } = true;
    public string PeopleBaseUrl { get; set; } = string.Empty;
    public string DocumentsBaseUrl { get; set; } = string.Empty;
    public string FunctionBaseUrl { get; set; } = string.Empty;
    public string FunctionSummaryPath { get; set; } = "api/enrichment-summary";
}
