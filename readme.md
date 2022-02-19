This plugin implements a Windows TAPI service for BBj. 

It is not based on JTAPI which we couldn't really manage to get working with the phone systems we were asked to support, but uses the standard Windows TAPI client. Windows TAPI has been a de-facto standard for years and many phone systems provide drivers for it.

The plugin works in GUI, BUI and DWC, but requires a small piece of communication relay (written in MS C#) to be installed on the Windows desktop that hosts the TAPI driver. So it's, in the end, a Windows-only solution by the nature of the TAPI driver, but the BBj server side can be anything, and the BBj UI could still run in a terminal emulator or in a browser. It does not have to be the Java Swing GUI with client-side Java.

The plug-in could be, but is not yet, adopted to Asterisk-based SIP VOIP systems. It would be a doable enhancement if there is funding and interest. Some other phone systems like large Siemens systems have direct IP socket-based APIs which could also be connected as part of a professional services assignment. Contact us if you have a need for non-Windows-TAPI support for your BBj system.

## Windows Client Relay

The Plug-in comes as a BBj background progrsam that dispatches incoming and outgoing calls, a BBj class that allows the developer to make calls and to register for incoming calls on a specific extension, and a Windows executable that links the Windows TAPI driver on a Windows desktop with the BBj background service / dispatcher.

The Windows executable features the following Command Line Options:
-S"<server>" 
-P<port> 
-E<extension> 
-D"TAPI Device Name" 
-A"TAPI Line Name" 
-debug"<debugfilename>"

  

 
## (German) Konzept

  
Das TAPI Plug-In ist so geschrieben dass es mit allen Clients arbeiten kann, auch dem Browser-Client (BUI und neu DWC). Daher besteht es aus einem zentralen Service, der am Server im BBj im Hintergrund seinen Dienst verrichtet. Außerdem gibt es ein Windows-exe Modul für jeden Client, welches sich einerseits über TCP/IP mit diesem Service verbindet und andererseits die Windows TAPI Schnittstelle implementiert. Das jeweilige BBj-Programm kommuniziert also nicht direkt mit der TAPI, sondern "hintenrum" über den BBjService.

Es wäre, wenn man nur die Java GUI betrachtet, möglich gewesen, direkt über den Client mit der Windows TAPI zu "sprechen". Da wir aber ausdrücklich den Browser mitbedienen wollten haben wir diesen Umweg eingebaut, sind aber so clientseitig komplett unabhängig von der Umgebung. Es ließe sich also später auch eine direkte Telefonie vom Server in die Telefonanlage realisieren, ohne TAPI, wie sie z.B. moderne Asterisk-Anlagen oder auch die Siemens HiPath bieten. Dies wäre dann eine reine Ergänzung im BBj Service-Module, das im Hintergrund am Server läuft.
  
