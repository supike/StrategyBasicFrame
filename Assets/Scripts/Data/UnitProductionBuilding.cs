using System.Collections.Generic;
using UnityEngine;

public class UnitProductionBuilding : Building
{
    [SerializeField] private List<UnitDataSO> availableUnits;
    [SerializeField] private Transform spawnPoint;
    
    private Queue<UnitDataSO> productionQueue = new Queue<UnitDataSO>();
    private float productionTimer;
    private float productionTime = 5f; // 5초당 1유닛
    
    void Update()
    {
        if (productionQueue.Count > 0)
        {
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= productionTime)
            {
                ProduceUnit();
                productionTimer = 0;
            }
        }
    }
    
    public void QueueUnit(UnitDataSO unitData)
    {
        // 비용 체크 및 차감
        List<ResourceAmount> cost = new List<ResourceAmount>
        {
            new ResourceAmount { type = ResourceType.Gold, amount = 50 },
            new ResourceAmount { type = ResourceType.Food, amount = 10 }
        };
        
        if (ResourceManager.Instance.CanAfford(cost))
        {
            ResourceManager.Instance.SpendResources(cost);
            productionQueue.Enqueue(unitData);
        }
    }
    
    void ProduceUnit()
    {
        UnitDataSO unitData = productionQueue.Dequeue();
        // 유닛 생성 로직
        // GameObject unitObj = Instantiate(unitData.prefab, spawnPoint.position, Quaternion.identity);
    }
}