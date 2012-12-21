<?xml version="1.0" encoding="utf-8"?>
<serviceModel xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema" name="TweeterReader.Cloud" generation="1" functional="0" release="0" Id="3b511fd1-4ede-44ed-89bf-11bc5020486d" dslVersion="1.2.0.0" xmlns="http://schemas.microsoft.com/dsltools/RDSM">
  <groups>
    <group name="TweeterReader.CloudGroup" generation="1" functional="0" release="0">
      <componentports>
        <inPort name="TwitterReader.Web:Endpoint1" protocol="http">
          <inToChannel>
            <lBChannelMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/LB:TwitterReader.Web:Endpoint1" />
          </inToChannel>
        </inPort>
      </componentports>
      <settings>
        <aCS name="TwitterReader.Web:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="">
          <maps>
            <mapMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/MapTwitterReader.Web:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </maps>
        </aCS>
        <aCS name="TwitterReader.WebInstances" defaultValue="[1,1,1]">
          <maps>
            <mapMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/MapTwitterReader.WebInstances" />
          </maps>
        </aCS>
      </settings>
      <channels>
        <lBChannel name="LB:TwitterReader.Web:Endpoint1">
          <toPorts>
            <inPortMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.Web/Endpoint1" />
          </toPorts>
        </lBChannel>
      </channels>
      <maps>
        <map name="MapTwitterReader.Web:Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" kind="Identity">
          <setting>
            <aCSMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.Web/Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" />
          </setting>
        </map>
        <map name="MapTwitterReader.WebInstances" kind="Identity">
          <setting>
            <sCSPolicyIDMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.WebInstances" />
          </setting>
        </map>
      </maps>
      <components>
        <groupHascomponents>
          <role name="TwitterReader.Web" generation="1" functional="0" release="0" software="C:\Users\Jon\SkyDrive\Projects\Git\DEMO-UsingCloudApplicationServices\source\TwitterReader\TwitterReader.Cloud\csx\Debug\roles\TwitterReader.Web" entryPoint="base\x64\WaHostBootstrapper.exe" parameters="base\x64\WaIISHost.exe " memIndex="1792" hostingEnvironment="frontendadmin" hostingEnvironmentVersion="2">
            <componentports>
              <inPort name="Endpoint1" protocol="http" portRanges="80" />
            </componentports>
            <settings>
              <aCS name="Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString" defaultValue="" />
              <aCS name="__ModelData" defaultValue="&lt;m role=&quot;TwitterReader.Web&quot; xmlns=&quot;urn:azure:m:v1&quot;&gt;&lt;r name=&quot;TwitterReader.Web&quot;&gt;&lt;e name=&quot;Endpoint1&quot; /&gt;&lt;/r&gt;&lt;/m&gt;" />
            </settings>
            <resourcereferences>
              <resourceReference name="DiagnosticStore" defaultAmount="[4096,4096,4096]" defaultSticky="true" kind="Directory" />
              <resourceReference name="localStore" defaultAmount="[1000,1000,1000]" defaultSticky="true" kind="Directory" />
              <resourceReference name="EventStore" defaultAmount="[1000,1000,1000]" defaultSticky="false" kind="LogStore" />
            </resourcereferences>
          </role>
          <sCSPolicy>
            <sCSPolicyIDMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.WebInstances" />
            <sCSPolicyUpdateDomainMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.WebUpgradeDomains" />
            <sCSPolicyFaultDomainMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.WebFaultDomains" />
          </sCSPolicy>
        </groupHascomponents>
      </components>
      <sCSPolicy>
        <sCSPolicyUpdateDomain name="TwitterReader.WebUpgradeDomains" defaultPolicy="[5,5,5]" />
        <sCSPolicyFaultDomain name="TwitterReader.WebFaultDomains" defaultPolicy="[2,2,2]" />
        <sCSPolicyID name="TwitterReader.WebInstances" defaultPolicy="[1,1,1]" />
      </sCSPolicy>
    </group>
  </groups>
  <implements>
    <implementation Id="aae2c041-c029-4440-94ac-c7d46cf17c48" ref="Microsoft.RedDog.Contract\ServiceContract\TweeterReader.CloudContract@ServiceDefinition">
      <interfacereferences>
        <interfaceReference Id="e677d1b5-a1cb-4f5a-8795-e8037312cc43" ref="Microsoft.RedDog.Contract\Interface\TwitterReader.Web:Endpoint1@ServiceDefinition">
          <inPort>
            <inPortMoniker name="/TweeterReader.Cloud/TweeterReader.CloudGroup/TwitterReader.Web:Endpoint1" />
          </inPort>
        </interfaceReference>
      </interfacereferences>
    </implementation>
  </implements>
</serviceModel>