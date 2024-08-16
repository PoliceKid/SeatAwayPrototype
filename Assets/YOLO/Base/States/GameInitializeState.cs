//using Game.Config;
//using Game.Tutorial;
using Game.Level;
using Injection;
using System;
using UnityEngine;
using Game.Core;
using System.Collections.Generic;

namespace Game.States
{
    public partial class GameInitializeState : GameState
    {
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private Context _context;
        [Inject] private Injector _injector;

        private readonly List<Module> _levelModules;

        public override void Initialize()
        {
            //Load config here
            //var config = GameConfig.Load();
            //var settingConfig = SettingConfig.Load();
            //var tutorial_config = TutorialConfig.Load();
            //var product_config = ProductConfig.Load();
            //var furniture_config = FurnitureConfig.Load();
            //var license_config = LicenseConfig.Load();
            //var growth_config = GrowthConfig.Load();
            //var bank_config = BankConfig.Load();
            //var cashReceived_config = CashReceivedConfig.Load();
            //var deliver_Config = DeliverConfig.Load();
            //var experience_Config = ExperienceConfig.Load();
            //var enegy_Config = EnegyConfig.Load();
            //var ads_Config = AdsConfig.Load();
            //var tutSystem_Config = TutSystemConfig.Load();
            //var decorationItemConfig = DecorationItemConfig.Load();
            //var gachaDecorationConfig = GachaDecorationConfig.Load();
            //var ratingConfig = RatingConfig.Load();
            //var decorTexSuperConfig = DecorTexSuperConfig.Load();
            //var hiringConfig = HiringConfig.Load();
            //var dailyReportConfig = DailyReportConfig.Load();

            //_context.Install(config);
            //_context.Install(settingConfig);
            //_context.Install(tutorial_config);
            //_context.Install(product_config);
            //_context.Install(furniture_config);
            //_context.Install(license_config);
            //_context.Install(growth_config);
            //_context.Install(bank_config);
            //_context.Install(cashReceived_config);
            //_context.Install(deliver_Config);
            //_context.Install(experience_Config);
            //_context.Install(enegy_Config);
            //_context.Install(ads_Config);
            //_context.Install(tutSystem_Config);
            //_context.Install(decorationItemConfig);
            //_context.Install(gachaDecorationConfig);
            //_context.Install(ratingConfig);
            //_context.Install(decorTexSuperConfig);
            //_context.Install(hiringConfig);
            //_context.Install(dailyReportConfig);    
            _context.ApplyInstall();

            ShazamLogger.Log("GameInitializeState StartLoadLevel");
            //_gameStateManager.SwitchToState(new GameLoadLevelState());
            _gameStateManager.SwitchToState(new GameLoadLevelState());
        }
       
        private void InitLevelModules()
        {
            //AddModule<GOPoolModule, GOPoolModuleView>(_levelView);
            //AddModule<CustomerModule, CustomerModuleView>(_levelView);
            //AddModule<DeliverModule, DeliverModuleView>(_levelView);
            //AddModule<ExpandStoreModule, ExpandStoreModuleView>(_levelView);
            //AddModule<TutSystemModule, TutSystemModuleView>(_levelView);
            //AddModule<RewardFreeModule, RewardFreeModuleView>(_levelView);
            //AddModule<OfferAdsModule, OfferAdsModuleView>(_levelView);
            //AddModule<EmployeeModule, EmployeeModuleView>(_levelView);
            //AddModule<DecorTexModule, DecorTexModuleView>(_levelView);
            //AddModule<OpenCloseSignModule, OpenCloseSignView>(_levelView);
            // AddModule<InventoryModule, InventoryModuleView>(_gameView);
            // AddModule<UISpritesModule, UISpritesModuleView>(_gameView);
            // AddModule<UINotificationModule, UINotificationModuleView>(_gameView);

            // AddModule<TutorialModule, TutorialModuleView>(_gameView);
        }

        private void AddModule<T, T1>(Component component) where T : Module
        {
            var view = component.transform.GetComponent<T1>();
            var result = (T)Activator.CreateInstance(typeof(T), new object[] { view });
            _levelModules.Add(result);
            _injector.Inject(result);

            _context.InstallByType(result, typeof(T));
            result.Initialize();
        }

        private void DisposeLevelModules()
        {
            foreach (var levelModule in _levelModules)
            {
                levelModule.Dispose();
            }

            _levelModules.Clear();
        }

        public override void Dispose()
        {
        }
    }
}