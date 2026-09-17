# P5-T14 — Follow-Up Handoff Record

Timestamp: 2026-09-17T02-39

This record is written for the orchestrator, who files each entry below through
`mcp__drm-copilot__new_potential_bug_entry`. The executor has no MCP tool surface and
`.claude/skills/feature-promotion-lifecycle/SKILL.md` permits no non-MCP route for potential-entry
creation, so the executor creates no file under `docs/features/potential/`.

Entries 1 and 2 are the two the plan anticipated. Entries 3 and 4 were observed during execution and
are recorded here under the same handoff mechanism.

---

## Entry 1

short_name: `breadcrumb-dispatcher-tests-assume-taskrun-distinct-thread`

promotion-type: bug

### Summary

Two tests assert that a cross-thread call fails, while obtaining their "different thread" from a
`Task.Run` delegate, against `BreadcrumbUiDispatcher`'s owner-thread-id check rather than against
`ItemViewer`'s boundary guard. They share the assumption that issue #900 removed from
`ItemViewerBreadcrumbThreadAffinityTests`: `Task.Run` guarantees only a thread-pool thread, never a
different one. When the test body is itself on a pool thread, the work item lands on that thread's
own local work-stealing queue and a blocking wait can pop it back and run the delegate inline on the
same thread, at which point the owner check passes and the expected failure never occurs. This is
the same defect class, against a different guard, in different files, and it is not fixed under
#900.

### Citations

- `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs:58-61`,
  `Dispatcher_OwnerOnlyWorker_ReportsWithoutRunningAction`: an owner-only `BreadcrumbUiDispatcher`
  created on the test thread must report that it cannot marshal when `Dispatch` is called from a
  `Task.Run` delegate, and `executions` must be 0. An inlined delegate passes the owner test and
  defeats both assertions.
- `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:298-307`: creates a dispatcher for the
  current thread and then awaits a `Task.Run` delegate expected to throw a cross-thread marshalling
  error.
- `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:40`, `:54`, `:64`: the owner check compares
  `Environment.CurrentManagedThreadId` against `_ownerThreadId`.

### Note carried forward from the research artifact

The second site uses `await Task.Run(...)` rather than a blocking `GetResult()`. `await` does not
attempt wait-inlining, so that site is exposed only to genuine idle-thread reuse rather than to the
inlining path. Its exposure is lower but not zero. The first site, which blocks, carries the same
exposure as the two tests #900 repaired.

### Suggested remedy

The same shape adopted under #900: a private static helper that runs the delegate on a dedicated
`System.Threading.Thread` the test constructs and joins, with a precondition assertion inside the
delegate proving the call is genuinely off the owning thread before the guarded call runs.

This entry is not fixed under #900.

---

## Entry 2

short_name: `breadcrumb-null-owner-test-discrimination-remark-holds-only-when-stolen`

promotion-type: bug

### Summary

The XML remark on `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` claims the test
discriminates against the pre-#781 context-reference guard. That claim holds only in the branch
where a remote worker steals the `Task.Run` work item. In the inlined branch the ambient context
equals the captured one, so the pre-fix guard would not have rejected the call and the test would
not have discriminated. The remark therefore states unconditionally a property that is conditional
on scheduling. This does not affect pass or fail on the current tree, because the assertion is
`NotThrow()` and both branches reach the same early return once the owning dispatcher is null. It is
a documentation-accuracy defect rather than a test defect.

### Citations

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`, remark at lines 311-317 in
  the post-fix file, with the discrimination claim at lines 315-316; the test itself is declared at
  line 319. In the pre-fix numbering the remark was at lines 272-278 and the test at line 280. The
  post-fix location was derived by searching for the literal
  `the pre-fix guard reads the non-null captured context`, which is the stable locator for this
  remark across renumbering.
- Research artifact section Q4 records the analysis.

### Scope note

This test still uses `Task.Run` and was deliberately left unchanged by #900: its `NotThrow()`
assertion is not defeated by inlining the way a `Throw()` assertion is. The remark's wording is the
only issue, and it is not fixed under #900.

---

## Entry 3, observed during execution

short_name: `analyzer-hintpath-versions-lag-packages-config-breaking-cold-worktrees`

promotion-type: bug

### Summary

A cold checkout of this repository cannot build. The first `msbuild TaskMaster.sln /t:Rebuild` fails
with `error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.203\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll'
could not be found`, raised by `VBFunctions/VBFunctions.csproj` and `UtilitiesCS/UtilitiesCS.csproj`,
after which every dependent project fails transitively and no test assembly is produced.

The cause is a skew between two places in the same project files. `packages.config` and the analyzer
`<Import>` elements name `Meziantou.Analyzer` 3.0.235, which restore installs, while most
first-party projects' hand-written `<Analyzer Include>` HintPaths still name 3.0.203. A long-lived
worktree keeps superseded package folders across version bumps, so the stale HintPath still resolves
there and the defect is invisible; a cold worktree restores only the current `packages.config`
versions and fails.

Measured on this worktree: 18 project files scanned, 162 `<Analyzer Include>` item lines, 7 distinct
analyzer package directories referenced, of which exactly one was missing. Both `3.0.203` and
`3.0.235` appear across the tree, which is the fingerprint of a partially applied bump. The other six
analyzer packages resolve.

### Impact

Every gate in the mandated C# toolchain fails from this single cause on a cold checkout: the
analyzer build, the nullable build, any scoped or full test run, and any coverage run. An agent that
diagnoses each gate separately investigates the same defect four times.

### Suggested remedy

Update the `<Analyzer Include>` version strings in the same commit as any future `packages.config`
analyzer bump, and consider a build-time check that compares the two. The precedent commit
`46ca9210 fix(build): repair NuGet upgrade fallout blocking CI` performed exactly this repair for an
earlier bump, so the condition recurs on every bump that is not followed by the repair.

### What this run did

Provisioned the missing version into the git-ignored `packages/` tree, which touched no tracked
file, and re-ran the mandated command unmodified. No project file was edited. This entry is not
fixed under #900.

---

## Entry 4, observed during execution

short_name: `fileinfowrapper-openread-test-depends-on-a-real-repository-file`

promotion-type: bug

### Summary

`UtilitiesCS.Test.HelperClasses.FileInfoWrapper_Tests.OpenRead_ShouldReturnReadableStreamForWrappedFile`
opens the repository's own `TaskMaster.sln` through the filesystem wrapper under test and asserts it
receives a readable stream. Its outcome therefore depends on whether any other process on the
machine holds that file at that instant.

Observed failing once during this run, in a full repository-wide coverage pass:

    System.IO.IOException: The process cannot access the file '<repo-root>\TaskMaster.sln' because it is being used by another process.

The holder was a resident MSBuild node-reuse worker process left behind by the plan's own mandated
`msbuild ... /m` gates, which run immediately before the test pass. The same test passed on the
baseline pass and on a re-run after the idle workers were cleared, so the failure is intermittent
and is decided by machine state rather than by the unit under test.

### Why it is a defect rather than noise

The repository's unit-test policy requires environment stability and prohibits a test's result from
depending on mutable external state. A unit test for a filesystem wrapper does not need a real
repository file: an injectable seam or a sentinel stream would exercise the same wrapper behaviour
deterministically. The current shape also cannot use a temporary file, which the same policy
prohibits, so the remedy is a seam rather than a scratch file.

### Impact

Any agent or CI run that executes the full suite shortly after a solution build can observe a
spurious red result in an assembly unrelated to its change, which is exactly the class of erosion of
trust that issue #900 itself addresses in a different file.

This entry is not fixed under #900.
