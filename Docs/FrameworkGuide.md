# 프레임워크 가이드

`Assets/_Project/Scripts/`의 구조와 사용법, 확장 방법을 설명합니다. 모든 스크립트는 네임스페이스 `Lampkeeper`에 있습니다.

- 필요한 패키지: **Input System** 하나. 유료 에셋 없음.
- 확인 상태
  - Unity 6000.3.6f1 + Input System 1.18.0 기준으로 **컴파일만 확인**했습니다(에디터용/빌드용 둘 다 오류 0).
  - Unity 에디터 임포트와 플레이는 아직 확인하지 않았습니다. 씬·프리팹·에셋은 아래 셋업 순서대로 직접 만듭니다.
  - `Rigidbody2D.linearVelocity`를 쓰므로 Unity 6 이상이 필요합니다. 2022.3 이하에서는 `velocity`로 바꿔야 합니다.

---

## 1. 폴더 구조

```
Assets/_Project/
├─ Scenes/  Prefabs/  Data/  Art/  Audio/
└─ Scripts/
   ├─ Core/        GameState.cs, GameManager.cs, SceneLoader.cs
   ├─ Events/      EventBus.cs, GameEvents.cs
   ├─ Pooling/     ComponentPool.cs (IPoolable, PooledObject 포함)
   ├─ Data/        PlayerData, EnemyData, WaveData, UpgradeData
   ├─ Input/       InputReader.cs
   ├─ Gameplay/
   │  ├─ Common/   Actor.cs, Projectile.cs, FuelPickup.cs, FuelDropper.cs
   │  ├─ Player/   PlayerController.cs, PlayerStats.cs, Lantern.cs
   │  ├─ Enemies/  EnemyBase.cs, ChaserEnemy.cs
   │  └─ WaveSpawner.cs
   └─ UI/          DebugHud.cs, MainMenuView.cs (임시 OnGUI)
```

---

## 2. 셋업 순서

1. Unity Hub에서 **2D 템플릿**으로 새 프로젝트를 만듭니다.
2. Package Manager에서 **Input System**을 설치합니다. 재시작을 묻는 창이 뜨면 Yes를 누릅니다.
   - Project Settings → Player → Active Input Handling이 `Input System Package (New)` 또는 `Both`인지 확인합니다.
3. 이 저장소의 `Assets/_Project`를 새 프로젝트의 `Assets/` 아래로 복사합니다. 콘솔 에러가 0개인지 확인합니다.
4. **씬 3개**를 `Assets/_Project/Scenes`에 만들고 Build Profiles의 씬 목록에 이 순서로 넣습니다: `Boot`, `MainMenu`, `Game`. 이름은 `SceneNames`와 같아야 합니다.
5. **GameManager 프리팹**: 빈 오브젝트에 `GameManager`를 붙이면 `SceneLoader`와 `InputReader`가 자동으로 추가됩니다. 프리팹으로 저장하고 `Boot` 씬에만 둡니다.
6. `MainMenu` 씬: 빈 오브젝트에 `MainMenuView`를 붙입니다.
7. **데이터 에셋**: Project 창 우클릭 → Create → Lampkeeper → Player Data / Enemy Data / Wave Data / Upgrade Data
8. **프리팹** (스프라이트는 Create → 2D → Sprites의 Square / Circle)

   | 프리팹 | 구성 |
   |---|---|
   | Projectile | SpriteRenderer + CircleCollider2D(**Is Trigger**) + Rigidbody2D(**Kinematic**) + `Projectile` |
   | Enemy | SpriteRenderer + CircleCollider2D + Rigidbody2D(Dynamic, Gravity Scale 0, Freeze Rotation Z) + `ChaserEnemy` |
   | FuelPickup | SpriteRenderer + CircleCollider2D(**Is Trigger**) + `FuelPickup` |
   | Player | SpriteRenderer + CircleCollider2D + Rigidbody2D(Dynamic, Gravity Scale 0, Freeze Rotation Z) + `PlayerController` |
   | └ Lantern (자식) | `Lantern` + 그 아래 반투명 Circle 스프라이트(`lightVisual`에 연결) |

