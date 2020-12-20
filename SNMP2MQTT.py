#System
import sys
import os
import json

#SNMP
from pysnmp.entity import engine, config
from pysnmp.carrier.asyncore.dgram import udp
from pysnmp.entity.rfc3413 import ntfrcv

#MQTT
import paho.mqtt.client as paho

#Classes currently unused -- put these in place in case the need/want to convert program to strongly typed arises
class SNMPDevice:
    def __init__(device, oid, age):
        device.ticks = ticks
        device.deviceoid = deviceoid
        device.ip = ip
        device.community = community
        device.octet = octet    
        device.payload = payload

class ParentDevice:
    def __init__(ParentDevice, ID, Name, Type, ChildDevices):
        ParentDevice.ID = ID
        ParentDevice.Name = Name
        ParentDevice.Type = Type
        ParentDevice.ChildDevices = ChildDevices

class ChildDevice:
    def __init__(ChildDevice, ID, Name, Type, MQTTName):
        ChildDevice.ID = ID
        ChildDevice.Name = Name
        ChildDevice.Type = Type
        ChildDevice.MQTTName = MQTTName

ConfigPath = os.path.join(sys.path[0], 'SNMP2MQTT.json')

with open(ConfigPath) as file:
    DataMap = json.load(file)

#MQTT Publishing Section
broker = DataMap.get('MQTTBrokerIP')
port=1883
def on_publish(client,userdata,result):             #create function for callback
    print("data published \n")
    pass
client1= paho.Client("control1")                           #create client object
client1.on_publish = on_publish                          #assign function to callback

#SNMP Trap Section
snmpEngine = engine.SnmpEngine()

TrapListenerAddress='192.168.86.229'; #Use the IP that the device sending packets would use not localhost
TrapPort=162;

print("Agent is listening SNMP Trap on "+TrapListenerAddress+" , Port : " +str(TrapPort));
print('--------------------------------------------------------------------------');
config.addTransport(
    snmpEngine,
    udp.domainName + (1,),
    udp.UdpTransport().openServerMode((TrapListenerAddress, TrapPort))
)

DeviceProfiles = list()

#Create listener for each configured device
DeviceConfigurations = DataMap['DeviceConfigurations']

for DeviceConfiguration in DeviceConfigurations:
    config.addV1System(snmpEngine, DeviceConfiguration['CommunityName'], DeviceConfiguration['CommunityName'])
    print("Added SNMP listener for " + DeviceConfiguration['DeviceName'] + " using community " + DeviceConfiguration['CommunityName'])



#Load settings for each configured device
SNMPConfigs = list(DataMap['SNMPConfigurations'])


#This function should be optimized with lambdas but works just fine as is -- would've done it myself but I'm no python expert
def cbFun(snmpEngine, stateReference, contextEngineId, contextName, VariablePairs, cbContext):
    print("Received new Trap message");

    CurrentDevice = ""
    IP = ""
    ChildDeviceID = ""
    ChildDeviceType = ""
    ChildDeviceValue = ""
    MQTTMessage = ""
    Sequence = 0
    
    for NameObject, ValueObject in VariablePairs:
        Name = NameObject.prettyPrint()
        Value = ValueObject.prettyPrint()
        print('%s = %s' % (Name, Value))

        for SNMPConfig in SNMPConfigs:            
            if (Name == SNMPConfig['IdentifierOID']) & (Value == SNMPConfig['IdentifierValue']):
                CurrentDevice = SNMPConfig['DeviceName']
                break
                
    for SNMPVariableBinding in list(SNMPConfig['SNMPVariableBindings']):
        BindingName = SNMPVariableBinding.get('Name')

        if BindingName == "IP" :
            Sequence = SNMPVariableBinding.get('Sequence')
            IP = VariablePairs[Sequence][1].prettyPrint() #Tuple 1 = Value

        elif  BindingName == "ChildDeviceType" :
            Sequence = SNMPVariableBinding.get('Sequence')
            ChildDeviceTypeOctet = VariablePairs[Sequence][1].prettyPrint() #Tuple 1 = Value

            for ChildDeviceTypePair in list(SNMPConfig['ChildDeviceTypes']) :
                if ChildDeviceTypePair.get('Value') == ChildDeviceTypeOctet :
                    ChildDeviceType = ChildDeviceTypePair.get('Type')
                    break

        elif  BindingName == "ChildDeviceID" :
            Sequence = SNMPVariableBinding.get('Sequence')

        elif  BindingName == "ChildDeviceValue" :
            Sequence = SNMPVariableBinding.get('Sequence')
            ChildDeviceID = VariablePairs[Sequence][0].prettyPrint() #Tuple 0 = Name
            ChildDeviceValue = VariablePairs[Sequence][1].prettyPrint() #Tuple 1 = Value

    #for DeviceConfig in DeviceConfigurations :

    client1.connect(broker,port)                             
    client1.publish("Wewo", CurrentDevice + " @ IP:" + IP + " " + ChildDeviceType + " " + ChildDeviceID + " = " + ChildDeviceValue)

ntfrcv.NotificationReceiver(snmpEngine, cbFun)

snmpEngine.transportDispatcher.jobStarted(1)


try:
    snmpEngine.transportDispatcher.runDispatcher()
except:
    snmpEngine.transportDispatcher.closeDispatcher()
    raise





