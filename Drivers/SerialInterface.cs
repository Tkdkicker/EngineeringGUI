using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;

namespace Drivers
{
    /// <summary>
    /// Class uses the .NET Framework SerialPort class for comms.
    /// Abstract calls should be inherited
    /// </summary>
    public abstract class SerialInterface
    {
        #region private fields

        protected int commandDelay_ms = 10;

        private int _baudRate = 9600;
        private int _dataBits = 8;
        private StopBits _stopBits = StopBits.One;
        private Parity _parity = Parity.None;
        private Handshake _handShakeType = Handshake.None;
        private string _newLine = "\n";

        private int _writeBufferSize = 1042;
        private int _readBufferSize = 1042;

        private string _device;

        #endregion private fields

        #region protected fields

        protected SerialPort comPort = null;
        protected int timeout_ms = 2000;

        #endregion protected fields

        #region public properties

        public int BaudRate { get => _baudRate; set => _baudRate = value; }
        public int Databits { get => _dataBits; set => _dataBits = value; }

        public int Timeout_ms { get => timeout_ms; set => timeout_ms = value; }

        public StopBits Stopbits { get => _stopBits; set => _stopBits = value; }

        public Parity Parity { get => _parity; set => _parity = value; }

        public int WriteBufferSize { get => _writeBufferSize; set => _writeBufferSize = value; }
        public int ReadBufferSize { get => _readBufferSize; set => _readBufferSize = value; }
        public Handshake HandShakeType { get => _handShakeType; set => _handShakeType = value; }

        public string NewLine { get => _newLine; set => _newLine = value; }

        /// <summary>
        /// returns whether we have configured com port with current settings and is opened
        /// </summary>
        public virtual bool PortOpen
        {
            get
            {
                return comPort.IsOpen;
            }
            set
            {
                ConfigurePort();
                comPort.Open();
                comPort.DtrEnable = true;
                value = comPort.IsOpen;
            }
        }

        #endregion public properties

        #region constructor

        /// <summary>
        /// abstract class used for serial comms on com port
        /// </summary>
        /// <param name="deviceName">name of the device using the port</param>
        /// <param name="portName">specified port name eg. COM1</param>
        public SerialInterface(string deviceName, string portProperties)
        {
            comPort = new SerialPort(GetPortId(portProperties));

            comPort.NewLine = "\n";
            comPort.WriteTimeout = timeout_ms;
            comPort.ReadTimeout = timeout_ms;
            _device = deviceName;
            // extract the port properties form the comPortProperties string
            // example string:
            // "PORT = COM5,BAUDRATE = 62500,READBUFFER = 8192,WRITEBUFFER = 8192"
            //  the base class has default properties set
            SetPortProperties(portProperties);
        }

        #endregion constructor

        /// <summary>
        /// Searches for the COM port identifier
        /// </summary>
        /// <returns>COM port identifier</returns>
        private static string GetPortId(string comPortProperties)
        {
            string[] items = comPortProperties.Split(',');

            string comPortItem = items.ToList().FirstOrDefault(s => s.Contains("PORT"));

            items = comPortItem.Split('=');

            return items[1];
        }

        /// <summary>
        /// Sets up the port properties
        /// </summary>
        /// <param name="comPortProperties">String of comma seperated key value pair properties</param>
        private void SetPortProperties(string comPortProperties)
        {
            string[] items = comPortProperties.Split(',');

            //There always has to be a port identifier
            string comPortItem = items.ToList().FirstOrDefault(s => s.Contains("PORT")).Split('=')[1];

            if (items.ToList().FirstOrDefault(s => s.Contains("BAUDRATE")).Split('=')[1] != null)
                BaudRate = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("BAUDRATE")).Split('=')[1]);

