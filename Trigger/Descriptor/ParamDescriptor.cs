using System.Xml.Serialization;

namespace Client.Core.Condition.Descriptor
{
    [XmlRoot("param")]
    public class ParamDescriptor
    {
        [XmlAttribute("key")]
        public string ParameterName { get; set; }
        [XmlAttribute("value")]
        public string ParameterValue { get; set; }
        
    }
}