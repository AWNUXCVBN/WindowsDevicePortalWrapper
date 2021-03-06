# Installing the project
The easiest way to get started is to clone this project to a local repo and open the solution in Visual Studio.  From there, you can compile the library (either .NET or UWP, as needed) and copy the DLL from the /bin/Release folder in that project.  Adding this DLL to an existing project will allow you to use Device Portal Wrapper. 

Alternately, to quickly test out the Device Portal Wrapper, you can open one of the SampleWdpClient apps and begin modifying from there. 

**Note**: Device Portal Wrapper requires .NET 4.5.2 from the [Windows SDK](https://developer.microsoft.com/en-US/windows/downloads/windows-10-sdk). 

# Using Device Portal Wrapper
The Device Portal Wrapper is built around a single object, the DevicePortal, which represents a Device Portal service instance running on a machine (either local or remote to the client).  The object is used to trigger Device Portal REST APIs on the target device (shutdown, app launch, etc). 

**Note**: The examples in the Getting Started guide assume that you have referenced Microsoft.Tools.WindowsDevicePortal in your project. 

## Creating a DevicePortal object
The DevicePortal object is initialized with a DevicePortalConnection object, which handles the connection between the client and the service on the target. The Device Portal Wrapper comes with a basic DefaultDevicePortalConnection implementation that can connect to IoT, HoloLens, Xbox, and Desktop instances. Once the object is created, the connection can be established using the Connect method. 
		
Example - initializing and establishing a Device Portal connection:
```C#	   
DevicePortal portal = new DevicePortal(
  new DefaultDevicePortalConnection(
    address, // a full URI (e.g. 'https://localhost:50443')
    username,
    password));
portal.Connect().Wait(); // The ConnectionStatus event will tell you if it succeeded 
```
For complete examples and stepping off points see the SampleWdpClients.  A UWP version and WPF version of the sample app are included in the solution.

### Verifying your connection
There are several different ways to verify your connection for using secure HTTPS communication with a device. Any of these methods can be used to secure your connection against a Man in the Middle attack or to allow your own proxy for debugging your communication.

1. If the certificate generated by your device is in your Trusted Root Certification Authorities certificate store then the connection will be automatically verified when connecting. No additional work is necessary.
2. You can manually provide the certificate object to the Connect method on the DevicePortal object to use for verification. This certificate could have been retrieved earlier via USB, or downloaded via a call to \<address\>/config/rootcertificate and then manually verified as being the proper certificate. This is a good method to use if using a web proxy such as Fiddler, as those proxies generally let you export their certificate.
3. Add your own logic for certificate checking. This works differently in Win32 and UWP
   - For UWP, you can make a call to the GetRootDeviceCertificate method on your DevicePortal object with the acceptUntrustedCerts parameter set to true. This will allow untrusted connections to your device for subsequent calls. **You should warn your user that they are making an untrusted connection or otherwise verify your connection is secure if using this method.**
   - For Win32, you can add a handler to the UnvalidatedCert event on your DevicePortal object which gives you a chance to perform custom handling such as presenting the thumbprint for the certificate to the user and asking them if they trust the connection, or using a prior cached decision about this connection. This is similar to how a web browser handles untrusted certificates.
   
The SampleWdpClients show examples of using all of these methods of establishing trust in an application.

## Using the DevicePortal object
Each REST API exposed by Device Portal is represented as a method off of the DevicePortal object. Most methods are a task that return an object representing the JSON return value.  

Example - finding a calculator app installed on the device: 
```C#
AppPackages apps = await portal.GetInstalledAppPackages();
PackageInfo package = apps.Packages.First(x => x.Name.Contains("calc"));
```

### Connecting over WebSocket
You can also establish websocket connections, for instance to get System Performance or Running Process information on a push basis.  Events are fired by the portal for each push of data from the server, and begin firing once the websocket connection is established. 

Example - print the process with the highest memory consumption, and stop listing processes if it's the Contoso process. 
```C#
await  portal.StartListeningForRunningProcesses();
portal.RunningProcessesMessageReceived += (portal, args) =>
  {
    DeviceProcessInfo proc =  args.Message.Processes.OrderByDescending(x=>x.TotalCommit).First();
    Console.WriteLine("Process with highest total commit:{0} - {1} ", proc.ProcessId, proc.Name);
    if (proc.Name.Contains("Contoso"))
      {
        portal.StopListeningForRunningProcesses();
      }
  };
```

## Using the Sample Apps and DefaultDevicePortalConnection
The SampleWdpClient apps are built on top of the DefaultDevicePortalConnection, which allows them to connect to most devices. At this time, the DefaultDevicePortalConnection is incompatible with Windows Phone. 
To connect using one of the sample apps, you can enter the full protocol, IP, and port for the target device.  

| Platform  | Default Uri |
| ------------- | ------------- |
| PC  | https://ipAddress:50443  |
| Xbox | https://ipAddress:11443  |
| IoT | http://ipAddress:8080 |
| HoloLens | https://ipAddress |

Once connected to the target device, you can see basic device information, collect the IP Config for the device, and power cycle it. 

There is also an [Xbox console application](https://github.com/Microsoft/WindowsDevicePortalWrapper/blob/master/XboxWDPDriver.md) which provides a helpful and extensible command line tool when working on Xbox.
