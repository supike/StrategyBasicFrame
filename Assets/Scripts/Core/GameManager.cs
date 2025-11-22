using UnityEngine;

namespace Core
{
    public class GameManager : MonoBehaviour
    {
        private float productionTimer;
        private float turnTimer;
        private float productionTime = 1f; // 5초당 1자원
        
        public static GameManager Instance { get; private set; }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            productionTimer = 0;
        }

        // Update is called once per frame
        void Update()
        {
            // 실행을 하는 로직.
            if (TurnManager.Instance.CurrentTurn == TurnManager.Turn.Action)
            {
                // Handle resource production during the Action turn
                LiveTimeProduceBaseResource();
                LiveTimeTurnTick();
            }
        }
        void LiveTimeTurnTick()
        {
            int nPreTurnCount = TurnManager.Instance.GetTurnCount();
            // Handle resource production over turn time
            turnTimer += Time.deltaTime;
            
            if (turnTimer >= 1f)      //1초에 한시간씩 지남
            {
                turnTimer = 0f;
                // Halfway through the turn
                TurnManager.Instance.AddTurnHour();
            }

            /*if (nPreTurnCount != 0 && nPreTurnCount != TurnManager.Instance.GetTurnHour())
            {
                TurnManager.Instance.EndTurn();
            }*/
            
        }
        void LiveTimeProduceBaseResource()
        {
            // Handle resource production over time
            productionTimer += Time.deltaTime;
            
            if (productionTimer >= productionTime)
            {
                productionTimer = 0;
                ProduceBaseResource();
            }
        }
        void ProduceBaseResource()
        {
            for (int i = 0; i < (int)ResourceType.Count; i++)
            {
                ResourceManager.Instance.AddResource(
                    (ResourceType)i,
                    1
                );
            }
        }

    }

}