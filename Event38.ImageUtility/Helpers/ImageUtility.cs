using Emgu.CV;
using Event38.ImageUtility._Classes;
using Event38.ImageUtility.Models;
using log4net;
using MissionPlanner;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Event38.Helpers
{
    public static class ImageUtilityHelper
    {

        private delegate void SetPropertyThreadSafeDelegate<TResult>(Control @this, Expression<Func<TResult>> property, TResult value);

        public static void SetPropertyThreadSafe<TResult>(this Control @this, Expression<Func<TResult>> property, TResult value)
        {
            var propertyInfo = (property.Body as MemberExpression).Member as PropertyInfo;

            if (propertyInfo == null ||
                !@this.GetType().IsSubclassOf(propertyInfo.ReflectedType) ||
                @this.GetType().GetProperty(propertyInfo.Name, propertyInfo.PropertyType) == null)
            {
                throw new ArgumentException("The lambda expression 'property' must reference a valid property on this Control.");
            }

            if (@this.InvokeRequired)
            {
                @this.Invoke(new SetPropertyThreadSafeDelegate<TResult>(SetPropertyThreadSafe), new object[] { @this, property, value });
            }
            else
            {
                @this.GetType().InvokeMember(propertyInfo.Name, BindingFlags.SetProperty, null, @this, new object[] { value });
            }
        }

        public static void SetAlpha(this Bitmap bmp, byte alpha)
        {
            if (bmp == null) throw new ArgumentNullException("bmp");

            var data = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                System.Drawing.Imaging.ImageLockMode.ReadWrite,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            var line = data.Scan0;
            var eof = line + data.Height * data.Stride;
            while (line != eof)
            {
                var pixelAlpha = line + 3;
                var eol = pixelAlpha + data.Width * 4;
                while (pixelAlpha != eol)
                {
                    System.Runtime.InteropServices.Marshal.WriteByte(
                        pixelAlpha, alpha);
                    pixelAlpha += 4;
                }
                line += data.Stride;
            }
            bmp.UnlockBits(data);
        }
        public static void WriteCoordinatesToImage(string Filename, double dLat, double dLong, double alt)
        {
            byte[] bLat = BitConverter.GetBytes(dLat);
            byte[] bLong = BitConverter.GetBytes(dLong);
            using (MemoryStream ms = new MemoryStream(File.ReadAllBytes(Filename)))
            {

                //File.Delete(Filename);
                using (Image Pic = Image.FromStream(ms))
                {
                    PropertyItem[] pi = Pic.PropertyItems;

                    pi[0].Id = 0x0004;
                    pi[0].Type = 5;
                    pi[0].Len = sizeof(ulong) * 3;
                    pi[0].Value = coordtobytearray(dLong);
                    Pic.SetPropertyItem(pi[0]); 

                    pi[0].Id = 0x0002;
                    pi[0].Type = 5;
                    pi[0].Len = sizeof(ulong) * 3;
                    pi[0].Value = coordtobytearray(dLat);
                    Pic.SetPropertyItem(pi[0]); 

                    pi[0].Id = 0x0006;
                    pi[0].Type = 5;
                    pi[0].Len = 8;
                    pi[0].Value = new Event38.ImageUtility._Classes.Rational(alt).GetBytes();
                    Pic.SetPropertyItem(pi[0]); 

                    pi[0].Id = 1;
                    pi[0].Len = 2;
                    pi[0].Type = 2;

                    if (dLat < 0)
                    {
                        pi[0].Value = new byte[] { (byte)'S', 0 };
                    }
                    else
                    {
                        pi[0].Value = new byte[] { (byte)'N', 0 };
                    }

                    Pic.SetPropertyItem(pi[0]); 

                    pi[0].Id = 3;
                    pi[0].Len = 2;
                    pi[0].Type = 2;
                    if (dLong < 0)
                    {
                        pi[0].Value = new byte[] { (byte)'W', 0 };
                    }
                    else
                    {
                        pi[0].Value = new byte[] { (byte)'E', 0 };
                    }
                    Pic.SetPropertyItem(pi[0]); 

                    // Save file into Geotag folder
                    //string rootFolder = TXT_jpgdir.Text;
                    //string geoTagFolder = rootFolder + Path.DirectorySeparatorChar + "geotagged";

                    //string outputfilename = geoTagFolder + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(Filename) + "_geotag" + Path.GetExtension(Filename);

                    string strFile = Path.GetFileName(Filename);
                    string outputfile = Path.GetDirectoryName(Filename) + "\\processed\\" + strFile;

                    bool exists = System.IO.Directory.Exists(Path.GetDirectoryName(Filename) + "\\processed\\");

                    if (!exists)
                        System.IO.Directory.CreateDirectory(Path.GetDirectoryName(Filename) + "\\processed\\");


                    // Just in case
                    //  File.Delete(Filename);

                    Pic.Save(outputfile);
                }
            }
        }

        public static byte[] coordtobytearray(double coordin)
        {
            double coord = Math.Abs(coordin);

            byte[] output = new byte[sizeof(double) * 3];

            int d = (int)coord;
            int m = (int)((coord - d) * 60);
            double s = ((((coord - d) * 60) - m) * 60);

            Array.Copy(BitConverter.GetBytes((uint)d), 0, output, 0, sizeof(uint));
            Array.Copy(BitConverter.GetBytes((uint)1), 0, output, 4, sizeof(uint));
            Array.Copy(BitConverter.GetBytes((uint)m), 0, output, 8, sizeof(uint));
            Array.Copy(BitConverter.GetBytes((uint)1), 0, output, 12, sizeof(uint));
            Array.Copy(BitConverter.GetBytes((uint)(s * 10)), 0, output, 16, sizeof(uint));
            Array.Copy(BitConverter.GetBytes((uint)10), 0, output, 20, sizeof(uint));

            return output;
        }


        public static List<CameraLog> ParseCSV2(string FileName)
        {
            List<CameraLog> data = new List<CameraLog>();
            int OrderAdded = 0;
            int firstGPSTime = 0;
            int firstATTTime = 0;
            int differanceATTGPS = 0;


            //     TimeMS,Roll,Pitch,Yaw,ErrorRP,ErrorYaw
            //ATT, 199621, -0.45, -2.00, 227.92, 0.07, 0.00

            //     Status,TimeMS,Week,NSats,HDop,Lat,Lng,RelAlt,Alt,Spd,GCrs,VZ,T
            //GPS, 3, 576146200, 1828, 8, 1.65, 41.0361204, -81.5315305, 115.42, 406.58, 7.33, 177.23, 0.330000, 277634

            //      GPSTime,  GPSWeek,   Lat,          Lng,      Alt,    RelAlt,  Roll,Pitch, Yaw
            //CAM, 576146000, 1828,    41.0361205, -81.5315307, 409.17, 115.36, -4.67, 0.34, 211.32


            string[] records = File.ReadAllLines(FileName);
            CameraLog newLog = new CameraLog();


            foreach (string item in records)
            {
                if (item.StartsWith("GPS"))
                {
                    newLog = new CameraLog();
                    string[] gpsLine = item.Split(new[] { ',' });

                    if (gpsLine[9] != null)
                    {
                        newLog.GPSAltitude = float.Parse(gpsLine[9])/1000;
                    }
                    newLog.GPSMilliseconds = int.Parse(gpsLine[2]);

                    if (firstGPSTime == 0)
                    {
                        firstGPSTime = int.Parse(gpsLine[2]);

                        differanceATTGPS = firstGPSTime - firstATTTime;
                    }


                    newLog.Latitude = decimal.Parse(gpsLine[6], System.Globalization.NumberStyles.Float);
                    newLog.Longitude = decimal.Parse(gpsLine[7], System.Globalization.NumberStyles.Float);


                    newLog.LogType = "GPS";
                    newLog.OrderAdded = OrderAdded;

                    data.Add(newLog);

                    OrderAdded = OrderAdded + 1;
                }
                else if (item.StartsWith("CAM"))
                {
                    newLog = new CameraLog();
                    string[] camLine = item.Split(new[] { ',' });

                    newLog.LogType = "CAM";
                    newLog.GPSMilliseconds = int.Parse(camLine[1]);
                    newLog.GPSWeek = int.Parse(camLine[2]);
                    newLog.Latitude = decimal.Parse(camLine[3], System.Globalization.NumberStyles.Float);
                    newLog.Longitude = decimal.Parse(camLine[4], System.Globalization.NumberStyles.Float);
                    newLog.Pitch = decimal.Parse(camLine[6], System.Globalization.NumberStyles.Float);
                    newLog.Roll = decimal.Parse(camLine[7], System.Globalization.NumberStyles.Float);
                    newLog.Yaw = decimal.Parse(camLine[8], System.Globalization.NumberStyles.Float);

                    data.Add(newLog);

                    newLog.OrderAdded = OrderAdded;

                    OrderAdded = OrderAdded + 1; ;

                }
                //else if (item.StartsWith("ATT"))
                //{
                //    string[] camLine = item.Split(new[] { ',' });

                //    newLog = new CameraLog();
                //    newLog.LogType = "ATT";
                //    newLog.OrderAdded = OrderAdded;

                //    firstATTTime = int.Parse(camLine[1]);

                //    newLog.GPSMilliseconds = differanceATTGPS + int.Parse(camLine[1]);//int.Parse(camLine[1]);



                //    newLog.Roll = decimal.Parse(camLine[2], System.Globalization.NumberStyles.Float);
                //    newLog.Pitch = decimal.Parse(camLine[3], System.Globalization.NumberStyles.Float);
                //    newLog.Yaw = decimal.Parse(camLine[4], System.Globalization.NumberStyles.Float);


                //    data.Add(newLog);

                //    OrderAdded = OrderAdded + 1; ;
                //}
            }

            return data;

        }


        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);



        public static List<CameraLog> ParseCSVtLog(string FileName)
        {

            //         mavlink_gps_raw_int_t 
            //mavlink_attitude_t 
            //mavlink_camera_feedback_t
            List<CameraLog> data = new List<CameraLog>();
            CameraLog newLog = new CameraLog();
            int OrderAdded = 0;
            int firstGPSTime = 0;
            int firstATTTime = 0;
            int curAttTime = 0;
            int differanceATTGPS = 0;
            float alt = 0;

            string[] RecordTypes = { "mavlink_gps_raw_int_t", "mavlink_attitude_t", "mavlink_camera_feedback_t" };
            string[] GPSRecords = { "time_usec", "lat", "lon", "alt" };
            string[] AttitudeRecords = { "time_boot_ms", "roll", "pitch", "yaw" };
            string[] CameraRecords = { "time_usec", "lat", "lng", "roll", "pitch", "yaw" };
            string[] records = File.ReadAllLines(FileName);

            //            mavlink_gps_raw_int_t time_usec
            //mavlink_gps_raw_int_t lat
            //mavlink_gps_raw_int_t lon
            //mavlink_gps_raw_int_t alt

            //mavlink_attitude_t time_boot_ms
            //mavlink_attitude_t roll
            //mavlink_attitude_t pitch
            //mavlink_attitude_t yaw

            //mavlink_camera_feedback_t time_usec
            //mavlink_camera_feedback_t lat
            //mavlink_camera_feedback_t lng
            //mavlink_camera_feedback_t roll
            //mavlink_camera_feedback_t pitch
            //mavlink_camera_feedback_t yaw



            foreach (var line in records)
            {
                if (line.Contains("mavlink_gps_raw_int_t"))
                {
                    newLog = new CameraLog();
                    string[] gpsLine = line.Split(new[] { ',' });
                    //mavlink_gps_raw_int_t time_usec
                    //mavlink_gps_raw_int_t lat
                    //mavlink_gps_raw_int_t lon
                    //mavlink_gps_raw_int_t alt


                    //
                    //2015-03-03T11:54:05.778,FE,1E,88, 1, 1,18,mavlink_gps_raw_int_t,time_usec,490409000,lat,410702385,lon,-815280022,alt,329770,eph,169,epv,65535,vel,13,cog,21630,fix_type,3,satellites_visible,7,,Len,38

                    //time = 9
                    //lat = 11
                    //lon = 13
                    //alt = 15

                    if (gpsLine[15] != null)
                    {
                        if (alt > 0)
                        {
                            newLog.GPSAltitude = alt;
                        }
                        else
                        {
                            newLog.GPSAltitude = float.Parse(gpsLine[15]);
                        }
                    }

                    newLog.GPSMilliseconds = int.Parse(gpsLine[9]) / 1000;

                    if (firstGPSTime == 0)
                    {
                        firstGPSTime = int.Parse(gpsLine[9]) / 1000;

                        differanceATTGPS = firstGPSTime - firstATTTime;
                    }

                    //string lat = ().ToString();

                    newLog.Latitude = decimal.Parse(gpsLine[11], System.Globalization.NumberStyles.Float) / 10000000;
                    newLog.Longitude = decimal.Parse(gpsLine[13], System.Globalization.NumberStyles.Float) / 10000000;// decimal.Parse(gpsLine[13]);


                    newLog.LogType = "GPS";
                    newLog.OrderAdded = OrderAdded;

                    data.Add(newLog);

                    OrderAdded = OrderAdded + 1;
                }
                else if (line.Contains("mavlink_global_position_int_t"))
                {
                    newLog = new CameraLog();
                    string[] gpsLine = line.Split(new[] { ',' });
                    //mavlink_gps_raw_int_t time_usec
                    //mavlink_gps_raw_int_t lat
                    //mavlink_gps_raw_int_t lon
                    //mavlink_gps_raw_int_t alt


                    //
                    //2015-03-03T11:54:05.778,FE,1E,88, 1, 1,18,mavlink_gps_raw_int_t,time_usec,490409000,lat,410702385,lon,-815280022,alt,329770,eph,169,epv,65535,vel,13,cog,21630,fix_type,3,satellites_visible,7,,Len,38

                    //time = 9
                    //lat = 11
                    //lon = 13
                    //alt = 15

                    if (gpsLine[15] != null)
                    {
                        newLog.Time = double.Parse(gpsLine[9]);
                        if (alt > 0)
                        {
                            newLog.GPSAltitude = alt;
                        }
                        else
                        {
                            newLog.GPSAltitude = float.Parse(gpsLine[15]);
                        }
                    }

                    newLog.GPSMilliseconds = int.Parse(gpsLine[9]) / 1000;

                    if (firstGPSTime == 0)
                    {
                        firstGPSTime = int.Parse(gpsLine[9]) / 1000;

                        differanceATTGPS = firstGPSTime - firstATTTime;
                    }

                    //string lat = ().ToString();

                    newLog.Latitude = decimal.Parse(gpsLine[11], System.Globalization.NumberStyles.Float) / 10000000;
                    newLog.Longitude = decimal.Parse(gpsLine[13], System.Globalization.NumberStyles.Float) / 10000000;// decimal.Parse(gpsLine[13]);


                    newLog.LogType = "GPS";
                    newLog.OrderAdded = OrderAdded;

                    data.Add(newLog);

                    OrderAdded = OrderAdded + 1;
                }
                //else if (line.Contains("mavlink_vfr_hud_t"))
                //{
                //    //2015-03-03T11:54:05.773,FE,14,77, 1, 1,4A,mavlink_vfr_hud_t,airspeed,0.1019804,groundspeed,0.09999999,alt,375.09,climb,-0.005979326,heading,19,throttle,0,,Len,28
                //    //13
                //    string[] gpsLine = line.Split(new[] { ',' });
                //    alt = decimal.Parse(gpsLine[13], System.Globalization.NumberStyles.Float);
                //}
                else if (line.Contains("mavlink_attitude_t"))
                {


                    //2015-03-03T11:54:05.776,FE,1C,7D, 1, 1,1E,mavlink_attitude_t,time_boot_ms,490309,roll,-0.01180428,pitch,0.02022862,yaw,0.3458395,rollspeed,0.0006728107,pitchspeed,0.0002039404,yawspeed,-0.0009780484,,Len,36

                    string[] camLine = line.Split(new[] { ',' });

                    newLog = new CameraLog();
                    newLog.LogType = "ATT";
                    newLog.OrderAdded = OrderAdded;

                    newLog.GPSAltitude = float.Parse(camLine[13]);
                    newLog.Time = double.Parse(camLine[9]);

                    firstATTTime = int.Parse(camLine[9], System.Globalization.NumberStyles.Float);
                    curAttTime = int.Parse(camLine[9], System.Globalization.NumberStyles.Float);

                    newLog.GPSMilliseconds = differanceATTGPS + int.Parse(camLine[9]);//int.Parse(camLine[1]);



                    newLog.Roll = decimal.Parse(camLine[11], System.Globalization.NumberStyles.Float);
                    newLog.Pitch = decimal.Parse(camLine[13], System.Globalization.NumberStyles.Float);
                    newLog.Yaw = decimal.Parse(camLine[15], System.Globalization.NumberStyles.Float);


                    data.Add(newLog);

                    OrderAdded = OrderAdded + 1; ;


                }

                else if (line.Contains("mavlink_camera_feedback_t"))
                {
                    newLog = new CameraLog();
                    string[] camLine = line.Split(new[] { ',' });




                    //2015-03-03T11:54:31.712,FE,2D,A4, 1, 1,B4,mavlink_camera_feedback_t,time_usec,1425401665561000,lat,410702317,lng,-815280296,alt_msl,375.75,alt_rel,7.85,roll,-0.49,pitch,0.85,yaw,20.38,foc_len,0,img_idx,18,target_system,0,cam_idx,0,flags,0,,Len,53


                    //mavlink_camera_feedback_t time_usec = 9
                    //mavlink_camera_feedback_t lat   =    11
                    //mavlink_camera_feedback_t lng   =    13
                    //mavlink_camera_feedback_t roll  =    19
                    //mavlink_camera_feedback_t pitch  =   21
                    //mavlink_camera_feedback_t yaw    = 23


                    newLog.LogType = "CAM";
                    newLog.GPSMilliseconds = curAttTime;// long.Parse(camLine[9]) / 1000;
                    //newLog.GPSWeek = int.Parse(camLine[11]);

                    newLog.Time = double.Parse(camLine[9]);
                    newLog.Latitude = decimal.Parse(camLine[11], System.Globalization.NumberStyles.Float) / 10000000; //decimal.Parse(camLine[11]);
                    newLog.Longitude = decimal.Parse(camLine[13], System.Globalization.NumberStyles.Float) / 10000000; //decimal.Parse(camLine[13]);
                    newLog.Pitch = decimal.Parse(camLine[21], System.Globalization.NumberStyles.Float);
                    newLog.Roll = decimal.Parse(camLine[19], System.Globalization.NumberStyles.Float);
                    newLog.Yaw = decimal.Parse(camLine[23], System.Globalization.NumberStyles.Float);

                    data.Add(newLog);

                    newLog.OrderAdded = OrderAdded;

                    OrderAdded = OrderAdded + 1; ;
                }
            }

            return data;

        }






        public static string ToCsv<T>(this IEnumerable<T> objectlist, string separator)
        {
            Type t = typeof(T);
            FieldInfo[] fields = t.GetFields(BindingFlags.Instance |
                       BindingFlags.Static |
                       BindingFlags.NonPublic |
                       BindingFlags.Public);

            string header = String.Join(separator, fields.Select(f => f.Name).ToArray());

            StringBuilder csvdata = new StringBuilder();
            csvdata.AppendLine(header);

            foreach (var o in objectlist)
                csvdata.AppendLine(ToCsvFields(separator, fields, o));

            return csvdata.ToString();
        }

        public static string ToCsvFields(string separator, FieldInfo[] fields, object o)
        {
            StringBuilder linie = new StringBuilder();

            foreach (var f in fields)
            {
                if (linie.Length > 0)
                    linie.Append(separator);

                var x = f.GetValue(o);

                if (x != null)
                    linie.Append(x.ToString());
            }

            return linie.ToString();
        }

        public static List<CameraLog> ParseCSV(string FileName)
        {
            List<CameraLog> data = new List<CameraLog>();

            string[] records = File.ReadAllLines(FileName);
            CameraLog newLog = new CameraLog();


            foreach (string item in records)
            {
                if (item.StartsWith("GPS"))
                {
                    string[] gpsLine = item.Split(new[] { ',' });

                    if (gpsLine[9] != null)
                    {
                        newLog.GPSAltitude = float.Parse(gpsLine[9]);
                    }
                }
                else if (item.StartsWith("CAM"))
                {
                    string[] camLine = item.Split(new[] { ',' });

                    newLog.GPSMilliseconds = int.Parse(camLine[1], System.Globalization.NumberStyles.Float);
                    newLog.GPSWeek = int.Parse(camLine[2], System.Globalization.NumberStyles.Float);
                    newLog.Latitude = decimal.Parse(camLine[3], System.Globalization.NumberStyles.Float);
                    newLog.Longitude = decimal.Parse(camLine[4], System.Globalization.NumberStyles.Float);
                    newLog.Pitch = decimal.Parse(camLine[6], System.Globalization.NumberStyles.Float);
                    newLog.Roll = decimal.Parse(camLine[7], System.Globalization.NumberStyles.Float);
                    newLog.Yaw = decimal.Parse(camLine[8], System.Globalization.NumberStyles.Float);

                    data.Add(newLog);

                    newLog = new CameraLog();
                }
            }

            return data;
        }

        public static void CreateTiff(Image R, Image G, Image B, Image A, String SavePath, string TiffFileName)
        {
            Image bmp = R;


            Bitmap bitmap = (Bitmap)bmp;

            //Select the image encoder
            System.Drawing.Imaging.Encoder enc = System.Drawing.Imaging.Encoder.SaveFlag;


            ImageCodecInfo info = null;
            info = (from ie in ImageCodecInfo.GetImageEncoders()
                    where ie.MimeType == "image/tiff"
                    select ie).FirstOrDefault();
            EncoderParameters encoderparams = new EncoderParameters(1);
            encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.MultiFrame);

            //Save the bitmap
            bitmap.Save(SavePath + TiffFileName + ".tiff", info, encoderparams);

            encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.FrameDimensionPage);

            //add another image
            bitmap.SaveAdd(G, encoderparams);
            bitmap.SaveAdd(B, encoderparams);
            bitmap.SaveAdd(A, encoderparams);


            //close file
            encoderparams.Param[0] = new EncoderParameter(enc, (long)EncoderValue.Flush);
            bitmap.SaveAdd(encoderparams);
        }
        public static IEnumerable<string> GetTLog(string Path)
        {
            //return Directory.GetFiles(Path, "*.JPG,*.tif", SearchOption.AllDirectories)
            //    .AsEnumerable();

            return Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.ToLower().EndsWith(".tlog"));
        }
        public static IEnumerable<string> GetFiles(string Path)
        {
            //return Directory.GetFiles(Path, "*.JPG,*.tif", SearchOption.AllDirectories)
            //    .AsEnumerable();

            return Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.ToLower().EndsWith(".jpg") || s.EndsWith(".tif") || s.ToLower().EndsWith(".tiff")
                || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".log") || s.ToLower().EndsWith(".tlog") || s.ToLower().EndsWith(".raw"));
        }

        public static IEnumerable<string> GetImages(string Path)
        {
            //return Directory.GetFiles(Path, "*.JPG,*.tif", SearchOption.AllDirectories)
            //    .AsEnumerable();

            return Directory.EnumerateFiles(Path, "*.*", SearchOption.TopDirectoryOnly)
            .Where(s => s.ToLower().EndsWith(".jpeg") || s.EndsWith(".jpg") || s.ToLower().EndsWith(".tiff")
                || s.ToLower().EndsWith(".png") || s.ToLower().EndsWith(".raw"));
        }


        //public static Image TranslateImage(Image img)
        //{

        //}

        public static Image ScaleImage(Image image, int maxWidth, int maxHeight)
        {
            var ratioX = (double)maxWidth / image.Width;
            var ratioY = (double)maxHeight / image.Height;
            var ratio = Math.Min(ratioX, ratioY);

            var newWidth = (int)(image.Width * ratio);
            var newHeight = (int)(image.Height * ratio);

            var newImage = new Bitmap(newWidth, newHeight);
            Graphics.FromImage(newImage).DrawImage(image, 0, 0, newWidth, newHeight);
            return newImage;
        }

        public static Image RotateImage(Image img, float rotationAngle)
        {
            //create an empty Bitmap image
            Bitmap bmp = new Bitmap(img.Width, img.Height);

            //turn the Bitmap into a Graphics object
            Graphics gfx = Graphics.FromImage(bmp);

            //now we set the rotation point to the center of our image
            gfx.TranslateTransform((float)bmp.Width / 2, (float)bmp.Height / 2);

            //now rotate the image
            gfx.RotateTransform(rotationAngle);

            gfx.TranslateTransform(-(float)bmp.Width / 2, -(float)bmp.Height / 2);

            //set the InterpolationMode to HighQualityBicubic so to ensure a high
            //quality image once it is transformed to the specified size
            gfx.InterpolationMode = InterpolationMode.HighQualityBicubic;

            //now draw our new image onto the graphics object
            gfx.DrawImage(img, new Point(0, 0));

            //dispose of our Graphics object
            gfx.Dispose();

            //return the image
            return bmp;
        }

        //speeds up garbage collection -- mWright
        private static Regex r = new Regex(":");

        //retrieves the datetime WITHOUT loading the whole image --mWright
        public static DateTime pullDateFromImage(string path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            using (Image myImage = Image.FromStream(fs, false, false))
            {
                PropertyItem propItem = myImage.GetPropertyItem(36867);
                string dateTaken = r.Replace(Encoding.UTF8.GetString(propItem.Value), "-", 2);
               return  DateTime.ParseExact(dateTaken, "yyyy-MM-dd HH:mm:ss\0",
                                       System.Globalization.CultureInfo.InvariantCulture);
            
            }
        }

        //finds epoch time of a picture when given a list of dateTime and converts to a list of epoch times --mWright
        public static List<double> convertToEpoch(List<DateTime> camDateTime)
        {
            List<double> camEpoch = new List<double>();
            foreach (DateTime item in camDateTime)
            {

                double epoch = (item - new DateTime(1970, 1, 1)).TotalSeconds;
                camEpoch.Add(epoch);
            }

            return camEpoch;
        }


        //compares two epoch times and returns the difference --mWright
        //there may be a problem with gps time zones
        //pictures are tagged in their time zone and calculation is done by the internal clock on your system
        // while mission planner uses ?GMT TIME? 
        // The epoch values will be off by a fixed number of time hours * 3600
        // hours = (Mission planner time zone (?GMT?)+- difference to your timezone)
        //each image is off by this offset so you can still determine the timing offset.
        public static double compareEpochTime(List<CameraLog> data, IEnumerable<double> epochCameraTime)
        {
            List<double> timeOffSetList = new List<double>();
            int count = 0;
            var enumerator = epochCameraTime.GetEnumerator();
            enumerator.MoveNext();
            double offSet = 0;
            double skipLogMessage = 0;
            double skipLogMessageMem = 0;
            bool offSetFound = false;
            Tuple<double, bool, double> tuple;
            while (offSetFound == false)
            {

                foreach (var item in data)
                {
                    //for (int i = 0; i < epochCamTime.Count; i++) { 

                    if (count < 4)
                    {
                        if (item.LogType == "CAM")
                        {
                            if (skipLogMessage == 0)
                            {

                                count++;
                                offSet = Math.Abs((item.Time / 1000000) - enumerator.Current);
                                timeOffSetList.Add(offSet);
                                enumerator.MoveNext();

                            }
                            if (skipLogMessage > 0)
                            {
                                skipLogMessage--;
                            }
                        }

                    }
                    else break;
                }
                count = 0;
                enumerator.Reset();
                enumerator.MoveNext();
                tuple = findOffSet(timeOffSetList, skipLogMessageMem);

                skipLogMessageMem = tuple.Item1;
                skipLogMessage = skipLogMessageMem;
                offSetFound = tuple.Item2;
                offSet = tuple.Item3;
                if (offSetFound == false)
                {
                    timeOffSetList.Clear();
                }
            }

            return offSet;


        }


        //gets datetime form each picture and makes a list of them. --mWright 
        public static Tuple<List<DateTime>, List<string>> getPictureDateTime(string filePath)
        {  
            List<DateTime> camDateTime = new List<DateTime>();
            List<string> pictureNames = new List<string>();
            foreach (string r in GetImages(filePath))
            {
                camDateTime.Add(pullDateFromImage(r));
                string lastPart = r.Split('/').Last();
                pictureNames.Add(lastPart);
            }

            return Tuple.Create(camDateTime, pictureNames);
        }

        // parses through list of offsets and finds the correct offset of pictures -mWright
        public static Tuple<double, bool, double> findOffSet(List<double> timeOffSet, double skipLogMessage)
        {
            double offSet = 0;
            foreach (var item in timeOffSet)
            {
                foreach (var item2 in timeOffSet)
                {
                    if ((item - 1) <= (item2) && (item + 1) >= item2)
                    {
                        offSet = item;
                    }
                    else
                    {
                        skipLogMessage++;
                        return Tuple.Create(skipLogMessage, false, offSet);
                    }
                }
            }
            return Tuple.Create(offSet, true, offSet);
        }

        //use offset with all cam logs to find beginning 
        public static List<CameraLog> findNearestGPS(List<CameraLog> data, double compareEpochTime, IEnumerable<double> epochCamTime)
        {
            List<CameraLog> finalCoords = new List<CameraLog>();
            var enumerator = epochCamTime.GetEnumerator();
            enumerator.MoveNext();
            double prevGPS = 0;
            decimal prevLong = 0;
            decimal prevLat = 0;

            foreach (var item in data)
            {
                if (item.LogType == "GPS")
                {
                    prevGPS = item.GPSAltitude;
                    prevLat = item.Latitude;
                    prevLong = item.Longitude;
                }
                if (item.LogType == "CAM")
                {

                    if (((item.Time / 1000000) - enumerator.Current) <= (compareEpochTime + 1) && ((item.Time / 1000000) - enumerator.Current) >= (compareEpochTime - 1))
                    {
                        CameraLog newlog = new CameraLog();
                        newlog.Latitude = prevLat;
                        newlog.Longitude = prevLong;
                        newlog.GPSAltitude = prevGPS;
                        finalCoords.Add(newlog);
                        enumerator.MoveNext();
                       
                       


                    }

                }
            }

           
            return finalCoords;
        }

        public static void writeToFile(List<CameraLog> finalCoords, List<String> pictureName, string ImageFileName)
        {
            IEnumerator<CameraLog> enum1 = finalCoords.GetEnumerator();
            IEnumerator<string> enum2 = pictureName.GetEnumerator();
            while ((enum1.MoveNext()) && (enum2.MoveNext()))
            {
                enum1.Current.ImageName = enum2.Current;
            }


            StringBuilder sb = new StringBuilder();

            foreach (var item in finalCoords)
            {
                item.GPSAltitude = item.GPSAltitude / 1000;
                sb.Append(string.Format("{0}\t{1}\t{2}\t{3}\t", item.ImageName, item.Longitude.ToString(), item.Latitude.ToString(), item.GPSAltitude.ToString()) + Environment.NewLine);


            }
            //changeimagefilename to imageppicture name
            using (StreamWriter outfile = new StreamWriter(ImageFileName + @"/ImageLog.txt"))
            {
                outfile.Write(sb.ToString());
            }
            IEnumerable<string> image = GetImages(ImageFileName);
            var enum3 = image.GetEnumerator();
            
            foreach (CameraLog item in finalCoords)
            {
                enum3.MoveNext();
              //  Helpers.ImageUtilityHelper.WriteCoordinatesToImage(enum3.Current,
                         //   double.Parse(item.Latitude.ToString()), double.Parse(item.Longitude.ToString()),
                          //  double.Parse(item.GPSAltitude.ToString()));

            }
        }

        //main function for epoch geo tagging --mWright
        public static void doEpoch(string imageFileName, List<CameraLog> data)
        {
            IEnumerable<double> epochCamTime;

            List<string> pictureNames = new List<string>();
            epochCamTime = convertToEpoch(getPictureDateTime(imageFileName).Item1);
            pictureNames = getPictureDateTime(imageFileName).Item2;
            writeToFile(findNearestGPS(data, compareEpochTime(data, epochCamTime), epochCamTime), pictureNames, imageFileName);



        }

        public static string tlogToCSV(string filepath){
            CurrentState.SpeedUnit = "m/s";
            CurrentState.DistanceUnit = "m";
            MAVLinkInterface proto = new MAVLinkInterface();

           OpenFileDialog openFileDialog1 = new OpenFileDialog();
            
           
              
               string LogFilePath;
              openFileDialog1.FileName = filepath;
                   
                    foreach (string logfile in openFileDialog1.FileNames)
                    {

                        using (MAVLinkInterface mine = new MAVLinkInterface())
                        {
                            try
                            {
                                mine.logplaybackfile = new BinaryReader(File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.Read));
                            }
                            catch (Exception ex) { log.Debug(ex.ToString()); }
                            mine.logreadmode = true;


                            mine.MAV.packets.Initialize(); // clear
                            
                            StreamWriter sw = new StreamWriter(Path.GetDirectoryName(logfile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(logfile) + ".csv");

                            while (mine.logplaybackfile.BaseStream.Position < mine.logplaybackfile.BaseStream.Length)
                            {

                                byte[] packet = mine.readPacket();
                                string text = "";
                                mine.DebugPacket(packet, ref text, true, ",");

                                sw.Write(mine.lastlogread.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "," + text);
                            }
                           
                            sw.Close();
                            
                           

                            mine.logreadmode = false;
                            mine.logplaybackfile.Close();
                            mine.logplaybackfile = null;
                            LogFilePath = (Path.GetDirectoryName(logfile) + Path.DirectorySeparatorChar + (Path.GetFileNameWithoutExtension(logfile) + ".csv"));
                       
                            return LogFilePath;
                        }
                    }



                    return null; 
            }

    }
}
