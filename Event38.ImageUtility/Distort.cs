using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.CvEnum;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Drawing.Imaging;
using LibTIFF_NET;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using ImageMagick;


namespace Event38.ImageUtility
{
    public partial class Distort : Form
    {
        public static IntPtr tiff = IntPtr.Zero;
        Models.CameraProperties camPropRGB = new Models.CameraProperties("RGBCalibration.xml");
        Models.CameraProperties camPropNIR = new Models.CameraProperties("NIRCalibration.xml");


        public Distort()
        {
            InitializeComponent();
        }


        private void RotateImages(string Two)
        {
            bool existsNIR = System.IO.Directory.Exists(ImagePathTwo.Text + "//processed//rotate");

            if (!existsNIR)
                System.IO.Directory.CreateDirectory(ImagePathTwo.Text + "//processed//rotate");


            string undistortImageTwo = ImagePathTwo.Text + "//processed//rotate//" + Path.GetFileName(Two);

            using (MagickImage image = new MagickImage(Two))
            {
                double Rotation = double.Parse(System.Configuration.ConfigurationManager.AppSettings["Rotation"].ToString());

                image.Rotate(Rotation);

                image.Write(undistortImageTwo);
            }
        }

        private void ImageTransForm(string Two)
        {
            bool existsNIR = System.IO.Directory.Exists(ImagePathTwo.Text + "//processed//translate");

            if (!existsNIR)
                System.IO.Directory.CreateDirectory(ImagePathTwo.Text + "//processed//translate");


            string undistortImageTwo = ImagePathTwo.Text + "//processed//translate//" + Path.GetFileName(Two);


            System.Drawing.Bitmap original = null; // Original image.
            System.Drawing.Bitmap transformed = null; // Transformed image.
            System.Drawing.Graphics graphics = null; // Drawing context.

            // Generate original image.
            original = new System.Drawing.Bitmap(Two);


            // Generate transformed images.
            transformed = new System.Drawing.Bitmap(Two);
            graphics = System.Drawing.Graphics.FromImage(transformed);
            graphics.Clear(System.Drawing.Color.Black);


            float x = float.Parse(System.Configuration.ConfigurationManager.AppSettings["x"].ToString());
            float y = float.Parse(System.Configuration.ConfigurationManager.AppSettings["y"].ToString());

            graphics.TranslateTransform(x, y);
            graphics.DrawImage(original, 0, 0);

            transformed.Save(undistortImageTwo, System.Drawing.Imaging.ImageFormat.Jpeg);
            graphics.Dispose();
            transformed.Dispose();

            original.Dispose();
        }


