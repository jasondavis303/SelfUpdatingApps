using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SelfUpdatingApp
{
    static class StreamHelper
    {

        public static bool IsHTTPUrl(string url) => url.StartsWith("http://", StringComparison.CurrentCultureIgnoreCase) || url.StartsWith("https://", StringComparison.CurrentCultureIgnoreCase);

        public static bool IsFTPUrl(string url) => url.StartsWith("ftp://", StringComparison.CurrentCultureIgnoreCase);

        public static bool IsWebUrl(string url) => IsHTTPUrl(url) || IsFTPUrl(url);

        public static Task<WebResponse> GetWebResponseAsync(string src)
        {
            if (IsHTTPUrl(src))
            {
                var request = WebRequest.Create(src);
                return request.GetResponseAsync();
            }
            else if (IsFTPUrl(src))
            {
                var request = (FtpWebRequest)WebRequest.Create(src);
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential("anonymous", string.Empty);
                return request.GetResponseAsync();
            }
            else
            {
                throw new Exception("Not a web url: " + src);
            }
        }

        public static FileStream OpenAsyncRead(string filename) => new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, Constants.BUFFER_SIZE);
       
        public static FileStream OpenAsyncRead(this FileInfo info) => OpenAsyncRead(info.FullName);

        public static FileStream CreateAsyncWrite(string filename) => File.Create(filename, Constants.BUFFER_SIZE, FileOptions.Asynchronous);
    }
}
