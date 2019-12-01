using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace BioInfo_Terminal.Methods.Updater
{
    public class RepoStore
    {
        public DateTime LastUpdated; // last time a component was added or updated in the repo.

        public List<VersionRecord> Versions; // all currently stored versions
        // used to prevent unnecessary deep checks to component files

        // TODO add LatestVersion - string or ref
        public static RepoStore Load(string fileName)
        {
            var serializer = new XmlSerializer(typeof(RepoStore));
            using (var fis = new FileStream(fileName, FileMode.Open))
            {
                return (RepoStore) serializer.Deserialize(fis);
            }
        }

        internal static void Save(string fileName, RepoStore store)
        {
            var serializer = new XmlSerializer(typeof(RepoStore));
            using (var fis = new FileStream(fileName, FileMode.Create))
            {
                serializer.Serialize(fis, store);
            }
        }
    }

    public class VersionRecord
    {
        public string
            ComponentFile; // main file for component - goes in "plugins" folder for plugins, or root folder for core

        public List<string> DependencyFiles; // libs, always go in "lib" folder
        public string Version;
    }
}