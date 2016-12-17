<Query Kind="FSharpProgram">
  <Reference Relative="PiCandy.Server.OPC\bin\Debug\PiCandy.Core.dll">C:\dev\PiCandy\src\PiCandy.Server.OPC\bin\Debug\PiCandy.Core.dll</Reference>
  <Reference Relative="PiCandy.Server.OPC\bin\Debug\PiCandy.Server.OPC.dll">C:\dev\PiCandy\src\PiCandy.Server.OPC\bin\Debug\PiCandy.Server.OPC.dll</Reference>
  <NuGetReference>FSharp.Core</NuGetReference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>PiCandy.Server.OPC</Namespace>
  <Namespace>System.Net</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

// Linqpad script to view an OPC feed graphically
let endpoint = new IPEndPoint(IPAddress.Loopback, 7890)
let height = 4
let width = 4*4
let scale = 20
let zigZag = true

let toImage bytes = 
    let image = new System.Drawing.Bitmap(width*scale, height*scale)
    use graphics = System.Drawing.Graphics.FromImage(image)
    bytes
    |> Seq.chunkBySize 3
    |> Seq.where (fun x -> x.Length >= 3)
    |> Seq.iteri (fun i bytes -> 
        let color = System.Drawing.Color.FromArgb(int bytes.[0],int bytes.[1],int bytes.[2])
        // This works for columns-then-rows mapping
        let x = i / height
        let y = 
            match x % 2 with 
            | 1 when zigZag -> height - (i % height + 1) // +1 because zero-based
            | _ -> i % height
        let brush = new System.Drawing.SolidBrush(color)
        graphics.FillRectangle(brush, x*scale, y*scale, scale, scale)
    )
    image

Observable.Using(
    (fun () -> new PiCandy.Server.OPC.OpcCommandListener(endpoint)),
    (fun (listener:OpcCommandListener) -> listener.MessageReceived.Select(id))
)
|> Observable.map(fun item -> item.Data |> toImage)
|> LINQPad.Extensions.DumpLatest