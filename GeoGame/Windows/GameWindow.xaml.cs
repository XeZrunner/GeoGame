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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using XeZrunner.UI.Popups;
using System.Windows.Media.Animation;

namespace GeoGame
{
    public partial class GameWindow : Window
    {
        public GameWindow()
        {
            InitializeComponent();

            UI_In = (Storyboard)FindResource("UI_In");
            UI_Out = (Storyboard)FindResource("UI_Out");
            UI_Choose_In = (Storyboard)FindResource("UI_ChooseLocation_In");
            UI_Choose_Out = (Storyboard)FindResource("UI_ChooseLocation_Out");
            UI_Correct_In = (Storyboard)FindResource("UI_Correct_In");
            UI_Correct_Out = (Storyboard)FindResource("UI_Correct_Out");
            UI_Wrong_In = (Storyboard)FindResource("UI_Wrong_In");
            UI_Wrong_Out = (Storyboard)FindResource("UI_Wrong_Out");
            UI_AutoPilot_Start_In = (Storyboard)FindResource("UI_AutoPilot_Start_In");
            UI_AutoPilot_Start_Out = (Storyboard)FindResource("UI_AutoPilot_Start_Out");
            UI_AutoPilot_Done_In = (Storyboard)FindResource("UI_AutoPilot_Done_In");
            UI_AutoPilot_Done_Out = (Storyboard)FindResource("UI_AutoPilot_Done_Out");
        }

        #region Storyboards

        Storyboard UI_In;
        Storyboard UI_Out;
        Storyboard UI_Choose_In;
        Storyboard UI_Choose_Out;
        Storyboard UI_Correct_In;
        Storyboard UI_Correct_Out;
        Storyboard UI_Wrong_In;
        Storyboard UI_Wrong_Out;
        Storyboard UI_AutoPilot_Start_In;
        Storyboard UI_AutoPilot_Start_Out;
        Storyboard UI_AutoPilot_Done_In;
        Storyboard UI_AutoPilot_Done_Out;

        #endregion

        public Game game;
        public Map.Mapping mapping = new Map.Mapping();
        public Properties.Settings Configuration = Properties.Settings.Default;

        /// <summary>
        /// If the game could not be positioned onto the second monitor, this returns false.
        /// </summary>
        public event EventHandler<bool> SecondaryScreenPositioning;

        public int secondaryScreenID = 0;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            game.GameStatusChanged += Game_GameStatusChanged;
            game.IsGameReadyChanged += Game_IsGameReadyChanged;
            game.IsGameDebugEnabledChanged += Game_IsGameDebugEnabledChanged;
            game.IsGameCursorEnabledChanged += Game_IsGameCursorEnabledChanged;
            game.IsLocationsOpacityEnabledChanged += Game_IsLocationsOpacityEnabledChanged;
            game.IsGameAutoPhaseChanged += Game_IsAutoPhaseChanged;
            game.IsGameLangDominationSlovakChanged += Game_IsGameLangDominationSlovakChanged;

            // hide UI on start
            uiGrid.Visibility = Visibility.Hidden;
            UI_ChooseLocation.Visibility = Visibility.Hidden;
            UI_CorrectAnswer.Visibility = Visibility.Hidden;
            UI_WrongAnswer.Visibility = Visibility.Hidden;
            map_targetlocationPanel.Opacity = 0;
            autoPilot_Restarting_Block.Visibility = Visibility.Hidden;


            // The game is loading...
            game.ChangeGameStatus(Game.GameStatus.Loading);

            // Load stuff
            await LoadGame();
        }

        private void Game_IsGameLangDominationSlovakChanged(object sender, bool e)
        {
            mapping.ChangeLangDomination(e);
        }

        public void Game_IsAutoPhaseChanged(object sender, bool e)
        {
            if (e)
            {
                AutoPilot_Start();
                autopilot_CountBlock.Visibility = Visibility.Visible;
            }
            else
            {
                AutoPilot_Stop();
                autopilot_CountBlock.Visibility = Visibility.Collapsed;
            }
        }

        public Random random = new Random();

