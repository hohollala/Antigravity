# Changelog

모든 notable한 변경사항은 이 파일에 기록됩니다.

## [1.1.18] - 2025-12-28
### Added
- **Antigravity/keybindings.json 생성 메뉴 추가**: 사용자 커스텀 `keybindings.json` 파일을 자동 생성하는 메뉴 추가
- **기본 단축키 설정 추가**: Explorer, Search, Debug, Extensions 뷰 단축키(Alt+1~4) 및 기타 코드 편집 단축키 제공

## [1.0.0] - 2024

### Added
- Antigravity 에디터 초기 통합
- Unity CodeEditor API (`IExternalCodeEditor`) 구현
- 자동 경로 감지 기능
- Windows, macOS, Linux 크로스플랫폼 지원
- Preferences UI에서 경로 설정 가능
- Browse 및 Auto-detect 버튼
- CLI 인자 형식 생성 기능
- 시스템 PATH 환경변수에서 자동 감지
- 공통 설치 위치 자동 검색
- EditorPrefs를 통한 경로 저장 및 로드

### Features
- **[InitializeOnLoad]**: Unity 에디터 시작 시 자동 등록
- **경로 감지 3단계**: EditorPrefs → PATH → 공통 위치
- **UI 설정**: Preferences > External Tools에서 간편한 설정
- **파일 열기**: 스크립트 더블클릭 시 자동 실행
- **위치 정보**: 파일, 라인, 컬럼 정보 자동 전달

### Technical
- Unity 2019.4 LTS 이상 지원
- Assembly Definition File (asmdef)로 에디터 전용 코드 분리
- UPM (Unity Package Manager) 표준 구조
- Process.Start()를 통한 안전한 프로세스 실행

---

## Version Format

- `[MAJOR].[MINOR].[PATCH]`
- Major: 호환성 없는 변경
- Minor: 하위 호환성이 있는 기능 추가
- Patch: 버그 수정

---

## 향후 계획 (Roadmap)

### v1.1.0 (계획 중)
- [ ] 여러 Antigravity 버전 지원
- [ ] 프로젝트별 경로 설정
- [ ] 커스텀 CLI 인자 형식 설정
- [ ] Antigravity 플러그인 발견 기능

### v1.2.0 (계획 중)
- [ ] .csproj 파일 자동 동기화
- [ ] 성능 최적화
- [ ] 더 나은 에러 메시지

### v2.0.0 (계획 중)
- [ ] 다른 에디터 지원 (VS Code, Rider 등)
- [ ] UI 개선
- [ ] 고급 설정 옵션

---

## Breaking Changes

현재 버전에서는 breaking changes가 없습니다.

---

## 지원 終了 (EOL)

현재 지원 중인 버전:
- **v1.0.x**: 완벽 지원

---

## 기여자

- Antigravity Team
