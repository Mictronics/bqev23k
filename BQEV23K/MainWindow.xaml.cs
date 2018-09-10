using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

/**
 * How to setup such project see:
 * http://e2e.ti.com/support/power_management/battery_management/f/180/p/640114/2363362#2363362
 *   
 * Download EV2300 customer kit:
 * https://e2e.ti.com/support/power_management/battery_management/f/180/p/671348/2470529#2470529
 * 
 */

namespace BQEV23K
{
    /// <summary>
    /// Main window class
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private const int CmdExecDelayMilliseconds = 1000;
        private const int ResetCmdExecDelayMilliseconds = 4000;
        private PlotViewModel plot;
        private EV23K board;
        private GaugeInfo gauge;
        private DispatcherTimer timerUpdateGUI;
        private DispatcherTimer timerUpdatePlot;
        private Cycle cycle;
        private CycleType selectedCycleType = CycleType.None;
        private CycleModeType selectedCycleModeType = CycleModeType.None;
        private GpcDataLog gpcLog;

        /// <summary>
        /// Constructor.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            plot = new PlotViewModel();
            DataContext = plot;

            Title = "BQEV2300 - v1.0 by Mictronics";
            System.Windows.Forms.Integration.WindowsFormsHost host;
            board = new EV23K(out host);
            host.Width = host.Height = 0;
            host.IsEnabled = false;
            MainGrid.Children.Add(host);
            timerUpdateGUI = new DispatcherTimer();
            timerUpdateGUI.Tick += new EventHandler(UpdateGui);
            timerUpdateGUI.Interval = new TimeSpan(0, 0, 0, 0, 500);
            timerUpdateGUI.Start();

