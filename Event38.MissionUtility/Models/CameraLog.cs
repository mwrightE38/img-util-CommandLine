using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event38.MissionUtility.Models
{
    public class CameraLog
    {
        //CAM, GPS Milliseconds, GPS Week, latitude, longitude, GPS altitude, Pitch, Roll, Yaw

        public string ImageName { get; set; }
        public int GPSMilliseconds { get; set; }

        public int GPSWeek { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public decimal GPSAltitude { get; set; }

        public decimal Pitch { get; set; }

        public decimal Roll { get; set; }

        public decimal Yaw { get; set; }


    }
}
