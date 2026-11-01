using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Locations;
using System;

namespace RegressionMod
{
    public static class StatusBars
    {
        private static Color barBackgroundColor = Color.Black;
        private static Color barBackgroundTick = new Color(120, 120, 120);
        private static Color barBorderColor = Color.DarkGoldenrod;
        private static int barBorderWidth = 2;
        private static Color barForegroundColor = new Color(150, 150, 150);
        private static Color barForegroundTick = new Color(50, 50, 50);
        private static int barHeight = 204;
        private static int barWidth = 24;
        private static Texture2D barBackground;
        private static Texture2D barForeground;

        private static Body body => Regression.body;
        private static Farmer who => Game1.player;
        private static Config config => Regression.config;

        public static void DrawStatusBars()
        {

            int x1 = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - (65 + (int)((StatusBars.barWidth)));
            int y1 = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Bottom - (25 + (int)((StatusBars.barHeight)));

            if (Game1.currentLocation is MineShaft || Game1.currentLocation is Woods || Game1.currentLocation is SlimeHutch || Game1.currentLocation is VolcanoDungeon || who.health < who.maxHealth)
                x1 -= 58;

            if (!config.NoHungerAndThirst || config.Debug)
            {
                float percentage1 = body.GetHungerPercent();
                StatusBars.DrawStatusBar(x1, y1, percentage1, new Color(115, byte.MaxValue, 56));
                int x2 = x1 - (10 + StatusBars.barWidth);
                float percentage2 = body.GetThirstPercent();
                StatusBars.DrawStatusBar(x2, y1, percentage2, new Color(117, 225, byte.MaxValue));
                x1 = x2 - (10 + StatusBars.barWidth);
            }
            if (config.Debug)
            {
                if (config.Messing)
                {
                    float percentage = body.Get(IncidentType.POOP).Percent;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color(146, 111, 91));
                    x1 -= 10 + StatusBars.barWidth;
                }
                if (config.Wetting)
                {
                    float percentage = body.Get(IncidentType.PEE).Percent;
                    StatusBars.DrawStatusBar(x1, y1, percentage, new Color(byte.MaxValue, 225, 56));
                }
            }
            if ((!config.Wetting && !config.Messing) || config.HideUnderwearStatusIcon)
                return;
            int y3 = (Game1.player.questLog).Count == 0 ? 250 : 310;
            var x3 = Game1.graphics.GraphicsDevice.Viewport.TitleSafeArea.Right - 94;
            Icons.DrawUnderwearIcon(body.underwear, x3, y3);

            Icons.DrawStateIcon(body, IncidentType.PEE, x3, y3 + 74);
            Icons.DrawStateIcon(body, IncidentType.POOP, x3, y3 + 74 + 74);
        }

        public static void DrawStatusBar(int x, int y, float percentage, Color color)
        {
            SpriteBatch spriteBatch = (SpriteBatch)Game1.spriteBatch;
            if (StatusBars.barBackground == null || StatusBars.barForeground == null)
                StatusBars.CreateTextures();

            if (Game1.eventUp || Game1.farmEvent != null)
            {
                return;
            }

            percentage = Math.Min(percentage, 1f);
            Rectangle destinationRectangle = new Rectangle(x, y, StatusBars.barWidth, StatusBars.barHeight);
            spriteBatch.Draw(StatusBars.barBackground, destinationRectangle, new Rectangle?(new Rectangle(0, 0, StatusBars.barWidth, StatusBars.barHeight)), Color.White);
            int height = (int)((double)(destinationRectangle.Height - StatusBars.barBorderWidth * 2) * (double)percentage);
            destinationRectangle.Y = destinationRectangle.Y + destinationRectangle.Height - height - StatusBars.barBorderWidth;
            destinationRectangle.Height = height;
            spriteBatch.Draw(StatusBars.barForeground, destinationRectangle, new Rectangle?(new Rectangle(0, 0, StatusBars.barWidth, height)), color);
        }

        private static void CreateTextures()
        {
            StatusBars.barBackground = new Texture2D(((GraphicsDeviceManager)Game1.graphics).GraphicsDevice, StatusBars.barWidth, StatusBars.barHeight);
            StatusBars.barForeground = new Texture2D(((GraphicsDeviceManager)Game1.graphics).GraphicsDevice, StatusBars.barWidth, StatusBars.barHeight);
            Color[] data1 = new Color[StatusBars.barHeight * StatusBars.barWidth];
            Color[] data2 = new Color[StatusBars.barHeight * StatusBars.barWidth];
            for (int index1 = 0; index1 < StatusBars.barWidth; ++index1)
            {
            for (int index2 = 0; index2 < StatusBars.barHeight; ++index2)
            {
                Color color1 = StatusBars.barBackgroundColor;
                Color color2 = StatusBars.barForegroundColor;
                bool flag1 = index1 + 1 <= StatusBars.barBorderWidth || index1 >= StatusBars.barWidth - StatusBars.barBorderWidth;
                bool flag2 = index2 + 1 <= StatusBars.barBorderWidth || index2 >= StatusBars.barHeight - StatusBars.barBorderWidth;
                if (flag1 | flag2)
                {
                color1 = StatusBars.barBorderColor;
                color2 = Color.Transparent;
                if (flag1 & flag2)
                    color1 = Color.Transparent;
                }
                if (!flag1)
                {
                float scale = new float[10]
                {
                    1f,
                    1.3f,
                    1.7f,
                    2f,
                    1.9f,
                    1.5f,
                    1.3f,
                    1f,
                    0.8f,
                    0.4f
                }[(int)((double)index1 * 10.0 / (double)StatusBars.barWidth)];
                color1 = Color.Multiply(color1, scale);
                color2 = Color.Multiply(color2, scale);
                }
                data1[index1 + index2 * StatusBars.barWidth] = color1;
                data2[index1 + index2 * StatusBars.barWidth] = color2;
            }
            }
            StatusBars.barBackground.SetData<Color>(data1);
            StatusBars.barForeground.SetData<Color>(data2);
        }

        
    }
}
