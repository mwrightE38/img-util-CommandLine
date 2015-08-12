using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Event38.Helpers;
using log4net;
using MissionPlanner;

namespace Event38.ImageUtility
{
    public partial class GeoTagV2 : Form
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public GeoTagV2()
        {
            InitializeComponent();
        }
        
     
        //private void convertToCSV_Click(object sender, EventArgs e)
        //{
            

        //    CurrentState.SpeedUnit = "m/s";
        //    CurrentState.DistanceUnit = "m";
        //    MAVLinkInterface proto = new MAVLinkInterface();
            
        //    using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
        //    {
        //        openFileDialog1.Filter = "*.tlog|*.tlog";
        //        openFileDialog1.FilterIndex = 2;
        //        openFileDialog1.RestoreDirectory = true;
        //        openFileDialog1.Multiselect = true;
        //        try
        //        {
        //            openFileDialog1.InitialDirectory = LogDir + Path.DirectorySeparatorChar;
        //        }
        //        catch { } // incase dir doesnt exist

        //        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        //        {
        //            foreach (string logfile in openFileDialog1.FileNames)
        //            {

        //                using (MAVLinkInterface mine = new MAVLinkInterface())
        //                {
        //                    try
        //                    {
        //                        mine.logplaybackfile = new BinaryReader(File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.Read));
        //                    }
        //                    catch (Exception ex) { log.Debug(ex.ToString()); CustomMessageBox.Show("Log Can not be opened. Are you still connected?"); return; }
        //                    mine.logreadmode = true;


        //                    mine.MAV.packets.Initialize(); // clear

        //                    StreamWriter sw = new StreamWriter(Path.GetDirectoryName(logfile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(logfile) + ".csv");

        //                    while (mine.logplaybackfile.BaseStream.Position < mine.logplaybackfile.BaseStream.Length)
        //                    {
        //                        int percent = (int)((float)mine.logplaybackfile.BaseStream.Position / (float)mine.logplaybackfile.BaseStream.Length * 100.0f);

        //                        if (progressBar1.Value != percent)
        //                        {
        //                            progressBar1.Value = percent;
        //                           progressBar1.Refresh();
        //                        }


        //                        byte[] packet = mine.readPacket();
        //                        string text = "";
        //                        mine.DebugPacket(packet, ref text, true, ",");

        //                        sw.Write(mine.lastlogread.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "," + text);
        //                    }

        //                    sw.Close();

        //                    progressBar1.Value = 100;

        //                    mine.logreadmode = false;
        //                    mine.logplaybackfile.Close();
        //                    mine.logplaybackfile = null;
        //                }
        //            }
        //        }
        //    }
        //}

        private void button3_Click_1(object sender, EventArgs e)
        {
            ProcessLogFile();
        }