9. 에셋 연결: `PlayerData.projectilePrefab` ← Projectile, `EnemyData.prefab` ← Enemy, `WaveData.groups` ← EnemyData
10. `Game` 씬
    - Player를 배치하고 `PlayerController.data`, `lantern`, `Lantern.data`를 연결합니다. Camera를 비워 두면 `Camera.main`(Orthographic)을 씁니다.
    - 빈 오브젝트에 `WaveSpawner`를 붙이고 waves, 스폰 위치 Transform 여러 개, player를 연결합니다.
    - 빈 오브젝트에 `FuelDropper`(pickupPrefab 연결), `DebugHud`(player 연결)를 붙입니다.
11. `Boot` 씬에서 Play → Start → 웨이브 시작. Esc로 일시정지합니다.

> Game 씬에서 바로 테스트하려면 GameManager 프리팹을 Game 씬에도 임시로 두세요. Boot에서 시작하면 중복 인스턴스는 자동으로 제거되고, Game 씬에서 시작하면 바로 `Playing` 상태가 됩니다.

---

## 3. 전체 구조

```
GameManager (Boot 씬, DontDestroyOnLoad)
 ├─ SceneLoader   비동기 씬 로드
 ├─ InputReader   Move / Point / Fire / Pause
 └─ 상태 머신

Game 씬
 ├─ PlayerController ─ Lantern          (탄 풀 보유)
 ├─ WaveSpawner                         (적 풀 보유)
 ├─ FuelDropper                         (연료 풀 보유)
 └─ DebugHud

모든 시스템은 서로 직접 부르는 대신 EventBus로 알린다.
```

### 이벤트 흐름 예시
| 일 | 보내는 쪽 | 이벤트 | 받는 쪽 |
|---|---|---|---|
| 적 사망 | `EnemyBase.Die` | `EnemyKilledEvent` | `WaveSpawner`(남은 수), `FuelDropper`(드롭) |
| 웨이브 종료 | `WaveSpawner` | `WaveClearedEvent` | `GameManager` → WaveClear 또는 Victory |
| 플레이어 사망 | `PlayerController.Die` | `PlayerDiedEvent` | `GameManager` → GameOver |
| 연료 0 | `Lantern` | `LanternEmptiedEvent` | `GameManager` → GameOver |
| HP/연료 변경 | Player, Lantern | `PlayerHealthChangedEvent`, `LanternFuelChangedEvent` | HUD |
| 상태 변경 | `GameManager` | `GameStateChangedEvent` | `WaveSpawner`(다음 웨이브 시작) |

---

## 4. 시스템별 사용법

### 4.1 GameManager / 상태 머신
상태: `Boot, MainMenu, Loading, Playing, Paused, WaveClear, GameOver, Victory`

```
Boot → Loading → MainMenu → Loading → Playing ⇄ Paused
                                       Playing → WaveClear → Playing
                                       Playing → GameOver / Victory → Loading
```

- 상태는 **반드시 `GameManager.Instance.ChangeState(...)`로만** 바꿉니다. 허용되지 않은 전환은 경고를 남기고 무시합니다.
- 전환 규칙은 `CanTransition`, 진입 시 처리는 `OnEnterState`에 있습니다.
- `Playing`일 때만 `Time.timeScale = 1`이고 게임 입력이 켜집니다. `Paused / WaveClear / GameOver / Victory`에서는 0입니다.
- UI 버튼에서 부르는 API: `StartGame()`, `GoToMainMenu()`, `TogglePause()`, `ContinueAfterWaveClear()`, `QuitGame()`

**상태 추가 예시 (예: 튜토리얼)**
1. `GameState`에 `Tutorial` 추가
2. `CanTransition`에 들어오고 나가는 규칙 추가
3. 필요하면 `OnEnterState`에서 timeScale·입력 처리
4. 다른 시스템은 `GameStateChangedEvent`를 구독해 반응

