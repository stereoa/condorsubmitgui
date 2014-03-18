using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CondorSubmitGUI.Objects.Geometry;

namespace CondorSubmitGUI.Objects.Ortho
{
    class ATProject
    {
        public List<Photo> atPhotos = new List<Photo>();
        public List<ATBlock> atBlocks = new List<ATBlock>();
        public string atDirectory;
        public ATProject(string photoFile)
        {
            this.atDirectory = photoFile.Substring(0, photoFile.Length - 6);
            ParsePhotoFile(photoFile);

        }

        private void ParsePhotoFile(string photoFile)
        {
            using (StreamReader sr = new StreamReader(photoFile))
            {
                string currentLine = sr.ReadLine();
                while (!currentLine.Contains("begin photo_measurements"))
                {
                    currentLine = sr.ReadLine();
                }
                //read in measurements
                while (!currentLine.Contains("begin photo_parameters"))
                {
                    string[] currentPhotoInfo = currentLine.Split(' ');
                    Photo currentPhoto = new Photo();
                    currentPhoto.photoName = currentPhotoInfo[2].Split('\t')[0];
                    if (currentPhotoInfo.Length > 3) currentPhoto.flight = currentPhotoInfo[3].Split('\t')[0];

                    currentLine = sr.ReadLine();
                    while (!currentLine.Contains("end photo_measurements"))
                    {
                        currentPhoto.photoMeasurements.Add(currentLine);
                        currentLine = sr.ReadLine();
                    }
                    atPhotos.Add(currentPhoto);
                    currentLine = sr.ReadLine();
                    while (!currentLine.Contains("begin")) currentLine = sr.ReadLine();
                }
                //read all the photos up to the block section
                int photoKey = 1;

                while ((!currentLine.Contains("begin block")) && (sr.Peek() >= 0))
                {


                    string[] currentPhotoInfo = currentLine.Split(' ');
                    currentPhotoInfo = currentPhotoInfo[2].Split('\t');
                    Photo currentPhoto = FindPhoto(currentPhotoInfo[0]);

                    currentLine = sr.ReadLine();
                    while (!currentLine.Contains("end photo_parameters"))
                    {
                        string[] currentParameters = currentLine.Split('\t');
                        //split by space for the params not tab-delimted
                        if (currentParameters.Length == 1)
                        {
                            currentParameters = currentLine.Split(' ');
                            currentParameters[0] = currentParameters[1];
                        }
                        switch (currentParameters[0])
                        {
                            case " camera_name:":
                                currentPhoto.cameraName = currentParameters[1];
                                break;
                            case " camera_orientation:":
                                currentPhoto.cameraOrient = currentParameters[1];
                                break;
                            case " image_id:":
                                currentPhoto.filePath = currentParameters[1];
                                break;
                            case " GPS_TimeStamp:":
                                currentPhoto.gpsTimestamp = currentParameters[1];
                                break;
                            case "view_geometry:":
                                currentPhoto.viewGeometry = currentParameters[2];
                                break;
                            case " EO_parameters:":
                                for (int i = 1; i < 7; i++)
                                {
                                    currentPhoto.eoParams += "\t" + currentParameters[i];
                                }
                                break;
                            case " GIVEN_parameters:":
                                for (int i = 1; i < 7; i++)
                                {
                                    currentPhoto.givenParams += "\t" + currentParameters[i];
                                }
                                currentPhoto.flyingHeight = Convert.ToDouble(currentParameters[3]);
                                break;
                            case " GIVEN_std_devs:":
                                for (int i = 1; i < 7; i++)
                                {
                                    currentPhoto.givenStdDev += "\t" + currentParameters[i];
                                }
                                break;
                            case "footprint:":
                                currentPhoto.footprintCoords = currentParameters[2];
                                for (int i = 3; i < 10; i++)
                                {
                                    currentPhoto.footprintCoords += " " + currentParameters[i];
                                }
                                
                                string[] footprintItems = new string[7];
                                footprintItems = currentPhoto.footprintCoords.Split(' ');
                                List<Point> footprintPoints = new List<Point>();
                                for (int i = 0; i < 8; i += 2)
                                {
                                    float currentX = float.Parse(footprintItems[i]);
                                    float currentY = float.Parse(footprintItems[i + 1]);
                                    footprintPoints.Add(new Point(currentX, currentY));
                                }
                                currentPhoto.shape = new Polygon(footprintPoints);

                                break;
                            case "active_elevation:":
                                currentPhoto.activeElevation = currentParameters[2];
                                break;
                            case "DRIVE_type:":
                                currentPhoto.driveType = currentParameters[2];
                                break;
                            case " COMPUTED_std_devs:":
                                currentPhoto.computedStdDev = currentParameters[1];
                                break;
                            case " image_size:":
                                currentPhoto.imageSize = currentParameters[1];
                                break;
                            case " sensor_id:":
                                currentPhoto.sensorID = currentParameters[1];
                                break;
                        }
                        currentLine = sr.ReadLine();
                    }

                    //move to the next photo
                    while ((!currentLine.Contains("begin photo_parameters")) && (!currentLine.Contains("begin block")) && (sr.Peek() >= 0))
                    {
                        currentLine = sr.ReadLine();
                    }
                    currentPhoto.photoKey = photoKey.ToString();
                    photoKey++;

                }
                //read all the blocks until the end of the file
                while (sr.Peek() >= 0)
                {
                    ATBlock currentBlock = new ATBlock(currentLine.Substring(12, currentLine.Length - 12));
                    currentLine = sr.ReadLine();
                    while (!currentLine.Contains("end block"))
                    {
                        currentBlock.blockPhotos.Add(currentLine.Split(' ')[2]);
                        currentLine = sr.ReadLine();
                    }
                    atBlocks.Add(currentBlock);
                    sr.ReadLine();
                    currentLine = sr.ReadLine();

                }
            }
        }


        public Photo FindPhoto(string photoToFind)
        {
            return atPhotos.Find(p => p.photoName.Equals(photoToFind));
        }
        public ATBlock FindBlock(string blockToFind)
        {
            return atBlocks.Find(b => b.blockName.Equals(blockToFind));
        }
    }
}