            if (items.ToList().FirstOrDefault(s => s.Contains("DATABITS")) != null)
                Databits = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("DATABITS")).Split('=')[1]);

            if (items.ToList().FirstOrDefault(s => s.Contains("STOPBITS")) != null)
            {
                string stopBits = (items.ToList().FirstOrDefault(s => s.Contains("STOPBITS")).Split('=')[1]);
                Stopbits = (StopBits)Enum.Parse(typeof(StopBits), stopBits);
            }

            if (items.ToList().FirstOrDefault(s => s.Contains("HANDSHAKE")) != null)
            {
                string handshake = (items.ToList().FirstOrDefault(s => s.Contains("HANDSHAKE")).Split('=')[1]);
                HandShakeType = (Handshake)Enum.Parse(typeof(Handshake), handshake);
            }

            if (items.ToList().FirstOrDefault(s => s.Contains("PARITY")) != null)
            {
                string parity = (items.ToList().FirstOrDefault(s => s.Contains("PARITY")).Split('=')[1]);
                Parity = (Parity)Enum.Parse(typeof(Parity), parity);
            }

            if (items.ToList().FirstOrDefault(s => s.Contains("READBUFFER")) != null)
                ReadBufferSize = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("READBUFFER")).Split('=')[1]);

            if (items.ToList().FirstOrDefault(s => s.Contains("WRITEBUFFER")) != null)
                WriteBufferSize = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("WRITEBUFFER")).Split('=')[1]);

            if (items.ToList().FirstOrDefault(s => s.Contains("NEWLINE")) != null)
                NewLine = (items.ToList().FirstOrDefault(s => s.Contains("NEWLINE")).Split('=')[1]);

            if (items.ToList().FirstOrDefault(s => s.Contains("TIMEOUT")) != null)
            {
                Timeout_ms = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("TIMEOUT")).Split('=')[1]);
            }
        }

        #region protected methods

        /// <summary>
        /// Configures the COM port for baudRate etc, can only be done when port is closed
        /// </summary>
        protected void ConfigurePort()
        {
            comPort.BaudRate = _baudRate;
            comPort.DataBits = _dataBits;
            comPort.StopBits = _stopBits;
            comPort.Parity = _parity;
            comPort.Handshake = _handShakeType;

            comPort.RtsEnable = false;

            comPort.ReadBufferSize = _readBufferSize;
            comPort.WriteBufferSize = _writeBufferSize;
            comPort.NewLine = _newLine;
            comPort.WriteTimeout = timeout_ms;
            comPort.ReadTimeout = timeout_ms;
        }

        /// <summary>
        /// Wwrite string of data to the serial port
        /// </summary>
        protected void Write(string data)
        {
            if (comPort.IsOpen)
            {
                comPort.Write(data);
            }
            else
            {
                throw new Exception("Cannot write data, serial port is closed");
            }
        }

        /// <summary>
        /// Write string of data to the serial port, appended by the string identified by the NewLine propery
        /// </summary>
        protected void WriteLine(string data)
        {
            if (comPort.IsOpen)
            {
                comPort.WriteLine(data);
            }
            else
            {
                throw new System.Exception("Cannot write data, serial port is closed");
            }
        }

        /// <summary>
        /// Reads data from the ports input buffer until the string identified by
        /// the NewLine property is matched
        /// </summary>
        /// <returns>A line of data read from the serial port</returns>
        protected string ReadLine()
        {
            if (comPort.IsOpen)
            {
                if (comPort.BytesToRead > 0)
                {
                    return comPort.ReadLine();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new Exception("Cannot read data, serial port is closed");
            }
        }

        /// <summary>
        /// Writes a query to the serial port and returns the response
        /// </summary>
        /// <returns></returns>
        protected string Query(string query)
        {
            if (comPort.IsOpen)
            {
                Read();
                WriteLine(query);
                return ReadLine();
            }
            else
            {
                throw new Exception("Cannot read data, serial port is closed");
            }
        }

        /// <summary>
        /// Reads all the data at the ports input buffer as a byte array
        /// </summary>
        /// <returns></returns>
        protected byte[] ReadByteArray()
        {
            byte[] response = new byte[0];
            int bytesToRead = this.comPort.BytesToRead;
            response = new byte[bytesToRead];
            for (int i = 0; i < bytesToRead; i++)
            {
                response[i] = (byte)comPort.ReadByte();
            }
            return response;
        }

        #endregion protected methods

        #region public methods

        /// <summary>
        /// Reads all the data currently in the serial ports input buffer
        /// </summary>
        /// <returns>Data read from serial port</returns>
        public string Read()
        {
            if (comPort.IsOpen)
            {
                if (comPort.BytesToRead > 0)
                {
                    return comPort.ReadExisting();
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                throw new Exception("Cannot read data, serial port is closed");
            }
        }

        #endregion public methods

        public static List<string> GetPorts()
        {
            string[] comPorts = SerialPort.GetPortNames();
            return comPorts.ToList();
        }
    }
}