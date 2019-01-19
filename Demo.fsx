#load "FrankLWC.fsx"
open FrankLWC
open System.Windows.Forms
open System.Drawing
let f = new Form(Size = Size(400,400))
f.Show()

let auxC = new LWContainer(Location = Point(0,0), Size = Size(400,400))
auxC.BackColor <- Color.Brown

let auxA = new LWControl(Location = PointF(100.f,50.f), Region = new Region(new Rectangle(0,0,200,200)))
let auxB = new LWControl(Location = PointF(10.f,10.f), Region = new Region(new Rectangle(0,0,50,50)))

auxA.Paint.Add(
    fun e -> 
        let g = e.Graphics
        let b = Brushes.LightBlue
        g.FillEllipse(b, 0,0,200,200)
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
t.Tick.Add(
    fun _ -> 
        auxA.Matrixs.XRotate(1.f,PointF(100.f,100.f))
        auxB.Location<-PointF( (auxB.Location.X+0.1f) % 20.f, (auxB.Location.Y+0.1f) % 20.f )
        auxC.Invalidate()
        )
t.Interval <- 10
t.Start()