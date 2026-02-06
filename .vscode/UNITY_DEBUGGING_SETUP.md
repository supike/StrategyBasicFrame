# Unity 디버깅 설정 완료 가이드

## 1. Unity Editor에서 Script Debugging 활성화

Unity Editor에서 다음 단계를 따라주세요:

1. **Unity Editor 열기**
2. **Edit > Project Settings** 메뉴 선택
3. **Player** 섹션 선택
4. **Other Settings** 섹션으로 스크롤
5. **Scripting Define Symbols** 확인 (필요시)
6. **Editor에서 디버깅 활성화:**
   - Unity Editor 상단 메뉴에서 **Edit > Preferences** (macOS) 또는 **Edit > Settings** (Windows)
   - **External Tools** 섹션으로 이동
   - **External Script Editor**를 Cursor로 설정:
     - macOS: `/Applications/Cursor.app` 또는 `cursor` 명령어
   - **Script Debugging** 옵션이 있다면 활성화

## 2. Cursor에서 확장 프로그램 설치

다음 확장 프로그램이 설치되어 있는지 확인하세요:

1. **Visual Studio Tools for Unity** (`visualstudiotoolsforunity.vstuc`)
   - Unity 디버깅을 위한 공식 확장
   - Cursor에서 `Cmd+Shift+X`로 확장 패널 열기
   - "Visual Studio Tools for Unity" 검색 및 설치

2. **C#** (`ms-dotnettools.csharp`)
   - C# 언어 지원 및 IntelliSense

## 3. 디버깅 시작 방법

### 방법 1: Unity Visual Studio Tools 사용 (권장)

1. **Unity Editor 실행** (프로젝트 열기)
2. **Cursor에서 F5 키 누르기** 또는 디버그 패널에서 "Attach to Unity Editor" 선택
3. Unity Editor가 자동으로 감지되어 연결됨
4. C# 파일에 **Breakpoint 설정** (F9 또는 라인 번호 클릭)
5. Unity Editor에서 **Play 모드 시작**
6. Breakpoint에서 코드가 멈추고 변수 확인 가능

### 방법 2: CoreCLR 디버거 사용 (대안)

1. **Unity Editor 실행**
2. Cursor에서 "Attach to Unity Editor (CoreCLR)" 선택
3. 프로세스 목록에서 "Unity" 선택
4. Breakpoint 설정 후 Play 모드 시작

## 4. 문제 해결

### 디버거가 연결되지 않는 경우:

1. **Unity Editor가 실행 중인지 확인**
2. **Unity Editor 재시작** (종료 후 다시 열기)
3. **Cursor 재시작**
4. **확장 프로그램 재설치:**
   - Visual Studio Tools for Unity 확장 제거 후 재설치
5. **Unity 프로젝트 재생성:**
   - Unity Editor에서 `Assets > Reimport All`
   - 또는 `.csproj` 파일 삭제 후 Unity Editor 재시작

### Breakpoint가 작동하지 않는 경우:

1. **Script Debugging이 활성화되어 있는지 확인**
2. **코드가 최신인지 확인** (Unity Editor에서 스크립트 컴파일 완료 대기)
3. **Debug 빌드인지 확인** (Release 모드에서는 디버깅이 제한될 수 있음)
4. **Burst 컴파일 비활성화** (Burst를 사용하는 경우):
   - `[BurstCompile]` 속성 주석 처리 또는
   - Unity Editor에서 `Jobs > Burst > Enable Compilation` 비활성화

### IntelliSense가 작동하지 않는 경우:

1. **OmniSharp 재시작:**
   - Command Palette (`Cmd+Shift+P`)
   - "OmniSharp: Restart OmniSharp" 실행
2. **프로젝트 파일 확인:**
   - `.sln` 파일이 있는지 확인
   - Unity Editor에서 프로젝트 파일 재생성

## 5. 디버깅 기능 사용법

- **Breakpoint 설정/해제**: 라인 번호 왼쪽 클릭 또는 F9
- **Step Over (F10)**: 다음 줄로 이동
- **Step Into (F11)**: 함수 내부로 들어가기
- **Step Out (Shift+F11)**: 현재 함수에서 나가기
- **Continue (F5)**: 다음 breakpoint까지 실행
- **Variables 패널**: 현재 스코프의 변수 값 확인
- **Watch 패널**: 특정 변수나 표현식 모니터링
- **Call Stack**: 함수 호출 스택 확인

## 6. Unity Editor 설정 확인

Unity Editor에서 다음 설정을 확인하세요:

- **Edit > Preferences > External Tools > External Script Editor**: Cursor로 설정
- **Edit > Project Settings > Player > Other Settings > Scripting Backend**: Mono 또는 IL2CPP
- **Edit > Project Settings > Editor > Asset Pipeline**: V2 (권장)

## 참고

- Unity 2021.2 이상에서는 Script Debugging이 기본적으로 활성화되어 있습니다.
- macOS에서는 Unity Editor 프로세스 이름이 "Unity"입니다.
- 디버깅 중에는 Unity Editor의 성능이 약간 저하될 수 있습니다.



