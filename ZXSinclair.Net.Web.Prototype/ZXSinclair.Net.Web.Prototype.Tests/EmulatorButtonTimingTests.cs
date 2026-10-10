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
    private sealed record Measurement(double HandlerMs, double WallMs);
    private sealed record Statistics(double Min, double Median, double P95, double Max, double Mean);
    private sealed record ButtonReport(string Name, Measurement Cold, Measurement[] Samples, Statistics Handler, Statistics Wall);

    [Fact]
    [Trait("Category", "Browser")]
    public async Task MeasureButtonsAsync()
    {
        string? configuredIterations = Environment.GetEnvironmentVariable("ZX_TIMING_ITERATIONS");
        int iterations = 30;
        if (configuredIterations != null && (!int.TryParse(configuredIterations, out iterations) || iterations < 1))
            throw new ArgumentException("ZX_TIMING_ITERATIONS must be a positive integer.");
        var errors = new ConcurrentQueue<string>();
        await using var context = await fixture.Browser.NewContextAsync();
        var page = await context.NewPageAsync();
        page.SetDefaultTimeout(120_000);
        page.Console += (_, message) => { if (message.Type == "error") errors.Enqueue($"{message.Text} ({message.Location})"); };
        page.PageError += (_, error) => errors.Enqueue(error);
        page.Response += (_, response) => { if (response.Status >= 400) errors.Enqueue($"HTTP {response.Status}: {response.Url}"); };
        var reports = new List<ButtonReport>();
        try
        {
            // A fresh context prevents earlier worker startup or page timings from leaking into the cold sample.
            await page.GotoAsync(fixture.Url);
            await page.Locator("canvas[data-ready='1']").WaitForAsync();
            foreach (var (selector, name) in new[] { ("test2", "Test2"), ("test-put-image", "TestPutImage") })
            {
                var cold = await ClickAndMeasureAsync(page, selector, name);
                for (int i = 0; i < 5; i++) await ClickAndMeasureAsync(page, selector, name);
                var samples = new Measurement[iterations];
                for (int i = 0; i < samples.Length; i++) samples[i] = await ClickAndMeasureAsync(page, selector, name);
                reports.Add(new(name, cold, samples, Summarize(samples.Select(s => s.HandlerMs)), Summarize(samples.Select(s => s.WallMs))));
            }
            // Exercise component disposal before checking errors.
            await page.EvaluateAsync("async () => { const m = await import('./Components/Emulator.razor.js'); m.dispose(); }");
        }
        finally
        {
            await page.CloseAsync();
            foreach (string error in errors) output.WriteLine($"Browser error: {error}");
        }
        Assert.Empty(errors);
        output.WriteLine($"URL: {fixture.Url}; {fixture.Channel} {fixture.Browser.Version}; headed={fixture.Headed}");
        output.WriteLine(fixture.Configuration);
        output.WriteLine($"Warmup: 5; measured iterations: {iterations}; all values in ms; compositor excluded.");
        output.WriteLine("Button | clock | cold | min | median | p95 | max | mean");
        foreach (var report in reports)
        {
            WriteStatistics(report.Name, "handler", report.Cold.HandlerMs, report.Handler);
            WriteStatistics(report.Name, "Playwright wall", report.Cold.WallMs, report.Wall);
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
            Iterations = iterations,
            Warmup = 5,
            Unit = "ms",
            CompositorIncluded = false,
            Buttons = reports
        }, new JsonSerializerOptions { WriteIndented = true }));
        output.WriteLine($"JSON: {resultPath}");
    }

    private static async Task<Measurement> ClickAndMeasureAsync(IPage page, string selector, string name)
    {
        int count = await page.EvaluateAsync<int>("name => (globalThis.__zxTimings ?? []).filter(t => t.name === name).length", name);
        long start = Stopwatch.GetTimestamp();
        await page.GetByTestId(selector).ClickAsync();
        await using var completed = await page.WaitForFunctionAsync("({name, count}) => (globalThis.__zxTimings ?? []).filter(t => t.name === name).length > count", new { name, count });
        double wallMs = Stopwatch.GetElapsedTime(start).TotalMilliseconds;
        double handlerMs = await page.EvaluateAsync<double>("({name, count}) => globalThis.__zxTimings.filter(t => t.name === name)[count].ms", new { name, count });
        Assert.True(double.IsFinite(handlerMs) && handlerMs >= 0, $"Invalid timing for {name}: {handlerMs}");
        int[] pixel = await page.EvaluateAsync<int[]>("() => Array.from(document.querySelector('canvas').getContext('2d').getImageData(0, 0, 1, 1).data)");
        Assert.Equal(0, pixel[1]);
        Assert.Equal(0, pixel[2]);
        Assert.Equal(255, pixel[3]);
        return new(handlerMs, wallMs);
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
