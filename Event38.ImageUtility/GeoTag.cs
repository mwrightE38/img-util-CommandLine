using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Event38.ImageUtility
{
    public partial class GeoTag : Form
    {
        public GeoTag()
        {
            InitializeComponent();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            List<Models.CameraLog> log = Helpers.ImageUtilityHelper.ParseCSV(LogFilePath.Text.ToString());
            int count = 0;

            IEnumerable<string> Images = Helpers.ImageUtilityHelper.GetImages(ImagePath.Text.ToString());
            var enumerator = Images.GetEnumerator();

            int LogCount = log.Count();
            int ImageCount = Images.Count();
            DialogResult result = DialogResult.OK;

            if (LogCount > ImageCount)
            {

                result = MessageBox.Show("CAM Message count is greater then Image count" + Environment.NewLine
                      + "Images: " + ImageCount + "  CAM Message: " + LogCount,
                    "Log Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);

            }
            else if (ImageCount > LogCount)
            {
                result = MessageBox.Show("Image count is greater then CAM Message count" + Environment.NewLine
                      + "Images: " + ImageCount + "  CAM Message: " + LogCount,
                    "Log Message", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            }


            if (result.Equals(DialogResult.OK))
            {
                //Do something


                StatusUpdate.Text = "Status: 0 of " + log.Count.ToString();


                enumerator.MoveNext();
                foreach (var item in log)
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

                  //  Helpers.ImageUtilityHelper.WriteCoordinatesToImage(ImageName, double.Parse(item.Latitude.ToString()), double.Parse(item.Longitude.ToString()), double.Parse(item.GPSAltitude.ToString()));


                    string img = System.IO.Path.GetFileName(ImageName);
                    item.ImageName = img;


                  

                    enumerator.MoveNext();

                    count++;

                    StatusUpdate.Text = "Status: " + count + " of " + log.Count.ToString();


                }

                CreatingCsvFiles(log);

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
                sb.Append(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", item.ImageName, item.Longitude.ToString(), item.Latitude.ToString(), item.GPSAltitude.ToString(), item.Yaw.ToString(), item.Pitch.ToString(), item.Roll.ToString()) + Environment.NewLine);
            }

            using (StreamWriter outfile = new StreamWriter(ImagePath.Text.ToString() + @"\ImageLog.log"))
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

        private void StatusUpdate_Click(object sender, EventArgs e)
        {

        }
    }
}
