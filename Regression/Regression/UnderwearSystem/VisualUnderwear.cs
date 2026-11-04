using System;
using System.Linq;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Pants;

namespace RegressionMod
{
    public static class VisualUnderwear
    {
        public const string TextureAssetPrefix = "Regression_Underwear/";
        public const string TextureFolder = "Assets/Underwear";
        public const string ItemIdPrefix = "Regression_Underwear_";

        /// <summary>All underwear types that can be drawn. Each has its own texture in <see cref="TextureFolder"/>.</summary>
        public static readonly string[] Types =
        {
            UnderwearConstants.BlackThong,
            UnderwearConstants.PolkaDotPanties,
            UnderwearConstants.BigKidUndies,
            UnderwearConstants.DinosaurUndies,
            UnderwearConstants.LavenderPullups,
            UnderwearConstants.TrainingPants,
            UnderwearConstants.JojaDiaper,
            UnderwearConstants.BabyPrintDiaper,
            UnderwearConstants.ClothDiaper,
            UnderwearConstants.SpaceDiaper,
            UnderwearConstants.PawprintDiaper,
            UnderwearConstants.HeartDiaper,
            UnderwearConstants.LilSwabbies
        };

        /// <summary>Visible states of the underwear. The value is the sprite index in the type's texture.</summary>
        public enum PantsState
        {
            Clean = 0,
            Wet = 1,
            Messy = 2,
            WetAndMessy = 3
        }

        private static readonly string WornUnderwearBaseKey = BodyConstants.ModDataPrefix + "/" + Container.GetStringSubtype(ContainerSubtype.Underwear);

        private static readonly string WornUnderwearTypeKey = Container.BuildKeyFor("type", WornUnderwearBaseKey);
        private static readonly string WornUnderwearWetnessKey = Container.BuildKeyFor("wetness", WornUnderwearBaseKey);
        private static readonly string WornUnderwearMessinessKey = Container.BuildKeyFor("messiness", WornUnderwearBaseKey);

        #region Names

        /// <summary>File and asset name part for an underwear type, e.g. "heart_diaper".</summary>
        public static string GetFileKey(string underwearType)
        {
            return underwearType.Trim().ToLowerInvariant().Replace(' ', '_');
        }

        public static string GetTextureAssetName(string underwearType)
        {
            return TextureAssetPrefix + GetFileKey(underwearType);
        }

        /// <summary>Internal pants ID for an underwear type and state, e.g. "Regression_Pants_heart_diaper_wet".</summary>
        public static string GetItemId(string underwearType, PantsState state)
        {
            string id = ItemIdPrefix + GetFileKey(underwearType);
            switch (state)
            {
                case PantsState.Wet: return id + "_wet";
                case PantsState.Messy: return id + "_messy";
                case PantsState.WetAndMessy: return id + "_wet_messy";
                default: return id;
            }
        }

        public static bool IsKnownType(string underwearType)
        {
            return underwearType != null && Types.Contains(underwearType.Trim().ToLowerInvariant());
        }

        #endregion

        #region Assets

