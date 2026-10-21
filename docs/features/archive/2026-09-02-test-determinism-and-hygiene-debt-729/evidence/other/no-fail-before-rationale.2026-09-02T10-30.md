# No fail-before run claimed for Finding 1 or for the `UtilitiesCS.Test` guard (P2-T5)

Timestamp: 2026-09-02T23-09

This artifact records, as an auditable negative claim, that no fail-before (red-before) run is
claimed for Finding 1 (the `NonBlockingDelay` `TimeProvider` seam and its rewritten tests) or for
the `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` structural guard. The genuine
red-before/green-after regression test for this item is the `SVGControl.Test` guard, recorded by
P3-T4 and P3-T8.

SearchScope: `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/`

SearchPatterns: `fail-before-exception.*.md`

SearchResult: none. At the time of this search that directory contained only
`nonblockingdelay-tests.2026-09-02T10-30.md` and
`nonblockingdelay-zero-delay-observation.2026-09-02T10-30.md`. The Finding 3 fail-before exception
dossier `fail-before-exception.2026-09-02T10-30.md` is authored later by P5-T7 and covers the
`Console.Out` parallel-execution hazard, not the two subjects of this artifact.

## WhyFailingRunImpossible:

(a) **Finding 1 — `NonBlockingDelay` and `NonBlockingDelayTests`.** The replacement tests
reference the two-argument overload `NonBlockingDelay.WaitAsync(TimeSpan, TimeProvider)` and the
`Microsoft.Extensions.Time.Testing.FakeTimeProvider` type. Neither exists in the pre-change tree:
the pre-change production file declared only the one-argument `WaitAsync(TimeSpan)`, and
`TaskMaster.Test` declared neither the `Microsoft.Bcl.TimeProvider` nor the
`Microsoft.Extensions.TimeProvider.Testing` package. Running the replacement tests against the
pre-change production file therefore produces a compile error (an unresolved method overload and
an unresolved type), not a test failure. A red-before state expressed as a build break is not a
failing test run, so no failing-run artifact can be produced for this finding.

(b) **`UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs`.** That guard is green from birth. No
`Form`-derived type has ever been compiled into the `UtilitiesCS.Test` assembly: the ten orphan
form and resource sources under `UtilitiesCS.Test/` are not referenced by any `<Compile>` or
`<EmbeddedResource>` item in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (verified by P4-T1), so
they never reach the compiler and never appear in the executing assembly's type metadata. The
guard therefore passes on its very first run, before and after the orphan deletion alike. It is
regression prevention rather than a fail-before/pass-after regression test, and no red run for it
exists or can be constructed without first adding a live `Form` type to that assembly, which this
plan does not do.

## Related evidence

- Genuine red-before run for Finding 2: `evidence/regression-testing/svgcontrol-guard-fail-before.2026-09-02T10-30.md` (P3-T4).
- Genuine green-after run for Finding 2: `evidence/regression-testing/svgcontrol-guard-pass-after.2026-09-02T10-30.md` (P3-T8).
- Green-from-birth statement for the `UtilitiesCS.Test` guard: `evidence/regression-testing/utilitiescs-guard-pass.2026-09-02T10-30.md` (P4-T6).
