// using UnityEngine;
// using Unity.MLAgents;
// using Unity.MLAgents.Sensors;
// using Unity.MLAgents.Actuators;

// namespace Core
// {
//     public class TacticalCombatAgent: Agent
//     {
//         [Header("Stats")]
//         public float hp = 1.0f;
//         public int enemyCount = 0;
        
//         [Header("Environment")]
//         public Transform safetyZone; // 도망칠 안전 구역
//         public Transform enemyGroup; // 적들이 모여있는 부모 오브젝트

//         private Rigidbody rb;

//         public override void Initialize()
//         {
//             rb = GetComponent<Rigidbody>();
//         }

//         public override void OnEpisodeBegin()
//         {
//             hp = 1.0f;
//             // 적들의 숫자나 위치를 매번 랜덤하게 설정하는 로직 추가 권장
//             UpdateEnemyCount(); 
//         }

//         // 1. 상황 관측 (핵심!)
//         public override void CollectObservations(VectorSensor sensor)
//         {
//             // 내 체력 정보 (0~1)
//             sensor.AddObservation(hp);
            
//             // 주변 적의 숫자 (정규화하여 입력: 예시 최대 10마리)
//             sensor.AddObservation(enemyCount / 10f);

//             // 가장 가까운 안전 구역까지의 방향/거리
//             Vector3 dirToSafety = (safetyZone.position - transform.position).normalized;
//             sensor.AddObservation(dirToSafety.x);
//             sensor.AddObservation(dirToSafety.z);
//             sensor.AddObservation(Vector3.Distance(transform.position, safetyZone.position));
//         }

//         // 2. 행동 결정 (Discrete Actions 사용)
//         public override void OnActionReceived(ActionBuffers actions)
//         {
//             // AI가 선택한 번호 (0: 공격모드, 1: 도망모드, 2: 방어모드)
//             int decision = actions.DiscreteActions[0];

//             // AI 결정 로그 출력
//             Debug.Log($"[AI] 선택된 행동: {GetActionName(decision)}");

//             switch (decision)
//             {
//                 case 0: // [공격]
//                     ExecuteAttack();
//                     break;
//                 case 1: // [도망]
//                     ExecuteEvade();
//                     break;
//                 case 2: // [방어]
//                     ExecuteDefend();
//                     break;
//             }

//             // --- 전략적 보상 설계 (중요!) ---

//             // 상황 1: 적이 많은데 도망을 선택했다면? -> 보상
//             if (enemyCount >= 3 && decision == 1) 
//             {
//                 AddReward(0.01f); 
//             }

//             // 상황 2: 체력이 적은데 방어를 선택했다면? -> 보상
//             if (hp < 0.3f && decision == 2)
//             {
//                 AddReward(0.01f);
//             }

//             // 상황 3: 체력이 적은데 무모하게 공격을 하다가 피해를 입으면? -> 큰 벌점
//             if (hp < 0.2f && decision == 0)
//             {
//                 AddReward(-0.05f);
//             }

//             // 생존 시 소량의 기본 보상 (살아남는 것이 목표임을 인지)
//             AddReward(0.001f);
//         }
//         private string GetActionName(int action)
//         {
//             return action switch
//             {
//                 0 => "공격",
//                 1 => "도망",
//                 2 => "방어",
//                 _ => "알 수 없음"
//             };
//         }
//         private void UpdateEnemyCount()
//         {
//             // 특정 범위 내의 적 숫자를 체크하는 로직
//             // Collider[] hitColliders = Physics.OverlapSphere(transform.position, 10f); ...
//         }

//         // 각 행동별 물리 로직 (단순 예시)
//         void ExecuteAttack() { /* 적 방향으로 돌진 */ }
//         void ExecuteEvade() { /* 안전 구역 방향으로 힘 가하기 */ }
//         void ExecuteDefend() { /* 속도 줄이고 방어막 활성화 */ }
//     }
// }