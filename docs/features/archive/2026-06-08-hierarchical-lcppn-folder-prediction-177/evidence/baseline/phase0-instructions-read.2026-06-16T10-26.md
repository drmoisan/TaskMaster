# Phase 0 — Instructions Read (Cycle 4, #177 / AC25)

Timestamp: 2026-06-16T10-26

Policy Order: CLAUDE.md -> .claude/rules/general-code-change.md -> .claude/rules/general-unit-test.md -> .claude/rules/csharp.md -> remediation-inputs (spec) -> research

Files read (in required order):
1. `CLAUDE.md` (project standing instructions; loaded in session context)
2. `.claude/rules/general-code-change.md` (cross-language code change policy)
3. `.claude/rules/general-unit-test.md` (cross-language unit test policy)
4. `.claude/rules/csharp.md` (C# code/test standards, toolchain order)
5. `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/2026-06-16T10-26-remediation/remediation-inputs.2026-06-16T10-26.md` (spec source)
6. `artifacts/research/2026-06-16-filepathhelper-deserialization-nre-research.md` (root cause + fix)

Key constraints absorbed:
- Bugfix discipline: failing regression test RED before the production guard, then GREEN.
- Containment: only `FilePathHelper.cs` (production) + `FilePathHelper_Tests.cs` (test) may change.
- Retain cycle-3 `DoNotSerializeContractResolver("Config")` in `LcppnFolderPredictorStore.cs` (INV-1).
- No serialized-shape change, no public-API change (INV-3).
- File-size cap: `FilePathHelper.cs` <= 500 lines (currently 494); test file <= 500 (currently 370).
- Recommended fix (research Option A): early `return false` in instance `AdjustForMaxPath()` when
  `_fileExtension`/`_fileStemSuffix`/`_fileStemSeed` is null, after the `StemInitialized()` check.
- Confirmed `FromSeed` signature: `FromSeed(fileNameSeed, fileExtension, fileNameSuffix, folderPath)`.

Output Summary: All six required policy/spec/research files read in order. No policy conflicts found.
