using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Event38.ImageUtility
{
    public partial class PropertiesForm : Form
    {
        public PropertiesForm()
        {
            InitializeComponent();

            UserName.Text = Properties.Settings.Default.UserName;
            Password.Text = Properties.Settings.Default.Password;

        }

        private void Save_Click(object sender, EventArgs e)
        {
          

            // Write settings
            Properties.Settings.Default.UserName = UserName.Text;
            Properties.Settings.Default.Password = Password.Text;

            // Save settings
            Properties.Settings.Default.Save();

            this.Close();
        }
    }
}
