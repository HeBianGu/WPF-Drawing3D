// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InputBindingX.cs" company="Helix Toolkit">
//   Copyright (c) 2014 Helix Toolkit contributors
// </copyright>
// <summary>
//   Input binding supporting binding the Gezture.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using System.Windows;
using System.Windows.Input;

namespace H.Drawing3D.View.Privider
{
    /// <summary>
    /// Input binding supporting binding the Gezture.
    /// </summary>
    public class InputBindingX : InputBinding
    {
        /// <summary>
        /// Gets or sets the gesture.
        /// </summary>
        /// <value>The gesture.</value>
        public InputGesture Gezture
        {
            get => (InputGesture)this.GetValue(GeztureProperty);
            set => this.SetValue(GeztureProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="Gezture"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty GeztureProperty =
            DependencyProperty.Register("Gezture", typeof(InputGesture), typeof(InputBindingX), new UIPropertyMetadata(null, GeztureChanged));

        /// <summary>
        /// Geztures the changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void GeztureChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((InputBindingX)d).OnGeztureChanged();
        }

        /// <summary>
        /// Called when [gezture changed].
        /// </summary>
        protected virtual void OnGeztureChanged()
        {
            this.Gesture = this.Gezture;
        }
    }
}