using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNMP2MQTT_cs_dotnet
{
    public enum DeviceType
    {
        NotConfigured,
        DigitalInput,
        AnalogInput,
        TemperatureInputC,
        TemperatureInputF,
        TemperatureInputK,
        DigitalOutput,
        AnalogOutput,
        Other
    }

    public class ChildDeviceType
    {
        public DeviceType DeviceType { get; set; }
        public string DeviceTypeOID { get; set; }
    }

    public class ChildDevice
    {
        public string OID { get; set; }
        public DeviceType DeviceType { get; set; }
        public string DeviceTypeOID { get; set; }
        public string MQTTTopic { get; set; }
        public string MQTTMessagePrefix { get; set; }
        public string Value { get; set; }
        public string MQTTMessageSuffix { get; set; }
    }    

    public class DeviceConfiguration
    {
        public string DeviceID { get; set; } //AKA Enterprise in SNMP terms
        public string DeviceIP { get; set; }
        public string CommunityName { get; set; }
        public string DeviceName { get; set; }
        public virtual List<ChildDevice> ChildDevices { get; set; }
    }

    public class SNMPVariableBinding
    {
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string OID { get; set; }
    }

    public class SNMPConfiguration
    {
        public string DeviceName { get; set; }
        public string DeviceOID { get; set; } 
        public string DeviceID { get; set; } //AKA Enterprise in SNMP terms
        public virtual List<ChildDeviceType> ChildDeviceTypes { get; set; }
        public virtual List<SNMPVariableBinding> SNMPVariableBindings { get; set; }
    }

    public class SNMPPayload
    {
        public string DeviceIP { get; set; }
        public string DeviceID { get; set; }
        public List<ChildDevice> ChildDevices { get; set; }
    }

    public class MQTTSettings
    { 
        public string MQTTClientName { get; set; }
        public string MQTTBrokerIP { get; set; }
        public int MQTTBrokerPort { get; set; }
        public string MQTTBrokerUserName { get; set; }
        public string MQTTBrokerPassword { get; set; }
    } 
}
