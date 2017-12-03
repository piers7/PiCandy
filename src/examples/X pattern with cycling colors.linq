<Query Kind="FSharpProgram">
  <Reference Relative="..\bin\PiCandy.Server.OPC.dll">C:\dev\PiCandy\src\bin\PiCandy.Server.OPC.dll</Reference>
  <NuGetReference>Rx-Main</NuGetReference>
  <Namespace>PiCandy.Server.OPC</Namespace>
  <Namespace>System.Drawing</Namespace>
  <Namespace>System.Reactive</Namespace>
  <Namespace>System.Reactive.Linq</Namespace>
</Query>

//let writer = OpcWriter.Create("pizero3.local", 7890);
let width = 4*6
let height = 4
let pixelCount = height * width
let scale = 10
let refreshRate = TimeSpan.FromSeconds(0.10)

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
    
/// For diagnostics etc...
let toBitmap (width,height) (scale:int) zigzag pixels =
    let bitmap = new Bitmap(width * scale, height * scale)
    use graphics = Graphics.FromImage(bitmap)
    pixels 
    |> Seq.iteri (fun i c -> 
        let x = i / height
        let y =  
            match x % 2 with 
            | 1 when zigzag -> height - (i % height + 1) // +1 because zero-based
            | _ -> i % height
        let brush = new System.Drawing.SolidBrush(c)
        graphics.FillRectangle(brush, System.Drawing.Rectangle(x*scale, y*scale, scale, scale))
    )
    bitmap

let solid color = 
    Array.mapi (fun i p -> color)
    
let crosshatch offset getColor = 
    Array.mapi (fun i p -> 
        match (i + offset) % 2 with
        | 0 -> p
        | _ -> getColor i
    )

let transform t = 
    solid Color.Black
    >> crosshatch (t / 20) (getColor t)

let display = 
    toBitmap (width,height) scale true

do
    Observable
        .Interval(refreshRate)
        |> Observable.map (fun t -> Array.create pixelCount Color.Black |> transform (int t))
        |> Observable.map display
        //|> Observable.map (Seq.map (fun (c:Color) -> c.ToArgb() |> uint32) >> Seq.toArray)
        //|> Observable.Do (fun pixels -> writer.WriteSetPixels(1uy, pixels))
        |> LINQPad.Extensions.DumpLatest
        |> ignore