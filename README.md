# Lampkeeper (등불지기)

연료가 줄어드는 등불을 들고, 빛 안으로 들어오는 그림자 적을 쏘아 5웨이브를 버티는 탑다운 2D 게임입니다. 이 저장소에는 그 게임을 위한 Unity C# 프레임워크가 들어 있습니다.

| 문서 | 내용 |
|---|---|
| [프레임워크 가이드](Docs/FrameworkGuide.md) | 셋업 순서, 구조, 시스템별 사용법과 확장 예시, 코드 규칙, 문제 해결 |

## 프레임워크 구성

| 시스템 | 파일 |
|---|---|
| 게임 상태 머신 | `Core/GameManager.cs`, `Core/GameState.cs` |
| 이벤트 버스 | `Events/EventBus.cs`, `Events/GameEvents.cs` |
| 오브젝트 풀 | `Pooling/ComponentPool.cs` |
| ScriptableObject 데이터 | `Data/PlayerData.cs`, `EnemyData.cs`, `WaveData.cs`, `UpgradeData.cs` |
| 씬 로더 | `Core/SceneLoader.cs` |
| 입력 래퍼 (Input System) | `Input/InputReader.cs` |
| 플레이어 / 적 베이스 | `Gameplay/Common/Actor.cs`, `Gameplay/Player/PlayerController.cs`, `Gameplay/Enemies/EnemyBase.cs` |

경로는 모두 `Assets/_Project/Scripts/` 기준입니다.

## 빠른 시작

1. Unity 6 (6000.3.6f1 기준)으로 2D 프로젝트를 만들고 Input System 패키지를 설치합니다.
2. 이 저장소의 `Assets/_Project`를 새 프로젝트의 `Assets/` 아래로 복사합니다.
3. [프레임워크 가이드 2장](Docs/FrameworkGuide.md#2-셋업-순서)대로 씬·프리팹·데이터를 만듭니다.

## 확인 상태

- 컴파일: Unity 6000.3.6f1 + Input System 1.18.0 기준 오류 0
- Unity 에디터 실행·플레이: 확인하지 않음
