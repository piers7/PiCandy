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
	var random = new Random();
	using(writer = OpcWriter.Create("pizero1", 7890)){
		uint[] pixels = new uint[60];
		int position = 0;
		while(true){
			pixels[position++] = Wheel((byte)random.Next(255));
			writer.WriteSetPixels(0, pixels);
			if(position >= 60)
				position = 0;
			Thread.Sleep(50);
		}
	}
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