using Microsoft.Xna.Framework;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewValley;

namespace RegressionMod
{
    public class NpcBody
    {

        public NPC npc;
        public NpcBody(NPC npc)
        {
            this.npc = npc;
        }
        
        // a very simplified accident mechanic that assumes that the npc has an accident.
        public void accident(IncidentType type, float accidentSize)
        {
            if (type == IncidentType.PEE)
            {
                underwear.AddPee(accidentSize);
                if (isWatching)
                {
                    npc.movementPause = 3000;
                    npc.doEmote(28, false);
                    Game1.playSound("wateringCan");
                }
                if (Regression.config.Debug) Regression.monitor.Log($"{npc.Name} piddled themselfs", LogLevel.Debug);


            }
            else
            {
                accidentSize = (accidentSize * 1.3f);
                underwear.AddPoop(accidentSize);
                if (isWatching)
                {
                    npc.movementPause = 3000;
                    npc.doEmote(12, false);
                    Animations.AnimateMessingEnd(npc);
                }
                if (Regression.config.Debug) Regression.monitor.Log($"{npc.Name} pooped themselfs", LogLevel.Debug);
            }
        }
        public void accidentFromFullness(IncidentType type)
        {
            var fullness = GetFullness(type);
            var accidentSize = (float)Regression.rnd.Next(300, 800);
            if (type == IncidentType.PEE)
            {
                if (fullness < 200) fullness += 200;
            }
            else
            {
                if (fullness < 300) fullness += 300;
            }
            accident(type, fullness);
            SetFullness(type, 0);
        }

        public void change(string underwearName = null, string pantsName = null)
        {
            var underwear = this.underwear;
            if (underwearName != null)
            {
                underwear.ResetToDefault(underwearName, ContainerSubtype.Underwear);
            }
            else
            {
                underwear.ResetToDefault(defaultUnderwear, ContainerSubtype.Underwear);
            }

            var pants = this.pants;
            pants.ResetToDefault(pants, 0, 0);
            if (Regression.config.Debug) Regression.monitor.Log($"{npc.Name} got changed and is now wearing {underwear.name} and {pants.name}", LogLevel.Debug);
        }

        // can npc get changed by player with dialogue very nice
        public bool canGetChangedByPlayerVeryNice
        {
            get
            {
                // special for Vincent and Jas if the option "Children in Diapers" is deactivated
                if (!Regression.ChildrenAndDiapers && (npc.Name.ToLower() == NpcConstants.NpcVincent || npc.Name.ToLower() == NpcConstants.NpcJas)) return false;

                return CheckSingleChangingOption(ChangingOptions.get_changed_verynice);
            }
        }

        // can npc get changed by player with dialogue nice
        public bool canGetChangedByPlayerNice
        {
            get
            {
                // special for Vincent and Jas if the option "Children in Diapers" is deactivated
                if (!Regression.ChildrenAndDiapers && (npc.Name.ToLower() == "vincent" || npc.Name.ToLower() == "jas")) return false;

                return CheckSingleChangingOption(ChangingOptions.get_changed_nice);
            }
        }

        // can npc give the player a change with dialogue very nice
        public bool canGivePlayerChangeVeryNice
        {
            get
            {
                return CheckSingleChangingOption(ChangingOptions.give_change_verynice);
            }
        }

        // can npc give the player a change with dialogue nice
        public bool canGivePlayerChangeNice
        {
            get
            {
                return CheckSingleChangingOption(ChangingOptions.give_change_nice);
            }
        }

        // can npc give the player a change when they messed there pants
        public bool canGivePlayerChangeDirty
        {
            get
            {
                return CheckSingleChangingOption(ChangingOptions.give_dirty_change);
            }
        }

