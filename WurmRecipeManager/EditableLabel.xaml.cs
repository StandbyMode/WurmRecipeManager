using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace WurmRecipeManager
{
    /// <summary>
    /// Interaction logic for EditableLabel.xaml
    /// </summary>
    public partial class EditableLabel : UserControl
    {
        private static DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(String), typeof(EditableLabel));

        public String Text
        {
            get
            {
                return (String)GetValue(TextProperty);
            }
            set
            {
                SetValue(TextProperty, value);
            }
        }

        private static DependencyProperty EditingProperty = DependencyProperty.Register("Editing", typeof(bool), typeof(EditableLabel));

        public bool Editing
        {
            get
            {
                return (bool)GetValue(EditingProperty);
            }
            set
            {
                SetValue(EditingProperty, value);
            }
        }

        public EditableLabel()
        {
            InitializeComponent();
            Editing = false;
        }
    }
}
