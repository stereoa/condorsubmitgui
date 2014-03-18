using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CondorSubmitGUI.Objects.Geometry;

namespace CondorSubmitGUI.Objects.Ortho
{
    class SpatialObject
    {
        public Polygon shape;
        public SpatialObject()
        {

        }
        public bool isIntersecting(SpatialObject objectToTest)
        {
            if (shape.isIntersecting(objectToTest.shape)) return true;
            return false;
        }
    }
}
