"""
GRU를 이용한 유닛 감정/상태 분석
- 최근 전투 이벤트 시퀀스를 입력으로 받아 유닛의 감정 상태를 예측
- 연속된 피해, 아군 손실 등의 시간적 패턴을 학습
"""

import torch
import torch.nn as nn
import matplotlib.pyplot as plt
import numpy as np

# 감정 상태 정의
# 이 값들은 유닛의 행동에 영향을 주는 modifier로 사용됩니다
EMOTIONS = {
    'FEAR': 0,        # 공포 (후퇴 확률 증가)
    'AGGRESSION': 1,  # 공격성 (공격 확률 증가)
    'CONFIDENCE': 2   # 자신감 (방어력 보너스)
}

class EmotionGRU(nn.Module):
    def __init__(self, input_size, hidden_size, output_size):
        super().__init__()
        self.gru = nn.GRU(
            input_size=input_size,
            hidden_size=hidden_size,
            batch_first=True
        )
        self.fc = nn.Sequential(
            nn.Linear(hidden_size, 16),
            nn.ReLU(),
            nn.Linear(16, output_size),
            nn.Sigmoid()  # 0~1 범위로 제한
        )

    def forward(self, x):
        # x: (batch, sequence, features)
        out, h = self.gru(x)
        last = out[:, -1, :]  # 마지막 타임스텝만 사용
        return self.fc(last)


def create_event_vector(event_data):
    """
    단일 전투 이벤트를 벡터로 변환
    
    이벤트 특성:
    - 현재 HP 비율 (0~1)
    - 받은 피해량 (정규화 0~1)
    - 아군 사망 여부 (0 or 1)
    - 적군 사망 여부 (0 or 1)
    - 적과의 거리 변화 (음수: 접근, 양수: 멀어짐)
    - 현재 주변 적군 수 (정규화)
    - 승리/패배 전투 여부 (1: 승리, -1: 패배, 0: 진행중)
    """
    return np.array([
        event_data['hp_ratio'],
        event_data['damage_taken'] / 100.0,  # 최대 100 피해 기준
        1.0 if event_data['ally_died'] else 0.0,
        1.0 if event_data['enemy_died'] else 0.0,
        event_data['distance_change'] / 50.0,  # -50~50 범위
        event_data['nearby_enemies'] / 10.0,
        event_data['battle_outcome']  # -1, 0, 1
    ], dtype=np.float32)


def generate_battle_sequence(sequence_length=5):
    """
    전투 이벤트 시퀀스 생성
    실제로는 게임에서 최근 N개의 이벤트를 기록해야 합니다.
    """
    # 시작 상태
    current_hp = 1.0
    events = []
    
    # 랜덤 시나리오 선택
    scenario_type = np.random.choice(['winning', 'losing', 'even'])
    
    for i in range(sequence_length):
        if scenario_type == 'winning':
            # 승리 중인 시퀀스
            damage = np.random.uniform(0, 15)
            ally_died = np.random.random() < 0.05
            enemy_died = np.random.random() < 0.3
            distance_change = np.random.uniform(10, 30)  # 적이 멀어짐
            nearby_enemies = max(0, 5 - i)
            outcome = 0 if i < sequence_length - 1 else 1
        elif scenario_type == 'losing':
            # 패배 중인 시퀀스
            damage = np.random.uniform(20, 50)
            ally_died = np.random.random() < 0.3
            enemy_died = np.random.random() < 0.05
            distance_change = np.random.uniform(-30, -10)  # 적이 접근
            nearby_enemies = min(10, 3 + i)
            outcome = 0 if i < sequence_length - 1 else -1
        else:
            # 팽팽한 전투
            damage = np.random.uniform(10, 30)
            ally_died = np.random.random() < 0.15
            enemy_died = np.random.random() < 0.15
            distance_change = np.random.uniform(-15, 15)
            nearby_enemies = 5
            outcome = 0
        
        current_hp = max(0.1, current_hp - damage / 100.0)
        
        event = create_event_vector({
            'hp_ratio': current_hp,
            'damage_taken': damage,
            'ally_died': ally_died,
            'enemy_died': enemy_died,
            'distance_change': distance_change,
            'nearby_enemies': nearby_enemies,
            'battle_outcome': outcome
        })
        events.append(event)
    
    return np.array(events), scenario_type


def create_emotion_label(scenario_type):
    """
    시나리오 타입에 따른 감정 레이블 생성
    """
    if scenario_type == 'winning':
        return np.array([0.1, 0.7, 0.9], dtype=np.float32)  # 낮은 공포, 높은 공격성, 높은 자신감
    elif scenario_type == 'losing':
        return np.array([0.9, 0.3, 0.2], dtype=np.float32)  # 높은 공포, 낮은 공격성, 낮은 자신감
    else:  # even
        return np.array([0.4, 0.5, 0.5], dtype=np.float32)  # 중간 값들


