# [P2-T12] Plan Completion Summary — Issue #418

Timestamp: 2026-08-04T20-06

Plan of record: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` (version 0.9)
AC source: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/issue.md`, `## Acceptance Criteria`
Work Mode: `minor-audit`
Branch: `bug/svg-renderer-null-document-nre-418`

## Plan checkbox reconciliation

| Phase | Tasks | Checked `[x]` | Unchecked `[ ]` |
|---|---|---|---|
| Phase 0 — Baseline Capture and Compliance Reads | 10 | 10 | 0 |
| Phase 1 — Constrained Small-Path Implementation | 24 | 24 | 0 |
| Phase 2 — Final QC Loop | 12 | 12 | 0 |
| **Total** | **46** | **46** | **0** |

Every `- [ ]` task in the plan file that was completed has been changed to `- [x]`. No task remains
unchecked. Task counts match the version 0.9 header's stated 10 / 24 / 12 = 46.

## Acceptance criteria reconciliation, AC-1 through AC-11

| AC | State in `issue.md` | Supporting evidence artifact |
|---|---|---|
| AC-1 — Failing regression test exists first | `[x]` | `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` (4 failed, `NullReferenceException` at `SvgRenderer.cs:133`); `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` (6139/6139 passed) |
| AC-2 — No silent exception swallow | `[x]` | `evidence/qa-gates/svgrenderer-file-size.2026-08-04T14-36.md`; `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` |
| AC-3 — Parse failure degrades visibly instead of throwing NRE | `[x]` | `evidence/regression-testing/ac1-fail-before.2026-08-04T14-36.md` and `ac1-pass-after.2026-08-04T14-36.md` (the four constructor tests, both overloads, malformed and empty payloads) |
| AC-4 — Fail-fast API exists; null-tolerant call sites keep their contract | `[x]` | `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` (nine parse-surface tests plus five `SvgRendererNullToleranceTests`) |
| **AC-5 — Coverage on changed code** | **`[x]` (checked off by `[P2-T10]`)** | `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md`; supported by `evidence/qa-gates/test-coverage.2026-08-04T14-36.md` and `evidence/qa-gates/coverage-gap-closure.2026-08-04T14-36.md` |
| **AC-6 — Toolchain passes in a single clean pass** | **`[x]` (checked off by `[P2-T10]`)** | `evidence/qa-gates/toolchain-clean-pass.2026-08-04T14-36.md` (`Pass number: 1`, no restart); per-step artifacts `csharpier-format`, `csharpier-check`, `restore`, `analyzer-build`, `nullable-build`, `test-coverage`, all `.2026-08-04T14-36.md` |
| AC-7 — Underlying failure identified in writing | `[x]` | `research/2026-08-04T15-05-svg-renderer-null-document-research.md`; corroborated in `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` |
| AC-8 — `AssemblyResolve` fallback resolves from the assembly's own directory | `[x]` | `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` (nine `SvgAssemblyProbeDirectoryTests`); coverage in `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` (both `SvgAssemblyProbe` helpers 100%) |
| AC-9 — `SVGControl.Test` builds and runs | `[x]` | `evidence/qa-gates/svgcontrol-test-build.2026-08-04T14-36.md`; `evidence/other/package-restore-decision.2026-08-04T14-36.md`; `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` |
| AC-10 — Incorrect ExCSS redirect in the test config is corrected | `[x]` | `SVGControl.Test/app.config:23` now `4.3.2.0`; corroborated in `evidence/regression-testing/ac1-pass-after.2026-08-04T14-36.md` |
| **AC-11 — Designer load verified by the documented human step** | **`[ ]` — intentionally unchecked** | `evidence/other/ac11-runbook-handoff.2026-08-04T14-36.md`; awaiting `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md` |

Totals: **11 acceptance criteria, 10 checked off, 1 remaining.**

### AC-11 is intentionally unchecked pending the human runbook

AC-11 is satisfied only by a human executing
`runbooks/verify-winforms-designer-load.runbook.md` against a live Visual Studio WinForms designer host.
The plan's Work-Mode Notes state that AC-11 is not an executable task and that the executor must leave
`- [ ] **AC-11 ...` unchecked; `[P2-T11]` is a record-only handoff task, and its acceptance clause
requires that `- [ ] **AC-11` still be present. The executor did not automate it and did not check it
off. This is a deliberate, documented outstanding item, not an omission.

## Phase 2 result summary

| Gate | Result |
|---|---|
| `csharpier format` | `EXIT_CODE: 0`, **0 files reformatted** |
| `csharpier check` | `EXIT_CODE: 0`, **0 files need formatting** (1466 files) |
| Restore | `EXIT_CODE: 0`, 0 errors, 0 warnings |
| Analyzer build | `EXIT_CODE: 0`, **0 errors, 6 warnings** — identical to baseline |
| Nullable / `TreatWarningsAsErrors` build | `EXIT_CODE: 0`, **0 errors, 5 warnings** — identical to baseline |
| Test + coverage | `EXIT_CODE: 0`, **6140 / 6140 passed, 0 failed, 0 skipped**, 9 assemblies |
| Repo-wide line coverage | **85.3844%** (93484 / 109486) vs `>= 85%` floor — PASS, improved from 85.3550% |
| Repo-wide branch coverage | **78.5521%** (21528 / 27406) vs `>= 75%` floor — PASS, improved from 78.5353% |
| Newly added members | **7 of 7 at 100.000% `line-rate`** vs `>= 90%` gate — PASS |
| No regression on changed lines | **yes** — `SVGControl.SvgRenderer` 62.559% -> 72.109% |

## File-size compliance, all five in-scope C# files

| File | Lines | `<= 500` |
|---|---|---|
| `SVGControl/SvgRenderer.cs` | 497 | yes |
| `SVGControl/SvgAssemblyProbe.cs` | 67 | yes |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | 332 | yes |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | 143 | yes |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | 187 | yes |

Measured after formatting in `[P2-T3]`.

## Reports to the orchestrator

- `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey` — the ratified named
  exception for host-bound `AssemblyResolve` wiring. Measured 68.116% `line-rate`; all of its new
  decision logic lives in `SVGControl.SvgAssemblyProbe`, which is at 100%.
- `COVERAGE_DENOMINATOR_CHANGE` — **not reported.** The fallback decision rule did not fire; both
  repo-wide metrics improved and both floors are met.

## Outstanding work

1. **AC-11**: human execution of `runbooks/verify-winforms-designer-load.runbook.md`, with the capture
   written to `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`. Owner: human operator.
2. **Deferred follow-up issues named by the plan's Scope Lock, all out of scope here**: the Fizzler
   binding redirects (research §5.3, latent and currently inert); the diverged
   `System.Runtime.CompilerServices.Unsafe` redirect in `SVGControl/app.config`; the
   `scripts/vscode/Invoke-MSTest.ps1` scalar-`.Count` defect that blocks the single-assembly
   `-SearchRoot` form; and open question U-3, whether a well-formed-XML-but-no-SVG-element payload
   reaches `SvgDocument.Open`'s null-returning path.
3. **Coverage artifact format for the downstream reduced audit**: this plan's toolchain emits Cobertura
   at `coverage/coverage.cobertura.xml`, while `validate-feature-review-coverage.ps1` reads
   `artifacts/csharp/coverage.xml` in JaCoCo format. That conversion is an audit-stage step outside this
   plan's scope, as the plan's Open Questions section records.