        private bool CheckSingleChangingOption(SingleNpcChangingOption changingOption)
        {
            int heartLevelForNpc = Game1.player.getFriendshipHeartLevelForNPC(npc.Name);

            // There are changing options for this npc in Regression.json

            if (changingOption != null && changingOption.hasOptions)
            {
                bool optionMinHeartLevel = false;
                bool optionMinHeartLevelOther = false;
                bool optionLocations = false;
                bool optionQuestionsAnswered = false;

                // The changing options contains a option for minheart_level

                if (changingOption.hasOptionMinHeartLevel)
                {
                    if (heartLevelForNpc >= changingOption.min_heart_level) optionMinHeartLevel = true;
                }

                // The changing options contains a option for minheart_level_other

                if (changingOption.hasOptionMinHeartLevelOther)
                {
                    int counter = 0;

                    foreach (KeyValuePair<string, int> keyValuePair in changingOption.min_heart_level_other)
                    {
                        string name = keyValuePair.Key;
                        int heartLevel = keyValuePair.Value;

                        if (Game1.player.getFriendshipHeartLevelForNPC(name) >= heartLevel) counter++;
                    }

                    // all entries must be true

                    if (changingOption.min_heart_level_other.Count == counter) optionMinHeartLevelOther = true;

                }

                // The changing options contains a option for locations

                if (changingOption.hasOptionLocations)
                {
                    int counter = 0;

                    foreach (KeyValuePair<string, int> keyValuePair in changingOption.locations)
                    {
                        string locationName = keyValuePair.Key;
                        int heartLevel = keyValuePair.Value;

                        if(locationName == "home")
                        {
                            if(npc.currentLocation == npc.getHome()) counter++;
                        }
                        else
                        {
                            if (npc.currentLocation.Name == locationName && heartLevelForNpc >= heartLevel) counter++;
                        }
                    }

                    // one entrie must be true

                    if (counter > 0) optionLocations = true;
                }

                // The changing options contains a option for question_answered

                if (changingOption.hasOptionQuestionnAnswered)
                {
                    int counter = 0;

                    foreach (KeyValuePair<string, int> keyValuePair in changingOption.questions_answered)
                    {
                        string responceId = keyValuePair.Key;
                        int heartLevel = keyValuePair.Value;

                        if (Game1.player.dialogueQuestionsAnswered.Contains(responceId) && heartLevelForNpc >= heartLevel) counter++;
                    }

                    // one entrie must be true

                    if (counter > 0) optionQuestionsAnswered = true;
                }

                if (optionMinHeartLevel || optionMinHeartLevelOther || 
                    optionLocations || optionQuestionsAnswered) return true;
            }

            return false;
        }

        public float GetFullnessMax(IncidentType type)
        {
            switch (npc.Name.ToLower())
            {
                case "vincent":
                    if (type == IncidentType.PEE) return 400;
                    return 900;
                case "jas":
                    if (type == IncidentType.PEE) return 400;
                    return 800;
                case "sam":
                    if (type == IncidentType.PEE) return 550;
                    return 900;
                default:
                    if (type == IncidentType.PEE) return 700;
                    return 1000;
            }
        }
        public bool GetSuccessNext(IncidentType type)
        {
            var key = $"Potty/{type}/Success";
            if (!npc.modData.ContainsKey(key)) return false;
            return bool.Parse(npc.modData[key]);
        }
        public void SetSuccessNext(IncidentType type, bool value)
        {
            var key = $"Potty/{type}/Success";
            npc.modData[key] = value.ToString();
        }
        public float GetFullness(IncidentType type)
        {
            var key = $"Potty/{type}/Fullness";
            if (!npc.modData.ContainsKey(key)) return 0.0f;
            return float.Parse(npc.modData[key]);
        }
        public void SetFullness(IncidentType type, float value)
        {
            var key = $"Potty/{type}/Fullness";
            npc.modData[key] = value.ToString();
        }
        public bool isWatching
        {
            get
            {
                if (Game1.player.currentLocation != npc.currentLocation) return false; // Forcing silent if different map
                else if (Vector2.Distance(Game1.player.Tile, npc.Tile) > 30) return false; // If the distance is to far, we silence it
                return true;
            }
        }

        public bool RandomAction(float hours)
        {
            if (npc == null) return false;
            if (!npc.IsVillager) return false;

            float getChangedChance = GetNpcChaningChance();

            if (getChangedChance > 0 && (underwear.messiness > 0 || underwear.wetness > 0) && Regression.rnd.NextDouble() <= getChangedChance * hours)
            {
                if (isWatching) npc.doEmote(12, false);
                change();
                return true;
            }

            if (PottyChanceIncidentWorker(IncidentType.PEE)) return true;
            if (PottyChanceIncidentWorker(IncidentType.POOP)) return true;


            return false;
        }

