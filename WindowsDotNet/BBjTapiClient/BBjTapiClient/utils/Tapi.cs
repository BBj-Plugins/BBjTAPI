/*
 * Wrapper of the tapi functionalities - interacting with TAPI drivers:
 * - Mark Smith JULMAR ATAPI .Net TAPI2.0 https://github.com/markjulmar/atapi.net
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using JulMar.Atapi;

namespace BBjTapiClient.utils
{
    public class Tapi : IDisposable
    {
        public string adapter = "atapi";

        private TapiManager mgr;
        private TapiLine[] lineCollection;
        public TapiLine currentLine;
        private TapiAddress[] addressCollection;
        public TapiAddress currentAddress;
        private ITapiCall currentOutgoingCall;
        private ITapiCall currentIncomingCall;
        string callNumberAsSoonAsPossible = "";
        public string allLinesAndAddresses = "";
        
        /* changed event states */
        bool lastCanMakeCall = false;
        bool lastLocked = false;
        bool lastLampState = false;
        bool lastInService = false;
        bool lastCanPickUpCall = false;
        bool lastConnected = false;
        string lastLineStates = "";
        string lastAddressStates = "";
        string lastAddressDetailStates = "";


        public Tapi()
        {
            App.log(String.Format("Using adapter '{0}'", adapter));
            mgr = new TapiManager("BBjTAPIClient.Net");
        }


        /* is called when main win is loaded */
        public void init()
        {
            App.isMgrInitializationPhase = true;
            bool didInitalize = false;
            try
            {
                didInitalize = mgr.Initialize(); // CRITICAL - will remain in an endless loop, if the windows system CONTROL PANEL is opened and the current bound TAPI DRIVER configuration is opened!
            }
            catch (Exception ex)
            {
                App.log(ex.Message);
            }
            App.isMgrInitializationPhase = false;
            if (didInitalize)
            {
                App.Setup.Lines.Clear();
                App.Setup.Addresses.Clear();
                currentLine = null;
                currentAddress = null;
                addressCollection = null;
                currentOutgoingCall = null;
                currentIncomingCall = null;
                allLinesAndAddresses = "";
                lineCollection = mgr.Lines;
                App.log(String.Format("{0}x TSP (Telephony service provider) lines detected", lineCollection.Count()));
                foreach (TapiLine line in lineCollection)
                {
                    App.Setup.Lines.Add(line.Name);
                    //line.Changed += Line_Changed;
                    //line.NewCall += Line_NewCall;
                    //line.Ringing += Line_Ringing;
                    allLinesAndAddresses += "|" + line.Name;
                    if (line.Addresses != null)
                    {
                        if (line.Addresses.Count() > 0)
                        {
                            foreach (TapiAddress adr in line.Addresses)
                            {
                                allLinesAndAddresses += "~" + adr.Address;
                            }
                        }
                        else
                            App.log(String.Format("Line '{0}' addresses are empty!", line.Name));
                    }
                    else
                        App.log(String.Format("Line '{0}' addresses collection is NULL!", line.Name));
                }
                if (allLinesAndAddresses != "")
                    allLinesAndAddresses = allLinesAndAddresses.Substring(1);

                if (App.Setup.Line != "")
                    setCurrentLine(App.Setup.Line);
                if (App.Setup.Address != "")
                    setCurrentAddress(App.Setup.Address);

                App.isRefreshingTapiSession = true; // start session
            }
        }


        /* is called when the item in the Device combobox has been changed */
        public void setCurrentLine(string lineName)
        {
            if (lineCollection.Count() > 0)
            {
                TapiLine item = lineCollection.FirstOrDefault(i => i.Name == lineName);
                if (item != null)
                {
                    if (currentLine != null)
                        if (currentLine.Name != lineName)
                            stopSession();

                    currentLine = item;
                    currentLine.Changed += Line_Changed;
                    currentLine.NewCall += Line_NewCall;
                    currentLine.Ringing += Line_Ringing;
                    App.log(String.Format("Selected TAPI line - current line is '{0}'", currentLine.Name));
                    addressCollection = (TapiAddress[])item.Addresses;
                    App.Setup.Addresses.Clear();
                    if (addressCollection.Count() == 1)
                    {
                        App.log(String.Format("Selected TAPI line includes one address {0}", addressCollection[0].Address));
                        App.Setup.Addresses.Clear();
                        App.Setup.Addresses.Add(addressCollection[0].Address); // refresh Addresses combobox content 
                        setCurrentAddress(addressCollection[0].Address);
                    }
                    else
                    {
                        App.log(String.Format("Selected TAPI line includes {0}x addresses", addressCollection.Count()));
                        App.Setup.Addresses.Clear();
                        foreach (var address in addressCollection)
                        {
                            App.Setup.Addresses.Add(address.Address); // refresh Addresses combobox content 
                        }
                    }
                    App.registry.write("Device", currentLine.Name);
                }
                else
                    App.log(String.Format("Unable to find line '{0}'", lineName));
            }
        }


        /* is called when the item in the Address combobox has been changed */
        public void setCurrentAddress(string addressName)
        {
            if (addressCollection != null)
            {
                TapiAddress item = addressCollection.FirstOrDefault(i => i.Address == addressName);
                if (item != null)
                {
                    currentAddress = item;
                    currentAddress.CallStateChanged += CurrentAddress_CallStateChanged;
                    App.log(String.Format("Selected TAPI address '{0}'", currentAddress.Address));
                    App.registry.write("Address", currentAddress.Address);
                }
                else
                    App.log(String.Format("Unable to apply address '{0}'", addressName));
            }
            else
                App.log("Collection of line addresses is not available");
        }


        private void CurrentAddress_CallStateChanged(object sender, CallStateEventArgs e)
        {
            if (sender == currentAddress)
            {
                string divState = "";
                string value = "";

                value = Enum.GetName(typeof(CallState), e.CallState);
                if (value!=null && value!="")
                    divState += String.Format("- CallState '{0}'", value);

                value = Enum.GetName(typeof(CallState), e.OldCallState);
                if (value != null && value != "")
                    divState += String.Format("- OldCallState '{0}'", value);

                value = Enum.GetName(typeof(MediaModes), e.MediaModes);
                if (value != null && value != "")
                    divState += String.Format("- MediaModes '{0}'", value);

                value = Enum.GetName(typeof(BearerModes), e.Call.BearerMode);
                if (value != null && value != "")
                    divState += String.Format("- BearerMode '{0}'", value);

                value = e.Call.CalledId;
                if (value != null && value != "")
                    divState += String.Format("- CalledId '{0}'", value);

                value = e.Call.CalledName;
                if (value != null && value != "")
                    divState += String.Format("- CalledName '{0}'", value);

                value = e.Call.CallerId;
                if (value != null && value != "")
                    divState += String.Format("- CallerId '{0}'", value);

                value = Enum.GetName(typeof(CallReasons), e.Call.CallReason);
                if (value != null && value != "")
                    divState += String.Format("- CallReason '{0}'", value);

                value = e.Call.ConnectedId;
                if (value != null && value != "") 
                    divState += String.Format("- ConnectedId '{0}'", e.Call.ConnectedId);

                value = e.Call.ConnectedName;
                if (value != null && value != "")
                    divState += String.Format("- ConnectedName '{0}'", value);

                value = e.Call.DataRate.ToString();
                if (value != null && value != "0")
                    divState += String.Format("- DataRate (bps) '{0}'", value);

                value = e.Call.Id.ToString();
                if (value != null && value != "0")
                    divState += String.Format("- Id '{0}'", value);

                //e.Call.Features
                //e.Call.MediaDetection

                value = Enum.GetName(typeof(MediaModes), e.Call.MediaMode);
                if (value != null && value != "")
                    divState += String.Format("- MediaMode of Owner '{0}'", value);

                value = Enum.GetName(typeof(Privilege), e.Call.Privilege);
                if (value != null && value != "")
                    divState += String.Format("- Privilege '{0}'", value);

                value = e.Call.UserUserInfo;
                if (value != null && value != "")
                    divState += String.Format("- UserInfo '{0}'", value);

                if (divState!=lastAddressDetailStates)
                    App.log("TAPI Adress state changed " + divState);
                lastAddressDetailStates = divState;
            }
            else
                App.log("TAPI Address - raised State changed event belongs not to the current Address!");
        }


        /* rock'n roll */
        public void startSession()
        {
            App.log("TAPI - Start session");
            if (!App.Setup.IsTapiSessionActive)
            {
                App.Setup.IsTapiSessionActive = true;
                if (currentLine != null)
                {
                    if (currentLine.IsOpen)
                        App.log("TAPI line is already open. There is no need to open it!");
                    else
                    {
                        bool hasBeenOpened = false;
                        try
                        {
                            currentLine.Open(currentLine.Capabilities.MediaModes);
                            App.log(String.Format("Opened TAPI line '{0}' (MediaModes)",currentLine.Name));
                            hasBeenOpened = true;
                        }
                        catch (TapiException te)
                        {
                            App.log(te.Message);
                        }
                        catch (Exception ex)
                        {
                            App.log(ex.Message);
                        }
                        if (!hasBeenOpened)
                        {
                            App.log("Unable to open line in Capabilities MediaModes - attempt to open line in MediaModes.DataModem");
                            try
                            {
                                currentLine.Open(MediaModes.DataModem);
                                App.log(String.Format("Opened TAPI line '{0}' (alternative DataModem)",currentLine.Name));
                                hasBeenOpened = true;
                            }
                            catch (TapiException te)
                            {
                                App.log(te.Message);
                            }
                            catch (Exception ex)
                            {
                                App.log(ex.Message);
                            }
                        }
                        if (hasBeenOpened)
                        {
                            refreshStateFlags();
                            if (currentLine.IsOpen)
                            {
                                App.log(String.Format("Current line object 'IsOpen' state is '{0}'", currentLine.IsOpen));
                            }
                        }
                        else
                        {
                            App.log("TAPI line couldn't been opened.");
                        }
                    }
                }
                else
                    App.log("Current line object is null");
            }
            else
                App.log("Session was marked as already active!");
        }


        /* what starts ends */
        public void stopSession()
        {
            App.log("TAPI - Stop session");
            if (App.Setup.IsTapiSessionActive)
            {
                App.Setup.IsTapiSessionActive = false;
                if (currentLine != null)
                {
                    if (currentLine.IsOpen)
                    {
                        currentLine.Close();
                        App.log(String.Format("TAPI line - closed"));
                        refreshStateFlags();
                    }
                    else
                        App.log("Current line is not open! Hence it can'be closed.");
                }
                else
                    App.log("Current TAPI line object is NULL. There is nothing to close.");
            }
        }


        /* drop outgoing call */
        public void dropCall()
        {
            if (currentOutgoingCall != null)
            {
                App.log("TAPI address - Dropping outgoing call");
                if (currentOutgoingCall.Line != null)
                {
                    if (currentOutgoingCall.Line.IsOpen)
                    {
                        try
                        {
                            currentOutgoingCall.Drop();
                            App.log("TAPI address - Dropped outgoing call");
                        }
                        catch (Exception ex)
                        {
                            App.log("TAPI address - Unable to drop call. " + ex.Message);
                        }
                    }
                }
            }
        }

        /* make outgoing call */
        public void makeCall(string plainPhoneNumber)
        {
            App.log("TAPI address - Making outgoing call to '" + plainPhoneNumber + "'");
            if (currentAddress != null)
            {
                if (currentAddress.Address != "")
                {
                    if (!App.Setup.CanMakeCall)
                    {
                        dropCall();
                        callNumberAsSoonAsPossible = plainPhoneNumber;
                    }
                    else
                    {
                        try
                        {
                            currentOutgoingCall = currentAddress.MakeCall(plainPhoneNumber);
                            App.log(String.Format("TAPI address - Made call to '{0}'", plainPhoneNumber));
                        }
                        catch (Exception ex)
                        {
                            App.log(ex.Message);
                        }
                    }
                }
            }
        }


        /*
         * dummy so far 
         */
        private void Line_Ringing(object sender, RingEventArgs e)
        {
            App.log(String.Format("TAPI line - Ringing event - Count '{0}'", e.RingCount));
        }


        /*
         * Incoming call event handler
         */
        private void Line_NewCall(object sender, NewCallEventArgs e)
        {
            currentIncomingCall = e.Call;
            string callingPhoneNumber = currentIncomingCall.CallerId;
            App.log(String.Format("TAPI line - Incoming call event - Caller ID '{0}' - Called ID '{1}' - Privilege '{2}' - Call '{3}'", currentIncomingCall.CallerId, currentIncomingCall.CalledId, e.Privilege, e.Call));
            App.network.incomingCall(callingPhoneNumber);
            e.Call.Line.Monitor(); 
            //currentIncomingCall.Line.Close();
            //currentIncomingCall.Line.Dispose();
            //currentIncomingCall.Dispose();
            //currentIncomingCall = null;
        }


        /* tapi line changed event handler */
        private void Line_Changed(object sender, LineInfoChangeEventArgs e)
        {
            App.log("TAPI line - State changed event");
            if (sender == currentLine)
                refreshStateFlags();
            else
                App.log("TAPI line - raised State changed event belongs not to the current line!");
        }


        /* cleanup the outgoing call object */
        private void disposeOutgoingCall()
        {
            if (currentOutgoingCall != null)
            {
                //App.log("TAPI address - disposing outgoing call");
                currentOutgoingCall.Dispose();
                App.log("TAPI address - Disposed outgoing call");
                currentOutgoingCall = null;
            }
        }


        /* final cleanup */
        public void Dispose()
        {
            disposeOutgoingCall();
            if (mgr != null)
            {
                if (currentLine != null)
                {
                    currentLine.Close();
                    currentLine.Dispose();
                    App.log("TAPI line - Closed and disposed line");
                }
                mgr.Shutdown();
                App.log("TAPI manager - Shut down");
            }
        }


        /* refresh the flags on line changed event */
        private void refreshStateFlags()
        {

            bool state = false;
            string divStates = "";

            /* on line level */
            state = currentLine.Status.Connected;
            if (state != lastConnected)
                divStates += String.Format("- Connected '{0}'", state);
            lastConnected= state;
            App.Setup.IsTapiSessionConnected = state; // must have

            state = currentLine.Status.Locked;
            if (state != lastLocked)
                divStates += String.Format("- Locked '{0}'", state);
            lastLocked = state;

            state = currentLine.Status.MessageWaitingLampState;
            if (state!=lastLampState)
                divStates += String.Format("- Waiting Lamp '{0}'", state);
            lastLampState = state;

            state = currentLine.Status.InService;
            if (state!=lastInService)
                divStates += String.Format("- In Service '{0}'", state);
            lastInService= state;

            //state = (currentLine.Capabilities.SupportsForwarding && currentLine.IsOpen);
            //App.log(String.Format("TAPI line support forwarding state is {0}", state));

            if (divStates!=lastLineStates && divStates!="")
                App.log(String.Format("TAPI Line {0}", divStates));
            lastLineStates = divStates;

            /* on address level*/
            divStates = "";
            lastCanMakeCall = App.Setup.CanMakeCall;
            if (App.Setup.CanMakeCall != lastCanMakeCall)
                divStates += String.Format("- Can make call '{0}'", state);
            
            state = (currentAddress != null && currentAddress.Status.CanPickupCall);
            if (state!=lastCanPickUpCall)
                divStates += String.Format("- Can pick up call '{0}'", state);
            lastCanPickUpCall= state;

            App.Setup.CanMakeCall = (currentLine.Status.Connected && currentAddress != null && currentAddress.Status.CanMakeCall);

            //state = (currentAddress != null && currentAddress.Status.CanUnparkCall);
            //App.log(String.Format("TAPI address can unpark call state is {0}", state));

            if (divStates!=lastAddressStates && divStates!="")
                App.log(String.Format("TAPI Address {0}", divStates));
            lastAddressStates = divStates;

            /* process */
            if (callNumberAsSoonAsPossible != "" && App.Setup.CanMakeCall == true)
                makeCall(callNumberAsSoonAsPossible);
            callNumberAsSoonAsPossible = "";
        }
    }
}
