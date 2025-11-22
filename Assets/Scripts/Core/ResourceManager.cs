using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Gold,
    Food,
    Wood,
    Stone,
    Count
}

[System.Serializable]
public class ResourceAmount
{
    public ResourceType type;
    public int amount;
}

public class ResourceManager : MonoBehaviour
{
    public static ResourceManager Instance { get; private set; }

    private Dictionary<ResourceType, int> resources = new Dictionary<ResourceType, int>();

    public GameEventSO onResourceChanged;

    void Awake()
    {
        Instance = this;
        InitializeResources();
    }

    void InitializeResources()
    {
        foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
        {
            resources[type] = 100; // 초기 자원
        }
    }

    public void AddResource(ResourceType type, int amount)
    {
        resources[type] += amount;
        onResourceChanged?.Raise();
    }

    public bool SpendResource(ResourceType type, int amount)
    {
        if (resources[type] >= amount)
        {
            resources[type] -= amount;
            onResourceChanged?.Raise();
            return true;
        }

        return false;
    }

    public bool CanAfford(List<ResourceAmount> costs)
    {
        foreach (var cost in costs)
        {
            if (resources[cost.type] < cost.amount)
                return false;
        }

        return true;
    }

    public void SpendResources(List<ResourceAmount> costs)
    {
        foreach (var cost in costs)
        {
            SpendResource(cost.type, cost.amount);
        }
    }

    public int GetResource(ResourceType type)
    {
        return resources[type];
    }
}