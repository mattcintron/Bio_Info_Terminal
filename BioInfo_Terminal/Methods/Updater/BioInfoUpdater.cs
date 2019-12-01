using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace BioInfo_Terminal.Methods.Updater
{
    public class BioInfoUpdater
    {
        private readonly object _lock = new object();

        // if lastUpdated > lastChecked, or version numbers are larger
        public bool UpdatesAvailable { get; private set; }

        // reference to our store
        public RepoStore RepoStore { get; private set; }

        // if configs etc are all functional and we can actually download things
        public bool Enabled { get; private set; }
        public bool Checking { get; private set; }

        internal event AsyncCompletedEventHandler RepoUpdated; // when repo.xml is downloaded

        // get repo.xml from update repository
        internal void CheckRemoteForUpdates()
        {
            lock (_lock)
            {
                if (Checking) return;
                Checking = true;
            }

            var tmpFile = Path.GetTempFileName();
            var path = Environment.CurrentDirectory;
            path = path.Replace(@"bin\Debug", "");
            path += @"TestRepo\Repo.xml";
            var webClient = new ImpatientWebClient {BaseAddress = path};
            webClient.DownloadFileCompleted += (obj, args) =>
            {
                lock (_lock)
                {
                    Checking = false;
                }

                try
                {
                    CheckStoreForUpdates(tmpFile);
                }
                catch (Exception)
                {
                    MessageBox.Show(@"Error while checking repo for updates.");
                    Enabled = false;
                }
                finally
                {
                    File.Delete(tmpFile);
                }

                RepoUpdated?.Invoke(obj, args);
            };
            var repoXmlUri = new Uri("repo.xml", UriKind.Relative);
            webClient.ImpatientAsyncDownload(repoXmlUri, tmpFile);
        }

        // deserialize downloaded repo.xml and check if it has updates
        private void CheckStoreForUpdates(string tmpStore)
        {
            RepoStore = RepoStore.Load(tmpStore);

            var lastUpdate = DateTime.Now; // TODO config
            UpdatesAvailable = RepoStore.LastUpdated > lastUpdate;
        }

        public void DownloadUpdate()
        {
            var success = false;
            try
            {
                var latest = RepoStore.Versions.Max(v => Version.Parse(v.Version));

                DownloadStuff(RepoStore.Versions.Find(v => v.Version == latest.ToString()));
                success = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                // TODO log blah
            }

            if (success)
            {
                // ReSharper disable once UnusedVariable
                var lastUpdate = DateTime.Now;
                // TODO save lastUpdate to config
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void DownloadStuff(VersionRecord record)
        {
            // todo download record.ComponentFile and record.DependencyFiles (loop)
        }
    }
}