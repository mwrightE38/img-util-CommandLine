using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Event38.ImageUtility.Models
{
    public class CameraLog
    {
        //TYPES CAM = 1, ATT = 2, GPS = 3

        //     TimeMS,Roll,Pitch,Yaw,ErrorRP,ErrorYaw
        //ATT, 199621, -0.45, -2.00, 227.92, 0.07, 0.00

        //     Status,TimeMS,Week,NSats,HDop,Lat,Lng,RelAlt,Alt,Spd,GCrs,VZ,T
        //GPS, 3, 576146200, 1828, 8, 1.65, 41.0361204, -81.5315305, 115.42, 406.58, 7.33, 177.23, 0.330000, 277634

        //      GPSTime,  GPSWeek,   Lat,          Lng,      Alt,    RelAlt,  Roll,Pitch, Yaw
        //CAM, 576146000, 1828,    41.0361205, -81.5315307, 409.17, 115.36, -4.67, 0.34, 211.32


        //CAM, GPS Milliseconds, GPS Week, latitude, longitude, GPS altitude, Pitch, Roll, Yaw

        //CAM

        //ATT

        //GPS


        //Shared Fields
            // - Time (ms), Lat, Lng, Alt, RelAlt, Roll, Pitch Yaw


        public int OrderAdded { get; set; }

        public string LogType { get; set; }

        public string ImageName { get; set; }

        public long GPSMilliseconds { get; set; }

        public int GPSWeek { get; set; }

        public decimal Latitude { get; set; }

        public decimal Longitude { get; set; }

        public double GPSAltitude { get; set; }

        public decimal Pitch { get; set; }

        public decimal Roll { get; set; }

        public decimal Yaw { get; set; }

        public double Time { get; set; }


    }
}
