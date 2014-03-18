using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondorSubmitGUI.Objects.Geometry
{
    class Polygon
    {
        public List<Line> lines;
        public List<Point> points;
        public BoundingBox boundingBox;

        public Polygon(List<Point> points)
        {
            lines = new List<Line>();
            this.points = points;
            for (int i = 0; i < points.Count - 1; i++)
            {
                lines.Add(new Line(points[i], points[i + 1]));
            }
            //close polygon with first and last point.
            lines.Add(new Line(points[0], points[points.Count - 1]));
            this.boundingBox = new BoundingBox(points);
        }

        public bool isIntersecting(Polygon polygonToCheck)
        {
            //check line intersections
            foreach (Line line in lines)
            {
                foreach (Line lineToCheck in polygonToCheck.lines)
                {
                    if (line.isIntersecting(lineToCheck))
                    {
                        return true;
                    }
                }
            }
            //check bb inside
            if (((boundingBox.westExtent >= polygonToCheck.boundingBox.westExtent) && (boundingBox.westExtent <= polygonToCheck.boundingBox.eastExtent)) || ((boundingBox.eastExtent >= polygonToCheck.boundingBox.westExtent) && (boundingBox.eastExtent <= polygonToCheck.boundingBox.eastExtent)))
            {
                if (((boundingBox.southExtent >= polygonToCheck.boundingBox.southExtent) && (boundingBox.southExtent <= polygonToCheck.boundingBox.northExtent)) || ((boundingBox.northExtent >= polygonToCheck.boundingBox.southExtent) && (boundingBox.northExtent <= polygonToCheck.boundingBox.northExtent)))
                {
                    return true;
                }
            }
            //and outside
            if (((polygonToCheck.boundingBox.westExtent >= boundingBox.westExtent) && (polygonToCheck.boundingBox.westExtent <= boundingBox.eastExtent)) || ((polygonToCheck.boundingBox.eastExtent >= boundingBox.westExtent) && (polygonToCheck.boundingBox.eastExtent <= boundingBox.eastExtent)))
            {
                if (((polygonToCheck.boundingBox.southExtent >= boundingBox.southExtent) && (polygonToCheck.boundingBox.southExtent <= boundingBox.northExtent)) || ((polygonToCheck.boundingBox.northExtent >= boundingBox.southExtent) && (polygonToCheck.boundingBox.northExtent <= boundingBox.northExtent)))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
