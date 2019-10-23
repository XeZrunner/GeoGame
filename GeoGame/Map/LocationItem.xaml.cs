using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GeoGame.Map
{
    public partial class LocationItem : UserControl
    {
        public LocationItem()
        {
            InitializeComponent();

            Anim_In = (Storyboard)FindResource("Anim_In");
            Anim_Out = (Storyboard)FindResource("Anim_Out");
        }

        #region Storyboards
        Storyboard Anim_In;
        Storyboard Anim_Out;
        #endregion

        string _locationName = "";
        string _locationNameSK = "";
        double _ellipseWidth = 148;
        double _ellipseHeight = 0;

        public string Location
        {
            get { return _locationName; }
            set { _locationName = value; locationText.Text = value; }
        }

        public string LocationSK
        {
            get { return _locationNameSK; }
            set
            {
                _locationNameSK = value; locationSKText.Text = value;

                if (value == null || value == "")
                    locationSKText.Visibility = Visibility.Collapsed;
                else
                    locationSKText.Visibility = Visibility.Visible;
            }
        }

        public double LocationWidth
        {
            get { return _ellipseWidth; }
            set
            {
                _ellipseWidth = value;
                locationEllipse.Width = _ellipseWidth;

                if (_ellipseHeight == 0)
                    locationEllipse.Height = _ellipseWidth;
            }
        }

        public double LocationHeight
        {
            get { return _ellipseHeight; }
            set
            {
                _ellipseHeight = value;
                locationEllipse.Height = _ellipseHeight;

                if (_ellipseWidth == 0)
                    locationEllipse.Width = _ellipseHeight;
            }
        }

        public enum Mode
        {
            Hidden,
            Hide,
            ShowTextOnly,
            ShowEllipseAndText,
            ShowEllipseAndText_Error,
            ShowEllipseOnly
        }

        Mode _state = Mode.Hidden;
        public Mode State
        {
            get { return _state; }
            set
            {
                _state = value;
                ChangeState(value);
            }
        }

        public async void ChangeState(Mode newState)
        {
            if (newState == Mode.Hidden)
            {
                locationEllipse.Opacity = 0;
                locationText.Opacity = 0;
                locationSKText.Opacity = 0;

                return;
            }

            if (newState == Mode.Hide)
            {
                Anim_Out.Begin();
                await Task.Delay(TimeSpan.FromSeconds(.8));

                locationEllipse.Opacity = 0;
                locationText.Opacity = 0;
                locationSKText.Opacity = 0;

                return;
            }

            if (newState == Mode.ShowEllipseAndText)
            {
                locationEllipse.Opacity = 1;
                locationText.Opacity = 1;
                locationSKText.Opacity = 1;

                Anim_In.Begin();
            }

            if (newState == Mode.ShowEllipseAndText_Error)
            {
                locationEllipse.Opacity = 1;
                locationText.Opacity = 1;
                locationSKText.Opacity = 1;

                Anim_In.Begin();
            }

            if (newState == Mode.ShowEllipseOnly)
            {
                locationEllipse.Opacity = 1;
                locationText.Opacity = 0;
                locationSKText.Opacity = 0;

                Anim_In.Begin();
            }

            if (newState == Mode.ShowTextOnly)
            {
                locationEllipse.Opacity = 0;
                locationText.Opacity = 1;
                locationSKText.Opacity = 1;

                Anim_In.Begin();
            }

            if (newState == Mode.ShowEllipseAndText_Error)
                locationEllipse.SetResourceReference(Ellipse.FillProperty, "Red");
            else
                locationEllipse.SetResourceReference(Ellipse.FillProperty, "Accent");
        }

        public event RoutedEventHandler Click;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(this, new RoutedEventArgs());
        }
    }
}
