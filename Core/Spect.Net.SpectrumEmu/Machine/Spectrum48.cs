﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Spect.Net.SpectrumEmu.Abstraction.Devices;
using Spect.Net.SpectrumEmu.Abstraction.Providers;
using Spect.Net.SpectrumEmu.Cpu;
using Spect.Net.SpectrumEmu.Devices.Beeper;
using Spect.Net.SpectrumEmu.Devices.Border;
using Spect.Net.SpectrumEmu.Devices.Interrupt;
using Spect.Net.SpectrumEmu.Devices.Keyboard;
using Spect.Net.SpectrumEmu.Devices.Memory;
using Spect.Net.SpectrumEmu.Devices.Screen;
using Spect.Net.SpectrumEmu.Devices.Tape;

#pragma warning disable 67

// ReSharper disable ConvertToAutoProperty
// ReSharper disable VirtualMemberCallInConstructor

namespace Spect.Net.SpectrumEmu.Machine
{
    /// <summary>
    /// This class represents a ZX Spectrum 48 virtual machine
    /// </summary>
    public class Spectrum48: ISpectrumVm, ISpectrumVmTestSupport
    {
        private readonly int _frameTacts;
        private bool _frameCompleted;
        private readonly List<ISpectrumBoundDevice> _spectrumDevices = new List<ISpectrumBoundDevice>();
        private readonly List<IFrameBoundDevice> _frameBoundDevices;
        private readonly List<ICpuOperationBoundDevice> _cpuBoundDevices;

        /// <summary>
        /// The CPU tick at which the last frame rendering started;
        /// </summary>
        public long LastFrameStartCpuTick;

        /// <summary>
        /// The last rendered ULA tact 
        /// </summary>
        public int LastRenderedUlaTact;

        /// <summary>
        /// The length of the physical frame in clock counts
        /// </summary>
        public double PhysicalFrameClockCount { get; }

        /// <summary>
        /// The Z80 CPU of the machine
        /// </summary>
        public IZ80Cpu Cpu { get; }

        /// <summary>
        /// Gets the ROM information of the virtual machine
        /// </summary>
        public RomInfo RomInfo { get; private set; }

        /// <summary>
        /// The current execution cycle options
        /// </summary>
        public ExecuteCycleOptions ExecuteCycleOptions { get; private set; }

        /// <summary>
        /// The memory device used by the virtual machine
        /// </summary>
        public ISpectrumMemoryDevice MemoryDevice { get; }

        /// <summary>
        /// The port device used by the virtual machine
        /// </summary>
        public ISpectrumPortDevice PortDevice { get; }

        /// <summary>
        /// The clock used within the VM
        /// </summary>
        public IClockProvider Clock { get; }

        /// <summary>
        /// The ULA border device used within the VM
        /// </summary>
        public IBorderDevice BorderDevice { get; }

        /// <summary>
        /// The ULA device that renders the VM screen
        /// </summary>
        public IScreenDevice ScreenDevice { get; }

        /// <summary>
        /// The ULA device that takes care of raising interrupts
        /// </summary>
        public IInterruptDevice InterruptDevice { get; }

        /// <summary>
        /// The current status of the keyboard
        /// </summary>
        public IKeyboardDevice KeyboardDevice { get; }

        /// <summary>
        /// The beeper device attached to the VM
        /// </summary>
        public IBeeperDevice BeeperDevice { get; }

        /// <summary>
        /// The tape device attached to the VM
        /// </summary>
        public ITapeDevice TapeDevice { get; }

        /// <summary>
        /// Debug info provider object
        /// </summary>
        public ISpectrumDebugInfoProvider DebugInfoProvider { get; set; }

        /// <summary>
        /// #of frames rendered
        /// </summary>
        public int FrameCount { get; private set; }

        /// <summary>
        /// #of tacts within the frame
        /// </summary>
        public int FrameTacts => _frameTacts;

        /// <summary>
        /// Gets the current frame tact according to the CPU tick count
        /// </summary>
        public virtual int CurrentFrameTact => (int)(Cpu.Tacts - LastFrameStartCpuTick);

        /// <summary>
        /// Overflow from the previous frame, given in #of tacts 
        /// </summary>
        public int Overflow { get; set; }

        /// <summary>
        /// The number of frame tact at which the interrupt signal is generated
        /// </summary>
        public int InterruptTact => 32;

