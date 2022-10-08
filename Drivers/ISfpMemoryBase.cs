using System.Collections.Generic;

namespace Drivers
{
    public interface ISfpMemoryBase
    {
        Dictionary<SlaveRegister, RegisterInfo> Registers { get; }

        RegisterInfo ReadRegisterInfo(SlaveRegister slaveMemory);
    }
}