def generate_training_data(num_samples=2000, sequence_length=5):
    """
    훈련 데이터 생성
    """
    sequences = []
    emotions = []
    
    for _ in range(num_samples):
        sequence, scenario = generate_battle_sequence(sequence_length)
        emotion = create_emotion_label(scenario)
        
        sequences.append(sequence)
        emotions.append(emotion)
    
    return np.array(sequences), np.array(emotions)


# 학습 설정
print("=" * 60)
print("GRU 기반 유닛 감정 분석 모델 학습")
print("=" * 60)

sequence_length = 5  # 최근 5개 이벤트
input_size = 7  # 이벤트 벡터 크기
hidden_size = 32  # GRU hidden size
output_size = len(EMOTIONS)  # 감정 개수

# 데이터 생성
print("\n훈련 데이터 생성 중...")
sequences, emotions = generate_training_data(3000, sequence_length)

x = torch.FloatTensor(sequences)
y = torch.FloatTensor(emotions)

print(f"시퀀스 길이: {sequence_length}")
print(f"이벤트 특성 수: {input_size}")
print(f"GRU Hidden Size: {hidden_size}")
print(f"출력 감정 수: {output_size}")
print(f"감정 종류: {list(EMOTIONS.keys())}")
print(f"훈련 샘플 수: {len(x)}")
print(f"입력 shape: {x.shape} (batch, sequence, features)")
print(f"출력 shape: {y.shape} (batch, emotions)\n")

# 모델 생성
model = EmotionGRU(input_size, hidden_size, output_size)
loss_fn = nn.MSELoss()
optimizer = torch.optim.Adam(model.parameters(), lr=0.001)

# 학습
losses = []
print("학습 시작...\n")

for epoch in range(1000):
    pred = model(x)
    loss = loss_fn(pred, y)
    
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()
    
    losses.append(loss.item())
    if (epoch + 1) % 100 == 0:
        print(f"Epoch {epoch + 1}/1000, Loss: {loss.item():.6f}")

# 학습 결과 시각화
plt.figure(figsize=(12, 5))

plt.subplot(1, 2, 1)
plt.plot(losses)
plt.xlabel('Epoch')
plt.ylabel('Loss (MSE)')
plt.title('Training Loss Over Time')
plt.grid(True)

# 예측 vs 실제 비교
with torch.no_grad():
    final_pred = model(x[:100]).numpy()
    final_true = y[:100].numpy()

plt.subplot(1, 2, 2)
x_pos = np.arange(len(EMOTIONS))
pred_mean = final_pred.mean(axis=0)
true_mean = final_true.mean(axis=0)

width = 0.35
plt.bar(x_pos - width/2, true_mean, width, label='Ground Truth', alpha=0.8)
plt.bar(x_pos + width/2, pred_mean, width, label='Predicted', alpha=0.8)
plt.xlabel('Emotions')
plt.ylabel('Average Value')
plt.title('Average Emotion Values (First 100 samples)')
plt.xticks(x_pos, EMOTIONS.keys(), rotation=45)
plt.legend()
plt.grid(True, axis='y')

plt.tight_layout()
plt.savefig('gru_training_results.png')
plt.show()
print("\n학습 완료! gru_training_results.png 파일이 생성되었습니다.")


# 테스트 예제
print("\n" + "=" * 60)
print("테스트 시나리오")
print("=" * 60)

model.eval()

# 테스트 시나리오 1: 연속 피격 (패배 중)
print("\n📉 시나리오 1: 연속으로 큰 피해를 받는 상황")
test_seq_1 = []
hp = 1.0
for i in range(5):
    damage = 30 + i * 5  # 점점 증가하는 피해
    hp = max(0.2, hp - damage / 100.0)
    event = create_event_vector({
        'hp_ratio': hp,
        'damage_taken': damage,
        'ally_died': i >= 2,
        'enemy_died': False,
        'distance_change': -20,  # 적이 계속 접근
        'nearby_enemies': 6 + i,
        'battle_outcome': 0 if i < 4 else -1
    })
    test_seq_1.append(event)
    print(f"  이벤트 {i+1}: HP {hp:.2f}, 피해 {damage}, 적 접근 중, 주변 적 {6+i}명")

test_tensor_1 = torch.FloatTensor(np.array(test_seq_1)).unsqueeze(0)
with torch.no_grad():
    emotion_1 = model(test_tensor_1)[0].numpy()
