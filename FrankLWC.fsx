open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D

type TransformMatrixs() =
    let mutable w2v = new Drawing2D.Matrix()
    let mutable v2w = new Drawing2D.Matrix()

    member this.NTranslate(tx, ty) =
        w2v.Translate(tx, ty)
        v2w.Translate(-tx, -ty, Drawing2D.MatrixOrder.Append)

    member this.NRotate(a) =
        w2v.Rotate(a)
        v2w.Rotate(-a, Drawing2D.MatrixOrder.Append)

    member this.NScale(sx, sy) =
        w2v.Scale(sx, sy)
        v2w.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)
  
    member this.XTranslate(tx, ty) =
        w2v.Translate(tx, ty, Drawing2D.MatrixOrder.Append)
        v2w.Translate(-tx, -ty)

    member this.XRotate(a, p:PointF) =
        w2v.RotateAt(a, p)
        v2w.RotateAt(-a, p, MatrixOrder.Append)

    member this.XScale(sx, sy, p:PointF) = 
        let xc, yc = p.X, p.Y
            
        w2v.Translate(xc,yc)
        v2w.Translate(-xc,-yc,MatrixOrder.Append)

        w2v.Scale(sx, sy)
        v2w.Scale(1.f/sx, 1.f/sy, Drawing2D.MatrixOrder.Append)
            
        w2v.Translate(-xc,-yc)
        v2w.Translate(xc,yc,MatrixOrder.Append) 

    member this.W2V with get() = w2v.Clone()
    member this.V2W with get() = v2w.Clone()

type AbstractLWControl() =
    let mutable location = PointF(0.f, 0.f)
    let mutable region = new Region()
    let mutable matrixs = TransformMatrixs()
    let mutable select = false
    let mutable color = Color.LightGray
    let mousedownevt = new Event<MouseEventArgs>()
    let mousemoveevt = new Event<MouseEventArgs>()
    let mouseupevt = new Event<MouseEventArgs>()
    let keydownevt = new Event<KeyEventArgs>()
    let keyupevt = new Event<KeyEventArgs>()
    let keypressevt = new Event<KeyPressEventArgs>()
    let resizeevt = new Event<System.EventArgs>()
    let paintevt = new Event<PaintEventArgs>()
    let invalidevt = new Event<obj>()

    member this.BackColor 
        with get() = color
        and set(v:Color) = color <- v
    member this.Matrixs 
        with get() = matrixs
        and set(v:TransformMatrixs) = matrixs <- v
    member this.Region 
        with get() = region.Clone()
        and set(v:Region) = region <- v.Clone()
    member this.Select
        with get() = select
        and set(v:bool) = select <- v
    member this.Location
        with get() = location
        and set(v:PointF) = location <- v

    abstract OnMouseDown : MouseEventArgs -> unit
    abstract OnMouseUp : MouseEventArgs -> unit
    abstract OnMouseMove : MouseEventArgs -> unit
    abstract OnKeyDown : KeyEventArgs -> unit
    abstract OnKeyUp : KeyEventArgs -> unit
    abstract OnKeyPress : KeyPressEventArgs -> unit
    abstract OnPaint : PaintEventArgs -> unit
    abstract OnResize : System.EventArgs -> unit
    abstract Invalidate : _ -> unit

    member this.MouseDown = mousedownevt.Publish
    member this.MouseMove = mousemoveevt.Publish
    member this.MouseUp = mouseupevt.Publish
    member this.KeyDown = keydownevt.Publish
    member this.KeyUp = keyupevt.Publish
    member this.KeyPress = keypressevt.Publish
    member this.Resize = resizeevt.Publish
    member this.Paint = paintevt.Publish
    member this.Invalidated = invalidevt.Publish
    default this.OnMouseDown e = mousedownevt.Trigger(e)
    default this.OnMouseUp e = mouseupevt.Trigger(e)
    default this.OnMouseMove e = mousemoveevt.Trigger(e)
    default this.OnKeyDown e = keydownevt.Trigger(e)
    default this.OnKeyUp e = keyupevt.Trigger(e)
    default this.OnKeyPress e = keypressevt.Trigger(e)
    default this.OnPaint e = paintevt.Trigger(e)
    default this.OnResize e = resizeevt.Trigger(e)
    default this.Invalidate _ = invalidevt.Trigger()

type LWArray(lwcontrols : ResizeArray<AbstractLWControl>) =
    let addevt = new Event<AbstractLWControl>()
    member this.AddE = addevt.Publish
    member this.Add (x:AbstractLWControl) = 
        lwcontrols.Add(x) 
        addevt.Trigger(x)
    member this.Count = lwcontrols.Count
    //TODO: aggiungere altri member utili

