using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CondorSubmitGUI.Objects.Ortho
{
    class RectifyJob
    {
        public RectifyJob()
        {

        }
        public RectifyJob(List<Photo> photos, List<ElevationTile> elevationTiles, bool compress, string pixelSpacing, double pixelSize, decimal voidColor, string rectifyOutput, string atProject, string outputCSF)
        {
            this.photos = photos;
            this.elevationTiles = elevationTiles;
            this.compress = compress;
            this.rectifyOutput = rectifyOutput;
            this.pixelSpacing = pixelSpacing;
            this.pixelSize = pixelSize;
            this.voidColor = voidColor;
            this.atProject = atProject;
            this.outputCSF = outputCSF;
        }
        public bool compress;
        public double pixelSize;
        public decimal voidColor;
        public List<Photo> photos;
        public List<ElevationTile> elevationTiles = new List<ElevationTile>();
        public string pixelSpacing, rectifyOutput, atProject, outputCSF;


    }
}
