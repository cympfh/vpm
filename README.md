# cc.cympfh.vrchat-scripts

Unity util scripts for VRChat (Avatar/World)

Unity で VRChat のアバターやワールドを作るときに便利な自分用スクリプト集

## Components

それぞれ `Tools > cympfh` メニューから実行できます

### avatar/ -- アバター制作に便利なスクリプト

- AnimationCombiner.cs
    - 複数の AnimationClip を時間間隔を指定して1つに結合する
- ParentConstraintChecker.cs
    - ParentConstraint を使っていて親オブジェクトを指定していないものを検出

### common/ -- Avatar/World 共通のスクリプト

- Assertion.cs
    - GameObject に直接アタッチする Component（`Add Component > cympfh/Assertion`）
    - オブジェクトの active/inactive、ParentConstraint の設定漏れなどの条件をあらかじめ登録しておき、VRC のビルド開始時に自動検証。違反があればビルドを中止する
    - ビルドすると自動的に除去されるため、アップロードされるアバター/ワールドには残らない

## インストール (VCC / ALCOM)

VPM リポジトリとして配布しています。以下のいずれかの方法で VCC (VRChat Creator Companion) / ALCOM に追加してください。

- ワンクリックで追加

```
vcc://vpm/addRepo?url=https://cympfh.cc/vpm/index.json
```

- 手動で追加: VCC/ALCOM の Settings > Packages > Add Repository に以下を入力

```
https://cympfh.cc/vpm/index.json
```

追加後、プロジェクトの Manage Packages から `cympfh's VRChat Editor Scripts` をインストールできます。

## LICENSE

MIT License です。煮るなり焼くなり好きにしてください。
