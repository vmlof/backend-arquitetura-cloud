using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace GestaoRH.Services;

public class PdfService
{
    private static bool _browserBaixado = false;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    // Garante que o Chromium está disponível (baixa uma vez ao iniciar)
    public static async Task InicializarAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!_browserBaixado)
            {
                var fetcher = new BrowserFetcher();
                await fetcher.DownloadAsync();
                _browserBaixado = true;
            }
        }
        finally { _lock.Release(); }
    }

    public async Task<string> HtmlParaPdfBase64Async(string html)
    {
        await InicializarAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args     = ["--no-sandbox", "--disable-setuid-sandbox", "--disable-dev-shm-usage"],
        });

        await using var page = await browser.NewPageAsync();

        // FIX: PuppeteerSharp v20+ usa WaitUntilNavigation.Networkidle0 (minúsculo)
        // A constante NetworkIdle0 foi renomeada. Use o enum correto da versão instalada:
        await page.SetContentAsync(html, new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.Networkidle0]
        });

        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format          = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions   = new MarginOptions
            {
                Top    = "30mm",
                Bottom = "25mm",
                Left   = "20mm",
                Right  = "20mm",
            }
        });

        return Convert.ToBase64String(pdfBytes);
    }
}
