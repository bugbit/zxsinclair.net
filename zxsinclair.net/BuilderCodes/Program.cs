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

//CreateLD_R_R1();
Z80OpCodesTest_CreateLD_R_R1();

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

void Z80OpCodesTest_CreateLD_R_R1()
{
    using (var pStream = new StreamWriter(Path.Combine(mAssDir, @"..\..\..\..\ZXSinclair.Test\Z80OpCodesTest.LD_R_R1.cs"), false))
    {
        AddLicense(pStream);
        pStream.WriteLine
       (
@"
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXSinclair.Machines.Z80;

namespace ZXSinclair.Test
{
    public partial class Z80OpCodesTest
    {
"
       );

        for (var i = 0; i < OpCodes.Car_Rs.Length; i++)
        {
            for (var j = 0; j < OpCodes.Car_Rs.Length; j++)
            {
                var pRegs = new[] { 1, 2, 3, 4, 5, 6, 7 };
                var pRegs2 = (int[])pRegs.Clone();

                pRegs2[i] = pRegs2[j];
                pStream.WriteLine
       (
$@"
        [TestMethod]
        public void LD_{OpCodes.Car_Rs[i]}_{OpCodes.Car_Rs[j]}_Method()
        {{
            var pRegs = mMachineTest.Regs;
            var pTStates = mMachineTest.TStates;

            pRegs.Reset();
            pRegs.A = {pRegs[0]};
            pRegs.B = {pRegs[1]};
            pRegs.C = {pRegs[2]};
            pRegs.D = {pRegs[3]};
            pRegs.E = {pRegs[4]};
            pRegs.H = {pRegs[5]};
            pRegs.L = {pRegs[6]};

            mMachineTest.Poke(0, OpCodes.LD_{OpCodes.Car_Rs[i]}_{OpCodes.Car_Rs[j]});
            mMachineTest.ExecInstruction();

            Assert.IsTrue(mMachineTest.TStates == pTStates + 4, ""TState"");
            Assert.IsTrue(pRegs.PC == 1, ""PC"");
            Assert.IsTrue(pRegs.A == {pRegs2[0]}, ""A"");
            Assert.IsTrue(pRegs.B == {pRegs2[1]}, ""B"");
            Assert.IsTrue(pRegs.C == {pRegs2[2]}, ""C"");
            Assert.IsTrue(pRegs.D == {pRegs2[3]}, ""D"");
            Assert.IsTrue(pRegs.E == {pRegs2[4]}, ""E"");
            Assert.IsTrue(pRegs.H == {pRegs2[5]}, ""H"");
            Assert.IsTrue(pRegs.L == {pRegs2[6]}, ""L"");
        }}
"
);
            }
        }
        pStream.WriteLine
       (
@"
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
