using GameEngineEditor.utilities.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GameEngineEditor.Editors
{
    /// <summary>
    /// Interaction logic for ComponentView.xaml
    /// </summary>
    [ContentProperty("ComponentContent")]//everything that is typed without any specification <CompentView.ComponentContent> will be assigned to ComponentContent
    public partial class ComponentView : UserControl
    {
        public string Header
        {
            get { return (string)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
                                                nameof(Header),                 // The name of the property
                                                typeof(string),                // The type of the property
                                                typeof(ComponentView)              // The type of the owner class (custom control)
                                                );

        public FrameworkElement ComponentContent
        {
            get { return (FrameworkElement)GetValue(ComponentContentProperty); }
            set { SetValue(ComponentContentProperty, value);}
        }

        public static readonly DependencyProperty ComponentContentProperty = DependencyProperty.Register(
                                                nameof(ComponentContent),                 // The name of the property
                                                typeof(FrameworkElement),                // The type of the property
                                                typeof(ComponentView)              // The type of the owner class (custom control)
                                                );
        public ComponentView()
        {
            InitializeComponent();
        }
    }
}
