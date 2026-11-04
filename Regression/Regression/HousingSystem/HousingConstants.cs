using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionMod
{
    internal class HousingConstants
    {
        public const string ToiletTextureAssetKey = "Regression_Toilets";
        public const string ToiletTexturePath = "Assets/Housing/toilets.png";
        public static readonly (string Id, string DisplayName, int SpriteIndex)[] ToiletVariants = new[]
        {
            // Each color variant occupies a 3x4 block; use the full 1x2 toilet sprite.
            ("Regression_Toilet_White", "White Toilet", 0),
            ("Regression_Toilet_Gray", "Gray Toilet", 12),
            ("Regression_Toilet_Purple", "Purple Toilet", 24),
            ("Regression_Toilet_Mint", "Mint Toilet", 36),
            ("Regression_Toilet_Yellow", "Yellow Toilet", 48),
            ("Regression_Toilet_Pink", "Pink Toilet", 60)
        };
    }
}
