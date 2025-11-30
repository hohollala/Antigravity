# Antigravity Editor for Unity - 설치 가이드

Antigravity를 Unity의 기본 코드 편집기로 설정하는 단계별 가이드입니다.

## 빠른 설치 (5분)

### 1. Antigravity 설치

먼저 Antigravity 에디터가 설치되어 있어야 합니다.

**macOS (Homebrew)**:
```bash
brew install antigravity
```

**Windows (Scoop)**:
```bash
scoop install antigravity
```

**Linux (Cargo)**:
```bash
cargo install antigravity
```

**또는 공식 사이트에서 다운로드**:
- https://antigravity.dev

설치 후 터미널에서 확인:
```bash
which antigravity   # macOS/Linux
where antigravity   # Windows
```

### 2. Unity 패키지 설치

#### 방법 A: Git URL (권장)

1. Unity 에디터에서 **Window > TextAsset > Package Manager** 열기
2. **+** 버튼 클릭
3. **Add package from git URL** 선택
4. 이 URL 입력: `https://github.com/yourusername/com.antigravity.editor.git`
5. **Add** 클릭
6. 설치 완료 대기 (1-2분)

#### 방법 B: 수동 설치

```bash
# 이 저장소 클론
git clone https://github.com/yourusername/com.antigravity.editor.git

# 프로젝트 내로 복사
cp -r com.antigravity.editor ~/Desktop/MyUnityProject/Packages/
```

### 3. 경로 설정

1. Unity 에디터 > **Edit > Preferences** (Windows/Linux) 또는 **Unity > Preferences** (macOS)
2. **External Tools** 섹션 검색
3. **External Script Editor** 드롭다운에서 **Antigravity** 선택
4. **Auto-detect Antigravity Path** 버튼 클릭
5. 또는 **Browse** 버튼으로 수동 선택

### 4. 테스트

1. 아무 C# 스크립트 파일을 **더블클릭**
2. Antigravity가 실행되고 파일이 열려야 함

완료! 🎉

---

## 상세 설치 가이드

### 필수 요구사항

| 항목 | 최소 버전 | 권장 버전 |
|------|---------|---------|
| Unity | 2019.4 LTS | 2022 LTS 이상 |
| Antigravity | 최신 | 최신 |
| 운영체제 | - | Windows 10+, macOS 10.12+, Ubuntu 16.04+ |

### 단계별 설치 (상세)

#### Step 1: Antigravity 설치

**macOS (Homebrew)**:
```bash
# Homebrew 업데이트
brew update

# Antigravity 설치
brew install antigravity

# 설치 확인
antigravity --version
```

**Windows (Scoop)**:
```powershell
# Scoop 업데이트
scoop update

# Antigravity 설치
scoop install antigravity

# 설치 확인
antigravity --version
```

**Linux (APT - Ubuntu/Debian)**:
```bash
# 패키지 업데이트
sudo apt update

# Antigravity 설치 (있으면)
sudo apt install antigravity

# Cargo로 설치 (권장)
cargo install antigravity

# 설치 확인
antigravity --version
```

#### Step 2: 패키지 설치

**Git 방법**:

```bash
# 터미널에서 프로젝트 디렉토리로 이동
cd ~/Desktop/MyUnityProject

# Package Manager 설정 (필요시)
# Packages/manifest.json 파일에 다음 추가:
# "com.antigravity.editor": "https://github.com/yourusername/com.antigravity.editor.git"
```

또는 Unity 에디터 UI 사용 (위의 빠른 설치 참조)

**수동 방법**:

```bash
# 패키지 클론
git clone https://github.com/yourusername/com.antigravity.editor.git

# 프로젝트 내 Packages 폴더로 이동
mv com.antigravity.editor ~/path/to/project/Packages/
```

#### Step 3: Preferences 설정

1. Unity 에디터 실행
2. **Edit > Preferences** (Windows/Linux) 또는 **Unity > Preferences** (macOS) 클릭
3. 왼쪽 패널에서 **External Tools** 찾기
4. **External Script Editor** 드롭다운 확인 (Antigravity가 표시되어야 함)
5. Antigravity 선택

#### Step 4: 경로 자동 감지

1. Antigravity 선택 후, **Auto-detect Antigravity Path** 버튼 클릭
2. 자동 감지 성공 메시지 확인
3. 또는 **Browse** 클릭하여 수동으로 선택

#### Step 5: 테스트

1. Unity 프로젝트에서 임의의 C# 스크립트 더블클릭
2. Antigravity가 실행되고 파일이 열려야 함

---

## 경로 설정 옵션

