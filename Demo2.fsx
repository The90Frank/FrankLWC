open System.Drawing
open System.Drawing.Drawing2D
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
let f = new Form(Size = Size(600,400), Text = "MyForm")
f.Show()

let c = new LWContainer(Location = Point(0,0), Dock = DockStyle.Fill)
f.Controls.Add(c)

let gpP = new GraphicsPath()
gpP.AddRectangle(Rectangle(0,0,500,300))
let papper = LWControl(Location = PointF(50.f,50.f), GraphicsPath = gpP, BackColor = Color.White)

let bar1 = [|"+";"-";"L";"R";"▲";"▼";"◄";"►"|]
let fu = [|
    fun _ -> (papper.Matrixs.NScale(1.1f,1.1f); papper.Invalidate());
    fun _ -> (papper.Matrixs.NScale(0.909f,0.909f); papper.Invalidate());
    fun _ -> (papper.Matrixs.NRotate(1.f); papper.Invalidate());
    fun _ -> (papper.Matrixs.NRotate(-1.f); papper.Invalidate());
    fun _ -> (papper.Location <- PointF(papper.Location.X, papper.Location.Y + 1.f); papper.Invalidate());
    fun _ -> (papper.Location <- PointF(papper.Location.X, papper.Location.Y - 1.f); papper.Invalidate());
    fun _ -> (papper.Location <- PointF(papper.Location.X + 1.f, papper.Location.Y); papper.Invalidate());
    fun _ -> (papper.Location <- PointF(papper.Location.X - 1.f, papper.Location.Y); papper.Invalidate());
|]
let size = 20.f

for i in 0 .. (bar1.Length - 1) do
    let ti = new Timer()
    ti.Interval <- 16
    ti.Tick.Add(fu.[i])
    let b = LWCButton()
    b.TextString <- bar1.[i]
    b.Location <- PointF(size * single i,0.f)
    b.TextFont <- new Font("Arial Black", 10.f)
    let gp = new GraphicsPath()
    gp.AddRectangle(RectangleF(0.f,0.f,size,size))
    b.GraphicsPath <- gp
    b.MouseDown.Add(fun e -> 
        //printfn "%s" b.TextString
        b.Press <- true
        b.Invalidate()
        ti.Start()
        )
    b.MouseUp.Add(fun e ->
        ti.Stop()
        b.Press <- false
        b.Invalidate()
        )
    b.Paint.Add(fun e ->
        let g = e.Graphics
        let rect = RectangleF(1.f,1.f,size-2.f,size-2.f)
        if b.Press then
            g.FillRectangle(Brushes.SlateGray,rect)
        else
            g.FillRectangle(Brushes.DimGray,rect)
        g.DrawString(b.TextString,b.TextFont,b.TextBrush,b.TextPoint)
        )
    c.LWControls.Add(b)

/////////////////////////////////////////////////////////
let r = 7
let gp = new GraphicsPath()
gp.AddEllipse(Rectangle(0,0,2*r,2*r))
let p0 = LWControl(Location = PointF(20.f,20.f), GraphicsPath = gp, BackColor = Color.DarkGreen)
let p1 = LWControl(Location = PointF(120.f,20.f), GraphicsPath = gp, BackColor = Color.DarkGreen)
let p2 = LWControl(Location = PointF(20.f,120.f), GraphicsPath = gp, BackColor = Color.DarkGreen)
let p3 = LWControl(Location = PointF(120.f,120.f), GraphicsPath = gp, BackColor = Color.DarkGreen)

p0.Draggable <- true
p1.Draggable <- true
p2.Draggable <- true
p3.Draggable <- true

type LWCBezier() as this = 
    inherit LWControl()

    let mutable p = [|PointF(0.f,0.f);PointF(0.f,0.f);PointF(0.f,0.f);PointF(0.f,0.f)|]

    do
        this.Paint.Add(fun e ->
            let g = e.Graphics
            g.DrawBezier(Pens.Black,p.[0],p.[1],p.[2],p.[3])
        )

    member this.ControlPoints
        with get() = p
        and set v = if Array.length v = 4 then 
                        p <- v
                        this.Invalidate()

//andrebbe inizializzato ma non ne ho voglia
let b = LWCBezier(BackColor = papper.BackColor)

papper.MouseMove.Add(fun _ -> 
    let bezgp = new GraphicsPath()
    let pp0 = [|PointF(p0.Location.X+single r,p0.Location.Y+single r)|]
    let pp1 = [|PointF(p1.Location.X+single r,p1.Location.Y+single r)|]
    let pp2 = [|PointF(p2.Location.X+single r,p2.Location.Y+single r)|]
    let pp3 = [|PointF(p3.Location.X+single r,p3.Location.Y+single r)|]
    p0.Matrixs.W2V.TransformPoints(pp0)
    p1.Matrixs.W2V.TransformPoints(pp1)
    p2.Matrixs.W2V.TransformPoints(pp2)
    p3.Matrixs.W2V.TransformPoints(pp3)
    let ppA = [|pp0.[0];pp1.[0];pp2.[0];pp3.[0]|]
    let ppAX = [|pp0.[0].X;pp1.[0].X;pp2.[0].X;pp3.[0].X|]
    let ppAY = [|pp0.[0].Y;pp1.[0].Y;pp2.[0].Y;pp3.[0].Y|]
    let maxX = Seq.max ppAX
    let minX = Seq.min ppAX
    let maxY = Seq.max ppAY
    let minY = Seq.min ppAY
    bezgp.AddRectangle(RectangleF(minX,minY,(maxX-minX),(maxY-minY)))
    b.GraphicsPath <- bezgp
    b.ControlPoints <- ppA
    b.Invalidate()
)

/////////////////////////////////

papper.LWControls.Add(p0)
papper.LWControls.Add(p1)
papper.LWControls.Add(p2)
papper.LWControls.Add(p3)

papper.LWControls.Add(b)

c.LWControls.Add(papper)
