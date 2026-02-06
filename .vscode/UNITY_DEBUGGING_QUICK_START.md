# Unity Editor 디버깅 빠른 시작 가이드

## 현재 설정 상태

✅ **launch.json 설정 완료**
- CoreCLR 디버거로 Unity Editor에 연결
- 자동 프로세스 매칭 및 수동 선택 옵션 제공

## 디버깅 시작 방법

### 방법 1: 자동 연결 (가장 간단)

1. **Unity Editor 실행 확인**
   - Unity Editor가 프로젝트를 열고 있는지 확인

2. **Cursor에서 디버깅 시작**
   - `F5` 키 누르기
   - 또는 디버그 패널에서 **"Attach to Unity Editor"** 선택
   - Unity 프로세스에 자동으로 연결됩니다

3. **Breakpoint 설정**
   - C# 파일에서 라인 번호 왼쪽 클릭하여 Breakpoint 설정
   - 또는 `F9` 키로 Breakpoint 토글

4. **Unity Editor에서 Play 모드 시작**
   - Unity Editor에서 ▶️ 버튼 클릭
   - Breakpoint에서 코드가 멈춥니다

### 방법 2: 프로세스 수동 선택

자동 연결이 안 되거나 여러 Unity 프로세스가 있는 경우:

1. Cursor에서 **"Attach to Unity Editor (Select Process)"** 선택
2. 프로세스 목록에서 **"Unity"** 선택
3. Breakpoint 설정 후 Play 모드 시작

## 디버깅 기능 사용법

- **Breakpoint 설정/해제**: 라인 번호 왼쪽 클릭 또는 `F9`
- **Step Over (F10)**: 다음 줄로 이동
- **Step Into (F11)**: 함수 내부로 들어가기
- **Step Out (Shift+F11)**: 현재 함수에서 나가기
- **Continue (F5)**: 다음 breakpoint까지 실행
- **Variables 패널**: 현재 스코프의 변수 값 확인
- **Watch 패널**: 특정 변수나 표현식 모니터링
- **Call Stack**: 함수 호출 스택 확인

## 문제 해결

### Unity Editor에 연결이 안 되는 경우

1. **Unity Editor 실행 확인**
   ```bash
   ps aux | grep Unity | grep -v grep
   ```
   Unity Editor가 실행 중이어야 합니다.

2. **C# 확장 설치 확인**
   - Cursor에서 `Cmd+Shift+X`로 확장 패널 열기
   - "C#" (Microsoft) 확장이 설치되어 있는지 확인
   - 설치되어 있지 않다면 설치

3. **Unity Editor 재시작**
   - Unity Editor 종료 후 다시 열기
   - Cursor 재시작

4. **프로세스 수동 선택**
   - "Attach to Unity Editor (Select Process)" 사용
   - 프로세스 목록에서 정확한 Unity 프로세스 선택

### Breakpoint가 작동하지 않는 경우

1. **코드가 최신인지 확인**
   - Unity Editor에서 스크립트 컴파일 완료 대기
   - Console 창에 오류가 없는지 확인

2. **Breakpoint 위치 확인**
   - 실행되는 코드 경로에 Breakpoint가 있는지 확인
   - 조건부 코드는 해당 조건이 만족될 때만 멈춤

3. **Debug 모드 확인**
   - Unity Editor가 Debug 모드로 실행 중인지 확인
   - Release 모드에서는 일부 디버깅 기능이 제한될 수 있음

### IntelliSense가 작동하지 않는 경우

1. **OmniSharp 재시작**
   - Command Palette (`Cmd+Shift+P`)
   - "OmniSharp: Restart OmniSharp" 실행

2. **프로젝트 파일 확인**
   - `.sln` 파일이 있는지 확인
   - Unity Editor에서 프로젝트 파일 재생성:
     - `Assets > Reimport All` (시간이 걸릴 수 있음)

## Unity Editor 설정 확인

Unity Editor에서 다음 설정을 확인하세요:

1. **External Script Editor 설정**
   - `Edit > Preferences > External Tools`
   - `External Script Editor`를 Cursor로 설정:
     - macOS: `/Applications/Cursor.app`

2. **Script Debugging 활성화** (Unity 2021.2+)
   - 기본적으로 활성화되어 있음
   - `Edit > Project Settings > Player > Other Settings`에서 확인

## 현재 프로젝트 정보

- **Unity 버전**: 6000.3.2f1
- **프로젝트 경로**: `/Users/hanjoonpyo/GitHub/BattleManager2025`
- **디버거 타입**: CoreCLR
- **프로세스 이름**: Unity

## 추가 팁

- 디버깅 중에는 Unity Editor의 성능이 약간 저하될 수 있습니다.
- 여러 Unity 프로젝트를 동시에 열고 있다면, 프로세스 선택 옵션을 사용하세요.
- Breakpoint는 코드가 실행될 때만 작동합니다. 초기화 코드를 디버깅하려면 Unity Editor 시작 시점에 Breakpoint를 설정하세요.

