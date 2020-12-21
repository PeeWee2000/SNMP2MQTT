using MQTTnet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SNMP2MQTT_cs_dotnet
{
    public class DeviceManager
    {
        private static List<DeviceConfiguration> DeviceConfigurations;

        public DeviceManager()
        {
            if (DeviceConfigurations == null)
            {
                string FileContents;

                string SettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.json";
                using (StreamReader FileReader = new StreamReader(SettingsPath))
                {
                    FileContents = FileReader.ReadToEnd();
                }

                JObject ProgramSettings = JObject.Parse(FileContents);

                var JSONSettings = ProgramSettings[nameof(DeviceConfiguration)].ToString();

                DeviceConfigurations = JsonConvert.DeserializeObject<List<DeviceConfiguration>>(JSONSettings);
            }
        }

        public static List<MqttApplicationMessage> ConvertPayloadToMessage(SNMPPayload SNMPPayload)
        {         
            DeviceConfiguration DeviceConfiguration = DeviceConfigurations.Where(i => i.DeviceName == SNMPPayload.SNMPConfiguration.DeviceName).FirstOrDefault();

            var Messages = new List<MqttApplicationMessage>();

            foreach (var ChildDevice in SNMPPayload.ChildDevices)
            {
                var Message = new MqttApplicationMessage();

                if (DeviceConfiguration.ChildDevices.Contains(ChildDevice) 
                    && DeviceConfiguration.ChildDevices.Where(i => i.OID == ChildDevice.OID).Count() == 1 
                    && ChildDevice.MQTTTopic != null)
                {   //Valid device settings found, convert the message
                    Message.Topic = ChildDevice.MQTTTopic;
                    Message.Payload = Encoding.UTF8.GetBytes(ChildDevice.MQTTMessagePrefix + ChildDevice.Value + ChildDevice.MQTTMessageSuffix); 
                    // UTF8 is preferred for MQTT messaging https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/

                    Messages.Add(Message);
                    Console.WriteLine("Message converted succesfully");
                }
                else if (DeviceConfiguration.ChildDevices.Where(i => i.OID == ChildDevice.OID).Count() > 1)
                {       //Duplicate OIDs found in settings
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Warning message will not be converted -- duplicate OIDs found for device: " + DeviceConfiguration.DeviceName);
                    foreach (var OID in DeviceConfiguration.ChildDevices.Where(i => i.OID == ChildDevice.OID).Select(i => i.OID))
                    {
                        Console.WriteLine(OID);
                    }
                    Console.WriteLine("Consider deleting duplicate devices from settings to correct this or changing/correcting the OID(s)");
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (DeviceConfiguration.ChildDevices.Where(i => i.OID == ChildDevice.OID).Count() == 1
                        && ChildDevice.MQTTTopic == null)
                {       //Device added by program but not configured by user
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Warning message will not be converted -- device not fully configured: " + DeviceConfiguration.DeviceName + " - " + ChildDevice.OID); ;
                    Console.ForegroundColor = ConsoleColor.Gray;
                }
                else if (DeviceConfiguration.ChildDevices.Contains(ChildDevice))
                {       //Device settings changed from SNMP side update the config
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.WriteLine("Expected SNMP payload changed for device: " + DeviceConfiguration.DeviceName + " - " + ChildDevice.OID);
                    Console.WriteLine("If this was expected ignore this message otherwise verify the internal SNMP parent device settings");
                    DeviceConfigurations.Remove(DeviceConfiguration);
                    DeviceConfiguration.ChildDevices.Add(ChildDevice);
                    DeviceConfigurations.Add(DeviceConfiguration);
                }
                else //New child device detected, add to config and wait for user input
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine("New child device detected -- adding to " + Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.json");
                    Console.WriteLine("Please fill out empty settings and restart the program once you are done adding new devices");
                    Console.ForegroundColor = ConsoleColor.Gray;

                    DeviceConfiguration.ChildDevices.Add(ChildDevice);
                    DeviceConfigurations.Add(DeviceConfiguration);
                }
            }

            return Messages;
        }

        private static void UpdateDeviceConfiguration(DeviceConfiguration DeviceConfiguration)
        {

        }
    }
}
