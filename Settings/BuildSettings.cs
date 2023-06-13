using System.Xml.Serialization;

namespace Client.Core.Settings
{
    [XmlRoot("buildSettings")]
    public class BuildSettings
    {
        [XmlAttribute("cheatBuild")]
        public bool CheatBuild { get; set; }
        
        [XmlAttribute("autoSaveTimeOutInSeconds")]
        public int AutoSaveTimeOutInSeconds;
    }
}