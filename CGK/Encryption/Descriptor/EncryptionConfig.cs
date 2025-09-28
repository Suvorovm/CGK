using System;
using System.Xml.Serialization;

namespace CGK.Encryption.Descriptor
{
    [Serializable]
    public class EncryptionConfig
    {
        [XmlAttribute("folderPath")]
        public string FolderPath { get; set; }
        
        [XmlAttribute("encryptionKey")]
        public string EncryptionKey { get; set; }
        
        [XmlAttribute("generatedKeyPath")]
        public string GeneratedKeyPath { get; set; }
        
        [XmlAttribute("tempBackupPath")]
        public string TempBackupPath { get; set; }
    }
}