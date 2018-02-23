#pragma once
#include "BBjTAPIClientDlg.h"



/////////////////////////////////////////////////////////////////////////////
// CMyLine
//
// Override of the CTapiLine for notifications
//
class CMyLine : public CTapiLine
{
	DECLARE_DYNCREATE (CMyLine)
protected:
    virtual void OnAddressStateChange (DWORD dwAddressID, DWORD dwState) {
		CTapiLine::OnAddressStateChange (dwAddressID, dwState);
		////((CPhoneDlg*)theApp.m_pMainWnd)->SendMessage(UM_ADDRESSCHANGE, (WPARAM)GetAddress(dwAddressID));
	}
	
	virtual void OnAgentStateChange (DWORD dwAddressID, DWORD dwFields, DWORD dwState) {
		CTapiLine::OnAgentStateChange (dwAddressID, dwFields, dwState);
		////((CPhoneDlg*)theApp.m_pMainWnd)->SendMessage(UM_AGENTCHANGE, (WPARAM)GetAddress(dwAddressID), dwFields);
	}

    virtual void OnDeviceStateChange (DWORD dwDeviceState, DWORD dwStateDetail1, DWORD dwStateDetail2) {
		CTapiLine::OnDeviceStateChange(dwDeviceState, dwStateDetail1, dwStateDetail2);
		//((CPhoneDlg*)theApp.m_pMainWnd)->SendMessage(UM_LINECHANGE, (WPARAM)this);
	}

    virtual void OnNewCall (CTapiCall* pCall) {
		CTapiLine::OnNewCall(pCall);
		LPLINECALLINFO lpCallInfo = pCall->GetCallInfo();
		if (lpCallInfo == NULL )
			return;

		if (lpCallInfo->dwOrigin & 
				(LINECALLORIGIN_INTERNAL |
				 LINECALLORIGIN_EXTERNAL |		
				 LINECALLORIGIN_INBOUND))
		{	
			//inbound calls only
			CString number = pCall->GetCallerIDNumber();
			CString called = pCall->GetCalledIDNumber();
			
			if (number.GetLength()>1)
			{
				CWinApp* theApp = AfxGetApp();
				CBBjTAPIClientDlg* d = ((CBBjTAPIClientDlg*)(theApp->m_pMainWnd));
				d->SendMessage(UM_NEWCALL, (WPARAM)&number);
				
			}
		}

	}
};

/////////////////////////////////////////////////////////////////////////////
// CMyCall
//
// Override of the CTapiCall for notifications
//
class CMyCall : public CTapiCall
{
	DECLARE_DYNCREATE (CMyCall)
public:
    virtual void OnInfoChange (DWORD dwInfoState) {
		CTapiCall::OnInfoChange(dwInfoState);
/*
		CWnd* pwnd = theApp.m_pMainWnd;
		if (pwnd != NULL && IsWindow(pwnd->GetSafeHwnd()))
		{
			if (dwInfoState & LINECALLINFOSTATE_USERUSERINFO)
				pwnd->PostMessage(UM_FLASHWINDOW, IDC_USERUSERINFO);
			if (dwInfoState & (LINECALLINFOSTATE_HIGHLEVELCOMP | LINECALLINFOSTATE_LOWLEVELCOMP | LINECALLINFOSTATE_CHARGINGINFO))
				pwnd->PostMessage(UM_FLASHWINDOW, IDC_ISDNINFO);
			if (dwInfoState & LINECALLINFOSTATE_QOS)
				pwnd->PostMessage(UM_FLASHWINDOW, IDC_QOS);
			if (dwInfoState & LINECALLINFOSTATE_CALLDATA)
				pwnd->PostMessage(UM_FLASHWINDOW, IDC_CALLDATA);

			//((CPhoneDlg*)pwnd)->SendMessage(UM_CALLCHANGE, (WPARAM)this);
		}
		*/
	}

    virtual void OnStateChange (DWORD dwState, DWORD dwStateDetail, DWORD dwPrivilage) {
		CTapiCall::OnStateChange(dwState, dwStateDetail, dwPrivilage);
		//((CPhoneDlg*)theApp.m_pMainWnd)->SendMessage(UM_CALLCHANGE, (WPARAM)this);
	}

	virtual void OnMediaModeChange (DWORD dwMediaMode) {
		CTapiCall::OnMediaModeChange(dwMediaMode);
		//((CPhoneDlg*)theApp.m_pMainWnd)->SendMessage(UM_CALLCHANGE, (WPARAM)this);
	}
};

/////////////////////////////////////////////////////////////////////////////
// CMyPhone
//
// Override of the CTapiPhone for notifications
//
class CMyPhone : public CTapiPhone
{
public:
	CWnd* m_pPhone;
	DECLARE_DYNCREATE (CMyPhone)
protected:
	CMyPhone() : CTapiPhone(), m_pPhone(0) {}
    virtual void OnDeviceStateChange (DWORD dwDeviceState, DWORD dwStateDetail) {
		CTapiPhone::OnDeviceStateChange(dwDeviceState, dwStateDetail);
		//if (m_pPhone != NULL)
		//	m_pPhone->SendMessage(UM_PHONECHANGE, dwDeviceState, dwStateDetail);
	}
	
};

IMPLEMENT_DYNCREATE (CMyLine, CTapiLine)
IMPLEMENT_DYNCREATE (CMyCall, CTapiCall)
IMPLEMENT_DYNCREATE (CMyPhone, CTapiPhone)