print(f"\n  예측된 감정:")
print(f"    공포(FEAR): {emotion_1[EMOTIONS['FEAR']]:.3f}")
print(f"    공격성(AGGRESSION): {emotion_1[EMOTIONS['AGGRESSION']]:.3f}")
print(f"    자신감(CONFIDENCE): {emotion_1[EMOTIONS['CONFIDENCE']]:.3f}")


# 테스트 시나리오 2: 연속 승리 (우세)
print("\n📈 시나리오 2: 적을 계속 격파하는 상황")
test_seq_2 = []
hp = 0.95
for i in range(5):
    damage = 5 + np.random.uniform(-3, 3)  # 작은 피해
    hp = max(0.7, hp - damage / 100.0)
    event = create_event_vector({
        'hp_ratio': hp,
        'damage_taken': damage,
        'ally_died': False,
        'enemy_died': True,  # 매번 적 격파
        'distance_change': 15,  # 적들이 후퇴
        'nearby_enemies': max(1, 5 - i),
        'battle_outcome': 0 if i < 4 else 1
    })
    test_seq_2.append(event)
    print(f"  이벤트 {i+1}: HP {hp:.2f}, 적 격파!, 적 후퇴, 주변 적 {max(1, 5-i)}명")

test_tensor_2 = torch.FloatTensor(np.array(test_seq_2)).unsqueeze(0)
with torch.no_grad():
    emotion_2 = model(test_tensor_2)[0].numpy()
print(f"\n  예측된 감정:")
print(f"    공포(FEAR): {emotion_2[EMOTIONS['FEAR']]:.3f}")
print(f"    공격성(AGGRESSION): {emotion_2[EMOTIONS['AGGRESSION']]:.3f}")
print(f"    자신감(CONFIDENCE): {emotion_2[EMOTIONS['CONFIDENCE']]:.3f}")


# 테스트 시나리오 3: 팽팽한 전투
print("\n⚖️  시나리오 3: 팽팽한 교전 상황")
test_seq_3 = []
hp = 0.8
for i in range(5):
    damage = 15 + np.random.uniform(-5, 5)
    hp = max(0.5, hp - damage / 100.0)
    event = create_event_vector({
        'hp_ratio': hp,
        'damage_taken': damage,
        'ally_died': i == 2,
        'enemy_died': i == 3,
        'distance_change': np.random.uniform(-10, 10),
        'nearby_enemies': 5,
        'battle_outcome': 0
    })
    test_seq_3.append(event)
    died_msg = "아군 사망!" if i == 2 else ("적 격파!" if i == 3 else "교전 중")
    print(f"  이벤트 {i+1}: HP {hp:.2f}, 피해 {damage:.1f}, {died_msg}")

test_tensor_3 = torch.FloatTensor(np.array(test_seq_3)).unsqueeze(0)
with torch.no_grad():
    emotion_3 = model(test_tensor_3)[0].numpy()
print(f"\n  예측된 감정:")
print(f"    공포(FEAR): {emotion_3[EMOTIONS['FEAR']]:.3f}")
print(f"    공격성(AGGRESSION): {emotion_3[EMOTIONS['AGGRESSION']]:.3f}")
print(f"    자신감(CONFIDENCE): {emotion_3[EMOTIONS['CONFIDENCE']]:.3f}")


# ONNX 변환
print("\n" + "=" * 60)
print("ONNX 모델 변환")
print("=" * 60)

dummy_input = torch.randn(1, sequence_length, input_size)

torch.onnx.export(
    model,
    dummy_input,
    "emotion_gru.onnx",
    input_names=["event_sequence"],
    output_names=["emotion_state"],
    opset_version=13,
    do_constant_folding=True,
    dynamic_axes={
        'event_sequence': {0: 'batch_size'},
        'emotion_state': {0: 'batch_size'}
    }
)
print("\n✅ ONNX 모델이 emotion_gru.onnx 파일로 저장되었습니다.")


# Unity Sentis 사용 예제
print("\n" + "=" * 60)
print("Unity Sentis 연동 가이드")
print("=" * 60)

