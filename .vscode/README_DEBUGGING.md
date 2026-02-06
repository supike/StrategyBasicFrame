# Unity 디버깅 설정 가이드

Cursor에서 Unity 디버깅을 위한 몇 가지 방법이 있습니다.

## 방법 1: Unity Editor 내장 디버거 사용 (가장 권장)

Unity Editor에는 내장 디버거가 있습니다:

1. **Unity Editor에서:**
   - `Window > Analysis > Debugger` 메뉴 열기
   - 또는 Unity Editor 상단의 디버거 아이콘 클릭

2. **Breakpoint 설정:**
   - Unity Editor의 코드 에디터에서 직접 breakpoint 설정 가능
   - 또는 Cursor에서 설정한 breakpoint가 Unity Editor와 동기화될 수 있음

3. **Play 모드에서 디버깅:**
   - Unity Editor에서 Play 모드 시작
   - Breakpoint에서 멈추고 변수 확인 가능

## 방법 2: C# Dev Kit 확장 사용 (시도 가능)

1. **Cursor에서 확장 설치:**
   - `Cmd+Shift+X`로 확장 패널 열기
   - "C# Dev Kit" 검색 및 설치
   - 또는 "C#" (Microsoft) 확장 설치

2. **디버깅 시작:**
   - Unity Editor가 실행 중인지 확인
   - Cursor에서 `F5` 키 누르기
   - 프로세스 선택 창에서 "Unity" 프로세스 선택

## 방법 3: 로그를 활용한 디버깅

Cursor에서 코드를 편집하고, Unity Editor의 Console 창에서 로그를 확인하는 방법:

```csharp
Debug.Log("값: " + 변수명);
Debug.LogWarning("경고 메시지");
Debug.LogError("에러 메시지");
```

Unity Editor의 Console 창 (`Window > General > Console`)에서 로그 확인 가능.

## 방법 4: 다른 IDE 사용 (최후의 수단)

Unity 공식 지원 IDE:
- **Visual Studio** (macOS: Visual Studio for Mac 또는 Visual Studio Code)
- **JetBrains Rider** (Unity 공식 지원, 강력한 디버깅 기능)

## 현재 설정 파일

- `.vscode/launch.json`: 디버깅 설정
- `.vscode/settings.json`: 에디터 설정
- `.vscode/extensions.json`: 권장 확장 프로그램 목록

