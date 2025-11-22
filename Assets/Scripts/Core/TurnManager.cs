using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public static TurnManager Instance { get; private set; }
    
    public enum Turn { Action, Strategy, Player, Enemy }
    public Turn CurrentTurn { get; private set; }
    
    private List<Unit> playerUnits = new List<Unit>();
    private List<Unit> enemyUnits = new List<Unit>();
    private int currentUnitIndex = 0;
    
    public GameEventSO onTurnChanged;
    public GameEventSO onTurnChanging;
    public GameEventSO onPlayerTurnStart;
    public GameEventSO onEnemyTurnStart;
    
    private int turnCount = 0;
    private int turnHour = 0;
    
    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        
    }
    
    public void StartBattle(List<Unit> players, List<Unit> enemies)
    {
        playerUnits = players;
        enemyUnits = enemies;
        CurrentTurn = Turn.Player;
        currentUnitIndex = 0;
        
        StartPlayerTurn();
    }
    
    void StartPlayerTurn()
    {
        CurrentTurn = Turn.Player;
        onPlayerTurnStart?.Raise();
        
        // 모든 플레이어 유닛 행동 가능 상태로
        foreach (var unit in playerUnits)
        {
            unit.ResetTurnActions();
        }
    }
    
    void StartEnemyTurn()
    {
        CurrentTurn = Turn.Enemy;
        onEnemyTurnStart?.Raise();
        
        // 적 AI 실행
        StartCoroutine(ExecuteEnemyTurn());
    }
    
    IEnumerator ExecuteEnemyTurn()
    {
        foreach (var enemy in enemyUnits)
        {
            if (enemy.IsAlive)
            {
                yield return StartCoroutine(enemy.ExecuteAI());
            }
        }
        
        EndTurn();
    }
    
    public void EndTurn()
    {
        /*if (CurrentTurn == Turn.Player)
        {
            StartEnemyTurn();
        }
        else
        {
            StartPlayerTurn();
        }*/

        ChangeTurn();
        onTurnChanged?.Raise();
    }

    public void ChangeTurn()
    {
        CurrentTurn = CurrentTurn == Turn.Action ? Turn.Strategy : Turn.Action;
    }
    public int AddTurnHour()
    {
        turnHour++;
        //turnHour = turnHour >= 24 ? 0 : turnHour;
        if (turnHour >= 24)
        {
            EndTurn();
            
            turnHour = 0;
            turnCount++;
        }
        onTurnChanging?.Raise();

        return turnHour;
    }
    public int GetTurnHour()
    {
        return turnHour;
    }

    public int GetTurnCount()
    {
        return turnCount;
    }
    public bool IsPlayerTurn() => CurrentTurn == Turn.Player;
}