        /// <summary>Initializes a new instance of the <see cref="T:System.Object" /> class.</summary>
        public Spectrum48(
            IRomProvider romProvider, 
            IClockProvider clockProvider, 
            IKeyboardProvider keyboardProvider, 
            IScreenFrameProvider pixelRenderer, 
            IEarBitFrameProvider earBitFrameProvider = null, 
            ITapeContentProvider loadContentProvider = null, 
            ISaveToTapeProvider tapeSaveToTapeProvider = null)
        {
            // --- Init the CPU 
            MemoryDevice = new Spectrum48MemoryDevice();
            PortDevice = new Spectrum48PortDevice();
            Cpu = new Z80Cpu(MemoryDevice, PortDevice);
            OsInitialized = false;

            // --- Setup the clock
            Clock = clockProvider;

            // --- Set up Spectrum devices
            BorderDevice = new BorderDevice();
            ScreenDevice = new Spectrum48ScreenDevice(pixelRenderer);
            BeeperDevice = new BeeperDevice(earBitFrameProvider);
            KeyboardDevice = new KeyboardDevice(keyboardProvider);
            InterruptDevice = new InterruptDevice(InterruptTact);
            TapeDevice = new TapeDevice(loadContentProvider, tapeSaveToTapeProvider);

            // --- Carry out frame calculations

            ResetUlaTact();
            _frameTacts = ScreenDevice.ScreenConfiguration.UlaFrameTactCount;
            PhysicalFrameClockCount = Clock.GetFrequency() / (double)ClockFrequeny * _frameTacts;
            FrameCount = 0;
            Overflow = 0;
            _frameCompleted = true;

            // --- Collect Spectrum devices
            _spectrumDevices.Add(MemoryDevice);
            _spectrumDevices.Add(PortDevice);
            _spectrumDevices.Add(BorderDevice);
            _spectrumDevices.Add(ScreenDevice);
            _spectrumDevices.Add(BeeperDevice);
            _spectrumDevices.Add(KeyboardDevice);
            _spectrumDevices.Add(InterruptDevice);
            _spectrumDevices.Add(TapeDevice);

            // --- Now, prepare devices to find each other
            foreach (var device in _spectrumDevices)
            {
                device.OnAttachedToVm(this);
            }

            // --- Prepare bound devices
            _frameBoundDevices = _spectrumDevices
                .OfType<IFrameBoundDevice>()
                .ToList();
            _cpuBoundDevices = _spectrumDevices
                .OfType<ICpuOperationBoundDevice>()
                .ToList();

            DebugInfoProvider = new SpectrumDebugInfoProvider();

            // --- Init the ROM
            InitRom(romProvider, "ZXSpectrum48");
        }

        /// <summary>
        /// Resets the CPU and the ULA chip
        /// </summary>
        public void Reset()
        {
            Cpu.SetResetSignal();
            ResetUlaTact();
            FrameCount = 0;
            Overflow = 0;
            _frameCompleted = true;
            foreach (var device in _spectrumDevices)
            {
                device.Reset();
            }
            Cpu.Reset();
            Cpu.ReleaseResetSignal();
        }

        /// <summary>
        /// Allow the device to react to the start of a new frame
        /// </summary>
        public void OnNewFrame()
        {
            foreach (var device in _frameBoundDevices)
            {
                device.OnNewFrame();
            }
        }

        /// <summary>
        /// Allow the device to react to the completion of a frame
        /// </summary>
        public void OnFrameCompleted()
        {
            foreach (var device in _frameBoundDevices)
            {
                device.Overflow = Overflow;
                device.OnFrameCompleted();
            }
        }

        public event EventHandler FrameCompleted;

        /// <summary>
        /// Resets the ULA tact to start screen rendering from the beginning
        /// </summary>
        public void ResetUlaTact()
        {
            LastRenderedUlaTact = -1;
        }

        /// <summary>
        /// Sets the debug info provider to the specified object
        /// </summary>
        /// <param name="provider">Provider object</param>
        public void SetDebugInfoProvider(ISpectrumDebugInfoProvider provider)
        {
            DebugInfoProvider = provider;
        }

