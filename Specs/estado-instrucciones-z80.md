# Estado de las instrucciones Z80

Fuente: [Z80 CPU User Manual, Zilog UM008011-0816](../Docs/z80cpu_um.pdf), apartado **Z80 Instruction Description**. Las páginas indicadas son las impresas en el manual; su número en el visor PDF es 14 mayor.

Estado (2026-10-03): **150 de 150 entradas completadas; 0 pendientes**.

## Cómo marcar el avance

- `[ ]`: pendiente. Si hay una implementación parcial, añadir una nota con las variantes disponibles y mantener la casilla sin marcar.
- `[x]`: implementada y verificada. Deben estar implementadas todas las variantes documentadas de esa entrada y pasar sus pruebas xUnit propias y los casos FUSE aplicables, con comparación de eventos.
- Al completar una entrada, añadir enlaces a su especificación y pruebas, y actualizar el recuento de este documento.

Hay una casilla por entrada del manual, no por opcode concreto. Las formas que el manual describe por separado mantienen casillas separadas. Una entrada con `s`, `m`, registros o condiciones simbólicas cubre todas sus variantes documentadas.

NOP, las 21 entradas de carga de 8 bits, las 20 de carga de 16 bits y pila, las 18 de saltos, llamadas y retornos y las 17 de ALU de 8 bits están completadas. El caso FUSE `10` de DJNZ también pasa tras implementar `INC C`. Las 11 entradas restantes de aritmética general y control también están completadas, incluidas HALT e IM 0/1/2. Las 11 entradas de aritmética de 16 bits también están completadas. Las 16 entradas de rotaciones y desplazamientos están completadas; SLL y las copias DDCB/FDCB también están implementadas y quedan fuera del recuento del manual.

Las 9 entradas de operaciones de bit están completadas; incluyen los alias BIT y las copias SET/RES de DDCB/FDCB. FUSE compara todos los datos salvo F5/F3 en los ocho casos BIT (HL), según la convención declarada; los tests propios comprueban MEMPTR.

Las instrucciones no documentadas, los alias de opcodes y los huecos ED quedan fuera de este recuento.

## Notación de operandos

La notación sigue la tabla 4 (p. 39) y las descripciones de cada instrucción.

| Símbolo | Significado |
|---|---|
| `r`, `r'` | Registros de 8 bits A, B, C, D, E, H o L; en `LD r, r'` son destino y origen. |
| `dd`, `ss` | Pares BC, DE, HL o SP. |
| `qq` | Pares BC, DE, HL o AF. |
| `pp` | Pares BC, DE, IX o SP. |
| `rr` | Pares BC, DE, IY o SP. |
| `n` | Inmediato sin signo de 8 bits, de 0 a 255. |
| `nn` | Inmediato o dirección de 16 bits, de 0 a 65535. |
| `d` | Desplazamiento indexado con signo de 8 bits, de -128 a 127. |
| `e` | Desplazamiento relativo: el manual lo expresa respecto al inicio de la instrucción (-126 a 129); el byte codificado se aplica al PC posterior a sus dos bytes (-128 a 127). |
| `b` | Posición de bit, de 0 a 7. |
| `cc` | Condición NZ, Z, NC, C, PO, PE, P o M. Las entradas JR muestran sus condiciones por separado. |
| `s` | Operando `r`, `n`, `(HL)`, `(IX+d)` o `(IY+d)`. |
| `m` | Operando `r`, `(HL)`, `(IX+d)` o `(IY+d)`. |
| `p` | Dirección de RST: 00h, 08h, 10h, 18h, 20h, 28h, 30h o 38h. |
| `(…)` | Contenido de la memoria indicada, salvo los puertos de IN/OUT. En `JP (HL)`, `JP (IX)` y `JP (IY)`, el destino es el valor del registro, sin lectura indirecta de memoria. |
| `AF'` | Juego alternativo de AF. |

## Lista de seguimiento

### Carga de 8 bits

- [x] `LD r, r'` — p. 71. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD r, n` — p. 72. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD r, (HL)` — p. 74. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD r, (IX+d)` — p. 75. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD r, (IY+d)` — p. 77. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (HL), r` — p. 79. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (IX+d), r` — p. 81. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (IY+d), r` — p. 83. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (HL), n` — p. 85. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (IX+d), n` — p. 86. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (IY+d), n` — p. 87. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD A, (BC)` — p. 88. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD A, (DE)` — p. 89. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD A, (nn)` — p. 90. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (BC), A` — p. 91. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (DE), A` — p. 92. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD (nn), A` — p. 93. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD A, I` — p. 94. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD A, R` — p. 95. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD I, A` — p. 96. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).
- [x] `LD R, A` — p. 97. [Spec](spec-instr-carga-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load8Tests.cs).

### Carga de 16 bits y pila

- [x] `LD dd, nn` — p. 99. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD IX, nn` — p. 100. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD IY, nn` — p. 101. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD HL, (nn)` — p. 102. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD dd, (nn)` — p. 103. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD IX, (nn)` — p. 105. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD IY, (nn)` — p. 106. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD (nn), HL` — p. 107. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD (nn), dd` — p. 108. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD (nn), IX` — p. 110. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD (nn), IY` — p. 111. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD SP, HL` — p. 112. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD SP, IX` — p. 113. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `LD SP, IY` — p. 114. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `PUSH qq` — p. 115. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `PUSH IX` — p. 117. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `PUSH IY` — p. 118. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `POP qq` — p. 119. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `POP IX` — p. 121. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).
- [x] `POP IY` — p. 122. [Spec](spec-instr-carga-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Load16Tests.cs).

### Intercambio y bloques

- [x] `EX DE, HL` — p. 124. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `EX AF, AF'` — p. 125. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `EXX` — p. 126. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `EX (SP), HL` — p. 127. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `EX (SP), IX` — p. 128. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `EX (SP), IY` — p. 129. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `LDI` — p. 130. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `LDIR` — p. 132. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `LDD` — p. 134. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `LDDR` — p. 136. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `CPI` — p. 138. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `CPIR` — p. 139. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `CPD` — p. 141. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).
- [x] `CPDR` — p. 142. [Spec](spec-instr-bloques.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BlockTests.cs).