        public async void AutoPilot_Start()
        {
            game.IsGameReady = false;

            autopilot_correctcounter = 0;
            autopilot_wrongcounter = 0;
            autopilot_wronglist.Clear();
            autopilot_correctlist.Clear();

            bool shouldShowRestartScreen = false;

            if (mapping.Locations_auto_temp.Count > 0)
                shouldShowRestartScreen = true;

            if (UI_AutoPilot_Done.Visibility == Visibility.Visible)
            {
                shouldShowRestartScreen = true;

                UI_AutoPilot_Done_Out.Begin();
                await Task.Delay(TimeSpan.FromSeconds(.3));
            }

            mapping.AutoPilot_PrepareTempList();

            if (shouldShowRestartScreen)
            {
                autoPilot_Restarting_Block.Visibility = Visibility.Visible;
                ChangeProgressUI(true);
                await Task.Delay(TimeSpan.FromSeconds(1));
                ChangeProgressUI(false);
                autoPilot_Restarting_Block.Visibility = Visibility.Hidden;
            }

            await ShowAutoPilotStartUI();
            game.IsGameReady = true;

            AutoPilot_ChooseNextTarget();
        }

        public void AutoPilot_ChooseNextTarget()
        {
            if (mapping.Locations_auto_temp.Count == 0)
            {
                ShowAutoPilotDoneUI();
                return;
            }
            mapping.ChangeTargetLocation(mapping.Locations_auto_temp[random.Next(0, mapping.Locations_auto_temp.Count - 1)]);

            AutoPilot_RefreshDebug();
        }

        public void AutoPilot_RefreshDebug()
        {
            autopilot_CountBlock.Text = "autopilot remaining: " + mapping.Locations_auto_temp.Count.ToString();
        }

        public void AutoPilot_Stop()
        {
            mapping.Locations_auto_temp.Clear();
            mapping.ChangeTargetLocation("");
        }

        public void Game_IsLocationsOpacityEnabledChanged(object sender, bool e)
        {
            if (e)
                mapping.locations_canvas.Opacity = 0.43;
            else
                mapping.locations_canvas.Opacity = 1;
        }

        public void Game_IsGameCursorEnabledChanged(object sender, bool e)
        {
            if (e)
                this.Cursor = Cursors.Arrow;
            else
                this.Cursor = Cursors.None;
        }

        public void Game_IsGameDebugEnabledChanged(object sender, bool e)
        {
            if (e)
                debugPanel.Visibility = Visibility.Visible;
            else
                debugPanel.Visibility = Visibility.Collapsed;
        }

        private void Game_GameStatusChanged(object sender, Game.GameStatus e)
        {
            gamestatus_Block.Text = "game status: " + e.ToString();
        }

        private void Game_IsGameReadyChanged(object sender, bool e)
        {
            if (!e)
                map_touchblocker.Visibility = Visibility.Visible;
            else
                map_touchblocker.Visibility = Visibility.Collapsed;

            gameready_Block.Text = "is game ready: " + e.ToString();
        }

        public async Task LoadGame()
        {
            ChangeProgressUI(true);

            // We want the game on the secondary screen (projector)
            PositionWindowToSecondaryScreen(secondaryScreenID);

            WindowState = WindowState.Maximized;

            // Load in the mapping
            map_frame.Content = mapping;

            // Hook up events
            mapping.Result_Received += Mapping_Result_Received;
            mapping.TargetLocationChanged += Mapping_TargetLocationChanged;

            // Load the map into the mapping
            await mapping.LoadMap(Configuration.MapName);

            // Ready!
            game.ChangeGameStatus(Game.GameStatus.Waiting);
            game.IsGameReady = false;

            await Task.Delay(TimeSpan.FromSeconds(1));

            ChangeProgressUI(false);
        }

        void HideTargetLocationIndicator()
        {
            DoubleAnimation anim_out = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
            map_targetlocationPanel.BeginAnimation(OpacityProperty, anim_out);
        }

        void ShowTargetLocationIndicator()
        {
            DoubleAnimation anim_in = new DoubleAnimation(1, TimeSpan.FromSeconds(.3));
            map_targetlocationPanel.BeginAnimation(OpacityProperty, anim_in);
        }

        private async void Mapping_TargetLocationChanged(object sender, string e)
        {
            targetLocationBlock.Text = e;
            map_targetlocationBlock.Text = e;

            HideTargetLocationIndicator();

            if (e == "")
                game.ChangeGameStatus(Game.GameStatus.Waiting);
            else
            {
                ChangeProgressUI(false);
                await ShowChooseUI();
                game.ChangeGameStatus(Game.GameStatus.Ingame);
                game.IsGameReady = true;

                await Task.Delay(TimeSpan.FromSeconds(.3));

                ShowTargetLocationIndicator();
            }
        }

