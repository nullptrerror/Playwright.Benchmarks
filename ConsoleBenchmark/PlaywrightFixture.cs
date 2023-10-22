using Microsoft.Playwright;
using System.Text.Json.Serialization;
using System.Text.Json;
using NUlid;
using System.Reflection;

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
    public static async Task<PlaywrightFixture> CreateAsync(string url = FixtureConstants.Urls.FlappyBirdIO)
    {
        var playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        var browser = await playwright.Chromium.LaunchAsync(FixtureConstants.PlaywrightOptions.BrowserTypeLaunchOptions);
        var context = await browser.NewContextAsync(FixtureConstants.PlaywrightOptions.BrowserNewContextOptions);
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
    public async Task<JsonElement> Expression1JsonElementEvaluateAsync() => await Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
    public async Task<string> Expression2StringEvaluateAsync() => await Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression)!;
    public async Task<GameState> Expression2StringDeserializeEvaluateAsync() => JsonSerializer.Deserialize<GameState>(await Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression), FixtureConstants.PlaywrightOptions.QuickJsonSerializerOptions)!;
    public async Task<GameState> Expression1JsonElementDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
        bool wbp = false;
        int wct = 0;
        bool wd = false;
        foreach (JsonProperty prop in jsonElement.EnumerateObject())
        {
            switch (prop.Name)
            {
                case FixtureConstants.GameStateConstants.WBP:
                    wbp = prop.Value.GetBoolean();
                    break;
                case FixtureConstants.GameStateConstants.WCT:
                    wct = prop.Value.GetInt32();
                    break;
                case FixtureConstants.GameStateConstants.WD:
                    wd = prop.Value.GetBoolean();
                    break;
            }
        }
        return new GameState(wbp, wct, wd);
    }
    public async Task<GameState> Expression1JsonElementGetPropertyDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
        return new GameState(jsonElement.GetProperty(FixtureConstants.GameStateConstants.WBP).GetBoolean(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WCT).GetInt32(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WD).GetBoolean());
    }
    public async Task<byte[]> ScreenshotBytesAsync() => await Page.ScreenshotAsync(FixtureConstants.PlaywrightOptions.PageScreenshotOptions);
    public async Task<byte[]> QuerySelectorAsyncScreenshotAsync() => await (await Page.QuerySelectorAsync(FixtureConstants.GameStateConstants.QuerySelector) ?? throw new NullReferenceException(FixtureConstants.GameStateConstants.QuerySelector)).ScreenshotAsync();
    public async Task<string> ScreenshotSaveAsync() {
        Ulid newUlid = Ulid.NewUlid();
        // save the screenshot in the same directory as this dll not the consuming dll
        string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? throw new NullReferenceException(), newUlid.ToString()+FixtureConstants.ScreenshotType.Png);
        PageScreenshotOptions pageScreenshotOptions = new() { Path = path, FullPage = true, Type = ScreenshotType.Png };
        await Page.ScreenshotAsync(pageScreenshotOptions);
        return pageScreenshotOptions.Path;
    }
}
public sealed class GameState
{
    [JsonPropertyName(FixtureConstants.GameStateConstants.WBP)]
    public bool WBP { get; private set; }

    [JsonPropertyName(FixtureConstants.GameStateConstants.WCT)]
    public int WCT { get; private set; }
    [JsonPropertyName(FixtureConstants.GameStateConstants.WD)]
    public bool WD { get; private set; }
    [JsonConstructor]
    public GameState(bool WBP, int WCT, bool WD)
    {
        this.WBP = WBP;
        this.WCT = WCT;
        this.WD = WD;
    }
}
public sealed class FixtureConstants
{
    public sealed class ScreenshotType
    {
        public const string Png = ".png";
    }
    public sealed class Urls
    {
        public const string FlappyBirdIO = "https://flappybird.io/";
    }
    public sealed class GameStateConstants
    {
        public const string WBP = nameof(WBP);
        public const string WCT = nameof(WCT);
        public const string WD = nameof(WD);
        public const string QuerySelector = "#testCanvas";
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