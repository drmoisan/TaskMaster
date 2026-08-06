# Cycle Inputs Read — Remediation Cycle 1

- Task: `[P0-T4]`
- Issue: #418
- Branch / HEAD: `bug/svg-renderer-null-document-nre-418` @ `ea106111`
- Evidence series: `2026-08-05T01-50`

Timestamp: 2026-08-05T01-24 (UTC)

Command: sequential full reads of the five cycle-input artifacts named by `[P0-T4]`

EXIT_CODE: 0

## Files read, in the mandated order

1. `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/remediation-inputs.2026-08-04T20-25.md` (253 lines)
2. `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/code-review.2026-08-04T20-25.md` (136 lines)
3. `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/policy-audit.2026-08-04T20-25.md` (682 lines)
4. `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/feature-audit.2026-08-04T20-25.md` (156 lines)
5. `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` (173 lines)

## Binding constraint set — `## Do Not Do`, reproduced verbatim from `remediation-inputs.2026-08-04T20-25.md`

> - Do **not** widen scope beyond the enumerated items. The work mode is `minor-audit` and the issue
>   #418 Scope Lock applies.
> - Do **not** edit `UtilitiesCS`. Its 195 pre-existing `CS86xx` diagnostics at forced-recompile scope
>   are tracked outside issue #418 and are not this feature's to fix. They are the reason a cold
>   solution-wide nullable build cannot pass on this repository independently of this branch.
> - Do **not** attempt to raise `SVGControl/SvgRenderer.cs` to the 85% modified-file floor in this cycle.
>   R-4 is deliberately bounded to two targeted items; see its explicit scope boundary.
> - Do **not** fix the deferred defects recorded in `docs/features/potential/`
>   (`2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`,
>   `2026-08-04-invoke-mstest-scalar-count-strictmode.md`). Deferring them was correct. Promote them
>   separately.
> - Do **not** edit `scripts/vscode/Invoke-MSTest.ps1`. Its single-assembly `Count` defect is real and is
>   already captured as a potential-feature entry; it is outside the Scope Lock.
> - Do **not** weaken any assertion, delete any test, or add `[ExcludeFromCodeCoverage]` to any
>   production file. `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy makes any exclusion
>   of a production source path a Blocking finding.
> - Do **not** relax any policy, rule, or threshold. Do not edit anything under `.claude/rules/` or
>   `.github/instructions/`.
> - Do **not** mark AC-11 as `[x]` without the human capture at
>   `evidence/regression-testing/designer-load-<yyyy-MM-ddTHH-mm>.md`. No amount of automated evidence
>   substitutes for it.
> - Do **not** create temporary files in tests. `.claude/rules/general-unit-test.md` UT4 prohibits it
>   with zero approved exceptions, and this is specifically the constraint that makes a live
>   `Assembly.LoadFrom` test inadmissible.
> - Do **not** write evidence to `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`, or
>   `artifacts/evidence/`. All evidence goes to
>   `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/evidence/<kind>/`.
> - Do **not** treat the mandated nullable command's exit 0 as evidence of nullable cleanliness. It is
>   vacuous in an up-to-date tree; record a forced-recompile result at project scope alongside it.

## Key figures carried forward as the comparison basis

From `evidence/qa-gates/coverage-delta.2026-08-04T14-36.md` (the end state of the completed plan, this
cycle's like-for-like comparison basis):

| Scope | Figure at `ea106111` |
|---|---|
| Repository-wide line | 93484 / 109486 = 85.3844% |
| Repository-wide branch | 21528 / 27406 = 78.5521% |
| `SVGControl` package line | 1648 / 3500 = 47.0857% |
| `SVGControl` package branch | 544 / 1236 = 44.0129% |
| `SVGControl.SvgRenderer` class line | 424 / 588 = 72.109% |
| `SVGControl.SvgAssemblyProbe` class line | 68 / 68 = 100.000% |
| `ResolveByNameAndKey` member line-rate | 68.116% (47/69), branch 45.5% |
| `PublicKeyTokensEqual` member line-rate | 0.000% (0/15) |
| `.ctor(byte[], Size, AutoSize)` member line-rate | 76.471% (13/17), branch 50.0% |
| `.ctor(byte[], Size, Padding, AutoSize)` member line-rate | 100.000% (18/18) |

Counting method to reproduce: per-`<line>`-descendant counting across deduplicated `<package>`
elements; per-member gates assessed on the Cobertura `<method>` element's `line-rate`, with
`branch-rate` recorded for information only.

Ratified exception carried forward:
`COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgRenderer.ResolveByNameAndKey`, to be re-recorded this
cycle as `COVERAGE_MEMBER_UNREACHABLE: SVGControl.SvgAssemblyResolver.ResolveByNameAndKey` after the
R-6 relocation.

## Output Summary

All five cycle-input artifacts read in full and in the mandated order. The `## Do Not Do` list is
reproduced verbatim above and is the binding constraint set for this cycle. The blocking item R-1
(AC-11 human designer-load runbook) is confirmed excluded from this plan and is not executable by any
agent.
