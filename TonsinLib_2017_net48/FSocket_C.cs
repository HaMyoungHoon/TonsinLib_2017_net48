using mhha;
using Microsoft.Win32.SafeHandles;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace TonsinLib_2017_net48
{
    /// <summary>Client Socket Class</summary>
    public class FSocket_C
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
            public Socket _working;
            public AsyncObject(Int32 nSize)
            {
                this._buffer = new byte[nSize];
            }
        }

        private bool _connectFlag;
        private bool _disconnectFlag;
        private bool _autoReconnect;
        private string _lastErrorMessage;

        private eMODE _mode = eMODE.TYPE_BY;

        private stThread[] _thread;
        private ThreadFunc[] _threadFunc;

        private IPEndPoint _ipEndPoint;
        private Socket _clientSocket;
        private AsyncCallback _recvHandler;
        private AsyncCallback _sendHandler;
        private AsyncCallback _disconnect;

        private SocketAsyncEventArgs _connectAsync;

        private SafeHandle _cs;

        private Thread _watchingConnection;

        private FSocket_C _this;
        /// <summary>Instance this</summary>
        protected FSocket_C Instance
        {
            get
            {
                if (_this == null)
                {
                    _this = new FSocket_C();
                }
                return _this;
            }
            set
            {
                _this = value;
            }
        }
        /// <summary>FSocket_C generator</summary>
        public FSocket_C()
        {
            _this = this;

            _connectFlag = false;
            _disconnectFlag = false;
            _autoReconnect = true;

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

            _recvHandler = new AsyncCallback(handleDataReceive);
            _sendHandler = new AsyncCallback(handleDataSend);
            _disconnect = new AsyncCallback(handleDisconnect);

            _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            _connectAsync = new SocketAsyncEventArgs();
            _connectAsync.Completed += new EventHandler<SocketAsyncEventArgs>(ConnectAsync_Completed);

            _cs = new SafeFileHandle(IntPtr.Zero, true);
        }
        /// <summary>FSocket_C destructor</summary>
        ~FSocket_C()
        {
            StopClient();
            if (_clientSocket != null)
            {
                _clientSocket.Close();
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

        /// <summary>Automatically Reconnection with Server</summary>
        /// <param name="setAutoReconnection">default true</param>
        public void SetAutoReConnectionMode(bool setAutoReconnection = true)
        {
            _autoReconnect = setAutoReconnection;
        }
        /// <summary>Get Last Socket Error Message</summary>
        /// <returns>string type Meesage</returns>
        public string GetSocketErrorMeesage()
        {
            return _lastErrorMessage;
        }

        /// <summary>Get Socket Connected</summary>
        /// <returns>Connect : ture</returns>
        public bool IsConnected()
        {
            if (_clientSocket == null)
            {
                return false;
            }
            return _clientSocket.Connected;
        }

        /// <summary>Connect to Server</summary>
        /// <param name="ipAddress">literally</param>
        /// <param name="portNumber">literally</param>
        /// <returns>Connect : true</returns>
        public bool ConnectStart(IPAddress ipAddress, int portNumber)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return false;
            }
#else
            return false;
#endif
            if (_clientSocket == null)
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            try
            {
                if (_clientSocket.Available == 0)
                {

                }
            }
            catch
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            }

            if (_connectFlag == false)
            {
                //                _ipEndPoint = new IPEndPoint(Dns.GetHostByName(Dns.GetHostName()).AddressList[0], portNumber);
                _ipEndPoint = new IPEndPoint(ipAddress, portNumber);
                _connectAsync.RemoteEndPoint = _ipEndPoint;
                _connectAsync.UserToken = _clientSocket;
                //                _clientSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);
                _clientSocket.LingerState = new LingerOption(false, 10);
                _clientSocket.ConnectAsync(_connectAsync);
                _connectFlag = true;
            }

            return IsConnected();
        }
        /// <summary>Client Close</summary>
        public void StopClient()
        {
            lock (_cs)
            {
                try
                {
                    if (_disconnectFlag == false)
                    {
                        if (IsConnected() == true)
                        {
                            _clientSocket.Disconnect(false);
                        }
                        //                        _clientSocket.BeginDisconnect(false, _disconnect, _clientSocket);
                        _clientSocket.Close();
                        OnDisconnect();
                        //                        _clientSocket.EndDisconnect(ad);
                        _clientSocket.Dispose();
                        _clientSocket = null;
//                        _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                        _disconnectFlag = false;
                        //                        _disconnectFlag = true;
                    }
                }
                catch
                {

                }
                try
                {
                    if (_watchingConnection != null)
                    {
                        if (_watchingConnection.IsAlive == true)
                        {
                            _watchingConnection.Abort();
                        }
                    }
                }
                catch
                {

                }
            }
        }

        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        public void SendMessage(string message)
        {
            lock (_cs)
            {
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._buffer = Encoding.Default.GetBytes(message);
                ao._working = _clientSocket;
                try
                {
                    ao._working.BeginSend(ao._buffer, 0, ao._buffer.Length, SocketFlags.None, _sendHandler, ao);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        StopClient();
                    }
                    _lastErrorMessage = ex.Message;
                    NotifySocketError();
                }
            }
        }
        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        public void SendMessage(byte[] message)
        {
            lock (_cs)
            {
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._buffer = message;
                ao._working = _clientSocket;
                try
                {
                    ao._working.BeginSend(ao._buffer, 0, ao._buffer.Length, SocketFlags.None, _sendHandler, ao);
                }
                catch (SocketException ex)
                {
                    if (ex.ErrorCode == (int)SocketError.ConnectionAborted)
                    {
                        StopClient();
                    }
                    _lastErrorMessage = ex.Message;
                    NotifySocketError();
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
        /// <summary>operating when server Connection.</summary>
        public virtual void OnConnect()
        {

        }
        /// <summary>operating when server Disconnection.</summary>
        public virtual void OnDisconnect()
        {

        }
        /// <summary>Call in case of Socket Error.</summary>
        public virtual void NotifySocketError()
        {

        }
        private void WatchingConnection()
        {
            while (IsConnected() == false)
            {
                try
                {
                    _clientSocket.Connect(_ipEndPoint);
                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
            if (IsConnected() == true)
            {
                if (_connectFlag == true)
                {
                    OnConnect();
                    _connectFlag = false;
                    AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                    ao._working = _clientSocket;
                    _clientSocket.BeginReceive(ao._buffer, 0, ao._buffer.Length, SocketFlags.None, _recvHandler, ao);
                }
            }
        }
        private void ConnectAsync_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.ConnectSocket != null && IsConnected() == true)
            {
                _connectFlag = false;
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._working = _clientSocket;
                try
                {
                    _clientSocket.BeginReceive(ao._buffer, 0, ao._buffer.Length, SocketFlags.None, _recvHandler, ao);
                    OnConnect();
                }
                catch
                {
                    StopClient();
                }
            }
            else
            {
                if (_watchingConnection == null)
                {
                    _watchingConnection = new Thread(new ThreadStart(WatchingConnection));
                    _watchingConnection.Start();
                }
                else if (_watchingConnection.IsAlive == false)
                {
                    _watchingConnection = new Thread(new ThreadStart(WatchingConnection));
                    _watchingConnection.Start();
                }
            }
        }
        private void handleConnect(IAsyncResult ac)
        {
            if (IsConnected())
            {
                OnConnect();
                _clientSocket.EndConnect(ac);
                _connectFlag = false;
                AsyncObject ao = new AsyncObject(_BUFFER_SIZE_);
                ao._working = _clientSocket;
                ao._buffer = new byte[_BUFFER_SIZE_];
                _clientSocket.BeginReceive(ao._buffer, 0, ao._buffer.Length, SocketFlags.None, _recvHandler, ao);
            }
        }
        private void handleDisconnect(IAsyncResult ad)
        {
            try
            {
                OnDisconnect();
                _clientSocket.EndDisconnect(ad);
                _clientSocket.Dispose();
                //                 _clientSocket = null;
                _disconnectFlag = false;
            }
            catch (Exception ex)
            {
                _lastErrorMessage = ex.Message;
                NotifySocketError();
            }
        }
        private void handleDataReceive(IAsyncResult ar)
        {
            lock (_cs)
            {
                if (_clientSocket == null)
                {
                    return;
                }
                try
                {
                    Int32 nRecvSize = _clientSocket.EndReceive(ar);
                    AsyncObject ao = (AsyncObject)ar.AsyncState;
                    ao._working = _clientSocket;
                    if (nRecvSize > 0)
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
                    if (_clientSocket.Connected == true)
                    {
                        ao._working.BeginReceive(ao._buffer, 0, _BUFFER_SIZE_, SocketFlags.None, _recvHandler, ao);
                    }
                    else
                    {
                        _connectFlag = false;
                        _clientSocket.Disconnect(false);
                    }
                }
                catch (SocketException ex)
                {
                    _lastErrorMessage = ex.Message;
                    NotifySocketError();

                    if (_autoReconnect == true)
                    {
                        StopClient();
                        ConnectStart(_ipEndPoint.Address, _ipEndPoint.Port);
                    }
                }
            }
        }
        private void handleDataSend(IAsyncResult ar)
        {
            Int32 nSendSize = _clientSocket.EndSend(ar);
            if (nSendSize > 0)
            {
            }
        }
    }
}