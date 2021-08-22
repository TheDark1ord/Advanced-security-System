using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Advanced_security_System.Main_Window.Frontend;

namespace Advanced_Security_system.Pin_Window.Frontend
{
    static class PinWindowAnimations
    {
        private const double anim_duration = 0.1;
        private const double pins_spacing = 5;

        static private Vector2 containerDimentions;
        static private double pins_right_border = 0;

        // If a pin is added or removed before previous animation is done,
        // previous animation will be overwritten, resulting in a visual bug,
        // to counteract it, animation queue is added
        static private Queue<Storyboard> animationQueue;

        static PinWindowAnimations()
        {
            animationQueue = new Queue<Storyboard>();
        }

        static public bool addNewPinEllipse(ref Canvas PinContainer, int typed_pin_length)
        {
            //TODO: Add new animation here
            if (!canAddNewPin(typed_pin_length))
                return false;

            PinWindow parentWindow = Window.GetWindow(PinContainer) as PinWindow;
            string ellipse_name = "E" + typed_pin_length.ToString();

            EllipseGeometry new_ellipse = new EllipseGeometry();
            new_ellipse.Center = new Point(PinContainer.Width / 2 + pins_right_border, PinContainer.Height / 2);
            new_ellipse.RadiusY = 0;
            new_ellipse.RadiusX = 0;
            parentWindow.RegisterName(ellipse_name, new_ellipse);

            System.Windows.Shapes.Path ellipsePath = new System.Windows.Shapes.Path();
            ellipsePath.Fill = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            ellipsePath.Data = new_ellipse;
            // New ellipse is added, but it vill be visible only after animation is done
            PinContainer.Children.Add(ellipsePath);
            pins_right_border += pins_right_border += PinContainer.Height / 2 + pins_spacing / 2;

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

            Storyboard.SetTargetProperty(anim_size_x, new PropertyPath(EllipseGeometry.RadiusXProperty));
            Storyboard.SetTargetProperty(anim_size_y, new PropertyPath(EllipseGeometry.RadiusYProperty));
            // Animation

            Storyboard add_new_ellipse_anim = new Storyboard();
            add_new_ellipse_anim.Children.Add(anim_size_x);
            add_new_ellipse_anim.Children.Add(anim_size_y);
            add_new_ellipse_anim.Children.Add(pins_pushLeft(ref PinContainer));

            animationQueue.Enqueue(add_new_ellipse_anim);
            // Remove storyboard from queue after animation is completed
            add_new_ellipse_anim.Completed += delegate (object sender, EventArgs e)
            {
                Storyboard parent = sender as Storyboard;
                if (!animationQueue.Contains(parent))
                    throw new InvalidOperationException("Given storyboard is not in the animation queue");

                if (animationQueue.Peek() != parent)
                    throw new Exception("Elements in queue are not in the right order");

                animationQueue.Dequeue();
                invokeNextAnimation();
            };

            // if animation queue is empty (containing only current animation), begin storyboard,
            // else wait for the queue to invoke it
            if (animationQueue.Count == 1)
            {
                add_new_ellipse_anim.Begin();
            }
            return true;
        }

        static public void RemovePinEllipse(ref Canvas PinContainer, int typed_pin_length)
        {
            if (!canAddNewPin(typed_pin_length - 1))
                return;

            System.Windows.Shapes.Path remove_ellipse = (Window.GetWindow(PinContainer)).FindName("E" + typed_pin_length.ToString()) as System.Windows.Shapes.Path;
            (Window.GetWindow(remove_ellipse)).UnregisterName("E" + typed_pin_length.ToString());

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

        static private Storyboard pins_pushLeft(ref Canvas PinContainer)
        {
            Storyboard out_anim = new Storyboard();
            int pinCount = PinContainer.Children.Count;

            for (int i = 0; i < pinCount - 1; i++)
            {
                PointAnimation cur_anim = new PointAnimation();
                EllipseGeometry cur_ellipse = (PinContainer.Children[i] as Path).Data as EllipseGeometry;

                Storyboard.SetTarget(cur_anim, cur_ellipse);
                cur_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                cur_anim.To = new Point(cur_ellipse.Center.X - PinContainer.Height / 2 - pins_spacing / 2, cur_ellipse.Center.Y);

                out_anim.Children.Add(cur_anim);
            }
            return out_anim;
        }

        static private Storyboard pins_pushRight(ref Canvas PinContainer)
        {
            Storyboard out_anim = new Storyboard();
            int pinCount = PinContainer.Children.Count;

            for(int i = 0; i < pinCount; i++)
            {
                PointAnimation cur_anim = new PointAnimation();
                EllipseGeometry cur_ellipse = (PinContainer.Children[i] as Path).Data as EllipseGeometry;

                Storyboard.SetTarget(cur_anim, cur_ellipse);
                cur_anim.From = new Point(cur_ellipse.Center.X, cur_ellipse.Center.Y);
                cur_anim.To = new Point(cur_ellipse.Center.X + PinContainer.Height / 2 + pins_spacing / 2, cur_ellipse.Center.Y);

                out_anim.Children.Add(cur_anim);
            }

            return out_anim;
        }


        static private bool canAddNewPin(int typed_pin_length)
        {
            return (typed_pin_length * (containerDimentions.Y + pins_spacing) > containerDimentions.X);
        }

        static private void invokeNextAnimation()
        {
            if (animationQueue.Count == 0)
                return;

            animationQueue.Peek().Begin();
        }
    }
}
