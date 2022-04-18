using mhha;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TonsinLib_2017_net48
{
    /// <summary>Server Socket Class</summary>
    public class FSocket_S
    {
        private delegate void ThreadFunc();

        private int _BUFFER_SIZE_ = 1024;

        /// <summary>recv type enum</summary>
        public enum eMODE
        {
            /// <summary>byte type</summary>
            TYPE_BY = 0,
            /// <summary>string type</summary>
            TYPE_ST = 1,
        }
        /// <summary>Number and Order of Threads</summary>
        public enum eTHREAD
        {
            /// <summary>Thread 1</summary>
            TH1 = 0,
            /// <summary>Thread 1</summary>
            TH2,
            /// <summary>Thread 1</summary>
            TH3,
            /// <summary>Thread Count</summary>
            TH_ALL,
        }
        private struct stThread
        {
            public Thread _th;
            public int _interval;
            public ManualResetEvent _thRun;
        }
        private class AsyncObject
        {
            public byte[] _buffer;
            public AsyncObject(Int32 nSize)
            {
                this._buffer = new byte[nSize];
            }
        }

        private bool _isOpen;
        private bool _acceptFlag;
        private string _lastErrorMessage;

        private eMODE _mode = eMODE.TYPE_BY;

        private stThread[] _thread;
        private ThreadFunc[] _threadFunc;

        private IPEndPoint _ipEndPoint;
        private Socket _serverSocket;
        private List<Socket> _clientSocket;

        private SafeHandle _cs;

        private FSocket_S _this;
        /// <summary>Instance this</summary>
        protected FSocket_S Instance
        {
            get
            {
                if (_this == null)
                {
                    _this = new FSocket_S();
                }
                return _this;
            }
            set
            {
                _this = value;
            }
        }
        /// <summary>FSocket_C generator</summary>
        public FSocket_S()
        {
            _this = this;

            _isOpen = false;

            _thread = new stThread[(int)eTHREAD.TH_ALL];
            _threadFunc = new ThreadFunc[(int)eTHREAD.TH_ALL];

            _threadFunc[(int)eTHREAD.TH1] = ThreadFunc1;
            _threadFunc[(int)eTHREAD.TH2] = ThreadFunc2;
            _threadFunc[(int)eTHREAD.TH3] = ThreadFunc3;
            _thread[(int)eTHREAD.TH1]._interval = 10;
            _thread[(int)eTHREAD.TH2]._interval = 10;
            _thread[(int)eTHREAD.TH3]._interval = 10;
            _thread[(int)eTHREAD.TH1]._thRun = new ManualResetEvent(false);
            _thread[(int)eTHREAD.TH2]._thRun = new ManualResetEvent(false);
            _thread[(int)eTHREAD.TH3]._thRun = new ManualResetEvent(false);

            _cs = new SafeFileHandle(IntPtr.Zero, true);
        }
        /// <summary>FSocket_S destructor</summary>
        ~FSocket_S()
        {
            StopServer();
            if (_serverSocket != null)
            {
                _serverSocket.Close();
            }
            CloseThread();
        }

        /// <summary>set recv mode</summary>
        /// <param name="mode">ref ENUM</param>
        public void SetMode(eMODE mode)
        {
            _mode = mode;
        }
        private void ThreadFunc1()
        {
            PreThread1();
            while (_thread[(int)eTHREAD.TH1]._thRun.SafeWaitHandle.IsClosed == false)
            {
                Thread.Sleep(_thread[(int)eTHREAD.TH1]._interval);
                if (_thread[(int)eTHREAD.TH1]._thRun.SafeWaitHandle.IsClosed == false)
                {
                    _thread[(int)eTHREAD.TH1]._thRun.WaitOne();
                }
                if (ProcThread1() == false)
                {
                    break;
                }
#if DEBUG
                if (Fmhha.Instance.LibraryPermit() == false)
                {
                    return;
                }
#else
            return;
#endif
            }
            PostThread1();
        }
        private void ThreadFunc2()
        {
            PreThread2();
            while (_thread[(int)eTHREAD.TH2]._thRun.SafeWaitHandle.IsClosed == false)
            {
                Thread.Sleep(_thread[(int)eTHREAD.TH2]._interval);
                if (_thread[(int)eTHREAD.TH2]._thRun.SafeWaitHandle.IsClosed == false)
                {
                    _thread[(int)eTHREAD.TH2]._thRun.WaitOne();
                }
                if (ProcThread2() == false)
                {
                    break;
                }
#if DEBUG
                if (Fmhha.Instance.LibraryPermit() == false)
                {
                    return;
                }
#else
            return;
#endif
            }
            PostThread2();
        }
        private void ThreadFunc3()
        {
            PreThread3();
            while (_thread[(int)eTHREAD.TH3]._thRun.SafeWaitHandle.IsClosed == false)
            {
                Thread.Sleep(_thread[(int)eTHREAD.TH3]._interval);
                if (_thread[(int)eTHREAD.TH3]._thRun.SafeWaitHandle.IsClosed == false)
                {
                    _thread[(int)eTHREAD.TH3]._thRun.WaitOne();
                }
                if (ProcThread3() == false)
                {
                    break;
                }
#if DEBUG
                if (Fmhha.Instance.LibraryPermit() == false)
                {
                    return;
                }
#else
            return;
#endif
            }
            PostThread3();
        }
        /// <remarks>The first run when the thread is created.</remarks>
        public virtual void PreThread1() { return; }
        /// <remarks>The first run when the thread is created.</remarks>
        public virtual void PreThread2() { return; }
        /// <remarks>The first run when the thread is created.</remarks>
        public virtual void PreThread3() { return; }
        /// <remarks>The last time the thread is closed.</remarks>
        public virtual void PostThread1() { return; }
        /// <remarks>The last time the thread is closed.</remarks>
        public virtual void PostThread2() { return; }
        /// <remarks>The last time the thread is closed.</remarks>
        public virtual void PostThread3() { return; }
        /// <returns>true : infinite, false : one time</returns>
        public virtual bool ProcThread1() { return false; }
        /// <returns>true : infinite, false : one time</returns>
        public virtual bool ProcThread2() { return false; }
        /// <returns>true : infinite, false : one time</returns>
        public virtual bool ProcThread3() { return false; }

        /// <summary>Changed the interval of the thread corresponding to the enum value.
        /// Default : 10ms
        /// </summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        /// <param name="interval">unit : ms</param>
        public void SetThreadInterval(eTHREAD threadEnum, int interval)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            if (threadEnum == eTHREAD.TH_ALL)
            {
                return;
            }
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                _thread[(int)threadEnum]._interval = interval;
            }
        }
        /// <summary>Create and Run Thread</summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        public void CreateThread(eTHREAD threadEnum)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            if (threadEnum == eTHREAD.TH_ALL)
            {
                return;
            }
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (_thread[(int)threadEnum]._th == null)
                {
                    _thread[(int)threadEnum]._th = new Thread(new ThreadStart(_threadFunc[(int)threadEnum]));
                }
                else if (_thread[(int)threadEnum]._th.IsAlive == false)
                {
                    _thread[(int)threadEnum]._th = new Thread(new ThreadStart(_threadFunc[(int)threadEnum]));
                }
                if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed)
                {
                    _thread[(int)threadEnum]._thRun = new ManualResetEvent(false);
                }
                if (_thread[(int)threadEnum]._th.IsAlive == true)
                {
                    return;
                }
                _thread[(int)threadEnum]._thRun.Set();
                _thread[(int)threadEnum]._th.Start();
            }
        }
        /// <summary>Close Thread</summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        public void CloseThread(eTHREAD threadEnum = eTHREAD.TH_ALL)
        {
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (threadEnum == eTHREAD.TH_ALL)
                {
                    for (int i = 0; i < (int)eTHREAD.TH_ALL; i++)
                    {
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == false)
                        {
                            _thread[i]._thRun.Set();
                        }
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == false)
                        {
                            _thread[i]._thRun.Close();
                        }
                    }
                }
                else
                {
                    if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == false)
                    {
                        _thread[(int)threadEnum]._thRun.Set();
                    }
                    if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == false)
                    {
                        _thread[(int)threadEnum]._thRun.Close();
                    }
                }
            }
        }
        /// <summary>Puase Thread</summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        public void PauseThread(eTHREAD threadEnum = eTHREAD.TH_ALL)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (threadEnum == eTHREAD.TH_ALL)
                {
                    for (int i = 0; i < (int)eTHREAD.TH_ALL; i++)
                    {
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == false)
                        {
                            _thread[i]._thRun.Reset();
                        }
                    }
                }
                else
                {
                    if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == false)
                    {
                        _thread[(int)threadEnum]._thRun.Reset();
                    }
                }
            }
        }
        /// <summary>Destroy Thread</summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        public void AbortThread(eTHREAD threadEnum = eTHREAD.TH_ALL)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (threadEnum == eTHREAD.TH_ALL)
                {
                    for (int i = 0; i < (int)eTHREAD.TH_ALL; i++)
                    {
                        if (_thread[i]._th == null)
                        {
                            continue;
                        }
                        _thread[i]._th.Abort();
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == false)
                        {
                            _thread[i]._thRun.Set();
                        }
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == false)
                        {
                            _thread[i]._thRun.Close();
                        }
                    }
                }
                else
                {
                    if (_thread[(int)threadEnum]._th != null)
                    {
                        _thread[(int)threadEnum]._th.Abort();
                    }
                    if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == false)
                    {
                        _thread[(int)threadEnum]._thRun.Set();
                    }
                    if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == false)
                    {
                        _thread[(int)threadEnum]._thRun.Close();
                    }
                }
            }
        }
        /// <summary>After closing the thread, wait for an end.</summary>
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        public void WaitThreadTerminate(eTHREAD threadEnum = eTHREAD.TH_ALL)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (threadEnum == eTHREAD.TH_ALL)
                {
                    for (int i = 0; i < (int)eTHREAD.TH_ALL; i++)
                    {
                        if (_thread[i]._th == null)
                        {
                            continue;
                        }
                        if (_thread[i]._thRun.SafeWaitHandle.IsClosed == true)
                        {
                            _thread[i]._th.Join();
                        }
                    }
                }
                else
                {
                    if (_thread[(int)threadEnum]._th != null)
                    {
                        if (_thread[(int)threadEnum]._thRun.SafeWaitHandle.IsClosed == true)
                        {
                            _thread[(int)threadEnum]._th.Join();
                        }
                    }
                }
            }
        }
        /// <param name="threadEnum">refer to enum eTHREAD</param>
        /// <returns>return true if thread is alive.</returns>
        public bool IsAliveThread(eTHREAD threadEnum)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return false;
            }
