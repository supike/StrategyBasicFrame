using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "Game/Building Data")]
public class BuildingDataSO : ScriptableObject
{
    public string buildingName;
    public Sprite icon;
    public GameObject prefab;
    public List<ResourceAmount> buildCost;
    public float productionInterval;
    public ResourceType producedResource;
    public int productionAmount;
}

public class Building : MonoBehaviour
{
    [SerializeField] private BuildingDataSO buildingData;
    
    private float productionTimer;
    
    void Update()
    {
        if (buildingData.producedResource != 0)
        {
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= buildingData.productionInterval)
            {
                ProduceResource();
                productionTimer = 0;
            }
        }
    }
    
    void ProduceResource()
    {
        ResourceManager.Instance.AddResource(
            buildingData.producedResource,
            buildingData.productionAmount
        );
    }
    
    public static bool TryBuild(BuildingDataSO data, Vector3 position)
    {
        if (ResourceManager.Instance.CanAfford(data.buildCost))
        {
            ResourceManager.Instance.SpendResources(data.buildCost);
            Instantiate(data.prefab, position, Quaternion.identity);
            return true;
        }
        return false;
    }
}