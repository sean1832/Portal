using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace Portal.Core.DataModel
{
    public abstract class PLight : PEntity
    {
        public PLightType PLightType { get; protected set; }
        public string LightBlenderType { get; protected set; }
        public PColor LightDiffuseColor { get; protected set; }
        public PAttenuation LightAttenuationType { get; protected set; }
        public PVector3D LightLocation { get; protected set; }
        public double LightIntensity { get; protected set; }
        public PVector3D LightDirection { get; protected set; }

        protected PLight(PLightType lightType) : base(PGeoType.Light)
        {
            PLightType = lightType;
        }
    }

    public class PPointLight : PLight
    {
        
        public PPointLight(string lightBlenderType,Color diffuseColor, PAttenuation lightAttenuationType,
            PVector3D lightLocation, double lightIntensity) : base(PLightType.PointLight)
        {
            LightBlenderType = lightBlenderType;
            LightDiffuseColor = new PColor(diffuseColor);
            LightAttenuationType = lightAttenuationType;
            LightLocation = lightLocation;
            LightIntensity = lightIntensity;
        }
    }

    public class PSunLight : PLight
    {

        public PSunLight(string lightBlenderType, Color diffuseColor, PAttenuation lightAttenuationType,
            PVector3D lightLocation, double lightIntensity) : base(PLightType.SunLight)
        {
            LightBlenderType = lightBlenderType;
            LightDiffuseColor = new PColor(diffuseColor);
            LightAttenuationType = lightAttenuationType;
            LightLocation = lightLocation;
            LightIntensity = lightIntensity;
        }
    }

    public class PRectangularLight : PLight
    {
        public PVector3D LightLength { get; private set; }
        public PVector3D LightWidth { get; private set; }

        public PRectangularLight(string lightBlenderType,Color diffuseColor, PAttenuation lightAttenuationType,
            PVector3D lightLocation, PVector3D lightDirection, double lightIntensity,
            PVector3D lightLength, PVector3D lightWidth) : base(PLightType.RectangularLight)
        {
            LightBlenderType = lightBlenderType;
            LightDiffuseColor = new PColor(diffuseColor);
            LightAttenuationType = lightAttenuationType;
            LightLocation = lightLocation;
            LightDirection = lightDirection;
            LightIntensity = lightIntensity;
            LightLength = lightLength;
            LightWidth = lightWidth;
        }
    }

    public class PSpotLight : PLight
    {
        public double LightSpotAngleRadians { get; private set; }
        public double LightHotSpot { get; private set; }
        public double LightShadowIntensity { get; private set; }

        public PSpotLight(string lightBlenderType, Color diffuseColor, PAttenuation lightAttenuationType,
            PVector3D lightLocation, PVector3D lightDirection, double lightIntensity,
            double lightSpotAngleRadians, double lightHotSpot, double lightShadowIntensity) : base(PLightType.SpotLight)
        {
            LightBlenderType = lightBlenderType;
            LightDiffuseColor = new PColor(diffuseColor);
            LightAttenuationType = lightAttenuationType;
            LightLocation = lightLocation;
            LightDirection = lightDirection;
            LightIntensity = lightIntensity;
            LightSpotAngleRadians = lightSpotAngleRadians;
            LightHotSpot = lightHotSpot;
            LightShadowIntensity = lightShadowIntensity;
        }
    }
}