        public static Hashtable config = new Hashtable();
        private void ProcessLogFile()
        {

            string fileExt = Path.GetExtension(LogFilePath.Text);
            List<Models.CameraLog> log = new List<Models.CameraLog>();

            if (fileExt == ".log")
            {
                log = Helpers.ImageUtilityHelper.ParseCSV2(LogFilePath.Text.ToString());
            }
            else if (fileExt.ToLower() == ".csv")
            {
                log = Helpers.ImageUtilityHelper.ParseCSVtLog(LogFilePath.Text.ToString());
            }
            //calls epoch function --mWright
            Helpers.ImageUtilityHelper.doEpoch(ImagePath.Text.ToString(), log);

            // "*.tlog|*.tlog"
            // List<Models.CameraLog> tLog = Helpers.ImageUtilityHelper.ParseCSV2(LogFilePath.Text.ToString());



            IEnumerable<Models.CameraLog> logtwo = log;// Helpers.ImageUtilityHelper.ParseCSV2(LogFilePath.Text.ToString());

            string csv = logtwo.ToCsv<Models.CameraLog>(",");

            //using (StreamWriter outfile = new StreamWriter(ImagePath.Text.ToString() + @"\ImageLog2.log"))
            //{
            //    outfile.Write(csv);
            //}

            int count = 0;

            IEnumerable<string> Images = Helpers.ImageUtilityHelper.GetImages(ImagePath.Text.ToString());
            var enumerator = Images.GetEnumerator();


            int LogCount = log.Where(c => c.LogType == "CAM").Count();
            int ImageCount = Images.Count();
            DialogResult result = DialogResult.OK;

            if (LogCount > ImageCount && (LogCount - ImageCount != 1))
            {
                result = MessageBox.Show("CAM Message count is greater then Image count tagged available images" + Environment.NewLine
                      + "Images: " + ImageCount + "  CAM Message: " + LogCount,
                    "Log Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            }
            else if (ImageCount > LogCount)
            {
                result = MessageBox.Show("Image count is greater then CAM Message count tagged available images" + Environment.NewLine
                      + "Images: " + ImageCount + "  CAM Message: " + LogCount,
                    "Log Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }


            if (result.Equals(DialogResult.OK))
            {

                StatusUpdate.Text = "Status: 0 of " + LogCount;

                int offset = 0;// int.Parse(CameraDelay.Text);

              


                enumerator.MoveNext();
                foreach (var item in log.OrderBy(c => c.OrderAdded))
                {
                    if (item.LogType == "CAM")//CAM
                    {
                        if (ImageCount < LogCount)
                        {
                            if (count == ImageCount)
                            {
                                break;
                            }
                        }

                        string ImageName = enumerator.Current;
                        if (ImageName == null)
                            break;

                        Models.CameraLog closestGPS = log.Where(c => c.LogType == "GPS").OrderBy(x => Math.Abs((item.GPSMilliseconds + 400) - x.GPSMilliseconds)).First();


                        var selectCurrentGPS = from l in log
                                               where l.LogType == "GPS" &&
                                                   l.GPSMilliseconds == item.GPSMilliseconds
                                               select l;

                        var selectGPS = (from l in log
                                         where l.LogType == "GPS"
                                         select new { l, distance = Math.Abs(l.GPSMilliseconds - (item.GPSMilliseconds + offset)) }).OrderBy(p => p.distance);

                        if (selectGPS.Count() > 0)
                        {
                            item.Latitude = selectGPS.First().l.Latitude;
                            item.Longitude = selectGPS.First().l.Longitude;
                            item.GPSAltitude = selectGPS.First().l.GPSAltitude / 1000;

                        }


                        //var selectATT = (from l in log
                        //                 where l.LogType == "ATT"
                        //                 select new { l, distance = Math.Abs(l.GPSMilliseconds - (item.GPSMilliseconds + offset)) }).OrderBy(p => p.distance);

                        //if (selectATT.Count() > 0)
                        //{
                        //    item.Roll = selectATT.First().l.Roll;
                        //    item.Pitch = selectATT.First().l.Pitch;
                        //    item.Yaw = selectATT.First().l.Yaw;
                        //}

                       //Helpers.ImageUtilityHelper.WriteCoordinatesToImage(ImageName,
                          //  double.Parse(item.Latitude.ToString()), double.Parse(item.Longitude.ToString()),
                        //   double.Parse(item.GPSAltitude.ToString()));


                        string img = System.IO.Path.GetFileName(ImageName);
                        item.ImageName = img;

                        enumerator.MoveNext();

                        count++;

                        StatusUpdate.Text = "Status: " + count + " of " + LogCount;
                    }



                }

                //CreatingCsvFiles(log.Where(l => l.LogType == "CAM").ToList());

                MessageBox.Show("Processing Complete!");
            }
            else
            {
            }
        }

        public void CreatingCsvFiles(List<Models.CameraLog> log)
        {
            StringBuilder sb = new StringBuilder();
            // sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6}", "ImageName", "Longitude", "Latitude","Altitude","Yaw","Pitch","Roll") + Environment.NewLine);

            foreach (var item in log)
            {
                //modified to not include plane attitude information -D Cironi 2015-04-23
               sb.Append(string.Format("{0}\t{1}\t{2}\t{3}\t", item.ImageName, item.Longitude.ToString(), item.Latitude.ToString(), item.GPSAltitude.ToString()) + Environment.NewLine); //item.Pitch.ToString(), item.Roll.ToString()) + );
            }

           using (StreamWriter outfile = new StreamWriter(ImagePath.Text.ToString() + @"\ImageLog.txt"))
           {
                outfile.Write(sb.ToString());
            }
        }

        private void btnImageFolder_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath.Text = dialog.SelectedPath;
            }
        }

        private void btnLogFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();


            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == DialogResult.OK)
            {
                // Open document 
                string filename = dlg.FileName;

                LogFilePath.Text = filename;
            }
        }

