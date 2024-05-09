using System.Windows.Media.Media3D;

namespace H.Drawing3D.Drawing.Base
{
    public class Drawing3DBase : IDrawing3D
    {

    }

    public abstract class Drawing3DBase<T> : Drawing3DBase where T : Model3D
    {
        public Drawing3DBase(T model)
        {
            this.Model = model;
        }
        public T Model { get; }
    }
}