#else
            return false;
#endif
            if (threadEnum == eTHREAD.TH_ALL)
            {
                return false;
            }
            bool result = Enum.IsDefined(typeof(eTHREAD), threadEnum);
            if (result)
            {
                if (_thread[(int)threadEnum]._th == null)
                {
                    return false;
                }
                return _thread[(int)threadEnum]._th.IsAlive;
            }

            return false;
        }

        /// <summary>Set Socket buffer Size</summary>
        /// <param name="bufferSize">literally</param>
        public void SetbufferSize(int bufferSize)
        {
            if (bufferSize <= 256)
            {
                bufferSize = 2048;
            }
            _BUFFER_SIZE_ = bufferSize;
        }

        /// <summary>Get Last Socket Error Message</summary>
        /// <returns>string type Meesage</returns>
        public string GetSocketErrorMeesage()
        {
            return _lastErrorMessage;
        }
        /// <summary>Get Net Open</summary>
        /// <returns>Net Open : true</returns>
        public bool IsOpen()
        {
            return _isOpen;
        }
        /// <summary>Get Socket Connected</summary>
        /// <returns>Connect : ture</returns>
        public bool IsConnected()
        {
            if (_serverSocket == null)
            {
                return false;
            }
            return _serverSocket.Connected;
        }
        /// <summary>Server Open</summary>
        /// <param name="portNumber">literally</param>
        public void StartServer(int portNumber)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif
            if (IsOpen() == false)
            {
                _ipEndPoint = new IPEndPoint(IPAddress.Any, portNumber);

                if (IsOpen() == false)
                {
                    _isOpen = true;

                    if (_acceptFlag == false)
                    {
                        _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                        _serverSocket.Bind(_ipEndPoint);
                        _serverSocket.Listen(10);
                        SocketAsyncEventArgs socketAsync = new SocketAsyncEventArgs();
                        socketAsync.Completed += new EventHandler<SocketAsyncEventArgs>(AcceptAsync_Completed);
                        _clientSocket = new List<Socket>();
                        _serverSocket.AcceptAsync(socketAsync);
                    }
                }
            }
        }
        /// <summary>Server Close</summary>
        public void StopServer()
        {
            lock (_cs)
            {
                if (_serverSocket != null)
                {
                    if (_serverSocket.Connected == true)
                    {
                        _serverSocket.Shutdown(SocketShutdown.Both);
                        _serverSocket.Disconnect(false);
                    }
                    _serverSocket.Close();
                    _serverSocket.Dispose();
                    _serverSocket = null;
                }
                if (_clientSocket != null)
                {
                    for (int i = 0; i < _clientSocket.Count; i++)
                    {
                        _clientSocket.ElementAt(i).Disconnect(false);
                        _clientSocket.ElementAt(i).Close();
                        _clientSocket.ElementAt(i).Dispose();
                    }
                    _clientSocket.Clear();
                    _clientSocket = null;
                }
            }

            _isOpen = false;
            _acceptFlag = false;
        }

        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        public void SendMessage(string message)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif

            lock (_cs)
            {
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._buffer = Encoding.Default.GetBytes(message);
                SocketAsyncEventArgs socketAsync = new SocketAsyncEventArgs();
                socketAsync.SetBuffer(ao._buffer, 0, ao._buffer.Length);
                try
                {
                    if (_clientSocket != null)
                    {
                        for (int i = 0; i < _clientSocket.Count; i++)
                        {
                            _clientSocket.ElementAt(i).SendAsync(socketAsync);
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifySocketError();
                    _lastErrorMessage = ex.Message;
                }
            }
        }
        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        public void SendMessage(byte[] message)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return;
            }
