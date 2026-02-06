import torch
import torch.nn as nn
import matplotlib.pyplot as plt
import numpy as np

# 행동 정의
ACTIONS = {
    'ATTACK': 0,
    'RETREAT': 1,
    'WAIT': 2,
    'DEFEND': 3,
    'MOVE_FORWARD': 4
}

class UnitMLP(nn.Module):
    def __init__(self, input_size, output_size):
        super().__init__()
        self.net = nn.Sequential(
            nn.Linear(input_size, 64),
            nn.ReLU(),
            nn.Dropout(0.2),
            nn.Linear(64, 64),
            nn.ReLU(),
            nn.Dropout(0.2),
            nn.Linear(64, 32),
            nn.ReLU(),
            nn.Linear(32, output_size)
        )

    def forward(self, x):
        return self.net(x)

def create_state_vector(unit_data):
    """
    유닛과 주변 상황을 벡터로 변환
    
    입력 특성 예시:
    - 유닛 HP 비율 (0~1)
    - 유닛 공격력 (정규화)
    - 유닛 방어력 (정규화)
    - 가장 가까운 적까지의 거리 (정규화)
    - 주변 아군 수
    - 주변 적군 수
    - 아군 평균 HP 비율
    - 적군 평균 HP 비율
    - 현재 위치 (x, y 정규화)
    - 목표 위치까지의 거리 (정규화)
    """
    return np.array([
        unit_data['hp_ratio'],
        unit_data['attack'] / 100.0,
        unit_data['defense'] / 100.0,
        unit_data['distance_to_nearest_enemy'] / 100.0,
        unit_data['nearby_allies'] / 10.0,
        unit_data['nearby_enemies'] / 10.0,
        unit_data['allies_avg_hp'],
        unit_data['enemies_avg_hp'],
        unit_data['position_x'] / 100.0,
        unit_data['position_y'] / 100.0,
        unit_data['distance_to_objective'] / 100.0
    ], dtype=np.float32)


# 예제 훈련 데이터 생성 (실제로는 게임 플레이 데이터 수집 필요)
def generate_sample_data(num_samples=1000):
    """
    샘플 훈련 데이터 생성
    실제로는 게임에서 전문가의 플레이를 기록하거나
    강화학습으로 데이터를 수집해야 합니다.
    """
    states = []
    actions = []
    
    for _ in range(num_samples):
        # 랜덤 상태 생성
        hp_ratio = np.random.uniform(0.1, 1.0)
        attack = np.random.uniform(30, 100)
        defense = np.random.uniform(20, 80)
        distance_to_enemy = np.random.uniform(5, 100)
        nearby_allies = np.random.randint(0, 10)
        nearby_enemies = np.random.randint(0, 10)
        allies_avg_hp = np.random.uniform(0.3, 1.0)
        enemies_avg_hp = np.random.uniform(0.3, 1.0)
        pos_x = np.random.uniform(0, 100)
        pos_y = np.random.uniform(0, 100)
        dist_objective = np.random.uniform(10, 100)
        
        state = create_state_vector({
            'hp_ratio': hp_ratio,
            'attack': attack,
            'defense': defense,
            'distance_to_nearest_enemy': distance_to_enemy,
            'nearby_allies': nearby_allies,
            'nearby_enemies': nearby_enemies,
            'allies_avg_hp': allies_avg_hp,
            'enemies_avg_hp': enemies_avg_hp,
            'position_x': pos_x,
            'position_y': pos_y,
            'distance_to_objective': dist_objective
        })
        
        # 간단한 규칙 기반 레이블링 (실제로는 전문가 데이터 필요)
        if hp_ratio < 0.3 or (nearby_enemies > nearby_allies + 2):
            action = ACTIONS['RETREAT']  # 후퇴
        elif distance_to_enemy < 15 and hp_ratio > 0.6:
            action = ACTIONS['ATTACK']  # 공격
        elif nearby_enemies > 0 and hp_ratio > 0.4:
            action = ACTIONS['DEFEND']  # 방어
        elif distance_to_enemy > 30:
            action = ACTIONS['MOVE_FORWARD']  # 전진
        else:
            action = ACTIONS['WAIT']  # 대기
        
        states.append(state)
        actions.append(action)
    
    return np.array(states), np.array(actions)


# 학습 설정
input_size = 11  # create_state_vector의 특성 개수
output_size = len(ACTIONS)  # 행동 개수

# 데이터 생성
print("훈련 데이터 생성 중...")
states, actions = generate_sample_data(5000)
x = torch.FloatTensor(states)
y = torch.LongTensor(actions)

print(f"입력 크기: {input_size}")
print(f"출력 크기: {output_size}")
print(f"행동 종류: {list(ACTIONS.keys())}")
print(f"훈련 샘플 수: {len(x)}\n")

model = UnitMLP(input_size, output_size)
loss_fn = nn.CrossEntropyLoss()  # 분류 문제이므로 CrossEntropyLoss 사용
optimizer = torch.optim.Adam(model.parameters(), lr=0.001)

losses = []
accuracies = []

for epoch in range(1000):
    pred = model(x)
    loss = loss_fn(pred, y)
    
    # 정확도 계산
    with torch.no_grad():
        predicted_actions = torch.argmax(pred, dim=1)
        accuracy = (predicted_actions == y).float().mean().item()
        accuracies.append(accuracy)
    
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    
    losses.append(loss.item())
    if (epoch + 1) % 100 == 0:
        print(f"Epoch {epoch + 1}/1000, Loss: {loss.item():.6f}, Accuracy: {accuracy:.4f}")

# Plot training loss and accuracy
fig, (ax1, ax2) = plt.subplots(1, 2, figsize=(15, 5))

