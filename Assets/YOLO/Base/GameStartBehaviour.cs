using Game.Core;
using Game.States;
using Injection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStartBehaviour : MonoBehaviour
{
    private Timer _timer;

    public Context Context { get; private set; }
    private void Start()
    {

        //Aplication setting
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        Application.runInBackground = true;

        _timer = new Timer();

        // Install context
        var context = new Context();
        context.Install(
              new Injector(context),
              new GameStateManager(),
              new PathFindingService()
          );
        context.Install(GetComponents<Component>());
        context.Install(_timer);
        context.ApplyInstall();

        context.Get<GameStateManager>().SwitchToState(typeof(GameInitializeState));

        Context = context;
    }
    private void Update()
    {
        _timer.Update();
    }

    private void LateUpdate()
    {
        _timer.LateUpdate();
    }

    private void FixedUpdate()
    {
        _timer.FixedUpdate();
    }

    private void OnApplicationQuit()
    {
        if (_timer != null)
            _timer.OnApplicationQuit();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (_timer != null)
            _timer.OnApplicationPause(pauseStatus);
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (_timer != null)
            _timer.OnApplicationFocus(hasFocus);
    }
}
