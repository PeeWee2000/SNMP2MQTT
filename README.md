# SNMP2MQTT
A tool to help convert SNMP messages to MQTT format for use in Home Assistant and other IoT setups.

To use this program download and install the docker container.
Once installed configure the ports to whatever ports your SNMP Trap will listen on and whatever port your MQTT broker is listening on.

Once the docker container is configured and running open the logs and start sending SNMP trap packets. The program will automatically log any received packets to Settings.json where they can be configured.

Within Settings.json "Child Devices" can be configured with a topic, message prefix and message suffix. If no prefix or suffix are supplied the message will be sent with just the raw value of the Child Device. Topic is the only required field and is the minimum necessary to start sending messages to the MQTT broker.

Note that the "Value" field in Settings.json is not used when translating trap packets and just shows an example of the last received value.

![alt text](https://github.com/PeeWee2000/SNMP2MQTT/blob/master/Example.jpg?raw=true)