        TimeSpan chooseUI_duration = TimeSpan.FromSeconds(3);
        TimeSpan autopilot_chooseUI_duration = TimeSpan.FromSeconds(2);

        public async Task ShowChooseUI()
        {
            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            mapping.ChangeAllLocationItemStates(Map.LocationItem.Mode.Hide);

            UI_In.Begin();

            UI_Choose_In.Begin();

            if (!game.IsGameAutoPhase)
                await Task.Delay(chooseUI_duration);
            else
                await Task.Delay(autopilot_chooseUI_duration);

            UI_Choose_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));

            UI_Out.Begin();
        }

        public async Task ShowCorrectUI()
        {
            game.IsGameReady = false;

            HideTargetLocationIndicator();

            mapping.ChangeLocationItemState(mapping.TargetLocation, Map.LocationItem.Mode.ShowEllipseAndText);

            await Task.Delay(TimeSpan.FromSeconds(.8));

            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            TimeSpan duration = TimeSpan.FromSeconds(3);

            UI_In.Begin();

            UI_Correct_In.Begin();

            await Task.Delay(duration);

            UI_Correct_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));

            UI_Out.Begin();

            game.ChangeGameStatus(Game.GameStatus.Waiting);
        }

        public async Task ShowWrongUI(Map.Mapping.Result e)
        {
            game.IsGameReady = false;

            HideTargetLocationIndicator();

            if (e != null)
            {
                if (e.clickLocation != null & e.clickLocation != "")
                {
                    mapping.ChangeLocationItemState(e.clickLocation, Map.LocationItem.Mode.ShowEllipseAndText_Error);
                    await Task.Delay(TimeSpan.FromSeconds(.8));
                }
            }

            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            TimeSpan duration;

            if (!game.IsGameAutoPhase)
                duration = TimeSpan.FromSeconds(3);
            else
                duration = autopilot_chooseUI_duration;

            UI_In.Begin();

            UI_Wrong_In.Begin();

            await Task.Delay(duration);

            UI_Wrong_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));

            UI_Out.Begin();

            bool shouldShowCorrection = true;

            if (e != null & !game.IsGameAutoPhase) // if the game is played normally, show the correction.
                shouldShowCorrection = true;
            if (game.IsGameAutoPhase & game.AutoPhaseShowsCorrectionAfterWrongAnswer) // if the game is autopiloting, and correction is on, show correction
                shouldShowCorrection = true;
            if (game.IsGameAutoPhase & !game.AutoPhaseShowsCorrectionAfterWrongAnswer) // if the game is autopiloting, and correction is off, don't show correction
                shouldShowCorrection = false;

            if (shouldShowCorrection)
                mapping.ChangeLocationItemState(mapping.TargetLocation, Map.LocationItem.Mode.ShowEllipseAndText);

            game.ChangeGameStatus(Game.GameStatus.Waiting);
        }

        public async Task ShowAutoPilotStartUI()
        {
            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            mapping.ChangeAllLocationItemStates(Map.LocationItem.Mode.ShowEllipseAndText);

            TimeSpan duration = TimeSpan.FromSeconds(5);

            UI_In.Begin();
            UI_AutoPilot_Start_In.Begin();

            await Task.Delay(duration);

            UI_AutoPilot_Start_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));

            mapping.ChangeAllLocationItemStates(Map.LocationItem.Mode.Hide);

            //UI_Out.Begin();
        }

        public async void ShowAutoPilotDoneUI()
        {
            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            mapping.ChangeAllLocationItemStates(Map.LocationItem.Mode.Hide);

            UI_In.Begin();
            UI_AutoPilot_Done_In.Begin();

            UI_AutoPilot_Done_CorrectBlock.Text = autopilot_correctcounter.ToString();
            UI_AutoPilot_Done_WrongBlock.Text = autopilot_wrongcounter.ToString();

            await Task.Delay(TimeSpan.FromSeconds(1));

            foreach (string location in mapping.Locations_auto)
            {
                foreach (string err_location in autopilot_wronglist)
                {
                    if (err_location == location)
                        mapping.ChangeLocationItemState(location, Map.LocationItem.Mode.ShowEllipseAndText_Error);
                }
                foreach (string corr_location in autopilot_correctlist)
                    if (corr_location == location)
                        mapping.ChangeLocationItemState(location, Map.LocationItem.Mode.ShowEllipseAndText);
            }
        }

        public async Task HideAutoPilotDoneUI()
        {
            // change game status
            game.ChangeGameStatus(Game.GameStatus.UI);

            UI_AutoPilot_Done_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));

            UI_Out.Begin();

            await Task.Delay(TimeSpan.FromSeconds(.3));
        }

        int autopilot_correctcounter;
        int autopilot_wrongcounter;
        List<string> autopilot_correctlist = new List<string>();
        List<string> autopilot_wronglist = new List<string>();

        private async void Mapping_Result_Received(object sender, Map.Mapping.Result e)
        {
            if (!game.IsGameAutoPhase)
            {
                if (e.isCorrect)
                    await ShowCorrectUI();
                else if (!e.isCorrect & e.clickLocation != null)
                    await ShowWrongUI(e);
                else if (!e.isCorrect & e.clickLocation == null)
                    await ShowWrongUI(e);
            }
            else
            {
                // remove the current target location so it can't pop up again
                mapping.Locations_auto_temp.Remove(e.targetLocation);

                // update debug
                AutoPilot_RefreshDebug();

                if (e.isCorrect)
                {
                    mapping.ChangeLocationItemState(e.targetLocation, Map.LocationItem.Mode.ShowEllipseAndText);

                    autopilot_correctcounter++;
                    autopilot_correctlist.Add(e.targetLocation);

                    await Task.Delay(TimeSpan.FromSeconds(1));
                }
                else if (!e.isCorrect & e.clickLocation != null)
                {
                    /*
                    mapping.ChangeLocationItemState(e.clickLocation, Map.LocationItem.Mode.ShowEllipseAndText);
                    await Task.Delay(TimeSpan.FromSeconds(0.5));
                    mapping.ChangeLocationItemState(e.targetLocation, Map.LocationItem.Mode.ShowEllipseAndText);
                    await Task.Delay(autopilot_locationduration);
                    mapping.ChangeLocationItemState(mapping.TargetLocation, Map.LocationItem.Mode.Hide);
                    */

                    await ShowWrongUI(e);

                    autopilot_wrongcounter++;
                    autopilot_wronglist.Add(e.targetLocation);

                    if (game.AutoPhaseShowsCorrectionAfterWrongAnswer)
                        await Task.Delay(TimeSpan.FromSeconds(2));
                }
                else
                {
                    await ShowWrongUI(e);

                    autopilot_wrongcounter++;
                    autopilot_wronglist.Add(e.targetLocation);

                    if (game.AutoPhaseShowsCorrectionAfterWrongAnswer)
                        await Task.Delay(TimeSpan.FromSeconds(2));
                }

                AutoPilot_ChooseNextTarget();
            }
        }

        public async void ChangeProgressUI(bool show)
        {
            Visibility vis;

            if (show)
                vis = Visibility.Visible;
            else
                vis = Visibility.Collapsed;

            if (show)
            {
                DoubleAnimation anim_in = new DoubleAnimation(1, TimeSpan.FromSeconds(.3));
                progressGrid.BeginAnimation(Grid.OpacityProperty, anim_in);

                progressGrid.Visibility = vis;
                progressArc.Visibility = vis;
            }

            if (!show)
            {
                DoubleAnimation anim_out = new DoubleAnimation(0, TimeSpan.FromSeconds(.3));
                progressGrid.BeginAnimation(Grid.OpacityProperty, anim_out);

                await Task.Delay(TimeSpan.FromSeconds(.3));
                progressGrid.Visibility = vis;
                progressArc.Visibility = vis;
            }
        }

        public void PositionWindowToSecondaryScreen(int screenid = 0)
        {
            if (System.Windows.Forms.Screen.AllScreens.Length > 1)
            {
                this.WindowState = WindowState.Normal;

                System.Windows.Forms.Screen screen = System.Windows.Forms.Screen.AllScreens[screenid];

                System.Drawing.Rectangle screenarea = screen.WorkingArea;
                window.Top = screenarea.Top;
                window.Left = screenarea.Left;
                window.Width = screenarea.Width;
                window.Height = screenarea.Height;

                SecondaryScreenPositioning?.Invoke(null, true);

                this.WindowState = WindowState.Maximized;
            }
            else
                SecondaryScreenPositioning?.Invoke(null, false);
        }
    }
}