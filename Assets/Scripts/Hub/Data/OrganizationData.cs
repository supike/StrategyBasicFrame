using System.Collections.Generic;

namespace Hub
{
    [System.Serializable]
    public class OrganizationData
    {
        public float stability = 50f;      // 안정도 (0-100)
        public float crueltyIndex = 0f;    // 잔혹성 지수 (0-100)
        public float trustLevel = 50f;     // 신뢰도 (0-100)

        public int forcedOperationsCount = 0;
        public int totalCasualties = 0;
        public int consecutiveLosses = 0;

        public string GetStabilityLabel()
        {
            if (stability >= 80f) return "Stable";
            if (stability >= 50f) return "Uneasy";
            if (stability >= 25f) return "Fragile";
            return "Collapsing";
        }

        public List<string> GetLongTermEffects()
        {
            var effects = new List<string>();

            if (forcedOperationsCount >= 3)
                effects.Add("Recent forced operations are increasing...");
            if (totalCasualties >= 5)
                effects.Add("Becoming numb to losses.");
            if (consecutiveLosses >= 2)
                effects.Add("Morale is declining from consecutive defeats.");
            if (crueltyIndex >= 50f)
                effects.Add("The organization's methods are increasingly brutal.");
            if (trustLevel < 30f)
                effects.Add("Internal trust is critically low.");

            if (effects.Count == 0)
                effects.Add("No significant long-term effects.");

            return effects;
        }
    }
}
