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

using System.Text;
using ZXSinclair.Net.Generate.Z80OpCodes.Emit;
using ZXSinclair.Net.Generate.Z80OpCodes.Patterns;
using ZXSinclair.Net.Generate.Z80OpCodes.Tables;

namespace ZXSinclair.Net.Generate.Z80OpCodes;

internal static class GeneratorApplication
{
    public static string FindRoot(string start)
    {
        for (var directory = new DirectoryInfo(start); directory is not null; directory = directory.Parent)
            if (File.Exists(Path.Combine(directory.FullName, "zxsinclair.net.slnx"))) return directory.FullName;
        throw new GeneratorException($"Cannot locate zxsinclair.net.slnx above '{start}'.");
    }

    public static int Run(string[] args, string workingDirectory, TextWriter output, TextWriter error)
    {
        try
        {
            string? target = null;
            var check = false;
            var verbose = false;
            for (var i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "--check": check = true; break;
                    case "--verbose": verbose = true; break;
                    case "--output":
                        if (target is not null || ++i == args.Length || args[i].StartsWith("--"))
                            throw new GeneratorException("--output requires one directory.");
                        target = Path.GetFullPath(args[i], workingDirectory);
                        break;
                    default: throw new GeneratorException($"Unknown option '{args[i]}'.");
                }
            }
            target ??= Path.Combine(FindRoot(workingDirectory), "ZXSinclair.Net.Core", "Z80", "Generated");
            // Complete parsing and pattern validation before writing any output.
            var dispatches = DispatchEmitter.Generate(OpcodeTableReader.ReadAll(), PatternCatalog.Default);
            CoverageReport.Write(dispatches, output, verbose);
            var different = false;
            foreach (var dispatch in dispatches)
            {
                var path = Path.Combine(target, dispatch.FileName);
                var existing = File.Exists(path) ? File.ReadAllText(path) : null;
                if (check)
                {
                    if (existing is null || SourceFormat.Normalize(existing) != SourceFormat.Normalize(dispatch.Text))
                    {
                        output.WriteLine($"Different or missing: {path}");
                        different = true;
                    }
                }
                else if (existing != dispatch.Text)
                {
                    Directory.CreateDirectory(target);
                    File.WriteAllText(path, dispatch.Text, new UTF8Encoding(false));
                }
            }
            return different ? 1 : 0;
        }
        catch (Exception exception) when (exception is GeneratorException or IOException or UnauthorizedAccessException or ArgumentException)
        {
            error.WriteLine(exception.Message);
            return 2;
        }
    }
}
