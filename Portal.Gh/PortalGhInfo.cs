using Grasshopper;
using Grasshopper.Kernel;
using System;
using System.Drawing;

namespace Portal.Gh
{
    public class PortalGhInfo : GH_AssemblyInfo
    {
        public override string Name => "Portal";

        //Return a 24x24 pixel bitmap to represent this GHA library.
        public override Bitmap Icon => null;

        //Return a short string describing the purpose of this GHA library.
        public override string Description => "";

        public override Guid Id => new Guid("235fe0a6-bc24-441f-8390-190baa88f2f7");

        //Return a string identifying you or your company.
        public override string AuthorName => "Zeke Zhang";

        //Return a string representing your preferred contact details.
        public override string AuthorContact => "";
    }
}