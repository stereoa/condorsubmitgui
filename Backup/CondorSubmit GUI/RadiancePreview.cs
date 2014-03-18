using BitMiracle.LibTiff.Classic;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Drawing.Drawing2D;
using System.Collections;

namespace CondorSubmitGUI
{

    public partial class RadiancePreview : System.Windows.Forms.Form
    {
        private CondorSubmitGUI MainParent = new CondorSubmitGUI();
        private Bitmap bmp;
        private static int imageWidth, imageHeight, tileWidth, tileHeight, tileSize, numOfColumns, numOfRows, numOfBands, currentImage;
        private Tiff image;

        public RadiancePreview(CondorSubmitGUI MainForm)
        {
            MainParent = MainForm;
            InitializeComponent();
            this.Location = new System.Drawing.Point(MainParent.Location.X + MainParent.Width, 0);

            //events to update the image on settings changed
            MainParent.RadianceRNUD.ValueChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceGNUD.ValueChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceBNUD.ValueChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceANUD.ValueChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceSharpenCheckbox.CheckedChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceRadiusTB.TextChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceSigmaTB.TextChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceContrastTB.TextChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadianceBrightTB.TextChanged += new System.EventHandler(this.Radiance_ValueChanged);
            MainParent.RadiancePreviewCheckbox.CheckedChanged += new System.EventHandler(this.Radiance_CheckboxChanged);
            MainParent.RadianceNextButton.Click += new System.EventHandler(this.Radiance_ChangeImage);
            MainParent.RadiancePrevButton.Click += new System.EventHandler(this.Radiance_ChangeImage);

            bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);


            //create required directories
            Directory.CreateDirectory(@"C:\temp\channels\red");
            Directory.CreateDirectory(@"C:\temp\channels\green");
            Directory.CreateDirectory(@"C:\temp\channels\blue");
            Directory.CreateDirectory(@"C:\temp\channels\alpha");
            Directory.CreateDirectory(@"C:\temp\channels\dodged\red");
            Directory.CreateDirectory(@"C:\temp\channels\dodged\green");
            Directory.CreateDirectory(@"C:\temp\channels\dodged\blue");
            Directory.CreateDirectory(@"C:\temp\channels\dodged\alpha");
            Directory.CreateDirectory(@"C:\temp\dodged\merged");
            Directory.CreateDirectory(@"C:\temp\sharpened");

            //open the first applicable image in the folder
            currentImage = 0;
            string tiffname = Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif")[currentImage];
            while (Directory.GetFiles(MainParent.RadianceInputTB.Text, Path.GetFileNameWithoutExtension(tiffname + ".tfw")).Length < 1)
            {
                currentImage++;
                tiffname = Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif")[currentImage];
            }
            LoadTiff(tiffname);
        }

        //Load tiff to display for the first time
        private bool LoadTiff(string tiffname)
        {
            int[] xyOffsets;
            using (image = Tiff.Open(tiffname, "r"))
            {
                if (image == null)
                {
                    MessageBox.Show("Could not open incoming image");
                    return false;
                }
                

                // Find the width and height of the image
                FieldValue[] value = image.GetField(TiffTag.TILEWIDTH);
                tileWidth = value[0].ToInt();

                value = image.GetField(TiffTag.TILELENGTH);
                tileHeight = value[0].ToInt();

                tileSize = tileHeight * tileWidth;

                
                // For the column and row calculations we round up 
                // because .21 or .55 of a column/row is still another
                // tile even if it isn't a complete tile.

                //find the number of tile columns
                value = image.GetField(TiffTag.IMAGEWIDTH);
                imageWidth = value[0].ToInt();
                numOfColumns = imageWidth / tileWidth + 1;

                //find the number of tile rows
                value = image.GetField(TiffTag.IMAGELENGTH);
                imageHeight = value[0].ToInt();
                numOfRows = imageHeight / tileHeight + 1;

                //find the number of bands
                value = image.GetField(TiffTag.SAMPLESPERPIXEL);
                numOfBands = value[0].ToInt();
                MainParent.RadianceNumBandsTB.Text = numOfBands.ToString();

                xyOffsets = AnalyzeTiff();
                createBase(xyOffsets[0], xyOffsets[1]);
                checkPreviewState();
                this.Text = tiffname;
                return true;
            }

        }

