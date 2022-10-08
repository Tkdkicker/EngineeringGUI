namespace Drivers
{
    /// <summary>
    ///
    /// </summary>
    public class SfpMemory3000 : SfpMemoryBase
    {
        #region constructors

        public SfpMemory3000() : base()
        {
            NarrowWaveRegisters();
            ExternalCalibration();
        }

        #endregion constructors

        #region protected override methods

        /// <summary>
        /// Add the A0 customer registers
        /// </summary>
        protected override void A0CustomerRegisters()
        {
            base.A0CustomerRegisters();
        }

        /// <summary>
        /// Add the Effect specific registers
        ///
        /// </summary>
        protected override void VendorRegisters()
        {
            registers.Add(SlaveRegister.effectFirmwareRevision, new RegisterInfo(0x84, 0, numberBytes: 4, regType: typeof(string)));
            registers.Add(SlaveRegister.effectSerialNumber, new RegisterInfo(0x84, 4, numberBytes: 16, regType: typeof(string)));

            registers.Add(SlaveRegister.effectTxPoHighAlarm, new RegisterInfo(0x84, 20, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "mW"));//mW
            registers.Add(SlaveRegister.effectBurinInErrorBit, new RegisterInfo(0x84, 22, numberBytes: 1, theBitMask: 0x02, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(byte)));


            //Bit 1
            registers.Add(SlaveRegister.effectRxTxCDRsOperationBit, new RegisterInfo(0x84, 23, numberBytes: 1, theBitMask: 0x02, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(byte)));
           
            //Bit 0
            registers.Add(SlaveRegister.effectNarrowWaveImplementedBit, new RegisterInfo(0x84, 23, numberBytes: 1, theBitMask: 0x01, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(byte)));


            registers.Add(SlaveRegister.effectMaxPeakTemp, new RegisterInfo(0x84, 28, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));
            registers.Add(SlaveRegister.effectMinPeakTemp, new RegisterInfo(0x84, 30, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: (1.0 / 256), regType: typeof(double), displayUnits: "degC"));




            registers.Add(SlaveRegister.effectMaxPeakSupply, new RegisterInfo(0x84, 32, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));
            registers.Add(SlaveRegister.effectMinPeakSupply, new RegisterInfo(0x84, 34, numberBytes: 2, isSignedRegister: false, pageSelect: 0, scaleFactor: (1.0 / 10000), regType: typeof(double), displayUnits: "V"));



            //Internal temp error latch, power supply latch, initialisation error latch, state transition error latch
            registers.Add(SlaveRegister.effectErrorLatch, new RegisterInfo(0x84, 36, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.effectErrorReason, new RegisterInfo(0x84, 37, numberBytes: 1, regType: typeof(byte)));

            //TEC error, internal fault  latches
            registers.Add(SlaveRegister.effectTxFaultReason, new RegisterInfo(0x84, 38, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.effectModuleState, new RegisterInfo(0x84, 39, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.calLaserFirstFreqTHz, new RegisterInfo(0x84, 100, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(ushort), displayUnits: "THz"));

            registers.Add(SlaveRegister.calLaserFirstFreqGHz, new RegisterInfo(0x84, 102, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.1, regType: typeof(double), displayUnits: "GHz"));

            registers.Add(SlaveRegister.calLaserLastFreqTHz, new RegisterInfo(0x84, 104, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 1, regType: typeof(ushort), displayUnits: "THz"));
            registers.Add(SlaveRegister.calLaserLastFreqGHz, new RegisterInfo(0x84, 106, numberBytes: 2, isSignedRegister: false, pageSelect: 2, scaleFactor: 0.1, regType: typeof(ushort), displayUnits: "GHz"));

            //other revision details
            registers.Add(SlaveRegister.aaaRevisionTag, new RegisterInfo(0x84, 41, numberBytes: 3, regType: typeof(string)));
            registers.Add(SlaveRegister.bbbRevisionTag, new RegisterInfo(0x84, 44, numberBytes: 3, regType: typeof(string)));
            registers.Add(SlaveRegister.cccRevisionTag, new RegisterInfo(0x84, 47, numberBytes: 3, regType: typeof(string)));

            registers.Add(SlaveRegister.hashGitCommit, new RegisterInfo(0x84, 50, numberBytes: 8, regType: typeof(string)));

            registers.Add(SlaveRegister.effectPartNumber, new RegisterInfo(0x84, 58, numberBytes: 16, regType: typeof(string)));
        }

        /// <summary>
        /// Add the Effect specific registers for Narrowwave
        /// </summary>
        protected void NarrowWaveRegisters()
        {
            registers.Add(SlaveRegister.nWRemoteChannel, new RegisterInfo(0xA2, 190, numberBytes: 2, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(ushort), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWAutoTuningMode, new RegisterInfo(0xA2, 192, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWStatus, new RegisterInfo(0xA2, 193, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));

            registers.Add(SlaveRegister.nWDiagTransmitMode, new RegisterInfo(0xA2, 194, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWDiagTransmitData, new RegisterInfo(0xA2, 195, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWDiagTransmitReg, new RegisterInfo(0xA2, 196, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));

            registers.Add(SlaveRegister.nWDiagReceiveAddr, new RegisterInfo(0xA2, 197, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWDiagReceiveRegister, new RegisterInfo(0xA2, 198, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));
            registers.Add(SlaveRegister.nWDiagReceiveData, new RegisterInfo(0xA2, 199, numberBytes: 2, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(ushort), displayUnits: string.Empty));

            registers.Add(SlaveRegister.nWControl, new RegisterInfo(0xA2, 201, numberBytes: 1, isSignedRegister: false, pageSelect: 0x80, scaleFactor: 1, regType: typeof(byte), displayUnits: string.Empty));

            #endregion protected override methods
        }

        protected void ExternalCalibration()
        {
            registers.Add(SlaveRegister.rxPwr4_calibrationValue, new RegisterInfo(0xA2, 56, numberBytes: 4, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.rxPwr3_calibrationValue, new RegisterInfo(0xA2, 60, numberBytes: 4, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.rxPwr2_calibrationValue, new RegisterInfo(0xA2, 64, numberBytes: 4, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.rxPwr1_calibrationValue, new RegisterInfo(0xA2, 68, numberBytes: 4, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.rxPwr0_calibrationValue, new RegisterInfo(0xA2, 72, numberBytes: 4, isSignedRegister: false, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));

            registers.Add(SlaveRegister.Tx_I_Slope_calibrationValue, new RegisterInfo(0xA2, 76, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.Tx_I_Offset_calibrationValue, new RegisterInfo(0xA2, 78, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.Tx_Pwr_Slope_calibrationValue, new RegisterInfo(0xA2, 80, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.Tx_Pwr_Offset_calibrationValue, new RegisterInfo(0xA2, 82, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));

            registers.Add(SlaveRegister.T_Slope_calibrationValue, new RegisterInfo(0xA2, 84, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.T_Offset_calibrationValue, new RegisterInfo(0xA2, 86, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.V_Slope_calibrationValue, new RegisterInfo(0xA2, 88, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
            registers.Add(SlaveRegister.V_Offset_calibrationValue, new RegisterInfo(0xA2, 90, numberBytes: 2, isSignedRegister: true, pageSelect: 0, scaleFactor: 1, regType: typeof(double), displayUnits: string.Empty));
        }
    }
}