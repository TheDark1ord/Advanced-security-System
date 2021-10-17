using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System;
using System.Globalization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;


namespace Advanced_security_System.Main_Window.Frontend
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

    class AlbumControl : Control
    {
        public static readonly DependencyProperty IndexProperty =
        DependencyProperty.Register(nameof(Index), typeof(int), typeof(AlbumControl),
        new PropertyMetadata(default(int)));
        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register(nameof(Size), typeof(int), typeof(AlbumControl),
                new PropertyMetadata(default(int)));
        public int Size
        {
            get { return (int)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty GalleryNameProperty =
            DependencyProperty.Register(nameof(GalleryName), typeof(string), typeof(SideAlbumControl),
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
            SideAlbumControl sideFolderControl = window.FindName("SG" + Index.ToString()) as SideAlbumControl;
            sideFolderControl.GalleryNameSide = GalleryName.ToString();
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(string), typeof(AlbumControl),
                new FrameworkPropertyMetadata("../../Resource files/billy.jpeg"));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public AlbumControl()
        {
            this.Loaded += OnLoaded;
        }
        public AlbumControl(AlbumControl copy)
        {
            Index = copy.Index;
            GalleryName = copy.GalleryName;
            Size = copy.Size;
            Source = copy.Source;
        }

        public AlbumControl GetCopy()
        {
            return new AlbumControl(this);
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
