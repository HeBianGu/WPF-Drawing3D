namespace H.Drawing3D.Shape.Base
{
    public interface IMouseOverableShape3D : IShape3D
    {
        bool UseMouseOverable { get; }
        void UpdateMouseOver();
    }
}