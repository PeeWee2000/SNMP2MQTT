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
        private static DeviceConfiguration CurrentDeviceConfiguration;
        private static ChildDevice CurrentChildDevice;
        private static List<MqttApplicationMessage> Messages;
        private static string SettingsPath;

        public DeviceManager()
        {
            SettingsPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\Settings.json";

            if (DeviceConfigurations == null)
            {
                string FileContents;

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
            CurrentDeviceConfiguration = DeviceConfigurations.Where(i => i.DeviceID == SNMPPayload.DeviceID).FirstOrDefault();

            Messages = new List<MqttApplicationMessage>();

            if (CurrentDeviceConfiguration != null)
            {
                for (int i = 0; i < SNMPPayload.ChildDevices.Count; i++)
                {
                    if (CurrentDeviceConfiguration.ChildDevices == null)
                    {
                        CurrentDeviceConfiguration.ChildDevices = new List<ChildDevice>();
                    }

                    int NumberOfChildDevicesWithOID = CurrentDeviceConfiguration.ChildDevices.Where(x => x.OID == SNMPPayload.ChildDevices[i].OID).Count();
                    if (NumberOfChildDevicesWithOID == 1)
                    { 
                        CurrentChildDevice = CurrentDeviceConfiguration.ChildDevices.Where(x => x.OID == SNMPPayload.ChildDevices[i].OID).Single();
                        CurrentChildDevice.Value = SNMPPayload.ChildDevices[i].Value;
                    }
                    else
                    {
                        CurrentChildDevice = SNMPPayload.ChildDevices[i];
                    }



                    if (CurrentDeviceConfiguration.ChildDevices.Contains(CurrentChildDevice)
                        && NumberOfChildDevicesWithOID == 1
                        && CurrentChildDevice.MQTTTopic != null)
                    {
                        var Message = new MqttApplicationMessage
                        {
                            Topic = CurrentChildDevice.MQTTTopic,
                            Payload = Encoding.UTF8.GetBytes(CurrentChildDevice.MQTTMessagePrefix + CurrentChildDevice.Value + CurrentChildDevice.MQTTMessageSuffix)
                            // UTF8 is preferred for MQTT messaging https://www.hivemq.com/blog/mqtt-essentials-part-5-mqtt-topics-best-practices/
                        };
                      
                        Messages.Add(Message);
                    }
                    else if (NumberOfChildDevicesWithOID > 1)
                    {
                        WarnOfDuplicateOIDs();
                    }
                    else if (NumberOfChildDevicesWithOID == 1 && CurrentChildDevice.MQTTTopic == null)
                    {
                        WarnOfUnconfiguredChildDevice();
                    }
                    else //Note this condition is only hit because the others are not, meaning this criteria is not guaranteed to be specific to the circumstance
                    {
                        AddNewChildDeviceAndNotify();
                    }
                }

                return Messages;
            }
            //else if (CurrentDeviceConfiguration != null && CurrentDeviceConfiguration.DeviceName == null)
            //{
            //    WarnOfUnconfiguredParentDevice();
            //    return null;
            //}
            else
            {
                AddNewParentDeviceAndNotify(SNMPPayload);
                return null;
            }
        }


        private static void WarnOfDuplicateOIDs()
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Warning message will not be converted -- duplicate OIDs found for device: " + CurrentDeviceConfiguration.DeviceName);
            foreach (var OID in CurrentDeviceConfiguration.ChildDevices.Where(i => i.OID == CurrentChildDevice.OID).Select(i => i.OID))
            {
                Console.WriteLine(OID);
            }
            Console.WriteLine("Consider deleting duplicate devices from settings to correct this or changing/correcting the OID(s)");
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void WarnOfUnconfiguredChildDevice()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning message will not be converted -- Child device not fully configured: " + CurrentDeviceConfiguration.DeviceName + " - " + CurrentChildDevice.OID); ;
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        private static void AddNewChildDeviceAndNotify()
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("New child device detected -- adding to " + SettingsPath);
            Console.WriteLine("Please fill out empty settings and restart the program once you are done adding new devices");
            Console.ForegroundColor = ConsoleColor.Gray;

            CurrentDeviceConfiguration.ChildDevices.Add(CurrentChildDevice);
            //DeviceConfigurations.Add(CurrentDeviceConfiguration);


            string JSONContents = File.ReadAllText(SettingsPath);
            JObject AllSettings = JObject.Parse(JSONContents);
            var DeviceSettingsJSON = AllSettings[nameof(DeviceConfiguration)].ToString();
            DeviceConfigurations = JsonConvert.DeserializeObject<List<DeviceConfiguration>>(DeviceSettingsJSON);

            if (DeviceConfigurations
                .Where(i => i.DeviceIP == CurrentDeviceConfiguration.DeviceIP && i.DeviceID == CurrentDeviceConfiguration.DeviceID)
                .SingleOrDefault()
                .ChildDevices == null)
            {
                DeviceConfigurations
                .Where(i => i.DeviceIP == CurrentDeviceConfiguration.DeviceIP && i.DeviceID == CurrentDeviceConfiguration.DeviceID)
                .SingleOrDefault()
                .ChildDevices = CurrentDeviceConfiguration.ChildDevices;
            }
            else
            {
                DeviceConfigurations.Where(i => i.DeviceIP == CurrentDeviceConfiguration.DeviceIP && i.DeviceID == CurrentDeviceConfiguration.DeviceID)
                    .Select(i => i.ChildDevices)
                    .SingleOrDefault()
                    .Add(CurrentChildDevice);
            }



            AllSettings.Remove(nameof(DeviceConfiguration));
            string DeviceConfigurationsString = JsonConvert.SerializeObject(DeviceConfigurations);
            AllSettings.Add(nameof(DeviceConfiguration), JToken.Parse(DeviceConfigurationsString));
            
            File.WriteAllText(SettingsPath, AllSettings.ToString());
        }

        //private static void WarnOfUnconfiguredParentDevice()
        //{
        //    Console.ForegroundColor = ConsoleColor.DarkYellow;
        //    Console.WriteLine("Warning message will not be converted -- Parent device not fully configured: " + CurrentDeviceConfiguration.DeviceID + " - " + CurrentDeviceConfiguration.DeviceIP); ;
        //    Console.ForegroundColor = ConsoleColor.Gray;
        //}

        private static void AddNewParentDeviceAndNotify(SNMPPayload SNMPPayload)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine("New parent device found with IP: " + SNMPPayload.DeviceIP + " &  OID: " + SNMPPayload.DeviceID);
            Console.WriteLine("Adding device to: " + SettingsPath);
            Console.ForegroundColor = ConsoleColor.Gray;


            var NewDevice = new DeviceConfiguration();
            NewDevice.DeviceIP = SNMPPayload.DeviceIP;
            NewDevice.DeviceID = SNMPPayload.DeviceID;
            NewDevice.DeviceCommunity = SNMPPayload.DeviceCommunity;

            
            string JSONContents = File.ReadAllText(SettingsPath);
            JObject AllSettings = JObject.Parse(JSONContents);
            var DeviceSettingsJSON = AllSettings[nameof(DeviceConfiguration)].ToString();
            DeviceConfigurations = JsonConvert.DeserializeObject<List<DeviceConfiguration>>(DeviceSettingsJSON);
            DeviceConfigurations.Add(NewDevice);
            string DeviceConfigurationsString = JsonConvert.SerializeObject(DeviceConfigurations);


            AllSettings.Remove(nameof(DeviceConfiguration));
            AllSettings.Add(nameof(DeviceConfiguration), JToken.Parse(DeviceConfigurationsString));

            File.WriteAllText(SettingsPath, AllSettings.ToString());
        }

    }
}
