using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event38.ImageUtility.Models
{

    public class ImageAPIUpload
    {
        public string type { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string filename { get; set; }

        public string FileBase64 { get; set; }
        public long FileSize { get; set; }
    }
}
