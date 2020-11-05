PMXExporter v0.2.4 by Furia

・概要
VRMprefabをPMXファイルに出力することができます。

・利用許諾
同梱物全てにおいて

CC BY-NC 4.0を適用します。LICENSE.txtを参照ください。

UniVRM : MITlicense
author
 DWANGO Co., Ltd. for UniVRM
 ousttrue for UniGLTF, UniHumanoid
 Masataka SUMI for MToon
libraryを参照しています。

MMDataIO : MITlicense
author
 Zyando
libraryを改変・利用しています。

モーフリネームテーブル
author
 側近Q

敬称略

・license設定の解説
NC : PMXフォーマットドキュメントを参考にlibrary改修他を行っているため、本プラグイン自体、および利用時に金銭の授受が発生することを許諾できません。

・出力されるPMXファイルのライセンス等
プログラムの著作権としての性質及び、リソースはVRMprefabを基に得られるものであるため、出力されるPMXファイルへ本プラグイン自体のライセンスの影響は一切発生しません。
よって出力されたPMXファイルは元となったVRMファイルの複製翻案となり、VRMファイルへ設定されたlicenseに基づきPMXファイルのライセンスが決定されます。

改変を伴う機能であるため、CC_??_NDなど改変禁止指定されたVRMファイルのPMX変換目的での利用は本プラグインのlicenseと適合しません。よって利用できません注意ください。


・利用方法

◆PMXExporter
UniVRMを用いてAssetにVRMをドロップしVRMprefabを作成します。

hierarchyに配置したVRMのprefabへPMXExporterをスクリプトコンポーネントとして付与してください。

設定
・Replace Bone Name
一部基幹ヒューマノイドボーンをMMD標準ボーン名に置換します。

・Convert Armatuar Need Replace Bone Name
Aスタンス化、左右足IK、両目ボーン、全ての親ボーン、センターボーン及び親子構造、初期サイズ（10倍）、向き反転
を行います。

・Replace Morph Name
VRoid標準のモーフについて、MMD準拠名へ置換します。

右クリックよりExportを選択。
保存先を決定すると出力されます。

※出力されるファイルの留意事項
出力したファイルはMMDでは直接読み込みできません。
かならずPMXEditorで最低限読み込み＆保存処理を行ってください。
テクスチャの透過順序は考慮していないのでPMXEditorで修正してください。
後述の機能にも目を通してください。

◆PMXExporterLite
ほぼ同様です。
VRM特有の情報を必要としません。Unity上のMeshをとにかくPMXにできます。


・機能
IKないこともないです。物理ないです。モーフはリネームないかもしれません。

MeshRendererは、親のゲームオブジェクトをボーンとして認識しWeightを100％付与します。
パーツ事に個別のボーンに分たので、親指定を追従させたいボーンにすれば比較的簡単にアクセサリとして書いたパーツを分離できるかも。

テクスチャはメインテクスチャのみ考慮します。
xx.Texturesフォルダごとpmxと同階層に"tex"とリネームして配置するとテクスチャが読めます。

テクスチャの透過順序は考慮していないのでPMXEditorで修正してください。

・更新履歴
v0.1
初版

v0.2
ボーン構造その他Aスタンス化等のMMD標準構造化を実装

v0.2.1
修正:v0.2追加機能においてモーフへの変形が適用されていなかった

v0.2.2
修正:MeshRenderer由来の頂点にボーン位置が反映されていなかった
修正:モーフ翻訳に一部誤植

v0.2.3
修正:モーフ翻訳に一部誤植

v0.2.4
修正:各種データのインデックスサイズ最適化
当修正によりPMXEditorでの再保存が不要でMMDに読み込めるようになります