# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 프로젝트 개요

**Antigravity Unity Editor Integration** - Unity의 외부 코드 편집기로 Antigravity를 통합하기 위한 UPM (Unity Package Manager) 패키지입니다.

### 프로젝트 목적
- Unity 에디터에서 C# 스크립트 편집 시 Antigravity 자동 실행
- 자동 경로 감지 (Windows, macOS, Linux)
- 사용자 친화적 설정 UI (Unity Preferences)
- IntelliSense 및 .csproj 자동 생성

## 아키텍처 개요

### 핵심 구조
```
com.antigravity.editor/
├── Editor/
│   ├── AntigravityEditor.cs          # IExternalCodeEditor 인터페이스 구현
│   ├── Antigravity.Editor.asmdef     # 에디터 전용 Assembly 정의
│   └── [Discovery 및 Installation 클래스들]  # 경로 감지 로직
├── package.json                       # UPM 패키지 메타데이터
├── README.md                          # 사용자 가이드
├── QUICK_START.md                     # 5분 빠른 시작
├── INSTALL.md                         # 상세 설치 및 트러블슈팅
└── CHANGELOG.md                       # 버전 이력
```

### 아키텍처 패턴

#### 1. IExternalCodeEditor 구현
- **클래스**: `AntigravityExternalEditor`
- **책임**:
  - Antigravity 설치 자동 감지
  - 스크립트 파일을 Antigravity에서 열기
  - Preferences UI 제공 (.csproj 생성 옵션)
- **주요 메서드**:
  - `Installations`: 발견된 모든 Antigravity 설치 반환
  - `OpenProject()`: 파일, 라인, 컬럼 정보로 에디터 실행
  - `OnGUI()`: Preferences 탭에서 UI 렌더링
  - `SyncIfNeeded()`: 프로젝트 파일 변경 시 .csproj 동기화

#### 2. 설치 감지 시스템 (Discovery)
- **책임**: 시스템에 설치된 Antigravity 위치 자동 감지
- **감지 전략**:
  1. EditorPrefs에 저장된 경로 확인
  2. 시스템 PATH 환경변수 검색
  3. 플랫폼별 표준 설치 위치 탐색
- **플랫폼 지원**:
  - **Windows**: Program Files, AppData, Scoop
  - **macOS**: /usr/local/bin, Homebrew, Cargo
  - **Linux**: /usr/local/bin, /usr/bin, Cargo, ~/.local/bin

#### 3. .csproj 생성 시스템
- **책임**: IntelliSense를 위한 Visual Studio 프로젝트 파일 생성
- **설정 옵션**: 패키지 타입별 생성 여부 제어
  - Embedded, Local, Registry, Git, Built-in, LocalTarBall, Unknown packages
  - Player Assemblies (선택사항)
- **실행 시점**:
  - 파일 추가/삭제/이동 시 (SyncIfNeeded)
  - 사용자가 Regenerate 버튼 클릭 시

#### 4. 비동기 작업 처리
- **AsyncOperation**: Antigravity 설치 감지를 백그라운드에서 실행
- **목적**: Unity 에디터 시작 시간에 영향 최소화

### 의존성 관계
```
AntigravityExternalEditor (Main)
├── Discovery (설치 감지)
├── IAntigravityBaseInstallation (설치 표현)
├── IGenerator (.csproj 생성)
└── CodeEditor (Unity API)
```

## 개발 명령어

### 빌드 및 실행
```bash
# Unity 2019.4 이상에서 패키지 설치
# Window > TextAsset > Package Manager
# + > Add package from git URL
# https://github.com/yourusername/com.antigravity.editor.git
```

### 로컬 테스트
```bash
# 1. 이 저장소 클론
cd /Users/supermac/Dev/aiProjects/Antigravity/com.antigravity.editor

# 2. Unity 프로젝트의 Packages 폴더에 링크
# Packages/com.antigravity.editor -> 이 디렉토리

# 3. Unity 에디터에서 스크립트 재컴파일 대기

# 4. Edit > Preferences > External Tools에서 확인
```

### 단위 테스트
```bash
# Unity Editor에서 Test Framework를 통해 실행
# Window > General > Test Runner
# PlayMode 또는 EditMode 테스트 실행
```

