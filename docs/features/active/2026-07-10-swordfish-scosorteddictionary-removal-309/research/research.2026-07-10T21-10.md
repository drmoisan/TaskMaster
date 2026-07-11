# Research — F3 `ScoSortedDictionary` Removal (Issue #309, epic `swordfish-removal`)

- Feature folder: `docs/features/active/2026-07-10-swordfish-scosorteddictionary-removal-309/`
- Epic manifest: `docs/features/epics/swordfish-removal/epic.md`
- Researcher: task-researcher agent
- Timestamp: 2026-07-10T21-10

## Scope Restated

Deliverable is DELETION ONLY of:
1. `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`
2. `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`
3. The two matching `<Compile Include>` entries in `UtilitiesCS.csproj` and
   `UtilitiesCS.Test.csproj` (classic MSBuild format requires this in the same change).

Explicitly out of scope (belongs to other epic children F1/F2/F4/F5): deleting
`UtilitiesSwordfish`/`UtilitiesSwordfish.Test`, removing any `ProjectReference`, touching
`TaskMaster.sln`, migrating `IScoCollection`/`IScoCollection2`, or touching `ScoDictionary`,
`ScoCollection`/`ScoStack`, or `KbdActions` raw-usage cleanup.

---

## Q1 — Auditable repo-wide search for production consumers

### Scope searched

Repo root: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a94d2dd606e76374e`
(full working tree — no directory exclusions applied by the search tool other than the glob
filters listed per command).

### Commands and patterns

1. `grep -rn "ScoSortedDictionary" --glob "*.cs"` (all `.cs` files, repo-wide)
2. `grep -rn "ScoSortedDictionary" --glob "*.csproj"` (all `.csproj` files, repo-wide)
3. `grep -rn "ConcurrentObservableSortedDictionary" --glob "*.cs"` (all `.cs` files, repo-wide)
4. `grep -rn "ScoSortedDictionary|ConcurrentObservableSortedDictionary" --glob "*.json"`
   (all `.json` files, repo-wide — checks for `TypeNameHandling.Auto` `$type` payloads)
5. `grep -rn "ScoSortedDictionary|ConcurrentObservableSortedDictionary"` with no glob filter
   (all file types, repo-wide — catches docs, scripts, coverage XML, evidence artifacts)

### Results, classified

**Command 1 — `ScoSortedDictionary` in `*.cs`:**
- `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs` — 12 hits,
  all inside the class's own definition (constructors, `CreateEmpty`, `Deserialize`,
  `DeserializeJson`). Category (a) own definition.
- `UtilitiesCS.Test\ReusableTypeClasses\ScoSortedDictionary_Tests.cs` — 33 hits, all inside the
  test class (`[TestClass] ScoSortedDictionary_Tests`, `new ScoSortedDictionary<...>(...)`,
  `typeof(ScoSortedDictionary<TKey, TValue>)` reflection in the test-local helper
  `OverrideScoSortedDictionaryField`). Category (b) own test.
- No other `.cs` file anywhere in the repo (production or test, any project) references the
  identifier.

**Command 2 — `ScoSortedDictionary` in `*.csproj`:**
- `UtilitiesCS\UtilitiesCS.csproj:1047` —
  `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs" />`
  (verified by direct read: line 1047, surrounded by the sibling `SCO/*` compile entries
  `ScoCollection.cs` line 1045, `SCODictionary.cs` line 1046, `ScoStack.cs` line 1048).
  Category (c) build entry.
- `UtilitiesCS.Test\UtilitiesCS.Test.csproj:414` —
  `<Compile Include="ReusableTypeClasses\ScoSortedDictionary_Tests.cs" />` (verified by direct
  read: line 414, between `ScoDictionaryNew_Tests.cs` line 413 and `SloLinkedList_Tests.cs`
  line 415). Category (c) build entry.
- No other csproj (`TaskMaster.csproj`, `QuickFiler.csproj`, `ToDoModel.csproj`, `Tags.csproj`,
  `TaskVisualization.csproj`, `UtilitiesSwordfish*.csproj`, etc.) contains the token.

