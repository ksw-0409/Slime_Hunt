# Slime Hunt 버그 정답지

> 버그핀 채점용 파일이다. **버그핀 분석 입력에 절대 포함하지 않는다.**
> 코드를 고쳐 줄 번호가 바뀌면 이 표도 함께 갱신한다.

| 버그 ID | 증상 | 파일 | 함수 | 줄 범위 | 채점 기준 |
|---|---|---|---|---|---|
| B2 | 처치한 슬라임이 회색으로 변한 채 그 자리에 계속 남는다. | `Assets/Scripts/Monster/MonsterHealth.cs` | `Die()` | 40 ~ 52 | 코드가 빠진 버그. 제시한 줄이 40~52 안이면 정답 |
| B3 | 스킬을 쓸 때마다 이펙트 원이 화면에 쌓여 계속 남는다. 피해는 생성 순간 한 번만 준다. | `Assets/Scripts/Skill/SkillEffect.cs` | `Start()` | 9 ~ 20 | 코드가 빠진 버그. 제시한 줄이 9~20 안이면 정답 |
| B4 | 대각선 이동이 직선 이동보다 약 1.4배 빠르다. | `Assets/Scripts/Player/PlayerMovement.cs` | `Move()` | 50 ~ 51 | 잘못 쓴 코드. 버그 줄 50(정규화 없이 만든 입력 벡터) 또는 51(그 벡터에 바로 `moveSpeed`를 곱해 `linearVelocity`에 대입)이면 정답 |
| B6 | 슬라임에 잠깐만 닿아도 체력이 거의 즉시 0이 된다. | `Assets/Scripts/Player/PlayerHealth.cs` | `OnCollisionStay2D()` | 24 | 잘못 쓴 코드. 버그 줄 24(무적 시간 없이 매 물리 프레임 `TakeDamage(contactDamage)` 호출)이면 정답. 함수 범위는 20~26 |
| B7 | 슬라임이 플레이어 반대쪽으로 멀어지다가 벽에 붙는다. | `Assets/Scripts/Monster/MonsterChase.cs` | `ChasePlayer()` | 40 | 잘못 쓴 코드. 버그 줄 40(`transform.position - playerTransform.position`)이면 정답. 함수 범위는 38~48 |

## 채점 규칙

- B4, B6, B7(잘못 쓴 코드가 있는 버그): AI가 제시한 줄이 버그 줄과 일치하면 정답.
- B2, B3(코드가 빠진 버그): AI가 제시한 줄이 해당 함수 범위 안에 있으면 정답.
- 파일과 함수가 맞고 줄이 틀리면 부분 정답으로 기록한다.

## 정상 동작 기준 (참고)

- B2: `Die()` 끝에서 `Destroy(gameObject)`를 호출해야 한다.
- B3: `Start()`에서 `Destroy(gameObject, 수명)`을 호출해야 한다.
- B4: `new Vector2(horizontalInput, verticalInput).normalized * moveSpeed`
- B6: 피해를 받은 뒤 일정 시간 무적이 되어 일정 간격으로만 피해를 받아야 한다.
- B7: `(playerTransform.position - transform.position).normalized`
