using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using GeoGame.Windows;
using XeZrunner.UI.Theming;
using XeZrunner.UI.Controls;
using XeZrunner.UI.Controls.Buttons;
using System.Windows.Media.Animation;
using XeZrunner.UI.Popups;
using System.Windows.Controls;

namespace GeoGame.Windows
{
    public partial class ControllerWindow : Window
    {
        public ControllerWindow()
        {
            InitializeComponent();

            // Exception handling
            Application.Current.Dispatcher.UnhandledException += Dispatcher_UnhandledException;
        }

        GameWindow gameWindow;
        Game game = new Game();
        ThemeManager ThemeManager = new ThemeManager(Application.Current.Resources);
        XeZrunner.UI.Configuration.Config XZUI_Config = XeZrunner.UI.Configuration.Config.Default;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            game.GameStatusChanged += Game_GameStatusChanged;
            ThemeManager.ConfigChanged += ThemeManager_ConfigChanged;

            // set default theming
            ThemeManager.Config_SetAccent(ThemeManager.Accent.Orange);
            XZUI_Config.controlfx = "Reveal"; XZUI_Config.Save();

            // check for the secondary screen
            if (System.Windows.Forms.Screen.AllScreens.Length < 2)
                GameWindow_SecondaryScreenPositioning(null, false);
            else
                GameWindow_SecondaryScreenPositioning(null, true);
        }

        #region Logic

        private void GameWindow_SecondaryScreenPositioning(object sender, bool e)
        {
            if (!e)
            {
                game.IsGameOnSecondaryScreen = false; // not on secondary screen

                NoSecondaryScreen_Block.Visibility = Visibility.Visible;
                NoSecondaryScreen_Rectangle.Visibility = Visibility.Visible;
                Grid.SetRowSpan(NoSecondaryScreen_Rectangle, 2);

                titleBar.SetResourceReference(XeZrunner.UI.Controls.Window_components.TitlebarControl.ForegroundProperty, "White");
                titleBar.Theme = ThemeManager.Theme.Dark;
            }
            else
            {
                game.IsGameOnSecondaryScreen = true; // the game is on the secondary screen

                NoSecondaryScreen_Block.Visibility = Visibility.Collapsed;
                NoSecondaryScreen_Rectangle.Visibility = Visibility.Collapsed;
                Grid.SetRowSpan(NoSecondaryScreen_Rectangle, 1);

                titleBar.SetResourceReference(XeZrunner.UI.Controls.Window_components.TitlebarControl.ForegroundProperty, "Foreground");
                titleBar.Theme = ThemeManager.CurrentTheme;
            }
        }

        private void Game_GameStatusChanged(object sender, Game.GameStatus e)
        {
            gameStatusText.Text = "Játék állapota: " + e.ToString();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // When we close the controller, quit the game!
            if (gameWindow != null)
                gameWindow.Close();
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            this.Hide();
        }

        #endregion

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Starting!
            game.ChangeGameStatus(Game.GameStatus.Starting);

            gameWindow = new GameWindow();
            gameWindow.secondaryScreenID = int.Parse(secondarymonitorTextBox.Text);
            gameWindow.mapping.LocationItemStatesChanged += Mapping_LocationItemStatesChanged;
            gameWindow.SecondaryScreenPositioning += GameWindow_SecondaryScreenPositioning;

            gameWindow.game = game;

            gameWindow.Show();

            // debug check

            await Task.Delay(TimeSpan.FromSeconds(1));

            gameWindow.Game_IsGameDebugEnabledChanged(null, game.IsGameDebugEnabled);
            gameWindow.Game_IsGameCursorEnabledChanged(null, game.IsGameCursorEnabled);
            gameWindow.Game_IsLocationsOpacityEnabledChanged(null, game.IsLocationsOpacityEnabled);
            gameWindow.Game_IsAutoPhaseChanged(null, game.IsGameAutoPhase);

