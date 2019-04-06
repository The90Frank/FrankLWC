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
let gp1 = new GraphicsPath()
gp1.AddBezier(PointF(0.f,0.f),PointF(150.f,300.f),PointF(300.f,300.f),PointF(300.f,0.f))
let gp2 = new GraphicsPath()
gp2.AddRectangle(Rectangle(0,0,50,50))
let auxA = LWControl(Location = PointF(100.f,50.f), GraphicsPath = gp1, BackColor = Color.Orange)
let auxB = LWControl(Location = PointF(10.f,10.f), GraphicsPath = gp2, BackColor = Color.Wheat)

auxA.Paint.Add(
    fun e -> 
        let g = e.Graphics
        let b = Brushes.LightBlue
        g.FillRectangle(b, 0,0,350,200)
)

let mutable b = false
auxB.Paint.Add(
    fun e -> 
        let g = e.Graphics
        let mutable br = Brushes.DarkViolet
        if b then
            br <- Brushes.LemonChiffon
        g.FillRectangle(br, 30,20,200,50)
)
auxB.MouseDown.Add(
    fun e ->
        b <- not(b)
        auxB.Invalidate()
)

papper.LWControls.Add(auxA)
auxA.LWControls.Add(auxB)
auxA.Draggable <- true
auxB.Draggable <- true
c.LWControls.Add(papper)
let t = new Timer()
let mutable counter = 4;
let mutable bo = true;
t.Tick.Add(
    fun _ -> 
        auxA.Matrixs.XRotate(1.f,PointF(100.f,100.f))
        auxB.Location<-PointF( (auxB.Location.X+0.1f) % 50.f, (auxB.Location.Y+0.1f) % 50.f )
        if bo then
            auxB.Matrixs.XScale(1.1f,1.1f,PointF(25.f,25.f))
            counter <- (counter + 1)
        else
            auxB.Matrixs.XScale(0.90909f,0.90909f,PointF(25.f,25.f))
            counter <- (counter - 1)
        if counter = 0 then bo <- true
        if counter = 9 then bo <- false
        //sono tutti e 3 equivalenti
        //auxA.Invalidate()
        auxB.Invalidate()
        //papper.Invalidate()
        )
t.Interval <- 16
t.Start()
//t.Stop()
