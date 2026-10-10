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
using Microsoft.Playwright;
using Xunit.Abstractions;

namespace ZXSinclair.Net.Web.Prototype.Tests;

[Collection("Prototype browser")]
public sealed class EmulatorButtonTimingTests(PrototypeServerFixture fixture, ITestOutputHelper output)
{
    private sealed record Measurement(double HandlerMs, double WallMs, double? WorkerMs = null);
    private sealed record Statistics(double Min, double Median, double P95, double Max, double Mean);
    private sealed record TimingEntry(double Ms, double? WorkerMs, int? Index, string? Backend);
    private sealed record ButtonReport(string Name, string ColdMeaning, string? Backend, string? Renderer,
        Measurement Cold, Measurement[] Samples, Statistics Handler, Statistics Wall, Statistics? Worker);

    [Fact]
    [Trait("Category", "Browser")]
    public async Task MeasureButtonsAsync()
    {
        string? configuredIterations = Environment.GetEnvironmentVariable("ZX_TIMING_ITERATIONS");
        int iterations = 30;
        if (configuredIterations != null && (!int.TryParse(configuredIterations, out iterations) || iterations < 1))
            throw new ArgumentException("ZX_TIMING_ITERATIONS must be a positive integer.");
        var reports = new List<ButtonReport>();
        await WithPageAsync(null, async page =>
        {
            reports.Add(await MeasureAsync(page, "test2", "Test2", "Test2", iterations,
                "First handler invocation in this page; main WASM already ready."));
            reports.Add(await MeasureAsync(page, "test-put-image", "TestPutImage", "TestPutImage", iterations,
                "First worker call including runtime startup; main runtime assets may be cached."));
        });
        foreach (string backend in new[] { "webgl2", "2d" })
        {
            await WithPageAsync(backend, async page =>
            {
                // The runtime is shared with TestPutImage. Warm it before timing canvas transfer/init.
                await ClickAndMeasureAsync(page, "test-put-image", "TestPutImage");
                reports.Add(await MeasureAsync(page, "test-present", "TestPresent", $"TestPresent ({backend})", iterations,
                    "First canvas transfer and graphics initialization; worker runtime prestarted by one TestPutImage.", backend));
            });
        }
        output.WriteLine($"URL: {fixture.Url}; {fixture.Channel} {fixture.Browser.Version}; headed={fixture.Headed}");
        output.WriteLine(fixture.Configuration);
        output.WriteLine($"Warmup: 5; measured iterations: {iterations}; all values in ms; compositor excluded.");
        output.WriteLine($"Headless SwiftShader opt-in flag: {!fixture.Headed}; actual WebGL renderer is recorded below.");
        output.WriteLine("Button | clock | cold | min | median | p95 | max | mean");
        foreach (var report in reports)
        {
            output.WriteLine($"{report.Name}: {report.ColdMeaning} Backend={report.Backend}; renderer={report.Renderer}");
            WriteStatistics(report.Name, "handler", report.Cold.HandlerMs, report.Handler);
            WriteStatistics(report.Name, "Playwright wall", report.Cold.WallMs, report.Wall);
            if (report.Worker != null) WriteStatistics(report.Name, "worker", report.Cold.WorkerMs!.Value, report.Worker);
        }
        string resultPath = Path.Combine(fixture.Root, "TestResults", "button-timings.json");
        Directory.CreateDirectory(Path.GetDirectoryName(resultPath)!);
        await File.WriteAllTextAsync(resultPath, JsonSerializer.Serialize(new
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            fixture.Url,
            fixture.Configuration,
            Browser = $"{fixture.Channel} {fixture.Browser.Version}",
            fixture.Headed,
            HeadlessSwiftShaderAllowed = !fixture.Headed,
            Iterations = iterations,
            Warmup = 5,
            Unit = "ms",
            CompositorIncluded = false,
            Buttons = reports
        }, new JsonSerializerOptions { WriteIndented = true }));
        output.WriteLine($"JSON: {resultPath}");
    }

    private async Task WithPageAsync(string? backend, Func<IPage, Task> measure)
    {
        var errors = new ConcurrentQueue<string>();
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        page.Console += (_, message) => { if (message.Type == "error") errors.Enqueue($"{message.Text} ({message.Location})"); };
        page.PageError += (_, error) => errors.Enqueue(error);
        page.Response += (_, response) => { if (response.Status >= 400) errors.Enqueue($"HTTP {response.Status}: {response.Url}"); };
        try
        {
            var url = new UriBuilder(fixture.Url);
            if (backend != null) url.Query = $"present={backend}";
            await page.GotoAsync(url.Uri.AbsoluteUri);
            await page.Locator("[data-testid='main-canvas'][data-ready='1']").WaitForAsync();
            await measure(page);
            // Dispose both the normal canvas and worker before checking errors.
            await page.EvaluateAsync("async () => { const m = await import('./Components/Emulator.razor.js'); m.dispose(); }");
        }
        finally
        {
            await page.CloseAsync();
            foreach (string error in errors) output.WriteLine($"Browser error: {error}");
        }
        Assert.Empty(errors);
    }

    private async Task<ButtonReport> MeasureAsync(IPage page, string selector, string timingName, string reportName,
        int iterations, string coldMeaning, string? requestedBackend = null)
    {
        var cold = await ClickAndMeasureAsync(page, selector, timingName);
        string? backend = null;
        string? renderer = null;
        if (requestedBackend != null)
        {
            backend = await page.GetByTestId("present-canvas").GetAttributeAsync("data-backend");
            renderer = await page.GetByTestId("present-canvas").GetAttributeAsync("data-renderer");
            Assert.True(backend == requestedBackend,
                $"Requested {requestedBackend}, got {backend}. WebGL2 unavailable or unexpected fallback; this result cannot be labelled {requestedBackend}.");
        }
        for (int i = 0; i < 5; i++) await ClickAndMeasureAsync(page, selector, timingName);
        var samples = new Measurement[iterations];
        for (int i = 0; i < samples.Length; i++) samples[i] = await ClickAndMeasureAsync(page, selector, timingName);
        return new(reportName, coldMeaning, backend, renderer, cold, samples,
            Summarize(samples.Select(s => s.HandlerMs)), Summarize(samples.Select(s => s.WallMs)),
            requestedBackend == null ? null : Summarize(samples.Select(s => s.WorkerMs!.Value)));
    }

    private static async Task<Measurement> ClickAndMeasureAsync(IPage page, string selector, string name)
    {
        int count = await page.EvaluateAsync<int>("name => (globalThis.__zxTimings ?? []).filter(t => t.name === name).length", name);
        long start = Stopwatch.GetTimestamp();
        await page.GetByTestId(selector).ClickAsync();
        await using var completed = await page.WaitForFunctionAsync("({name, count}) => (globalThis.__zxTimings ?? []).filter(t => t.name === name).length > count", new { name, count });
        double wallMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        string timingJson = await page.EvaluateAsync<string>("({name, count}) => JSON.stringify(globalThis.__zxTimings.filter(t => t.name === name)[count])", new { name, count });
        var timing = JsonSerializer.Deserialize<TimingEntry>(timingJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.True(double.IsFinite(timing.Ms) && timing.Ms >= 0, $"Invalid timing for {name}: {timing.Ms}");
        int[] expected;
        int[] pixel;
        if (name == "TestPresent")
        {
            Assert.True(timing.Index is >= 0 and < 16);
            Assert.True(timing.WorkerMs.HasValue && double.IsFinite(timing.WorkerMs.Value) && timing.WorkerMs.Value >= 0);
            int index = timing.Index!.Value;
            int level = (index & 8) != 0 ? 255 : 215;
            expected = [(index & 2) != 0 ? level : 0, (index & 4) != 0 ? level : 0, (index & 1) != 0 ? level : 0, 255];
            pixel = await page.EvaluateAsync<int[]>("async () => { const m = await import('./Components/Emulator.razor.js'); return m.readPresentPixel(); }");
            Assert.Equal(expected, pixel);
        }
        else
        {
            pixel = await page.EvaluateAsync<int[]>("() => Array.from(document.querySelector('[data-testid=main-canvas]').getContext('2d').getImageData(0, 0, 1, 1).data)");
            Assert.Equal(0, pixel[1]);
            Assert.Equal(0, pixel[2]);
            Assert.Equal(255, pixel[3]);
        }
        return new(timing.Ms, wallMs, timing.WorkerMs);
    }

    private static Statistics Summarize(IEnumerable<double> values)
    {
        double[] sorted = values.Order().ToArray();
        int middle = sorted.Length / 2;
        double median = sorted.Length % 2 == 0 ? (sorted[middle - 1] + sorted[middle]) / 2 : sorted[middle];
        return new(sorted[0], median, sorted[(int)Math.Ceiling(sorted.Length * 0.95) - 1], sorted[^1], sorted.Average());
    }

    private void WriteStatistics(string name, string clock, double cold, Statistics stats)
    {
        output.WriteLine($"{name} | {clock} | {cold:F3} | {stats.Min:F3} | {stats.Median:F3} | {stats.P95:F3} | {stats.Max:F3} | {stats.Mean:F3}");
    }
}
