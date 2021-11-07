using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tactics_Script_Tool
{
    static class Extensions
    {
        public static string ReadAnsiString(this BinaryReader reader, Encoding encoding = null)
        {
            var buffer = new List<byte>(256);

            for (var b = reader.ReadByte(); b != 0; b = reader.ReadByte())
            {
                buffer.Add(b);
            }

            if (buffer.Count == 0)
            {
                return string.Empty;
            }

            if (encoding == null)
            {
                encoding = Encoding.GetEncoding("shift_jis");
            }

            return encoding.GetString(buffer.ToArray());
        }

        public static void WriteAlignmentBytes(this BinaryWriter writer, int align)
        {
            var position = Convert.ToInt32(writer.BaseStream.Position);
            var numBytesToPad = Binary.GetAlignedValue(position, 4) - position;

            for (var i = 0; i < numBytesToPad; i++)
            {
                writer.Write((byte)0);
            }
        }
    }
}
