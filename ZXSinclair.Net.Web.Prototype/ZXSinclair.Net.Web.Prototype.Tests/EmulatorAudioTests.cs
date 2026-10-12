#region LICENSE
/*
    ZXSinclair Emulador ZX Computers make in .Net and .Net CORE
    Copyright (C) 2016 Oscar Hernandez Bano
    This file is part of ZXSincalir.Net.
    ZXSincalir.Net is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.*/
#endregion

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using Xunit.Abstractions;

namespace ZXSinclair.Net.Web.Prototype.Tests;

[Collection("Prototype browser")]
public sealed class EmulatorAudioTests(PrototypeServerFixture fixture, ITestOutputHelper output)
{
    [Theory]
    [InlineData(3)]
    [InlineData(2)]
    [Trait("Category", "Browser")]
    public async Task AudioConsumptionDrivesFramesAndStopReturnsPool(int target)
    {
        int seconds = 3;
        string? configured = Environment.GetEnvironmentVariable("ZX_AUDIO_SECONDS");
        if (configured != null && (!int.TryParse(configured, out seconds) || seconds < 1))
            throw new ArgumentException("ZX_AUDIO_SECONDS must be a positive integer.");
        var errors = new ConcurrentQueue<string>();
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        page.Console += (_, message) => { if (message.Type == "error") errors.Enqueue(message.Text); };
        page.PageError += (_, error) => errors.Enqueue(error);
        page.Response += (_, response) => { if (response.Status >= 400) errors.Enqueue($"HTTP {response.Status}: {response.Url}"); };
        async Task<JsonElement> Stats() => JsonSerializer.Deserialize<JsonElement>(await page.EvaluateAsync<string>(
            "async () => JSON.stringify(await (await import('./Components/Emulator.razor.js')).audioStats())"));
        async Task Toggle(string state)
        {
            await page.GetByTestId("test-audio").ClickAsync();
            await page.Locator($"[data-testid='test-audio'][data-audio-state='{state}']:not(:disabled)").WaitForAsync();
        }
        try
        {
            var url = new UriBuilder(fixture.Url) { Query = $"audioFrames={target}" };
            await page.GotoAsync(url.Uri.AbsoluteUri);
            await page.Locator("[data-testid='main-canvas'][data-ready='1']").WaitForAsync();
            await Toggle("running");
            JsonElement before = await Stats();
            long start = Stopwatch.GetTimestamp();
            await Task.Delay(TimeSpan.FromSeconds(seconds));
            JsonElement stats = await Stats();
            double elapsed = Stopwatch.GetElapsedTime(start).TotalSeconds;
            int rate = stats.GetProperty("sampleRate").GetInt32();
            long consumed = stats.GetProperty("consumed").GetInt64();
            long delta = consumed - before.GetProperty("consumed").GetInt64();
            Assert.InRange(delta, elapsed * rate * 0.75, elapsed * rate * 1.25);
            Assert.True(stats.GetProperty("frames").GetInt64() > target);
            Assert.Equal(0, stats.GetProperty("dropped").GetInt32());
            Assert.Equal("running", stats.GetProperty("contextState").GetString());
            Assert.Equal(JsonValueKind.Null, stats.GetProperty("error").ValueKind);
            Assert.InRange(stats.GetProperty("frequency").GetDouble(), 261.6256 * 0.98, 261.6256 * 1.02);
            long produced = stats.GetProperty("produced").GetInt64();
            Assert.InRange(produced - consumed, 0, (target + 2) * (long)Math.Ceiling(rate / 50.0));
            Assert.Equal(stats.GetProperty("frames").GetInt64() * rate / 50, produced);
            output.WriteLine($"{fixture.Channel} {fixture.Browser.Version}; headed={fixture.Headed}; {fixture.Configuration}");
            output.WriteLine(stats.ToString());
            string resultPath = Path.Combine(fixture.Root, "TestResults", "audio-loop.json");
            Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
            var reports = File.Exists(resultPath)
                ? JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(await File.ReadAllTextAsync(resultPath))!
                : new Dictionary<string, JsonElement>();
            reports[target.ToString()] = JsonSerializer.SerializeToElement(new
            {
                TimestampUtc = DateTimeOffset.UtcNow, fixture.Url, fixture.Configuration,
                Browser = $"{fixture.Channel} {fixture.Browser.Version}", fixture.Headed, Seconds = elapsed, Stats = stats
            });
            await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(reports, new JsonSerializerOptions { WriteIndented = true }));
            await Toggle("stopped");
            JsonElement stopped = await Stats();
            Assert.Equal(target + 1, stopped.GetProperty("free").GetInt32());
            Assert.Equal(0, stopped.GetProperty("queued").GetInt32());
            Assert.Equal(stopped.GetProperty("produced").GetInt64(), stopped.GetProperty("consumed").GetInt64());
            await Task.Delay(200);
            Assert.Equal(stopped.GetProperty("frames").GetInt64(), (await Stats()).GetProperty("frames").GetInt64());
            await Toggle("running");
            await Task.Delay(300);
            Assert.True((await Stats()).GetProperty("frames").GetInt64() > target);
            await Toggle("stopped");
            Assert.Equal(target + 1, (await Stats()).GetProperty("free").GetInt32());
        }
        finally
        {
            await page.EvaluateAsync("async () => { await (await import('./Components/Emulator.razor.js')).dispose(); }");
            await page.CloseAsync();
            foreach (string error in errors) output.WriteLine(error);
        }
        Assert.Empty(errors);
    }
}
