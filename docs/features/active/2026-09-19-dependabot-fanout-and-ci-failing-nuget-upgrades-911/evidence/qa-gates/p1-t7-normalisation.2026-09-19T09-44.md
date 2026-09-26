# P1-T7 — One-time normalisation of every manifest and application configuration

Timestamp: 2026-09-19T12-54

Command: `pwsh -NoProfile -Command` driving `Invoke-ManifestNormalization` from
`scripts/dependencies/PackageGraph.psm1` with a `git ls-files "*/packages.config" "*/app.config"`
discovery delegate, a `[System.IO.File]::ReadAllText` reader delegate and a
`[System.IO.File]::WriteAllText` writer delegate using `UTF8Encoding($true)`; followed by
`git diff --numstat 734112ed25bba293cb074e71fee2286bc3b72fae -- "*/packages.config" "*/app.config"`,
`git diff --name-only <same ref and pathspec>` and
`git status --porcelain --untracked-files=all -- "*/packages.config" "*/app.config"`

EXIT_CODE: 0

## Byte-exactness, per gate rule 14

The rewrite is performed through `[System.IO.File]::WriteAllText`, never with `sed` through the
Bash tool. The writer supplies `UTF8Encoding($true)` so the byte-order mark every one of these 35
files already carries is preserved, and the renderer emits CRLF line endings, so no line-ending
rewrite is introduced.

## Examined counts, emitted by the normaliser itself

| Kind | Examined |
|---|---|
| `packages.config` | **18** |
| `app.config` | **17** |
| Total | **35** |

Those are the totals in the tree, so a shortfall would mean the discovery glob missed a file. None
occurred.

## Changed counts, measured from the hash-difference set

| Kind | Changed |
|---|---|
| `packages.config` | **17** |
| `app.config` | **17** |
| Total hash-difference set | **34** |

**The examined and changed counts legitimately differ, and today they do.** Exactly one file is
examined and left byte-identical: `SVGControl/packages.config`, whose SHA-256 is
`0FDB33F6F9FBECDE2C401C0E94DEAFCE6989911015701EB4D4B7A25E67B4BE27` before and after. That file
carries no wrapped `<package>` element and is already in canonical inline form, so the renderer
reproduces it exactly and it never enters the diff. A shortfall of the **changed** counts against
the examined totals is not a defect; a shortfall of the **examined** counts would be. P9-T12 reads
the changed counts recorded here rather than any literal.

## Content residual, asserted positively

The count of lines across the 35 files whose text is exactly `<package` or exactly
`<assemblyIdentity`:

| Point | Count |
|---|---|
| Before the run | **1193** |
| After the run | **0** |

The non-zero before-count is what makes the zero meaningful. A line-ending-only rewrite would have
left 1193 standing while still changing every file's hash and producing a 34-entry porcelain, which
is precisely the vacuous shape gate rule 14 exists to catch.

## `git diff --numstat` against `<MERGE_BASE>` `734112ed25bba293cb074e71fee2286bc3b72fae`

| Measure | Value |
|---|---|
| Files | **34** |
| Added lines | **1201** |
| Deleted lines | **6077** |

Both files kinds exist at the merge base, so the merge-base anchor is correct here. The deletion
total dominates because collapsing a reflowed element removes the continuation lines: a real
content change, not a no-op.

## `git diff --name-only` against the same ref

34 paths, listed below, **0 of them outside** the 35-member discovery set. `SVGControl/packages.config`
is the one member of the set that is absent, which is the byte-identical file.

```
QuickFiler.Test/app.config
QuickFiler.Test/packages.config
QuickFiler/app.config
QuickFiler/packages.config
SVGControl.Test/app.config
SVGControl.Test/packages.config
SVGControl/app.config
Tags.Test/app.config
Tags.Test/packages.config
Tags/app.config
Tags/packages.config
TaskMaster.Test/app.config
TaskMaster.Test/packages.config
TaskMaster/app.config
TaskMaster/packages.config
TaskTree.Test/app.config
TaskTree.Test/packages.config
TaskTree/app.config
TaskTree/packages.config
TaskVisualization.Test/app.config
TaskVisualization.Test/packages.config
TaskVisualization/app.config
TaskVisualization/packages.config
ToDoModel.Test/app.config
ToDoModel.Test/packages.config
ToDoModel/app.config
ToDoModel/packages.config
UtilitiesCS.Test/app.config
UtilitiesCS.Test/packages.config
UtilitiesCS/app.config
UtilitiesCS/packages.config
VBFunctions.Test/app.config
VBFunctions.Test/packages.config
VBFunctions/packages.config
```

