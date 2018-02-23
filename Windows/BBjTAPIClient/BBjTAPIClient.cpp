// BBjTAPIClient.cpp : Defines the class behaviors for the application.
//

#include "stdafx.h"
#include "BBjTAPIClient.h"
#include "BBjTAPIClientDlg.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif


// CBBjTAPIClientApp

BEGIN_MESSAGE_MAP(CBBjTAPIClientApp, CWinApp)
	ON_COMMAND(ID_HELP, &CWinApp::OnHelp)
END_MESSAGE_MAP()


// CBBjTAPIClientApp construction

CBBjTAPIClientApp::CBBjTAPIClientApp()
{
	// TODO: add construction code here,
	// Place all significant initialization in InitInstance
}


// The one and only CBBjTAPIClientApp object

CBBjTAPIClientApp theApp;


// CBBjTAPIClientApp initialization

BOOL CBBjTAPIClientApp::InitInstance()
{

	const char szUniqueNamedMutex[] = "BBjTAPIClient";
   HANDLE hHandle = CreateMutex( NULL, TRUE, szUniqueNamedMutex );
   if( ERROR_ALREADY_EXISTS == GetLastError() )
   {
      // AfxMessageBox("BBjTAPIClient already active!");
      return FALSE; // Exit program
   }

	// InitCommonControlsEx() is required on Windows XP if an application
	// manifest specifies use of ComCtl32.dll version 6 or later to enable
	// visual styles.  Otherwise, any window creation will fail.
	INITCOMMONCONTROLSEX InitCtrls;
	InitCtrls.dwSize = sizeof(InitCtrls);
	// Set this to include all the common control classes you want to use
	// in your application.
	InitCtrls.dwICC = ICC_WIN95_CLASSES;
	InitCommonControlsEx(&InitCtrls);

	CWinApp::InitInstance();

	if (!AfxSocketInit())
	{
		AfxMessageBox(IDP_SOCKETS_INIT_FAILED);
		return FALSE;
	}

	// Standard initialization
	// If you are not using these features and wish to reduce the size
	// of your final executable, you should remove from the following
	// the specific initialization routines you do not need
	// Change the registry key under which our settings are stored
	// TODO: You should modify this string to be something appropriate
	// such as the name of your company or organization
	SetRegistryKey(_T("BASIS"));

	CBBjTAPIClientDlg dlg;
	m_pMainWnd = &dlg;
	INT_PTR nResponse = dlg.DoModal();

	return FALSE;
}
