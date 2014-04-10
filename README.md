UnixSignalWaiter
================

wait unix signal utility, to support C# program running as daemon.

Usage(for Daemon)
---------------

You call SignalWaiter.Instance.WaitExitSignal() for waiting exit.

```Program.cs
Console.WriteLine("start owin and trying to wait signal");
using (WebApp.Start<Startup>("http://+:12345"))
{
    try
    {
        //wait until SIGINT/SIGTERM comes
        SignalWaiter.Instance.WaitExitSignal();
    }
    catch (InvalidOperationException)
    {
        Console.Error.WriteLine("Cannot wait exit signal. is Windows platform?");
    }
}
```

and you write /etc/init/YourDaemonName.conf

```YourDaemonName.conf
#YourDaemonName

description "YourDaemonName"
author "YourName"

start on started rc
stop on stopping rc

respawn

exec start-stop-daemon --start -c username --exec /usr/local/bin/mono /path/to/your/app.exe
```

and execute "sudo service YourDaemonName start"

