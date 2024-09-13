using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Portal.Core.DataModel
{
    public class PCamera
    {
        public PVector3D Position { get; set; }
        public PVector3D LookDirection { get; set; }  // This should be a normalized vector
        public float VerticalFov { get; private set; }  // Vertical field of view in degrees
        public float HorizontalFov { get; private set; }  // Horizontal field of view in degrees
        public float SensorWidth { get; set; }  // Sensor width in millimeters (mm)
        public float FocalLength { get; set; }  // Focal length in millimeters (mm)
        public PVector2Di Resolution { get; set; }  // Resolution of the camera output in pixels

        public PCamera(PVector3D position, PVector3D lookDirection, float focalLength, float sensorWidth, PVector2Di resolution)
        {
            Position = position;
            LookDirection = lookDirection.Normalize();
            FocalLength = focalLength;
            SensorWidth = sensorWidth;
            Resolution = resolution;
        }

        public static double CalculateFov(double angleRadians)
        {
            return (angleRadians * 180.0d) / Math.PI;
        }

        public void SetVerticalFov(float fov)
        {
            VerticalFov = fov;
        }

        public void SetHorizontalFov(float fov)
        {
            HorizontalFov = fov;
        }
    }


}
