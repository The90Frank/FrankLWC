open System.Windows.Forms
open System.Drawing
open System.Drawing.Drawing2D
open System

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

type LWControl() =

    let mutable location = PointF(0.f, 0.f)
    let mutable region = new Region()
    let mutable parent : LWContainer = Unchecked.defaultof<LWContainer>
    let lwcontrols = ResizeArray<LWControl>() //ogni LWC è container a sua volta
    let mutable matrixs = TransformMatrixs()
    let mutable select = false
    let HitTest (p:PointF) (r:Region) =
        r.IsVisible(p)

    let mousedownevt = new Event<MouseEventArgs>()
    let mousemoveevt = new Event<MouseEventArgs>()
    let mouseupevt = new Event<MouseEventArgs>()
    let keydownevt = new Event<KeyEventArgs>()
    let keyupevt = new Event<KeyEventArgs>()
    let keypressevt = new Event<KeyPressEventArgs>()
    let resizeevt = new Event<System.EventArgs>()
    let paintevt = new Event<PaintEventArgs>()

    let transformPoint (m:Drawing2D.Matrix) (p:PointF) =
        let pts = [| p |]
        m.TransformPoints(pts)
        pts.[0]
    
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

    member this.Parent 
        with get() = parent
        and set(v:LWContainer) = parent <- v
    
    member this.LWControls with get() = lwcontrols

    member this.MouseDown = mousedownevt.Publish
    member this.MouseMove = mousemoveevt.Publish
    member this.MouseUp = mouseupevt.Publish
    member this.KeyDown = keydownevt.Publish
    member this.KeyUp = keyupevt.Publish
    member this.KeyPress = keypressevt.Publish
    member this.Resize = resizeevt.Publish
    member this.Paint = paintevt.Publish

    abstract OnMouseDown : MouseEventArgs -> unit
    default this.OnMouseDown e = 
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
        mousedownevt.Trigger(e)

    abstract OnMouseMove : MouseEventArgs -> unit
    default this.OnMouseMove e = 
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
        mousemoveevt.Trigger(e)

    abstract OnMouseUp : MouseEventArgs -> unit
    default this.OnMouseUp e = 
        let p = PointF(single e.X, single e.Y)
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnMouseUp(e)
        mouseupevt.Trigger(e)

    abstract OnKeyDown : KeyEventArgs -> unit
    default this.OnKeyDown e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyDown(e)
        | None -> ()
        keydownevt.Trigger(e)

    abstract OnKeyUp : KeyEventArgs -> unit
    default this.OnKeyUp e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyUp(e)
        | None -> ()
        keyupevt.Trigger(e)

    abstract OnKeyPress : KeyPressEventArgs -> unit
    default this.OnKeyPress e = 
        match (lwcontrols |> Seq.tryFind (fun c -> c.Select)) with
        | Some c -> c.OnKeyPress(e)
        | None -> ()
        keypressevt.Trigger(e)

    abstract OnPaint : PaintEventArgs -> unit
    default this.OnPaint e = 
        paintevt.Trigger(e)
        let g = e.Graphics
        g.SmoothingMode <- System.Drawing.Drawing2D.SmoothingMode.AntiAlias
        for idx in (lwcontrols.Count - 1) .. -1 .. 0 do
            let c = lwcontrols.[idx]
            let m = g.Transform
            m.Translate(c.Location.X,c.Location.Y)
            m.Multiply(c.Matrixs.W2V)
            g.Transform <- m
            g.SetClip(c.Region, CombineMode.Intersect)
            c.OnPaint e
            g.ResetClip()
        done
        
    abstract OnResize : System.EventArgs -> unit
    default this.OnResize e = 
        for idx in 0 .. (lwcontrols.Count - 1) do
            let c = lwcontrols.[idx]
            c.OnResize(e)
        resizeevt.Trigger(e)

    abstract Invalidate : _ -> unit
    default this.Invalidate _ = this.Parent.Invalidate()

and LWContainer() as this =
    inherit UserControl()

    let lwcontrols = ResizeArray<LWControl>()
    let HitTest (p:PointF) (r:Region) =
        r.IsVisible(p)
    
    let transformPoint (m:Drawing2D.Matrix) (p:PointF) =
        let pts = [| p |]
        m.TransformPoints(pts)
        pts.[0]
  
    do 
        this.SetStyle(ControlStyles.DoubleBuffer, true)
        this.SetStyle(ControlStyles.AllPaintingInWmPaint, true)
    done

    member this.LWControls with get() = lwcontrols

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
        for idx in (lwcontrols.Count - 1) .. -1 .. 0 do
            let c = lwcontrols.[idx]
            let m = g.Transform
            m.Translate(c.Location.X,c.Location.Y)
            m.Multiply(c.Matrixs.W2V)
            g.Transform <- m
            g.SetClip(c.Region, CombineMode.Replace)
            c.OnPaint e
            g.ResetClip()
        done
