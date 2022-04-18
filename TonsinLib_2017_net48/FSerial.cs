using System;
using Microsoft.Win32.SafeHandles;
using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Threading;
using mhha;

namespace TonsinLib_2017_net48
{
    /// <summary>Serial Class</summary>
    public class FSerial
    {
        private delegate void ThreadFunc();

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

        /// <summary>Read Data Type</summary>
        public enum eREAD_TYPE
        {
            /// <summary>Read Byte</summary>
            BY = 0,
            /// <summary>Read String</summary>
            ST = 1,
        }

        private struct stThread
        {
            public Thread _th;
            public int _interval;
            public ManualResetEvent _thRun;
        }

        private stThread[] _thread;
        private ThreadFunc[] _threadFunc;

        private SerialPort _serialPort;

        private SafeHandle _cs;

        private eREAD_TYPE _ReadType;
        private FSerial _this;
        /// <summary>Instance this</summary>
        protected FSerial Instance
        {
            get
            {
                if (_this == null)
                {
                    _this = new FSerial();
                }
                return _this;
            }
            set
            {
                _this = value;
            }
        }
        /// <summary>FSerial generator</summary>
        public FSerial()
        {
            _this = this;

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
            
            _cs = new SafeFileHandle(IntPtr.Zero, false);

            _ReadType = eREAD_TYPE.BY;
        }
        /// <summary>FSerial destructor</summary>
        ~FSerial()
        {
            PortClose();
            CloseThread();
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

        /// <summary>Serial Port InitialPort</summary>
        /// <param name="portName">Com Port</param>
        /// <param name="baudRate">literally</param>
        /// <param name="parity">literally</param>
        /// <param name="dataBits">literally</param>
        /// <param name="stopBits">literally</param>
        /// <param name="handshake">literally</param>
        /// <param name="readTimeout">literally</param>
        /// <param name="writeTimeout">literally</param>
        public void InitialPort(string portName = "COM1", int baudRate = 9600, Parity parity = Parity.None, int dataBits = 8,
            StopBits stopBits = StopBits.One, Handshake handshake = Handshake.None, int readTimeout = 500, int writeTimeout = 500)
        {
            if(_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if(_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.PortName = portName;
            _serialPort.BaudRate = baudRate;
            _serialPort.Parity = parity;
            _serialPort.DataBits = dataBits;
            _serialPort.StopBits = stopBits;
            _serialPort.Handshake = handshake;
            _serialPort.Encoding = new System.Text.ASCIIEncoding();
            _serialPort.DataReceived += _serialPort_DataReceived;
            _serialPort.ReadTimeout = readTimeout;
            _serialPort.WriteTimeout = writeTimeout;
        }

        /// <summary>Setting Com Port</summary>
        /// <param name="portName">Com Port</param>
        public void SetPortName(string portName = "COM1")
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.PortName = portName;
        }
        /// <summary>Setting Port BaudRate</summary>
        /// <param name="baudRate">literally</param>
        public void SetBaudRate(int baudRate = 9600)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            // 어떤 괴팍한 놈은 통신 도중에 이걸 바꿈
//            if (_serialPort.IsOpen == true)
//            {
//                return;
//            }
            _serialPort.BaudRate = baudRate;
        }
        /// <summary>Setting Port Parity Bit</summary>
        /// <param name="parity">literally</param>
        public void SetParity(Parity parity = Parity.None)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.Parity = parity;
        }
        /// <summary>Setting Port DataBits</summary>
        /// <param name="dataBits">literally</param>
        public void SetDataBits(int dataBits = 8)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.DataBits = dataBits;
        }
        /// <summary>Setting Port StopBits</summary>
        /// <param name="stopBits">literally</param>
        public void SetStopBits(StopBits stopBits = StopBits.None)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.StopBits = stopBits;
        }
        /// <summary>Setting Port HandShake</summary>
        /// <param name="handshake">literally</param>
        public void SetHandshake(Handshake handshake = Handshake.None)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.Handshake = handshake;
        }
        /// <summary>Setting Port ReadTimeout</summary>
        /// <param name="readTimeout">literally</param>
        public void SetReadTimeout(int readTimeout = 500)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.ReadTimeout = readTimeout;
        }
        /// <summary>Setting Port WriteTimeout</summary>
        /// <param name="writeTimeout">literally</param>
        public void SetWriteTimeout(int writeTimeout = 500)
        {
            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
            }
            if (_serialPort.IsOpen == true)
            {
                return;
            }
            _serialPort.WriteTimeout = writeTimeout;
        }

        /// <summary>Setting Read Type</summary>
        /// <param name="readtype">literally</param>
        /// <returns>Set OK : True</returns>
        public bool SetReadType(eREAD_TYPE readtype)
        {
            if(_serialPort != null)
            {
                if(_serialPort.IsOpen == true)
                {
                    return false;
                }
            }

            if (Enum.IsDefined(readtype.GetType(), readtype) == false) 
            {
                return false;
            }

            _ReadType = readtype;
            return true;
        }

        /// <summary>Check Serial Port Open State</summary>
        /// <returns>Port Open : true</returns>
        protected bool IsOpen()
        {
            if (_serialPort == null)
            {
                return false;
            }

            return _serialPort.IsOpen;
        }
        /// <summary>Serial Port Open</summary>
        /// <returns>if not InitialPort : return false</returns>
        protected bool PortOpen()
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                return false;
            }
