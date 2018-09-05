using System;
using OxyPlot;
using OxyPlot.Series;
using System.ComponentModel;
using OxyPlot.Axes;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace BQEV23K
{
    /// <summary>
    /// This class is the view model for Oxyplot.
    /// </summary>
    public class PlotViewModel : INotifyPropertyChanged
    {
        private IList<DataPoint> voltage;
        private IList<DataPoint> current;
        private IList<DataPoint> temperature;
        public event PropertyChangedEventHandler PropertyChanged;

        #region Properties
        /// <summary>
        /// Plot model that controls the main plot view.
        /// </summary>
        public PlotModel Plot1 { get; private set; }

        /// <summary>
        /// Add new voltage data point.
        /// </summary>
        public double Voltage
        {
            set
            {
                voltage.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), value));
                RaisePropertyChanged("Voltage");
            }
        }

        /// <summary>
        /// Add new current data point.
        /// </summary>
        public double Current
        {
            set
            {
                current.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), value));
                RaisePropertyChanged("Current");
            }
        }

        /// <summary>
        /// Add new temperature data point.
        /// </summary>
        public double Temperature
        {
            set
            {
                temperature.Add(new DataPoint(DateTimeAxis.ToDouble(DateTime.Now), value));
                RaisePropertyChanged("Temperature");
            }
        }
        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        public PlotViewModel()
        {
            voltage = new List<DataPoint>();
            current = new List<DataPoint>();
            temperature = new List<DataPoint>();

            var model = new PlotModel();

            var xAxis = new DateTimeAxis
            {
                Position = AxisPosition.Bottom,
                StringFormat = "HH:mm:ss",
                Key = "TimeAxis"
            };
            model.Axes.Add(xAxis);

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "VoltageAxis",
                AxisDistance = 70,
                AxisTitleDistance = -60,
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColors.Blue,
                TicklineColor = OxyColors.Blue,
                TitleColor = OxyColors.Blue,
                Title = "Voltage",
                Unit = "mV"
            };
            model.Axes.Add(yAxis);

            yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "CurrentAxis",
                AxisDistance = 0,
                AxisTitleDistance = -5,
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColors.Red,
                TicklineColor = OxyColors.Red,
                TitleColor = OxyColors.Red,
                Title = "Current",
                Unit = "mA"
            };
            model.Axes.Add(yAxis);

            yAxis = new LinearAxis
            {
                Position = AxisPosition.Right,
                Key = "TempAxis",
                AxisDistance = 0,
                AxisTitleDistance = 0,
                AxislineStyle = LineStyle.Solid,
                TextColor = OxyColors.Orange,
                TicklineColor = OxyColors.Orange,
                TitleColor = OxyColors.Orange,
                Title = "Temperature",
                Unit = "°C"
            };
            model.Axes.Add(yAxis);

            var series = new LineSeries
            {
                ItemsSource = voltage,
                YAxisKey = "VoltageAxis",
                XAxisKey = "TimeAxis",
                Color = OxyColors.Blue,
            };
            model.Series.Add(series);
            series = new LineSeries
            {
                ItemsSource = current,
                YAxisKey = "CurrentAxis",
                XAxisKey = "TimeAxis",
                Color = OxyColors.Red,
            };
            model.Series.Add(series);

            series = new LineSeries
            {
                ItemsSource = temperature,
                YAxisKey = "TempAxis",
                XAxisKey = "TimeAxis",
                Color = OxyColors.Orange,
            };
            model.Series.Add(series);

            Plot1 = model;
            RaisePropertyChanged("Plot1");
        }

        /// <summary>
        /// Notify on property change.
        /// </summary>
        /// <param name="property"></param>
        protected void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
    }
}