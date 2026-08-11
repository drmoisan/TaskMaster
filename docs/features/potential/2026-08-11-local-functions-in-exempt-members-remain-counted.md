# local-functions-in-exempt-members-remain-counted (Potential Bug)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

Local functions declared inside a member that carries `[ExcludeFromCodeCoverage]` remain in the
coverage denominator after the issue #457 closure filter. This is residual (b) of #457: a deliberate
scope choice, recorded so it can be addressed on its own terms rather than by widening #457.

A local function is emitted as `<method name="&lt;Member&gt;g__Local|N_M">` inside the **declaring
type's own** `<class>` element rather than inside a compiler-generated closure type, and it does not
inherit the enclosing member's attribute. The #457 filter scopes strictly to closure classes — those
whose name carries the `.<>c` marker — so a local function on a non-closure class is never a candidate
for removal.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- .NET/framework: .NET Framework 4.8.1 (`net481`); post-processing runs under PowerShell 7
- Command/flags used: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
- Data source or fixture: any first-party C# file with a member carrying `[ExcludeFromCodeCoverage]` that declares a local function in its body

## Steps to Reproduce

1. Take a member that carries `[ExcludeFromCodeCoverage]` and declares a local function in its body.
2. Run the coverage pipeline.
3. Inspect the post-processed Cobertura report for that file.
4. Observe a `<method name="&lt;Member&gt;g__Local|N_M">` element still present on the declaring type's own `<class>`, with its lines counted in `lines-valid`.

## Expected Behavior

A local function declared inside a member carrying `[ExcludeFromCodeCoverage]` is excluded from the
coverage denominator, exactly as the attributed member's own lines are.

## Actual Behavior

The local function's lines are retained, because it lives on the declaring type's own class, which the
#457 filter never mutates.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- The shape is pinned by regression case 5 part B in
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1`, whose fixture carries a
  declaring class `Ns.B` whose only method is `&lt;Exempt&gt;g__Local|7_0`:

  ```xml
  <class name="Ns.B" filename="Ns\B.cs" ...>
    <methods><method name="&lt;Exempt&gt;g__Local|7_0" ...>...</method></methods>
  </class>
  ```

  That test asserts `Ns.B` is retained byte-for-byte unmutated, which is the current, deliberate
  behaviour.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The failure mode is **under-exclusion**: a file measures no better than it truly is. No coverage is
wrongly deleted. The cost is a residual ceiling on files that use local functions inside exempt
members.

## Suspected Cause / Notes

Deliberate scope boundary, not a defect in the implementation.

The #457 filter's fail-safe invariant is that it never mutates a `<class>` whose name carries no
`.<>c` marker. Stripping `g__` methods from a declaring type's own class would require breaking that
invariant, which broadens the blast radius from compiler-generated closure types to every production
class in the report. #457's stated scope is "a lambda declared inside a member", and its acceptance
criteria are written around closure types.

Note the interaction that must be preserved by any fix: `<Member>g__Local|N_M` methods are deliberately
**not** admitted to #457's presence set, so a local function cannot mask an otherwise-absent declaring
member and keep that member's lambdas in the denominator. Regression case 5 part B is the discharging
test for that rule. A fix for this residual must not disturb it.

## Proposed Fix / Validation Ideas

The natural symmetric extension: when a declaring member is absent from the presence set, also drop
`<Member>g__Local|N_M` methods from the declaring type's own class, rebuilding that class's `<lines>`
and rates exactly as the closure path already does.

- [ ] Unit coverage areas: a fixture with a declaring class carrying both a plain instrumented method and a `g__` local function of an absent member, asserting only the latter's lines are dropped and the class survives with recomputed rates. Plus a fixture asserting a `g__` local function of a **present** member is retained, the required opposite direction.
- [ ] Integration scenario to retest: the full coverage pipeline, comparing per-file figures before and after.
- [ ] Manual verification notes: regression case 5 part B must continue to pass, and the fail-safe invariant (an underivable name is retained, never removed) must hold on the widened surface.

Note for whoever takes this: PowerShell changes carry the PoshQC format -> PSScriptAnalyzer -> Pester
toolchain and the `>= 85%` line coverage floor. This option was evaluated during #457 and deliberately
deferred; do not fold it back into #457.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Provenance

- Parent issue: #457 (`docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`)
- Epic: `build-ci-coverage-gate-fidelity` (wave 1)
- Recorded as residual (b) in `spec.md` § Risks & Mitigations (and research §6.3) and in
  `<FEATURE>/evidence/other/documented-residuals.2026-08-11T02-04.md`
- Intended promotion path: `potential_to_issue`, to be run by the epic-orchestrator at epic close