        /// <summary>
        /// The main execution cycle of the Spectrum VM
        /// </summary>
        /// <param name="token">Cancellation token</param>
        /// <param name="options">Execution options</param>
        /// <return>True, if the cycle completed; false, if it has been cancelled</return>
        public bool ExecuteCycle(CancellationToken token, ExecuteCycleOptions options)
        {
            ExecuteCycleOptions = options;

            // --- We use these variables to calculate wait time at the end of the frame
            var cycleStartTime = Clock.GetCounter(); 
            var cycleFrameCount = 0;

            // --- We use this variable to check whether to stop in Debug mode
            var executedInstructionCount = -1;

            // --- Loop #1: The main cycle that goes on until cancelled
            while (!token.IsCancellationRequested)
            {
                if (_frameCompleted)
                {
                    // --- This counter helps us to calculate where we are in the frame after
                    // --- each CPU operation cycle
                    LastFrameStartCpuTick = Cpu.Tacts - Overflow;

                    // --- Notify devices to start a new frame
                    OnNewFrame();
                    LastRenderedUlaTact = Overflow;
                    _frameCompleted = false;
                }

                // --- We use this timestamp for debug information calculation
                var currentFrameStartCounter = Clock.GetCounter();

                DebugInfoProvider.CpuTime = 0;
                DebugInfoProvider.ScreenRenderingTime = 0;

                // --- Loop #2: The physical frame cycle that goes on while CPU and ULA 
                // --- processes everything whithin a physical frame (0.019968 second)
                while (!_frameCompleted)
                {
                    // --- Check debug mode when a CPU instruction has been entirelly executed
                    if (!Cpu.IsInOpExecution)
                    {
                        // --- Check for cancellation
                        if (token.IsCancellationRequested)
                        {
                            return false;
                        }

                        // --- The next instruction is about to be executed
                        executedInstructionCount++;

                        // --- Check for reaching the termination point
                        if (options.EmulationMode == EmulationMode.UntilExecutionPoint
                            && options.TerminationPoint == Cpu.Registers.PC)
                        {
                            // --- We reached the termination point
                            return true;
                        }

                        // --- Check for a debugging stop point
                        if (options.EmulationMode == EmulationMode.Debugger)
                        {
                            if (IsDebugStop(options.DebugStepMode, executedInstructionCount))
                            {
                                // --- At this point, the cycle should be stopped because of debugging reasons
                                // --- The screen should be refreshed
                                ScreenDevice.OnFrameCompleted();
                                return true;
                            }
                        }
                    }

                    // --- Check for interrupt signal generation
                    InterruptDevice.CheckForInterrupt(CurrentFrameTact);

                    // --- Run a single Z80 instruction
                    var cpuStart = Clock.GetCounter();
                    Cpu.ExecuteCpuCycle();
                    DebugInfoProvider.CpuTime += Clock.GetCounter() - cpuStart;

                    // --- Run a rendering cycle according to the current CPU tact count
                    var lastTact = CurrentFrameTact;
                    var renderStart = Clock.GetCounter();
                    ScreenDevice.RenderScreen(LastRenderedUlaTact + 1, lastTact);
                    DebugInfoProvider.ScreenRenderingTime += Clock.GetCounter() - renderStart;
                    LastRenderedUlaTact = lastTact;

                    // --- Exit if the emulation mode specifies so
                    if (options.EmulationMode == EmulationMode.UntilHalt 
                        && (Cpu.StateFlags & Z80StateFlags.Halted) != 0)
                    {
                        return true;
                    }

                    // --- Notify each CPU-bound device that the current operation has been completed
                    foreach (var device in _cpuBoundDevices)
                    {
                        device.OnCpuOperationCompleted();
                    }

                    // --- Decide whether this frame has been completed
                    _frameCompleted = !Cpu.IsInOpExecution && CurrentFrameTact >= _frameTacts;

                } // -- End Loop #2

                // --- A physical frame has just been completed. Take care about screen refresh
                cycleFrameCount++;
                FrameCount++;

                // --- Calculate debug information
                DebugInfoProvider.FrameTime = Clock.GetCounter() - currentFrameStartCounter;
                DebugInfoProvider.UtilityTime = DebugInfoProvider.FrameTime - DebugInfoProvider.CpuTime
                                                - DebugInfoProvider.ScreenRenderingTime;
                DebugInfoProvider.CpuTimeInMs = DebugInfoProvider.CpuTime / (double) Clock.GetFrequency() * 1000;
                DebugInfoProvider.ScreenRenderingTimeInMs =
                    DebugInfoProvider.ScreenRenderingTime / (double) Clock.GetFrequency() * 1000;
                DebugInfoProvider.UtilityTimeInMs =
                    DebugInfoProvider.UtilityTime / (double) Clock.GetFrequency() * 1000;
                DebugInfoProvider.FrameTimeInMs = DebugInfoProvider.FrameTime / (double) Clock.GetFrequency() * 1000;

                // --- Notify devices that the current frame completed
                OnFrameCompleted();

                // --- Exit if the emulation mode specifies so
                if (options.EmulationMode == EmulationMode.UntilFrameEnds)
                {
                    return true;
                }

                // --- Wait while the frame time ellapses
                var nextFrameCounter = cycleStartTime + cycleFrameCount * PhysicalFrameClockCount;
                Clock.WaitUntil((long) nextFrameCounter, token);

                // --- Start a new frame and carry on
                Overflow = CurrentFrameTact % _frameTacts;

            } // --- End Loop #1

            // --- The cycle has been inerrupted by cancellation
            return false;
        }

