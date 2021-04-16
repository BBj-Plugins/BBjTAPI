/*
 * this is the data model.
 * mostly each public member could be changed in any UI page.
 * The UI page refers to this object class using the datacontext functionality.
 * The controls of the UI page are then able to bind one of the public members
 * of this datacontext.
 * ThE UI page mostly applies the two way binding.
 * Means changing a value in the UI will directly transfer the change into the binded
 * public member here.
 * And vise versa - changing the public property of this class object here will directly
 * force a refresh in the proper UI page control.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BBjTapiClient.utils;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace BBjTapiClient.viewmodels
{
    public class Settings : Bindbase
    {
        // connection to cental BBjTapi.bbj available?
        private bool isNetworkConnectionEstablished;
        public bool IsNetworkConnectionEstablished
        {
            get { return isNetworkConnectionEstablished; }
            set { SetProperty(ref isNetworkConnectionEstablished, value); }
        }

        // connection to cental BBjTapi.bbj available?
        private bool isExtensionRegistered;
        public bool IsExtensionRegistered
        {
            get { return isExtensionRegistered; }
            set { SetProperty(ref isExtensionRegistered, value); }
        }

        // if was TAPI started, then true - if was TAPI stopped, then false +++ this will not indicate a valid connection - see isTapiSessionConnected
        private bool isTapiSessionActive;
        public bool IsTapiSessionActive
        {
            get { return isTapiSessionActive; }
            set { SetProperty(ref isTapiSessionActive, value); }
        }

        // if the current TAPI line connected?
        private bool isTapiSessionConnected;
        public bool IsTapiSessionConnected
        {
            get { return isTapiSessionConnected; }
            set { SetProperty(ref isTapiSessionConnected, value); }
        }

        // IP ADDR. OF BBjTapi.bbj binded host ; -S<server>
        private string server;
        public string Server
        {
            get { return server; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref server, value);
                if (App.isPreparationPhase == false)
                {
                    App.network.disconnect();
                    App.network.initialize(); // method code continues before call is completed - is okay here
                }
            }
        }

        // Port number of BBjTapi.bbj binded port ; -P<port>
        private string port;
        public string Port
        {
            get { return port; }
            set
            {
                value = value != null ? value : "";
                value = Regex.Replace(value, @"[^\d]", "");
                SetProperty(ref port, value);
                if (App.isPreparationPhase == false)
                {
                    App.network.disconnect();
                    App.network.initialize(); // method code continues before call is completed - is okay here
                }
            }
        }


        // Value to collaborate with BBjTapi.bbj ; kind of sesion id between BBjTapi.bbj and BBjTapiClient ; -E<extension>
        private string extension;
        public string Extension
        {
            get { return extension; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref extension, value);
                if (App.isPreparationPhase == false)
                {
                    App.network.disconnect();
                    App.network.initialize(); // method code continues before call is completed - is okay here
                }
            }
        }


        //Device collection - combobox
        private ObservableCollection<string> lines;
        public ObservableCollection<string> Lines
        {
            get { return lines; }
            set
            {
                value = value != null ? value : new ObservableCollection<string>();
                SetProperty(ref lines, value);
            }
        }


        //selected device ; -D"TAPI Device Name" 
        private string line;
        public string Line
        {
            get { return line; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref line, value);
            }
        }


        //Address collection (subelement of Device/Line) ;  -A"TAPI Line Name"  ; combobox
        private ObservableCollection<string> addresses;
        public ObservableCollection<string> Addresses
        {
            get { return addresses; }
            set
            {
                value = value != null ? value : new ObservableCollection<string>();
                SetProperty(ref addresses, value);
            }
        }


        //Address;  -A"TAPI Line Name" 
        private string address;
        public string Address
        {
            get { return address; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref address, value);
            }
        }


        // -A"TAPI Line Name" 
        private string debugfilename;
        public string Debugfilename
        {
            get { return debugfilename; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref debugfilename, value);
            }
        }


        // number for incoming call simulation
        private string phoneNumberIncoming;
        public string PhoneNumberIncoming
        {
            get { return phoneNumberIncoming; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref phoneNumberIncoming, value);
            }
        }


        // number for outgoing call testing
        private string phoneNumberOutgoing;
        public string PhoneNumberOutgoing
        {
            get { return phoneNumberOutgoing; }
            set
            {
                value = value != null ? value : "";
                SetProperty(ref phoneNumberOutgoing, value);
            }
        }

        // outgoing call allowed
        private bool canMakeCall;
        public bool CanMakeCall
        {
            get { return canMakeCall; }
            set
            {
                SetProperty(ref canMakeCall, value);
                CannotMakeCall = !canMakeCall;
            }
        }

        // outgoing call not allowed
        private bool cannotMakeCall;
        public bool CannotMakeCall
        {
            get { return cannotMakeCall; }
            set
            {
                SetProperty(ref cannotMakeCall, value);
            }
        }


        public Settings()
        {
            /** defaults */
            IsNetworkConnectionEstablished = false; // below host and port input
            isExtensionRegistered = false; // below extension input
            IsTapiSessionActive = false;
            IsTapiSessionConnected = false; // below tapi line and address selection
            Server = "localhost";
            Port = "12000";
            Extension = "";
            Lines = new ObservableCollection<string>();
            Line = "";
            Addresses = new ObservableCollection<string>();
            Address = "";
            Debugfilename = "";
            CanMakeCall = true;
            App.log("Set default settings");
        }

    }
}
