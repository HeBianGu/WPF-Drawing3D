
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawing
{
    public interface ICameraViewport3D
    {
        Viewport3D Viewport { get; }
        ProjectionCamera Camera { get; }
        IEnumerable<Model3D> HitModel3Ds(Point position);
        IEnumerable<Visual3D> HitVisual3Ds(Point position);
    }
}