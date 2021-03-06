﻿using System;

namespace Spect.Net.SpectrumEmu.Disassembler
{
    /// <summary>
    /// This class specifies the spectrum disassembly flags that can be passed
    /// to the Z80 disassembler to provide Spectrum ROM-specific disassembly
    /// </summary>
    [Flags]
    public enum SpectrumSpecificDisassemblyFlags
    {
        Spectrum48Rst08 = 0x0001,
        Spectrum48Rst28 = 0x0002,
        Spectrum48All = Spectrum48Rst08 | Spectrum48Rst28
    }
}