﻿namespace Spect.Net.SpectrumEmu.Devices.Keyboard
{
    /// <summary>
    /// This class represents the information about an emulated key press 
    /// </summary>
    public class EmulatedKeyStroke
    {
        /// <summary>
        /// Start CPU tact
        /// </summary>
        public long StartTact { get; }

        /// <summary>
        /// End CPU tact
        /// </summary>
        public long EndTact { get; }

        /// <summary>
        /// The primary key's code
        /// </summary>
        public SpectrumKeyCode PrimaryCode { get; }

        /// <summary>
        /// Secondary key code
        /// </summary>
        public SpectrumKeyCode? SecondaryCode { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object"></see> class.</summary>
        public EmulatedKeyStroke(long startTact, long endTact, SpectrumKeyCode primaryCode, 
            SpectrumKeyCode? secondaryCode)
        {
            StartTact = startTact;
            EndTact = endTact;
            PrimaryCode = primaryCode;
            SecondaryCode = secondaryCode;
        }
    }
}