using StardewValley;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionMod
{
    public class ContinenceSystem
    {
        public const float TrainingThresholdFactor = 0.5f;
        public const float AttemptThresholdFactor = 0.15f;

        private readonly string keyPrefix;
        private readonly Func<float> maxCapacity;
        private readonly Func<float> startContinence;
        private readonly Func<bool> isEnabled;

        public ContinenceSystem(
            IncidentType type,
            string keyPrefix,
            Func<float> maxCapacity,
            float minContinence,
            Func<float> startContinence,
            Func<bool> isEnabled,
            float[] fullnessWarnThresholds,
            string[][] fullnessWarnMessages,
            float[] continenceWarnThresholds,
            string[][] continenceWarnMessages)
        {
            if (string.IsNullOrWhiteSpace(keyPrefix))
                throw new ArgumentException("A key prefix is required.", nameof(keyPrefix));
            if (minContinence < 0f || minContinence > 1f)
                throw new ArgumentOutOfRangeException(nameof(minContinence), "Must be between 0 and 1.");

            Type = type;
            this.keyPrefix = keyPrefix;
            this.maxCapacity = maxCapacity ?? throw new ArgumentNullException(nameof(maxCapacity));
            MinContinence = minContinence;
            this.startContinence = startContinence ?? throw new ArgumentNullException(nameof(startContinence));
            this.isEnabled = isEnabled ?? throw new ArgumentNullException(nameof(isEnabled));

            FullnessWarnThresholds = fullnessWarnThresholds ?? Array.Empty<float>();
            FullnessWarnMessages = fullnessWarnMessages ?? Array.Empty<string[]>();
            ContinenceWarnThresholds = continenceWarnThresholds ?? Array.Empty<float>();
            ContinenceWarnMessages = continenceWarnMessages ?? Array.Empty<string[]>();
        }

        #region Configuration

        public IncidentType Type { get; }

        public float MinContinence { get; }

        public bool IsEnabled => isEnabled();

        public float[] FullnessWarnThresholds { get; }
        public string[][] FullnessWarnMessages { get; }
        public float[] ContinenceWarnThresholds { get; }
        public string[][] ContinenceWarnMessages { get; }

        #endregion

        #region Saved values

        public float Fullness
        {
            get => LoadFloat("Fullness", 0f);
            set => SaveFloat("Fullness", Math.Max(0f, value));
        }

        public float Continence
        {
            get => LoadFloat("Continence", Math.Clamp(startContinence(), MinContinence, 1f));
            set => SaveFloat("Continence", Math.Clamp(value, MinContinence, 1f));
        }

        #endregion

        #region Derived values
        public float MaxCapacity => maxCapacity();

        public float Capacity => Continence * MaxCapacity;

        public float Percent
        {
            get
            {
                float capacity = Capacity;
                return capacity > 0f ? Fullness / capacity : 0f;
            }
        }

        public float TrainingThreshold => Capacity * TrainingThresholdFactor;

        public float AttemptThreshold => Capacity * AttemptThresholdFactor;

        public bool IsFull => Fullness >= Capacity;

        #endregion

        #region Changes

        public void AddFullness(float amount)
        {
            Fullness += amount;
            Fullness = Math.Min(Fullness, MaxCapacity);
        }

        public void LoseFullness(float amount)
        {
            Fullness -= amount;
            Fullness = Math.Max(Fullness, 0f);
        }

        public float Empty()
        {
            float amount = Fullness;
            Fullness = 0f;
            return amount;
        }

        public float ChangeContinence(float delta)
        {
            float previous = Continence;
            Continence = previous + delta;
            return previous;
        }

        #endregion

        #region Persistence

        private string BuildKey(string name) => $"{BodyConstants.ModDataPrefix}/{keyPrefix}{name}";

        private float LoadFloat(string name, float defaultValue)
        {
            var modData = Game1.player?.modData;
            if (modData == null || !modData.TryGetValue(BuildKey(name), out string raw) || string.IsNullOrWhiteSpace(raw))
                return defaultValue;

            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
                return value;
            if (float.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return value;

            Regression.monitor?.Log($"Could not read saved value '{BuildKey(name)}' = '{raw}', using default {defaultValue}.", StardewModdingAPI.LogLevel.Warn);
            return defaultValue;
        }

        private void SaveFloat(string name, float value)
        {
            var modData = Game1.player?.modData;
            if (modData == null) return;
            modData[BuildKey(name)] = value.ToString("R", CultureInfo.InvariantCulture);
        }

        #endregion

        public override string ToString()
        {
            return $"{keyPrefix}: {Fullness:0.#}/{Capacity:0.#} ({Percent:P0}), continence {Continence:0.00}";
        }
    }
}
