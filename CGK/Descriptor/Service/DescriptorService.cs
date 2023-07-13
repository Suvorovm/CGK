using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace CGK.Descriptor.Service
{
    [Obsolete("Use Descriptor File Loader + DescriptorHolder")]
    public class DescriptorService
    {
        private readonly IList<object> _createdDescriptor = new List<object>();
        public void LoadDescriptor<T>(string path)
        {
            TextAsset textAsset = Resources.Load<TextAsset>(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(textAsset.text);
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
            using (TextReader textReader = new StringReader(doc.OuterXml)) {
                object descriptor = xmlSerializer.Deserialize(textReader);
                _createdDescriptor.Add(descriptor);
            }
        }

        public T GetDescriptor<T>()
        {
            object descriptor = _createdDescriptor.FirstOrDefault((s) => s is T);
            if (descriptor == null) {
                return default;
            }
            return (T) descriptor;
        }

        public List<object> GetAllDescriptors<T>()
        {
            List<object> descriptor = _createdDescriptor.Where((s) => s is T).ToList();
            return descriptor.Count == 0 ? default : descriptor;
        }
    }
}