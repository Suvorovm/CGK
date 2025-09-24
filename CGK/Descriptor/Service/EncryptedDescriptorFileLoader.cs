using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using CGK.Encryption;

namespace CGK.Descriptor.Service
{
    public class EncryptedDescriptorFileLoader : IDescriptorFileLoader
    {
        /// <summary>
        /// Load descriptor from encrypted string
        /// </summary>
        /// <param name="content">Base64 or hex</param>
        /// <typeparam name="T">Target Descriptor</typeparam>
        /// <returns></returns>
        public T LoadDescriptorFromString<T>(string content)
        {
            byte[] encryptedBytes = Convert.FromBase64String(content);
            byte[] decryptedBytes = RuntimeDecryptor.Decrypt(encryptedBytes);
            string xml = Encoding.UTF8.GetString(decryptedBytes);

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (TextReader reader = new StringReader(doc.OuterXml))
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }
        /// <summary>
        /// Load descriptor from encrypted file
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <typeparam name="T">Target Descriptor</typeparam>
        /// <returns></returns>
        public T LoadDescriptor<T>(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            if (textAsset == null)
            {
                throw new FileNotFoundException($"Encrypted file not found at path: {path}");
            }

            return LoadDescriptorFromString<T>(textAsset.text);
        }

        public DescriptorCollection<T> LoadDescriptorCollection<T>(string filePath, string rootElementName, string collectionElementName)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(filePath);
            if (textAsset == null)
            {
                throw new FileNotFoundException($"Encrypted collection not found at path: {filePath}");
            }

            byte[] decryptedBytes = RuntimeDecryptor.Decrypt(Convert.FromBase64String(textAsset.text));
            string xml = Encoding.UTF8.GetString(decryptedBytes);

            var xmlOverrides = new XmlAttributeOverrides();
            var attributes = new XmlAttributes { XmlElements = { new XmlElementAttribute(collectionElementName) } };
            xmlOverrides.Add(typeof(DescriptorCollection<T>), "Collection", attributes);

            var serializer = new XmlSerializer(typeof(DescriptorCollection<T>), xmlOverrides, null, new XmlRootAttribute(rootElementName), null);

            using (var reader = new StringReader(xml))
            {
                return (DescriptorCollection<T>)serializer.Deserialize(reader);
            }
        }
    }
}
