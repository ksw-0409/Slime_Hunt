# Slime Hunt

## 프로젝트 개요

- **게임명:** Slime Hunt
- **장르:** 탑다운 2D 몬스터 사냥
- **개발 엔진:** Unity 6 (6000.3.24f1)
- **용도:** QA 제보 분석 AI 서비스 **버그핀(BugPin)** 의 테스트용 샘플 게임

### 이 프로젝트가 존재하는 이유

이 게임은 일부러 버그 5개를 심어 둔 테스트용 게임이다.

버그핀은 QA 제보를 받아 같은 버그끼리 묶고, 이 게임의 소스 코드에서 버그 위치를 찾는다.
버그를 직접 심었기 때문에 정답이 정해져 있고, 버그핀의 결과를 정답지와 비교해 정확도를 잰다.

> 이 저장소의 버그는 **의도된 것**이다. 고치지 않는다.
> 버그 목록은 `Docs/BugAnswerKey.md`에만 적는다. 이 README에는 적지 않는다.


## 게임 규칙

- 플레이어는 맵 안을 위, 아래, 왼쪽, 오른쪽, 대각선으로 이동한다.
- 몬스터(슬라임)는 일정 시간마다 맵 가장자리에서 생성되어 플레이어를 쫓아온다.
- 몬스터에 닿으면 플레이어 체력이 줄어든다.
- 플레이어는 스킬을 써서 주변 몬스터에게 피해를 준다. 스킬에는 쿨타임이 있다.
- 몬스터를 처치하면 점수를 얻는다.
- 플레이어 체력이 0이 되면 게임오버 화면이 나오고, 재시작할 수 있다.


## 조작

| 입력 | 동작 |
|---|---|
| W A S D / 방향키 | 이동 |
| Space | 스킬 사용 |
| R | 게임오버 후 재시작 |


## 프로젝트 구조

```text
Assets/
  Scenes/
    Main.unity
  Prefabs/
    Player.prefab
    Slime.prefab
    SkillEffect.prefab
  Scripts/
    Core/
      GameManager.cs
    Player/
      PlayerMovement.cs
      PlayerHealth.cs
      PlayerSkill.cs
    Skill/
      SkillEffect.cs
    Monster/
      MonsterChase.cs
      MonsterHealth.cs
      MonsterSpawner.cs
    UI/
      PlayerHealthUI.cs
      ScoreUI.cs
    Animation/
      CharacterWalkAnimation.cs
Docs/
  BugAnswerKey.md     버그 정답지 (버그핀에 입력하지 않음)
  WorkLogs/           작업자별 작업 로그
Packages/
ProjectSettings/
```

스크립트는 전체 400~500줄을 목표로 한다. 버그핀이 코드 전체를 한 번에 AI에 넣을 수 있어야 하기 때문이다.


## 버그핀에 넣는 범위

| 넣는 것 | 넣지 않는 것 |
|---|---|
| `Assets/Scripts/` 아래의 `.cs` 파일 | `README.md`, `AGENTS.md`, `Docs/` 전체 |

`AGENTS.md`와 `Docs/BugAnswerKey.md`에는 버그 내용이 적혀 있다.
이 파일이 분석 입력에 섞이면 AI가 코드를 보지 않고 정답을 읽게 되므로 테스트가 무의미해진다.


## 실행 방법

1. Unity Hub에서 Unity 6 (6000.3.24f1)로 프로젝트를 연다.
2. `Assets/Scenes/Main.unity`를 연다.
3. Play 버튼을 누른다.

아트는 16x16 도트 스프라이트(`Assets/Art/Sprites/`)를 쓰고, 플레이어와 슬라임은 3프레임 걷기 애니메이션(`Assets/Art/Animations/`)이 있다. 스프라이트, 애니메이션, 프리팹, `Main.unity`는 에디터 메뉴 `Tools > Slime Hunt > Build Battle Scene`(`Assets/Editor/SlimeHuntSceneBuilder.cs`)으로 다시 생성할 수 있다.
