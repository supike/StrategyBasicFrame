using System.Collections.Generic;
using UnityEngine;

namespace Core
{
    public enum GameState
    {
        LiveTurn,
        PreBattle,
        PlayerTurn,
        EnemyTurn,
        BattleEnd
    }

    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager Instance { get; private set; }
        
        private IGameState currentState;
        public GameState CurrentGameState { get; private set; }
        
        private Dictionary<GameState, IGameState> states;
        
        void Awake()
        {
            Instance = this;
            InitializeStates();
        }
        
        void InitializeStates()
        {
            states = new Dictionary<GameState, IGameState>
            {
                { GameState.LiveTurn, new LiveTurnState() },
                { GameState.PreBattle, new PreBattleState() },
                { GameState.PlayerTurn, new PlayerTurnState() },
                { GameState.EnemyTurn, new EnemyTurnState() },
                { GameState.BattleEnd, new BattleEndState() }
            };
        }
        
        public void ChangeState(GameState newState)
        {
            currentState?.OnExit();
            CurrentGameState = newState;
            currentState = states[newState];
            currentState?.OnEnter();
        }
        
        void Update()
        {
            currentState?.Update();
        }
    }

    public interface IGameState
    {
        void OnEnter();
        void Update();
        void OnExit();
    }

    public class LiveTurnState : IGameState
    {
        public void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void OnExit()
        {
            throw new System.NotImplementedException();
        }
    }

    public class PreBattleState : IGameState
    {
        public void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void OnExit()
        {
            throw new System.NotImplementedException();
        } 
    }

    public class PlayerTurnState : IGameState
    {
        public void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void OnExit()
        {
            throw new System.NotImplementedException();
        } 
    }

    public class EnemyTurnState : IGameState
    {
        public void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void OnExit()
        {
            throw new System.NotImplementedException();
        } 
    }

    public class BattleEndState : IGameState
    {
        public void OnEnter()
        {
            throw new System.NotImplementedException();
        }

        public void Update()
        {
            throw new System.NotImplementedException();
        }

        public void OnExit()
        {
            throw new System.NotImplementedException();
        } 
    }
}