# nullable-gate-cannot-fail-incremental-build (Issue #512)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/nullable-gate-cannot-fail-incremental-build/ (Issue #512)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #512
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/512
- Last Updated: 2026-08-08
## Summary

The repository-mandated type-check gate `msbuild TaskMaster.sln /t:Build /p:Nullable=enable /p:TreatWarningsAsErrors=true` cannot fail in practice. MSBuild's incremental up-to-date check does not invalidate on a command-line `/p:` change alone, so when the outputs are already current from a prior `/t:Build`, `CoreCompile` is skipped entirely and the gate returns exit 0 without ever re-running nullable analysis. The gate has been reporting success while never executing.

## Environment

- OS/version: Windows 11
- Runtime: .NET Framework 4.8.1
- Command/flags used: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- Data source or fixture: none

## Steps to Reproduce

1. Run the analyzer build: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`. It succeeds.
2. Immediately run the mandated type-check gate: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`. It returns exit 0.
3. Now force recompilation of the same code with the same properties: `msbuild TaskMaster\TaskMaster.csproj /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
4. Observe it returns exit 1 with a large number of `CS86xx` nullable errors.

## Expected Behavior

The type-check gate actually performs nullable flow analysis on every invocation and fails when nullable violations exist, so that "the nullable gate passed" is a meaningful statement.

## Actual Behavior

Step 2 returns exit 0 because MSBuild considers the outputs up to date and skips `CoreCompile`. Step 3, which forces recompilation of identical source under identical properties, returns exit 1 with **195 errors, 64 of them `CS86xx`** in `TaskMaster.csproj`. A separate measurement during the same session counted 220 `CS86xx` errors concentrated in `AppOlObjects.cs`, `AppAutoFileObjects.cs`, `AppToDoObjects.cs`, `AppOlObjects.FolderTreeService.cs`, `AppStagingFilenames.cs`, `ApplicationGlobals.cs`, and `AppItemEngines.cs`.

The same source, the same properties, and two different verdicts depending only on whether the output timestamps happened to be current.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet:

```text
# /t:Build with /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT=0

# /t:Rebuild, same properties, same source
EXIT=1
195 errors (64 CS86xx)
```

Precedent: `.github/workflows/ci.yml` already carries a comment documenting exactly this MSBuild behavior and uses `/t:Rebuild` for its own `TreatWarningsAsErrors` step for that reason. The repository's own CI has the workaround; the policy command documented in `CLAUDE.md` and `.claude/rules/csharp.md` does not.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

This is a quality gate that reports success without running. Every agent and developer who has run the documented toolchain has recorded a passing nullable check that did not execute, which means the recorded evidence across prior features overstates what was verified. It also masks a substantial pre-existing nullable debt in `TaskMaster.csproj`.

Severity is High rather than Blocker because the debt it masks is pre-existing rather than newly introduced, and CI's `/t:Rebuild` step does exercise `TreatWarningsAsErrors` (though CI does not pass `/p:Nullable=enable`, so CI does not surface the `CS86xx` set either).

## Suspected Cause / Notes

MSBuild's up-to-date check compares source and output timestamps; it does not hash the effective property set. A `/p:` value that changes compiler behavior therefore does not invalidate a current output.

Two things are entangled here and should be separated when fixing:

1. **The gate defect** — the documented command cannot fail. Fix by changing the documented type-check command to `/t:Rebuild` (matching the existing `ci.yml` precedent), in `CLAUDE.md` § C#1/CUT3, `.claude/rules/csharp.md` § Toolchain, and the `csharp-qa-gate` skill.
2. **The debt it reveals** — roughly 195-220 errors, ~64 of them `CS86xx`, in `TaskMaster.csproj`. Turning the gate on without addressing the debt would block every subsequent C# change. The debt should be quantified and burned down on its own track, or the gate scoped to changed files first.

Fixing (1) without a plan for (2) will halt C# delivery, so they must be sequenced deliberately.

Found during feature review of issue #503 (ribbon engine readiness guard). Notably, **zero** of the errors are in that change's six new production files, so #503 is not implicated.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: not directly unit-testable; validate by asserting that a deliberately-introduced nullable violation causes the documented command to fail.
- [x] Integration scenario to retest: add a temporary `CS8600`-triggering line, run the documented gate, confirm exit 1, then remove it.
- [x] Manual verification notes: measure the exact debt per project before promoting the gate, and decide between full burn-down and changed-files scoping.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
