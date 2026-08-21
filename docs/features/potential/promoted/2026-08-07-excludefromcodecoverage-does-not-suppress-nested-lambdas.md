# excludefromcodecoverage-does-not-suppress-nested-lambdas (Issue #457)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/excludefromcodecoverage-does-not-suppress-nested-lambdas/ (Issue #457)
- Work Mode: full-bug
- Discovered during: preparation research for issue #455 (epic #136, child F13)

- Issue: #457
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/457
- Last Updated: 2026-08-08
## Summary

A method-level `[ExcludeFromCodeCoverage]` attribute does not suppress instrumentation of lambdas
declared inside the attributed member. The C# compiler hoists those lambdas into a separate
compiler-generated closure type whose members do not inherit the attribute, so the lambda bodies
remain in the coverage denominator. When the attributed member is exempt precisely because it
cannot execute in a unit-test host, its nested lambda bodies are therefore **permanently
uncovered** and permanently counted against the file.

This is a silent measurement defect: it does not crash, it quietly and irreducibly depresses the
line-coverage figure of any file that uses the "thin exempt production forwarder" seam pattern.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1
- Coverage pipeline: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` producing Cobertura output

## Steps to Reproduce

1. Take a method carrying `[ExcludeFromCodeCoverage]` that declares one or more lambdas in its body.
2. Run the coverage pipeline.
3. Inspect the Cobertura report for the file's `<line>` entries.
4. Observe that the source line numbers of the lambda bodies are present with `hits="0"`, while the
   attributed member's own lines are correctly absent.

## Evidence (verified 2026-08-07)

Report: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` reports 258 instrumented lines with 24
uncovered. The uncovered line numbers are:

```
58, 325, 406, 409, 471, 472, 473, 474, 475, 476, 477, 478, 479, 480,
481, 482, 483, 484, 485, 486, 487, 488, 489, 490
```

Mapping those against the source:

- Lines **406** and **409** are lambda bodies inside `BeginProductionNavigation`, which carries
  `[ExcludeFromCodeCoverage]` at `BreadcrumbPopupUiOperations.cs:394`.
- Lines **471-490** are the lambda body passed to
  `BreadcrumbPopupLifecycleOperations.NavigateWithSubscription` inside `BindProductionNavigation`,
  which carries `[ExcludeFromCodeCoverage]` at `BreadcrumbPopupUiOperations.cs:457`.

That is **22 of the file's 24 uncovered lines** attributable solely to this defect. Only lines 58
and 325 are ordinary coverage gaps.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The seam pattern this repository prefers — move decision logic into a testable host-neutral member
and leave a thin, exempt production forwarder — is precisely the pattern that triggers the defect,
because those forwarders commonly wire SDK event handlers using lambdas. The consequence is a hard,
invisible ceiling on the achievable line coverage of every file that adopts the pattern.

Concretely, `BreadcrumbPopupUiOperations.cs` cannot exceed roughly **91.5%** line coverage
((258 - 22) / 258) no matter how many tests are written. It currently measures 90.7%. Any gate,
audit, or acceptance criterion that assumes the remaining 9.3% is closable by testing is working
from a false premise.

This matters at epic scale: epic #136 requires every testable file to reach >= 80% line coverage,
and several children plan to adopt exactly this seam pattern to remove `[ExcludeFromCodeCoverage]`
attributes. Each will inherit an unannounced ceiling.

## Suspected Cause

The C# compiler lowers lambdas into members of a generated closure class (commonly `<>c` or
`<>c__DisplayClass*`). `[ExcludeFromCodeCoverage]` is applied to the original method's metadata and
is not propagated to the generated closure type or its members, so the coverage collector continues
to instrument them.

## Suggested Remediation

Options, in rough order of preference:

1. Apply `[ExcludeFromCodeCoverage]` at the **type** level on the generated closure where the
   tooling permits, or configure the coverage collector to exclude compiler-generated types
   (`<>c*`) that originate from an exempt member.
2. Add a `coverage.config` / runsettings exclusion rule for compiler-generated closure types whose
   declaring member carries the attribute.
3. Restructure the affected production forwarders to avoid lambdas — use named private methods
   (which can each carry their own attribute) instead of inline lambdas.
4. At minimum, document the ceiling so per-file coverage gates and audits do not treat a
   structurally uncoverable remainder as a test gap.

Option 3 is available to individual children today and requires no tooling change; options 1 and 2
are the durable repo-wide fix.

## Related

- Issue #441 — Cobertura post-processing double-counts `<line>` nodes. Distinct defect, same
  pipeline; both distort reported coverage.
- Issue #432 — QuickFiler per-file coverage ledger and harness (epic #136, child F1). The ledger
  should record this ceiling so children do not chase unreachable lines.
- Issue #136 — parent epic requiring >= 80% per-file line coverage.

## Next Step

- [ ] Promote to GitHub issue
- [ ] Decide between tooling-level exclusion and the no-lambda forwarder convention
