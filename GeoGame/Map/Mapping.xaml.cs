using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace GeoGame.Map
{

    public partial class Mapping : Page
    {
        public Mapping()
        {
            InitializeComponent();
        }

        public async Task LoadMap(string mapName)
        {
            image.Source = await GetBitmapImageAsync(mapName);

            ChangeAllLocationItemStates(StartLocationItemMode);
            ChangeTargetLocation("");
        }

        public async Task<BitmapImage> GetBitmapImageAsync(string path)
        {
            BitmapImage image;

            byte[] bytes = null;
            path = @"Maps\" + path;

            try
            {
                using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    bytes = new byte[stream.Length];
                    await stream.ReadAsync(bytes, 0, (int)stream.Length);
                }

                image = new BitmapImage();

                if (bytes.Length != 0)
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.None;
                    image.StreamSource = new MemoryStream(bytes);
                    image.EndInit();
                }
                else
                    throw new Exception("The image is null.");

                return image;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't load image: " + ex.Message);
            }
        }

        public event EventHandler<int> LocationItemStatesChanged;
        public event EventHandler<Result> Result_Received;
        public event EventHandler<string> TargetLocationChanged;

        public LocationItem.Mode StartLocationItemMode { get; set; } = LocationItem.Mode.Hidden;

        public void ChangeLangDomination(bool slovak = true)
        {
            foreach (LocationItem item in locations_canvas.Children)
            {
                if (slovak)
                    item.SK_LangDomination = true;
                else
                    item.SK_LangDomination = false;
            }
        }

        public void ChangeAllLocationItemStates(LocationItem.Mode newState)
        {
            foreach (LocationItem item in locations_canvas.Children)
            {
                item.State = newState;
            }

            LocationItemStatesChanged?.Invoke(null, (int)newState);
        }

        public void ChangeLocationItemState(string location, LocationItem.Mode newState)
        {
            foreach (LocationItem item in locations_canvas.Children)
            {
                if (item.Location.Contains(location))
                    item.State = newState;
            }
            LocationItemStatesChanged?.Invoke(null, -1);
        }

        /// <summary>
        /// Returns a list of ALL possible locations that can be clicked in the game.
        /// </summary>
        public List<string> GetAllLocations(bool slovak = false)
        {
            List<string> _List = new List<string>();

            foreach (LocationItem item in locations_canvas.Children)
            {
                if (!slovak)
                    _List.Add(item.Location);
                else
                    _List.Add(item.LocationSK);
            }

            return _List;
        }

        public List<string> Locations = new List<string>()
        {
            "Nagytárkány",
            "Királyhelmec",
            "Lelesz",
            "Perbenyík",
            "Nagykövesd",
            "Bodrogszerdahely",
            "Bély",
            "Battyán",
            "Kisgéres",
            "Dobra",
            "Szentes",
            "Szomotor"
        };

        public List<string> Locations_auto = new List<string>()
        {
            "Nagytárkány",
            "Királyhelmec",
            "Lelesz",
            "Perbenyík",
            "Nagykövesd",
            "Bodrogszerdahely",
            "Bély",
            "Battyán",
            "Kisgéres",
            "Dobra",
            "Szentes",
            "Szomotor",
            "Zétény",
            "Boly",
            "Szolnocska",
            "Pólyán",
            "Rad",
            "Tiszacsernyő",
            "Szentmária"
        };

        public List<string> Locations_SK = new List<string>()
        {
            "Veľké Trakany",
            "Kráľovský Chlmec",
            "Leles",
            "Pribeník",
            "Veľký Kamenec",
            "Streda nad Bodrogom",
            "Biel",
            "Boťany",
            "Malý Horeš",
            "Dobrá",
            "Svätuše",
            "Somotor",
            "Zatín",
            "Boľ",
            "Soľnička",
            "Poľany",
            "Rad",
            "Čierna nad Tisou",
            "Svätá Mária"
        };

        public List<string> Locations_auto_temp = new List<string>();
        public void AutoPilot_PrepareTempList()
        {
            // Add locations to temp list, then we get rid of it as we go forwards in autopilot
            Locations_auto_temp.Clear();
            Locations_auto_temp.AddRange(Locations_auto);
        }

        public string TargetLocation { get; set; } = "";

        /// <summary>
        /// This checks the list of possible locations and changes the target location.
        /// </summary>
        /// <param name="newLocation"></param>
        public void ChangeTargetLocation(string newLocation)
        {
            if (newLocation == "")
            {
                TargetLocation = "";
                TargetLocationChanged?.Invoke(null, "");
                return;
            }

            int counter = 0;
            foreach (string location in GetAllLocations())
            {
                if (location.Contains(newLocation))
                {
                    TargetLocation = location;
                    string finalLocation = location + " - " + GetAllLocations(slovak:true)[counter]; //TODO: GetAllLocations(slovak:<boolvalue>) seems odd
                    TargetLocationChanged?.Invoke(null, finalLocation);

                    return;
                }
                counter++;
            }
        }

        public static Point GetMousePosition()
        {
            System.Drawing.Point point = System.Windows.Forms.Control.MousePosition;
            return new Point(point.X, point.Y);
        }

        /// <summary>
        /// Checks whether the clicked location was correct (in the circle).
        /// </summary>
        public Result DetermineResult(Point clickPos, string clickLocation)
        {
            // they clicked a LocationItem
            if (clickLocation != null) // they might have picked the correct location
            {
                // if they didn't click the correct thing
                if (clickLocation != TargetLocation)
                {
                    return new Result() { isCorrect = false, clickPos = clickPos, clickLocation = clickLocation, targetLocation = TargetLocation };
                }
                else
                    return new Result() { isCorrect = true, clickLocation = clickLocation, targetLocation = TargetLocation };
            }
            // they clicked on the canvas
            else // they definitely did not click on a possibly correct location
            {
                return new Result() { isCorrect = false, clickPos = clickPos, targetLocation = TargetLocation };
            }
        }

        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // get mouse coordinates
            Point mousepos = GetMousePosition();

            Result result = DetermineResult(mousepos, null);
            Result_Received?.Invoke(this, result);
        }

        private void location_Click(object sender, RoutedEventArgs e)
        {
            // get mouse coordinates
            Point mousepos = GetMousePosition();

            // get location name
            LocationItem item = (LocationItem)sender;
            string location = item.Location;

            Result result = DetermineResult(mousepos, location);
            Result_Received?.Invoke(this, result);
        }

        public class Result
        {
            public bool isCorrect;
            public Point clickPos;
            public Point targetPos;
            public string clickLocation;
            public string targetLocation;
        }
    }
}
