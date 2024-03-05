using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameEngineEditor.utilities.Controls
{
    [TemplatePart(Name = "PART_textblock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_textbox", Type = typeof(TextBox))]
    class NumberBox : Control
    {

        private double _originalValue;
        private bool _captured = false;
        private bool _valueChanged = false;
        private double _mouseXStart;
        private double _multiplier;

        public static readonly DependencyProperty MultiplierProperty = DependencyProperty.Register(
                                            nameof(Multiplier),                 // The name of the property
                                            typeof(double),                // The type of the property
                                            typeof(NumberBox),              // The type of the owner class (custom control)
                                            new PropertyMetadata(1.0));  // Metadata options)
        public double Multiplier
        {
            get => (double)GetValue(MultiplierProperty);
            set => SetValue(MultiplierProperty, value);
        }

        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
                                            nameof(Value),                 // The name of the property
                                            typeof(string),                // The type of the property
                                            typeof(NumberBox),             // The type of the owner class (custom control)

                                            new FrameworkPropertyMetadata(
                                            null,                       
                                            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault)); // The default value of the property
        public string Value 
        {
            get => (string)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        static NumberBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NumberBox), new FrameworkPropertyMetadata(typeof(NumberBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if(GetTemplateChild("PART_textblock") is TextBlock textBlock)
            {
                textBlock.MouseLeftButtonDown += onTextBlock_Mouse_LBD;
                textBlock.MouseLeftButtonUp += onTextBlock_Mouse_LBU;
                textBlock.MouseMove += onTextBlock_Mouse_Move;
            }
        }

        private void onTextBlock_Mouse_Move(object sender, MouseEventArgs e)
        {
            if(_captured)
            {
                var mouseX = e.GetPosition(this).X;
                var d = mouseX - _mouseXStart;
                if(Math.Abs(d) > SystemParameters.MinimumHorizontalDragDistance)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) _multiplier = 0.001;
                    else if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) _multiplier = 0.1;
                    else _multiplier = 0.01;
                    var newValue = _originalValue + (d*_multiplier*Multiplier);
                    Value = newValue.ToString("0.#####");
                    _valueChanged = true;
                }
            }
        }

        private void onTextBlock_Mouse_LBU(object sender, MouseButtonEventArgs e)
        {
            if(_captured)
            {
                Mouse.Capture(null);
                _captured = false;
                e.Handled = true;
                if(!_valueChanged && GetTemplateChild("PART_textbox") is TextBox textBox)
                {
                    textBox.Visibility = Visibility.Visible;
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }
        }

        private void onTextBlock_Mouse_LBD(object sender, MouseButtonEventArgs e)
        {
            double.TryParse(Value, out _originalValue);

            Mouse.Capture(sender as UIElement);//captures the mouse, it receives mouse input even if the mouse pointer is outside the bounds of the element.
            _captured = true;
            _valueChanged = false;
            e.Handled = true;
            _mouseXStart = e.GetPosition(this).X;
            Focus();
        }
    }
}
