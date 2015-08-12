using Amazon.S3;
using Amazon;
using Amazon.S3.Model;
using Event38.ImageUtility._Classes;
using Event38.ImageUtility.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Collections.Specialized;
using System.Web;
using Amazon.S3.Transfer;
using System.Linq.Expressions;
using Event38.Helpers;
using Amazon.Runtime;

namespace Event38.ImageUtility
{
    public partial class UploadFiles : Form
    {

        BindingList<ImageUpload> imgClass = new BindingList<ImageUpload>();


        public UploadFiles()
        {
            InitializeComponent();

        }


        public DrupalBridge db = null;
        static string existingBucketName = "e38rawstore";
        AmazonS3Config S3Config = new AmazonS3Config
        {
            ServiceURL = "s3-website-us-west-2.amazonaws.com"
        };

        public IAmazonS3 GetS3Client()
        {
            Amazon.Util.ProfileManager.RegisterProfile("default", "AKIAIZADOOW5W2SI42IQ", "bFal757yYJFZgRorNZBVpjstZvjp/cLvBIoYGS4K");

            AWSCredentials credentials = new StoredProfileAWSCredentials("default");
            IAmazonS3 s3Client = new AmazonS3Client(credentials, RegionEndpoint.USWest2);
           
            return s3Client;
        }

        public void LoopAndUploadFiles()
        {
            Amazon.Util.ProfileManager.RegisterProfile("default", "AKIAIZADOOW5W2SI42IQ", "bFal757yYJFZgRorNZBVpjstZvjp/cLvBIoYGS4K");


            List<string> Images = Helpers.ImageUtilityHelper.GetFiles(ImagePath.Text.ToString()).ToList();

            string FolderName = Guid.NewGuid().ToString() + "/Inbound";

            int Queued = Images.Count();
            int Processed = 0;


            foreach (var item in imgClass)
            {
                string FullFolderName = FolderName + "/" + Path.GetFileName(item.ImagePath);


                richTextBox1.Text += "Upload Started for " + Path.GetFileName(item.ImagePath) + Environment.NewLine;
                richTextBox1.Refresh();

                UploadMultiPartFile(item.ImagePath, FullFolderName);


                richTextBox1.Text += "Finished Upload for " + Path.GetFileName(item.ImagePath) + Environment.NewLine;
                richTextBox1.Refresh();


                Queued--;
                Processed++;

                lblTotalQueued.Text = Queued.ToString();
                lblTotalQueued.Refresh();

                lblTotalProcessed.Text = Processed.ToString();
                lblTotalProcessed.Refresh();


                item.Status = "Uploaded";
                dataGridView1.Refresh();
            }



            if (db != null)
            {
                db.logout();
                // Remove the event listener
                db.NewDrupalViewData -= handleNewViewData;
                db = null;
            }


            string UserName = Properties.Settings.Default.UserName;
            string Password = Properties.Settings.Default.Password;

            db = new DrupalBridge("http://ec2-54-244-149-251.us-west-2.compute.amazonaws.com/", "api/products", UserName, Password);
            db.NewDrupalViewData += handleNewViewData;
            db.login();

           
            List<string> Images2 = Helpers.ImageUtilityHelper.GetFiles(ImagePath.Text.ToString()).ToList();
            string li = "<li>{0}</li>";
            string ul = "<ul>{0}</ul>";
            string a = "<a href='{0}'>{1}</a>";


            string body = "";
            foreach (var item in Images2)
            {
                string fileName = Path.GetFileName(item);
                string baseURL = "http://e38rawstore.s3-website-us-west-2.amazonaws.com/raw/" + FolderName + fileName;

                string content = string.Format(li, string.Format(a, baseURL, fileName));

                body = string.Concat(body, content);
            }

            db.CreateNode("Mission-Data-" + FolderName.Replace("/Inbound", ""), string.Format(ul, body), "", true);


            richTextBox1.Text += "Complete!";
            richTextBox1.Refresh();

            //MessageBox.Show(FolderName);


        }

