# F5 Research — Swordfish Interface / Project Teardown (Issue #308)

- Feature: swordfish-interface-project-teardown (epic `swordfish-removal`, child F5, wave 1)
- Mode: PREPARATION (planning only). Findings are grounded in the CURRENT worktree source
  (F1–F4 not yet merged) and reason explicitly about the post-F1–F4 end-state.
- Worktree root: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a60b2db6bdd19bba3`
- All searches below were run against that root.

---

## Executive summary

- Q1: The Tags and TaskVisualization project references to UtilitiesSwordfish are stale.
  No `*.cs` under `Tags\` or `TaskVisualization\` references any `Swordfish.NET.*`, `Sco*`,
  `IScoCollection`, `ISubjectMapSco`, or `ConcurrentObservable` type. Removal is safe.
- Q2: Recommend REMOVING `IScoCollection<T>`, `IScoCollection2<T>`, AND the dead
  `ISubjectMapSco` interface outright. `IScoCollection2` has zero implementers/consumers.
  `IScoCollection` is consumed only by `ISubjectMapSco`, which has no implementer and is
  referenced only by one never-called private static method
  (`QfcExplorerController.UpdateForMove`). No surviving first-party type needs a Swordfish-free
  equivalent contract, and the clean stack does not expose `IConcurrentObservableBase<T>` /
  `IConcurrentObservableCollection<T>` equivalents, so migration would be wasted effort.
- Q3: Exactly three first-party test files reference Swordfish types directly and are F5-owned
  for removal. All other `Sco*` tests are F1/F2/F3-owned via their type migrations.
- Q4: Authoritative inventory produced below. IMPORTANT DISCREPANCY: there are **nine** external
  `ProjectReference` entries to `UtilitiesSwordfish.NET.General.csproj`, not eight.
  `TaskVisualization.Test.csproj` carries a ninth reference that is absent from the F5 scope
  list in both the epic manifest and issue #308.
- Q5: Two additional teardown items beyond the literal scope list must be handled or the build
  breaks: (a) the ninth `ProjectReference` in `TaskVisualization.Test.csproj`; (b) the
  `GlobalSection(ProjectConfigurationPlatforms)` entries for both Swordfish GUIDs in
  `TaskMaster.sln` (lines 194–217), in addition to the `Project(...)`/`EndProject` declarations
  (lines 33–36). No app.config binding redirects and no `.props`/`.targets` reference Swordfish.
- Autonomy: every teardown action (csproj/sln text edits, folder deletion, git operations) is
  scriptable. No Visual Studio GUI-only step is required. No human-interaction requirement.

---

## Q1 — Are the Tags and TaskVisualization ProjectReferences genuinely UNUSED?

**Answer: Yes. Both references are stale and safe to remove.**

Search 1 (any Swordfish/Sco/interface/clean-collection token in Tags source):
```
Grep pattern: Sco(Collection|Stack|Dictionary|SortedDictionary)|IScoCollection|ISubjectMapSco|Swordfish|ConcurrentObservable
glob: *.cs  path: \Tags
Result: No files found
```

Search 2 (same, TaskVisualization source):
```
Grep pattern: Sco(Collection|Stack|Dictionary|SortedDictionary)|IScoCollection|ISubjectMapSco|Swordfish|ConcurrentObservable
glob: *.cs  path: \TaskVisualization
Result: No files found
```

The repo-wide `Swordfish` search over `*.cs` (see Q4) also returns zero files under `Tags\` or
`TaskVisualization\`.

`Tags.csproj` (lines 84–93) and `TaskVisualization.csproj` (lines 138–145) each reference
`..\UtilitiesCS\UtilitiesCS.csproj` and `..\UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj`.
Because neither project's source names any `Sco*` / `IScoCollection` / `ISubjectMapSco` /
`Swordfish.NET.*` type, there is no transitive path that forces the Swordfish assembly to load:
the only UtilitiesCS public surface tied to Swordfish is the `Sco*` types and the two interfaces,
none of which Tags/TaskVisualization consume. Removing the `ProjectReference` block does not break
compilation.

Note (build-order, feeds Q5): `TaskVisualization.Test.csproj` line 297 ALSO references
UtilitiesSwordfish. Its two matching test files (`ManageFiltersControllerTests.cs`,
`AutoAssignPeopleTests.cs`) match only on the clean/first-party `ConcurrentObservable`/`Sco*`
tokens (they do NOT appear in the `Swordfish` `*.cs` search), so their reference is stale too.

---

## Q2 — Disposition of IScoCollection<T> and IScoCollection2<T>

**Recommendation: REMOVE all three interfaces outright — `IScoCollection2<T>`,
`IScoCollection<T>`, and `ISubjectMapSco` — plus remove the dead `UpdateForMove` method in
`QfcExplorerController.cs`. No Swordfish-free replacement contract is needed.**

### Current wiring (verified)

`IScoCollection<T>` (`UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs`):
- `public interface IScoCollection<T> : IConcurrentObservableBase<T>, IList<T>, IList`
- `using Swordfish.NET.Collections;` supplies the `IConcurrentObservableBase<T>` base.

`IScoCollection2<T>` (`...\IScoCollection2.cs`):
- `internal interface IScoCollection2<T> : IConcurrentObservableCollection<T>`
- `using Swordfish.NET.General.Collections;` supplies the base.

Consumer graph (from `Grep IScoCollection2?\b`, `*.cs`, repo-wide — only three hits total):
- `IScoCollection2<T>`: defined only; **no implementer, no consumer** → dead.
- `IScoCollection<T>`: consumed only by `ISubjectMapSco`
  (`UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs:7 — public interface ISubjectMapSco : IScoCollection<SubjectMapEntry>`).

`ISubjectMapSco` graph (from `Grep ISubjectMapSco`, `*.cs`, repo-wide — only two hits):
- Definition at `ISubjectMapSco.cs:7`.
- One reference: `QuickFiler\Controllers\QfcExplorerController.cs:275`, as the parameter type of
  `private static void UpdateForMove(MailItem, string, CtfMap, ISubjectMapSco subMap)`.
- `UpdateForMove` has **no call site** (`Grep UpdateForMove` in `QuickFiler\**\*.cs` returns only
  the definition; the only other `UpdateForMove` in the repo is an unrelated private method in
  `ToDoModel\Email Utilities\SortItemsToExistingFolder.cs`). It is dead code.

Critically, the concrete `SubjectMapSco` does **not** implement `ISubjectMapSco`. Both partials
declare only `public partial class SubjectMapSco : ScoCollection<SubjectMapEntry>`
(`SubjectMapSco.cs:24`) and `public partial class SubjectMapSco`
(`SubjectMapSco.Orchestration.cs:16`). `ScoCollection<T>` itself does not implement
`IScoCollection<T>` either (`ScoCollection.cs:55` declares
`: ConcurrentObservableCollection<T>, IList<T>, IList`). Therefore `ISubjectMapSco` has no
implementer anywhere in the solution; it is a vestigial contract.

### End-state after F2 (reasoned)

F2 re-bases `SubjectMapSco`, `CtfMap`, and `RecentsList<T>` onto the clean collection and
re-points direct `ScoCollection<T>` consumers. F2 does not touch the interface files
(`IScoCollection.cs`, `IScoCollection2.cs`, `ISubjectMapSco.cs`); those are F5-owned. After F2,
`SubjectMapSco` still does not declare `ISubjectMapSco`, and the interface chain
`ISubjectMapSco : IScoCollection<T> : IConcurrentObservableBase<T> [Swordfish]` remains and still
imports `Swordfish.NET.Collections`. F5 must break this chain.

### Why removal, not migration

- `IScoCollection2<T>` is unambiguously dead.
- `IScoCollection<T>` and `ISubjectMapSco` have no live consumer (the sole reference is a
  never-called private method). Preserving a Swordfish-free equivalent contract would serve no
  surviving type.
- No drop-in Swordfish-free base exists. `Grep IConcurrentObservableBase|IConcurrentObservableCollection`
  over `UtilitiesCS\**\*.cs` returns only the two IScoCollection interface files. The clean stack
  (`UtilitiesCS.ReusableTypeClasses.Concurrent.Observable.*`) exposes `Bag` and `Dictionary`
  namespaces plus `SloLinkedList`, and a clean `IConcurrentObservableDictionary`, but no
  `IConcurrentObservableBase<T>` / `IConcurrentObservableCollection<T>`. Migrating would require
  authoring new base interfaces for a contract nobody consumes — contrary to the repo
  "Simplicity first" design principle.

### Required F5 edits for Q2

1. Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs`.
2. Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs`.
3. Delete `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs` (dead consequence of removing
   `IScoCollection<T>`; not owned by any other feature).
4. Remove the dead `UpdateForMove` method from `QuickFiler\Controllers\QfcExplorerController.cs`
   (or, minimally, retype/drop its `ISubjectMapSco` parameter). Removing the whole dead method is
   the simplest option; it also drops an unused `CtfMap` parameter reference and leaves no dangling
   symbol.

Lower-blast-radius fallback if the team prefers not to touch QuickFiler production code: keep
`ISubjectMapSco` but flatten it to a standalone interface (drop the `: IScoCollection<SubjectMapEntry>`
base and inline whatever members it needs), then delete only the two `IScoCollection*` files. This
is not recommended because `ISubjectMapSco` still has no implementer and its only reference is dead.

---

## Q3 — Test files referencing Swordfish types directly, and Sco* legacy tests

### Direct-Swordfish test files (from `Grep using Swordfish|Swordfish\.NET`, `*.cs`, `\UtilitiesCS.Test`)

Exactly three first-party test files use `using Swordfish.NET.Collections;` — all **F5-owned for
removal** (their subject type is deleted with the UtilitiesSwordfish project):

| File | Swordfish type exercised | Classification |
|---|---|---|
| `UtilitiesCS.Test\ReusableTypeClasses\ObservableDictionary_Tests.cs` | `Swordfish.NET.Collections.ObservableDictionary<TKey,TValue>` | F5 — remove |
| `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs` | `Swordfish.NET.Collections.ConcurrentObservableCollection<int>` (sender-identity regression) | F5 — remove |
| `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs` | `Swordfish.NET.Collections.ConcurrentObservableCollection<>` / `ConcurrentObservableBase` (LockRecursion regression) | F5 — remove |

The two `Concurrent\Observable\Collection` tests are regression guards for real bugs (CollectionChanged
sender identity; `LockRecursionException`) in the Swordfish `ConcurrentObservableBase` relay, and
their own comments reference `SubjectMapSco`. Because the Swordfish type they instantiate is
deleted, they cannot be "migrated" as-is. Their regression intent applies to whatever clean
collection base F2 re-bases `SubjectMapSco` onto. **F5 should remove these three tests and flag
that equivalent sender-identity and lock-recursion regression coverage must exist against the
clean collection base as part of F2** (verify at F5 execution; if absent, raise a new issue rather
than F5 re-authoring tests against a clean type it does not own).

### Sco* legacy tests (from `Glob **/*Sco*_Tests.cs`) — classification

| File | Type under test | Owner |
|---|---|---|
| `UtilitiesCS.Test\ReusableTypeClasses\ScoCollection_Tests.cs` | `ScoCollection<T>` (F2 re-base) | F2 (not F5) |
| `UtilitiesCS.Test\ReusableTypeClasses\ScoStack_Tests.cs` | `ScoStack<T>` (F2 re-base) | F2 (not F5) |
| `UtilitiesCS.Test\ReusableTypeClasses\ScoSortedDictionary_Tests.cs` | `ScoSortedDictionary` (F3 deletes class + test) | F3 (not F5) |
| `UtilitiesCS.Test\ReusableTypeClasses\ScoDictionaryNew_Tests.cs` | `ScoDictionaryNew` (clean) | F1 (not F5) |
| `UtilitiesCS.Test\EmailIntelligence\PeopleScoDictionaryNew_Tests.cs` | clean `*New` | F1 (not F5) |
| `UtilitiesCS.Test\NewtonsoftHelpers\PeopleScoConverter_Tests.cs` | clean converter | F1 (not F5) |
| `UtilitiesCS.Test\NewtonsoftHelpers\PeopleScoRemainingObjectConverter_Tests.cs` | clean converter | F1 (not F5) |
| `UtilitiesCS.Test\NewtonsoftHelpers\WrapperPeopleScoDictionaryNew_Tests.cs` | clean wrapper | F1 (not F5) |
| `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs` | `SubjectMapSco` (F2 re-base) | F2 (not F5) |
| `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Orchestration_Tests.cs` | `SubjectMapSco` (F2 re-base) | F2 (not F5) |

None of these ten `Sco*` test files reference `Swordfish` textually (they are absent from the
`Swordfish` `*.cs` search), so they migrate with their production type under F1/F2/F3 and are OUT
of F5 scope. F5's scope-item-4 phrase "any residual `Sco*` legacy tests not already migrated by
F1–F3" is a precondition to VERIFY at execution, not expected residual work: if F1–F3 do their
job, no `Sco*` legacy test remains for F5.

### Coverage implication (CLAUDE.md 80% floor / 90% changed-new)

Per the task directive, apply CLAUDE.md thresholds: repo-wide line coverage >= 80% on the testable
denominator; changed/new code >= 90%; MSTest + Moq + FluentAssertions. The three removed tests
exercise `Swordfish.NET.*` types in the `UtilitiesSwordfish` assembly, which is deleted wholesale
in F5 and leaves both the numerator and denominator. Their removal therefore does not reduce
coverage of any surviving first-party code. No coverage backfill is owed by F5 for these removals;
regression coverage of the clean collection base is F2's responsibility.
(Note: `.claude/rules/general-unit-test.md` states 85%/75% branch; CLAUDE.md in this worktree
states 80%/90%. The task instruction pins CLAUDE.md's 80/90 for this feature. The discrepancy is
pre-existing and out of F5 scope.)

---

## Q4 — Authoritative repo-wide "Swordfish" inventory (must be zero after F5)

Base searches: `Grep Swordfish` over `*.cs`, `*.csproj`, `*.sln` (repo-wide).

### A. First-party production `*.cs` (10 files) — eliminated by F1–F4, NOT F5

| File | Eliminated by |
|---|---|
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs` | F1 |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoCollection.cs` | F2 |
| `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs` | F3 |
| `UtilitiesCS\HelperClasses\Logging\TraceUtility.cs` (string literals lines 392–393) | F4 |
| `UtilitiesCS\EmailIntelligence\OlFolderTools\FolderRemap\FolderRemapController.cs` | F4 |
| `UtilitiesCS\EmailIntelligence\Flags\FlagDetails.cs` | F4 |
| `QuickFiler\Controllers\KeyboardHandler.cs` | F4 |
| `QuickFiler\Controllers\KbdActions.cs` | F4 |

(`ScoStack.cs` inherits `ScoCollection` but contains no literal "Swordfish" token, so it is not in
this text inventory; it is F2-owned via the base-type re-base.)

### B. First-party interfaces (2 files) — F5-owned

- `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs`
- `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs`
- (plus `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs`, which has no literal "Swordfish" token
  but is a required F5 deletion — see Q2.)

### C. First-party tests (3 files) — F5-owned (see Q3)

- `UtilitiesCS.Test\ReusableTypeClasses\ObservableDictionary_Tests.cs`
- `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs`
- `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs`

### D. `ProjectReference` entries to `UtilitiesSwordfish.NET.General.csproj` — F5-owned (NINE, not eight)

| csproj | Line | In F5 scope list? |
|---|---|---|
| `UtilitiesCS\UtilitiesCS.csproj` | 1083 | yes |
| `UtilitiesCS.Test\UtilitiesCS.Test.csproj` | 854 | yes |
| `TaskMaster\TaskMaster.csproj` | 510 | yes |
| `TaskMaster.Test\TaskMaster.Test.csproj` | 323 | yes |
| `QuickFiler\QuickFiler.csproj` | 451 | yes |
| `ToDoModel\ToDoModel.csproj` | 162 | yes |
| `Tags\Tags.csproj` | 89 | yes |
| `TaskVisualization\TaskVisualization.csproj` | 142 | yes |
| **`TaskVisualization.Test\TaskVisualization.Test.csproj`** | **297** | **NO — missing from scope** |

(A tenth `ProjectReference` at `UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj:132`
references General but is inside a folder F5 deletes wholesale, so it needs no separate edit.)

### E. Solution + vendored project internals — F5-owned

- `TaskMaster.sln` project declarations: lines 33–34 (`UtilitiesSwordfish.NET.General`,
  `{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}`) and 35–36 (`UtilitiesSwordfish.NET.Test`,
  `{9A04D222-2B52-4E93-9B92-CC6EF54D5848}`).
- `TaskMaster.sln` `GlobalSection(ProjectConfigurationPlatforms)` entries: lines 194–205
  (General GUID) and 206–217 (Test GUID). Both blocks must be removed (see Q5).
- `UtilitiesSwordfish\UtilitiesSwordfish.NET.General.csproj` (`RootNamespace`/`AssemblyName`
  `Swordfish.NET.General`) — deleted with the folder.
- `UtilitiesSwordfish.Test\UtilitiesSwordfish.NET.Test.csproj` (`RootNamespace`/`AssemblyName`
  `Swordfish.NET.Test`) — deleted with the folder.
- `UtilitiesSwordfish\Swordfish.NET.sln` (a nested vendored solution inside the deleted folder) —
  deleted with the folder.
- All 24 `UtilitiesSwordfish\**\*.cs` and 7 `UtilitiesSwordfish.Test\**\*.cs` matches — deleted
  with the two folders.

### F. Allowed-to-remain references (docs/memory, NOT code)

`Swordfish` legitimately remains in Markdown/planning artifacts: the epic manifest
(`docs\features\epics\swordfish-removal\epic.md`), this feature folder
(`docs\features\active\2026-07-10-swordfish-interface-project-teardown-308\*`), and any
`.claude\agent-memory\**`. The Cross-cutting Acceptance Criterion "repo-wide Swordfish search
returns only archived docs/memory" is satisfied when categories A–E are zero.

### End-state assertion for the plan

At F5 execution time (post-F1–F4 merge), the ONLY remaining first-party `Swordfish` references in
categories A–E must be exactly F5's targets: the two `IScoCollection*` interfaces (B), the three
tests (C), the nine `ProjectReference` entries (D), and the solution/folder structural items (E).
If any category-A production reference still resolves to `Swordfish`, that is an upstream F1–F4
defect and F5 preflight must halt rather than absorb it.

---

## Q5 — Build-order / breakage risks

1. **Ninth ProjectReference (TaskVisualization.Test.csproj:297).** Deleting the UtilitiesSwordfish
   project folder and removing its `.sln` entries while leaving this `ProjectReference` in place
   yields a dangling reference and a broken `TaskVisualization.Test` build. F5 MUST remove it even
   though it is absent from the scope-list of "eight" csprojs in the epic/issue. Its source
   (`ManageFiltersControllerTests.cs`, `AutoAssignPeopleTests.cs`) uses only clean/first-party
   types, so removal is safe.

2. **`.sln` GlobalSection cleanup.** Removing only the `Project(...)`/`EndProject` blocks
   (lines 33–36) leaves orphaned `GlobalSection(ProjectConfigurationPlatforms)` entries for both
   GUIDs (lines 194–217). Visual Studio tolerates orphans, but a clean teardown removes both the
   declarations and the configuration entries. `TaskMaster.sln` has no `GlobalSection(NestedProjects)`,
   so there are no solution-folder nesting rows to update.

3. **No app.config binding redirects.** `Grep Swordfish` over `*.config` (repo-wide) returns no
   matches. UtilitiesSwordfish is a project reference, not a NuGet package, so there is nothing in
   `packages.config` or any `app.config` `bindingRedirect` to clean up.

4. **No shared build props/targets.** `Grep Swordfish|UtilitiesSwordfish` over `*.props`/`*.targets`
   returns no matches.

5. **UtilitiesSwordfish.Test is a WPF app and nothing depends on it.** It is referenced by no other
   project (`Grep 9A04D222-...` finds the GUID only in `TaskMaster.sln` declaration/config rows and
   in the vendored `UtilitiesSwordfish\Swordfish.NET.sln`). It contains `App.xaml`, `MainWindow.xaml`,
   `ObservableSortedDictionaryTest.xaml`, `DictionaryTester.xaml`. It is deleted wholesale; no
   surviving project consumes its types.

6. **Surviving type needs from the Swordfish assembly.** After F1–F4, the only first-party
   references to `Swordfish.NET.*` are F5's own interface and test targets (categories B/C). Once
   those are removed, no surviving first-party code needs a type from the Swordfish assembly, so
   removing all nine `ProjectReference` entries and the two project folders cannot leave an
   unresolved type — CONDITIONAL on F1–F4 having landed. F5 preflight should assert category-A is
   zero before proceeding.

7. **TraceUtility string literals (F4, verify-only).** `UtilitiesSwordfish.NET.General` /
   `UtilitiesSwordfish.NET.Test` appear as trace-filter string literals in
   `TraceUtility.cs:392–393`. Per the epic, F4 updates/removes them. F5 verifies they are already
   gone; if F4 left them, they are stale strings (non-compiling-impact) that F5 should also clear
   to satisfy the repo-wide-zero criterion.

---

## Autonomy assessment

All F5 actions are fully scriptable and require no Visual Studio GUI-only operation:
- Interface/test/method edits and file deletions: text edits + `git rm`.
- `ProjectReference` removal from nine csprojs: text edits to XML `ItemGroup` blocks.
- `.sln` project-declaration and `GlobalSection` entry removal: text edits.
- Project-folder deletion (`UtilitiesSwordfish\`, `UtilitiesSwordfish.Test\`): `git rm -r`.

No human-interaction requirement is raised by this teardown.

---

## Plan-ready F5-owned edit/removal list (grouped by work item)

**WI-1 — Interfaces (Q2).**
- Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection2.cs`.
- Delete `UtilitiesCS\Interfaces\IReusableTypeClasses\IScoCollection.cs`.
- Delete `UtilitiesCS\Interfaces\IToDo\ISubjectMapSco.cs`.
- Remove dead `UpdateForMove` method from `QuickFiler\Controllers\QfcExplorerController.cs`
  (lines ~271–280), including its `ISubjectMapSco` parameter dependency.

**WI-2 — ProjectReferences (Q4.D / Q5.1) — NINE csprojs.**
- Remove the `UtilitiesSwordfish.NET.General.csproj` `ProjectReference` block from:
  `UtilitiesCS.csproj` (1083–1086), `UtilitiesCS.Test.csproj` (854–857),
  `TaskMaster.csproj` (510–513), `TaskMaster.Test.csproj` (323–326),
  `QuickFiler.csproj` (451–454), `ToDoModel.csproj` (162–165),
  `Tags.csproj` (89–92), `TaskVisualization.csproj` (142–145),
  and `TaskVisualization.Test.csproj` (297–300).  (Line numbers approximate; match on the block.)

**WI-3 — Solution + folders (Q4.E / Q5.2).**
- Remove `TaskMaster.sln` `Project(...)`/`EndProject` for `{F2E1680E-...}` (33–34) and
  `{9A04D222-...}` (35–36).
- Remove `TaskMaster.sln` `GlobalSection(ProjectConfigurationPlatforms)` entries for both GUIDs
  (194–205 and 206–217).
- `git rm -r UtilitiesSwordfish\` and `git rm -r UtilitiesSwordfish.Test\` (removes 24 + 7 `*.cs`,
  both csprojs, XAML, and the nested `Swordfish.NET.sln`).

**WI-4 — Tests (Q3).**
- Delete `UtilitiesCS.Test\ReusableTypeClasses\ObservableDictionary_Tests.cs`.
- Delete `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionSenderTests.cs`.
- Delete `UtilitiesCS.Test\ReusableTypeClasses\Concurrent\Observable\Collection\ConcurrentObservableCollectionLockRecursionTests.cs`.
- Flag (do not author): confirm F2 carries sender-identity and lock-recursion regression coverage
  against the clean collection base; if absent, open a new issue.

**WI-0 — Preflight assertion (precondition).**
- Before executing, assert category-A (production `*.cs`) `Swordfish` references are zero (F1–F4
  merged). Assert `TraceUtility.cs` no longer contains the `UtilitiesSwordfish.NET.*` literals.
  If either fails, halt: the upstream feature has not landed.

**Final gate.** Full C# toolchain in order (csharpier -> analyzers -> nullable -> MSTest), then
`Grep Swordfish` over `*.cs`/`*.csproj`/`*.sln` must return only archived docs/memory (categories
A–E zero). Solution must build and tests pass with both UtilitiesSwordfish projects removed.

---

## Rejected alternatives (brief)

- **Migrate `IScoCollection<T>`/`IScoCollection2<T>` to Swordfish-free contracts instead of
  removing.** Rejected: no surviving type consumes them; the clean stack exposes no
  `IConcurrentObservableBase`/`IConcurrentObservableCollection` equivalent, so migration means
  authoring new unused base interfaces — contrary to "Simplicity first."
- **Keep `ISubjectMapSco`, flatten its base only.** Rejected as the primary path (kept as a
  fallback): `ISubjectMapSco` has no implementer and its only reference is a dead method; full
  removal is simpler.
- **Attempt to migrate the three Swordfish regression tests to the clean type inside F5.**
  Rejected: the clean collection base is F2-owned; F5 removing the deleted-type tests plus flagging
  F2 for equivalent coverage keeps ownership boundaries intact.
