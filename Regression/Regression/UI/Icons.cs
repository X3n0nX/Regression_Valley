using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using System;

namespace RegressionMod
{
    public class Icons
    {
        private static Texture2D _sprites;
        public static Texture2D sprites
        {
            get
            {
                _sprites ??= Regression.help.ModContent.Load<Texture2D>(AnimationConstants.Sprites);
                return _sprites;
            }
        }
        private static Texture2D _peepoopSprites;
        public static Texture2D peepoopSprites
        {
            get
            {
                _peepoopSprites ??= Regression.help.ModContent.Load<Texture2D>(AnimationConstants.PeePoopIcon);
                return _peepoopSprites;
            }
        }

        public static void DrawUnderwearIcon(Container c, int x, int y)
        {
            Color defaultColor = Color.White;

            Texture2D underwearSprites = sprites;
            Rectangle srcBoxCurrent = UnderwearRectangle(c, FullnessType.None, AnimationConstants.LargeSpriteDim);

            Rectangle destBoxCurrent = new Rectangle(x, y, AnimationConstants.DiaperHudDim, AnimationConstants.DiaperHudDim);

            ((SpriteBatch)Game1.spriteBatch).Draw(underwearSprites, destBoxCurrent, srcBoxCurrent, defaultColor);
            if (Game1.getMouseX() >= x && Game1.getMouseX() <= x + AnimationConstants.DiaperHudDim && Game1.getMouseY() >= y && Game1.getMouseY() <= y + AnimationConstants.DiaperHudDim)
            {
                string source = Strings.DescribeUnderwear(c, (string)null);
                string str = source.First<char>().ToString().ToUpper() + source.Substring(1);
                int num = Game1.tileSize * 6 + Game1.tileSize / 6;
                IClickableMenu.drawHoverText((SpriteBatch)Game1.spriteBatch, Game1.parseText(str, (SpriteFont)Game1.tinyFont, num), (SpriteFont)Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, x - (AnimationConstants.DiaperHudDim * 5), y);
            }
        }
        public static void DrawStateIcon(Body b, IncidentType type, int x, int y)
        {
            float newFullness = b.Get(type).Fullness / b.Get(type).Capacity;

            if (newFullness < Body.trainingThreshold || newFullness < (1 - b.Get(type).Continence)) return;

            int topOffset = AnimationConstants.LargeSpriteDim - (int)Math.Ceiling(newFullness * AnimationConstants.LargeSpriteDim);
            Color defaultColor = Color.White;

            Texture2D peeOrPoopIcon = peepoopSprites;
            if (peeOrPoopIcon == null) return;
            Rectangle emptyIcon = StatusRectangle(b, type, false, AnimationConstants.LargeSpriteDim);
            Rectangle filledIcon = StatusRectangle(b, type, true, AnimationConstants.LargeSpriteDim, topOffset);

            Rectangle emptyBoxCurrent = new Rectangle(x, y, AnimationConstants.DiaperHudDim, AnimationConstants.DiaperHudDim);
            Rectangle fullBoxCurrent = new Rectangle(x, y + topOffset, AnimationConstants.DiaperHudDim, AnimationConstants.DiaperHudDim - topOffset);

            ((SpriteBatch)Game1.spriteBatch).Draw(peeOrPoopIcon, emptyBoxCurrent, emptyIcon, defaultColor);
            ((SpriteBatch)Game1.spriteBatch).Draw(peeOrPoopIcon, fullBoxCurrent, filledIcon, defaultColor);
            if (Game1.getMouseX() >= x && Game1.getMouseX() <= x + AnimationConstants.DiaperHudDim && Game1.getMouseY() >= y && Game1.getMouseY() <= y + AnimationConstants.DiaperHudDim)
            {
                string source = Strings.tryGetI18nText(Animations.GetPottyFeeling(b, type)[0]);
                string str = source.First<char>().ToString().ToUpper() + source.Substring(1);
                int num = Game1.tileSize * 6 + Game1.tileSize / 6;
                IClickableMenu.drawHoverText((SpriteBatch)Game1.spriteBatch, Game1.parseText(str, (SpriteFont)Game1.tinyFont, num), (SpriteFont)Game1.smallFont, 0, 0, -1, null, -1, null, null, 0, null, -1, x - (AnimationConstants.DiaperHudDim * 5), y);
            }
        }

        public static Rectangle UnderwearRectangle(Container c, FullnessType type = FullnessType.None, int height = AnimationConstants.LargeSpriteDim)
        {
            if (c.spriteIndex == -1)
                throw new Exception("Invalid sprite index.");
            int num = 0;
            //Using switch statement instead of ternary operator for better readability and to add another type.
            if (type != FullnessType.None)
            {
                switch (type)
                {
                    case (FullnessType.Clear):
                    {
                        num = 0;
                        break;
                    }
                    case (FullnessType.Messy):
                    {
                        num = AnimationConstants.LargeSpriteDim;
                        break;
                    }
                    case (FullnessType.Wet):
                    {
                        num = AnimationConstants.LargeSpriteDim * 2;
                        break;
                    }
                    case (FullnessType.WetMessy):
                    {
                        num = AnimationConstants.LargeSpriteDim * 3;
                        break;
                    }
                    case (FullnessType.Drying):
                    {
                        num = AnimationConstants.LargeSpriteDim * 4;
                        break;
                    }
                    default:
                    {
                        num = 0;
                        break;
                    }
                }
            }
            else
            {
                if (!c.drying)
                {
                    if ((double)c.messiness <= .0f)
                    {
                        if ((double)c.wetness <= .0f)
                            num = 0;
                        else
                            num = AnimationConstants.LargeSpriteDim;
                    }
                    else
                    {
                        if ((double)c.wetness <= .0f)
                            num = AnimationConstants.LargeSpriteDim * 2;
                        else
                            num = AnimationConstants.LargeSpriteDim * 3;
                    }
                }
                else
                {
                    num = AnimationConstants.LargeSpriteDim * 4;
                }
            }
            return new Rectangle(c.spriteIndex * AnimationConstants.LargeSpriteDim, num + (AnimationConstants.LargeSpriteDim - height), AnimationConstants.LargeSpriteDim, height);
        }
        public static Rectangle StatusRectangle(Body b, IncidentType type, bool isFilledPicture, int height = AnimationConstants.LargeSpriteDim, int topOffset = 0)
        {
            return new Rectangle((int)type * AnimationConstants.LargeSpriteDim, (isFilledPicture ? AnimationConstants.LargeSpriteDim : 0) + topOffset + (AnimationConstants.LargeSpriteDim - height), AnimationConstants.LargeSpriteDim, height - topOffset);
        }
    }
}
