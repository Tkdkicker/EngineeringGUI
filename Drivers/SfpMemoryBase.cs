using System;
using System.Collections.Generic;

namespace Drivers
{
    /// <summary>
    /// Enum used as dictionary key to the register information
    /// </summary>
    public enum SlaveRegister
    {
        #region A0

        A0physicalDeviceID,
        A0physicalDeviceExtendedID,
        A0connectorValues,
        A010GCompCode,
        A0ESCON_SONET_Compliance1,
        A0ESCON_SONET_Compliance2,
        A0EthernetCompliance,
        A0FibreChanLinkLength,
        A0SFPCableTech,
        A0FibreChanTransMedia,
        A0FibreChanSpeed,
        A0Encoding,
        A0NomBitRate,
        A0RateID,
        A0Length_km,
        A0Length_m,
        A0Length_OM2,
        A0Length_OM1,
        A0Length_OM4,
        A0Length_OM3,

        A0vendorName,

        A0ExtendedCompCodes,
        A0VendorOUI,

        A0vendorPN,
        A0fixedWavelengthASCII,
        A0fixedWavelength,
        A0vendorRev,

        #region fixedWavelength

        A0fixedWavelengthInt,
        A0fixedWavelengthFraction,

        #endregion fixedWavelength

        A0wavelengthFraction,
        A0wavelength,

        A0firmwareVersion,

        A0implementation_1,
        A0MaxBitRate,
        A0MinBitRate,
        A0serialNumber,
        A0VendorLotCode,
        A0implementation_2,
        A0implementation_3,

        #endregion A0

        #region Effect

        //Can be A0 or 0x84 depending on firmware version need password set to read these for 0x84 (version 3000 onwards)
        effectFirmwareRevision,

        effectSerialNumber,

        effectTxPoHighAlarm,
        effectBurinInErrorBit,

        effectNarrowWaveImplementedBit,
        effectRxTxCDRsOperationBit,

        effectMaxPeakTemp,
        effectMinPeakTemp,
        effectMaxPeakSupply,
        effectMinPeakSupply,

        effectErrorLatch,
        effectErrorReason,
        effectTxFaultReason,
        effectModuleState,

        effectWavelengthBand,

        aaaRevisionTag,
        bbbRevisionTag,
        cccRevisionTag,
        hashGitCommit,

        effectPartNumber,

        calLaserFirstFreqTHz,
        calLaserFirstFreqGHz,
        calLaserLastFreqTHz,
        calLaserLastFreqGHz,

        #endregion Effect

        #region alarmswarnings

        moduleTempHighAlarm,
        moduleTempLowAlarm,
        moduleTempHighWarning,
        moduleTempLowWarning,
        supplyVoltageHighAlarm,
        supplyVoltageLowAlarm,
        supplyVoltageHighWarning,
        supplyVoltageLowWarning,
        txBiasHighAlarm,
        txBiasLowAlarm,
        txBiasHighWarning,
        txBiasLowWarning,
        txPoHighAlarm,
        txPoLowAlarm,
        txPoHighWarning,
        txPoLowWarning,
        rxPoHighAlarm,
        rxPoLowAlarm,
        rxPoHighWarning,
        rxPoLowWarning,

        #endregion alarmswarnings

        #region externalCal

        rxPwr4_calibrationValue,
        rxPwr3_calibrationValue,
        rxPwr2_calibrationValue,
        rxPwr1_calibrationValue,
        rxPwr0_calibrationValue,
        Tx_I_Slope_calibrationValue,
        Tx_I_Offset_calibrationValue,
        Tx_Pwr_Slope_calibrationValue,
        Tx_Pwr_Offset_calibrationValue,
        T_Slope_calibrationValue,
        T_Offset_calibrationValue,
        V_Slope_calibrationValue,
        V_Offset_calibrationValue,

        #endregion externalCal

        measuredModuleTemperature,

        measuredSupplyVoltage,
        measuredTxBias,
        measuredTxPower,
        measuredRxPower,

        pinStateAndSoftSelect,
        dataReadyBarStateBit,
        cdrState,

        wavelengthReporting,
        alarmFlags_1,
        alarmFlags_2,
        warningFlags_1,
        warningFlags_2,

        #region A2Page2tunable

        laserFirstFreqTHz,
        laserFirstFreqGHz,
        laserLastFreqTHz,
        laserLastFreqGHz,
        minSupportedGridSpacingGHz,

        channelNumber,
        wavelengthSet,
        TxDither,

        frequencyErrorGHz,
        wavelengthError_nm,

