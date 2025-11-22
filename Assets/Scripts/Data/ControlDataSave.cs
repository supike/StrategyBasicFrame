using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int gold;
    public int food;
    public int wood;
    public int stone;
    public List<BuildingSaveData> buildings;
    public List<UnitSaveData> units;
}

[System.Serializable]
public class BuildingSaveData
{
    public string buildingId;
    public float posX;
    public float posY;
}

[System.Serializable]
public class UnitSaveData
{
    public string unitId;
    public int currentHealth;
}

public class SaveSystem : MonoBehaviour
{
    private static string SavePath => Application.persistentDataPath + "/savegame.json";
    
    public static void SaveGame()
    {
        SaveData data = new SaveData();
        
        // 자원 저장
        data.gold = ResourceManager.Instance.GetResource(ResourceType.Gold);
        data.food = ResourceManager.Instance.GetResource(ResourceType.Food);
        data.wood = ResourceManager.Instance.GetResource(ResourceType.Wood);
        data.stone = ResourceManager.Instance.GetResource(ResourceType.Stone);
        
        // 건물 저장
        data.buildings = new List<BuildingSaveData>();
        foreach (var building in FindObjectsOfType<Building>())
        {
            BuildingSaveData buildingData = new BuildingSaveData
            {
                buildingId = building.name,
                posX = building.transform.position.x,
                posY = building.transform.position.y
            };
            data.buildings.Add(buildingData);
        }
        
        // JSON 변환 및 저장
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        
        Debug.Log("Game Saved!");
    }
    
    public static SaveData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            
            Debug.Log("Game Loaded!");
            return data;
        }
        
        Debug.Log("No save file found.");
        return null;
    }
}