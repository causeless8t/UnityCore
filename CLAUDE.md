# CLAUDE.md

이 문서는 Claude Code가 Unity Core 저장소에서 작업할 때 따라야 하는 프로젝트 지침입니다.

## 프로젝트 개요

Unity Core는 Unity 프로젝트에서 반복적으로 사용되는 기본 기능과 공통 유틸리티를 제공하는 경량 Runtime 패키지입니다.

특정 게임의 비즈니스 로직을 포함하지 않으며, 다른 Unity 패키지와 게임 시스템이 의존할 수 있는 가장 낮은 수준의 공통 계층을 지향합니다.

주요 기능:

* 일반 C# 객체를 위한 `Singleton<T>`
* Unity Component를 위한 `MonoSingleton<T>`
* Unity Object 관련 Extension
* 공통 Runtime Interface
* 작은 범용 Utility

이 프로젝트의 목표는 많은 기능을 제공하는 Framework를 만드는 것이 아닙니다.

여러 프로젝트에서 반복적으로 사용되고 게임 로직과 독립적인 최소한의 기능만 Core에 포함합니다.

---

# Architecture Principles

## 1. Core는 상위 시스템에 의존하지 않는다

의존성 방향은 항상 다음과 같아야 합니다.

```text
Game Systems
     │
     ├── UI Binding
     ├── DataTable
     ├── Asset Management
     │
     ▼
  Unity Core
     │
     ▼
    Unity
```

Unity Core에서 다음 시스템을 직접 참조하지 않는다.

* 게임 콘텐츠
* UI 시스템
* AssetManager
* DataTable
* Network 시스템
* 특정 프로젝트의 Manager 또는 Service
* 게임별 ScriptableObject

새로운 기능을 추가할 때 상위 시스템에 대한 의존성이 필요하다면 Unity Core에 포함하기 적절한 기능인지 먼저 검토한다.

---

## 2. 기능 추가보다 작은 API를 우선한다

편의를 위해 Utility를 무분별하게 추가하지 않는다.

새로운 기능을 추가하기 전에 다음을 확인한다.

1. 여러 Unity 프로젝트에서 반복적으로 필요한 기능인가?
2. 특정 게임이나 시스템에 종속되지 않는가?
3. Unity Core에 포함하지 않고 독립적인 패키지로 구성하는 것이 더 적절하지 않은가?
4. 기존 API를 이용해 충분히 해결할 수 없는가?

위 조건을 만족하지 않는 기능은 Core에 추가하지 않는 것을 우선한다.

---

## 3. Unity 생명주기를 명시적으로 고려한다

`MonoBehaviour` 또는 `UnityEngine.Object`를 다루는 코드에서는 일반 C# 객체와 다른 Unity의 동작을 고려한다.

특히 다음 상황을 확인한다.

* `Awake`
* `OnDestroy`
* `OnApplicationQuit`
* Scene 전환
* `DontDestroyOnLoad`
* Unity Object의 overloaded null 비교
* Domain Reload
* 중복 Component 생성
* Runtime 객체 파괴

Unity lifecycle callback을 구현할 때 상속 클래스가 동일한 callback을 선언함으로써 부모의 처리가 누락되지 않는지 확인한다.

필요한 확장 지점은 lifecycle callback 자체를 상속하도록 요구하기보다 별도의 `protected virtual` hook을 제공하는 방식을 우선한다.

예:

```csharp
private void Awake()
{
    // Core lifecycle handling

    OnSingletonAwake();
}

protected virtual void OnSingletonAwake()
{
}
```

---

# Singleton Rules

## Singleton<T>

`Singleton<T>`는 Unity Component가 필요하지 않은 일반 C# 서비스에 사용한다.

다음 동작을 유지해야 한다.

* 최초 접근 시 Instance 생성
* 여러 번 접근해도 동일한 Instance 반환
* 명시적인 Instance 생성 지원
* Instance 존재 여부 확인
* Instance 제거
* 필요할 경우 `IDisposable.Dispose()` 호출

Singleton 생성 규칙은 가능한 한 컴파일 타임에 표현하는 것을 우선하고 불필요한 Reflection 사용은 피한다.

---

## MonoSingleton<T>

`MonoSingleton<T>`는 Unity `MonoBehaviour`의 Singleton 생명주기를 관리한다.

다음 동작을 보장해야 한다.

* 기존 Component가 존재하면 해당 Instance 사용
* Instance가 없으면 새로운 GameObject와 Component 생성
* 여러 번 접근해도 동일한 Instance 반환
* 중복 Component가 생성되어도 기존 정상 Instance 유지
* Scene 전환 이후 Instance 유지
* Application 종료 과정에서 불필요한 재생성 방지
* 정상적인 Singleton 파괴와 외부의 비정상적인 GameObject 파괴를 구분
* Instance 제거 이후 static reference가 남지 않음

특히 중복 Instance를 제거하는 과정에서 기존 정상 Instance의 reference를 제거해서는 안 된다.

---

# Coding Guidelines

## 가독성을 우선한다

복잡한 한 줄 코드보다 의도가 명확한 코드를 우선한다.

불필요한 abstraction을 만들지 않는다.

이 프로젝트는 작은 Core 패키지이므로 확장 가능성만을 이유로 interface, factory, wrapper 등의 계층을 추가하지 않는다.

---

## Naming

C# 및 Unity의 일반적인 naming convention을 따른다.

```text
Class / Struct / Enum    PascalCase
Method                   PascalCase
Property                 PascalCase
Private Field            _camelCase
Parameter                camelCase
Local Variable           camelCase
```

