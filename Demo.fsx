open System.Drawing.Drawing2D
#load "FrankLWC.fsx"
open FrankLWC
open System.Windows.Forms
open System.Drawing
let f = new Form(Size = Size(500,500))
f.Show()

let auxC = new LWContainer(Location = Point(0,0), Dock = DockStyle.Fill)
auxC.BackColor <- Color.Brown

let gp = new GraphicsPath()
gp.AddBezier(PointF(0.f,0.f),PointF(0.f,300.f),PointF(300.f,300.f),PointF(0.f,0.f))
let reg = new Region(gp)
let auxA = new LWControl(Location = PointF(100.f,50.f), Region = reg)
let auxB = new LWControl(Location = PointF(10.f,10.f), Region = new Region(new Rectangle(0,0,50,50)))

auxA.Paint.Add(
    fun e -> 
        let g = e.Graphics
        let b = Brushes.LightBlue
        g.FillRectangle(b, 0,0,900,900)
)

let mutable b = false
auxB.Paint.Add(
    fun e -> 
        let g = e.Graphics
        let mutable br = Brushes.DarkViolet
        if b then
            br <- Brushes.LemonChiffon
        g.FillRectangle(br, 0,0,1000,1000)
)
auxB.MouseDown.Add(
    fun e ->
        b <- not(b)
        auxC.Invalidate()
)

f.Controls.Add(auxC)
auxC.LWControls.Add(auxA)
auxA.LWControls.Add(auxB)
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
        (*sono tutti e 3 equivalenti*)
        //auxA.Invalidate()
        auxB.Invalidate()
        //auxC.Invalidate()
        )
t.Interval <- 16
t.Start()
