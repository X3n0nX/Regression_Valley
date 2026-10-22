using Microsoft.Xna.Framework;
using StardewValley.Characters;
using StardewValley;
using System.Collections.Generic;
using System;

namespace RegressionMod
{
    public static class NpcHelper
    {
        private static Farmer _player => Game1.player;

        public static List<NPC> NearbyVillager(Body b, bool mess, bool inUnderwear, bool overflow, bool attempt = false)
        {
            int radius = 3;

            //If we are messing, increase the radius of noticeability (stinky)
            if (mess)
            {
                radius *= 2;
            }

            //If we pulled down our pants, quadruple the radius (not contained and visible!)
            //Double loss since you're just going in front of people (how uncouth)
            if (!inUnderwear)
            {
                radius *= 4;
            }

            //Double noticeability is we had a blow-out/leak (people can see)
            if (overflow)
                radius *= 2;

            return NearbyVillager(radius);
        }

        public static List<NPC> NearbyVillager(int radius)
        {
            var list = Utility.GetNpcsWithinDistance(_player.Tile, radius, Game1.currentLocation);

            var newList = new List<NPC>();
            foreach (var npcEntry in list)
            {
                newList.Add(npcEntry);
            }
            return newList;
        }

        public static bool IsValidNpc(string npcName)
        {
            List<NPC> npcs = Utility.getAllCharacters();

            npcName = npcName.FirstCharToUpper();

            foreach (NPC npc in npcs)
            {
                if (npc is Horse || npc is Pet) continue;

                if (npc.Name == npcName) return true;
            }

            return false;
        }

        public static Gender GetNpcGender(string npcName)
        {
            List<NPC> npcs = Utility.getAllCharacters();

            npcName = npcName.FirstCharToUpper();

            foreach (NPC npc in npcs)
            {
                if (npc is Horse || npc is Pet) continue;

                if (npc.Name == npcName) return npc.Gender;
            }

            return Gender.Undefined;
        }

        public static List<string> npcTypeList(NPC npc)
        {
            List<string> npcType = new List<string>();

            if (npc is Horse || npc is Pet)
            {
                npcType.Add("animal");
            }
            else
            {
                switch (npc.Age)
                {
                    case 0:
                    npcType.Add("adult");
                    break;
                    case 1:
                    npcType.Add("teen");
                    break;
                    case 2:
                    npcType.Add("kid");
                    break;
                }
                npcType.Add(npc.getName().ToLower());
            }
            return npcType;
        }

        public static NpcBody GetNpcByName(string name, int range = 20)
        {
            name = name.ToLower();
            foreach (NPC npc in range <= 0 ? Utility.getAllCharacters() : NpcHelper.NearbyVillager(range))
            {
                if (npc.Name.ToLower() == name)
                {
                    return new NpcBody(npc);
                }
            }
            return null;
        }
        public static List<NpcBody> GetNpcsByRange(int range = 10)
        {
            List<NpcBody> list = new List<NpcBody>();
            foreach (NPC npc in Utility.GetNpcsWithinDistance(_player.Tile, range, Game1.currentLocation))
            {
                list.Add(new NpcBody(npc));
            }
            return list;
        }

        public static NpcBody GetNpcInFront()
        {
            // Tile in front of the player, based on the facing direction
            Vector2 frontTile = _player.GetGrabTile();
            Rectangle frontRect = new Rectangle(
                (int)frontTile.X * Game1.tileSize,
                (int)frontTile.Y * Game1.tileSize,
                Game1.tileSize,
                Game1.tileSize);

            NPC npc = null;

            foreach (NPC n in Game1.currentLocation.characters)
            {
                if (n == null || n is Horse || n is Pet || !n.IsVillager) continue;

                // Exact tile match first, otherwise the NPC's bounding box overlaps the tile (NPC is walking between tiles)
                if (n.Tile == frontTile)
                {
                    npc = n;
                    break;
                }
                if (npc == null && n.GetBoundingBox().Intersects(frontRect))
                {
                    npc = n;
                }
            }

            return npc == null ? null : new NpcBody(npc);
        }

        public static bool NpcInRange(Vector2 actualPosition, GameLocation location, string name, int range = 10)
        {
            name = name.ToLower();
            foreach (NPC npc in Utility.GetNpcsWithinDistance(actualPosition, range, location))
            {
                if (npc.Name == name) return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a specific npc is in range of an other npc.
        /// </summary>
        public static bool NpcInRange(NPC npcRequester, string name, int range = 10)
        {
            name = name.ToLower();
            foreach (NPC npc in Utility.GetNpcsWithinDistance(npcRequester.Tile, range, npcRequester.currentLocation))
            {
                if (npc.Name == name) return true;
            }

            return false;
        }

        /// <summary>
        /// Check if a specific npc is at an specific location.
        /// </summary>
        public static bool NpcAtLocation(NPC npcRequester, string location)
        {
            location = location.ToLower();
            if (npcRequester.currentLocation.Name.ToLower() == location) return true;
            return false;
        }
    }
}