## 주요 개발 개념

### InitializeOnLoad 패턴
- `[InitializeOnLoad]` 속성으로 Unity 에디터 시작 시 자동 초기화
- 정적 생성자에서 `CodeEditor.Register()`로 등록
- 패키지 로드 순서 보장 안 함 (비동기 처리로 해결)

### EditorPrefs 활용
- `EditorPrefs.GetBool/SetBool`: 윈도우 재사용 옵션 저장
- `EditorPrefs.GetString/SetString`: Antigravity 경로 저장
- 프로젝트별 독립적인 설정 유지

### .csproj 동기화 전략
- **IsSupportedPath()**: cs, uxml, uss, shader 등 지원 파일 확인
- **IsProjectGeneratedFor()**: 특정 파일이 .csproj에 포함되어 있는지 확인
- **ProjectGenerationFlag**: 패키지 타입별 생성 여부 제어 (비트 플래그)

## 코드 조직 원칙

### 네임스페이스
- **`Antigravity.Editor`**: 메인 에디터 통합 로직
- 모든 에디터 코드는 `Editor/` 폴더에 위치

### Assembly Definition (.asmdef)
- **Editor 전용**: `includePlatforms: ["Editor"]`
- **프로덕션 빌드 제외**: 플레이어 빌드에 포함 안 됨
- **Newtonsoft.Json 참조**: JSON 파싱 필요

### 코드 스타일
- Microsoft/Unity C# 컨벤션 따름
- 에러 처리: `try-catch` 사용, `Debug.LogError/Warning` 로깅
- GUI: EditorGUILayout 사용 (자동 레이아웃)

## 자주 수정되는 부분

### 1. 새로운 표준 경로 추가
**파일**: `Discovery.cs`의 `GetAntigravityBaseInstallations()`
```csharp
// 새 경로 추가 예시
var commonPaths = new[]
{
    "/new/standard/path",  // Linux
    // 또는
    $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}\\Antigravity"  // Windows
};
```

### 2. .csproj 생성 옵션 변경
**파일**: `AntigravityExternalEditor.cs`의 `OnGUI()`
```csharp
// SettingsButton() 호출로 옵션 추가
SettingsButton(ProjectGenerationFlag.NewType, "새 패키지 타입", "", installation);
```

### 3. Preferences UI 커스터마이징
**파일**: `AntigravityExternalEditor.cs`의 `OnGUI()` 메서드
- EditorGUILayout 컴포넌트로 UI 구성
- EditorPrefs로 설정값 저장/로드

## 테스트 전략

### 필수 테스트 시나리오
1. **경로 감지**
   - 표준 위치에 Antigravity 설치 시 자동 감지
   - 비표준 위치에 설치된 경우 수동 설정

2. **파일 열기**
   - C# 스크립트 더블클릭 → Antigravity 실행
   - 파일, 라인, 컬럼 정보 정확히 전달

3. **.csproj 생성**
   - 패키지 타입별로 정확한 .csproj 생성
   - 옵션 토글 시 변경사항 반영

4. **크로스플랫폼**
   - Windows, macOS, Linux에서 동작 확인

## 문서 구조

| 파일 | 대상 | 내용 |
|------|------|------|
| README.md | 개발자 & 사용자 | 전체 기능, 설치, 설정 |
| QUICK_START.md | 신규 사용자 | 5분 내 기본 설정 |
| INSTALL.md | 고급 사용자 | 상세 설치, 트러블슈팅 |
| CHANGELOG.md | 유지보수자 | 버전별 변경사항 |

## 향후 개발 로드맵 (참고용)

- [ ] Unity 2020+ 최신 API 지원
- [ ] 추가 경로 감지 (WSL, Docker)
- [ ] 성능 프로파일링 최적화
- [ ] 완전한 자동화 테스트 스위트

## 주요 참고 문서

- **Unity CodeEditor API**: Unity 공식 문서
- **package.json 스펙**: https://docs.unity3d.com/Manual/upm-manifestSchema.html
- **Assembly Definition**: https://docs.unity3d.com/Manual/AssemblyDefinitionFiles.html