            timerUpdatePlot = new DispatcherTimer();
            timerUpdatePlot.Tick += new EventHandler(UpdatePlot);
            timerUpdatePlot.Interval = new TimeSpan(0, 0, 0, 5, 0);
        }

        /// <summary>
        /// Initialize when main window was loaded.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CfgCycleCellCount.Text = Properties.Settings.Default.CellCount;
            CfgCycleTaperCurr.Text = Properties.Settings.Default.TaperCurrent;
            CfgCycleTermVolt.Text = Properties.Settings.Default.TerminationVoltage;
            CfgCycleChargeRelaxHours.Text = Properties.Settings.Default.ChargeRelaxHours;
            CfgCycleDischargeRelaxHours.Text = Properties.Settings.Default.DischargeRelaxHours;

            try
            {
                int BrdsNumber = 0;
                string BrdsName = "";

                if (board.Connect(out BrdsNumber, out BrdsName) != 0)
                    throw new ArgumentException("EV2300 not found.");

                LogView.AddEntry("EV2300 connected...");
                Console.WriteLine(BrdsName);

                EV23KError err = board.CheckForError();
                gauge = new GaugeInfo(board);

                UpdateGui(null, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Clean up when main window will be closed.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(cycle != null)
                cycle.CancelCycle();

            if(gauge != null)
            {
                gauge.StopPolling();
                gauge.ReadDeviceMutex.WaitOne();
            }

            board.Disconnect();
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
            if (gauge != null)
            {
                gauge.ReadDeviceMutex.WaitOne();
                gauge.Dispose();
            }
            cycle.Dispose();
            board.Dispose();
        }

        /// <summary>
        /// Priodically update GUI elements.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used</param>
        public void UpdateGui(object sender, System.EventArgs e)
        {
            if (board != null)
            {
                if (board.IsPresent)
                {
                    IconEV23K.Source = GetImageSourceFromResource("usb-icon_128.png");
                    LabelEV23KName.Content = board.Name;
                }
                else
                {
                    IconEV23K.Source = GetImageSourceFromResource("USB-Disabled-128.png");
                    LabelEV23KName.Content = string.Empty;
                }
            }

            if (gauge != null)
            {
                if(gauge.HasSMBusError)
                {
                    IconArrows.Source = GetImageSourceFromResource("Arrows-Disabled-48.png");
                    IconGauge.Source = GetImageSourceFromResource("Gauge-Disabled-128.png");
                    LabelGaugeName.Content = LabelGaugeVersion.Text = string.Empty;
                    LabelGaugeVoltage.Content = LabelGaugeTemperature.Content = string.Empty;
                }
                else
                {
                    IconArrows.Source = GetImageSourceFromResource("Arrows-48.png");
                    IconGauge.Source = GetImageSourceFromResource("Gauge-128.png");
                    LabelGaugeName.Content = gauge.GetDisplayValue("Device Name");

                    string s = gauge.GetDisplayValue("DEVICE_NUMBER") + "_";
                    s += gauge.GetDisplayValue("HW_VERSION").Replace("0", "") + "_";
                    s += gauge.GetDisplayValue("FW_VERSION").Replace("0", "") + "_";
                    s += gauge.GetDisplayValue("FW_BUILD").Replace("0", "");
                    LabelGaugeVersion.Text = s;

                    LabelGaugeVoltage.Content = gauge.GetDisplayValue("Voltage");
                    LabelGaugeTemperature.Content = gauge.GetDisplayValue("Temperature");

                    FlagFC.IsChecked = gauge.FlagFC;
                    FlagGAUGE_EN.IsChecked = gauge.FlagGAUGE_EN;
                    FlagQEN.IsChecked = gauge.FlagQEN;
                    FlagQMAX.IsChecked = gauge.FlagQMAX;
                    FlagRDIS.IsChecked = gauge.FlagRDIS;
                    FlagREST.IsChecked = gauge.FlagREST;
                    FlagVOK.IsChecked = gauge.FlagVOK;
                    FlagCHG.IsChecked = gauge.FlagCHG;
                    FlagDSG.IsChecked = gauge.FlagDSG;
                    FlagOCVFR.IsChecked = gauge.FlagOCV;

                    CfgBattChemID.Text = gauge.GetDisplayValue("CHEM_ID");
                    CfgBattDesignVoltage.Text = gauge.DFDesignVoltage;
                    CfgBattDesignCapacity.Text = gauge.DFDesignCapacity;
                    CfgBattCellCount.Text = gauge.DFCellCount;
                    CfgBattTermVolt.Text = gauge.DFTermVoltage;
                    CfgBattTaperCurr.Text = gauge.DFTaperCurrent;
                    CfgBattChgCurrThres.Text = gauge.DFChgCurrentThreshold;
                    CfgBattDsgCurrThres.Text = gauge.DFDsgCurrentThreshold;
                }
            }

            if (cycle != null && cycle.CycleInProgress)
            {
                LearningVoltageLabel.Content = "Voltage: " + gauge.GetDisplayValue("Voltage");
                LearningCurrentLabel.Content = "Current: " + gauge.GetDisplayValue("Current");
                LearningTemperatureLabel.Content = "Temperature: " + gauge.GetDisplayValue("Temperature");
                if (cycle.RunningTaskName == "RelaxTask")
                {
                    LearningTimeLabel.Content = "Waiting Time: " + cycle.ElapsedTime.ToString(@"hh\:mm\:ss");
                }
                else
                {
                    LearningTimeLabel.Content = "Elapsed Time: " + cycle.ElapsedTime.ToString(@"hh\:mm\:ss");
                }
            }
        }

        /// <summary>
        /// Update voltage, current and temperature plot.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        public void UpdatePlot(object sender, System.EventArgs e)
        {
            lock (plot.Plot1.SyncRoot)
            {
                plot.Voltage = gauge.Voltage;
                plot.Current = gauge.Current;
                plot.Temperature = gauge.Temperature;
            }
            plot.Plot1.InvalidatePlot(true); // Refresh plot view

            if(selectedCycleType == CycleType.GpcCycle && gpcLog != null)
            {
                gpcLog.WriteLine(gauge.Voltage, gauge.Current, gauge.Temperature);
            }
        }

        /// <summary>
        /// Get image from assembly ressource
        /// </summary>
        /// <param name="resourceName">Image resource name to get.</param>
        /// <returns>ImageSource from resource name.</returns>
        static internal ImageSource GetImageSourceFromResource(string resourceName)
        {
            Uri oUri = new Uri("pack://application:,,,/" + "BQEV23K" + ";component/Resources/" + resourceName, UriKind.RelativeOrAbsolute);
            return BitmapFrame.Create(oUri);
        }

        /// <summary>
        /// Start new learning or GPC cycle on button click.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private async void ButtonCycleStart_Click(object sender, RoutedEventArgs e)
        {
            int cellCount = 0;
            int.TryParse(CfgCycleCellCount.Text, out cellCount);

            if (cellCount <= 0 || cellCount > 7)
            {
                LogView.AddEntry("Invalid cell count!");
                return;
            }

            int termVoltage = 0;
            int.TryParse(CfgCycleTermVolt.Text, out termVoltage);

            int ctv = (termVoltage / cellCount);
            if (termVoltage <= 0 || (ctv < 2500) || (termVoltage / cellCount > 4200))
            {
                LogView.AddEntry("Invalid termination voltage! (" + ctv.ToString() + "mV/cell)");
                return;
            }

            int taperCurrent = 0;
            if (!int.TryParse(CfgCycleTaperCurr.Text, out taperCurrent))
            {
                taperCurrent = 100; // mA
                CfgCycleTaperCurr.Text = taperCurrent.ToString("D");
            }

            if (taperCurrent <= 0)
            {
                LogView.AddEntry("Invalid taper current!");
                return;
            }

            /* See TI SLUA848, chapter 4.2.1 */
            LogView.AddEntry("Preparing...");
            /* Clear FET_EN to be able to set charge and discharge FETs */
            if (gauge.FlagFET_EN == true)
                gauge.CommandToogleFETenable();

            await Task.Delay(CmdExecDelayMilliseconds);

            if (gauge.FlagFET_EN == true)
            {
                LogView.AddEntry("Failed to clear FET_EN.");
                return;
            }

            if (selectedCycleType == CycleType.LearningCycle) { 
                /* Enable gauging mode */
                if (gauge.FlagGAUGE_EN == false || gauge.FlagQEN == false)
                    gauge.CommandSetGaugeEnable();

                await Task.Delay(CmdExecDelayMilliseconds);

                if (gauge.FlagGAUGE_EN == false || gauge.FlagQEN == false)
                {
                    LogView.AddEntry("Failed to enable gauging mode.");
                    return;
                }

                /* Reset to disable resistance update */
                gauge.CommandReset();
                await Task.Delay(ResetCmdExecDelayMilliseconds);

                if (gauge.FlagRDIS == false)
                {
                    LogView.AddEntry("Error: RDIS not set after reset.");
                    return;
                }

                if (gauge.GetReadValue("LStatus") != 0x04)
                {
                    LogView.AddEntry("Error: LStatus != 0x04.");
                    return;
                }
            }
            else if (selectedCycleType == CycleType.GpcCycle)
            {
                /* Disable gauging mode */
                if (gauge.FlagGAUGE_EN == true || gauge.FlagQEN == true)
                    gauge.CommandSetGaugeEnable();

                await Task.Delay(CmdExecDelayMilliseconds);

                if (gauge.FlagGAUGE_EN == true || gauge.FlagQEN == true)
                {
                    LogView.AddEntry("Failed to disable gauging mode.");
                    return;
                }
            }

            if (gauge.FlagDSG == false)
                gauge.CommandToggleDischargeFET();

            /* Set charge and discharge FETs */
            if (gauge.FlagCHG == false)
                gauge.CommandToggleChargeFET();

            await Task.Delay(CmdExecDelayMilliseconds);

            if (gauge.FlagCHG == false && gauge.FlagDSG == false)
            {
                LogView.AddEntry("Failed to set charge and discharge FETs.");
                return;
            }

            LogView.AddEntry("Device preparation successful.");

            int relaxTimeCharge = 0;
            int relaxTimeDischarge = 0;

            if (int.TryParse(CfgCycleChargeRelaxHours.Text, out relaxTimeCharge))
                relaxTimeCharge *= 60; // Convert to minute
            else
                relaxTimeCharge = 120;

            if (int.TryParse(CfgCycleDischargeRelaxHours.Text, out relaxTimeDischarge))
                relaxTimeDischarge *= 60; // Conver to minutes
            else
                relaxTimeDischarge = 300;

            Properties.Settings.Default.CellCount = CfgCycleCellCount.Text;
            Properties.Settings.Default.TaperCurrent = CfgCycleTaperCurr.Text;
            Properties.Settings.Default.TerminationVoltage = CfgCycleTermVolt.Text;
            Properties.Settings.Default.ChargeRelaxHours = CfgCycleChargeRelaxHours.Text;
            Properties.Settings.Default.DischargeRelaxHours = CfgCycleDischargeRelaxHours.Text;
            Properties.Settings.Default.Save();

            List<GenericTask> tl = new List<GenericTask>();

            if (selectedCycleType == CycleType.LearningCycle)
            {
                tl = new List<GenericTask> {
                new DischargeTask(termVoltage),
                new RelaxTask(relaxTimeDischarge),
                new ChargeTask(taperCurrent),
                new RelaxTask(relaxTimeCharge),
                new DischargeTask(termVoltage),
                new RelaxTask(relaxTimeDischarge) };

                if (true)
                { // Perform field update cycle to reach LStatus 0x0E
                    tl.AddRange(new GenericTask[]{
                    new ChargeTask(taperCurrent),
                    new RelaxTask(relaxTimeCharge),
                    new DischargeTask(termVoltage),
                    new RelaxTask(relaxTimeDischarge)});
                }
            }
            else if (selectedCycleType == CycleType.GpcCycle)
            {
                tl = new List<GenericTask> {
                new ChargeTask(taperCurrent),
                new RelaxTask(relaxTimeCharge),
                new DischargeTask(termVoltage),
                new RelaxTask(relaxTimeDischarge) };

                gpcLog = new GpcDataLog(cellCount);
            }

            cycle = new Cycle(tl, gauge);
            cycle.CycleModeType = selectedCycleModeType;
            cycle.LogWriteEvent += LogView.AddEntry;
            cycle.CycleCompleted += OnCycleCompleted;
            cycle.StartCycle();

            timerUpdatePlot.Start();

            ButtonCycleStart.IsEnabled = false;
            ButtonCycleCancel.IsEnabled = true;
        }

        /// <summary>
        /// Called on entire cycle completed event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnCycleCompleted(object sender, EventArgs e)
        {
            // Add trophy here.
            timerUpdatePlot.Stop();
            ButtonCycleStart.IsEnabled = true;
            ButtonCycleCancel.IsEnabled = false;
        }

        /// <summary>
        /// Cancel running learning or GPC cycle on button click.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonCycleCancel_Click(object sender, RoutedEventArgs e)
        {
            timerUpdatePlot.Stop();
            cycle.CancelCycle();
            ButtonCycleStart.IsEnabled = true;
            ButtonCycleCancel.IsEnabled = false;
        }

        /// <summary>
        /// Set selected cyle mode on button click.
        /// </summary>
        /// <param name="sender">Get manual or automatic mode from button name.</param>
        /// <param name="e">Not used.</param>
        private void ModeSelectButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button b = (System.Windows.Controls.Button)sender;
            if (b.Name == "ManualModeButton")
            {
                selectedCycleModeType = CycleModeType.Manual;
                ConfigHelpImage.Source = GetImageSourceFromResource("ManualMode.png");
            }
            else if (b.Name == "AutomaticModeButton")
            {
                selectedCycleModeType = CycleModeType.Automatic;
                ConfigHelpImage.Source = GetImageSourceFromResource("AutoMode.png");
            }
            else
            {
                selectedCycleModeType = CycleModeType.None;
                return;
            }

            switch (selectedCycleType)
            {
                case CycleType.LearningCycle:
                    TabItemCycle.Header = "Learning Cycle";
                    break;
                case CycleType.GpcCycle:
                    TabItemCycle.Header = "GPC Cycle";
                    break;
                default:
                    return;
            }

            TabItemCycle.IsEnabled = true;
            TabItemConfiguration.IsEnabled = true;
            System.Windows.Controls.TabItem t = (System.Windows.Controls.TabItem)tabControl.Items[1];
            t.IsSelected = true;

        }

        /// <summary>
        /// Set cycle type on button click.
        /// </summary>
        /// <param name="sender">Get learning or GPC cylce type from button name.</param>
        /// <param name="e">Not used.</param>
        private void CycleTypeSelected(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.RadioButton rb = (System.Windows.Controls.RadioButton)sender;
            if(rb.Name == "ModeLearningCycle")
            {
                selectedCycleType = CycleType.LearningCycle;
            }
            else if(rb.Name == "ModeGpcCycle")
            {
                selectedCycleType = CycleType.GpcCycle;
            }
            else
            {
                selectedCycleType = CycleType.None;
            }
        }

        /// <summary>
        /// Reset zoom on all plot axes.
        /// </summary>
        /// <param name="sender">Not used.</param>
        /// <param name="e">Not used.</param>
        private void ButtonResetZoom_Click(object sender, RoutedEventArgs e)
        {
            PlotView.ResetAllAxes();
        }
    }
}
