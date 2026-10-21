# P5-T6: Scope-Boundary Gate for AC10

Timestamp: 2026-09-03T12-06

Command: git diff --name-only BASELINE_SHA -- . ":(exclude)docs" ":(exclude).claude"
Command: git status --porcelain -- UtilitiesCS/Threading "UtilitiesCS/To Depricate"

Output Summary:
First command (BASELINE_SHA = b24b62fd15b4956ca8ffa9358f57c90ea3e35413), verbatim:

```
UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs
UtilitiesCS/EmailIntelligence/FolderConverter.cs
UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs
```

Exactly the plan's four Write Set source paths; no path begins with
`UtilitiesCS/Threading/` and no path equals
`UtilitiesCS/To Depricate/FileIO2.cs`.

Second command: empty output.

Jointly, no tracked-or-untracked change exists in either excluded area, satisfying
AC10.
