using Xunit;
using Microsoft.Playwright;
using System.Text.Json;

namespace PlaywrightTests;

public class FrontendE2ETest
{

    [Fact]
    public async Task UpdateInvoice_FromUI_PersistsToBackend()
    {
        var baseUrl = Environment.GetEnvironmentVariable("E2E_BASE_URL") ?? "http://nginx";

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        await page.GotoAsync(baseUrl, new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        await page.WaitForSelectorAsync("[data-testid='invoice-amount']", new() { Timeout = 60000 });

        await page.ClickAsync("[data-testid='invoice-amount']");
        await page.FillAsync("[data-testid='invoice-input']", "123,45");

        // Wait for POST completion (this implies SaveChangesAsync finished)
        var postTask = page.WaitForResponseAsync(r =>
            r.Url.Contains("/api/invoices") &&
            r.Request.Method == "POST" &&
            r.Ok);

        await page.ClickAsync("[data-testid='invoice-submit']");
        await postTask;

        // Verify backend
        var api = await playwright.APIRequest.NewContextAsync();
        var response = await api.GetAsync($"{baseUrl}/api/invoices/current");
        Assert.True(response.Ok);

        var json = JsonSerializer.Deserialize<JsonElement>(await response.TextAsync());
        Assert.Equal(123.45m, json.GetProperty("amount").GetDecimal());

        // Verify UI (tolerate locale formatting)
        await page.WaitForFunctionAsync(@"
            () => {
            const el = document.querySelector('[data-testid=""invoice-amount""]');
            if (!el) return false;
            const t = (el.textContent || '').replace(/\s/g,'');
            return t.includes('123,45EUR') || t.includes('123.45EUR');
            }", null, new() { Timeout = 60000 });
    }
}
