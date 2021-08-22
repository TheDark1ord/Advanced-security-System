using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


using Advanced_security_System.Main_Window.Frontend;

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
            NameScope.SetNameScope(this, null);

            Canvas PinContainer = FindName("PinContainer") as Canvas;
            InitializeComponent();
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

            PinWindowAnimations.addNewPinEllipse(ref PinContainer, typed_pin.Length);

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

            PinWindowAnimations.RemovePinEllipse(ref PinContainer, typed_pin.Length);
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
    }
}
