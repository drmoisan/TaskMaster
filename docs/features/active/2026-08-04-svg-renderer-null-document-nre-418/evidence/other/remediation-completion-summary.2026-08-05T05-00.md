# Remediation Cycle 2 — Completion Summary

- Task: `[P2-T12]`
- Issue: #418
- Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`
- Evidence series: `2026-08-05T05-00`
- Timestamp: 2026-08-05T00-30
- Plan of record: `remediation-plan.2026-08-05T05-00.md` — **30 of 30 tasks complete**
- Cycle inputs: `remediation-inputs.2026-08-04T22-28.md`
- Work mode: `minor-audit`

## 1. Items delivered

| Item | Inputs label | Directive label | Delivering tasks | Outcome | Evidence artifact |
|---|---|---|---|---|---|
| Missing `ExCSS` reference on `SVGControl.Test` (the blocking finding) | **R-7** | **R-11** | `[P1-T1]` (`<Reference>` block, 4 lines), `[P1-T2]` (`packages.config` entry, 1 line) | **DELIVERED.** `ExCSS.dll` now copies into `SVGControl.Test/bin/Debug`; order dependence closed | `evidence/other/excss-copy-local.2026-08-05T05-00.md`; `evidence/qa-gates/order-independence.2026-08-05T05-00.md` |
| `<Private>True</Private>` on the existing `Svg` reference | **R-11** | — | `[P1-T3]` (1 line) | **DELIVERED.** Behavior-preserving; `Svg.dll` still present in the output | `evidence/other/excss-copy-local.2026-08-05T05-00.md` § 3 |

The label collision is recorded because the inputs and the orchestrator directive use **R-11** for
different items. Both readings were delivered; both fall inside the same Scope Lock and edit the same
`<ItemGroup>`, so neither can conflict with the other. Reconciliation:
`evidence/remediation-baseline/cycle-inputs-read.2026-08-05T05-00.md`.

**Total functional change: two build-configuration files, six added lines, zero removed.**
`SVGControl.Test/SVGControl.Test.csproj` (+5/−0) and `SVGControl.Test/packages.config` (+1/−0). **No `.cs`
file was changed anywhere in the repository.** Verified at `[P1-T7]`:
`evidence/other/scope-guard.2026-08-05T05-00.md`.

## 2. Before / after order-dependence table

| Run shape | Before | After | Failed delta |
|---|---|---|---|
| **Standalone** `SVGControl.Test.dll` | exit 1 — 75 total, 69 passed, **6 failed** | exit 0 — **75 total, 75 passed, 0 failed** | **−6** |
| **`SVGControl.Test` first**, `VBFunctions.Test` second | exit 1 — 76 total, 70 passed, **6 failed** | exit 0 — **76 total, 76 passed, 0 failed** | **−6** |
| `VBFunctions.Test` first, `SVGControl.Test` second | exit 0 — 76/76/0 | not re-run: it passed before the fix and cannot discriminate | 0 |
| Nine-assembly wrapper | 6150/6150, 0 failed | 6150/6150, 0 failed | 0 — passes either way |

Before sources: `evidence/remediation-baseline/order-standalone.2026-08-05T05-00.md` (`[P0-T7]`),
`evidence/remediation-baseline/order-paired.2026-08-05T05-00.md` (`[P0-T8]`). After sources:
`evidence/regression-testing/order-standalone-after.2026-08-05T05-00.md` (`[P1-T5]`),
`evidence/regression-testing/order-paired-after.2026-08-05T05-00.md` (`[P1-T6]`),
`evidence/qa-gates/order-independence.2026-08-05T05-00.md` (`[P2-T9]`, inside the clean pass).

All runs held switch parity: no `/EnableCodeCoverage`, no `/InIsolation`, no `/Settings`. Test outcomes
are now invariant under assembly ordering, closing **G-8** and the code review's single **Blocking**
finding (labelled **CR-8** by the inputs and the feature audit). AC-10's stated objective is achievable in
the standalone host.

## 3. R-1 remains open

- **R-1 / AC-11 is NOT delivered and remains open.** It was represented by no task in this plan.
- **It is human-only.** Opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the legacy in-process Visual
  Studio WinForms designer has no unattended automation surface. No agent can execute it, and assigning it
  to one would produce a false capture.
- **It is tracked as ratified human-interaction requirements H-1 and H-2**, both with
  `response: "exception"` and a non-empty `runbook_path`. Verified at the point of writing this artifact by
  reading `artifacts/orchestration/orchestrator-state.json`:

  ```
  H-1 | exception | .../runbooks/verify-winforms-designer-load.runbook.md | satisfies AC-11
  H-2 | exception | .../runbooks/verify-winforms-designer-load.runbook.md | satisfies AC-7
  ```

  Both satisfy the `.claude/rules/orchestrator-state.md` invariant that an `exception` response carry a
  non-empty `runbook_path`.
- **AC-11 is still `- [ ]`** at `issue.md:112`, verified after the `[P2-T11]` edit. It may be checked off
  only after a human capture exists at
  `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`, or after an explicit maintainer
  waiver recorded in the orchestrator-state `human_interaction` block.

## 4. Items deliberately not addressed

Each with the one-line reason from this plan's § Explicitly excluded.

| Item | Reason not addressed |
|---|---|
| **G-9** — `SVGControl/SvgAssemblyResolver.cs` file-level coverage floor (61.6279%) | Dispositioned non-blocking by the reviewer, who stated it needs a **maintainer decision rather than code**. Surfaced to the user, not remediated. No task targets it, and no testable member was relocated into that file to lift its ratio. |
| **G-1** — `SVGControl/SvgRenderer.cs` file-level floor (80.1932%) | Residual is pre-existing members outside issue #418, already owned by `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` (verified present on disk). |
| **R-12** — repository-level nullable-gate vacuity | Not this feature's to fix. Re-observed this cycle at `[P2-T6]` (18/18 `CoreCompile` skipped, 0 `csc.exe`) and disclosed rather than glossed. |
| **R-8** — complete the `SvgAssemblyResolver` separation | Modifies production `.cs` files, which the Scope Lock forbids. |
| **R-9** — correct the resolver's diagnostic message prefixes | Modifies production `.cs` files, which the Scope Lock forbids. |
| **R-10** — remove the duplicated byte-array constructor bodies | Modifies production `.cs` files, which the Scope Lock forbids. |

Both file-level floors are recorded as **not targeted this cycle** in
`evidence/qa-gates/coverage-delta.2026-08-05T05-00.md` § 6, with their owning entries named.

## 5. The decision not to add a `Fizzler` reference, with its four measured grounds

`remediation-inputs.2026-08-04T22-28.md` § R-7 directs adding a `Fizzler` reference "for parity with the
eight sibling test projects". **This plan deliberately omitted it**, per Design Decision 3. All four
grounds were re-measured at `[P0-T9]` rather than transcribed —
`evidence/remediation-baseline/reference-census.2026-08-05T05-00.md`:

1. **No test project references `Fizzler`.** `git ls-files '*.csproj' | xargs grep 'Reference Include="Fizzler'`
   returns exactly two matches, `SVGControl/SVGControl.csproj:58` and `UtilitiesCS/UtilitiesCS.csproj:63`,
   both **production**. The `packages.config` search returns the same two projects.
2. **No test project's output contains `Fizzler.dll`.** The glob `*.Test/bin/Debug/Fizzler.dll` returns
   **0** files, while `*.Test/bin/Debug/ExCSS.dll` returns **8**. Adding `Fizzler` would make
   `SVGControl.Test` the **only** test project carrying it — divergence from the siblings, not parity.
3. **`Fizzler` is empirically unnecessary.** The passing ordering probes a directory containing `ExCSS.dll`
   and **no** `Fizzler.dll`, so `ExCSS` alone is sufficient. Confirmed by outcome: `[P1-T5]` reached
   **75/75 with `ExCSS` alone**, so the halt-rather-than-expand contingency was never triggered.
4. **Adding it carries a real risk.** The on-disk `Fizzler` identity is **`Version=1.3.1.0`** — measured,
   and **contradicting the inputs' snippet, which states `Version=1.3.0.0`** — while
   `SVGControl.Test/app.config` redirects `Fizzler` to `1.3.0.0`. Placing a `1.3.1.0` assembly into that
   output directory would activate a stale redirect that is inert today only because no `Fizzler.dll` is
   present. That latent defect stays owned by
   `docs/features/potential/2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md` (verified present on
   disk). The only remedies would be an `app.config` edit — forbidden by the binding `## Do Not Do` list —
   or reverting the reference.

