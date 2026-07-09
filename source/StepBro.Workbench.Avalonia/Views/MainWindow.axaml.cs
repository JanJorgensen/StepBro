using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using StepBro.Core;
using StepBro.Core.General;
using StepBro.Core.Logging;
using StepBro.HostSupport.Models;
using StepBro.UI.Controls;
using StepBro.Workbench.ViewModels;
using System;
using System.Threading;

namespace StepBro.Workbench.Views
{
    public partial class MainWindow : Window
    {
        private bool m_primaryOnLeft = true;
        private bool m_xBottomShown = true;
        private bool m_xLeftShown = true;
        private bool m_xRightShown = true;
        private bool m_closing = false;
        private ILogger m_threadLogger = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private MainWindowViewModel Model { get { return this.DataContext as MainWindowViewModel; } }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            this.UpdatePanels();

            if (m_threadLogger != null)
            {
                var logger = new System.Threading.Thread(LoggerThread);
                logger.Start();
            }
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (this.Model != null)
            {
                this.Model.PropertyChanged += Model_PropertyChanged;
            }
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"Prop change: {e.PropertyName}");
            if (e.PropertyName == nameof(MainWindowViewModel.PrimaryPanelVisible))
            {
                this.UpdatePanels();
            }
        }

        internal void StartLogging()
        {
            m_threadLogger = this.Model.StepBroHostModel?.RootLogger.LogEntering("Workbench", "Crazy Logging");
        }

        private void LoggerThread()
        {
            System.Random rnd = new System.Random();
            while (!m_closing)
            {
                Thread.Sleep(50);

                for (int i = 0; i < 10; i++)
                {
                    m_threadLogger.Log("Spunk " + StepBro.Core.Data.AlphaID.Create((uint)rnd.Next(2000000000), 5));
                }
            }
        }

        protected override void OnClosing(WindowClosingEventArgs e)
        {
            base.OnClosing(e);
            m_closing = true;
        }

        void checkBox_Layout(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Layout Check Change");
            this.UpdatePanels();
        }

        void CheckBoxPrimaryOnLeft_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PrimaryOnLeft");

            bool onLeft = checkBoxPrimaryLeft.IsChecked.GetValueOrDefault(true);

            if (checkBoxPrimaryLeft != null && onLeft != m_primaryOnLeft)
            {
                m_primaryOnLeft = onLeft;

                //// Switch the enable flags for left and right panels.
                //bool leftOn = checkBoxLeft.IsChecked.GetValueOrDefault(true);
                //checkBoxLeft.IsChecked = checkBoxRight.IsChecked.GetValueOrDefault(true);
                //checkBoxRight.IsChecked = leftOn;

                this.UpdatePanels();

                DockPanel.SetDock(primarySideMenu, onLeft ? Dock.Left : Dock.Right);
                DockPanel.SetDock(primarySideMenuLine, onLeft ? Dock.Right : Dock.Left);

                xLeft.Children.Clear();
                xRight.Children.Clear();
                xLeft.Children.Add(xLeftSplitterSpacing);
                xLeft.Children.Add(onLeft ? xLeftContent : xRightContent);
                xRight.Children.Add(xRightSplitterSpacing);
                xRight.Children.Add(onLeft ? xRightContent : xLeftContent);

                var w = mainGrid.ColumnDefinitions[0].Width;
                mainGrid.ColumnDefinitions[0].Width = mainGrid.ColumnDefinitions[2].Width;
                mainGrid.ColumnDefinitions[2].Width = w;
            }
        }

        void Button_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Button");
        }

        private void panelAlignmentSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.UpdatePanels();
            //var port = new StepBro.UI.Controls.ChronoListViewPort();
        }

        private void UpdatePanels()
        {
            if (panelAlignmentSelector != null && checkBoxBottomPanel != null && checkBoxSecondaryPanel != null)
            {
                System.Diagnostics.Debug.WriteLine("panelAlignmentSelector " + panelAlignmentSelector.SelectedIndex.ToString());

                //xLeft
                //xSplitterLeft
                //xMid
                //xRight
                //xSplitterRight
                //xBottom
                //xSplitterBottom

                bool leftOn = m_primaryOnLeft ? this.Model.PrimaryPanelVisible : checkBoxSecondaryPanel.IsChecked.GetValueOrDefault(true);
                bool rightOn = m_primaryOnLeft ? checkBoxSecondaryPanel.IsChecked.GetValueOrDefault(true) : this.Model.PrimaryPanelVisible;

                if (checkBoxBottomPanel.IsChecked.GetValueOrDefault(true) != m_xBottomShown)
                {
                    if (checkBoxBottomPanel.IsChecked.GetValueOrDefault(true))
                    {
                        m_xBottomShown = true;
                        mainGrid.Children.Add(xBottom);
                        mainGrid.Children.Add(xSplitterBottom);
                    }
                    else
                    {
                        m_xBottomShown = false;
                        mainGrid.Children.Remove(xBottom);
                        mainGrid.Children.Remove(xSplitterBottom);
                    }
                }
                if (leftOn != m_xLeftShown)
                {
                    if (leftOn)
                    {
                        m_xLeftShown = true;
                        mainGrid.Children.Add(xLeft);
                        mainGrid.Children.Add(xSplitterLeft);
                    }
                    else
                    {
                        m_xLeftShown = false;
                        mainGrid.Children.Remove(xLeft);
                        mainGrid.Children.Remove(xSplitterLeft);
                    }
                }
                if (rightOn != m_xRightShown)
                {
                    if (rightOn)
                    {
                        m_xRightShown = true;
                        mainGrid.Children.Add(xRight);
                        mainGrid.Children.Add(xSplitterRight);
                    }
                    else
                    {
                        m_xRightShown = false;
                        mainGrid.Children.Remove(xRight);
                        mainGrid.Children.Remove(xSplitterRight);
                    }
                }

                Grid.SetColumn(xMid, m_xLeftShown ? 1 : 0);
                Grid.SetColumnSpan(xMid, 1 + (m_xLeftShown ? 0 : 1) + (m_xRightShown ? 0 : 1));
                Grid.SetRowSpan(xMid, m_xBottomShown ? 1 : 2);

                bool bottomToLeft = m_xBottomShown && (!m_xLeftShown || panelAlignmentSelector.SelectedIndex == 0 || panelAlignmentSelector.SelectedIndex == 3);
                bool bottomToRight = m_xBottomShown && (!m_xRightShown || panelAlignmentSelector.SelectedIndex == 1 || panelAlignmentSelector.SelectedIndex == 3);

                if (m_xBottomShown)
                {
                    var col = bottomToLeft ? 0 : 1;
                    var span = 1 + (bottomToLeft ? 1 : 0) + (bottomToRight ? 1 : 0);
                    Grid.SetColumn(xBottom, col);
                    Grid.SetColumnSpan(xBottom, span);
                    Grid.SetColumn(xSplitterBottom, col);
                    Grid.SetColumnSpan(xSplitterBottom, span);
                }

                if (m_xLeftShown)
                {
                    Grid.SetRowSpan(xLeft, bottomToLeft ? 1 : 2);
                    Grid.SetRowSpan(xSplitterLeft, bottomToLeft ? 1 : 2);
                }

                if (m_xRightShown)
                {
                    Grid.SetRowSpan(xRight, bottomToRight ? 1 : 2);
                    Grid.SetRowSpan(xSplitterRight, bottomToRight ? 1 : 2);
                }
            }
        }

        private void SettingsSelector_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("SettingsSelector");
        }

        private void FilesSelector_Click(object? sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("FilesSelector Click");
        }

        private void FilesSelector_PointerPressed(object? sender, PointerPressedEventArgs e)    // TODO: Why does this not work?
        {
            System.Diagnostics.Debug.WriteLine("FilesSelector Pressed");
        }

        //private void FilesSelector_PointerPressed(object? sender, PointerPressedEventArgs e)
        //{
        //    System.Diagnostics.Debug.WriteLine("FilesSelector Pressed");
        //}
    }
}