        //When we load a tiff (LoadTiff) we want to create a base of it
        private void createBase(int xOffset, int yOffset)
        {
            using (Tiff output = Tiff.Open(@"C:\base.tif", "w"))
            {
                if (output == null)
                {
                    System.Console.Error.WriteLine("Could not open outgoing image");
                    return;
                }


                // We need to know the width and the height before we can malloc

                int[] raster = new int[tileWidth * tileHeight];

                // Magical stuff for creating the image
                // Populating each spot in the array with the correct byte (0-255)

                //Write the tiff tags to the file
                output.SetField(TiffTag.IMAGEWIDTH, 1024);
                output.SetField(TiffTag.IMAGELENGTH, 1024);
                output.SetField(TiffTag.TILEWIDTH, 256);
                output.SetField(TiffTag.TILELENGTH, 256);
                output.SetField(TiffTag.ORIENTATION, 1);
                output.SetField(TiffTag.COMPRESSION, Compression.DEFLATE);
                output.SetField(TiffTag.PLANARCONFIG, PlanarConfig.CONTIG);
                output.SetField(TiffTag.PHOTOMETRIC, Photometric.RGB);
                output.SetField(TiffTag.BITSPERSAMPLE, 8);
                output.SetField(TiffTag.SAMPLESPERPIXEL, 4);
                byte[] inputBuffer = new byte[tileWidth * tileHeight * 4];
                // loop through every column
                xOffset = 20;
                yOffset = 20;
                for (int x = 0; x < 1024 / tileWidth; x++)
                {
                    //loop through every row in the current column
                    for (int y = 0; y < 1024 / tileHeight; y++)
                    {
                        image.ReadRGBATile((x + xOffset) * tileWidth, (y + yOffset) * tileHeight, raster);
                        Tiff.IntsToByteArray(raster, 0, raster.Length, inputBuffer, 0);
                        output.WriteEncodedTile(getTileIndex(x, y, 1024 / tileWidth), inputBuffer, inputBuffer.Length);
                    }
                }
            }
        }

        //Analyzes the tiff for an interesting part of it
        private int[] AnalyzeTiff()
        {
            int currentBestCount = 0;
            List<Color> colorList = new List<Color>();
            int xOffset = 0;
            int yOffset = 0;
            int[] raster = new int[tileSize];
            for (int g = 0; g < numOfColumns; g += 20)
            {
                for (int h = 0; h < numOfRows; h += 20)
                {
                    image.ReadRGBATile(tileWidth * g, tileHeight * h, raster);
                    for (int i = 0; i < tileWidth; i += 10)
                    {
                        for (int j = 0; j < tileHeight; j += 10)
                        {
                            Color color = getSample(i, j, raster, tileWidth, tileHeight);
                            if (!colorList.Contains(color))
                            {
                                colorList.Add(color);
                            }
                        }
                    }
                    if (colorList.Count > currentBestCount)
                    {
                        currentBestCount = colorList.Count;
                        xOffset = g;
                        yOffset = h;
                    }
                    colorList.Clear();
                }
            }
            int[] xyOffsets = new int[2];
            xyOffsets[0] = xOffset;
            xyOffsets[1] = yOffset;
            return xyOffsets;
        }

        //Generate a new preview tiff (only used when first loading an image otherwise individual changes are done via UpdateImage)
        private void generatePreview()
        {
            Process p = new Process();
            if (numOfBands == 3)
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "mr_file.exe";
                p.StartInfo.Arguments = @"-o 3 -T -S 256 C:\base.tif C:\temp\channels\red\temp.tif C:\temp\channels\green\temp.tif C:\temp\channels\blue\temp.tif";
                p.Start();
                p.WaitForExit();
            }
            else
            {
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "mr_file.exe";
                p.StartInfo.Arguments = @"-o 4 -T -S 256 C:\base.tif C:\temp\channels\red\temp.tif C:\temp\channels\green\temp.tif C:\temp\channels\blue\temp.tif C:\temp\channels\alpha\temp.tif";
                p.Start();
                p.WaitForExit();
            }
            //dodge red channel
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "DodgeCmd.exe";
            p.StartInfo.Arguments = @"-i C:\temp\channels\red\temp.tif -o C:\temp\channels\dodged\red\temp.tif -u -c " + MainParent.RadianceRNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
            p.Start();
            p.WaitForExit();

            //dodge blue channel
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "DodgeCmd.exe";
            p.StartInfo.Arguments = @"-i C:\temp\channels\blue\temp.tif -o C:\temp\channels\dodged\blue\temp.tif -u -c " + MainParent.RadianceBNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
            p.Start();
            p.WaitForExit();

