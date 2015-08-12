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


namespace Event38.ImageUtility
{

    delegate void SetTextCallback(string text);


    public partial class UploadImages : Form
    {
        public UploadImages()
        {
            InitializeComponent();

        }

        //public void CreatePostRequestPHP()
        //{
        //   // const string url = "http://localhost/e38aws_apitest.php";
        //    const string url = "http://app.event38.com/e38aws_apitest.php";

        //    //create an HTTP request to the URL that we need to invoke
        //    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
        //    request.ContentType = "application/json; charset=utf-8"; //set the content type to JSON
        //    request.Method = "POST"; //make an HTTP POST

        //    //string json = "";


        //    List<string> Images = Helpers.ImageUtility.GetFiles(ImagePath.Text.ToString()).ToList();
        //    List<ImageAPIUpload> imageJSON = new List<ImageAPIUpload>();

        //    //$type = 'test_app'; // node type
        //    //$title = 'Testing Testing - 1 - app.event38.com'; // title 
        //    //$body = '<p>Body lorem ipsum</p>'; // body
        //    //$first_name = 'booo'; // first name
        //    //$last_name = 'yahhhhh'; // last name
        //    //$filename = '/Users/laxg/Downloads/azurelogo.jpg';  // file to upload and attach with content

        //    foreach (var item in Images)
        //    {
        //        ImageAPIUpload img = new ImageAPIUpload();

        //        img.type = "test_app";
        //        img.title = "Testing C# ingegration";
        //        img.body = "<p>Test Uploads from Kevin Innes</p>";
        //        img.first_name = "Kevin";
        //        img.last_name = "Innes";
        //        img.FileBase64 = ImageToBase64(Image.FromFile(item),ImageFormat.Jpeg);
        //        img.filename = Path.GetFileName(item);
        //        FileInfo f = new FileInfo(item);
        //        img.FileSize = f.Length;


        //        imageJSON.Add(img);

        //    }

        //    var jsonSerialiser = new JavaScriptSerializer();
        //    jsonSerialiser.MaxJsonLength = 999999999;

        //    var json = jsonSerialiser.Serialize(imageJSON);

        //    using (var streamWriter = new StreamWriter(request.GetRequestStream()))
        //    {
        //        //initiate the request
        //        //JavaScriptSerializer serializer = new JavaScriptSerializer();
        //        //serializer.MaxJsonLength = 999999999;
        //        //var resToWrite = serializer.Deserialize<List<ImageAPIUpload>>(json);
        //        streamWriter.Write(json);
        //        streamWriter.Flush();
        //        streamWriter.Close();
        //    }

        //    // Get the response.
        //    WebResponse response = request.GetResponse();
        //    var streamReader = new StreamReader(response.GetResponseStream());
        //    var result = streamReader.ReadToEnd();
        //}

        //public string ImageToBase64(Image image,  ImageFormat format)
        //{
        //    using (MemoryStream ms = new MemoryStream())
        //    {
        //        // Convert Image to byte[]
        //        image.Save(ms, format);
        //        byte[] imageBytes = ms.ToArray();