`[P1-T4]` and `[P2-T6]` both confirm `Fizzler.dll` remains **absent** from `SVGControl.Test/bin/Debug`,
including after a clean `/t:Rebuild`.

## 6. `[P2-T5]` disposition outcome

Artifact: `evidence/qa-gates/reference-resolution-disposition.2026-08-05T05-00.md`.

- **Added diagnostics: 0.** In particular **zero** `MSB3243`, `MSB3245`, or `MSB3277` lines for
  `SVGControl.Test` or any project, in both the `[P2-T4]` build and the genuinely-recompiling `[P1-T4]`
  build. The plan's accepted-with-evidence escalation clause was conditional on such a line existing, so
  **there is nothing to escalate to the orchestrator on that account.** The absence is explained: the added
  identity matches the deployed assembly exactly on name, version, and public key token, and the file
  exists, so `ResolveAssemblyReference` had no mismatch, missing file, or conflict to report.
- **Removed diagnostics: 1** — `CS2002` in `UtilitiesCS.Test`
  (`Source file '...\PercentageFormatterTests.cs' specified multiple times`). Dispositioned **expected and
  non-regressive**: `CS2002` is `CoreCompile`-gated and the `[P2-T4]` run executed **0** `csc.exe`
  invocations with all 18 `CoreCompile` targets skipped, so its emitting project did not recompile. The
  underlying duplicate `<Compile>` item is untouched and latent; `UtilitiesCS.Test` appears nowhere in this
  cycle's diff. **No fix required, no loop restart.**
