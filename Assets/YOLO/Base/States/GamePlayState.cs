//using Game.Config;
using Game.Core;
using Game.Level;
//using Game.Level;
//using Game.Level.Employee;
//using Game.Level.Employee.State;
//using Game.Level.Entity;
//using Game.Managers;
//using Game.UI;
using Injection;
using System;
using System.Collections.Generic;
using UnityEngine;
//using Game.UI.Hud;
//using Game.Level.Inventory;
//using Game.Level.Modules.Customer;
//using Game.Level.Modules.GOPoolModule;
//using Game.Level.Modules.DeliverModule;
//using Game.Level.Modules.Employee;
//using Game.Level.Modules.ExpandStore;
//using Game.Level.Modules.TutSystemModule;
//using Game.Level.Modules.RewardFreeModule;
//using Game.Level.Modules.OfferAds;
//using Game.Level.Modules.DecorTextureModule;
//using Game.Level.Player.PlayerState;
//using Game.Level.Unit;
//using Game.Modules.UISpritesModule;
//using Game.Modules.UINotificationModule;
//using Game.Level.Modules.OpenCloseSignModule;
//using Random = UnityEngine.Random;
//using static SoundDataScriptableObject;

namespace Game.States
{
    public sealed class GamePlayState : GameState
    {
        [Inject] private Injector _injector;
        [Inject] private Context _context;
        [Inject] private RoomSort2DGameView _gameView;
        //[Inject] private HudManager _hudManager;
        //[Inject] private GameConfig _config;
        //[Inject] private LevelView _levelView;
        [Inject] private Timer _timer;

        public GamePlayState()
        {
        }

        public override void Initialize()
        {
          

            // _gameManager.CheckStoreEmpty();
        }
        private void InitItemViews()
        {
           
        }

        public override void Dispose()
        {
            //_gameView.CameraController.enabled = false;
            _timer.Application_Pause -= OnApplication_Pause;
            _timer.Application_Focus -= OnApplication_Focus;
            _timer.Application_Quit -= OnApplication_Quit;
    
            //_gameManager.Dispose();

            //_context.Uninstall(_gameManager);
        }

       

        private void OnApplication_Quit()
        {
            ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Quit");
            //_gameManager.Model.SaveGameData();
        }

        private void OnApplication_Focus(bool focusStatus)
        {
#if UNITY_EDITOR
            if (focusStatus) return;

            ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Focus: " + focusStatus);
            //_gameManager.Model.SaveGameData();
#endif
        }

        private void OnApplication_Pause(bool pauseStatus)
        {
            if (pauseStatus == false) return;

            ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Pause: " + pauseStatus);
            //_gameManager.Model.SaveGameData();
        }
    }
}