        private float GetNpcChaningChance()
        {
            if(ChangingOptions != null && ChangingOptions.hasOptionChangeAutomaticly)
            {
                NpcChangeAutomaticly changeAutomaticly = ChangingOptions.change_automaticly;

                // Npc is in situation that changing is possible
                bool canGetChanged = false;

                // Npc is curently at there home
                bool atHome = npc.currentLocation == npc.getHome();

                if (atHome)
                {
                    if(changeAutomaticly.at_home != null && changeAutomaticly.at_home.Count > 0)
                    {
                        bool npcInRange = false;
                        bool underwearSelfchange = false;

                        // The changing options contains a option for npc_in_range at home

                        if (changeAutomaticly.hasOptionHomeNpc)
                        {
                            foreach(string otherNpc in changeAutomaticly.AtHomeNpcs)
                            {
                                if(NpcHelper.NpcInRange(npc,otherNpc)) 
                                { 
                                    npcInRange = true;
                                    break;
                                }
                            }
                        }

                        // The changing options contains a option for wearing_underwear at home

                        if (changeAutomaticly.hasOptionHomeUnderwear)
                        {
                            if (changeAutomaticly.AtHomeUnderwear.Length == 1 && changeAutomaticly.AtHomeUnderwear[0] == "all") underwearSelfchange = true;
                            else
                            {
                                foreach (string underwearType in changeAutomaticly.AtHomeUnderwear)
                                {
                                    if (underwear.type == underwearType)
                                    {
                                        underwearSelfchange = true;
                                        break;
                                    }
                                }
                            }
                        }

                        if (npcInRange || underwearSelfchange) canGetChanged = true;
                    }
                }
                else
                {
                    if (changeAutomaticly.outside != null && changeAutomaticly.outside.Count > 0)
                    {
                        bool npcInRange = false;
                        bool underwearSelfchange = false;
                        bool atLocation = false;

                        // The changing options contains a option for npc_in_range outside

                        if (changeAutomaticly.hasOptionOutsideNpc)
                        {
                            foreach (string otherNpc in changeAutomaticly.OutsideNpcs)
                            {
                                if (NpcHelper.NpcInRange(npc, otherNpc)) 
                                { 
                                    npcInRange = true;
                                    break;
                                }
                            }
                        }

                        // The changing options contains a option for wearing_underwear outside

                        if (changeAutomaticly.hasOptionOutsideUnderwear)
                        {
                            if (changeAutomaticly.OutsideUnderwear.Length == 1 && changeAutomaticly.OutsideUnderwear[0] == "all") underwearSelfchange = true;
                            else
                            {
                                foreach (string underwearType in changeAutomaticly.OutsideUnderwear)
                                {
                                    if (underwear.type == underwearType) 
                                    { 
                                        underwearSelfchange = true;
                                        break;
                                    }
                                }
                            }
                        }

                        // The changing options contains a option for at_location outside

                        if (changeAutomaticly.hasOptionOutsideLocations)
                        {
                            foreach (string location in changeAutomaticly.OutsideLocation)
                            {
                                if(NpcHelper.NpcAtLocation(npc,location)) 
                                {
                                    atLocation = true;
                                    break;
                                }
                            }
                        }

                        if (npcInRange || underwearSelfchange || atLocation) canGetChanged = true;
                    }
                }

                // if one of the upper conditions is true, get the chance for an automatic change

                if (canGetChanged)
                {
                    float changing_Chance = changeAutomaticly.ChangeingChanceBase;

                    // get changing chance if the npc has pooped there underwear

                    if (underwear.messiness > 0 && changeAutomaticly.ChangeingChancePoopy != 0.0f) 
                    {
                        changing_Chance = changeAutomaticly.ChangeingChancePoopy;
                    }

                    // get changing chance if the npc has peed there underwear and the underwear is more than half full

                    if (underwear.wetness > (underwear.absorbency / 2f) && changeAutomaticly.ChangeingChanceWetHalfCapacity != 0.0f) 
                    {
                        changing_Chance = changeAutomaticly.ChangeingChanceWetHalfCapacity;
                    }

                    if (underwear.messiness > (underwear.containment / 2f) && changeAutomaticly.ChangeingChanceMessyHalfCapacity != 0.0f)
                    {
                        changing_Chance = changeAutomaticly.ChangeingChanceMessyHalfCapacity;
                    }

                    return changing_Chance;
                }
                else return 0.0f;
            }

            // return 100 percent if no options where found

            return 1.0f;
        }


