using System;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Mods;
using static StardewValley.Objects.BedFurniture;

namespace RegressionMod
{

    public class Container
    {

        public static readonly string modDataAdd = "Container";

        private string modDataPrefix; // comes from the parent
        private ModDataDictionary modDataDictionary; // also comes from the parent

        private static TypesData typesData = Regression.typesData;

        #region Class Definitions

        //This class describes anything that we could wet/mess in. Usually underwear, but it could also be something like the bed.
        //These functions are pretty self-explanatory
        public Container()
        {
            wetness = 0.0f;
            messiness = 0.0f;
        }
        // in this case we have a parent. We keep track and update this parent for network sync purposes
        public Container(Underwear underwear, string fallbackType)
        {
            modDataPrefix = UnderwearConstants.ModDataPrefix;
            modDataDictionary = underwear.modData;
        }
        public Container(Body body, ContainerSubtype subtype, string fallbackType, bool updateToFallback = false)
        {
            modDataPrefix = BodyConstants.ModDataPrefix + "/" + GetStringSubtype(subtype);
            modDataDictionary = Game1.player.modData;

            if(updateToFallback)
            {
                ResetToDefault(fallbackType, subtype);
            }
            else
            {
                if (name == "")
                {
                    ResetToDefault(fallbackType, subtype);
                }
            }
        }
        public Container(NPC npc, ContainerSubtype subtype, string fallbackType, bool updateToFallback = false)
        {
            //Regression.monitor.Log($"Container: {npc.Name}  {subtype.ToString()}");
            modDataPrefix = "NPC/" + GetStringSubtype(subtype);
            modDataDictionary = npc.modData;

            if (updateToFallback)
            {
                ResetToDefault(fallbackType, subtype);
            }
            else
            {
                if (name == "")
                {
                    ResetToDefault(fallbackType, subtype);
                }
            }
        }

        #endregion

        #region Saved values

        // Here starts the block that contains all values we have to save
        private string _type = "";
        public string type
        {
            get => LoadString("type", _type);
            set
            {
                save("type", value);
                _type = value;
            }
        }

        private ContainerSubtype _subtype = ContainerSubtype.Underwear;
        public ContainerSubtype subtype
        {
            get => GetContainerSubtypeFromString(LoadString("subtype", GetStringSubtype(_subtype)));
            set
            {
                save("subtype", GetStringSubtype(value));
                _subtype = value;
            }
        }

        private string _name = "";
        public string name
        {
            get => LoadString("name", _name);
            set
            {
                save("name", value);
                _name = value;
            }
        }

        private string _displayName = "";
        public string displayName
        {
            get => LoadString("displayName", _displayName == "" ? name : _displayName);
            set
            {
                save("displayName", value);
                _displayName = value;
            }
        }

        private int _durability;
        public int durability
        {
            get => LoadInt("durability", _durability);
            set
            {
                save("durability", value);
                _durability = value;
            }
        }

        private float _messiness;
        public float messiness
        {
            get => LoadFloat("messiness", _messiness);
            set
            {
                save("messiness", value);
                _messiness = value;
            }
        }

        private float _wetness;
        public float wetness
        {
            get => LoadFloat("wetness", _wetness);
            set
            {
                save("wetness", value);
                _wetness = value;
            }
        }

        private string _timeWhenDoneDrying = "";
        public Date timeWhenDoneDrying
        {
            get => parseDryingDate(LoadString("timeWhenDoneDrying", _timeWhenDoneDrying));
            set
            {
                save("timeWhenDoneDrying", value);
                _timeWhenDoneDrying = serializeDryingDate(value);
            }
        }

#endregion

        public struct Date
        {
            public int time;
            public int day;
            public int season;
            public int year;
        }
                
        public static Date parseDryingDate(string date)
        {
            var dateObj = new Date();
            if (date == "")
            {
                dateObj.time = 0;
                dateObj.day = 0;
                dateObj.season = 0;
                dateObj.year = 0;
                return dateObj;
            }

            string[] splitted = date.Split("-");
            dateObj.time = int.Parse(splitted[0]);
            dateObj.day = int.Parse(splitted[1]);
            dateObj.season = int.Parse(splitted[2]);
            dateObj.year = int.Parse(splitted[3]);

            return dateObj;
        }

        public static string serializeDryingDate(Date timeWhenDoneDrying)
        {
            return string.Format("{0}-{1}-{2}-{3}", timeWhenDoneDrying.time, timeWhenDoneDrying.day, timeWhenDoneDrying.season, timeWhenDoneDrying.year);
        }
        public bool drying
        {
            get
            {
                if (timeWhenDoneDrying.Equals(new Date())) return false;
                Date currentDate;
                currentDate.time = Game1.timeOfDay;
                currentDate.day = Game1.dayOfMonth;
                currentDate.season = Utility.getSeasonNumber(Game1.currentSeason);
                currentDate.year = Game1.year;

                bool yearEq = currentDate.year == timeWhenDoneDrying.year;
                bool seasonEq = currentDate.season == timeWhenDoneDrying.season;
                bool dayEq = currentDate.day == timeWhenDoneDrying.day;
                bool timeEq = currentDate.time == timeWhenDoneDrying.time;
                bool yearGt = currentDate.year > timeWhenDoneDrying.year;
                bool seasonGt = currentDate.season > timeWhenDoneDrying.season;
                bool dayGt = currentDate.day > timeWhenDoneDrying.day;
                bool timeGt = currentDate.time > timeWhenDoneDrying.time;
                if ((yearGt) || (yearEq && seasonGt) || (yearEq && seasonEq && dayGt) || (yearEq && seasonEq && dayEq && (timeGt || timeEq)))
                {
                    timeWhenDoneDrying = new Date();
                    wetness = 0;
                    messiness = 0;
                    return false;
                }
                return true;
            }
        }

