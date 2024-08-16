//using Cysharp.Threading.Tasks;
//using Game.Config;
//using Game.Domain;
//using Game.Managers;
//using Game.UI.Hud;
using Cysharp.Threading.Tasks;
using Injection;
using UnityEngine.SceneManagement;
//using YOGames.Firebase;
//using YOGames.MAX;

namespace Game.States
{
    public class GameLoadLevelState : GameState
    {
        [Inject] protected GameStateManager _gameStateManager;
        //[Inject] protected GameConfig _config;
        [Inject] protected Context _context;
        //[Inject] protected HudManager _hudManager;

        private int _level;
        private bool _isLoaded;
        public bool IsLoaded => _isLoaded;

        //SplashScreenHudMediator _splashScreenHudMediator;
        private RoomSort2DGameManager _game2DManager;
        private RoomSort3DGameManager _game3DManager;
        public override void Initialize()
        {
            _isLoaded = false;
            //_splashScreenHudMediator = _hudManager.ShowAdditional<SplashScreenHudMediator>(new []{this});

            _level = 1;
            
            if (_level < 1) _level = 1;
            else if (_level >= SceneManager.sceneCountInBuildSettings)
            {
                _level = SceneManager.sceneCountInBuildSettings - 1;
            }
            
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            for (int i = 1; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (SceneManager.GetSceneByBuildIndex(i).isLoaded)
                {
                    SceneManager.UnloadSceneAsync(i);
                }
            }
            LoadScene();
        }

        public override void Dispose()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public virtual void OnSceneLoaded(Scene scene, LoadSceneMode arg)
        {
            InitGameManager(scene);

            ShazamLogger.Log("GameLoadLevelState OnSceneLoaded");
            _gameStateManager.SwitchToState<GamePlay2DState>();


            UniTask.Create(async () =>
            {
                _isLoaded = true;
                ShazamLogger.Log("GameLoadLevelState Firebase Init Finished !");
            }).Forget();

            _isLoaded = true;
        }

        private void InitGameManager(Scene scene)
        {
            RoomSort3DGameView Game3DView = null;
            RoomSort2DGameView Game2DView = null;
            var sceneObjects = scene.GetRootGameObjects();
            foreach (var sceneObject in sceneObjects)
            {
                if (sceneObject.GetComponent<RoomSort3DGameView>() != null)
                {
                    Game3DView = sceneObject.GetComponent<RoomSort3DGameView>();
                }
                if (sceneObject.GetComponent<RoomSort2DGameView>() != null)
                {
                    Game2DView = sceneObject.GetComponent<RoomSort2DGameView>();
                }
                if (null != Game3DView && null != Game2DView)
                    break;
            }
            _game2DManager = new RoomSort2DGameManager(Game2DView);
            _game3DManager = new RoomSort3DGameManager();
            _context.Install(Game3DView);
            _context.Install(Game2DView);
            _context.Install(_game2DManager);
            _context.Install(_game3DManager);
            _context.ApplyInstall();
        }
        public virtual void LoadScene()
        {

            //load scene async and get progress
            ShazamLogger.Log($"GameLoadLevelState LoadLevel StartLoadingScene: {_level}");
            var asyncOperation = SceneManager.LoadSceneAsync(_level, LoadSceneMode.Additive);
            UniTask.Create(async () =>
            {
                while (!asyncOperation.isDone)
                {
                    //_splashScreenHudMediator.UpdateLoadSceneProgression(asyncOperation.progress);
                    await UniTask.Delay(100);
                }
                ShazamLogger.Log($"GameLoadLevelState LoadLevel Finished: {_level}");
                //LoadingScreenContainer.Instance.CallBackFinishLoadScene();
            }).Forget();

        }
    }
}