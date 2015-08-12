using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event38.ImageUtility._Classes
{
    public class ImageUpload : INotifyPropertyChanged
    {
        public String ImageName { get; set; }

        public string ImagePath { get; set; }

        public string Size { get; set; }


        private string status;
        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                if (this.status != value)
                {
                    this.status = value;
                    this.OnPropretyChanged("Status");
                }
            }
        }


        protected virtual void OnPropretyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