            // enable controls
            startButton.IsEnabled = false;
            positionButton.IsEnabled = true;
            chooselocationButton.IsEnabled = true;
            locationitem_debugstackpanel.IsEnabled = true;
            gameWindow_UIDebugPanel.IsEnabled = true;
            game_progressUIDebugPanel.IsEnabled = true;
            game_autophaseResetButton.IsEnabled = true;
            game_autopilotDebugRemoveButton.IsEnabled = true;
            slovakSwitch.IsEnabled = true;
        }

        private void Mapping_LocationItemStatesChanged(object sender, int e)
        {
            int counter = 0;
            foreach (XeZrunner.UI.Controls.RadioButton button in locationitem_debugstackpanel.Children)
            {
                if (e == -1)
                    button.DeactivateButton();
                else
                {
                    if (counter == e)
                        button.ActivateButton();
                    else
                        button.DeactivateButton();
                }

                counter++;
            }
        }

        private async void ChooselocationButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog() { Title = "Település kiválasztása", PrimaryButtonText = "Mégse" };
            StackPanel panel = new StackPanel();

            TextBlock block0 = new TextBlock() { Text = "A következő település játék közben is kiválasztható.\nA jelenlegi települést nem változik." };
            //panel.Children.Add(block0);

            // Add buttons of locations
            List<string> Locations;

            if (!Keyboard.IsKeyDown(Key.LeftShift))
            {
                //Locations = gameWindow.mapping.Locations;
                Locations = gameWindow.mapping.GetAllLocations();
            }                
            else
            {
                Locations = gameWindow.mapping.Locations_auto;
                dialog.Title += " (autopilóta)";
            }

            foreach (string loc in Locations)
            {
                XeZrunner.UI.Controls.Buttons.Button button = new XeZrunner.UI.Controls.Buttons.Button() { Text = loc, Background = Brushes.Transparent };
                button.Click += (s, ev) => { gameWindow.mapping.ChangeTargetLocation(button.Text); dialog.CloseDialog(); };
                panel.Children.Add(button);
            }

            dialog.Content = panel;

            if (await dialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Primary)
                return;
        }

        private void Game_loadingShowButton_Click(object sender, RoutedEventArgs e)
        {
            gameWindow.ChangeProgressUI(true);
        }

        private void Game_loadingHideButton_Click(object sender, RoutedEventArgs e)
        {
            gameWindow.ChangeProgressUI(false);
        }

        private void Locitem_DEBUG_Click(object sender, EventArgs e)
        {
            int counter = 0;
            foreach (XeZrunner.UI.Controls.RadioButton button in locationitem_debugstackpanel.Children)
            {
                if (button.IsActive)
                {
                    gameWindow.mapping.ChangeAllLocationItemStates((Map.LocationItem.Mode)counter);
                    return;
                }

                counter++;
            }
        }


        #region Xesign UI debug 

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (Keyboard.IsKeyDown(Key.Q))
                    ThemeManager.Config_SetTheme(ThemeManager.Theme.Light);
                if (Keyboard.IsKeyDown(Key.E))
                    ThemeManager.Config_SetTheme(ThemeManager.Theme.Dark);

                if (Keyboard.IsKeyDown(Key.D0))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Teal);
                if (Keyboard.IsKeyDown(Key.D1))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Blue);
                if (Keyboard.IsKeyDown(Key.D2))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Green);
                if (Keyboard.IsKeyDown(Key.D3))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Orange);
                if (Keyboard.IsKeyDown(Key.D4))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Pink);
                if (Keyboard.IsKeyDown(Key.D5))
                    ThemeManager.Config_SetAccent(ThemeManager.Accent.Purple);

                //titleBar.Theme = ThemeManager.CurrentTheme;
            }
        }

        RenderTargetBitmap Screenshot(FrameworkElement element)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(element);

            //await Task.Delay(1);

            return renderTargetBitmap;
        }

        public async void ThemeManager_ConfigChanged(object sender, EventArgs e)
        {
            themechangeImage.Source = Screenshot(this);

            themechangeImage.Visibility = Visibility.Visible;

            DoubleAnimation anim = new DoubleAnimation(1, 0, TimeSpan.FromSeconds(.3));
            themechangeImage.BeginAnimation(OpacityProperty, anim);

            await Task.Delay(TimeSpan.FromSeconds(.3));
            themechangeImage.Visibility = Visibility.Hidden;
        }

        #endregion

        private void PositionButton_Click(object sender, RoutedEventArgs e)
        {
            gameWindow.PositionWindowToSecondaryScreen(int.Parse(secondarymonitorTextBox.Text));
        }

        private void Game_debugCheckBox_IsActiveChanged(object sender, EventArgs e)
        {
            game.IsGameDebugEnabled = game_debugCheckBox.IsActive;
        }

        private void Game_cursorCheckBox_IsActiveChanged(object sender, EventArgs e)
        {
            game.IsGameCursorEnabled = game_cursorCheckBox.IsActive;
        }

        private void Game_locationsOpacityCheckBox_IsActiveChanged(object sender, EventArgs e)
        {
            game.IsLocationsOpacityEnabled = game_locationsOpacityCheckBox.IsActive;
        }

        private void Ui_lightButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Config_SetTheme(ThemeManager.Theme.Light);
        }

        private void Ui_darkButton_Click(object sender, RoutedEventArgs e)
        {
            ThemeManager.Config_SetTheme(ThemeManager.Theme.Dark);
        }

        private async void Game_chooselocationDebugButton_Click(object sender, RoutedEventArgs e)
        {
            await gameWindow.ShowChooseUI();
        }

        private async void Game_correctDebugButton_Click(object sender, RoutedEventArgs e)
        {
            await gameWindow.ShowCorrectUI();
        }

        private async void Game_wrongDebugButton_Click(object sender, RoutedEventArgs e)
        {
            await gameWindow.ShowWrongUI(null);
        }

        private void Ui_cntdlgButton_Click(object sender, RoutedEventArgs e)
        {
            dialogHost.TextContentDialog("ContentDialog testing", "github.com/XesignSoftware\n\nXesign UI 2018/2019 v2.0 dev\nPrerelease channel: dev-edge_00", false, "OK, got it!");
        }

        bool game_autopilottoggleConfirm_Handled = false;

        private async void game_autophaseCheckBox_IsActiveChanged(object sender, EventArgs e)
        {
            if (game_autopilottoggleConfirm_Handled)
                return;

            if (game.IsGameAutoPhase)
            {
                game_autopilottoggleConfirm_Handled = true;
                ContentDialog dialog = new ContentDialog() { Title = "Autopilóta kikapcsolása", Content = "Biztos hogy meg akarod szakítani autopilótát?", SecondaryButtonText = "Igen!", PrimaryButtonText = "Nem..." };
                if (await dialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Secondary)
                {
                    game_autopilottoggleConfirm_Handled = false;
                    game.IsGameAutoPhase = game_autophaseCheckBox.IsActive;
                }
                else
                {
                    game_autophaseCheckBox.IsActive = true;
                    game_autopilottoggleConfirm_Handled = false;
                }
            }
            else
                game.IsGameAutoPhase = game_autophaseCheckBox.IsActive;
        }

        private async void game_autophaseResetButton_Click(object sender, RoutedEventArgs e)
        {
            ContentDialog dialog = new ContentDialog() { Title = "Autopilóta újraindítása", Content = "Biztos hogy újra szeretnéd indítani az autopilótát?", SecondaryButtonText = "Igen!", PrimaryButtonText = "Nem..." };
            if (await dialogHost.ShowDialogAsync(dialog) == ContentDialogHost.ContentDialogResult.Secondary)
            {
                if (game.IsGameAutoPhase)
                    gameWindow.AutoPilot_Start();
            }
        }

        private void game_autopilotDebugRemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (game.IsGameAutoPhase & gameWindow.mapping.Locations_auto_temp.Count > 2)
            {
                gameWindow.mapping.Locations_auto_temp.RemoveRange(0, gameWindow.mapping.Locations_auto_temp.Count - 1);
                gameWindow.AutoPilot_RefreshDebug();
            }
        }

        private void game_autopilotShowCorrectionCheckBox_IsActiveChanged(object sender, EventArgs e)
        {
            game.AutoPhaseShowsCorrectionAfterWrongAnswer = game_autopilotShowCorrectionCheckBox.IsActive;
        }

        private void slovakSwitch_IsActiveChanged(object sender, EventArgs e)
        {
            game.IsGameLangDominationSlovak = slovakSwitch.IsActive;
        }
    }
}
