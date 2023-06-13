using System.Xml.Serialization;

namespace Client.Core.Settings
{
    [XmlRoot("gameConfig")]
    public class GameConfig
    {
        [XmlElement("buildSettings")]
        public BuildSettings BuildSettings;
    }
}