# Claude 작업 로그

## 2026-11-20 전투 씬 1차 구현

### 한 일
- `Assets/Scripts/` 아래 게임 스크립트 10개 작성(총 433줄): GameManager, PlayerMovement, PlayerHealth, PlayerSkill, SkillEffect, MonsterChase, MonsterHealth, MonsterSpawner, PlayerHealthUI, ScoreUI
- AGENTS.md 명세대로 B2, B3, B4, B6, B7을 심음. 버그가 없는 스크립트는 정상 동작하도록 작성
- `Assets/Editor/SlimeHuntSceneBuilder.cs` 작성. 메뉴 `Tools > Slime Hunt > Build Battle Scene`을 누르면 아래를 생성한다.
  - 16x16 도트 PNG(Player, Slime, Wall, Grass), 32x32 스킬 이펙트 링, UI용 흰 텍스처 → `Assets/Art/Sprites/`
  - 프리팹 Player, Slime, SkillEffect → `Assets/Prefabs/`
  - `Assets/Scenes/Main.unity`(카메라, Global Light 2D, 20x12 경기장과 벽, 플레이어, 스포너, GameManager, 체력, 점수, 게임오버 UI)와 Build Settings 등록
- `Docs/BugAnswerKey.md` 작성
- `README.md` 실행 방법의 아트 설명을 도트 스프라이트와 빌더 메뉴 기준으로 수정
- `Packages/manifest.json`에 `com.coplaydev.unity-mcp`가 이미 있는 것을 확인. 로컬 MCP 서버 `mcp-for-unity`(http://127.0.0.1:8080/mcp)를 Claude Code 로컬 스코프에 등록

### 설계 메모
- 슬라임 스프라이트는 무채색으로 그리고 프리팹 SpriteRenderer 색으로 초록을 입힘. 사망 시 `deadColor`(회색)가 실제로 회색으로 보이게 하기 위함
- 플레이어 Rigidbody2D는 NeverSleep. 정지 상태에서도 접촉 판정이 끊기지 않게 하기 위함
- 몬스터 판정은 태그 대신 `MonsterHealth` 컴포넌트로 함(TagManager 변경 없음)
- `SampleScene.unity`와 템플릿의 `InputSystem_Actions.inputactions`는 건드리지 않음

### 확인하지 못한 것 (다음 작업자 확인 필요)
- Unity 컴파일 결과(작성 시점 Editor.log에는 에러 기록 없음. 재컴파일 전일 수 있음)
- 빌더 메뉴 실행과 생성 결과(씬, 프리팹, 스프라이트 임포트 설정)
- Play 모드에서 버그 5개 증상 재현, 스킬 쿨타임, 몬스터 생성, 점수 UI, 게임오버와 R 재시작 동작
- 위 항목은 MCP 연결 후 확인 예정. 빌더 실행 후 정답지 줄 번호는 바뀌지 않음(스크립트를 수정하지 않는 한)

## 2026-11-20 MCP로 도트 스프라이트와 전투 씬 생성

### 한 일
- Unity MCP(mcp-for-unity) 연결 확인. 컴파일 에러, 경고 없음
- MCP AI 이미지 생성(fal, openrouter)은 API 키가 없어 쓰지 않음. 사용자 결정에 따라 빌더의 16x16 도트 데이터로 생성
- MCP로 `Tools > Slime Hunt > Build Battle Scene` 실행. 생성 결과 확인
  - `Assets/Art/Sprites/` Player, Slime, Wall, Grass, SkillEffect, White PNG (Sprite, PPU 16, Point, 무압축)
  - `Assets/Prefabs/` Player, Slime, SkillEffect 프리팹. skillEffectPrefab, monsterPrefab, GameManager의 playerTransform, gameOverPanel 참조 연결됨
  - `Assets/Scenes/Main.unity` 생성, Build Settings에 등록
- 스크립트는 수정하지 않음. `Docs/BugAnswerKey.md` 줄 번호 변경 없음

### Play 모드 확인
- 몬스터가 가장자리에서 생성되고, 플레이어 반대 방향으로 이동해 벽에 붙음 (B7)
- 처치한 슬라임이 회색으로 남음 (B2), 스킬 이펙트 원이 계속 쌓임 (B3), 점수 UI 증가 확인
- 스크린샷에서 도트 스프라이트(플레이어, 슬라임, 벽, 잔디, 이펙트 링)와 체력바, 점수 UI 정상 표시
- Play 중 콘솔 에러, 경고 없음

### 확인하지 못한 것
- B4(대각선 이동 속도), B6(접촉 시 체력 급감), 스킬 쿨타임, 게임오버와 R 재시작은 키 입력이 필요해 MCP로 직접 확인하지 않음

## 2026-11-20 플레이어, 슬라임 3프레임 걷기 애니메이션

### 한 일
- `Assets/Scripts/Animation/CharacterWalkAnimation.cs` 추가(32줄). Rigidbody2D 실제 속도가 기준값보다 크면 Animator의 `IsWalking`을 켠다. 플레이어와 슬라임이 함께 쓴다
- `SlimeHuntSceneBuilder.cs` 수정
  - 플레이어: 몸통 13줄은 같고 다리 3줄만 바뀌는 3프레임(선 자세, 왼발, 오른발) → `PlayerWalk0~2.png`
  - 슬라임: 기본, 납작, 늘어남 3프레임(바닥 높이 같음) → `SlimeWalk0~2.png`
  - `Assets/Art/Animations/`에 Idle(첫 프레임), Walk(3프레임 반복) 클립과 컨트롤러 생성. 플레이어 8fps, 슬라임 6fps, 전환은 즉시
  - 프리팹에 Animator와 CharacterWalkAnimation 추가
- 예전 단일 프레임 `Player.png`, `Slime.png` 삭제 후 빌더 재실행
- `AGENTS.md` 스크립트 표와 `README.md` 구조, 실행 방법에 새 스크립트와 애니메이션 반영
- 버그 스크립트 5개는 수정하지 않음. 정답지 줄 번호 변경 없음. 게임 스크립트 합계 469줄

### 확인
- 컴파일 에러 없음. Play 중 콘솔 에러, 경고 없음
- 슬라임: 이동 중 Walk 상태로 프레임 순환, 처치되면(회색) Idle 첫 프레임에서 멈춤
- 플레이어: 이동 중 Walk 상태로 순환, 멈추면 Idle로 돌아감. 좌우 뒤집기(flipX) 유지
- 벽에 정면으로 붙어 실제 속도가 0이 된 슬라임은 Idle 프레임으로 멈춘다(속도 기준 판정이라서)

## 2026-11-20 HP 바, 스코어 UI 개선

### 한 일
- `PlayerHealthUI.cs`(버그 없음) 수정
  - 체력 비율에 따라 채움 색 변경(`healthColorGradient`, 구간 고정: 30% 이하 빨강, 60% 이하 노랑, 그 위 초록)
  - 잔상 바(`healthTrailImage`)가 `trailFollowSpeed`로 천천히 따라와 방금 받은 피해량이 보임. 게임오버로 timeScale이 0이어도 움직이도록 unscaledDeltaTime 사용
  - 숫자 표기를 `HP 100 / 100`에서 `100 / 100`으로 변경(하트 아이콘이 대신함)
- `ScoreUI.cs`(버그 없음) 수정: 점수가 오르면 숫자를 `popScale`배로 키웠다가 `popRecoverSpeed`로 되돌림. 첫 표시 때는 키우지 않음. 숫자만 표시
- `SlimeHuntSceneBuilder.cs` UI 재구성
  - 도트 패널 테두리(`UIPanel.png`, 8x8, 9-slice 테두리 2픽셀)와 하트 아이콘(`UIHeart.png`, 16x16) 생성
  - 왼쪽 위 체력 패널: 하트 + 바 슬롯 + 잔상 바 + 채움 바 + 숫자. 오른쪽 위 점수 패널: 슬라임 아이콘 + SCORE 라벨 + 점수 숫자
  - 글자에 Shadow 추가. CanvasScaler `referencePixelsPerUnit`을 16으로 맞추고 도트 한 칸을 5단위로 그림
- 버그 스크립트 5개는 수정하지 않음. 정답지 줄 번호 변경 없음. 게임 스크립트 합계 493줄(500줄 이내)

### 확인
- 컴파일, Play 중 콘솔 에러, 경고 없음
- Play 중 피해를 주어 확인: 채움 바가 즉시 줄고 잔상 바가 뒤따라 줄어듦, 색이 초록 → 노랑 → 빨강으로 바뀜
- 몬스터 처치 시 점수 숫자가 약 1.23배로 커졌다가 돌아옴
- Unity 창이 비활성이면 Game 뷰가 그려지지 않아 MCP 스크린샷에 UI가 빠짐. 캔버스를 카메라에 임시로 렌더해 확인함(씬에는 저장하지 않음)

### 알려진 사항
- 게임 스크립트가 493줄이라 앞으로 기능을 더 넣으면 500줄 제한을 넘기기 쉽다

## 2026-11-20 프로젝트 구조 PDF

### 한 일
- `Docs/ProjectStructure.pdf`(A4 2쪽) 작성: 게임 개요, 조작, 폴더 구조, 게임 스크립트별 책임과 줄 수, Main 씬 구성, 작업 규칙 요약
- 공유될 수 있는 문서라 심은 버그의 내용과 위치는 넣지 않음
- HTML로 작성해 Edge 헤드리스 인쇄로 PDF 변환(맑은 고딕). 원본 HTML은 임시 폴더에만 있음
- 내용은 작성 시점 기준(스크립트 493줄). 구조가 바뀌면 다시 만들어야 함

## 2026-11-20 버그 목록, 정답지 PDF

### 한 일
- `Docs/BugAnswerKey.pdf`(A4 3쪽) 작성: 버그 목록 표, 채점 규칙, 버그별 위치, 원인, 정상 동작, 해당 코드(버그 줄 강조), Play 모드 확인 현황
- 작성 전 현재 스크립트와 `Docs/BugAnswerKey.md`의 줄 범위가 일치함을 확인(B2 40~52, B3 9~20, B4 50~51, B6 24, B7 40)
- B6의 "초당 50회"는 `ProjectSettings/TimeManager.asset`의 Fixed Timestep 0.02 기준
- 문서 맨 위에 "버그핀 입력, 테스터 자료에 포함 금지" 경고를 넣음
- 원본 HTML은 임시 폴더에만 있음. 코드 줄이 바뀌면 md와 PDF를 함께 다시 만들어야 함

### 확인하지 못한 것
- B4, B6은 아직 Play 모드에서 직접 측정하지 않음 (PDF의 확인 현황 표에도 그대로 적음)
