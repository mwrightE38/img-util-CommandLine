using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;


namespace Event38.MissionUtility
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
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
            
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePathTwo.Content = dialog.SelectedPath;
            }
        }

        private void MatchImages(string PathOne, string Pathtwo)
        {
            IEnumerable<string> ImagesOne = Helpers.ImageUtility.GetFiles(ImagePath.Content.ToString());
            IEnumerable<string> ImagesTwo = Helpers.ImageUtility.GetFiles(ImagePathTwo.Content.ToString());
            var enumerator = ImagesTwo.GetEnumerator();
            enumerator.MoveNext();

            foreach (var item in ImagesOne)
            {
                string one = item;
                string two = enumerator.Current;
                
                //CvInvoke.cvUndistort2

             
                //Match files and process them

               
                enumerator.MoveNext();

            }
        }
    }
}
