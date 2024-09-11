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
        public static DescriptorCollection<T> LoadDescriptorCollection<T>(string filePath, string rootElementName,
            string collectionElementName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(filePath);

            if (textAsset == null)
            {
                throw new FileNotFoundException($"File not found at path: {filePath}");
            }

            var xmlOverrides = new XmlAttributeOverrides();
            var attributes = new XmlAttributes { XmlElements = { new XmlElementAttribute(collectionElementName) } };
            xmlOverrides.Add(typeof(DescriptorCollection<T>), "Collection", attributes);

            var serializer = new XmlSerializer(typeof(DescriptorCollection<T>), xmlOverrides, null,
                new XmlRootAttribute(rootElementName), null);

            using (var reader = new StringReader(textAsset.text))
            {
                var result = (DescriptorCollection<T>) serializer.Deserialize(reader);
                return result;
            }
        }
    }
}