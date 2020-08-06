using System;
using System.Diagnostics;
using System.Threading;
using System.Device.Gpio;
using System.Collections.Generic;
using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System.Reflection;

namespace GateControl
{
    class Program
    {
        private const int DEBOUNCE_TIME = 200;
        private static readonly Dictionary<int, DateTime> lastChange = new Dictionary<int, DateTime>();
        private static readonly Dictionary<int, PinEventTypes> lastChangeType = new Dictionary<int, PinEventTypes>();
        private static HubConnection hubConnection;
        private static GpioController gpioController;

        static void Main(string[] args)
        {
            var task = ConnectToHub();
            task.Wait();
            InitGpio();
            Console.ReadKey();
        }

        private static void InitGpio()
        {
            gpioController = new GpioController(PinNumberingScheme.Logical);
            InitPinInputPullDown(19);
            InitPinInputPullDown(26);
        }

        private static void InitPinInputPullDown(int pinNr)
        {
            gpioController.OpenPin(pinNr, PinMode.InputPullDown);
            gpioController.RegisterCallbackForPinValueChangedEvent(pinNr, PinEventTypes.Falling, PinFalling);
            gpioController.RegisterCallbackForPinValueChangedEvent(pinNr, PinEventTypes.Rising, PinRising);
        }

        private async static Task ConnectToHub()
        {
            hubConnection = new HubConnectionBuilder()
                .WithUrl("http://192.168.68.118:5000/garagehub")
                .WithAutomaticReconnect()
                .Build();
            await hubConnection.StartAsync()
                .ContinueWith(async task =>
                {
                    if (task.IsFaulted)
                    {
                        Cons.WriteError(task.Exception.Message);
                        await ConnectToHub();
                    }
                    else
                    {
                        Cons.WriteInfo("Hub Connected!");
                        await hubConnection.InvokeAsync("RegisterClient");
                    }
                });
        }

        private static void WaitForDebugger()
        {
            int i = 0;
            while (!Debugger.IsAttached)
            {
                if (i % 10 == 0)
                {
                    Cons.WriteWarning("Waiting for debugger to attatch");
                }
                i++;
                Thread.Sleep(1000);
            }
            Cons.WriteInfo("Debugger attatched");
        }

        private static void PinRising(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            var callTime = DateTime.Now;
            var pin = pinValueChangedEventArgs.PinNumber;
            var changeType = pinValueChangedEventArgs.ChangeType;
            if (IsOutOfDebounce(pin, callTime, changeType))
            {
                Cons.WriteStatus($"{callTime:hh:mm:ss.fff}: pin #{pin} rising");
                switch (pin)
                {
                    case 19:
                        hubConnection.InvokeAsync("SendState", "open");
                        break;
                    case 26:
                        hubConnection.InvokeAsync("SendState", "closed");
                        break;
                }
            }
        }

        private static void PinFalling(object sender, PinValueChangedEventArgs pinValueChangedEventArgs)
        {
            var callTime = DateTime.Now;
            var pin = pinValueChangedEventArgs.PinNumber;
            var changeType = pinValueChangedEventArgs.ChangeType;
            if (IsOutOfDebounce(pin, callTime, changeType))
            {
                Cons.WriteStatus($"{callTime:hh:mm:ss.fff}: pin #{pin} falling");
                switch (pin)
                {
                    case 19:
                        hubConnection.InvokeAsync("SendState", "closing");
                        break;
                    case 26:
                        hubConnection.InvokeAsync("SendState", "opening");
                        break;
                }
            }
        }

        private static bool IsOutOfDebounce(int pinNumber, DateTime callTime, PinEventTypes changeType)
        {
            var wrongRead = lastChangeType.ContainsKey(pinNumber) && lastChangeType[pinNumber] == changeType;
            var withinDebounce = lastChange.ContainsKey(pinNumber) && lastChange[pinNumber].AddMilliseconds(DEBOUNCE_TIME) > callTime;
            if (!withinDebounce && !wrongRead)
            {
                lastChange[pinNumber] = callTime;
                lastChangeType[pinNumber] = changeType;
            }
            return !withinDebounce && !wrongRead;
        }

    }
}