ax1.plot(losses)
ax1.set_xlabel('Epoch')
ax1.set_ylabel('Loss')
ax1.set_title('Training Loss Over Time')
ax1.grid(True)

ax2.plot(accuracies)
ax2.set_xlabel('Epoch')
ax2.set_ylabel('Accuracy')
ax2.set_title('Training Accuracy Over Time')
ax2.grid(True)

plt.tight_layout()
plt.savefig('training_results.png')
plt.show()
print("\n학습 완료! training_results.png 파일이 생성되었습니다.")

# 테스트 예제
print("\n=== 테스트 예제 ===")
test_cases = [
    {
        'name': '낮은 체력, 적이 많음',
        'data': {'hp_ratio': 0.2, 'attack': 50, 'defense': 40, 
                'distance_to_nearest_enemy': 10, 'nearby_allies': 2, 'nearby_enemies': 5,
                'allies_avg_hp': 0.6, 'enemies_avg_hp': 0.8, 
                'position_x': 50, 'position_y': 50, 'distance_to_objective': 30}
    },
    {
        'name': '높은 체력, 적이 가까움',
        'data': {'hp_ratio': 0.9, 'attack': 80, 'defense': 60, 
                'distance_to_nearest_enemy': 12, 'nearby_allies': 4, 'nearby_enemies': 2,
                'allies_avg_hp': 0.7, 'enemies_avg_hp': 0.5, 
                'position_x': 50, 'position_y': 50, 'distance_to_objective': 20}
    },
    {
        'name': '중간 체력, 적이 멀리',
        'data': {'hp_ratio': 0.6, 'attack': 60, 'defense': 50, 
                'distance_to_nearest_enemy': 50, 'nearby_allies': 3, 'nearby_enemies': 1,
                'allies_avg_hp': 0.6, 'enemies_avg_hp': 0.6, 
                'position_x': 50, 'position_y': 50, 'distance_to_objective': 40}
    }
]

model.eval()
action_names = {v: k for k, v in ACTIONS.items()}

with torch.no_grad():
    for test in test_cases:
        state = create_state_vector(test['data'])
        state_tensor = torch.FloatTensor(state).unsqueeze(0)
        output = model(state_tensor)
        probabilities = torch.softmax(output, dim=1)[0]
        predicted_action = torch.argmax(output, dim=1).item()
        
        print(f"\n시나리오: {test['name']}")
        print(f"  예측된 행동: {action_names[predicted_action]}")
        print(f"  행동 확률:")
        for action_name, action_id in ACTIONS.items():
            print(f"    {action_name}: {probabilities[action_id].item():.3f}")



dummy_input = torch.randn(1, input_size)

torch.onnx.export(
    model,
    dummy_input,
    "unit_action_mlp.onnx",
    input_names=["state"],
    output_names=["action_logits"],
    opset_version=13,
    do_constant_folding=True,
    dynamic_axes={'state': {0: 'batch_size'}, 'action_logits': {0: 'batch_size'}}
)
print("\nONNX 모델이 unit_action_mlp.onnx 파일로 저장되었습니다.")


# ==========================================
# Unity Sentis에서 사용하는 방법
# ==========================================
"""
C# 코드 예제:

using Unity.Sentis;
using UnityEngine;

public class UnitAIController : MonoBehaviour
{
    [SerializeField] ModelAsset modelAsset;
    
    private Model model;
    private IWorker worker;
    
    // 행동 정의 (Python의 ACTIONS와 동일한 순서)
    public enum UnitAction
    {
        ATTACK = 0,
        RETREAT = 1,
        WAIT = 2,
        DEFEND = 3,
        MOVE_FORWARD = 4
    }
    
    void Awake()
    {
        model = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
    }
    
    public UnitAction DecideAction(UnitState state)
    {
        // 1. 상태 벡터 생성 (Python의 create_state_vector와 동일)
        float[] stateVector = new float[11]
        {
            state.hpRatio,                          // HP 비율
            state.attack / 100f,                     // 공격력 (정규화)
            state.defense / 100f,                    // 방어력 (정규화)
            state.distanceToNearestEnemy / 100f,    // 가장 가까운 적까지의 거리
            state.nearbyAllies / 10f,               // 주변 아군 수
            state.nearbyEnemies / 10f,              // 주변 적군 수
            state.alliesAvgHp,                      // 아군 평균 HP
            state.enemiesAvgHp,                     // 적군 평균 HP
            state.positionX / 100f,                 // 현재 x 위치
            state.positionY / 100f,                 // 현재 y 위치
            state.distanceToObjective / 100f        // 목표까지의 거리
        };
        
        // 2. 입력 텐서 생성
        TensorFloat input = new TensorFloat(
            new TensorShape(1, 11),
            stateVector
        );
        
        // 3. 모델 실행
        worker.Execute(input);
        TensorFloat output = worker.PeekOutput() as TensorFloat;
        
        // 4. 최고 점수의 행동 선택
        int bestAction = 0;
        float bestScore = float.MinValue;
        
        for (int i = 0; i < 5; i++)  // 5가지 행동
        {
            float score = output[i];
            if (score > bestScore)
            {
                bestScore = score;
                bestAction = i;
            }
        }
        
        // 텐서 정리
        input.Dispose();
        output.Dispose();
        
        return (UnitAction)bestAction;
    }
    
    void OnDestroy()
    {
        worker?.Dispose();
    }
}

// 유닛 상태 구조체
public struct UnitState
{
    public float hpRatio;
    public float attack;
    public float defense;
    public float distanceToNearestEnemy;
    public int nearbyAllies;
    public int nearbyEnemies;
    public float alliesAvgHp;
    public float enemiesAvgHp;
    public float positionX;
    public float positionY;
    public float distanceToObjective;
}
"""