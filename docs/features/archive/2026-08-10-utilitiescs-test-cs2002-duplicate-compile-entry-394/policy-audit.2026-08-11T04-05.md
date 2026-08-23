# Policy Audit — utilitiescs-test-cs2002-duplicate-compile-entry-394 (Reaudit — Remediation Cycle 1 Exit Gate)

- **Issue:** #394
- **Branch under review:** `bug/utilitiescs-test-cs2002-duplicate-compile-entry-394` (HEAD `f39c6fc9`)
- **Base branch (resolved):** `origin/epic/build-ci-coverage-gate-fidelity-integration`
- **Diff command:** `git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD`
- **Work mode:** `full-bug` (AC source: `spec.md` only)
- **Timestamp:** 2026-08-11T04-05
- **Entry audit (unmodified, retained):** `policy-audit.2026-08-10T23-45.md`
- **Remediation plan under verification:** `remediation-plan.2026-08-10T23-45.md` (cycle 1, commits `2a2116eb`, `f39c6fc9`), driven by `remediation-inputs.2026-08-10T23-45.md`

## Purpose of This Reaudit

The entry audit (`policy-audit.2026-08-10T23-45.md`) raised exactly one BLOCKING finding: a newly committed PowerShell file (`<FEATURE>/evidence/baseline/duplicate-sweep.ps1`) placed PowerShell into the changed-language set with no coverage artifact behind it. This reaudit independently re-derives, from the current diff and working tree, whether that finding is closed and whether the remediation cycle introduced any new violation.

## Rejected Scope Narrowing

No new caller-attempted scope narrowing is present in this reaudit's directive beyond what the entry audit already evaluated and accepted (C# coverage exemption, nullable-gate substitution, CSharpier N/A — all re-confirmed below as still valid because zero `.cs` files are touched anywhere on the branch). No entry is added to this section.

## 1. Blocking Finding Closure — Independently Re-Derived — PASS

