﻿using System.Collections.Generic;

namespace SNMP2MQTT_cs_dotnet
{

    public class ChildDevice
    {
        public string OID { get; set; }
        public string MQTTTopic { get; set; }
        public string MQTTMessagePrefix { get; set; }
        public string Value { get; set; }
        public string MQTTMessageSuffix { get; set; }
    }    

    public class DeviceConfiguration
    {
        public string DeviceID { get; set; } 
        public string DeviceIP { get; set; }
        public string DeviceCommunity { get; set; }
        public string DeviceName { get; set; }
        public virtual List<ChildDevice> ChildDevices { get; set; }
    }

    public class SNMPConfiguration
    {
        public string DeviceName { get; set; }
        public string DeviceEnterprise { get; set; }
    }

    public class SNMPPayload
    {
        public string DeviceIP { get; set; }
        public string DeviceID { get; set; }
        public string DeviceCommunity { get; set; }
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
