using System;

namespace Drivers

{
    public enum RegisterWriteProperties
    {
        /// <summary>
        /// UART only or firmware EG. only writable during firmware control loop or at calibration.
        /// No NVM storage
        /// </summary>
        SFP_WR_PROPERTY_RO,

        /// <summary>
        /// I2C or UART control funtions like page select. no NVM storage
        /// </summary>
        SFP_WR_PROPERTY_W,

        /// <summary>
        /// NVM storage over I2C only with password..cannot find examples
        /// </summary>
        SFP_WR_PROPERTY_Wl,

        /// <summary>
        /// UART only eg. alarm levels and module properties
        /// </summary>
        SFP_WR_PROPERTY_WE,

        /// <summary>
        /// I2C with password or UART, warning levels or vendor names
        /// </summary>
        SFP_WR_PROPERTY_WV,

        /// <summary>
        /// I2C or UART, no password needed (cannot find exampls)
        /// </summary>
        SFP_WR_PROPERTY_WU,

        /// <summary>
        /// I2C or UART, no password needed, active if valid channel
        /// </summary>
        SFP_WR_PROPERTY_WC,
    }

    /// <summary>
    /// Class to hold information on the different registers
    /// </summary>
    public class RegisterInfo
    {
        private readonly byte slaveAddress;
        private readonly byte startAddress;
        private readonly byte numBytes;
        private readonly byte[] pageSelectByte;
        private readonly double scalingFactor = 1.0;
        private readonly Type registerType = typeof(ushort);
        private readonly bool isSigned = false;
        private readonly ushort bitMask = 0xFF;
        private readonly string units = string.Empty;

        #region constructors

        /// <summary>
        /// Register information for slave
        /// </summary>
        /// <param name="slave">Address of slave</param>
        /// <param name="address">Start register address</param>
        /// <param name="numberBytes">Number of bytes</param>
        /// <param name="regType">Data type for the register information</param>
        public RegisterInfo(byte slave, byte address, byte numberBytes, Type regType)
        {
            slaveAddress = slave;
            startAddress = address;
            numBytes = numberBytes;
            registerType = regType;
            pageSelectByte = new byte[] { 0 };
        }

        /// <summary>
        /// Register information for slave
        /// </summary>
        /// <param name="slave">Address of slave</param>
        /// <param name="address">Start register address</param>
        /// <param name="numberBytes">Number of bytes</param>
        /// <param name="theBitMask">Used to extract bit value</param>
        /// <param name="isSignedRegister">True if register information is signed</param>
        /// <param name="pageSelect">Page select byte</param>
        /// <param name="scaleFactor">Scaling factor to give ISO units where applicable</param>
        /// <param name="regType">Data type for the register information</param>
        public RegisterInfo(byte slave, byte address, byte numberBytes, ushort theBitMask, bool isSignedRegister, byte pageSelect, double scaleFactor, Type regType)
        {
            slaveAddress = slave;
            startAddress = address;
            numBytes = numberBytes;
            pageSelectByte = new byte[] { pageSelect };
            scalingFactor = scaleFactor;
            registerType = regType;
            bitMask = theBitMask;
            isSigned = isSignedRegister;
        }

        /// <summary>
        /// Register information for slave
        /// </summary>
        /// <param name="slave">Address of slave</param>
        /// <param name="address">Start register address</param>
        /// <param name="numberBytes">Number of bytes</param>
        /// <param name="isSignedRegister">True if register information is signed</param>
        /// <param name="pageSelect">Page select byte</param>
        /// <param name="scaleFactor">Scaling factor to give ISO units where applicable</param>
        /// <param name="regType">Data type for the register information</param>
        public RegisterInfo(byte slave, byte address, byte numberBytes, bool isSignedRegister, byte pageSelect, double scaleFactor, Type regType, string displayUnits)
        {
            slaveAddress = slave;
            startAddress = address;
            numBytes = numberBytes;
            pageSelectByte = new byte[] { pageSelect };
            scalingFactor = scaleFactor;
            registerType = regType;
            isSigned = isSignedRegister;
            units = displayUnits;
        }

        #endregion constructors

        #region properties

        /// <summary>
        /// Address of slave device
        /// </summary>
        public byte SlaveAddress => slaveAddress;

        /// <summary>
        /// Start address on slave device
        /// </summary>
        public byte StartAddress => startAddress;

        /// <summary>
        /// Number of bytes to read
        /// </summary>
        public byte NumBytes => numBytes;

        /// <summary>
        /// Page to select for A2 device, only relevant for A2 device
        /// </summary>
        public byte[] PageSelectByte => pageSelectByte;

        /// <summary>
        /// Scaling factor for register
        /// </summary>
        public double ScalingFactor => scalingFactor;

        /// <summary>
        /// Data type for the information the register holds
        /// </summary>
        public Type RegisterType => registerType;

        /// <summary>
        /// Is the register signed
        /// </summary>
        public bool IsSigned => isSigned;

        /// <summary>
        /// Used if we want to extract the bit
        /// </summary>
        public ushort BitMask => bitMask;

        /// <summary>
        /// To indicate units we have converted to with scaling factor
        /// </summary>
        public string Units => units;

        #endregion properties
    }
}