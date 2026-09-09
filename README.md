# Unity Core

Unity 프로젝트에서 반복적으로 사용되는 **기본 기능과 공통 유틸리티를 제공하는 경량 Runtime 패키지**입니다.

게임에 종속되지 않는 공통 기능을 프로젝트마다 반복해서 구현하지 않고 일관된 형태로 사용할 수 있도록 구성했습니다.

Unity Core는 다른 시스템이 의존할 수 있는 가장 낮은 수준의 공통 패키지를 지향하며, 특정 게임의 비즈니스 로직이나 상위 시스템에 대한 의존성을 갖지 않습니다.

## 주요 기능

### Singleton

일반 C# 클래스와 Unity `MonoBehaviour`에서 사용할 수 있는 Singleton 구현을 제공합니다.

#### `Singleton<T>`

`MonoBehaviour`가 필요하지 않은 일반 C# 객체에서 사용할 수 있습니다.

```csharp
public class GameService : Singleton<GameService>
{
}
```

```csharp
GameService.Instance;
```

#### `MonoSingleton<T>`

Unity Component를 Singleton으로 관리할 때 사용할 수 있습니다.

```csharp
public class GameManager : MonoSingleton<GameManager>
{
}
```

```csharp
GameManager.Instance;
```

다음과 같은 Unity 환경의 객체 생명주기를 고려합니다.

* 기존 Instance 자동 탐색
* Instance가 존재하지 않을 경우 자동 생성
* 중복 Instance 감지
* `DontDestroyOnLoad`를 통한 Scene 간 유지
* Singleton Instance 명시적 제거
* Application 종료 시 생명주기 처리

---

### Extensions

Unity 프로젝트에서 반복적으로 사용하는 Extension Method를 제공합니다.

현재 다음 기능을 포함합니다.

* Unity Object의 특성을 고려한 Null 검사

---

### Utilities

프로젝트 전반에서 사용할 수 있는 작은 공통 기능을 제공합니다.

현재 다음 기능을 포함합니다.

* Inspector에서 값을 확인하되 수정할 수 없도록 하는 `ReadOnly` Attribute
* 공통 Update 대상에 사용할 수 있는 `IUpdatable` Interface

---

## 프로젝트 구조

```text
Runtime/
├── Extensions/
│   └── NullCheckExtension.cs
│
├── Singleton/
│   ├── Singleton.cs
│   └── MonoSingleton.cs
│
├── Utils/
│   └── ReadOnlyAttribute.cs
│
├── IUpdatable.cs
└── UnityCore.asmdef
```

## 설계 방향

Unity Core는 많은 기능을 제공하는 범용 Framework를 목표로 하지 않습니다.

여러 프로젝트에서 반복적으로 필요하고 게임 로직과 독립적으로 사용할 수 있는 기능만을 포함하는 것을 원칙으로 합니다.

### 최소한의 의존성

Core 계층은 게임 시스템이나 다른 상위 패키지에 의존하지 않습니다.

```text
Game Systems
     │
     ├── UI Binding
     ├── DataTable
     ├── Asset Management
     │
     └──────────────┐
                    ↓
                Unity Core
                    ↓
                  Unity
```

이를 통해 Unity Core를 다른 프로젝트에서도 독립적으로 사용할 수 있도록 구성합니다.

### 작은 API

단순히 편의를 위한 기능을 계속 추가하기보다 여러 프로젝트에서 반복적으로 필요한 기능만 Core에 포함하는 것을 지향합니다.

### Unity 생명주기 고려

Unity Object의 Null 처리, `MonoBehaviour`의 생명주기, Scene 전환과 Application 종료 등 Unity Runtime의 특성을 고려하여 공통 기능을 구현합니다.

## 설치

Unity Package Manager에서 Git URL을 통해 설치할 수 있습니다.

```text
https://github.com/causeless8t/UnityCore.git
```

Unity Editor에서:

```text
Window
→ Package Manager
→ +
→ Add package from git URL...
```

을 선택한 후 위 주소를 입력합니다.

## 요구 사항

* Unity 2022 이상
* C#

## Package

```text
com.causeless3t.unitycore
```

## 향후 개선

* Runtime Test 추가
* Singleton 생명주기 테스트 강화
* Sample 프로젝트 추가
* Unity 최신 버전 호환성 검증
* Package 구조 개선

## License

MIT License
