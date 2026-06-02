namespace GestaoRH.Domain.Interfaces;

public interface IPdfService
{
    Task<string> HtmlParaPdfBase64Async(string html);
}