**Command 3 — `ConcurrentObservableSortedDictionary` in `*.cs`:**
- `UtilitiesSwordfish\Collections\ConcurrentObservableSortedDictionary.cs` — the Swordfish base
  class definition itself. Category (a)-adjacent: this is the *base type's own* definition, not a
  consumer of `ScoSortedDictionary`; it is untouched by F3 (its removal is F5's concern, gated on
  all of F1–F4 completing).
- `UtilitiesSwordfish.Test\ObservableSortedDictionaryTest.xaml.cs:44` —
  `new ConcurrentObservableSortedDictionary<string, string>()`. This tests the Swordfish base type
  directly, not `ScoSortedDictionary`. Category (a)-adjacent / out of F3 scope (F5 territory).
- `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs:17` — the
  `: ConcurrentObservableSortedDictionary<TKey, TValue>` base-class declaration, i.e. the same
  file already classified as (a) own definition.
- No other `.cs` file references `ConcurrentObservableSortedDictionary`.

**Command 4 — either token in `*.json`:** zero files matched. No persisted JSON fixture, sample
data, or config embeds either type name. This directly answers the reflective/`$type` sub-question
in Q1: no on-disk `TypeNameHandling.Auto` payload references `ScoSortedDictionary` or its base.

**Command 5 — either token, no glob filter (all file types):** 126 total files matched (both
tokens combined; de-duplicated below). Beyond the four `.cs`/`.csproj` files already classified,
every remaining hit is one of:
- Category (d) non-code/historical artifact:
  - `scripts\temp-extract-coverage.ps1:30` — the token appears inside a large regex alternation
    used only to bucket per-file coverage percentages into "easy/medium/hard" categories for a
    2026-03-19 coverage-uplift research script. It is a filter-list label, not a reference to the
    type; the script does not construct, call, or import `ScoSortedDictionary`.
  - Approximately 115 `*.cobertura.xml` / `coverage*.xml` / `*.trx` files under
    `docs/features/archive/**/evidence/**` and `docs/features/active/2026-06-19-...-208/evidence/**`
    — these are historical coverage/test-result artifacts from unrelated past features. They
    contain the class name only because Cobertura enumerates every class the coverage run touched
    (including `ScoSortedDictionary` and its test, which existed and were exercised at the time
    those reports were generated). None of these represents a current production dependency; they
    are frozen snapshots of past QA runs.
  - `docs\features\archive\2026-03-19-utilities-coverage-part-three-87\**` and
    `docs\features\archive\2026-03-13-utilities-coverage-65\**` (research.md, plan files,
    per-file-coverage tables) — archived coverage-uplift feature documentation that lists
    `ScoSortedDictionary` as one of many classes in a coverage inventory table. Historical
    documentation, not a code reference.
  - `docs\features\archive\2026-03-20-triage-null-classifier-group-88\evidence\baseline\baseline-csharpier.md`
    — a csharpier baseline log from an unrelated feature; incidental mention.
- Category — this feature's own planning docs (not a "consumer", expected self-reference):
  `docs\features\active\2026-07-10-swordfish-scosorteddictionary-removal-309\issue.md`, `spec.md`,
  `user-story.md`, and `docs\features\epics\swordfish-removal\epic.md`.

No hit falls into category (e) genuine production consumer.

### Reflective / indirect-reference check

- `nameof`/`typeof` usage: the only `typeof(ScoSortedDictionary<...>)` in the repo is inside
  `ScoSortedDictionary_Tests.cs`'s own `OverrideScoSortedDictionaryField<TKey, TValue>` helper
  (test-local reflection to swap the static `_showMessageBox` prompt delegate for the test's own
  instance). This is test-internal reflection against the class under test, not an external
  consumer.
- DI/registration by string name: no `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`
  string literal appears in any DI container registration, factory, or settings file (confirmed by
  the unfiltered search above — the only string-literal-adjacent hit is the coverage-categorization
  regex in `temp-extract-coverage.ps1`, which is a label match against class names for reporting,
  not a runtime type resolution).
- JSON `$type` payloads: zero matches in any `*.json` file repo-wide (command 4).

### Q1 Conclusion

No genuine production consumer (category e) exists. Every hit outside the class's own definition,
its own test, and the two `<Compile Include>` build entries is either the unrelated Swordfish base
type's own file (out of F3 scope), this feature's own planning documents, or a non-code historical
artifact (coverage XML/TRX from past runs, or a dev-only coverage-categorization script). This
confirms and expands the starting evidence supplied in the delegation prompt; no BLOCKING finding.

