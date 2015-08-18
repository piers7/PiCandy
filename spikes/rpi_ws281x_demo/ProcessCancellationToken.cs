using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#if(__MonoCS__)
using Mono.Unix.Native;
#endif

namespace rpi_ws281x
{
    /// <summary>
    /// Attempts to hook as many ways of possible of detecting that the user
    /// has requested to terminate the application.
    /// </summary>
    /// <remarks>There's a couple of (equally problematic) approaches here.
    /// On Windows, we can hook SetConsoleCtrlHandler
    /// On Mono/Unix, we can use Signals, but only if we build and run on Mono
    /// If we build on Windows, and run on Mono, we have to use a fallback strategy
    /// </remarks>
    public class ProcessCancellationToken
    {
        private static byte _cancelRequested;

        // Mind you, maybe I shouldn't be doing Thread.VolatileRead
        // Does have a bad rap
        // http://joeduffyblog.com/2010/12/04/sayonara-volatile/
        // http://stackoverflow.com/a/15052688
        // http://stackoverflow.com/questions/15039188/volatile-vs-volatileread-write
        public static bool CancelRequested
        {
            get { return Thread.VolatileRead(ref _cancelRequested) > 0; }
            set { Thread.VolatileWrite(ref _cancelRequested, (byte)(value ? 1 : 0)); }
        }

        public static void SetupHooks(){
            switch(System.Environment.OSVersion.Platform){
#if(__MonoCS__)
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                    {
                        return HookUnixCancelSignals();
                        return;
                    }
#endif
                case PlatformID.Win32NT:
                case PlatformID.Win32S:
                case PlatformID.Win32Windows:
                    {
                        // If running on Windows, always use Kernel32
                        HookWindowsCancelSignals();
                        return;
                    }
            }

            HookGenericCancelSignals();
        }

        private static void HookGenericCancelSignals()
        {
            Console.TreatControlCAsInput = true;
            Console.CancelKeyPress += SetCancel;
        }

        private static void SetCancel(object sender, ConsoleCancelEventArgs e)
        {
            CancelRequested = true;
        }

        #region Mono implementation
#if(__MonoCS__)
        private static void HookUnixCancelSignals()
        {
            // SetConsoleCtrlHandler not exposed on mono
            // Seems like a bit of a complex topic http://mono.1490590.n4.nabble.com/Control-C-handler-td1532196.html
            // Some hope http://stackoverflow.com/questions/6546509/detect-when-console-application-is-closing-killed
            // http://www.mono-project.com/docs/faq/technical/#operating-system-questions

            // Need to conditionally reference Mono.Posix for all this to work
            // https://github.com/ninjarobot/MonoConditionalReference
            // Catch SIGINT and SIGUSR1
            UnixSignal[] signals = new UnixSignal [] {
                new UnixSignal (Signum.SIGINT),
                new UnixSignal (Signum.SIGUSR1),
            };

            var token = new ProcessCancellationToken(); 
            var signal_thread = new Thread(() =>
            {
                while (true)
                {
                    // Wait for a signal to be delivered
                    int index = UnixSignal.WaitAny(signals, -1);

                    var signal = signals[index].Signum;

                    // Notify the main thread that a signal was received
                    CancelRequested = true;
                }
            });
        }
#endif
        #endregion

        #region Windows implementation
        private static void HookWindowsCancelSignals()
        {
            SetConsoleCtrlHandler(ConsoleCtrlCheck, true);
        }

        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine for SetConsoleCtrlHandler.
        private delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages sent to the handler routine.
        private enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            if (Enum.IsDefined(typeof(CtrlTypes), ctrlType))
            {
                Console.WriteLine("{0} received, shutting down", ctrlType);
                CancelRequested = true;
                return true;
            }
            return false;
        }
        #endregion
    }
}
