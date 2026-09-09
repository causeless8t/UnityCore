# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2024-12-11
### Added
- IUpdatable
  - OnUpdate를 구현하기 위한 인터페이스
### Modified
- Singleton
  - OnUpdatable 구현 추가

## [1.0.0] - 2023-09-26
### Added
- NullCheckExtension
  - Unity Object Fake Null 체크를 위한 Extension
- MonoSingleton
  - MonoBehaviour를 상속하는 싱글턴 클래스 정의
- Singleton
  - 싱글턴 클래스 정의
- ReadOnlyAttribute
  - Inspector에서 ReadOnly Attribute를 사용하기 위한 Util 클래스
