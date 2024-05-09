using H.Drawing3D.Drawing;

namespace H.Drawing3D.Shape.Base
{
    public interface IPresenter2DShape3D
    {
        object GetPresenter2D();

        void UpdatePresenter2D(IPresenter2DView3D view);
    }
}