        tunableWavelengthStatus,
        tunableWavelengthUnlocked,
        latchedTunableStatus,

        #endregion A2Page2tunable

        #region NarrowWave

        nWRemoteChannel,
        nWAutoTuningMode,
        nWStatus,
        nWDiagTransmitMode,
        nWDiagTransmitData,
        nWDiagTransmitReg,
        nWDiagReceiveAddr,
        nWDiagReceiveRegister,
        nWDiagReceiveData,
        nWDiagReceiveDataPlusOne,
        nWControl,

        #endregion NarrowWave

        password_3,
        password_2,
        password_1,
        password_0
        //tempHighAlarmFlag,
        //tempLowAlarmFlag,
        //supplyVoltageHighAlarmFlag,
    }

    /// <summary>
    ///  Base class to hold the common NVM register information of SFP+ device
    /// </summary>
    public class SfpMemoryBase : ISfpMemoryBase
    {
        /// <summary>
        /// Dictionary to hold the register information as a key value pair
        /// </summary>
        protected Dictionary<SlaveRegister, RegisterInfo> registers;

        public Dictionary<SlaveRegister, RegisterInfo> Registers => registers;

        #region constructors

        public SfpMemoryBase()
        {
            registers = new Dictionary<SlaveRegister, RegisterInfo>();
            LoadRegisters();
        }

        #endregion constructors

        #region protected methods

        /// <summary>
        /// Load the register information
        /// </summary>
        protected void LoadRegisters()
        {
            A0CustomerRegisters();
            A0CustomerFixedWavelengthRegisters();
            VendorRegisters();
            A2Page2tunable();
            A2AlarmWarningThresholds();
            A2MeasuredValues();
            A2Diagnostics();
            A2pinStates();
            A2Password();
        }

