using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Advanced_security_System_C
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        [DllImport("CPlus.dll")]
        private static extern bool validate_pin(string pin);


        [DllImport("CPlus.dll")]
        private static extern bool LoadGalleryFile(string fileName);

        public MainWindow()
        {
            NameScope.SetNameScope(this, null);
            InitializeComponent();

            //LoadGalleryFile("C:\\Users\\rubco\\source\\repos\\Advanced security System C\\C++ DLLs\\Data\\TestFile.gd");

            animationQueue = new Queue<Tuple<PointAnimation, bool>>();
        }

        private void OpenMainWindow(object sender, EventArgs e)
        {
            MainAppWindow mainwindow = new MainAppWindow();
            mainwindow.Show();
            this.Close();

            Thread.Sleep(500);

            System.Windows.Threading.DispatcherTimer rootTimer = sender as System.Windows.Threading.DispatcherTimer;
            rootTimer.Stop();
            clearTypedPin();
        }

        void clearTypedPin()
        {
            typed_pin = "";
            Canvas PinContainer = FindName("PinContainer") as Canvas;
            PinContainer.Children.Clear();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            Button pressed_button = e.Source as Button;
            typed_pin += pressed_button.Content.ToString();

            Canvas PinContainer = FindName("PinContainer") as Canvas;

            if (typed_pin.Length * (PinContainer.Height + pins_spacing) > PinContainer.Width)
            {
                return;
            }
            else
            {
                EllipseGeometry new_ellipse = new EllipseGeometry();
                new_ellipse.Center = new Point(PinContainer.Width / 2 + pins_right_border, PinContainer.Height / 2);
                new_ellipse.RadiusY = PinContainer.Height / 2;
                new_ellipse.RadiusX = new_ellipse.RadiusY;

                pins_right_border += PinContainer.Height / 2 + pins_spacing / 2;

                this.RegisterName("E" + typed_pin.Length.ToString(), new_ellipse);

                DoubleAnimation anim_size_x = new DoubleAnimation();
                anim_size_x.From = 0;
                anim_size_x.To = new_ellipse.RadiusX;
                anim_size_x.Duration = TimeSpan.FromSeconds(anim_duration);

                DoubleAnimation anim_size_y = new DoubleAnimation();
                anim_size_y.From = 0;
                anim_size_y.To = new_ellipse.RadiusY;
                anim_size_y.Duration = TimeSpan.FromSeconds(anim_duration);

                Storyboard.SetTargetName(anim_size_x, "E" + typed_pin.Length.ToString());
                Storyboard.SetTargetName(anim_size_y, "E" + typed_pin.Length.ToString());
                Storyboard.SetTargetProperty(anim_size_x, new PropertyPath(EllipseGeometry.RadiusXProperty));
                Storyboard.SetTargetProperty(anim_size_y, new PropertyPath(EllipseGeometry.RadiusYProperty));


                Storyboard anim_story = new Storyboard();
                anim_story.Children.Add(anim_size_x);
                anim_story.Children.Add(anim_size_y);

                System.Windows.Shapes.Path ellipsePath = new System.Windows.Shapes.Path();
                ellipsePath.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
                ellipsePath.Data = new_ellipse;


                this.RegisterName("P" + typed_pin.Length.ToString(), ellipsePath);

                ellipsePath.Loaded += delegate (object sender, RoutedEventArgs e)
                {
                    anim_story.Begin(this);
                };

                PointAnimation move_left_anim = new PointAnimation();
                move_left_anim.Duration = TimeSpan.FromSeconds(anim_duration);
                Storyboard.SetTargetProperty(move_left_anim, new PropertyPath(EllipseGeometry.CenterProperty));

                Storyboard move_left_board = new Storyboard();

                //If second button press occurs when animation is not fully finished
                // Next animation will override previous
                // To fix that I use animation queue as shown below

                // Remove animation from queue after it is finished
                if (animationQueue.Count != 0)
                {
                    animationQueue.Enqueue(new Tuple<PointAnimation, bool>(move_left_anim, false));

                }
                else
                {

                    animationQueue.Enqueue(new Tuple<PointAnimation, bool>(move_left_anim, false));

                    int it = 1;
                    foreach (UIElement element in PinContainer.Children)
                    {
                        System.Windows.Shapes.Path cur_path = element as System.Windows.Shapes.Path;
                        EllipseGeometry cur_ellipse = cur_path.Data as EllipseGeometry;

                        Storyboard.SetTargetName(move_left_anim, "E" + it.ToString());
                        move_left_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                        move_left_anim.To = new Point(
                            cur_ellipse.Center.X - PinContainer.Height / 2 - pins_spacing / 2, cur_ellipse.Center.Y);

                        move_left_board.Children.Clear();
                        move_left_board.Children.Add(move_left_anim);

                        move_left_board.Begin(this);
                        it++;
                    }

                    DispatcherTimer anim_complete_timer = new DispatcherTimer();
                    anim_complete_timer.Interval = new TimeSpan(0, 0, 0, 0, (int)(anim_duration * 1000));
                    anim_complete_timer.Tick += new EventHandler(anim_completed);
                    anim_complete_timer.Start();

                }

                element_queue.Enqueue(ellipsePath);

                if (validate_pin(typed_pin))
                {
                    System.Windows.Threading.DispatcherTimer newWindowTimer = new System.Windows.Threading.DispatcherTimer();
                    newWindowTimer.Tick += new EventHandler(OpenMainWindow);
                    newWindowTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                    newWindowTimer.Start();
                }
            }
        }
        private void backspace_click(object source, RoutedEventArgs e)
        {
            Button pressed_button = e.Source as Button;
            Canvas PinContainer = FindName("PinContainer") as Canvas;
            if (typed_pin == null || typed_pin.Length == 0)
            {
                return;
            }

            if ((typed_pin.Length) * (PinContainer.Height + pins_spacing) > PinContainer.Width)
            {
                typed_pin = typed_pin.Remove(typed_pin.Length - 1);
                return;
            }

            System.Windows.Shapes.Path remove_ellipse = FindName("P" + typed_pin.Length.ToString()) as System.Windows.Shapes.Path;
            this.UnregisterName("E" + typed_pin.Length.ToString());
            this.UnregisterName("P" + typed_pin.Length.ToString());
            typed_pin = typed_pin.Remove(typed_pin.Length - 1);


            PinContainer.Children.Remove((UIElement)(remove_ellipse));
            pins_right_border -= PinContainer.Height / 2 + pins_spacing / 2;

            PointAnimation move_right_anim = new PointAnimation();
            move_right_anim.Duration = TimeSpan.FromSeconds(anim_duration);
            Storyboard.SetTargetProperty(move_right_anim, new PropertyPath(EllipseGeometry.CenterProperty));

            //If second button press occurs when animation is not fully finished
            // Next animation will override previous
            // To fix that I use animation queue as shown below

            // Remove animation from queue after it is finished
            if (animationQueue.Count != 0)
            {
                animationQueue.Enqueue(new Tuple<PointAnimation, bool>(move_right_anim, true));
            }
            else
            {
                animationQueue.Enqueue(new Tuple<PointAnimation, bool>(move_right_anim, true));
                Storyboard move_right_board = new Storyboard();

                int it = 1;
                foreach (UIElement element in PinContainer.Children)
                {
                    System.Windows.Shapes.Path cur_path = element as System.Windows.Shapes.Path;
                    EllipseGeometry cur_ellipse = cur_path.Data as EllipseGeometry;

                    Storyboard.SetTargetName(move_right_anim, "E" + it.ToString());
                    move_right_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                    move_right_anim.To = new Point(
                        cur_ellipse.Center.X + PinContainer.Height / 2 + pins_spacing / 2, cur_ellipse.Center.Y);

                    move_right_board.Children.Clear();
                    move_right_board.Children.Add(move_right_anim);

                    move_right_board.Begin(this);
                    it++;
                }
                DispatcherTimer anim_complete_timer = new DispatcherTimer();
                anim_complete_timer.Interval = new TimeSpan(0, 0, 0, 0, (int)(anim_duration * 1000));
                anim_complete_timer.Tick += new EventHandler(anim_completed);
                anim_complete_timer.Start();

            }
        }

        private void anim_completed(object sender, EventArgs e)
        {
            Canvas PinContainer = FindName("PinContainer") as Canvas;

            if (element_queue.Count > 0)
                PinContainer.Children.Add(element_queue.Dequeue());

            (sender as DispatcherTimer).Stop();
            animationQueue.Dequeue();
            if (animationQueue.Count > 0)
            {
                (sender as DispatcherTimer).Stop();

                PointAnimation cur_anim = animationQueue.Peek().Item1;
                Storyboard cur_story = new Storyboard();

                int it = 1;
                foreach (UIElement element in PinContainer.Children)
                {
                    System.Windows.Shapes.Path cur_path = element as System.Windows.Shapes.Path;
                    EllipseGeometry cur_ellipse = cur_path.Data as EllipseGeometry;

                    Storyboard.SetTargetName(cur_anim, "E" + it.ToString());
                    cur_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);

                    if (!animationQueue.Peek().Item2)
                    {
                        cur_anim.To = new Point(
                            cur_ellipse.Center.X - PinContainer.Height / 2 - pins_spacing / 2, cur_ellipse.Center.Y);
                    }
                    else
                    {
                        cur_anim.To = new Point(
                            cur_ellipse.Center.X + PinContainer.Height / 2 + pins_spacing / 2, cur_ellipse.Center.Y);
                    }

                    cur_story.Children.Clear();
                    cur_story.Children.Add(cur_anim);

                    cur_story.Begin(this);
                    it++;
                }
                if (it > 1)
                {
                    DispatcherTimer anim_complete_timer = new DispatcherTimer();
                    anim_complete_timer.Interval = new TimeSpan(0, 0, 0, 0, (int)(anim_duration * 1000));
                    anim_complete_timer.Tick += new EventHandler(anim_completed);
                    anim_complete_timer.Start();
                }
            }
        }

        private void Button_KeyDown(object sender, KeyEventArgs e)
        {
            Button b;
            string key = e.Key.ToString();
            if (key == "D0")
                b = FindName("Zero") as Button;
            else if (key == "D1")
                b = FindName("One") as Button;
            else if (key == "D2")
                b = FindName("Two") as Button;
            else if (key == "D3")
                b = FindName("Three") as Button;
            else if (key == "D4")
                b = FindName("Four") as Button;
            else if (key == "D5")
                b = FindName("Five") as Button;
            else if (key == "D6")
                b = FindName("Six") as Button;
            else if (key == "D7")
                b = FindName("Seven") as Button;
            else if (key == "D8")
                b = FindName("Eight") as Button;
            else if (key == "D9")
                b = FindName("Nine") as Button;
            else if (key == "Back")
                b = FindName("Backspace") as Button;
            else
                return;

            ButtonAutomationPeer peer = new ButtonAutomationPeer(b);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        private string typed_pin;
        private double pins_right_border = 0;
        const double pins_spacing = 5;

        const double anim_duration = 0.10;

        Queue<Tuple<PointAnimation, bool>> animationQueue;
        Queue<UIElement> element_queue = new Queue<UIElement>();
    }
}
