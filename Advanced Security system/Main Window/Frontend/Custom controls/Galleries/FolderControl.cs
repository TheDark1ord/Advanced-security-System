using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;

using Advanced_security_System.Main_Window.Frontend.Custom_Controls.Side_Panel_Galleries;

namespace Advanced_security_System.Main_Window.Frontend.Custom_Controls.Galleries
{

    public class GetImageSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            if (value.GetType() != typeof(int))
                throw new ArgumentException("Bad argument type, int requaired");

            return (int)value * 0.80;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            throw new NotImplementedException();
        }
    }
    public class GetLabelSize : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            if (value.GetType() != typeof(int))
                throw new ArgumentException("Bad argument type, int requaired");

            return (int)value * 0.20;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo cultureinfo)
        {
            throw new NotImplementedException();
        }
    }

    class FolderControl : Control
    {
        public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(Index), typeof(int), typeof(FolderControl),
        new PropertyMetadata(default(int)));
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(int), typeof(FolderControl),
                new PropertyMetadata(default(int)));
        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty GalleryNameProperty =
            DependencyProperty.Register(nameof(GalleryName), typeof(string), typeof(SideFolderControl),
                new FrameworkPropertyMetadata("Hello world!"));

        public string GalleryName
        {
            get { return(string)GetValue(GalleryNameProperty); }
            set { SetValue(GalleryNameProperty, value); GalleryNameChanged(); }
        }

        private void GalleryNameChanged()
        {
            ControlTemplate template = this.Template;
            if (template == null)
                return;
            Label labelControl = template.FindName("LabelControl", this) as Label;
            labelControl.Content = GalleryName.ToString();

            // Find linked side folder control
            MainAppWindow window = Window.GetWindow(this) as MainAppWindow;
            SideFolderControl sideFolderControl = window.FindName("SG" + Index.ToString()) as SideFolderControl;
            sideFolderControl.GalleryNameSide = GalleryName.ToString();
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(string), typeof(FolderControl),
                new FrameworkPropertyMetadata("../../Resource files/billy.jpeg"));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public FolderControl()
        {
            this.Loaded += OnLoaded;
        }
        public FolderControl(FolderControl copy)
        {
            Index = copy.Index;
            GalleryName = copy.GalleryName;
            Size = copy.Size;
            Source = copy.Source;
        }

        public FolderControl GetCopy()
        {
            return new FolderControl(this);
        }
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            MainAppWindow window = Window.GetWindow(this) as MainAppWindow;
            window.RegisterName("G" + Index.ToString(), this);
            (Window.GetWindow(this) as MainAppWindow).MouseMove += new MouseEventHandler(MouseMoved);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            ControlTemplate Template = this.Template;

            LoadImage(Template);
            AssignControlActions(Template);
        }

        private void LoadImage(ControlTemplate template)
        {
            //Todo : Load image from byte array insted
            // Load byte array from c++ code
            Image img = Template.FindName("ImageControl", this) as Image;
            BitmapImage img_src = new BitmapImage(new Uri(Source, UriKind.Relative));
            //img_src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            img.Source = img_src;
        }

        private void AssignControlActions(ControlTemplate template)
        {
            Label label = template.FindName("LabelControl", this) as Label;
            label.MouseDoubleClick += new MouseButtonEventHandler(ShowTextBlock);

            TextBox text = template.FindName("HiddenTextControl", this) as TextBox;
            text.KeyDown += new KeyEventHandler(HideTextBlock);
            (Window.GetWindow(this) as MainAppWindow).MouseLeftButtonUp += new MouseButtonEventHandler(HideTextBlock);

            Image img = template.FindName("ImageControl", this) as Image;
            MainAppWindow window = Window.GetWindow(img) as MainAppWindow;
            img.MouseLeftButtonDown += new MouseButtonEventHandler(window.show_sidePanel);
            //img.MouseLeftButtonDown += new MouseButtonEventHandler(window.UnloadGalleryElements);
        }

        private void ShowTextBlock(object sender, RoutedEventArgs e)
        {
            ControlTemplate Template = this.Template;
            TextBox text = Template.FindName("HiddenTextControl", this) as TextBox;

            text.Visibility = Visibility.Visible;
            (Template.FindName("LabelControl", this) as Label).Visibility = Visibility.Hidden;
            text.Focus();
            text.Select(text.Text.Length, 0);
        }
        private void HideTextBlock(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                (sender as TextBox).Visibility = Visibility.Hidden;
                (Template.FindName("LabelControl", this) as Label).Visibility = Visibility.Visible;
            }
        }

        private void HideTextBlock(object sender, MouseButtonEventArgs e)
        {
            TextBox text = (this.Template.FindName("HiddenTextControl", this) as TextBox);
            if (text.IsMouseOver)
                return;
            if (text.Visibility == Visibility.Visible)
            {
                text.Visibility = Visibility.Hidden;
                Label label = Template.FindName("LabelControl", this) as Label;
                text.Text = label.Content.ToString();
                label.Visibility = Visibility.Visible;
            }
            else
                return;
        }

        // Drag element
        private Point anchor_point;
        TranslateTransform transform = new TranslateTransform();
        public void BeginMouseDragging()
        {
            mouse_drag = true;
            anchor_point = Mouse.GetPosition(null);
        }

        public void StopMouseDragging()
        {
            mouse_drag = false;
        }

        public void MouseMoved(object sender, MouseEventArgs e)
        {
            if (!mouse_drag)
                return;

            Point currentPoint = Mouse.GetPosition(null);

            transform.X += currentPoint.X - anchor_point.X;
            transform.Y += currentPoint.Y - anchor_point.Y;

            this.RenderTransform = transform;
            anchor_point = currentPoint;
        }
        private bool mouse_drag = false;
    }
}