Re-derived directly from the diff and working tree (not from the remediation evidence artifacts' narrative):

```
$ git diff --name-only origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD | grep -iE "\.(ps1|psm1|psd1)$"
(no output; grep exit code 1 — zero matches)

$ git status --porcelain
(empty — working tree clean)
```

Zero `.ps1`/`.psm1`/`.psd1` paths exist anywhere in the full 33-file branch diff. `git status --porcelain` is clean (no untracked or modified files). The file `docs/features/active/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/evidence/baseline/duplicate-sweep.ps1` is confirmed absent from disk and absent from the diff.

**Verdict: PASS.** The changed-language set for this branch contains no PowerShell. The single BLOCKING finding from the entry audit is closed. No coverage-verification obligation exists for PowerShell on this branch.

## 2. No Regression to the Original Fix — PASS

```
$ git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD -- UtilitiesCS.Test/UtilitiesCS.Test.csproj
@@ -353,7 +353,6 @@
     <Compile Include="OutlookObjects\Folder\FolderScorerRegressionTests.cs" />
     <Compile Include="OutlookObjects\Folder\FolderScoreTests.cs" />
     <Compile Include="OutlookObjects\Folder\FolderScorerTests.cs" />
-    <Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
     <Compile Include="OutlookObjects\Folder\FolderNodeViewModelTests.cs" />
     <Compile Include="OutlookObjects\Folder\FolderHierarchyBuilderTests.cs" />
     <Compile Include="OutlookObjects\Folder\FolderTreeStateModelTests.cs" />
```

Exactly one deletion, zero insertions, no reordering — identical to the diff evaluated at the entry audit. Cross-checked against the original fix commit alone (`git diff origin/epic/build-ci-coverage-gate-fidelity-integration...f58f8474 --stat -- UtilitiesCS.Test/UtilitiesCS.Test.csproj`), which shows the same `1 file changed, 1 deletion(-)`. The remediation cycle (commits `2a2116eb`, `f39c6fc9`) touched zero lines of `UtilitiesCS.Test.csproj`.

**Verdict: PASS.**

## 3. Mid-Execution Acceptance Revision (`[P2-T2]`) — Judged Legitimate, Not Gate-Softening — PASS

**Original wording** (as committed in `2a2116eb`, before revision):
> "Acceptance: the `.csproj`-scoped diff shows exactly one deletion (`1 -`) and zero insertions (`0 +`), and the full changed-path list contains no path outside `<FEATURE>/` other than `UtilitiesCS.Test/UtilitiesCS.Test.csproj`."

**Revised wording** (final, in `f39c6fc9`): four explicit clauses (a)-(d) — (a) csproj-scoped diff exactly `1 -`/`0 +`; (b) no path outside `<FEATURE>/` other than the csproj and `.claude/agent-memory/**`; (c) zero occurrences of `CLAUDE.md`, `.claude/rules/**`, `scripts/**`, any `.cs` file, any other `.csproj`, or any coverage-threshold change; (d) zero `.ps1`/`.psm1`/`.psd1` paths anywhere, with the `.claude/agent-memory/**` exclusion in (b) explicitly stated to NOT apply to clause (d).

**Trigger for the revision:** the full merge-base diff contains exactly one path outside `<FEATURE>/` other than the csproj: `.claude/agent-memory/feature-review/project_durable-feature-script-triggers-python-coverage-gate.md`. Independently verified:

```
$ git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD -- .claude/agent-memory/feature-review/project_durable-feature-script-triggers-python-coverage-gate.md
@@ -10,3 +10,5 @@
 ...
+
+**Confirmed instance under `evidence/` (not just `scripts/`)**: #394 ...
```
2 insertions, 0 deletions — confirmed a two-line append to a pre-existing memory-index file (the feature-review subagent's own persistent memory record of this feature's entry-audit finding), not new source, policy, rule, or script content.

**Assessment:** Under the *original* wording, this legitimate byproduct of running the review subagent would have failed clause "no path outside `<FEATURE>/` other than the csproj," which is exactly why the revision was made. The revision does not weaken the substantive prohibitions the original clause was protecting:
- The original clause's strict "no path outside `<FEATURE>/`" condition would have implicitly also forbidden `CLAUDE.md`, `.claude/rules/**`, `scripts/**`, other `.cs`/`.csproj` files, and any `.ps1`/`.psm1`/`.psd1` file (all of these are "paths outside `<FEATURE>/` other than the csproj"). The revision does not remove this protection — it restates it explicitly as clauses (c) and (d), and clause (d) explicitly states the `.claude/agent-memory/**` exclusion does **not** apply to the `.ps1`/`.psm1`/`.psd1` check. A `.ps1` file placed under `.claude/agent-memory/**` would still fail clause (d).
- The narrowing is scoped to exactly one named, verified-benign path class (`.claude/agent-memory/**`), not a broad or vague exemption.
- Independently re-verified: the actual diff contains exactly two paths outside `<FEATURE>/` — the permitted csproj and the one memory-index append — confirming the carve-out was not exploited to hide anything else.

**Verdict: PASS.** The revision preserved original intent; it is not gate-softening. It correctly distinguishes a delegation byproduct (subagent memory write) from a scope violation, while explicitly re-affirming that the `.ps1`/`.psm1`/`.psd1` prohibition this remediation cycle exists to enforce is not subject to any carve-out.

## 4. Evidence Completeness for the Remediation Cycle — PASS

All 15 tasks in `remediation-plan.2026-08-10T23-45.md` are checked `[x]`. Every command-bearing task's artifact was read directly and confirmed to carry `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:` fields, with zero `EXIT_CODE: SKIPPED` occurrences:

| Task | Artifact | Timestamp | Command | EXIT_CODE | Output Summary |
|---|---|---|---|---|---|
| P0-T4 | `evidence/other/remediation-phase0-instructions-read.2026-08-10T23-45.md` | present | N/A (read-only task, documented) | N/A (no command) | present |
| P0-T5 | `evidence/remediation-baseline/pre-remediation-changed-languages.2026-08-10T23-45.md` | present | present | `0` | present |
| P0-T6 | `evidence/remediation-baseline/pre-remediation-spec-table.2026-08-10T23-45.md` | present | present | `0` | present |
| P1-T1 | `evidence/other/ps1-deletion.2026-08-10T23-45.md` | present | present (`git rm`) | `0` | present |
| P1-T2/T3/T4 | `spec.md` edits (verified directly, no separate evidence artifact required by the plan) | — | — | — | Analyzer=11, Reference=126, packages.config=105 present in current `spec.md` |
| P1-T5 | `evidence/qa-gates/analyzer-not-applicable.2026-08-10T23-45.md` | present | N/A (determination, documented as such) | N/A | present |
| P2-T1 | `evidence/qa-gates/post-remediation-changed-languages.2026-08-10T23-45.md` | present | present | `0` (both commands) | present |
| P2-T2 | `evidence/qa-gates/post-remediation-diff-scope.2026-08-10T23-45.md` | present (superseding re-run at `2026-08-11T03-13` recorded in the same file, post-revision) | present | `0` (all three commands) | present |
| P2-T3 | `evidence/qa-gates/no-csharp-rerun-rationale.2026-08-10T23-45.md` | present | N/A (determination, documented as such) | N/A | present |
| P2-T4 | `evidence/qa-gates/post-remediation-spec-table.2026-08-10T23-45.md` | present | present | `0` | present |

No `EXIT_CODE: SKIPPED` found anywhere in the remediation-cycle evidence tree.

**Verdict: PASS.**

## 5. Spec Figure Corrections — Verified Accurate — PASS

Cross-checked `spec.md`'s current "Duplicate Sweep Result" table against `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md`'s raw sweep output:

| Item type | `evidence/baseline/duplicate-sweep.2026-08-10T22-31.md` raw output | `spec.md` (current) | Match |
|---|---|---|---|
| Analyzer | `ItemType=Analyzer Total=11` | `11` | Yes |
| Reference | `ItemType=Reference Total=126` | `126` | Yes |
| packages.config | `packages.config Total=105` | `105` | Yes |

All three corrections are numerically accurate against the cited evidence source. The `Duplicates found` column is unchanged (`none`) for all three rows, consistent with the pre-remediation capture.

**Verdict: PASS.**

## 6. Coverage Verification (Mandatory, Per Language With Changed Files)

| Language | Changed files in diff | Verdict |
|---|---|---|
| TypeScript | 0 | N/A (no `.ts`/`.tsx` files changed) |
| Python | 0 | N/A (no `.py` files changed) |
| C# | 0 `.cs` files (only `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, a `.csproj`, is changed, and it is unchanged from the original fix commit) | N/A — no `.cs` source line is added, removed, or modified anywhere on the branch. Re-confirmed by direct `git diff --name-only` re-derivation this cycle. |
| PowerShell | **0** (the sole `.ps1` file, `duplicate-sweep.ps1`, is removed by this remediation cycle and confirmed absent from the current diff and working tree) | **N/A — closed.** Zero changed `.ps1`/`.psm1`/`.psd1` files remain in the branch diff; the mandatory coverage-verification obligation no longer applies. |

**New/changed-code coverage:** N/A — no `.cs`, `.ts`, `.py`, or `.ps1` source lines are added, removed, or modified anywhere in the current branch diff.

**Verdict: PASS (no language with changed files lacks a required coverage artifact).**

## 7. Nullable Gate — Non-Blocking (Accepted, Re-confirmed)

Zero `.cs` files changed anywhere on the branch (confirmed this cycle). `CLAUDE.md`'s `/p:Nullable=enable` command is a known repo-wide defect (issue #522) unrelated to this change. The CI-equivalent solution-level `TreatWarningsAsErrors` rebuild (`evidence/qa-gates/solution-rebuild.2026-08-10T22-31.md`, EXIT_CODE 0) remains the applicable substitute, captured against the original fix and unaffected by the remediation cycle (which touches zero `.cs`/`.csproj` content).

**Verdict: PASS (documented exception, non-blocking).**

## 8. CSharpier — Non-Blocking (Accepted, Re-confirmed)

Zero `.cs` files changed anywhere on the branch, including the remediation cycle.

**Verdict: PASS (N/A, non-blocking).**

## 9. Epic Scope Discipline — Re-Derived — PASS

`git diff origin/epic/build-ci-coverage-gate-fidelity-integration...HEAD --name-only` lists 33 files (up from 19 at the entry audit, reflecting the remediation cycle's added/edited evidence and plan artifacts). Direct inspection confirms:
- No `CLAUDE.md` in the diff.
- No path under `.claude/rules/**` in the diff.
- No path under `scripts/**` in the diff.
- No coverage-threshold value changed anywhere in the diff.
- The one path outside `<FEATURE>/` and the csproj (`.claude/agent-memory/feature-review/project_durable-feature-script-triggers-python-coverage-gate.md`) is a subagent memory-index append, an expected byproduct of delegation per this reaudit's own governing directive, and is correctly not treated as a scope violation.

**Verdict: PASS.**

## 10. Evidence Location Compliance

`git diff --name-only` scanned for any path under `artifacts/baselines/`, `artifacts/qa/`, `artifacts/evidence/`, or `artifacts/coverage/`. **None found.** Every evidence artifact added or modified in the remediation cycle is under the canonical `<FEATURE>/evidence/{baseline,qa-gates,other,remediation-baseline}/` scheme. `validate_evidence_locations.py` does not exist in this repository; this finding is based on direct manual path-pattern inspection of the full 33-file changed-path list.

**Verdict: PASS.**

## Findings Summary

| # | Severity | Area | Finding | Blocking? |
|---|---|---|---|---|
| — | — | — | No findings raised in this reaudit. The entry audit's single BLOCKING finding (PowerShell coverage-artifact absence) is confirmed closed. | No |

**Total BLOCKING findings in this artifact: 0.**

## Verdict

**PASS.** The remediation cycle closes the entry audit's sole BLOCKING finding by removing the unplanned PowerShell file from the branch, independently re-derived from the diff and working tree (not merely from the remediation evidence's own narrative). The underlying CS2002 fix is unregressed. The `[P2-T2]` mid-execution acceptance revision is judged legitimate: it carves out exactly one verified-benign path class (a subagent memory-index append) while explicitly re-affirming, and in fact making more explicit, every substantive prohibition the original clause protected — including the `.ps1`/`.psm1`/`.psd1` check this remediation cycle exists to satisfy. All 15 remediation-plan tasks have complete, non-SKIPPED evidence. The three corrected `spec.md` figures are numerically accurate against their cited evidence source. No new BLOCKING finding is introduced by the remediation cycle.