        private void ImagePath_Click(object sender, EventArgs e)
        {

        }

        private void LogFilePath_Click(object sender, EventArgs e)
        {

        }
        public static string LogDir
        {
            get
            {
                if (config["logdirectory"] == null)
                    return _logdir;
                return config["logdirectory"].ToString();
            }
            set
            {
                _logdir = value;
                config["logdirectory"] = value;
            }
        }
        static string _logdir = Path.GetDirectoryName(Application.ExecutablePath) + Path.DirectorySeparatorChar + @"logs";

        private void btnImageFolder_Click_1(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath.Text = dialog.SelectedPath;
            }
        }
        //add code here
        private void btnLogFile_Click_1(object sender, EventArgs e)
        {
            button3.Enabled = false; 
            CurrentState.SpeedUnit = "m/s";
            CurrentState.DistanceUnit = "m";
            MAVLinkInterface proto = new MAVLinkInterface();

            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                
                openFileDialog1.Filter = "*.tlog|*.tlog";
                openFileDialog1.FilterIndex = 2;
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Multiselect = true;
                try
                {
                    openFileDialog1.InitialDirectory = LogDir + Path.DirectorySeparatorChar;
                }
                catch { } // incase dir doesnt exist

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                   
                    foreach (string logfile in openFileDialog1.FileNames)
                    {

                        using (MAVLinkInterface mine = new MAVLinkInterface())
                        {
                            try
                            {
                                mine.logplaybackfile = new BinaryReader(File.Open(logfile, FileMode.Open, FileAccess.Read, FileShare.Read));
                            }
                            catch (Exception ex) { log.Debug(ex.ToString()); CustomMessageBox.Show("Log Can not be opened. Are you still connected?"); return; }
                            mine.logreadmode = true;


                            mine.MAV.packets.Initialize(); // clear
                            
                            StreamWriter sw = new StreamWriter(Path.GetDirectoryName(logfile) + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(logfile) + ".csv");

                            while (mine.logplaybackfile.BaseStream.Position < mine.logplaybackfile.BaseStream.Length)
                            {
                                int percent = (int)((float)mine.logplaybackfile.BaseStream.Position / (float)mine.logplaybackfile.BaseStream.Length * 100.0f);

                                if (progressBar1.Value != percent)
                                {
                                    progressBar1.Value = percent;
                                    progressBar1.Refresh();
                                }


                                byte[] packet = mine.readPacket();
                                string text = "";
                                mine.DebugPacket(packet, ref text, true, ",");

                                sw.Write(mine.lastlogread.ToString("yyyy-MM-ddTHH:mm:ss.fff") + "," + text);
                            }
                           
                            sw.Close();
                            
                            progressBar1.Value = 100;

                            mine.logreadmode = false;
                            mine.logplaybackfile.Close();
                            mine.logplaybackfile = null;
                            LogFilePath.Text = (Path.GetDirectoryName(logfile) + Path.DirectorySeparatorChar + (Path.GetFileNameWithoutExtension(logfile) + ".csv"));
                        
                        }
                    }
                }
               
             
                // = filename;
            }
            


            // Display OpenFileDialog by calling ShowDialog method 
            


            // Get the selected file name and display in a TextBox 
           
                // Open document 

            button3.Enabled = true;
        }

        private void GeoTagV2_Load(object sender, EventArgs e)
        {

        }

        private void progressBar1_Click(object sender, EventArgs e)
        {

        }

        private void LogFilePath_Click_1(object sender, EventArgs e)
        {

        }

        private void CameraDelay_TextChanged(object sender, EventArgs e)
        {

        }

    }
}
