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
    /// Interaction logic for extras.xaml
    /// </summary>
    public partial class extras : Page
    {
        public extras()
        {
            InitializeComponent();
        }


        private void btnSimIncoming_Click(object sender, RoutedEventArgs e)
        {
            string num = App.Setup.PhoneNumberIncoming;
            if (num!=null && num!="")
                App.network.incomingCall(num);
            else
                tbxNumCaller.Focus();
        }

        private void btnMakeCall_Click(object sender, RoutedEventArgs e)
        {
            string num = App.Setup.PhoneNumberOutgoing;
            if (num!=null && num!="")
                App.tapi.makeCall(num);
            else
                tbxNumCall.Focus();
        }

        private void btnDropCall_Click(object sender, RoutedEventArgs e)
        {
            App.tapi.dropCall();
        }

        private void btnPostActual_Click(object sender, RoutedEventArgs e)
        {
            App.network.actualLineAndAddress(); // send actual binding
            System.Windows.MessageBox.Show("DONE");
        }

        private void btnPostAll_Click(object sender, RoutedEventArgs e)
        {
            App.network.allLinesAndAddresses(); //send collection of all line address pairs
            System.Windows.MessageBox.Show("DONE");
        }
    }
}
