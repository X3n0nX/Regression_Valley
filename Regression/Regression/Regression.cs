using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Delegates;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using StardewValley.Tools;
using StardewValley.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace RegressionMod
{
    public class Regression : Mod
    {
        public static int lastTimeOfDay = 0;
        public static Random rnd = new Random();
        public static bool started = false;
        public static Body body;
        public static Config config;
        public static IModHelper help;
        public static IMonitor monitor;
        public bool shiftHeld;

        public static ChangeData changeData;
        public static ConsumablesData consumablesData;
        public static GeneralData generalData;
        public static PeePoopData peePoopData; 
        public static StatesContinenceData statesContinenceData;
        public static TypesData typesData;
        public static VillagerData villagerData;
        public static VilagerDialogData villagerDialogData;

        public static Farmer who;
        //public static string dirtyEventToken = "dirtyEventToken";
        public static string checkNpcEventToken = "checkNpcEventToken";
        public static string checkPlayerEventToken = "checkPlayerEventToken";
        public static bool SelfIsFurry = false;
        public static bool WorldIsFurry = false;
        const float timeInTick = (1f / 43f); //One second realtime ~= 1/43 hours in game
        public Dictionary<string, bool> jsonLoaded = new();
        public override void Entry(IModHelper h)
        {
            var harmony = new HarmonyLib.Harmony("com.primevaltitmouse.regression");
            harmony.PatchAll();

            help = h;

            monitor = Monitor;
            config = Helper.ReadConfig<Config>();

            changeData = LoadData<ChangeData>("Data/ChangeData.json");
            consumablesData = LoadData<ConsumablesData>("Data/ConsumablesData.json");
            generalData = LoadData<GeneralData>("Data/GeneralData.json");
            statesContinenceData = LoadData<StatesContinenceData>("Data/StatesContinenceData.json");
            peePoopData = LoadData<PeePoopData>("Data/PeePoopData.json");
            typesData = LoadData<TypesData>("Data/TypesData.json");
            villagerData = LoadData<VillagerData>("Data/VillagerData.json");
            villagerDialogData = LoadData<VilagerDialogData>("Data/VilagerDialogData.json");

            h.Events.GameLoop.Saving += new EventHandler<SavingEventArgs>(this.BeforeSave);
            h.Events.GameLoop.Saved += new EventHandler<SavedEventArgs>(this.AfterSave);
            h.Events.GameLoop.DayStarted += new EventHandler<DayStartedEventArgs>(ReceiveAfterDayStarted);
            h.Events.GameLoop.OneSecondUpdateTicking += new EventHandler<OneSecondUpdateTickingEventArgs>(ReceiveUpdateTick);
            h.Events.GameLoop.TimeChanged += new EventHandler<TimeChangedEventArgs>(ReceiveTimeOfDayChanged);
            h.Events.Content.AssetRequested += Toilets.OnAssetRequested;
            h.Events.GameLoop.GameLaunched += new EventHandler<GameLaunchedEventArgs>(OnGameLaunched);
            h.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(ReceiveKeyPress);
            h.Events.Input.ButtonPressed += new EventHandler<ButtonPressedEventArgs>(ReceiveMouseChanged);
            h.Events.Display.MenuChanged += new EventHandler<MenuChangedEventArgs>(ReceiveMenuChanged);
            h.Events.Display.RenderingHud += new EventHandler<RenderingHudEventArgs>(ReceivePreRenderHudEvent);

            h.Events.GameLoop.UpdateTicked += new EventHandler<UpdateTickedEventArgs>(OnUpdateTicked);

            h.Events.Player.Warped += new EventHandler<WarpedEventArgs>(OnWarped);

            h.ConsoleCommands.Add("dialog", "Triggers a debug dialog. Usage: dialog <npcName> <message>", TriggerDialogCommand);
            h.ConsoleCommands.Add("emote", "Sets an NPC's emote. Usage: set_emote <NPCName> <EmoteID>", SetEmoteCommand);

            ActionManager.RegisterActions();

            GameStateQueryDelegate queryDelegate = (GameStateQueryDelegate)Delegate.CreateDelegate(typeof(GameStateQueryDelegate), this, "DIAPER_USED");
            GameStateQuery.Register("DIAPER_USED", queryDelegate);

            GameStateQueryDelegate queryDelegateBad = (GameStateQueryDelegate)Delegate.CreateDelegate(typeof(GameStateQueryDelegate), this, "DIAPER_USED_BAD");
            GameStateQuery.Register("DIAPER_USED_BAD", queryDelegateBad);

            WorldIsFurry = Helper.ModRegistry.IsLoaded("sion9000.AnthroCharactersContinued");
            SelfIsFurry = Helper.ModRegistry.IsLoaded("krystedez.FurryFarmer");
        }

        /// <summary>
        /// Handler for the "emote" console command.
        /// Usage: set_emote <npcName> <enoteId>
        /// </summary>
        private void SetEmoteCommand(string command, string[] args)
        {
            if (args.Length < 2)
            {
                Monitor.Log("Usage: set_emote <NPCName> <EmoteID>", LogLevel.Error);
                return;
            }

            string npcName = args[0];
            if (!int.TryParse(args[1], out int emoteId))
            {
                Monitor.Log("EmoteID must be an integer.", LogLevel.Error);
                return;
            }

            NPC npc = Game1.getCharacterFromName(npcName);
            if (npc == null)
            {
                Monitor.Log($"NPC '{npcName}' not found.", LogLevel.Error);
                return;
            }

            if (emoteId < 0 || emoteId > 64)
            {
                Monitor.Log($"EmoteID '{emoteId}' is out of range. Valid IDs: 0-64.", LogLevel.Error);
                return;
            }

            npc.doEmote(emoteId, false);

            Monitor.Log($"Set {npcName}'s emote to {emoteId}.", LogLevel.Info);
        }

        /// <summary>
        /// Handler for the "dialog" console command.
        /// Usage: dialog <npcName> <message>
        /// </summary>
        private void TriggerDialogCommand(string command, string[] args)
        {
            if (args.Length < 2)
            {
                Monitor.Log("Usage: dialog <npcName> <message>", LogLevel.Error);
                return;
            }

            string npcName = args[0];
            string message = string.Join(" ", args, 1, args.Length - 1);

            TriggerDebugDialog(npcName, message);
        }

        /// <summary>
        /// Triggers a dialog for the specified NPC with the given message.
        /// </summary>
        /// <param name="npcName">Name of the NPC.</param>
        /// <param name="message">Dialog message.</param>
        private void TriggerDebugDialog(string npcName, string message)
        {
            NPC npc = Game1.getCharacterFromName(npcName);
            if (npc != null)
            {
                // Create a new Dialogue object
                Dialogue dialogue = new Dialogue(npc, "", message);

                npc.setNewDialogue(dialogue, true, true);

                // Show the dialogue to the player
                Game1.drawDialogue(npc);

                // Log to confirm the dialog was triggered
                Monitor.Log($"Dialog triggered for {npcName}: {message}", LogLevel.Info);
            }
            else
            {
                Monitor.Log($"NPC '{npcName}' not found!", LogLevel.Warn);
            }
        }

        private void OnWarped(object sender, WarpedEventArgs e)
        {
            return;
        }
        public bool DIAPER_USED(string[] query, GameStateQueryContext context)
        {
            Container underwear = body.underwear;

            string targetNpcName = query.Length > 1 ? query[1] : null;
            if (targetNpcName != null)
            {
                switch (targetNpcName)
                {
                    case "pee":
                        return underwear.wetness > 0;
                    case "poop":
                        return underwear.messiness > 0;
                    default:
                        break;
                }

                var npc = NpcHelper.GetNpcByName(targetNpcName, 20);
                if (npc == null) return false;

                underwear = npc.underwear;
                // we don't null check, that should not happen and if it does, we want to see that

                // if only one parameter used in quarry whitch is the npc name
                if (query.Length == 2 && underwear.used) return true;

            }
            switch (query.Length > 2 ? query[2]?.ToLower() : null)
            {
                case "pee":
                    return underwear.wetness > 0;
                case "poop":
                    return underwear.messiness > 0;
                default:
                    return underwear.used;
            }
        }
        public bool DIAPER_USED_BAD(string[] query, GameStateQueryContext context)
        {
            Container underwear = body.underwear;

            string targetNpcName = query.Length > 1 ? query[1] : null;
            if (targetNpcName != null)
            {
                switch (targetNpcName)
                {
                    case "pee":
                        return underwear.wetness > (underwear.absorbency / 2);
                    case "poop":
                        return underwear.messiness > (underwear.containment / 2);
                    default:
                        break;
                }
                NpcBody npc = NpcHelper.GetNpcByName(targetNpcName, 20);
                if (npc == null) return false;

                underwear = npc.underwear;
                // we don't null check, that should not happen and if it does, we want to see that

                // if only one parameter used in quarry whitch is the npc name
                if (query.Length == 2 && underwear.used_bad) return true;
            }
            switch (query.Length > 2 ? query[2]?.ToLower() : null)
            {
                case "pee":
                    return underwear.wetness > (underwear.absorbency / 2);
                case "poop":
                    return underwear.messiness > (underwear.containment / 2);
                default:
                    return underwear.used_bad; // if very wet OR slightly poopy (yes, even if a little, not like the specific questions on top)
            }
        }

        private void DebugGiveUnderwear()
        {
            List<Item> objList = new List<Item>();
            foreach (string validUnderwearType in UnderwearHelper.ValidUnderwearTypes())
                objList.Add(new Underwear(validUnderwearType, 20));
            objList.Add(new StardewValley.Object(GameConstants.ItemIdSpringOnion, 99, false, -1, 0));
            objList.Add(new StardewValley.Object(GameConstants.ItemIdWine, 99, false, -1, 0));
            Game1.activeClickableMenu = new ItemGrabMenu(objList);
        }

        private void ReceiveAfterDayStarted(object sender, DayStartedEventArgs e)
        {
            var configFile = "RegressionSave.json";
            if (config.ReadSaveFiles)
            {
                body = Helper.Data.ReadJsonFile<Body>(string.Format("{0}/{1}", Constants.SaveFolderName, configFile));
                if (body == null) body = new Body();
                else jsonLoaded[configFile] = true;
            }
            else
            {
                body = new Body();
            }

            started = true;
            who = Game1.player;

            configFile = "RegressionSaveInv.json";
            Dictionary<int, Dictionary<string, string>> invReplacement = null;
            if (config.ReadSaveFiles) invReplacement = Helper.Data.ReadJsonFile<Dictionary<int, Dictionary<string, string>>>(string.Format("{0}/{1}", Constants.SaveFolderName, configFile));
            if (invReplacement != null) jsonLoaded[configFile] = true;
            restoreItems(Game1.player.Items, invReplacement);

            configFile = "RegressionSaveChest.json";
            Dictionary<string, Dictionary<int, Dictionary<string, string>>> chestReplacement = null;
            if (config.ReadSaveFiles) chestReplacement = Helper.Data.ReadJsonFile<Dictionary<string, Dictionary<int, Dictionary<string, string>>>>(string.Format("{0}/{1}", Constants.SaveFolderName, configFile));
            if (chestReplacement != null) jsonLoaded[configFile] = true;

            int locId = 0;
            foreach (var location in GetAllLocations())
            {
                foreach (var obj in location.Objects.Values)
                {
                    var id = string.Format("{0}-{1}-{2}", locId, obj.TileLocation.X, obj.TileLocation.Y);
                    if (obj is Chest chest)
                    {
                        restoreItems(chest.Items, chestReplacement != null && chestReplacement.ContainsKey(id) ? chestReplacement[id] : null);
                    }
                }
                foreach (var furn in location.furniture.OfType<StorageFurniture>())
                {
                    restoreItems(furn.heldItems, null);
                }
                locId++;
            }

            Animations.AnimateNight(body);
            HandleMorning(sender, e);
        }

        private void HandleMorning(object Sender, DayStartedEventArgs e)
        {
            body.HandleMorning();
            Mail.CheckMail();
        }

        public static Dictionary<int, Dictionary<string, string>> replaceItems(IList<Item> items)
        {
            var replacements = new Dictionary<int, Dictionary<string, string>>();

            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] is Underwear)
                {
                    var underwear = (items[i] as Underwear);
                    items[i] = underwear.getReplacement();
                    replacements.Add(i, underwear.getAdditionalSaveData());
                }
            }

            return replacements;
        }
        public static void restoreItems(IList<Item> items, Dictionary<int, Dictionary<string, string>> invReplacement = null)
        {
            if (invReplacement != null)
            {
                foreach (KeyValuePair<int, Dictionary<string, string>> entry in invReplacement)
                {
                    var underwear = new Underwear();
                    underwear.rebuild(entry.Value, items[entry.Key]);
                    if (items[entry.Key] != null)
                    {
                        underwear.modData.CopyFrom(items[entry.Key].modData);
                    }
                    items[entry.Key] = underwear;
                }
            }


            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                // we have to access the type this way, because it's not yet of the correct class
                if (item?.modData?.ContainsKey(Container.BuildKeyFor("type", Underwear.modDataKey)) == true)
                {
                    var underwear = new Underwear();
                    underwear.modData.CopyFrom(item.modData);
                    underwear.Stack = item.Stack;
                    underwear.rebuildFromModData();
                    items[i] = underwear;
                }
            }
        }


        //Save Mod related variables in separate JSON. Also trigger night handling if not on the very first day.
        private void BeforeSave(object Sender, SavingEventArgs e)
        {
            body.bedtime = lastTimeOfDay;
            if (Game1.dayOfMonth != 1 || Game1.currentSeason != "spring" || Game1.year != 1)
                body.HandleNight();

            var chestReplacements = new Dictionary<string, Dictionary<int, Dictionary<string, string>>>();

            if (config.WriteSaveFiles)
            {
                int locId = 0;
                foreach (var location in GetAllLocations())
                {
                    foreach (var obj in location.Objects.Values)
                    {
                        if (obj is Chest chest)
                        {
                            var id = string.Format("{0}-{1}-{2}", locId, obj.TileLocation.X, obj.TileLocation.Y);
                            var replacements = replaceItems(chest.Items);
                            if (replacements.Count > 0)
                            {
                                chestReplacements[id] = replacements;
                            }
                        }
                    }

                    foreach (var furn in location.furniture.OfType<StorageFurniture>())
                    {
                        replaceItems(furn.heldItems);
                    }
                    locId++;
                }

                foreach (var farmer in Game1.getAllFarmers())
                {
                    if (farmer != Game1.player) replaceItems(farmer.Items);
                    
                    if (farmer.ActiveObject is Underwear)
                    {
                        var underwear = farmer.ActiveObject as Underwear;
                        farmer.ActiveObject = underwear.getReplacement() as StardewValley.Object;
                    }
                }
            }
            var invReplacements = config.WriteSaveFiles ? replaceItems(Game1.player.Items) : null;


            var configFile = "RegressionSave.json";
            if (config.WriteSaveFiles || jsonLoaded.ContainsKey(configFile))
            {
                Helper.Data.WriteJsonFile(string.Format("{0}/{1}", Constants.SaveFolderName, configFile), config.WriteSaveFiles ? body : null);
                if (jsonLoaded.ContainsKey(configFile)) jsonLoaded.Remove(configFile);
            }

            configFile = "RegressionSaveInv.json";
            if (config.WriteSaveFiles || jsonLoaded.ContainsKey(configFile))
            {
                Helper.Data.WriteJsonFile(string.Format("{0}/{1}", Constants.SaveFolderName, configFile), config.WriteSaveFiles ? invReplacements : null);
                if (jsonLoaded.ContainsKey(configFile)) jsonLoaded.Remove(configFile);
            }

            configFile = "RegressionSaveChest.json";
            if (config.WriteSaveFiles || jsonLoaded.ContainsKey(configFile))
            {
                Helper.Data.WriteJsonFile(string.Format("{0}/{1}", Constants.SaveFolderName, configFile), config.WriteSaveFiles ? chestReplacements : null);
                if (jsonLoaded.ContainsKey(configFile)) jsonLoaded.Remove(configFile);
            }
        }

        private void AfterSave(object Sender, SavedEventArgs e)
        {
            if (!config.WriteSaveFiles) return;

            foreach (var farmer in Game1.getAllFarmers())
            {
                restoreItems(farmer.Items);

                if (farmer.ActiveObject != null)
                {
                    var activeList = new List<Item> { farmer.ActiveObject };
                    restoreItems(activeList);
                    farmer.ActiveObject = activeList[0] as StardewValley.Object;
                }
            }

            foreach (var location in GetAllLocations())
            {
                foreach (var furn in location.furniture.OfType<StorageFurniture>())
                {
                    restoreItems(furn.heldItems);
                }
                foreach (var obj in location.Objects.Values)
                {
                    if (obj is Chest chest)
                    {
                        restoreItems(chest.Items);
                    }
                }
            }
        }

        public static IEnumerable<GameLocation> GetAllLocations()
        {
            var visited = new HashSet<GameLocation>();
            var queue = new Queue<GameLocation>(Game1.locations);
            while (queue.Count > 0)
            {
                var loc = queue.Dequeue();
                if (loc == null || visited.Contains(loc)) continue;
                visited.Add(loc);
                yield return loc;
                if (loc.buildings != null)
                {
                    foreach (var b in loc.buildings)
                    {
                        if (b.indoors.Value != null) queue.Enqueue(b.indoors.Value);
                    }
                }
            }
        }                

        public static void GetChangedByNpc(string npcName, string newUnderwearName, string newPantsName = null)
        {
            Underwear newUnderwear = new Underwear(newUnderwearName, 1);

            Underwear oldUnderwear = new Underwear(body.underwear, 1);

            bool dirtyPants = body.HasWetOrMessyDebuff();

            if (newPantsName != null && dirtyPants)
            {
                Container oldPants = new Container(body, ContainerSubtype.Pants, body.pants.type);
                oldPants.ResetToDefault(body.pants);

                Container newPants = new Container(body, ContainerSubtype.Pants, newPantsName);

                body.ChangeUnderwearAndPants(newUnderwear, oldUnderwear, newPants, oldPants, npcName: npcName);
            }
            else
            {
                body.ChangeUnderwear(newUnderwear, oldUnderwear, npcName: npcName);
            }

            //body.ChangeUnderwear(newUnderwear, oldUnderwear, npc: npcName);
            //body.ResetPants();

            //If the underwear returned is not removable, destroy it
            bool returnDiaper = oldUnderwear.container.washable ? config.ReturnUsedCloth : config.ReturnUsedDisposable;
            if (!returnDiaper)
            {
                Dialoges.Warn(changeData.Getting_Changed_Destroyed, body, oldUnderwear.container);
            }
            //Otherwise put the old underwear into the inventory, but pull up the management window if it can't fit
            else if (!who.addItemToInventoryBool(oldUnderwear, false))
            {
                List<Item> objList = new List<Item>();
                objList.Add(oldUnderwear);
                Game1.activeClickableMenu = new ItemGrabMenu(objList);
            }
        }               

        private void ReceiveUpdateTick(object sender, OneSecondUpdateTickingEventArgs e)
        {

            //Ignore everything until we've started the day
            if (!started)
                return;


            //If time is moving, update our body state (Hunger, thirst, etc.)
            if (ShouldTimePass())
            {
                //body.HandleTime(timeInTick); to handle this cleaner, we move this to the time-changing tick

                // The following block makes npc (usually) not talk to you if you wear wet or messy pants... short of the special texts
                var hasMessyPants = body.pants.used;

                bool keyAskCheckNpcPressed = help.Input.GetState(config.KeyAskCheckNpc).IsDown();
                bool keyAskCheckPlayerPressed = help.Input.GetState(config.KeyAskCheckPlayer).IsDown();

                NpcBody npcBody = NpcHelper.GetNpcInFront();

                // check if actually a button for asking npc is pressed
                if (help.Input.GetState(SButton.LeftShift).IsDown() && (keyAskCheckNpcPressed || keyAskCheckPlayerPressed))
                {              
                    if (npcBody != null)
                    {
                        if((npcBody.Age == 2 && ChildrenAndDiapers) || npcBody.Age != 2)
                        {
                            int heartLevelForNpc = who.getFriendshipHeartLevelForNPC(npcBody.npc.getName());

                            string eventToken = "";
                            string responseKey = "";
                            string randQuestionNpcString = "";
                            string randNpcString = "";
                            bool hasOptionVeryNice = false;
                            bool hasOptionNice = false;
                            bool optionVeryNice = false;
                            bool optionNice = false;
                            bool hasOptionDirty = false;
                            bool dirtyChange = false;

                            if (keyAskCheckNpcPressed)
                            {
                                if(npcBody.npc.CurrentDialogue.Count > 0)
                                {
                                    npcBody.RemoveDialogue(checkNpcEventToken);
                                    npcBody.RemoveDialogue(checkPlayerEventToken);
                                }

                                eventToken = checkNpcEventToken;
                                responseKey = "check_npc";
                                hasOptionVeryNice = npcBody.ChangingOptions.hasOptionGetChangeVeryNice;
                                hasOptionNice = npcBody.ChangingOptions.hasOptionGetChangeNice;

                                optionVeryNice = hasOptionVeryNice ? npcBody.canGetChangedByPlayerVeryNice : false;
                                optionNice = hasOptionNice ? npcBody.canGetChangedByPlayerNice : false;
                            }
                            else if (keyAskCheckPlayerPressed)
                            {
                                if (npcBody.npc.CurrentDialogue.Count > 0)
                                {
                                    npcBody.RemoveDialogue(checkNpcEventToken);
                                    npcBody.RemoveDialogue(checkPlayerEventToken);
                                }

                                eventToken = checkPlayerEventToken;
                                responseKey = "check_player";
                                string responseKeyQuestion = "general_question_change";
                                randQuestionNpcString = Strings.RandString(npcBody.GetVillagerReactions(responseKeyQuestion).ToArray());
                                randQuestionNpcString = randQuestionNpcString + "#$b#";

                                hasOptionVeryNice = npcBody.ChangingOptions.hasOptionGiveChangeVeryNice;
                                hasOptionNice = npcBody.ChangingOptions.hasOptionGiveChangeNice;

                                optionVeryNice = hasOptionVeryNice ? npcBody.canGivePlayerChangeVeryNice : false;
                                optionNice = hasOptionNice ? npcBody.canGivePlayerChangeNice : false;

                                if (hasMessyPants)
                                {
                                    hasOptionDirty = npcBody.ChangingOptions.hasOptionGiveDirtyChange;
                                    dirtyChange = hasOptionDirty ? npcBody.canGivePlayerChangeDirty : false;
                                }
                            }

                            if(responseKey != "" && eventToken != "")
                            {
                                // get dialoge based on change options and friendship
                                // check if we have no messy pants or we have messy pants but no option for change dirty were found
                                if (hasMessyPants && !hasOptionDirty || !hasMessyPants)
                                {

                                    // get modificator for dialoge 
                                    string mod = "_mean";

                                    if ((hasOptionVeryNice && optionVeryNice) ||
                                        (!hasOptionVeryNice && heartLevelForNpc >= 8) ||
                                        config.FriendshipDebugVeryNice)
                                    {
                                        mod = "_verynice";
                                    }
                                    else if (!hasOptionVeryNice && heartLevelForNpc >= 6 &&
                                        !config.FriendshipDebugNice && !config.FriendshipDebugVeryNice)
                                    {
                                        var niceRand = rnd.NextDouble(); //allows a small chance for the very_nice line to be chosen instead for variety.
                                        if (niceRand <= 0.1f)
                                            mod = "_verynice";
                                        else
                                            mod = "_nice";
                                    }
                                    else if ((hasOptionNice && optionNice) ||
                                        (!hasOptionNice && heartLevelForNpc >= 4) ||
                                        (config.FriendshipDebugNice && !config.FriendshipDebugVeryNice))
                                    {
                                        mod = "_nice";
                                    }

                                    responseKey = responseKey + mod;
                                    randNpcString = Strings.RandString(npcBody.GetVillagerReactions(responseKey).ToArray());

                                    if (randNpcString != "")
                                    {
                                        randNpcString = randQuestionNpcString + randNpcString;
                                    }
                                }
                                else
                                {
                                    responseKey = "check_player_dirty";

                                    if (dirtyChange)
                                    {
                                        randNpcString = Strings.RandString(npcBody.GetVillagerReactions(responseKey).ToArray());
                                    }
                                }

                                if(randNpcString != "")
                                {
                                    string npcStatement = Strings.ReplaceAndOr(randNpcString, body.underwear.wetness > 0, body.underwear.messiness > 0);
                                    npcStatement = Strings.InsertVariables(npcStatement, body, (Container)null);

                                    // checking for query token in the dialoge and adding # for the answer options
                                    // this is nessesary because the game will not show the dialoge correctly if the query is used in the dialoge
                                    string token = "$query";

                                    if (npcStatement.Contains(token))
                                    {
                                        int i = npcStatement.IndexOf(token);
                                        string[] arr = npcStatement.Split(token)[1]?.Split('|');

                                        // get index of '|' Separator to insert '#'
                                        i = i + arr[0].Length + token.Length + 1;

                                        int amount = npcStatement.Split("#$b#").Length - 1;

                                        npcStatement = npcStatement.Insert(i, new string('#', amount * 2));
                                    }

                                    npcBody.npc.setNewDialogue(new Dialogue(npcBody.npc, eventToken, npcStatement), true, true);
                                }
                            }
                        }
                    }
                }

                // check if no npc is in front 
                if (npcBody == null)
                {
                    foreach (var npc in NpcHelper.GetNpcsByRange(10))
                    {
                        if (npc.CurrentDialogue.Count > 0)
                        {
                            npc.RemoveDialogue(checkNpcEventToken);
                            npc.RemoveDialogue(checkPlayerEventToken);
                        }
                    }
                }          
            }
        }

        //Determine if we need to handle time passing (not the same as Game time passing)
        private static bool ShouldTimePass()
        {
            return ((Game1.game1.IsActive || Game1.options.pauseWhenOutOfFocus == false) && (Game1.paused == false && Game1.dialogueUp == false) && (Game1.currentMinigame == null && Game1.eventUp == false && Game1.activeClickableMenu == null) && Game1.fadeToBlack == false);
        }

        //Interprete key-presses
        private void ReceiveKeyPress(object sender, ButtonPressedEventArgs e)
        {
            //If we haven't started the day, ignore the key presses
            if (!started) return;

            // We ignore keypresses in menues
            if (Game1.activeClickableMenu != null) return;

            bool altDown = e.IsDown(SButton.RightAlt);
            bool shiftDown = e.IsDown(SButton.LeftShift);

            //START Keybind-section
            if (!altDown)
            {
                bool triggered = true;

                if (e.Button == config.KeyGoInPants && (!shiftDown || config.KeyGoInPants != config.KeyGoInToilet))
                    body.WetAndMess(true, true);
                else if (e.Button == config.KeyGoInToilet)
                    body.WetAndMess(true, false);
                else if (e.Button == config.KeyPee && (!shiftDown || config.KeyPee != config.KeyPeeInToilet))
                    body.Wet(true, true);
                else if (e.Button == config.KeyPoop && (!shiftDown || config.KeyPoop != config.KeyPoopInToilet))
                    body.Mess(true, true);
                else if (e.Button == config.KeyPeeInToilet)
                    body.Wet(true, false);
                else if (e.Button == config.KeyPoopInToilet)
                    body.Mess(true, false);
                else triggered = false;

                if (triggered) return;
            }
            // END Keybind-section


            //Interpret buttons differently if holding Left Alt & Debug is enabled
            if (altDown && config.Debug)
            {
                if(e.Button == config.KeyDebugDecrease) body.DecreaseEverything();
                else if(e.Button == config.KeyDebugIncrease) body.IncreaseEverything();
                else if(e.Button == config.KeyDebugGiveUnderwear) DebugGiveUnderwear();
                else if(e.Button == config.KeyDebugFastForward) TimeMagic.doMagic();
                else if(e.Button == config.KeyDebugToggleWetting) config.Wetting = !config.Wetting;
                else if (e.Button == config.KeyDebugToggleMessing) config.Messing = !config.Messing;
                else if(e.Button == config.KeyDebugToggleEasymode)
                {
                    config.Easymode = !config.Easymode;
                    Dialoges.Write(config.Easymode ? generalData.EasyMode_On : generalData.EasyMode_Off, body);
                }
                else if(e.Button == config.KeyDebugIncreaseBladderContinence) body.ChangeContinence(IncidentType.PEE, -0.1f);
                else if (e.Button == config.KeyDebugDecreaseBladderContinence) body.ChangeContinence(IncidentType.PEE, 0.1f);
                else if (e.Button == config.KeyDebugIncreaseBowelContinence) body.ChangeContinence(IncidentType.POOP, -0.1f);
                else if (e.Button == config.KeyDebugDecreaseBowelContinence) body.ChangeContinence(IncidentType.POOP, 0.1f);
            }
            else
            {
                if(e.Button == config.KeyCheckUnderwear) Animations.CheckUnderwear(body);
                else if(e.Button == config.KeyCheckPants) Animations.CheckPants(body);
                else if (e.Button == config.KeyCheckPottyTraining) Animations.CheckContinence(body);
                else if (e.Button == config.KeyCheckPottyFeeling) Animations.CheckPottyFeeling(body);
                else if(e.Button == config.KeyToggleDebug)
                {
                    config.Debug = !config.Debug;
                    Regression.monitor.Log("Debug Mode Changed", LogLevel.Debug);
                }
            }
        }

        //A menu has been opened, figure out if we need to modify it
        private void ReceiveMenuChanged(object sender, MenuChangedEventArgs e)
        {
            //Don't do anything if our day hasn't started
            if (!started)
                return;

            DialogueBox attemptToSleepMenu;
            ShopMenu currentShopMenu;

            //If we try to sleep, check if the bed is done drying (only matters in Hard Mode)
            if (Game1.currentLocation is FarmHouse && (attemptToSleepMenu = e.NewMenu as DialogueBox) != null && Game1.currentLocation.lastQuestionKey == "Sleep" && !config.Easymode)
            {
                //If enough time has passed, the bed has dried
                if (body.bed.drying)
                {
                    Response[] sleepAttemptResponses = attemptToSleepMenu.responses;
                    if (sleepAttemptResponses.Length == 2)
                    {
                        Response response = sleepAttemptResponses[1];
                        Game1.currentLocation.answerDialogue(response);
                        Game1.currentLocation.lastQuestionKey = null;
                        attemptToSleepMenu.closeDialogue();
                        Animations.AnimateDryingBedding(body);
                    }
                }
            }
            //If we're in the mailbox, handle the initial letter from Jodi that contains protection
            else if (e.NewMenu is LetterViewerMenu && Game1.currentLocation is Farm)
            {
                LetterViewerMenu letterMenu = (LetterViewerMenu)e.NewMenu;
                Mail.ShowLetter(letterMenu);
            }
            //If we're trying to shop, handle the underwear inventory
            else if ((currentShopMenu = e.NewMenu as ShopMenu) != null)
            {
                ShopManager.AddItemsToShops(currentShopMenu);             
            }
        }

        //Check if we are at a natural water source
        private static bool AtWaterSource()
        {
            GameLocation currentLocation = Game1.currentLocation;
            Vector2 toolLocation = who.GetToolLocation(false);
            int x = (int)toolLocation.X;
            int y = (int)toolLocation.Y;
            return currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "Water", "Back") != null || currentLocation.doesTileHaveProperty(x / Game1.tileSize, y / Game1.tileSize, "WaterSource", "Back") != null;
        }

        //Check if we are at the Well (and its constructed)
        private static bool AtWell()
        {
            GameLocation currentLocation = Game1.currentLocation;
            Vector2 toolLocation = who.GetToolLocation(false);
            int x = (int)toolLocation.X;
            int y = (int)toolLocation.Y;
            Vector2 vector2 = new Vector2((float)(x / Game1.tileSize), y / Game1.tileSize);
            return currentLocation.IsBuildableLocation() && currentLocation.getBuildingAt(vector2) != null && (currentLocation.getBuildingAt(vector2).buildingType.Value.Equals("Well") && currentLocation.getBuildingAt(vector2).daysOfConstructionLeft.Value <= 0);
        
        }

        //Handle Mouse Clicks/Movement
        private void ReceiveMouseChanged(object sender, ButtonPressedEventArgs e)
        {
            //Ignore if we aren't started or otherwise paused
            if (!(Game1.game1.IsActive && !Game1.paused && started))
            {
                return;
            }

            
            if (e.Button == SButton.MouseLeft)
            {
                //If we try to take or put down pants, this should only work if you are allowed to change them.
                var men = Game1.activeClickableMenu as GameMenu;
                if (men != null)
                {
                    var inventory = men.pages[men.currentTab] as InventoryPage;
                    if (inventory != null)
                    {
                        var clothing = inventory.hoveredItem as Clothing;
                        if (clothing != null)
                        {
                            if (clothing.clothesType.Value == Clothing.ClothesType.PANTS)
                            {
                                if (body.HasWetOrMessyDebuff())
                                {
                                    Game1.activeClickableMenu = null;
                                    if (!config.PantsChangeRequiresHome || body.InPlaceWithPants())
                                    {
                                        body.ResetPants();
                                        Dialoges.Write(changeData.Change_At_Home, body);
                                    }
                                    else
                                    {
                                        Dialoges.Write(changeData.Change_Requires_Home, body);
                                    }

                                    return;

                                }
                            }
                        }
                    }
                }

                //If Left click is already being interpreted by another event (or we otherwise wouldn't process such an event. Ignore it.
                if ((Game1.dialogueUp || Game1.currentMinigame != null || 
                    (Game1.eventUp || Game1.activeClickableMenu != null) || 
                    Game1.fadeToBlack) || (who.isRidingHorse() || 
                    !who.canMove || (Game1.player.isEating || 
                    who.canOnlyWalk) || who.FarmerSprite.pauseForSingleAnimation))
                    return;

                // This block tries to figure out if the clicked point was in the toolbar. Because if it was, we assume the intent was not to use the selected item.
                var pos = e.Cursor.ScreenPixels;
                Toolbar toolbar = Game1.onScreenMenus.OfType<Toolbar>().FirstOrDefault();
                if (toolbar != null)
                {
                    foreach (var button in toolbar.buttons)
                    {
                        if (button.containsPoint((int)pos.X, (int)pos.Y))
                        {
                            // The click was on the toolbar
                            return;
                        }
                    }
                }

                ////If we're holding the watering can, attempt to drink from it.
                /////This is the highest priority (apparently?)
                if (who.CurrentTool != null && who.CurrentTool is WateringCan && e.IsDown(SButton.LeftShift))
                {
                    body.DrinkWateringCan();
                    return;
                }

                //Otherwise Check if we're holding underwear
                Underwear activeObject = who.ActiveObject as Underwear;
                if (activeObject != null)
                {
                    //If the Underwear we are holding isn't currently wet, messy, or drying; change into it.
                    if ((double)activeObject.container.wetness + (double)activeObject.container.messiness == 0.0 && !activeObject.container.drying)
                    {                      

                        if (Regression.config.PantsChangeRequiresHome && body.HasWetOrMessyDebuff() && !body.InPlaceWithPants())
                        {
                            Dialoges.Write(changeData.Change_Requires_Pants, body);
                            return;
                        }

                        Underwear OldUnderwear = new Underwear(body.underwear, 1);

                        if (Regression.config.UnderwearChangeCauseExposure)
                        {
                            Animations.HandleVillager(body, false, false, false, true);
                        }

                        if (!body.HasWetOrMessyDebuff())
                        {
                            body.ChangeUnderwear(activeObject, OldUnderwear);
                        }
                        else
                        {
                            Container oldPants = new Container(body,ContainerSubtype.Pants, body.pants.type);
                            oldPants.ResetToDefault(body.pants);

                            Container newPants = new Container(body,ContainerSubtype.Pants,oldPants.type);

                            if (!Regression.config.PantsChangeRequiresHome)
                            {
                                body.ChangeUnderwearAndPants(activeObject, OldUnderwear, newPants, oldPants);
                            }
                            else
                            {
                                if (body.InPlaceWithPants()) body.ChangeUnderwearAndPants(activeObject, OldUnderwear, newPants, oldPants);
                            }
                        }

                        //body.ChangeUnderwear(activeObject);
                        who.reduceActiveItemByOne();
                        //body.ResetPants();

                        //If the underwear returned is not removable, destroy it
                        bool returnDiaper = OldUnderwear.container.washable ? config.ReturnUsedCloth : config.ReturnUsedDisposable;
                        if (!returnDiaper)
                        {
                            Dialoges.Warn(changeData.Change_Destroyed, body, OldUnderwear.container);
                        }
                        //Otherwise put the old underwear into the inventory, but pull up the management window if it can't fit
                        else if (!who.addItemToInventoryBool(OldUnderwear, false))
                        {
                            List<Item> objList = new List<Item>();
                            objList.Add(OldUnderwear);
                            Game1.activeClickableMenu = new ItemGrabMenu(objList);
                        }
                    }
                    //If it is wet, messy or drying, check if we can wash it
                    else if (activeObject.container.washable)
                    {
                        //Are we at a water source? If so, wash the underwear.
                        if (AtWaterSource())
                        {
                            activeObject.container.Wash();
                            Animations.AnimateWashingUnderwear(activeObject.container);
                        }
                    }
                    return; //Done with underwear
                }


                //If we're at a water source, and not holding underwear, drink from it.
                if ((AtWaterSource() || AtWell()) && e.IsDown(SButton.LeftShift))
                    body.DrinkWaterSource();
            }

        }
        public static bool ChildrenAndDiapers
        {
            get
            {
                string yes = "child_diaper_yes";
                string no = "child_diaper_no";

                if (Game1.player.dialogueQuestionsAnswered.Contains(no)) return false;
                if (Game1.player.dialogueQuestionsAnswered.Contains(yes)) return true;
                return true;
            }
            set
            {
                string yes = "child_diaper_yes";
                string no = "child_diaper_no";

                if (value)
                {
                    if (Game1.player.dialogueQuestionsAnswered.Contains(no)) Game1.player.DialogueQuestionsAnswered.Remove(no);
                    if (!Game1.player.dialogueQuestionsAnswered.Contains(yes)) Game1.player.DialogueQuestionsAnswered.Add(yes);
                }
                else
                {
                    if (Game1.player.dialogueQuestionsAnswered.Contains(yes)) Game1.player.DialogueQuestionsAnswered.Remove(yes);
                    if (!Game1.player.dialogueQuestionsAnswered.Contains(no)) Game1.player.DialogueQuestionsAnswered.Add(no);
                }
            }
        }

        //If approppriate, draw bars for Hunger, thirst, bladder and bowels
        public void ReceivePreRenderHudEvent(object sender, RenderingHudEventArgs args)
        {
            if (!started || Game1.currentMinigame != null || Game1.eventUp || Game1.globalFade)
                return;
            StatusBars.DrawStatusBars();
        }
        private void OnGameLaunched(object sender, GameLaunchedEventArgs e)
        {
            ConfigMenu.GenerateMenu(this.Helper, this.ModManifest);

        }

        /// <param name="oldTime">The original time in HHMM format (e.g., 1550 for 15:50).</param>
        /// <param name="newTime">The new time in HHMM format (e.g., 1600 for 16:00).</param>
        /// <returns>The difference in minutes as an integer.</returns>
        /// <exception cref="ArgumentException">Thrown when input times are invalid.</exception>
        public int GetTimeDifference(int oldTime, int newTime)
        {
            // Validate and convert HHMM to total minutes since midnight
            int ConvertToMinutes(int time)
            {
                int hours = time / 100;
                int minutes = time % 100;
                if (hours < 0 || hours > 99 || minutes < 0 || minutes > 59)
                    throw new ArgumentException($"Invalid time format: {time}. Expected HHMM with HH=00-99 and MM=00-59.");
                return hours * 60 + minutes;
            }

            int oldMinutes = ConvertToMinutes(oldTime);
            int newMinutes = ConvertToMinutes(newTime);

            // Calculate the difference, accounting for midnight wrap-around
            return newMinutes >= oldMinutes
                ? newMinutes - oldMinutes
                : (1440 - oldMinutes) + newMinutes;
        }
        private void ReceiveTimeOfDayChanged(object sender, TimeChangedEventArgs e)
        {

            //lastTimeOfDay = Game1.timeOfDay;

            int newTime = e.NewTime;
            int oldTime = e.OldTime;

            // Calculate the difference
            int difference = GetTimeDifference(oldTime, newTime);

            //Monitor.Log($"Time changed from {oldTime} to {newTime}. Difference: {difference} minutes.", LogLevel.Debug);
            if (difference < 120) // we make sure that this doesn't get out of hand. Daychange is handled seperatly.
            {
                // If all of the 24 hours would be done as a 1 minute tick, there would be 1440 ticks in theory 
                /*float tickLen = 1440f;
                float lengthTotal = (float) difference / (float)tickLen;*/
                // But we handle this as fraction of an hour, so its 1/60 * minutes
                float fractionOfAnHour = (float)difference * (1f / 60f);
                body.HandleTime((float)difference * (1f / 60f));

                // NPC actions are based on chance. Chances that would be vastly different if every client would run them seperatly
                if (!Game1.IsMultiplayer || Game1.IsServer)
                {

                    foreach (NPC npc in Utility.getAllCharacters())
                    {
                        new NpcBody(npc).RandomAction(fractionOfAnHour);
                    }

                }
            }


            // Update lastTimeOfDay
            lastTimeOfDay = newTime;

            if (newTime < 630)
                return;

            //If its earlier than 6:30, we aren't wet/messy don't notice that we're still soiled (or don't notice with ~5% chance even if soiled)
            if (rnd.NextDouble() < 0.0555555559694767 && body.underwear.wetness + (double)body.underwear.messiness > 0.0)
                Animations.AnimateStillSoiled(body);

            if (rnd.NextDouble() < 0.0555555559694767 && (body.NeedsChangies(IncidentType.PEE) || body.NeedsChangies(IncidentType.POOP)))
                Animations.AnimateShouldChange(body);

            // Reset all dialogue answers for changing
            ResetAllNpcChangeAnswers();
        }

        public void ResetAllNpcChangeAnswers()
        {
            foreach (NPC npc in Utility.getAllCharacters())
            {
                string yes = "dirty_change_yes_" + npc.Name.ToLower();
                string no = "dirty_change_no_" + npc.Name.ToLower();

                if (Game1.player.dialogueQuestionsAnswered.Contains(yes)) Game1.player.dialogueQuestionsAnswered.Remove(yes);
                if (Game1.player.dialogueQuestionsAnswered.Contains(no)) Game1.player.dialogueQuestionsAnswered.Remove(no);
            }
        }        

        private static Queue<(Action action, int delay)> actionQueue = new Queue<(Action, int)>();
        private static bool isExecutingAction = false;

        public static void QueueAction(Action action, int delay = 1000)
        {
            actionQueue.Enqueue((action, delay));
        }

        private static void ExecuteNextAction()
        {
            if (actionQueue.Count == 0) return;

            isExecutingAction = true;
            (Action action, int delay) nextAction = actionQueue.Dequeue();

            // Execute the custom action
            nextAction.action();

            // Wait before allowing the next action to execute
            DelayedAction.functionAfterDelay(() =>
            {
                isExecutingAction = false;
            }, nextAction.delay);
        }

        private void OnUpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return; // Ensure the game world is loaded

            // If the queue has actions and no menu/dialogue is active, execute the next action
            if (actionQueue.Count > 0 && Game1.activeClickableMenu == null && !isExecutingAction)
            {
                ExecuteNextAction();
            }
        }

        public void CheckForDialogueCommands(DialogueBox dialogueBox)
        {
            string currentDialogue = ReturnCurrentDialogue();

            if (currentDialogue == null) return;

            NPC npc = Game1.currentSpeaker;
            Dialogue dialogue;

            // Dialoge command $CHANGED_BY_NPC$
            if (currentDialogue.Contains("$CHANGED_BY_NPC$"))
            {
                string newDialogue = Strings.ReplaceChangedByNpc(currentDialogue, body.underwear);

                dialogue = new Dialogue(npc, null, newDialogue);

                npc.CurrentDialogue.Clear();
                npc.CurrentDialogue.Push(dialogue);
                Game1.drawDialogue(npc);
            }
        }

        public string ReturnCurrentDialogue()
        {
            if (Game1.dialogueUp && Game1.currentSpeaker != null)
            {
                NPC npc = Game1.currentSpeaker;
                Dialogue dialogue;
                npc.CurrentDialogue.TryPeek(out dialogue);

                if (dialogue.currentDialogueIndex > dialogue.dialogues.Count - 1) return null;


                if (dialogue.dialogues[dialogue.currentDialogueIndex].Text != null)
                {
                    string line = dialogue.dialogues[dialogue.currentDialogueIndex].Text;
                    return line;
                }
            }

            return null;
        }


        public Regression()
        {
            //base.Actor();
        }
        private T LoadData<T>(string path) where T : class
        {
            T data = Helper.Data.ReadJsonFile<T>(path);
            if (data == null)
            {
                Monitor.Log($"Failed to load {path}. This will cause issues with mod functionality.", LogLevel.Error);
            }
            return data;
        }
    }
}
