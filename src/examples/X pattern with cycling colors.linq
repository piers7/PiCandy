<Query Kind="FSharpProgram">
  <Reference Relative="..\bin\PiCandy.Server.OPC.dll">C:\dev\PiCandy\src\bin\PiCandy.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>PiCandy.Server.OPC</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

let writer = OpcWriter.Create("localhost", 7890);
let pixelCount = 4*4*6

module Observable =
    let Do fn seq = seq |> Observable.map (fun item -> fn item; item)

let color (r,g,b) = 
    let r,g,b = (
        r |> byte |> int,
        g |> byte |> int,
        b |> byte |> int
    )
    Color.FromArgb(255, r, g, b)

let getColor i pos = 
    (i + pos) % 255
    |> function
    | pos when pos < 85 -> 
        (pos * 3 , 255 - pos * 3 , 0)
    | pos when pos < 170 -> 
        let pos = pos - 85
        (255 - pos * 3 , 0 , pos * 3)
    | _ -> 
        let pos = pos - 170
        (0 , (pos * 3) , (255 - pos * 3))
    |> color

do
    Observable
        .Interval(TimeSpan.FromSeconds(0.10))
        .Select(fun i ->
            let i = int i
            Array.init pixelCount (fun p -> 
                match p % 2 with
                | x when x = (i / 20) % 2 -> getColor i p
                | _ -> Color.Black
            )
            |> Array.map (fun col -> col.ToArgb() |> uint32)
        )
        |> Observable.Do (fun pixels -> writer.WriteSetPixels(1uy, pixels))
        |> LINQPad.Extensions.DumpLatest
        |> ignore
