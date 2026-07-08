# Code Review: global-json-sdk-pin-regressed-to-10 (Issue #194)

**Review Date:** 2026-06-13
**Reviewer:** feature-reviewer agent
**Feature Folder:** `docs/features/active/2026-06-12-global-json-sdk-pin-regressed-to-10-194`
**Feature Folder Selection Rule:** Supplied by caller; matches issue #194 suffix and the only active folder with the scoping docs for this change.
**Base Branch:** `origin/main` (merge-base `1b3f5350`)
**Head Branch:** `feature/csharp-coverage-uplift` (PR-context head `bug/global-json-sdk-pin-194` @ `057dbc82`)
**Review Type:** Initial review

---

## Executive Summary

This branch reverts a single configuration value in the repo-root `global.json`: `sdk.version` from `10.0.200` to `8.0.205`. The revert restores the deliberate repo-local .NET 8 SDK pin used by the `Install-RepoDotNetSdk.ps1` workaround and the `codex-web-setup-test.yml` workflow marker, and it makes the previously failing `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` regression test pass. The remaining branch content is feature-folder documentation, an atomic plan, Phase 0/Phase 2 evidence artifacts, and the promotion rename of the potential-feature markdown into `issue.md`.

**What changed:**
The only non-documentation change is `global.json` line 3: `"version": "10.0.200"` -> `"version": "8.0.205"`. A `git diff` of `global.json` against the merge-base shows exactly one changed line; all other keys (`rollForward`, `allowPrerelease`, `paths`, `errorMessage`) are unchanged. No PowerShell, Python, TypeScript, C#, or Bash source files changed.

**Top 3 risks:**
1. The `errorMessage` string in `global.json` still references `dotnet format`, which CLAUDE.md now prohibits in favor of `csharpier`. This is a pre-existing cosmetic inconsistency, explicitly flagged as optional/out-of-scope in `issue.md`; it does not affect behavior.
2. The committed `8.0.205` value remains load-bearing for the codex-web-setup workflow marker directory; correctness depends on that workflow continuing to read `sdk.version`. This is the intended behavior being restored, not a new risk.
3. None beyond the above. The change carries no executable-code risk.

**PR readiness recommendation:** **Go** — A minimal, well-evidenced single-field config revert that satisfies all acceptance criteria with a passing regression test and a clean toolchain.

---

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | `global.json` | `errorMessage` (line 10) | The error message references `dotnet format`, which CLAUDE.md prohibits in favor of `csharpier`. | Optional follow-up: update the message to reference `csharpier .`. Out of scope for this minor-audit. | Cosmetic only; does not affect the SDK pin or any behavior. `issue.md` already records this as an optional, non-blocking item. | `global.json` line 10; `issue.md` "Suspected Cause / Notes". |
| Info | `global.json` | line 3 | `sdk.version` reverted `10.0.200` -> `8.0.205`; no other key changed. | None — change is correct and minimal. | Restores the intended .NET 8 pin and fixes the regression test. | `git diff 1b3f5350...HEAD -- global.json`; `final-qa-pester-2026-06-13T09-00.md`. |

No Blockers or Major findings.

---

## Implementation Audit

### PowerShell implementation audit

#### What changed well

- No PowerShell production code was changed. The fix correctly targets the configuration value rather than weakening or modifying the regression test that documents the pin. This preserves the test as the specification, consistent with the General Code Change Policy ("treat existing unit tests as part of the spec").

#### API and safety notes

- No advanced-function, parameter, or ShouldProcess surface changed. The related test file `Install-RepoDotNetSdk.Tests.ps1` (28 lines) uses `Set-StrictMode -Version Latest`, `BeforeAll` dot-sourcing, and `$PSScriptRoot`-relative path resolution — all consistent with repo PowerShell standards.

#### Error handling and logging

- Not applicable; no PowerShell logic changed. The 16 pre-existing PSScriptAnalyzer findings (`PSAvoidUsingWriteHost`, `PSUseOutputTypeCorrectly`, `PSUseSingularNouns`) in unrelated `scripts/vscode` production scripts are unchanged by this branch (delta 0).

---

## Test Quality Audit

The verification evidence is complete and includes both fail-before and pass-after states for the regression test, which is the correct bug-fix sequence.

### Reviewed test and QA artifacts

- `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1` — Asserts the post-revert `global.json` SDK fields (version `8.0.205`, `rollForward` `latestFeature`, `allowPrerelease` false, `paths` contains `.dotnet-sdk` and `$host$`). Deterministic, no external dependencies, no temp files.
- `evidence/regression-testing/baseline-pester-2026-06-13T09-00.md` — Fail-before evidence: Passed 1 / Failed 1, with the version assertion failing (`Expected '8.0.205' But was '10.0.200'`).
- `evidence/qa-gates/final-qa-pester-2026-06-13T09-00.md` — Pass-after evidence: Passed 2 / Failed 0; PoshQC gate ok=true.
- `evidence/qa-gates/final-qa-format-2026-06-13T09-00.md` — Format clean (EXIT 0).
- `evidence/qa-gates/final-qa-analyze-2026-06-13T09-00.md` — Analyzer delta 0 (16 = 16). Independently reproduced during this review (exactly 16 findings).
- `evidence/qa-gates/minor-audit-reconciliation-2026-06-13T09-00.md` — Per-AC reconciliation citing the supporting evidence for AC1–AC4.

### Quality assessment prompts

- **Determinism:** Tests read repo-root files only; no network, time, or random inputs.
- **Isolation:** Each `It` targets a single behavior (URL builder vs config selection).
- **Speed:** Sub-second Pester v5.6.1 run.
- **Diagnostics:** The fail-before message identifies the exact assertion and file line.

---

## Security / Correctness Checks

| Check | Status | Evidence |
|---|---|---|
| No secrets in code | ✅ PASS | `global.json` contains only an SDK version and paths; no secrets. |
| No unsafe subprocess or command construction | ✅ PASS | No executable code changed. |
| Input validation at boundaries | N/A | No code path with inputs changed. |
| Error handling remains explicit | ✅ PASS | No error handling changed; pre-existing test uses strict mode. |
| Configuration / path handling is safe | ✅ PASS | The `paths` array and `errorMessage` are unchanged; only the version value changed, restoring consistency with `Install-RepoDotNetSdk.ps1` (`.dotnet-sdk/sdk/8.0.205`). |

---

## Research Log

No external research was required. All conclusions are grounded in the branch diff, the feature-folder evidence artifacts, the regression test, and an independent re-run of the PowerShell analyzer.

---

## Verdict

The change is ready for normal PR flow. It is a minimal, correct, single-field `global.json` revert that restores the intended .NET 8 SDK pin and fixes the regression test, with a complete fail-before/pass-after evidence chain and a clean PowerShell toolchain (analyzer delta 0). The only finding is an Info-level, pre-existing cosmetic reference to `dotnet format` in the `errorMessage`, which `issue.md` already records as an optional, out-of-scope follow-up. This conclusion is consistent with the Findings Table and the Go recommendation above.
