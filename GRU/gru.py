# 1. GRU를 써야 하는 문제 정의 (중요)

# GRU는 **“시간 흐름”**이 핵심입니다.

# GRU가 적합한 질문

# 최근에 계속 밀리고 있는가?

# 연속 피격이 있었는가?

# 전투 흐름이 악화되고 있는가?

# 공포가 누적되고 있는가?

# ❌ “지금 공격할까?”
# ✔️ “요즘 전투가 불리한가?”

# 2. 입력 시퀀스 설계 (가장 중요)

# ❗ GRU 성능의 90%는 시퀀스 설계에서 결정됩니다

# 2.1 프레임 단위 ❌ / 이벤트 단위 ✔️
# ❌ 잘못된 예
# 매 프레임 HP, 위치 입력

# ✔️ 올바른 예
# 피격 발생
# 큰 피해 발생
# 아군 사망
# 적 접근

# 2.2 시퀀스 벡터 예시
# 단일 타임스텝 입력
# [hp_ratio, damage_taken, ally_lost, enemy_near]

# 시퀀스 길이
# T = 3 ~ 8 (권장)


# ➡ Sentis + GRU는 짧을수록 안정

# 2.3 시퀀스 텐서 형태
# (batch, sequence, feature)
# (1, 5, 6)

# 3. GRU 모델 구조 설계 (Sentis 친화)
# 권장 구조 (최소)
# Input (T, F)
#  → GRU (hidden=16)
#  → Dense
#  → Output (감정 벡터)

# 출력 예
# [fear, aggression, confidence]

import torch
import torch.nn as nn

class EmotionGRU(nn.Module):
    def __init__(self, input_size, hidden_size, output_size):
        super().__init__()
        self.gru = nn.GRU(
            input_size=input_size,
            hidden_size=hidden_size,
            batch_first=True
        )
        self.fc = nn.Linear(hidden_size, output_size)

    def forward(self, x):
        out, h = self.gru(x)
        last = out[:, -1, :]     # 마지막 타임스텝
        return self.fc(last)

# Example usage:
seq_len = 5  # 시퀀스 길이
input_size = 6  # [hp_ratio, damage_taken, ally_lost, enemy_near, ...]
output_size = 3  # [fear, aggression, confidence]

x = torch.rand(500, seq_len, input_size)
y = torch.rand(500, output_size)

model = EmotionGRU(input_size, 16, output_size)
loss_fn = nn.MSELoss()
optimizer = torch.optim.Adam(model.parameters(), lr=0.001)

for _ in range(1000):
    pred = model(x)
    loss = loss_fn(pred, y)
    optimizer.zero_grad()
    loss.backward()
    optimizer.step()


dummy = torch.randn(1, seq_len, input_size)
torch.onnx.export(
    model,
    dummy,
    "emotion_gru.onnx",
    input_names=["sequence"],
    output_names=["emotion"],
    opset_version=13,
    do_constant_folding=True
)


# 6. Unity Sentis에서 GRU 실행
# 6.1 모델 로드
# Model model;
# IWorker worker;

# void Awake()
# {
#     model = ModelLoader.Load(gruModelAsset);
#     worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, model);
# }

# 6.2 시퀀스 입력 텐서 생성
# TensorFloat input = new TensorFloat(
#     new TensorShape(1, sequenceLength, featureSize),
#     sequenceDataArray
# );

# worker.Execute(input);
# TensorFloat output = worker.PeekOutput() as TensorFloat;

# 6.3 출력 사용 예 (감정 반영)
# float fear = output[0];
# float aggression = output[1];

# unitState.Emotion.Fear = fear;
# unitState.Emotion.Aggression = aggression;


# ➡ 직접 행동 결정 ❌
# ➡ 가중치/Modifier로 사용 ✔️

# 7. UnitState에 GRU 히스토리 관리 (중요)
# Queue<float[]> history = new();

# void PushEvent(float[] data)
# {
#     history.Enqueue(data);
#     if (history.Count > sequenceLength)
#         history.Dequeue();
# }


# 히스토리는 UnitState가 관리

# GRU는 읽기만 함