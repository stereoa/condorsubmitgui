using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondorSubmitGUI.Objects.Geometry
{
    class BoundingBox
    {
        public float westExtent, eastExtent, northExtent, southExtent;
    
        public BoundingBox(float north, float south, float east, float west)
        {
            this.westExtent = west;
            this.eastExtent = east;
            this.southExtent = south;
            this.northExtent = north;
        }

        public BoundingBox(List<Point> points)
        {
            float westSide = points[0].x;
            float eastSide = points[0].x;
            float northSide = points[0].y;
            float southSide = points[0].y;

            foreach (Point currentPoint in points)
            {
                float currentX = currentPoint.x;
                float currentY = currentPoint.y;

                if (currentX < westSide)
                {
                    westSide = currentX;
                }
                if (currentX > eastSide)
                {
                    eastSide = currentX;
                }
                if (currentY < southSide)
                {
                    southSide = currentY;
                }
                if (currentY > northSide)
                {
                    northSide = currentY;
                }
            }
            this.northExtent = northSide;
            this.southExtent = southSide;
            this.eastExtent = eastSide;
            this.westExtent = westSide;
        }
    }
}
