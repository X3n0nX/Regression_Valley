using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buffs;
using StardewValley.GameData.Characters;
using StardewValley.Locations;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RegressionMod
{
    //<TODO> A lot of bladder and bowel stuff is processed similarly. Consider refactor with arrays and Function pointers.
    public class Body
    {
        // Floximo: In general you need to go to the toilet (in reality) about every 3-4 hours on average, lets go with 4 and assume 500mL and 16 waking hours a day (and we assume you healthy, not waking up at night) its around 2500mL of pee a day.
        // For Pee the gameplay aligns pretty good, considering the values this is based on are wildly out of propotions. But i guess if only 20% of the water you drink is peed out, and the rest sweat, this might be possible.
        // Poop is usually once a day on average. The values set balanced out to you needing to poop every 4 hours at least. That is a lot if you don't want to only pay potty simulator. Gets worse with bad potty training.
        // Balancing poop with the given values "foodToBowelConversion" and "requiredCaloriesPerDay" is challanging, as the bowel capacity is linear to this 2 values. 

        //Lets think of Food in Calories, and water in mL
        //For a day Laborer (like a farmer) that should be ~3500 Cal, and 14000 mL - NOTE: (Floximo) you mean 1400 mL probably. Up to 3000 mL is considered healthy
        //Of course this is dependent on amount of work, but let's go one step at a time
        private static readonly float requiredCaloriesPerDay = 3500f;
        public float hungerMeterMax
        {
            get => requiredCaloriesPerDay * 2;
        }

        private static readonly float requiredWaterPerDay = 8000f; //8oz glasses: every 20min for 8 hours (5455mL) + every 40 min for 8 hour (2727mL) Floximo: That is rough. Possible in extrem heat but rough to drink.

        //Average # of Pees per day is ~3
        public static float maxBladderCapacity => Regression.config.MaxBladderCapacity; //about 600mL => changed to 800mL as player will start with lower bladder by default
        public static readonly float minBladderContinence = 0.4f; // Also describes capacity as changes are linear
        private static readonly float waterToBladderConversion = 0.2f;//Only ~1/4 (0.225f) water becomes pee, rest is sweat etc. => changed to 0.2f (1/5) for game balance reasons (too intrusive)

        //Average # of poops per day varies wildly. Let's say about 1.5 per day.
        private static readonly float foodToBowelConversion = 0.4f; // lowered from 0.67f to 0.4f to make the numbers smaller (too much poop) and bowel capacity / 1.4f (1000f in total)
        private static float maxBowelCapacity => (requiredCaloriesPerDay * foodToBowelConversion) / 1.4f / 1000f * Regression.config.MaxBowelCapacity; // The last 2 numbers usually end up as / 1200 * 1200 (cancle eachother out) to make configuration easier to understand 
        public static readonly float minBowelContinence = 0.4f; // Also describes capacity as changes are linear

        private static ChangeData changeData = Regression.changeData;
        private static ConsumablesData consumablesData = Regression.consumablesData;
        private static PeePoopData peePoopData = Regression.peePoopData;
        private static StatesContinenceData statesContinenceData = Regression.statesContinenceData;

        //Setup Thresholds and messages
        public static readonly float trainingThreshold = 0.5f; // we set a threshold that allowes for potty training, so that should also be the warning level, for the player to understand whats going on
        private static readonly float lastWarningThreshold = 0.8f; // with a minimum continence of 0.3, 0.8 is still warned about, as it would warn up to 0.7, while 0.69 is out of range (means only 1 warning)
        public static readonly float[] WETTING_THRESHOLDS = { trainingThreshold + 0.05f, 0.69f, lastWarningThreshold };
        public static readonly string[][] WETTING_MESSAGES = { statesContinenceData?.Bladder_Yellow ?? new string[0], statesContinenceData?.Bladder_Orange ?? new string[0], statesContinenceData?.Bladder_Red ?? new string[0] };
        public static readonly string[] WETTING_MESSAGE_GREEN = statesContinenceData?.Bladder_Green ?? new string[0];
        public static readonly float[] MESSING_THRESHOLDS = { trainingThreshold + 0.05f, 0.69f, lastWarningThreshold };
        public static readonly string[][] MESSING_MESSAGES = { statesContinenceData?.Bowels_Yellow ?? new string[0], statesContinenceData?.Bowels_Orange ?? new string[0], statesContinenceData?.Bowels_Red ?? new string[0] };
        public static readonly string[] MESSING_MESSAGE_GREEN = statesContinenceData?.Bowels_Green ?? new string[0];
        public static readonly float[] BLADDER_CONTINENCE_THRESHOLDS = { minBladderContinence, 0.55f, 0.65f, 0.8f, 1.0f };
        public static readonly string[][] BLADDER_CONTINENCE_MESSAGES = { statesContinenceData?.Bladder_Continence_Min ?? new string[0], statesContinenceData?.Bladder_Continence_Red ?? new string[0], statesContinenceData?.Bladder_Continence_Orange ?? new string[0], statesContinenceData?.Bladder_Continence_Yellow ?? new string[0], statesContinenceData?.Bladder_Continence_Green ?? new string[0] };
        public static readonly float[] BOWEL_CONTINENCE_THRESHOLDS = { minBowelContinence, 0.55f, 0.65f, 0.8f, 1.0f };
        public static readonly string[][] BOWEL_CONTINENCE_MESSAGES = { statesContinenceData?.Bowel_Continence_Min ?? new string[0], statesContinenceData?.Bowel_Continence_Red ?? new string[0], statesContinenceData?.Bowel_Continence_Orange ?? new string[0], statesContinenceData?.Bowel_Continence_Yellow ?? new string[0], statesContinenceData?.Bowel_Continence_Green ?? new string[0] };
        private static readonly float[] HUNGER_THRESHOLDS = { 0.0f, 0.25f };
        private static readonly string[][] HUNGER_MESSAGES = { statesContinenceData?.Food_None ?? new string[0], statesContinenceData?.Food_Low ?? new string[0] };
        private static readonly float[] THIRST_THRESHOLDS = { 0.0f, 0.25f };
        private static readonly string[][] THIRST_MESSAGES = { statesContinenceData?.Water_None ?? new string[0], statesContinenceData?.Water_Low ?? new string[0] };
        private static readonly int wakeUpPenalty = 8;
        private static readonly Debuff MESSY_DEBUFF = new Debuff(BodyConstants.DebuffMessy);
        private static readonly Debuff WET_DEBUFF = new Debuff(BodyConstants.DebuffWet);

        #region Saved values

        //Things that describe an individual
        public int bedtime
        {
            get => LoadInt("bedtime", 0);
            set => save("bedtime", value);
        }

        public float bladderContinence
        {
            get => LoadFloat("bladderContinence", Math.Min(1f, Math.Max(minBowelContinence, Regression.config.StartBladderContinence / 100f)));
            set => save("bladderContinence", value);
        }

        public float bladderFullness
        {
            get => LoadFloat("bladderFullness", 0f);
            set => save("bladderFullness", value);
        }

        public float bowelContinence
        {
            get => LoadFloat("bowelContinence", Math.Min(1f, Math.Max(minBowelContinence, Regression.config.StartBowelContinence / 100f)));
            set => save("bowelContinence", value);
        }

        public float bowelFullness
        {
            get => LoadFloat("bowelFullness", 0f);
            set => save("bowelFullness", value);
        }

        public float hunger
        {
            get => LoadFloat("hunger", 0f);
            set => save("hunger", value);
        }

        public float thirst
        {
            get => LoadFloat("thirst", 0f);
            set => save("thirst", value);
        }

        public bool isSleeping
        {
            get => LoadBool("isSleeping", false);
            set => save("isSleeping", value);
        }

        public int numPottyPooAtNight
        {
            get => LoadInt("numPottyPooAtNight", 0);
            set => save("numPottyPooAtNight", value);
        }

        public int numPottyPeeAtNight
        {
            get => LoadInt("numPottyPeeAtNight", 0);
            set => save("numPottyPeeAtNight", value);
        }

        public int numAccidentPooAtNight
        {
            get => LoadInt("numAccidentPooAtNight", 0);
            set => save("numAccidentPooAtNight", value);
        }

        public int numAccidentPeeAtNight
        {
            get => LoadInt("numAccidentPeeAtNight", 0);
            set => save("numAccidentPeeAtNight", value);
        }

        #endregion

        #region ModData save/load functions
        private void save(string name, int val)
        {
            Game1.player.modData[BuildKeyFor(name)] = val.ToString();
        }
        private void save(string name, float val)
        {
            Game1.player.modData[BuildKeyFor(name)] = val.ToString();
        }
        private void save(string name, bool val)
        {
            Game1.player.modData[BuildKeyFor(name)] = val.ToString();
        }
        private static int LoadInt(string name, int defaultVal)
        {
            if (!Game1.player.modData.ContainsKey(BuildKeyFor(name))) return defaultVal;
            return int.Parse(Game1.player.modData[BuildKeyFor(name)]);
        }
        private static float LoadFloat(string name, float defaultVal)
        {
            if (!Game1.player.modData.ContainsKey(BuildKeyFor(name))) return defaultVal;
            return float.Parse(Game1.player.modData[BuildKeyFor(name)]);
        }
        private static bool LoadBool(string name, bool defaultVal)
        {
            if (!Game1.player.modData.ContainsKey(BuildKeyFor(name))) return defaultVal;
            return bool.Parse(Game1.player.modData[BuildKeyFor(name)]);
        }
        private static string BuildKeyFor(string varName)
        {
            return $"{BodyConstants.ModDataPrefix}/{varName}";
        }
        private bool HasKeyFor(string varName)
        {
            return Game1.player.modData != null && Game1.player.modData.ContainsKey(BuildKeyFor(varName));
        }

        #endregion
              

        private float _lastStamina = 0;

        public float bladderCapacity
        {
            get
            {
                //Decrease our maximum capacity (bladder shrinks as we become incontinent)
                return bladderContinence * maxBladderCapacity;

                //Ceiling at base value and floor at 25% base value
                //return Math.Max(bladderCapacity, maxBladderCapacity * minBladderContinence);
            }
        }

        public float bowelCapacity
        {
            get
            {
                //Decrease our maximum capacity (bowel shrinks as we become incontinent)
                return bowelContinence * maxBowelCapacity;

                //Ceiling at base value and floor at 25% base value
                //return Math.Max(bowelCapacity, minBowelCapacity * minBowelContinence);
            }
        }
        private Container _bed;
        [JsonIgnore]
        public Container bed
        {
            get
            {
                if (_bed == null)
                {
                    _bed = new Container(this, ContainerSubtype.Bed, ContainerConstants.Bed);
                }
                return _bed;
            }
        }
        private Container _pants;
        [JsonIgnore]
        public Container pants
        {
            get
            {
                if (_pants == null)
                {
                    _pants = new Container(this, ContainerSubtype.Pants, ContainerConstants.BlueJeans);
                }
                return _pants;
            }
        }

        private Container _underwear;
        [JsonIgnore]
        public Container underwear
        {
            get
            {
                if (_underwear == null)
                {
                    _underwear = new Container(this, ContainerSubtype.Underwear, UnderwearConstants.DinosaurUndies);
                }
                return _underwear;
            }
        }

        public Body()
        {

        }

        public bool IsAllowedResource(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return Regression.config.Wetting;
                case IncidentType.POOP:
                    return Regression.config.Messing;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }

        public float GetBladderTrainingThreshold()
        {
            return bladderCapacity * trainingThreshold;
        }

        public float GetBowelTrainingThreshold()
        {

            return bowelCapacity * trainingThreshold;
        }
        public float GetTrainingThreshold(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return GetBladderTrainingThreshold();
                case IncidentType.POOP:
                    return GetBowelTrainingThreshold();
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetAttemptThreshold(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return GetBladderAttemptThreshold();
                case IncidentType.POOP:
                    return GetBowelAttemptThreshold();
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetBladderAttemptThreshold()
        {
            return bladderCapacity * 0.15f;
        }

        public float GetBowelAttemptThreshold()
        {
            return bowelCapacity * 0.15f;
        }
        public float GetFullness(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return bladderFullness;
                case IncidentType.POOP:
                    return bowelFullness;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public void SetFullness(IncidentType type, float value)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    bladderFullness = Math.Max(0f, value);
                    return;
                case IncidentType.POOP:
                    bowelFullness = Math.Max(0f, value);
                    return;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetMaxCapacity(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return maxBladderCapacity;
                case IncidentType.POOP:
                    return maxBowelCapacity;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetCapacity(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return bladderCapacity;
                case IncidentType.POOP:
                    return bowelCapacity;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetContinenceMin(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return minBladderContinence;
                case IncidentType.POOP:
                    return minBowelContinence;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetContinence(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return bladderContinence;
                case IncidentType.POOP:
                    return bowelContinence;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        // This function sets, it doesn't substract, be aware to use a calculation that is aware of that, like GetContinence(type) + change
        public void SetContinence(IncidentType type, float value)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    bladderContinence = Math.Max(minBladderContinence, Math.Min(1f, value));
                    return;
                case IncidentType.POOP:
                    bowelContinence = Math.Max(minBowelContinence, Math.Min(1f, value));
                    return;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetHungerPercent()
        {
            return (hungerMeterMax - hunger) / hungerMeterMax;
        }

        public float GetThirstPercent()
        {
            return (requiredWaterPerDay - thirst) / requiredWaterPerDay;
        }

        public float GetBowelPercent()
        {
            return bowelFullness / bowelCapacity;
        }

        public float GetBladderPercent()
        {
            return bladderFullness / bladderCapacity;
        }

        public void AddResource(IncidentType type, float amount)
        {
            //If Resource (poop/pee) is disabled, don't do anything
            if (!IsAllowedResource(type))
                return;

            var fullness = GetFullness(type);
            var capacity = GetCapacity(type);
            //Increment the current amount
            //We allow bladder to go over-full, to simulate the possibility of multiple night wettings
            //This is determined by the amount of water you have in your system when you go to bed
            float oldFullnessPercent = fullness / capacity;

            SetFullness(type, fullness + amount);

            fullness = GetFullness(type);
            //Did we go over? Then have an accident.
            if (fullness >= capacity)
            {
                if (!isSleeping)
                {
                    if (MinorAccident(type)) return;
                }
                Accident(type, voluntary: false, inUnderwear: true);
            }
            else if (!underwear.removable && oldFullnessPercent > 0.7f && !IsTryingToHoldIt(type))
            {
                Accident(type, voluntary: true, inUnderwear: true);
            }
            else
            {
                float newFullnessPercent = fullness / capacity;
                // No randomness in this. We get warned later and it is more urgent immediatly, giving less time, but reliable.
                if (newFullnessPercent > (1 - GetContinence(type)))
                {
                    // old and new is inverted, because we expect a rising, not falling trend
                    Warn(newFullnessPercent, oldFullnessPercent, type == IncidentType.POOP ? MESSING_THRESHOLDS : WETTING_THRESHOLDS, type == IncidentType.POOP ? MESSING_MESSAGES : WETTING_MESSAGES, false);
                }
            }
        }

        //Change current Food value and handle warning messages
        //Notice that we do things here even if Hunger and Thirst are disabled
        //This is due to Food and Water's effect on Wetting/Messing
        public void AddFood(float amount, float conversionRatio = 1f)
        {
            //How full are we?
            float oldPercent = GetHungerPercent();
            hunger -= amount;
            float newPercent = GetHungerPercent();

            //Convert food lost into poo at half rate
            if (amount < 0)
            {
                if (hunger < hungerMeterMax)
                    AddResource(IncidentType.POOP, amount * -1f * conversionRatio * foodToBowelConversion);
                else
                    // People do stop pooping after days not eating, but a) thats probably not the case and b) its a game. We assume a little poop is produced.
                    AddResource(IncidentType.POOP, amount * -0.4f * conversionRatio * foodToBowelConversion);
            }


            //If we go over full, add additional to bowels at half rate
            if (hunger < 0)
            {
                AddResource(IncidentType.POOP, hunger * -0.5f * conversionRatio * foodToBowelConversion);
                hunger = 0f;
                newPercent = GetHungerPercent();
            }

            if (Regression.config.NoHungerAndThirst)
            {
                hunger = 0; //Reset if disabled
                return;
            }

            //If we're starving and not eating, take a stamina hit
            if (hunger > hungerMeterMax && amount < 0)
            {
                //Take percentage off stamina equal to percentage above max hunger
                // Floximo: This is now half as punishing, preventing deadlock situations where the farmer could no longer reach field or town. If intended, this is the place to edit the final values or insert config.
                Game1.player.stamina += newPercent * (hungerMeterMax / requiredCaloriesPerDay) * Game1.player.MaxStamina / 2;
                hunger = hungerMeterMax;
                newPercent = 1;
            }

            Warn(oldPercent, newPercent, HUNGER_THRESHOLDS, HUNGER_MESSAGES, false);
        }

        public void AddWater(float amount, float conversionRatio = 1f)
        {
            //How full are we?
            float oldPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            thirst -= amount;
            float newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;

            //Convert food lost into poo at half rate
            if (amount < 0)
            {
                if (hunger < requiredWaterPerDay)
                    AddResource(IncidentType.PEE, amount * -1f * conversionRatio * waterToBladderConversion);
                else
                    // As long as we are not dead, some pee will be generated. We need to filter out toxins.
                    AddResource(IncidentType.POOP, amount * -0.4f * conversionRatio * waterToBladderConversion);
            }

            //Also if we go over full, add additional to Bladder at half rate
            if (thirst < 0)
            {
                AddResource(IncidentType.PEE, (thirst * -0.5f * conversionRatio * waterToBladderConversion));
                thirst = 0f;
                newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            }

            if (Regression.config.NoHungerAndThirst)
            {
                thirst = 0; //Reset if disabled
                return;
            }

            //If we're starving and not eating, take a stamina hit
            if (thirst > requiredWaterPerDay && amount < 0)
            {
                //Take percentage off health equal to percentage above max thirst
                float lostHealth = newPercent * (float)Game1.player.maxHealth;
                Game1.player.health = Game1.player.health + (int)lostHealth;
                thirst = requiredWaterPerDay;
                newPercent = (requiredWaterPerDay - thirst) / requiredWaterPerDay;
            }

            Warn(oldPercent, newPercent, THIRST_THRESHOLDS, THIRST_MESSAGES, false);
        }

        //Note that a NEGATIVE percent is a LOSS of continence
        public void ChangeContinence(IncidentType type, float percent = 0.01f)
        {
            //OLD: If we're increasing, no need to warn. (maybe we should tell people that they're regaining?)
            // We now only return if something has changed. Otherwise we can handle changes now in both directions (new)
            if (percent == 0f)
                return;
            float previousContinence = GetContinence(type);
            SetContinence(type, previousContinence + percent);

            //Change of bladder capacity is no longer nessesary. Handled by getter.
            if (Regression.config.Debug) Regression.monitor.Log(string.Format("{0} continence changed by {1} to {2}, {0} capacity now {3}", type == IncidentType.POOP ? "Bowel" : "Bladder", GetContinence(type) - previousContinence, GetContinence(type), GetCapacity(type)), LogLevel.Debug);

            //Warn that we may be losing control
            if (type == IncidentType.POOP)
            {
                Warn(previousContinence, bowelContinence, BOWEL_CONTINENCE_THRESHOLDS, BOWEL_CONTINENCE_MESSAGES, true);
            }
            else
            {
                Warn(previousContinence, bladderContinence, BLADDER_CONTINENCE_THRESHOLDS, BLADDER_CONTINENCE_MESSAGES, true);
            }
        }

        //Put on underwear
        public void ChangeUnderwear(Underwear uwNew, Underwear uwOld, string npcName = null)
        {
            this.underwear.ResetToDefault(uwNew.container);
            string msg = "";

            if (npcName != null)
            {
                msg = Strings.RandString(changeData.Change_Underwear_by_Npc);
                msg = Strings.ReplaceNpcName(msg, npcName);
                
            }
            else
            {
                msg = Strings.RandString(changeData.Change_Underwear);
            }

            msg = Strings.ReplaceUnderwearToken(msg, uwNew.container, npc: npcName != null);
            msg = Strings.ReplaceUnderwearToken(msg, uwNew.container, npc: npcName != null,oldNew: Strings.OldNew.New);
            msg = Strings.ReplaceInspectUnderwearToken(msg, uwOld.container, npc: npcName != null, Strings.OldNew.Old);

            Dialoges.Say(msg, this);
        }

        public void ChangeUnderwearAndPants(Underwear uwNew, Underwear uwOld, Container pantsNew, Container pantsOld, string npcName = null)
        {
            this.underwear.ResetToDefault(uwNew.container);
            string msg = "";

            if (npcName != null)
            {
                ResetPants(newPants: pantsNew, npcName: npcName);
                msg = Strings.RandString(changeData.Change_Underwear_Pants_by_Npc);
                msg = Strings.ReplaceNpcName(msg, npcName);           
            }
            else
            {
                ResetPants(newPants: pantsNew);
                msg = Strings.RandString(changeData.Change_Underwear_Pants);
            }

            msg = Strings.ReplaceUnderwearToken(msg, uwNew.container, npc: npcName != null);
            msg = Strings.ReplaceUnderwearToken(msg, uwNew.container, npc: npcName != null, oldNew: Strings.OldNew.New);
            msg = Strings.ReplaceInspectUnderwearToken(msg, uwOld.container, npc: npcName != null, Strings.OldNew.Old);
            msg = Strings.ReplacePantsToken(msg, pantsNew, npc: npcName != null, Strings.OldNew.New);
            msg = Strings.ReplacePantsToken(msg, pantsOld, npc: npcName != null, Strings.OldNew.Old);

            Dialoges.Say(msg, this);
        }

        public StardewValley.Objects.Clothing GetPantsStardew()
        {
            var farmer = Animations.player;
            StardewValley.Objects.Clothing pants = (StardewValley.Objects.Clothing)farmer.pantsItem.Value;
            if (pants != null) pants.LoadData(); // YES, this is nessesary to load the data of the pants before accessing them... if there are pants (watch out for null)

            return pants;
        }        
       
        public void ResetPants(Container newPants = null, string npcName = null)
        {
            var myPants = GetPantsStardew();

            if(newPants == null)
            {
                newPants = Container.GetTypeDefault(myPants == null ? "legs" : Animations.player.Gender == Gender.Female ? "skirt" : "blue jeans", ContainerSubtype.Pants);
            }

            pants.ResetToDefault(newPants,0,0);

            /*
            if (myPants != null)
            {
                pants.displayName = myPants.displayName.ToLower();
                pants.description = myPants.displayName.ToLower();
            }
            */

            if (npcName != null) CleanPantsByNpc(npcName);
            else CleanPants();

            
        }

        //If we put on our pants, remove wet/messy debuffs
        public void CleanPants()
        {
            RemoveBuff(WET_DEBUFF.debuff_id);
            RemoveBuff(MESSY_DEBUFF.debuff_id);
        }
        public void CleanPantsByNpc(string npcName)
        {
            RemoveBuff(WET_DEBUFF.debuff_id);
            RemoveBuff(MESSY_DEBUFF.debuff_id);
            if (Game1.player.dialogueQuestionsAnswered.Contains("dirty_change_yes" + "_" + npcName.ToLower()))
                Game1.player.dialogueQuestionsAnswered.Remove("dirty_change_yes" + "_" + npcName.ToLower());
            if (Game1.player.dialogueQuestionsAnswered.Contains("dirty_change_no" + "_" + npcName.ToLower()))
                Game1.player.dialogueQuestionsAnswered.Remove("dirty_change_no" + "_" + npcName.ToLower());

        }
        public bool HasWetOrMessyDebuff()
        {
            return Game1.player.hasBuff(MESSY_DEBUFF.debuff_id) || Game1.player.hasBuff(WET_DEBUFF.debuff_id);
        }

        //Debug Function, Add a bit of everything
        public void DecreaseEverything()
        {
            AddWater(requiredWaterPerDay * -0.1f, 0f);
            AddFood(requiredCaloriesPerDay * -0.1f, 0f);
            AddResource(IncidentType.PEE, maxBladderCapacity * -0.1f);
            AddResource(IncidentType.POOP, maxBladderCapacity * -0.1f);
        }

        public void IncreaseEverything()
        {
            AddWater(requiredWaterPerDay * 0.1f, 0f);
            AddFood(requiredCaloriesPerDay * 0.1f, 0f);
            AddResource(IncidentType.PEE, maxBladderCapacity * 0.5f);
            AddResource(IncidentType.POOP, maxBladderCapacity * 0.5f);
        }

        public void DrinkWateringCan()
        {
            Farmer player = Game1.player;
            WateringCan currentTool = (WateringCan)player.CurrentTool;
            // Half a watering can should be good in any case?!
            if (currentTool.WaterLeft * 200 >= thirst)
            {
                this.AddWater(thirst);
                currentTool.WaterLeft -= (int)(thirst * 200f);
                Animations.AnimateDrinking(false);
            }
            else if (currentTool.WaterLeft > 0)
            {
                this.AddWater(currentTool.WaterLeft * 200);
                currentTool.WaterLeft = 0;
                Animations.AnimateDrinking(false);
            }
            else
            {
                player.doEmote(4);
                Game1.showRedMessage("Out of water");
            }
        }

        public void DrinkWaterSource()
        {
            this.AddWater(thirst);
            Animations.AnimateDrinking(true);
        }

        public bool InToilet(bool inUnderwear)
        {
            //return !inUnderwear && (Game1.currentLocation is FarmHouse || Game1.currentLocation is IslandFarmHouse || Game1.currentLocation.Name == "BathHouse_MensLocker" || Game1.currentLocation.Name == "BathHouse_WomensLocker");
            return !inUnderwear && IsOnToiletSeat();
        }

        private bool IsOnToiletSeat()
        {
            Farmer player = Game1.player;
            if (player == null)
                return false;

            object sittingFurnitureRef = typeof(Farmer).GetField("sittingFurniture", BindingFlags.Public | BindingFlags.Instance)?.GetValue(player)
                ?? typeof(Farmer).GetProperty("sittingFurniture", BindingFlags.Public | BindingFlags.Instance)?.GetValue(player);
            object furnitureValue = sittingFurnitureRef?.GetType().GetProperty("Value", BindingFlags.Public | BindingFlags.Instance)?.GetValue(sittingFurnitureRef)
                ?? sittingFurnitureRef;

            if (furnitureValue is Furniture furniture)
            {
                string itemId = furniture.ItemId ?? string.Empty;
                if (Toilets.IsToiletFurnitureId(itemId))
                    return true;
            }

            return false;
        }

        public bool InPlaceWithPants()
        {
            return Game1.currentLocation is FarmHouse;
        }
        public bool NeedsChangies(IncidentType type)
        {
            // If the underwear was already used and the capacity remaining is smaller than the size of an accident of this type
            return underwear.GetUsed(type) > 0 && underwear.GetCapacity(type) - underwear.GetUsed(type) < GetCapacity(type);
        }

        public bool IsTryingToHoldIt(IncidentType type, float vsAmount = 0)
        {
            float capacity = underwear.GetCapacity(type);
            float used = underwear.GetUsed(type);

            //if (!underwear.removable) return false; // If we don't have pants or training pants, there is no point
            if (used > 350) return false; // If the underwear is already heavily used, we stop trying
            if (used > GetCapacity(type) / 3) return false; // If its more than 1/3 our bladder/bowel size already, we stop trying
            if ((vsAmount + used) > capacity) return false; // If the underwear would be more than full, there is no point

            return true;
        }
        // Minor incident on very full bladder/bowel. Balances the players chances to get to the potty, even if that means minor incidents. Also obviously cute
        public bool MinorAccident(IncidentType type)
        {
            float capacity = GetCapacity(type);
            float fullness = GetFullness(type);
            float fullnessPercent = fullness / capacity;
            if (fullnessPercent < lastWarningThreshold) return false; // Just to make it easily readable that this is for after and inside the last warning threshold
            if (fullnessPercent > 1.1f) return false; // If its too much, leaking will not do

            // We can lose maximal the amount until the last warning threshold, because we would trigger that warning again. This only happens after the last warning anyway.
            float amount = Math.Min((fullnessPercent - lastWarningThreshold - 0.01f) * capacity, GetMaxCapacity(type) * 0.8f);
            if (!IsTryingToHoldIt(type, amount)) return false;

            if (type == IncidentType.POOP)
            {
                Animations.AnimateMessingMinor(this);
            }
            else
            {
                Animations.AnimateWettingMinor(this);
            }

            ChangeContinence(type, CalculateContinenceLossOrGain(type, false, true, amount / GetMaxCapacity(type)));
            AddAccidentFromFullness(type, amount);
            return true;
        }
        public void ChangeFullness(IncidentType type, float amount)
        {
            // We intentionally allow fullness over max 
            switch (type)
            {
                case IncidentType.PEE:
                    SetFullness(type, bladderFullness + amount);
                    break;
                case IncidentType.POOP:
                    SetFullness(type, bowelFullness + amount);
                    break;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public void AddAccidentFromFullness(IncidentType type, float amount)
        {
            ChangeFullness(type, -amount);
            AddAccident(type, amount);
        }
        public void AddAccident(IncidentType type, float amount)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    if (isSleeping)
                    {
                        _ = this.bed.AddPee(this.underwear.AddPee(amount));
                    }
                    else
                    {
                        _ = this.pants.AddPee(this.underwear.AddPee(amount));
                    }
                    break;
                case IncidentType.POOP:
                    if (isSleeping)
                    {
                        _ = this.bed.AddPoop(this.underwear.AddPoop(amount));
                    }
                    else
                    {
                        _ = this.pants.AddPoop(this.underwear.AddPoop(amount));
                    }
                    break;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }

        }
        public void Accident(IncidentType type, bool voluntary = false, bool inUnderwear = true)
        {
            float attemptThreshold = GetAttemptThreshold(type);
            float capacity = GetCapacity(type);
            float fullness = GetFullness(type);

            float maxCapacity = GetMaxCapacity(type); // yes, this is the maximum capacity a bladder/bowel can have, used for changing continence

            //If we're sleeping check if we have an accident or get up to use the potty
            if (isSleeping)
            {
                //When we're sleeping, our fullness can exceed our capacity since we calculate for the whole night at once
                //Hehehe, this may be evil, but with a smaller bladder/bowel, you'll have to go multiple times a night
                //So roll the dice each time >:)
                //<TODO>: Give stamina penalty every time you get up to go potty. Since you disrupted sleep.
                int numIncidentAmount = (int)((fullness - attemptThreshold) / capacity);
                float additionalAmount = fullness - (numIncidentAmount * capacity);
                int numAccident = 0;
                int numPotty = 0;

                if (additionalAmount > 0)
                    numIncidentAmount++;

                for (int i = 0; i < numIncidentAmount; i++)
                {
                    float adjustedContinence = GetContinence(type) - GetContinenceMin(type);
                    //Randomly decide if we get up. Less likely if we have lower continence
                    //Up to 80% control we never have accidents and at min-continence we always have accidents
                    bool lclVoluntary = voluntary || GetContinence(type) > 0.8f || adjustedContinence > 0 && Regression.rnd.NextDouble() < adjustedContinence;
                    StartAccident(type, lclVoluntary && underwear.removable, true); //Always in underwear in bed
                    float amountToLose = (i != numIncidentAmount - 1) ? capacity : additionalAmount;
                    if (!lclVoluntary)
                    {
                        numAccident++;
                        ChangeContinence(type, CalculateContinenceLossOrGain(type, voluntary, inUnderwear, amountToLose / maxCapacity)); // maxCapacity is correct, for balancing reasons
                                                                                                                                         //Any overage in the container, add to the pants. Ignore overage over that.
                                                                                                                                         //When sleeping, the pants are actually the bed
                        AddAccidentFromFullness(type, amountToLose);
                    }
                    else
                    {
                        numPotty++; // we still count it as potty success to generate the message later (to know if we even tried)
                        if (!underwear.removable) //Certain underwear can't be taken off to use the toilet (ie diapers)
                        {
                            numAccident++;
                            ChangeContinence(type, CalculateContinenceLossOrGain(type, voluntary, inUnderwear, amountToLose / maxCapacity)); // maxCapacity is correct, for balancing reasons
                            AddAccidentFromFullness(type, amountToLose);
                        }
                        else
                        {
                            ChangeFullness(type, -amountToLose);
                        }
                    }
                }
                switch (type)
                {
                    case IncidentType.PEE:
                        numPottyPeeAtNight = numPotty;
                        numAccidentPeeAtNight = numAccident;
                        break;
                    case IncidentType.POOP:
                        numPottyPooAtNight = numPotty;
                        numAccidentPooAtNight = numAccident;
                        break;
                    default:
                        throw new Exception("Not implemented: type " + type.ToString());
                }

            }
            else if (inUnderwear)
            {
                StartAccident(type, voluntary, true);
                bool attemptOnly = fullness < attemptThreshold;
                if (!attemptOnly)
                {
                    ChangeContinence(type, CalculateContinenceLossOrGain(type, voluntary, inUnderwear, fullness / maxCapacity)); // yes this is correct, we do relative to max bladder to temper loss on near no control (and frequent accidents)
                    AddAccidentFromFullness(type, fullness);
                }
                FinalizeAccident(type, voluntary, true, attemptOnly); // Trying in your underwear is different, people might be acting differently
            }
            else
            {

                if (underwear.removable)
                {
                    bool attemptOnly = fullness < attemptThreshold;
                    StartAccident(type, voluntary, false);
                    if (!attemptOnly)
                    {
                        ChangeContinence(type, CalculateContinenceLossOrGain(type, voluntary, inUnderwear, fullness / capacity)); // yes this is correct, its capacity as the gain (for making it) should be relative to the bladder state, not max
                        ChangeFullness(type, -fullness);
                    }
                    FinalizeAccident(type, voluntary, true, attemptOnly); // Trying in your underwear is different, people might be acting differently
                }

            }
        }
        public void Mess(bool voluntary = false, bool inUnderwear = true)
        {
            Accident(IncidentType.POOP, voluntary, inUnderwear);
        }
        public void StartAccident(IncidentType type, bool voluntary = false, bool inUnderwear = true)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    if (!Regression.config.Wetting) return;
                    break;
                case IncidentType.POOP:
                    if (!Regression.config.Messing) return;
                    break;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
            float attemptThreshold = GetAttemptThreshold(type);
            float fullness = GetFullness(type);

            if (fullness < attemptThreshold)
            {
                switch (type)
                {
                    case IncidentType.PEE:
                        Animations.AnimatePeeAttempt(this, inUnderwear);
                        break;
                    case IncidentType.POOP:
                        Animations.AnimatePoopAttempt(this, inUnderwear);
                        break;
                    default:
                        throw new Exception("Not implemented: type " + type.ToString());
                }
            }
            else
            {
                switch (type)
                {
                    case IncidentType.PEE:
                        Animations.AnimateWettingStart(this, voluntary, inUnderwear);
                        break;
                    case IncidentType.POOP:
                        Animations.AnimateMessingStart(this, voluntary, inUnderwear);
                        break;
                    default:
                        throw new Exception("Not implemented: type " + type.ToString());
                }
            }
            switch (type)
            {
                case IncidentType.PEE:
                    Animations.AnimateWettingEnd(this);
                    break;
                case IncidentType.POOP:
                    Animations.AnimateMessingEnd(Game1.player);
                    break;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        // percentLost: The amount of gain or loss is dependend on how much percent of the maxBladder or maxBowel we lost
        // This allowes for small gains or losses on small accidents and reduces edge cases 
        public float CalculateContinenceLossOrGain(IncidentType type, bool voluntary, bool inUnderwear, float percentLost)
        {
            float fullness = GetFullness(type);

            int rate;
            if (voluntary && !inUnderwear)
            {
                rate = type == IncidentType.POOP ? Regression.config.BowelGainContinenceRate : Regression.config.BladderGainContinenceRate;
            }
            else
            {
                rate = type == IncidentType.POOP ? Regression.config.BowelLossContinenceRate : Regression.config.BladderLossContinenceRate;
            }


            //If we have an accident (not voluntary) or just go in our pants, decrease continence
            if (!voluntary || inUnderwear)
                return -0.01f * rate * percentLost * situationMultiplier(voluntary, inUnderwear);
            else if (fullness > GetTrainingThreshold(type) && InToilet(inUnderwear)) //If we REALLY hafe to go AND use toilet, increase continence
                return 0.01f * rate * percentLost * situationMultiplier(voluntary, inUnderwear);
            else
            {
                //Otherwise, if it is voluntary but just peed/pooped on the ground, don't change anything
                return 0f;
            }
        }
        private void FinalizeAccident(IncidentType type, bool voluntary = false, bool inUnderwear = true, bool attemptOnly = false)
        {
            var dirt = type == IncidentType.POOP ? pants.messiness : pants.wetness;
            if (!this.InToilet(inUnderwear))
                _ = Animations.HandleVillager(this, type == IncidentType.POOP, inUnderwear, dirt > 0, false);
            if (attemptOnly || dirt <= 0.0 || !inUnderwear)
                return;
            HandleOverflow(type);
        }

        private void HandleOverflow(IncidentType type)
        {
            if (isSleeping)
                return;

            switch (type)
            {
                case IncidentType.PEE:
                    _HandlePeeOverflow();
                    break;
                case IncidentType.POOP:
                    _HandlePoopOverflow();
                    break;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        // Overflow functions are so different that it is less confusing to keep them as-is

        private void _HandlePeeOverflow()
        {
            Dialoges.Write(peePoopData.Pee_Overflow, this, null, AnimationConstants.peeAnimationTime);

            int defenseReduction = -Math.Max(Math.Min((int)(pants.wetness / pants.absorbency * 10.0), 10), 1);

            Buff buff = new Buff(id: WET_DEBUFF.debuff_id, displayName: WET_DEBUFF.name, effects: new BuffEffects()
            {
                Defense = { defenseReduction }
            })
            {
                description = string.Format("{0} {1} {2}.", Strings.RandString(WET_DEBUFF.descriptions), defenseReduction, WET_DEBUFF.type),
                millisecondsDuration = 1080000,
                glow = pants.messiness != 0.0 ? Color.Brown : Color.Yellow
            };
            if (Game1.player.hasBuff(WET_DEBUFF.debuff_id))
                this.RemoveBuff(WET_DEBUFF.debuff_id);
            Game1.player.applyBuff(buff);
        }

        private void _HandlePoopOverflow()
        {
            Dialoges.Write(peePoopData.Poop_Overflow, this, null, AnimationConstants.poopAnimationTime);
            float howMessy = pants.messiness / pants.containment;
            int speedReduction = howMessy >= 0.5 ? (howMessy > 1.0 ? -3 : -2) : -1;
            Buff buff = new Buff(id: MESSY_DEBUFF.debuff_id, displayName: MESSY_DEBUFF.name, effects: new BuffEffects()
            {
                Speed = { speedReduction }
            })
            {
                description = string.Format("{0} {1} {2}.", Strings.RandString(MESSY_DEBUFF.descriptions), (object)speedReduction, MESSY_DEBUFF.type),
                millisecondsDuration = 1080000,
                glow = Color.Brown
            };
            if (Game1.player.hasBuff(MESSY_DEBUFF.debuff_id))
                this.RemoveBuff(MESSY_DEBUFF.debuff_id);
            Game1.player.applyBuff(buff);
        }
        public float situationMultiplier(bool voluntary, bool inUnderwear)
        {
            float multiplier = 1.0f;
            // If we are sleeping, night time modifiers apply. As the player is no longer in charge we use other values for game balance reasons
            if (isSleeping)
            {
                multiplier = (voluntary ? (float)Regression.config.NighttimeGainMultiplier : (float)Regression.config.NighttimeLossMultiplier) / 100f;
            }
            else if (voluntary && inUnderwear)
            {
                multiplier *= (Regression.config.InUnderwearOnPurposeMultiplier / 100f);
            }

            return multiplier;
        }
        public void WetAndMess(bool voluntary = false, bool inUnderwear = true)
        {
            Wet(voluntary, inUnderwear);
            Mess(voluntary, inUnderwear);
        }
        public void Wet(bool voluntary = false, bool inUnderwear = true)
        {
            Accident(IncidentType.PEE, voluntary, inUnderwear);
        }

        public void HandleMorning()
        {
            isSleeping = false;
            var dryingTime = 0;
            if (Regression.config.Easymode)
            {
                hunger = 0;
                thirst = 0;
            }
            else
            {

                Farmer player = Game1.player;
                if (bed.messiness > 0.0)
                {
                    dryingTime = 800;
                    player.stamina -= 60f;
                }
                else if (bed.wetness > 0.0)
                {
                    dryingTime = 600;
                    player.stamina -= 40f;
                }

                int timesUpAtNight = Math.Max(numPottyPeeAtNight, numPottyPooAtNight);
                player.stamina -= (timesUpAtNight * wakeUpPenalty);

            }

            Animations.AnimateMorning(this);
            bed.Wash(dryingTime);
            ResetPants(newPants: this.pants);
        }

        public void HandleNight()
        {
            isSleeping = true;
            if (bedtime <= 0)
                return;

            //How long are we sleeping? (Minimum of 4 hours)
            const int timeInDay = 2400;
            const int wakeUpTime = timeInDay + 600;
            const float sleepRate = 3.0f; //Let's say body functions change @ 1/3 speed while sleeping. Arbitrary.
            int timeSlept = wakeUpTime - bedtime; //Bedtime will never exceed passout-time of 2:00AM (2600)

            // Resets
            numPottyPeeAtNight = 0;
            numAccidentPeeAtNight = 0;
            numPottyPooAtNight = 0;
            numAccidentPooAtNight = 0;

            HandleTime(timeSlept / 100.0f / sleepRate);
        }

        //If Stamina has decreased, Use up Food and water along with it
        public void HandleStamina()
        {
            float staminaDifference = (float)(Game1.player.stamina - this._lastStamina) / Game1.player.maxStamina.Value;
            if ((double)staminaDifference == 0.0)
                return;
            if (staminaDifference < 0.0)
            {
                this.AddFood(staminaDifference * requiredCaloriesPerDay * 0.25f);
                this.AddWater(staminaDifference * requiredWaterPerDay * 0.10f);
            }
            this._lastStamina = Game1.player.stamina;
        }


        public void HandleTime(float hours)
        {
            this.HandleStamina();
            //normally divide 24hr/day, but this only happens while awake,
            //We have night set to go at 1/3 rate. Assume 8hr sleep. So we need to adjust by 8*(2/3)
            this.AddWater((float)(requiredWaterPerDay * (double)hours / -18.67));
            this.AddFood((float)(requiredCaloriesPerDay * (double)hours / -18.67));
        }

        public static bool IsFishing()
        {
            FishingRod currentTool;
            return (currentTool = Game1.player.CurrentTool as FishingRod) != null && (currentTool.isCasting || currentTool.isTimingCast || (currentTool.isNibbling || currentTool.isReeling) || currentTool.castedButBobberStillInAir || currentTool.pullingOutOfWater);
        }

        public void RemoveBuff(string debuffName)
        {
            Game1.player.buffs.Remove(debuffName);
        }

        public static string[] GetMessagesByTreshold(float oldPercent, float newPercent, float[] thresholds, string[][] msgs)
        {
            if (newPercent > oldPercent)
            {
                for (int index = thresholds.Length - 1; index > 0; --index)
                {
                    if ((double)oldPercent <= (double)thresholds[index] && (double)newPercent > (double)thresholds[index])
                    {
                        if (thresholds.Length - 1 > index)
                        {
                            return msgs[index + 1];
                        }

                    }
                }

            }
            else
            {
                for (int index = 0; index < thresholds.Length; ++index)
                {
                    if ((double)oldPercent > (double)thresholds[index] && (double)newPercent <= (double)thresholds[index])
                    {
                        return msgs[index];
                    }
                }
            }

            return null;
        }
        public void Warn(float oldPercent, float newPercent, float[] thresholds, string[][] msgs, bool write = false)
        {
            if (isSleeping)
                return;

            var messages = GetMessagesByTreshold(oldPercent, newPercent, thresholds, msgs);
            if (messages == null) return;
            if (write)
            {
                Dialoges.Write(messages, this);
            }
            Dialoges.Warn(messages, this);
        }


        public void Consume(Item item)
        {
            // It seams like the worth in health recovery is twice
            var foodWorth = item.staminaRecoveredOnConsumption() + item.healthRecoveredOnConsumption() * 2;
            // This value ends up 25 + 22 = 47 for green beans, parsnip, potatoes and 50 + 44 = 94 for kale and is used as a messure of effective food in the game. As such a good start for our calculation.
            // Cooked foods naturally have higher values, for example stir fry, having: 38 17 cave carrot + 30 13 mushroom + 50 22 kale + 25 11 oil (corn) = 143 63 ingredien worth => 200 90 as Stir Fry
            // => As we double the health value this is 200 + 180 = 380 
            // Having a base value, we can now freely choose a muliplier to control the difficulty of survival. If we go from Stir fry, a difficult dish that definitly should fill us up for the day (days are short), should be around 3500 kcal = 9,2 muliplier.
            // We go with 10 should be more than difficult already, as that means a parsnip/potato/... has only 470 calories anyway, 940 for kale
            foodWorth = foodWorth * 10;

            string itemName = item.Name;
            Consumable consumable;
            if (consumablesData.Consumables.TryGetValue(itemName, out consumable))
            {
                this.AddWater(consumable.waterContent);
            }
            else
            {
                this.AddWater(10);
            }

            this.AddFood(foodWorth);
        }
    }
    public enum IncidentType
    {
        PEE = 0,
        POOP = 1
    }
}