        private bool PottyChanceIncidentWorker(IncidentType incidentType)
        {
            float pottyChance = 1.0f;

            if(!GetPottyChance(incidentType, out pottyChance)) return false;

            Color color = incidentType == IncidentType.PEE ? new Color(byte.MaxValue, 225, 56) : new Color(146, 111, 91);
            float fullness = GetFullness(incidentType);


            float max = GetFullnessMax(incidentType);
            if (fullness >= max)
            {
                bool success = GetSuccessNext(incidentType);
                var dialogs = PottyDialogs(incidentType, false, success);
                if (dialogs != null && dialogs.Length > 0)
                {
                    npc.showTextAboveHead(Strings.RandString(dialogs), color, 1, 3000, 2000);
                }
                if (success)
                {
                    npc.doEmote(5, false);
                    npc.movementPause = 3000;
                    SetFullness(incidentType, 0);
                }
                else
                {
                    accidentFromFullness(incidentType);
                }
                SetSuccessNext(incidentType, Regression.rnd.NextDouble() < pottyChance);
                return true;

            }
            else if (fullness + 70f >= max)
            {
                bool success = GetSuccessNext(incidentType);
                var dialogs = PottyDialogs(incidentType, true, success);
                if (dialogs != null && dialogs.Length > 0)
                {
                    npc.showTextAboveHead(Strings.RandString(dialogs), color, 0, 3000, 0);
                }
            }
            SetFullness(incidentType, fullness + 35f + (35f * (float)Regression.rnd.NextDouble()));

            return false;
        }

        private bool GetPottyChance(IncidentType incidentType, out float chance)
        {
            chance = 0f;

            if (PottyOptions != null && PottyOptions.hasOptions)
            {
                bool minHeartLevel = false;
                bool questionAnswered = false;

                if (PottyOptions.hasOptionMinHeartLevel)
                {
                    if (Game1.player.getFriendshipHeartLevelForNPC(npc.Name) >= PottyOptions.min_heart_level) minHeartLevel = true;
                }
                else minHeartLevel = true;

                if (PottyOptions.hasOptionQuestionsAnswered)
                {
                    foreach (string responceId in PottyOptions.questions_answered)
                    {
                        if (Game1.player.dialogueQuestionsAnswered.Contains(responceId))
                        {
                            questionAnswered = true;
                            break;
                        }
                    }
                }
                else questionAnswered = true;

                if (minHeartLevel || questionAnswered || Regression.config.FriendshipDebugNice || Regression.config.FriendshipDebugVeryNice)
                {
                    switch (incidentType)
                    {
                        case IncidentType.PEE:
                            chance = PottyOptions.pottyChancePee;
                            return true;
                        case IncidentType.POOP:
                            chance = PottyOptions.pottyChancePoop;
                            return true;
                        default:
                            return false;
                    }
                }
            }
            return false;
        }
        public string[] PottyDialogs(IncidentType type, bool preStage, bool success)
        {
            var dialogsStorage = Regression.villagerDialogData.Villager_Potty_Dialogs;
            string stageStr = preStage ? "pre" : "post";
            string typeStr = type == IncidentType.PEE ? "pee" : "poop";
            string successStr = success ? "success" : "fail";

            Dictionary<string, Dictionary<string, Dictionary<string, string[]>>> dictionary;
            Dictionary<string, Dictionary<string, string[]>> typeDict;
            Dictionary<string, string[]> stageDict = null;
            string[] successDialogs;
            if (dialogsStorage.TryGetValue(npc.Name.ToLower(), out dictionary) && dictionary.TryGetValue(typeStr, out typeDict) && typeDict.TryGetValue(stageStr, out stageDict) && stageDict.TryGetValue(successStr, out successDialogs))
            {
                return successDialogs;
            }
            return null;
        }
        public void RemoveDialogue(string keyRemove)
        {

            // Temporary stack to hold dialogues we want to keep
            Stack<Dialogue> tempStack = new Stack<Dialogue>();

            // Iterate through the stack to find the dialogue to remove
            while (npc.CurrentDialogue.Count > 0)
            {
                Dialogue current = npc.CurrentDialogue.Pop();

                if (current.TranslationKey != keyRemove)
                {
                    tempStack.Push(current); // Keep this dialogue if it doesn't match
                }
            }

            // Restore the remaining dialogues back into the original stack
            while (tempStack.Count > 0)
            {
                npc.CurrentDialogue.Push(tempStack.Pop());
            }
        }

