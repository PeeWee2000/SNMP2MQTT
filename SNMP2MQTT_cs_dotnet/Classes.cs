using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNMP2MQTT_cs_dotnet
{
    public enum DeviceType
    {
        DigitalInput,
        AnalogInput,
        TemperatureInputC,
        TemperatureInputF,
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
        public int ID { get; set; }
        public string OID { get; set; }
        public virtual ChildDeviceType ChildDeviceType { get; set; }
    }    

    public class DeviceConfigurations
    {
        public int ID { get; set; }
        public string IP { get; set; }
        public string Community { get; set; }
        public string DeviceName { get; set; }
        public virtual ICollection<ChildDevice> ChildDevices { get; set; }
    }

    public class SNMPVariableBinding
    {
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string OID { get; set; }
    }

    public class SNMPConfigurations
    {
        public string DeviceName { get; set; }
        public string DeviceOID { get; set; }
        public string DeviceID { get; set; }
        public virtual ICollection<ChildDeviceType> ChildDeviceTypes { get; set; }
        public virtual ICollection<SNMPVariableBinding> SNMPVariableBindings { get; set; }
    }

    public class MQTTSettings
    { 
        public string MQTTBrokerIP { get; set; }
        public int MQTTBrokerPort { get; set; }
        public string MQTTBrokerUserName { get; set; }
        public string MQTTBrokerPassword { get; set; }
    } 
}