---

## Q2 — Open question: is a Swordfish-free sorted dictionary wanted for future use?

`docs/features/epics/swordfish-removal/epic.md` addresses this directly in two places:

- **Shared Design** section (line 65–66): "**Sorted dictionary:** NONE exists Swordfish-free. Do
  not assume one; deletion of the unused `ScoSortedDictionary` is the scoped action (see F3)."
- **F3 workstream detail** (line 138–142): "`ScoSortedDictionary.cs` ... has no known production
  consumer ... Confirm no consumer, then delete the class and its test. A Swordfish-free sorted
  type, if wanted for future use, is scoped separately; it cannot inherit `ScoDictionaryNew` (hash)
  and needs a new clean base or a sort-maintaining decorator."
- **Non-Goals** section (line 190): "No new Swordfish-free sorted dictionary implementation
  (scoped separately if wanted)."

No repo evidence (current callers, open issues, or roadmap notes found during this research pass)
indicates an active, scoped demand for a Swordfish-free sorted dictionary today — the only
consumer that ever existed was `ScoSortedDictionary` itself, and Q1 established it has zero
production callers. The epic manifest treats this as a genuinely open, deferred question rather
than a committed follow-on: "if wanted" language in both the Shared Design and Non-Goals sections
signals no current requirement has been identified.

**Finding:** Confirmed — a Swordfish-free sorted dictionary is explicitly out of scope for F3. If
ever wanted, it cannot simply inherit `ScoDictionaryNew` (hash-based dictionary, no ordering
guarantee) and would require either a new clean sorted base or a sort-maintaining decorator wrapped
around an existing observable/serializable collection. This is a separately-scoped future
enhancement with no current driving consumer identified in this repository.

---

## Q3 — Deletion surface and build implications

### Files to delete

1. `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/ScoSortedDictionary.cs`
2. `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs`

### `<Compile Include>` entries to remove (same change, classic MSBuild format)

1. `UtilitiesCS\UtilitiesCS.csproj:1047`:
   `<Compile Include="ReusableTypeClasses\Serializable\Concurrent\SCO\ScoSortedDictionary.cs" />`
2. `UtilitiesCS.Test\UtilitiesCS.Test.csproj:414`:
   `<Compile Include="ReusableTypeClasses\ScoSortedDictionary_Tests.cs" />`

Both projects use the classic (non-SDK-style) `.csproj` format with explicit `<Compile Include>`
item lists (confirmed by reading the surrounding lines in both files — dozens of sibling
`<Compile Include>` entries enumerate every `.cs` file individually, e.g. `ScoCollection.cs`,
`SCODictionary.cs`, `ScoStack.cs`, `ScoDictionaryNew_Tests.cs`, `SloLinkedList_Tests.cs`
immediately adjacent to the two target lines). Unlike SDK-style projects (`<Project Sdk="...">`
with implicit globbing), a classic project does not auto-discover `.cs` files; a `<Compile
Include>` entry pointing at a file that no longer exists on disk causes an MSBuild error
(`error MSB3030: Could not copy the file ... because it was not found` / a build-break on missing
source). Removing the two lines in the same change as the file deletions is mandatory, not
optional cleanup.

### Companion-file check

`Glob` over `UtilitiesCS/ReusableTypeClasses/Serializable/Concurrent/SCO/*` returns exactly four
files: `ScoCollection.cs`, `SCODictionary.cs`, `ScoSortedDictionary.cs`, `ScoStack.cs`. No
`.Designer.cs`, `.resx`, `.xaml`/`.xaml.cs` pairing, or other companion file exists for
`ScoSortedDictionary.cs`. `ScoSortedDictionary_Tests.cs` likewise has no companion file (it is a
plain MSTest class file). No additional `<Compile Include>`, `<None Include>`, `<EmbeddedResource
Include>`, or `<Content Include>` entries reference either filename beyond the two `<Compile
Include>` lines already identified (confirmed by the repo-wide csproj search in Q1, which found
only the two hits).

### Q3 Conclusion

Deletion surface is exactly two `.cs` files and two `<Compile Include>` lines, with no companion
artifacts. No `.sln` change is implicated (project structure, not solution structure).

---

## Q4 — Shared test infrastructure coupling check