        /// <summary>
        /// Returns true when the current ZX Spectrum operating system has been initialized
        /// </summary>
        public bool OsInitialized { get; private set; }

        /// <summary>
        /// Trasfers the specified code into the virtual machine's memory
        /// </summary>
        /// <param name="startAddress">Start address</param>
        /// <param name="code">Code bytes</param>
        /// <param name="length">Code length, if not all bytes should by transferred</param>
        public void TransferCode(ushort startAddress, byte[] code, ushort? length = null)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Jumps to the specified code address
        /// </summary>
        /// <param name="startAddress">Address to jump to</param>
        public void JumpTo(ushort startAddress)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Checks whether the execution cycle should be stopped for debugging
        /// </summary>
        /// <param name="stepMode">Debug setp mode</param>
        /// <param name="executedInstructionCount">
        /// The count of instructions already executed in this cycle
        /// </param>
        /// <returns>True, if the execution should be stopped</returns>
        private bool IsDebugStop(DebugStepMode stepMode, int executedInstructionCount)
        {
            // --- No debug provider, no stop
            if (DebugInfoProvider == null) return false;

            // --- In Step-Into mode we always stop when we're about to
            // --- execute the next instruction
            if (stepMode == DebugStepMode.StepInto)
            {
                return executedInstructionCount > 0;
            }

            // --- In Stop-At-Breakpoint mode we stop only if a predefined
            // --- breakpoint is reached
            if (stepMode == DebugStepMode.StopAtBreakpoint
                && DebugInfoProvider.Breakpoints.ContainsKey(Cpu.Registers.PC))
            {
                // --- We do not stop unless we executed at least one instruction
                return executedInstructionCount > 0;
            }

            // --- We're in Step-Over mode
            if (stepMode == DebugStepMode.StepOver)
            {
                if (DebugInfoProvider.ImminentBreakpoint != null)
                {
                    // --- We also stop, if an imminent breakpoint is reached, and also remove
                    // --- this breakpoint
                    if (DebugInfoProvider.ImminentBreakpoint == Cpu.Registers.PC)
                    {
                        DebugInfoProvider.ImminentBreakpoint = null;
                        return true;
                    }
                }
                else
                {
                    var imminentJustCreated = false;

                    // --- We check for a CALL-like instruction
                    var length = Cpu.GetCallInstructionLength();
                    if (length > 0)
                    {
                        // --- Its a CALL-like instraction, create an imminent breakpoint
                        DebugInfoProvider.ImminentBreakpoint = (ushort)(Cpu.Registers.PC + length);
                        imminentJustCreated = true;
                    }

                    // --- We stop, we executed at least one instruction and if there's no imminent 
                    // --- breakpoint or we've just created one
                    if (executedInstructionCount > 0
                        && (DebugInfoProvider.ImminentBreakpoint == null || imminentJustCreated))
                    {
                        return true;
                    }
                }
            }

            // --- In any other case, we carry on
            return false;
        }

        /// <summary>
        /// This flag tells if the frame has just been completed.
        /// </summary>
        public bool HasFrameCompleted => _frameCompleted;

        /// <summary>
        /// Writes a byte to the memory
        /// </summary>
        /// <param name="addr">Memory address</param>
        /// <param name="value">Data byte</param>
        public void WriteSpectrumMemory(ushort addr, byte value) =>
            MemoryDevice.OnWriteMemory(addr, value);

        #region Helper functions

        /// <summary>
        /// Loads the content of the ROM through the specified provider
        /// </summary>
        /// <param name="romProvider">ROM provider instance</param>
        /// <param name="romResourceName">ROM Resource name</param>
        /// <remarks>
        /// The content of the ROM is copied into the memory
        /// </remarks>
        public void InitRom(IRomProvider romProvider, string romResourceName)
        {
            RomInfo = romProvider.LoadRom(romResourceName);
            MemoryDevice.FillMemory(RomInfo.RomBytes);
        }

        #endregion

        /// <summary>
        /// Gets the frequency of the virtual machine's clock in Hz
        /// </summary>
        public int ClockFrequeny => 3_500_000;
    }

#pragma warning restore
}