        public string defaultUnderwear
        {
            get
            {
                switch (npc.Name.ToLower())
                {
                    case NpcConstants.NpcVincent:
                        return "baby print diaper";
                    case NpcConstants.NpcJas:
                        return Regression.rnd.NextDouble() < 33.0 ? "training pants" : "lavender pullups"; // sometimes the training pants are all dirty
                    case NpcConstants.NpcSam:
                        return Regression.rnd.NextDouble() < 25.0 ? "training pants" : "baby print diaper"; // sometimes sam tries bigger diapers
                    case NpcConstants.NpcAbigail:
                        return Regression.rnd.NextDouble() < 50.0 ? "lavender pullups" : "joja diaper"; // sometimes abigail tries bigger diapers
                    case NpcConstants.NpcMaru:
                        return Regression.rnd.NextDouble() < 10.0 ? "lavender pullups" : "space diaper"; // sometimes maru tries bigger diapers
                    case NpcConstants.NpcEmily:
                        return "cloth diaper";  // emily tailor her own diapers
                    case NpcConstants.NpcHaley:
                        return Regression.rnd.NextDouble() < 10.0 ? "lavender pullups" : "heart diaper"; // sometimes haley tries bigger diapers
                    case NpcConstants.NpcLeah:
                        return Regression.rnd.NextDouble() < 10.0 ? "lavender pullups" : "pawprint diaper"; // sometimes leah tries bigger diapers
                    case NpcConstants.NpcPenny:
                        return Regression.rnd.NextDouble() < 10.0 ? "lavender pullups" : "heart diaper"; // sometimes haley tries bigger diapers
                    default:
                        return npc.Gender == Gender.Female ? "polka dot panties" : "big kid undies";
                }
            }
        }

        private Container _underwear;
        public Container underwear
        {
            get
            {
                if ( _underwear == null)
                {
                    _underwear = new Container(npc, ContainerSubtype.Underwear, defaultUnderwear);
                }

                return _underwear;
            }
            set { _underwear = value; } 
        }
        public string npcDefaultPantsName
        {
            get
            {
                switch (npc.Name.ToLower())
                {
                    case NpcConstants.NpcVincent:
                        return "toddler pants";
                    case NpcConstants.NpcJas:
                        return "toddler skirt";
                    case NpcConstants.NpcEmily:
                        return "self made skirt";
                    default:
                        return npc.Gender == Gender.Female ? "skirt" : "blue jeans";
                }
            }

        }

        private Container _pants;

        public Container pants
        {
            get
            {
                if (_pants == null)
                {
                    //var staticType = "blue jeans";
                    var defaultName = npcDefaultPantsName;
                    //var pants = new Container(npc, ContainerSubtype.Pants, staticType);
                    _pants = new Container(npc, ContainerSubtype.Pants, defaultName);
                    /*
                    if (pants.displayName == staticType && defaultName != "" && defaultName != pants.displayName)
                    {
                        pants.displayName = defaultName;
                        pants.description = defaultName;
                    }
                    */
                }

                return _pants;
            }
            set { _pants = value; } 
        }

        public Stack<Dialogue> CurrentDialogue
        {
            get
            {
                return npc.CurrentDialogue;
            }
        }
        public int Age
        {
            get
            {
                return npc.Age;
            }
        }



        private NpcChangingOptions _changingOptions;

        public NpcChangingOptions ChangingOptions
        {
            get
            {
                if (_changingOptions == null) _changingOptions = new NpcChangingOptions(npc.Name.ToLower());
                return _changingOptions;
            }
        }

        private NpcPottyOptions _pottyOptions;
        public NpcPottyOptions PottyOptions
        {
            get
            {
                if (_pottyOptions == null) _pottyOptions = new NpcPottyOptions(npc.Name.ToLower());
                return _pottyOptions;
            }
        }

        public List<string> GetVillagerReactions(string responseKey, string fallbackKey = null)
        {
            var npcType = NpcHelper.npcTypeList(npc);
            List<string> stringList3 = new List<string>();
            foreach (string key2 in npcType)
            {
                Dictionary<string, string[]> dictionary;
                string[] strArray;
                if (Regression.villagerDialogData.Villager_Reactions.TryGetValue(key2, out dictionary) && dictionary.TryGetValue(responseKey, out strArray))
                {
                    stringList3 = new List<string>(); // We could remove this line again, but the general texts are more meant as fallback, they often don't fit well if custom texts are defined
                    stringList3.AddRange((IEnumerable<string>)strArray);
                }
            }
            if (fallbackKey != null && stringList3.Count < 1) return GetVillagerReactions(fallbackKey, null);
            return stringList3;
        }
    }
}
