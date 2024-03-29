﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace StepBro
{
    public class SideKickPipe : IDisposable
    {
        private PipeStream m_pipe = null;
        private StreamString m_stream = null;
        private Thread m_thread = null;
        private bool m_disposed = false;
        private bool m_continue = false;
        private bool m_continueReceiving = false;
        private ConcurrentQueue<Tuple<string, string>> m_received = null;
        private ManualResetEvent m_disposeEvent = null;

        //public static void MyMainFunctionality()
        //{
        //    //for (i = 0; i < numThreads; i++)
        //    //{
        //    //}
        //    Thread.Sleep(250);
        //    while (i > 0)
        //    {
        //        for (int j = 0; j < numThreads; j++)
        //        {
        //            if (servers[j] != null)
        //            {
        //                if (servers[j]!.Join(250))
        //                {
        //                    Console.WriteLine("Server thread[{0}] finished.", servers[j]!.ManagedThreadId);
        //                    servers[j] = null;
        //                    i--;    // decrement the thread watch count
        //                }
        //            }
        //        }
        //    }
        //    Console.WriteLine("\nServer threads exhausted, exiting.");
        //}

        private SideKickPipe(string id, PipeStream pipe)
        {
            m_pipe = pipe;
        }

        public void Dispose()
        {
            if (!m_disposed)
            {
                System.Diagnostics.Trace.WriteLine("Sidekick pipe dispose!!");

                m_continueReceiving = false;
                m_continue = false;
                if (m_disposeEvent != null)
                {
                    m_disposeEvent.Set();
                }
                m_disposed = true;

                if (m_thread != null && m_thread.IsAlive)
                {
                    m_thread.Join();
                    m_thread = null;
                }
                System.Diagnostics.Trace.WriteLine("Sidekick thread stopped");
                if (m_pipe != null)
                {
                    m_pipe.Close();
                    m_pipe = null;
                }
            }
        }

        private static SideKickPipe Create(string id, PipeStream pipe, ParameterizedThreadStart threadFunction)
        {
            var instance = new SideKickPipe(id, pipe);
            instance.m_thread = new Thread(threadFunction);
            instance.m_received = new ConcurrentQueue<Tuple<string, string>>();
            return instance;
        }

        public static SideKickPipe StartServer(string id)
        {
            var pipeServer = new NamedPipeServerStream("StepBroConsoleSidekick" + id, PipeDirection.InOut, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
            var pipe = SideKickPipe.Create(id, pipeServer, ServerThread);
            pipe.m_disposeEvent = new ManualResetEvent(false);
            pipe.m_continue = true;
            pipe.m_thread.Start(pipe);
            return pipe;
        }

        public static SideKickPipe StartClient(string id)
        {
            var pipeClient = new NamedPipeClientStream(".", "StepBroConsoleSidekick" + id, PipeDirection.InOut, PipeOptions.Asynchronous, TokenImpersonationLevel.None);
            var pipe = SideKickPipe.Create(id, pipeClient, ReceiverThread);
            pipeClient.Connect();
            pipe.m_stream = new StreamString(pipeClient);
            var firstString = pipe.m_stream.ReadString();
            if (firstString == "StepBro is it")
            {
                pipe.m_continueReceiving = true;
                System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE starting client");
                pipe.m_thread.Start(pipe);
                return pipe;
            }
            else
            {
                pipe.Dispose();
                return null;
            }
        }

        public bool IsConnected()
        {
            return !m_disposed && m_stream != null;
        }

        public Tuple<string, string> TryGetReceived()
        {
            Tuple<string, string> received = null;
            m_received.TryDequeue(out received);
            return received;
        }

        public void Send(string message)
        {
            m_stream.WriteString(message);
        }

        public void Send(object message)
        {
            System.Diagnostics.Debug.WriteLine("Pipe Sending " + message.GetType().Name);
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.WriteIndented = true;
            options.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
            string jsonString = JsonSerializer.Serialize(message, options);
            m_stream.WriteString(message.GetType().Name + ":" + jsonString);
        }

        private static void ServerThread(object data)
        {
            var instance = data as SideKickPipe;
            var pipeStream = (instance.m_pipe as NamedPipeServerStream);

            int threadId = Thread.CurrentThread.ManagedThreadId;

            while (instance.m_continue)
            {
                Exception connectException = null;
                AutoResetEvent connectEvent = new AutoResetEvent(false);
                pipeStream.BeginWaitForConnection(
                    ar =>
                    {
                        try
                        {
                            pipeStream.EndWaitForConnection(ar);

                            System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE server connect");
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE server connect exception");
                            connectException = ex;
                        }
                        connectEvent.Set();
                    }, null);
                WaitHandle.WaitAny(new WaitHandle[] { connectEvent, instance.m_disposeEvent });

                if (instance.m_continue)
                {
                    try
                    {
                        instance.m_stream = new StreamString(instance.m_pipe);
                        instance.m_stream.WriteString("StepBro is it");

                        instance.m_continueReceiving = true;
                        ReceiverThread(instance);
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException)
                    {
                        //Console.WriteLine("ERROR: {0}", ex.Message);
                    }
                }
                pipeStream.Close();
            }
        }

        private static void ReceiverThread(object data)
        {
            var instance = data as SideKickPipe;
            //instance.m_pipe.ReadTimeout = 1000;
            System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE receiver thread started");
            try
            {
                while (instance.m_continueReceiving)
                {
                    System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE receiver: ReadString");
                    var s = instance.m_stream.ReadString();
                    if (s == null)
                    {
                        System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE Received nothing");
                        instance.m_continueReceiving = false;
                    }
                    else
                    {
                        System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE Received: " + s);
                        var colonIndex = s.IndexOf(':');
                        if (colonIndex > 0)
                        {
                            var message = new Tuple<string, string>(s.Substring(0, colonIndex), s.Substring(colonIndex + 1));
                            instance.m_received.Enqueue(message);
                            if (instance.m_continueReceiving && String.Equals(message.Item2, "close", StringComparison.InvariantCultureIgnoreCase))
                            {
                                instance.m_continueReceiving = false;
                                instance.Send(s);   // Return the close-request to the other side.
                            }
                        }
                    }
                }
            }
            finally
            {
                System.Diagnostics.Trace.WriteLine("### SIDEKICKPIPE stopping receiver");
            }
        }
    }

    // Defines the data protocol for reading and writing strings on our stream
    public class StreamString
    {
        private Stream ioStream;
        private UnicodeEncoding streamEncoding;
        private object sendSync = new object();

        public StreamString(Stream ioStream)
        {
            this.ioStream = ioStream;
            streamEncoding = new UnicodeEncoding();
        }

        public string ReadString()
        {
            var b1 = ioStream.ReadByte();
            var b2 = ioStream.ReadByte();
            if (b1 >= 0 && b2 >= 0)
            {
                int len = (b1 * 256) + b2;
                byte[] inBuffer = new byte[len];
                ioStream.Read(inBuffer, 0, len);
                return streamEncoding.GetString(inBuffer);
            }
            else
            {
                return null;
            }
        }

        public int WriteString(string outString)
        {
            byte[] outBuffer = streamEncoding.GetBytes(outString);
            int len = outBuffer.Length;
            if (len > UInt16.MaxValue)
            {
                len = (int)UInt16.MaxValue;
            }
            lock (sendSync)
            {
                ioStream.WriteByte((byte)(len / 256));
                ioStream.WriteByte((byte)(len & 255));
                ioStream.Write(outBuffer, 0, len);
                ioStream.Flush();
            }

            return outBuffer.Length + 2;
        }
    }
}