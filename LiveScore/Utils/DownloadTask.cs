using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace LiveScore.Utils
{
    class DownloadTask
    {
        private readonly HttpClient client = new HttpClient();
        private HttpWebRequest httpRequest;

        private string url;
        private string method;
        private string response;

        public DownloadTask(string url, string method)
        {
            this.Url = url;
            this.Method = method;
            httpRequest = (HttpWebRequest)WebRequest.Create(Url);
            httpRequest.Method = Method;
        }

        public string Url { get => url; set => url = value; }
        public string Method { get => method; set => method = value; }
        public string Response { get => response; set => response = value; }

        public void SetPostParams(string postParams)
        {
            try
            {
                using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
                {
                    string json = postParams;

                    streamWriter.Write(json);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
            } catch (Exception e)
            {
                throw new WebException();
            }
        }

        public async Task<string> ExecuteRequestAsync()
        {
            httpRequest.ContentType = "application/json";

            HttpWebResponse response;
            string result = String.Empty;
            try
            {
                response = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(response.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }

                response.Close();
            }
            catch (Exception e)
            {

                throw new WebException();
            }

            return result;
        }
    }
}
