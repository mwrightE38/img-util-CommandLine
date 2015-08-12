using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Event38.ImageUtility._Classes
{
    /// <summary>
    /// Boy probably not very syntactically nice but
    /// I need to pass along data with these custom events!
    /// </summary>
    public class drupalEventArgs : EventArgs
    {
        public string _responseType = "";
        public string _theMessage = "";

        public drupalEventArgs(string _rType, string _message)
        {
            _responseType = _rType;
            _theMessage = _message;
        }
    }

    /// <summary>
    /// This class is intented for communication with the Drupal content management system
    /// version 7 and the Services module version 3 using the REST server.
    /// 
    /// Primary functionality is:
    /// 1. Submitting images as an authenticated user
    /// 2. Pulling several images from the CMS at once
    /// 
    /// In .NET WebRequests cookies are disabled by default.
    /// To make login/logout easier we will explicitly enable .NET
    /// cookie support to help us out.
    /// 
    /// In .NET by default WebRequests are syncronous and block.
    /// We are going to use blocking behavior in this class.
    /// </summary>
    public class DrupalBridge
    {
        #region Drupal Data Members
        // The event delegate
        public delegate void FireDrupalEventHandler(object sender, drupalEventArgs fe);

        // The event
        public event FireDrupalEventHandler NewDrupalViewData;

        private string _dServerUrl = "";
        private string _dServiceName = "";
        private string _dUserName = "";
        private string _dUserPW = "";
        private bool _loggedIn = false;
        private string _loggedInUserId = "-1";
        public string _latestFID = "-1";
        private string _latestNewURI = "";
        private string _latestNID = "-1";
        private string _latestnidURI = "";
        private string _token = "";


        //public const string METHOD_USER_LOGOUT = "/user/logout";
        //public const string METHOD_USER_LOGIN = "/user/login";
        //public const string METHOD_FILE_CREATE = "/file";
        //public const string METHOD_NODE_RETRIEVE = "/node/";
        //public const string METHOD_NODE_CREATE = "/node";
        //public const string METHOD_VIEW_RETRIEVE = "/views/";
        #endregion

        #region HTTP Request Data Members
        private CookieContainer drupalCookies = null;
        #endregion

        public DrupalBridge(string _serverUrl, string _serviceName, string _drupalUserName, string _drupalUserPW)
        {
            this._dServerUrl = _serverUrl;
            this._dServiceName = _serviceName;
            this._dUserName = _drupalUserName;
            this._dUserPW = _drupalUserPW;

            // Initialize a new cookie container for use with any requests
            drupalCookies = new CookieContainer();
        }

        public string getServerBaseURI()
        {
            return this._dServerUrl;
        }

        /// <summary>
        /// Im kind of annoyed that I need to set boundries and items
        /// myself, but oh well.
        /// </summary>
        public void login()
        {
            HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.USER_LOGIN);
            thisHttpRequest.CookieContainer = drupalCookies;
            thisHttpRequest.Method = "POST";

            // Add Post parameters manually
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            thisHttpRequest.KeepAlive = true;

            // Add/Edit Headers
            thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
            thisHttpRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            NameValueCollection nvc = new NameValueCollection();
            nvc.Add("username", this._dUserName);
            nvc.Add("password", this._dUserPW);
            Stream rs = thisHttpRequest.GetRequestStream();

            foreach (string key in nvc.Keys)
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                rs.Write(formitembytes, 0, formitembytes.Length);
            }
            rs.Write(boundarybytes, 0, boundarybytes.Length);

            byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            rs.Write(trailer, 0, trailer.Length);
            rs.Close();

            // Send to the consolodated response handler
            parseHttpResponse(ref thisHttpRequest, DrupalSettings.USER_LOGIN);
        }

        /// <summary>
        /// Call to logout if any session is currently active.
        /// 
        /// We dont need any explicit parameters here except for the cookie
        /// to be passed along with this request.
        /// </summary>
        public void logout()
        {
            if (this._loggedIn == true)
            {
                HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.USER_LOGOUT);
                thisHttpRequest.CookieContainer = drupalCookies;
                thisHttpRequest.Method = "POST";

                // Add/Edit Headers
                thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

                // Send to the consolodated response handler
                parseHttpResponse(ref thisHttpRequest, DrupalSettings.USER_LOGOUT);

            }
        }

        /// <summary>
        /// Attempts to retrieve a node with the specified id
        /// returns in xml format
        /// </summary>
        /// <param name="_nodeID"></param>
        public void getNode(int _nodeID)
        {
            HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.NODE_RETRIEVE + _nodeID.ToString());
            thisHttpRequest.CookieContainer = drupalCookies;
            thisHttpRequest.Method = "GET";

            // Add/Edit headers - This leaves user agent blank
            thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // Send to the consolodated response handler
            parseHttpResponse(ref thisHttpRequest, DrupalSettings.NODE_RETRIEVE);

        }

        /// <summary>
        /// Attempts to retrieve a drupal view. May require a logged in state.
        /// </summary>
        /// <param name="_viewName"></param>
        public void getNamedView(string _viewName)
        {
            HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.VIEW_RETRIEVE + _viewName);
            thisHttpRequest.CookieContainer = drupalCookies;
            thisHttpRequest.Method = "GET";

            // Add/Edit headers - This leaves user agent blank
            thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";

            // Send to the consolodated response handler
            parseHttpResponse(ref thisHttpRequest, DrupalSettings.VIEW_RETRIEVE);
        }


        public void submitFileB64(ref byte[] _fileContentNoB64, string _fileNameOnServerWExtension, string _validMIMEType)
        {
            if (this._loggedIn == true)
            {
                HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.FILE_CREATE);
                thisHttpRequest.CookieContainer = drupalCookies;
                thisHttpRequest.Method = "POST";

                // Add Post parameters manually
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                thisHttpRequest.KeepAlive = true;

                // Add/Edit Headers
                //--> Worked in Java thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                thisHttpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                thisHttpRequest.Headers.Add("X-CSRF-Token", _token);

                // Base64 Encode the data - likeley to suck a lot of memory and cpu time
                string base64String = "";
                try
                {
                    base64String = System.Convert.ToBase64String(_fileContentNoB64, 0, _fileContentNoB64.Length);
                }
                catch (System.ArgumentNullException)
                {
                    System.Diagnostics.Debug.WriteLine("Error encoding file to b64");
                    return;
                }

                if (base64String != "")
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add("filename", _fileNameOnServerWExtension);
                    nvc.Add("filesize", _fileContentNoB64.Length.ToString());
                    nvc.Add("uid", _loggedInUserId);
                    nvc.Add("file", base64String);

                    Stream rs = thisHttpRequest.GetRequestStream();

                    foreach (string key in nvc.Keys)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, nvc[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                    rs.Write(boundarybytes, 0, boundarybytes.Length);

                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    rs.Write(trailer, 0, trailer.Length);
                    rs.Close();

                    // Send to the consolodated response handler
                    parseHttpResponse(ref thisHttpRequest, DrupalSettings.FILE_CREATE);

                }
            }
        }

        /// <summary>
        /// Doing things the drupal way we need to first create a new file inside of
        /// drupal. To do this with the REST server we base 64 encode a bytearray
        /// of an already encoded media file. So if its an image, the bytearray
        /// should already be jpeg/png encoded. If its a video the bytearray
        /// should already be encoded in the desired format.
        /// </summary>
        public void submitFileB64v2(string base64String, string _fileNameOnServerWExtension, long filesize)
        {
            if (this._loggedIn == true)
            {
                string requestURL = this._dServerUrl + this._dServiceName + DrupalSettings.FILE_CREATE;
                HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(requestURL);
                thisHttpRequest.CookieContainer = drupalCookies;
                thisHttpRequest.Method = "POST";

                // Add Post parameters manually
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                thisHttpRequest.KeepAlive = true;

                // Add/Edit Headers
                //--> Worked in Java thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                thisHttpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                thisHttpRequest.Headers.Add("X-CSRF-Token", _token);

                // Base64 Encode the data - likeley to suck a lot of memory and cpu time
                //string base64String = "";
                //try
                //{
                //    //base64String = System.Convert.ToBase64String(_fileContentNoB64, 0, _fileContentNoB64.Length);
                //}
                //catch (System.ArgumentNullException)
                //{
                //    System.Diagnostics.Debug.WriteLine("Error encoding file to b64");
                //    return;
                //}

                if (base64String != "")
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add("filename", _fileNameOnServerWExtension);
                    nvc.Add("filesize", filesize.ToString());
                    nvc.Add("uid", _loggedInUserId);
                    nvc.Add("file", base64String);

                    Stream rs = thisHttpRequest.GetRequestStream();

                    foreach (string key in nvc.Keys)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, nvc[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                    rs.Write(boundarybytes, 0, boundarybytes.Length);

                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    rs.Write(trailer, 0, trailer.Length);
                    rs.Close();

                    // Send to the consolodated response handler
                    parseHttpResponse(ref thisHttpRequest, DrupalSettings.FILE_CREATE);
                }
            }
        }

        /// <summary>
        /// Creates a new node using the previously submitted file ID stored internally. Requires
        /// a valid logged in cookie state and the user must have appropriate permissions in
        /// drupal.
        /// </summary>
        /// <param name="_contentTitle"></param>
        /// <param name="_contentBody"></param>
        /// <param name="_altText"></param>
        /// <param name="_width"></param>
        /// <param name="_height"></param>
        /// <param name="_publish"></param>
        public void submitContentTypeUsingPreviousFID(string _contentTitle, string _contentBody, string _altText, bool _publish, List<string> FileId)
        {
            if (this._loggedIn == true)
            {
                HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.NODE_CREATE);
                thisHttpRequest.CookieContainer = drupalCookies;
                thisHttpRequest.Method = "POST";

                // Add Post parameters manually
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                thisHttpRequest.KeepAlive = true;

                // Add/Edit Headers
                //--> Worked in Java thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                thisHttpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                thisHttpRequest.Headers.Add("X-CSRF-Token", _token);

                if (this._latestFID != "-1")
                {
                    NameValueCollection nvc = new NameValueCollection();
                    nvc.Add("type", "test_app");


                    int FileIndex = 0;

                    foreach (var item in FileId)
                    {
                        string curFileIdIndex = string.Format("field_photo[und][{0}][fid]", FileIndex);
                        string curUIDIndex = string.Format("field_photo[und][{0}][uid]", FileIndex);

                        nvc.Add(curFileIdIndex, this._latestFID);
                        nvc.Add(curUIDIndex, this._loggedInUserId);

                        FileIndex += 1;
                    }


                    if (_publish == true)
                    {
                        nvc.Add("status", "1");
                    }
                    nvc.Add("title", _contentTitle);
                    nvc.Add("body[und][0][value]", _contentBody);

                    Stream rs = thisHttpRequest.GetRequestStream();
                    foreach (string key in nvc.Keys)
                    {
                        rs.Write(boundarybytes, 0, boundarybytes.Length);
                        string formitem = string.Format(formdataTemplate, key, nvc[key]);
                        byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                        rs.Write(formitembytes, 0, formitembytes.Length);
                    }
                    rs.Write(boundarybytes, 0, boundarybytes.Length);

                    byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                    rs.Write(trailer, 0, trailer.Length);
                    rs.Close();

                    // Send to the consolodated response handler
                    parseHttpResponse(ref thisHttpRequest, DrupalSettings.NODE_CREATE);

                }
            }
        }


        public void CreateNode(string _contentTitle, string _contentBody, string _altText, bool _publish)
        {
            if (this._loggedIn == true)
            {
                HttpWebRequest thisHttpRequest = (HttpWebRequest)WebRequest.Create(this._dServerUrl + this._dServiceName + DrupalSettings.NODE_CREATE);
                thisHttpRequest.CookieContainer = drupalCookies;
                thisHttpRequest.Method = "POST";

                // Add Post parameters manually
                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
                string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
                thisHttpRequest.KeepAlive = true;

                // Add/Edit Headers
                //--> Worked in Java thisHttpRequest.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                thisHttpRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
                thisHttpRequest.ContentType = "multipart/form-data; boundary=" + boundary;
                thisHttpRequest.Headers.Add("X-CSRF-Token", _token);


                NameValueCollection nvc = new NameValueCollection();
                nvc.Add("type", "missiondata");


                if (_publish == true)
                {
                    nvc.Add("status", "1");
                }
                nvc.Add("title", _contentTitle);

                nvc.Add("title_field[und][0][value]", _contentTitle);

                nvc.Add("body[und][0][value]", _contentBody);

                nvc.Add("field_pcstatus[und][0][value]", "Queued");
              
                nvc.Add("field_app_order[und][0][value]", "Mosaic");

                Stream rs = thisHttpRequest.GetRequestStream();
                foreach (string key in nvc.Keys)
                {
                    rs.Write(boundarybytes, 0, boundarybytes.Length);
                    string formitem = string.Format(formdataTemplate, key, nvc[key]);
                    byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                    rs.Write(formitembytes, 0, formitembytes.Length);
                }
                rs.Write(boundarybytes, 0, boundarybytes.Length);

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();

                // Send to the consolodated response handler
                parseHttpResponse(ref thisHttpRequest, DrupalSettings.NODE_CREATE);


            }
        }

        /// <summary>
        /// Consolodates parsing the response for all of these http requests
        /// using a reference to the originals.  These can throw errors
        /// so be careful.
        /// 
        /// _responseType is the type of request that was made so we know how
        /// to parse the results if sucessful.
        /// </summary>
        /// <param name="_theRequestRef"></param>
        private void parseHttpResponse(ref HttpWebRequest _theRequestRef, string _responseType)
        {
            WebResponse wresp = null;
            try
            {
                _theRequestRef.Timeout = 10000000;
                wresp = _theRequestRef.GetResponse();
                Stream stream2 = wresp.GetResponseStream();
                StreamReader reader2 = new StreamReader(stream2);
                string xmlResponse = reader2.ReadToEnd();

                switch (_responseType)
                {
                    case DrupalSettings.USER_LOGOUT:
                        System.Diagnostics.Debug.WriteLine(string.Format("Logout complete, server response is: {0}", xmlResponse));
                        this._loggedIn = false;
                        break;
                    case DrupalSettings.USER_LOGIN:
                        System.Diagnostics.Debug.WriteLine(string.Format("Login sucessful, server response is: {0}", xmlResponse));
                        this._loggedIn = true;
                        break;
                    case DrupalSettings.FILE_CREATE:
                      //  MessageBox.Show(string.Format("File created, server response is: {0}", xmlResponse));
                        System.Diagnostics.Debug.WriteLine(string.Format("File created, server response is: {0}", xmlResponse));
                        break;
                    case DrupalSettings.NODE_CREATE:
                      //  MessageBox.Show(string.Format("Node created, server response is: {0}", xmlResponse));

                        System.Diagnostics.Debug.WriteLine(string.Format("Node created, server response is: {0}", xmlResponse));
                        break;
                    case DrupalSettings.NODE_RETRIEVE:
                        System.Diagnostics.Debug.WriteLine(string.Format("Node retrieved, server response is: {0}", xmlResponse));
                        break;
                    case DrupalSettings.VIEW_RETRIEVE:
                        System.Diagnostics.Debug.WriteLine(string.Format("View retrieved, server response is: {0}", xmlResponse));
                        break;
                }

                // Send the response off for xml parsing
                parseDrupalXMLResponse(xmlResponse, _responseType);
            }
            catch (WebException ex)
            {
                using (WebResponse response = ex.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    switch (_responseType)
                    {
                        case DrupalSettings.USER_LOGOUT:
                            System.Diagnostics.Debug.WriteLine("Error logging out Status: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                        case DrupalSettings.USER_LOGIN:
                            System.Diagnostics.Debug.WriteLine("Error logging inn Status: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                        case DrupalSettings.FILE_CREATE:
                            System.Diagnostics.Debug.WriteLine("Error creating file Status: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                        case DrupalSettings.NODE_CREATE:
                            System.Diagnostics.Debug.WriteLine("Error creating node Status: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                        case DrupalSettings.NODE_RETRIEVE:
                            System.Diagnostics.Debug.WriteLine("Error recieving node: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                        case DrupalSettings.VIEW_RETRIEVE:
                            System.Diagnostics.Debug.WriteLine("Error getting view: {0} Description: {1}", httpResponse.StatusCode, httpResponse.StatusDescription);
                            break;
                    }

                    using (Stream data = response.GetResponseStream())
                    {
                        string text = new StreamReader(data).ReadToEnd();
                        System.Diagnostics.Debug.WriteLine(text);
                        data.Close();
                    }
                    httpResponse.Close();
                }

                if (wresp != null)
                {
                    wresp.Close();
                    wresp = null;
                }
            }
            finally
            {
                _theRequestRef = null;
            }
        }

        private void parseDrupalXMLResponse(string _response, string _responseType)
        {
            if (_response != "")
            {
                // Create an XmlReader
                using (XmlReader reader = XmlReader.Create(new StringReader(_response)))
                {
                    try
                    {
                        switch (_responseType)
                        {
                            case DrupalSettings.USER_LOGOUT:
                                reader.ReadToFollowing("result");
                                string tempLogoutValue = reader.ReadElementContentAsString();
                                System.Diagnostics.Debug.WriteLine("Logout xml parse result: " + tempLogoutValue);
                                if (tempLogoutValue == "1")
                                {
                                    this._loggedInUserId = "-1";
                                }

                                // DISPATCH
                                drupalEventArgs logoutEventData = new drupalEventArgs(DrupalSettings.USER_LOGOUT, _response);
                                NewDrupalViewData(this, logoutEventData);
                                break;
                            case DrupalSettings.USER_LOGIN:
                                reader.ReadToFollowing("token");
                                this._token = reader.ReadElementContentAsString();

                                reader.ReadToFollowing("uid");
                                this._loggedInUserId = reader.ReadElementContentAsString();


                                System.Diagnostics.Debug.WriteLine("Login xml parse userID: " + this._loggedInUserId);
                                // DISPATCH
                                drupalEventArgs loginEventData = new drupalEventArgs(DrupalSettings.USER_LOGIN, _response);
                                NewDrupalViewData(this, loginEventData);
                                break;
                            case DrupalSettings.FILE_CREATE:
                                reader.ReadToFollowing("fid");
                                this._latestFID = reader.ReadElementContentAsString();
                                // ReadToFollowing auto hops to the next element
                                if (reader.Name != "uri")
                                {
                                    reader.ReadToFollowing("uri");
                                }
                                this._latestNewURI = reader.ReadElementContentAsString();
                                drupalEventArgs FileEventData = new drupalEventArgs(DrupalSettings.NODE_CREATE, _response);
                                NewDrupalViewData(this, FileEventData);
                                break;
                            case DrupalSettings.NODE_CREATE:
                                reader.ReadToFollowing("nid");
                                this._latestNID = reader.ReadElementContentAsString();
                                // ReadToFollowing auto hops to the next element
                                if (reader.Name != "uri")
                                {
                                    reader.ReadToFollowing("uri");
                                }
                                this._latestnidURI = reader.ReadElementContentAsString();
                                break;
                            case DrupalSettings.NODE_RETRIEVE:
                                // Data here will be really unique to the content type of the node may want to consider
                                // passing this off to a specific class.

                                // DISPATCH
                                drupalEventArgs thisEventData = new drupalEventArgs(DrupalSettings.NODE_RETRIEVE, _response);
                                NewDrupalViewData(this, thisEventData);

                                break;
                            case DrupalSettings.VIEW_RETRIEVE:
                                // Data here will be really unique to the content types contained in the view may want to consider
                                // passing this off to a specific class.

                                // DISPATCH
                                drupalEventArgs nextEventData = new drupalEventArgs(DrupalSettings.VIEW_RETRIEVE, _response);
                                NewDrupalViewData(this, nextEventData);

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
}