        protected void A2Diagnostics()
        {
            registers.Add(SlaveRegister.warningFlags_1, new RegisterInfo(0xA2, 116, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.warningFlags_2, new RegisterInfo(0xA2, 117, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.alarmFlags_1, new RegisterInfo(0xA2, 112, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.alarmFlags_2, new RegisterInfo(0xA2, 113, numberBytes: 1, regType: typeof(byte)));
        }

        /// <summary>
        /// Load the A2 alarms & warning thresholds
        /// </summary>
        protected void A2AlarmWarningThresholds()
        {
            registers.Add(SlaveRegister.moduleTempHighAlarm, new RegisterInfo(0xA2, 0, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));
            registers.Add(SlaveRegister.moduleTempLowAlarm, new RegisterInfo(0xA2, 2, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));

            registers.Add(SlaveRegister.moduleTempHighWarning, new RegisterInfo(0xA2, 4, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));
            registers.Add(SlaveRegister.moduleTempLowWarning, new RegisterInfo(0xA2, 6, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));

            registers.Add(SlaveRegister.supplyVoltageHighAlarm, new RegisterInfo(0xA2, 8, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));
            registers.Add(SlaveRegister.supplyVoltageLowAlarm, new RegisterInfo(0xA2, 10, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));

            registers.Add(SlaveRegister.supplyVoltageHighWarning, new RegisterInfo(0xA2, 12, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));
            registers.Add(SlaveRegister.supplyVoltageLowWarning, new RegisterInfo(0xA2, 14, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));

            registers.Add(SlaveRegister.txBiasHighAlarm, new RegisterInfo(0xA2, 16, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (2.0 / 1000), regType: typeof(double), displayUnits: "mA"));//mA
            registers.Add(SlaveRegister.txBiasLowAlarm, new RegisterInfo(0xA2, 18, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (2.0 / 1000), regType: typeof(double), displayUnits: "mA"));

            registers.Add(SlaveRegister.txBiasHighWarning, new RegisterInfo(0xA2, 20, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (2.0 / 1000), regType: typeof(double), displayUnits: "mA"));//mA
            registers.Add(SlaveRegister.txBiasLowWarning, new RegisterInfo(0xA2, 22, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (2.0 / 1000), regType: typeof(double), displayUnits: "mA"));

            registers.Add(SlaveRegister.txPoHighAlarm, new RegisterInfo(0xA2, 24, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.txPoLowAlarm, new RegisterInfo(0xA2, 26, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));

            registers.Add(SlaveRegister.txPoHighWarning, new RegisterInfo(0xA2, 28, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.txPoLowWarning, new RegisterInfo(0xA2, 30, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));

            registers.Add(SlaveRegister.rxPoHighAlarm, new RegisterInfo(0xA2, 32, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.rxPoLowAlarm, new RegisterInfo(0xA2, 34, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));

            registers.Add(SlaveRegister.rxPoHighWarning, new RegisterInfo(0xA2, 36, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.rxPoLowWarning, new RegisterInfo(0xA2, 38, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));
        }

        protected void A2MeasuredValues()
        {
            registers.Add(SlaveRegister.measuredModuleTemperature, new RegisterInfo(0xA2, 96, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));
            registers.Add(SlaveRegister.measuredSupplyVoltage, new RegisterInfo(0xA2, 98, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));
            registers.Add(SlaveRegister.measuredTxBias, new RegisterInfo(0xA2, 100, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (2.0 / 1000), regType: typeof(double), displayUnits: "mA"));//mA
            registers.Add(SlaveRegister.measuredTxPower, new RegisterInfo(0xA2, 102, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (0.1 / 1000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.measuredRxPower, new RegisterInfo(0xA2, 104, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (0.1 / 1000), regType: typeof(double), displayUnits: "mW"));//mW
        }

        protected void A2Password()
        {
            registers.Add(SlaveRegister.password_3, new RegisterInfo(0xA2, 123, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.password_2, new RegisterInfo(0xA2, 124, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.password_1, new RegisterInfo(0xA2, 125, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.password_0, new RegisterInfo(0xA2, 126, numberBytes: 1, regType: typeof(byte)));
        }

        protected void A2pinStates()
        {
            registers.Add(SlaveRegister.pinStateAndSoftSelect, new RegisterInfo(0xA2, 110, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.cdrState, new RegisterInfo(0xA2, 119, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.dataReadyBarStateBit, new RegisterInfo(0xA2, 110, numberBytes: 1, theBitMask: 0x01, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(byte)));
        }

        /// <summary>
        /// Add the 2A page 2 tunable registers
        /// </summary>
        protected void A2Page2tunable()
        {
            //This one only applicable for single wavelength set up
            registers.Add(SlaveRegister.wavelengthReporting, new RegisterInfo(0xA2, 111, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.laserFirstFreqTHz, new RegisterInfo(0xA2, 132, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(ushort), displayUnits: "THz"));

            registers.Add(SlaveRegister.laserFirstFreqGHz, new RegisterInfo(0xA2, 134, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.1, regType: typeof(ushort), displayUnits: "GHz"));

            registers.Add(SlaveRegister.laserLastFreqTHz, new RegisterInfo(0xA2, 136, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(ushort), displayUnits: "THz"));
            registers.Add(SlaveRegister.laserLastFreqGHz, new RegisterInfo(0xA2, 138, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.1, regType: typeof(ushort), displayUnits: "GHz"));
            registers.Add(SlaveRegister.minSupportedGridSpacingGHz, new RegisterInfo(0xA2, 140, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.1, regType: typeof(ushort), displayUnits: "GHz"));

            registers.Add(SlaveRegister.channelNumber, new RegisterInfo(0xA2, 144, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(ushort), displayUnits: string.Empty));
            registers.Add(SlaveRegister.wavelengthSet, new RegisterInfo(0xA2, 146, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.05, regType: typeof(double), displayUnits: "nm"));

            registers.Add(SlaveRegister.TxDither, new RegisterInfo(0xA2, 151, numberBytes: 1, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.frequencyErrorGHz, new RegisterInfo(0xA2, 152, numberBytes: 2, isSignedRegister: true, pageSelect: 2, scaleFactor: 0.1, regType: typeof(double), displayUnits: "GHz"));
            registers.Add(SlaveRegister.wavelengthError_nm, new RegisterInfo(0xA2, 154, numberBytes: 2, isSignedRegister: true, pageSelect: 2, scaleFactor: 0.005, regType: typeof(double), displayUnits: "nm"));

            registers.Add(SlaveRegister.tunableWavelengthStatus, new RegisterInfo(0xA2, 168, numberBytes: 1, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));

            registers.Add(SlaveRegister.tunableWavelengthUnlocked, new RegisterInfo(0xA2, 168, numberBytes: 1, theBitMask: 0x20, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.latchedTunableStatus, new RegisterInfo(0xA2, 172, numberBytes: 1, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
        }

        #endregion protected methods

        #region protected virtual methods

        /// <summary>
        /// Add the A0 customer registers
        /// </summary>
        protected virtual void A0CustomerRegisters()
        {
            registers.Add(SlaveRegister.A0physicalDeviceID, new RegisterInfo(slave: 0xA0, address: 0, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0physicalDeviceExtendedID, new RegisterInfo(slave: 0xA0, address: 1, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0connectorValues, new RegisterInfo(slave: 0xA0, address: 2, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A010GCompCode, new RegisterInfo(slave: 0xA0, address: 3, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0ESCON_SONET_Compliance1, new RegisterInfo(slave: 0xA0, address: 4, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0ESCON_SONET_Compliance2, new RegisterInfo(slave: 0xA0, address: 5, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0EthernetCompliance, new RegisterInfo(slave: 0xA0, address: 6, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0FibreChanLinkLength, new RegisterInfo(slave: 0xA0, address: 7, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0SFPCableTech, new RegisterInfo(slave: 0xA0, address: 8, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0FibreChanTransMedia, new RegisterInfo(slave: 0xA0, address: 9, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0FibreChanSpeed, new RegisterInfo(slave: 0xA0, address: 10, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Encoding, new RegisterInfo(slave: 0xA0, address: 11, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0NomBitRate, new RegisterInfo(slave: 0xA0, address: 12, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0RateID, new RegisterInfo(slave: 0xA0, address: 13, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Length_km, new RegisterInfo(slave: 0xA0, address: 14, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Length_m, new RegisterInfo(slave: 0xA0, address: 15, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0Length_OM2, new RegisterInfo(slave: 0xA0, address: 16, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Length_OM1, new RegisterInfo(slave: 0xA0, address: 17, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Length_OM4, new RegisterInfo(slave: 0xA0, address: 18, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0Length_OM3, new RegisterInfo(slave: 0xA0, address: 19, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0vendorName, new RegisterInfo(slave: 0xA0, address: 20, numberBytes: 16, regType: typeof(string)));

            registers.Add(SlaveRegister.A0ExtendedCompCodes, new RegisterInfo(slave: 0xA0, address: 36, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0vendorPN, new RegisterInfo(slave: 0xA0, address: 40, numberBytes: 16, regType: typeof(string)));

            registers.Add(SlaveRegister.A0vendorRev, new RegisterInfo(slave: 0xA0, 56, numberBytes: 4, regType: typeof(string)));

            registers.Add(SlaveRegister.A0implementation_1, new RegisterInfo(slave: 0xA0, address: 65, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0MaxBitRate, new RegisterInfo(slave: 0xA0, address: 66, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0MinBitRate, new RegisterInfo(slave: 0xA0, address: 67, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.A0serialNumber, new RegisterInfo(slave: 0xA0, address: 68, numberBytes: 16, regType: typeof(string)));

            registers.Add(SlaveRegister.A0VendorLotCode, new RegisterInfo(slave: 0xA0, address: 90, numberBytes: 2, regType: typeof(string)));

            registers.Add(SlaveRegister.A0implementation_2, new RegisterInfo(slave: 0xA0, address: 92, numberBytes: 1, regType: typeof(byte)));
            registers.Add(SlaveRegister.A0implementation_3, new RegisterInfo(slave: 0xA0, address: 93, numberBytes: 1, regType: typeof(byte)));
        }

        /// <summary>
        /// Add the A0 customer registers
        /// </summary>
        protected virtual void A0CustomerFixedWavelengthRegisters()
        {
            registers.Add(SlaveRegister.A0fixedWavelengthASCII, new RegisterInfo(slave: 0xA0, address: 48, numberBytes: 8, regType: typeof(string)));
            registers.Add(SlaveRegister.A0fixedWavelength, new RegisterInfo(slave: 0xA0, address: 48, numberBytes: 8, regType: typeof(string)));

            registers.Add(SlaveRegister.A0fixedWavelengthInt, new RegisterInfo(slave: 0xA0, address: 60, numberBytes: 2, regType: typeof(ushort)));
            registers.Add(SlaveRegister.A0fixedWavelengthFraction, new RegisterInfo(0xA0, 62, numberBytes: 1, isSignedRegister: false, pageSelect: 0, scaleFactor: 0.01, regType: typeof(double), displayUnits: "nm"));
        }

        /// <summary>
        /// Add the Effect specific registers this will be overwriiten in the derived classes
        /// </summary>
        protected virtual void VendorRegisters()
        {
        }

        #endregion protected virtual methods

        /// <summary>
        /// Returns information about a register you may want to read
        /// </summary>
        /// <param name="slaveMemory"></param>
        /// <returns></returns>
        public RegisterInfo ReadRegisterInfo(SlaveRegister slaveMemory)
        {
            if (Registers.ContainsKey(slaveMemory))
            {
                return Registers[slaveMemory];
            }
            return null;
        }
    }
}