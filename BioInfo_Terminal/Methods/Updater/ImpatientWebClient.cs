using System;
using System.ComponentModel;
using System.Net;
using System.Timers;

namespace BioInfo_Terminal.Methods.Updater
{
    internal class ImpatientWebClient : WebClient
    {
        private const int WaitTime = 5 * 1000; // 5 seconds

        protected override WebRequest GetWebRequest(Uri address)
        {
            var wr = base.GetWebRequest(address);
            if (wr != null)
                wr.Timeout = WaitTime;
            return wr;
        }

        // this is unfortunately necessary since WebClient doesn't respect the Timeout property of HttpWebRequest if you call
        // DownloadFileAsync (only works for non-Async calls). since we still want to modify the timeout, we either have
        // to deal with HttpWebRequests manually to download the file ourselves, or make this extra timer thread for a few seconds
        // that will call CancelAsync to forcefully stop the download if it isn't connecting or finishing
        public void ImpatientAsyncDownload(Uri uri, string target)
        {
            DownloadFileAsync(uri, target);
            var cancelTimer = new Timer();
            ElapsedEventHandler handler = null;
            handler = (s, a) =>
            {
                cancelTimer.Elapsed -= handler;
                CancelAsync();
                cancelTimer.Stop();
            };
            AsyncCompletedEventHandler finished = null;
            finished = (s, a) =>
            {
                cancelTimer.Stop();
                DownloadFileCompleted -= finished;
            };
            DownloadFileCompleted += finished;
            cancelTimer.Interval = WaitTime;
            cancelTimer.Elapsed += handler;
            cancelTimer.Start();
        }
    }
}