using System.Collections.Generic;
using UnityEngine;

namespace Hub
{
    public enum HubResourceType
    {
        Personnel,   // 인원
        Information, // 정보
        Influence,   // 영향력
        Count
    }

    public class HubResourceManager : MonoBehaviour
    {
        public static HubResourceManager Instance { get; private set; }

        private Dictionary<HubResourceType, int> resources = new Dictionary<HubResourceType, int>();

        public GameEventSO onHubResourceChanged;
        public GameEventSO onWorldTensionChanged;

        [SerializeField] private float worldTension = 10f;
        public float WorldTension => worldTension;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            InitializeResources();
        }

        private void InitializeResources()
        {
            resources[HubResourceType.Personnel] = 12;
            resources[HubResourceType.Information] = 5;
            resources[HubResourceType.Influence] = 8;
        }

        public int GetResource(HubResourceType type)
        {
            return resources.TryGetValue(type, out int val) ? val : 0;
        }

        public void AddResource(HubResourceType type, int amount)
        {
            if (!resources.ContainsKey(type)) resources[type] = 0;
            resources[type] += amount;
            onHubResourceChanged?.Raise();
        }

        public bool SpendResource(HubResourceType type, int amount)
        {
            if (resources.TryGetValue(type, out int val) && val >= amount)
            {
                resources[type] -= amount;
                onHubResourceChanged?.Raise();
                return true;
            }
            return false;
        }

        public void IncreaseWorldTension(float amount)
        {
            worldTension = Mathf.Clamp(worldTension + amount, 0f, 100f);
            onWorldTensionChanged?.Raise();
        }

        public string GetTensionLevel()
        {
            if (worldTension >= 80f) return "Critical";
            if (worldTension >= 60f) return "High";
            if (worldTension >= 40f) return "Elevated";
            if (worldTension >= 20f) return "Guarded";
            return "Low";
        }
    }
}
