using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace OpenNETCF.Web
{
    public class RestConnector
    {
        public string DeviceAddress { get; set; }

        public RestConnector(string deviceAddress)
        {
            DeviceAddress = deviceAddress;
        }

        protected virtual CredentialCache GenerateCredentials()
        {
            return null;
        }

        public string Get(string directory)
        {
            directory = directory.Replace('\\', '/');
            if (directory.StartsWith("/"))
            {
                directory = directory.Substring(1);
            }

            string page = string.Format("http://{0}/{1}", DeviceAddress, directory);

            StringBuilder sb = new StringBuilder();

            byte[] buf = new byte[8192];

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);

            CredentialCache creds = GenerateCredentials();
            if (creds != null)
            {
                request.Credentials = creds;
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                return GetResponseData(response);
            }
            catch
            {
                return string.Empty;
            }
        }

        private string SendData(string method, string directory, string data)
        {
            directory = directory.Replace('\\', '/');
            if (directory.StartsWith("/"))
            {
                directory = directory.Substring(1);
            }

            string page = string.Format("http://{0}/{1}", DeviceAddress, directory);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(page);
            request.KeepAlive = false;
            request.ProtocolVersion = HttpVersion.Version10;
            request.Method = method;

            CredentialCache creds = GenerateCredentials();
            if (creds != null)
            {
                request.Credentials = creds;
            }

            // turn our request string into a byte stream
            byte[] postBytes;
            
            if(data != null)
            {
                postBytes = Encoding.UTF8.GetBytes(data);
            }
            else
            {
                postBytes = new byte[0];
            }

            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = postBytes.Length;

            try
            {
                Stream requestStream = request.GetRequestStream();

                // now send it
                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();

                HttpWebResponse response;

                response = (HttpWebResponse)request.GetResponse();

                return GetResponseData(response);
            }
            catch
            {
                throw new Exception("Error Sending Data to MTConnect Device");
            }
        }

        public string Post(string directory, string data)
        {
            return SendData("POST", directory, data);
        }

        public string Put(string directory, string data)
        {
            return SendData("PUT", directory, data);
        }

        public string Delete(string directory)
        {
            return SendData("DELETE", directory, null);
        }

        private string GetResponseData(HttpWebResponse response)
        {
            StringBuilder sb = new StringBuilder();

            byte[] buf = new byte[8192];

            Stream stream = response.GetResponseStream();

            string result = null;
            int count = 0;

            do
            {
                count = stream.Read(buf, 0, buf.Length);

                if (count != 0)
                {
                    // look for a UTF8 header
                    if ((buf[0] == 0xEF) && (buf[1] == 0xBB) && (buf[2] == 0xBF))
                    {
                        result = Encoding.UTF8.GetString(buf, 3, count - 3);
                    }
                    else
                    {
                        result = Encoding.UTF8.GetString(buf, 0, count);
                    }
                    sb.Append(result);
                }
            }
            while (count > 0);

            return sb.ToString();
        }
        
    }
}
