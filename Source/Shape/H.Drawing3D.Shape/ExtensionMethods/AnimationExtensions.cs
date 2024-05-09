// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AnimationExtensions.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides extension methods for animatable objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows;
using System.Windows.Media.Animation;

namespace H.Drawing3D.Shape.ExtensionMethods
{
    /// <summary>
    /// Provides extension methods for animatable objects.
    /// </summary>
    public static class AnimationExtensions
    {
        /// <summary>
        /// Animates the opacity of the specified object.
        /// </summary>
        /// <param name="obj">
        /// The object to animate.
        /// </param>
        /// <param name="targetOpacity">
        /// The target opacity.
        /// </param>
        /// <param name="animationTime">
        /// The animation time.
        /// </param>
        public static void AnimateOpacity(this IAnimatable obj, double targetOpacity, double animationTime)
        {
            DoubleAnimation animation = new(targetOpacity, new Duration(TimeSpan.FromMilliseconds(animationTime)))
            {
                AccelerationRatio = 0.3,
                DecelerationRatio = 0.5
            };
            obj.BeginAnimation(UIElement.OpacityProperty, animation);
        }
    }
}