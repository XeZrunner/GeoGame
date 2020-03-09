using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeoGame
{
    public class Game
    {
        public enum GameStatus
        {
            None,
            Starting,
            Loading,
            Waiting,
            Ingame,
            UI,
            Paused,
            Miscellaneous,
            Error
        }

        public string MapName;

        bool _isGameReady = false;
        bool _isGameOnSecondaryScreen;
        bool _IsGameDebugEnabled = true;
        bool _IsGameCursorEnabled = true;
        bool _IsLocationsOpacityEnabled = false;
        bool _IsGameAutoPhase = false;
        bool _IsGameLangDominationSlovak = false;

        public bool IsGameLangDominationSlovak
        {
            get { return _IsGameLangDominationSlovak; }
            set 
            {
                _IsGameLangDominationSlovak = value;
                IsGameLangDominationSlovakChanged.Invoke(null, value);
            }
        }

        public bool AutoPhaseShowsCorrectionAfterWrongAnswer { get; set; } = true;

        public bool IsGameAutoPhase
        {
            get { return _IsGameAutoPhase; }
            set
            {
                _IsGameAutoPhase = value;
                IsGameAutoPhaseChanged?.Invoke(null, value);
            }
        }

        /// <summary>
        /// Controls whether the game can be played.
        /// </summary>
        public bool IsGameReady
        {
            get { return _isGameReady; }
            set
            {
                _isGameReady = value;
                IsGameReadyChanged?.Invoke(this, value);
            }
        }

        public bool IsGameOnSecondaryScreen
        {
            get { return _isGameOnSecondaryScreen; }
            set
            {
                _isGameOnSecondaryScreen = value;
                IsGameOnSecondaryScreenChanged?.Invoke(this, value);
            }
        }

        public bool IsGameDebugEnabled
        {
            get { return _IsGameDebugEnabled; }
            set
            {
                _IsGameDebugEnabled = value;
                IsGameDebugEnabledChanged?.Invoke(null, value);
            }
        }

        public bool IsGameCursorEnabled
        {
            get { return _IsGameCursorEnabled; }
            set
            {
                _IsGameCursorEnabled = value;
                IsGameCursorEnabledChanged?.Invoke(null, value);
            }
        }

        public bool IsLocationsOpacityEnabled
        {
            get { return _IsLocationsOpacityEnabled; }
            set
            {
                _IsLocationsOpacityEnabled = value;
                IsLocationsOpacityEnabledChanged?.Invoke(null, value);
            }
        }

        public GameStatus CurrentGameStatus = GameStatus.None;
        public event EventHandler<GameStatus> GameStatusChanged;
        public event EventHandler<bool> IsGameReadyChanged;
        public event EventHandler<bool> IsGameOnSecondaryScreenChanged;
        public event EventHandler<bool> IsGameDebugEnabledChanged;
        public event EventHandler<bool> IsGameCursorEnabledChanged;
        public event EventHandler<bool> IsLocationsOpacityEnabledChanged;
        public event EventHandler<bool> IsGameAutoPhaseChanged;
        public event EventHandler<bool> IsGameLangDominationSlovakChanged;

        public void ChangeGameStatus(GameStatus newGameStatus)
        {
            CurrentGameStatus = newGameStatus;
            GameStatusChanged?.Invoke(this, newGameStatus);
        }
    }
}