        public bool stackable
        {
            get
            {
                return !used && !drying && (InnerContainer == null || durability == InnerContainer.durability);
            }
        } 

        #region ModData save/load functions

        private void save(string varName, string val)
        {
            if (modDataDictionary == null) return;
            modDataDictionary[BuildKeyFor(varName)] = val;
        }
        private void save(string varName, int val)
        {
            if (modDataDictionary == null) return;
            modDataDictionary[BuildKeyFor(varName)] = val.ToString();
        }
        private void save(string varName, float val)
        {
            if (modDataDictionary == null) return;
            modDataDictionary[BuildKeyFor(varName)] = val.ToString();
        }
        private void save(string varName, bool val)
        {
            if (modDataDictionary == null) return;
            modDataDictionary[BuildKeyFor(varName)] = val.ToString();
        }
        private void save(string varName, Date val)
        {
            if (modDataDictionary == null) return;
            modDataDictionary[BuildKeyFor(varName)] = serializeDryingDate(val);
        }

        private string LoadString(string varName, string defaultVal)
        {
            if (!HasKeyFor(varName)) return defaultVal;
            return modDataDictionary[BuildKeyFor(varName)];
        }
        private int LoadInt(string varName, int defaultVal)
        {
            if (!HasKeyFor(varName)) return defaultVal;
            return int.Parse(modDataDictionary[BuildKeyFor(varName)]);
        }

        private float LoadFloat(string varName, float defaultVal)
        {
            if (!HasKeyFor(varName)) return defaultVal;
            return float.Parse(modDataDictionary[BuildKeyFor(varName)]);
        }
        private bool LoadBool(string varName, bool defaultVal)
        {
            if (!HasKeyFor(varName)) return defaultVal;
            return bool.Parse(modDataDictionary[BuildKeyFor(varName)]);
        }
        private Date LoadDate(string varName, Date defaultVal)
        {
            if (!HasKeyFor(varName)) return defaultVal;
            return parseDryingDate(modDataDictionary[BuildKeyFor(varName)]);
        }
        private string BuildKeyFor(string varName)
        {
            return BuildKeyFor(varName, modDataPrefix);
        }
        public static string BuildKeyFor(string varName, string modDataBaseKey)
        {
            return $"{modDataBaseKey}/{modDataAdd}/{varName}";
        }
        private bool HasKeyFor(string varName)
        {
            return modDataDictionary != null && modDataDictionary.ContainsKey(BuildKeyFor(varName));
        }

        #endregion

        #region Type variables from json configuration

        // Here starts the block that contains all values we can take from type
        private string _description;
        private float _absorbency;
        private float _containment;
        private bool _plural;
        private int _price;
        private int _spriteIndex;
        private bool _washable;
        private int _dryingTime;
        private bool _removable;

        public string description { get => _innerContainer != null && _description == null ? _innerContainer.description : _description; set => _description = value; }
        public float absorbency { get => InnerContainer != null ? InnerContainer.absorbency : _absorbency; set => _absorbency = value; }
        public float containment { get => InnerContainer != null ? InnerContainer.containment : _containment; set => _containment = value; }
        public bool plural { get => InnerContainer != null ? InnerContainer.plural : _plural; set => _plural = value; }
        public int price { get => InnerContainer != null ? InnerContainer.price : _price; set => _price = value; }
        public int spriteIndex { get => InnerContainer != null ? InnerContainer.spriteIndex : _spriteIndex; set => _spriteIndex = value; }
        public bool washable { get => InnerContainer != null ? InnerContainer.washable : _washable; set => _washable = value; }
        public int dryingTime { get => InnerContainer != null ? InnerContainer.dryingTime : _dryingTime; set => _dryingTime = value; }
        public bool removable { get => InnerContainer != null ? InnerContainer.removable : _removable; set => _removable = value; }

        public bool used { get => wetness > 0 || messiness > 0; }
        public bool used_bad { get => wetness > (absorbency / 2f) || messiness > 0; }

        private Container _innerContainer = null;
        public Container InnerContainer
        {
            get
            {
                if (_innerContainer == null && modDataDictionary != null)
                {
                    Container cont;

                    if(typesData.Type_Beds.TryGetValue(type,out cont)) _innerContainer = GetTypeDefault(type, ContainerSubtype.Bed);
                    else if((typesData.Type_Pants.TryGetValue(type, out cont))) _innerContainer = GetTypeDefault(type, ContainerSubtype.Pants);
                    else if ((typesData.Type_Underwears.TryGetValue(type, out cont))) _innerContainer = GetTypeDefault(type, ContainerSubtype.Underwear);
                    else _innerContainer = GetTypeDefault(type, ContainerSubtype.Underwear);
                }

                return _innerContainer;
            }
        }

