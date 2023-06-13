using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using Client.Core.Condition.Descriptor;

namespace Client.Core.Trigger.Descriptor
{
    [XmlRoot("trigger")]
    public class TriggerDescriptor
    {
        [XmlAttribute("conditionType")]
        public string TriggerType { get; set; }
        
        [XmlElement("param")]
        public ParamDescriptor[] ParamDescriptor { get; set; }
        [XmlIgnore]
        public Dictionary<string, string> ParametersDictionary
        {
            get { return ParamDescriptor.ToDictionary(p => p.ParameterName, q => q.ParameterValue); }
        }
    }
}