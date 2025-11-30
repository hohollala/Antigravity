# Antigravity Editor for Unity - 빠른 시작 (5분)

5분 안에 Antigravity를 Unity의 기본 코드 에디터로 설정하세요.

## 1️⃣ Antigravity 설치 (2분)

### macOS
```bash
brew install antigravity
```

### Windows
```bash
scoop install antigravity
```

### Linux
```bash
cargo install antigravity
```

설치 확인:
```bash
antigravity --version
```

---

## 2️⃣ Unity 패키지 설치 (2분)

### 방법 A: Unity Package Manager (권장)

1. Unity 에디터 열기
2. **Window > TextAsset > Package Manager** 클릭
3. **+** 버튼 > **Add package from git URL**
4. URL 입력: `https://github.com/yourusername/com.antigravity.editor.git`
5. **Add** 클릭

### 방법 B: 수동

```bash
git clone https://github.com/yourusername/com.antigravity.editor.git
# 폴더를 프로젝트의 Packages 폴더에 복사
```

---

## 3️⃣ 경로 설정 (1분)

1. **Edit > Preferences** (또는 **Unity > Preferences** on macOS)
2. **External Tools** 선택
3. **External Script Editor** 드롭다운에서 **Antigravity** 선택
4. **Auto-detect Antigravity Path** 버튼 클릭

✅ 완료!

---

## 사용 방법

C# 스크립트 파일을 **더블클릭**하면 Antigravity에서 자동으로 열립니다.

---

## 문제 발생?

- ❌ Antigravity가 드롭다운에 안 보임?
  → Unity 재시작

- ❌ 스크립트 더블클릭 시 실행 안 됨?
  → Preferences에서 경로 재확인 및 Browse로 다시 설정

- ❌ 경로 자동 감지 안 됨?
  → **Browse** 버튼으로 수동 선택

---

더 자세한 정보는 **README.md** 또는 **INSTALL.md**를 참고하세요.
