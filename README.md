# BQEV23K
A project that controls the Texas Instruments (TI) EV2300 hardware interface through C# WPF.
Especially written for the BQ40Z80 (2-Series to 7-Series battery pack manager with impedance track gas gauge). It runs a learning cycle or GPC cycle in manual or full automatic mode.

## Features
As a side effect it demonstrates:
- Implementation and access of bq80xrw.ocx COM component in C# WPF
- Controlling the EV2300 GPIO pins (relay jig control)
- Read device configuration files in TI .bqz format
- Read and write to BQ40Z80 registers in normal mode and ManufacturerAccessBlock mode
- Reading the entire BQ40Z80 dataflash
- Running the BQ40Z80 learning and GPC cycle
- Implementation and usage of Oxyplot WPF with real time update

## Recommended documentation

[BQ40Z80 Technical documents](http://www.ti.com/product/BQ40Z80/technicaldocuments)
- SLUA848 How to Complete a Successful Learning Cycle for the bq40z80
- SLUA868 bq40z80 Manufacture, Production, and Calibration
- SLUUBT4A bq40z80EVM Li-Ion Battery Pack Manager Evaluation Module
- SLUUBT5 bq40z80 Technical Reference Manual
- SLVA725 Simple Guide to Chemical ID Selection Tool (GPC)

## Requirements

- Project written for and tested with custom design BQ40Z80 battery pack management board.
- EV2300 hardware interface (EV2400 not tested, but expected to work).
- External relay jig for fully automated cycle (charger and load control).

## Build instructions
Built and tested on Windows7 64bit with Visual Studio 2015.

### Dependencies

- Installed EV2300 customer kit (see below).
- NuGet Oxplot.Core and Oxplot.Wpf packages.

### Setup of new Visual Studio project with bq80xrw implementation
1. Download [EV2300 customer kit](https://www.mictronics.de/aO8F2bW9/EV2300_Customer_Kit.zip)
2. Run and install *EV2300DevKitSetup.exe*
3. Create new C# WPF project in Visual Studio
   - BQEK23K_Demo
   - Add a new a Visual Studio C# Windows Form Control Library project (located in *Visual C#* => *Classic Desktop*
     - AxBQEV23K
	 - Rename automatically generated UserControl1 into AxBQEV23KControl. Choose *Yes* in popup dialog.
   - Add Bq80xRW COM control in AxBQEV23KControl
     - In Toolbox, right click => *Choose Item* => Select *COM Components* tab => Select checkbox *Bq80xRW Control*
     - Search Bq80xRW Control in Toolbox (General) and drag it into AxBQEV23KControl, AxBQ80XRWLib and BQ80XRWLib will be added as reference automatically.
     - Build AxBQEV23K
   - Add AxInterop.BQ80XRWLib as reference in BQEV23K
     - In solution explorer, selected *References*, right click and *Add Reference*
	 - Browse to AxBQEV23K project build folder (bin\Debug or bin\Release)
	 - Select *AxInterop.BQ80XRWLib.dll* and add
   - Add System.Windows.Forms as reference in BQEV23K
   - Add WindowsFormsIntegration as reference in BQEV23K
4. Add new AxBQ80XRWLib.AxBq80xRW class to your project code.

MainWindow.xaml.cs
```
using System.Windows;

namespace BQEV23K_Demo
{
    public partial class MainWindow : Window
    {
        AxBQ80XRWLib.AxBq80xRW EV23K;
        public MainWindow()
        {
            InitializeComponent();
            AxBQ80XRWLib.AxBq80xRW EV23K;
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            EV23K = new AxBQ80XRWLib.AxBq80xRW();
            host.Child = EV23K;
            MainGrid1.Children.Add(host);
        }
    }
}
```
MainWindow.xaml
```
<Window x:Class="BQEV23K_Demo.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:local="clr-namespace:BQEV23K_Demo"
        mc:Ignorable="d"
        Title="MainWindow" Height="350" Width="525">
    <Grid Name="MainGrid1">
        
    </Grid>
</Window>
```
5. Build project.

[Original source](http://e2e.ti.com/support/power_management/battery_management/f/180/p/640114/2363362#2363362)

## License

Copyright &copy; 2018 Michael Wolf, mictronics.de
Distributed under the modified [MIT license](LICENSE).

#### Disclaimer
I'm not an TI employee or part of their product support. This is a pure hobbyist project.