        private void UndistortImages(string One, string Two, int ImageNumber)
        {
            Image<Bgr, Byte> imgOne = new Image<Bgr, byte>(One);
            Image<Bgr, Byte> imgTwo = new Image<Bgr, byte>(Two);

            Image<Bgr, Byte> undistort = imgOne.CopyBlank();
            Image<Bgr, Byte> undistort2 = imgTwo.CopyBlank();


            CvInvoke.cvUndistort2(imgOne.Ptr, undistort.Ptr, camPropRGB.Intrinsic.Ptr, camPropRGB.Distortion.Ptr, IntPtr.Zero);
            CvInvoke.cvUndistort2(imgTwo.Ptr, undistort2.Ptr, camPropNIR.Intrinsic.Ptr, camPropNIR.Distortion.Ptr, IntPtr.Zero);


            bool existsRGB = System.IO.Directory.Exists(ImagePathOne.Text + "//processed//undistorted");

            if (!existsRGB)
                System.IO.Directory.CreateDirectory(ImagePathOne.Text + "//processed//undistorted");

            bool existsNIR = System.IO.Directory.Exists(ImagePathTwo.Text + "//processed//undistorted");

            if (!existsNIR)
                System.IO.Directory.CreateDirectory(ImagePathTwo.Text + "//processed//undistorted");


            string undistortImageOne = ImagePathOne.Text + "//processed//undistorted//" + Path.GetFileName(One);
            string undistortImageTwo = ImagePathTwo.Text + "//processed//undistorted//" + Path.GetFileName(Two);

            undistort.Save(undistortImageOne);
            undistort2.Save(undistortImageTwo);

            undistort.Dispose();
            undistort2.Dispose();


        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            IEnumerable<string> ImagesOne = Helpers.ImageUtilityHelper.GetFiles(ImagePathOne.Text.ToString());
            IEnumerable<string> ImagesTwo = Helpers.ImageUtilityHelper.GetFiles(ImagePathTwo.Text.ToString());
            var enumerator = ImagesTwo.GetEnumerator();
            enumerator.MoveNext();

            int count = 1;

            foreach (var item in ImagesOne)
            {
                string one = item;
                string two = enumerator.Current;


                UndistortImages(one, two, count);


                string imgOneUnDistortPath = ImagePathOne.Text + "//processed//undistorted//" + Path.GetFileName(one);
                string imgTwoUnDistortPath = ImagePathTwo.Text + "//processed//undistorted//" + Path.GetFileName(two);


                RotateImages(imgTwoUnDistortPath);


                string imgTwoRotatePath = ImagePathTwo.Text + "//processed//rotate//" + Path.GetFileName(two);


                ImageTransForm(imgTwoRotatePath);

                string imgTwotranslatePath = ImagePathTwo.Text + "//processed//translate//" + Path.GetFileName(two);

                bool existsCHRGB = System.IO.Directory.Exists(ImagePathOne.Text + "//processed//channels");

                if (!existsCHRGB)
                    System.IO.Directory.CreateDirectory(ImagePathOne.Text + "//processed//channels");


                using (MagickImage image = new MagickImage(imgOneUnDistortPath))
                {
                    int i = 0;
                    foreach (MagickImage channel in image.Separate(Channels.RGB))
                    {
                        channel.Write(ImagePathOne.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(imgOneUnDistortPath) + "Ch_" + i + ".jpg");
                        i++;
                    }
                }

                bool existsCHNIR = System.IO.Directory.Exists(ImagePathTwo.Text + "//processed//channels");

                if (!existsCHNIR)
                    System.IO.Directory.CreateDirectory(ImagePathTwo.Text + "//processed//channels");


                using (MagickImage image = new MagickImage(imgTwotranslatePath))
                {
                    int i = 0;
                    foreach (MagickImage channel in image.Separate(Channels.All))
                    {
                        channel.Write(ImagePathTwo.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(imgTwotranslatePath) + "NIRCh_" + i + ".jpg");
                        i++;
                    }
                }

                Image R = Image.FromFile(ImagePathOne.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(one) + "Ch_0.jpg");
                Image G = Image.FromFile(ImagePathOne.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(one) + "Ch_1.jpg");
                Image B = Image.FromFile(ImagePathOne.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(one) + "Ch_2.jpg");
                Image A = Image.FromFile(ImagePathTwo.Text + "//Processed//channels//" + Path.GetFileNameWithoutExtension(two) + "NIRCh_0.jpg");


                bool exists = System.IO.Directory.Exists(ImagePathOne.Text + "//processed//geoTiff");

                if (!exists)
                    System.IO.Directory.CreateDirectory(ImagePathOne.Text + "//processed//geoTiff");


                Event38.Helpers.ImageUtilityHelper.CreateTiff(R, G, B, A, ImagePathOne.Text + "//Processed//geoTiff//", Path.GetFileNameWithoutExtension(one));


                string TiffFileName = ImagePathOne.Text + "//Processed//geoTiff//" + Path.GetFileNameWithoutExtension(one) + ".tiff";


                bool existsFinal = System.IO.Directory.Exists(ImagePathOne.Text + "//processed//final");

                if (!existsFinal)
                    System.IO.Directory.CreateDirectory(ImagePathOne.Text + "//processed//final");


                Save8(TiffFileName, ImagePathOne.Text + "//Processed//final//", 1, 0, DateTime.Now.ToShortDateString(), "", "", "");

                enumerator.MoveNext();

                count++;
            }


            MessageBox.Show("Processing Complete!");

        }