Read `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs` in full (452 lines). All
helper members are `private static` and scoped to the `ScoSortedDictionary_Tests` class itself:

- `InvokeNonPublic<T>(object target, string methodName, params object[] args)` — private static
  reflection helper, used only within this file.
- `StopPendingTimer(object target)` — private static, used only within this file.
- `CreateInvalidFilePath()` — private static, used only within this file.
- `OverrideScoSortedDictionaryField<TKey, TValue>(string fieldName, object replacement)` —
  private static, name-scoped to this class, used only within this file.
- `CallbackDisposable` — a `private sealed class` nested inside `ScoSortedDictionary_Tests`, not
  visible outside it.
- `RepoRoot` — a `private static readonly string` field local to this class.

No `[TestClass]` inheritance, no shared abstract test base class, no `partial class` split across
files, and no `InternalsVisibleTo`-dependent helper unique to this test file. The class accesses
`ScoSortedDictionary`'s `internal` `Deserialize(FilePathHelper, bool)` overload and non-public
members (`AskUser`, `CreateEmpty`, `_timer`, `_showMessageBox`) via ordinary MSTest project
`InternalsVisibleTo` wiring and reflection — the same repo-wide mechanism every other
`UtilitiesCS.Test` file already relies on for its own class-under-test; it is not something other
test files borrow from `ScoSortedDictionary_Tests.cs` specifically. No other test file in the repo
references `ScoSortedDictionary_Tests`, `OverrideScoSortedDictionaryField`, `CallbackDisposable`,
or any other symbol defined in this file (confirmed: the Q1 repo-wide `.cs` search found the
`ScoSortedDictionary` token nowhere outside this file and the production class).

### Q4 Conclusion

Deleting `ScoSortedDictionary_Tests.cs` removes no shared test infrastructure used elsewhere. It
is a fully self-contained test class with no cross-file coupling.

---

## GO / STOP Recommendation

**GO.** All four research questions resolve favorably for deletion:

1. No genuine production consumer of `ScoSortedDictionary` or `ConcurrentObservableSortedDictionary`
   exists anywhere in first-party source, csproj files, or persisted JSON. All non-definitional,
   non-test hits are historical/non-code artifacts or this feature's own planning docs.
2. A Swordfish-free sorted dictionary is confirmed out of scope for F3 by the epic manifest itself
   (Shared Design, F3 detail, and Non-Goals sections all concur); no current consumer would need it.
3. The deletion surface is exactly two `.cs` files and two `<Compile Include>` lines (one per
   classic csproj), with no companion `.Designer.cs`/resx files.
4. The test file is fully self-contained; no other test depends on anything it defines.

No BLOCKING finding was raised. F3 can proceed to spec/plan finalization and execution as scoped:
delete `ScoSortedDictionary.cs`, delete `ScoSortedDictionary_Tests.cs`, remove the two `<Compile
Include>` lines (`UtilitiesCS.csproj:1047`, `UtilitiesCS.Test.csproj:414`), and run the full C#
toolchain (csharpier → analyzers → nullable → MSTest) to confirm the build and test suite remain
green with no unrelated coverage regression.

## Candidate Approaches Considered

Only one viable approach exists for this deliverable (deletion is the entire scope; there is no
alternative implementation strategy to compare). No "candidate approaches" comparison applies —
the research questions themselves (confirm no consumer, confirm deletion surface, confirm no
shared test coupling) constitute the full technical due diligence needed before executing a
straight deletion. Rejected alternatives: none considered, since the epic manifest already fixes
the action (deletion) and F3's only open questions were verification questions, not design
choices.

## Testing Implications

- No new tests are required; the change is a pure deletion of a class and its dedicated test.
- Post-deletion verification is regression-only: full solution build (classic csproj format,
  `msbuild` per the C# toolchain) and full MSTest run must remain green, confirming the removed
  `<Compile Include>` lines did not leave a dangling reference and no other test depended on the
  deleted symbols (consistent with the Q4 finding).
- Coverage: removing a class and its 100%-dedicated test in the same change should not change the
  repository-wide coverage percentage materially; the atomic-executor should confirm no regression
  on the coverage of files that remain (i.e., no lines in *other* files lost coverage as a side
  effect of this deletion, which is expected to be zero given the class had no consumers).
