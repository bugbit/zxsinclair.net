// 0x00 NOP
case NOP:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x01 LD BC,nnnn
case LD_BC_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x02 LD (BC),A
case LD_MM_BC_MM_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x03 INC BC
case INC_BC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x04 INC B
case INC_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x05 DEC B
case DEC_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x06 LD B,nn
case LD_B_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x07 RLCA
case RLCA:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x08 EX AF,AF'
case EX_AF_AF_:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x09 ADD HL,BC
case ADD_HL_BC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0a LD A,(BC)
case LD_A_MM_BC_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0b DEC BC
case DEC_BC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0c INC C
case INC_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0d DEC C
case DEC_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0e LD C,nn
case LD_C_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x0f RRCA
case RRCA:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x10 DJNZ offset
case DJNZ_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x11 LD DE,nnnn
case LD_DE_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x12 LD (DE),A
case LD_MM_DE_MM_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x13 INC DE
case INC_DE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x14 INC D
case INC_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x15 DEC D
case DEC_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x16 LD D,nn
case LD_D_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x17 RLA
case RLA:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x18 JR offset
case JR_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x19 ADD HL,DE
case ADD_HL_DE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1a LD A,(DE)
case LD_A_MM_DE_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1b DEC DE
case DEC_DE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1c INC E
case INC_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1d DEC E
case DEC_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1e LD E,nn
case LD_E_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x1f RRA
case RRA:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x20 JR NZ,offset
case JR_NZ_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x21 LD HL,nnnn
case LD_HL_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x22 LD (nnnn),HL
case LD_MM_nnnn_MM_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x23 INC HL
case INC_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x24 INC H
case INC_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x25 DEC H
case DEC_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x26 LD H,nn
case LD_H_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x27 DAA
case DAA:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x28 JR Z,offset
case JR_Z_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x29 ADD HL,HL
case ADD_HL_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2a LD HL,(nnnn)
case LD_HL_MM_nnnn_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2b DEC HL
case DEC_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2c INC L
case INC_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2d DEC L
case DEC_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2e LD L,nn
case LD_L_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x2f CPL
case CPL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x30 JR NC,offset
case JR_NC_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x31 LD SP,nnnn
case LD_SP_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x32 LD (nnnn),A
case LD_MM_nnnn_MM_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x33 INC SP
case INC_SP:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x34 INC (HL)
case INC_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x35 DEC (HL)
case DEC_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x36 LD (HL),nn
case LD_MM_HL_MM_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x37 SCF
case SCF:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x38 JR C,offset
case JR_C_offset:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x39 ADD HL,SP
case ADD_HL_SP:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3a LD A,(nnnn)
case LD_A_MM_nnnn_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3b DEC SP
case DEC_SP:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3c INC A
case INC_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3d DEC A
case DEC_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3e LD A,nn
case LD_A_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x3f CCF
case CCF:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x40 LD B,B
case LD_B_B:
__zLD_R_R1__(__zB__,__zB__);
break;
// 0x41 LD B,C
case LD_B_C:
__zLD_R_R1__(__zB__,__zC__);
break;
// 0x42 LD B,D
case LD_B_D:
__zLD_R_R1__(__zB__,__zD__);
break;
// 0x43 LD B,E
case LD_B_E:
__zLD_R_R1__(__zB__,__zE__);
break;
// 0x44 LD B,H
case LD_B_H:
__zLD_R_R1__(__zB__,__zH__);
break;
// 0x45 LD B,L
case LD_B_L:
__zLD_R_R1__(__zB__,__zL__);
break;
// 0x46 LD B,(HL)
case LD_B_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x47 LD B,A
case LD_B_A:
__zLD_R_R1__(__zB__,__zA__);
break;
// 0x48 LD C,B
case LD_C_B:
__zLD_R_R1__(__zC__,__zB__);
break;
// 0x49 LD C,C
case LD_C_C:
__zLD_R_R1__(__zC__,__zC__);
break;
// 0x4a LD C,D
case LD_C_D:
__zLD_R_R1__(__zC__,__zD__);
break;
// 0x4b LD C,E
case LD_C_E:
__zLD_R_R1__(__zC__,__zE__);
break;
// 0x4c LD C,H
case LD_C_H:
__zLD_R_R1__(__zC__,__zH__);
break;
// 0x4d LD C,L
case LD_C_L:
__zLD_R_R1__(__zC__,__zL__);
break;
// 0x4e LD C,(HL)
case LD_C_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x4f LD C,A
case LD_C_A:
__zLD_R_R1__(__zC__,__zA__);
break;
// 0x50 LD D,B
case LD_D_B:
__zLD_R_R1__(__zD__,__zB__);
break;
// 0x51 LD D,C
case LD_D_C:
__zLD_R_R1__(__zD__,__zC__);
break;
// 0x52 LD D,D
case LD_D_D:
__zLD_R_R1__(__zD__,__zD__);
break;
// 0x53 LD D,E
case LD_D_E:
__zLD_R_R1__(__zD__,__zE__);
break;
// 0x54 LD D,H
case LD_D_H:
__zLD_R_R1__(__zD__,__zH__);
break;
// 0x55 LD D,L
case LD_D_L:
__zLD_R_R1__(__zD__,__zL__);
break;
// 0x56 LD D,(HL)
case LD_D_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x57 LD D,A
case LD_D_A:
__zLD_R_R1__(__zD__,__zA__);
break;
// 0x58 LD E,B
case LD_E_B:
__zLD_R_R1__(__zE__,__zB__);
break;
// 0x59 LD E,C
case LD_E_C:
__zLD_R_R1__(__zE__,__zC__);
break;
// 0x5a LD E,D
case LD_E_D:
__zLD_R_R1__(__zE__,__zD__);
break;
// 0x5b LD E,E
case LD_E_E:
__zLD_R_R1__(__zE__,__zE__);
break;
// 0x5c LD E,H
case LD_E_H:
__zLD_R_R1__(__zE__,__zH__);
break;
// 0x5d LD E,L
case LD_E_L:
__zLD_R_R1__(__zE__,__zL__);
break;
// 0x5e LD E,(HL)
case LD_E_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x5f LD E,A
case LD_E_A:
__zLD_R_R1__(__zE__,__zA__);
break;
// 0x60 LD H,B
case LD_H_B:
__zLD_R_R1__(__zH__,__zB__);
break;
// 0x61 LD H,C
case LD_H_C:
__zLD_R_R1__(__zH__,__zC__);
break;
// 0x62 LD H,D
case LD_H_D:
__zLD_R_R1__(__zH__,__zD__);
break;
// 0x63 LD H,E
case LD_H_E:
__zLD_R_R1__(__zH__,__zE__);
break;
// 0x64 LD H,H
case LD_H_H:
__zLD_R_R1__(__zH__,__zH__);
break;
// 0x65 LD H,L
case LD_H_L:
__zLD_R_R1__(__zH__,__zL__);
break;
// 0x66 LD H,(HL)
case LD_H_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x67 LD H,A
case LD_H_A:
__zLD_R_R1__(__zH__,__zA__);
break;
// 0x68 LD L,B
case LD_L_B:
__zLD_R_R1__(__zL__,__zB__);
break;
// 0x69 LD L,C
case LD_L_C:
__zLD_R_R1__(__zL__,__zC__);
break;
// 0x6a LD L,D
case LD_L_D:
__zLD_R_R1__(__zL__,__zD__);
break;
// 0x6b LD L,E
case LD_L_E:
__zLD_R_R1__(__zL__,__zE__);
break;
// 0x6c LD L,H
case LD_L_H:
__zLD_R_R1__(__zL__,__zH__);
break;
// 0x6d LD L,L
case LD_L_L:
__zLD_R_R1__(__zL__,__zL__);
break;
// 0x6e LD L,(HL)
case LD_L_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x6f LD L,A
case LD_L_A:
__zLD_R_R1__(__zL__,__zA__);
break;
// 0x70 LD (HL),B
case LD_MM_HL_MM_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x71 LD (HL),C
case LD_MM_HL_MM_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x72 LD (HL),D
case LD_MM_HL_MM_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x73 LD (HL),E
case LD_MM_HL_MM_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x74 LD (HL),H
case LD_MM_HL_MM_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x75 LD (HL),L
case LD_MM_HL_MM_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x76 HALT
case HALT:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x77 LD (HL),A
case LD_MM_HL_MM_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x78 LD A,B
case LD_A_B:
__zLD_R_R1__(__zA__,__zB__);
break;
// 0x79 LD A,C
case LD_A_C:
__zLD_R_R1__(__zA__,__zC__);
break;
// 0x7a LD A,D
case LD_A_D:
__zLD_R_R1__(__zA__,__zD__);
break;
// 0x7b LD A,E
case LD_A_E:
__zLD_R_R1__(__zA__,__zE__);
break;
// 0x7c LD A,H
case LD_A_H:
__zLD_R_R1__(__zA__,__zH__);
break;
// 0x7d LD A,L
case LD_A_L:
__zLD_R_R1__(__zA__,__zL__);
break;
// 0x7e LD A,(HL)
case LD_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x7f LD A,A
case LD_A_A:
__zLD_R_R1__(__zA__,__zA__);
break;
// 0x80 ADD A,B
case ADD_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x81 ADD A,C
case ADD_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x82 ADD A,D
case ADD_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x83 ADD A,E
case ADD_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x84 ADD A,H
case ADD_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x85 ADD A,L
case ADD_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x86 ADD A,(HL)
case ADD_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x87 ADD A,A
case ADD_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x88 ADC A,B
case ADC_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x89 ADC A,C
case ADC_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8a ADC A,D
case ADC_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8b ADC A,E
case ADC_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8c ADC A,H
case ADC_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8d ADC A,L
case ADC_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8e ADC A,(HL)
case ADC_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x8f ADC A,A
case ADC_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x90 SUB A,B
case SUB_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x91 SUB A,C
case SUB_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x92 SUB A,D
case SUB_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x93 SUB A,E
case SUB_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x94 SUB A,H
case SUB_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x95 SUB A,L
case SUB_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x96 SUB A,(HL)
case SUB_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x97 SUB A,A
case SUB_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x98 SBC A,B
case SBC_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x99 SBC A,C
case SBC_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9a SBC A,D
case SBC_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9b SBC A,E
case SBC_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9c SBC A,H
case SBC_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9d SBC A,L
case SBC_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9e SBC A,(HL)
case SBC_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0x9f SBC A,A
case SBC_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa0 AND A,B
case AND_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa1 AND A,C
case AND_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa2 AND A,D
case AND_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa3 AND A,E
case AND_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa4 AND A,H
case AND_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa5 AND A,L
case AND_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa6 AND A,(HL)
case AND_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa7 AND A,A
case AND_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa8 XOR A,B
case XOR_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xa9 XOR A,C
case XOR_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xaa XOR A,D
case XOR_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xab XOR A,E
case XOR_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xac XOR A,H
case XOR_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xad XOR A,L
case XOR_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xae XOR A,(HL)
case XOR_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xaf XOR A,A
case XOR_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb0 OR A,B
case OR_A_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb1 OR A,C
case OR_A_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb2 OR A,D
case OR_A_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb3 OR A,E
case OR_A_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb4 OR A,H
case OR_A_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb5 OR A,L
case OR_A_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb6 OR A,(HL)
case OR_A_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb7 OR A,A
case OR_A_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb8 CP B
case CP_B:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xb9 CP C
case CP_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xba CP D
case CP_D:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xbb CP E
case CP_E:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xbc CP H
case CP_H:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xbd CP L
case CP_L:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xbe CP (HL)
case CP_MM_HL_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xbf CP A
case CP_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc0 RET NZ
case RET_NZ:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc1 POP BC
case POP_BC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc2 JP NZ,nnnn
case JP_NZ_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc3 JP nnnn
case JP_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc4 CALL NZ,nnnn
case CALL_NZ_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc5 PUSH BC
case PUSH_BC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc6 ADD A,nn
case ADD_A_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc7 RST 00
case RST_00:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc8 RET Z
case RET_Z:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xc9 RET
case RET:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xca JP Z,nnnn
case JP_Z_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xcb shift CB
case shift_CB:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xcc CALL Z,nnnn
case CALL_Z_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xcd CALL nnnn
case CALL_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xce ADC A,nn
case ADC_A_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xcf RST 8
case RST_8:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd0 RET NC
case RET_NC:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd1 POP DE
case POP_DE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd2 JP NC,nnnn
case JP_NC_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd3 OUT (nn),A
case OUT_MM_nn_MM_A:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd4 CALL NC,nnnn
case CALL_NC_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd5 PUSH DE
case PUSH_DE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd6 SUB nn
case SUB_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd7 RST 10
case RST_10:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd8 RET C
case RET_C:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xd9 EXX
case EXX:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xda JP C,nnnn
case JP_C_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xdb IN A,(nn)
case IN_A_MM_nn_MM:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xdc CALL C,nnnn
case CALL_C_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xdd shift DD
case shift_DD:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xde SBC A,nn
case SBC_A_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xdf RST 18
case RST_18:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe0 RET PO
case RET_PO:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe1 POP HL
case POP_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe2 JP PO,nnnn
case JP_PO_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe3 EX (SP),HL
case EX_MM_SP_MM_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe4 CALL PO,nnnn
case CALL_PO_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe5 PUSH HL
case PUSH_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe6 AND nn
case AND_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe7 RST 20
case RST_20:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe8 RET PE
case RET_PE:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xe9 JP HL
case JP_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xea JP PE,nnnn
case JP_PE_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xeb EX DE,HL
case EX_DE_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xec CALL PE,nnnn
case CALL_PE_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xed shift ED
case shift_ED:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xee XOR A,nn
case XOR_A_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xef RST 28
case RST_28:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf0 RET P
case RET_P:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf1 POP AF
case POP_AF:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf2 JP P,nnnn
case JP_P_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf3 DI
case DI:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf4 CALL P,nnnn
case CALL_P_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf5 PUSH AF
case PUSH_AF:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf6 OR nn
case OR_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf7 RST 30
case RST_30:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf8 RET M
case RET_M:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xf9 LD SP,HL
case LD_SP_HL:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xfa JP M,nnnn
case JP_M_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xfb EI
case EI:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xfc CALL M,nnnn
case CALL_M_nnnn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xfd shift FD
case shift_FD:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xfe CP nn
case CP_nn:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
// 0xff RST 38
case RST_38:
// not implement
#ifdef Z80_OPCODES_TEST
instrNotImp=true;
#endif
break;
