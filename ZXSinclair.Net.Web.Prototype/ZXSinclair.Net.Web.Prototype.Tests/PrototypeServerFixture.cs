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
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;

namespace ZXSinclair.Net.Web.Prototype.Tests;

[CollectionDefinition("Prototype browser", DisableParallelization = true)]
public sealed class PrototypeBrowserCollection : ICollectionFixture<PrototypeServerFixture>;

public sealed class PrototypeServerFixture : IAsyncLifetime
{
    private Process? server;
    private IPlaywright? playwright;
    private readonly ConcurrentQueue<string> serverLog = new();
    public IBrowser Browser { get; private set; } = null!;
    public string Url { get; private set; } = "";
    public string Configuration { get; private set; } = "";
    public string Root { get; } = FindRoot();
    public string Channel { get; } = Environment.GetEnvironmentVariable("ZX_PLAYWRIGHT_CHANNEL") ?? "chromium";
    public bool Headed { get; } = Environment.GetEnvironmentVariable("ZX_PLAYWRIGHT_HEADED") == "1";

    private static string FindRoot()
    {
        for (DirectoryInfo? directory = new(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            if (File.Exists(Path.Combine(directory.FullName, "zxsinclair.net.slnx")))
                return directory.FullName;
        }
        throw new InvalidOperationException("Repository root not found.");
    }

    public async Task InitializeAsync()
    {
        try
        {
            string? externalUrl = Environment.GetEnvironmentVariable("ZX_PROTOTYPE_URL");
            if (!string.IsNullOrWhiteSpace(externalUrl))
            {
                var uri = new Uri(externalUrl, UriKind.Absolute);
                if (uri.Scheme != "http" && uri.Scheme != "https")
                    throw new ArgumentException("ZX_PROTOTYPE_URL must be HTTP or HTTPS.");
                Url = uri.AbsoluteUri;
                Configuration = "External server; build/AOT configuration unknown (record it separately).";
            }
            else
            {
                using var listener = new TcpListener(IPAddress.Loopback, 0);
                listener.Start();
                int port = ((IPEndPoint)listener.LocalEndpoint).Port;
                listener.Stop();
                Url = $"http://127.0.0.1:{port}";
                Configuration = "dotnet run -c Debug; interpreted WASM; Development";
                var start = new ProcessStartInfo("dotnet")
                {
                    WorkingDirectory = Root,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                foreach (string argument in new[] { "run", "--project", "ZXSinclair.Net.Web.Prototype/ZXSinclair.Net.Web.Prototype", "-c", "Debug", "--no-launch-profile", "--urls", Url })
                    start.ArgumentList.Add(argument);
                start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
                server = new Process { StartInfo = start };
                server.OutputDataReceived += (_, e) => { if (e.Data != null) serverLog.Enqueue(e.Data); };
                server.ErrorDataReceived += (_, e) => { if (e.Data != null) serverLog.Enqueue(e.Data); };
                server.Start();
                server.BeginOutputReadLine();
                server.BeginErrorReadLine();
            }
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(3) };
            var deadline = Stopwatch.StartNew();
            bool ready = false;
            while (deadline.Elapsed < TimeSpan.FromSeconds(120))
            {
                if (server?.HasExited == true)
                    throw new InvalidOperationException($"Server exited ({server.ExitCode}):\n{string.Join('\n', serverLog)}");
                try
                {
                    using var response = await client.GetAsync(Url);
                    ready = response.StatusCode == HttpStatusCode.OK;
                    if (ready) break;
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) { }
                await Task.Delay(250);
            }
            if (!ready) throw new TimeoutException($"Server not ready at {Url}:\n{string.Join('\n', serverLog)}");
            playwright = await Playwright.CreateAsync();
            if (Channel != "chromium" && Channel != "chrome" && Channel != "msedge")
                throw new ArgumentException("ZX_PLAYWRIGHT_CHANNEL must be chromium, chrome or msedge.");
            Browser = await playwright.Chromium.LaunchAsync(new() { Headless = !Headed, Channel = Channel == "chromium" ? null : Channel });
        }
        catch
        {
            await DisposeAsync();
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (Browser != null) await Browser.CloseAsync();
        }
        finally
        {
            playwright?.Dispose();
            if (server != null)
            {
                if (!server.HasExited)
                {
                    server.Kill(entireProcessTree: true);
                    await server.WaitForExitAsync();
                }
                server.Dispose();
                server = null;
            }
        }
    }
}
