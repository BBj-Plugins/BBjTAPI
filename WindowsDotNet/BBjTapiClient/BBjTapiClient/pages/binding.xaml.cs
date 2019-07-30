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

namespace BBjTapiClient.pages
{
    /// <summary>
    /// Interaction logic for binding.xaml
    /// W h a t   i s   t h e   b i n d i n g   p a g  e ?
    /// This is the main page where the user defines the connection IP to BBjTapi.bbj 
    /// and the corresponding port.
    /// Where the user defines the extension collaboration id, selects the tapi device
    /// and address.
    /// </summary>
    public partial class binding : Page
    {
        bool isSuppressLineRefresh = false;
        bool isSuppressAddressRefresh = false;

        public binding()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isSuppressLineRefresh)
            {
                ComboBox cb = (ComboBox)sender;
                App.tapi.setCurrentLine((string)cb.SelectedItem);
                App.isRefreshingTapiSession = true; // impulse for re-connecting
                Addresses.Text = App.tapi.currentAddress.Address; // refresh the address - mostly the only one available within the TAPI LINE
                App.Setup.Line = (string)cb.SelectedItem;
            }
            isSuppressLineRefresh = false;
        }

        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (!isSuppressAddressRefresh)
            {
                ComboBox cb = (ComboBox)sender;
                App.tapi.setCurrentAddress((string)cb.SelectedItem);
                App.isRefreshingTapiSession = true;// impulse for re-connecting
                App.Setup.Address = (string)cb.SelectedItem;
            }
            isSuppressAddressRefresh = false;
        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            //App.isBindingPageLoaded = true;
            if (Lines.Text == "")
            {
                if (App.Setup.Address != "")
                {
                    isSuppressAddressRefresh = true;
                    Addresses.Text = App.Setup.Address;
                }
                isSuppressLineRefresh = true;
                Lines.Text = App.Setup.Line;
            };
        }

        /* SERVER setting lost focus */
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            App.registry.write("Host", tb.Text);
        }

        /* Port setting lost focus */
        private void TextBox_LostFocus_1(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            App.registry.write("Port", tb.Text);
        }

        /* Extension setting lost focus */
        private void TextBox_LostFocus_2(object sender, RoutedEventArgs e)
        {
            TextBox tb = (TextBox)sender;
            App.registry.write("Ext", tb.Text);
        }
    }
}
