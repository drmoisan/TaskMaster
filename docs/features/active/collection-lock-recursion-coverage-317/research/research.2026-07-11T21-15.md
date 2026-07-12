# Research — Issue #317: Re-express lock-recursion regression coverage for `ConcurrentObservableCollection<T>`

- **Timestamp:** 2026-07-11T21-15
- **Worktree:** `C:/Users/DanMoisan/repos/TaskMaster-wt/collection-lock-recursion-coverage-317`, branch cut from `main` at `5ecbc4c6`
- **Issue:** #317

## Tool-access caveat (read first)

This research session's tool set is Read/Grep/Glob/Write/Edit/WebFetch only — no shell/`git`
execution capability is available. The orchestrator's brief asked me to independently `git show
0ec111b29923cfadd63c26908e41e069924d4ea5~1:<path>` to re-read the pre-deletion file content
verbatim. I could not run that command. I could, and did, cross-check the claim against three
already-committed, git-tracked artifacts written by different agent sessions, which is the
strongest verification available under this tool set. All three independently agree with the
orchestrator's finding (see "Independent corroboration" below). The literal byte-for-byte content
of the deleted file therefore still needs to be pulled by whichever execution step has real `git`
access (the atomic-executor does) — via `git show 0ec111b2~1:UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`.
That is a read-only, non-mutating command and is the correct first step of the atomic plan's
restoration phase.

## Independent corroboration (three separate committed sources)

1. **F5's own AC-12 evidence artifact**, `docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/other/f2-regression-coverage-confirmation.md`, states plainly that
   `ConcurrentObservableCollectionLockRecursionTests.cs` bound to
   `using UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection;` — the clean, first-party
   namespace — not `Swordfish.NET.Collections`, and classifies it "NO — clean first-party base (F2
   deliverable)" in a table alongside the genuinely Swordfish-bound `ObservableDictionary_Tests.cs`.
2. **F5's WI-4 removal-verification evidence**,
   `.../evidence/regression-testing/wi4-test-swordfish-zero.md`, records the literal `git rm` of the
   file plus its `<Compile Include>` csproj entry, and separately lists four *documentary comment*
   Swordfish mentions in surviving files that needed rewording — the removed file is not among them,
   consistent with it never having referenced the vendored library in its logic.
3. **A persistent cross-session memory note already committed to this repo**,
   `.claude/agent-memory/atomic-executor/project_swordfish_f5_test_misclassification.md`, records
   (from a separate agent's execution of F5) that the two "direct-Swordfish" files F5 was told to
   delete were misdescribed: `ConcurrentObservableCollectionSenderTests.cs` and
   `ConcurrentObservableCollectionLockRecursionTests.cs` "bound to the CLEAN first-party type ... and
   were F2's own clean-base regression coverage. Their doc comments literally said 'for the clean,
   Swordfish-free ...'." This matches the orchestrator's quoted XML doc comment verbatim in spirit.

Additionally, **F2's own atomic plan** (the feature that authored the clean collection base),
`docs/features/active/2026-07-10-swordfish-collection-stack-lineage-307/plan.2026-07-10T20-14.md`,
task `[P4-T7]`, is the origin of this file's *current* (pre-F5-deletion) content:

> Re-point `ConcurrentObservableCollectionSenderTests.cs` and
> `ConcurrentObservableCollectionLockRecursionTests.cs` from the Swordfish `ConcurrentObservableCollection`
> (`using Swordfish.NET.Collections`) to the clean collection, adjusting sender-identity and
> lock-behavior expectations to the `ObservableCollection<T>` base ... behaviors not reproducible on
> the clean base (e.g., `ReaderWriterLockSlim` recursion) are removed or re-expressed with a
> documented rationale.

This is a fourth, independent confirmation: F2 (not F5, not this issue) is the feature that last
wrote the file's logic, deliberately re-expressing the lock-recursion guard for the lock-free
`ObservableCollection<T>`-based clean type. F5 then deleted a file it explicitly recognized as
clean-base coverage (per corroboration #3), consistent with spec's WI-4 instruction to remove all
three named files regardless of binding, and instead raised #317 per AC-12's directive ("if absent,
raise a new issue rather than authoring"). This is process-compliant, not a misclassification bug —
F5's own audit trail already flags the tradeoff and defers authorship to the collection-lineage
owner (this issue).

**Conclusion on root cause: this is a restoration, not new test authoring.** The file that needs
to exist post-fix is functionally identical to a file that existed on this exact branch one commit
before deletion (`0ec111b2~1`), already re-expressed against the clean base by F2, already reviewed
and accepted by F5's own AC-12 gate as legitimate clean-base coverage.

## Current API surface verified (target of the restored tests)

Read directly (this session, current worktree, `main` tip `5ecbc4c6`):
`UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection.cs`

```csharp
namespace UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection
{
    public partial class ConcurrentObservableCollection<T> : ObservableCollection<T>, IList
    {
        public ConcurrentObservableCollection() : base() { }
        public ConcurrentObservableCollection(IEnumerable<T> enumerable) : base(enumerable) { }
        // Add(T), Count, this[int], CollectionChanged are all inherited, unmodified, from
        // ObservableCollection<T> / Collection<T> / ObservableCollection<T>.
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) { ... }
    }
}
```

Confirmed facts relevant to the two required test methods:

- `Add(T)` is not overridden by `ConcurrentObservableCollection<T>` — it is the plain
  `Collection<T>.Add` → `ObservableCollection<T>.InsertItem` → synchronous
  `OnCollectionChanged(NotifyCollectionChangedEventArgs)` raise path. `CollectionChanged` is the
  native `ObservableCollection<T>.CollectionChanged` event (type
  `NotifyCollectionChangedEventHandler`), not re-declared.
- `Count` is the plain `Collection<T>.Count` (backed by the internal `List<T>`); reading it from
  inside a `CollectionChanged` handler invoked synchronously during `Add` does not re-enter any lock,
  because **this type holds no lock at all**. The class XML doc (lines 26–29 of the file) says so
  explicitly: *"unlike the former vendored base, this type does not use a `ReaderWriterLockSlim`.
  Mutations raise `ObservableCollection<T>` events synchronously on the calling thread."*
- `NotifyCollectionChangedEventArgs.NewItems` is the standard BCL member (`IList`); no custom
  wrapping. Reading `e.NewItems` inside the handler is likewise lock-free.

Because the type has no lock, both required tests are true by construction today — they exist as
**regression guards** against a future change (e.g., a future thread-safety fix that re-introduces a
lock, per production TODOs elsewhere in the Swordfish-removal epic) silently reintroducing
`LockRecursionException`. This matches F2's own P4-T7 acceptance-criterion framing ("behaviors ...
re-expressed with a documented rationale") — the tests were kept meaningful specifically because
they will catch a real regression if a lock is added later, even though they cannot fail today.

No signature drift exists between what the deleted file's `using` directive targeted
(`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.Collection`) and what exists today: same
namespace, same class name, same public constructors, same inherited `Add`/`Count`/`CollectionChanged`
surface. **No test-body rewriting is required beyond what F2 already performed.**

## Surviving-coverage duplicate check

Searched `UtilitiesCS.Test/**/*.cs` (this worktree, current HEAD) for `LockRecursion`,
`CollectionChangedHandler`, and `DoesNotThrow`+`CollectionChanged` combinations. Zero matches
anywhere in `UtilitiesCS.Test/`. The only surviving `ConcurrentObservableCollection`-adjacent test
file with `CollectionChanged` coverage is
`UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection_Tests.cs`,
which has one relevant method, `CollectionChanged_RaisedOnAdd_WithWrapperSender` (lines 212–229),
covering **sender identity only** — it asserts `capturedSender.Should().BeSameAs(sut)`, not
re-entrant reads of `Count`/`NewItems` from inside the handler. This confirms the issue's premise:
sender-identity coverage survives, lock-recursion coverage does not. No restoration work would
duplicate existing coverage.

## Namespace convention — inconsistency to resolve at restoration time

Verified via `Grep` across the whole worktree:

- The literal namespace `ConcurrentObservableCollection.Tests` **already exists** in three
  currently-surviving files, all under the older, non-folder-mirroring convention used for the
  Dictionary-side tests:
  - `UtilitiesCS.Test/ReusableTypeClasses/ConcurrentObservableDictionaryTest/ConcurrentObservableDictionaryTest.cs:5`
  - `UtilitiesCS.Test/ReusableTypeClasses/ConcurrentObservableDictionaryTest/SimpleObserver.cs:3`
  - `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Dictionary/ConcurrentObservableDictionaryTests.cs:9`
- Both **currently-surviving Collection-side siblings**, in the exact folder the restored file
  belongs to, use the newer folder-mirroring convention instead:
  - `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollection_Tests.cs:13`:
    `namespace UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection`
  - `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionSerialization_Tests.cs:12`:
    same namespace.

If the deleted file in fact declared `namespace ConcurrentObservableCollection.Tests` (as reported
by the orchestrator from its own `git show`), restoring it verbatim would **not** cause a compile
collision — the class name `ConcurrentObservableCollectionLockRecursionTests` is unique and does not
clash with `ConcurrentObservableDictionaryTest`/`ConcurrentObservableDictionaryTests` in that
namespace — but it would leave the Collection folder with two different namespace conventions
side by side. Per CLAUDE.md §7 ("Where the repo already has a clear style, match that style") and
the fact that both living siblings in the same physical folder already use the folder-mirroring
convention, the atomic plan should normalize the restored file's namespace to
`UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection` rather than preserve
whatever the pre-deletion namespace literally was. This is a one-line, non-semantic edit on top of
the restored body and should be called out explicitly as a deliberate deviation from a pure `git
checkout`/`git show`-based restore.

## csproj scaffolding — confirmed sufficient with one line

Read `UtilitiesCS.Test/UtilitiesCS.Test.csproj` directly. The two surviving Collection-folder test
files are wired at lines 391–392:

```
391: <Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollection_Tests.cs" />
392: <Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSerialization_Tests.cs" />
```

The `ItemGroup` is **not** strictly alphabetized (confirmed by scanning lines 380–399 — entries are
grouped by topic/history, not sorted), so there is no ordering constraint beyond placing the new
line inside this `ItemGroup`. Restoration requires exactly one inserted line, e.g. immediately after
line 391:

```xml
<Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs" />
```

No other project-file scaffolding (folders, `ItemGroup` wrappers, `Content`/`None` entries) is
needed — the target directory
`UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/` already exists and already
holds two sibling `.cs` files in source control, so `git checkout`/manual re-creation of the `.cs`
file into that directory requires no new directory creation.

## Coverage-tooling implication

Per F5's own AC-16 evidence
(`docs/features/active/2026-07-10-swordfish-interface-project-teardown-308/evidence/qa-gates/coverage-delta-verification.md`,
summarized in `feature-audit.2026-07-11T13-36.md` row AC-16), the WI-4 removal was raw-repo-neutral
on production coverage: "changed/new executable production lines = 0" for this removal, because the
deleted test exercised zero *unique* production lines (the surviving
`ConcurrentObservableCollection_Tests.cs` already covers `Add`/`CollectionChanged`/`Count` on the
same production type). Restoring the file is therefore expected to be **net-neutral on production
line/branch coverage percentages** — it adds test-only lines and re-exercises already-covered
production lines (`Add`, `OnCollectionChanged`, `Count`, `CollectionChanged` add/remove) rather than
opening new production surface. No new baseline coverage comparison is needed beyond the standard
before/after `vstest.console.exe /EnableCodeCoverage` capture that any change requires per CLAUDE.md
§C#1.3/CUT3; there is no reason to expect a coverage regression or an unusual delta requiring special
scrutiny beyond the routine baseline-vs-final diff.

Two production-file candidates that could theoretically need
`[ExcludeFromCodeCoverage]` review are moot here: `ConcurrentObservableCollection<T>` has no
Outlook/COM/WinForms dependency (confirmed by its `using` list — `System`, `System.Collections`,
`System.Collections.Generic`, `System.Collections.ObjectModel`, `System.Collections.Specialized`,
`System.Linq` only) and is not in the COM/VSTO/WinForms exemption list in CLAUDE.md's "General Unit
Test Policy" §UT2. It is fully testable, and F5's own evidence already confirms 100% of its members
remain exercised by the surviving sibling test file.

## Files to touch (concrete, path-level)

1. **Restore** (new file, same path as before deletion):
   `UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs`
   - Source of truth for body content: `git show 0ec111b2~1:UtilitiesCS.Test/ReusableTypeClasses/Concurrent/Observable/Collection/ConcurrentObservableCollectionLockRecursionTests.cs` (run this at execution time — this research session has no git/shell access).
   - Required deviation from a pure restore: change the namespace declaration to
     `UtilitiesCS.Test.ReusableTypeClasses.Concurrent.Observable.Collection` (see "Namespace
     convention" above) so it matches its two living siblings in the same folder. If the
     pre-deletion namespace already matches (this session could not confirm the literal string),
     no namespace edit is needed — verify at execution time before editing.
   - Must contain (per the issue and per F2's P4-T7 acceptance criterion, confirmed compatible with
     the current API): `Add_WhenCollectionChangedHandlerReadsCountFromCollection_DoesNotThrow` and
     `Add_WhenCollectionChangedHandlerUsesNewItemsFromEventArgs_DoesNotThrow`, each constructing a
     `ConcurrentObservableCollection<T>`, attaching a `CollectionChanged` handler that reads
     `sut.Count` (first test) or `e.NewItems` (second test), calling `Add`, and asserting via
     FluentAssertions `Action.Should().NotThrow()` (repo's mandated assertion library per CLAUDE.md
     CUT2) rather than a bare MSTest `Assert`.
2. **Edit** (one line added):
   `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — insert
   `<Compile Include="ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs" />`
   immediately after line 391 (the sibling `ConcurrentObservableCollection_Tests.cs` entry).

