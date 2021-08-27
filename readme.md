This plugin implements a Windows TAPI service for BBj. It comes with a background process that dispatches incoming and outgoing calls, a BBj class that allows the developer to make calls and to register for incoming calls on a specific extension, and a Windows executable that links the Windows TAPI driver on a Windows desktop with the BBj background service / dispatcher.


The Windows executable features the following Command Line Options:
-S"<server>" 
-P<port> 
-E<extension> 
-D"TAPI Device Name" 
-A"TAPI Line Name" 
-debug"<debugfilename>"

  
********
  
Konzept

  
Das TAPI Plug-In ist so geschrieben dass es mit allen Clients arbeiten kann, auch dem Browser-Client (BUI und neu DWC). Daher besteht es aus einem zentralen Service, der am Server im BBj im Hintergrund seinen Dienst verrichtet. Außerdem gibt es ein Windows-exe Modul für jeden Client, welches sich einerseits über TCP/IP mit diesem Service verbindet und andererseits die Windows TAPI Schnittstelle implementiert. Das jeweilige BBj-Programm kommuniziert also nicht direkt mit der TAPI, sondern "hintenrum" über den BBjService.

Ich verstehe Deine Verwunderung - es wäre, wenn man nur die Java GUI betrachtet, möglich gewesen, direkt über den Client mit der Windows TAPI zu "sprechen". Da wir aber ausdrücklich den Browser mitbedienen wollten haben wir diesen Umweg eingebaut, sind aber so clientseitig komplett unabhängig von der Umgebung. Es ließe sich also später auch eine direkte Telefonie vom Server in die Telefonanlage realisieren, ohne TAPI, wie sie z.B. moderne Asterisk-Anlagen oder auch die Siemens HiPath bieten. Dies wäre dann eine reine Ergänzung im BBj Service-Module, das im Hintergrund am Server läuft.
  
