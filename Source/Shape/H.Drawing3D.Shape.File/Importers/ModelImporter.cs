﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ModelImporter.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Imports a model from a file.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using HelixToolkit.Wpf;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace H.Drawing3D.Shape.File.Importers
{
    /// <summary>
    /// Imports a model from a file.
    /// </summary>
    public class ModelImporter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ModelImporter"/> class.
        /// </summary>
        public ModelImporter()
        {
            this.DefaultMaterial = new DiffuseMaterial(Brushes.Blue);
        }

        /// <summary>
        /// Gets or sets the default material.
        /// </summary>
        /// <value>
        /// The default material.
        /// </value>
        public Material DefaultMaterial { get; set; }

        /// <summary>
        /// Loads a model from the specified path.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <param name="dispatcher">The dispatcher used to create the model.</param>
        /// <param name="freeze">Freeze the model if set to <c>true</c>.</param>
        /// <returns>A model.</returns>
        /// <exception cref="InvalidOperationException">File format not supported.</exception>
        public Model3DGroup Load(string path, Dispatcher dispatcher = null, bool freeze = false)
        {
            if (path == null)
                return null;

            dispatcher ??= Dispatcher.CurrentDispatcher;

            Model3DGroup model;
            string ext = Path.GetExtension(path);
            if (ext != null)
                ext = ext.ToLower();

            switch (ext)
            {
                case ".3ds":
                    {
                        StudioReader r = new StudioReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".lwo":
                    {
                        LwoReader r = new LwoReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);

                        break;
                    }

                case ".obj":
                    {
                        ObjReader r = new ObjReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".objz":
                    {
                        ObjReader r = new ObjReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.ReadZ(path);
                        break;
                    }

                case ".stl":
                    {
                        StLReader r = new StLReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }

                case ".off":
                    {
                        OffReader r = new OffReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }
                case ".ply":
                    {
                        PlyReader r = new PlyReader(dispatcher) { DefaultMaterial = this.DefaultMaterial, Freeze = freeze };
                        model = r.Read(path);
                        break;
                    }
                default:
                    throw new InvalidOperationException("File format not supported.");
            }

            //if (!freeze)
            //{
            //    dispatcher.Invoke(new Action(() => model.SetName(Path.GetFileName(path))));
            //}

            return model;
        }
    }
}