        #endregion

        #region Public functions

        public void Wash(int dryingTimeOverwrite = -1)
        {
            if (washable)
            {
                if (durability != -1 && durability != 0) //infinite durability if -1
                {
                    durability--;
                }
                var done = new Date();
                done.time = Game1.timeOfDay + (dryingTimeOverwrite == -1 ? dryingTime : dryingTimeOverwrite);
                done.day = Game1.dayOfMonth;
                done.season = Utility.getSeasonNumber(Game1.currentSeason);
                done.year = Game1.year;

                if (done.time >= 2400)
                {
                    done.time -= 2400;
                    done.day += 1;
                }
                if (done.day > 28)
                {
                    done.day -= 28;
                    done.season += 1;
                }
                if (done.season > 4)
                {
                    done.season -= 4;
                    done.year += 1;
                }
                timeWhenDoneDrying = done;
            }
        }

        public bool MarkedForDestroy()
        {
            return (durability == 0) && washable;
        }


        public float AddPee(float amount)
        {
            wetness += amount;
            float difference = wetness - absorbency;
            if (difference > 0)
            {
                wetness = absorbency;
                return difference;
            }
            return 0.0f;
        }

        public float AddPoop(float amount)
        {
            messiness += amount;
            float difference = messiness - containment;
            if (difference > 0)
            {
                messiness = containment;
                return difference;
            }
            return 0.0f;
        }
        public float GetCapacity(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return absorbency;
                case IncidentType.POOP:
                    return containment;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public float GetUsed(IncidentType type)
        {
            switch (type)
            {
                case IncidentType.PEE:
                    return wetness;
                case IncidentType.POOP:
                    return messiness;
                default:
                    throw new Exception("Not implemented: type " + type.ToString());
            }
        }
        public void ResetToDefault(Container c, float wetness = -100, float messiness = -100, int durability = -100)
        {
            this._innerContainer = null;
            this.type = c.type;
            this.subtype = c.subtype;
            this.name = c.name;
            this.timeWhenDoneDrying = c.timeWhenDoneDrying;

            this.wetness = c.wetness;
            this.messiness = c.messiness;
            this.durability = c.durability;
            this.displayName = c.displayName;
            this.description = c.description;
            if (wetness != -100) this.wetness = wetness;
            if (messiness != -100) this.messiness = messiness;
            if (durability != -100) this.durability = durability;
        }

        public void ResetToDefault(string type, ContainerSubtype subtype, float wetness = -100, float messiness = -100, int durability = -100)
        {
            ResetToDefault(GetTypeDefault(type, subtype), wetness, messiness, durability);
        }
        public static Container GetTypeDefault(string type, ContainerSubtype subtype)
        {
            Container c = new Container();

            switch (subtype)
            {
                case ContainerSubtype.Bed:
                    if (!typesData.Type_Beds.TryGetValue(type, out c))
                    {
                        Regression.monitor.Log(string.Format("Invalid bed choice: {0}. Use default type.", type), LogLevel.Error);
                        typesData.Type_Beds.TryGetValue(ContainerConstants.Bed, out c);
                    }
                    break;
                case ContainerSubtype.Pants:
                    if (!typesData.Type_Pants.TryGetValue(type, out c))
                    {
                        Regression.monitor.Log(string.Format("Invalid pants choice: {0}. Use default type.", type), LogLevel.Error);
                        typesData.Type_Pants.TryGetValue(ContainerConstants.BlueJeans, out c);
                    }
                break;
                case ContainerSubtype.Underwear:
                default:
                    if (!typesData.Type_Underwears.TryGetValue(type, out c))
                    {
                        Regression.monitor.Log(string.Format("Invalid underwear choice: {0}. Use default type.", type),LogLevel.Error);
                        typesData.Type_Underwears.TryGetValue(UnderwearConstants.DinosaurUndies, out c);
                    }
                    break;
            }

            c.type = type;
            c.subtype = subtype;
            c.name = Strings.tryGetI18nText(c.name);
            c.description = Strings.tryGetI18nText(c.description);

            return c;
        }

        public static string GetStringSubtype(ContainerSubtype subtype)
        {
            switch (subtype)
            {
                case ContainerSubtype.Bed:
                    return "bed";
                case ContainerSubtype.Pants:
                    return "pants";
                case ContainerSubtype.Underwear:
                default:
                    return "underwear"; 
            }
        }

        public static ContainerSubtype GetContainerSubtypeFromString(string subtype)
        {
            if (subtype == "bed") return ContainerSubtype.Bed;
            if (subtype == "pants") return ContainerSubtype.Pants;
            if (subtype == "underwear") return ContainerSubtype.Underwear;
            return ContainerSubtype.Underwear;

        }

        #endregion
    }

    public enum ContainerSubtype
    {
        Bed = 1,
        Pants = 2,
        Underwear = 3,
    }
}