- Totals reconcile: basis 6 − 1 removed + 0 added = **5**, the measured figure.
- None of the three forbidden responses was taken: no `app.config` edit, no `<NoWarn>`, no reference
  removal.

## 7. Neither prior plan file was modified by this cycle

```
Command: git diff --stat HEAD -- docs/features/active/.../plan.2026-08-04T14-36.md docs/features/active/.../remediation-plan.2026-08-05T01-50.md
Output:  (empty)
```

**Both `plan.2026-08-04T14-36.md` (complete at 46/46) and `remediation-plan.2026-08-05T01-50.md` (complete
at 40/40) show an empty diff.** They were read-only for the whole of this cycle, as `[P0-T5]` invariant (b)
established at entry and this task re-confirms at exit.

Complete `git diff --stat HEAD` at cycle exit — four files, and neither prior plan is among them:

```
 SVGControl.Test/SVGControl.Test.csproj             |  5 ++
 SVGControl.Test/packages.config                    |  1 +
 .../issue.md                                       |  2 +
 .../remediation-plan.2026-08-05T05-00.md           | 58 +++++++++++-----------
 4 files changed, 37 insertions(+), 29 deletions(-)
```

Reconciliation: 5 + 1 + 2 functional/documentation insertions = 8, plus 29 checkbox-flip line replacements
(29 insertions + 29 deletions) = **37 insertions, 29 deletions**. Twenty-nine check-offs at the time of
this measurement, with `[P2-T12]` itself the thirtieth and final.

## 8. Toolchain result

`evidence/qa-gates/toolchain-clean-pass.2026-08-05T05-00.md` records **`Pass number: 1`** with **no loop
restart**. All six mandated commands returned `EXIT_CODE: 0` in `CLAUDE.md` order, plus two supplementary
forced `/t:Rebuild` project-scope runs at `EXIT_CODE: 0` with 0 diagnostics each.

| Gate | Result |
|---|---|
| csharpier format | exit 0, **0 files reformatted** |
| csharpier check | exit 0, **0 files needing formatting** |
| Restore | exit 0, no `packages/` mutation |
| Analyzer build | exit 0, **0 errors**, 5 warnings, **0 added diagnostics** |
| Nullable (mandated) | exit 0 — **vacuous, disclosed** |
| Nullable (forced `SVGControl.Test`, `SVGControl`) | exit 0 each, **0 diagnostics** each |
| Tests + coverage | 9 assemblies, **6150/6150 passed, 0 failed** |
| Repository line coverage | **85.4006%** (93529/109518) — **PASS** vs `>= 85%` |
| Repository branch coverage | **78.6928%** (21576/27418) — **PASS** vs `>= 75%` |
| Order independence | **0 failed in both** the standalone (75/75) and the previously failing pair (76/76) |

## 9. Evidence artifacts produced by this cycle — 26, all canonical

