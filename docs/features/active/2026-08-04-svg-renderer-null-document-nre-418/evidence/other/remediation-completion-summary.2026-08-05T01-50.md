# Remediation Cycle 1 — Completion Summary

- Task: `[P2-T11]`
- Issue: #418
- Plan of record: `remediation-plan.2026-08-05T01-50.md` (40 tasks: `[P0-T1]`–`[P0-T10]`, `[P1-T1]`–`[P1-T19]`, `[P2-T1]`–`[P2-T11]`)
- Branch / HEAD at cycle entry: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T02-14 (UTC)

## Item-by-item exit state, R-2 through R-6

| Item | Delivering task IDs | Outcome | Evidence artifact |
|---|---|---|---|
| **R-2** — `<LangVersion>` on `SVGControl.Test.csproj` (CR-1 / G-3) | `[P1-T5]`, `[P1-T6]`, `[P1-T7]`, `[P1-T8]`, `[P1-T9]` | **DELIVERED — gate token `R2_KEEP`.** `<LangVersion>latest</LangVersion>` added and **retained**. The pre-existing `CS8630` is eliminated: the forced project-scope nullable rebuild goes from `EXIT_CODE: 1` with one `CS8630` to `EXIT_CODE: 0` with zero diagnostics. The 24 nullable diagnostics that real analysis then surfaced (21 `CS8600`, 3 `CS8625`) were all in-scope and all cleared, plus a 15-diagnostic `CS8632` follow-up cleared by adding `#nullable enable` to the three test files. | `evidence/other/langversion-probe.2026-08-05T01-50.md`, `evidence/other/langversion-gate.2026-08-05T01-50.md`, `evidence/qa-gates/nullable-build.2026-08-05T01-50.md` |
| **R-3** — exception containment in the resolve handler (CR-2) | `[P1-T10]`, `[P1-T11]`, `[P1-T12]`, `[P1-T13]` | **DELIVERED, both parts.** One `catch (Exception ex)` with a `Trace.TraceWarning` body added to the outer `try` (now exactly one catch and one finally), newly containing `Path.Combine`, `self.Location`, and `self.CodeBase`. The `Path.GetInvalidPathChars()` filter applied to the third `GetProbeDirectories` candidate. One new test proves the drop-without-throwing behavior and passes. Known residual recorded: the pre-guard region stays outside the new catch, per Design Decision 11. | `evidence/other/resolver-containment.2026-08-05T01-50.md` |
| **R-4** — two targeted coverage items (CR-5, CR-6) | `[P1-T1]` (CR-6 accessibility), `[P1-T14]` (CR-5 test), `[P1-T15]` (CR-6 tests), `[P1-T18]` (residual entry) | **DELIVERED, both items.** `PublicKeyTokensEqual` relocated to `SvgAssemblyProbe` as `internal static` and taken from **0/15 = 0%** to **15/15 = 100%** line-rate (18/18 = 100% branch) by eight tests. The three-argument byte-array constructor taken from **13/17 = 76.471%** to **17/17 = 100%** by one test. `SVGControl/SvgRenderer.cs` rose 72.109% -> **80.1932%**. The 85% modified-file floor was deliberately not targeted, per R-4's scope boundary, and its residual is filed. | `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md`, `evidence/regression-testing/remediation-tests.2026-08-05T01-50.md`, `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` |
| **R-5** — stale and overbroad comments (CR-4, CR-7) | `[P1-T16]`, `[P1-T17]` | **DELIVERED, both.** The resolver header comment no longer references `4.2.3.0` or `4.3.1.0`, states the delivered `Svg 3.4.8` / `ExCSS 4.3.2` pins, attributes the redirect-ignoring host to **`devenv.exe`** (not the vstest testhost), and cites the research artifact by path. The test Arrange comment's universal claim "No plain byte payload reaches it" is replaced by the measured statement and names open question **U-3**. No code changed; no coverage figure moved for either. | `evidence/qa-gates/csharpier-check.2026-08-05T01-50.md` (both tasks gated on `csharpier check` at exit 0), `evidence/other/remediation-completion-summary.2026-08-05T01-50.md` (this file) |
| **R-6** — reduce `SvgRenderer.cs` below the 500-line pressure point (CR-3) | `[P1-T1]`, `[P1-T2]`, `[P1-T3]`, `[P1-T4]` | **DELIVERED as a pure move.** `SVGControl/SvgRenderer.cs` **497 -> 362 lines** (138 of headroom, and within the plan's "at most 400"). New `SVGControl/SvgAssemblyResolver.cs` at 157 lines; `SVGControl/SvgAssemblyProbe.cs` 67 -> 93. Only the three permitted deltas occurred. `SvgRenderer`'s static constructor is retained and calls `SvgAssemblyResolver.Install()`, so the AC-8 resolver still installs. | `evidence/other/resolver-extraction.2026-08-05T01-50.md`, `evidence/qa-gates/file-size.2026-08-05T01-50.md` |

## The `[P1-T7]` gate token and what it means for R-2

```
R2_KEEP
```

The measurement was **non-vacuous** — the `SVGControl` project-reference diagnostic set was empty and
`SVGControl.Test` reached its own `CoreCompile`, proven by the 24 diagnostics emitted from its own three
source files — and the **out-of-scope set was empty (0 diagnostics)**. Branch A's condition was therefore
satisfied and Branch B's was not, so Branch A was taken. No vacuity re-run was needed.

Meaning for R-2: the fix is **permanent, not reverted**. `SVGControl.Test` now declares
`<LangVersion>latest</LangVersion>`, nullable analysis genuinely runs on it, and it compiles clean under the
mandated `/p:Nullable=enable /p:TreatWarningsAsErrors=true` property set at forced-recompile scope. The
branch's only newly reachable type-check diagnostic is eliminated at source rather than deferred.

The plan's § Risks item 1 predicted the revert branch on the basis of source inspection of
`Form1.Designer.cs`, `Form2.Designer.cs`, and `Resources.Designer.cs`. Both predicted conditions do exist
in the source and were confirmed present, but they emit nothing because **Roslyn suppresses nullable
diagnostics in generated code** (all four `*.Designer.cs` files match the compiler's generated-code
detection). The prediction did not account for that suppression; the gate is deterministic on the measured
partition, so the measured result governs.

`[P2-T5]`'s `SVGControl.Test` supplementary diagnostic set is **zero diagnostics**, which is exactly what
the plan requires under the `R2_KEEP` token. Exit-criterion 5 is satisfied.

## R-1 remains open

- R-1 is **AC-11 — Designer load verified by the documented human step**, the only blocking item from
  `feature-review`'s PARTIAL verdict.
- It is **not represented by any task in this plan** and was not attempted.
- It is **human-only**: it requires opening `UtilitiesCS/Dialogs/MyBoxViewer.cs` in the Visual Studio
  WinForms designer and observing the load, following
  `runbooks/verify-winforms-designer-load.runbook.md`. No agent can execute it and no automated evidence
  substitutes for the human capture at
  `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`, which does not exist.
- It is tracked as **human_interaction requirements H-1 and H-2 with response `exception`** and a runbook
  path, and it resolves only when the user runs the runbook.
- **AC-11 is still `- [ ]`** at `issue.md:110`. Verified: `grep -oE "^- \[[ x]\] \*\*AC-[0-9]+"` returns
  `[x]` for AC-1 through AC-10 and **`- [ ] **AC-11`**. `git diff -U0 -- issue.md` changed **zero**
  checkbox lines.

This cycle therefore **cannot** clear the blocking count. It clears the five non-blocking items and leaves
the single blocking item where only the user can close it, exactly as the plan states.

## `docs/features/potential/` entries

| Entry | Created? | Reason |
|---|---|---|
| `docs/features/potential/2026-08-05-svgcontrol-coverage-uplift.md` | **created** (`[P1-T18]`) | Owns the coverage residual R-4 deliberately does not close: on `SVGControl/SvgRenderer.cs`, `AddMargins` 0/15, `Render()` 18/26, and the two `SvgDocument` constructor overloads 0/8 each; and in the rest of the assembly, `DropDownEditor` 0/99, `SVGParser` 0/122, `ToggleSwitch` 0/62 plus 0/23 designer, `SvgFileNameEditor` 0/104, and three converters at 0/48, 0/48, 0/26. Names issue #418 as its origin and cites `evidence/qa-gates/coverage-delta.2026-08-05T01-50.md` as the measurement source. |
| `docs/features/potential/2026-08-05-test-project-langversion-alignment.md` | **deliberately not created** (`[P1-T9]`) | `[P1-T9]` is Branch-B-only and `[P1-T7]` recorded `R2_KEEP`, so the task's own text directs "create no file". The repository-wide context that entry would have carried is recorded instead in `evidence/other/langversion-gate.2026-08-05T01-50.md` § `[P1-T9]`: five test projects (`QuickFiler.Test`, `Tags.Test`, `TaskTree.Test`, `TaskVisualization.Test`, `ToDoModel.Test`) still lack `<LangVersion>` against three that set it, down from six because R-2 removed `SVGControl.Test` from that set. |

## The completed plan file was not modified

Command: `git diff --stat HEAD -- docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`

EXIT_CODE: 0

Output: **empty** (no lines emitted).

`plan.2026-08-04T14-36.md` is byte-identical to its committed state at `ea106111`. It was read-only for
the whole of this cycle: `[P0-T10]` recorded an empty diff at entry and this task re-confirms an empty diff
at exit. It was cited only as a reference for Design Decisions 1 through 12 and for the ratified
`COVERAGE_MEMBER_UNREACHABLE` exception. Exit-criterion 6 is satisfied.

## Exit criteria

| # | Criterion | Status |
|---|---|---|
| 1 | Every task in Phases 0, 1, 2 is `[x]` with its artifact on disk | **MET** — 40/40 checked off; 25 artifacts in the `2026-08-05T01-50` series on disk |
| 2 | `[P2-T9]` records one consecutive clean pass with `EXIT_CODE: 0` at all six stages | **MET** — `Pass number: 1`, no restart |
| 3 | Repo line `>= 85%`, branch `>= 75%`, no regression on changed lines, `Install()` `>= 90%` line-rate | **MET** — 85.4097%, 78.7220%, no line lost coverage, `Install()` 100% |
| 4 | `SvgRenderer.cs` at most 400 lines and no file above 500 | **MET** — 362; largest of the six is 358 |
| 5 | `[P1-T7]` recorded a literal gate token and `[P2-T5]`'s `SVGControl.Test` set matches it | **MET** — `R2_KEEP` / zero diagnostics |
| 6 | AC-11 still `- [ ]` and `plan.2026-08-04T14-36.md` unmodified | **MET** — both verified above |
| 7 | Reaudit input set present | **MET** — see below |

## Reaudit input set

All present on disk:

- `evidence/qa-gates/*.2026-08-05T01-50.md` — `csharpier-format`, `csharpier-check`, `restore`,
  `analyzer-build`, `nullable-build`, `test-coverage`, `coverage-delta`, `file-size`,
  `toolchain-clean-pass` (9 artifacts)
- `evidence/other/*.2026-08-05T01-50.md` — `resolver-extraction`, `langversion-probe`, `langversion-gate`,
  `resolver-containment`, `remediation-completion-summary` (5 artifacts)
- `evidence/regression-testing/remediation-tests.2026-08-05T01-50.md` (1 artifact)
- `evidence/issue-updates/issue-418.2026-08-05T01-50.md` (1 artifact)

Plus the Phase 0 baseline set, `evidence/remediation-baseline/*.2026-08-05T01-50.md` (10 artifacts), which
supplies the before-state for every comparison in this cycle.

**No existing artifact was overwritten.** Every artifact this cycle wrote carries the `2026-08-05T01-50`
stamp; the `2026-08-04T14-36` and `2026-08-04T21-04` series are untouched.

## Checkbox state matches the evidence

Every one of the 40 tasks in `remediation-plan.2026-08-05T01-50.md` is `[x]`, and every command-bearing
task's artifact exists on disk carrying `Timestamp:`, `Command:`, `EXIT_CODE:`, and `Output Summary:`. No
task was marked complete without its artifact, and no `EXIT_CODE: SKIPPED` appears anywhere in the series.

## Output Summary

All five non-blocking items **R-2 through R-6 are delivered** and evidenced. R-2 ended on
**Branch A, `R2_KEEP`** — the `<LangVersion>` fix is permanent and `CS8630` is eliminated. The toolchain
passed in **one consecutive clean pass** with no restart. Repository coverage improved on both metrics and
both floors pass. `SVGControl/SvgRenderer.cs` is at **362** lines and no Scope Lock file exceeds 500.
**R-1 / AC-11 remains open, is human-only, and AC-11 is still `- [ ]`.** `plan.2026-08-04T14-36.md` is
unmodified. This cycle cannot clear the blocking count of 1.
