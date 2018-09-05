using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Linq;

namespace BQEV23K
{
    /// <summary>
    /// EV2300 error codes.
    /// </summary>
    /// <remarks>
    /// Defined in BQ80XRW.dll.
    /// </remarks>
    public enum EV23KError : short
    {
        NoError = 0,
        LostSync = 1,
        NoUSB = 2,
        BadPEC = 3,
        WronNumBytes = 5,
        Unknown = 6,
        SMBCLocked = 260,
        SMBDLocked = 516,
        SMBNAC = 772,
        SMBDLow = 1028,
        SMBLocked = 1284,
        IncorrectParameter = 7, // Invalid parameter type passed to function
        USBTimeoutError = 8,
        InvalidData = 9, // AssemblePacket could not build a valid packet
        UnsolicitedPacket = 10
    }

    /// <summary>
    /// EV2300 GPIO mask.
    /// </summary>
    /// <remarks>
    /// Some GPIOs are only accessible on the EV2300 PCB.
    /// </remarks>
    public enum EV23KGpioMask : short
    {
        D19 = 0x0001,       // D17 PCB bottom = LED D19 PCB top, open collector
        D15 = 0x0002,       // D15 PCB bottom, open collector
        D14 = 0x0004,       // D14 PCB bottom, open collector
        D13 = 0x0008,       // D13 PCB bottom, open collector
        VOUT = 0x0010,      // Pin VOUT on HDQ header, push-pull
        HDQ = 0x0020,       // Pin HDQ on HDQ header, push-pull
        I2CSCL = 0x0040,    // I2C SCL pin, open collector
        I2CSDA = 0x0080     // I2C SDA pin, open collector
    }

    /// <summary>
    /// EV2300 hardware interface control class.
    /// </summary>
    public class EV23K : IDisposable
    {
        private AxBQ80XRWLib.AxBq80xRW EV23KBoard;
        private const double CheckStatusPeriodeMilliseconds = 5000;
        private Timer timerCheckStatus;
        private bool isPresent = false;
        private string name = string.Empty;
        private string version = string.Empty;

        #region Properties
        /// <summary>
        /// Get presents status of EV2300 hardware interface.
        /// </summary>
        public bool IsPresent
        {
            get
            {
                return isPresent;
            }
        }

        /// <summary>
        /// Get name of EV2300 hardware interface.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
        }

        /// <summary>
        /// Get EV2300 version string.
        /// </summary>
        public string Version
        {
            get
            {
                return version;
            }
        }
        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="host">Host for BQ80xRW COM object.</param>
        public EV23K(out System.Windows.Forms.Integration.WindowsFormsHost host)
        {
            host = new System.Windows.Forms.Integration.WindowsFormsHost();
            EV23KBoard = new AxBQ80XRWLib.AxBq80xRW();
            host.Child = EV23KBoard;

            timerCheckStatus = new Timer(5000);
            timerCheckStatus.Elapsed += new ElapsedEventHandler(CheckStatus);
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            isPresent = false;
            timerCheckStatus.Close();
            EV23KBoard.Dispose();
        }

