// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Manipulator.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides an abstract base class for manipulators.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;

namespace H.Drawing3D.Drawable.Manipulators
{
    public interface IManipulator
    {
        event EventHandler<double> ValueChanged;
    }
}