using Advanced_security_System.Main_Window.Frontend;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Advanced_Security_system.Pin_Window.Frontend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class PinWindow : Window
    {

        [DllImport("CPlus.dll")]
        static extern bool validate_pin(string pin);

        public PinWindow()
        {
            animationQueue = new Queue<Storyboard>();
            NameScope.SetNameScope(this, new NameScope());
            InitializeComponent();

            Canvas PinContainer = FindName("PinContainer") as Canvas;
        }

        private void OpenMainWindow(object sender, EventArgs e)
        {
            MainAppWindow mainwindow = new MainAppWindow();
            mainwindow.Show();
            this.Close();
            // TODO: Remove this, after pin validation is done
            Thread.Sleep(500);

            (sender as DispatcherTimer).Stop();
            clearTypedPin();
        }

        private void clearTypedPin()
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
            addNewPinEllipse(ref PinContainer, typed_pin.Length);

            if (validate_pin(typed_pin))
            {
                System.Windows.Threading.DispatcherTimer newWindowTimer = new System.Windows.Threading.DispatcherTimer();
                newWindowTimer.Tick += new EventHandler(OpenMainWindow);
                newWindowTimer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                newWindowTimer.Start();
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

            typed_pin = typed_pin.Remove(typed_pin.Length - 1);

            RemovePinEllipse(ref PinContainer, typed_pin.Length);
        }

        private void Button_KeyDown(object sender, KeyEventArgs e)
        {
            Button b;
            string key = e.Key.ToString();

            if (!buttonNameMapping.ContainsKey(key))
                throw new ArgumentNullException(String.Format("buttonNameMappin Dictionary does not contain key named {0}", key));
            b = FindName(buttonNameMapping[key]) as Button;

            ButtonAutomationPeer peer = new ButtonAutomationPeer(b);
            IInvokeProvider invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
            invokeProv.Invoke();
        }

        Dictionary<string, string> buttonNameMapping = new Dictionary<string, string>
        {
            {"D0",   "Zero"     },
            {"D1",   "One"      },
            {"D2",   "Two"      },
            {"D3",   "Three"    },
            {"D4",   "Four"     },
            {"D5",   "Five"     },
            {"D6",   "Six"      },
            {"D7",   "Seven"    },
            {"D8",   "Eight"    },
            {"D9",   "Nine"     },
            {"Back", "Backspace"},
        };

        private string typed_pin;

        Queue<UIElement> element_queue = new Queue<UIElement>();

        ///Animations 
        private const double anim_duration = 0.1;
        private const double pins_spacing = 5;

        private double pins_right_border = 0;

        // If a pin is added or removed before previous animation is done,
        // previous animation will be overwritten, resulting in a visual bug,
        // to counteract it, animation queue is added
        private Queue<Storyboard> animationQueue;

        public bool addNewPinEllipse(ref Canvas PinContainer, int typed_pin_length)
        {
            //TODO: Add new animation here
            if (!canAddNewPin(ref PinContainer, typed_pin_length))
                return false;

            EllipseGeometry new_ellipse = new EllipseGeometry();
            new_ellipse.Center = new Point(PinContainer.Width / 2 + pins_right_border, PinContainer.Height / 2);
            new_ellipse.RadiusY = 0;
            new_ellipse.RadiusX = 0;
            string ellipse_name = "E" + typed_pin_length.ToString();

            NameScope.GetNameScope(this).RegisterName(ellipse_name, new_ellipse);
            // New ellipse is added, but it vill be visible only after animation is done

            pins_right_border += PinContainer.Height / 2 + pins_spacing / 2;

            //Animation
            DoubleAnimation anim_size_x = new DoubleAnimation();
            DoubleAnimation anim_size_y = new DoubleAnimation();

            anim_size_x.From = 0;
            anim_size_y.From = 0;

            anim_size_x.To = PinContainer.Height / 2;
            anim_size_y.To = PinContainer.Height / 2;

            anim_size_x.Duration = TimeSpan.FromSeconds(anim_duration);
            anim_size_y.Duration = TimeSpan.FromSeconds(anim_duration);

            Storyboard.SetTargetName(anim_size_x, ellipse_name);
            Storyboard.SetTargetName(anim_size_y, ellipse_name);

            //Storyboard.SetTarget(anim_size_x, new_ellipse);
            //Storyboard.SetTarget(anim_size_y, new_ellipse);

            Storyboard.SetTargetProperty(anim_size_x, new PropertyPath(EllipseGeometry.RadiusXProperty));
            Storyboard.SetTargetProperty(anim_size_y, new PropertyPath(EllipseGeometry.RadiusYProperty));
            // Animation

            Storyboard add_new_ellipse_anim = new Storyboard();
            add_new_ellipse_anim.Children.Add(anim_size_x);
            add_new_ellipse_anim.Children.Add(anim_size_y);
            add_new_ellipse_anim.Children.Add(pins_pushLeft(ref PinContainer));

            System.Windows.Shapes.Path ellipsePath = new System.Windows.Shapes.Path();
            ellipsePath.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            ellipsePath.Data = new_ellipse;
            PinContainer.Children.Add(ellipsePath);

            ellipsePath.Loaded += delegate (object Sender, RoutedEventArgs e)
            {
                // if animation queue is empty (containing only current animation), begin storyboard,
                // else wait for the queue to invoke it
                if (animationQueue.Count == 1)
                {
                    add_new_ellipse_anim.Begin(this);
                }
            };

            animationQueue.Enqueue(add_new_ellipse_anim);
            // Remove storyboard from queue after animation is completed
            add_new_ellipse_anim.Completed += delegate (object sender, EventArgs e)
            {
                animationQueue.Dequeue();
                invokeNextAnimation();
            };


            return true;
        }

        public void RemovePinEllipse(ref Canvas PinContainer, int typed_pin_length)
        {
            if (!canAddNewPin(ref PinContainer, typed_pin_length - 1))
                return;

            System.Windows.Shapes.Path remove_ellipse = this.FindName("E" + typed_pin_length.ToString()) as System.Windows.Shapes.Path;
            this.UnregisterName("E" + typed_pin_length.ToString());

            PinContainer.Children.Remove(remove_ellipse);
            pins_right_border -= pins_right_border += PinContainer.Height / 2 + pins_spacing / 2;

            Storyboard remove_ellipse_anim = new Storyboard();
            remove_ellipse_anim.Children.Add(pins_pushRight(ref PinContainer));

            animationQueue.Enqueue(remove_ellipse_anim);
            remove_ellipse_anim.Completed += delegate (object sender, EventArgs e)
            {
                Storyboard parent = sender as Storyboard;
                if (!animationQueue.Contains(parent))
                    throw new InvalidOperationException("Given storyboard is not in the animation queue");

                if (animationQueue.Peek() != parent)
                    throw new Exception("Elements in queue are not in the right order");

                animationQueue.Dequeue();
                invokeNextAnimation();
            };

            if (animationQueue.Count == 1)
            {
                remove_ellipse_anim.Begin();
            }
        }

        private Storyboard pins_pushLeft(ref Canvas PinContainer)
        {
            Storyboard out_anim = new Storyboard();
            int pinCount = PinContainer.Children.Count;

            for (int i = 0; i < pinCount; i++)
            {
                PointAnimation cur_anim = new PointAnimation();
                EllipseGeometry cur_ellipse = (PinContainer.Children[i] as Path).Data as EllipseGeometry;
                string target_name = "E" + (i + 1).ToString();

                Storyboard.SetTargetName(cur_anim, target_name);
                Storyboard.SetTargetProperty(cur_anim, new PropertyPath(EllipseGeometry.CenterProperty));

                cur_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                cur_anim.To = new Point(cur_ellipse.Center.X - PinContainer.Height / 2 - pins_spacing / 2, cur_ellipse.Center.Y);
                cur_anim.Duration = TimeSpan.FromSeconds(anim_duration);

                out_anim.Children.Add(cur_anim);
            }
            return out_anim;
        }

        private Storyboard pins_pushRight(ref Canvas PinContainer)
        {
            Storyboard out_anim = new Storyboard();
            int pinCount = PinContainer.Children.Count;

            for (int i = 0; i < pinCount; i++)
            {
                PointAnimation cur_anim = new PointAnimation();
                EllipseGeometry cur_ellipse = (PinContainer.Children[i] as Path).Data as EllipseGeometry;
                string target_name = "E" + (i + 1).ToString();

                Storyboard.SetTargetName(cur_anim, target_name);
                Storyboard.SetTargetProperty(cur_anim, new PropertyPath(EllipseGeometry.CenterProperty));

                cur_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                cur_anim.To = new Point(cur_ellipse.Center.X + PinContainer.Height / 2 + pins_spacing / 2, cur_ellipse.Center.Y);
                cur_anim.Duration = TimeSpan.FromSeconds(anim_duration);

                out_anim.Children.Add(cur_anim);
            }

            return out_anim;
        }

        private bool canAddNewPin(ref Canvas PinContainer, int typed_pin_length)
        {
            return !(typed_pin_length * (PinContainer.Height + pins_spacing) > PinContainer.Width);
        }

        private void invokeNextAnimation()
        {
            if (animationQueue.Count == 0)
                return;

            animationQueue.Peek().Begin();
        }
    }
}
