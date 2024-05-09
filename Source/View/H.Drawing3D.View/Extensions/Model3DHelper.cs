// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Model3DHelper.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Provides extension methods for Model3D objects.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Windows.Media.Media3D;

namespace H.Drawing3D.View.Extensions
{
    /// <summary>
    /// Provides extension methods for <see cref="Model3D"/> objects.
    /// </summary>
    public static class Model3DHelper
    {
        /// <summary>
        /// Gets the transform.
        /// </summary>
        /// <param name="current">
        /// The current.
        /// </param>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="parentTransform">
        /// The parent transform.
        /// </param>
        /// <returns>
        /// The transform.
        /// </returns>
        public static GeneralTransform3D GetTransform(this Model3D current, Model3D model, Transform3D parentTransform)
        {
            Transform3D currentTransform = Transform3DHelper.CombineTransform(current.Transform, parentTransform);
            if (ReferenceEquals(current, model))
            {
                return currentTransform;
            }

            if (current is Model3DGroup mg)
            {
                foreach (Model3D m in mg.Children)
                {
                    GeneralTransform3D result = m.GetTransform(model, currentTransform);
                    if (result != null)
                    {
                        return result;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Traverses the Model3D tree and invokes the specified action on each Model3D of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type.
        /// </typeparam>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void Traverse<T>(this Model3D model, Action<T, Transform3D> action) where T : Model3D
        {
            model.Traverse(Transform3D.Identity, action);
        }

        /// <summary>
        /// Traverses the Model3D tree and invokes the specified action on each Model3D of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type.
        /// </typeparam>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="transform">
        /// The transform.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void Traverse<T>(this Model3D model, Transform3D transform, Action<T, Transform3D> action)
            where T : Model3D
        {
            if (model is Model3DGroup mg)
            {
                Transform3D childTransform = Transform3DHelper.CombineTransform(model.Transform, transform);
                foreach (Model3D m in mg.Children)
                {
                    m.Traverse(childTransform, action);
                }
            }

            if (model is T gm)
            {
                Transform3D childTransform = Transform3DHelper.CombineTransform(model.Transform, transform);
                action(gm, childTransform);
            }
        }

        /// <summary>
        /// Traverses the Model3D tree and invokes the specified action on each Model3D of the specified type.
        /// </summary>
        /// <typeparam name="T">
        /// The type.
        /// </typeparam>
        /// <param name="model">
        /// The model.
        /// </param>
        /// <param name="visual">
        /// The visual.
        /// </param>
        /// <param name="transform">
        /// The transform.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        public static void Traverse<T>(this Model3D model, Visual3D visual, Transform3D transform, Action<T, Visual3D, Transform3D> action)
            where T : Model3D
        {
            if (model is Model3DGroup mg)
            {
                Transform3D childTransform = Transform3DHelper.CombineTransform(model.Transform, transform);
                foreach (Model3D m in mg.Children)
                {
                    m.Traverse(visual, childTransform, action);
                }
            }

            if (model is T gm)
            {
                Transform3D childTransform = Transform3DHelper.CombineTransform(model.Transform, transform);
                action(gm, visual, childTransform);
            }
        }
    }
}