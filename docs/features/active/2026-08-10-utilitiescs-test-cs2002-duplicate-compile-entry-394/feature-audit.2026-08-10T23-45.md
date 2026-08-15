# Feature Audit — utilitiescs-test-cs2002-duplicate-compile-entry-394

- **Issue:** #394
- **Branch:** `bug/utilitiescs-test-cs2002-duplicate-compile-entry-394`
- **Timestamp:** 2026-08-10T23-45

## Scope and Baseline

- **Base branch (resolved):** `origin/epic/build-ci-coverage-gate-fidelity-integration`
- **Merge-base SHA:** `a5e336e5ae3443d4197caf5f87036fae1d538f89` (recomputed via `git merge-base HEAD origin/epic/build-ci-coverage-gate-fidelity-integration`; matches the caller-supplied base)
- **Diff command:** `git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD`
- **Work mode:** `full-bug` — authoritative AC source is `spec.md` `## Acceptance Criteria` only (8 items). `user-story.md` exists but is explicitly not the AC source for `full-bug` per `acceptance-criteria-tracking`.

## Summary

This feature deletes one duplicate `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` item from `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, eliminating compiler warning CS2002. All 8 acceptance criteria in `spec.md` are independently verified against evidence artifacts under this feature's `evidence/` tree and confirmed PASS. A related but distinct policy-level finding (mandatory PowerShell coverage verification for a committed helper script) is tracked in `policy-audit.2026-08-10T23-45.md` and does not map to, or invalidate, any of the 8 spec-defined acceptance criteria below.

## Acceptance Criteria Inventory

Source: `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md`, `## Acceptance Criteria` (8 items, all currently checked `[x]`):

1. Exactly one `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` item remains after the change.
2. Fail-before evidence captures CS2002 using a `/t:Rebuild` command, recorded under `evidence/baseline/`.
3. Post-change build using the same `/t:Rebuild` command emits no CS2002 for the file, recorded under `evidence/qa-gates/`.
4. `PercentageFormatterTests` test count unchanged at 7, verified via vstest, before/after recorded numerically under `evidence/regression-testing/`.
5. Duplicate sweep across every item type in the `.csproj` recorded, with findings reported.
6. Diff touches only the `.csproj` (single-line deletion) plus this feature folder's own docs/evidence, with no reformatting/reordering/line-ending change.
7. Full toolchain pass completed for applicable stages (CSharpier/analyzer/nullable N/A; build/test verification passes).
8. Docs/config references (`spec.md`, `issue.md`) updated and mutually consistent.

## Acceptance Criteria Evaluation