### 4.2 EventBus
```csharp
// 1) GameEvents.cs에 이벤트 정의 (struct + IGameEvent)
public readonly struct ScoreChangedEvent : IGameEvent
{
    public readonly int Score;
    public ScoreChangedEvent(int score) { Score = score; }
}

// 2) 보내기
EventBus<ScoreChangedEvent>.Publish(new ScoreChangedEvent(score));

// 3) 받기: OnEnable에서 구독, OnDisable에서 해제
private void OnEnable()  => EventBus<ScoreChangedEvent>.Subscribe(OnScore);
private void OnDisable() => EventBus<ScoreChangedEvent>.Unsubscribe(OnScore);
private void OnScore(ScoreChangedEvent e) { /* ... */ }
```
- 해제를 빼먹으면 씬이 바뀐 뒤 파괴된 오브젝트가 호출되어 에러가 납니다.
- 이벤트를 struct로 만들면 보낼 때마다 GC 할당이 생기지 않습니다.

### 4.3 오브젝트 풀
```csharp
private ComponentPool<Projectile> _pool;

private void Awake() => _pool = new ComponentPool<Projectile>(prefab, prewarm: 30);

// 꺼내기
Projectile p = _pool.Get(position, Quaternion.identity);

// 돌려주기: 어디서든 이 한 줄. 풀 소속이 아니면 Destroy로 처리됨
PooledObject.Despawn(gameObject);
```
- 꺼내고 돌려줄 때 초기화가 필요하면 `IPoolable`(`OnSpawned`, `OnDespawned`)을 구현합니다.
- 같은 오브젝트를 두 번 돌려줘도 한 번만 처리됩니다.
- 풀이 만든 오브젝트는 현재 씬에 속해 씬이 바뀌면 함께 사라집니다. 풀은 그 오브젝트를 쓰는 쪽(플레이어, 스포너)이 들고 있습니다.

### 4.4 ScriptableObject 데이터
| 에셋 | 내용 |
|---|---|
| `PlayerData` | HP, 이동 속도, 무적 시간, 탄 프리팹·속도·피해, 연료·빛 반경 |
| `EnemyData` | 이름, 프리팹, HP, 속도, 접촉 피해, 연료 드롭 확률·양 |
| `WaveData` | 시작 지연, 스폰 그룹 목록(적, 수, 간격). 그룹은 위에서부터 순서대로 |
| `UpgradeData` | 이름, 설명, 종류(MaxHealth/MoveSpeed/FireRate/Damage/MaxFuel), 값 |

- 플레이 중에 SO 값을 코드로 바꾸지 마세요. 에디터에서는 플레이를 멈춰도 바뀐 값이 에셋에 남습니다.
- 업그레이드는 런타임 복사본 `PlayerStats`에 적용됩니다(`PlayerController.ApplyUpgrade`).

### 4.5 SceneLoader
```csharp
GameManager.Instance.Scenes.Load(SceneNames.Game, onLoaded: () => { /* 로드 완료 후 */ });
```
- `IsLoading`, `Progress`(0~1)로 로딩 화면을 만들 수 있습니다.
- 보통은 직접 부르지 않고 `GameManager.StartGame()` / `GoToMainMenu()`를 씁니다.

### 4.6 InputReader
| 속성/이벤트 | 기본 바인딩 |
|---|---|
| `Move` (Vector2, 길이 최대 1) | WASD, 방향키, 게임패드 왼쪽 스틱 |
| `PointerScreenPosition` | 마우스 위치 |
| `FireHeld` | 마우스 왼쪽, 게임패드 오른쪽 트리거 |
| `PausePressed` (이벤트) | Esc, 게임패드 Start |

- `Move`와 `FireHeld`는 `Playing` 상태에서만 값이 들어옵니다.
- 게임 코드에서 `Keyboard.current`, `Mouse.current`를 직접 읽지 않습니다. 새 입력은 `InputReader`에 액션을 추가해서 씁니다.
- 액션은 코드로 정의했습니다. `.inputactions` 에셋으로 바꾸려면 `InputReader` 내부만 고치면 됩니다.

