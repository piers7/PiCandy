<Query Kind="Statements">
  <Reference Relative="bin\Debug\OpenPixels.Server.OPC.dll">C:\dev\github\openpixels.net\src\OpenPixels.Server.OPC\bin\Debug\OpenPixels.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>OpenPixels.Server.OPC</Namespace>
  <Namespace>System.Net</Namespace>
</Query>

using(var listener = OpcListener.Start(new System.Net.IPEndPoint(IPAddress.Any, 8022))){

	Thread.Sleep(TimeSpan.FromMinutes(1));	

}
