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
        public float FieldOfView { get; set; }  // Vertical field of view in degrees
        public float SensorWidth { get; set; }  // Sensor width in millimeters (mm)
        public float FocalLength { get; set; }
        public PVector2Di Resolution { get; set; }  // Resolution of the camera output in pixels
        

        public PCamera(PVector3D position, PVector3D lookDirection, double lensLength, float sensorWidth, PVector2Di resolution)
        {
            Position = position;
            LookDirection = lookDirection;
            SensorWidth = sensorWidth;
            Resolution = resolution;
            FieldOfView = CalculateFieldOfView(lensLength, sensorWidth);
            FocalLength = ComputeFocalLength();
            ValidateDirection();
        }

        private void ValidateDirection()
        {
            if (!LookDirection.IsNormalized())
            {
                LookDirection = LookDirection.Normalize();
            }
        }

        private float ComputeFocalLength()
        {
            // Convert field of view from degrees to radians for calculation
            double fieldOfViewRadians = FieldOfView * (Math.PI / 180.0);

            // Calculate focal length using the formula: focalLength = (sensorWidth / 2) / Math.Tan(FOV / 2)
            double focalLength = (SensorWidth / 2) / Math.Tan(fieldOfViewRadians / 2);

            return (float)focalLength;
        }

        private float CalculateFieldOfView(double lensLength, double sensorWidth)
        {
            // Calculate horizontal FOV using lens length
            double horizontalFov = 2 * Math.Atan((sensorWidth / 2) / lensLength);

            // Convert horizontal FOV to vertical FOV
            double aspectRatio = (double)Resolution.X / Resolution.Y;
            double verticalFov = 2 * Math.Atan(Math.Tan(horizontalFov / 2) / aspectRatio);

            // Convert radians to degrees
            double verticalFovDegrees = verticalFov * (180.0 / Math.PI);

            return (float)verticalFovDegrees;
        }
    }
}