        void handleNewViewData(object sender, drupalEventArgs args)
        {
            System.Diagnostics.Debug.WriteLine("handleNewViewData: ");
            // Create an XmlReader
            using (XmlReader reader = XmlReader.Create(new StringReader(args._theMessage)))
            {
                try
                {
                    //MessageBox.Show(args._theMessage);

                    switch (args._responseType)
                    {
                        case DrupalSettings.NODE_RETRIEVE:

                            System.Diagnostics.Debug.WriteLine("Node Data: " + args._theMessage);
                            break;

                        case DrupalSettings.VIEW_RETRIEVE:
                            System.Diagnostics.Debug.WriteLine("View Data: " + args._theMessage);
                            break;
                    }
                }
                catch (XmlException e)
                {
                    System.Diagnostics.Debug.WriteLine("Exception parsing drupal result xml: {0}", e.ToString());
                }
            }

        }

        public void UploadMultiPartFile(string filePath, string FolderName)
        {
            // MessageBox.Show(FolderName);


            IAmazonS3 s3Client = GetS3Client();

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

            // 1. Initialize.
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = existingBucketName,
                Key = "raw/" + FolderName
            };


            initiateRequest.CannedACL = S3CannedACL.PublicRead;

            InitiateMultipartUploadResponse initResponse =
                s3Client.InitiateMultipartUpload(initiateRequest);

            // 2. Upload Parts.
            long contentLength = new FileInfo(filePath).Length;
            long partSize = 5 * (long)Math.Pow(2, 20); // 5 MB