## Porcelain companion, per gate rule 8

`git status --porcelain --untracked-files=all -- "*/packages.config" "*/app.config"` returns 34
entries, every one ` M`, and **0 of them outside** the 35-member set. The companion is what observes
any path the name-listing diff cannot see; there is none.

## Per-file SHA-256, before and after

| Path | Before | After | Changed |
|---|---|---|---|
| QuickFiler.Test/app.config | 737F8566D02B36EB7B6824F21FBF1CB270FE4F44235A08F79463D2BF414996C2 | C40630895531957FFCE72D518B11E987A947F3133A23B239366994A1CD6992D1 | yes |
| QuickFiler.Test/packages.config | 9D97FFBFDFF863E948BBC73DFF9E00467ECD90B35A71A56F26FAF74DE57775C7 | 534A43110021D660A0CB70922BFF0D2B8D7BAAC6A93D2558161CE49D3E336D57 | yes |
| QuickFiler/app.config | 066CCA04C89BBDF0ADE276823A0E77385A5CE5DF15EAB352C4531A886CA20ED2 | 23913A64949891DD2F702675A819E6D1FD579B1D9B8D2DB54308CCA2493E3640 | yes |
| QuickFiler/packages.config | DC2BD40292568DD91668559A0C944E8EF3ADB142F1B1EBB614C2F6C6E3D8A0F1 | 40E4D03623C474EC0ECDB9D9103190BEDC229E3CBEC1D3D1C29D755F3F8B30FE | yes |
| SVGControl.Test/app.config | 70C60B18EA76632D87CEF49BB760A40797CAE4FE29A4A5CC0B37CA8FDDB02ABF | BD58582AC8A9C7800DE38DC59A2AC97EB783E04B213A341315265EE98B0C6801 | yes |
| SVGControl.Test/packages.config | 03CF4BDA276928335BA04B2BA439E4A8ED2FA4D0382959E0E41DA8BEF3F91FCB | 8623BFB1E8F37DF4EFD1A2EA29DB1DFCF39C8830344F5800EBD18996949EA23B | yes |
| SVGControl/app.config | D56B62DFC859A05C2561A5B0EA339C03258569A402C152EE8DD92754B1A650B3 | E7A57220BCAF11A8823D929E7EFEB29D61E597B0B6C79E12873F80DD045B08D1 | yes |
| **SVGControl/packages.config** | 0FDB33F6F9FBECDE2C401C0E94DEAFCE6989911015701EB4D4B7A25E67B4BE27 | 0FDB33F6F9FBECDE2C401C0E94DEAFCE6989911015701EB4D4B7A25E67B4BE27 | **no — already canonical** |
| Tags.Test/app.config | 56501463F2601E97A14717C801FD847E34D0E2C78504FC0B055165E455E5BAF9 | DFE679F96CDB1F216009EAC8737E501217A616E8774D418F7760888610D1156D | yes |
| Tags.Test/packages.config | CAB13480CD2ACC39499C361358E72AB2F9E6ADA8F201525A88841FD484B3DFF3 | 428CC5EFB71B2C7F44A29FA6BA897818A72FAACB66B077E6DCEC6EBF3E110AE7 | yes |
| Tags/app.config | 279F56FA2C7CD1EFEFE9650A3B497D485F2062F2A9E383F903597F9230155DA2 | DF70C80CDC5CDFA5D4C3141C562B0EC15D5511FA74F4B53702A45F85C68778DA | yes |
| Tags/packages.config | FDA0FA6C58B9166DAF55E7F1111D4779321C5E261D8C7D26D54A83340E0DE9DA | 8930EA8D8A9FB04147F95E952ECEE321E554FA6681B605203F7FE681A53A67C5 | yes |
| TaskMaster.Test/app.config | 7573A6F7D3CF2CD06B70ED74F1FAB73C181FE99F517EC093358799961187FB0C | D36A9CF7E0D3C45251BA261F3AECB8366CC57F1AD0C92DC147F3F338C42DA671 | yes |
| TaskMaster.Test/packages.config | C6427CDB58904E9485CBCE1C8ED474D004F19BB51A288C1337E7D85AEBC35A99 | F5AFE63AA163BE62E89C2071B373A46EDB567F54C2F0C17DF235F5D6A0EEC677 | yes |
| TaskMaster/app.config | 0B0A408F0CA78811CCDC1AABF9659571E8F4145C4DD8DFE7DFD5A6C7D83B96F3 | 56BB3E5D37C58237B7FE7A5B2307A4C503ACF3D07E9AB630E664693B9DC34FEB | yes |
| TaskMaster/packages.config | DC05E68EA099317BB2B4FB4B9D088C6D86EE4F78C7C17C737ADA9E63D55AD222 | DE46C39711648E912B42160E16DD16BEBD433B2883EA3BBA4A81680072BC3A8C | yes |
| TaskTree.Test/app.config | 56501463F2601E97A14717C801FD847E34D0E2C78504FC0B055165E455E5BAF9 | DFE679F96CDB1F216009EAC8737E501217A616E8774D418F7760888610D1156D | yes |
| TaskTree.Test/packages.config | CAB13480CD2ACC39499C361358E72AB2F9E6ADA8F201525A88841FD484B3DFF3 | 428CC5EFB71B2C7F44A29FA6BA897818A72FAACB66B077E6DCEC6EBF3E110AE7 | yes |
| TaskTree/app.config | 39C601E1784B33AF2AB8590E58BD616510006D47D225302C104E42A8786E7E1F | A5BE9EA2DA0B1455E8FF94CDD94155F6E4DD1B48B54C20AA16F674122ACE326B | yes |
| TaskTree/packages.config | B1B21334554D8C6505CCA8AC4DB6A1DAFCEEFDEF81D8FF6B1106F571B7D9B65D | 89E24B7A6EF3FEAEAEB9DAA13D9A856D2133A9E17A96AA2126A3E77C45170417 | yes |
| TaskVisualization.Test/app.config | 5C5C71CED088232E534C45072F37AD0A26073D9BF0DC6166190F7ACC94F2CF3A | B8D9366B3579446C248E0212B0737F000DA075689648CEE72A1C5805F831708D | yes |
| TaskVisualization.Test/packages.config | CAB13480CD2ACC39499C361358E72AB2F9E6ADA8F201525A88841FD484B3DFF3 | 428CC5EFB71B2C7F44A29FA6BA897818A72FAACB66B077E6DCEC6EBF3E110AE7 | yes |
| TaskVisualization/app.config | 82DBA40CD423732571819813B68E8E7407C5F7FF035FEA68F879C21C9C394EFE | 574350FF1AD4DE6B01A85D0A8EE249C5E8C7ADF8D8E4C3EBAB43184813ED60B7 | yes |
| TaskVisualization/packages.config | B5866DF548209C72FAA1CD360DB56CEFFEAA011696C32F1056D345004D54725C | 26F42481A9C3C624BFFB2AFA0FEFB81E620C880481CB0D585874557EA7AF69F5 | yes |
| ToDoModel.Test/app.config | 800EED024C5728CC66242BE5E779CD3ECDBDA064DF876E846A0853C0E3E1D9C1 | C38C86BE44A4EF571660EC1353B47E93D9BEBA699F61D6043E5F100858CB9984 | yes |
| ToDoModel.Test/packages.config | 029CB434001028728FDFBD3555D65AF586D4459A99BFF5616FC72F3844001FCA | FAB6626A0BB95D5E443AB7E3E2E2E458DCB91F2AE2206F6ED3A35CDA473F4E1A | yes |
| ToDoModel/app.config | 7420E67550878AB81DB1E17D0F1406E5A693DBCF4480CBC47460636D5E14A58A | CF9C6DD7D37BE1FF9D99717D1C65F3B5106A53B8EC4A9C453917B7355E8CD3E1 | yes |
| ToDoModel/packages.config | FAC708F1B51408201668F902475E3DD972243FC1CA16FF5D3FAF1DFC54A950C2 | CA81F617D4035746224D3C00E51E340559E19D74467F2323476B8AB9CA542D06 | yes |
| UtilitiesCS.Test/app.config | 9A5E6310D11BDB5D5A871AD0EDAB1A6CDF2C7C7BEDB1E3678C12927D804BF652 | 75091BA29F319617790656F7A2DD75E48AC03685183A6261D7CD7DEC5705F74F | yes |
| UtilitiesCS.Test/packages.config | 46C7D348EF872B9350DAB0F96BDF52D349E5960A094D579FA155CFEFA70EE441 | 6792137D37296A6258AC0434DC11A04B1331F9AC9E4FD335FE4452B18F0C93B4 | yes |
| UtilitiesCS/app.config | BFB8285BD02E5E4CE631575470C308F2EEAE2FA88E9E8B903C390801046D745C | CB04D3815FC7AC3B0EFF087831EFBB72B6AB6E474D9DC526C2AA895ACA33F5FC | yes |
| UtilitiesCS/packages.config | 620A32C250DE1D07D60D27970CB19B14F2FFAC868CF88455F782A8219303AD70 | 0D812B23CD5A407EBF434E2E847079105C30477EAB939DDD0410BC17D9A40F56 | yes |
| VBFunctions.Test/app.config | DE20A5EBF79D6570B77501C435C56804A9D54C138239DD9B86BC479D3C140E0F | C1B49C74888DDCB8FF94E4758D8C56E69929EB83EC95282C3D0C1BF65FC1D38F | yes |
| VBFunctions.Test/packages.config | 5FA5A95BDC07E6136006CCA9A3291F6511CEC578FC6900E866BBCCFD0F62AC02 | EF9192E14364D6E239F4196742B6C02AD0FEC5879ACB2367E2F9EF5A0974DBA6 | yes |
| VBFunctions/packages.config | 355B9B96807AD5415719B42920EE9DF571D009B44655CC98D608F3FE12321E55 | 60DB03E24757613CAF808A82D32358CA82811B8CCCBDF2BB752E5A24B5E6FB48 | yes |

