using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PColor: PEntity
    {
        public Byte R { get;}
        public Byte G { get;}
        public Byte B { get;}
        public Byte A { get;}

        public PColor():base(PType.Color) {  }

        public PColor(Byte r, Byte g, Byte b, Byte a) : base(PType.Color)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public PColor(Byte r, Byte g, Byte b) : base(PType.Color)
        {
            R = r;
            G = g;
            B = b;
            A = 255;
        }

        public PColor(byte[] array) : base(PType.Color)
        {
            if (array.Length != 4)
            {
                throw new ArgumentException("Array must have 4 elements");
            }

            R = array[0];
            G = array[1];
            B = array[2];
            A = array[3];
        }
        public PColor(Color color) : base(PType.Color)
        {
            R = color.R;
            G = color.G;
            B = color.B;
            A = color.A;
        }

        public string ToHex()
        {
            return $"#{R:X2}{G:X2}{B:X2}{A:X2}";
        }

        public byte[] ToByteArray() {
            return new byte[] { R, G, B, A };
        }

        public override string ToString()
        {
            return $"R:{R}, G:{G}, B:{B}, A:{A}";
        }

        public override int GetHashCode() {
            return R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode() ^ A.GetHashCode();
        }

        public static PColor FromHexColor(string hexColor)
        {
            if (hexColor.Length != 7 && hexColor.Length != 9)
            {
                throw new ArgumentException("Hex color must be 7 or 9 characters long");
            }

            if (hexColor[0] != '#')
            {
                throw new ArgumentException("Hex color must start with #");
            }

            byte r = Convert.ToByte(hexColor.Substring(1, 2), 16);
            byte g = Convert.ToByte(hexColor.Substring(3, 2), 16);
            byte b = Convert.ToByte(hexColor.Substring(5, 2), 16);
            byte a = hexColor.Length == 9 ? Convert.ToByte(hexColor.Substring(7, 2), 16) : (byte)255;

            return new PColor(r, g, b, a);
        }
    }
}
