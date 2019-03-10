using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace The_MTG_Metagamer_Shared.Clients
{
    public class HtmlClient : IDisposable
    {
        private readonly HttpWebRequest _request;
        public HtmlClient(string uri)
        {
            _request = (HttpWebRequest)WebRequest.Create(new Uri(uri));
        }

        public async Task<string> MakeRequestAsync()
        {
            string data = null;
            try
            {
                using (HttpWebResponse response = (HttpWebResponse)await _request.GetResponseAsync())
                {
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        using (Stream receiveStream = response.GetResponseStream())
                        {
                            StreamReader readStream = null;

                            if (response.CharacterSet == null)
                            {
                                readStream = new StreamReader(receiveStream);
                            }
                            else
                            {
                                readStream = new StreamReader(receiveStream, Encoding.GetEncoding(response.CharacterSet));
                            }

                            data = readStream.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e.Message}");
                throw e;
            }

            return data;
        }

        public void Dispose()
        {
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
    }
}
