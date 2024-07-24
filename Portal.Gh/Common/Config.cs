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
            public static readonly string Utils = "0.Utils";
            public static readonly string RemoteIpc = "1.RemoteIPC";
            public static readonly string LocalIpc = "2.LocalIPC";

        }
    }
}
