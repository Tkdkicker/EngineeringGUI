using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace Drivers
{
    /// <summary>
    /// Allowed module firmware modes: DEBUG => allows UART comms
    /// </summary>
    public enum FirmwareMode
    {
        DEBUG,
        CUSTOMER,
        SWD
    }

    /// <summary>
    /// Class used for UART comms to the sfp+ module inherits from the abstract class SerialInterface
    /// </summary>
    public class SfpUART : SerialInterface
    {
        private string _writeResponseSuccess = "OK";
        private string _writeResponseFail = "NOK";
        private int _commandDelay_ms = 10;

        /// <summary>
        /// Create instance of UART driver for sfp comms we expect an 'OK' response every time we write, if everything is good
        /// </summary>
        /// <param name="deviceName">Instrument name for logging... tbd</param>
        /// <param name="comPortProperties">Comma separated key value pairs for comm port properties</param>
        public SfpUART(string deviceName, string comPortProperties)
            : base(deviceName, comPortProperties)
        {
        }

        /// <summary>
        /// Returns whether we have configured COM port with current settings and is opened
        /// </summary>
        public override bool PortOpen
        {
            get
            {
                return comPort.IsOpen;
            }
            set
            {
                if (value == false)
                {
                    comPort.Close();
                }
                else
                {
                    ConfigurePort();
                    comPort.Open();
                    comPort.DtrEnable = true;
                    value = comPort.IsOpen;
                    WriteLine("SET CRC 0");//Always need this or comms goes wrong
                }
            }
        }

        /// <summary>
        /// Writes a debug command and waits for a response
        /// </summary>
        /// <param name="data">sfp UART debug command</param>
        /// <returns></returns>
        public bool WriteCommand(string data)
        {
            string reply;
            WriteLine(data);
            Stopwatch timer = new Stopwatch();
            do
            {
                timer.Start();
                reply = ReadLine();
                Thread.Sleep(_commandDelay_ms);
            } while (!reply.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));

            if (reply.Contains(_writeResponseFail))
                return false;

            if (reply.Contains(_writeResponseSuccess))
                return true;

            return false;
        }

        /// <summary>
        /// Writes a debug GET to the serial port and returns the value as a string
        /// </summary>
        /// <returns>results of query to the UART</returns>
        public string QueryCommand(string query)
        {
            string response;
            if (comPort.IsOpen)
            {
                Read();
                WriteLine(query);
                Stopwatch timer = new Stopwatch();
                do
                {
                    timer.Start();
                    response = ReadLine();
                    Thread.Sleep(_commandDelay_ms);
                } while (!response.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));
            }
            else
            {
                throw new System.Exception("Cannot read data, serial port is closed");
            }
            if (response.Count() < 2)
                return string.Empty;//Could be in CUSTOMER mode so gives empty response
            if (response.Contains(_writeResponseFail))
                return string.Empty;//Could be unrecognised command, so gives

            return response.Split(' ')[1];
        }

        /// <summary>
        /// Writes a command that expects a list of lines back, last line should contains 'OK'
        /// </summary>
        /// <returns>Results of query to the Erymanthos UART</returns>
        public List<string> WriteReadLines(string query)
        {
            List<string> response = new List<string>();
            string line;
            if (comPort.IsOpen)
            {
                Read();
                WriteLine(query);
                Stopwatch timer = new Stopwatch();
                do
                {
                    timer.Start();
                    line = ReadLine();
                    if (line == string.Empty)
                        continue;

                    response.Add(line);
                    Thread.Sleep(_commandDelay_ms);
                } while (!line.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));
            }
            else
            {
                throw new System.Exception("Cannot read data, serial port is closed");
            }

            return response;
        }
    }
}