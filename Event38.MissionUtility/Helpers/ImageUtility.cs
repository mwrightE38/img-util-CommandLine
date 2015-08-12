using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using Event38.MissionUtility._Classes;


namespace Event38.MissionUtility.Helpers
{
    public static class ImageUtility
    {
        public static void WriteCoordinatesToImage(string Filename, double dLat, double dLong,double alt)
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
                    pi[0].Value = new Rational(alt).GetBytes();
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


        public static List<Models.CameraLog> ParseCSV(string FileName)
        {
            var data = File.ReadLines(FileName)
    .Where(s => s != "").Where(s=>s.StartsWith("CAM"))
    .Select(s => s.Split(new[] { ',' }))
    .Select(a => new Models.CameraLog
    {
        GPSMilliseconds = int.Parse( a[1]),
        GPSWeek = int.Parse(a[2]),
        Latitude = decimal.Parse(a[3]),
        Longitude = decimal.Parse(a[4]),
        GPSAltitude = decimal.Parse(a[5]),
        Pitch = decimal.Parse(a[6]),
        Roll = decimal.Parse(a[7]),
        Yaw = decimal.Parse(a[8])
    })
    .ToList();

            return data;
        }


        public static IEnumerable<string> GetFiles(string Path)
        {
            return Directory.GetFiles(Path, "*.JPG", SearchOption.AllDirectories)
                .AsEnumerable();
        }

    }
}
