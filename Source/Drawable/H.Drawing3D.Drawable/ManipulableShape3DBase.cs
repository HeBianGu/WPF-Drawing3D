using H.Drawing3D.Drawable.Manipulators;
using H.Drawing3D.Shape.Base;
using System.Collections.Generic;

namespace H.Drawing3D.Drawable
{
    public interface IManipulableShape3D
    {
        IEnumerable<IManipulator> GetManipulators();
    }

    public abstract class ManipulableShape3DBase : MeshGeometryShape3DBase, IManipulableShape3D
    {
        public virtual IEnumerable<IManipulator> GetManipulators()
        {
            return null;
        }
    }
}