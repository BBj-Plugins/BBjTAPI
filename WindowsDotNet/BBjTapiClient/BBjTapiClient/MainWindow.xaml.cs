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

using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Timers;
using System.Threading;
using JulMar.Atapi;
using System.Reflection;

namespace BBjTapiClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private System.Timers.Timer timer;
        private bool virgin = false;
        /* tray icon stuff */
        private bool cancelShutDown = true;
        public NotifyIcon notifyIcon = null;
        private System.Windows.Forms.ContextMenuStrip contextMenu1Strip;
        private ToolStripItem menuItem1Strip;
        private ToolStripItem menuItem2Strip;
        private bool timerCurrentlyRaised = false;
        private Int64 tickCounter = 0;

        public MainWindow()
        {
            bool startThrough = true;
            /* this program may only run once using this EXTENSION - avoid parallel processing of the same Extension */
            if (App.Setup.Extension != "")
            {
                App.mutex = new System.Threading.Mutex(true, "BBjTAPIClient.Net.Extension" + App.Setup.Extension, out App.createdNewMutex);
                if (!App.createdNewMutex)
                {
                    startThrough = false;
                    cancelShutDown = false;
                    closeApplication(false);
                }
            }
            if (startThrough)
            {
                /* isn't running yet using this Extension on this server/client ' */
                InitializeComponent();
                hideApplication();
                App.mainWin = this;
                AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(Application_ThreadException);
                initializeAsTrayIconApplication();
                setMainWindowTitle();
                App.displayPage("binding"); // main settings
                App.network.initialize(); // method code continues before call is completed - is okay here

                if (App.Setup.Extension == "" | App.Setup.Line == "")
                {
                    showApplication(); // only display the app on the desktop if the setup is incomplete
                    virgin = true;
                }

                App.isPreparationPhase = false; //Preparation done - user interaction allowed
                startTimer();
                if (!App.startAppSilent)
                {
                    showBalloonTip("Telephony service provider client executed.");
                }
            }
        }

        #region trayIconApplicationHandling
        /* initialize the tray icon application style */
        private void initializeAsTrayIconApplication()
        {
            /** init context menu and tray icon */
            this.contextMenu1Strip = new System.Windows.Forms.ContextMenuStrip();

            menuItem1Strip = contextMenu1Strip.Items.Add("show");
            menuItem1Strip.Click += MenuItem1Strip_Click;
            menuItem1Strip.Image = Properties.Resources.show;

            menuItem2Strip = contextMenu1Strip.Items.Add("exit");
            menuItem2Strip.Click += MenuItem2Strip_Click;
            menuItem2Strip.Image = Properties.Resources.exit;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
            notifyIcon.Icon = Properties.Resources.BBjTAPIClient;
            notifyIcon.Visible = true;

            notifyIcon.ContextMenuStrip = contextMenu1Strip;
        }


        /* show application by doubleclick */
        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            showApplication();
        }


        /** exit context clicked */
        private void MenuItem2Strip_Click(object sender, EventArgs e)
        {
            cancelShutDown = false;
            closeApplication(false);
        }


        /** show context clicked */
        private void MenuItem1Strip_Click(object sender, EventArgs e)
        {
            showApplication();
        }


        /* hide this */
        private void hideApplication()
        {
            this.ShowInTaskbar = false;
            this.Visibility = System.Windows.Visibility.Hidden;
        }

        /* show this app window on the desktop */
        private void showApplication()
        {
            this.ShowInTaskbar = true;
            this.Visibility = System.Windows.Visibility.Visible;
        }


        /* sure to close this wonderful application? */
        private void mainWin_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cancelShutDown)
            {
                hideApplication();
                e.Cancel = cancelShutDown;
            }
        }
        

        /* triggered by close() - final cleanup activities */
        private void mainWin_Closed(object sender, EventArgs e)
        {
            App.isShuttingDown = true;
            stopTimer();
            App.tapi.Dispose();
            App.network.disconnect();
            /* tray icon */
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            /* free again and open for a restart */
            if (App.createdNewMutex)
            {
                try
                {
                    App.mutex.ReleaseMutex();
                }
                catch { }
            }
        }


        /* say good bye */
        public void closeApplication(bool isHarderTermination)
        {
            if (isHarderTermination)
            {
                try
                {
                    System.Windows.Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;
                    System.Windows.Application.Current.Shutdown();
                }
                catch
                {
                    var text = "TIMEOUT! READ CAREFULLY!" +
                        "   THE TAPI MANAGER INITIALIZATION PROCESS TAKES TOO LONG!" +
                        "   PLEASE ENSURE A CLOSED CONFIGURATION OF THE TELEPHONY SERVICE PROVIDER MAINTENANCE!" +
                        "   MEANS, IF YOU HAVE OPENED THE WINDOWS SYSTEM CONTROL PANEL BEFORE AND HAVE MOVED TO PHONE AND MODEM" +
                        " DETAILS, YOU HAVE TO CLOSE THEM."+
                        "   DO DESTROY THIS TAPI CLIENT NOW!   RESTART IT AFTER THE UPPER INSTRUCTIONS HAVE BEEN FOLLOWED!";
                    System.Windows.MessageBox.Show(text);
                    App.terminate();
                }
            }
            else
            { 
                try
                {
                    Close();
                }
                catch (Exception ex)
                {
                    App.log(String.Format("Unable to close the application. {0}", ex.Message));
                }
            }
        }
        #endregion

        #region miscellaneous 
        
        /* tray icon balloon tip */
        public void showBalloonTip(string text)
        {
            notifyIcon.BalloonTipText = text;
            notifyIcon.BalloonTipTitle = "BBjTAPIClient.Net";
            notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
            notifyIcon.BalloonTipClicked += NotifyIcon_BalloonTipClicked;
            notifyIcon.ShowBalloonTip(1000);
        }


        private void NotifyIcon_BalloonTipClicked(object sender, EventArgs e)
        {
            showApplication();
        }


        /* SECUNDARY global exception handler */
        static void Application_ThreadException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            if (e is TapiException)
                App.log("Tapi exception! " + e.Message);
            else
                App.log("Exception! " + e.Message);
        }


        /* set the main window caption */
        private void setMainWindowTitle()
        {
            var assembly = Assembly.GetExecutingAssembly();
            string version = assembly.GetName().Version.ToString();
            string title = App.aim + (IntPtr.Size == 4 ? " - x86" : " - x64") + " - V." + version;
            mainWin.Title = title;
        }

        /* is raised when the tray icon application comes up to be shown as the window - window opening the first time */
        /* if the app run silently in the background as usual - this method won't be called */
        private void mainWin_Loaded(object sender, RoutedEventArgs e)
        {
            App.tapi.init(); // CRITICAL
            //raiseAppTapiInit = true; // execute App.tapi.init() in the time to ensure an displayed window!
        }

        #endregion 

        #region timerManaging
        /* program heart beat timer configuration */
        private void startTimer()
        {
            /* arrange timer in order to connect unattended to the bbjtapi central server*/
            timer = new System.Timers.Timer(2000);
            timer.Elapsed += raiseTimer;
            timer.AutoReset = true;
            timer.Start();
        }


        private void stopTimer()
        {
            if (timer != null)
            {
                timer.Enabled = false;
                timer.Stop();
                timer.Close();
                timer.Dispose();
            }
        }


        /* check if BBjTapi was started in the time being / if the connection is available now */
        private void raiseTimer(object sender, ElapsedEventArgs e)
        {
            // if (raiseAppTapiInit)
            // {
            //     raiseAppTapiInit = false;
            //     App.tapi.init(); // CRITICAL - PROCESS MAY HANG - get lines -- display lines
            // }
            if (App.isMgrInitializationPhase)
            {
                App.mgrInitializationPhaseCounter++;
                if (App.mgrInitializationPhaseCounter==8)
                {
                    /* timeout of mgr.Initialize() */
                    App.isMgrInitializationPhase = false;
                    cancelShutDown = false;
                    closeApplication(true); // hard
                }
            }
            if (!timerCurrentlyRaised)
            {
                timerCurrentlyRaised = true;
                if (App.isPreparationPhase == false)
                {
                    tickCounter++;
                    /* this program may only run once using this EXTENSION - avoid parallel processing of the same Extension */
                    if (App.mutex==null && App.Setup.Extension!= "")
                    {
                        App.mutex = new System.Threading.Mutex(true, "BBjTAPIClient.Net.Extension" + App.Setup.Extension, out App.createdNewMutex);
                        /** the App.createdNewMutex should prevent from further parallel processing the same extension on the same server */
                        if (App.createdNewMutex)
                        {
                            App.log("Okay, did pin extension '" + App.Setup.Extension + "' to avoid a concurrent session using the same extension!");
                        }
                        else
                        {
                            App.log("Unable to pin the extension '" + App.Setup.Extension + "'. It has already been pinned! Please close this BBjTapiClient!");
                        }
                    }
                    if (App.Setup.IsNetworkConnectionEstablished == false)
                    {
                        App.network.disconnect();
                        App.network.initialize(); // async embedded - continues before initialize call is completed - is okay here 
                    }
                    /* attempt to connect tapi line from time to time */
                        if (App.Setup.IsTapiSessionConnected == false && tickCounter>0 && tickCounter % 5 == 0)
                        App.isRefreshingTapiSession = true;
                    /* refresh tapi line session */
                    if (App.isRefreshingTapiSession)
                    {
                        App.tapi.stopSession();
                        App.tapi.startSession();
                        App.isRefreshingTapiSession = false;
                    }
                    /* only once */
                    if (virgin)
                    {
                        /* signalize fully connectivity */
                        if (App.mainWin.Visibility == Visibility.Visible && App.Setup.IsExtensionRegistered && App.Setup.IsTapiSessionConnected)
                        {
                            showBalloonTip("connected");
                            virgin = false;
                        }
                    }
                }
                timerCurrentlyRaised = false;
            }
        }
        #endregion

        #region page_management
        /* on 'Extras' page click */
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            btnBinding.Style = (Style)mainWin.FindResource("normalButton");
            btnExtras.Style = (Style)mainWin.FindResource("activeButton");
            App.displayPage("extras");
        }

        /* on 'Binding' page click */
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            btnBinding.Style = (Style)mainWin.FindResource("activeButton");
            btnExtras.Style = (Style)mainWin.FindResource("normalButton");
            App.displayPage("binding");
        }
        #endregion

        /* will be call from App.terminate() */
        public void BtnTerminate_Click(object sender, RoutedEventArgs e)
        {
            cancelShutDown = false;
            closeApplication(false); // cleaned up shut down
        }

        /* not used anymore - but call this before showing a message box to ensure no overlay of the client window above the message box */
        public void BtnMinimize_Click(object sender, RoutedEventArgs e)
        {
            this.WindowStyle = WindowStyle.None;
            this.Topmost = false;
            this.Top = 0;
            this.Left = 0;
        }
    }
}
