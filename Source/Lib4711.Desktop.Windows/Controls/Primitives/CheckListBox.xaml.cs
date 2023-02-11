using System.Collections;
using System.Windows.Controls;

namespace Lib4711.Desktop.Windows.Controls.Primitives
{
    public partial class CheckListBox : UserControl
    {
        public event EventHandler<SelectionChangedEventArgs>? ItemSelectionChanged;

        public IList SelectedItems => this.CoreListBox.SelectedItems;
        public IEnumerable ItemsSource
        {
            get => this.CoreListBox.ItemsSource;
            set
            {
                this.CoreListBox.ItemsSource = value;
            }
        }

        public ListBox ListBox => this.CoreListBox;


        public CheckListBox() => InitializeComponent();

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.ItemSelectionChanged?.Invoke(this, e);
        }
    }
}
