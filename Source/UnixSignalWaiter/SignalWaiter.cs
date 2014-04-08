/*
The MIT License (MIT)

Copyright (c) 2013 Kazuki Yasufuku (wilfrem)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace UnixSignalWaiter
{
    /// <summary>
    /// utility for waiting unix signal
    /// </summary>
    public class SignalWaiter
    {
        bool _isInit;
        readonly PlatformID _platform;
        Assembly _posixAsm;
        Type _unixSignalType, _signumType;
        MethodInfo _unixSignalWaitAny;

        Array _signals;

        static SignalWaiter _instance;
        public static SignalWaiter Instance
        {
            get { return _instance ?? (_instance = new SignalWaiter()); }
        }

        private SignalWaiter()
        {
            _platform = Environment.OSVersion.Platform;
        }

        private void Setup()
        {
            if (_isInit)
                return;
            //dynamic load assembly
            //load Mono.Posix assembly
            _posixAsm = Assembly.Load("Mono.Posix, Version=4.0.0.0, Culture=neutral, PublicKeyToken=0738eb9f132ed756");
            //create UnixSignal Type
            _unixSignalType = _posixAsm.GetType("Mono.Unix.UnixSignal");
            //get member function WaitAny
            _unixSignalWaitAny = _unixSignalType.GetMethod("WaitAny", new[] { _unixSignalType.MakeArrayType() });
            //create Signum enum type
            _signumType = _posixAsm.GetType("Mono.Unix.Native.Signum");
            //create Mono.Unix.UnixSignal[] type
            _signals = Array.CreateInstance(_unixSignalType, 2);
            //set SIGINT and SIGTERM
            _signals.SetValue(Activator.CreateInstance(_unixSignalType, _signumType.GetField("SIGINT").GetValue(null)), 0);
            _signals.SetValue(Activator.CreateInstance(_unixSignalType, _signumType.GetField("SIGTERM").GetValue(null)), 1);
            _isInit = true;
        }

        public bool CanWaitExitSignal()
        {
            return !(_platform != PlatformID.Unix && _platform != PlatformID.MacOSX);
        }

        /// <summary>
        /// wait while getting exit signal
        /// </summary>
        /// <exception cref="InvalidOperationException">when call it on Windows</exception>
        public void WaitExitSignal()
        {
            if (!CanWaitExitSignal())
            {
                throw new InvalidOperationException("not unix platform");
            }
            Setup();
            // Wait for a unix SIGINT/SIGTERM signal
            for (var exit = false; !exit; )
            {
                var id = (int)_unixSignalWaitAny.Invoke(null, new object[] { _signals });

                if (id >= 0 && id < _signals.Length)
                {
                    dynamic val = _signals.GetValue(id);
                    if (val.IsSet) 
                        exit = true;
                }
            }
        }
    }
}
