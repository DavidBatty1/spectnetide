﻿namespace Spect.Net.Spectrum.Ula
{
    /// <summary>
    /// This class represents the parameters the ULA chip uses to render the Spectrum
    /// screen.
    /// </summary>
    public class UlaVideoDisplayParameters
    {
        /// <summary>
        /// Number of lines used for vertical synch
        /// </summary>
        public int VerticalSyncLines { get; }

        /// <summary>
        /// The number of top border lines that are not visible
        /// when rendering the screen
        /// </summary>
        public int NonVisibleTopBorderLines { get; }

        /// <summary>
        /// The number of border lines before the display
        /// </summary>
        public int BorderTopLines { get; }

        /// <summary>
        /// The number of border lines after the display
        /// </summary>
        public int BorderBottomLines { get; }

        /// <summary>
        /// The number of bottom border lines that are not visible
        /// when rendering the screen
        /// </summary>
        public int NonVisibleBottomBorderLines { get; }

        /// <summary>
        /// Number of display lines
        /// </summary>
        public int DisplayHeight { get; }

        /// <summary>
        /// The total number of lines in the screen
        /// </summary>
        public int ScreenHeight { get; }

        /// <summary>
        /// The number of border pixels to the left of the display
        /// </summary>
        public int BorderLeftPixels { get; }

        /// <summary>
        /// The number of border pixels to the right of the display
        /// </summary>
        public int BorderRightPixels { get; }

        /// <summary>
        /// The number of displayed pixels in a display row
        /// </summary>
        public int DisplayWidth { get; }

        /// <summary>
        /// The total width of the screen in pixels
        /// </summary>
        public int ScreenWidth { get; }

        /// <summary>
        /// Horizontal blanking time (HSync+blanking).
        /// Given in Z80 clock cycles.
        /// </summary>
        public int HorizontalBlankingTime { get; }

        /// <summary>
        /// The time of displaying left part of the border.
        /// Given in Z80 clock cycles.
        /// </summary>
        public int BorderLeftTime { get; }

        /// <summary>
        /// The time of displaying a pixel row.
        /// Given in Z80 clock cycles.
        /// </summary>
        public int PixelLineTime { get; }

        /// <summary>
        /// The time of displaying right part of the border.
        /// Given in Z80 clock cycles.
        /// </summary>
        public int BorderRightTime { get; }

        /// <summary>
        /// The time of displaying a full screen line.
        /// Given in Z80 clock cycles.
        /// </summary>
        public int ScreenLineTime { get; }

        /// <summary>
        /// Defines the number of Z80 clock cycles used for the full rendering
        /// of the screen.
        /// </summary>
        public int UlaFrameTactCount { get; }

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public UlaVideoDisplayParameters()
        {
            VerticalSyncLines = 8;
            NonVisibleTopBorderLines = 8;    // --- In a real screen this value is 0
            BorderTopLines = 48;             // --- In a real screen this value is 55
            BorderBottomLines = 48;          // --- In a real screen this value is 56
            NonVisibleBottomBorderLines = 8; // --- In a real screen this value is 0
            DisplayHeight = 192;
            ScreenHeight = BorderTopLines + DisplayHeight + BorderBottomLines;
            BorderLeftPixels = 48;
            BorderRightPixels = 48;
            DisplayWidth = 256;
            ScreenWidth = BorderLeftPixels + DisplayWidth + BorderRightPixels;
            HorizontalBlankingTime = 48;
            BorderLeftTime = 16;
            PixelLineTime = 128;
            BorderRightTime = 32;
            ScreenLineTime = HorizontalBlankingTime + BorderLeftTime + PixelLineTime + BorderRightTime;
            UlaFrameTactCount = (VerticalSyncLines + NonVisibleTopBorderLines + BorderTopLines 
                + DisplayHeight + BorderBottomLines + NonVisibleTopBorderLines) *
                (HorizontalBlankingTime + ScreenWidth/2);
        }
    }
}