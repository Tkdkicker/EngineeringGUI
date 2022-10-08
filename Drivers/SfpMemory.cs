using System;

namespace Drivers
{
    /// <summary>
    /// Class to hold the NVM register information of SFP+ device
    /// </summary>
    public class SfpMemory : SfpMemoryBase
    {
        #region constructors

        public SfpMemory() : base()
        {
        }

        #endregion constructors

        #region protected override methods

        /// <summary>
        /// Add the A0 customer registers
        /// </summary>
        protected override void A0CustomerRegisters()
        {
            base.A0CustomerRegisters();
            registers.Add(SlaveRegister.A0wavelength, new RegisterInfo(slave: 0xA0, address: 60, numberBytes: 2, regType: typeof(ushort)));

            registers.Add(SlaveRegister.A0wavelengthFraction, new RegisterInfo(slave: 0xA0, address: 62, numberBytes: 1, isSignedRegister: false, pageSelect: 0, scaleFactor: 0.01, regType: typeof(double), displayUnits:"nm"));//nm
        }

        /// <summary>
        /// Add the Effect specific registers
        /// </summary>
        protected override void VendorRegisters()
        {
            registers.Add(SlaveRegister.effectFirmwareRevision, new RegisterInfo(0xA0, 96, numberBytes: 4, regType: typeof(string)));
            registers.Add(SlaveRegister.effectSerialNumber, new RegisterInfo(0xA0, 100, numberBytes: 16, regType: typeof(string)));

            //Internal temp error latch, power supply latch, initialisation error latch, state transition error latch
            registers.Add(SlaveRegister.effectErrorLatch, new RegisterInfo(0xA0, 116, numberBytes: 1, regType: typeof(byte)));

            //TEC error, internal fault  latches
            registers.Add(SlaveRegister.effectTxFaultReason, new RegisterInfo(0xA0, 118, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.effectModuleState, new RegisterInfo(0xA0, 125, numberBytes: 1, regType: typeof(byte)));

            registers.Add(SlaveRegister.effectWavelengthBand, new RegisterInfo(0xA0, 127, numberBytes: 1, regType: typeof(byte)));
        }

        #endregion protected override methods
    }
}