`VBFunctions` carries no `app.config`, which is why the 17-member application-configuration
population has no `VBFunctions/app.config` row.

## Ordering precondition

This task ran **after** P1-T2, which removed `**/packages.config` and `**/app.config` from
CSharpier's scope. A normalisation performed while the formatter still owned those paths would be
undone by the next format step and would make AC3 unsatisfiable.

## Acceptance evaluation

| Clause | Measured | Verdict |
|---|---|---|
| Examined `packages.config` exactly 18 | 18 | PASS |
| Examined `app.config` exactly 17 | 17 | PASS |
| Examined total exactly 35 | 35 | PASS |
| Changed counts per kind recorded as measured | 17 and 17 | PASS |
| Content residual after the run exactly 0, against a non-zero before-count | 0, against 1193 | PASS |
| `git diff --numstat` added and deleted totals recorded as integers | 1201 added, 6077 deleted, 34 files | PASS |
| `git diff --name-only` lists only files drawn from the 35-member set | 34 listed, 0 outside | PASS |
| Porcelain companion captured in the same task | 34 entries, 0 outside the set | PASS |

Output Summary: the normaliser examined 18 `packages.config` and 17 `app.config` files, 35 in
total, and rewrote 17 and 17 of them. `SVGControl/packages.config` was examined and left
byte-identical because it is already canonical, which is the expected and recorded reason the
changed counts fall one short of the examined totals. The content residual fell from 1193 reflowed
element lines to 0; the merge-base diff is 1201 added and 6077 deleted lines across 34 files; the
name-listing diff and the porcelain companion each list 34 paths and neither names anything outside
the 35-member discovery set.