### 자동 감지 (권장)

**Preferences > External Tools > Antigravity Editor Settings**에서:
- **Auto-detect Antigravity Path** 버튼 클릭
- 자동으로 다음 위치에서 검색:
  - 시스템 PATH 환경변수
  - macOS: `/usr/local/bin`, `/opt/homebrew/bin`
  - Windows: `Program Files`, `AppData\Local\Programs`
  - Linux: `/usr/local/bin`, `/usr/bin`, `~/.local/bin`

### 수동 설정

**Browse** 버튼으로 직접 선택:
1. 파일 브라우저 오픈
2. Antigravity 실행 파일 선택
3. **Open** 클릭

### 터미널에서 설정 (macOS/Linux)

```bash
# 경로 확인
which antigravity

# EditorPrefs에 저장 (macOS)
defaults write com.unity3d.UnityEditor AntigravityEditorPath -string "/usr/local/bin/antigravity"
```

---

## 문제 해결

### 패키지가 설치되지 않음

**증상**: Package Manager에 com.antigravity.editor가 보이지 않음

**해결책**:
1. Unity 재시작
2. Window > TextAsset > Package Manager 다시 열기
3. 또는 수동으로 Packages 폴더에 복사

### Antigravity가 Preferences에 나타나지 않음

**증상**: External Script Editor 드롭다운에 Antigravity 없음

**해결책**:
1. Unity 완전 종료 (강제 종료 필요할 수 있음)
2. Unity 다시 실행
3. Edit > Preferences > External Tools 확인
4. 또는 Library/ScriptAssemblies 폴더 삭제 후 재시작

### 스크립트 더블클릭 시 실행되지 않음

**증상**: 경로가 설정되어 있지만 Antigravity 실행 안 됨

**해결책**:
1. 경로 재확인: **Browse** 버튼으로 올바른 경로 확인
2. 경로 재설정: **Auto-detect** 또는 **Browse** 사용
3. Unity 권한 확인: 관리자 권한으로 Unity 실행 시도

### "Antigravity executable not found" 오류

**증상**: Antigravity를 실행할 수 없음

**해결책**:
1. Antigravity가 실제로 설치되었는지 확인:
   ```bash
   which antigravity     # macOS/Linux
   where antigravity     # Windows
   ```

2. 설치 경로가 올바른지 확인:
   ```bash
   antigravity --version
   ```

3. 권한 확인 (Linux/macOS):
   ```bash
   chmod +x /path/to/antigravity
   ```

### 경로가 저장되지 않음

**증상**: 경로를 설정해도 다시 열면 초기화됨

**해결책**:
1. Unity를 관리자 권한으로 실행
2. EditorPrefs 캐시 삭제:
   - macOS: `~/Library/Preferences/com.unity3d.UnityEditor.plist`
   - Windows: `HKEY_CURRENT_USER\Software\Unity\EditorPreferences`
3. Unity 재시작

---

## 고급 설정

### 커스텀 CLI 인자

AntigravityEditor.cs의 `ParseArguments()` 메서드를 수정하여 커스텀 인자 형식 사용 가능:

```csharp
private string ParseArguments(string filePath, int line, int column)
{
    // 기본 형식: "file.cs":10:5
    // return $"\"{filePath}\":{line}:{column}";

    // 커스텀 형식 예시
    return $"open \"{filePath}\" --line {line} --column {column}";
}
```

### 프로젝트별 설정

각 Unity 프로젝트마다 다른 Antigravity 버전 사용 가능:
- ProjectA: Antigravity v1.0
- ProjectB: Antigravity v2.0

Preferences에서 각각 설정하면 프로젝트별로 독립적으로 관리 가능

### 글로벌 설치

모든 Unity 프로젝트에서 같은 패키지 사용:

```bash
# macOS/Linux
mkdir -p ~/.unity/packages
cp -r com.antigravity.editor ~/.unity/packages/

# Windows
mkdir %APPDATA%\Unity\packages
copy com.antigravity.editor %APPDATA%\Unity\packages\
```

---

## 다음 단계

1. **README.md** 읽기: 패키지 기능 상세 설명
2. **스크립트 편집**: C# 파일 더블클릭하여 Antigravity에서 편집
3. **피드백**: 문제 발견 시 GitHub Issues에 보고

---

## 지원

문제가 발생하면:
1. 이 문서의 **문제 해결** 섹션 확인
2. GitHub Issues에서 검색
3. GitHub Issues에 새로운 issue 생성

---

## 라이선스

MIT License - 자유롭게 수정, 배포 가능합니다.
