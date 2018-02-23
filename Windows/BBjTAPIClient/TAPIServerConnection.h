#pragma once
#include "afxsock.h"



class CBBjTAPIClientDlg;


class CTAPIServerConnection :
	public CAsyncSocket 
{
public:
	CTAPIServerConnection(void);
	~CTAPIServerConnection(void);
	void OnSend(int nErrorCode);
	void OnReceive(int nErrorCode);
	void OnConnect(int nErrorCode);
	void OnClose(int nErrorCode);
	void SocketErrorMessage();
	BOOL Reconnect(CString host, CString port, CString extension);

public:
		BOOL m_fTryingConnect;
		BOOL m_fConnected;
		CBBjTAPIClientDlg* dlg;
		CString LastMessage;
		CString host;
		int port;
		CString extension;
		CArray<CString> Devices; 
		CArray<CObject*> DeviceObjects;
		CArray<CString> Addresses; 
		CArray<CObject*> AddressObjects;
		int SelectedLine;
		int SelectedAddress;
		CObject* OpenLine;
		
public:
	void IncomingCall(CString number);
	void BuildTAPIData(void);
	void SelectLine(int line);
	void SelectAddress(int address);
	BOOL StartTAPISession(void);
	void MakeCall(CString);
};