            //dodge green channel
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "DodgeCmd.exe";
            p.StartInfo.Arguments = @"-i C:\temp\channels\green\temp.tif -o C:\temp\channels\dodged\green\temp.tif -u -c " + MainParent.RadianceGNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
            p.Start();
            p.WaitForExit();

            if (numOfBands == 4)
            {
                //dodge alpha channel
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "DodgeCmd.exe";
                p.StartInfo.Arguments = @"-i C:\temp\channels\alpha\temp.tif -o C:\temp\channels\dodged\alpha\temp.tif -u -c " + MainParent.RadianceANUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
                p.Start();
                p.WaitForExit();

                //merge dodged channels
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "mr_file.exe";
                p.StartInfo.Arguments = @"-T -i 4 -o 1 -S 256 C:\temp\channels\dodged\red\temp.tif C:\temp\channels\dodged\green\temp.tif C:\temp\channels\dodged\blue\temp.tif C:\temp\channels\dodged\alpha\temp.tif C:\temp\dodged\merged\temp.tif";
                p.Start();
                p.WaitForExit();
            }
            else
            {
                //merge dodged channels
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "mr_file.exe";
                p.StartInfo.Arguments = @"-T -i 3 -o 1 -S 256 C:\temp\channels\dodged\red\temp.tif C:\temp\channels\dodged\green\temp.tif C:\temp\channels\dodged\blue\temp.tif C:\temp\dodged\merged\temp.tif";
                p.Start();
                p.WaitForExit();
            }
            File.Copy(@"C:\temp\dodged\merged\temp.tif", @"C:\temp.tif", true);
            //sharpen/apply brightness contrast
            if (MainParent.RadianceSharpenCheckbox.Checked)
            {
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = @"C:\Program Files\ImageMagick-6.7.0-Q16\convert.exe";
                p.StartInfo.Arguments = @"-sharpen " + MainParent.RadianceRadiusTB.Text + "x" + MainParent.RadianceSigmaTB.Text + " -brightness-contrast " + MainParent.RadianceBrightTB.Text + "x" + MainParent.RadianceContrastTB.Text + @" -define tiff:tile-geometry=256x256 C:\temp\dodged\merged\temp.tif C:\temp\sharpened\temp.tif";
                p.Start();
                p.WaitForExit();
                File.Copy(@"C:\temp\sharpened\temp.tif", @"C:\temp.tif", true);
            }

        }

        //Displays the specified tiff in the picturebox
        private bool displayTiff(string tiffname)
        {
            int[] raster = new int[tileSize];
            try
            {
                using (image = Tiff.Open(tiffname, "r"))
                {
                    //loop through each tile column
                    for (int g = 0; g < bmp.Width / tileWidth + 1; g++)
                    {
                        //loop through each tile in that column
                        for (int h = 0; h < bmp.Height / tileHeight + 1; h++)
                        {
                            image.ReadRGBATile(tileWidth * g, tileHeight * h, raster);
                            //loop through each pixel column
                            for (int i = 0; i < tileWidth; i++)
                            {
                                //loop through each pixel in that column
                                for (int j = 0; j < tileHeight; j++)
                                {

                                    if ((g * tileWidth + i < bmp.Width) && (h * tileHeight + j < bmp.Height))
                                    {

                                        bmp.SetPixel(g * tileWidth + i, h * tileHeight + j, getSample(i, j, raster, tileWidth, tileHeight));

                                    }
                                }
                            }
                        }
                    }
                    pictureBox1.Image = bmp;
                    return true;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return false;
            }
        }

        #region helper functions
        //Used for getting the color of each pixel
        private static Color getSample(int x, int y, int[] raster, int tileWidth, int tileHeight)
        {
            int offset = (tileHeight - y - 1) * tileWidth + x;
            int red = Tiff.GetR(raster[offset]);
            int green = Tiff.GetG(raster[offset]);
            int blue = Tiff.GetB(raster[offset]);
            if (numOfBands == 4)
            {
                int alpha = Tiff.GetA(raster[offset]);
                return Color.FromArgb(alpha, red, green, blue);
            }
            else
            {
                return Color.FromArgb(red, green, blue);
            }
        }

        //Find if we are displaying the preview or base and display that tiff
        private void checkPreviewState()
        {
            if (!MainParent.RadiancePreviewCheckbox.Checked)
            {
                displayTiff(@"C:\base.tif");
            }
            else
            {
                generatePreview();
                displayTiff(@"C:\temp.tif");
            }
        }

        //When in preview mode update a change to the image settings
        private void updateImage(string senderName)
        {
            //update status label so the user knows something is working
            MainParent.StatusLabel.Text = "Generating..";
            Application.DoEvents();
            Process p;

            //split image into channels
            switch (senderName)
            {
                case "RadianceRNUD":
                    //dodge red channel
                    p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "DodgeCmd.exe";
                    p.StartInfo.Arguments = @"-i C:\temp\channels\red\temp.tif -o C:\temp\channels\dodged\red\temp.tif -u -c " + MainParent.RadianceRNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
                    p.Start();
                    p.WaitForExit();
                    break;
                case "RadianceBNUD":
                    //dodge blue channel
                    p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "DodgeCmd.exe";
                    p.StartInfo.Arguments = @"-i C:\temp\channels\blue\temp.tif -o C:\temp\channels\dodged\blue\temp.tif -u -c " + MainParent.RadianceBNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
                    p.Start();
                    p.WaitForExit();
                    break;
                case "RadianceGNUD":
                    //dodge green channel
                    p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.CreateNoWindow = true;
                    p.StartInfo.FileName = "DodgeCmd.exe";
                    p.StartInfo.Arguments = @"-i C:\temp\channels\green\temp.tif -o C:\temp\channels\dodged\green\temp.tif -u -c " + MainParent.RadianceGNUD.Value + " -n -t 128 -g -0 +g 0 -k 15 -f " + MainParent.RadianceFillNUD.Value;
                    p.Start();
                    p.WaitForExit();
                    break;
            }
            //merge dodged channels
            p = new Process();
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.FileName = "mr_file.exe";
            p.StartInfo.Arguments = @"-T -i 3 -o 1 -S 256 C:\temp\channels\dodged\red\temp.tif C:\temp\channels\dodged\green\temp.tif C:\temp\channels\dodged\blue\temp.tif C:\temp\dodged\merged\temp.tif";
            p.Start();
            p.WaitForExit();
            File.Copy(@"C:\temp\dodged\merged\temp.tif", @"C:\temp.tif", true);
            //sharpen/apply brightness contrast
            if (MainParent.RadianceSharpenCheckbox.Checked)
            {
                p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.FileName = "convert.exe";
                p.StartInfo.Arguments = @"-sharpen " + MainParent.RadianceRadiusTB.Text + "x" + MainParent.RadianceSigmaTB.Text + " -brightness-contrast " + MainParent.RadianceBrightTB.Text + "x" + MainParent.RadianceContrastTB.Text + @" -define tiff:tile-geometry=256x256 C:\temp\dodged\merged\temp.tif C:\temp\sharpened\temp.tif";
                p.Start();
                p.WaitForExit();
                File.Copy(@"C:\temp\sharpened\temp.tif", @"C:\temp.tif", true);
            }
            MainParent.StatusLabel.Text = "Idle";
            displayTiff(@"C:\temp.tif");
        }

        private int getTileIndex(int x, int y, int numOfColumns)
        {
            return y * numOfColumns + x;
        }
        //Find program files folder for x64/x86
        static string programFilesx86()
        {

            // Used to find all the tools on x64 and x86
            if (8 == IntPtr.Size
                || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }

            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        #endregion

        #region events
        //Apply changes to preview tiff and redisplay
        private void Radiance_ValueChanged(object sender, EventArgs e)
        {
            if (MainParent.RadiancePreviewCheckbox.Checked)
            {
                //Figure out which setting was changed and grab its name
                Control senderCasted = (Control)sender;
                string senderName = senderCasted.Name;
                updateImage(senderName);
            }
        }

        //Toggle preview
        private void Radiance_CheckboxChanged(object sender, EventArgs e)
        {
            checkPreviewState();
        }

        //Open next or previous image in folder
        private void Radiance_ChangeImage(object sender, EventArgs e)
        {
            //Figure out if we need the next or previous image
            Control senderCasted = (Control)sender;
            string senderName = senderCasted.Name;
            if (senderName == "RadianceNextButton") currentImage++;
            else currentImage--;

            //loop around the directory's array
            if (currentImage < 0) currentImage = Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif").Length - 1;
            if (currentImage > Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif").Length - 1) currentImage = 0;

            string tiffname = Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif")[currentImage];
            while (Directory.GetFiles(MainParent.RadianceInputTB.Text, Path.GetFileNameWithoutExtension(tiffname + ".tfw")).Length < 1)
            {
                if (senderName == "RadianceNextButton") currentImage++;
                else currentImage--;
                tiffname = Directory.GetFiles(MainParent.RadianceInputTB.Text, "*.tif")[currentImage];
            }
            LoadTiff(tiffname);
        }
        #endregion
    }
}