### 4.7 Actor / 플레이어 / 적
```
Actor (HP, TakeDamage, Heal, Die)
 ├─ PlayerController   이동, 조준, 사격, 무적 시간, 업그레이드
 └─ EnemyBase          Init, 이동(GetDesiredVelocity), 접촉 피해, 풀 반환
     └─ ChaserEnemy    플레이어를 향해 직진
```
- 등불 규칙: 적은 등불 반경 안에 있을 때만 탄에 맞습니다(`EnemyBase.CanBeHit`).
- 접촉 피해는 매 물리 프레임 들어가지만, 플레이어 무적 시간 동안은 무시됩니다.

**새 적 만들기 예시 (빛 반경 바깥을 도는 적)**
```csharp
public class CirclerEnemy : EnemyBase
{
    [SerializeField] private float orbitBias = 0.7f;

    protected override Vector2 GetDesiredVelocity()
    {
        Vector2 toTarget = ((Vector2)Target.position - Body.position).normalized;
        Vector2 side = new Vector2(-toTarget.y, toTarget.x);
        bool lit = Lantern != null && Lantern.Contains(Body.position);
        Vector2 dir = lit ? toTarget : Vector2.Lerp(toTarget, side, orbitBias).normalized;
        return dir * Data.moveSpeed;
    }
}
```
새 프리팹에 붙이고, 새 `EnemyData`의 prefab에 연결한 뒤 `WaveData`에 넣으면 됩니다. 스포너 코드는 고치지 않아도 됩니다.

### 4.8 WaveSpawner
- `Playing` 상태로 들어올 때(Loading 또는 WaveClear에서) 다음 웨이브를 시작합니다. Paused에서 돌아올 때는 이어서 진행합니다.
- 스폰이 끝나고 살아 있는 적이 0이 되면 `WaveClearedEvent`를 보냅니다. 마지막 웨이브면 `IsLastWave = true`입니다.
- `WaitForSeconds`를 쓰므로 일시정지 중에는 스폰도 멈춥니다.

### 4.9 임시 UI
- `DebugHud`, `MainMenuView`는 OnGUI로 만든 임시 화면입니다. UGUI 또는 UI Toolkit으로 교체해서 씁니다.
- 새 HUD도 값을 **이벤트로만** 받습니다. Player를 매 프레임 조회하지 않습니다.

---

## 5. 코드 규칙

- 상태는 `GameManager.ChangeState`로만 바꿉니다.
- 다른 시스템에 알릴 일은 직접 참조 대신 이벤트(`GameEvents.cs`에 struct 추가)로 보냅니다.
- 반복 생성되는 오브젝트(탄, 적, 이펙트)는 `ComponentPool`을 씁니다.
- 수치는 코드에 숫자로 박지 않고 SO에 둡니다.
- 장치 입력은 `InputReader` 밖에서 읽지 않습니다.
- 인스펙터에 노출할 필드는 `public` 대신 `[SerializeField] private`를 씁니다(SO 데이터 클래스 제외).

---

## 6. 문제 해결

| 증상 | 확인할 것 |
|---|---|
| 탄이 적을 통과함 | 적이 빛 반경 밖에 있음(의도된 규칙) / Projectile Collider가 Is Trigger가 아님 / Rigidbody2D 없음 |
| 입력이 안 먹음 | Active Input Handling 설정 / 상태가 `Playing`인지 (DebugHud의 State) |
| "GameManager가 없습니다" 경고 | Boot 씬부터 실행했는지 / 테스트용 GameManager 프리팹을 씬에 뒀는지 |
| 씬 로드 실패 에러 | Build Profiles 씬 목록과 `SceneNames` 철자 |
| 재시작 후 이벤트가 두 번 호출됨 | `OnDisable` 구독 해제 누락 / Domain Reload를 끈 경우 `EventBus<T>.Clear()` |
| 웨이브가 시작되지 않음 | `WaveSpawner.waves`, `spawnPoints` 비어 있지 않은지 / 콘솔 경고 |
