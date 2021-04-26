using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SelfUpdatingApp
{
    class XmlData
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public Version Version { get; set; }
        public string Depo { get; set; }
        public string ExeName { get; set; }
        
        public void Save(string filename)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(filename));
            new XDocument
                (
                    new XElement
                    (
                        "SelfUpdatingApp",
                        new XElement("Name", Name),
                        new XElement("Id", Id),
                        new XElement("Version", Version.ToString()),
                        new XElement("Depo", Depo),
                        new XElement("ExeName", ExeName)
                    )
                ).Save(filename);
        }

        public static async Task<XmlData> ReadAsync(string url)
        {
            XDocument doc = null;
            if (StreamHelper.IsWebUrl(url))
            {
                using var response = await StreamHelper.GetWebResponseAsync(url).ConfigureAwait(false);
                using var stream = response.GetResponseStream();

#if NETCOREAPP
                doc = await XDocument.LoadAsync(stream, LoadOptions.None, default).ConfigureAwait(false);
#else
                doc = XDocument.Load(stream, LoadOptions.None);
#endif
            }
            else
            {
                using var stream = StreamHelper.OpenAsyncRead(url);
#if NETCOREAPP
                doc = await XDocument.LoadAsync(stream, LoadOptions.None, default).ConfigureAwait(false);
#else
                doc = XDocument.Load(stream, LoadOptions.None);
#endif
            }

            return ReadDoc(doc);
        }

        public static XmlData Read(string filename) => ReadDoc(XDocument.Load(filename));
        
        private static XmlData ReadDoc(XDocument doc)
        {
            XElement root = doc.Element("SelfUpdatingApp");
            XElement name = root.Element("Name");
            XElement id = root.Element("Id");
            XElement version = root.Element("Version");
            XElement depo = root.Element("Depo");
            XElement exeName = root.Element("ExeName");

            return new XmlData
            {
                Depo = depo.Value,
                ExeName = exeName.Value,
                Id = id.Value,
                Name = name.Value,
                Version = new Version(version.Value)
            };
        }
    }
}
