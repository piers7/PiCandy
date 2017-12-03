<Query Kind="FSharpExpression">
  <Namespace>System.Drawing</Namespace>
</Query>

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
    //|> color
    
let getBlock (r,g,b) (width:int,height:int) scale =
    let bitmap = new Bitmap(width * scale, height * scale)
    let color = Color.FromArgb(255,r,g,b)
    let brush = new SolidBrush(color)
    use graphics = Graphics.FromImage(bitmap)
    graphics.FillRectangle(brush, 0,0,width,height)
    bitmap
    
Seq.init 255 (getColor 0)
|> Seq.map (fun (r,g,b) -> (getBlock (r,g,b) (10,10) 1))
