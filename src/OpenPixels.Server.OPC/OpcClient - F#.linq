<Query Kind="FSharpProgram">
  <Reference Relative="bin\Debug\OpenPixels.Server.dll">C:\dev\github\openpixels.net\src\OpenPixels.Server.OPC\bin\Debug\OpenPixels.Server.dll</Reference>
  <Reference Relative="bin\Debug\OpenPixels.Server.OPC.dll">C:\dev\github\openpixels.net\src\OpenPixels.Server.OPC\bin\Debug\OpenPixels.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>OpenPixels.Server</Namespace>
  <Namespace>OpenPixels.Server.OPC</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

let pixels = 8*8;
let client = OpcWriter.CreateAsync("192.168.0.119", 7890)

let data = 