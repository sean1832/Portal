using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public enum PGeoType
    {
        Undefined,
        Mesh,
        Curve,
        Plane,
        PointLight,
        RectangularLight,
        SpotLight
    }

    public enum PCurveType
    {
        Base,
        Line,
        Arc,
        Circle,
        Polyline,
        Nurbs,
    }

    public enum PTextureType
    {
        None = 0,
        Bitmap = 1,
        Diffuse = 1,
        PBR_BaseColor = 1,
        Bump = 2,
        Opacity = 3,
        Transparency = 3,
        PBR_Subsurface = 10, 
        PBR_SubsurfaceScattering = 11,
        PBR_SubsurfaceScatteringRadius = 12,
        PBR_Metallic = 13, 
        PBR_Specular = 14,
        PBR_SpecularTint = 15, 
        PBR_Roughness = 16, 
        PBR_Anisotropic = 17, 
        PBR_Anisotropic_Rotation = 18, 
        PBR_Sheen = 19, 
        PBR_SheenTint = 20, 
        PBR_Clearcoat = 21, 
        PBR_ClearcoatRoughness = 22, 
        PBR_OpacityIor = 23, 
        PBR_OpacityRoughness = 24,
        PBR_Emission = 25, 
        PBR_AmbientOcclusion = 26, 
        PBR_Displacement = 28, 
        PBR_ClearcoatBump = 29, 
        PBR_Alpha = 30,
        Emap = 86,
    }

}
