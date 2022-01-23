/*
    ZXSinclair Emulador ZX Computers
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

// http://www.z80.info/zip/z80cpu_um.pdf
// http://www.z80.info/zip/z80-documented.pdf
// https://worldofspectrum.org/z88forever/dn327/z80undoc.htm
// https://worldofspectrum.org/faq/resources/documents.htm
// http://biblioteca.museo8bits.es/index.php
// https://trastero.speccy.org/cosas/Libros/Libros.htm
// https://www.imd.guru/retropedia/desarrollo/z80/opcodes.html
// https://wiki.speccy.org/cursos/ensamblador/lenguaje_1
// https://worldofspectrum.org/faq/reference/48kreference.htm#Contention

#include "pch.h"
#include "z80.h"

z80_registers z80_regs;
int z80_tstates;
Tz80_memory *z80_memory;