        //        // Convert byte[] to Base64 String
        //        string base64String = Convert.ToBase64String(imageBytes);
        //        return base64String;
        //    }
        //}

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ImagePath.Text = dialog.SelectedPath;
            }
        }


        public DrupalBridge db = null;
        static string existingBucketName = "e38rawstore";
        AmazonS3Config S3Config = new AmazonS3Config
        {
            ServiceURL = "s3-website-us-west-2.amazonaws.com"
        };

        public IAmazonS3 GetS3Client()
        {
            //Amazon.Runtime.AWSCredentials credentials = new Amazon.Runtime.StoredProfileAWSCredentials("development");
            //Amazon.S3.IAmazonS3 s3Client = new AmazonS3Client(credentials, Amazon.RegionEndpoint.USWest2);

            IAmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client(Amazon.RegionEndpoint.USWest2);


            //IAmazonS3 s3Client = AWSClientFactory.CreateAmazonS3Client(
            //        "AKIAIZADOOW5W2SI42IQ",
            //        "bFal757yYJFZgRorNZBVpjstZvjp/cLvBIoYGS4K", S3Config
            //        );
            return s3Client;
        }


        //private void SetText(string text)
        //{
        //    // InvokeRequired required compares the thread ID of the
        //    // calling thread to the thread ID of the creating thread.
        //    // If these threads are different, it returns true.
        //    if (this.textBox1.InvokeRequired)
        //    {
        //        SetTextCallback d = new SetTextCallback(SetText);
        //        this.Invoke(d, new object[] { text });
        //    }
        //    else
        //    {
        //        this.textBox1.Text = text;
        //    }
        //}

        public void LoopAndUploadFiles()
        {
            List<string> Images = Helpers.ImageUtilityHelper.GetFiles(ImagePath.Text.ToString()).ToList();

            string FolderName = Guid.NewGuid().ToString();


            foreach (var item in Images)
            {
                //richTextBox1.Text += "Upload Started for " + Path.GetFileName(item) + Environment.NewLine;
                //richTextBox1.Refresh();

                UploadMultiPartFile(item, FolderName);


                //richTextBox1.Text += "Finished Upload for " + Path.GetFileName(item) + Environment.NewLine;
                //richTextBox1.Refresh();
            }
        }

        public void UploadMultiPartFile(string filePath, string FolderName)
        {
            MessageBox.Show(FolderName);

            IAmazonS3 s3Client = new AmazonS3Client("AKIAIZADOOW5W2SI42IQ",
                   "bFal757yYJFZgRorNZBVpjstZvjp/cLvBIoYGS4K", Amazon.RegionEndpoint.USWest2);

            // List to store upload part responses.
            List<UploadPartResponse> uploadResponses = new List<UploadPartResponse>();

            // 1. Initialize.
            InitiateMultipartUploadRequest initiateRequest = new InitiateMultipartUploadRequest
            {
                BucketName = existingBucketName,
                Key = "raw/" + FolderName
            };

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

                    // Upload part and add response to our list.
                    uploadResponses.Add(s3Client.UploadPart(uploadRequest));

                    filePosition += partSize;

                    //richTextBox1.Text += Path.GetFileName(filePath) + " - " + (filePosition * .001) + "kb / " + (contentLength * .001) + "kb" + Environment.NewLine;
                    //richTextBox1.Refresh();

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
                Console.WriteLine("Exception occurred: {0}", exception.Message);
                AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
                {
                    BucketName = existingBucketName,
                    Key = "raw/" + FolderName,
                    UploadId = initResponse.UploadId
                };
                s3Client.AbortMultipartUpload(abortMPURequest);
            }
        }

        public void UploadFile(IAmazonS3 client)
        {
            try
            {

                TransferUtility directoryTransferUtility =
                    new TransferUtility(new AmazonS3Client("AKIAIZADOOW5W2SI42IQ",
                   "bFal757yYJFZgRorNZBVpjstZvjp/cLvBIoYGS4K", Amazon.RegionEndpoint.USWest2));


                string FolderName = Guid.NewGuid().ToString();

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

            catch (AmazonS3Exception e)
            {
                Console.WriteLine(e.Message, e.InnerException);
            }


            MessageBox.Show("Uplaod Complete!");


        }


        private void UpdateText(Label label, string text)
        {
            // If the current thread is not the UI thread, InvokeRequired will be true
            if (label.InvokeRequired)
            {
                // If so, call Invoke, passing it a lambda expression which calls
                // UpdateText with the same label and text, but on the UI thread instead.
                label.Invoke((Action)(() => UpdateText(label, text)));
                return;
            }
            // If we're running on the UI thread, we'll get here, and can safely update 
            // the label's text.
            label.Text = text;
            label.Update();

        }

        private static Dictionary<string, int> uploadTracker = new Dictionary<string, int>();
        public void uploadRequest_UploadPartProgressEvent(object sender, UploadDirectoryProgressArgs e)
        {
            //string fileName = e.CurrentFile;

            //Console.WriteLine(e.CurrentFile);

            //textBox1.Invoke((Action)delegate{ textBox1.Text = e.TotalNumberOfBytesForCurrentFile.ToString();});
            //TransferUtilityUploadRequest req = sender as TransferUtilityUploadRequest;
            //if (req != null)
            //{
            //    string fileName = req.FilePath.Split('\\').Last();
            //    if (!uploadTracker.ContainsKey(fileName))
            //        uploadTracker.Add(fileName, e.);

            //    //When percentage done changes add logentry:
            //    if (uploadTracker[fileName] != e.PercentDone)
            //    {
            //        uploadTracker[fileName] = e.PercentDone;
            //       // Log.Add(LogTypes.Debug, 0, String.Format("WritingLargeFile progress: {1} of {2} ({3}%) for file '{0}'", fileName, e.TransferredBytes, e.TotalBytes, e.PercentDone));
            //    }
            //}

        }

        public static int GetAmazonUploadPercentDone(string fileName)
        {
            if (!uploadTracker.ContainsKey(fileName))
                return 0;

            return uploadTracker[fileName];
        }


        public delegate void UpdateTextCallback(string text);



        private void btnSave_Click(object sender, EventArgs e)
        {
            IAmazonS3 s3Client = GetS3Client();

            UploadFile(s3Client);
            //LoopAndUploadFiles();

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
    }
}