#else
            return;
#endif

            lock (_cs)
            {
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._buffer = message;
                SocketAsyncEventArgs socketAsync = new SocketAsyncEventArgs();
                socketAsync.SetBuffer(ao._buffer, 0, ao._buffer.Length);
                try
                {
                    if (_clientSocket != null)
                    {
                        for (int i = 0; i < _clientSocket.Count; i++)
                        {
                            _clientSocket.ElementAt(i).SendAsync(socketAsync);
                        }
                    }
                }
                catch (Exception ex)
                {
                    NotifySocketError();
                    _lastErrorMessage = ex.Message;
                }
            }
        }


        /// <summary>Recv Data</summary>
        /// <param name="message">literally</param>
        public virtual void RecvMessage(string message)
        {

        }
        /// <summary>Recv Data</summary>
        /// <param name="message">literally</param>
        public virtual void RecvMessage(byte[] message)
        {

        }
        /// <summary>operating when server open.</summary>
        public virtual void OnAccept()
        {

        }
        /// <summary>operating when client Connection.</summary>
        public virtual void OnConnection()
        {

        }
        /// <summary>operating when client Disconnection.</summary>
        public virtual void OnDisConnection()
        {
        }
        /// <summary>Call in case of Socket Error.</summary>
        public virtual void NotifySocketError()
        {

        }
        private void AcceptAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            lock (_cs)
            {
                if (e.AcceptSocket == null)
                {
                    return;
                }

                if (_clientSocket != null)
                {
                    var clientSocket = e.AcceptSocket;
                    _clientSocket.Add(clientSocket);
                    OnConnection();
                    AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                    SocketAsyncEventArgs socketAsync = new SocketAsyncEventArgs();
                    socketAsync.SetBuffer(ao._buffer, 0, ao._buffer.Length);
                    socketAsync.UserToken = _clientSocket;
                    socketAsync.Completed += new EventHandler<SocketAsyncEventArgs>(RecvAsync_Completed);
                    clientSocket.ReceiveAsync(socketAsync);
                }

                if (_serverSocket != null)
                {
                    e.AcceptSocket = null;
                    _serverSocket.AcceptAsync(e);
                    OnAccept();
                    _acceptFlag = true;
                }
            }
        }
        private void RecvAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._buffer = e.Buffer;
                Array.Clear(ao._buffer, 0, ao._buffer.Length);
                e.SetBuffer(ao._buffer, 0, ao._buffer.Length);
                return;
            }
#else
            return;
#endif
            lock (_cs)
            {
                var clientSocket = sender as Socket;
                if (clientSocket.Connected == true && e.BytesTransferred > 0)
                {
                    Int32 recvSize = e.BytesTransferred;
                    AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                    ao._buffer = e.Buffer;
                    if (recvSize > 0)
                    {
                        if (_mode == eMODE.TYPE_BY)
                        {
                            RecvMessage(ao._buffer);
                        }
                        else
                        {
                            RecvMessage(Encoding.Default.GetString(ao._buffer));
                        }
                    }
                    Array.Clear(ao._buffer, 0, ao._buffer.Length);
                    e.SetBuffer(ao._buffer, 0, ao._buffer.Length);
                    clientSocket.ReceiveAsync(e);
                }
                else
                {
                    if (_clientSocket == null)
                    {
                        return;
                    }

                    if (_clientSocket.Count > 0)
                    {
                        try
                        {
                            clientSocket.Disconnect(false);
                            clientSocket.Dispose();
                            _clientSocket.Remove(clientSocket);
                            OnDisConnection();
                        }
                        catch (Exception ex)
                        {
                            NotifySocketError();
                            _lastErrorMessage = ex.Message;
                        }
                    }
                }
            }
        }
    }
}