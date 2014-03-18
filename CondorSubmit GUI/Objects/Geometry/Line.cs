using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CondorSubmitGUI.Objects.Geometry
{
    class Line
    {
        public Point p1, p2;
        public Line(Point p1, Point p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
        public bool isIntersecting(Line lineToCheck)
        {
            float denominator = ((p2.x - p1.x) * (lineToCheck.p2.y - lineToCheck.p1.y)) - ((p2.y - p1.y) * (lineToCheck.p2.x - lineToCheck.p1.x));
            float numerator1 = ((p1.y - lineToCheck.p1.y) * (lineToCheck.p2.x - lineToCheck.p1.x)) - ((p1.x - lineToCheck.p1.x) * (lineToCheck.p2.y - lineToCheck.p1.y));
            float numerator2 = ((p1.y - lineToCheck.p1.y) * (p2.x - p1.x)) - ((p1.x - lineToCheck.p1.x) * (p2.y - p1.y));

            // Detect coincident lines (has a problem, read below)
            if (denominator == 0) return numerator1 == 0 && numerator2 == 0;

            float r = numerator1 / denominator;
            float s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }
    }
}
