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

namespace ZXSinclair.Net.Hardware.Z80;

public partial class Z80Regs
{
    //public void SetA_B() => A = B;
	public void SetA_B() => A = B;
	public void SetA_C() => A = C;
	public void SetA_D() => A = D;
	public void SetA_E() => A = E;
	public void SetA_H() => A = H;
	public void SetA_L() => A = L;
	public void SetB_A() => B = A;
	public void SetB_C() => B = C;
	public void SetB_D() => B = D;
	public void SetB_E() => B = E;
	public void SetB_H() => B = H;
	public void SetB_L() => B = L;
	public void SetC_A() => C = A;
	public void SetC_B() => C = B;
	public void SetC_D() => C = D;
	public void SetC_E() => C = E;
	public void SetC_H() => C = H;
	public void SetC_L() => C = L;
	public void SetD_A() => D = A;
	public void SetD_B() => D = B;
	public void SetD_C() => D = C;
	public void SetD_E() => D = E;
	public void SetD_H() => D = H;
	public void SetD_L() => D = L;
	public void SetE_A() => E = A;
	public void SetE_B() => E = B;
	public void SetE_C() => E = C;
	public void SetE_D() => E = D;
	public void SetE_H() => E = H;
	public void SetE_L() => E = L;
	public void SetH_A() => H = A;
	public void SetH_B() => H = B;
	public void SetH_C() => H = C;
	public void SetH_D() => H = D;
	public void SetH_E() => H = E;
	public void SetH_L() => H = L;
	public void SetL_A() => L = A;
	public void SetL_B() => L = B;
	public void SetL_C() => L = C;
	public void SetL_D() => L = D;
	public void SetL_E() => L = E;
	public void SetL_H() => L = H;
	public void SetBC_DE() => BC = DE;
	public void SetBC_HL() => BC = HL;
	public void SetDE_BC() => DE = BC;
	public void SetDE_HL() => DE = HL;
	public void SetHL_BC() => HL = BC;
	public void SetHL_DE() => HL = DE;

}
