using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Gh.Common
{
    internal static class Config
    {
        public static string Category = "Portal";

        public static class SubCat
        {
            public static readonly string Params = "0.Params";
            public static readonly string Utils = "1.Utils";
            public static readonly string Remote = "2.Remote IO";
            public static readonly string Local = "3.Local IO";
            public static readonly string Serialization = "4.Serialization";
        }
    }
}
