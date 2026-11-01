using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Extensions;
using StardewValley.Menus;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RegressionMod
{
    //Added enum to indicate type instead of strings.
    public enum FullnessType
    {
        None,
        Clear,
        Wet,
        Messy,
        WetMessy,
        Drying
    }

    internal static class Animations
    {

        enum FaceDirection : int
        {
            Down = 2,
            Left = 1,
            Right = 3,
            Up = 0
        };
                
        private static ChangeData changeData = Regression.changeData;
        private static ConsumablesData consumablesData = Regression.consumablesData;
        private static GeneralData generalData = Regression.generalData;
        private static PeePoopData peePoopData = Regression.peePoopData;
        private static StatesContinenceData statesContinenceData = Regression.statesContinenceData;
        private static VillagerData villagerData = Regression.villagerData;
        private static VilagerDialogData villagerDialogData = Regression.villagerDialogData;
        public static Farmer player = Game1.player;

        public static float ZoomScale()
        {
            return Game1.options.zoomLevel / Game1.options.uiScale;
        }

        public static void AnimateDrinking(bool waterSource = false)
        {
            //If we aren't facing downward, turn
            if (player.getFacingDirection() != (int)FaceDirection.Down)
                player.faceDirection((int)FaceDirection.Down);


            //Stop doing anything that would prevent us from moving
            //Essentially take control of the variable
            player.forceCanMove();


            //Stop any form of animation
            player.completelyStopAnimatingOrDoingAction();


            // ISSUE: method pointer
            //Start Drinking animation. While drinking pause time and don't allow movement.
            player.FarmerSprite.animateOnce(FarmerSprite.drink, AnimationConstants.DrinkAnimationInterval, AnimationConstants.DrinkAnimationFrames, new AnimatedSprite.endOfAnimationBehavior(EndDrinking));
            player.freezePause = AnimationConstants.PauseTime;
            player.canMove = false;

            //If we drink from the watering can, don't say anything
            if (!waterSource)
                return;

            //Otherwise say something about it
            Dialoges.Say(consumablesData.Drink_Water_Source, null);
        }

        //Not really an animation. Just say the bedding's current state.
        public static void AnimateDryingBedding(Body b)
        {
            Dialoges.Write(generalData.Bedding_Still_Wet, b);
        }


        public static void AnimateMessingStart(Body b, bool voluntary, bool inUnderwear)
        {
            if (b.underwear.removable || inUnderwear)
                Game1.playSound("slosh");

            if (b.isSleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bowelContinence + 0.449999988079071 <= Regression.rnd.NextDouble())
                return;

            if (!(b.underwear.removable || inUnderwear))
            {
                Dialoges.Say(Regression.changeData.Cant_Remove, b);
                return;
            }

            if (!inUnderwear)
            {
                if (b.InToilet(inUnderwear))
                    Dialoges.Say(peePoopData.Poop_Toilet, b);
                else
                    Dialoges.Say(peePoopData.Poop_Voluntary, b);
            }
            else if (voluntary)
                Dialoges.Say(peePoopData.Mess_Voluntary, b);
            else
                Dialoges.Say(peePoopData.Mess_Accident, b);


            player.doEmote(12, false);
            if (Body.IsFishing() || !player.canMove) return; // We skip the actual animations if nessesary

            player.jitterStrength = 1.0f;
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, player.position.Value - new Vector2(((Character)player).facingDirection.Value == 1 ? 0.0f : (float)-Game1.tileSize, (float)(Game1.tileSize * 2)), false, ((Character)player).facingDirection.Value == 1, (float)((Character)player).StandingPixel.Y / 10000f, 0.01f, Color.White, 1f, 0.0f, 0.0f, 0.0f, false));

            player.freezePause = AnimationConstants.poopAnimationTime;
            player.canMove = false;
        }
        public static void AnimateMessingEnd(Character target)
        {

            if (Body.IsFishing()) return;
            Game1.playSound("coin");
            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite("TileSheets\\animations", new Rectangle(192, 1152, Game1.tileSize, Game1.tileSize), 50f, 4, 0, target.position.Value - new Vector2(target.facingDirection.Value == 1 ? 0.0f : -Game1.tileSize, Game1.tileSize * 2), false, target.facingDirection.Value == 1, target.StandingPixel.Y / 10000f, 0.01f, Color.White, 1f, 0.0f, 0.0f, 0.0f, false));
        }
        public static void AnimateMessingMinor(Body b)
        {
            Dialoges.Say(statesContinenceData.Bowels_Leak, b);
            ((Character)player).doEmote(12, false);
        }

        public static void AnimateWettingStart(Body b, bool voluntary, bool inUnderwear)
        {
            if (b.underwear.removable || inUnderwear)
                Game1.playSound("wateringCan");

            // check if we notice the accident.
            if (b.isSleeping || !voluntary && !Regression.config.AlwaysNoticeAccidents && (double)b.bladderContinence + 0.200000002980232 <= Regression.rnd.NextDouble())
                return;

            if (!(b.underwear.removable || inUnderwear))
            {
                Dialoges.Say(changeData.Cant_Remove, b);
                return;
            }

            if (!inUnderwear)
            {
                if (b.InToilet(inUnderwear))
                    Dialoges.Say(peePoopData.Pee_Toilet, b);
                else
                    Dialoges.Say(peePoopData.Pee_Voluntary, b);

                ((GameLocation)player.currentLocation).temporarySprites.Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position.Value, Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (player.currentLocation.terrainFeatures.ContainsKey(((Character)player).Tile) && (terrainFeature = player.currentLocation.terrainFeatures[((Character)player).Tile] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
            else if (voluntary)
                Dialoges.Say(peePoopData.Wet_Voluntary, b);
            else
                Dialoges.Say(peePoopData.Wet_Accident, b);

            // We skip the actual animations if nessesary
            if (Body.IsFishing() || !player.canMove) return;

            player.jitterStrength = 0.5f;
            player.freezePause = AnimationConstants.peeAnimationTime; //milliseconds
            player.canMove = false;
            ((Character)player).doEmote(28, false);
        }
        public static void AnimateWettingMinor(Body b)
        {
            Dialoges.Say(statesContinenceData.Bladder_Leak, b);
            ((Character)player).doEmote(28, false);
        }


        public static void AnimateWettingEnd(Body b)
        {
            if (Body.IsFishing()) return;
            if ((double)b.pants.wetness > (double)b.pants.absorbency)
            {
                ((GameLocation)player.currentLocation).temporarySprites.Add(new TemporaryAnimatedSprite(13, (Vector2)((Character)Game1.player).position.Value, Color.White, 10, ((Random)Game1.random).NextDouble() < 0.5, 70f, 0, (int)Game1.tileSize, 0.05f, -1, 0));
                HoeDirt terrainFeature;
                if (player.currentLocation.terrainFeatures.ContainsKey(((Character)player).Tile) && (terrainFeature = player.currentLocation.terrainFeatures[((Character)player).Tile] as HoeDirt) != null)
                    terrainFeature.state.Value = 1;
            }
        }

        public static void AnimateMorning(Body b)
        {
            bool flag = (double)b.bed.wetness > 0.0;
            bool second = (double)b.bed.messiness > 0.0;
            string msg = "" + Strings.RandString(generalData.Wake_Up_Underwear_State);
            if (second)
            {
                msg = msg + " " + Strings.ReplaceOptional(Strings.RandString(generalData.Messed_Bed), flag);
                if (!Regression.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(generalData.Washing_Bedding), flag, second, "&");
            }
            else if (flag)
            {
                msg = msg + " " + Strings.RandString(generalData.Wet_Bed);
                if (!Regression.config.Easymode)
                    msg = msg + " " + Strings.ReplaceAndOr(Strings.RandString(generalData.Spot_Washing_Bedding), flag, second, "&");
            }
            Dialoges.Write(msg, b);
        }

        // Function to make the first character of the input string upper case
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1))
            };
        public static void AnimateNight(Body b)
        {
            bool first = b.numPottyPeeAtNight > 0;
            bool second = b.numPottyPooAtNight > 0;
            if (!b.Get(IncidentType.PEE).IsEnabled && !b.Get(IncidentType.POOP).IsEnabled)
                return;

            var list = generalData.Night;
            // assumtion: if we wake up to pee/poop, we do that togehter if possible. So if we wake up 2 times for pee and 1 time for poop, it is likely we only got up twice, not 3 times
            int gotUpAtNight = Math.Max(b.numPottyPeeAtNight, b.numPottyPooAtNight);
            string toiletMsg = "";

            if (gotUpAtNight > 0)
            {
                toiletMsg = Strings.ReplaceAndOr(Strings.RandString(list["Toilet_Night"]), first, second, "&");
            }
            else
            {
                toiletMsg = Strings.RandString(list["Sleep_All_Night"]);
            }
            toiletMsg = FirstCharToUpper(toiletMsg);

            if (b.numAccidentPooAtNight == 0 && b.numAccidentPeeAtNight == 0)
                toiletMsg += ".";
            else
            {
                var butPlaceholder = false;
                if (gotUpAtNight > 0)
                {
                    butPlaceholder = true;
                    if (!b.underwear.removable)
                    {
                        toiletMsg += " " + Strings.RandString(list["Underwear_Stuck"]);
                        butPlaceholder = false;
                    }
                }
                toiletMsg += ".";

                var accidentReport = Strings.ReplaceOptional(Strings.ReplaceAndOr(Strings.RandString(list["Accident_At_Night"]), b.numAccidentPeeAtNight > 0, b.numAccidentPooAtNight > 0), butPlaceholder);
                accidentReport = FirstCharToUpper(accidentReport);

                toiletMsg += " " + accidentReport + ".";
            }

            toiletMsg = Strings.ReplaceNightTokens(toiletMsg, gotUpAtNight, b);
            Dialoges.Write(toiletMsg, b);
        }

        public static void AnimatePeeAttempt(Body b, bool inUnderwear)
        {

            if (Body.IsFishing()) return;
            if (inUnderwear)
                Dialoges.Say(peePoopData.Wet_Attempt, b);
            else if (b.InToilet(inUnderwear))
                Dialoges.Say(peePoopData.Pee_Toilet_Attempt, b);
            else
                Dialoges.Say(peePoopData.Pee_Attempt, b);
        }

        public static void AnimatePoopAttempt(Body b, bool inUnderwear)
        {

            if (Body.IsFishing()) return;
            if (inUnderwear)
                Dialoges.Say(peePoopData.Mess_Attempt, b);
            else if (b.InToilet(inUnderwear))
                Dialoges.Say(peePoopData.Poop_Toilet_Attempt, b);
            else
                Dialoges.Say(peePoopData.Poop_Attempt, b);
        }

        public static void AnimateStillSoiled(Body b)
        {
            string baseString = Strings.RandString(changeData.Still_Soiled);
            baseString = Strings.ReplaceInspectUnderwearToken(baseString,b.underwear);
            Dialoges.Say(baseString, b);
        }
        public static void AnimateShouldChange(Body b)
        {
            string baseString = Strings.RandString(changeData.Should_Change);
            baseString = Strings.ReplaceInspectUnderwearToken(baseString, b.underwear);
            Dialoges.Say(baseString, b);
        }
        public static void AnimateWashingUnderwear(Container c)
        {
            if (c.MarkedForDestroy())
            {
                Dialoges.Write(Strings.InsertVariables(Strings.RandString(generalData.Overwashed_Underwear), (Body)null, c), (Body)null);
                Game1.player.reduceActiveItemByOne();
            }
            else
            {
                Dialoges.Write(Strings.InsertVariables(Strings.RandString(generalData.Washing_Underwear), (Body)null, c), (Body)null);
            }
        }

        public static void CheckPants(Body b)
        {
            Dialoges.Say(Strings.tryGetI18nText(changeData.LookPants) + " " + Strings.DescribeUnderwear(b.pants) + ".", b);
        }
        public static bool WarnCurrentThreshold(float[] thresholds, string[][] messages, float currentValue, bool greaterAs = false)
        {
            string[] curr = GetCurrentThreshold(thresholds, messages, currentValue, greaterAs);

            if (curr != null)
            {
                Dialoges.Warn(curr);
                return true;
            }
            return false;
        }
        public static string[] GetCurrentThreshold(float[] thresholds, string[][] messages, float currentValue, bool greaterAs = false)
        {
            int index = GetCurrentTresholdIndex(thresholds, messages, currentValue, greaterAs);
            if (index >= 0) return messages[index];
            return null;
        }
        public static int GetCurrentTresholdIndex(float[] thresholds, string[][] messages, float currentValue, bool greaterAs = false)
        {
            for (int index = thresholds.Length - 1; index >= 0; index--)
            {
                if (thresholds[index] > currentValue && !greaterAs) continue;
                if (thresholds[index] <= currentValue && greaterAs) continue;

                return index;
            }
            return -1;
        }

        public static void CheckContinence(Body b)
        {
            if (b.Get(IncidentType.PEE).IsEnabled)
            {
                WarnCurrentThreshold(Body.BLADDER_CONTINENCE_THRESHOLDS, Body.BLADDER_CONTINENCE_MESSAGES, b.bladderContinence);
            }

            if (b.Get(IncidentType.POOP).IsEnabled)
            {
                WarnCurrentThreshold(Body.BOWEL_CONTINENCE_THRESHOLDS, Body.BOWEL_CONTINENCE_MESSAGES, b.bowelContinence);
            }
        }
        public static void CheckPottyFeeling(Body b)
        {
            Dialoges.Warn((b.Get(IncidentType.PEE).IsEnabled ? Strings.RandString(GetPottyFeeling(b, IncidentType.PEE)) + " " : "") + (b.Get(IncidentType.POOP).IsEnabled ? Strings.RandString(GetPottyFeeling(b, IncidentType.POOP)) : ""));
        }

        public static string[] GetPottyFeeling(Body b, IncidentType type)
        {
            float newFullness = b.Get(type).Fullness / b.Get(type).Capacity;
            string[] pottyMessages = null;
            if (newFullness > (1 - b.Get(type).Continence))
            {
                pottyMessages = GetCurrentThreshold(type == IncidentType.PEE ? Body.WETTING_THRESHOLDS : Body.MESSING_THRESHOLDS, type == IncidentType.PEE ? Body.WETTING_MESSAGES : Body.MESSING_MESSAGES, newFullness, false);
            }
            if (pottyMessages != null) return pottyMessages;
            return type == IncidentType.PEE ? Body.WETTING_MESSAGE_GREEN : Body.MESSING_MESSAGE_GREEN;
        }
        public static void CheckUnderwear(Body b)
        {
            string waistband = Strings.tryGetI18nText(changeData.PeekWaistband);

            Dialoges.Say(waistband + " " + Strings.DescribeUnderwear(b.underwear) + ".", b);
        }        

        private static void EndDrinking(Farmer who)
        {
            player.completelyStopAnimatingOrDoingAction();
            player.forceCanMove();
        }

        public static void HandlePasserby()
        {
            if (Utility.isThereAFarmerOrCharacterWithinDistance(player.Tile, 3, (GameLocation)Game1.currentLocation) is not NPC npc)
                return;

            string msg = Strings.RandString(generalData.Passby);

            npc.CurrentDialogue.Push(new Dialogue(npc, null, msg));
        }
        

        public static Dictionary<string, int> FriendshipLossOnAccident(Body b, bool mess, bool inUnderwear, bool overflow, bool attempt = false)
        {
            var list = new Dictionary<string, int>();

            //Get NPC in radius
            var nearby = NpcHelper.NearbyVillager(b, mess, inUnderwear, overflow, attempt);

            foreach (var npc in nearby)
            {
                int finalLossValue = FriendshipLossOnAccident(npc, mess, inUnderwear, overflow, attempt);
                list[npc.getName().ToLower()] = finalLossValue;
            }
            return list;
        }
        public static int FriendshipLossOnAccident(NPC npc, bool mess, bool inUnderwear, bool overflow, bool attempt = false)
        {
            int heartLevelForNpc = player.getFriendshipHeartLevelForNPC(npc.getName().ToLower());
            int baseFriendshipLoss = (mess ? Regression.config.FriendshipPenaltyBowelMultiplier : Regression.config.FriendshipPenaltyBladderMultiplier);

            //Does this leave the possibility of friendship gain if we have enough hearts already? Maybe because they find the vulnerability endearing?
            float friendshipLoss = -1 + (heartLevelForNpc - 2) / 2;

            var modifiers = villagerData.Villager_Friendship_Modifier;
            var modifierKey = modifierForIncident(npc, mess, inUnderwear, overflow, attempt);

            float finalModifier = 1.0f;
            foreach (string key2 in NpcHelper.npcTypeList(npc))
            {
                float floatOut;
                Dictionary<string, float> dictionary;
                if (modifiers.TryGetValue(key2, out dictionary) && dictionary.TryGetValue(modifierKey, out floatOut))
                {
                    finalModifier = floatOut;
                }
            }

            return (int)Math.Floor((baseFriendshipLoss / 100 / 5) * friendshipLoss * finalModifier);
        }
        

        public static string modifierForIncident(NPC npc, bool mess, bool inUnderwear, bool overflow, bool attempt = false)
        {
            if (!inUnderwear)
            {
                //If we weren't wearing underwear, we tried on the ground.
                //But did we succeed or just try to go?
                return attempt ? "ground_attempt" : "ground";
            }
            else
            {
                //Otherwise, we are soiling ourselves
                return "soiled";
            }
        }
        public static string responseKeyAdditionForState(NPC npc)//, bool isDirty = false)
        {
            var body = new NpcBody(npc);

            // randoise if we check npc or npc check player
            if (Regression.rnd.NextBool())
            {
                return "check_npc";
            }
            else
            {
                return "check_player";
            }

        }
        public static string responseKeyAdditionForIncident(NPC npc, bool inUnderwear, int friendshipLoss)
        {
            // we only have special responses for soiling our underwear
            if (inUnderwear)
            {
                //Animals only have a "nice" response
                if (NpcHelper.npcTypeList(npc).Contains("animal"))
                {
                    return "_nice";
                }
                else
                {
                    int heartLevelForNpc = player.getFriendshipHeartLevelForNPC(npc.getName());
                    //If we have a really high relationship with the NPC, they're very nice about our accident
                    if (heartLevelForNpc >= 8 || Regression.config.FriendshipDebugVeryNice)
                    {
                        return "_verynice";
                    }
                    //Otherwise they'll be mean or nice depending on how much friendship we're losing
                    if (friendshipLoss < 0 || (Regression.config.FriendshipDebugNice && !Regression.config.FriendshipDebugVeryNice)) return "_nice";
                    return "_mean";
                }
            }
            return "";
        }

        public static bool HandleVillager(Body b, bool mess, bool inUnderwear, bool overflow, bool attempt = false)
        {
            bool someoneNoticed = false;

            //Get NPC in radius
            var nearby = NpcHelper.NearbyVillager(b, mess, inUnderwear, overflow, attempt);
            foreach (var npc in nearby)
            {
                if (!npc.IsVillager && !(npc is Horse) && !(npc is Pet)) continue;
                if (npc.Age == 2 && !Regression.ChildrenAndDiapers) continue;
                someoneNoticed = true;

                //Make a list based on who saw us.
                var npcType = NpcHelper.npcTypeList(npc);

                string npcName = "";
                if (npc is Horse || npc is Pet)
                {
                    npcName += string.Format("{0}: ", npc.Name);
                }

                //What did we do? Use to figure out the response.
                string modifierKey = modifierForIncident(npc, mess, inUnderwear, overflow, attempt);
                int finalLossValue = FriendshipLossOnAccident(npc, mess, inUnderwear, overflow, attempt);

                //If we're in debug mode, notify how the relationship was effected
                if (Regression.config.Debug && finalLossValue < 0)
                    Dialoges.Say(string.Format("{0} ({1}) changed friendship from {2} by {3}.", npc.Name, npc.Age, player.getFriendshipHeartLevelForNPC(npc.getName()), finalLossValue), (Body)null);


                //If we didn't lose any friendship, or we disabled friendship penalties (by adjusting it to 0), then don't adjust the value
                if (finalLossValue < 0)
                    player.changeFriendship(finalLossValue, npc);

                string responseKey = modifierKey + responseKeyAdditionForIncident(npc, inUnderwear, finalLossValue);
                List<string> stringList3 = new List<string>();
                foreach (string key2 in npcType)
                {
                    Dictionary<string, string[]> dictionary;
                    string[] strArray;
                    if (villagerDialogData.Villager_Reactions.TryGetValue(key2, out dictionary) && dictionary.TryGetValue(responseKey, out strArray))
                    {
                        stringList3 = new List<string>(); // We could remove this line again, but the general texts are more meant as fallback, they often don't fit well if custom texts are defined
                        stringList3.AddRange((IEnumerable<string>)strArray);
                    }

                }

                if (stringList3.Count <= 0) continue;

                //Construct and say Statement
                string npcStatement = Strings.InsertVariables(Strings.ReplaceAndOr(Strings.RandString(stringList3.ToArray()), !mess, mess, "&"), b, (Container)null);
                if (npc is Horse || npc is Pet)
                {
                    npcStatement = npcName + " " + npcStatement;
                }
                npcStatement = Strings.ReplaceNpcName(npcStatement, npc.Name);

                Regression.QueueAction(() =>
                {
                    if (b.underwear.used || attempt || !inUnderwear)
                    {
                        npcStatement = Strings.InsertVariables(npcStatement, npc);
                        npc.setNewDialogue(new Dialogue(npc, null, npcStatement), true, true);
                        Game1.drawDialogue(npc);
                    }
                });
            }

            return someoneNoticed;
        }

        public enum FriendshipLevel
        {
            Mean,
            Nice,
            Verynice
        }
    }
}
