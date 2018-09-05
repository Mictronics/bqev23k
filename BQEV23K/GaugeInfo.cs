using System;
using System.Threading;
using System.Threading.Tasks;

namespace BQEV23K
{
    /// <summary>
    /// Gauge/device handling class. Reads device configuration, register data and issues commands.
    /// </summary>
    public class GaugeInfo : IDisposable
    {
        private const int GaugeDataPollingInterval = 500;
        private string[] cyclicReadGaugeDataRegisters = new string[]{
            "Voltage",
            "Temperature",
            "Current",
            "LStatus",
            "IT Status",
            "Manufacturer Name",
            "Battery Status",
            "Manufacturing Status",
            "Operation Status A"
        };

        private EV23K EV23KBoard;
        private SbsItems sbsItems;
        private BcfgItems bcfgItems;
        private CancellationTokenSource cancelSource;
        private CancellationToken cancelToken;
        private int voltage = 0;
        private int current = 0;
        private double temperature = 0.0;
        private Timer pollTimer;
        private bool hasEV23KError = false;
        private bool hasSMBusError = false;
        private bool isReadingGauge = false;
        private bool isDataflashAvail = false;
        private Mutex readDeviceMutex = new Mutex();

        #region Properties
        /// <summary>
        /// Get battery voltage.
        /// </summary>
        public int Voltage
        {
            get
            {
                return voltage;
            }
        }

        /// <summary>
        /// Get battery current.
        /// </summary>
        public int Current
        {
            get
            {
                return current;
            }
        }

        /// <summary>
        /// Get battery temperature.
        /// </summary>
        public double Temperature
        {
            get
            {
                return temperature;
            }
        }

        /// <summary>
        /// True on EV2300 returned error code.
        /// </summary>
        public bool HasEV23KError
        {
            get
            {
                return hasEV23KError;
            }
        }

        /// <summary>
        /// True on SMBus communication error.
        /// </summary>
        public bool HasSMBusError
        {
            get
            {
                return hasSMBusError;
            }
        }
        /// <summary>
        /// True when async thread is reading the device/gauge register data.
        /// </summary>
        public bool IsReadingGauge
        {
            get
            {
                return isReadingGauge;
            }
        }