No other production or test file requires modification. No `IConcurrentObservableCollectionSeams.cs`
or `ConcurrentObservableCollection.Serialization.cs` changes are implicated — the lock-recursion
guard only touches the base `Add`/`CollectionChanged`/`Count` surface defined in
`ConcurrentObservableCollection.cs`.

## Candidate approaches (brief, per research workflow)

1. **Restore via `git show <deletion-commit>~1:<path>` + re-add csproj line (recommended).**
   Advantages: reuses F2's already-reviewed, already-re-expressed test bodies verbatim (or with only
   a namespace normalization); zero risk of introducing new assertions inconsistent with what F5's
   own AC-12 audit already validated as legitimate coverage; fastest, lowest-risk path; matches
   CLAUDE.md's bugfix-workflow spirit (smallest change that restores the missing regression guard).
   Limitation: requires a git/shell-capable execution step (this research session lacks one) to
   fetch the literal historical content; the exact original test-method bodies were not
   independently re-verified byte-for-byte in this session (see caveat above).
2. **Author the two test methods fresh, from the issue's stated intent, without consulting git
   history.** Advantages: avoids any dependency on git history at all. Limitation: needlessly
   discards F2's already-reviewed implementation and risks introducing a different (but similarly
   trivial) assertion shape than what F5's own audit trail already vetted; higher chance of drift
   from the "documented rationale" F2 committed to in P4-T7; provides no advantage over approach 1
   since the current API is confirmed unchanged from the deleted file's target.