#else
            return false;
#endif

            if (_serialPort == null)
            {
                _serialPort = new SerialPort();
                InitialPort();
            }

            if (_serialPort.PortName.Length == 0)
            {
                return false;
            }
            else
            {
                if (_serialPort.IsOpen == false)
                {
                    try
                    {
                        _serialPort.Open();
                    }
                    catch
                    {
                        return false;
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>Serial Port Close</summary>
        protected void PortClose()
        {
            if (_serialPort == null)
            {
                return;
            }
            else
            {
                if (_serialPort.IsOpen == true)
                {
                    _serialPort.Close();
                }
            }
        }

        private void _serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                _serialPort.ReadExisting();
                return;
            }
#else
            return;
#endif
            try
            {
                switch (_ReadType)
                {
                    case eREAD_TYPE.BY:
                        {
                            int bufferOffset = 0;
                            int bytesToRead = _serialPort.BytesToRead;
                            byte[] msg = new byte[bytesToRead];

                            while (bytesToRead > 0)
                            {
                                int readMsg = _serialPort.Read(msg, bufferOffset, bytesToRead - bufferOffset);
                                bytesToRead = bytesToRead - readMsg;
                                bufferOffset = bufferOffset + readMsg;
                            }

                            RecvMessageB(msg);
                        }
                        break;
                    case eREAD_TYPE.ST:
                        {
                            RecvMessageS(_serialPort.ReadExisting());
                        }
                        break;
                    default: break;
                }
            }
            catch
            {

            }
        }

        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        protected void SendMessage(string message)
        {
            if (IsOpen() == false)
            {
                return;
            }
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                _serialPort.Close();
                return;
            }
#else
            return;
#endif
            _serialPort.Write(message);
        }
        /// <summary>Send Data</summary>
        /// <param name="message">literally</param>
        /// <param name="offset">literally</param>
        protected void SendMessage(byte[] message, int offset = 0)
        {
            if (IsOpen() == false)
            {
                return;
            }
#if DEBUG
            if (Fmhha.Instance.LibraryPermit() == false)
            {
                _serialPort.Close();
                return;
            }
#else
            return;
#endif
            _serialPort.Write(message, offset, message.Length);
        }
        /// <summary>Recv Data</summary>
        /// <param name="message">literally</param>
        public virtual void RecvMessageB(byte[] message)
        {

        }
        /// <summary>Recv Data</summary>
        /// <param name="message">literally</param>
        public virtual void RecvMessageS(string message)
        {

        }
    }
}