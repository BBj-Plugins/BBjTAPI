This plugin implements a Windows TAPI service for BBj. It comes with a background process that dispatches incoming and outgoing calls, a BBj class that allows the developer to make calls and to register for incoming calls on a specific extension, and a Windows executable that links the Windows TAPI driver on a Windows desktop with the BBj background service / dispatcher.


The Windows executable features the following Command Line Options:
-S"<server>" 
-P<port> 
-E<extension> 
-D"TAPI Device Name" 
-A"TAPI Line Name" 
-debug"<debugfilename>"
