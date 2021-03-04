/*
 * driven by madness
 * App start goes here!
 */
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using BBjTapiClient.utils;
using BBjTapiClient.viewmodels;
using System.Windows.Controls;
using System.IO;

namespace BBjTapiClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        public static string aim = "BBjTAPIClient - TSP communication client - .Net";

        public static bool isPreparationPhase = true;
        public static bool isShuttingDown = false;
        public static bool isRefreshingTapiSession = false;
        public static bool isTapiInitRan = false;

        /* logging */
        public static MainWindow mainWin;
        public static int logCount = 0;
        public static string lastMessage = "";
        private static bool isCtemp = false;
        private static string cTempLogFilename = String.Format("C:\\Temp\\BBjTapiClientNet.{0}.txt", DateTime.Now.Ticks);
        private static string cTempLogFilenameMask = String.Format("C:\\Temp\\BBjTapiClientNet.*");
        private static StreamWriter streamLogWriter;

        /* engine */
        public static Tapi tapi; // adapter = "atapi"

        /* div */
        public static Network network;
        public static RegEdit registry;
        public static string lastDisplayedPageName = "";
        public static bool startAppSilent = true;

        /* settings/parameter input output handling */
        private static Settings setup;
        public static Settings Setup
        {
            get { return setup; }
            set { setup = value; }
        }

        /* page */
        public static Page bindingPage;
        public static Page extrasPage;

        /* avoid multiple execution */
        public static System.Threading.Mutex mutex;
        public static bool createdNewMutex;

        /* log */
        public static List<string> backlog = new List<string>();
        public static bool isWorkoutBacklog = false;

        /*  control the TAPI Manager execution - The manager might run in an endless loop - the following VAR in the timer is the watchdog */
        public static bool isMgrInitializationPhase = false;
        public static int mgrInitializationPhaseCounter = 0;

        /* open a page */
        public static void displayPage(string pageName)
        {
            if (pageName != App.lastDisplayedPageName)
            {
                if (pageName == "binding")
                {
                    if (bindingPage == null)
                        bindingPage = new pages.binding();
                    App.mainWin.mainFrame.Navigate(bindingPage);
                }
                if (pageName == "extras")
                {
                    if (extrasPage == null)
                        extrasPage = new pages.extras();
                    App.mainWin.mainFrame.Navigate(extrasPage);
                }
            }
            App.lastDisplayedPageName = pageName;
        }


        /* testwise */
        public static void minimize()
        {
            mainWin.btnTerminate.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)
                delegate ()
                {
                    mainWin.BtnMinimize_Click(null, null);
                }
            );
        }



        public static void terminate()
        {
            if (streamLogWriter != null)
            {
                streamLogWriter.Close();
                streamLogWriter.Dispose();
                streamLogWriter = null;
            }
            mainWin.btnTerminate.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)
                delegate ()
                {
                    mainWin.BtnTerminate_Click(null, null); // SHUT DOWN APPLICATION
                }
            );
        }



        /* log information */
        public static void log(String message)
        {
            if (streamLogWriter == null && isCtemp)
            {
                streamLogWriter = new StreamWriter(cTempLogFilename, true, System.Text.Encoding.Default);
            }
            if (streamLogWriter != null)
            {
                streamLogWriter.WriteLine(message);
                streamLogWriter.Flush();
            }
            int maxVisibleLines = 512;
            if (message != lastMessage)
            {
                lastMessage = message;
                string line = DateTime.Now.ToLocalTime().ToString() + " " + message;
                try
                {
                    if (backlog.Count() > 0 && isWorkoutBacklog)
                    {
                        foreach (var item in backlog)
                        {
                            mainWin.logbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)
                                delegate ()
                                {
                                    if (logCount > maxVisibleLines)
                                        mainWin.logbox.Items.RemoveAt(0);
                                    mainWin.logbox.Items.Add(item);
                                    mainWin.logbox.SelectedIndex = mainWin.logbox.Items.Count - 1;
                                    mainWin.logbox.ScrollIntoView(mainWin.logbox.SelectedItem);
                                }
                            );
                        }
                        backlog.Clear();
                    }
                    if (mainWin != null)
                    {
                        mainWin.logbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)
                            delegate ()
                            {
                                if (logCount > maxVisibleLines)
                                    mainWin.logbox.Items.RemoveAt(0);
                                mainWin.logbox.Items.Add(line);
                                mainWin.logbox.SelectedIndex = mainWin.logbox.Items.Count - 1;
                                mainWin.logbox.ScrollIntoView(mainWin.logbox.SelectedItem);
                                isWorkoutBacklog = true;
                            }
                        );
                        logCount++;
                    }
                    else
                        backlog.Add(line);
                }
                catch
                {
                    backlog.Add(line);
                }
            }
        }


        /*
         * CleanUpFolder
         */
        private void CleanUpFolder()
        {
            App.log($"CleanUpFolder {cTempLogFilenameMask} and removing files older than 16 days.");
            try
            {
                string[] files = Directory.GetFiles(@"C:\Temp\", "BBjTapiClientNet.*", SearchOption.TopDirectoryOnly);
                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            FileInfo fi = new FileInfo(file);
                            TimeSpan age = DateTime.Now.Subtract(fi.CreationTime);
                            if (age.Days > 16)
                            {
                                try
                                {
                                    File.Delete(file);
                                    App.log("Removing of obsolete file '" + file + "' done.");
                                }
                                catch (Exception ex)
                                {
                                    App.log("Unable to delete file '" + file + "' Exception : " + ex.Message);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            App.log("Unable to get age of file '" + file + "' Exception : " + ex.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                App.log("Unable to get files of folder 'C:/Temp/' using pattern 'BBjTapiClientNet.*'. Exception : " + ex.Message);
            }
        }


        /* start - process arguments */
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            isCtemp = Directory.Exists(@"C:\Temp\");
            if (isCtemp)
                CleanUpFolder();
            else
                App.log(@"C:\Temp\ does not exist. No logging will be saved in C:\Temp\. The logging is only available at runtime in the buffer of the client in this situation.");
            tapi = new Tapi();
            network = new Network();
            registry = new RegEdit();
            setup = new Settings(); // sets defaults, load setup from registry, override setup with values given by the starting args
                                    //#if !DEBUG
            registry.readAll(); // try to override the defaults with the values stored in the registry
                                //#endif
            string arg, value;
            string myServer = "", myPort = "", myLine = "", myAddress = "", myExtension = "";
            string prevArg = "";
            bool isShowPossibleArgs = false;
            if (e.Args.Length > 0)
            {
                App.log("Overriding settings using startup arguments");
                for (int i = 0; i != e.Args.Length; ++i)
                {
                    value = "";
                    arg = e.Args[i]; // -S127.0.0.1
                    if (arg.StartsWith("-"))
                    {
                        if (arg.Length > 2)
                        {
                            value = arg.Substring(2);
                            if (value != "")
                            {
                                switch (arg.Substring(0, 2))
                                {
                                    case "-S":
                                        prevArg = "-S";
                                        myServer = value;
                                        break;
                                    case "-P":
                                        prevArg = "-P";
                                        myPort = value;
                                        break;
                                    case "-E":
                                        prevArg = "-E";
                                        myExtension = value;
                                        break;
                                    case "-D":
                                        prevArg = "-D";
                                        myLine = value;
                                        break;
                                    case "-A":
                                        prevArg = "-A";
                                        myAddress = value;
                                        break;
                                    default:
                                        App.log($"Unknown arg received: '{arg}'.");
                                        isShowPossibleArgs = true;
                                        break;
                                }
                            }
                        }
                        if (arg.Length > 6)
                        {
                            if (arg.Substring(0, 6) == "-debug")
                            {
                                App.Setup.Debugfilename = arg.Substring(6);
                                App.log("Using Start-Argument -debug");
                            }
                        }
                    }
                    else
                    {
                        App.log($"Invalid arg format: {arg}. The previous type of arg was '{prevArg}'.");
                        if (prevArg != "")
                        {
                            if (prevArg == "-S")
                            {
                                myServer = myServer + " " + arg;
                                App.log($"Appending '{arg}' to the previous -S(ERVER) setting separated by a blank character");
                            }
                            if (prevArg == "-E")
                            {
                                myExtension = myExtension + " " + arg;
                                App.log($"Appending '{arg}' to the previous -E(XTENSION) setting separated by a blank character");
                            }
                            if (prevArg == "-D")
                            {
                                myLine = myLine + " " + arg;
                                App.log($"Appending '{arg}' to the previous -D(EVICE) setting separated by a blank character");
                            }
                            if (prevArg == "-A")
                            {
                                myAddress = myAddress + " " + arg;
                                App.log($"Appending '{arg}' to the previous -A(DDRESS) setting separated by a blank character");
                            }
                        }
                    }
                }
                if (myServer != "")
                {
                    App.Setup.Server = myServer;
                    App.log($"Using Start-Argument -S of value '{myServer}'. This is the BBjTapi.bbj bound Host Address.");
                }
                if (myPort != "")
                {
                    App.Setup.Port = myPort;
                    App.log($"Using Start-Argument -P of value '{myPort}'. This is the BBjTapi.bbj bound Port number.");
                }
                if (myExtension != "")
                {
                    App.Setup.Extension = myExtension;
                    App.log($"Using Start-Argument -E of value '{myExtension}'. This is the BBj collaborating Extension.");
                }
                if (myLine != "")
                {
                    App.Setup.Line = myLine;
                    App.log($"Using Start-Argument -D of value '{myLine}'. This is the major TAPI Line/Device.");
                }
                if (myAddress != "")
                {
                    App.Setup.Address = myAddress;
                    App.log($"Using Start-Argument -A of value '{myAddress}'. This is the minor TAPI Address of the Line/Device.");
                }
            }
            else
                startAppSilent = false; // if no args are given, show a BalloonTip briefly.
            if (isShowPossibleArgs)
                App.log("Valid args are : -S.., -P.., -E.., -D.., -A.., -debug.. (Server,Port,Extension,Device,Address)");
        }


        /* PRIOR global exception handler */
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
                App.log("Exception! " + e.Exception.Message);
        }


    }
}

