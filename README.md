# unity-vrchat-scripts

Unity util scripts for VRChat (Avatar/World)

Unity で VRChat のアバターやワールドを作るときに便利な自分用スクリプト集

## Contents

```
Editor/
├── avatar/ -- アバター制作に便利なスクリプト
│  ├── AnimationCombiner.cs -- 複数の AnimationClip を時間間隔を指定して1つに結合する (Tools > cympfh > AnimationCombiner)
│  └── ParentConstraintChecker.cs -- ParentConstraint を使っていて親オブジェクトを指定していないものを検出 (Tools > cympfh > Check Inactive Parent Constraints)
└── world -- ワールド制作に便利なスクリプト（予定）
```

## インストール (VCC / ALCOM)

VPM リポジトリとして配布しています。以下のいずれかの方法で VCC (VRChat Creator Companion) / ALCOM に追加してください。

- ワンクリックで追加: [`vcc://vpm/addRepo?url=https://cympfh.cc/vpm/index.json`](vcc://vpm/addRepo?url=https://cympfh.cc/vpm/index.json)
- 手動で追加: VCC/ALCOM の Settings > Packages > Add Repository に `https://cympfh.cc/vpm/index.json` を入力

追加後、プロジェクトの Manage Packages から `cympfh's VRChat Editor Scripts` をインストールできます。

## LICENSE

MIT License です。煮るなり焼くなり好きにしてください。
