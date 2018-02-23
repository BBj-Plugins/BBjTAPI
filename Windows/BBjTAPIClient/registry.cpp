#include "stdafx.h"


#define MAX_VALUE_NAME 255

CString QueryRegistryKey(HKEY primkey, CString key, CString retval)
{

    
CString KeyRoot, KeyEntry;
if (key.Find("\\")==-1)
{
	KeyRoot.Empty ();
	KeyEntry = key;
}
else
{
	KeyEntry = key;
	while (KeyEntry.Find('\\')>-1)
	{
		if (KeyRoot.GetLength()>0) KeyRoot +='\\';
		KeyRoot += KeyEntry.Left(KeyEntry.Find('\\'));
		KeyEntry=KeyEntry.Right(KeyEntry.GetLength ()-KeyEntry.Find('\\')-1);
	}
}
	 HKEY hKey;
LONG l=RegOpenKeyEx( primkey,KeyRoot,0, KEY_READ | KEY_QUERY_VALUE, &hKey);

    CHAR     achClass[MAX_PATH] = "";  /* buffer for class name   */ 
    DWORD    cchClassName = MAX_PATH;  /* length of class string  */ 
    DWORD    cSubKeys;                 /* number of subkeys       */ 
    DWORD    cbMaxSubKey;              /* longest subkey size     */ 
    DWORD    cchMaxClass;              /* longest class string    */ 
    DWORD    cValues;              /* number of values for key    */ 
    DWORD    cchMaxValue;          /* longest value name          */ 
    DWORD    cbMaxValueData;       /* longest value data          */ 
    DWORD    cbSecurityDescriptor; /* size of security descriptor */ 
    FILETIME ftLastWriteTime;      /* last write time             */  
    DWORD  j;     DWORD retValue;  
    CHAR  achValue[MAX_VALUE_NAME];     
	DWORD cchValue = MAX_VALUE_NAME; 
	//BYTE bData[MAX_VALUE_NAME];
	DWORD bcData;
	DWORD dwType;


    RegQueryInfoKey(hKey,        /* key handle                    */ 
        achClass,                /* buffer for class name         */ 
        &cchClassName,           /* length of class string        */ 
        NULL,                    /* reserved                      */ 
        &cSubKeys,               /* number of subkeys             */ 
        &cbMaxSubKey,            /* longest subkey size           */ 
        &cchMaxClass,            /* longest class string          */ 
        &cValues,                /* number of values for this key */ 
        &cchMaxValue,            /* longest value name            */ 
        &cbMaxValueData,         /* longest value data            */ 
        &cbSecurityDescriptor,   /* security descriptor           */ 
        &ftLastWriteTime);       /* last write time               */  

    // Enumerate the child keys, looping until RegEnumKey fails. Then 
    // get the name of each child key and copy it into the list box. 
/*
    SetCursor(LoadCursor(NULL, IDC_WAIT)); 

    SetCursor(LoadCursor (NULL, IDC_ARROW));      // Enumerate the key values. 
    SetCursor(LoadCursor(NULL, IDC_WAIT));      
*/

        for (j = 0, retValue = ERROR_SUCCESS; 
               retValue == ERROR_SUCCESS || retValue == ERROR_MORE_DATA; j++) 
	{       
			unsigned char* x = new unsigned char[255];
			bcData=255;
			cchValue = MAX_VALUE_NAME; 
            achValue[0] = '\0'; 
            retValue = RegEnumValue(hKey, j, achValue, &cchValue,
				NULL, 
                &dwType,               
				x,  
                &bcData  );


		CString tmp(achValue);
		if (tmp.Find(KeyEntry)>-1)
		   retval = x;
			
			delete x;
    } 
	return retval;
}


BOOL SetRegistryKey(HKEY primkey, CString key, CString value)
{

	HKEY  hKey1;
	DWORD  dwDisposition;   
	LONG   lRetCode;  

	CString KeyRoot, KeyEntry;
	if (key.Find("\\")==-1)
	{
		KeyRoot.Empty ();
		KeyEntry = key;
	}
		else
	{
		KeyEntry = key;
		while (KeyEntry.Find('\\')>-1)
		{
			if (KeyRoot.GetLength()>0) KeyRoot +='\\';
			KeyRoot += KeyEntry.Left(KeyEntry.Find('\\'));
			KeyEntry=KeyEntry.Right(KeyEntry.GetLength ()-KeyEntry.Find('\\')-1);
		}
	}

  
	lRetCode = RegCreateKeyEx ( primkey, 
                              KeyRoot, 
                              0, NULL, REG_OPTION_NON_VOLATILE, KEY_WRITE, 
                              NULL, &hKey1, 
                              &dwDisposition);  
  
	if (lRetCode != ERROR_SUCCESS)
		return FALSE;
  
	lRetCode = RegSetValueEx ( hKey1, 
                             KeyEntry,
							 0, 
                             REG_SZ, 
                             (unsigned char*)value.GetBuffer(1), 
                             value.GetLength()+1);  

  if (lRetCode != ERROR_SUCCESS) 
  return FALSE;
  

  return TRUE;

}