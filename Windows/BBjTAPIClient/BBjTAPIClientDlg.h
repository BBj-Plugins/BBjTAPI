// BBjTAPIClientDlg.h : header file
//

#pragma once
#define UM_NEWCALL		 (WM_USER + 100)
#include "DebugLog.h"
#include "TrayDialog.h"
#include "afxwin.h"
#include "TAPIServerConnection.h"
#include "resource.h"

// CBBjTAPIClientDlg dialog
class CBBjTAPIClientDlg : public CTrayDialog
{
// Construction
public:
	CBBjTAPIClientDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_BBJTAPICLIENT_DIALOG };

	protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support


// Implementation
protected:
	HICON m_hIcon;
	CTAPIServerConnection* con;

	CString getCmdLineOption(CString);

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnSysCommand(UINT nID, LPARAM lParam);
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();

	//}}AFX_MSG
	DECLARE_MESSAGE_MAP()
public:
	afx_msg void OnBnClickedOk();
	afx_msg void OnBnClickedButton1();
	afx_msg void OnEnChangeSimuDocall();
	afx_msg LRESULT i_OnNewCall(WPARAM wParam, LPARAM lParam);
	CString Port;
	CString Host;
	CString Status;
	BOOL WaitingForMinimize;
	BOOL m_fDebug;
	CDebugLog* logger;


	afx_msg void OnBnClickedSimuIncomingcall();

	void UpdateDisplay();
	afx_msg void OnTimer(UINT_PTR nIDEvent);
	CString Ext;
	CComboBox cbDevice;
	afx_msg void OnCbnSelchangeDevice();
	CComboBox cbAddress;
	afx_msg void OnCbnSelchangeAddress();
	afx_msg void OnBnClickedCancel();
	CEdit edServer;
	CEdit edPort;
	CEdit edExt;
	afx_msg void OnBnClickedCancel2();

	void Log(CString msg);

};
