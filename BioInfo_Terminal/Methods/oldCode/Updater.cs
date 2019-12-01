using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Timers;
using System.Xml.Serialization;

namespace BioInfo_Terminal.Methods
{
    public class Updater
    {
        // if lastUpdated > lastChecked, or version numbers are larger
        public bool UpdatesAvailable { get; private set; }
        // reference to our store
        public RepoStore RepoStore { get; private set; }
        // if configs etc are all functional and we can actually download things
        public bool Enabled { get; private set; }
        public bool Checking { get; private set; }

        internal event AsyncCompletedEventHandler RepoUpdated; // when repo.xml is downloaded
        internal event AsyncCompletedEventHandler ComponentUpdated; // when any component is downloaded

        private readonly object _lock = new object();
        private readonly WebClient _componentDownloader;

        // get repo.xml from update repository
        internal void CheckRemoteForUpdates()
        {
            lock (_lock)
            {
                if (Checking) return;
                Checking = true;
            }
            string tmpFile = Path.GetTempFileName();
            var webClient = new ImpatientWebClient { BaseAddress = "http://140.176.10.7:8000/cintron/BioInfoTerminal.git" };
            webClient.DownloadFileCompleted += (obj, args) =>
            {
                lock (_lock)
                    Checking = false;
                try
                {
                    CheckStoreForUpdates(tmpFile);
                }
                catch (Exception)
                {
                    const string msg = "Error while checking repo for updates.";
                }
                finally
                {
                    File.Delete(tmpFile);
                }

                RepoUpdated?.Invoke(obj, args);
            };
            Uri repoXmlUri = new Uri("repo.xml", UriKind.Relative);
            webClient.ImpatientAsyncDownload(repoXmlUri, tmpFile);
        }

        // after downloading the RepoStore, get a component from it
        public void InstallOrUpdateComponent(string componentName)
        {
            if (!Enabled) return;
            InstallOrUpdateComponent(RepoStore.Plugins.Find(c => c.Name == componentName));
        }

        // download "component.xml" for specified component
        private void InstallOrUpdateComponent(ComponentReference comp)
        {
            string tmpFile = Path.GetTempFileName();
            var webClient = new ImpatientWebClient { BaseAddress = "http://140.176.10.7:8000/cintron/BioInfoTerminal.git"};
            webClient.DownloadFileCompleted += (obj, args) =>
            {
                InstallComponentFromRecord(tmpFile, comp);
                File.Delete(tmpFile);
            };
            Uri compXmlUri = new Uri("component.xml", UriKind.Relative);
            webClient.ImpatientAsyncDownload(compXmlUri, tmpFile);
        }

        private void InstallComponentFromRecord(string tmpRecord, ComponentReference comp)
        {
            Uri compXmlUri = new Uri("component.xml", UriKind.Relative);
            ComponentRecord compRecord = ComponentRecord.Load(tmpRecord);
            var versionRecord = compRecord.Versions.Find(v => v.Version == comp.LatestVersion);
            _componentDownloader.DownloadFileAsync(compXmlUri, tmpRecord);
        }


        // deserialize downloaded repo.xml and check if it has updates
        private void CheckStoreForUpdates(string tmpStore)
        {
            RepoStore = RepoStore.Load(tmpStore);
            DateTime lastChecked = RepoStore.Lastchecked;
            DateTime lastUpdated = RepoStore.LastUpdated;
            if (lastChecked ==null || lastUpdated > lastChecked) UpdatesAvailable = true;

            //Add-check version for updates----
            //-------------------------------

            RepoStore.Lastchecked = DateTime.Now;
        }
    }

    #region Updater Classes

    public class RepoStore
    {
        public List<ComponentReference> Plugins;// all currently stored components 

        public DateTime Lastchecked; //last time the repo was checked

        public DateTime LastUpdated; // last time a component was added or updated in the repo.
        // used to prevent unnecessary deep checks to component files

        public static RepoStore Load(string fileName)
        {
            var serializer = new XmlSerializer(typeof(RepoStore));
            using (var fis = new FileStream(fileName, FileMode.Open))
                return (RepoStore)serializer.Deserialize(fis);
        }

        internal static void Save(string fileName, RepoStore store)
        {
            var serializer = new XmlSerializer(typeof(RepoStore));
            using (var fis = new FileStream(fileName, FileMode.Create))
                serializer.Serialize(fis, store);
        }
    }

    // within repo.xml, a reference to a component residing in another folder (or elsewhere)
    public class ComponentReference
    {
        public string Name;

        public string LatestVersion; // latest release/public version. not necessarily highest number in VersionRecords below

        public string ComponentRecordPath; // path to folder in which "component.xml" resides
    }

    // the "component.xml" file pointed to by the ComponentReference
    public class ComponentRecord
    {
        public string Name;

        public List<VersionRecord> Versions;

        public static ComponentRecord Load(string fileName)
        {
            var serializer = new XmlSerializer(typeof(ComponentRecord));
            using (var fis = new FileStream(fileName, FileMode.Open))
                return (ComponentRecord)serializer.Deserialize(fis);
        }

        internal static void Save(string fileName, ComponentRecord record)
        {
            var serializer = new XmlSerializer(typeof(ComponentRecord));
            using (var fis = new FileStream(fileName, FileMode.Create))
                serializer.Serialize(fis, record);
        }
    }

    public class VersionRecord
    {
        public string Version;

        public string ComponentFile; // main file for component - goes in "plugins" folder for plugins, or root folder for core

        public List<string> DependencyFiles; // libs, always go in "lib" folder
    }

    #endregion

    #region Web Interaction

    internal class ImpatientWebClient : WebClient
    {
        private const int WaitTime = 5 * 1000; // 5 seconds

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest wr = base.GetWebRequest(address);
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
            Timer cancelTimer = new Timer();
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

    #endregion
}