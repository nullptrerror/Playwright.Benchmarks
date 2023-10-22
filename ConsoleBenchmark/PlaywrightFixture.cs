using Microsoft.Playwright;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ConsoleBenchmark;

public sealed class PlaywrightFixture : IAsyncDisposable
{
    public readonly IPlaywright Playwright = null!;
    public readonly IBrowser Browser = null!;
    public readonly IBrowserContext Context = null!;
    public readonly IPage Page = null!;
    public readonly IResponse GotoFlappyBirdIOResponse = null!;
    public PlaywrightFixture(IPlaywright playwright, IBrowser browser, IBrowserContext context, IPage page, IResponse gotoFlappyBirdIOResponse)
    {
        Playwright = playwright;
        Browser = browser;
        Context = context;
        Page = page;
        GotoFlappyBirdIOResponse = gotoFlappyBirdIOResponse;
    }
    public static async Task<PlaywrightFixture> CreateAsync(string url = ConsoleConstants.Urls.FlappyBirdIO)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(ConsoleConstants.PlaywrightOptions.BrowserTypeLaunchOptions);
        var context = await browser.NewContextAsync(ConsoleConstants.PlaywrightOptions.BrowserNewContextOptions);
        var page = await context.NewPageAsync();
        var gotoFlappyBirdIOResponse = await page.GotoAsync(url) ?? throw new System.Net.Http.HttpRequestException(url);

        return new(playwright: playwright, // playwright,
                   browser: browser, // browser,
                   context: context, // context,
                   page: page, // page,
                   gotoFlappyBirdIOResponse: gotoFlappyBirdIOResponse); // gotoFlappyBirdIOResponse);
    }
    public async ValueTask DisposeAsync()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
    public async Task<JsonElement> Expression1JsonElementEvaluateAsync() => await Page.EvaluateAsync<JsonElement>(ConsoleConstants.GameStateConstants.GameStateJsObjectExpression);
    public async Task<string> Expression2StringEvaluateAsync() => await Page.EvaluateAsync<string>(ConsoleConstants.GameStateConstants.GameStateJsStringExpression)!;
    public async Task<GameState> Expression2StringDeserializeEvaluateAsync() => JsonSerializer.Deserialize<GameState>(await Page.EvaluateAsync<string>(ConsoleConstants.GameStateConstants.GameStateJsStringExpression), ConsoleConstants.PlaywrightOptions.QuickJsonSerializerOptions)!;
    public async Task<GameState> Expression1JsonElementDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await Page.EvaluateAsync<JsonElement>(ConsoleConstants.GameStateConstants.GameStateJsObjectExpression);
        bool wbp = false;
        int wct = 0;
        bool wd = false;
        foreach (JsonProperty prop in jsonElement.EnumerateObject())
        {
            switch (prop.Name)
            {
                case ConsoleConstants.GameStateConstants.WBP:
                    wbp = prop.Value.GetBoolean();
                    break;
                case ConsoleConstants.GameStateConstants.WCT:
                    wct = prop.Value.GetInt32();
                    break;
                case ConsoleConstants.GameStateConstants.WD:
                    wd = prop.Value.GetBoolean();
                    break;
            }
        }
        return new GameState(wbp, wct, wd);
    }
    public async Task<GameState> Expression1JsonElementGetPropertyDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await Page.EvaluateAsync<JsonElement>(ConsoleConstants.GameStateConstants.GameStateJsObjectExpression);
        return new GameState(jsonElement.GetProperty(ConsoleConstants.GameStateConstants.WBP).GetBoolean(),
            jsonElement.GetProperty(ConsoleConstants.GameStateConstants.WCT).GetInt32(),
            jsonElement.GetProperty(ConsoleConstants.GameStateConstants.WD).GetBoolean());
    }
}
public sealed class GameState
{
    [JsonPropertyName(ConsoleConstants.GameStateConstants.WBP)]
    public bool WBP { get; private set; }

    [JsonPropertyName(ConsoleConstants.GameStateConstants.WCT)]
    public int WCT { get; private set; }
    [JsonPropertyName(ConsoleConstants.GameStateConstants.WD)]
    public bool WD { get; private set; }
    [JsonConstructor]
    public GameState(bool WBP, int WCT, bool WD)
    {
        this.WBP = WBP;
        this.WCT = WCT;
        this.WD = WD;
    }
}
public sealed class ConsoleConstants
{
    public sealed class Urls
    {
        public const string FlappyBirdIO = "https://flappybird.io/";
    }
    public sealed class GameStateConstants
    {
        public const string WBP = nameof(WBP);
        public const string WCT = nameof(WCT);
        public const string WD = nameof(WD);
        public const string GameStateJsObjectExpression = "(()=>{return{WBP:window.bird.paused,WCT:window.counter.text,WD:window.dead};})()";
        public const string GameStateJsStringExpression = "(()=>{return JSON.stringify({WBP: window.bird.paused,WCT: window.counter.text,WD: window.dead});})()";
    }
    public sealed class PlaywrightOptions
    {
        public static readonly PageScreenshotOptions PageScreenshotOptions = new() { FullPage = true };
        public static readonly BrowserTypeLaunchOptions BrowserTypeLaunchOptions = new() { Headless = false };
        public static readonly BrowserNewContextOptions BrowserNewContextOptions = new() { ScreenSize = new() { Width = 1280, Height = 720 } };
        public static readonly JsonSerializerOptions QuickJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = false, // Assuming our JSON property names always match the C# model
            DefaultBufferSize = 512,             // Adjust based on typical JSON size
            MaxDepth = 1                         // Adjust based on typical JSON structure depth
        };
    }
}