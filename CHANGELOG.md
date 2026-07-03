# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/ja/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-07-03

### Added
- Assertion コンポーネント（`Runtime/Assertion.cs`, `Editor/common/AssertionEditor.cs`, `Editor/common/AssertionBuildValidator.cs`）— GameObject の active/inactive、ParentConstraint の設定漏れなどの条件を登録し、VRChat ビルド開始時に自動検証。違反があればビルドを中止する。`IEditorOnly` によりビルド後は自動的に除去される
- `package.json` の `vpmDependencies` に `com.vrchat.base` を追加（ビルドフック利用のため）

(git commit: f3853be)

## [1.0.2] - 2026-07-03

### Changed
- README のコンテンツ構成を見直し、インストール手順のコードブロックを整理

(git commit: 36f4b24)

## [1.0.1] - 2026-07-03

### Fixed
- VPMリスティングのURLを `vpm.cympfh.cc` から `cympfh.cc/vpm` に修正

(git commit: 2b6d6c4)

## [1.0.0] - 2026-07-03

### Added
- VPM (VRChat Package Manager) パッケージ `cc.cympfh.vrchat-scripts` として配布開始。リポジトリ自身が VPM リスティング（`https://vpm.cympfh.cc/index.json`）をホスト
- `.github/workflows/release.yml` — タグpushで draft の GitHub Release を自動作成
- `.github/workflows/build-listing.yml` — Release公開時に `index.json` を自動生成・デプロイ
- README にVCC/ALCOMからのインストール手順を追加
- AnimationCombiner (`Editor/avatar/AnimationCombiner.cs`) — 複数の `AnimationClip` を `ReorderableList` でドラッグ並べ替え可能な状態で保持し、一定間隔でシーケンシャルに結合する `EditorWindow`
  - float カーブ・`ObjectReferenceCurve`・アニメーションイベントを保持したまま結合
  - `_meta.json` による設定の保存・復元（D&Dで復元可能）
  - 既存 `.anim` への上書き保存時にGUIDを保持
- ParentConstraintChecker (`Editor/avatar/ParentConstraintChecker.cs`) — シーン内の非アクティブな `ParentConstraint` をConsoleに列挙する静的メニューツール

(git commit: 99d8fa3)
