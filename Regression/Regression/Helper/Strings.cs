using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley;
using StardewValley.GameData.Characters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace RegressionMod
{
    //Lots of Regex functions to handle variability in our strings.
    public static class Strings
    {
        private static ChangeData changeData = Regression.changeData;
        private static VillagerData villagerData = Regression.villagerData;
        private static VilagerDialogData villagerDialogData = Regression.villagerDialogData;
        private static Farmer who = Game1.player;

        public static string DescribeUnderwear(Container u, string baseDescription = null, bool noPrefix = false)
        {
            string newValue = baseDescription != null ? baseDescription : u.description;
            float peeFill = u.wetness / u.absorbency;
            float poopFill = u.messiness / u.containment;
            if ((double)peeFill == 0.0 && (double)poopFill == 0.0)
            {
                newValue = !u.drying ? tryGetI18nText(changeData.Underwear_Clean).Replace(StringConstants.Token.UnderwearDesc, newValue) :
                                        tryGetI18nText(changeData.Underwear_Drying).Replace(StringConstants.Token.UnderwearDesc, newValue);
            }
            else
            {
                if ((double)poopFill > 0.0)
                {
                    for (int index = 0; index < changeData.Underwear_Messy.Length; ++index)
                    {
                        float num3 = (float)((index + 1.0) / (changeData.Underwear_Messy.Length - 1.0));
                        if (index == changeData.Underwear_Messy.Length - 1 || (double)poopFill <= (double)num3)
                        {
                            newValue = ReplaceOptional(tryGetI18nText(changeData.Underwear_Messy[index]).Replace(StringConstants.Token.UnderwearDesc, newValue), (double)peeFill > 0.0);
                            break;
                        }
                    }
                }
                if ((double)peeFill > 0.0)
                {
                    for (int index = 0; index < changeData.Underwear_Wet.Length; ++index)
                    {
                        float num3 = (float)((index + 1.0) / (changeData.Underwear_Wet.Length - 1.0));
                        if (index == changeData.Underwear_Wet.Length - 1 || (double)peeFill <= (double)num3)
                        {
                            string input = tryGetI18nText(changeData.Underwear_Wet[index]).Replace(StringConstants.Token.UnderwearDesc, newValue);
                            Regex regex = new Regex("<([^>]*)>");
                            newValue = (double)poopFill != 0.0 ? regex.Replace(input, "$1") : regex.Replace(input, "");
                            break;
                        }
                    }
                }
            }
            string newOrOld = "";
            if (u.washable && u.InnerContainer != null && u.durability < u.InnerContainer.durability)
            {
                newOrOld = "used ";
                if (u.durability < u.InnerContainer.durability / 2)
                {
                    newOrOld = "old ";
                }
            }
            if (noPrefix) return newValue;
            return getPrefix(u.plural) + " " + newOrOld + newValue;
        }
        public static string npcUnderwearOptions(NPC npc)
        {
            var modifiers = villagerData.Villager_Underwear_Options;
            Dictionary<string, Dictionary<string, string>> foundDict = null;

            foreach (string key2 in NpcHelper.npcTypeList(npc))
            {
                Dictionary<string, Dictionary<string, string>> dictionary;
                if (modifiers.TryGetValue(key2, out dictionary))
                {
                    foundDict = dictionary;
                }
            }

            if (foundDict == null) return "";
            var list = new List<string>();
            foreach (string key in foundDict.Keys)
            {
                if (!UnderwearHelper.HasUnderwear(key)) continue;
                var entry = foundDict[key];

                // check if npc has dialog key for underwear option
                if (!NpcHasDialogKey(npc, entry["dialog_key"]) || entry["dialog_key"] == null)
                {
                    Regression.monitor.Log($"NPC '{npc.Name}' has no dialog '{entry["dialog_key"]}', option '{key}' is skipped.", LogLevel.Warn);
                    continue;
                }

                // #$r change_other_yes 2 Change_Diaper_Accept#Yes
                list.Add($"#$r change_other_yes {entry["friendship"]} {entry["dialog_key"]} {(entry.TryGetValue("observerfriendship", out string val) ? val : "")}#{key.FirstCharToUpper()}");
            }

            // check if npc has dialog key for refuse option
            if (!NpcHasDialogKey(npc, "Change_Diaper_Refuse"))
            {
                Regression.monitor.Log($"NPC '{npc.Name}' has no dialog 'Change_Diaper_Refuse', changing request is canceled.", LogLevel.Warn);
                return "";
            }

            list.Add($"#$r change_other_no 0 Change_Diaper_Refuse#" + Regression.help.Translation.Get("Change_Other.Dialog.Answer_No"));
            return string.Join("#", list);
        }

        public static bool NpcHasDialogKey(NPC npc, string key)
        {
            try
            {
                return npc?.Dialogue != null && npc.Dialogue.ContainsKey(key);
            }
            catch
            {
                return false;
            }
        }

        public static string InsertVariables(string msg, Body b, Container c = null)
        {
            string str = msg;
            if (b != null && c == null)
                c = b.underwear;
            if (c != null)
            {
                str = ReplaceGetChangeDialog(str, c);       // replace the $GETTING_CHANGED_DIALOG$ token with the corresponding dialogue

                str = ReplaceConditionalOptional(str, "OnSlightlyWet", c.used && !c.used_bad);
                str = ReplaceConditionalOptional(str, "OnClean", !c.used);

                str = ReplaceChangedByNpc(str, c);
                str = ReplaceGetChangeDialogAccept(str, c);
                str = ReplaceGetChangeDialogRefuse(str, c);
                str = ReplaceUnderwearToken(str, c);
                str = ReplaceUnderwearToken(str, c, oldNew: OldNew.New);
                str = ReplaceInspectUnderwearToken(str, c);
                str = ReplaceInspectUnderwearToken(str, c, oldNew: OldNew.New);

                str = ReplaceOr(str,!c.plural,"#");
            }

            if (b != null)
            {
                str = ReplaceConditionalOptional(str, "OnDebuffed", b.HasWetOrMessyDebuff());

                str = ReplacePantsToken(str,b.pants, oldNew: OldNew.New);
                str = str.Replace("$BEDDING_DRYTIME$", Game1.getTimeOfDayString(b.bed.timeWhenDoneDrying.time));
            }

            str = ReplaceOr(str, who.IsMale, "/");
            str = ReplaceFarmername(str);

            return str;
        }
        
        public static string InsertVariables(string msg, NPC b, Container c = null)
        {
            string str = msg;
            if (b != null && c == null)
                c = new NpcBody(b).underwear;
            if (c != null)
            {
                var changeOtherDialog = RandString(changeData.Change_Other_Dialog);
                changeOtherDialog = ReplaceAndOr(changeOtherDialog, c.wetness > 0, c.messiness > 0);
                changeOtherDialog += npcUnderwearOptions(b);
                str = str.Replace(StringConstants.Token.ChangeOtherDialog, changeOtherDialog);

                str = ReplaceAndOr(str, c.wetness > 0, c.messiness > 0);

                str = ReplaceConditionalOptional(str, "NpcOnSlightlyWet", c.used && !c.used_bad);
                str = ReplaceConditionalOptional(str, "NpcOnUsedBad", c.used && c.used_bad);
                str = ReplaceConditionalOptional(str, "NpcOnClean", !c.used);

                str = ReplaceUnderwearToken(str, c, npc: true);
                str = ReplaceUnderwearToken(str, c, npc: true,oldNew: OldNew.New);
                str = ReplaceInspectUnderwearToken(str, c, npc: true);
                str = ReplaceInspectUnderwearToken(str, c, npc: true, oldNew: OldNew.New);

                str = ReplaceOr(str, !c.plural, "#");
            }

            if (b != null)
            {
                var pants = new NpcBody(b).pants;
                if (pants != null)
                {
                    str = ReplaceNpcName(str, b.Name.ToLower());
                    str = ReplacePantsToken(str, pants, true, oldNew: OldNew.New);
                    str = ReplaceGenderTextNpc(str, b.Gender);
                }
                else Regression.monitor.Log($"NPC Error: {b.Name}", LogLevel.Info);
            }


            return str;
        }

        public static string InsertVariable(string inputString, string variableName, string variableValue)
        {
            string outputString = inputString;
            outputString = outputString.Replace(variableName, variableValue);
            return outputString;
        }

        public static string RandString(string[] msgs = null)
        {
            if (msgs == null || msgs.Length == 0) return "";
            return tryGetI18nText(msgs[Regression.rnd.Next(msgs.Length)]);
        }

        public static string ReplaceAndOr(string str, bool first, bool second, string splitChar = "&")
        {
            Regex regex = new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>");
            if (first && !second)
                return regex.Replace(str, "$1");
            if (!first && second)
                return regex.Replace(str, "$2");
            if (first && second)
                return regex.Replace(str, "$1 and $2");
            return regex.Replace(str, "");
        }

        public static string ReplaceOptional(string str, bool keep)
        {
            return new Regex("<([^>]*)>").Replace(str, keep ? "$1" : "");
        }
        /// <summary>
        /// Replaces or removes [triggerText: ...] tokens in the dialogue string.
        /// </summary>
        /// <param name="str">The original dialogue string.</param>
        /// <param name="key">The key-triggerText that causes that token to be replaced or not.</param>
        /// <param name="keep">If true, replace with the inner text; if false, remove the token.</param>
        /// <returns>The processed dialogue string.</returns>
        public static string ReplaceConditionalOptional(string str, string key, bool keep)
        {
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(key))
                return str;

            // Escape the triggerKey to handle any special regex characters
            string escapedKey = Regex.Escape(key);

            string pattern = $@"\[{escapedKey}:\s*(.*?)\]";

            Regex triggerTextRegex = new Regex(pattern, RegexOptions.Compiled);
            return triggerTextRegex.Replace(str, keep ? " $1" : "");
        }
        public static string ReplaceOr(string str, bool first, string splitChar = "/")
        {
            return new Regex("<([^>" + splitChar + "]*)" + splitChar + "([^>]*)>").Replace(str, first ? "$1" : "$2");
        }               

        #region Replace Tokens
        public static string ReplaceFarmername(string str)
        {
            string token = StringConstants.Token.Farmername;
            return str.Replace(token, who.Name);
        }

        public static string ReplaceNpcName(string str, string npcName)
        {
            string token = StringConstants.Token.NpcName;
            return str.Replace(token, npcName.ToLower());
        }

        #region Replace Token Underwear
        public static string ReplaceUnderwearToken(string str, Container underwear, bool npc = false, OldNew oldNew = OldNew.None)
        {
            str = ReplaceUnderwearPrefix(str,underwear.plural, npc: npc, oldNew: oldNew);
            str = ReplaceUnderwearName(str, underwear.displayName, npc: npc, oldNew: oldNew);
            str = ReplaceUnderwearDesc(str, underwear.description, npc: npc, oldNew: oldNew);

            return str;
        }

        // possible tokens:
        //      $UNDERWEAR_PREFIX$              Players actual weared underwear prefix
        //      $NEW_UNDERWEAR_PREFIX$          Players new underwear prefix
        //      $OLD_UNDERWEAR_PREFIX$          Players old underwear prefix
        //      $NPC_UNDERWEAR_PREFIX$          NPCs actual weared underwear prefix
        //      $NPC_NEW_UNDERWEAR_PREFIX$      NPCs new underwear prefix
        //      $NPC_OLD_UNDERWEAR_PREFIX$      NPCs old underwear prefix
        private static string ReplaceUnderwearPrefix(string str, bool plural = false, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "UNDERWEAR_PREFIX";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, getPrefix(plural));
        }

        // possible tokens:
        //      $UNDERWEAR_NAME$                Players actual weared underwear name
        //      $NEW_UNDERWEAR_NAME$            Players new underwear name
        //      $OLD_UNDERWEAR_NAME$            Players old underwear name
        //      $NPC_UNDERWEAR_NAME$            NPCs actual weared underwear name
        //      $NPC_NEW_UNDERWEAR_NAME$        NPCs new underwear name
        //      $NPC_OLD_UNDERWEAR_NAME$        NPCs old underwear name
        private static string ReplaceUnderwearName(string str, string underwearName, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "UNDERWEAR_NAME";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, underwearName);
        }

        // possible tokens:
        //      $UNDERWEAR_DESC$                Description of the players actual weared underwear
        //      $NEW_UNDERWEAR_DESC$            Description of the players new underwear
        //      $OLD_UNDERWEAR_DESC$            Description of the players old underwear
        //      $NPC_UNDERWEAR_DESC$            Description of the npcs actual weared underwear
        //      $NPC_NEW_UNDERWEAR_DESC$        Description of the npcs new underwear
        //      $NPC_OLD_UNDERWEAR_DESC$        Description of the npcs old underwear
        private static string ReplaceUnderwearDesc(string str, string description, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "UNDERWEAR_DESC";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, description);
        }

        public static string ReplaceInspectUnderwearToken(string str, Container underwear, bool npc = false, OldNew oldNew = OldNew.None)
        {
            str = ReplaceInspectUnderwearName(str,underwear,npc: npc, oldNew: oldNew);
            str = ReplaceInspectUnderwearName(str, underwear, npc: npc, noPrefix: true, oldNew: oldNew);
            str = ReplaceInspectUnderwearDesc(str, underwear, npc: npc, oldNew: oldNew);

            return str;
        }

        // possible tokens:
        //      $INSPECT_UNDERWEAR_NAME$                        State and name of the players actual weared underwear
        //      $INSPECT_UNDERWEAR_NAME_NO_PREFIX$              State and name of the players actual weared underwear without prefix
        //      $INSPECT_NEW_UNDERWEAR_NAME$                    State and name of the players new underwear
        //      $INSPECT_NEW_UNDERWEAR_NAME_NO_PREFIX$          State and name of the players new underwear without prefix
        //      $INSPECT_OLD_UNDERWEAR_NAME$                    State and name of the players old underwear
        //      $INSPECT_OLD_UNDERWEAR_NAME_NO_PREFIX$          State and name of the players old underwear without prefix
        //      $NPC_INSPECT_UNDERWEAR_NAME$                    State and name of the npcs actual weared underwear
        //      $NPC_INSPECT_UNDERWEAR_NAME_NO_PREFIX$          State and name of the npcs nactual weared underwear without prefix
        //      $NPC_INSPECT_NEW_UNDERWEAR_NAME$                State and name of the npcs new underwear
        //      $NPC_INSPECT_NEW_UNDERWEAR_NAME_NO_PREFIX$      State and name of the npcs new underwear without prefix
        //      $NPC_INSPECT_OLD_UNDERWEAR_NAME$                State and name of the npcs old underwear
        //      $NPC_INSPECT_OLD_UNDERWEAR_NAME_NO_PREFIX$      State and name of the npcs old underwear without prefix
        private static string ReplaceInspectUnderwearName(string str, Container underwear, bool noPrefix = false, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "UNDERWEAR_NAME";
            string tokenPre = "INSPECT_";

            if (noPrefix) token = token + "_NO_PREFIX";
            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            token = tokenPre + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, DescribeUnderwear(underwear, underwear.displayName, noPrefix));
        }

        // possible tokens:
        //      $INSPECT_UNDERWEAR_DESC$            Description of the players actual weared underwear
        //      $INSPECT_NEW_UNDERWEAR_DESC$        Description of the players new underwear
        //      $INSPECT_OLD_UNDERWEAR_DESC$        Description of the players old underwear
        //      $NPC_INSPECT_UNDERWEAR_DESC$        Description of the npcs actual weared underwear
        //      $NPC_INSPECT_NEW_UNDERWEAR_DESC$    Description of the npcs new underwear
        //      $NPC_INSPECT_OLD_UNDERWEAR_DESC$    Description of the npcs old underwear
        private static string ReplaceInspectUnderwearDesc(string str, Container underwear, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "UNDERWEAR_DESC";
            string tokenPre = "INSPECT_";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            token = tokenPre + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, DescribeUnderwear(underwear, underwear.description));
        }
        #endregion

        #region Replace Token Pants
        public static string ReplacePantsToken(string str, Container pants, bool npc = false, OldNew oldNew = OldNew.None)
        {
            str = ReplacePantsPrefix(str, pants.plural, npc, oldNew: oldNew);
            str = ReplacePantsName(str, pants.displayName, npc, oldNew: oldNew);
            str = ReplacePantsDesc(str, pants.description, npc, oldNew: oldNew);

            return str;
        }

        // possible tokens:
        //      $PANTS_PREFIX$              Prefix of the players actual weared pants
        //      $NEW_PANTS_PREFIX$          Prefix of the players new pants
        //      $OLD_PANTS_PREFIX$          Prefix of the players old pants
        //      $NPC_PANTS_PREFIX$          Prefix of the npsc actual weared pants
        //      $NPC_NEW_PANTS_PREFIX$      Prefix of the npsc new pants
        //      $NPC_OLD_PANTS_PREFIX$      Prefix of the npsc old pants
        private static string ReplacePantsPrefix(string str, bool plural = false, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "PANTS_PREFIX";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, getPrefix(plural));
        }

        // possible tokens:
        //      $PANTS_NAME$                Name of the players actual weared pants
        //      $NEW_PANTS_NAME$            Name of the players new pants
        //      $OLD_PANTS_NAME$            Name of the players old pants
        //      $NPC_PANTS_NAME$            Name of the npsc actual weared pants
        //      $NPC_NEW_PANTS_NAME$        Name of the npcs new pants
        //      $NPC_OLD_PANTS_NAME$        Name of the npcs old pants
        private static string ReplacePantsName(string str, string pantsName, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "PANTS_NAME";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, pantsName);
        }

        // possible tokens:
        //      $PANTS_DESC$                Description of the players actual weared pants
        //      $NEW_PANTS_DESC$            Description of the players new pants
        //      $OLD_PANTS_DESC$            Description of the players old pants
        //      $NPC_PANTS_DESC$            Description of the npcs actual weared pants
        //      $NPC_NEW_PANTS_DESC$        Description of the npcs new pants
        //      $NPC_OLD_PANTS_DESC$        Description of the npcs old pants
        private static string ReplacePantsDesc(string str, string description, bool npc = false, OldNew oldNew = OldNew.None)
        {
            string token = "PANTS_DESC";

            if (oldNew == OldNew.Old) token = "OLD_" + token;
            if (oldNew == OldNew.New) token = "NEW_" + token;
            if (npc) token = "NPC_" + token;

            token = "$" + token + "$";

            return str.Replace(token, description);
        }

        #endregion

        // format string: $GETTING_CHANGED_DIALOG$ \"npcName\"
        private static string ReplaceGetChangeDialog(string str, Container underwear)
        {
            string token = StringConstants.Token.GettingChangedDialog;

            if(!str.Contains(token)) return str;

            string gettingChangedDialog = RandString(changeData.Diaper_Change_Dialog);
            gettingChangedDialog = ReplaceAndOr(gettingChangedDialog, underwear.wetness > 0, underwear.messiness > 0);

            string npcName = "";
            string[] parameters;

            str = GetParameters(str, token,out parameters);

            if (parameters.Length >= 1)
            {
                npcName = parameters[0];
            }
            else throw new Exception($"Wrong Count of Parameters set for Token {token}");

            if (Game1.player.dialogueQuestionsAnswered.Contains("dirty_change_yes" + "_" + npcName.ToLower()))
                Game1.player.dialogueQuestionsAnswered.Remove("dirty_change_yes" + "_" + npcName.ToLower());
            if (Game1.player.dialogueQuestionsAnswered.Contains("dirty_change_no" + "_" + npcName.ToLower()))
                Game1.player.dialogueQuestionsAnswered.Remove("dirty_change_no" + "_" + npcName.ToLower());

            gettingChangedDialog = ReplaceNpcName(gettingChangedDialog, npcName.ToLower());

            str = str.Replace(token, "#$b#" + gettingChangedDialog);

            return str;
        }

        // format string: $CHANGED_BY_NPC$ \"npcName\" \"underwearName\" \"pantsName\" \"id\"
        // parameter 1: name of the npc who changes you
        // parameter 2: name of new underwear the npc will change the player
        // parameter 3: (optional) name of the pants the npc will change the player if there are dirty
        // parameter 4: (optional) id of the npc´s dialogue in "Villager_Changeing_Reactions" in file VillagerData.json
        //              if free try to randomize the id
        //              if no insert in "Villager_Changeing_Reactions" use gender specific dialogue
        public static string ReplaceChangedByNpc(string str, Container underwear)
        {
            string token = StringConstants.Token.ChangedByNpc;

            if (!str.Contains(token)) return str;

            string npcName = "";
            string underwearName = "";
            string pantsName = "";
            int id = -1;

            string[] parameters;

            str = GetParameters(str,token,out parameters);

            if (parameters.Length >= 2)
            {
                npcName = parameters[0].ToLower();
                underwearName = parameters[1];

                if (parameters.Length >= 3)
                {
                    if (!int.TryParse(parameters[2], out id))
                    {
                        id = -1;
                        pantsName = parameters[2];
                    }

                    if(parameters.Length >= 4)
                    {
                        if (!int.TryParse(parameters[3], out id)) 
                            throw new Exception("Parameter for id is not an int for Token: " + token);
                    }
                }

                if(!NpcHelper.IsValidNpc(npcName)) throw new Exception($"Parameter for npcName is not an valid npc: {npcName}");
            }
            else throw new Exception($"Wrong Count of Parameters set for Token {token}");

            string changedByNpc = GetVilagerChangingDialogByKey(npcName, "change", id);

            changedByNpc = tryGetI18nText(changedByNpc);

            changedByNpc = ReplaceAndOr(changedByNpc, underwear.wetness > 0, underwear.messiness > 0);

            changedByNpc = "#$b#" + changedByNpc;

            npcName = " \"" + npcName + "\"";
            underwearName = " \"" + underwearName + "\"";
            pantsName = pantsName != "" ? " \"" + pantsName + "\"" : "";

            string actionString = $"#$action DIAPER_CHANGE{npcName}{underwearName}{pantsName}";

            changedByNpc = changedByNpc + actionString;

            str = str.Replace(token, changedByNpc);

            return str;
        }

        // format string: $GET_CHANGE_PLAYER_ACCEPT_DIALOG$ \"npcName\" \"id\"
        // parameter 1: name of the npc which changing offer you accept
        // parameter 2: (optional) id of the npc´s dialogue in "Villager_Changeing_Reactions" in file VillagerData.json
        //              if free try to randomize the id
        //              if no insert in "Villager_Changeing_Reactions" use gender specific dialogue
        public static string ReplaceGetChangeDialogAccept(string str, Container underwear)
        {
            string token = StringConstants.Token.GetChangeAcceptDialog;

            if (!str.Contains(token)) return str;

            string npcName = "";
            int id = -1;

            string[] parameters;

            str = GetParameters(str, token, out parameters);

            if (parameters.Length >= 1)
            {
                npcName = parameters[0].ToLower();

                if (parameters.Length >= 2)
                {
                    if (!int.TryParse(parameters[1], out id))
                    {
                        id = -1;
                        throw new Exception("Parameter for id is not an int for Token " + token);
                    }
                }

                if (!NpcHelper.IsValidNpc(npcName)) throw new Exception($"Parameter for npcName is not an valid npc: {npcName}");
            }
            else throw new Exception($"Wrong Count of Parameters set for Token {token}");

            string acceptChangingByNpc = GetVilagerChangingDialogByKey(npcName, "player_accept", id);

            acceptChangingByNpc = tryGetI18nText(acceptChangingByNpc);

            acceptChangingByNpc = ReplaceAndOr(acceptChangingByNpc, underwear.wetness > 0, underwear.messiness > 0);

            acceptChangingByNpc = "#$b#" + acceptChangingByNpc;

            str = str.Replace(token, acceptChangingByNpc);

            return str;
        }

        // format string: $GET_CHANGE_PLAYER_REFUSE_DIALOG$ \"npcName\" \"id\"
        // parameter 1: name of the npc which changing offer you refuse
        // parameter 2: (optional) id of the npc´s dialogue in "Villager_Changeing_Reactions" in file VillagerData.json
        //              if free try to randomize the id
        //              if no insert in "Villager_Changeing_Reactions" use gender specific dialogue
        public static string ReplaceGetChangeDialogRefuse(string str, Container underwear)
        {
            string token = StringConstants.Token.GetChangeRefuseDialog;

            if (!str.Contains(token)) return str;

            string npcName = "";
            int id = -1;

            string[] parameters;

            str = GetParameters(str, token, out parameters);

            if (parameters.Length >= 1)
            {
                npcName = parameters[0].ToLower();

                if (parameters.Length >= 2)
                {
                    if (!int.TryParse(parameters[1], out id))
                    {
                        id = -1;
                        throw new Exception("Parameter for id is not an int for Token " + token);
                    }
                }

                if (!NpcHelper.IsValidNpc(npcName)) throw new Exception($"Parameter for npcName is not an valid npc: {npcName}");
            }
            else throw new Exception($"Wrong Count of Parameters set for Token {token}");

            string acceptChangingByNpc = GetVilagerChangingDialogByKey(npcName, "player_refuse", id);

            acceptChangingByNpc = tryGetI18nText(acceptChangingByNpc);

            acceptChangingByNpc = ReplaceAndOr(acceptChangingByNpc, underwear.wetness > 0, underwear.messiness > 0);

            acceptChangingByNpc = "#$b#" + acceptChangingByNpc;

            str = str.Replace(token, acceptChangingByNpc);

            return str;
        }

        private static string GetVilagerChangingDialogByKey(string npcName, string key = "player_get_change", int id = -1)
        {
            Dictionary<int, string> changingDialoges = new Dictionary<int, string>();

            changingDialoges = villagerDialogData.Villager_Changeing_Dialoges[npcName]?[key];

            if(changingDialoges != null)
            {
                if (id < 1)
                {
                    int rndId = 0;

                    rndId = Regression.rnd.Next(1, changingDialoges.Count);
                    return changingDialoges[rndId];
                }
                else
                {
                    string str = "";

                    if (!changingDialoges.TryGetValue(id, out str))
                        throw new Exception($"Id {id} not found in Villager_Changeing_Dialoge for {npcName} and key {key}");

                    return str;
                }
            }
            else
            {
                bool isFemale = NpcHelper.GetNpcGender(npcName) == Gender.Female ? true : false;

                return GetGenderChangingDialog(isFemale, key);
            }
        }

        public static string GetGenderChangingDialog(bool isFemale, string key = "player_get_change")
        {
            Dictionary<int, string> changingDialoges = new Dictionary<int, string>();

            int rndId = 0;

            if (!isFemale)
            {
                string keyGender = "adult_male";

                changingDialoges = villagerDialogData.Villager_Changeing_Dialoges[keyGender]?[key];

                if (changingDialoges != null)
                {
                    rndId = Regression.rnd.Next(1, changingDialoges.Count);
                    return changingDialoges[rndId];
                }
                else throw new Exception($"Key {keyGender} or Key {key} not found in Villager_Changeing_Dialoges");
            }
            else
            {
                string keyGender = "adult_female";

                changingDialoges = villagerDialogData.Villager_Changeing_Dialoges[keyGender]?[key];

                if (changingDialoges != null)
                {
                    rndId = Regression.rnd.Next(1, changingDialoges.Count);
                    return changingDialoges[rndId];
                }
                else throw new Exception($"Key {keyGender} or Key {key} not found in Villager_Changeing_Dialoges");
            }
        }

        // format string: $GET_CHANGE_NPC_ACCEPT_DIALOG$ \"npcName\" \"underwearName\" \"id\"
        // parameter 1: name of the npc which you offer a changing
        // parameter 2: name of new underwear you want to change the npc
        // parameter 3: (optional) id of the npc´s dialogue in "Villager_Changeing_Reactions" in file VillagerData.json
        //              if free try to randomize the id
        //              if no insert in "Villager_Changeing_Reactions" use gender specific dialogue
        public static string ReplaceChangeNpcDialogAccept(string str)
        {
            string token = StringConstants.Token.GetChangeAcceptDialog;

            if (!str.Contains(token)) return str;

            string npcName = "";
            string underwearName = "";
            int id = -1;

            string[] parameters;

            str = GetParameters(str, token, out parameters);

            if (parameters.Length >= 2)
            {
                npcName = parameters[0].ToLower();
                underwearName = parameters[1];

                if (parameters.Length >= 3)
                {
                    if (!int.TryParse(parameters[1], out id))
                    {
                        id = -1;
                        throw new Exception("Parameter for id is not an int for Token " + token);
                    }
                }

                if (!NpcHelper.IsValidNpc(npcName)) throw new Exception($"Parameter for npcName is not an valid npc: {npcName}");
            }
            else throw new Exception($"Wrong Count of Parameters set for Token {token}");

            string acceptChangingByNpc = GetVilagerChangingDialogByKey(npcName, underwearName, id);

            acceptChangingByNpc = tryGetI18nText(acceptChangingByNpc);

            acceptChangingByNpc = "#$b#" + acceptChangingByNpc;

            str = str.Replace(token, acceptChangingByNpc);

            return str;
        }

        public static string GetParameters(string str,string token, out string[] parameters)
        {
            string pattern = "\"([^\"]+)\"";
            parameters = null;

            if (!str.Contains(token)) return str;

            int indexParameter = str.IndexOf(token, 0) + token.Length;
            string substring = str.Substring(indexParameter, str.Length - indexParameter);
            int indexParameterEnd = substring.IndexOf("#", 0);

            if(indexParameterEnd > 0)
                substring = substring.Remove(indexParameterEnd + 1);

            MatchCollection matches = Regex.Matches(substring, pattern);
            parameters = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                parameters[i] = matches[i].Groups[1].Value;
            }

            return str.Replace(substring, "");
        }

        public static string ReplaceGenderTextNpc(string str, Gender gender)
        {
            str = str.Replace("$NPC_HE_SHE$", gender == Gender.Male ? getGenderText("He") : gender == Gender.Female ? getGenderText("She") : getGenderText("They"));
            str = str.Replace("$NPC_HIS_HER$", gender == Gender.Male ? getGenderText("His") : gender == Gender.Female ? getGenderText("Her") : getGenderText("Their"));
            str = str.Replace("$NPC_HIM_HER$", gender == Gender.Male ? getGenderText("Him") : gender == Gender.Female ? getGenderText("Her") : getGenderText("Them"));
            str = str.Replace("$NPC_HIS_HER_IS_ARE$", gender == Gender.Male ? getGenderText("His") : gender == Gender.Female ? getGenderText("Her") : getGenderText("Their"));

            return str;
        }

        private static string getPrefix(bool plural)
        {
            return plural ? Regression.help.Translation.Get("Numeric_Translation.Plural") : Regression.help.Translation.Get("Numeric_Translation.Sigle");
        }

        private static string getGenderText(string key)
        {
            key = "Gender_Text" + "." + key;

            return Regression.help.Translation.Get(key);
        }

        #region Replace Token Numeric

        public static string ReplaceNightTokens(string input, int gotUpAtNight, Body body)
        {
            string toiletMsg = input;

            toiletMsg = Strings.InsertVariable(toiletMsg, "$TOILET_ONCE_TWICE_TOTAL$", GetNumericTranslationOnceTwice(gotUpAtNight));
            toiletMsg = Strings.InsertVariable(toiletMsg, "$TOILET_TIMES_TOTAL$", GetNumericTranslationTimes(gotUpAtNight));
            toiletMsg = Strings.InsertVariable(toiletMsg, "$ACCIDENT_ONCE_TWICE_TOTAL$", GetNumericTranslationOnceTwice(Math.Max(body.numAccidentPeeAtNight, body.numAccidentPooAtNight)));
            toiletMsg = Strings.InsertVariable(toiletMsg, "$ACCIDENT_TIMES_TOTAL$", GetNumericTranslationTimes(Math.Max(body.numAccidentPeeAtNight, body.numAccidentPooAtNight)));
            toiletMsg = Strings.InsertVariable(toiletMsg, "$ACCIDENT_TIMES_MULTIPLE_TOTAL$", GetNumericTranslationMultiple(Math.Max(body.numAccidentPeeAtNight, body.numAccidentPooAtNight)));

            return toiletMsg;
        }

        private static string GetNumericTranslationMultiple(int amount)
        {
            switch (amount)
            {
                case 1:
                return "";
                default:
                return GetNumericTranslation("Multiple_Times");
            }
        }
        private static string GetNumericTranslationOnceTwice(int amount)
        {
            switch (amount)
            {
                case 1:
                return GetNumericTranslation("Once");
                case 2:
                return GetNumericTranslation("Twice");
                default:
                string st = GetNumericTranslation("Times");
                return $"{amount} {st}";
            }
        }
        private static string GetNumericTranslationTimes(int amount)
        {
            switch (amount)
            {
                case 1:
                return GetNumericTranslation("One_Time");
                case 2:
                return GetNumericTranslation("Two_Times");
                case 3:
                return GetNumericTranslation("Three_Times");
                case 4:
                return GetNumericTranslation("Four_Times");
                case 5:
                return GetNumericTranslation("Five_Times");
                default:
                string st = GetNumericTranslation("Times");
                return $"{amount} {st}";
            }
        }

        private static string GetNumericTranslation(string key)
        {
            return Regression.help.Translation.Get("Numeric_Translation." + key);
        }

        #endregion

        #endregion

        public static string tryGetI18nText(string key)
        {
            if (key == null) return null;

            string pattern = @"\{\{i18n:(.*?)\}\}";
            Regex regex = new Regex(pattern);

            string result = regex.Replace(key, match =>
            {
                string st = match.Groups[1].Value;
                return Regression.help.Translation.Get(st);
            });

            // Replace capacity tokens
            if (Regression.config != null)
            {
                result = result.Replace("$MAX_BLADDER_CAPACITY$", Regression.config.MaxBladderCapacity.ToString());
                result = result.Replace("$MAX_BOWEL_CAPACITY$", Regression.config.MaxBowelCapacity.ToString());
                // Fallback for @ tokens as reported by user
                result = result.Replace("@MAX_BLADDER_CAPACITY", Regression.config.MaxBladderCapacity.ToString());
                result = result.Replace("@MAX_BOWEL_CAPACITY", Regression.config.MaxBowelCapacity.ToString());
            }

            return result;
        }

        public enum OldNew
        {
            None,
            Old,
            New
        }
    }
}
