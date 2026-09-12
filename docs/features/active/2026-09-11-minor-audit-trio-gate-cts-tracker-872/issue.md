# 2026-09-11-minor-audit-trio-gate-cts-tracker

- Work Mode: minor-audit
- Issue: #872
- Promotion type: bug
- Closes on merge: #794, #840, #841
- Source potential record: docs/features/potential/promoted/2026-09-11-minor-audit-trio-gate-log-assertion-cts-disposal-dormant-tracker.md

## Problem / Why

Three independent minor-audit defects are consolidated into one delivery, following the #823 precedent
of closing several small residuals in a single item. Their files are disjoint, each fix is confined to
one or two files, and none alters a public contract that an existing caller depends on.

**Defect A (issue #794) — the scan-bound log line is not content-asserted.**
The #791 fix added three log lines to the high-confidence dequeue gate: a launch line, a
zero-acceptance checkpoint line, and a scan-bound line. The launch and checkpoint lines are
content-asserted by existing tests in
`QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
(`DequeueAsync_Launch_LogsCutoffQuantityAndBounds` and
`DequeueAsync_CheckpointExpiry_LogsCutoffAndCounts`). The scan-bound line emitted by the private
`LogScanBoundReached` method is not asserted anywhere, so a regression in the `Bound=` value or the
`Decision=stop` token would pass the suite. The two tests that do reach the bounded exit
(`DequeueAsync_ZeroAcceptedAndCapReached_StopsAndReportsScanCapReached` and
`DequeueAsync_ZeroAcceptedAndCeilingReached_StopsWhileSourceStillRefilling`) assert the stop reason
and the take count but pass no `debugLog` delegate at all, so they observe no log output.

The emitted line already carries every field the assertion needs. This defect is closed test-side
only; the gate's production source is correct as it stands and is deliberately left unmodified.

**Defect B (issue #840) — two CancellationTokenSource instances are never disposed.**
`UtilitiesCS/Threading/ProgressPackage.cs` constructs a `CancellationTokenSource` in each of its two
`InitializeAsync` overloads when the caller supplies none, and the class implements no disposal
contract at all. `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` constructs
a local `CancellationTokenSource` in `RebuildAsync` and never disposes it. In both cases the timer and
registration resources held by the source are released only by finalization.

The disposal rule is not a blanket one. The source that `ProgressPackage` constructs is shared beyond
the package: it is handed to the `ProgressTracker` or `ProgressTrackerPane` the same overload
constructs, it is copied by reference into every child produced by `SpawnChild`, and it is returned to
callers through `ToTuple`, `ToTuplePane`, `CreateAsTupleAsync` and `CreateAsTuplePaneAsync`. A
caller-supplied source belongs to that caller and must never be disposed by the package. The fix must
therefore distinguish an owned source from an injected one and must not disturb the tuple factories'
existing behaviour of returning the source to the caller.

**Defect C (issue #841) — ProgressTrackerAsync is dormant production code.**
`UtilitiesCS/Threading/ProgressTrackerAsync.cs` has no construction site outside
`UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`. Removal is preferred to wiring a caller,
because the type has had no caller since it was added and the #778 null-race fix on it protected code
that nothing executes. The only other reference in the tree is a prose mention inside a `<c>` tag in
an XML doc comment in `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, which is
not a compile-time dependency.

## Implementation Intent

The three defects are independent and are implemented as three separate task groups. Defect A is
test-only. Defect B changes two production files and extends one existing test file. Defect C deletes
one production file and one test file and removes their Compile items from the two owning project
files.

This repository's C# projects are not SDK-style: every compiled source is named by an explicit
`<Compile Include>` item, and there is no wildcard glob. Deleting a source file therefore requires the
matching Compile item to be removed from its owning project file in the same change, or the build
fails. The two items to remove are at `UtilitiesCS/UtilitiesCS.csproj` line 971 and
`UtilitiesCS.Test/UtilitiesCS.Test.csproj` line 496. No other Compile item may be dropped; the NuGet
precedent in which a tool silently removed unrelated Compile items is the reason this is called out.

## Write Set

Every path below is a repository-relative path that the delivered diff creates, modifies or deletes.

- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
- `UtilitiesCS/Threading/ProgressPackage.cs`
- `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs`
- `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs`
- `UtilitiesCS/Threading/ProgressTrackerAsync.cs`
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`
- `UtilitiesCS/UtilitiesCS.csproj`
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

The last four entries are the deletion group: the first two of them are deleted outright and the two
project files lose exactly one Compile item each.

Paths outside that list are out of scope for this delivery. In particular, the production source of
the high-confidence dequeue gate under the QuickFiler Controllers directory is read for the assertion
but is not edited, no new test part file is added to the QuickFiler test project so its project file
is not edited, and the ProgressTracker report-and-viewer test file is not edited because its only
mention of the removed type sits in a code tag in an XML doc comment rather than in compiled code.

## Acceptance Criteria

- [ ] AC1 — A test in `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs`
  drives the gate to the item-cap bound with a `debugLog` delegate injected, and asserts that the
  captured scan-bound line reports the cutoff in force, the accepted count, the scanned count, the
  bound token `scan-cap`, and the stop decision `Decision=stop`.
- [ ] AC2 — A test in the same file drives the gate to the time-ceiling bound with a `debugLog`
  delegate injected, and asserts that the captured scan-bound line reports the bound token
  `zero-acceptance-ceiling` rather than the item-cap token, so that a regression which collapsed the
  two bounds to one value would fail.
- [ ] AC3 — `UtilitiesCS/Threading/ProgressPackage.cs` records, in per-instance state assigned at the
  construction site inside each `InitializeAsync` overload, whether the `CancellationTokenSource` it
  holds was constructed by the class or supplied by the caller, and exposes a disposal contract that
  releases the source only when the class constructed it. The ownership state is never assigned
  through the public `CancelSource` property setter, because `SpawnChild` assigns through that
  setter and a child must not claim its parent's source. This is a capability criterion: see the
  Out of Scope section for why the class cannot also guarantee that every constructed source is
  released, and for the residual that is tracked separately.
- [ ] AC4 — `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` contains three new tests: one
  proving a source the class constructed is released on disposal, one proving a caller-supplied
  source is left usable after disposal, and one proving a child produced by `SpawnChild` does not
  release the source its parent constructed. All three drive the tracker overload with a non-null
  injected tracker and an explicit stop watch so that no UI thread is touched, and all three probe
  release by observing that the source's `Token` getter throws rather than by inspecting a timer.
- [ ] AC5 — `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` releases the
  `CancellationTokenSource` that `RebuildAsync` constructs, on the completing path and on the faulting
  path alike, and the release happens after the last use of the token and of the tracker that holds
  the source.
- [ ] AC6 — `UtilitiesCS/Threading/ProgressTrackerAsync.cs` and
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` are deleted from the working tree.
- [ ] AC7 — The Compile item naming the deleted production source is removed from
  `UtilitiesCS/UtilitiesCS.csproj` and the Compile item naming the deleted test source is removed from
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, and the count of Compile items in each of those two
  project files falls by exactly one relative to the base commit, so that no sibling Compile item is
  dropped.
- [ ] AC8 — `dotnet tool run csharpier check .` reports no unformatted file.
- [ ] AC9 — The analyzer build of the solution with `/t:Rebuild`, `EnableNETAnalyzers` and
  `EnforceCodeStyleInBuild` succeeds, and the build log shows compilation actually ran rather than
  being skipped as up to date.
- [ ] AC10 — The nullable build of the solution with `/t:Rebuild` and `TreatWarningsAsErrors`
  succeeds. The `Nullable` property is not supplied on the command line, matching the CI workflow.
- [ ] AC11 — The MSTest run over the affected test assemblies passes with no failed test. Measured
  against the Phase 0 per-assembly baseline, the UtilitiesCS test assembly's executed-test count
  changes by exactly minus six, being the nine test methods removed with the dormant tracker's test
  class and the three added by AC4, and the QuickFiler test assembly's executed-test count changes
  by exactly plus two, being the tests added by AC1 and AC2.
- [ ] AC12 — Line coverage on the changed lines of `UtilitiesCS/Threading/ProgressPackage.cs` does not
  regress against the baseline captured in Phase 0.

## Out of Scope — Follow-up Required

Research established that an ownership-aware disposal contract on the progress package closes the
defect for a holder that disposes the package, and does not close it for the paths on which nothing
holds the package. The two static tuple factories construct a package locally, return the source to
the caller inside a tuple, and discard the package, so there is no holder left to dispose. The same
shape recurs one level up in the two LoadIfNullAsync overloads of the Bayesian performance
measurement type, which return an owned package to a caller that never disposes it.

Nine production call sites carry that residual, in six files: three sites in the transform partial
of the email data miner, one in its folder-extraction partial, one each in the OlFolder classifier
group, the multiclass engine and the category classifier group, and two in the Bayesian performance
measurement type. None of those six files is in the Write Set, and none is edited by this delivery.

The decision is to keep the Write Set unchanged and to treat AC3 as a capability criterion, for
three reasons. Three of the nine sites sit in host-bound methods that carry the
`[ExcludeFromCodeCoverage]` attribute and cannot be covered by a unit test. Disposal at each site
requires first proving the derived token is no longer in flight, because a token whose source has
been disposed still answers `IsCancellationRequested` but throws from `Register` and from
`WaitHandle`. And the Bayesian performance measurement file exceeds fifteen hundred lines, so
changing the ownership shape of its LoadIfNullAsync contract is not a minor-audit change.

The residual is a real defect and must be promoted to its own issue by the caller of this
preparation run. It is recorded here, and in the research artifact under scope finding SF-1 with the
nine sites named by file and line, so that it is tracked rather than buried.

Two further findings are recorded for completeness. The progress tracker and progress tracker pane
types need no change, because the two viewer types that actually hold the source already catch
`ObjectDisposedException` and document the borrowed-source contract. And the generated coverage
summary at the repository root names the deleted type and will become stale after the deletion; it
is a generated artifact, it is not compiled, and it is not edited by this delivery.

## Dependencies / Risks

- The `[ExcludeFromCodeCoverage]` attribute on `RebuildAsync` in
  `UtilitiesCS/EmailIntelligence/SubjectMap/SubjectMapSco.Orchestration.cs` makes that method
  invisible to the coverage report rather than reporting it at zero, so no per-file coverage assertion
  on that file can discriminate the AC5 change. AC5 must be verified structurally and by the build,
  not by a coverage number.
- `RebuildAsync` installs a `WindowsFormsSynchronizationContext` and starts a long-running task, so it
  is not unit-testable without a host. AC5 carries no test obligation for that reason.
- Deleting the dormant tracker lowers the repository-wide coverage denominator. The resulting movement
  in the headline percentage is expected and is the stated purpose of issue #841.
- `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part4.cs` is 348 lines at the
  base commit and the repository file-size limit is 500 lines. The two new tests must fit inside that
  budget; splitting into a further part file is out of scope because it would require editing the
  QuickFiler test project file, which this delivery does not claim.

## Verification Steps

1. Capture the Phase 0 baseline for the four C# toolchain stages and for coverage on the two
   production files that change behaviour.
2. Implement Defect A, Defect B and Defect C as separate task groups.
3. Run the full C# toolchain in the mandated order: CSharpier format, then the analyzer rebuild, then
   the nullable rebuild, then the MSTest run with coverage. Restart from the first stage whenever a
   stage fails or rewrites a file.
4. Compare the post-change Compile-item counts and test counts against the Phase 0 baseline.

## Evidence Checklist

- [ ] baseline
- [ ] targeted verification
- [ ] end-state
