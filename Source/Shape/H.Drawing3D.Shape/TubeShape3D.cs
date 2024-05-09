namespace H.Drawing3D.Shape
{
    //[DisplayName("箭头")]
    //public class TubeShape3D : GeometryShape3DBase
    //{
    //    public int ThetaDiv { get; set; } = 36;
    //    public bool AddCaps { get; set; } = false;
    //    public double Diameter { get; set; } = 1.0;
    //    protected override Geometry3D Geometry3D => this.Tessellate();

    //    public Point3DCollection Path { get; set; } = new Point3DCollection();

    //    protected void OnSectionChanged()
    //    {
    //        var pc = new PointCollection();
    //        var circle = MeshBuilder.GetCircle(this.ThetaDiv);

    //        // If Diameters is not set, create a unit circle
    //        // otherwise, create a circle with the specified diameter
    //        double r = this.Diameters != null ? 1 : this.Diameter / 2;
    //        for (int j = 0; j < this.ThetaDiv; j++)
    //        {
    //            pc.Add(new Point(circle[j].X * r, circle[j].Y * r));
    //        }

    //        this.Section = pc;

    //        this.OnGeometryChanged();
    //    }

    //    protected MeshGeometry3D Tessellate()
    //    {
    //        if (this.Path == null || this.Path.Count < 2)
    //        {
    //            return null;
    //        }

    //        // See also "The GLE Tubing and Extrusion Library":
    //        // http://linas.org/gle/
    //        // http://sharpmap.codeplex.com/Thread/View.aspx?ThreadId=18864
    //        var builder = new MeshBuilder(false, this.TextureCoordinates != null);

    //        var sectionXAxis = this.SectionXAxis;
    //        if (sectionXAxis.Length < 1e-6)
    //        {
    //            sectionXAxis = new Vector3D(1, 0, 0);
    //        }

    //        var forward = this.Path[1] - this.Path[0];
    //        var up = Vector3D.CrossProduct(forward, sectionXAxis);
    //        if (up.LengthSquared < 1e-6)
    //        {
    //            sectionXAxis = forward.FindAnyPerpendicular();
    //        }

    //        builder.AddTube(
    //            this.Path,
    //            this.Angles,
    //            this.TextureCoordinates,
    //            this.Diameters,
    //            this.Section,
    //            sectionXAxis,
    //            this.IsPathClosed,
    //            this.IsSectionClosed, this.AddCaps, this.AddCaps);

    //        return builder.ToMesh();
    //    }
    //}

}