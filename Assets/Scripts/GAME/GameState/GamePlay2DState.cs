using Game.Level;
using Game.States;
using Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using System;

public class GamePlay2DState : GameState
{
    [Inject] private Injector _injector;
    [Inject] private Context _context;
    //[Inject] private HudManager _hudManager;
    //[Inject] private GameConfig _config;
    //[Inject] private LevelView _levelView;
    [Inject] private Timer _timer;
    [Inject] private RoomSort2DGameManager _gameManager;
    [Inject] private RoomSort2DGameView _gameView;

    public GamePlay2DState()
    {
    }
    public override void Initialize()
    {
        //_timer.POST_TICK += PostTick;
        //_timer.FIXED_TICK += FixedTick;
        _gameManager.Initialize();
    }

    private void PostTick()
    {        
    }

    private void FixedTick()
    {
    }

    public override void Dispose()
    {
        //_gameView.CameraController.enabled = false;
        _timer.Application_Pause -= OnApplication_Pause;
        _timer.Application_Focus -= OnApplication_Focus;
        _timer.Application_Quit -= OnApplication_Quit;

        _gameManager.Dispose();

        //_context.Uninstall(_gameManager);
    }

 
    private void OnApplication_Quit()
    {
        ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Quit");
        _gameManager.SaveGameSystem.SaveGameData();
    }

    private void OnApplication_Focus(bool focusStatus)
    {
#if UNITY_EDITOR
        if (focusStatus) return;

        ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Focus: " + focusStatus);
        _gameManager.SaveGameSystem.SaveGameData();
#endif
    }

    private void OnApplication_Pause(bool pauseStatus)
    {
        if (pauseStatus == false) return;

        ShazamLogger.LogTemporaryChannel("SaveGame", "GamePlayState.OnApplication_Pause: " + pauseStatus);
        _gameManager.SaveGameSystem.SaveGameData();
    }
}
