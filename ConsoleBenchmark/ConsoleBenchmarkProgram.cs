using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Microsoft.Playwright;
using System.Text.Json;
using BenchmarkDotNet.Engines;

namespace ConsoleBenchmark;

public sealed class ConsoleBenchmarkProgram
{
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
    warmupCount: 50,
    iterationCount: 2,
    invocationCount: 2000,
    id: "Throughput @ HostProcess")]
public class PlaywrightBenchmark
{
    public PlaywrightFixture LocalPlaywrightFixture { get; private set; } = null!;
    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;
    public IBrowserContext Context { get; private set; } = null!;
    public IPage Page { get; private set; } = null!;
    public IResponse? GotoFlappyBirdIOResponse { get; private set; } = null!;
    [GlobalSetup]
    public async Task GlobalSetup()
    {
        LocalPlaywrightFixture = await PlaywrightFixture.CreateAsync();
        Playwright = LocalPlaywrightFixture.Playwright;
        Browser = LocalPlaywrightFixture.Browser;
        Context = LocalPlaywrightFixture.Context;
        Page = LocalPlaywrightFixture.Page;
        GotoFlappyBirdIOResponse = LocalPlaywrightFixture.GotoFlappyBirdIOResponse;
    }
    [GlobalCleanup]
    public async Task GlobalCleanup()
    {
        await LocalPlaywrightFixture.DisposeAsync();
    }
    [Benchmark]
    public async Task<JsonElement> Expression1JsonElementEvaluateAsync() => await Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
    [Benchmark]
    public async Task<string> Expression2StringEvaluateAsync() => await Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression)!;
    [Benchmark]
    public async Task<GameState> Expression2StringDeserializeEvaluateAsync() => JsonSerializer.Deserialize<GameState>(await Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression), FixtureConstants.PlaywrightOptions.QuickJsonSerializerOptions)!;
    [Benchmark]
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
    [Benchmark]
    public async Task<GameState> Expression1JsonElementGetPropertyDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
        return new GameState(jsonElement.GetProperty(FixtureConstants.GameStateConstants.WBP).GetBoolean(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WCT).GetInt32(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WD).GetBoolean());
    }

    [Benchmark]
    public async Task<JsonElement> LocalExpression1JsonElementEvaluateAsync() => await LocalPlaywrightFixture.Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
    [Benchmark]
    public async Task<string> LocalExpression2StringEvaluateAsync() => await LocalPlaywrightFixture.Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression)!;
    [Benchmark]
    public async Task<GameState> LocalExpression2StringDeserializeEvaluateAsync() => JsonSerializer.Deserialize<GameState>(await LocalPlaywrightFixture.Page.EvaluateAsync<string>(FixtureConstants.GameStateConstants.GameStateJsStringExpression), FixtureConstants.PlaywrightOptions.QuickJsonSerializerOptions)!;
    [Benchmark]
    public async Task<GameState> LocalExpression1JsonElementDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await LocalPlaywrightFixture.Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
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
    [Benchmark]
    public async Task<GameState> LocalExpression1JsonElementGetPropertyDeserializeEvaluateAsync()
    {
        JsonElement jsonElement = await LocalPlaywrightFixture.Page.EvaluateAsync<JsonElement>(FixtureConstants.GameStateConstants.GameStateJsObjectExpression);
        return new GameState(jsonElement.GetProperty(FixtureConstants.GameStateConstants.WBP).GetBoolean(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WCT).GetInt32(),
            jsonElement.GetProperty(FixtureConstants.GameStateConstants.WD).GetBoolean());
    }
}

