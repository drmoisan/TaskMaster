# Baseline — Git/Working-Tree State

Timestamp: 2026-06-09T11-31
Command: git status --porcelain
EXIT_CODE: 0

Output Summary:
```
 M UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs
 M UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs
 M UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs
?? docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/remediation-inputs.2026-06-09T11-31.md
?? docs/features/active/2026-06-08-csharp-analyzer-stack-hardening-181/remediation-plan.2026-06-09T11-31.md
```

Required file-state confirmations:
- (a) IN-SCOPE modified test file present and UNSTAGED:
  `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableBase_Tests.cs` — status ` M` (modified, not staged). This is the named-test conversion baseline.
- (b) OUT-OF-SCOPE WIP files present, modified, UNSTAGED, and explicitly EXCLUDED from this cycle:
  - `UtilitiesCS/ReusableTypeClasses/Other/StackGeek.cs` — status ` M` (modified, not staged).
  - `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` — status ` M` (modified, not staged).
  These MUST remain modified-but-unstaged through the entire cycle; never `git add -A`.

Notes:
- `artifacts/` is gitignored; the research doc and orchestrator-state are untracked (not shown by porcelain because ignored).
- The two `??` entries are this cycle's remediation-inputs and remediation-plan documents.
