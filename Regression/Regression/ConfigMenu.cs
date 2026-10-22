using StardewModdingAPI;
using GenericModConfigMenu;
using static RegressionMod.Regression;

namespace RegressionMod
{
    public class ConfigMenu
    {       

        public static void GenerateMenu(IModHelper helper, IManifest modManifest)
        {
            // get Generic Mod Config Menu's API (if it's installed)
            var configMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
                return;

            // register mod
            configMenu.Register(
                mod: modManifest,
                reset: () => config = new Config(),
                save: () => helper.WriteConfig(config)
            );

            // Config of the main page. Most important options
            // Cheat Mode
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Cheat_Mode.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Cheat_Mode.Tooltip}}"),
                getValue: () => config.Debug,
                setValue: value => config.Debug = value
            );
            // Easy Mode
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Easy_Mode.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Easy_Mode.Tooltip}}"),
                getValue: () => config.Easymode,
                setValue: value => config.Easymode = value
            );
            // Wetting
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Wetting.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Wetting.Tooltip}}"),
                getValue: () => config.Wetting,
                setValue: value => config.Wetting = value
            );
            // Messing 
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Messing.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Messing.Tooltip}}"),
                getValue: () => config.Messing,
                setValue: value => config.Messing = value
            );
            // Children And Diapers
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Children_And_Diapers.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Children_And_Diapers.Tooltip}}"),
                getValue: () => ChildrenAndDiapers,
                setValue: value => ChildrenAndDiapers = value
            );
            // Max Bladder Capacity
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Bladder_Capacity.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Bladder_Capacity.Tooltip}}"),
                getValue: () => config.MaxBladderCapacity,
                setValue: value => config.MaxBladderCapacity = value,
                min: 300, max: 1800, interval: 50
            );
            // Max Bowel Capacity
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Bowel_Capacity.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Bowel_Capacity.Tooltip}}"),
                getValue: () => config.MaxBowelCapacity,
                setValue: value => config.MaxBowelCapacity = value,
                min: 300, max: 1800, interval: 50
            );
            // Always Notice Accidents
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Always_Notice_Accidents.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Always_Notice_Accidents.Tooltip}}"),
                getValue: () => config.AlwaysNoticeAccidents,
                setValue: value => config.AlwaysNoticeAccidents = value
            );
            // Pants Change Requires Home
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Pants_Change_At_Home.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Pants_Change_At_Home.Tooltip}}"),
                getValue: () => config.PantsChangeRequiresHome,
                setValue: value => config.PantsChangeRequiresHome = value
            );
            // Underwear Change Cause Exposure
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Underwear_Change_Causes_Exposure.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Underwear_Change_Causes_Exposure.Tooltip}}"),
                getValue: () => config.UnderwearChangeCauseExposure,
                setValue: value => config.UnderwearChangeCauseExposure = value
            );
            // Return Used Cloth
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Return_Used_Cloth.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Return_Used_Cloth.Tooltip}}"),
                getValue: () => config.ReturnUsedCloth,
                setValue: value => config.ReturnUsedCloth = value
            );
            // Return Used Disposable
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Return_Used_Disposable.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Return_Used_Disposable.Tooltip}}"),
                getValue: () => config.ReturnUsedDisposable,
                setValue: value => config.ReturnUsedDisposable = value
            );

            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Key Bindings",
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Key_Bindings_Menu.Name}}")
            );
            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Continence",
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Continence_Menu.Name}}")
            );
            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Friendships",
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Friendships_Menu.Name}}")
            );
            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Save Files",
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Save_Files_Menu.Name}}")
            );

            #region Page Key Bindings

            // All the options for key bindings
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Key Bindings"
            );
            // Pee Pants
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Pee_Pants.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Pee_Pants.Tooltip}}"),
                getValue: () => (SButton)config.KeyPee,
                setValue: value => config.KeyPee = value
            );
            // Poop Pants
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Poop_Pants.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Poop_Pants.Tooltip}}"),
                getValue: () => (SButton)config.KeyPoop,
                setValue: value => config.KeyPoop = value
            );
            // Pee In Potty
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Pee_In_Potty.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Pee_In_Potty.Tooltip}}"),
                getValue: () => (SButton)config.KeyPeeInToilet,
                setValue: value => config.KeyPeeInToilet = value
            );
            // Poop In Potty
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Poop_In_Potty.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Poop_In_Potty.Tooltip}}"),
                getValue: () => (SButton)config.KeyPoopInToilet,
                setValue: value => config.KeyPoopInToilet = value
            );
            // Go In Pants
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Go_In_Pants.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Go_In_Pants.Tooltip}}"),
                getValue: () => (SButton)config.KeyGoInPants,
                setValue: value => config.KeyGoInPants = value
            );
            // Go Potty
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Go_Potty.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Go_Potty.Tooltip}}"),
                getValue: () => (SButton)config.KeyGoInToilet,
                setValue: value => config.KeyGoInToilet = value
            );
            // Check Underwear
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Underwear.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Underwear.Tooltip}}"),
                getValue: () => (SButton)config.KeyCheckUnderwear,
                setValue: value => config.KeyCheckUnderwear = value
            );
            // Check Pants
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Pants.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Pants.Tooltip}}"),
                getValue: () => (SButton)config.KeyCheckPants,
                setValue: value => config.KeyCheckPants = value
            );
            // Check Potty Training
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Potty_Training.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Potty_Training.Tooltip}}"),
                getValue: () => (SButton)config.KeyCheckPottyTraining,
                setValue: value => config.KeyCheckPottyTraining = value
            );
            // Check Potty Feeling
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Potty_Feeling.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Check_Potty_Feeling.Tooltip}}"),
                getValue: () => (SButton)config.KeyCheckPottyFeeling,
                setValue: value => config.KeyCheckPottyFeeling = value
            );
            // Ask NPC to Check Underwear
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Ask_Check_Npc.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Ask_Check_Npc.Tooltip}}"),
                getValue: () => (SButton)config.KeyAskCheckNpc,
                setValue: value => config.KeyAskCheckNpc = value
            );
            // Ask NPC to Check Player Underwear
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Ask_Check_Player.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Ask_Check_Player.Tooltip}}"),
                getValue: () => (SButton)config.KeyAskCheckPlayer,
                setValue: value => config.KeyAskCheckPlayer = value
            );
            // Toggle Debug Mode
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Toggle_Debug.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Toggle_Debug.Tooltip}}"),
                getValue: () => (SButton)config.KeyToggleDebug,
                setValue: value => config.KeyToggleDebug = value
            );
            // Debug Keys
            configMenu.AddPageLink(
                mod: modManifest,
                pageId: "Key Bindings Debug",
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Main.Key_Bindings_Debug_Menu.Name}}")
            );

            #region Page Key Bindings Debug
            
            // All the options for debug key bindings
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Key Bindings Debug"
            );
            // Decrease Food, Water, Bladder Fullness and Bowel Fullness
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugDecrease,
                setValue: value => config.KeyDebugDecrease = value
            );
            // Increase Food, Water, Bladder Fullness and Bowel Fullness
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugIncrease,
                setValue: value => config.KeyDebugIncrease = value
            );
            // Give Underwear
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Give_Underwear.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Give_Underwear.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugGiveUnderwear,
                setValue: value => config.KeyDebugGiveUnderwear = value
            );
            // Fast Forward Game Time
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Fast_Forward.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Fast_Forward.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugFastForward,
                setValue: value => config.KeyDebugFastForward = value
            );
            // Toggle Wetting Option
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Wetting.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Wetting.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugToggleWetting,
                setValue: value => config.KeyDebugToggleWetting = value
            );
            // Toggle Messing Option
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Messing.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Messing.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugToggleMessing,
                setValue: value => config.KeyDebugToggleMessing = value
            );
            // Toggle Easymode Option
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Easy_Mode.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Toggle_Easy_Mode.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugToggleEasymode,
                setValue: value => config.KeyDebugToggleEasymode = value
            );
            // Increase Bladder Continence
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase_Bladder_Continence.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase_Bladder_Continence.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugIncreaseBladderContinence,
                setValue: value => config.KeyDebugIncreaseBladderContinence = value
            );
            // Decrease Bladder Continence
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease_Bladder_Continence.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease_Bladder_Continence.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugDecreaseBladderContinence,
                setValue: value => config.KeyDebugDecreaseBladderContinence = value
            );
            // Increase Bowel Continence
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase_Bowel_Continence.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Increase_Bowel_Continence.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugIncreaseBowelContinence,
                setValue: value => config.KeyDebugIncreaseBowelContinence = value
            );
            // Decrease Bowel Continence
            configMenu.AddKeybind(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease_Bowel_Continence.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Key_Bindings.Debug.Decrease_Bowel_Continence.Tooltip}}"),
                getValue: () => (SButton)config.KeyDebugDecreaseBowelContinence,
                setValue: value => config.KeyDebugDecreaseBowelContinence = value
            );

            #endregion

            #endregion

            #region Page Continence

            // All the options related to continence balancing
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Continence"
            );
            // Nighttime Losses
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Nighttime_Losses.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Nighttime_Losses.Tooltip}}"),
                getValue: () => config.NighttimeLossMultiplier,
                setValue: value => config.NighttimeLossMultiplier = value,
                min: 0, max: 200, interval: 10
            );
            //´Nighttime Gains
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Nighttime_Gains.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Nighttime_Gains.Tooltip}}"),
                getValue: () => config.NighttimeGainMultiplier,
                setValue: value => config.NighttimeGainMultiplier = value,
                min: 0, max: 200, interval: 10
            );
            // In Underwear on Purpose Modifier
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.In_Diaper_on_Purpose_Modifier.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.In_Diaper_on_Purpose_Modifier.Tooltip}}"),
                getValue: () => config.InUnderwearOnPurposeMultiplier,
                setValue: value => config.InUnderwearOnPurposeMultiplier = value,
                min: 0, max: 200, interval: 10
            );
            // Accident Bladder Loss
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Accident_Bladder_Loss.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Accident_Bladder_Loss.Tooltip}}"),
                getValue: () => config.BladderLossContinenceRate,
                setValue: value => config.BladderLossContinenceRate = value,
                min: 0, max: 20, interval: 1
            );
            // Accident Bowel Loss
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Accident_Bowel_Loss.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Accident_Bowel_Loss.Tooltip}}"),
                getValue: () => config.BowelLossContinenceRate,
                setValue: value => config.BowelLossContinenceRate = value,
                min: 0, max: 20, interval: 1
            );
            // Toilet Bladder Gain
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Toilet_Bladder_Gain.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Toilet_Bladder_Gain.Tooltip}}"),
                getValue: () => config.BladderGainContinenceRate,
                setValue: value => config.BladderGainContinenceRate = value,
                min: 0, max: 20, interval: 1
            );
            // Toilet Bowel Gain
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Toilet_Bowel_Gain.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Toilet_Bowel_Gain.Tooltip}}"),
                getValue: () => config.BowelGainContinenceRate,
                setValue: value => config.BowelGainContinenceRate = value,
                min: 0, max: 20, interval: 1
            );
            // Start Bladder Continence
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Start_Bladder_Continence.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Start_Bladder_Continence.Tooltip}}"),
                getValue: () => config.StartBladderContinence,
                setValue: value => config.StartBladderContinence = value,
                min: (int)(Body.minBladderContinence * 100), max: 100, interval: 5
            );
            // Start Bowel Continence
            configMenu.AddNumberOption(
               mod: modManifest,
               name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Start_Bowel_Continence.Name}}"),
               tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Continence.Start_Bowel_Continence.Tooltip}}"),
               getValue: () => config.StartBowelContinence,
               setValue: value => config.StartBowelContinence = value,
               min: (int)(Body.minBowelContinence * 100), max: 100, interval: 5
            );

            #endregion

            #region Page Friendships

            // All the options related to friendship changes caused by accidents
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Friendships"
            );
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Penalty_Peeing.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Penalty_Peeing.Tooltip}}"),
                getValue: () => config.FriendshipPenaltyBladderMultiplier,
                setValue: value => config.FriendshipPenaltyBladderMultiplier = value,
                min: 0, max: 500, interval: 10
            );
            configMenu.AddNumberOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Penalty_Pooping.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Penalty_Pooping.Tooltip}}"),
                getValue: () => config.FriendshipPenaltyBowelMultiplier,
                setValue: value => config.FriendshipPenaltyBowelMultiplier = value,
                min: 0, max: 500, interval: 10
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Nice.Debug.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.Nice.Debug.Tooltip}}"),
                getValue: () => config.FriendshipDebugNice,
                setValue: value => config.FriendshipDebugNice = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.VeryNice.Debug.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Friendships.VeryNice.Debug.Tooltip}}"),
                getValue: () => config.FriendshipDebugVeryNice,
                setValue: value => config.FriendshipDebugVeryNice = value
            );

            #endregion

            #region Save Files

            // All the options related to save files
            configMenu.AddPage(
                mod: modManifest,
                pageId: "Save Files"
            );
            configMenu.AddParagraph(
                mod: modManifest,
                text: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Save_Files.Headline}}")
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Save_Files.Read.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Save_Files.Read.Tooltip}}"),
                getValue: () => config.ReadSaveFiles,
                setValue: value => config.ReadSaveFiles = value
            );
            configMenu.AddBoolOption(
                mod: modManifest,
                name: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Save_Files.Wtite.Name}}"),
                tooltip: () => Strings.tryGetI18nText("{{i18n:Config_Menu.Save_Files.Wtite.Tooltip}}"),
                getValue: () => config.WriteSaveFiles,
                setValue: value => config.WriteSaveFiles = value
            );

            #endregion
        }
    }
}
