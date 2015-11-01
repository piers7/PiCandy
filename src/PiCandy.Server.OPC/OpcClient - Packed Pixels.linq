<Query Kind="Statements">
  <Reference Relative="bin\Debug\OpenPixels.Server.dll">C:\dev\github\openpixels.net\src\OpenPixels.Server.OPC\bin\Debug\OpenPixels.Server.dll</Reference>
  <Reference Relative="bin\Debug\OpenPixels.Server.OPC.dll">C:\dev\github\openpixels.net\src\OpenPixels.Server.OPC\bin\Debug\OpenPixels.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>OpenPixels.Server</Namespace>
  <Namespace>OpenPixels.Server.OPC</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

using(var client = await OpcWriter.CreateAsync("192.168.0.119", 7890)){
	var pixels = 8*8;
	var data = new uint[pixels];
	
	for (int i = 0; i < pixels; i++)
	{
		// hmm. Need to handle GRB conversion somewhere
		data[i] = 0x00ff00;
		client.Write(0, OpcCommandType.SetPixels, data);
		Thread.Sleep(100);
	}
}