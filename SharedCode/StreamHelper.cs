﻿using System;
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
                request.Proxy = WebRequest.GetSystemWebProxy();
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                return request.GetResponseAsync();
            }
            else if (IsFTPUrl(src))
            {
                var request = (FtpWebRequest)WebRequest.Create(src);
                request.Proxy = WebRequest.GetSystemWebProxy();
                request.Proxy.Credentials = CredentialCache.DefaultCredentials;
                request.Method = WebRequestMethods.Ftp.DownloadFile;
                request.Credentials = new NetworkCredential("anonymous", string.Empty);
                return request.GetResponseAsync();
            }
            else
            {
                throw new Exception("Not a web url: " + src);
            }
        }

        public static FileStream OpenAsyncRead(string filename)
        {
            var ret = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, Constants.BUFFER_SIZE, true);
            
            try
            {
                var buffer = new byte[1];
                _ = ret.ReadAsync(buffer, 0, 1).Result;
                ret.Seek(0, SeekOrigin.Begin);
            }
            catch
            {
                ret.Dispose();
                ret = File.OpenRead(filename);
            }

            return ret;
        }

        public static FileStream OpenAsyncRead(this FileInfo info) => OpenAsyncRead(info.FullName);

        public static FileStream CreateAsyncWrite(string filename) => File.Create(filename, Constants.BUFFER_SIZE, FileOptions.Asynchronous);
    }
}