        public static void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.StartsWith(TextureAssetPrefix, allowPartialWord: false, allowSubfolder: true))
            {
                string fileKey = e.NameWithoutLocale.BaseName.Substring(TextureAssetPrefix.Length);
                string type = Types.FirstOrDefault(t => GetFileKey(t).Equals(fileKey, StringComparison.OrdinalIgnoreCase));
                if (type != null)
                    e.LoadFromModFile<Texture2D>($"{TextureFolder}/{GetFileKey(type)}.png", AssetLoadPriority.Exclusive);
            }
            else if (e.NameWithoutLocale.IsEquivalentTo("Data/Pants"))
            {
                e.Edit(asset =>
                {
                    var data = asset.AsDictionary<string, PantsData>().Data;
                    foreach (string type in Types)
                    {
                        Container info = Container.GetTypeDefault(type, ContainerSubtype.Underwear);
                        string baseName = Capitalize(Strings.tryGetI18nText(info?.displayName ?? type));

                        foreach (PantsState state in Enum.GetValues(typeof(PantsState)))
                        {
                            string id = GetItemId(type, state);
                            data[id] = new PantsData
                            {
                                Name = id,
                                DisplayName = state == PantsState.Clean ? baseName : $"{baseName} ({GetStateName(state)})",
                                Description = Strings.tryGetI18nText(info?.description ?? ""),
                                Price = Math.Max(1, info?.price ?? 50),
                                Texture = GetTextureAssetName(type),
                                SpriteIndex = (int)state,
                                DefaultColor = "255 255 255", // the sprites have their own colors, white keeps them unchanged
                                CanBeDyed = false,
                                IsPrismatic = false,
                                CanChooseDuringCharacterCustomization = false
                            };
                        }
                    }
                });
            }
        }

        private static string GetStateName(PantsState state)
        {
            string key = state switch
            {
                PantsState.Wet => "Underwear.State.Wet",
                PantsState.Messy => "Underwear.State.Messy",
                _ => "Underwear.State.WetAndMessy"
            };
            return Regression.help.Translation.Get(key);
        }

        private static string Capitalize(string s)
        {
            return string.IsNullOrEmpty(s) ? s : char.ToUpper(s[0]) + s.Substring(1);
        }

        #endregion

        #region Show worn underwear when no pants are equipped

        /// <summary>
        /// Returns the internal pants ID that shows the underwear this farmer wears in its current state,
        /// or null if the option is off, the farmer wears pants (or a bathing suit) or no known underwear.
        /// </summary>
        public static string GetUnderwearPantsIdFor(Farmer farmer)
        {
            if (farmer == null || Regression.config == null || !Regression.config.ShowUnderwearWithoutPants)
                return null;
            if (farmer.pantsItem.Value != null || farmer.bathingClothes.Value)
                return null;
            if (!farmer.modData.TryGetValue(WornUnderwearTypeKey, out string type) || !IsKnownType(type))
                return null;

            return GetItemId(type, GetWornState(farmer));
        }

        /// <summary>Current state of the underwear the farmer wears, read from their modData.</summary>
        public static PantsState GetWornState(Farmer farmer)
        {
            bool wet = ReadFloat(farmer, WornUnderwearWetnessKey) > 0f;
            bool messy = ReadFloat(farmer, WornUnderwearMessinessKey) > 0f;

            if (wet && messy) return PantsState.WetAndMessy;
            if (messy) return PantsState.Messy;
            if (wet) return PantsState.Wet;
            return PantsState.Clean;
        }

        private static float ReadFloat(Farmer farmer, string key)
        {
            if (!farmer.modData.TryGetValue(key, out string raw) || string.IsNullOrWhiteSpace(raw))
                return 0f;
            // Container writes with the current culture; accept both formats
            if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float value))
                return value;
            if (float.TryParse(raw, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.CurrentCulture, out value))
                return value;
            return 0f;
        }

        /// <summary>
        /// Farmer.IsOverridingPants is what the game uses to draw other pants than the equipped item
        /// (e.g. the bathing suit). Draw the worn underwear when the farmer has no pants item on.
        /// </summary>
        [HarmonyPatch(typeof(Farmer), "IsOverridingPants")]
        public static class Farmer_IsOverridingPants_Patch
        {
            static void Postfix(Farmer __instance, ref string id, ref Color? color, ref bool __result)
            {
                if (__result) return; // the game already overrides (bathing suit etc.)

                try
                {
                    string pantsId = GetUnderwearPantsIdFor(__instance);
                    if (pantsId == null) return;

                    id = pantsId;
                    color = Color.White;
                    __result = true;
                }
                catch (Exception ex)
                {
                    Regression.monitor?.LogOnce($"Could not draw the worn underwear: {ex}", LogLevel.Warn);
                }
            }
        }

        // The two patches below make sure the texture and color are right even if the renderer
        // does not go through IsOverridingPants for every call.

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.GetDisplayPants))]
        public static class Farmer_GetDisplayPants_Patch
        {
            static void Postfix(Farmer __instance, ref Texture2D texture, ref int spriteIndex)
            {
                try
                {
                    string pantsId = GetUnderwearPantsIdFor(__instance);
                    if (pantsId == null) return;

                    var data = ItemRegistry.GetDataOrErrorItem("(P)" + pantsId);
                    texture = data.GetTexture();
                    spriteIndex = data.SpriteIndex;
                }
                catch (Exception ex)
                {
                    Regression.monitor?.LogOnce($"Could not draw the worn underwear: {ex}", LogLevel.Warn);
                }
            }
        }

        [HarmonyPatch(typeof(Farmer), nameof(Farmer.GetPantsColor))]
        public static class Farmer_GetPantsColor_Patch
        {
            static void Postfix(Farmer __instance, ref Color __result)
            {
                if (GetUnderwearPantsIdFor(__instance) != null)
                    __result = Color.White;
            }
        }

        #endregion
    }
}
