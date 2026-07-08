# Phase 0 — Instructions Read (Cycle 2)

Timestamp: 2026-06-12T16:53Z

Policy Order:
1. CLAUDE.md (standing instructions)
2. .claude/rules/general-code-change.md (cross-language code change policy)
3. .claude/rules/general-unit-test.md (cross-language unit test policy)
4. .claude/rules/csharp.md (C#-specific rules)

Files read (explicit list):
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\CLAUDE.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\general-code-change.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\general-unit-test.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\.claude\rules\csharp.md
- C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-06\docs\features\active\2026-06-08-hierarchical-lcppn-folder-prediction-177\remediation-inputs.2026-06-12T16-45.md

Output Summary: All four policy documents and the cycle-2 remediation inputs were read in
the required order. Cycle 2 scope is the single finding F3 (AC20): split the over-cap
test file LcppnFolderPredictor_Tests.cs (554 lines) into two files each <= 500 lines,
preserving all 21 test cases and LcppnFolderPredictor strict coverage >= 90% (baseline
97.71%). Containment invariant: zero diff to ManagerAsyncLazy.cs, the shared Manager
value type, Triage.cs, SpamBayes.cs, CategoryClassifierGroup.cs, MulticlassEngine.cs.
This cycle touches only the two test files and UtilitiesCS.Test/UtilitiesCS.Test.csproj.
