using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZXSinclair.Machines.Z80
{
    public partial class MachineZ80
    {
        protected void FillTableOpCodesED()
        {
            FillTableOpCodes
            (
                mOpCodesED, new Dictionary<byte, Action>
                {
                    [OpCodes.LD_A_I] = LD_A_I,
                    [OpCodes.LD_A_R] = LD_A_R,
                    [OpCodes.LD_I_A] = LD_I_A,
                    [OpCodes.LD_R_A] = LD_R_A
                }
            );
        }

        // LD A,I
        protected void LD_A_I()
        {
            var n = mRegs.I;

            mRegs.A = n;

            /*
             S is set if the I Register is negative; otherwise, it is reset.
Z is set if the I Register is 0; otherwise, it is reset.
H is reset.
P/V contains contents of IFF2.
N is reset.
C is not affected.
If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
             */
            mRegs.F = (byte)((uint)(mRegs.F & Flags.C) | mTableZS53[n]);
            if (IFF2)
                mRegs.F |= Flags.PV;

            AddCycles(1);
        }

        // LD A,R
        protected void LD_A_R()
        {
            var n = mRegs.R;

            mRegs.A = n;

            /*
             S is set if the I Register is negative; otherwise, it is reset.
Z is set if the I Register is 0; otherwise, it is reset.
H is reset.
P/V contains contents of IFF2.
N is reset.
C is not affected.
If an interrupt occurs during execution of this instruction, the Parity flag contains a 0.
             */
            mRegs.F = (byte)((uint)(mRegs.F & Flags.C) | mTableZS53[n]);
            if (IFF2)
                mRegs.F |= Flags.PV;

            AddCycles(1);
        }

        // LD I,A
        protected void LD_I_A()
        {
            mRegs.LD_I_A();
            AddCycles(1);
        }
        // LD I,A
        protected void LD_R_A()
        {
            mRegs.LD_R_A();
            AddCycles(1);
        }
    }
}