### Aritmética y lógica de 8 bits

- [x] `ADD A, r` — p. 145. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `ADD A, n` — p. 147. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `ADD A, (HL)` — p. 148. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `ADD A, (IX+d)` — p. 149. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `ADD A, (IY+d)` — p. 150. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `ADC A, s` — p. 151. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `SUB s` — p. 153. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `SBC A, s` — p. 155. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `AND s` — p. 157. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `OR s` — p. 159. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `XOR s` — p. 161. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `CP s` — p. 163. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `INC r` — p. 165. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `INC (HL)` — p. 167. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `INC (IX+d)` — p. 168. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `INC (IY+d)` — p. 169. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).
- [x] `DEC m` — p. 170. [Spec](spec-instr-alu-8.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu8Tests.cs).

### Control de CPU y operaciones generales de AF

- [x] `DAA` — p. 173. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `CPL` — p. 175. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `NEG` — p. 176. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `CCF` — p. 178. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `SCF` — p. 179. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `NOP` — p. 180. [Especificación](spec-instr-nop.md) · [Pruebas](../ZXSinclair.Net.Core.Tests/Z80/Instructions/NopTests.cs).
- [x] `HALT` — p. 181. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `DI` — p. 182. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `EI` — p. 183. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `IM 0` — p. 184. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `IM 1` — p. 185. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).
- [x] `IM 2` — p. 186. [Spec](spec-instr-control.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/ControlTests.cs).

### Aritmética de 16 bits

- [x] `ADD HL, ss` — p. 188. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `ADC HL, ss` — p. 190. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `SBC HL, ss` — p. 192. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `ADD IX, pp` — p. 194. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `ADD IY, rr` — p. 196. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `INC ss` — p. 198. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `INC IX` — p. 199. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `INC IY` — p. 200. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `DEC ss` — p. 201. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `DEC IX` — p. 202. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).
- [x] `DEC IY` — p. 203. [Spec](spec-instr-alu-16.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/Alu16Tests.cs).

### Rotaciones y desplazamientos

- [x] `RLCA` — p. 205. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLA` — p. 207. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RRCA` — p. 209. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RRA` — p. 211. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLC r` — p. 213. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLC (HL)` — p. 215. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLC (IX+d)` — p. 217. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLC (IY+d)` — p. 219. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RL m` — p. 221. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RRC m` — p. 224. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RR m` — p. 227. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `SLA m` — p. 230. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `SRA m` — p. 233. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `SRL m` — p. 236. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RLD` — p. 238. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).
- [x] `RRD` — p. 240. [Spec](spec-instr-rotaciones.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/RotateTests.cs).

### Bits

- [x] `BIT b, r` — p. 243. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `BIT b, (HL)` — p. 245. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `BIT b, (IX+d)` — p. 247. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `BIT b, (IY+d)` — p. 249. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `SET b, r` — p. 251. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `SET b, (HL)` — p. 253. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `SET b, (IX+d)` — p. 255. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `SET b, (IY+d)` — p. 257. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).
- [x] `RES b, m` — p. 259. [Spec](spec-instr-bits.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/BitTests.cs).

### Saltos, llamadas y retornos

- [x] `JP nn` — p. 262. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JP cc, nn` — p. 263. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JR e` — p. 265. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JR C, e` — p. 267. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JR NC, e` — p. 269. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JR Z, e` — p. 271. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JR NZ, e` — p. 273. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JP (HL)` — p. 275. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JP (IX)` — p. 276. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `JP (IY)` — p. 277. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `DJNZ e` — p. 278. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `CALL nn` — p. 281. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `CALL cc, nn` — p. 283. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `RET` — p. 285. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `RET cc` — p. 286. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `RETI` — p. 288. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `RETN` — p. 290. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).
- [x] `RST p` — p. 292. [Spec](spec-instr-saltos.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs).

### Entrada/salida

- [x] `IN A, (n)` — p. 295. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `IN r, (C)` — p. 296. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `INI` — p. 298. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `INIR` — p. 300. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `IND` — p. 302. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `INDR` — p. 304. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OUT (n), A` — p. 306. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OUT (C), r` — p. 307. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OUTI` — p. 309. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OTIR` — p. 311. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OUTD` — p. 313. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).
- [x] `OTDR` — p. 315. [Spec](spec-instr-io.md) · [Tests](../ZXSinclair.Net.Core.Tests/Z80/Instructions/IoTests.cs).

El recuento corresponde a las 150 entradas del manual. Incluye las variantes no documentadas previstas en sus specs; los 178 huecos de ED y las respuestas arbitrarias de IM 0 siguen en el grupo 11.