        private unsafe void SortChannels(Image im, int[] order, int[] wavelength)
        {
            PropertyItem[] propertyItems = null;
            string[] strArray = null;
            char[] separator = null;
            ASCIIEncoding encoding = null;
            string str = null;
            FrameDimension dimension = null;
            //int num7 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
            int frameCount = 0;
            dimension = new FrameDimension(im.FrameDimensionsList[0]);
            frameCount = im.GetFrameCount(dimension);
            int frameIndex = 0;
            goto Label_0053;
        Label_004F:
            frameIndex++;
        Label_0053:
            if (frameIndex < frameCount)
            {
                im.SelectActiveFrame(dimension, frameIndex);
                //try
                //{
                order[frameIndex] = frameIndex;
                propertyItems = im.PropertyItems;
                str = " ";
                int num2 = 0;
                goto Label_007F;
            Label_007B:
                num2++;
            Label_007F:
                if (num2 < propertyItems.Length)
                {
                    encoding = new ASCIIEncoding();
                    if (propertyItems[num2].Id == 800)
                    {
                        str = encoding.GetString(propertyItems[num2].Value, 0, propertyItems[num2].Len - 1);
                        separator = new char[] { '-' };
                        strArray = str.Split(separator);
                        wavelength[frameIndex] = Convert.ToInt32(strArray[0].Substring(strArray[0].Length - 3, 3));
                        IDisposable disposable = encoding as IDisposable;
                        if (disposable != null)
                        {
                            disposable.Dispose();
                        }
                    }
                    goto Label_007B;
                }
                //}
                //catch when (?)
                //{
                uint num4 = 0;
                //int num9 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num7);
                //    try
                //    {
                //        try
                //        {
                goto Label_004F;
                //        }
                //        catch when (?)
                //        {
                //        }
                if (num4 != 0)
                {
                    throw new Exception();
                }
                //    }
                //    finally
                //    {
                //        __CxxUnregisterExceptionObject((void*) num7, (int) num4);
                //    }
                // }
                goto Label_004F;
            }

            int num6 = 0;

        Label_0175:
            if (num6 >= frameCount)
            {
                return;
            }
            int index = frameCount - 2;
            while (true)
            {
                if (index <= -1)
                {
                    num6++;
                    goto Label_0175;
                }
                if (wavelength[index + 1] < wavelength[index])
                {
                    Array.Reverse(order, index, 2);
                    Array.Reverse(wavelength, index, 2);
                }
                index--;
            }
        }

