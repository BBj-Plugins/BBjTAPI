/**
 * helps reading and writing default values. (see b) below)
 * a) getting the master defaults defined fix in App.cs here
 * b) overriding with values read from registry (t h i s   p a r t   i  s   m a n a g e d    h e r e)
 * c) overriding with values of the app starting arguments from extern
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Win32;

namespace BBjTapiClient.utils
{
    public class RegEdit
    {


        /* current user -> software -> BASIS -> BBjTAPIClient */
        RegistryKey baseRegistryKey;
        RegistryKey subRegistryKey;
        bool isValid = false;
        

        public RegEdit()
        {
            baseRegistryKey = Registry.CurrentUser;
            try
            { 
                subRegistryKey = baseRegistryKey.CreateSubKey("SOFTWARE\\BASIS\\BBjTAPIClient");
                subRegistryKey.SetValue("RegTestEntryBBjTAPIClient.Net", DateTime.Now.ToString());
                isValid = true;
            }
            catch (Exception ex)
            {
                App.log("Unable to write values into the Registry location CurrentUser.SOFTWARE\\BASIS\\BBjTAPIClient. " + ex.Message);
            }
        }


        public void write(string keyName, object value)
        {
            if (isValid)
            {
                try
                {
                    subRegistryKey.SetValue(keyName, value);
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to write registry key '{0}' and value '{1}'! {2}", keyName, value, ex.Message));
                }
            }
            else
                App.log("Unable to store value '" + value + "' in the registry");
        }


        private string read(string keyName)
        {
            string value = "";
            try
            {
                value = (string)subRegistryKey.GetValue(keyName);
            }
            catch (Exception ex)
            {
                App.log(String.Format("Unable to read registry key '{0}'! {1}", keyName, ex.Message));
            }
            return value;
        }


        /* not used */
        public void writeAll()
        {
            if (isValid)
            {
                write("Device", App.Setup.Line);
                write("Address", App.Setup.Address);
                write("Ext", App.Setup.Extension);
                write("Host", App.Setup.Server);
                write("Port", App.Setup.Port);
                App.log("wrote settings to registry");
            }
            else
                App.log("Unable to write all settings to the registry");
        }


        public void readAll()
        {
            if (isValid)
            {
                string value = "";
                bool exi = false;

                value = read("Device");
                if (value != "")
                {
                    App.Setup.Line = value;
                    App.log(String.Format("Read the major Line/Device '{0}' from registry", value));
                    exi = true;
                }

                value = read("Address");
                if (value != "")
                {
                    App.Setup.Address = value;
                    App.log(String.Format("Read the minor Address '{0}' of the Line/Device from registry", value));
                    exi = true;
                }

                value = read("Ext");
                if (value != "")
                {
                    App.Setup.Extension = value;
                    App.log(String.Format("Read collaborating BBj Extension '{0}' from registry", value));
                    exi = true;
                }

                value = read("Host");
                if (value != "")
                {
                    App.Setup.Server = value;
                    App.log(String.Format("Read BBjTapi.bbj bound Host '{0}' from registry", value));
                    exi = true;
                }

                value = read("Port");
                if (value != "")
                {
                    App.Setup.Port = value;
                    App.log(String.Format("Read BBjTapi.bbj bound Port '{0}' from registry", value));
                    exi = true;
                }
                if (exi)
                    App.log("Read settings from registry");
                else
                    App.log("No overriding settings in registry available yet.");
            }
            else
                App.log("Unable to read all settings of the registry");
        }

    }
}
