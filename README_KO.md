# Antigravity Editor for Unity - 한국어 가이드

이 문서는 한국어 사용자를 위한 종합 가이드입니다.

## 설치 (3분)

### 1단계: Antigravity 설치

**macOS:**
```bash
brew install antigravity
```

**Windows:**
```bash
scoop install antigravity
```

**Linux:**
```bash
cargo install antigravity
```

### 2단계: Unity 패키지 설치

Unity 에디터에서:
1. Window > TextAsset > Package Manager 열기
2. + 버튼 클릭
3. "Add package from git URL" 선택
4. URL 입력: `https://github.com/yourusername/com.antigravity.editor.git`
5. Add 클릭

### 3단계: 경로 설정

1. Edit > Preferences (macOS는 Unity > Preferences)
2. External Tools 섹션 찾기
3. External Script Editor에서 Antigravity 선택
4. "Auto-detect Antigravity Path" 버튼 클릭

완료! ✅

## 사용 방법

C# 스크립트 파일을 더블클릭하면 Antigravity가 자동으로 실행됩니다.

## 문제 해결

| 문제 | 해결책 |
|------|-------|
| Antigravity가 드롭다운에 안 보여요 | Unity를 완전히 재시작하세요 |
| 스크립트 더블클릭이 안 돼요 | 경로를 Auto-detect 또는 Browse로 다시 설정하세요 |
| 경로 자동 감지가 안 돼요 | Browse 버튼으로 수동으로 선택하세요 |

## 더 알아보기

- **README.md**: 전체 가이드 (영어)
- **QUICK_START.md**: 5분 빠른 시작
- **INSTALL.md**: 상세 설치 가이드

## 지원

문제가 발생하면 GitHub Issues에 보고해주세요.

---

**Happy Coding with Antigravity! 🚀**
