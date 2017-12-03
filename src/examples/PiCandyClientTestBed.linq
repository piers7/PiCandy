<Query Kind="Program">
  <Reference Relative="bin\PiCandy.Server.OPC.dll">C:\dev\PiCandy\src\bin\PiCandy.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
  <Namespace>PiCandy.Server.OPC</Namespace>
</Query>

static OpcWriter writer;

void Main()
{
	using(writer = OpcWriter.Create("10.0.0.2", 7890)){
		while(true){
			Draw();
		}
	}
}

private static void Draw(){
	var offset = (Environment.TickCount / 200) % 120;
	writer.WriteSetPixels(0, 
		Enumerable
			.Range(0, 60)
			.Select(i => Wheel((byte)(i+offset)))
			.ToArray()
		);
}

//Input a value 0 to 255 to get a color value.
//The colours are a transition r - g -b - back to r
private static uint Wheel(byte pos)
{
	if (pos < 85) {
		return Color((byte)(pos * 3), (byte)(255 - pos * 3), 0);
	} else if (pos < 170) {
		pos -= 85;
		return Color((byte)(255 - pos * 3), 0, (byte)(pos * 3));
	} else {
		pos -= 170; 
		return Color(0, (byte)(pos * 3), (byte)(255 - pos * 3));
	}
}

private static uint Color(byte r, byte g, byte b){
	return ((uint)r) << 16 
		+ g << 8 
		+ b
		;
}