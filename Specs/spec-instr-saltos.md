# Especificación: grupo 3, saltos, llamadas y retornos

Grupo 3 de `Specs/spec-proceso-instrucciones.md`: el *Jump Group* y el *Call and Return Group* del manual de Zilog (`JP`, `JR`, `DJNZ`, `CALL`, `RET`, `RETI`, `RETN`, `RST`), con sus variantes `DD`/`FD` y los alias de ED. Se implementa con patrones del generador (`Specs/spec-generador-z80.md`) y auxiliares escritos a mano en el Core. Sigue la estructura de `Specs/spec-instr-carga-16.md`.

Lo no confirmado se marca **(verificar)**.

## 1. Fuentes

- `Docs/z80cpu_um.pdf` (UM0080), capítulos *Jump Group* y *Call and Return Group*.
- Sinclair Wiki, [Contended memory](https://sinclair.wiki.zxnet.co.uk/wiki/Contended_memory): ciclos de bus.
- Sean Young, *The Undocumented Z80 Documented*: MEMPTR (WZ) en saltos y llamadas, `RETI`/`RETN` y sus alias de ED.
- FUSE: 80 casos (sección 5.1) y entradas de `opcodes_base.dat`, `opcodes_ed.dat` y `opcodes_ddfd.dat`. Los eventos de `18`, `20_1`, `10`, `cd`, `c0_1`, `c7` y `ed45` confirman las direcciones de los ciclos internos de la tabla 2; en `c7` (`PC = 0x6D33`, `I = 0`) y en la segunda vuelta de `10` el ciclo interno va a `IR` y no a `PC`.

## 2. Semántica

Notación de ciclos de spec CPU 4.4. `cc` ∈ {NZ, Z, NC, C, PO, PE, P, M} (para `JR` solo NZ, Z, NC, C). `e` es un desplazamiento con signo relativo al PC **tras** leer `e`. Los T-states incluyen los M1 de los prefijos. Ninguna instrucción del grupo modifica F.

| Instrucción | Opcodes | Ciclos de bus | T | Efecto | WZ |
|---|---|---|---|---|---|
| `JP nn` | `C3` | `pc:4, pc+1:3, pc+2:3` | 10 | `PC = nn` | `nn` |
| `JP cc,nn` | `C2 CA D2 DA E2 EA F2 FA` | `pc:4, pc+1:3, pc+2:3` (no tomado: las dos lecturas con `ReadDiscarded`) | 10 | Si `cc`: `PC = nn` | `nn` (se cumpla o no) |
| `JR e` | `18` | `pc:4, pc+1:3, pc+1:1 ×5` | 12 | `PC += e` | Nuevo PC |
| `JR cc,e` | `20 28 30 38` | Se cumple: `pc:4, pc+1:3, pc+1:1 ×5`; no: `pc:4, pc+1:3` (`ReadDiscarded`) | 12 / 7 | Si `cc`: `PC += e` | Nuevo PC si salta; sin cambios si no |
| `DJNZ e` | `10` | `B ≠ 0`: `pc:4, ir:1, pc+1:3, pc+1:1 ×5`; `B = 0`: `pc:4, ir:1, pc+1:3` (`ReadDiscarded`) | 13 / 8 | `B -= 1`; si `B ≠ 0`: `PC += e` | Nuevo PC si salta; sin cambios si no |
| `JP (HL)` | `E9` | `pc:4` | 4 | `PC = HL` | Sin cambios |
| `JP (ii)` | `DD`/`FD` `E9` | `pc:4, pc+1:4` | 8 | `PC = IX`/`IY` | Sin cambios |
| `CALL nn` | `CD` | `pc:4, pc+1:3, pc+2:3, pc+2:1, sp-1:3, sp-2:3` | 17 | Apila PC (de la siguiente instrucción), `PC = nn` | `nn` |
| `CALL cc,nn` | `C4 CC D4 DC E4 EC F4 FC` | Se cumple: como `CALL nn`; no: `pc:4, pc+1:3, pc+2:3` (`ReadDiscarded`) | 17 / 10 | Si `cc`: como `CALL nn` | `nn` (se cumpla o no) |
| `RET` | `C9` | `pc:4, sp:3, sp+1:3` | 10 | `PC = desapilado` | Nuevo PC |
| `RET cc` | `C0 C8 D0 D8 E0 E8 F0 F8` | Se cumple: `pc:4, ir:1, sp:3, sp+1:3`; no: `pc:4, ir:1` | 11 / 5 | Si `cc`: como `RET` | Nuevo PC si retorna; sin cambios si no |
| `RETN` / `RETI` | `ED 45`, `ED 4D` y 6 alias (2.1) | `pc:4, pc+1:4, sp:3, sp+1:3` | 14 | `IFF1 = IFF2`; como `RET` | Nuevo PC |
| `RST p` | `C7 CF D7 DF E7 EF F7 FF` | `pc:4, ir:1, sp-1:3, sp-2:3` | 11 | Apila PC, `PC = p` (`0x00`…`0x38`) | `p` |

Reglas comunes:
- `PC` y `SP` dan la vuelta a 64K; `PC + e` también (`JR` en `0xFFFE` con `e = +4` salta a `0x0004`).
- Apilado: byte alto en `SP-1` y bajo en `SP-2` (`Push`); desapilado: bajo y después alto (`Pop`). El valor apilado es la dirección de la instrucción siguiente (PC tras leer todos los operandos).
- Ciclos internos: en `JR`/`DJNZ` sobre la dirección de `e` (`pc+1`); en `CALL` sobre la del byte alto de `nn` (`pc+2`); en `DJNZ`, `RET cc` y `RST` sobre `IR` con R ya incrementada por el M1.
- **Operandos de un salto no tomado** (`JR cc`, `JP cc`, `CALL cc`, `DJNZ` con `B = 0`): el Z80 real lee los bytes igualmente, pero FUSE los modela con `contend_read` y registra solo `MC`, sin `MR` (casos `20_2`, `c2_2`, `c4_2`…). La CPU los lee con `bus.ReadDiscarded` (spec de buses 9.2), que en `SpectrumBus` es igual que `Read` y en `FuseTestBus` no registra `MR`. `JP cc`/`CALL cc` no tomados siguen usando el valor leído para `WZ = nn`.
- `DJNZ` decrementa B **antes** de leer `e` y el decremento no toca flags; con `B = 1` no salta y deja `B = 0`; con `B = 0` da la vuelta a `0xFF` y salta.
- Las condiciones leen F a través de `Z80Flags` (`OperandEmitter` ya las traduce: `(Registers.F & Z80Flags.Z) == 0` para NZ).
- `Q`: el grupo no lo escribe (grupo 5).

### 2.1 `RETN`, `RETI` y alias de ED

- En `opcodes_ed.dat` los ocho opcodes `ED 45 4D 55 5D 65 6D 75 7D` son alias de `RETN`, incluido `ED 4D` (`RETI`). En el Z80, `RETI` también copia `IFF2` en `IFF1`; la diferencia solo la ven los periféricos Z80 con cadena de prioridad (no presentes en el Spectrum), así que los ocho comparten cuerpo.
- `IFF1 = IFF2` se aplica en la instrucción; el `Step()` siguiente ya ve el nuevo `IFF1` (no hay retraso como en `EI`).
- Con `NMI` (spec CPU 6.2): `IFF1 = 0` y `IFF2` conservado; `RETN` restaura `IFF1`.

## 3. Variantes

| Secuencia | Comportamiento | T |
|---|---|---|
| `DD`/`FD` `E9` | `JP (IX)` / `JP (IY)` (en la tabla: `JP REGISTER`) | 8 |
| `DD`/`FD` + resto del grupo (`10 18 20 28 30 38 C0`–`FF` del grupo) | El prefijo no afecta: como sin prefijo (regla de ausentes del generador) | +4 |
| `ED 55 5D 65 6D 75 7D` | Alias no documentados de `RETN` | 14 |
| IM 0 con `RST` en el bus | Ya implementado en `Z80Cpu.Interrupts.cs` con su temporización de reconocimiento; no usa estos patrones | — |
| IM 0 con `CALL`/`JP` en el bus | Grupo 11 | — |

## 4. Implementación

### 4.1 Auxiliares escritos a mano (Core)

Se reutilizan `ReadPc()`, `ReadPc16()`, `Push(ushort)` y `Pop()` (`Z80Cpu.cs`), y se añaden allí `ReadPcDiscarded()` y `ReadPc16Discarded()` (como `ReadPc`/`ReadPc16`, pero con `bus.ReadDiscarded`). En un `partial` nuevo `ZXSinclair.Net.Core/Z80/Z80Cpu.Jumps.cs`, todos `[MethodImpl(MethodImplOptions.AggressiveInlining)]`:

| Método | Comportamiento |
|---|---|
| `void JumpAbsolute(bool condition)` | Si `condition`: `PC = WZ = ReadPc16()`; si no: `WZ = ReadPc16Discarded()` |
| `void JumpRelative(bool condition)` | Si `condition`: `e = (sbyte)ReadPc()`, `bus.Internal((ushort)(PC - 1), 5)`, `PC += e`, `WZ = PC`; si no: `ReadPcDiscarded()` |
| `void DecrementJumpNonZero()` | `bus.Internal(IR, 1)`; `B -= 1`; `JumpRelative(B != 0)` |
| `void CallAbsolute(bool condition)` | Si `condition`: `address = ReadPc16()`, `WZ = address`, `bus.Internal((ushort)(PC - 1), 1)`, `Push(PC)`, `PC = address`; si no: `WZ = ReadPc16Discarded()` |
| `void Return()` | `PC = Pop()`; `WZ = PC` |
| `void ReturnConditional(bool condition)` | `bus.Internal(IR, 1)`; si `condition`, `Return()` |
| `void ReturnFromInterrupt()` | `IFF1 = IFF2`; `Return()` |
| `void Restart(ushort address)` | `bus.Internal(IR, 1)`; `Push(PC)`; `PC = address`; `WZ = address` |

- Las formas incondicionales pasan `true`; tras el inlining, el JIT elimina la rama.
- `JP (HL)`/`JP (ii)` no necesitan auxiliar.
- `Push`/`Pop` no cambian: el ciclo interno de `CALL` (dirección `pc+2`) y el de `RST` (`IR`) los ponen estos auxiliares.

### 4.2 Patrones del generador

En `ZXSinclair.Net.Generate.Z80OpCodes/Patterns/Jumps.cs`, añadidos a `PatternCatalog.Default`. La condición se emite con `OperandEmitter.Value` (clase `Condition`).

| Patrón | Reconoce | Cuerpo emitido (ejemplo) |
|---|---|---|
| `JumpAbsolutePattern` | `JP nnnn`, `JP cc,nnnn` | `JumpAbsolute(true);` / `JumpAbsolute((Registers.F & Z80Flags.Z) == 0);` |
| `JumpRegister` | `JP HL`, `JP REGISTER` (en la tabla sin paréntesis) | `Registers.PC = Registers.HL;` / `Registers.PC = TIndex.Pair(ref Registers);` |
| `JumpRelativePattern` | `JR offset`, `JR cc,offset` | `JumpRelative(true);` / `JumpRelative((Registers.F & Z80Flags.C) != 0);` |
| `DecrementJump` | `DJNZ offset` | `DecrementJumpNonZero();` |
| `CallPattern` | `CALL nnnn`, `CALL cc,nnnn` | `CallAbsolute(true);` |
| `ReturnPattern` | `RET` sin operandos | `Return();` |
| `ReturnConditionalPattern` | `RET cc` | `ReturnConditional((Registers.F & Z80Flags.S) != 0);` |
| `ReturnFromInterruptPattern` | `RETN` y `RETI` | `ReturnFromInterrupt();` (los ocho alias se apilan en un `case` múltiple) |
| `RestartPattern` | `RST p` | `Restart(0x38);` (dirección en hexadecimal a partir de `Operand.Value`) |

- La entrada de la tabla para `RET` lleva un espacio final (`0xc9 RET `): el patrón exige mnemónico `RET` y cero operandos.
- `C` como condición frente a `C` registro lo distingue ya el parser (`Condition` solo como primer operando de `JP`, `JR`, `CALL`, `RET`).

### 4.3 Cobertura esperada del generador

| Tabla | Implementados (acumulado) | Del grupo |
|---|---|---|
| base | 135 | 42 (`10 18 20 28 30 38`, `C3`, 8 `JP cc`, `CD`, 8 `CALL cc`, `C9`, 8 `RET cc`, 8 `RST`, `E9`) |
| ED | 20 | 8 (`RETN` y alias) |
| DD/FD | 135 | 42 (`E9` propio + 41 por la regla de ausentes) |
| CB, DDFDCB | 0 | 0 |

### 4.4 Rendimiento

- Una sola rama por condición, sobre una expresión de bits de F; sin asignaciones.
- Hito de medición según la guía del proceso. Arrancar la ROM del 48K aún no es posible (necesita ALU, `DI`, `OUT`, `EX`…), así que se añade un benchmark sintético `Z80CpuBenchmarks.ExecuteLoopFrame` con un bucle de las instrucciones ya implementadas (`LD r,n`, `LD (HL),r`, `LD A,(nn)`, `PUSH`/`POP`, `CALL`/`RET`, `DJNZ`, `JR`) ejecutado frame a frame en `Z80Cpu<SpectrumBus>`; se mide en ns por T-state y × tiempo real y se anota en spec CPU 8.4 junto al de `NOP`. El arranque de la ROM queda como carga de trabajo cuando estén los grupos que necesita.

## 5. Pruebas

### 5.1 FUSE

80 casos del grupo. Los sufijos `_1`/`_2` son las dos variantes de cada condicional; cuál salta depende de los flags iniciales de cada caso (por ejemplo, `20_1` salta y `28_1` no):

| Bloque | Casos |
|---|---|
| Relativos (10) | `10 18 20_1 20_2 28_1 28_2 30_1 30_2 38_1 38_2` |
| `JP`/`CALL`/`RET`/`RST` (60) | `c0_1 c0_2 c2_1 c2_2 c3 c4_1 c4_2 c7 c8_1 c8_2 c9 ca_1 ca_2 cc_1 cc_2 cd cf`, los equivalentes `d0`…`df`, `e0`…`ef` (incluido `e9`) y `f0`…`ff` |
| ED (8) | `ed45 ed4d ed55 ed5d ed65 ed6d ed75 ed7d` |
| DD/FD (2) | `dde9 fde9` |

El caso `10` ejecuta un bucle `DJNZ` de 132 T-states que termina con `INC C` (grupo 4): **seguirá omitido** hasta implementar el grupo 4. Por eso se esperan **79** casos nuevos y el runner debe pasar de 201 a **280** pasados, 0 fallos. FUSE no comprueba WZ: lo cubren los tests propios.

### 5.2 Tests propios (xUnit, no FUSE)

Reglas de spec CPU 8.3. Clase `ZXSinclair.Net.Core.Tests/Z80/Instructions/JumpTests.cs`, con `TestBus` y el estado inicial de `NopTests` (registros distintos de cero, `I = 0x42`). Cada test compara el `Z80Registers` completo y la secuencia de `state.Accesses`.

| Test | Montaje | Comprobación |
|---|---|---|
| `JpAbsolute_SetsPcAndWz` | `C3 34 12` | `PC = WZ = 0x1234`; 10 T |
| `JpConditional_AllConditions` (`[Theory]`, 16) | Cada `JP cc` con F que cumple y que no | Salta o `PC += 3`; `WZ = nn` en ambos casos; 10 T |
| `JrRelative_ForwardAndBackward` (`[Theory]`) | `18 e` con `e` = `+0x40`, `-2` (bucle a sí mismo), `-128`, `+127` | `PC`, `WZ = PC`; `Internal(pc+1, 5)`; 12 T |
| `JrConditional_AllConditions` (`[Theory]`, 8) | Cada `JR cc` cumplida/no | 12 T y salto / 7 T, `PC += 2`, WZ sin cambios |
| `JrRelative_WrapsPc` | `18 04` en `0xFFFE` | `PC = 0x0004` (tras leer `e`, `PC = 0x0000`) |
| `Djnz_DecrementsAndJumps` (`[Theory]`) | `B` = `2`, `1`, `0` | `B` = `1`/`0`/`0xFF`; salta / no / salta; 13 / 8 / 13 T; `Internal(IR, 1)` antes de leer `e`; flags intactos |
| `JpHl_AndIndex` (`[Theory]`) | `E9`, `DD E9`, `FD E9` | `PC = HL/IX/IY`; WZ sin cambios; 4 / 8 T |
| `Call_PushesReturnAddress` | `CD 34 12` en `0x8000`, `SP = 0x9000` | `(0x8FFF) = 0x80`, `(0x8FFE) = 0x03`; `PC = WZ = 0x1234`; `Internal(0x8002, 1)` antes de las escrituras; 17 T |
| `CallConditional_AllConditions` (`[Theory]`, 16) | Cada `CALL cc` cumplida/no | 17 T con apilado / 10 T sin escrituras; `WZ = nn` en ambos |
| `Call_WrapsStackPointer` | `CD` con `SP = 0x0001` | Escribe `0x0000` y `0xFFFF`; `SP = 0xFFFF` |
| `Ret_PopsPc` | `C9` con `0x1234` en la pila | `PC = WZ = 0x1234`; `SP += 2`; 10 T |
| `RetConditional_AllConditions` (`[Theory]`, 16) | Cada `RET cc` cumplida/no | 11 T / 5 T; `Internal(IR, 1)` siempre; WZ solo si retorna |
| `CallRet_RoundTrip` | `CALL` a una subrutina con `RET` | Vuelve a la instrucción siguiente con `SP` restaurado |
| `Retn_CopiesIff2ToIff1` (`[Theory]`, 8 opcodes × IFF2 0/1) | `ED 45`…`ED 7D` con `IFF1 = 0` | `IFF1 = IFF2`; `IFF2` intacto; `PC = WZ = desapilado`; 14 T |
| `Retn_AfterNmiRestoresIff1` | `IFF1 = 1`, NMI y en `0x0066` un `RETN` | Tras la NMI `IFF1 = 0`; tras `RETN`, `IFF1 = 1` y PC original |
| `Retn_IntAcceptedOnNextStep` | `RETN` con `IFF2 = 1`, IM 1 e INT activa | El `Step()` siguiente acepta la INT (sin retraso tipo `EI`) |
| `Rst_AllVectors` (`[Theory]`, 8) | `C7`…`FF` en `0x1234` | Apila `0x1235`; `PC = WZ = p`; `Internal(IR, 1)`; 11 T |
| `IndexPrefix_UnaffectedJumpsRunAsBase` (`[Theory]`) | `DD C3 nn`, `FD 18 e`, `DD CD nn`, `FD C9`, `DD FF` | Efecto de base; +4 T; IX/IY intactos |
| `Jumps_DoNotTouchFlagsOrCounter` (`[Theory]`) | Un opcode de cada patrón | `F` sin cambios; `UnimplementedOpcodes == 0` |

### 5.3 Tests existentes que hay que adaptar

| Test | Cambio |
|---|---|
| `GeneratorTests` (cobertura) | Cifras de 4.3: base y DD/FD `135`; `135 implemented / 117 pending / 4 prefixes`; ED `20 implemented / 58 pending` |
| `Z80CpuTests.UnsupportedIm0DoesNotInventJumpOrStackWrites` | Sin cambios: `CD` en IM 0 sigue sin implementar (grupo 11) |
| `Z80CpuTests`, `NopTests` | Sin cambios: `D3` (grupo 10) sigue como opcode pendiente |

Revisar que ningún test de `Load8Tests`/`Load16Tests` ejecute por accidente bytes de este grupo como "no implementados" (los `0xCD`/`0xFF` actuales son datos).

## 6. Criterios de aceptación

- Patrones de 4.2 en `Patterns/Jumps.cs` y auxiliares de 4.1; `*.g.cs` regenerados (no editados a mano) y `--check` a 0.
- Cobertura del generador igual a 4.3.
- Los 79 casos FUSE de 5.1 (todos salvo `10`) pasan con eventos; runner con 280 pasados y 0 fallos.
- Tests de 5.2 en verde y los de 5.3 revisados; `dotnet test` de los dos proyectos en verde.
- Benchmark `ExecuteLoopFrame` añadido y medido, y `*Z80Cpu*` sin regresión ni asignaciones; resultados en spec CPU 8.4.
- Tabla de seguimiento de `spec-proceso-instrucciones.md` (anotando `10` como pendiente del grupo 4), spec CPU 8.1 y `CLAUDE.md`/`AGENTS.md` actualizados.

## 7. Fuera de alcance

- `INC C` y el resto de la ALU de 8 bits (grupo 4), necesarios para el caso FUSE `10`.
- IM 0 con instrucciones distintas de `RST` (grupo 11).
- `HALT`, `DI`/`EI` (grupo 5).
- Arranque de la ROM del 48K como benchmark (cuando estén los grupos que necesita).

## 8. Pendiente de verificar

- Comportamiento de `RETI` frente a `RETN` en un Z80 NMOS real (Young indica que ambos copian `IFF2` en `IFF1`); sin efecto en el Spectrum.
- WZ en todo el grupo frente a z80test (`z80memptr`), cuando se integre.