All under `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`. **Zero**
under `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`,
`artifacts/coverage/`, or `artifacts/evidence/`. No `EVIDENCE_LOCATION_OVERRIDE_REJECTED` event occurred:
the cycle inputs and the execution directive supplied only canonical paths.

| Kind | Count | Artifacts |
|---|---|---|
| `remediation-baseline/` | 11 | `toolchain-bootstrap`, `phase0-instructions-read`, `ac-source-check`, `cycle-inputs-read`, `tree-state`, `vstest-path`, `order-standalone`, `order-paired`, `reference-census`, `build-basis`, `coverage-basis` |
| `regression-testing/` | 2 | `order-standalone-after`, `order-paired-after` |
| `qa-gates/` | 10 | `csharpier-format`, `csharpier-check`, `restore`, `analyzer-build`, `reference-resolution-disposition`, `nullable-build`, `test-coverage`, `coverage-delta`, `order-independence`, `toolchain-clean-pass` |
| `other/` | 3 | `excss-copy-local`, `scope-guard`, `remediation-completion-summary` (this file) |
| `issue-updates/` | 1 | `issue-418` |

Every row in this summary cites an artifact verified present on disk.

## 10. Checkbox state matches the evidence recorded

All 30 tasks in `remediation-plan.2026-08-05T05-00.md` are `[x]`, and each was checked off only after its
acceptance criteria were verified and its artifact written. The two `[expect-fail]` tasks, `[P0-T7]` and
`[P0-T8]`, carry their `[expect-fail]` evidence: a non-zero exit code was the expected measurement outcome,
the defect reproduced on this host at the exact expected counts, and both artifacts declare the
`[expect-fail]` status explicitly.

## 11. Cycle exit condition

This **plan** is complete: 30/30 tasks, and all three of its § Exit Criteria are satisfied —
`order-independence.2026-08-05T05-00.md` records 75/75 standalone and 0 failed for the
`SVGControl.Test`-first pair; `toolchain-clean-pass.2026-08-05T05-00.md` records one uninterrupted clean
pass of all six mandated commands; `scope-guard.2026-08-05T05-00.md` confirms two modified tracked
functional files with no `.cs` and no `app.config` change.

The **cycle** exit condition (`blocking_count == 0`) is **not** satisfied by this plan alone. Of the two
blocking findings at entry:

| Blocking finding | Status |
|---|---|
| **G-8 / CR-8** — test order dependence | **CLOSED** by this cycle |
| **G-2 / R-1 / AC-11** — human designer-load runbook | **OPEN.** Requires a human operator session or an explicit maintainer waiver. Outside this plan; no task here could satisfy it. |

## Output Summary

Remediation cycle 2 delivered both readings of the label-colliding item: the missing `ExCSS` reference on
`SVGControl.Test` (inputs R-7 / directive R-11, via `[P1-T1]` and `[P1-T2]`) and `<Private>True</Private>`
on the existing `Svg` reference (inputs R-11, via `[P1-T3]`). Total functional change is **six added lines
across two build-configuration files, with no `.cs` file touched anywhere**. Order dependence is closed:
the standalone `SVGControl.Test` run moved from 75/69/**6 failed** to **75/75/0**, and the previously
failing `SVGControl.Test`-first pair from 76/70/**6 failed** to **76/76/0**, at switch parity. The
toolchain completed in **one clean pass with no restart**, with repository line coverage at **85.4006%** and
branch at **78.6928%**, both PASS, and every `SVGControl` coverage figure byte-identical to the basis.
`[P2-T5]` recorded zero added diagnostics — so no `MSB3243`/`MSB3245`/`MSB3277` escalation exists — and one
expected `CoreCompile`-gated `CS2002` removal. A `Fizzler` reference was deliberately **not** added on four
re-measured grounds, one of which contradicts the cycle inputs (the on-disk identity is `1.3.1.0`, not
`1.3.0.0`). **R-1 / AC-11 remains open, is human-only, is tracked as H-1 and H-2 with `response: exception`
and a verified `runbook_path`, and AC-11 is still `- [ ]`.** G-9, G-1, R-8, R-9, R-10, and R-12 were
deliberately not addressed, each with its recorded reason. Neither `plan.2026-08-04T14-36.md` nor
`remediation-plan.2026-08-05T01-50.md` was modified — both show an empty `git diff --stat HEAD`.
