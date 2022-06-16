using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LKDS_Type
{
    class Parent : UserControl
    {
        public const int mask = 0x01;

        public static byte[] ModbusCRC16Calc(byte[] data, int len)
        {
            byte[] CRC = new byte[2];
            ushort Register = 0xFFFF;
            ushort Polynom = 0xA001;

            for (int i = 0; i < len; i++)
            {
                Register = (ushort)(Register ^ data[i]);
                for (int j = 0; j < 8; j++)
                    if ((ushort)(Register & mask) == 1)
                    {
                        Register = (ushort)(Register >> 1);
                        Register = (ushort)(Register ^ Polynom);
                    }
                    else
                        Register = (ushort)(Register >> 1);
            }

            CRC[1] = (byte)(Register >> 8);
            CRC[0] = (byte)(Register & 0x00FF);

            return CRC;
        }
    }
}
