using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace CGK.Descriptor.Service
{
    public class DescriptorFileLoader
    {
        public T LoadDescriptorFromString<T>(string content)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(content);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            object descriptor;
            using (TextReader textReader = new StringReader(doc.OuterXml)) {
                descriptor = xmlSerializer.Deserialize(textReader);
            }

            return (T) descriptor;
        }
        public T LoadDescriptor<T>(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            return LoadDescriptorFromString<T>(textAsset.text)
        }
        
        public DescriptorCollection<T> LoadDescriptorCollection<T>(string filePath, string rootElementName,
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