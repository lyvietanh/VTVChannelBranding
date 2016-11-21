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
using VTVPlaylistEditor.Models;

namespace VTVPlaylistEditor.Views
{
    /// <summary>
    /// Interaction logic for ComingUpBlockItemView.xaml
    /// </summary>
    public partial class ComingUpBlockItemView : UserControl
    {
        public static readonly DependencyProperty DataBoundItemProperty = DependencyProperty.Register("DataBoundItem", typeof(ComingUpBlockModel), typeof(ComingUpBlockItemView), new PropertyMetadata(null));

        public ComingUpBlockModel DataBoundItem
        {
            get { return (ComingUpBlockModel)GetValue(DataBoundItemProperty); }
            set { SetValue(DataBoundItemProperty, value); }
        }

        public ComingUpBlockItemView()
        {
            InitializeComponent();
        }
    }
}