        private unsafe void Save8(string filename, string newpath, int length, int pos, string date, string serial, string gps, string exp)
        {
            try
            {
                Bitmap bitmap = null;
                IntPtr zero;
                Image im = null;
                byte[] source = null;
                string val = null;
                IntPtr ptr2;
                BitmapData bitmapdata = null;
                byte[, ,] buffer2 = null;
                Graphics graphics = null;
                int[] wavelength = null;
                int[] order = null;
                FrameDimension dimension = null;
                string str2 = null;
                // int num13 = (int) stackalloc byte[(__CxxQueryExceptionSize() * 1)];
                im = Image.FromFile(filename);
                int frameCount = 0;
                dimension = new FrameDimension(im.FrameDimensionsList[0]);
                frameCount = im.GetFrameCount(dimension);
                order = new int[frameCount];
                wavelength = new int[frameCount];
                this.SortChannels(im, order, wavelength);
                val = "descTetracam MCA:";
                for (int i = 0; i < frameCount; i++)
                {
                    val = val + " " + wavelength[i].ToString();
                }
                val.Trim();
                int width = im.Width;
                int height = im.Height;
                buffer2 = new byte[frameCount, width, height];
                int index = 0;
                while (true)
                {
                    if (index >= frameCount)
                    {
                        if (im != null)
                        {
                            im.Dispose();
                            im = null;
                        }
                        //this.progressBar1.Value++;
                        //this.progressBar1.Update();
                        filename = Path.GetFileNameWithoutExtension(filename);
                        str2 = newpath + @"\" + filename + "_8.TIF";
                        if (tiff != IntPtr.Zero)
                        {
                            LibTIFF.Close(tiff);
                            tiff = IntPtr.Zero;
                        }
                        zero = LibTIFF.Open(str2, "w");
                        ptr2 = new IntPtr();
                        short num15 = (short)height;
                        //try
                        //{
                        string[] strArray = new string[] { "Writing to 8bit: ", filename, "_8.TIF ", (pos + 1).ToString(), " of ", length.ToString() };
                        

                        LibTIFF.SetField(zero, 0x13c, exp);
                        LibTIFF.SetField(zero, 0x100, (short)width);
                        LibTIFF.SetField(zero, 0x101, (short)height);
                        LibTIFF.SetField(zero, 0x102, 8);
                        LibTIFF.SetField(zero, 0x115, (short)frameCount);
                        LibTIFF.SetField(zero, 0x116, num15);
                        LibTIFF.SetField(zero, 0x106, (short)1);
                        LibTIFF.SetField(zero, 0x11c, (short)1);
                        LibTIFF.SetField(zero, 0x132, date);
                        LibTIFF.SetField(zero, 270, val);
                        LibTIFF.SetField(zero, 0xc62f, serial);



                        //if (this.compress.Checked)
                        //{
                        //    LibTIFF.SetField(zero, 0x103, 0x80b2);
                        //}


                        LibTIFF.SetField(zero, 0x13b, gps);
                        ptr2 = LibTIFF._malloc(width * frameCount);
                        source = new byte[width * frameCount];
                        int num5 = 0;
                        goto Label_0456;
                    Label_0450:
                        num5++;
                    Label_0456:
                        if (num5 >= height)
                        {
                            goto Label_04B8;
                        }
                        int num4 = 0;
                        goto Label_0467;
                    Label_0461:
                        num4 += frameCount;
                    Label_0467:
                        if (num4 >= (width * frameCount))
                        {
                            goto Label_049E;
                        }
                        int num3 = 0;
                        goto Label_047A;
                    Label_0474:
                        num3++;
                    Label_047A:
                        if (num3 >= frameCount)
                        {
                            goto Label_0461;
                        }
                        int num14 = num4 / frameCount;
                        source[num4 + num3] = buffer2[num3, num14, num5];
                        goto Label_0474;
                    Label_049E:
                        Marshal.Copy(source, 0, ptr2, source.Length);
                        LibTIFF.WriteScanline(zero, ptr2, (uint)num5);
                        goto Label_0450;
                    Label_04B8:
                        source = null;
                        buffer2 = null;
                        //}
                        //catch when (?)
                        //{
                        //    uint num7 = 0;
                        // int num16 = __CxxRegisterExceptionObject((void*) Marshal.GetExceptionPointers(), (void*) num13);
                        //try
                        //{
                        //    try
                        //    {
                        //this.label1.Text = "Error writing" + filename + "_8.TIF ";
                        //this.label1.Update();
                        //break;
                        //}
                        //catch when (?)
                        //{
                        //}
                        //if (num7 != 0)
                        //{
                        //    throw;
                        //}
                        //}
                        //finally
                        //{
                        //__CxxUnregisterExceptionObject((void*) num13, (int) num7);
                        //}
                        //}
                        break;
                    }
                    if (bitmap != null)
                    {
                        bitmap.Dispose();
                    }
                    im.SelectActiveFrame(dimension, order[index]);
                    bitmap = new Bitmap(im.Width, im.Height, PixelFormat.Format24bppRgb);
                    graphics = Graphics.FromImage(bitmap);
                    graphics.DrawImage(im, 0, 0, im.Width, im.Height);
                    graphics.Dispose();
                    this.label1.Text = Path.GetFileName(filename) + " reading page " + index.ToString();
                    this.label1.Update();
                    //if (this.enDisplay.Checked)
                    //{
                    //    if (this.screen != null)
                    //    {
                    //        this.screen.Dispose();
                    //    }
                    //    Rectangle rectangle2 = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    //    this.screen = bitmap.Clone(rectangle2, bitmap.PixelFormat);
                    //    this.pB1.Image = this.screen;
                    //    this.pB1.Invalidate();
                    //    this.pB1.Update();
                    //}
                    int num12 = 3;
                    Rectangle rect = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    bitmapdata = bitmap.LockBits(rect, ImageLockMode.ReadWrite, bitmap.PixelFormat);
                    byte* numPtr = (byte*)bitmapdata.Scan0;
                    uint num9 = 0;
                    while (true)
                    {
                        if (num9 >= bitmap.Height)
                        {
                            bitmap.UnlockBits(bitmapdata);
                            if (bitmap != null)
                            {
                                bitmap.Dispose();
                                bitmap = null;
                            }
                            break;
                        }
                        for (uint j = 0; j < bitmap.Width; j++)
                        {
                            buffer2[index, j, num9] = numPtr[0];
                            numPtr += num12;
                        }
                        numPtr += bitmapdata.Stride - (bitmap.Width * num12);
                        num9++;
                    }
                    index++;
                }
                LibTIFF._free(ptr2);
                LibTIFF.Close(zero);
                zero = IntPtr.Zero;
                Application.DoEvents();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private Bitmap rotateImage90(Bitmap b)
        {
            Bitmap returnBitmap = new Bitmap(b.Height, b.Width);
            Graphics g = Graphics.FromImage(returnBitmap);
            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
            g.RotateTransform(90);
            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
            g.DrawImage(b, new Point(0, 0));
            return returnBitmap;
        }

        private void btnImageOne_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePathOne.Text = dialog.SelectedPath;
            }
        }

        private void btnImageTwo_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePathTwo.Text = dialog.SelectedPath;
            }
        }
    }
}
