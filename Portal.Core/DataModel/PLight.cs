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
        public string Name { get; protected set; }
        public string LightType { get; protected set; }
        public string Color { get; protected set; }
        public string Attenuation { get; protected set; }
        public PVector3D AttenuationVector { get; protected set; }
        public PVector3D Position { get; protected set; }
        public double Intensity { get; protected set; }
        public double ShadowIntensity { get; protected set; }
        public PVector3D Direction { get; protected set; }

        protected PLight(PLightType lightType) : base(PGeoType.Light)
        {
            LightType = lightType.ToString();
        }
    }

    public class PPointLight : PLight
    {
        public PPointLight(string name, Color diffuseColor, PAttenuation attenuation, PVector3D attenuationVector,
            PVector3D position, double intensity, double shadowIntensity) : base(PLightType.Point)
        {
            Name = name;
            Color = new PColor(diffuseColor).ToHex();
            Attenuation = attenuation.ToString();
            AttenuationVector = attenuationVector;
            Position = position;
            Intensity = intensity;
            ShadowIntensity = shadowIntensity;
        }
    }

    public class PDirectionalLight : PLight
    {
        public PDirectionalLight(string name, Color diffuseColor, PAttenuation attenuation, PVector3D attenuationVector,
            PVector3D position, PVector3D direction, double intensity, double shadowIntensity) : base(PLightType.Directional)
        {
            Name = name;
            Color = new PColor(diffuseColor).ToHex();
            Attenuation = attenuation.ToString();
            AttenuationVector = attenuationVector;
            Position = position;
            Direction = direction;
            Intensity = intensity;
            ShadowIntensity = shadowIntensity;
        }
    }

    public class PRectangularLight : PLight
    {
        public PVector3D Length { get; private set; }
        public PVector3D Width { get; private set; }

        public PRectangularLight(string name, Color diffuseColor, PAttenuation attenuation, PVector3D attenuationVector,
            PVector3D position, PVector3D direction, double intensity, double shadowIntensity,
            PVector3D length, PVector3D width) : base(PLightType.Rectangular)
        {
            Name = name;
            Color = new PColor(diffuseColor).ToHex();
            Attenuation = attenuation.ToString();
            AttenuationVector = attenuationVector;
            Position = position;
            Direction = direction;
            Intensity = intensity;
            ShadowIntensity = shadowIntensity;
            Length = length;
            Width = width;
        }
    }

    public class PSpotRadii
    {
        public double Inner { get; set; }
        public double Outer { get; set; }

        public PSpotRadii(double inner, double outer)
        {
            Inner = inner;
            Outer = outer;
        }
    }
    public class PSpotLight : PLight
    {
        public double SpotAngleRadians { get; private set; }
        public PSpotRadii SpotRadii { get; private set; }
        public double HotSpot { get; private set; }

        public PSpotLight(string name, Color diffuseColor, PAttenuation attenuation, PVector3D attenuationVector,
            PVector3D position, PVector3D direction, double intensity,
            double spotAngleRadians, double hotSpot, double shadowIntensity, PSpotRadii spotRadii) : base(PLightType.Spot)
        {
            Name = name;
            Color = new PColor(diffuseColor).ToHex();
            Attenuation = attenuation.ToString();
            AttenuationVector = attenuationVector;
            Position = position;
            Direction = direction;
            Intensity = intensity;
            SpotAngleRadians = spotAngleRadians * 2; // Convert from half angle to full angle
            HotSpot = hotSpot;
            ShadowIntensity = shadowIntensity;
            SpotRadii = spotRadii;
        }
    }

    public class PLinearLight : PLight
    {
        public PLinearLight(string name, Color diffuseColor, PAttenuation attenuation, PVector3D attenuationVector,
            PVector3D position, PVector3D direction, double intensity, double shadowIntensity) : base(PLightType.Linear)
        {
            Name = name;
            Color = new PColor(diffuseColor).ToHex();
            Attenuation = attenuation.ToString();
            AttenuationVector = attenuationVector;
            Position = position;
            Direction = direction;
            Intensity = intensity;
            ShadowIntensity = shadowIntensity;
        }
    }

}
