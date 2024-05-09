
using System;
using System.Windows.Controls;

namespace H.Drawing3D.Drawing
{
    public interface IPresenter2DView3D : ICameraViewport3D
    {
        void Add(object presenter, Action<ContentPresenter> action = null);
        void Delete(object presenter);

        ContentPresenter GetContentPresenter(object presenter);

    }
}