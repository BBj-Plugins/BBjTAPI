/*
 * This class handled the TCP interaction with BBjTapi.bbj
 * Just open the tcp ip channel for listening and sending small streams/commands.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net.Sockets;
using System.Windows;

namespace BBjTapiClient.utils
{
    /* object of Network class is instantiated within the App.initialize() */
    public class Network
    {
        private TcpClient client;
        private NetworkStream networkStream;
        private StreamWriter streamWriter;
        private string lastExceptionMessage = "";

        /* init */
        public async Task initialize()
        {
            await App.network.connect();
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                Task.Run(() => this.listen()); // method code continues before call is completed - is okay here
                App.log("Started listening task");
            }
        }


        /* connect with BBjTapi.bbj */
        public async Task connect()
        {
            bool isConnected = false;
            App.Setup.IsNetworkConnectionEstablished = false;
            App.Setup.IsExtensionRegistered = false;
            App.log(String.Format("Waiting for connectivity with {0}:{1}", App.Setup.Server, App.Setup.Port));
            client = new TcpClient();
            try
            {
                await client.ConnectAsync(App.Setup.Server, Convert.ToInt16(App.Setup.Port));
                isConnected = true;
            }
            catch (Exception ex)
            {
                if (ex.Message != lastExceptionMessage)
                {
                    lastExceptionMessage = ex.Message;
                    App.log(ex.Message);
                }
            }
            if (isConnected)
            {
                App.log(String.Format("Connected to: {0}:{1}", App.Setup.Server, App.Setup.Port));
                networkStream = client.GetStream();
                streamWriter = new StreamWriter(networkStream);
                App.Setup.IsNetworkConnectionEstablished = true;
                this.registerExtension();
            }
        }


        /* disconnect from BBjTapi.bbj */
        public void disconnect()
        {
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                streamWriter.Close();
                streamWriter.Dispose();
                networkStream.Close();
                networkStream.Dispose();
                client.Close();
                client = null;
                App.log(String.Format("Disconnected from {0}:{1}", App.Setup.Server, App.Setup.Port));
                    
            }
        }


        /* register this TAPI middleware at BBjTapi.bbj through sending  REG:26  for instance */
        public void registerExtension()
        {
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                try
                {
                    string command = String.Format("REG:{0}", App.Setup.Extension);
                    streamWriter.WriteLine(command + "\r\n");
                    streamWriter.Flush();
                    App.log(String.Format("Sent registration command stream '{0}'", command));
                    App.Setup.IsExtensionRegistered = true;
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to send the register extension command stream. {0}", ex.Message));
                    App.Setup.IsNetworkConnectionEstablished = false; // try to reconnect soon then 
                }
            }
        }


        /* push the signal of an incoming call to BBjTapi.bbj  like  CALL:02152899799  */
        public void incomingCall(string plainPhoneNumber)
        {
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                try
                {
                    string command = String.Format("CALL:{0}", plainPhoneNumber);
                    streamWriter.WriteLine(command + "\r\n");
                    streamWriter.Flush();
                    App.log(String.Format("Sent incoming call command stream '{0}'", command));
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to send the incoming call command stream. {0}", ex.Message));
                    App.Setup.IsNetworkConnectionEstablished = false; // try to reconnect soon then 
                }
            }
        }


        /* push the signal of the actual TSP binding to BBjTapi.bbj  like  TSPBINDING:MY_EXT|MY_LINE|MY_ADDRESS  */
        public void actualLineAndAddress()
        {
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                try
                {
                    string command = String.Format("TSPBINDING:{0}|{1}|{2}", App.Setup.Extension, App.Setup.Line, App.Setup.Address);
                    streamWriter.WriteLine(command + "\r\n");
                    streamWriter.Flush();
                    App.log(String.Format("Sent actual TSP binding command stream '{0}'", command));
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to send the actual TSP binding command stream. {0}", ex.Message));
                    App.Setup.IsNetworkConnectionEstablished = false; // try to reconnect soon then 
                }
               
            }
        }

        /* 
         * push a list of all available lines and their addresses to BBjTapi.bbj. Lines are separated by PIPE.
         * Each Line contains addresses separated by TILDE.
         * Sample : ALLBINDINGS:|LINE1~ADDRESS1~ADDRESS2~ADDRESS3|LINE2~ADDRESS1|LINE3~ADDRESS1~ADDRESS2 etc.
         */
        public void allLinesAndAddresses()
        {
            if (App.Setup.IsNetworkConnectionEstablished)
            {
                try
                {
                    string command = String.Format("ALLBINDINGS:{0}", App.tapi.allLinesAndAddresses);
                    streamWriter.WriteLine(command + "\r\n");
                    streamWriter.Flush();
                    App.log(String.Format("Sent all available TSP bindings command stream"));
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to send all available TSP bindings command stream. {0}",ex.Message));
                    App.Setup.IsNetworkConnectionEstablished = false; // try to reconnect soon then 
                }
            }
        }


        /* retrieving something like  OUTCALL:23882382938823  or DROPCALL:  from BBjTapi.bbj */
        public async Task listen()
        {
            StreamReader sr = new StreamReader(networkStream);
            while (true)
            {
                string bbjTapiCommand = await sr.ReadLineAsync();
                App.log(String.Format("Received command stream '{0}'", bbjTapiCommand));
                if (bbjTapiCommand.StartsWith("OUTCALL:"))
                {
                    string plainPhoneNumber = bbjTapiCommand.Replace("OUTCALL:", "");
                    App.tapi.makeCall(plainPhoneNumber); // perform outgoing call signal
                }
                if (bbjTapiCommand.StartsWith("DROPCALL"))
                {
                    App.tapi.dropCall();
                }
                if (bbjTapiCommand.StartsWith("TERMINATE"))
                {
                    App.terminate();
                }
                if (App.isShuttingDown)
                    break;
            }
            try
            {
                sr.Close();
                sr.Dispose();
            }
            catch { }
        }

    }
}
