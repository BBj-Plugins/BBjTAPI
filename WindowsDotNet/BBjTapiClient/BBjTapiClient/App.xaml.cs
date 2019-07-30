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

        /* logging */
        public static MainWindow mainWin;
        public static int logCount = 0;
        public static string lastMessage = "";

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

        /* termination flag retrieved from BBjTapi.bbj for instance */
        /* This might be true if the admin shuts down remotly all active BBjTapiCientNet.exe Clients */
        //public static bool terminationFlag = false;

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
                                    if (logCount > 255)
                                        mainWin.logbox.Items.RemoveAt(0);
                                    mainWin.logbox.Items.Add(item);
                                    mainWin.logbox.SelectedIndex = mainWin.logbox.Items.Count - 1;
                                    mainWin.logbox.ScrollIntoView(mainWin.logbox.SelectedItem);
                                }
                            );
                        }
                        backlog.Clear();
                    }
                    mainWin.logbox.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, (Action)
                        delegate ()
                        {
                            if (logCount > 255)
                                mainWin.logbox.Items.RemoveAt(0);
                            mainWin.logbox.Items.Add(line);
                            mainWin.logbox.SelectedIndex = mainWin.logbox.Items.Count - 1;
                            mainWin.logbox.ScrollIntoView(mainWin.logbox.SelectedItem);
                            isWorkoutBacklog = true;
                        }
                    );
                    logCount++;
                }
                catch
                {
                    backlog.Add(line);
                }
            }
        }


        /* start - process arguments */
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            tapi = new Tapi();
            network = new Network();
            registry = new RegEdit();
            setup = new Settings(); // sets defaults, load setup from registry, override setup with values given by the starting args
            registry.readAll(); // try to override the defaults with the values stored in the registry
            string arg, value;
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
                                        App.Setup.Server = value;
                                        break;
                                    case "-P":
                                        App.Setup.Port = value;
                                        break;
                                    case "-E":
                                        App.Setup.Extension = value;
                                        break;
                                    case "-D":
                                        App.Setup.Line = value;
                                        break;
                                    case "-A":
                                        App.Setup.Address = value;
                                        break;
                                    default:
                                        App.log("Unknown arg received: " + arg);
                                        isShowPossibleArgs = true;
                                        break;
                                }
                            }
                        }
                        if (arg.Length > 6)
                        {
                            if (arg.Substring(0, 6) == "-debug")
                                App.Setup.Debugfilename = arg.Substring(6);
                        }
                    }
                    else
                    {
                        App.log("Invalid arg format: " + arg);
                    }
                }
            }
            else
                startAppSilent = false; // if no args are given, show a BalloonTip briefly.
            if (isShowPossibleArgs)
                App.log("Valid args are : -S.., -P.., -E.., -D.., -A.., -debug..");
        }


        /* PRIOR global exception handler */
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            if (e.Exception != null)
                App.log("Exception! " + e.Exception.Message);
        }


    }
}

