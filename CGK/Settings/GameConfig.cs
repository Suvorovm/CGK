using System.Xml.Serialization;

namespace CGK.Settings
{
    [XmlRoot("gameConfig")]
    public class GameConfig
    {
        [XmlElement("buildSettings")]
        public BuildSettings BuildSettings;
    }
}