        /// <summary>
        /// Check present status of EV2300 board.
        /// </summary>
        /// <param name="sender">Not used</param>
        /// <param name="e">Not used</param>
        private void CheckStatus(object sender, System.EventArgs e)
        {
            try
            {
                EV23KError err = (EV23KError)EV23KBoard.GPIOMask(0);
                if (err == EV23KError.NoUSB)
                {
                    isPresent = false;
                }
                else
                {
                    isPresent = true;
                }
            } catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Connect to EV2300 board.
        /// </summary>
        /// <param name="BrdsNumber">Board numbers found and connected to.</param>
        /// <param name="BrdsName">Connected board name.</param>
        /// <returns>EV2300 error code</returns>
        public EV23KError Connect(out int BrdsNumber, out string BrdsName)
        {
            try
            {
                BrdsNumber = 0;
                BrdsName = "";

                object obj = null;
                short len = 0, ver = 0, rev = 0;

                EV23KBoard.GetFreeBoards(1, ref BrdsNumber, ref BrdsName);

                if (BrdsNumber <= 0)
                    return EV23KError.IncorrectParameter;

                BrdsName = BrdsName.Substring(0, BrdsName.Length - 1);
                EV23KError err = (EV23KError)EV23KBoard.OpenDevice(ref BrdsName);
                if(err == EV23KError.NoError)
                {
                    timerCheckStatus.Enabled = true;
                    CheckStatus(null, null);

                    err = (EV23KError)EV23KBoard.GetEV2300Name(ref obj, ref len);
                    if(name != null)
                    {
                        name = Encoding.ASCII.GetString((byte[])obj);
                        int i = name.IndexOf('\0');
                        if (i >= 0) name = name.Substring(0, i);
                    }

                    err = (EV23KError)EV23KBoard.GetEV2300Version(ref ver, ref rev);
                    if(err == EV23KError.NoError)
                    {
                        version = Encoding.ASCII.GetString(new byte[] { (byte)((ver & 0xff00) >> 8), 0x2E, (byte)(ver & 0x00ff), (byte)rev });
                    }

                    err = (EV23KError)EV23KBoard.GPIOWrite(0x7FF, 0);
                }
                return err;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Disconnect EV2300 board.
        /// </summary>
        public async void Disconnect()
        {
            isPresent = false;
            timerCheckStatus.Enabled = false;

            await Task.Delay(3000);

            try {
                EV23KBoard.CloseDevice();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Read word from device via SMBus on EV2300.
        /// </summary>
        /// <param name="SMBusCmd">Device command to read.</param>
        /// <param name="SMBusWord">Read back device word.</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError ReadSMBusWord(short SMBusCmd, out short SMBusWord, short targetAddr)
        {
            try
            {
                SMBusWord = 0;
                short nWord = 0;

                if (!isPresent)
                    return EV23KError.NoUSB;

                EV23KError err = (EV23KError)EV23KBoard.ReadSMBusWord(SMBusCmd, ref nWord, targetAddr);

                if (err != EV23KError.NoError)
                    return err;

                SMBusWord = nWord;
                return EV23KError.NoError;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Write command to device via SMBus on EV2300.
        /// </summary>
        /// <param name="SMBusCmd">Device command to write.</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError WriteSMBusCommand(short SMBusCmd, short targetAddr)
        {
            try
            {
                if (!isPresent)
                    return EV23KError.NoUSB;

                return (EV23KError)EV23KBoard.WriteSMBusCmd(SMBusCmd, (short)(targetAddr - 1));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Write word to device via SMBus on EV2300.
        /// </summary>
        /// <param name="SMBusCmd">Device command to write.</param>
        /// <param name="SMBusWord">Data word to write.</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError WriteSMBusWord(short SMBusCmd, short SMBusWord, short targetAddr)
        {
            try
            {
                if (!isPresent)
                    return EV23KError.NoUSB;

                return (EV23KError)EV23KBoard.WriteSMBusWord(SMBusCmd, SMBusWord, (short)(targetAddr-1));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Read data block from device via SMBus on EV2300.
        /// </summary>
        /// <param name="SMBusCmd">Device command to write.</param>
        /// <param name="DataBlock">Variable to store data block in.</param>
        /// <param name="BlockLength">Read length of data block</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError ReadSMBusBlock(short SMBusCmd, out object DataBlock, out short BlockLength, short targetAddr)
        {
            try
            {
                DataBlock = null;
                BlockLength = 0;
                short len = 0;
                object data = null;

                if (!isPresent)
                    return EV23KError.NoUSB;

                EV23KError err = (EV23KError)EV23KBoard.ReadSMBusBlock(SMBusCmd, ref data, ref len, targetAddr);

                if (err != EV23KError.NoError)
                    return err;

                DataBlock = data;
                BlockLength = len;

                return EV23KError.NoError;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Write data block to device via SMBus on EV2300.
        /// </summary>
        /// <param name="SMBusCmd">Device command to write.</param>
        /// <param name="DataBlock">Data block to write.</param>
        /// <param name="BlockLength">Write length of data block</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError WriteSMBusBlock(short SMBusCmd, object DataBlock, short BlockLength, short targetAddr)
        {
            try
            {
                if (!isPresent)
                    return EV23KError.NoUSB;

                return (EV23KError)EV23KBoard.WriteSMBusBlock(SMBusCmd, DataBlock, BlockLength, (short)(targetAddr-1));
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Read manufacturer access block from device via SMBus on EV2300.
        /// </summary>
        /// <param name="MacAddr">Manufacturer Access address/command.</param>
        /// <param name="Cmd">Manufacturer Access command/register to read.</param>
        /// <param name="DataBlock">Variable to store data block in.</param>
        /// <param name="BlockLength">Read length of data block</param>
        /// <param name="targetAddr">Device target address.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError ReadManufacturerAccessBlock(short MacAddr, short Cmd, out object DataBlock, out short BlockLength, short targetAddr)
        {
            try
            {
                DataBlock = null;
                BlockLength = 0;
                short len = 0;
                object data = null;

                if (!isPresent)
                    return EV23KError.NoUSB;

                byte[] cmd = { (byte)(Cmd & 0xFF), (byte)((Cmd & 0xFF00) >> 8) };
                EV23KError err = (EV23KError)EV23KBoard.WriteSMBusBlock(MacAddr, cmd, (short)2, (short)(targetAddr-1));

                if (err != EV23KError.NoError)
                    return err;

                err = (EV23KError)EV23KBoard.ReadSMBusBlock(MacAddr, ref data, ref len, targetAddr);

                if (err != EV23KError.NoError)
                    return err;

                DataBlock = data;
                BlockLength = len;

                return EV23KError.NoError;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Read entire dataflash content into buffer.
        /// </summary>
        /// <param name="MacAddr">Manufacturer access command.</param>
        /// <param name="DataBlock">Flash data block.</param>
        /// <param name="BlockLength">Flash data block length.</param>
        /// <param name="targetAddr">Target device address.</param>
        /// <returns>EV2300 error code.</returns>
        /// <remarks>For dataflash access see SLUUBT5, chapter 18.1.75</remarks>
        public EV23KError ReadDataflash(short MacAddr, out object DataBlock, out int BlockLength, short targetAddr)
        {
            try
            {
                DataBlock = null;
                BlockLength = 0;
                short datalen = 32;
                object data = null;
                IEnumerable<byte> df = null;

                if (!isPresent)
                    return EV23KError.NoUSB;

                byte[] cmd = { 0x00, 0x40 };
                EV23KError err = (EV23KError)EV23KBoard.WriteSMBusBlock(MacAddr, cmd, (short)2, (short)(targetAddr - 1));

                if (err != EV23KError.NoError)
                    return err;

                err = (EV23KError)EV23KBoard.ReadSMBusBlock(MacAddr, ref data, ref datalen, targetAddr);
                if (err == EV23KError.NoError)
                    df = ((byte[])data).Skip(2); // Skip first two address bytes, copy only flash data

                for(int i = 1; i < 103; i++)
                {
                    data = null;
                    datalen = 0;
                    err = (EV23KError)EV23KBoard.ReadSMBusBlock(MacAddr, ref data, ref datalen, targetAddr);
                    if (err == EV23KError.NoError)
                    {
                        df = df.Concat(((byte[])data).Skip(2)); // Skip frst two address bytes, concat only flash data
                    }
                    else
                        return err;
                }

                DataBlock = df.ToArray();
                BlockLength = df.Count();

                return EV23KError.NoError;
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message);
            }
        }

        /// <summary>
        /// Get last stored EV2300 error code.
        /// </summary>
        /// <returns>EV2300 error code.</returns>
        public EV23KError CheckForError()
        {
            if (!isPresent)
                return EV23KError.NoUSB;

            return (EV23KError)EV23KBoard.CheckForError();
        }

        /// <summary>
        /// Set EV2300 GPIO pin high.
        /// </summary>
        /// <param name="gpio">GPIO pin mask to set.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError GpioHigh(EV23KGpioMask gpio)
        {
            if (!isPresent)
                return EV23KError.NoUSB;

            return (EV23KError)EV23KBoard.GPIOWrite((short)gpio, (short)gpio);
        }

        /// <summary>
        /// Set EV2300 GPIO pin low.
        /// </summary>
        /// <param name="gpio">GPIO pin mask to set.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError GpioLow(EV23KGpioMask gpio)
        {
            if (!isPresent)
                return EV23KError.NoUSB;

            return (EV23KError)EV23KBoard.GPIOWrite((short)gpio, 0);
        }

        /// <summary>
        /// Toogle EV2300 GPIO pin.
        /// </summary>
        /// <param name="gpio">GPIO pin mask to toggle.</param>
        /// <returns>EV2300 error code.</returns>
        public EV23KError GpioToggle(EV23KGpioMask gpio)
        {
            if (!isPresent)
                return EV23KError.NoUSB;

            short data = 0;
            EV23KError err = (EV23KError)EV23KBoard.GPIORead((short)gpio, ref data);
            if(err == EV23KError.NoError)
            {
                data = (short)~data;
                err = (EV23KError)EV23KBoard.GPIOWrite((short)gpio, data);
            }

            return err;
        }
    }
}
