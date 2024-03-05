using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace GameEngineEditor.utilities.Controls
{
    public enum VectorType
    { Vector2, Vector3, Vector4 }

    internal class VectorBox : Control
    {
        public static readonly DependencyProperty XProperty = DependencyProperty.Register(
                                            nameof(X),                 // The name of the property
                                            typeof(string),                // The type of the property
                                            typeof(VectorBox),             // The type of the owner class (custom control)
                                            new FrameworkPropertyMetadata(
                                            null,                       // The default value of the property
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));  // Metadata options)
        public string X
        {
            get => (string)GetValue(XProperty);
            set => SetValue(XProperty, value);
        }

        public static readonly DependencyProperty YProperty = DependencyProperty.Register(
                                            nameof(Y),                 // The name of the property
                                            typeof(string),                // The type of the property
                                            typeof(VectorBox),             // The type of the owner class (custom control)
                                            new FrameworkPropertyMetadata(
                                            null,                       // The default value of the property
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));  // Metadata options)
        public string Y
        {
            get => (string)GetValue(YProperty);
            set => SetValue(YProperty, value);
        }

        public static readonly DependencyProperty ZProperty = DependencyProperty.Register(
                                            nameof(Z),                 // The name of the property
                                            typeof(string),                // The type of the property
                                            typeof(VectorBox),             // The type of the owner class (custom control)
                                            new FrameworkPropertyMetadata(
                                            null,                       // The default value of the property
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));  // Metadata options)
        public string Z
        {
            get => (string)GetValue(ZProperty);
            set => SetValue(ZProperty, value);
        }

        public static readonly DependencyProperty WProperty = DependencyProperty.Register(
                                            nameof(W),                 // The name of the property
                                            typeof(string),                // The type of the property
                                            typeof(VectorBox),             // The type of the owner class (custom control)
                                            new FrameworkPropertyMetadata(
                                            null,                       // The default value of the property
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));  // Metadata options)
        public string W
        {
            get => (string)GetValue(WProperty);
            set => SetValue(WProperty, value);
        }


        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(
                                            nameof(Multiplier),                 // The name of the property
                                            typeof(double),                // The type of the property
                                            typeof(VectorBox),              // The type of the owner class (custom control)
                                            new PropertyMetadata(1.0));  // Metadata options)
        public double Multiplier
        {
            get => (double)GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }

        public static readonly DependencyProperty VectorTypeProperty = DependencyProperty.Register(
                                            nameof(VectorType),                 // The name of the property
                                            typeof(VectorType),                // The type of the property
                                            typeof(VectorBox),              // The type of the owner class (custom control)
                                            new PropertyMetadata(VectorType.Vector3));  // Metadata options)
        public VectorType VectorType
        {
            get => (VectorType)GetValue(VectorTypeProperty);
            set => SetValue(VectorTypeProperty, value);
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
                                            nameof(Orientation),                 // The name of the property
                                            typeof(Orientation),                // The type of the property
                                            typeof(VectorBox),              // The type of the owner class (custom control)
                                            new PropertyMetadata(Orientation.Horizontal));  // Metadata options)
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        static VectorBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VectorBox), new FrameworkPropertyMetadata(typeof(VectorBox)));
        }
    }
}
