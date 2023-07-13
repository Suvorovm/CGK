using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace CGK.Descriptor.Service
{
    public class DescriptorFileLoader
    {
        public T LoadDescriptor<T>(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(textAsset.text);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            object descriptor;
            using (TextReader textReader = new StringReader(doc.OuterXml)) {
                descriptor = xmlSerializer.Deserialize(textReader);
            }

            return (T) descriptor;
        }

    }
}