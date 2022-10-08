using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace Drivers
{
    /// <summary>
    /// Module states
    /// </summary>
    public enum ModuleState
    {
        MS_INIT = 0,         // Transient state
        MS_LPWR,                // Steady state
        MS_GO2HPWR,         // Transient state
        MS_HPWR,                // Steady state
        MS_BIST,                // Transient state
        MS_BURNIN,          // Transient state
        MS_TUNE,                // Transient state
        MS_TXOFF,               // Steady state
        MS_GO2TXON,         // Transient state
        MS_TXON,                // Steady state
        MS_GO2TXOFF,        // Transient state
        MS_GO2FAULT,        // Transient state
        MS_FAULT,               // Steady state
        MS_INVALID,
    }

    public enum GpioIrqType
    {
        GPIO_IRQ_DOWN = 0,
        GPIO_IRQ_UP,
        GPIO_IRQ_LIMIT
    }

    /// <summary>
    /// Each board has 6 positions
    /// </summary>
    public enum ModulePosition
    {
        bay_0 = 0,
        bay_1,
        bay_2,
        bay_3,
        bay_4,
        bay_5
    }

    //2 LEDs per position
    public enum LedNumber
    {
        LED1 = 0,
        LED2
    }

    public enum LedColour
    {
        OFF = 0,
        Red,
        Green,
        Orange,
        Blue,
        Purple,
        Teal,
        White,
        notUsed,
        RedBlinking,
        GreenBlinking,
        OrangeBlinking,
        BlueBlinking,
        PurpleBlinking,
        TealBlinking,
        WhiteBlinking
    }

    /// <summary>
    /// GPIO pins: Includes the virtual pins
    /// </summary>
    public enum GPIOPins
    {
        GPIO_CPF_OR_CFP2 = 0,      // 0
        GPIO_DAC_RESET,           //  1
        GPIO_DBG_UART_MUX_0,      //  2
        GPIO_DBG_UART_MUX_1,      //  3
        GPIO_DBG_UART_MUX_2,      //  4
        GPIO_FPGA_IRQ0,           //  5
        GPIO_FPGA_IRQ1,           //  6
        GPIO_FPGA_IRQ2,           //  7
        GPIO_FPGA_IRQ3,           //  8
        GPIO_IO_INPUT_SEL_MOD0,   //  9
        GPIO_IO_INPUT_SEL_MOD1,   // 10
        GPIO_IO_INPUT_SEL_MOD2,   // 11
        GPIO_IO_INPUT_SEL_MOD3,   // 12
        GPIO_IO_INPUT_SEL_MOD4,   // 13
        GPIO_IO_INPUT_SEL_MOD5,   // 14
        GPIO_IO_LATCH_MOD0,       // 15
        GPIO_IO_LATCH_MOD1,       // 16
        GPIO_IO_LATCH_MOD2,       // 17
        GPIO_IO_LATCH_MOD3,       // 18
        GPIO_IO_LATCH_MOD4,       // 19
        GPIO_IO_LATCH_MOD5,       // 20
        GPIO_LED_Q1,              // 21
        GPIO_LED_Q2,              // 22
        GPIO_LED0_BLUE,           // 23 // Note: Virtualised IO Pins need to be in a single contiguous block
        GPIO_LED0_GREEN,          // 24
        GPIO_LED0_RED,            // 25
        GPIO_LED1_BLUE,           // 26
        GPIO_LED1_GREEN,          // 27
        GPIO_LED1_RED,            // 28
        GPIO_MOD_ABSENT,          // 29
        GPIO_MOD_ALRMn0,          // 30
        GPIO_MOD_ALRMn1,          // 31
        GPIO_MOD_ALRMn2,          // 32
        GPIO_MOD_CTRLn0,          // 33
        GPIO_MOD_CTRLn1,          // 34
        GPIO_MOD_CTRLn2,          // 35
        GPIO_MOD_GLOBAL_ALRM,     // 36
        GPIO_MOD_LOW_PWR,         // 37
        GPIO_MOD_MDIO_ADDR_0,     // 38
        GPIO_MOD_MDIO_ADDR_1,     // 39
        GPIO_MOD_MDIO_ADDR_2,     // 40
        GPIO_MOD_MDIO_ADDR_3,     // 41
        GPIO_MOD_MDIO_ADDR_4,     // 42
        GPIO_MOD_RESET,           // 43
        GPIO_MOD_RX_LOSS,         // 44
        GPIO_MOD_TX_DIS,          // 45
        GPIO_MOD_VSUP_INH_CH0,    // 46
        GPIO_MOD_VSUP_INH_CH1,    // 47
        GPIO_MOD_VSUP_INH_CH2,    // 48
        GPIO_MOD_VSUP_INH_CH3,    // 49
        GPIO_MOD_VSUP_INH_CH4,    // 50
        GPIO_MOD_VSUP_INH_CH5,    // 51
        GPIO_TEC_INHIBIT_CH0,     // 52
        GPIO_TEC_INHIBIT_CH1,     // 53
        GPIO_TEC_INHIBIT_CH2,     // 54
        GPIO_TEC_INHIBIT_CH3,     // 55
        GPIO_TEC_INHIBIT_CH4,     // 56
        GPIO_TEC_INHIBIT_CH5,     // 57
        GPIO_TEC_POL_CH0,         // 58
        GPIO_TEC_POL_CH1,         // 59
        GPIO_TEC_POL_CH2,         // 60
        GPIO_TEC_POL_CH3,         // 61
        GPIO_TEC_POL_CH4,         // 62
        GPIO_TEC_POL_CH5,         // 63
        GPIO_VMOD_ALERT_0,        // 64
        GPIO_VMOD_ALERT_1,        // 65
        GPIO_VMOD_ALERT_2,        // 66
        GPIO_VMOD_ALERT_3,        // 67
        GPIO_VMOD_ALERT_4,        // 68
        GPIO_VMOD_ALERT_5,        // 69
        CFG_REV_PD0,              // 70
        CFG_REV_PD1,              // 71
        CFG_REV_PD2,              // 72
        CFG_SFPTYPEn_PD3,         // 73 -- Is grounded on SFP/QSFP low power boards. Grounded = asserted in current config.
        CFG_BAYCNT_PD0,           // 74
        CFG_BAYCNT_PD1,           // 75
        GPIO_DAC_SYNC            // 76
    }

    /// <summary>
    /// Class used for UART comms to the Erymanthos board
    /// Inherits from the abstract class SerialInterface
    /// </summary>
    public class Erymanthos : SerialInterface
    {
        private string _writeResponseSuccess = "OK";
        private string _writeResponseFail = "NOK";
        private int _commandDelay_ms = 50;

        private const int _Gpio_first_virstualisation = 23;  // GPIO_LED0_BLUE: The first IOPin that gets virtualised per Module
        private const int _Gpio_last_virstualisation = 45;

        //I2C register names and information for the selcted module
        private ISfpMemoryBase _moduleMemory;

        public ISfpMemoryBase ModuleMemory { get => _moduleMemory; set => _moduleMemory = value; }

        private int _moduleNvmWriteDelay = 2000;//To ensure the NVM write has happened as NVM now a background process
        private int _modulePowerOffDelay = 3000;//bad things seem to happen if we do not power off and then delay, before power on
        private int _modulePowerOnDelay = 3000;//after a power on, time to wait before carrying on
        private int _moduleSetUartDelay = 500;//wait for UART to switch...no idea how long I need....but Erymanthos boards are ropey

        #region properties

        public int ModuleSetUartDelay { get => _moduleSetUartDelay; }

        #endregion properties

        #region constructor

        /// <summary>
        ///  Create instance of Erymanthos driver for serial comms:
        ///  We expect an 'OK' response every time we write if everything is good,
        ///  'NOK' when not good
        /// </summary>
        /// <param name="deviceName">instrument name for logging... tbd</param>
        /// <param name="comPortProperties">comma separated key value pairs for comm port properties</param>
        public Erymanthos(string deviceName, string comPortProperties)
            : base(deviceName, comPortProperties)
        {
            timeout_ms = 2000; // because of the nasty IrqS
            _moduleMemory = new SfpMemoryBase();
        }

        #endregion constructor

        #region overrides

        /// <summary>
        /// Returns whether we have configured com port with current settings and is opened
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
                    WriteLine("SET CRC 0");//Always need this or comms goes wrong dude
                }
            }
        }

        #endregion overrides

        #region public methods

        /// <summary>
        /// Strip out the firmware revision tag from the INF:VER command
        /// </summary>
        /// <returns>Firmware revisionon tag</returns>
        public string GetFirmwareRevionTag()
        {
            List<string> versionInfo = WriteReadLines("INF:VER");

            var revisionLine = versionInfo.FirstOrDefault(stringToCheck => stringToCheck.Contains("Revision Tag"));
            if (revisionLine != null)
            {
                string[] revTag = revisionLine.Split(':');
                if (revTag.Length == 2)
                    return revTag[1];
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the Erymanthos serial number
        /// </summary>
        /// <returns></returns>
        public string GetSerialNumber()
        {
            string reply;
            WriteCommand("SET CRC 0", out reply);
            string query = QueryCommand("GET SNR");
            return query;
        }

        /// <summary>
        /// Get the module state via the I2C command
        /// </summary>
        /// <param name="module">Module position 0 to 5 per board</param>
        /// <returns></returns>
        public ModuleState GetModuleState(int module, out string errorMessage)
        {
            if (module > 5 || module < 0)
                throw new ArgumentException("Module position out of range", "module");

            ModuleState state = ModuleState.MS_INVALID;
            if (SetModuleMemory(module, out errorMessage))
            {
                RegisterInfo register = _moduleMemory.ReadRegisterInfo((SlaveRegister.effectModuleState));

                string cmd = $"GET HOST {module} 0x{register.SlaveAddress:X2} {register.StartAddress} {register.NumBytes}";

                string query = QueryCommand(cmd);

                Enum.TryParse(query, out state);
                return state;
            }
            else
            {
                return state;
            }
        }

        /// <summary>
        /// Get the module state via the I2C command
        /// </summary>
        /// <param name="modulePosition">Module position 0 to 5 per board</param>
        /// <returns></returns>
        public ModuleState GetModuleStateViaUart(int modulePosition, SfpUART sfp)
        {
            if (modulePosition > 5 || modulePosition < 0)
                throw new ArgumentException("Module position out of range", "module");

            try
            {
                string UARTstate = sfp.QueryCommand("GET FSM");
                if (UARTstate == string.Empty)
                {
                    return ModuleState.MS_INVALID;
                }
                else
                {
                    int tmpState = Convert.ToInt32(UARTstate);
                    return (ModuleState)tmpState;
                }
            }
            catch
            {
                return ModuleState.MS_INVALID;
            }
        }

        /// <summary>
        /// Wait for an expected state
        /// </summary>
        /// <param name="expectedFinalState">The mdoule state we are waiting for</param>
        /// <param name="Uart">Should always be true for this method, as we are suing the UART interface</param>
        /// <returns>True if we reach expected state with TIME LIMIT</returns>
        public bool WaitUntilModuleState(int modulePosn, ModuleState expectedFinalState, ref ModuleState lastState, SfpUART sfp = null)
        {
            int timeout = 25;
            Stopwatch timer = new Stopwatch();
            ModuleState tempModuleState;
            string errorMessage = string.Empty;
            do
            {
                timer.Start();
                if (sfp == null)
                    tempModuleState = GetModuleState(modulePosn, out errorMessage);
                else
                    tempModuleState = GetModuleStateViaUart(modulePosn, sfp);

                if (expectedFinalState == tempModuleState)
                    break;

                if (tempModuleState == ModuleState.MS_INVALID)
                    break;

                // we were expecting to stop in high power, but gone past that, prob in cusomter mode
                if (tempModuleState == ModuleState.MS_TXON & expectedFinalState == ModuleState.MS_HPWR)
                    break;

                Thread.Sleep(500);
            } while ((tempModuleState != expectedFinalState) && (timer.Elapsed.TotalSeconds < timeout) && (tempModuleState != ModuleState.MS_FAULT));

            if (expectedFinalState == tempModuleState)
            {
                Thread.Sleep(1000);
                return true;
            }
            lastState = tempModuleState;
            return false;
        }

        public void SetHost(int module, int slaveAddress, int address, int numberBytes, byte[] data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Disable the IRQs for all the virtual pins
        /// </summary>
        /// <returns>True if IRQs disabled</returns>
        public bool DisableIRQs()
        {
            Stopwatch timer = new Stopwatch();

            string query;
            do
            {
                timer.Start();
                query = ReadLine();
                if (query.Contains("IRQ"))
                {
                    //we need to stop this
                    string[] response = query.Split(' ');
                    string irqNumber = response[1];

                    WriteLine("OK " + irqNumber);
                }
            } while (query.Contains("IRQ") && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));

            string reply = string.Empty;
            //Disable IRQs for all the virtual pins from now on.....
            for (int module = 0; module < 6; module++)
            {
                //Need to disable all the virtual pins
                for (int itt = _Gpio_first_virstualisation; itt <= _Gpio_last_virstualisation; itt++) // Inputs were defined to limit for-scope
                {
                    int pinNumber = itt + (module + 1) * 100;

                    //Disable leading edge
                    //Disable back edge
                    WriteCommand("SET IRQ " + pinNumber + " " + (int)GpioIrqType.GPIO_IRQ_DOWN + " " + "0", out reply);
                }
            }
            return true;
        }

        /// <summary>
        /// Returns if a module is present in the slot
        /// </summary>
        /// <param name="modulePosition">Slot number starting form 0</param>
        /// <returns></returns>
        public bool ModulePresent(int modulePosition)
        {
            string reply = string.Empty;
            int pinNumber = Convert.ToInt32(GPIOPins.GPIO_MOD_ABSENT) + (modulePosition + 1) * 100;

            if (!WriteCommand("SET CRC 0", out reply))
                return false;

            //see if a module is present
            if (Convert.ToInt32(QueryCommand("GET GPIO " + pinNumber.ToString())) == 0)
                return true;
            return false;
        }

        /// <summary>
        /// Power down in case we want to use module present thing again.
        /// Does not seem to work after power module up
        /// </summary>
        /// <param name="modulePosition"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public bool PositionPoweredDown(int modulePosition, out string errorMessage)
        {
            errorMessage = "";
            List<string> response = null;
            //Convert module position into a virtual number
            int pinNumber = Convert.ToInt32(GPIOPins.GPIO_MOD_ABSENT) + (modulePosition + 1) * 100;
            string reply = string.Empty;
            try
            {
                WriteCommand("SET CRC 0", out reply);

                string pinDescr = $"GPIO_MOD_VSUP_INH_CH{modulePosition}";
                pinNumber = (int)System.Enum.Parse(typeof(GPIOPins), pinDescr);

                //Supply off
                if (!WriteCommand($"ASS GPIO {pinNumber} {1}", out reply))
                {
                    errorMessage = "Problem turning supply off to module" + '\n';
                    errorMessage += reply;
                    return false;
                }

                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                errorMessage = $"Erymanthos driver, ResetModule {e.Message}\n";
                if (response != null && response.Count > 0)
                    errorMessage += response.First();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Power cycles a module present on the board
        /// </summary>
        /// <param name="modulePosition">Module slot position on the board</param>
        /// <param name="errorMessage">Error message</param>
        /// <returns>True if successfully reset</returns>
        public bool ResetModule(int modulePosition, out string errorMessage)
        {
            errorMessage = "";
            List<string> response = null;
            //Convert module position into a virtual number
            int pinNumber = Convert.ToInt32(GPIOPins.GPIO_MOD_ABSENT) + (modulePosition + 1) * 100;
            string reply = string.Empty;
            try
            {
                WriteCommand("SET CRC 0", out reply);

                string pinDescr = $"GPIO_MOD_VSUP_INH_CH{modulePosition}";
                pinNumber = (int)Enum.Parse(typeof(GPIOPins), pinDescr);

                //Supply off
                if (!WriteCommand($"ASS GPIO {pinNumber} {1}", out reply))
                {
                    errorMessage = "Problem turning supply off to module" + '\n';
                    errorMessage += reply;
                    return false;
                }

                Thread.Sleep(_modulePowerOffDelay);

                //Supply on
                if (!WriteCommand($"ASS GPIO {pinNumber} {0}", out reply))
                {
                    errorMessage = "Problem turning supply on to module" + '\n';
                    errorMessage += reply;
                    return false;
                }

                Thread.Sleep(_modulePowerOnDelay);

                if (!WriteCommand("SET CRC 0", out reply))
                {
                    errorMessage = "Problem setting CRC" + '\n';
                    errorMessage += reply;
                    return false;
                }

                if (!WriteCommand($"SET UART {modulePosition}", out reply))
                {
                    response = WriteReadLines($"SET UART {modulePosition}");
                    errorMessage = "Problem setting up UART" + '\n';
                    if (response != null && response.Count > 0)
                        errorMessage += response.First();
                    return false;
                }

                Thread.Sleep(_moduleSetUartDelay);
            }
            catch (Exception e)
            {
                errorMessage = $"Erymanthos driver, ResetModule {e.Message}\n";
                if (response != null && response.Count > 0)
                    errorMessage += response.First();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Select and power module if present and set the Uart for the module position
        /// </summary>
        /// <param name="modulePosition">Erymanthos module position, starts at zero</param>
        /// <returns></returns>
        public bool SetUpModuleOn(int modulePosition, out string errorMessage)
        {
            string LedNumber = "0";
            errorMessage = "";
            List<string> response = null;
            //Convert module position into a virtual number
            int pinNumber = Convert.ToInt32(GPIOPins.GPIO_MOD_ABSENT) + (modulePosition + 1) * 100;
            string reply = string.Empty;
            try
            {
                WriteCommand("SET CRC 0", out reply);

                response = WriteReadLines("GET GPIO " + pinNumber.ToString());

                //See if a module is present
                if (Convert.ToInt32(QueryCommand("GET GPIO " + pinNumber.ToString())) == 0)
                {
                    //Set LED
                    if (!WriteCommand($"SET LED {modulePosition} {LedNumber} {Convert.ToInt32(LedColour.GreenBlinking)}", out reply))
                        return false;

                    //Set supply voltage
                    //if (!WriteCommand($"SET VCC {modulePosition.ToString()} {3300}"))
                    //  return false;
                    //Thread.Sleep(5000);

                    string pinDescr = $"GPIO_MOD_VSUP_INH_CH{modulePosition}";
                    pinNumber = (int)Enum.Parse(typeof(GPIOPins), pinDescr);

                    //Supply off
                    if (!WriteCommand($"ASS GPIO {pinNumber} {1}", out reply))
                    {
                        errorMessage = "Problem turning supply off to module" + '\n';
                        errorMessage += reply;
                        return false;
                    }
                    Thread.Sleep(1000);

                    //Supply on
                    if (!WriteCommand($"ASS GPIO {pinNumber} {0}", out reply))
                    {
                        errorMessage = "Problem turning supply on to module" + '\n';
                        errorMessage += reply;
                        return false;
                    }
                    Thread.Sleep(3000);

                    // seems to need this after the module is powered on
                    if (!WriteCommand("SET CRC 0", out reply))
                    {
                        errorMessage = "Problem setting CRC" + '\n';
                        errorMessage += reply;
                        return false;
                    }

                    //Module should be on now, and in high power if in DEBUG MODE
                    // 54
                    //pinDescr = $"GPIO_TEC_INHIBIT_CH{modulePosition}";
                    //pinNumber = (int)System.Enum.Parse(typeof(GPIOPins), pinDescr);
                    //if (!WriteCommand($"ASS GPIO {pinNumber.ToString()} {0}")) // TEC on
                    //return false;

                    Thread.Sleep(500);

                    if (!WriteCommand($"SET UART {modulePosition}", out reply))
                    {
                        response = WriteReadLines($"SET UART {modulePosition}");
                        errorMessage = "Problem setting up UART" + '\n';
                        if (response != null && response.Count > 0)
                            errorMessage += response.First();
                        return false;
                    }

                    Thread.Sleep(500);
                }
                else
                {
                    errorMessage = $"Module not present in position{modulePosition}";
                    return false;
                }
            }
            catch (Exception e)
            {
                errorMessage = $"Erynmanthos driver, SetUpModuleOn {e.Message}\n";
                if (response != null && response.Count > 0)
                    errorMessage += response.First();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Writes a Erymanthos command waits for a one line response
        /// </summary>
        /// <param name="data">Erymanthos command</param>
        /// <param name="reply">Last line replied before timeout or reciever 'OK' or 'NOK'</param>
        /// <returns>True if command successfully sent</returns>
        public bool WriteCommand(string data, out string reply)
        {
            WriteLine(data);
            Stopwatch timer = new Stopwatch();
            do
            {
                timer.Start();
                reply = ReadLine();
                if (reply.Contains("IRQ"))
                    continue;
                Thread.Sleep(_commandDelay_ms);
            } while (!reply.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));

            if (reply.Contains(_writeResponseFail))
                return false;

            if (reply.Contains(_writeResponseSuccess))
                return true;

            return false;
        }

        /// <summary>
        /// Writes a debug query to the serial port and returns first line response as string
        /// </summary>
        /// <returns>Results of query to the UART</returns>
        public string QueryCommand(string query)
        {
            string response = string.Empty;
            if (comPort.IsOpen)
            {
                Read();
                WriteLine(query);
                string line;
                Stopwatch timer = new Stopwatch();
                do
                {
                    timer.Start();
                    line = ReadLine();
                    if (line.Contains("IRQ"))
                        continue;
                    response = line;
                    Thread.Sleep(_commandDelay_ms);
                } while (!response.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));
            }
            else
            {
                throw new Exception("Cannot read data, serial port is closed");
            }
            if (response.Count() < 2)
                return string.Empty;
            if (response.Contains(_writeResponseFail))
                return string.Empty;//Could be unrecognised command, so gives

            return response.Split(' ')[1];
        }

        /// <summary>
        /// Writes a command that expects a list of lines back, last line should contains OK
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
                    if (line.Contains("IRQ"))
                        continue;

                    if (line == string.Empty)
                        continue;

                    response.Add(line);
                    Thread.Sleep(_commandDelay_ms);
                } while (!line.Contains(_writeResponseSuccess) && timer.Elapsed.TotalSeconds < (timeout_ms / 1000));
            }
            else
            {
                throw new Exception("Cannot read data, serial port is closed");
            }

            return response;
        }

        /// <summary>
        /// Reads a slave register
        /// </summary>
        /// <typeparam name="T">Data type of register to read</typeparam>
        /// <param name="slaveMemory">Enum used to reference register information</param>
        /// <returns>Data read</returns>
        public T Read<T>(int modulePosition, SlaveRegister slaveMemory)
        {
            RegisterInfo register;
            List<byte> returnedData = new List<byte>();
            if (_moduleMemory.Registers.ContainsKey(slaveMemory))
            {
                register = _moduleMemory.Registers[slaveMemory];

                SelectA2Page(modulePosition, register.SlaveAddress, register.PageSelectByte);

                string cmd = $"GET HOST {modulePosition}  0x{register.SlaveAddress:X2} {register.StartAddress} {register.NumBytes}";

                List<string> response = WriteReadLines(cmd);

                foreach (string item in response)
                {
                    if (item.Contains("NOK"))
                    {
                        // problem
                    }

                    if (item.Contains("OK"))
                    {
                        List<string> bytesReturned = item.Split(' ').ToList();
                        List<string> returnedList = bytesReturned.Where(x => x != "OK").ToList();
                        foreach (string aByte in returnedList)
                        {
                            returnedData.Add(Convert.ToByte(aByte));
                        }
                    }
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(Encoding.ASCII.GetString(returnedData.ToArray()).Trim(), typeof(T));
                }
                else if (typeof(T) == typeof(byte))
                {
                    return (T)Convert.ChangeType(returnedData[0], typeof(T));
                }
                else if (typeof(T) == typeof(ushort))
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);
                    ushort value = (ushort)(BitConverter.ToUInt16(data, 0) * register.ScalingFactor);
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else if (typeof(T) == typeof(short))
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);
                    short value = (short)(BitConverter.ToInt16(data, 0) * register.ScalingFactor);
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else if (typeof(T) == typeof(double))
                {
                    double value;
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);

                    if (register.IsSigned)
                    {
                        value = (double)(BitConverter.ToInt16(data, 0) * register.ScalingFactor);
                    }
                    else
                    {
                        if (register.NumBytes == 1)
                        {
                            value = (double)(data[0] * register.ScalingFactor);
                        }
                        else
                        {
                            value = (double)(BitConverter.ToUInt16(data, 0) * register.ScalingFactor);
                        }
                    }
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                else
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);
                    return (T)Convert.ChangeType(BitConverter.ToUInt32((byte[])returnedData.ToArray().Reverse(), 0), typeof(T));
                }
            }

            return default;
        }

        /// <summary>
        /// Set A2 page
        /// </summary>
        /// <param name="slaveAddress">Address of slave</param>
        /// <param name="pageSelectByte">Page to select</param>
        private void SelectA2Page(int modulePosition, byte slaveAddress, byte[] pageSelectByte)
        {
            byte startAddress = 0x7F;
            if (slaveAddress == 0xA2)
            {
                if (pageSelectByte == null)
                    throw new ArgumentException("PageSelectByte cannot be null or empty", nameof(pageSelectByte));

                //Module position[]slave address[]start address[]numberBytes[space separated bytes

                string cmd = $"SET HOST {modulePosition}  0x{slaveAddress:X2} {startAddress} {pageSelectByte.Count()}";

                StringBuilder bytesStringBuild = new StringBuilder(cmd);

                foreach (byte item in pageSelectByte)
                {
                    bytesStringBuild.Append(" ");
                    bytesStringBuild.Append("0x" + item.ToString("X2"));
                }

                List<string> response = WriteReadLines(bytesStringBuild.ToString());

                //Convert vendor revision to ASCII
                foreach (string item in response)
                {
                    if (item.Contains("NOK"))
                        throw new Exception("Problems reading from I2C , is adaptor plugged in, module powered up");

                    if (item.Contains("OK"))
                    {
                        return;
                    }
                }
            }
        }

        #endregion public methods

        #region private methods

        //Set the memory dependant on version of firmware we have on a module
        public bool SetModuleMemory(int module, out string errorMessage)
        {
            errorMessage = string.Empty;
            int numVendorRevision = 0;
            string vendorRevision = "";
            bool setMemory = true;

            RegisterInfo register = _moduleMemory.ReadRegisterInfo(SlaveRegister.A0vendorRev);

            string cmd = $"GET HOST {module}  0x{register.SlaveAddress:X2} {register.StartAddress} {register.NumBytes}";

            List<string> response = WriteReadLines(cmd);

            try
            {
                //Convert vendor revision to ASCII
                foreach (string item in response)
                {
                    if (item.Contains("NOK"))
                    {
                        errorMessage = item;
                        setMemory = false;
                        break;
                    }

                    if (item.Contains("OK"))
                    {
                        List<string> revision = item.Split(' ').ToList<string>();
                        List<string> revisionList = revision.Where(x => x != "OK").ToList();
                        foreach (string digit in revisionList)
                        {
                            vendorRevision += Convert.ToChar(Convert.ToInt32(digit));
                        }
                    }
                }

                numVendorRevision = Convert.ToInt32(vendorRevision);
            }
            catch (Exception e)
            {
                errorMessage = e.Message.ToString();
                if (response.Count > 0)
                    errorMessage = $"{errorMessage}{+'\n'}{response.First()}";
            }

            if (!setMemory)
            {
                //Convert vendor revision to ASCII
                foreach (string item in response)
                {
                    errorMessage = $"{errorMessage}\n{item}";
                    setMemory = false;
                }
                return setMemory;
            }

            if (numVendorRevision >= 3000 || numVendorRevision == 0)
            {
                _moduleMemory = new SfpMemory3000();
                //Need to set passsword if we want to read stuff on slave 0x84
                WritePasswordWP3(module);
            }
            else
            {
                _moduleMemory = new SfpMemory();
            }
            return setMemory;
        }

        /// <summary>
        /// Write the NVM password SO we can read 0x84NVM
        /// Otherwise FF returned
        /// </summary>
        /// <returns>True if password successfully written</returns>
        public bool WritePasswordWP3(int module)
        {
            byte pw3 = 0xFD;//Use this as start address
            byte pw2 = 0x59;
            byte pw1 = 0x2A;
            byte pw0 = 0x5A;

            //Need to write 4 bytes from start address 123
            RegisterInfo register = _moduleMemory.ReadRegisterInfo(SlaveRegister.password_3);

            string cmd = $"SET HOST {module} 0x{register.SlaveAddress:X2} {register.StartAddress} 4  0x{pw3:X2} 0x{pw2:X2} 0x{pw1:X2} 0x{pw0:X2}";
            string reply = string.Empty;
            return WriteCommand(cmd, out reply);
        }

        #endregion private methods
    }
}