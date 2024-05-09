// Copyright © 2022 By HeBianGu(QQ:908293466) https://github.com/HeBianGu/WPF-ControlBase

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace H.Drawing3D.Drawing
{
    public static partial class DispatcherExtension
    {
        private static readonly Dictionary<object, bool> _isRefreshings = new();

        public static async void DelayInvoke(this object obj, Action action, DispatcherPriority priority = DispatcherPriority.Input)
        {
            if (!_isRefreshings.ContainsKey(obj))
            {
                _isRefreshings.Add(obj, false);
            }
            bool isRefreshing = _isRefreshings[obj];

            if (isRefreshing)
            {
                return;
            }

            _isRefreshings[obj] = true;

            await Application.Current?.Dispatcher?.BeginInvoke(priority, new Action(() =>
              {
                  _isRefreshings[obj] = false;
                  action?.Invoke();
              }));
        }
    }
}