**Rejected alternative:** approach 2. Recommendation: **approach 1** — restore, verify compile/pass,
normalize namespace only if it does not already match the sibling convention.

## Testing implications

- No new production code is introduced; this is purely test-file restoration.
- The two restored `[TestMethod]`s are independent, isolated (each constructs its own
  `ConcurrentObservableCollection<T>`), deterministic (no timing/threading), and fast (in-memory,
  synchronous `Add`) — compliant with CLAUDE.md's General Unit Test Policy (independence, isolation,
  determinism, no external dependencies, no temp files).
- Use FluentAssertions (`Action act = () => sut.Add(item); act.Should().NotThrow();`) rather than a
  raw try/catch, consistent with `ConcurrentObservableCollection_Tests.cs`'s existing style
  (FluentAssertions throughout) and with CLAUDE.md CUT2.
- After restoring, run the mandated four-step C# toolchain (csharpier → analyzer build → nullable
  build → `vstest.console.exe ... /EnableCodeCoverage`) per CLAUDE.md §C#1/CUT3, and diff the
  resulting coverage report against a fresh baseline captured on this branch before the restoration,
  per the repo's evidence-and-timestamp conventions — not because a regression is expected (see
  "Coverage-tooling implication" above), but because every change requires this gate regardless of
  expected direction.
