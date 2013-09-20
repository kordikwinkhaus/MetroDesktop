using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace MetroDesktop
{
    internal static class ResourceReader
    {
        internal static Dictionary<string, string> ReadResources(string appPath, string language)
        {
            Dictionary<string, string> resources = new Dictionary<string, string>();
            string filename = Path.Combine(appPath, language, "MetroDesktop.xml");

            try
            {
                ReadResourcesCore(resources, filename);
            }
            catch (FileNotFoundException)
            {
                string msg = string.Format("MetroDesktop: resource file '{0}' does not exist.", filename);
                throw new Exception(msg);
            }
            catch (Exception ex)
            {
                string msg = string.Format("MetroDesktop: cannot read resources from file '{0}'.\r\n{1}: {2}",
                    filename,
                    ex.GetType().ToString(),
                    ex.Message);
                throw new Exception(msg);
            }

            return resources;
        }

        private static void ReadResourcesCore(Dictionary<string, string> resources, string filename)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(filename);

            var nodes = doc.SelectNodes("//resource");
            foreach (XmlElement elem in nodes)
            {
                string key = elem.Attributes.GetNamedItem("key").Value;
                string value = elem.Attributes.GetNamedItem("value").Value;

                resources.Add(key, value);
            }
        }
    }
}
