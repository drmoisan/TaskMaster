# AC-18 numeric coverage reset

Timestamp: `2026-07-22T09-32`

Command: `$spec='docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/spec.md'; $items=@(Get-Content $spec | Where-Object { $_ -match '^- \[[ x]\] AC-\d+:' }); $wording=($items -replace '^- \[[ x]\] ','') -join "`n"; $sha=[Security.Cryptography.SHA256]::Create(); try{$wordingHash=[BitConverter]::ToString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($wording))).Replace('-','')}finally{$sha.Dispose()}; "FILE_SHA256=$((Get-FileHash -Algorithm SHA256 $spec).Hash)"; "WORDING_SHA256=$wordingHash"; "COUNT=$($items.Count)|CHECKED=$(@($items | Where-Object { $_ -match '^- \[x\]' }).Count)|OPEN=$(@($items | Where-Object { $_ -match '^- \[ \]' }).Count)"; $items`

EXIT_CODE: `0`

Output Summary: `PASS. Exactly the AC-18 checkbox marker changed from [x] to [ ]. All 19 criterion texts and their order retained the same wording SHA-256. The current inventory is four supported and 15 open.`

## Exact marker change

```diff
- [x] AC-18: One final uninterrupted C# toolchain pass succeeds ...
+ [ ] AC-18: One final uninterrupted C# toolchain pass succeeds ...
```

No other acceptance-criterion marker or text changed in this task.

## Integrity

- Pre-reset `spec.md` SHA-256: `2AE9BAE9C58019CF329CCAB2A242EF61214EB31DFE74F39F28ACB3E20D20B0B8`.
- Post-reset `spec.md` SHA-256: `42A7E11878CBED4F63F8DF6F7A83F538C7610247510576EF47496DCC5E9603E1`.
- Pre-reset AC wording SHA-256: `85F08730E24A6A4BED0092802FA173D94DDE86F20007B92847B23ED73A8F7EB3`.
- Post-reset AC wording SHA-256: `85F08730E24A6A4BED0092802FA173D94DDE86F20007B92847B23ED73A8F7EB3`.
- AC count: `19` before and after.
- Pre-reset inventory: five checked, 14 open.
- Post-reset inventory: four supported, 15 open.
- Supported markers retained: AC-2, AC-4, AC-9, and AC-17.

## Recheck rule

Only P10-T2 may recheck AC-18. Recheck requires P9-T1 through P9-T7 and P10-T1 to provide current authoritative full-repository evidence for every AC-18 condition. Focused P5 evidence cannot recheck AC-18.
