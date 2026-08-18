using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO.Pipes;
using TopNotify.Common;
using TopNotify.GUI;

namespace TopNotify.Daemon
{
    public class Daemon
    {
        const string PipeName = "samsidparty_topnotify";

        public static Daemon Instance;

        public InterceptorManager Manager;

        public Daemon() {
            Instance = this;

            TrayIcon.Setup();

            Thread managerThread = new Thread(CreateManager);
            managerThread.Start();

            Task.Run(PipeListener);

            TrayIcon.MainLoop();
        }

        // Per-connection ceiling on the daemon side. Bounds how long one stuck client
        // can hold up the single-instance server loop before it gives up and accepts
        // the next connection.
        static readonly TimeSpan ServerConnectionTimeout = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Listens for messages that affect the app lifecycle. Named pipe replaces the old
        /// mailslot transport, which had no delivery confirmation - a settings change sent
        /// while the daemon wasn't yet listening (or under any other timing edge case) would
        /// silently vanish, leaving the daemon on stale config with no indication to the user.
        ///
        /// Uses raw ReadAsync/WriteAsync directly on the PipeStream rather than
        /// StreamReader/StreamWriter - AutoFlush on a StreamWriter wrapping an async-mode
        /// pipe can fall back to a synchronous Flush() internally, which issues an
        /// uncancellable blocking Win32 WriteFile call with no timeout. Every I/O call here
        /// is bound to a CancellationToken so a stuck peer can never hang this loop forever.
        /// </summary>
        async Task PipeListener()
        {
            while (true)
            {
                try
                {
                    using (var server = new NamedPipeServerStream(PipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous))
                    {
                        await server.WaitForConnectionAsync();

                        using (var cts = new CancellationTokenSource(ServerConnectionTimeout))
                        {
                            var buffer = new byte[1024];
                            var bytesRead = await server.ReadAsync(buffer, 0, buffer.Length, cts.Token);
                            var msg = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                            if (msg == "UpdateConfig") // Runs when the user changes a setting from the GUI
                            {
                                InterceptorManager.Instance.OnSettingsChanged();
                            }

                            var ackBytes = Encoding.UTF8.GetBytes("ACK");
                            await server.WriteAsync(ackBytes, 0, ackBytes.Length, cts.Token);
                            await server.FlushAsync(cts.Token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Program.Logger.Warning(ex, "Daemon pipe listener iteration failed, restarting listener");
                }
            }
        }

        /// <summary>
        /// This should be called from an external (non-daemon) process to send a message to the daemon.
        /// Returns true only once the daemon has acknowledged receipt - callers can use this to warn
        /// the user if a settings change didn't actually reach the running daemon.
        ///
        /// Every step past Connect() is bound to timeoutMs via a CancellationToken, and the pipe
        /// is explicitly opened with PipeOptions.Asynchronous so that bound is actually enforceable -
        /// a synchronously-opened pipe's Write can't be cancelled once issued, which is what turned a
        /// transient stall into a permanent freeze of the calling (GUI/UI) thread previously.
        /// </summary>
        public static bool SendCommandToDaemon(string message, int timeoutMs = 2000)
        {
            try
            {
                using (var client = new NamedPipeClientStream(".", PipeName, PipeDirection.InOut, PipeOptions.Asynchronous))
                using (var cts = new CancellationTokenSource(timeoutMs))
                {
                    client.Connect(timeoutMs);

                    var msgBytes = Encoding.UTF8.GetBytes(message);
                    client.WriteAsync(msgBytes, 0, msgBytes.Length, cts.Token).GetAwaiter().GetResult();
                    client.FlushAsync(cts.Token).GetAwaiter().GetResult();

                    var buffer = new byte[1024];
                    var bytesRead = client.ReadAsync(buffer, 0, buffer.Length, cts.Token).GetAwaiter().GetResult();
                    var response = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    return response == "ACK";
                }
            }
            catch (Exception ex)
            {
                Program.Logger.Warning(ex, $"SendCommandToDaemon('{message}') failed to reach the daemon");
                return false;
            }
        }

        public void CreateManager()
        {
            Manager = new InterceptorManager();
            Manager.Start();
        }
    }
}
