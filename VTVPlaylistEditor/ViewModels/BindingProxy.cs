using System.Windows;

namespace VTVPlaylistEditor.ViewModels
{
//We can then declare an instance of this class in the resources of the DataGrid, and bind the Data property to the current DataContext:
//<DataGrid.Resources>
//    <local:BindingProxy x:Key="proxy" Data="{Binding}" />
//</DataGrid.Resources>

//        The last step is to specify this BindingProxy object (easily accessible with StaticResource) as the Source for the binding:
//<DataGridTextColumn Header="Price" Binding="{Binding Price}" IsReadOnly="False"
//                    Visibility="{Binding Data.ShowPrice,
//                        Converter={ StaticResource visibilityConverter},
//                        Source={ StaticResource proxy} "/>

    public class BindingProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }

        #endregion

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Data.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));
    }
}
