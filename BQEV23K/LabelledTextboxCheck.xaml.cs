using System.Windows;
using System.Windows.Controls;

namespace BQEV23K
{
    /// <summary>
    /// Interaction logic for LabelledTextboxCheck.xaml
    /// </summary>
    public partial class LabelledTextboxCheck : UserControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty
            .Register("Label",
                    typeof(string),
                    typeof(LabelledTextboxCheck),
                    new FrameworkPropertyMetadata("Unnamed Label"));

        public static readonly DependencyProperty TextProperty = DependencyProperty
            .Register("Text",
                    typeof(string),
                    typeof(LabelledTextboxCheck),
                    new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty UnitProperty = DependencyProperty
            .Register("Unit",
                    typeof(string),
                    typeof(LabelledTextboxCheck),
                    new FrameworkPropertyMetadata("No Unit"));

        public LabelledTextboxCheck()
        {
            InitializeComponent();
            Root.DataContext = this;
        }

        public string Label
        {
            get { return (string)GetValue(LabelProperty); }
            set { SetValue(LabelProperty, value); }
        }

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public string Unit
        {
            get { return (string)GetValue(UnitProperty); }
            set { SetValue(UnitProperty, value); }
        }
    }

    public class LayoutGroup : StackPanel
    {
        public LayoutGroup()
        {
            Grid.SetIsSharedSizeScope(this, true);
        }
    }
}
