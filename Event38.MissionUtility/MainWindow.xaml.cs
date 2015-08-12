using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Event38.MissionUtility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
         
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath.Content = dialog.SelectedPath;
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;

                LogFilePath.Content = filename;

            }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            List<Models.CameraLog> log = Helpers.ImageUtility.ParseCSV(LogFilePath.Content.ToString());
            int count = 0;

            IEnumerable<string> Images = Helpers.ImageUtility.GetFiles(ImagePath.Content.ToString());
            var enumerator = Images.GetEnumerator();

            enumerator.MoveNext();
            foreach (var item in log)
            {
                string ImageName = enumerator.Current;

                Helpers.ImageUtility.WriteCoordinatesToImage(ImageName, double.Parse(item.Latitude.ToString()), double.Parse(item.Longitude.ToString()),double.Parse(item.GPSAltitude.ToString()));


                string img = System.IO.Path.GetFileName(ImageName);
                item.ImageName = img;

                enumerator.MoveNext();
            }

            CreatingCsvFiles(log);

            MessageBox.Show("Processing Complete!");
        }

        public void CreatingCsvFiles(List<Models.CameraLog> log)
        {
            StringBuilder sb = new StringBuilder();
           // sb.Append(string.Format("{0},{1},{2},{3},{4},{5},{6}", "ImageName", "Longitude", "Latitude","Altitude","Yaw","Pitch","Roll") + Environment.NewLine);
           
            
            foreach (var item in log)
            {
                sb.Append(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", item.ImageName, item.Longitude.ToString(), item.Latitude.ToString(), item.GPSAltitude.ToString(), item.Yaw.ToString(), item.Pitch.ToString(), item.Roll.ToString()) + Environment.NewLine);
            }

            using (StreamWriter outfile = new StreamWriter(ImagePath.Content.ToString() + @"\ImageLog.log"))
            {
                outfile.Write(sb.ToString());
            }
        }



    }
}
