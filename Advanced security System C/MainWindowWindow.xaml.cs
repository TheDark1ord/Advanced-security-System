using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Advanced_security_System_C
{
    public class GridCellWidth : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((double)value / 175);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class GridCellHeight : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((double)value / 175);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    public partial class MainAppWindow : Window
    {
        public MainAppWindow()
        {
            InitializeComponent();

            var SidePanel = FindName("SidePanel") as StackPanel;
            side_panel_width = SidePanel.Width;
        }

        public void toggle_side_panel(object sender, RoutedEventArgs e)
        {
            StackPanel stackPanel = FindName("SidePanel") as StackPanel;

            if (stackPanel.Visibility == Visibility.Visible)
            {
                hide_side_panel(sender, e);
            }
            else
                show_sidePanel(sender, e);
        }

        public void show_sidePanel(object sender, RoutedEventArgs e)
        {
            var selectedIndex = ((sender as Image).TemplatedParent as FolderControl).Index;
            SelectFolder(selectedIndex);

            StackPanel SidePanel = FindName("SidePanel") as StackPanel;
            if (SidePanel.Visibility == Visibility.Visible)
                return;

            (SidePanel.Parent as Border).Margin = new Thickness(0, 0, 5, 0);
            SidePanel.Visibility = Visibility.Visible;

            Border border = SidePanel.Parent as Border;
            border.Visibility = Visibility.Visible;

            DoubleAnimation slide_out = new DoubleAnimation();
            slide_out.Duration = TimeSpan.FromSeconds(side_panel_anim_dur);

            Storyboard.SetTargetProperty(slide_out, new PropertyPath(StackPanel.WidthProperty));
            slide_out.From = 0;
            slide_out.To = side_panel_width;
            Storyboard.SetTarget(slide_out, SidePanel);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(slide_out);
            storyboard.Begin();

            Storyboard.SetTargetProperty(slide_out, new PropertyPath(SideFolderControl.WidthProperty));
            foreach (UIElement element in SidePanel.Children)
            {
                Storyboard.SetTarget(slide_out, element);
                storyboard.Children.Clear();
                storyboard.Children.Add(slide_out);
                storyboard.Begin();
            }
        }
        public void hide_side_panel(object sender, RoutedEventArgs e)
        {
            SelectedGallery.Deselect();
            SelectedGallery = null;

            StackPanel SidePanel = FindName("SidePanel") as StackPanel;

            if (SidePanel.Visibility == Visibility.Collapsed)
                return;

            (SidePanel.Parent as Border).Margin = new Thickness(0, 0, 0, 0);


            DoubleAnimation slide_out = new DoubleAnimation();
            slide_out.Duration = TimeSpan.FromSeconds(side_panel_anim_dur);

            Storyboard.SetTargetProperty(slide_out, new PropertyPath(StackPanel.WidthProperty));
            slide_out.From = side_panel_width;
            slide_out.To = 0;
            Storyboard.SetTarget(slide_out, SidePanel);

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(slide_out);
            storyboard.Begin();

            Storyboard.SetTargetProperty(slide_out, new PropertyPath(SideFolderControl.WidthProperty));
            foreach (UIElement element in SidePanel.Children)
            {
                Storyboard.SetTarget(slide_out, element);
                storyboard.Children.Clear();
                storyboard.Children.Add(slide_out);
                storyboard.Begin();
            }

            // Set Visibility to hidden after animation completes
            DispatcherTimer visibility_timer = new DispatcherTimer();
            visibility_timer.Interval = new TimeSpan(0, 0, 0, 0, (int)(side_panel_anim_dur * 1000));
            visibility_timer.Tick += new EventHandler(delegate (object sender, EventArgs e)
            {
                (sender as DispatcherTimer).Stop();

                StackPanel SidePanel = FindName("SidePanel") as StackPanel;
                SidePanel.Visibility = Visibility.Collapsed;

                Border border = SidePanel.Parent as Border;
                border.Visibility = Visibility.Hidden;
            });
            visibility_timer.Start();
        }

        public void SelectFolder(int selectedIndex)
        {
            if (SelectedGallery is not null)
                SelectedGallery.Deselect();

            var SideFolders = (FindName("SidePanel") as StackPanel).Children;
            foreach (var element in SideFolders)
            {
                var SideFolder = element as SideFolderControl;
                if (SideFolder.Index == selectedIndex)
                {
                    SelectedGallery = SideFolder;
                    SelectedGallery.Select();
                    break;
                }
            }
        }

        public void UnloadGalleryElements(object sender, RoutedEventArgs e)
        {
            Grid grid = FindName("GalleryGrid") as Grid;
            grid.Children.Clear();
        }

        // Drag FolderControl
        private void mousePressed(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                mouse_hold = true;

            UniformGrid grid = FindName("GalleryGrid") as UniformGrid;

            foreach (UIElement element in grid.Children)
            {
                FolderControl folderControl = element as FolderControl;
                if ((folderControl.Template.FindName("LabelControl", folderControl) as Label).IsMouseOver)
                {
                    dragged_element = folderControl;
                    dragged_element.BeginMouseDragging();

                    break;
                }
            }
        }

        private void mouseReleased(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                mouse_hold = false;

                if (dragged_element != null)
                {
                    dragged_element.StopMouseDragging();
                    dragged_element = null;
                }
            }
        }

        private bool mouse_hold = false;
        private FolderControl dragged_element = null;

        private SideFolderControl SelectedGallery = null;

        private const float side_panel_anim_dur = 0.15f;
        private double side_panel_width;
    };

}
