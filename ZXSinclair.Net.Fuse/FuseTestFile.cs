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

namespace ZXSinclair.Net.Fuse;

/// <summary>
/// Loads the FUSE Z80 test suite (<c>tests.in</c> and <c>tests.expected</c>, embedded in this assembly).
/// Malformed files throw <see cref="FormatException"/> with the line number, in Debug and Release.
/// </summary>
public static class FuseTestFile
{
    public const string InputResource = "tests.in";
    public const string ExpectedResource = "tests.expected";

    public static List<clsTestIn> LoadInputs() => ParseInputs(ReadResource(InputResource));

    public static Dictionary<string, clsTestExpected> LoadExpected() => ParseExpected(ReadResource(ExpectedResource));

    public static List<clsTestIn> ParseInputs(string[] lines)
    {
        var tests = new List<clsTestIn>();
        var i = 0;

        while (NextName(lines, ref i) is { } name)
        {
            var test = new clsTestIn();

            test.Base.Name = name;
            Parse(name, i, () =>
            {
                test.Base.Line1.read(Line(lines, ref i));
                test.Base.Line2.read(Line(lines, ref i));
                test.Base.Memories = clsTestMemory.Read(lines, ref i);
            });
            tests.Add(test);
        }

        return tests;
    }

    public static Dictionary<string, clsTestExpected> ParseExpected(string[] lines)
    {
        var tests = new Dictionary<string, clsTestExpected>();
        var i = 0;

        while (NextName(lines, ref i) is { } name)
        {
            var test = new clsTestExpected();

            test.Base.Name = name;
            Parse(name, i, () =>
            {
                test.Events = clsTestEvent.ReadEvents(lines, ref i);
                test.Base.Line1.read(Line(lines, ref i));
                test.Base.Line2.read(Line(lines, ref i));
                test.Base.Memories = clsTestMemory.Read(lines, ref i);
            });
            if (!tests.TryAdd(name, test))
                throw new FormatException($"Duplicate test '{name}' in {ExpectedResource}.");
        }

        return tests;
    }

    private static string[] ReadResource(string name)
    {
        using var stream = typeof(FuseTestFile).Assembly.GetManifestResourceStream(name)
            ?? throw new InvalidOperationException($"Embedded resource '{name}' not found.");
        using var reader = new StreamReader(stream);
        var lines = new List<string>();

        while (reader.ReadLine() is { } line)
            lines.Add(line);

        return lines.ToArray();
    }

    private static string? NextName(string[] lines, ref int i)
    {
        while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
            i++;

        return i < lines.Length ? lines[i++].Trim() : null;
    }

    private static string Line(string[] lines, ref int i) =>
        i < lines.Length ? lines[i++] : throw new FormatException("Unexpected end of file.");

    private static void Parse(string name, int line, Action parse)
    {
        try
        {
            parse();
        }
        catch (FormatException ex)
        {
            throw new FormatException($"Test '{name}' (line {line + 1}): {ex.Message}", ex);
        }
    }
}
