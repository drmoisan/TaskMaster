# Code Review — 2026-08-10-csharp-toolchain-gate-fidelity-512

- **Artifact:** `code-review.2026-08-10T23-51.md`
- **Branch:** `bug/csharp-toolchain-gate-fidelity-512` (head `9773d6f5`) vs `origin/epic/build-ci-coverage-gate-fidelity-integration` (merge base `a5e336e5`)
- **Companion artifacts:** `policy-audit.2026-08-10T23-51.md`, `feature-audit.2026-08-10T23-51.md`

## Executive Summary

The code changes are small, precisely scoped, and match the spec's replacement tables. The PowerShell changes preserve backward compatibility (`-Target` defaults to `Build`; `-EnableNullable` remains bindable as a warning-emitting no-op), keep the changed logic inside the already-tested pure region, and are covered by a genuine red-before-green regression pair. The governance edits are faithful to the canonical replacement strings and carry the load-bearing rationale at the normative sites. No blocking findings. One Minor documentation-consistency finding and three informational observations follow.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Minor | CLAUDE.md; .claude/skills/csharp-qa-gate/SKILL.md | § CUT3 item 2; § "C# Toolchain" item 2; SKILL step 2 | The analyzer command at the three condensed sites uses `/t:Rebuild` (deliberately different from CI's `/t:Build`) without the R4 rationale sentence carried at the two normative sites, while `spec.md` SD2 states the rationale "must appear adjacent to the command at each site". The delivery follows the spec's replacement table (rows 7, 10, 17), which omits R4 at these sites; the SD2 prose and the table are internally inconsistent. | In a follow-up (candidate: fold into issue #535 or the next governance-doc touch), add a one-line R4 pointer at the condensed sites, or amend SD2's prose to name the two normative sites as the rationale carriers. | AC5's criterion text requires the deliberate difference to be "stated in-line with its rationale," which is satisfied at the normative sites (`CLAUDE.md` § C#1 item 2; `.claude/rules/csharp.md` items 2–3); the residual risk is a reader of only the condensed list "restoring" `/t:Build` for CI parity. The type-check sites all carry their prohibitions, so the highest-risk regression (restoring `/p:Nullable=enable`) is guarded everywhere. | `evidence/qa-gates/ci-parity.2026-08-11T00-28.md` (adjudicates the condensed sites as citations of the normative expansion); diff hunks for the three files |
| Info | scripts/vscode/Invoke-VSBuild.ps1 | `Get-RequestedMSBuildProperties`, line 117 | The deprecated-switch branch now emits `Write-Warning` from inside an otherwise pure argument-builder function, adding a side effect to a function the tests treat as pure. | None required. If the function grows further side effects, extract notification out of the builder. | The warning is the documented design (spec row 23), fires only on the deprecated path, uses the repo logging pattern (`Write-Warning`, not `Write-Host`), and is asserted inert by the updated test (property array contains only `TreatWarningsAsErrors=true`). Pester coverage counts the line as executed. | Diff; `evidence/qa-gates/enablenullable-noop-proof.2026-08-11T00-14.md` (exit 0 with the warning text present, contrasted against the 195-error debt probe) |
| Info | scripts/vscode/Invoke-VSBuild.ps1 | uncovered I/O tail (lines 134–166 post-change) | Seven pre-existing uncovered commands (throws and the MSBuild invocation) remain uncovered; the missed-command set is byte-identical to baseline, only line numbers shifted. | None for this feature. A wrapper seam was considered and rejected in spec option (g); revisit only if this script's I/O tail changes behavior. | Changed lines are all in the covered pure region; introducing an `Invoke-MSBuildExe` seam solely to cover pre-existing throws would be scope creep in a gate-fidelity bugfix. | `evidence/qa-gates/powershell-coverage-delta.2026-08-11T00-45.md` § 4 |
| Info | .vscode/tasks.json | `test: MSTest (vstest.console)` args array | A pre-existing trailing comma makes the file JSONC-only (it fails a strict JSON parse). Present at the merge base; this diff adds no new instance (the two hunks insert well-formed `"-Target", "Rebuild"` pairs). | None; VS Code parses tasks.json as JSONC. Raised for the record only because a strict-JSON consumer would mis-read the file. | Pre-existing condition outside this feature's edits; the feature's own task-lint evidence exercised both edited tasks successfully. | `evidence/qa-gates/vscode-task-lint.2026-08-11T00-06.md`, `vscode-task-typecheck.2026-08-11T00-12.md` (both tasks perform genuine rebuilds; type-check no longer passes `Nullable=enable`) |

## Detailed Review Notes

### scripts/vscode/Invoke-VSBuild.ps1

- `-Target` is added with `[ValidateSet('Build', 'Rebuild')]` at both the script and function level, defaulting to `Build`; the hardcoded `'/t:Build'` becomes `"/t:$Target"`, and the call site threads `-Target $Target`. This is the minimal correct shape: existing callers (including the unchanged `build:` task) see identical behavior, and the two corrected tasks opt into `Rebuild` explicitly. The spec's correction of research D4 (do not invert the default-target assertion) was honored — the default-target test is byte-identical.
- The deprecation comment is duplicated at both `param()` blocks, matching spec rows 20–21, so a reader of either surface sees the deprecation before the behavior.

### tests/scripts/vscode/Invoke-VSBuild.Tests.ps1

- The new `It` asserts the full argument array with `/t:Rebuild` in the exact positional slot, not a `-Contains` check — position matters to MSBuild argument readability and this pins it. The renamed `It` asserts exact array equality `@('TreatWarningsAsErrors=true')`, which fails if `Nullable=enable` ever reappears. Both were demonstrated red against the pre-fix implementation (parameter-binding failure and property-emission failure respectively) and green after.

### Governance documents

- All command strings match the four canonical replacement strings byte-for-byte, including the `"/p:Platform=Any CPU"` CI spelling. The severity-first invariant at `.claude/rules/csharp.md` § "Severity-first ordering invariant" is preserved verbatim except for the embedded command string, as required. The R6 evidence bullet added to the qa-gate skill converts the non-vacuity assertion from a one-feature practice into a standing obligation, which is the correct place for it.
- One observational note, no action needed: the type-check positive control recorded `6 Warning(s)` alongside exit 0 under `/p:TreatWarningsAsErrors=true`. These are MSBuild engine-level warnings, which that property does not promote (it governs compiler warnings); CI exhibits the same behavior, so documented-vs-CI parity holds.

### .vscode/tasks.json

- The `lint:` task inserts `"-Target", "Rebuild"` before the analyzer switches; the `type-check:` task replaces `"-EnableNullable"` with `"-Target", "Rebuild"` and retains `"-TreatWarningsAsErrors"`. Task labels are unchanged, so external references by label continue to resolve. Both corrected tasks were executed from the task surface with genuine-rebuild proof.

## Verdict

No blocking findings. One Minor (documentation consistency at condensed sites), three Info. The change is approved from a code-quality standpoint.
