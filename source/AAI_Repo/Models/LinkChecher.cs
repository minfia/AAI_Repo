using System;
using System.Net;

namespace AAI_Repo.Models
{
    enum CheckResult
    {
        /// <summary>
        /// 完了
        /// </summary>
        Complete,
        /// <summary>
        /// 404エラー
        /// </summary>
        Connection404Error,
        /// <summary>
        /// 接続タイムアウトエラー
        /// </summary>
        ConnectionTimeoutError,
        /// <summary>
        /// 接続エラー
        /// </summary>
        ConnectionError,
        /// <summary>
        /// 対象ファイルがウィルスに感染している可能性あり(GoogleDriveのみ)
        /// </summary>
        GDriveVirus,
    }

    class LinkChecher
    {
        public CheckResult CheckStart(string url)
        {
            CheckResult result;
            if (url.Contains("drive.google"))
            {
                GDriveChecker gDriveChecker = new GDriveChecker();
                result = gDriveChecker.IsConnectURL(url);
            }
            else
            {
                CheckerBase checkerBase = new CheckerBase();
                result = checkerBase.IsConnectURL(url);
            }

            return result;
        }

        private class CheckerBase
        {
            private Uri _uri;
            private WebHeaderCollection _header;
            private protected CookieCollection _cookieCollection;
            /// <summary>
            /// ダウンロード完了サイズ
            /// </summary>
            public ulong DownloadCompleteSize { get; private protected set; }
            /// <summary>
            /// ダウンロードするファイルサイズ
            /// </summary>
            public ulong DownloadFileSize { get; private set; }

            public CheckerBase()
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls |
                                                       SecurityProtocolType.Tls11 |
                                                       SecurityProtocolType.Tls12 |
                                                       SecurityProtocolType.Tls13;
            }

            /// <summary>
            /// ヘッダを取得
            /// </summary>
            /// <param name="uri">取得するURI</param>
            /// <returns>DownloadResult</returns>
            private CheckResult GetHeader(Uri uri)
            {
                HttpWebRequest webRequest = null;
                try
                {
                    webRequest = (HttpWebRequest)WebRequest.Create(uri);
                    webRequest.Credentials = CredentialCache.DefaultCredentials;
                    CookieContainer cookieContainer = new CookieContainer();
                    webRequest.CookieContainer = new CookieContainer();
                    webRequest.CookieContainer.Add(cookieContainer.GetCookies(webRequest.RequestUri));
                    webRequest.UserAgent = "AviUtlAutoInstaller";
                    WebResponse webResponse = webRequest.GetResponse();
                    _header = ((HttpWebResponse)webResponse).Headers;
                    _cookieCollection = ((HttpWebResponse)webResponse).Cookies;
                }
                catch (WebException e)
                {
                    Console.WriteLine(e.Message);
                    return WebExceptionPaser(e);

                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return CheckResult.ConnectionError;
                }
                finally
                {
                    if (webRequest != null)
                    {
                        // 連続呼び出しでエラーになる場合があるのでその対処
                        webRequest.Abort();
                    }
                }

                return CheckResult.Complete;
            }

            private CheckResult WebExceptionPaser(WebException webException)
            {
                if (webException.Response == null)
                {
                    // WebExceptionStatus
                    var status = webException.Status;
                    switch (status)
                    {
                        case WebExceptionStatus.Success:
                            return CheckResult.Complete;
                        case WebExceptionStatus.Timeout:
                            return CheckResult.ConnectionTimeoutError;
                    }
                }
                else
                {
                    // HttpWebResponce Status
                    var response = webException.Response as HttpWebResponse;
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            return CheckResult.Complete;
                        case HttpStatusCode.NotFound:
                            return CheckResult.Connection404Error;
                    }
                }
                return CheckResult.ConnectionError;
            }

            /// <summary>
            /// 指定URLに接続できるか確認する
            /// </summary>
            /// <param name="url"></param>
            /// <returns>DownloadResult</returns>
            public virtual CheckResult IsConnectURL(string url)
            {
                if (string.IsNullOrWhiteSpace(url))
                {
                    return CheckResult.ConnectionError;
                }

                CheckResult res = CheckResult.Complete;
                try
                {
                    _uri = new Uri(url);
                    res = GetHeader(_uri);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return res;
                }

                return res;
            }

            /// <summary>
            /// 指定文字列から範囲(文字列)を指定して切り抜く
            /// </summary>
            /// <param name="src">切り抜く文字列</param>
            /// <param name="start">開始文字列</param>
            /// <param name="end">終了文字列</param>
            /// <param name="dst">切り抜いた文字列</param>
            /// <returns>成否</returns>
            protected bool SubString(string src, string start, string end, out string dst)
            {
                dst = "";
                int indexStart = src.IndexOf(start);
                if (indexStart == -1)
                {
                    return false;
                }
                indexStart += start.Length;

                int indexEnd = 0;
                if (!string.IsNullOrEmpty(end))
                {
                    indexEnd = src.IndexOf(end);
                    if (indexEnd == -1)
                    {
                        return false;
                    }
                }

                dst = (end == null) ? src.Substring(indexStart) : src.Substring(indexStart, (indexEnd - indexStart));

                return true;
            }
        }

        private class GDriveChecker : CheckerBase
        {
            string _fileId;

            /// <summary>
            /// 指定URLに接続できるか確認する
            /// </summary>
            /// <param name="shareURL">共有URL</param>
            /// <returns>成否</returns>
            public override CheckResult IsConnectURL(string shareURL)
            {
                string[][] strArray = new string[][] {  new string[] {"uc?id=", null},    // 直接URL
                                                        new string[] {"/d/", "/view"},    // 共有URL
                                                        new string[] {"open?id=", null},  // 共有URL
                                                     };

                string url;
                foreach (string[] str in strArray)
                {
                    string temp;
                    if (SubString(shareURL, str[0], str[1], out temp))
                    {
                        _fileId = temp;
                        break;
                    }
                }
                url = $"https://drive.google.com/uc?id={_fileId}";

                return base.IsConnectURL(url);
            }
        }
    }
}
