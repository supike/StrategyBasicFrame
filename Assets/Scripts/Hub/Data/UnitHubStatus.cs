namespace Hub
{
    public enum DangerSignal
    {
        None,
        CollapseImminent, // 붕괴 임박
        Trauma,           // 트라우마
        Rebellious        // 반항 성향
    }

    [System.Serializable]
    public class UnitHubStatus
    {
        public Unit unit;

        public UnitHubStatus(Unit unit)
        {
            this.unit = unit;
        }

        public DangerSignal GetDangerSignal()
        {
            if (unit == null || !unit.IsAlive) return DangerSignal.None;

            if (unit.stats.morale < 20)
                return DangerSignal.CollapseImminent;
            if (unit.unitData != null && unit.unitData.EmotionCondition == EmotionCondition.desperatated)
                return DangerSignal.Trauma;
            if (unit.stats.morale < 40 && unit.unitData != null && unit.unitData.EmotionCondition == EmotionCondition.bad)
                return DangerSignal.Rebellious;

            return DangerSignal.None;
        }

        public string GetEmotionSummary()
        {
            if (unit == null || unit.unitData == null) return "Unknown";
            return unit.unitData.EmotionCondition switch
            {
                EmotionCondition.desperatated => "Desperate",
                EmotionCondition.bad => "Troubled",
                EmotionCondition.normal => "Stable",
                EmotionCondition.good => "Content",
                EmotionCondition.happy => "High Spirits",
                _ => "Unknown"
            };
        }

        public string GetDangerSignalText()
        {
            return GetDangerSignal() switch
            {
                DangerSignal.CollapseImminent => "COLLAPSE IMMINENT",
                DangerSignal.Trauma => "TRAUMA",
                DangerSignal.Rebellious => "REBELLIOUS",
                _ => ""
            };
        }
    }
}
