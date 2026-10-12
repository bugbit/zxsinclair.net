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

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Versioning;

namespace ZXSinclair.Net.Web.Prototype.Workers;

struct Result
{
    public int Value { get; set; }
    public byte[]? Data { get; set; }
}


[SupportedOSPlatform("browser")]
public partial class WorkTest
{
    private static byte[]? _data1;
    private static readonly byte[] sFrame = new byte[320 * 240];
    private static readonly float[] sAudio = new float[4096];
    private static int sSampleRate;
    private static int sSampleRemainder;
    private static int sLastAudioLength;
    private static double sPhase;
    private static double sPhaseStep;

    [JSImport("pushAudio", "emulator")]
    private static partial void PushAudio([JSMarshalAs<JSType.MemoryView>] Span<byte> samples);

    [JSExport]
    internal static int InitAudio(int sampleRate)
    {
        if (sampleRate < 8000 || sampleRate > 192000)
            throw new ArgumentOutOfRangeException(nameof(sampleRate));
        sSampleRate = sampleRate;
        sSampleRemainder = 0;
        sLastAudioLength = 0;
        sPhase = 0;
        sPhaseStep = 2 * Math.PI * 261.6256 / sampleRate;
        return sampleRate / 50;
    }

    [JSExport]
    internal static int RunAudioFrame()
    {
        if (sSampleRate == 0) throw new InvalidOperationException("Audio is not initialized.");
        sSampleRemainder += sSampleRate;
        int length = sSampleRemainder / 50;
        sSampleRemainder %= 50;
        for (int i = 0; i < length; i++)
        {
            sAudio[i] = 0.25f * MathF.Sin((float)sPhase);
            sPhase += sPhaseStep;
            if (sPhase >= 2 * Math.PI) sPhase -= 2 * Math.PI;
        }
        sLastAudioLength = length;
        PushAudio(MemoryMarshal.AsBytes(sAudio.AsSpan(0, length)));
        return length;
    }

    [JSExport]
    internal static double MeasureLastAudioFrame()
    {
        double first = 0, last = 0;
        int crossings = 0;
        for (int i = 1; i < sLastAudioLength; i++)
        {
            if (sAudio[i - 1] <= 0 && sAudio[i] > 0)
            {
                last = i - 1 + (double)-sAudio[i - 1] / (sAudio[i] - sAudio[i - 1]);
                if (crossings++ == 0) first = last;
            }
        }
        return crossings > 1 ? (crossings - 1) * sSampleRate / (last - first) : 0;
    }

    [JSImport("present", "emulator")]
    private static partial void Present([JSMarshalAs<JSType.MemoryView>] Span<byte> frame);

    [JSExport]
    internal static int TestPresent()
    {
        int index = Random.Shared.Next(0, 16);
        sFrame.AsSpan().Fill((byte)index);
        Present(sFrame);
        return index;
    }

    [JSImport("createResult", "emulator")]
    private static partial JSObject CreateResult(int value, byte[] data);

    [JSExport]
    internal static JSObject Test()
    {
        return CreateResult(42, new byte[] { 1, 2, 3, 4, 5 });
    }

    [JSExport]
    internal static JSObject TestPutImage()
    {
        byte color = (byte)Random.Shared.Next(0, 255);
        byte[] data = _data1 ?? (_data1 = new byte[320 * 240 * 4]);

        for (int i = 0; i < data.Length; i += 4)
        {
            data[i] = color;     // Rojo; verde y azul quedan en 0.
            data[i + 3] = 255; // Alfa opaco.
        }

        return CreateResult(42, data);
    }
}