Boolean은 가능한 한 상태나 질문의 의미가 드러나도록 작성한다.

예:

```csharp
_isApplicationQuitting
_isDestroying
InstanceExists
```

---

## 접근 제한자

가능한 한 가장 제한적인 접근 범위를 사용한다.

외부에서 사용할 필요가 없는 API를 `public`으로 만들지 않는다.

상속 클래스의 확장 지점은 `protected virtual`을 사용한다.

---

## Nullable

프로젝트의 nullable reference type 설정을 존중한다.

`null!`을 단순히 compiler warning을 제거하기 위한 목적으로 사용하지 않는다.

nullable warning이 발생하면 실제 null 가능성을 먼저 검토한다.

---

# Testing

기존 동작을 변경하거나 버그를 수정할 때는 가능한 경우 먼저 해당 동작을 재현하는 테스트를 작성한다.

## EditMode Tests

일반 C# 로직은 EditMode Test를 우선한다.

예:

* `Singleton<T>`
* Utility
* Extension
* 순수 데이터 처리

## PlayMode Tests

Unity lifecycle이 필요한 기능은 PlayMode Test를 사용한다.

예:

* `MonoSingleton<T>`
* GameObject 생성/제거
* `Awake`
* `OnDestroy`
* Scene lifecycle
* `DontDestroyOnLoad`

---

## Singleton 테스트에서 유지해야 하는 핵심 계약

`Singleton<T>` 변경 시 최소한 다음 동작을 검증한다.

```text
First Access
    → Instance Created

Multiple Access
    → Same Instance

Destroy
    → Instance Cleared

Access After Destroy
    → New Instance

IDisposable
    → Dispose Called
```

---

## MonoSingleton 테스트에서 유지해야 하는 핵심 계약

```text
No Instance
    → GameObject Created

Existing Component
    → Existing Instance Used

Duplicate Component
    → Duplicate Destroyed
    → Original Instance Preserved

Destroy Singleton
    → Instance Cleared

Application Quit
    → Instance Not Recreated
```

버그 수정 시 해당 버그를 재현하는 regression test를 남긴다.

---

# Change Workflow

코드를 수정하기 전에 다음 순서로 작업한다.

### 1. Understand

관련 코드와 테스트를 먼저 읽고 현재 동작을 파악한다.

호출부를 검색하여 변경이 다른 API에 미치는 영향을 확인한다.

### 2. Plan

변경 범위를 최소화한다.

기존 API를 변경해야 한다면 이유와 영향을 먼저 확인한다.

### 3. Test

동작 변경이나 버그 수정이라면 가능한 경우 실패하는 테스트를 먼저 작성한다.

### 4. Implement

테스트를 통과하는 가장 단순한 구현을 작성한다.

불필요한 리팩터링을 함께 수행하지 않는다.

### 5. Verify

관련 EditMode / PlayMode Test를 실행한다.

컴파일 오류와 warning을 확인한다.

### 6. Review

작업 완료 전 다음을 확인한다.

* Core의 책임 범위를 벗어난 기능이 추가되지 않았는가?
* 새로운 불필요한 dependency가 생기지 않았는가?
* Unity lifecycle edge case를 고려했는가?
* 기존 public API가 의도치 않게 변경되지 않았는가?
* 테스트가 구현 세부사항이 아니라 observable behavior를 검증하는가?

---

# Refactoring Rules

리팩터링 시 기존 public API의 호환성을 우선한다.

API 변경이 필요한 경우 단순히 코드가 깔끔해진다는 이유만으로 breaking change를 만들지 않는다.

다음 상황에서는 리팩터링을 적극적으로 고려한다.

* 동일 책임이 여러 곳에 중복됨
* lifecycle 처리가 모호함
* 이름과 실제 동작이 일치하지 않음
* Reflection을 compile-time constraint로 대체할 수 있음
* nullable suppression이 실제 문제를 숨기고 있음
* 테스트하기 어려운 구조가 실제 책임 분리 문제를 나타냄

---

# Documentation

Public API의 동작이 직관적이지 않은 경우 XML documentation을 작성한다.

README에는 구현 세부사항보다 다음 내용을 우선한다.

* 이 패키지가 해결하는 문제
* 주요 기능
* 사용 방법
* 설계 방향
* 설치 방법

코드 변경으로 README의 설명이 더 이상 정확하지 않다면 README도 함께 수정한다.

---

# Do Not

다음 작업은 명확한 필요성이 없는 한 수행하지 않는다.

* Core에 게임별 기능 추가
* 새로운 외부 package dependency 추가
* 기존 public API의 임의 변경
* 필요하지 않은 Design Pattern 도입
* 모든 기능을 interface로 추상화
* 테스트 없이 Singleton lifecycle 변경
* unrelated code의 대규모 formatting
* 요청받지 않은 파일 전체 리팩터링
* compiler warning을 숨기기 위한 무분별한 suppression

---

# Definition of Done

작업 완료 전에 다음을 확인한다.

* [ ] 코드가 정상적으로 컴파일된다.
* [ ] 관련 EditMode Test가 통과한다.
* [ ] 관련 PlayMode Test가 통과한다.
* [ ] 새로운 warning이 발생하지 않는다.
* [ ] public API의 의도치 않은 변경이 없다.
* [ ] 새로운 dependency가 추가되지 않았다.
* [ ] Unity Core의 책임 범위를 유지한다.
* [ ] 필요한 경우 README / documentation을 갱신했다.
