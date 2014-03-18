using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CondorSubmitGUI.Objects.Geometry;
namespace CondorSubmitGUI.Objects.Ortho
{
    class Photo : SpatialObject
    {
        public Photo(string flight, string photoName, string filePath)
        {
            this.flight = flight;
            this.photoName = photoName;
            this.filePath = filePath;
        }
        public Photo()
        {

        }
        public string flight, 
            photoName, 
            filePath,
            cameraName,
            cameraOrient,
            photoKey,
            gpsTimestamp,
            viewGeometry,
            eoParams,
            givenParams,
            givenStdDev,
            footprintCoords,
            activeElevation,
            driveType,
            computedStdDev,
            imageSize,
            sensorID;
        public List<string> photoMeasurements = new List<string>();
        public double flyingHeight;

    }
}
