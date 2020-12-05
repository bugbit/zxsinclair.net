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

using System;
using System.IO;
using System.Reflection;
using ZXSinclair.Machines.Z80;

Assembly mAssembly = Assembly.GetExecutingAssembly();
string mAssDir = Path.GetDirectoryName(mAssembly.Location);

CreateLD_R_R1();

void CreateLD_R_R1()
{
    using (var pStream = new StreamWriter(Path.Combine(mAssDir, @"..\..\..\..\ZXSinclair.Shared\Machines\Z80\Regs.ldr_r1.cs"), false))
    {
        AddLicense(pStream);

        pStream.WriteLine
        (
@"
using System;

namespace ZXSinclair.Machines.Z80
{
    public partial class Regs
    {
        /// <summary>
        /// R=r1
        /// 00rrrrRRR
        /// </summary>
        /// <param name=""r_r1""></param>
        /// <returns></returns>
        public Action CreateLDR_R1(int r_r1)=> r_r1 switch
        {
"
        );

        for (var i = 0; i < OpCodes.Car_Rs.Length; i++)
        {
            for (var j = 0; j < OpCodes.Car_Rs.Length; j++)
            {
                pStream.WriteLine($"(OpCodes.R_{OpCodes.Car_Rs[i]} << 3) | OpCodes.R_{OpCodes.Car_Rs[j]} => () => {OpCodes.Car_Rs[i]} = {OpCodes.Car_Rs[j]},");
            }
        }
        pStream.WriteLine
        (
@"
        };
    }
}
"
        );
    }
}

void AddLicense(TextWriter argStream)
{
    argStream.WriteLine
    (
@"
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
"
    );
}