unity_code = """
// ==========================================
// Unity C# 코드 예제
// ==========================================

using Unity.Sentis;
using UnityEngine;
using System.Collections.Generic;

public class UnitEmotionController : MonoBehaviour
{
    [SerializeField] private ModelAsset emotionModelAsset;
    
    private Model model;
    private IWorker worker;
    
    // 감정 상태 (GRU 출력)
    public struct EmotionState
    {
        public float Fear;        // 0~1
        public float Aggression;  // 0~1
        public float Confidence;  // 0~1
    }
    
    // 전투 이벤트 데이터
    public struct BattleEvent
    {
        public float hpRatio;
        public float damageTaken;
        public bool allyDied;
        public bool enemyDied;
        public float distanceChange;
        public int nearbyEnemies;
        public float battleOutcome;  // -1: 패배, 0: 진행중, 1: 승리
    }
    
    private Queue<BattleEvent> eventHistory = new Queue<BattleEvent>();
    private const int SEQUENCE_LENGTH = 5;
    private const int EVENT_FEATURES = 7;
    
    void Awake()
    {
        model = ModelLoader.Load(emotionModelAsset);
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
    }
    
    // 전투 이벤트 기록
    public void RecordEvent(BattleEvent battleEvent)
    {
        eventHistory.Enqueue(battleEvent);
        
        // 최대 시퀀스 길이 유지
        while (eventHistory.Count > SEQUENCE_LENGTH)
        {
            eventHistory.Dequeue();
        }
    }
    
    // 감정 상태 예측
    public EmotionState PredictEmotion()
    {
        if (eventHistory.Count < SEQUENCE_LENGTH)
        {
            // 이벤트가 충분하지 않으면 중립 상태 반환
            return new EmotionState 
            { 
                Fear = 0.5f, 
                Aggression = 0.5f, 
                Confidence = 0.5f 
            };
        }
        
        // 1. 시퀀스 데이터를 1D 배열로 변환
        float[] sequenceData = new float[SEQUENCE_LENGTH * EVENT_FEATURES];
        int index = 0;
        
        foreach (var evt in eventHistory)
        {
            sequenceData[index++] = evt.hpRatio;
            sequenceData[index++] = evt.damageTaken / 100f;
            sequenceData[index++] = evt.allyDied ? 1f : 0f;
            sequenceData[index++] = evt.enemyDied ? 1f : 0f;
            sequenceData[index++] = evt.distanceChange / 50f;
            sequenceData[index++] = evt.nearbyEnemies / 10f;
            sequenceData[index++] = evt.battleOutcome;
        }
        
        // 2. 텐서 생성 (1, 5, 7) - batch=1, sequence=5, features=7
        TensorFloat input = new TensorFloat(
            new TensorShape(1, SEQUENCE_LENGTH, EVENT_FEATURES),
            sequenceData
        );
        
        // 3. 모델 실행
        worker.Execute(input);
        TensorFloat output = worker.PeekOutput() as TensorFloat;
        
        // 4. 결과 추출
        EmotionState emotion = new EmotionState
        {
            Fear = output[0],
            Aggression = output[1],
            Confidence = output[2]
        };
        
        // 텐서 정리
        input.Dispose();
        output.Dispose();
        
        return emotion;
    }
    
    // 감정 상태에 따른 행동 modifier 적용 예제
    public float GetActionModifier(string actionType, EmotionState emotion)
    {
        switch (actionType)
        {
            case "Attack":
                // 공격성이 높고 자신감이 있으면 공격 보너스
                return 1f + (emotion.Aggression * emotion.Confidence * 0.5f);
                
            case "Retreat":
                // 공포가 높으면 후퇴 확률 증가
                return 1f + (emotion.Fear * 0.8f);
                
            case "Defend":
                // 자신감이 있으면 방어 보너스
                return 1f + (emotion.Confidence * 0.3f);
                
            default:
                return 1f;
        }
    }
    
    void OnDestroy()
    {
        worker?.Dispose();
    }
}

// ==========================================
// 사용 예제
// ==========================================

public class UnitCombatManager : MonoBehaviour
{
    private UnitEmotionController emotionController;
    
    void Start()
    {
        emotionController = GetComponent<UnitEmotionController>();
    }
    
    // 피격 시 호출
    void OnDamaged(float damage)
    {
        var evt = new UnitEmotionController.BattleEvent
        {
            hpRatio = GetComponent<Unit>().CurrentHP / GetComponent<Unit>().MaxHP,
            damageTaken = damage,
            allyDied = false,
            enemyDied = false,
            distanceChange = CalculateDistanceChange(),
            nearbyEnemies = CountNearbyEnemies(),
            battleOutcome = 0
        };
        
        emotionController.RecordEvent(evt);
        
        // 감정 상태 업데이트
        var emotion = emotionController.PredictEmotion();
        
        // 감정에 따라 행동 조정
        if (emotion.Fear > 0.7f)
        {
            Debug.Log("유닛이 공포를 느낍니다! 후퇴 고려");
            // TriggerRetreat();
        }
        else if (emotion.Aggression > 0.7f && emotion.Confidence > 0.6f)
        {
            Debug.Log("유닛이 공격적입니다! 적극적 공격");
            // TriggerAggressiveAttack();
        }
    }
}
"""

print(unity_code)
print("\n" + "=" * 60)
