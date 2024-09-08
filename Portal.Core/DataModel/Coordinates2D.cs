using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public abstract class Coordinates2D<T>
    {
        public T X { get; set; }
        public T Y { get; set; }

        protected Coordinates2D() { }

        protected Coordinates2D(T x, T y)
        {
            X = x;
            Y = y;
        }
    }

    public class PVector2Df:Coordinates2D<float>
    {
        public PVector2Df(float x, float y) : base(x, y)
        {
        }
    }

    public class PVector2Di : Coordinates2D<int>
    {
        public PVector2Di(int x, int y) : base(x, y)
        {
        }

        public PVector2Di(Size size) : base(size.Width, size.Height)
        {
        }
    }
}
