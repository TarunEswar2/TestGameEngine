using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GameEngineEditor.Dictionaries
{
    partial class ControlTemplates:ResourceDictionary
    {
        private void OnTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);
            if (exp == null) return;

            if(e.Key == System.Windows.Input.Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                   exp.UpdateSource(); //update binding without command
                }
                Keyboard.ClearFocus();
                e.Handled = true;
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                exp.UpdateTarget();
                Keyboard.ClearFocus();
            }
        }

        private void OnTextBoxRename_KeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);
            if (exp == null) return;

            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                    exp.UpdateSource(); //update binding without command
                }
                textBox.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }

            if (e.Key == System.Windows.Input.Key.Escape)
            {
                exp.UpdateTarget();
                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OnTextBoxRename_LostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            if (!textBox.IsVisible) return;
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);
            if (exp != null)
            {
                exp.UpdateTarget();
                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void onClose_Button_Click(object sender, RoutedEventArgs e)
        {
            var window =(Window)((FrameworkElement)sender).TemplatedParent;//templated parent od button is window
            window.Close();
        }

        private void onMaximizeRestore_Button_Click(object sender, RoutedEventArgs e)
        {
            var window =((Window)((FrameworkElement)sender).TemplatedParent);
            window.WindowState = (window.WindowState == WindowState.Normal)?WindowState.Maximized : WindowState.Normal;
        }

        private void onMinimize_Button_Click(object sender, RoutedEventArgs e)
        {
            var window = ((Window)((FrameworkElement)sender).TemplatedParent);
            window.WindowState = WindowState.Minimized;
        }

        
    }
}
