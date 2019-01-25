open System.Drawing
#load "FrankLWC.fsx"
open FrankLWC
open System.Windows.Forms

////////////////////////////
type LWCButton() as this = 
    inherit LWControl()

    let mutable s = ""
    let mutable b = Brushes.Black
    let mutable p = PointF(single 0,single 0)
    let mutable f = new Font("Arial Black", 12.f)
    let mutable press = false

    do
        this.Paint.Add(fun e ->
            let g = e.Graphics
            ()//tutto il disegno di sfondo del bottone, non necessario in questo caso
            //[!] da usare da modello per evitare di fare override ed alterare la struttura degli LWC
        )
        this.BackColor <- Color.DarkGray
    
    member this.TextString
        with get() = s
        and set(v) = s <- v

    member this.TextBrush
        with get() = b
        and set(v) = b <- v

    member this.TextPoint
        with get() = p
        and set(v) = p <- v

    member this.TextFont
        with get() = f
        and set(v) = f <- v

    member this.Press
        with get() = press
        and set(v) = press <- v

////////////////////////////
let f = new Form(Size = Size(500,400))
f.Show()

let c = new LWContainer(Location = Point(0,0), Dock = DockStyle.Fill)
f.Controls.Add(c)
let bar1 = [|"+";"-";"L";"R";"▲";"▼";"◄";"►"|]
let size = 20.f

for i in 0 .. (bar1.Length - 1) do
    let b = new LWCButton()
    b.TextString <- bar1.[i]
    b.Location <- PointF(size * single i,0.f)
    b.TextFont <- new Font("Arial Black", 10.f)
    b.Region <- new Region(new RectangleF(0.f,0.f,size,size))
    b.MouseDown.Add(fun e -> 
        printfn "%s" b.TextString
        b.Press <- true
        b.Invalidate()
        )
    b.MouseUp.Add(fun e ->
        b.Press <- false
        b.Invalidate()
        )
    b.Paint.Add(fun e ->
        let g = e.Graphics
        let rect = new RectangleF(1.f,1.f,size-2.f,size-2.f)
        if b.Press then
            g.FillRectangle(Brushes.SlateGray,rect)
        else
            g.FillRectangle(Brushes.DimGray,rect)
        g.DrawString(b.TextString,b.TextFont,b.TextBrush,b.TextPoint)
        )
    c.LWControls.Add(b)

let papper = new LWControl(Location = PointF(50.f,50.f), Region = new Region(new Rectangle(0,0,300,200)), BackColor = Color.White)
c.LWControls.Add(papper)
