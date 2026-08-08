# nullable-gate-masked-by-incremental-build (Issue #492)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/nullable-gate-masked-by-incremental-build/ (Issue #492)

- Work Mode: full-bug

- Issue: #492
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/492
- Last Updated: 2026-08-08
## Problem / Why

The repository's prescribed nullable type-check gate does not actually type-check `UtilitiesCS`.

The policy command (CLAUDE.md § C#1.3, CUT3 step 3) is:

```
msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

Because this uses `/t:Build`, MSBuild's incremental up-to-date check skips recompiling assemblies whose inputs have not changed. The nullable and treat-warnings-as-errors properties therefore never reach the compiler for those projects, and the gate reports `EXIT_CODE: 0` without having evaluated them.

Forcing a full recompile reveals the masked state:

```
msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
```

produces **195 errors, all in `UtilitiesCS.csproj`**, and zero in `QuickFiler` / `QuickFiler.Test`.

Breakdown by diagnostic:

| Diagnostic | Count |
|---|---|
| CS8766 | 130 |
| CS8618 | 23 |
| CS8625 | 12 |
| CS8600 | 9 |
| CS8601 | 8 |
| CS8604 | 7 |
| CS8602 | 3 |
| CS8603 | 2 |
| CS8714 | 1 |

## Impact

The nullable gate is a required step in the mandatory toolchain loop and is treated as a passing quality signal by every agent and contributor who runs it. In its current form it provides materially weaker assurance than it appears to for `UtilitiesCS`, which is a widely-consumed core library. A green nullable gate does not currently mean the solution is nullable-clean.

This is a gate-fidelity defect, not a request to fix the 195 diagnostics. The two concerns should be separated: first make the gate report truthfully, then decide how to burn down the debt it exposes.

## Discovery Context

Found during execution of issue #230 (PR #479, WinForms message-pump test seam). The finding was recorded as an out-of-scope observation and is not caused by, and does not block, that change. Evidence artifact:

`docs/features/active/2026-08-07-winforms-message-pump-test-seam-230/evidence/baseline/baseline-nullable.2026-08-07T21-45.md`

## Proposed Behavior

Decide and implement a gate that cannot silently skip compilation. Options to evaluate:

1. Change the prescribed command to `/t:Rebuild`, accepting the longer runtime.
2. Keep `/t:Build` but add an explicit verification that the compile actually ran (for example asserting a non-vacuous `CoreCompile` count, as the #424 evidence pattern already does).
3. Enable nullable analysis as a persisted project property in `Directory.Build.props` / the `.csproj` files rather than as a command-line override, so it is part of the normal build inputs and participates in up-to-date checks.

Whichever option is chosen, update CLAUDE.md § C#1.3 and the CUT3 command list so the documented command and the enforced behavior agree.

Sequencing note: making the gate honest will turn the 195 diagnostics into build failures. Plan the burn-down (or a scoped, documented suppression baseline for `UtilitiesCS`) as part of the same change, or the repository will be left with a red gate.

## Acceptance Criteria (early draft)

- [ ] The prescribed nullable gate command fails when a project in the solution has nullable violations, verified by a deliberately-introduced violation.
- [ ] The gate cannot report success while skipping compilation of a project (verified by a non-vacuous compile assertion or by using a target that always compiles).
- [ ] CLAUDE.md § C#1.3 and the CUT3 command list match the enforced behavior.
- [ ] The `UtilitiesCS` diagnostics are either resolved or captured in an explicit, documented, reviewable baseline — not left to be masked by the incremental check.

## Constraints & Risks

- Switching to `/t:Rebuild` increases toolchain loop time for every agent and contributor; measure before committing to it.
- A suppression baseline must be narrow and reviewable, per CLAUDE.md § C#7 ("If suppression is unavoidable, keep it as narrow as possible and document the rationale in-code").

## Test Conditions to Consider

- [ ] Deliberately introduce a nullable violation in `UtilitiesCS` and confirm the gate fails.
- [ ] Confirm the gate fails on a clean checkout and on a warm incremental checkout (the warm case is the one that currently regresses).
- [ ] Confirm `QuickFiler` / `QuickFiler.Test` remain clean.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Create active feature folder from the template
