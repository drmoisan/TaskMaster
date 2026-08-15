# overload-name-collision-under-exclusion (Issue #560)

- Date captured: 2026-08-11
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/overload-name-collision-under-exclusion/ (Issue #560)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #560
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/560
- Last Updated: 2026-08-15
## Summary

When one overload of a member carries `[ExcludeFromCodeCoverage]` and another overload of the same
name does not, lambdas declared inside the attributed overload remain in the coverage denominator.
This is residual (c) of #457: a deliberate scope choice, recorded so it can be addressed on its own
terms rather than by widening #457.

The #457 presence set is keyed by member **name**, not by signature. The non-attributed overload
contributes its name to the presence set, the attributed overload's closure resolves to that same
name, and the closure is therefore retained.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- .NET/framework: .NET Framework 4.8.1 (`net481`); post-processing runs under PowerShell 7
- Command/flags used: `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
- Data source or fixture: any first-party C# type with two same-named overloads where exactly one carries `[ExcludeFromCodeCoverage]` and declares a lambda

## Steps to Reproduce

1. Take a type with two overloads of the same member name. Attribute one with `[ExcludeFromCodeCoverage]` and have it declare a lambda; leave the other unattributed.
2. Run the coverage pipeline.
3. Inspect the post-processed Cobertura report for that file.
4. Observe that the attributed overload's closure class survives, because the unattributed overload keeps the shared name in the presence set.

`UtilitiesCS.SortEmail` is a live instance of the shape: `SortAsync` appears as four attributed
overloads at `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:43, 77, 113, 304`, and
`SaveAttachmentAsync` as two at `:763, 826`.

## Expected Behavior

Exemption is resolved per overload, so a lambda inside an attributed overload is excluded even when a
same-named unattributed overload exists.

## Actual Behavior

The lambda's lines are retained, because the presence-set key cannot distinguish overloads.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- The keying is visible in `Get-CoberturaInstrumentedMemberName`
  (`scripts/vscode/Invoke-MSTestWithCoverage.ClosureFilter.ps1`), whose presence set is keyed by
  `"$declaringType|$filename"` with member **names** as the set members. The Cobertura
  `<method>` element does carry a `signature` attribute, but the synthesized closure method name
  (`<Member>b__N_M`) carries no signature information for its declaring member, so the two cannot be
  joined on signature from the report alone.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The failure mode is **under-exclusion in every case**: a file measures no better than it truly is. No
coverage is wrongly deleted, and no gate can be passed that should have failed. Two related keying
dimensions behave correctly and are not affected: two types in the same file sharing a member name are
separated by the declaring-type component of the key, and a partial type spanning files is separated
by the filename component (which also errs toward under-exclusion).

## Suspected Cause / Notes

Deliberate, and arguably not resolvable from the Cobertura document alone.

A closure method is named `<Member>b__N_M`, where `N` and `M` are compiler-assigned ordinals. The
ordinals are not a stable, documented mapping back to a specific overload's signature, so joining a
closure to a particular overload would rest on a Roslyn implementation detail rather than a contract.
Signature-based keying was therefore deliberately not attempted in #457.

Any serious fix likely needs the same additional metadata source that residual (a) needs — assembly
attribute metadata read via Mono.Cecil or `System.Reflection.Metadata` — at which point overload
resolution could be done properly on signatures rather than on synthesized-name ordinals.

## Proposed Fix / Validation Ideas

Evaluate whether the residual is worth closing, given that the failure direction is conservative and
that a name-ordinal join would be built on an implementation detail. If it is:

- [ ] Unit coverage areas: a fixture with two same-named overloads, one attributed and one not, each with its own closure class, asserting only the attributed overload's closure is dropped. Plus the opposite direction: both overloads unattributed, both closures retained.
- [ ] Integration scenario to retest: the full coverage pipeline against `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`, which carries four `SortAsync` and two `SaveAttachmentAsync` attributed overloads.
- [ ] Manual verification notes: the fail-safe invariant must hold — any fix that cannot resolve an overload confidently must RETAIN, never remove. Over-exclusion is not an acceptable failure mode.

Note for whoever takes this: PowerShell changes carry the PoshQC format -> PSScriptAnalyzer -> Pester
toolchain and the `>= 85%` line coverage floor. Signature-based keying was deliberately not attempted
in #457; do not fold it back in.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch

## Provenance

- Parent issue: #457 (`docs/features/active/2026-08-10-excludefromcodecoverage-nested-lambdas-457`)
- Epic: `build-ci-coverage-gate-fidelity` (wave 1)
- Recorded as residual (c) in `spec.md` § Risks & Mitigations (and research §6.1) and in
  `<FEATURE>/evidence/other/documented-residuals.2026-08-11T02-04.md`
- Intended promotion path: `potential_to_issue`, to be run by the epic-orchestrator at epic close
