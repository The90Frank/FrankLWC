# FrankLWC

A lightweight custom controls framework built in F# for Windows Forms, supporting hierarchical composition, 2D transformations, and interactive event handling.

![demo](forreadme/demo.gif)

## Features

- **Custom lightweight controls** with arbitrary shapes (rectangles, ellipses, Bezier curves)
- **2D transformations** (translate, rotate, scale) both accumulative and non-accumulative
- **Hierarchical composition** - controls can contain other controls
- **Hit-testing** with accurate detection on transformed/rotated regions
- **Mouse and keyboard events** propagated through the control hierarchy
- **Double-buffered rendering** with anti-aliasing

## Architecture

| Class | Description |
|---|---|
| `TransformMatrixs` | Manages World-to-View and View-to-World transformation matrices |
| `AbstractLWControl` | Abstract base defining the interface for all lightweight controls |
| `LWControl` | Concrete control that can contain child controls |
| `LWContainer` | Root Windows Forms UserControl integrating LWC with WinForms |
| `LWArray` | Collection wrapper for managing multiple controls |

## Usage

Include the framework and create controls:

```fsharp
#load "FrankLWC.fsx"
open FrankLWC

let container = new LWContainer(Dock = DockStyle.Fill)
let control = new LWControl(...)
container.LWControls.Add(control)
```

See `Demo.fsx` and `Demo2.fsx` for complete interactive examples including draggable shapes, animated transformations, and a Bezier curve editor.

## Tech stack

- F# (.fsx scripts)
- Windows Forms / GDI+ / System.Drawing.Drawing2D

## License

Apache License 2.0 - see [LICENSE](LICENSE) for details.
