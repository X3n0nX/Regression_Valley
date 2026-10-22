using StardewModdingAPI;
using StardewValley;
using System.Text.Json.Serialization;

namespace RegressionMod
{
    public class Config
    {
        public bool AlwaysNoticeAccidents = true;
        public bool Debug = false;
        public bool Easymode = false;
        public bool PantsChangeRequiresHome = true;
        public bool UnderwearChangeCauseExposure = true;
        public bool Wetting = true;
        public bool Messing = true;
        public int FriendshipPenaltyBladderMultiplier = 100;
        public int FriendshipPenaltyBowelMultiplier = 200;
        public bool FriendshipDebugVeryNice = false;
        public bool FriendshipDebugNice = false;
        public bool NoHungerAndThirst = false;
        public int NighttimeLossMultiplier = 50;
        public int NighttimeGainMultiplier = 50;
        public int InUnderwearOnPurposeMultiplier = 50;
        public int BladderLossContinenceRate = 2;
        public int BowelLossContinenceRate = 4;
        public int BladderGainContinenceRate = 3;
        public int BowelGainContinenceRate = 4;
        public int MaxBladderCapacity = 600;
        public int MaxBowelCapacity = 1000;
        public int StartBladderContinence = 70;
        public int StartBowelContinence = 90;
        public bool ReadSaveFiles = true;
        public bool WriteSaveFiles = false;
        public bool ReturnUsedCloth = true;
        public bool ReturnUsedDisposable = true;
        public bool HideUnderwearStatusIcon = false;

        #region Keys              

        /// <summary> Key to pee in underwear.</summary>
        public SButton KeyPee = SButton.F1;

        /// <summary> Key to poop in underwear.</summary>
        public SButton KeyPoop = SButton.F2;

        public SButton KeyGoInPants = 0;
        public SButton KeyGoInToilet = 0;

        /// <summary> 
        /// Key combined with shift to pee on ground or, <br/>
        /// in place with toilet, in toilet.
        /// </summary>
        public SButton KeyPeeInToilet = SButton.F1;

        /// <summary> 
        /// Key combined with shift to poop on ground or, </summary>br> 
        /// in place with toilet, in toilet.
        /// </summary>
        public SButton KeyPoopInToilet = SButton.F2;

        /// <summary> Key to check the state of your underwear.</summary>
        public SButton KeyCheckUnderwear = SButton.F5;

        /// <summary> Key to ckeck the state of your pants.</summary>
        public SButton KeyCheckPants = SButton.F6;

        /// <summary> Key to ckeck your potty training.</summary>
        public SButton KeyCheckPottyTraining = SButton.F7;

        /// <summary> Key to check your potty feeling.</summary>
        public SButton KeyCheckPottyFeeling = SButton.F8;

        /// <summary> Key, in combination with left shift, to ask a npc to check it underwear.</summary>
        public SButton KeyAskCheckNpc = SButton.V;

        /// <summary> Key, in combination with left shift, to ask a npc to check your underwear.</summary>
        public SButton KeyAskCheckPlayer = SButton.B;

        #region Debug keys

        /// <summary> Key to toggle debug mode.</summary>
        public SButton KeyToggleDebug = SButton.F9;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to decrease food, water, bladder fullness and bowel fullness <br/>
        /// in debug mode.
        /// </summary>
        public SButton KeyDebugDecrease = SButton.F1;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to increase food, water, bladder fullness and bowel fullness <br/>
        /// in debug mode.
        /// </summary>
        public SButton KeyDebugIncrease = SButton.F2;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to open an menu with all underwears <br/>
        /// and some wine in debug mode.
        /// </summary>
        public SButton KeyDebugGiveUnderwear = SButton.F3;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to fast forward game time in debug mode.
        /// </summary>
        public SButton KeyDebugFastForward = SButton.F5;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to toggle the wetting option in debug mode.
        /// </summary>
        public SButton KeyDebugToggleWetting = SButton.F6;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to toggle the messing option in debug mode.
        /// </summary>
        public SButton KeyDebugToggleMessing = SButton.F7;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to toggle the easymode option in debug mode.
        /// </summary>
        public SButton KeyDebugToggleEasymode = SButton.F8;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to increase the bladder continence in debug mode.
        /// </summary>
        public SButton KeyDebugIncreaseBladderContinence = SButton.W;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to decrease the bladder continence in debug mode.
        /// </summary>
        public SButton KeyDebugDecreaseBladderContinence = SButton.S;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to increase the bowel continence in debug mode.
        /// </summary>
        public SButton KeyDebugIncreaseBowelContinence = SButton.E;

        /// <summary> 
        /// Key combined with right alt key <br/> 
        /// to decrease the bowel continence in debug mode.
        /// </summary>
        public SButton KeyDebugDecreaseBowelContinence = SButton.D;

        #endregion
        #endregion
    }
}
