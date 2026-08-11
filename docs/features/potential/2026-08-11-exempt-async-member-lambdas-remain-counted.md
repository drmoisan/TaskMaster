# exempt-async-member-lambdas-remain-counted (Potential Bug)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Draft

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

## Summary

Lambda bodies declared inside a member that carries `[ExcludeFromCodeCoverage]` **and** is `async` (or
an iterator) remain in the coverage denominator after the issue #457 closure filter. This is residual
(a) of #457: a deliberate, documented scope choice rather than an oversight, recorded so it can be
addressed on its own terms rather than by widening #457.

`Remove-CoberturaExemptClosureCoverage` (added by #457 in
`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`) infers that a member is exempt from its
absence from the report's instrumented method set. An `async` member emits no plain `<method>`
element — its whole body moves to a state machine class `Type.<Member>d__<N>` — so the filter must
admit `d__` class names as proof that the declaring member exists. That admission is mandatory in the
other direction: without it, covered lambdas inside **non-exempt** async members would be deleted,
failing #457's required direction 2. The consequence is that an attributed async member also enters
the presence set, and its lambdas are retained.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- .NET/framework: .NET Framework 4.8.1 (`net481`); post-processing runs under PowerShell 7
- Command/flags used: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
- Data source or fixture: any first-party C# file with an `async` member carrying `[ExcludeFromCodeCoverage]` that declares a lambda in its body

## Steps to Reproduce

1. Take a member that carries `[ExcludeFromCodeCoverage]`, is declared `async`, and declares one or more lambdas in its body. `QuickFiler.Controllers.QfcItemController.ToggleExpansionAsync` (`QuickFiler/Controllers/QfcItemController.Navigation.cs:191-192`) is a live example of the attributed async shape.
2. Run the coverage pipeline.
3. Inspect the post-processed Cobertura report for that file.
4. Observe that the closure class carrying the member's lambdas survives the filter, and its lines remain in `lines-valid`.

## Expected Behavior

A lambda declared inside a member carrying `[ExcludeFromCodeCoverage]` is excluded from the coverage
denominator regardless of whether that member is `async`.

## Actual Behavior

The lambda's lines are retained when the declaring member is `async`, because the member's state
machine class admits it to the presence set. The lines are correctly removed when the declaring
member is not `async`.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Measured evidence from the #457 `[P0-T12]` probe, recorded in
  `docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457/evidence/baseline/async-d-state-machine-probe.2026-08-11T00-38.md`:

  ```
  name="QuickFiler.Controllers.QfcItemController.&lt;ToggleExpansionAsync&gt;d__203"
  ```

  `ToggleExpansionAsync` carries `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`, and the
  attribute was verified present in commit `6b821480` (2026-07-03), 35 days before the raw corpus was
  captured (2026-08-07T02:19:25Z). Probe answer: **YES** — the collector does emit a `d__` class for an
  attributed async member.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The failure mode is **under-exclusion**: a file measures no better than it truly is. No coverage is
wrongly deleted, and no gate can be passed that should have failed. The cost is a residual, invisible
ceiling on any file that uses the "thin exempt production forwarder" seam pattern on an `async`
member. 62 first-party members carry `[ExcludeFromCodeCoverage]` and are declared `async` (enumerated
in the probe artifact), though only those that also declare lambdas are affected.

## Suspected Cause / Notes

By construction, not by defect. `Get-CoberturaInstrumentedMemberName` admits members from exactly two
sources, and source (2) — the `<Member>` token of a class named `Type.<Member>d__<N>` — cannot
distinguish an attributed async member from a non-attributed one, because the Cobertura report carries
no attribute metadata (`<class>` exposes only `line-rate`, `branch-rate`, `complexity`, `name`,
`filename`).

Resolving this requires information the Cobertura document does not contain. Options, none of which
were in #457's scope:

- Read attribute metadata from the built assemblies (Mono.Cecil or `System.Reflection.Metadata`) and
  join it to the report. That is .NET work rather than PowerShell work, and it binds the
  post-processor to build outputs that may not exist when a report is consumed.
- Parse the `.cs` source for the attribute above the enclosing member. Requires C# attribute-list,
  comment and expression-body parsing in PowerShell, and makes the post-processor fail when a report
  is processed away from its source tree.
- Emit richer metadata at instrumentation time, if the collector can be made to.

## Proposed Fix / Validation Ideas

Decide first whether the residual is worth closing at all, given that the failure direction is
conservative. If it is:

- [ ] Unit coverage areas: extend `tests/scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.Tests.ps1` with a fixture pairing an attributed async member's `d__` class against a non-attributed one, asserting the two are distinguished. Note that this test cannot be written at all until a mechanism exists to tell them apart.
- [ ] Integration scenario to retest: the full coverage pipeline, comparing per-file figures for a file with an attributed async member declaring lambdas.
- [ ] Manual verification notes: regression case 3 in the #457 suite pins the opposite direction (a covered lambda inside a **non-exempt** async member must be retained) and must continue to pass. Any fix that breaks case 3 is worse than the residual.

Note for whoever takes this: PowerShell changes carry the PoshQC format -> PSScriptAnalyzer -> Pester
toolchain and the `>= 85%` line coverage floor. Do not widen #457 to absorb this.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Provenance

- Parent issue: #457 (`docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`)
- Epic: `build-ci-coverage-gate-fidelity` (wave 1)
- Recorded as residual (a) in `spec.md` § Risks & Mitigations and in
  `<FEATURE>/evidence/other/documented-residuals.2026-08-11T02-04.md`
- Intended promotion path: `potential_to_issue`, to be run by the epic-orchestrator at epic close