| # | Status | Evidence | Notes |
|---|---|---|---|
| 1 | PASS | `git diff` hunk shows exactly one deleted `<Compile Include>` line at the former line 356; `grep -n "PercentageFormatterTests"` post-change returns exactly one match (line 304) | Check-off is supported. |
| 2 | PASS | `evidence/baseline/fail-before-cs2002.2026-08-10T22-31.md`: command is genuine `/t:Rebuild` (not `/t:Build`); output contains the literal CS2002 warning text for `PercentageFormatterTests.cs`; EXIT_CODE 0 | Check-off is supported. Command matches spec.md's mandated invocation exactly. |
| 3 | PASS | `evidence/qa-gates/post-fix-cs2002.2026-08-10T22-31.md`: identical `/t:Rebuild` command (explicitly annotated "exactly as P0-T9"); warning count drops from 6 to 5; zero `CS2002` matches | Check-off is supported. Comparison is non-vacuous — same command, genuine rebuild. |
| 4 | PASS | `evidence/baseline/baseline-test-count.2026-08-10T22-31.md` (7 tests, 7 passed) and `evidence/regression-testing/post-fix-test-count.2026-08-10T22-31.md` (7 tests, 7 passed, explicitly compares against the recorded baseline) | Check-off is supported. Numeric before/after counts recorded and equal, matching spec's documented 7 `[TestMethod]` members. |
| 5 | PASS | `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` and its retained script `evidence/baseline/duplicate-sweep.ps1`: sweep covers `Compile` (452, 1 duplicate), `EmbeddedResource`, `None`, `Reference`, `ProjectReference`, `BootstrapperPackage`, `Analyzer`, `AdditionalFiles`, and `packages.config` (0 duplicates in all but `Compile`) | Check-off is supported. Minor, non-blocking documentation discrepancy: `spec.md`'s Root Cause Analysis table states `Analyzer`=9, `Reference`~=114, `packages.config`~=99, while the captured evidence shows `Analyzer`=11, `Reference`=126, `packages.config`=105 — does not change the zero-additional-duplicates conclusion (see `code-review.2026-08-10T23-45.md`). Retaining the sweep script itself as a committed `.ps1` file raises a separate, policy-level PowerShell coverage finding tracked in `policy-audit.2026-08-10T23-45.md` Section 4 (blocking) — this does not affect AC5's own PASS status, which concerns the sweep's findings, not the tooling used to produce them. |
| 6 | PASS | `evidence/qa-gates/diff-scope.2026-08-10T22-31.md` and independent `git diff --name-only`/`--stat` re-verification: only `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (1 deletion, 0 insertions) plus this feature folder's own docs/evidence files appear in the diff; no `CLAUDE.md`, `.claude/rules/**`, or `scripts/**` file appears | Check-off is supported by the AC's literal wording, which explicitly permits "this feature folder's own documentation and evidence files" — the newly committed `.ps1` helper script falls within that allowance textually. The policy-level PowerShell coverage gap this creates is a real, separate finding (see `policy-audit.2026-08-10T23-45.md`) but is not an AC6 violation per AC6's own text. |
| 7 | PASS | `evidence/qa-gates/csharpier-not-applicable.2026-08-10T22-31.md`, `evidence/qa-gates/nullable-gate-not-run.2026-08-10T22-31.md`, `evidence/qa-gates/solution-rebuild.2026-08-10T22-31.md` (EXIT_CODE 0, CI-equivalent `TreatWarningsAsErrors` gate) | Check-off is supported. Non-blocking observation: unlike CSharpier and the nullable gate, there is no dedicated `evidence/qa-gates/analyzer-not-applicable.*.md` artifact explicitly documenting why the `EnableNETAnalyzers=true` analyzer-build command was not separately run; the solution-level `TreatWarningsAsErrors` rebuild is a reasonable substitute given zero `.cs` files changed, but a dedicated artifact would give this determination the same evidentiary parity as the other two "not applicable" determinations. Recommend adding one for completeness; not blocking. |
| 8 | PASS | Direct read of `spec.md` and `issue.md`: both carry the same 8-item AC list (issue.md's is an explicit mirror per its own text), both fully checked `[x]`, and both are internally consistent with the delivered change | Check-off is supported. |

## Acceptance Criteria Check-off Verification

All 8 items in `spec.md` (and their mirror in `issue.md`) are already checked `[x]`. This audit confirms each check-off is supported by a corresponding evidence artifact (see table above); no unsupported check-offs were found. No AC items require reversion to `[ ]`.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/spec.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

## Relationship to Policy Audit

This feature-audit's PASS verdicts on all 8 spec-defined acceptance criteria are independent of the one BLOCKING finding recorded in `policy-audit.2026-08-10T23-45.md` (PowerShell coverage-artifact absence for the committed `evidence/baseline/duplicate-sweep.ps1`). That finding is a policy/toolchain-compliance gap introduced incidentally by the executor's choice to commit a helper script beyond what the plan required; it does not correspond to, and does not falsify, any of the 8 acceptance criteria evaluated above. See `remediation-inputs.2026-08-10T23-45.md` for the required remediation before this branch can be considered fully policy-compliant.
