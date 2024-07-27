using System;
using System.Collections.Generic;
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
}