        /// <summary>
        /// Get status of VOK flag.
        /// </summary>
        public bool FlagVOK
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "IT Status").SbsBitItems.Find(x => x.SbsCaption == "VOK").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of REST flag.
        /// </summary>
        public bool FlagREST
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "IT Status").SbsBitItems.Find(x => x.SbsCaption == "REST").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of RDIS flag.
        /// </summary>
        public bool FlagRDIS
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "IT Status").SbsBitItems.Find(x => x.SbsCaption == "RDIS").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of QMAX flag.
        /// </summary>
        public bool FlagQMAX
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "IT Status").SbsBitItems.Find(x => x.SbsCaption == "QMAX").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of QEN flag.
        /// </summary>
        public bool FlagQEN
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "IT Status").SbsBitItems.Find(x => x.SbsCaption == "QEN").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of FC flag.
        /// </summary>
        public bool FlagFC
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "Battery Status").SbsBitItems.Find(x => x.SbsCaption == "FC").SbsBitValue != 0;
            }
        }
        
        /// <summary>
        /// Get status of GAUGE_EN flag.
        /// </summary>
        public bool FlagGAUGE_EN
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "Manufacturing Status").SbsBitItems.Find(x => x.SbsCaption == "GAUGE_EN").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of CHG flag.
        /// </summary>
        public bool FlagCHG
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "Operation Status A").SbsBitItems.Find(x => x.SbsCaption == "CHG").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of DSG flag.
        /// </summary>
        public bool FlagDSG
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "Operation Status A").SbsBitItems.Find(x => x.SbsCaption == "DSG").SbsBitValue != 0;
            }
        }

        /// <summary>
        /// Get status of FET_EN flag.
        /// </summary>
        public bool FlagFET_EN
        {
            get
            {
                return sbsItems.SbsRegister.Find(x => x.Caption == "Manufacturing Status").SbsBitItems.Find(x => x.SbsCaption == "FET_EN").SbsBitValue != 0;
            }
        }

        public string DFCellCount
        {
            get
            {
                return bcfgItems.DataflashItems.Find(x => x.Caption == "Cell Configuration").RawValue.ToString();
            }
        }

        public string DFTermVoltage
        {
            get
            {
                return bcfgItems.DataflashItems.Find(x => x.Caption == "Term Voltage").RawValue.ToString();
            }
        }

        public string DFTaperCurrent
        {
            get
            {
                return bcfgItems.DataflashItems.Find(x => x.Caption == "Charge Term Taper Current").RawValue.ToString();
            }
        }

        public string DFDsgCurrentThreshold
        {
            get
            {
                return bcfgItems.DataflashItems.Find(x => x.Caption == "Dsg Current Threshold").RawValue.ToString();
            }
        }

        public string DFChgCurrentThreshold
        {
            get
            {
                return bcfgItems.DataflashItems.Find(x => x.Caption == "Chg Current Threshold").RawValue.ToString();
            }
        }

        public Mutex ReadDeviceMutex
        {
            get
            {
                return readDeviceMutex;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="ev">Reference to EV2300 board</param>
        public GaugeInfo(EV23K ev)
        {
            EV23KBoard = ev;
            sbsItems = new SbsItems(@"4800_0_02-bq40z80.bqz");
            bcfgItems = new BcfgItems(@"4800_0_02_03-bq40z80.bcfgx");
            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;

            WriteDevice("DEVICE_NUMBER");
            WriteDevice("HW_VERSION");
            WriteDevice("FW_VERSION");
            WriteDevice("FW_BUILD");
            WriteDevice("CHEM_ID");
            ReadDevice("Device Name");

            StartPolling();
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
            cancelSource.Dispose();
            pollTimer.Dispose();
            readDeviceMutex.Dispose();
            EV23KBoard.Dispose();
        }

        /// <summary>
        /// Start polling register data from device.
        /// </summary>
        public void StartPolling()
        {
            cancelSource = new CancellationTokenSource();
            cancelToken = cancelSource.Token;
            pollTimer = new Timer(ReadGaugeData, null, 0, GaugeDataPollingInterval);
        }

        /// <summary>
        /// Stop polling register data from device.
        /// </summary>
        public void StopPolling()
        {
            pollTimer.Dispose();
            if(!cancelSource.IsCancellationRequested)
                cancelSource.Cancel();
        }

        /// <summary>
        /// Read data from device registers listed in cyclicReadGaugeDataRegisters.
        /// </summary>
        /// <param name="state">Not used.</param>
        private async void ReadGaugeData(object state)
        {
            if(!EV23KBoard.IsPresent)
            { 
                hasEV23KError = true;
                return;
            }

            if (isReadingGauge) return;

            isReadingGauge = true;

            isReadingGauge = await Task.Run(() =>
            {
                if (cancelToken.IsCancellationRequested) return false;

                if (!EV23KBoard.IsPresent) return false;

                hasSMBusError = false;

                readDeviceMutex.WaitOne();

                foreach(string cmd in cyclicReadGaugeDataRegisters)
                {
                    if (ReadDevice(cmd) != EV23KError.NoError)
                        hasSMBusError = true;
                }
                
                voltage = (int)GetReadValue("Voltage");
                temperature = GetReadValue("Temperature");
                current = (int)GetReadValue("Current");

                ReadDataflash();

                readDeviceMutex.ReleaseMutex();
                return false;
            }, cancelToken);
        }

        /// <summary>
        /// Read specific device register using methode defined in configuration.
        /// </summary>
        /// <param name="caption">Register name to read.</param>
        /// <returns>EV2300 error code.</returns>
        private EV23KError ReadDevice(string caption)
        {
            EV23KError err = EV23KError.Unknown;
            short dataWord = 0;
            object dataBlock = null;
            short dataLength = 0;

            SbsRegisterItem i = sbsItems.SbsRegister.Find(x => x.Caption == caption);
            if(i != null)
            {
                if (i.ReadStyle == 1)
                {
                    err = (EV23KError)EV23KBoard.ReadSMBusWord(i.Command, out dataWord, sbsItems.TargetAdress);
                    if(err == EV23KError.NoError)
                    {
                        i.RawValue = dataWord;
                        i.CalculateBitFields();
                        i.CalculateDisplayValue();
                    }
                }
                else if (i.ReadStyle == 2)
                {
                    if (!i.IsMac)
                    {
                        err = (EV23KError)EV23KBoard.ReadSMBusBlock(i.Command, out dataBlock, out dataLength, sbsItems.TargetAdress);
                        if (err == EV23KError.NoError)
                        {
                            i.GetRawValueFromDataBlock(dataBlock, dataLength, i.OffsetWithinBlock, i.LengthWithinBlock);
                            i.CalculateBitFields();
                            i.CalculateDisplayValue();
                        }
                    }
                    else
                    {
                        err = (EV23KError)EV23KBoard.ReadManufacturerAccessBlock(sbsItems.TargetMacAdress, i.Command, out dataBlock, out dataLength, sbsItems.TargetAdress);
                        if (err == EV23KError.NoError)
                        {
                            i.GetRawValueFromDataBlock(dataBlock, dataLength, i.OffsetWithinBlock, i.LengthWithinBlock);
                            i.CalculateBitFields();
                            i.CalculateDisplayValue();
                        }
                    }
                }
                else if (i.ReadStyle == 3)
                {
                    err = (EV23KError)EV23KBoard.ReadManufacturerAccessBlock(sbsItems.TargetMacAdress, i.Command, out dataBlock, out dataLength, sbsItems.TargetAdress);
                    if (err == EV23KError.NoError)
                    {
                        i.GetRawValueFromDataBlock(dataBlock, dataLength, i.OffsetWithinBlock, i.LengthWithinBlock);
                        i.CalculateBitFields();
                        i.CalculateDisplayValue();
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Write specific device register using methode defined in configuration.
        /// </summary>
        /// <param name="caption">Register name to write.</param>
        /// <returns>EV2300 error code.</returns>
        /// <remarks>For now only works to set the ManufacturerAccess command.</remarks>
        private EV23KError WriteDevice(string caption)
        {
            EV23KError err = EV23KError.Unknown;
            object dataBlock = null;
            short dataLength = 0;

            SbsCommandItem c = sbsItems.SbsCommands.Find(x => x.Caption == caption);
            if( c != null)
            {
                if(c.WriteStyle == 1)
                {
                    err = EV23KBoard.WriteSMBusWord(0, c.Command, sbsItems.TargetAdress);
                }
                else if(c.WriteStyle == 2)
                {
                    err = EV23KBoard.ReadManufacturerAccessBlock(sbsItems.TargetMacAdress, c.Command, out dataBlock, out dataLength, sbsItems.TargetAdress);
                    if (c.HasResult && err == EV23KError.NoError)
                    {
                        c.GetRawValueFromDataBlock(dataBlock, dataLength, c.OffsetWithinBlock, c.LengthWithinBlock);
                        c.CalculateDisplayValue();
                    }
                }
            }
            return err;
        }

        /// <summary>
        /// Read entire data flash from device if not already present.
        /// </summary>
        private  void ReadDataflash()
        {
            if (isDataflashAvail)
                return;

            EV23KError err = EV23KError.Unknown;
            object dataBlock = null;
            int dataLength = 0;

            if(EV23KBoard.IsPresent)
            {
                err = EV23KBoard.ReadDataflash(sbsItems.TargetMacAdress, out dataBlock, out dataLength, sbsItems.TargetAdress);
                if (err == EV23KError.NoError)
                {
                    bcfgItems.CreateDataflashModel(dataBlock, dataLength);
                    isDataflashAvail = true;
                }
            }
            return;
        }

        /// <summary>
        /// Get formatted string of register value, scaled and including the unit.
        /// </summary>
        /// <param name="caption">Register name to read from.</param>
        /// <returns>Formatted value string.</returns>
        public string GetDisplayValue(string caption)
        {
            SbsRegisterItem i = sbsItems.SbsRegister.Find(x => x.Caption == caption);
            if (i == null)
            {
                SbsCommandItem c = sbsItems.SbsCommands.Find(x => x.Caption == caption);
                if (c == null)
                    return "";
                else
                    return c.DisplayValue;
            }
            else
            {
                return i.DisplayValue;
            }
        }

        /// <summary>
        /// Get scaled register value.
        /// </summary>
        /// <param name="caption">Register name to read from.</param>
        /// <returns>Scaled value.</returns>
        public double GetReadValue(string caption)
        {
            SbsRegisterItem i = sbsItems.SbsRegister.Find(x => x.Caption == caption);
            if(i == null)
            {
                SbsCommandItem c = sbsItems.SbsCommands.Find(x => x.Caption == caption);
                if (c == null)
                    return 0;
                else
                    return c.ReadValue;
            }
            else
            {
                return i.ReadValue;
            }
        }

        /// <summary>
        /// Send toggle charge FET command to device.
        /// </summary>
        public void CommandToggleChargeFET()
        {
            WriteDevice("CHG_FET_TOGGLE");
        }

        /// <summary>
        /// Send toggle discharge FET command to device.
        /// </summary>
        public void CommandToggleDischargeFET()
        {
            WriteDevice("DSG_FET_TOGGLE");
        }

        /// <summary>
        /// Send FET enable command to device.
        /// </summary>
        public void CommandToogleFETenable()
        {
            WriteDevice("FET_EN");
        }

        /// <summary>
        /// Send gauge enable command to device.
        /// </summary>
        public void CommandSetGaugeEnable()
        {
            WriteDevice("GAUGE_EN");
        }

        /// <summary>
        /// Commands a gauge reset with defined waiting time.
        /// </summary>
        public void CommandReset()
        {
            WriteDevice("RESET");
        }
        
        /// <summary>
        /// Changes status of the charger relay connected to EV2300 on pin VOUT.
        /// </summary>
        /// <param name="state">Logical ouput state.</param>
        public void ToggleChargerRelay(bool state)
        {
            if(state)
                EV23KBoard.GpioHigh(EV23KGpioMask.VOUT);
            else
                EV23KBoard.GpioLow(EV23KGpioMask.VOUT);
        }

        /// <summary>
        /// Changes status of the load relay connected to EV2300 on pin HDQ.
        /// </summary>
        /// <param name="state">Logical output state.</param>
        public void ToggleLoadRelay(bool state)
        {
            if (state)
                EV23KBoard.GpioHigh(EV23KGpioMask.HDQ);
            else
                EV23KBoard.GpioLow(EV23KGpioMask.HDQ);
        }

        /// <summary>
        /// Pushes a remote button via relay starting the load bench.
        /// </summary>
        public async void RemoteLoadStartButton()
        {
            await Task.Delay(3000);
            EV23KBoard.GpioHigh(EV23KGpioMask.I2CSDA);
            await Task.Delay(100);
            EV23KBoard.GpioLow(EV23KGpioMask.I2CSDA);
        }
    }
}
