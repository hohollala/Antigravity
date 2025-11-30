# Antigravity Editor for Unity

Unity 외부 스크립트 편집기로 Antigravity를 사용하기 위한 공식 통합 패키지입니다.

## 기능

- ✅ Antigravity 자동 감지 및 설치
- ✅ Unity Preferences에서 간단한 경로 설정
- ✅ Windows, macOS, Linux 완벽 지원
- ✅ 스크립트 더블클릭 시 자동으로 Antigravity 실행
- ✅ 파일, 라인, 컬럼 정보 자동 전달

## 설치 방법

### 방법 1: Git URL을 이용한 설치 (권장)

Unity 2019.4 이상에서:

1. **Window > TextAsset > Package Manager** 열기
2. **+** 버튼 클릭
3. **Add package from git URL** 선택
4. 다음 URL 입력:
   ```
   https://github.com/yourusername/com.antigravity.editor.git
   ```
5. **Add** 클릭

### 방법 2: 수동 설치

1. 이 저장소를 클론:
   ```bash
   git clone https://github.com/yourusername/com.antigravity.editor.git
   ```

2. `com.antigravity.editor` 폴더를 다음 위치에 복사:
   - **프로젝트별**: `Assets/Packages/com.antigravity.editor/`
   - **전역**: `~/.unity/packages/com.antigravity.editor/` (macOS/Linux)
   - **전역**: `%APPDATA%\Unity\packages\com.antigravity.editor\` (Windows)

## 사용 방법

### 1단계: Antigravity 경로 설정

1. Unity 에디터 > **Edit > Preferences** (Windows) 또는 **Unity > Preferences** (macOS) 열기
2. **External Tools** 섹션으로 이동
3. **External Script Editor** 드롭다운에서 **Antigravity** 선택
4. 경로를 설정합니다:
   - **자동 감지** 버튼을 클릭하거나
   - **Browse** 버튼으로 수동 선택

### 2단계: 스크립트 편집

1. Unity 프로젝트에서 C# 스크립트 파일을 **더블클릭**
2. Antigravity가 자동으로 실행되고 파일이 열립니다

## 설정

### 자동 감지

Antigravity가 표준 위치에 설치된 경우, **Auto-detect Antigravity Path** 버튼을 클릭하여 자동으로 감지합니다.

지원하는 위치:
- **Windows**:
  - `C:\Program Files\Antigravity\antigravity.exe`
  - `AppData\Local\Programs\Antigravity\antigravity.exe`
  - Scoop 설치 위치

- **macOS**:
  - `/usr/local/bin/antigravity`
  - `/opt/homebrew/bin/antigravity`
  - `~/.cargo/bin/antigravity` (Cargo)

- **Linux**:
  - `/usr/local/bin/antigravity`
  - `/usr/bin/antigravity`
  - `~/.cargo/bin/antigravity`
  - `~/.local/bin/antigravity`

### 수동 설정

**Browse** 버튼을 클릭하여 Antigravity 실행 파일을 수동으로 선택합니다.

## 문제 해결

### Antigravity가 External Script Editor 드롭다운에 나타나지 않음

**해결책:**
1. Unity 에디터를 완전히 종료
2. 다시 실행하여 스크립트 재컴파일 대기
3. **Preferences > External Tools**로 이동하여 확인

### 스크립트를 더블클릭해도 Antigravity가 열리지 않음

**해결책:**
1. **Preferences > External Tools**에서 경로가 올바르게 설정되었는지 확인
2. 파일 경로가 정확한지 확인:
   ```bash
   which antigravity  # macOS/Linux
   where antigravity  # Windows
   ```
3. 경로를 수동으로 설정 시도

### 권한 오류 (Linux/macOS)

Antigravity 실행 파일에 실행 권한이 있는지 확인:

```bash
chmod +x /path/to/antigravity
```

## 요구사항

- **Unity**: 2019.4 LTS 이상
- **Antigravity**: 최신 버전
- **운영체제**: Windows, macOS, Linux

## 개발자 정보

이 패키지는 Unity CodeEditor API를 사용하여 구현되었습니다.

### 프로젝트 구조

```
com.antigravity.editor/
├── Editor/
│   ├── AntigravityEditor.cs          # IExternalCodeEditor 구현
│   └── Antigravity.Editor.asmdef     # 에디터 전용 Assembly
├── package.json                       # UPM 패키지 정의
└── README.md                          # 이 파일
```

### 주요 클래스

- **AntigravityEditor**: `IExternalCodeEditor` 인터페이스를 구현
  - `GetAntigravityPath()`: 자동 경로 감지
  - `OpenProject()`: 스크립트 파일 열기
  - `OnGUI()`: Preferences UI 제공
  - `ParseArguments()`: CLI 인자 형식 생성

## 라이선스

MIT License - 자유롭게 사용, 수정, 배포 가능합니다.

## 기여

버그 리포트, 기능 제안, 풀 리퀘스트를 환영합니다!

## 변경 이력

### v1.0.0 (2024)
- 초기 릴리스
- Antigravity 에디터 통합
- 자동 경로 감지
- 크로스플랫폼 지원 (Windows, macOS, Linux)
