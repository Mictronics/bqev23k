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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BQEV23K
{
    /// <summary>
    /// Interaction logic for LedControl.xaml
    /// </summary>
    public partial class LedControl : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
            typeof(string),
            typeof(LedControl),
            new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty
            .Register("IsChecked",
            typeof(bool),
            typeof(LedControl),
            new FrameworkPropertyMetadata(false));

        public LedControl()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public bool IsChecked
        {
            get { return (bool)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }
    }
}
