using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Event38.ImageUtility.Models
{
    public class CameraProperties
    {
        public double fx { get; set; }
        public double cx { get; set; }
        public double fy { get; set; }
        public double cy { get; set; }
        public double k1 { get; set; }
        public double k2 { get; set; }
        public double k3 { get; set; }
        public double p1 { get; set; }
        public double p2 { get; set; }

        public Matrix<double> Intrinsic { get; set; }

        public Matrix<double> Distortion { get; set; }

        public CameraProperties(string XMLFilePath)
        {

            Intrinsic = new Matrix<double>(3, 3);
            Distortion = new Matrix<double>(4, 1);

            var doc = XDocument.Load(XMLFilePath);
            var calibrations =
                doc.Descendants("calibration").Select(calibrationElement =>
                    new
                    {
                        fx = calibrationElement.Descendants("fx").Single().Value,
                        cx = calibrationElement.Descendants("cx").Single().Value,
                        fy = calibrationElement.Descendants("fy").Single().Value,
                        cy = calibrationElement.Descendants("cy").Single().Value,
                        k1 = calibrationElement.Descendants("k1").Single().Value,
                        k2 = calibrationElement.Descendants("k2").Single().Value,
                        k3 = calibrationElement.Descendants("k3").Single().Value,
                        p1 = calibrationElement.Descendants("p1").Single().Value,
                        p2 = calibrationElement.Descendants("p2").Single().Value
                    });


            foreach (var item in calibrations)
            {
                fx = double.Parse(item.fx);
                cx = double.Parse(item.cx);
                fy = double.Parse(item.fy);
                cy = double.Parse(item.cy);
                k1 = double.Parse(item.k1);
                k2 = double.Parse(item.k2);
                k3 = double.Parse(item.k3);
                p1 = double.Parse(item.p1);
                p2 = double.Parse(item.p2);
            }



            Intrinsic[0, 0] = fx;
            Intrinsic[0, 1] = 0;
            Intrinsic[0, 2] = cx;

            Intrinsic[1, 0] = 0;
            Intrinsic[1, 1] = fy;
            Intrinsic[1, 2] = cy;

            Intrinsic[2, 0] = 0;
            Intrinsic[2, 1] = 0;
            Intrinsic[2, 2] = 1;


            Distortion[0, 0] = k1;
            Distortion[1, 0] = k2;
            Distortion[2, 0] = p1;
            Distortion[3, 0] = p2;


        }


        //double fx = Double.Parse("3.3323488178948746E+003", System.Globalization.NumberStyles.Float);
        //double cx = Double.Parse("1.9715688477918509E+003", System.Globalization.NumberStyles.Float);
        //double fy = Double.Parse("3.3354097677052719E+003", System.Globalization.NumberStyles.Float);
        //double cy = Double.Parse("1.3121688712062946E+003", System.Globalization.NumberStyles.Float);

        //double k1 = Double.Parse("-8.1764475728019329E-002", System.Globalization.NumberStyles.Float);
        //double k2 = Double.Parse("4.7254586533735238E-002", System.Globalization.NumberStyles.Float);
        //double k3 = Double.Parse("6.1624773813921244E-002", System.Globalization.NumberStyles.Float);
        //double p1 = Double.Parse("-1.4374161464224780E-003", System.Globalization.NumberStyles.Float);
        //double p2 = Double.Parse("-1.9010907289352781E-003", System.Globalization.NumberStyles.Float);



    }
}
