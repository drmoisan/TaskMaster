# Baseline — Raw-Output Directory Bootstrap (Issue #656)

Timestamp: 2026-09-01T14-38
Task: [P0-T7]

Command:
```
New-Item -ItemType Directory -Force -Path 'TestResults\msbuild','TestResults\p0-t10','TestResults\p1-t3','TestResults\p3-t2','TestResults\p3-t4','TestResults\p4-t7','TestResults\p4-t8-repeat' | Out-Null
(Test-Path 'TestResults\msbuild')
(Test-Path 'TestResults\p0-t10')
(Test-Path 'TestResults\p1-t3')
(Test-Path 'TestResults\p3-t2')
(Test-Path 'TestResults\p3-t4')
(Test-Path 'TestResults\p4-t7')
(Test-Path 'TestResults\p4-t8-repeat')
```

EXIT_CODE: 0

Test-Path results (all seven):

- `TestResults\msbuild` = True
- `TestResults\p0-t10` = True
- `TestResults\p1-t3` = True
- `TestResults\p3-t2` = True
- `TestResults\p3-t4` = True
- `TestResults\p4-t7` = True
- `TestResults\p4-t8-repeat` = True

This step precedes every task that writes a raw log because the msbuild file logger opens its log
with a `StreamWriter` and fails the build with an invalid-file-logger-file error when the parent
directory is missing, and `Tee-Object -FilePath` likewise does not create a missing parent.
`TestResults/` is git-ignored, so none of these paths enters the change set.

Output Summary: Bootstrap succeeded. All seven raw-output directories created and confirmed present.
This is a bootstrap step, not a toolchain gate.