type LWControl() as this =
    inherit AbstractLWControl()
    //let mutable parent : LWContainer = Unchecked.defaultof<LWContainer> // non più necessario, lo tengo per ricordo
    let lwcontrols = ResizeArray<AbstractLWControl>() //ogni LWC è container a sua volta
    let publicarray = new LWArray(lwcontrols)
    let HitTest (p:PointF) (r:Region) =
        r.IsVisible(p)
    let transformPoint (m:Drawing2D.Matrix) (p:PointF) =
        let pts = [| p |]
        m.TransformPoints(pts)
        pts.[0]

    do
        publicarray.AddE.Add(fun c -> c.Invalidated.Add(fun _ -> this.Invalidate()))

    member this.LWControls with get() = publicarray
    override this.OnMouseDown e = 
        this.Select <- true
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.Select <- false
        let p = PointF(single e.X, single e.Y)
        match (lwcontrols |> Seq.tryFind (fun c -> 
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            let r = c.Region
            r.Transform(c.Matrixs.W2V)
            HitTest pp r
            )) with
        | Some c ->
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            pp <- transformPoint c.Matrixs.V2W pp
            let ee = new MouseEventArgs(MouseButtons.Left,1, int pp.X, int pp.Y,0) 
            c.OnMouseDown(ee)
        | None -> ()
        base.OnMouseDown(e)
    override this.OnMouseMove e = 
        let p = PointF(single e.X, single e.Y)
        match (lwcontrols |> Seq.tryFind (fun c -> 
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            let r = c.Region
            r.Transform(c.Matrixs.W2V)
            HitTest pp r
            )) with
        | Some c ->
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            pp <- transformPoint c.Matrixs.V2W pp
            let ee = new MouseEventArgs(MouseButtons.None,1, int pp.X, int pp.Y,0) 
            c.OnMouseMove(ee)
        | None -> ()
        base.OnMouseMove(e)
    override this.OnMouseUp e = 
        let p = PointF(single e.X, single e.Y)
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnMouseUp(e)
        base.OnMouseUp(e)
    override this.OnKeyDown e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyDown(e)
        | None -> ()
        base.OnKeyDown(e)
    override this.OnKeyUp e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyUp(e)
        | None -> ()
        base.OnKeyUp(e)
    override this.OnKeyPress e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyPress(e)
        | None -> ()
        base.OnKeyPress(e)
    override this.OnPaint e = 
        let g = e.Graphics
        let br = new SolidBrush(this.BackColor)
        g.FillRegion(br, this.Region)
        base.OnPaint(e)
        let auxm = g.Transform
        for idx in (lwcontrols.Count - 1) .. -1 .. 0 do
            let c = lwcontrols.[idx]
            let m = g.Transform
            m.Translate(c.Location.X,c.Location.Y)
            m.Multiply(c.Matrixs.W2V)
            g.Transform <- m
            g.SetClip(c.Region, CombineMode.Intersect)
            c.OnPaint e
            g.ResetClip()
            g.Transform <- auxm
        done   
    override this.OnResize e = 
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnResize(e)
        base.OnResize(e)
    override this.Invalidate _ = base.Invalidate()

and LWContainer() as this =
    inherit UserControl()

    let lwcontrols = ResizeArray<AbstractLWControl>()
    let publicarray = new LWArray(lwcontrols)
    let HitTest (p:PointF) (r:Region) =
        r.IsVisible(p)
    let transformPoint (m:Drawing2D.Matrix) (p:PointF) =
        let pts = [| p |]
        m.TransformPoints(pts)
        pts.[0]
    do 
        this.SetStyle(ControlStyles.DoubleBuffer, true)
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true)
        publicarray.AddE.Add(fun c -> c.Invalidated.Add(fun _ -> this.Invalidate()))

    member this.LWControls with get() = publicarray
    override this.OnResize e =
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnResize(e)   
    override this.OnKeyDown e =
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyDown(e)
        | None -> ()
    override this.OnKeyUp e =
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyUp(e)
        | None -> ()
    override this.OnKeyPress e =
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyPress(e)
        | None -> ()   
    override this.OnMouseDown e =
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.Select <- false
        let p = PointF(single e.X, single e.Y)
        match (lwcontrols |> Seq.tryFind (fun c -> 
            let pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            let r = c.Region
            r.Transform(c.Matrixs.W2V)
            HitTest pp r
            )) with
        | Some c ->
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            pp <- transformPoint c.Matrixs.V2W pp
            let ee = new MouseEventArgs(MouseButtons.Left,1, int pp.X, int pp.Y,0) 
            c.OnMouseDown(ee)
        | None -> ()
    override this.OnMouseUp e =
        let p = PointF(single e.X, single e.Y)
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnMouseUp(e)
    override this.OnMouseMove e =
        let p = PointF(single e.X, single e.Y)
        match (lwcontrols |> Seq.tryFind (fun c -> 
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            let r = c.Region
            r.Transform(c.Matrixs.W2V)
            HitTest pp r            
            )) with
        | Some c ->
            let mutable pp = PointF(p.X-c.Location.X,p.Y-c.Location.Y)
            pp <- transformPoint c.Matrixs.V2W pp
            let ee = new MouseEventArgs(MouseButtons.None,1, int pp.X, int pp.Y,0) 
            c.OnMouseMove(ee)
        | None -> ()
    override this.OnPaint e =
        let g = e.Graphics
        g.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        let auxm = g.Transform
        for idx in (lwcontrols.Count - 1) .. -1 .. 0 do
            let c = lwcontrols.[idx]
            let m = g.Transform
            m.Translate(c.Location.X,c.Location.Y)
            m.Multiply(c.Matrixs.W2V)
            g.Transform <- m
            g.SetClip(c.Region, CombineMode.Replace)
            c.OnPaint e
            g.ResetClip()
            g.Transform <- auxm
        done
