using System;
using System.Linq;
using System.Threading;

namespace Drivers
{
    /// <summary>
    /// Class used for serial comms to the Tenma power supply unit inherits from the abstract class SerialInterface
    /// </summary>
    public class Tenma2000 : SerialInterface
    {
        private int _channelNumber = 1;
        private int _commandDelay_ms = 30;

        /// <summary>
        ///  Create instance of driver for serial comms
        /// </summary>
        /// <param name="deviceName">Instrument name for logging... tbd</param>
        /// <param name="comPortProperties">Comma separated key value pairs for comm port properties</param>
        public Tenma2000(string deviceName, string comPortProperties)
            : base(deviceName, comPortProperties)
        {
            string[] items = comPortProperties.Split(',');
            //Power supply can have more than one output
            if (items.ToList().FirstOrDefault(s => s.Contains("CHANNEL")) != null)
                _channelNumber = Convert.ToInt32(items.ToList().FirstOrDefault(s => s.Contains("CHANNEL")).Split('=')[1]);
        }

        /// <summary>
        /// Set the output voltage in V
        /// </summary>
        /// <param name="voltage"></param>
        /// <returns></returns>
        public void SetVoltage(float voltage)
        {
            Write($"VSET{_channelNumber}:{voltage}");
            Thread.Sleep(_commandDelay_ms);
        }

        /// <summary>
        /// Set the output current in A
        /// </summary>
        /// <param name="current">Output current to set in A</param>
        /// <returns></returns>
        public void SetCurrent(float current)
        {
            Write($"ISET{_channelNumber}:{current}");
            Thread.Sleep(_commandDelay_ms);
        }

        /// <summary>
        /// Returns the identity string of the power supply
        /// </summary>
        /// <returns></returns>
        public string Identity()
        {
            return Query("*IDN?");
        }

        /// <summary>
        /// Turns the output on/off
        /// </summary>
        /// <param name="outputOn">If true then turn on output, if false turn off output</param>
        public void Ouput(bool outputOn)
        {
            if (outputOn)
            {
                Write($"OUT1");
            }
            else
            {
                Write($"OUT0");
            }
        }

        /// <summary>
        /// Returns state of output
        /// </summary>
        /// <returns>True if output on</returns>
        public bool OutputOn()
        {
            Write("STATUS?");
            Thread.Sleep(_commandDelay_ms);
            byte[] response = ReadByteArray();
            if (response.Length == 1)
            {
                int output = response[0] & 0x40;
                if (output == 0x40)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Turn off then turn on
        /// </summary>
        public void ToggleOutput()
        {
            Ouput(outputOn: false);
            Thread.Sleep(4000);
            Ouput(outputOn: true);
            Thread.Sleep(3000);
        }
    }
}