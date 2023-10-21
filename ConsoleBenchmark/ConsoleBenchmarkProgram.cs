using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Playwright;
using System.Text.Json;
using System.Text.Json.Serialization;
using BenchmarkDotNet.Engines;

namespace ConsoleBenchmark;

public sealed class ConsoleBenchmarkProgram
{
    public static readonly JsonSerializerOptions QuickJsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = false, // Assuming our JSON property names always match the C# model
        DefaultBufferSize = 512,             // Adjust based on typical JSON size
        MaxDepth = 1                         // Adjust based on typical JSON structure depth
    };

    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<PlaywrightBenchmark>();
        // Write a summary to console
        Console.WriteLine(summary);
    }
}

[MemoryDiagnoser]
[SimpleJob(runStrategy: RunStrategy.Throughput,
    // runtimeMoniker: RuntimeMoniker.HostProcess,
    launchCount: 1,
    warmupCount: 5,
    iterationCount: 50,
    invocationCount: 1000,
    id: "Throughput @ HostProcess")]
public class PlaywrightBenchmark
{

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;
    public IResponse? GotoFlappyBirdIOResponse { get; private set; } = null!;
    [GlobalSetup]
    public async Task GlobalSetup()
    {
        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(ConsoleConstants.PlaywrightOptions.BrowserTypeLaunchOptions);
        Context = await Browser.NewContextAsync(ConsoleConstants.PlaywrightOptions.BrowserNewContextOptions);
        Page = await Context.NewPageAsync();
        GotoFlappyBirdIOResponse = await Page.GotoAsync(ConsoleConstants.Urls.FlappyBirdIO);
    }
    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await Page.CloseAsync();
        await Context.CloseAsync();
        await Browser.CloseAsync();
        Playwright.Dispose();
    }
    [Benchmark]
    public async Task<JsonElement> Expression1JsonElementEvaluateAsync() => await Page.EvaluateAsync<JsonElement>(ConsoleConstants.GameStateConstants.GameStateJsObjectExpression);
    [Benchmark]
    public async Task<string> Expression2StringEvaluateAsync() => await Page.EvaluateAsync<string>(ConsoleConstants.GameStateConstants.GameStateJsStringExpression)!;
    [Benchmark]
    public async Task<GameState> Expression2StringDeserializeEvaluateAsync() => JsonSerializer.Deserialize<GameState>(await Page.EvaluateAsync<string>(ConsoleConstants.GameStateConstants.GameStateJsStringExpression), ConsoleBenchmarkProgram.QuickJsonSerializerOptions)!;
    [Benchmark]
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
    [Benchmark]
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

    // public readonly byte[] Image = default!;

    [JsonConstructor]
    public GameState(bool WBP, int WCT, bool WD)
    {
        this.WBP = WBP;
        this.WCT = WCT;
        this.WD = WD;
    }
    // public GameState(GameState gameState, byte[] image) : this(gameState.WBP, gameState.WCT, gameState.WD) => Image = image;
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
    public sealed class PlaywrightOptions {
        public static readonly PageScreenshotOptions PageScreenshotOptions = new() { FullPage = true };
        public static readonly BrowserTypeLaunchOptions BrowserTypeLaunchOptions = new() { Headless = false };
        public static readonly BrowserNewContextOptions BrowserNewContextOptions = new() { ScreenSize = new() { Width = 1280, Height = 720 } };
    }
}