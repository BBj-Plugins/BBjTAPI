// BBjTAPIClientDlg.cpp : implementation file
//

#include "stdafx.h"
#include "BBjTAPIClient.h"

#include "BBjTAPIClientDlg.h"
#include "registry.h"

#ifdef _DEBUG
#define new DEBUG_NEW
#endif

class CTapiCall;

// CAboutDlg dialog used for App About

class CAboutDlg : public CDialog 
{
public:
	CAboutDlg();


// Dialog Data
	enum { IDD = IDD_ABOUTBOX };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);    // DDX/DDV support

	

// Implementation
protected:
	DECLARE_MESSAGE_MAP()
};

CAboutDlg::CAboutDlg() : CDialog(CAboutDlg::IDD)
{	
}

void CAboutDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialog::DoDataExchange(pDX);
}

BEGIN_MESSAGE_MAP(CAboutDlg, CDialog)

END_MESSAGE_MAP()


// CBBjTAPIClientDlg dialog




CBBjTAPIClientDlg::CBBjTAPIClientDlg(CWnd* pParent /*=NULL*/)
	: CTrayDialog(CBBjTAPIClientDlg::IDD, pParent)
	, Port(_T(""))
	, Host(_T(""))
	, Status(_T(""))
	, Ext(_T(""))
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
	WaitingForMinimize=false;

}

void CBBjTAPIClientDlg::DoDataExchange(CDataExchange* pDX)
{
	CTrayDialog::DoDataExchange(pDX);
	DDX_Text(pDX, IDC_PORT, Port);
	DDX_Text(pDX, IDC_SERVER, Host);
	DDV_MaxChars(pDX, Host, 255);
	DDX_Text(pDX, IDC_STATUS, Status);
	DDX_Text(pDX, IDC_EXTENSION, Ext);
	DDX_Control(pDX, IDC_DEVICE, cbDevice);
	DDX_Control(pDX, IDC_ADDRESS, cbAddress);
	DDX_Control(pDX, IDC_SERVER, edServer);
	DDX_Control(pDX, IDC_PORT, edPort);
	DDX_Control(pDX, IDC_EXTENSION, edExt);
}

BEGIN_MESSAGE_MAP(CBBjTAPIClientDlg, CTrayDialog)
	ON_WM_SYSCOMMAND()
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()

	//}}AFX_MSG_MAP
	ON_BN_CLICKED(IDOK, &CBBjTAPIClientDlg::OnBnClickedOk)
	ON_BN_CLICKED(IDC_SIMU_INCOMINGCALL, &CBBjTAPIClientDlg::OnBnClickedSimuIncomingcall)
	ON_WM_TIMER()
	ON_CBN_SELCHANGE(IDC_DEVICE, &CBBjTAPIClientDlg::OnCbnSelchangeDevice)
	ON_CBN_SELCHANGE(IDC_ADDRESS, &CBBjTAPIClientDlg::OnCbnSelchangeAddress)
	ON_MESSAGE(UM_NEWCALL, i_OnNewCall)
	ON_BN_CLICKED(IDCANCEL, &CBBjTAPIClientDlg::OnBnClickedCancel2)
END_MESSAGE_MAP()


// CBBjTAPIClientDlg message handlers


CString CBBjTAPIClientDlg::getCmdLineOption(CString option)
{
	CString cmdline = GetCommandLineA();
	CString ret = "";
	if (cmdline.Find(option) >-1)
	{
		ret = cmdline.Right(cmdline.GetLength()-cmdline.Find(option)-option.GetLength());
		if (ret.Left(1)=="\"")
		{
			ret=ret.Left(ret.Find("\"",2));	
			ret=ret.Right(ret.GetLength()-1);
		}
		else
			if (ret.Find(" ")>-1)
			{
				ret=ret.Left(ret.Find(" "));
			}

	}
	return ret;
}


