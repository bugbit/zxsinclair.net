# Z80: instrucciones y comportamiento no documentados

Recopilación de todo lo que el manual de Zilog (`Docs/z80cpu_um.pdf`) no documenta o documenta mal del Z80: opcodes, flags F5/F3 (YF/XF), el registro interno MEMPTR (WZ), el registro R, interrupciones y diferencias entre modelos. Parte de la página de z80.info indicada y se completa con las fuentes de referencia actuales. La última sección contrasta cada punto con lo que ya especifica e implementa este proyecto.

Revisado el 2026-10-04. Es material de referencia externo resumido con palabras propias; ante una duda, mandan las fuentes originales y, sobre todo, los casos de prueba de hardware (FUSE, z80test).

Notación: F5 = YF (bit 5 de F), F3 = XF (bit 3 de F). `ii` = IX o IY. `(ii+d)` con `d` con signo.

## 1. Fuentes

| Fuente | Autor / fecha | Qué aporta | Dónde |
|---|---|---|---|
| *Z80 Undocumented Instructions* | Richard Spijkers (1992), traducido y ampliado por Jacco J. T. Bot (1996); incluye *Undocumented Z-80 Opcodes* de Bill Smythe (*Northern Bytes*, 1982) | Opcodes no documentados: IXH/IXL, `SLL`, copias `DDCB`, duplicados de ED, `IN (C)`, `OUT (C),0`, combinaciones de prefijos, huecos de ED | [z80.info/z80undoc.htm](http://www.z80.info/z80undoc.htm) (certificado HTTPS no válido; usar `http://`) |
| *The Undocumented Z80 Documented* | Sean Young (v0.6, 2003; hay versiones posteriores) | Flags F5/F3 de cada familia, `BIT`, bloques, E/S en bloque (Ramsoft), DAA, interrupciones, R, valores tras reset, erratas del manual | [raine.1emulation.com/…/z80-documented.pdf](https://raine.1emulation.com/archive/dev/z80-documented.pdf) |
| *MEMPTR, esoteric register of the ZiLOG Z80 CPU* | Boo-boo, traducción de Vladimir Kladov (2006); pruebas de Wlodek, CHRV, icebear y otros | Reglas de WZ instrucción a instrucción y F5/F3 de `BIT n,(HL)` | [floooh/emu-info: memptr_eng.txt](https://raw.githubusercontent.com/floooh/emu-info/master/z80/memptr_eng.txt) |
| *Z80Decoder — Undocumented Flags* | David Banks (hoglet), con Patrik Rak y TonyB | `SCF`/`CCF` y `Q` por modelo, flags de los bloques interrumpidos, efecto de un prefijo antes de `SCF`/`CCF` | [github.com/hoglet67/Z80Decoder/wiki/Undocumented-Flags](https://github.com/hoglet67/Z80Decoder/wiki/Undocumented-Flags) |
| *New discovery on Z80 I/O block instructions* | ZjoyKiLer y otros, Spectrum Computing (finales de 2023), confirmado en Zilog y NEC reales | MEMPTR de `INxR`/`OTxR` al repetir; corrección de z80test 1.2 → 1.2a | [spectrumcomputing.co.uk/forums/viewtopic.php?t=10555](https://spectrumcomputing.co.uk/forums/viewtopic.php?t=10555) |
| *Z80 Known Bugs* | Howard Goldstein | Fallo del NMOS en el P/V de `LD A,I`/`LD A,R` | [z80.info/z80bugs.htm](http://www.z80.info/z80bugs.htm) |
| Issue *NMOS/CMOS Z80 differences* | proyecto kosarev/z80 | `OUT (C),0` en NMOS frente a CMOS y otras diferencias | [github.com/kosarev/z80/issues/35](https://github.com/kosarev/z80/issues/35) |
| UM0080 | Zilog | Temporización de IM 0 (normal + 2 T, p. 19) y lectura de los bytes siguientes en IM 0 (p. 184) | `Docs/z80cpu_um.pdf` |

## 2. Opcodes no documentados

### 2.1 Mitades de IX e IY (prefijos `DD`/`FD`)

Un prefijo `DD` (o `FD`) delante de una instrucción que usa **H o L sueltos** (no `(HL)`) hace que use **IXH/IXL** (o IYH/IYL). Afecta a:

- `INC`/`DEC` (`DD 24 25 2C 2D`), `LD IXH,n` / `LD IXL,n` (`DD 26 nn`, `DD 2E nn`).
- `LD r,IXH/IXL` y `LD IXH/IXL,r` (`DD 44 45 4C 4D 54 55 5C 5D 60`–`65 67`–`6D 6F 7C 7D`), incluidos `LD IXH,IXL` y los inútiles `LD IXH,IXH`/`LD IXL,IXL`.
- ALU con A: `ADD`, `ADC`, `SUB`, `SBC`, `AND`, `XOR`, `OR`, `CP` (`DD 84 85 8C 8D 94 95 9C 9D A4 A5 AC AD B4 B5 BC BD`).

Excepciones:
- Si la instrucción tiene a la vez H/L y `(HL)` (`LD H,(HL)`, `LD (HL),L`…), el prefijo solo afecta a `(HL)`, que pasa a `(ii+d)`; H y L siguen siendo los reales. No existe `LD IXH,(IX+d)`.
- Una instrucción usa solo IX **o** IY: no existe `LD IXL,IYH`.

### 2.2 `SLL` (también llamada `SL1` o `SLS`)

`CB 30`–`CB 37` (y `DD CB d 36` / `FD CB d 36`): desplaza a la izquierda como `SLA`, el bit 7 va a C, pero **el bit 0 entra a 1** (multiplica por 2 y suma 1). Flags como el resto de desplazamientos del grupo CB.

### 2.3 `DDCB`/`FDCB`: copias a registro y alias de `BIT`

En `DD CB d op` (y `FD CB d op`) los 3 bits bajos de `op` (no del desplazamiento) eligen además un registro destino:

| Bits bajos | Registro |
|---|---|
| 0 1 2 3 4 5 7 | B C D E H L A |
| 6 | Ninguno (la forma documentada) |

- **Rotaciones y desplazamientos** (`op` `00`–`3F`) y **`RES`/`SET`** (`80`–`FF`): la operación se hace sobre `(ii+d)`, el resultado se escribe en memoria **y** en el registro elegido (H y L son los reales, no las mitades del índice). Ejemplo: `DD CB d 00` = `RLC (IX+d)` y copia en B.
- **`BIT`** (`40`–`7F`): no hay copia; los ocho opcodes de cada bit son alias de `BIT b,(ii+d)`.

### 2.4 Tabla ED

| Opcodes | Instrucción |
|---|---|
| `ED 44 4C 54 5C 64 6C 74 7C` | `NEG` (solo `ED 44` documentado) |
| `ED 45 55 5D 65 6D 75 7D` y `ED 4D` | `RETN`; todas, **incluida `RETI`**, copian `IFF2` en `IFF1`. La única diferencia de `RETI` es su opcode, que reconocen los periféricos Z80 (PIO) de la cadena de prioridad |
| `ED 46 4E 66 6E` | `IM 0` (los `ED 4E`/`6E` a veces se describen como "IM 0/1"; según Gerton Lunter, citado por Young, se comportan como IM 0) |
| `ED 56 76` | `IM 1` |
| `ED 5E 7E` | `IM 2` |
| `ED 63 nn` / `ED 6B nn` | `LD (nn),HL` / `LD HL,(nn)` (duplicados más lentos de `22`/`2A`; 20 T frente a 16) |
| `ED 70` | `IN (C)` / `IN F,(C)`: lee el puerto `BC` y fija los flags como `IN r,(C)`, pero **no guarda** el dato |
| `ED 71` | `OUT (C),0`: saca 0 en el NMOS; en el **CMOS** saca `FF` |
| Resto (178 opcodes: `ED 00`–`3F`, `ED 77`, `ED 7F`, `ED 80`–`9F`, los `ED A4`–`A7`, `AC`–`AF`, `B4`–`B7`, `BC`–`BF`, `ED C0`–`FF`) | Sin efecto: como **dos NOP** (8 T, R `+= 2`) |

### 2.5 Combinaciones de prefijos

- `DD`/`FD` no son instrucciones, sino un indicador de "usar IX/IY en lugar de HL" para el opcode siguiente. En una cadena de `DD`/`FD` solo cuenta el **último**; cada uno consume un M1 (4 T, R `+= 1`).
- `DD`/`FD` delante de una instrucción que no usa HL, H ni L no tiene efecto (salvo los 4 T y el R `+= 1` de su M1). Delante de `ED`, se ignora. `EX DE,HL` y `EXX` no se ven afectados; `JP (HL)` pasa a `JP (IX)` (sin desplazamiento).
- `CB` y `ED` forman instrucción con el byte siguiente; `ED` seguido de `CB`, `DD`, `ED` o `FD` cae en huecos de ED (dos NOP).
- **No se aceptan interrupciones entre un prefijo `DD`/`FD` y su opcode** (ni en una cadena de prefijos); sí después de `CB xx` y `ED xx`.

## 3. Flags no documentados

### 3.1 Regla general y excepciones de F5/F3

En la mayoría de instrucciones que fijan flags, **F5 y F3 son copia de los bits 5 y 3 del resultado**. Excepciones:

| Instrucción | F5 / F3 |
|---|---|
| `CP s` | Del **operando** `s`, no del resultado |
| `BIT b,r` | Bits 5 y 3 del **registro probado** (el valor completo, no el resultado del `AND` con la máscara; ver la errata de 7) |
| `BIT b,(HL)` | Bits 13 y 11 de **MEMPTR** (byte alto de WZ) |
| `BIT b,(ii+d)` | Bits 5 y 3 del byte alto de `ii + d` |
| `ADD/ADC/SBC` de 16 bits | Del byte alto del resultado (la suma se hace en dos mitades de 8 bits) |
| `LDI`/`LDD` (y repetitivas) | `n = A + dato`: F5 = bit **1** de `n`, F3 = bit 3 de `n` |
| `CPI`/`CPD` (y repetitivas) | `n = A − (HL) − H`: F5 = bit 1 de `n`, F3 = bit 3 de `n` |
| `SCF`/`CCF` | Ver 3.4 |
| Rotaciones de A (`RLCA`…), `CPL`, `DAA`, `NEG` | Del nuevo A |

### 3.2 Otras reglas de flags

- **`BIT`**: Z = P/V = bit probado a 0; S = 1 solo con `b = 7` y bit a 1; H = 1; N = 0; C se conserva; F5/F3: ver 3.1.
- **`LDI`/`LDD`**: S, Z y C se conservan; H = 0; N = 0; P/V = `BC ≠ 0` tras decrementar.
- **`CPI`/`CPD`**: S, Z y H de la comparación `A − (HL)`; P/V = `BC ≠ 0`; N = 1; C se conserva.
- **E/S en bloque** (`INI`, `IND`, `OUTI`, `OUTD` y repetitivas; descubierto por Ramsoft): S, Z, F5, F3 como `DEC B`; N = bit 7 del dato transferido; con `k = dato + ((C ± 1) & 0xFF)` en `INI`/`IND` y `k = dato + L` (L tras incrementar/decrementar HL) en `OUTI`/`OUTD`: H = C = `k > 255`; P/V = paridad de `(k & 7) xor B`.
- **`IN r,(C)`**: S, Z, F5, F3 y paridad del dato; H = 0 y N = 0 (alguna documentación dice que H depende del resultado: es imposible, no hay aritmética); C se conserva.
- **`DAA`**: tabla de Stefano Donati (Ramsoft) según A, C, H y N: se suma o resta `00/06/60/66`; C y H nuevos según los nibbles; S, Z, F5, F3 y paridad del resultado; N se conserva.
- **`LD A,I` / `LD A,R`**: P/V = `IFF2`; H = N = 0; S, Z, F5, F3 del valor; C se conserva (fallo del NMOS en 4.4).

### 3.3 Instrucciones de bloque interrumpidas (2018–2023)

Cuando `LDxR`/`CPxR`/`INxR`/`OTxR` **repiten**, la CPU añade un ciclo de 5 T para retroceder PC en 2, y ese ciclo **cambia flags**. Normalmente no se ve, porque la siguiente iteración los recalcula; se ve si se acepta una interrupción entre iteraciones (la rutina de servicio ve el F de la iteración que repitió). La iteración que **termina** deja los flags clásicos.

- `LDxR` / `CPxR` al repetir: F5 = bit 13 de PC, F3 = bit 11 de PC, con PC = dirección del prefijo `ED` (es decir, tras retroceder).
- `INxR` / `OTxR` al repetir: lo mismo para F5/F3, y además (con `dato` el byte transferido y B ya decrementado):
  - si C = 1: si el bit 7 de `dato` es 1, P/V se invierte con la paridad de `(B − 1) & 7` y H = `(B & 0x0F) == 0x00`; si es 0, P/V se invierte con la paridad de `(B + 1) & 7` y H = `(B & 0x0F) == 0x0F`;
  - si C = 0: P/V se invierte con la paridad de `B & 7`; H no cambia.
  - "Invertir con la paridad de x" significa invertir P/V cuando `x` tiene un número impar de bits a 1 (en la notación de la fuente, `PF ^= Parity(x) ^ 1` con `Parity` = 1 si la paridad es par).

### 3.4 `SCF`, `CCF` y el registro interno `Q` (Patrik Rak, 2012)

| Modelo | F5 / F3 tras `SCF`/`CCF` |
|---|---|
| Zilog NMOS | Si la instrucción anterior **modificó** flags: F5/F3 = bits 5/3 de A. Si no: F5/F3 = (F5/F3 anteriores) **OR** bits 5/3 de A. Equivale a `((Q ^ F) \| A) & 0x28`, donde `Q` = F escrito por la instrucción anterior, o 0 si no escribió flags |
| NEC NMOS | Siempre de A |
| ST CMOS | F3 siempre de A; F5 como en Zilog NMOS |

- `POP AF` (y por la misma lógica `EX AF,AF'`) **no** cuenta como instrucción que modifica flags.
- **Prefijo delante de `SCF`/`CCF`** (TonyB, 2026): un `DD`/`FD` antes de `SCF`/`CCF` actúa como "instrucción anterior" que no modificó flags, así que se pierde la historia (`Q = 0`).

## 4. MEMPTR (WZ)

Registro interno de 16 bits (`WZ`, el `MEMPTR` del 8080). Solo es observable a través de F5/F3 de `BIT b,(HL)`. Reglas (Boo-boo, 2006, y descubrimiento de 2023):

| Instrucción | WZ |
|---|---|
| `LD A,(nn)` / `LD rp,(nn)` / `LD (nn),rp` | `nn + 1` |
| `LD (nn),A` | Bajo = `(nn + 1) & 0xFF`, alto = A |
| `LD A,(BC/DE)` | `rp + 1` |
| `LD (BC/DE),A` | Bajo = `(rp + 1) & 0xFF`, alto = A |
| `JP nn`, `CALL nn` (también condicionales, se cumpla o no) | `nn` |
| `JR`, `DJNZ` (cuando saltan), `RET`, `RETI`, `RETN`, `RST`, aceptación de interrupción | Dirección de destino |
| `EX (SP),rp` | Nuevo valor de `rp` |
| `ADD/ADC/SBC rp1,rp2` | `rp1` (antes) + 1 |
| `IN A,(n)` | `(A << 8) + n + 1` (A antes) |
| `OUT (n),A` | Bajo = `(n + 1) & 0xFF`, alto = A |
| `IN r,(C)`, `OUT (C),r` | `BC + 1` |
| `CPI` / `CPD` | `WZ + 1` / `WZ − 1` |
| `CPIR`/`CPDR` | Si repite: PC + 1 (PC = dirección del `ED`); si termina: como `CPI`/`CPD` |
| `LDIR`/`LDDR` | Si repite: PC + 1; si termina: sin cambios |
| `INI`/`IND` | `BC ± 1` con B **antes** de decrementar |
| `OUTI`/`OUTD` | `BC ± 1` con B **después** de decrementar |
| `INIR`/`INDR`/`OTIR`/`OTDR` | Como la simple; **si repite, PC + 1** (descubrimiento de 2023, confirmado en Zilog y NEC; corrigió z80test 1.2 → 1.2a) |
| `RLD`/`RRD` | `HL + 1` |
| Cualquier acceso `(ii+d)` | `ii + d` |
| `LD r,r'` y la mayoría de instrucciones sin memoria | Sin cambios |

Las variantes soviéticas KP1858BM1/T34BM1 ponen 0 en lugar de A en el byte alto de `LD (nn),A`, `LD (rp),A` y `OUT (n),A`.

## 5. Registro R, reset e interrupciones

### 5.1 Registro R

- Los 7 bits bajos se incrementan en cada M1; el bit 7 solo cambia con `LD R,A`.
- Sin prefijo: +1 por instrucción; con `CB`, `ED`, `DD`, `FD`: +2; `DDCB`/`FDCB`: +2 (los bytes `d` y `op` no son M1); un `DD`/`FD` suelto: +1.
- `LD A,R` y `LD R,A` ven R ya incrementado por la propia instrucción.
- Cada iteración de una instrucción de bloque suma 2. Aceptar una INT o una NMI suma 1.
- En el ciclo de refresco se pone `IR` completo en el bus de direcciones.

### 5.2 Valores tras reset

PC = 0, `IFF1 = IFF2 = 0`, IM = 0, I = R = 0; **AF y SP = FFFFh**; el resto, indefinido (Young recomienda FFFFh en emuladores).

### 5.3 Interrupciones

- Solo se aceptan **entre instrucciones**: nunca tras un prefijo `DD`/`FD`, ni justo después de `EI` (ni durante una cadena de `EI`); después de `DI` tampoco, porque las deshabilita. Las instrucciones de bloque admiten interrupción entre iteraciones.
- **NMI**: `IFF1 = 0`, `IFF2` se conserva (no se copia `IFF1` en `IFF2`, contra lo que dice parte de la documentación); salta a `0066h`; 11 T. `RETN` restaura `IFF1 = IFF2`.
- **INT**: `IFF1 = IFF2 = 0`. Reconocimiento con un M1 especial con 2 estados de espera.
  - IM 0: se ejecuta la instrucción que pone el dispositivo; dura **lo normal + 2 T** (UM0080 p. 19); un `RST` tarda 13 T. En instrucciones de varios bytes, el primero llega en el reconocimiento y los siguientes se leen con ciclos normales de lectura de memoria (p. 184).
  - IM 1: `RST 38h` sea cual sea el byte del bus; 13 T.
  - IM 2: llamada a la dirección leída en `(I << 8) | byte del bus`; 19 T.
  - En muchos sistemas (Spectrum, MSX) nadie conduce el bus y el byte vale `FFh`.
- **`HALT`**: la CPU repite NOP sin avanzar PC hasta que se acepta una interrupción; entonces avanza PC antes de llamar a la rutina, así que la dirección de retorno es la instrucción siguiente a `HALT`.

### 5.4 Fallo del NMOS en `LD A,I` / `LD A,R`

Si se acepta una **INT** mientras se ejecuta `LD A,I` o `LD A,R`, la instrucción copia en P/V el valor de `IFF2` **después** de que el reconocimiento lo haya puesto a 0: P/V queda a 0 aunque las interrupciones estuvieran habilitadas. Afecta a código que usa este truco para saber si las interrupciones estaban activas. Como la NMI no modifica `IFF2`, el fallo no tiene efecto visible con NMI. Corregido en el CMOS.

## 6. Diferencias entre modelos

| Comportamiento | Zilog NMOS | Zilog CMOS / ST CMOS | NEC NMOS | KP1858BM1 / T34BM1 |
|---|---|---|---|---|
| `OUT (C),0` (`ED 71`) | Saca 0 | Saca FFh | 0 | — |
| `SCF`/`CCF` (F5/F3) | `((Q ^ F) \| A)` | ST: F3 de A, F5 como NMOS | De A | — |
| P/V de `LD A,I/R` interrumpido | 0 (fallo) | Correcto | — | — |
| Byte alto de WZ en `LD (nn),A`, `LD (rp),A`, `OUT (n),A` | A | A | A | 0 |

## 7. Erratas conocidas

**En la documentación oficial** (recogidas por Young):
- `LDI`… no dejan S y Z indefinidos: los conservan. `CPI`… no dejan S y H indefinidos.
- En `INI`/`OUTD`… la tabla dice "Z = 0 en ambos casos"; con B = 0, Z = 1.
- Las instrucciones de E/S en bloque **sí** modifican C, y N no siempre vale 1.
- Al aceptar una NMI no se copia `IFF1` en `IFF2`.
- `IN r,(C)` nunca pone H a 1.
- Errores de codificación en `LD r,(IX+d)` y `ADD IX,pp` en algunas ediciones.

**En fuentes no oficiales:**
- Young v0.6 dice que, en `BIT b,r`, F5/F3 salen del resultado del `AND` con la máscara (F5 = 1 solo si `b = 5` y el bit vale 1). Los casos FUSE lo contradicen: `cb40` (`BIT 0,B` con `B = BC`) espera `F = 7C`, con F5 y F3 copiados del valor completo de B aunque se pruebe el bit 0. El proyecto sigue a FUSE (`BitTest` en `Z80Cpu.Bits.cs`).
- Young v0.6 dice que `INI`… usan `BC` **después** de decrementar B y `OUTI`… **antes**. Es al revés: los eventos de bus de FUSE (`eda2`, `eda3`) muestran que `INI` usa el B original y `OUTI` el B ya decrementado, lo que coincide con las reglas de MEMPTR de 4.
- El resumen de `DDCB` de Bill Smythe lista `28`–`2F` como `SRL`; son `SRA`.

## 8. Estado en este proyecto

| Comportamiento | Dónde | Estado |
|---|---|---|
| IXH/IXL e IYH/IYL, excepción de H/L con `(HL)` | `Specs/spec-instr-carga-8.md`, `spec-instr-alu-8.md` | Implementado; FUSE |
| `SLL` y copias `DDCB` (rotaciones, `SET`/`RES`), alias de `BIT` | `spec-instr-rotaciones.md`, `spec-instr-bits.md` | Implementado; FUSE |
| Duplicados de ED (`NEG`, `RETN`/`RETI`, `IM`, `ED 63/6B`) | `spec-instr-control.md`, `spec-instr-saltos.md`, `spec-instr-carga-16.md` | Implementado; FUSE |
| `IN F,(C)`, `OUT (C),0` (NMOS) | `spec-instr-io.md` | Implementado; FUSE |
| Huecos de ED como dos NOP | `spec-instr-restos.md` | Especificado y planificado (grupo 11) |
| Combinaciones de prefijos; sin interrupción tras `DD`/`FD` | `spec-cpu-z80.md` 4.3 | Implementado |
| F5/F3 de cada familia, `CP`, `BIT` (r, `(HL)` con WZ, `(ii+d)`) | Specs de cada grupo | Implementado. `BIT b,r` toma F5/F3 del registro completo, como FUSE y no como Young v0.6 (errata en 7). Los fixtures FUSE de `BIT b,(HL)` usan el valor leído: convención declarada (`spec-instr-bits.md` 2.2.1) |
| `Q` en `SCF`/`CCF` (Zilog NMOS) | `spec-instr-control.md` | Implementado (`POP AF`/`EX AF,AF'` con `Q = 0`, que coincide con la fuente) |
| `SCF`/`CCF` tras un prefijo `DD`/`FD` (TonyB, 2026) | `spec-correcciones-no-documentado.md`, `spec-instr-control.md` 2.2 | Implementado: `ExecuteIndexed` pone `Q = 0` tras los prefijos; tests en `ControlTests` |
| Flags de `LDxR`/`CPxR` interrumpidos | `spec-instr-bloques.md` | Implementado |
| Flags de `INxR`/`OTxR` interrumpidos | `spec-instr-io.md` | Implementado con la fórmula de 3.3 |
| MEMPTR de `INxR`/`OTxR` al repetir = PC + 1 (2023) | `spec-correcciones-no-documentado.md`, `spec-instr-io.md` 2.3 | Implementado en `BlockInCore`/`BlockOutCore`; tests en `IoTests` |
| MEMPTR del resto de instrucciones | Specs de cada grupo | Implementado según la tabla de 4 |
| R y valores tras reset | `spec-cpu-z80.md` 3, 4.2 y 7 | Implementado (`AF = SP = FFFF`) |
| Temporización de IM 0 (normal + 2 T) y HALT en IM 0 | `spec-instr-restos.md` | Especificado y planificado (grupo 11) |
| Fallo del NMOS en `LD A,I`/`LD A,R` con INT | `spec-instr-restos.md` | Especificado y planificado (grupo 11); con NMI no se aplica, coherente con 5.4 |
| Diferencias CMOS/NEC/BM1 | — | Fuera de alcance (se emula el Zilog NMOS del Spectrum) |

Siguiente paso recomendado: integrar z80test (versión 1.2a o posterior, que ya incluye la corrección de 2023) para contrastar con hardware los puntos de las secciones 3 y 4 (ver `Docs/benchmarks-maquinas-reales.md`).
