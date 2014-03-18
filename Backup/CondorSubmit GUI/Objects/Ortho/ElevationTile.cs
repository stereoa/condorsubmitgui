using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CondorSubmitGUI.Objects.Geometry;


namespace CondorSubmitGUI.Objects.Ortho
{
    class ElevationTile : SpatialObject
    {
        public string tileName, filePath;

        public ElevationTile(string filePath)
        {
            this.filePath = filePath;
        }

        public void initElevationTile()
        {
            DateTime ascEditDT = new FileInfo(this.filePath).LastWriteTime;
            string regionFile = Path.GetDirectoryName(filePath) + "\\region\\" + Path.GetFileNameWithoutExtension(filePath) + ".rgn";
            if (!File.Exists(regionFile))
            {
                generateRegion(regionFile);
            }
            else
            {
                DateTime rgnEditDT = new FileInfo(regionFile).LastWriteTime;
                if (ascEditDT > rgnEditDT) generateRegion(regionFile);
                else loadRegionFile(regionFile);
            }
        }

        private void loadRegionFile(string regionFile)
        {
            using (StreamReader sr = new StreamReader(regionFile))
            {
                List<Point> points = new List<Point>();
                while (sr.Peek() >= 0)
                {
                    string currentLine = sr.ReadLine();
                    string[] currentLineItems = currentLine.Split(' ');
                    float x = float.Parse(currentLineItems[0]);
                    float y = float.Parse(currentLineItems[1]);
                    points.Add(new Point(x, y));
                }
                shape = new Polygon(points);
            }
        }

        public void generateRegion(string regionFile)
        {
            string currentTile = filePath;
            using (StreamReader sr = new StreamReader(currentTile))
            {
                float currentWest = 0;
                float currentEast = 0;
                float currentNorth = 0;
                float currentSouth = 0;
                switch (Path.GetExtension(currentTile).ToLower())
                {
                    #region ASC
                    case ".asc":
                        string currentLine = sr.ReadLine();
                        //determine the delimeter
                        char delimiter = ';';
                        if (currentLine.Contains(','))
                        {
                            delimiter = ',';
                        }
                        else if (currentLine.Contains(' '))
                        {
                            delimiter = ' ';
                        }
                        else if (currentLine.Contains('\t'))
                        {
                            delimiter = '\t';
                        }
                        string[] currentLineItems = currentLine.Split(delimiter);
                        float[] currentLineCoords = new float[2];
                        currentLineCoords[0] = float.Parse(currentLineItems[0]);
                        currentLineCoords[1] = float.Parse(currentLineItems[1]);
                        currentWest = currentLineCoords[0];
                        currentEast = currentLineCoords[0];
                        currentNorth = currentLineCoords[1];
                        currentSouth = currentLineCoords[1];
                        while (sr.Peek() >= 0)
                        {
                            currentLine = sr.ReadLine();
                            currentLineItems = currentLine.Split(delimiter);
                            currentLineCoords[0] = float.Parse(currentLineItems[0]);
                            currentLineCoords[1] = float.Parse(currentLineItems[1]);
                            if (currentLineCoords[0] < currentWest)
                            {
                                currentWest = currentLineCoords[0];
                            }
                            if (currentLineCoords[0] > currentEast)
                            {
                                currentEast = currentLineCoords[0];
                            }
                            if (currentLineCoords[1] < currentSouth)
                            {
                                currentSouth = currentLineCoords[1];
                            }
                            if (currentLineCoords[1] > currentNorth)
                            {
                                currentNorth = currentLineCoords[1];
                            }
                        }

                        break;
                    #endregion
                    #region DEM
                    case ".dem":
                        char[] buffer = new char[1024]; // size of A Record
                        sr.Read(buffer, 0, 1024);
                        float[] sw_coord = new float[2];
                        float[] nw_coord = new float[2];
                        float[] ne_coord = new float[2];
                        float[] se_coord = new float[2];
                        sw_coord[0] = (float)ParseDouble(buffer, 546);
                        sw_coord[1] = (float)ParseDouble(buffer, 570);
                        nw_coord[0] = (float)ParseDouble(buffer, 594);
                        nw_coord[1] = (float)ParseDouble(buffer, 618);
                        ne_coord[0] = (float)ParseDouble(buffer, 642);
                        ne_coord[1] = (float)ParseDouble(buffer, 666);
                        se_coord[0] = (float)ParseDouble(buffer, 690);
                        se_coord[1] = (float)ParseDouble(buffer, 714);
                        // find west extent
                        if (sw_coord[0] < nw_coord[0])
                        {
                            currentWest = sw_coord[0];
                        }
                        else
                        {
                            currentWest = nw_coord[0];
                        }
                        // find east extent
                        if (se_coord[0] > ne_coord[0])
                        {
                            currentEast = se_coord[0];
                        }
                        else
                        {
                            currentEast = ne_coord[0];
                        }
                        // find south extent
                        if (sw_coord[1] < se_coord[1])
                        {
                            currentSouth = sw_coord[1];
                        }
                        else
                        {
                            currentSouth = se_coord[1];
                        }
                        // find north extent
                        if (nw_coord[1] > ne_coord[1])
                        {
                            currentNorth = nw_coord[1];
                        }
                        else
                        {
                            currentNorth = ne_coord[1];
                        }

                        break;
                    #endregion
                }

                this.tileName = Path.GetFileNameWithoutExtension(currentTile);
                this.filePath = currentTile;

                List<Point> points = new List<Point>();
                points.Add(new Point(currentWest, currentNorth));
                points.Add(new Point(currentEast, currentNorth));
                points.Add(new Point(currentEast, currentSouth));
                points.Add(new Point(currentWest, currentSouth));
                shape = new Polygon(points);

                if (!Directory.Exists(Path.GetDirectoryName(filePath))) Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                using (StreamWriter sw = new StreamWriter(regionFile))
                {
                    foreach (Point p in points)
                    {
                        sw.WriteLine("{0} {1}", p.x, p.y);
                    }
                }
            }
        }
        double ParseDouble(char[] buffer, int start)
        {
            return ParseDouble(buffer, start, 24);
        }

        double ParseDouble(char[] buffer, int start, int count)
        {
            String s = new string(buffer, start, count).Replace('D', 'E');
            double d = 0;
            if (!double.TryParse(s.Trim(), out d)) d = -1;
            return d;

        }

    }
}