BOOL CBBjTAPIClientDlg::OnInitDialog()
{
	CTrayDialog::OnInitDialog();

	// Add "About..." menu item to system menu.

	// IDM_ABOUTBOX must be in the system command range.
	ASSERT((IDM_ABOUTBOX & 0xFFF0) == IDM_ABOUTBOX);
	ASSERT(IDM_ABOUTBOX < 0xF000);

	CMenu* pSysMenu = GetSystemMenu(FALSE);
	if (pSysMenu != NULL)
	{
		CString strAboutMenu;
		strAboutMenu.LoadString(IDS_ABOUTBOX);
		if (!strAboutMenu.IsEmpty())
		{
			pSysMenu->AppendMenu(MF_SEPARATOR);
			pSysMenu->AppendMenu(MF_STRING, IDM_ABOUTBOX, strAboutMenu);
		}
	}

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	TraySetIcon(IDR_MAINFRAME);
	TraySetToolTip("BBj TAPI Client");
	//TraySetMenu(IDR_MENU1);
	BringWindowToTop();
	CenterWindow();

	

	CString tmp;
	m_fDebug=FALSE;

	tmp=this->getCmdLineOption("-debug");
	if (tmp > ""){
		m_fDebug = TRUE;
		
		logger =new CDebugLog(tmp);
		logger->Log("Started with debug mode");
	} 

	tmp=this->getCmdLineOption("-S");
	if (tmp.GetLength()>0)
		Host=tmp;
	else
		Host=QueryRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Host","");

	tmp=this->getCmdLineOption("-P");
	if (tmp.GetLength()>0)
		Port=tmp;
	else
		Port=QueryRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Port","");

	tmp=this->getCmdLineOption("-E");
	if (tmp.GetLength()>0)
		Ext=tmp;
	else
		Ext=QueryRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Ext","");
	
	UpdateData(0);

	//send to tray icon immediately:
	//PostMessage(WM_SYSCOMMAND, SC_MINIMIZE, 0);
	
	con	= new CTAPIServerConnection();
	con->dlg = this;
	con->BuildTAPIData();
	
	cbDevice.ResetContent();
	for (int i=0; i<con->Devices.GetSize(); i++)
	{
		cbDevice.AddString(con->Devices.GetAt(i));
	}
	cbDevice.SetCurSel(0);

	tmp=this->getCmdLineOption("-D");
	if (tmp.IsEmpty())
		tmp=QueryRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Device","");

	if (tmp.GetLength()>0)
	{
		for (int i=0; i<con->Devices.GetSize(); i++)
		{
			if (con->Devices.GetAt(i) == tmp)
			{
				cbDevice.SetCurSel(i);
				break;
			}
		}		
	}
	OnCbnSelchangeDevice();

	tmp=this->getCmdLineOption("-A");
	if (tmp.IsEmpty())
		tmp=QueryRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Address","");
	if (tmp.GetLength()>0)
	{
		for (int i=0; i<con->Addresses.GetSize(); i++)
		{
			if (con->Addresses.GetAt(i) == tmp)
			{
				cbAddress.SetCurSel(i);
				
				if (con->StartTAPISession() && 
				con->Reconnect(Host,Port,Ext) )
					WaitingForMinimize=true;
				break;
			}
		}		
	}
	
	SetTimer(1, 10000, NULL);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CBBjTAPIClientDlg::OnSysCommand(UINT nID, LPARAM lParam)
{
	if ((nID & 0xFFF0) == IDM_ABOUTBOX)
	{
		CAboutDlg dlgAbout;
		dlgAbout.DoModal();
	}
	else
	{
		CTrayDialog::OnSysCommand(nID, lParam);
	}
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CBBjTAPIClientDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CTrayDialog::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CBBjTAPIClientDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}




void CBBjTAPIClientDlg::OnBnClickedOk()
{
	UpdateData(1);
	SetRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Host",Host);
	SetRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Port",Port);
	SetRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Ext",Ext);
	CString x = con->Devices.GetAt(con->SelectedLine);
	SetRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Device",x);
	x=con->Addresses.GetAt(con->SelectedAddress);
	SetRegistryKey(HKEY_CURRENT_USER,"SOFTWARE\\BASIS\\BBjTAPIClient\\Address",x);

	
	if (
		con->StartTAPISession() && 
		con->Reconnect(Host,Port,Ext) )
	WaitingForMinimize=true;
}

void CBBjTAPIClientDlg::OnBnClickedSimuIncomingcall()
{
	//con->IncomingCall("+49681968140");
	//con->MakeCall("230");
}

void CBBjTAPIClientDlg::UpdateDisplay()
{
		UpdateData(1);
		if (con->m_fConnected){
			Status = "Connected ";
		}
		else
		{
			Status="Not Connected ";
		};

		if (con->LastMessage.GetLength()>0){
			Status +="(";
			Status +=con->LastMessage;
			Status +=")";
		}
		UpdateData(0);

		if (WaitingForMinimize)
		{
			WaitingForMinimize=false;
			PostMessage(WM_SYSCOMMAND, SC_MINIMIZE, 0);
		}
}


void CBBjTAPIClientDlg::OnTimer(UINT_PTR nIDEvent)
{
	// TODO: Add your message handler code here and/or call default
	if (!con->m_fConnected){
		UpdateData(1);
		con->Reconnect(Host,Port,Ext);
	}
	CTrayDialog::OnTimer(nIDEvent);
}

void CBBjTAPIClientDlg::OnCbnSelchangeDevice()
{

	con->SelectLine(cbDevice.GetCurSel());
	cbAddress.ResetContent();

	for (int i=0; i<con->Addresses.GetSize(); i++)
	{
		cbAddress.AddString(con->Addresses.GetAt(i));
	}
	cbAddress.SetCurSel(0);
	con->SelectAddress(0);
}

void CBBjTAPIClientDlg::OnCbnSelchangeAddress()
{
	con->SelectAddress(cbAddress.GetCurSel());
}



LRESULT CBBjTAPIClientDlg::i_OnNewCall(WPARAM wParam, LPARAM lParam)
{
	CString* number = (CString*) wParam;
	if (number->GetLength()>1)
	{
		con->IncomingCall(*number);
	}
	return 0;
}

void CBBjTAPIClientDlg::OnBnClickedCancel2()
{
	int a= AfxMessageBox("Shutdown TAPI Client?",MB_YESNO);
	if (a==6)
		this->EndDialog(0);
}

void CBBjTAPIClientDlg::Log(CString msg)
{
	if (this->m_fDebug)
		this->logger->Log(msg);
}
