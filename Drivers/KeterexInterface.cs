using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Drivers
{
    /// <summary>
    ///  Class written to interface to the usb-910h api for I2C comms.
    ///  TODO ...will expand to read/ drive the GPIO pins
    /// </summary>
    public class KeterexInterface
    {
        #region DLLImport for keterex usb-910h api which is c++ unmanaged code

        //Declare all the functions needed from usb-910h api
        [DllImport("kxusb910h.dll", EntryPoint = "kxFindAdapters", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte kxFindAdapters();

        [DllImport("kxusb910h.dll", EntryPoint = "kxOpenAdapter", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxOpenAdapter(byte adapter);

        [DllImport("kxusb910h.dll", EntryPoint = "kxEnableFeatures", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxEnableFeatures(byte adapter, uint feature);

        [DllImport("kxusb910h.dll", EntryPoint = "kxI2CsetBitRate", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxI2CsetBitRate(byte adapter, double bitRate);

        [DllImport("kxusb910h.dll", EntryPoint = "kxI2CsetConfig", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxI2CsetConfig(byte adapter, uint features);

        [DllImport("kxusb910h.dll", EntryPoint = "kxI2Cexecute", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxI2Cexecute(byte adapter, string script);

        [DllImport("kxusb910h.dll", EntryPoint = "kxStartTimer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void kxStartTimer(byte adapter, double time);

        [DllImport("kxusb910h.dll", EntryPoint = "kxTimerExpired", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxTimerExpired(byte adapter);

        [DllImport("kxusb910h.dll", EntryPoint = "kxAbort", CallingConvention = CallingConvention.Cdecl)]
        public static extern void kxAbort(byte adapter);

        [DllImport("kxusb910h.dll", EntryPoint = "kxGetStatus", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxGetStatus(byte adapter, ref ushort status);

        [DllImport("kxusb910h.dll", EntryPoint = "kxGetInData", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxGetInData(byte adapter, ref IntPtr buffer, ref ushort bytesRead);

        [DllImport("kxusb910h.dll", EntryPoint = "kxCloseAdapter", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxCloseAdapter(byte adapter);

        [DllImport("kxusb910h.dll", EntryPoint = "kxGetSerialString", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool kxGetSerialString(byte adapter, ref IntPtr buffer);

        #endregion DLLImport for keterex usb-910h api which is c++ unmanaged code

        /// <summary>
        /// Only ever one in this simple set up for regression
        /// </summary>
        private byte _adapter = 0;

        /// <summary>
        /// I2C register names and information
        /// </summary>
        private ISfpMemoryBase _moduleMemory;

        /// <summary>
        /// Time to wait for the module process a write command
        /// </summary>
        private int _commandDelay_ms = 100;

        public ISfpMemoryBase _ModuleMemory { get => _moduleMemory; set => _moduleMemory = value; }

        private const short _teraToGiga = 1000;
        private const int _gigaToHz = 1000000000;
        private const int _mTo_nm = 1000000000;
        private const ulong _speedOfLight = 299792458;//Metres per second 299792458u

        #region contructors

        /// <summary>
        /// Class constructor connects adapter and sets up adapter as host I2C master, enables I2C pull up resistors
        /// set up comms options
        /// </summary>
        public KeterexInterface()
        {
            _moduleMemory = new SfpMemoryBase();

            uint KX_FEATURE_I2CMST = 0x01;
            uint KX_FEATURE_I2CRPU = 0x10;
            uint KX_I2CCONFIG_ABORT_ON_EXPECT = 0x02;
            uint KX_I2CCONFIG_FREE_BUS_ENABLE = 0x08;

            //byte num_adapter = kxFindAdapters();

            if (kxFindAdapters() < 1)
                throw new Exception("No Keterex adapters found");

            IntPtr buffer = new IntPtr();

            if (kxGetSerialString(0, ref buffer))
            {
                byte[] charArray = new byte[20];

                Marshal.Copy(buffer, charArray, 0, 20);

                string serialNumber = System.Text.Encoding.ASCII.GetString(charArray).Trim();
            }

            //if (kxGetSerialString(1, ref buffer))
            //{
            //    byte[] charArray = new byte[6];

            //    Marshal.Copy(buffer, charArray, 0, 6);

            //    string serialNumber = (System.Text.Encoding.ASCII.GetString(charArray)).Trim();
            //}

            //Configure the interface
            if (!kxOpenAdapter(_adapter))
                throw new Exception("Cannot connect to Keterex adaptor");

            if (!kxEnableFeatures(_adapter, KX_FEATURE_I2CMST | KX_FEATURE_I2CRPU))
                throw new Exception("Cannot set adapter to I2C master or set pull up resistors");

            if (!kxI2CsetBitRate(_adapter, 100))
                throw new Exception("Cannot set the bit rate");

            if (!kxI2CsetConfig(_adapter, KX_I2CCONFIG_ABORT_ON_EXPECT | KX_I2CCONFIG_FREE_BUS_ENABLE))
                throw new Exception("Cannot set I2C config");

            SetMemory();
        }

        /// <summary>
        /// Set the memory dependant on version of firmware we have
        /// </summary>
        public void SetMemory()
        {
            //Password correct
            WritePassword();
            WritePasswordWP2();
            string vendorRevision = Read<string>(SlaveRegister.A0vendorRev);
            bool isNumeric = int.TryParse(vendorRevision, out int numVendorRevision);
            if (!isNumeric)
            {
                _moduleMemory = new SfpMemory3000();
            }
            else if (numVendorRevision >= 3000)
            {
                _moduleMemory = new SfpMemory3000();
            }
            else
            {
                _moduleMemory = new SfpMemory();
            }
        }

        #endregion contructors

        #region private methods

        /// <summary>
        /// Write to I2C slave/address
        /// </summary>
        /// <param name="slaveAddress">Slave address</param>
        /// <param name="address">Address to write to</param>
        /// <param name="data">Data to write</param>
        /// <returns>Returns true if successful</returns>
        private bool Send(byte slaveAddress, byte address, List<byte> data = null)
        {
            //Convert to hex string
            string message = $"/S{slaveAddress:X2}{address:X2}";

            if (data != null)
            {
                string dataToWrite = BitConverter.ToString(data.ToArray()).Replace("-", " ");
                message += dataToWrite;
            }

            message += "/P";

            // initiate I2C script
            if (kxI2Cexecute(_adapter, message))
            {
                return WaitForWrite(_adapter);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Receive incoming data
        /// </summary>
        /// <param name="slaveAddress">Slave address</param>
        /// <param name="address">Address to read</param>
        /// <param name="numberBytes">Number of bytes expected</param>
        /// <param name="data">List of bytes received</param>
        /// <returns></returns>
        private bool Receive(byte slaveAddress, byte address, byte numberBytes, List<byte> data)
        {
            //Send address to read
            Send(slaveAddress, address);

            //Set LSB 1 to indicate read
            byte readAddress = (byte)(slaveAddress | 0x01);

            //Read script
            string message = $"/S{readAddress:X2}/R 00 {numberBytes:X2}";

            message += "/P";

            if (kxI2Cexecute(_adapter, "/S" + readAddress.ToString("X2") + "/R 00 " + numberBytes.ToString("X2")))
            {
                if (WaitForWrite(_adapter))
                {
                    ushort bytesAvailable = 0;
                    IntPtr buffer = new IntPtr();

                    //retrieve incoming data from the adapter
                    if (kxGetInData(_adapter, ref buffer, ref bytesAvailable))
                    {
                        byte[] bytes = new byte[bytesAvailable];
                        Marshal.Copy(buffer, bytes, 0, bytesAvailable);
                        foreach (byte item in bytes)
                        {
                            data.Add(item);
                        }
                        return true;
                    }
                }
            }
            return false;
        }

        private bool WaitForWrite(byte adapter)
        {
            Thread.Sleep(_commandDelay_ms);

            ushort KX_STATUS_I2C_MASTER_PENDING = 0x0001;
            ushort status = 0;

            kxGetStatus(adapter, ref status);
            if (kxGetStatus(adapter, ref status) && (status & KX_STATUS_I2C_MASTER_PENDING) == 0)
                return true;

            //Do not wait more than 5 secomds
            kxStartTimer(adapter, 5.0);
            int finishedWrite = 0;
            do
            {
                if (kxTimerExpired(adapter))
                {
                    //Operation is taking too long!
                    kxAbort(adapter);
                    return false;
                }
                kxGetStatus(adapter, ref status);
                finishedWrite = status & KX_STATUS_I2C_MASTER_PENDING;
            } while (finishedWrite == 1);
            return true;
        }

        /// <summary>
        /// Set A2 page
        /// </summary>
        /// <param name="slaveAddress">Address of slave</param>
        /// <param name="pageSelectByte">Page to select</param>
        private void SelectA2Page(byte slaveAddress, byte[] pageSelectByte)
        {
            if (slaveAddress == 0xA2)
            {
                if (pageSelectByte == null)
                    throw new ArgumentException("'pageSelectByte' cannot be null or empty", nameof(pageSelectByte));

                if (!Send(0xA2, 0x7f, pageSelectByte.ToList()))
                    throw new Exception("Problems reading from I2C , is adaptor plugged in, module powered up");
            }
        }

        /// <summary>
        /// Calculates the freq of the channel based on
        /// grid spacing and start freq.
        /// </summary>
        /// <param name="channel">Channel number</param>
        /// <returns>Channel frequncy in GHz</returns>
        private double CalculateChannelFreqGhz(ushort channel)
        {
            WritePasswordWP2();

            int laserFirstFreqGHz = Read<ushort>(SlaveRegister.laserFirstFreqTHz) * _teraToGiga;

            double gigaFreq = laserFirstFreqGHz + Read<ushort>(SlaveRegister.laserFirstFreqGHz);

            double startFrequencyHz = gigaFreq * _gigaToHz;

            double gridSpacing = Read<double>(SlaveRegister.minSupportedGridSpacingGHz) * _gigaToHz;

            return (startFrequencyHz + ((channel - 1) * gridSpacing)) / _gigaToHz;
        }

        /// <summary>
        /// Wait for an expected state
        /// </summary>
        /// <param name="expectedFinalState"></param>
        /// <returns></returns>
        private bool WaitUntilModuleState(ModuleState expectedFinalState)
        {
            int timeout = 20;
            Stopwatch timer = new Stopwatch();
            ModuleState tempModuleState;

            WritePasswordWP3();
            do
            {
                timer.Start();
                tempModuleState = ((ModuleState)Read<byte>(SlaveRegister.effectModuleState));
                if (expectedFinalState == tempModuleState)
                    break;

                Thread.Sleep(500);
            } while ((tempModuleState != expectedFinalState) && (timer.Elapsed.TotalSeconds < timeout) && (tempModuleState != ModuleState.MS_FAULT));

            if (expectedFinalState == tempModuleState)
            {
                Thread.Sleep(1000);
                return true;
            }
            return false;
        }

        #endregion private methods

        #region public functions

        /// <summary>
        /// Disconnect I2C master adapter
        /// </summary>
        public void Disconnect()
        {
            kxAbort(_adapter);

            kxCloseAdapter(_adapter);
        }

        /// <summary>
        /// Read 1 byte of data 
        /// </summary>
        /// <param name="slave">Address of the slave</param>
        /// <param name="address">Adrress on slave</param>
        /// <returns></returns>
        public byte Read(byte slave, byte address)
        {
            List<byte> returnedData = new List<byte>();
            if (!Receive(slave, address, 1, returnedData))
                throw new Exception("Problems reading form I2C , is adaptor plugged in, module powered up");

            return returnedData[0];
        }

        /// <summary>
        /// Reads a slave register
        /// </summary>
        /// <typeparam name="T">Data type of register to read</typeparam>
        /// <param name="slaveMemory">Enum used to reference register information</param>
        /// <returns>Data read</returns>
        public T Read<T>(SlaveRegister slaveMemory)
        {
            RegisterInfo register;
            List<byte> returnedData = new List<byte>();
            if (_moduleMemory.Registers.ContainsKey(slaveMemory))
            {
                register = _moduleMemory.Registers[slaveMemory];

                SelectA2Page(register.SlaveAddress, register.PageSelectByte);

                if (!Receive(register.SlaveAddress, register.StartAddress, register.NumBytes, returnedData))
                    throw new Exception("Problems reading form I2C , is adaptor plugged in, module powered up");

                //Try until we have all the required bytes, up tp 4 times
                if (returnedData.Count < register.NumBytes)
                {
                    for (int readTries = 0; readTries < 4; readTries++)
                    {
                        Thread.Sleep(10);
                        Receive(register.SlaveAddress, register.StartAddress, register.NumBytes, returnedData);
                        if (returnedData.Count == register.NumBytes)
                        {
                            break;
                        }
                    }
                }

                if (typeof(T) == typeof(string))
                {
                    return (T)Convert.ChangeType(System.Text.Encoding.ASCII.GetString(returnedData.ToArray()).Trim(), typeof(T));
                }
                else if (typeof(T) == typeof(byte))
                {
                    return (T)Convert.ChangeType((returnedData[0] & register.BitMask), typeof(T));
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
                            value = (double)(BitConverter.ToUInt16(data, 0) * register.ScalingFactor);// ;
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
        /// Read from I2C, only supports Byte, UInt16 and INT16 data types
        /// </summary>
        /// <typeparam name="T">Data type to read</typeparam>
        /// <param name="slaveAddress">Address of slave to read</param>
        /// <param name="startAddress">Start register address</param>
        /// <param name="pageSelectByte">A2 page select if applicable</param>
        /// <returns></returns>
        public T Read<T>(byte slaveAddress, byte startAddress, ushort bitMask = 0Xff, byte[] pageSelectByte = null)
        {
            byte numberBytes;
            List<byte> returnedData = new List<byte>();

            SelectA2Page(slaveAddress, pageSelectByte);

            if (typeof(T) == typeof(byte))
            {
                numberBytes = sizeof(byte);

                if (returnedData.Count < numberBytes)
                {
                    for (int readTries = 0; readTries < 4; readTries++)
                    {
                        Thread.Sleep(10);
                        Receive(slaveAddress, startAddress, numberBytes, returnedData);
                        if (returnedData.Count == numberBytes)
                        {
                            break;
                        }
                    }
                }

                return (T)Convert.ChangeType(returnedData[0] & bitMask, typeof(T));
            }
            else if (typeof(T) == typeof(ushort))
            {
                numberBytes = sizeof(ushort);
                if (returnedData.Count < numberBytes)
                {
                    for (int readTries = 0; readTries < 4; readTries++)
                    {
                        Thread.Sleep(10);
                        Receive(slaveAddress, startAddress, numberBytes, returnedData);
                        if (returnedData.Count == numberBytes)
                        {
                            break;
                        }
                    }
                }

                byte[] data = returnedData.ToArray();
                Array.Reverse(data);
                ushort value = BitConverter.ToUInt16(data, 0);
                return (T)Convert.ChangeType(value & bitMask, typeof(T));
            }
            else if (typeof(T) == typeof(short))
            {
                numberBytes = sizeof(ushort);
                if (returnedData.Count < numberBytes)
                {
                    for (int readTries = 0; readTries < 4; readTries++)
                    {
                        Thread.Sleep(10);
                        Receive(slaveAddress, startAddress, numberBytes, returnedData);
                        if (returnedData.Count == numberBytes)
                        {
                            break;
                        }
                    }
                }
                //Assume little endian
                byte[] data = returnedData.ToArray();
                Array.Reverse(data);
                short value = BitConverter.ToInt16(data, 0);
                return (T)Convert.ChangeType(value & bitMask, typeof(T));
            }

            return default;
        }

        /// <summary>
        /// Reads the memory address, using the data type information held in the RegisterInfo object
        /// </summary>
        /// <param name="slaveMemory">Start addreess of register</param>
        /// <returns>Data returned as string for display</returns>
        public string ReadAsString(SlaveRegister slaveMemory)
        {
            RegisterInfo register;
            List<byte> returnedData = new List<byte>();
            if (_moduleMemory.Registers.ContainsKey(slaveMemory))
            {
                register = _moduleMemory.Registers[slaveMemory];

                SelectA2Page(register.SlaveAddress, register.PageSelectByte);

                Receive(register.SlaveAddress, register.StartAddress, register.NumBytes, returnedData);
                if (register.RegisterType == typeof(string))
                {
                    return System.Text.Encoding.ASCII.GetString(returnedData.ToArray()).Trim();
                }
                else if (register.RegisterType == typeof(byte))
                {
                    return Convert.ToString(returnedData[0] & register.BitMask);
                }
                else if (register.RegisterType == typeof(ushort))
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);
                    ushort value = (ushort)(BitConverter.ToUInt16(data, 0) * register.ScalingFactor);
                    return value.ToString();
                }
                else if (register.RegisterType == typeof(double))
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);
                    double value;
                    if (register.IsSigned)
                    {
                        if (register.NumBytes == 4)
                        {
                            value = (double)(BitConverter.ToInt32(data, 0) * register.ScalingFactor);
                        }
                        else
                        {
                            value = (double)(BitConverter.ToInt16(data, 0) * register.ScalingFactor);
                        }
                        return value.ToString();
                    }
                    else
                    {
                        if (register.NumBytes == 4)
                        {
                            value = (double)(BitConverter.ToUInt32(data, 0) * register.ScalingFactor);
                        }
                        else if (register.NumBytes == 1)
                        {
                            value = (double)(data[0] * register.ScalingFactor);
                        }
                        else
                        {
                            value = (double)(BitConverter.ToUInt16(data, 0) * register.ScalingFactor);
                        }
                        return value.ToString();
                    }
                }
                else if (register.RegisterType == typeof(short))
                {
                    byte[] data = returnedData.ToArray();
                    Array.Reverse(data);

                    double value = (double)(BitConverter.ToInt16(data, 0) * register.ScalingFactor);
                    return value.ToString();
                }
                else
                {
                    throw new Exception("Register has undefined type");
                }
            }
            else
            {
                throw new Exception("Register is not in the register map");
            }
        }

        /// <summary>
        /// Returns information about a register you may want to read
        /// </summary>
        /// <param name="slaveMemory"></param>
        /// <returns></returns>
        public RegisterInfo ReadRegisterInfo(SlaveRegister slaveMemory)
        {
            if (_moduleMemory.Registers.ContainsKey(slaveMemory))
            {
                return _moduleMemory.Registers[slaveMemory];
            }
            return null;
        }

        /// <summary>
        /// Writes to a memory address
        /// </summary>
        /// <typeparam name="T">Data type of the register information</typeparam>
        /// <param name="slaveMemory">Enum that points to the register information to write</param>
        /// <param name="value">Data to write</param>
        /// <returns>Returns true if successful write</returns>
        public bool Write<T>(SlaveRegister slaveMemory, T value)
        {
            RegisterInfo register;
            byte[] arrayToWrite = null;

            //Password depends on slave registers here
            if (_moduleMemory.Registers.ContainsKey(slaveMemory))
            {
                register = _moduleMemory.Registers[slaveMemory];

                SelectA2Page(register.SlaveAddress, register.PageSelectByte);

                if (register.RegisterType == typeof(ushort))
                {
                    ushort valueToWrite = (ushort)Convert.ChangeType(value, typeof(ushort));
                    valueToWrite = (ushort)(valueToWrite / register.ScalingFactor);
                    arrayToWrite = ReverseAsNeeded(BitConverter.GetBytes(valueToWrite), false);
                }
                else if (register.RegisterType == typeof(short))
                {
                    short valueToWrite = (short)Convert.ChangeType(value, typeof(short));
                    valueToWrite = (short)(valueToWrite / register.ScalingFactor);
                    arrayToWrite = ReverseAsNeeded(BitConverter.GetBytes(valueToWrite), false);
                }
                else if (register.RegisterType == typeof(byte))
                {
                    byte valueToWrite = (byte)Convert.ChangeType(value, typeof(byte));
                    valueToWrite = (byte)(valueToWrite & register.BitMask);

                    arrayToWrite = new byte[] { valueToWrite };
                }
                else if (register.RegisterType == typeof(string))
                {
                    string valueToWrite = (string)Convert.ChangeType(value, typeof(string));
                    arrayToWrite = ReverseAsNeeded(System.Text.Encoding.ASCII.GetBytes(valueToWrite), true);
                }
                else if (register.RegisterType == typeof(double))
                {
                    double valueToWrite = (double)Convert.ChangeType(value, typeof(double));
                    //now use scaling factor
                    valueToWrite = Math.Round(valueToWrite / register.ScalingFactor);

                    if (register.IsSigned && register.NumBytes == 2)
                    {
                        arrayToWrite = ReverseAsNeeded(BitConverter.GetBytes((short)valueToWrite), false);
                    }
                    else if (register.NumBytes == 2)
                    {
                        arrayToWrite = ReverseAsNeeded(BitConverter.GetBytes((ushort)valueToWrite), false);
                    }
                    else
                    {
                        // write one byte
                        arrayToWrite = new byte[] { (byte)valueToWrite };
                    }
                }
                else
                {
                    throw new Exception("Register has undefined type");
                }

                bool sendSuccess = Send(slaveAddress: register.SlaveAddress, address: register.StartAddress, data: arrayToWrite.ToList());

                // do not need if its a password
                if (!slaveMemory.ToString().ToLower().Contains("password"))
                    Thread.Sleep(2000); /// because the module firmware now does NVM bulk writes in background process....nice, from 3031 or there abouts
                return sendSuccess;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        ///  Sets a bit on a byte register
        /// </summary>
        /// <param name="slaveAddress">Address of slave</param>
        /// <param name="byteAddress">Regieter address of byte</param>
        /// <param name="bit">Bit you want to set </param>
        /// <param name="pageSelectByte"></param>
        /// <returns></returns>
        public bool SetBit(byte slaveAddress, byte byteAddress, ushort bit = 0xff, byte[] pageSelectByte = null)
        {
            //TODO :....get register details, only want to use this on a register of data type byte

            SelectA2Page(slaveAddress, pageSelectByte);
            return WriteBit(slaveAddress, byteAddress, bit, clearBit: false);
        }

        /// <summary>
        /// Clears the specified bit in a register
        /// </summary>
        /// <param name="slaveAddress">Address of slve device</param>
        /// <param name="byteAddress">Address of the register</param>
        /// <param name="bit">Bit you want to set </param>
        /// <param name="pageSelectByte">A2 page to set</param>
        /// <returns>Returns true of write succesful</returns>
        public bool ClearBit(byte slaveAddress, byte byteAddress, ushort bit = 0xff, byte[] pageSelectByte = null)
        {
            //TODO :....get register details, only want to use this on a register of data type byte
            SelectA2Page(slaveAddress, pageSelectByte);

            return WriteBit(slaveAddress, byteAddress, bit, clearBit: true);
        }

        /// <summary>
        /// Either clears or sets the specified bit of a byte
        /// </summary>
        /// <param name="slaveAddress">Address of the slave device</param>
        /// <param name="byteAddress">Address of the register</param>
        /// <param name="bit">Bit number to set/clear</param>
        /// <param name="clearBit">True if we want to clear the bit</param>
        /// <returns>Return true of successful</returns>
        private bool WriteBit(byte slaveAddress, byte byteAddress, ushort bit, bool clearBit)
        {
            List<byte> returnedData = new List<byte>();
            byte valueToWrite;
            byte[] arrayToWrite = null;
            //read the register
            if (Receive(slaveAddress, byteAddress, sizeof(byte), returnedData))
            {
                ushort value = returnedData[0];

                if (clearBit)
                {
                    ushort test = (ushort)~bit;
                    valueToWrite = (byte)(value & ~bit);
                }
                else
                {
                    valueToWrite = (byte)(value | bit);
                }

                arrayToWrite = new byte[] { valueToWrite };
                bool sent = Send(slaveAddress: slaveAddress, address: byteAddress, data: arrayToWrite.ToList());
                Thread.Sleep(2000);//Becuse the module does NVM wrties in the background
                return sent;
            }
            return false;
        }

        /// <summary>
        /// Write the NVM password
        /// </summary>
        /// <returns>True if password successfully written</returns>
        public bool WritePassword()
        {
            byte pw3 = 0x00;
            byte pw2 = 0x00;
            byte pw1 = 0x10;
            byte pw0 = 0x11;
            bool pw3Write = Write<ushort>(SlaveRegister.password_3, pw3);
            bool pw2Write = Write<ushort>(SlaveRegister.password_2, pw2);
            bool pw1Write = Write<ushort>(SlaveRegister.password_1, pw1);
            bool pw0Write = Write<ushort>(SlaveRegister.password_0, pw0);

            return pw3Write && pw2Write && pw1Write && pw0Write;
        }

        /// <summary>
        /// Write the NVM password SO we can read 0x84NVM
        /// otherwise FF returned
        /// </summary>
        /// <returns>True if password successfully written</returns>
        public bool WritePasswordWP3()
        {
            byte pw3 = 0xFD;
            byte pw2 = 0x59;
            byte pw1 = 0x2A;
            byte pw0 = 0x5A;
            bool pw3Write = Write<ushort>(SlaveRegister.password_3, pw3);
            bool pw2Write = Write<ushort>(SlaveRegister.password_2, pw2);
            bool pw1Write = Write<ushort>(SlaveRegister.password_1, pw1);
            bool pw0Write = Write<ushort>(SlaveRegister.password_0, pw0);

            return pw3Write && pw2Write && pw1Write && pw0Write;
        }

        /// <summary>
        /// Write the NVM password for Prolabs stuff
        /// otherwise FF returned
        /// </summary>
        /// <returns>true if password successfully written</returns>
        public bool WritePasswordWP2()
        {
            byte pw3 = 0x5D;
            byte pw2 = 0x0D;
            byte pw1 = 0x2A;
            byte pw0 = 0xDB;
            bool pw3Write = Write<ushort>(SlaveRegister.password_3, pw3);
            bool pw2Write = Write<ushort>(SlaveRegister.password_2, pw2);
            bool pw1Write = Write<ushort>(SlaveRegister.password_1, pw1);
            bool pw0Write = Write<ushort>(SlaveRegister.password_0, pw0);
           
            return pw3Write && pw2Write && pw1Write && pw0Write;
        }

        /// <summary>
        /// Change the channel
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public bool WriteChannel(ushort channelNumber, ref double channelFreqGHz)
        {
            ushort channelRead = Read<ushort>(SlaveRegister.channelNumber);
            if (channelRead == channelNumber)
                return true; //Already at that channel

            //Get start module state
            WritePasswordWP3();
            ModuleState startModuleState = (ModuleState)Read<byte>(SlaveRegister.effectModuleState);

            if (!Write(SlaveRegister.channelNumber, channelNumber))
                return false;

            channelRead = Read<ushort>(SlaveRegister.channelNumber);

            if (channelRead != channelNumber)
                return false;

            WaitUntilModuleState(startModuleState);

            channelFreqGHz = CalculateChannelFreqGhz(channelRead);

            return true;
        }

        /// <summary>
        /// Reads the current channel information of the module
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="channelFreq"></param>
        public void ChannelInfo(ref ushort channelRead, ref double channelFreqGHz)
        {
            channelRead = Read<ushort>(SlaveRegister.channelNumber);

            WritePasswordWP2();

            channelFreqGHz = CalculateChannelFreqGhz(channelRead);
        }

        /// <summary>
        /// Sets the firmware mode
        /// </summary>
        /// <param name="firmwareMode">Firmware mode to set</param>
        /// <returns>True if successfully set</returns>
        public bool WriteFirmwareMode(string firmwareMode)
        {
            byte[] page = { 0x02 };

            //Set up to page 2
            if (!Send(slaveAddress: 0xA2, address: 0x7f, page.ToList()))
                return false;

            switch (firmwareMode)
            {
                case "UART":
                    byte[] debugCode = { 0x37, 0xf1, 0xb8, 0x0c, 0x79, 0x3a, 0xad, 0x2c };
                    if (!Send(slaveAddress: 0xa2, address: 0xad, debugCode.ToList()))
                    {
                        return false;
                    }
                    break;

                case "SWD":
                    byte[] swdCode = { 0x6f, 0x73, 0xa8, 0x92, 0xc4, 0xad, 0xdf, 0x41 };
                    if (!Send(slaveAddress: 0xa2, address: 0xad, swdCode.ToList()))
                    {
                        return false;
                    }
                    break;

                case "CUSTOMER":
                    byte[] customerCode = { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0 };
                    if (!Send(slaveAddress: 0xa2, address: 0xad, customerCode.ToList()))
                    {
                        return false;
                    }
                    break;

                default:
                    break;
            }
            return true;
        }

        #endregion public functions

        private static byte[] ReverseAsNeeded(byte[] bytes, bool wantsLittleEndian)
        {
            if (wantsLittleEndian == BitConverter.IsLittleEndian)
                return bytes;
            else
                return bytes.Reverse().ToArray();
        }
    }
}