            try
            {
                long filePosition = 0;
                for (int i = 1; filePosition < contentLength; i++)
                {
                    UploadPartRequest uploadRequest = new UploadPartRequest
                    {
                        BucketName = existingBucketName,
                        Key = "raw/" + FolderName,
                        UploadId = initResponse.UploadId,
                        PartNumber = i,
                        PartSize = partSize,
                        FilePosition = filePosition,
                        FilePath = filePath
                    };


                    //Image image = Image.FromFile(filePath);
                    //MemoryStream stream = new MemoryStream();

                    //// Save image to stream.
                    //image.Save(stream, ImageFormat.Bmp);

                    //uploadRequest.InputStream = stream;

                    // Upload part and add response to our list.
                    uploadResponses.Add(s3Client.UploadPart(uploadRequest));

                    filePosition += partSize;

                    richTextBox1.Text += Path.GetFileName(filePath) + " - " + (filePosition * .001) + "kb / " + (contentLength * .001) + "kb" + Environment.NewLine;
                    richTextBox1.Refresh();

                    //stream.Dispose();

                }

                // Step 3: complete.
                CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
                {
                    BucketName = existingBucketName,
                    Key = "raw/" + FolderName,
                    UploadId = initResponse.UploadId,
                    //PartETags = new List<PartETag>(uploadResponses)

                };
                completeRequest.AddPartETags(uploadResponses);

                CompleteMultipartUploadResponse completeUploadResponse =
                    s3Client.CompleteMultipartUpload(completeRequest);

            }
            catch (Exception exception)
            {
                MessageBox.Show("Exception occurred: {0}", exception.Message);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = existingBucketName,
                    Key = "raw/" + FolderName,
                    UploadId = initResponse.UploadId
                };
                s3Client.AbortMultipartUpload(abortMPURequest);
            }
        }

        private void CreateDrupalNode(string FolderName)
        {

            if (db != null)
            {
                db.logout();
                // Remove the event listener
                db.NewDrupalViewData -= handleNewViewData;
                db = null;
            }


            string UserName = Properties.Settings.Default.UserName;
            string Password = Properties.Settings.Default.Password;

            db = new DrupalBridge("http://ec2-54-244-149-251.us-west-2.compute.amazonaws.com/", "api/products", UserName, Password);
            db.NewDrupalViewData += handleNewViewData;
            db.login();


            List<string> Images = Helpers.ImageUtilityHelper.GetFiles(ImagePath.Text.ToString()).ToList();
            string li = "<li>{0}</li>";
            string ul = "<ul>{0}</ul>";
            string a = "<a href='{0}'>{1}</a>";


            string body = "";
            foreach (var item in Images)
            {
                string fileName = Path.GetFileName(item);
                string baseURL = "http://e38rawstore.s3-website-us-west-2.amazonaws.com/raw/" + FolderName + "/" + fileName;

                string content = string.Format(li, string.Format(a, baseURL, fileName));

                body = string.Concat(body, content);
            }

            db.CreateNode("Mission-Data-" + FolderName, string.Format(ul, body), "", true);
        }

        public void UploadFile(IAmazonS3 client)
        {
            try
            {

                TransferUtility directoryTransferUtility = new TransferUtility(client);


                string FolderName = Guid.NewGuid().ToString();

                CreateDrupalNode(FolderName);

                TransferUtilityUploadDirectoryRequest request =
                    new TransferUtilityUploadDirectoryRequest
                    {
                        BucketName = existingBucketName,
                        Directory = ImagePath.Text,
                        SearchOption = SearchOption.AllDirectories,
                        SearchPattern = "*",
                        KeyPrefix = "raw/" + FolderName
                    };

                request.CannedACL = S3CannedACL.PublicRead;

                //request.UploadProgressEvent += (source, progress) =>
                //{
                //    Console.WriteLine("{0}% - {1} / {2}",
                //        progress.PercentDone,
                //        progress.TransferredBytes,
                //        progress.TotalBytes);
                //};


                //TextBoxStreamWriter _writer = new TextBoxStreamWriter(textBox1);
                //// Redirect the out Console stream
                //Console.SetOut(_writer);


                request.UploadDirectoryProgressEvent += (source, progress) =>
                {
                    //textBox1.Invoke((Action)delegate
                    //{
                    //    textBox1.Text = string.Format("{0} - {1} / {2}",
                    //        progress.CurrentFile,
                    //        progress.TransferredBytesForCurrentFile,
                    //        progress.TotalNumberOfBytesForCurrentFile);
                    //});

                    //textBox1.Text = string.Format("{0} - {1} / {2}",
                    //        progress.CurrentFile,
                    //        progress.TransferredBytesForCurrentFile,
                    //        progress.TotalNumberOfBytesForCurrentFile);

                    //this.SetText(string.Format("{0} - {1} / {2}",
                    //        progress.CurrentFile,
                    //        progress.TransferredBytesForCurrentFile,
                    //        progress.TotalNumberOfBytesForCurrentFile));



                    Console.WriteLine("{0} - {1} / {2}",
                        progress.CurrentFile,
                        progress.TransferredBytesForCurrentFile,
                        progress.TotalNumberOfBytesForCurrentFile);

                    Application.DoEvents();
                };

                //request.UploadDirectoryProgressEvent +=
                //   new EventHandler<UploadDirectoryProgressArgs>
                //       (uploadRequest_UploadPartProgressEvent);

                directoryTransferUtility.UploadDirectory(request);



                // http://e38rawstore.s3-website-us-west-2.amazonaws.com/raw/imageToSave.jpg



            }

            catch (AmazonS3Exception e)
            {
                Console.WriteLine(e.Message, e.InnerException);
            }


            MessageBox.Show("Uplaod Complete!");
        }

        private void SelectImage_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath.Text = dialog.SelectedPath;
            }

            List<string> Images = Helpers.ImageUtilityHelper.GetFiles(ImagePath.Text.ToString()).ToList();


            foreach (var item in Images)
            {
                ImageUpload newImage = new ImageUpload();

                newImage.ImageName = Path.GetFileName(item);
                newImage.ImagePath = item;
                newImage.Status = "Queued";
                newImage.Size = ConvertBytesToMegabytes(new System.IO.FileInfo(item).Length).ToString() + " MB";

                imgClass.Add(newImage);
            }

            lblTotalQueued.Text = imgClass.Count().ToString();

            dataGridView1.DataSource = imgClass;


        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return Math.Round((bytes / 1024f) / 1024f, 2);
        }

        private void btnSave_Click_1(object sender, EventArgs e)
        {
            IAmazonS3 s3Client = GetS3Client();

            //UploadFile(s3Client);
            LoopAndUploadFiles();
        }
    }
}
