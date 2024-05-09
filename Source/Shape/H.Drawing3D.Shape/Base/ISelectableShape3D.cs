namespace H.Drawing3D.Shape.Base
{
    public interface ISelectableShape3D : IShape3D
    {
        bool UseSelectable { get; }
        void UpdateSelect();
    }
}