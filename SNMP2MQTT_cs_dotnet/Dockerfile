#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0-buster-slim AS build
WORKDIR /src
COPY ["SNMP2MQTT_cs_dotnet/SNMP2MQTT_cs_dotnet.csproj", "SNMP2MQTT_cs_dotnet/"]
RUN dotnet restore "SNMP2MQTT_cs_dotnet/SNMP2MQTT_cs_dotnet.csproj"
COPY . .
WORKDIR "/src/SNMP2MQTT_cs_dotnet"
RUN dotnet build "SNMP2MQTT_cs_dotnet.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SNMP2MQTT_cs_dotnet.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SNMP2MQTT_cs_dotnet.dll"]

EXPOSE 162/tcp
EXPOSE 162/udp
EXPOSE 1883/tcp
EXPOSE 1883/udp

ENV Test="John Doe"
ARG Wewo
LABEL Wewo="Pepo"
