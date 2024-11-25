using Avalonia.Controls;
using Avalonia.Interactivity;

namespace StepBro.Workbench.Views
{
    public partial class MainWindow : Window
    {
        private bool m_xBottomShown = true;
        private bool m_xLeftShown = true;
        private bool m_xRightShown = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            this.UpdatePanels();
        }

        void checkBox_Layout(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Layout Check Change");
            this.UpdatePanels();
        }

        void CheckBoxPrimaryOnLeft_Click(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PrimaryOnLeft");

            if (checkBoxPrimaryLeft != null)
            {
                bool onLeft = checkBoxPrimaryLeft.IsChecked.GetValueOrDefault(true);

                DockPanel.SetDock(primarySideMenu, onLeft ? Dock.Left : Dock.Right);

                xLeft.Children.Clear();
                xRight.Children.Clear();
                xLeft.Children.Add(onLeft ? xLeftContent : xRightContent);
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

        private void panelAlignmentSelector_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            this.UpdatePanels();
            var port = new StepBro.UI.Controls.ChronoListViewPort();
        }

        private void UpdatePanels()
        {
            if (panelAlignmentSelector != null && checkBoxBottom != null && checkBoxLeft != null && checkBoxRight != null)
            {
                System.Diagnostics.Debug.WriteLine("panelAlignmentSelector " + panelAlignmentSelector.SelectedIndex.ToString());

                //xLeft
                //xSplitterLeft
                //xMid
                //xRight
                //xSplitterRight
                //xBottom
                //xSplitterBottom

                if (checkBoxBottom.IsChecked.GetValueOrDefault(true) != m_xBottomShown)
                {
                    if (checkBoxBottom.IsChecked.GetValueOrDefault(true))
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
                if (checkBoxLeft.IsChecked.GetValueOrDefault(true) != m_xLeftShown)
                {
                    if (checkBoxLeft.IsChecked.GetValueOrDefault(true))
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
                if (checkBoxRight.IsChecked.GetValueOrDefault(true) != m_xRightShown)
                {
                    if (checkBoxRight.IsChecked.GetValueOrDefault(true))
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

    }
}