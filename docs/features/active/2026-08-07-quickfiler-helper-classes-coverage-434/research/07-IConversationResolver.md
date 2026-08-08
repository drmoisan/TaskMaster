# F4 per-file research — `QuickFiler/Helper Classes/IConversationResolver.cs`

Timestamp: 2026-08-07T22-40

Cluster: CONVERSATION-RESOLUTION (artifacts 05–08). Cross-cutting facts are in
`research/00-cluster-overview.md`.

Upstream contract: child F1 owns the per-file line-coverage harness and the ratified exemption ledger
at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
This artifact does not define its own coverage measurement. `coverage.config` is a shared file this
child must not modify.

**Headline finding.** This file is an interface-only declaration with **zero executable statements**
and therefore an **empty coverage denominator**. It also has **zero consumers**: no field, parameter,
local, generic argument, or mock in production or test code anywhere in the repository is typed as
`IConversationResolver`. The correct disposition is an F1-ledger classification, not test authoring.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/IConversationResolver.cs` | — |
| Line count | 33 (last line `}` at `:33`) | `IConversationResolver.cs:33` |
| Compiled | yes | `QuickFiler/QuickFiler.csproj:348` — `<Compile Include="Helper Classes\IConversationResolver.cs" />` |
| `[ExcludeFromCodeCoverage]` | **absent** | grep across `QuickFiler/Helper Classes/` returns no match |
| Type declared | `public interface IConversationResolver` (`:12`) | `:12` |
| Namespace | `QuickFiler.Helper_Classes` (`:10`) | `:10` |
| Executable statements | **0** — all 13 members are abstract declarations; there is no default interface implementation, no static member, no field initializer | `:14-31` |
| 500-line limit | 33 / 500 — compliant with 467 lines of headroom | — |
| `using` directives | 8 (`:1-8`), all consumed by member signatures: `System` (`Action`), `System.Collections.Generic` (`List`, `IList`), `System.ComponentModel` (`PropertyChangedEventHandler`, `PropertyChangedEventArgs`), `System.Threading` (`CancellationToken`), `System.Threading.Tasks` (`Task`), `Microsoft.Data.Analysis` (`DataFrame`), `Microsoft.Office.Interop.Outlook` (`MailItem`), `UtilitiesCS` (`MailItemHelper`) | `:1-8` |

`Pair<T>`, used in five of the member signatures, is declared in the sibling F4 file
`QuickFiler/Helper Classes/ConversationResolver.cs:18`.

---

## 2. Member inventory (coverage denominator for THIS file)

All members are abstract declarations. Decision points: **0** for every member. Sequence points:
**0** for every member. This table is therefore an API-shape inventory, not a coverage denominator.

| # | Kind | Signature | Line |
| --- | --- | --- | --- |
| 1 | property | `Pair<List<MailItemHelper>> ConversationInfo { get; set; }` | `:14` |
| 2 | property | `Pair<IList<MailItem>> ConversationItems { get; set; }` | `:15` |
| 3 | property | `Pair<int> Count { get; }` | `:16` |
| 4 | property | `Pair<DataFrame> Df { get; }` | `:17` |
| 5 | property | `Action<List<MailItemHelper>> UpdateUI { get; set; }` | `:18` |
| 6 | property | `bool FullyLoaded { get; }` | `:19` |
| 7 | property | `object Parent { get; }` | `:20` |
| 8 | event | `event PropertyChangedEventHandler PropertyChanged;` | `:22` |
| 9 | method | `Task BackgroundInitInfoItemsAsync(CancellationToken token);` | `:24` |
| 10 | method | `void Handler_PropertyChanged(object sender, PropertyChangedEventArgs e);` | `:25` |
| 11 | method | `Task<Pair<List<MailItemHelper>>> LoadConversationInfoAsync(CancellationToken token, bool backgroundLoad);` | `:26-29` |
| 12 | method | `Task LoadConversationItemsAsync(CancellationToken token, bool backgroundLoad);` | `:30` |
| 13 | method | `Task LoadDfAsync(CancellationToken token, bool backgroundLoad);` | `:31` |

**Coverage denominator for this file: 0 executable lines.** Cobertura reports such a file either as
absent from the `<packages>/<classes>` set entirely, or as a `<class>` element with an empty `<lines>`
collection and `line-rate="1"` or `line-rate="0"` depending on the emitter. See §12 for how F1's
harness must treat that.

---

## 3. Existing test inventory

**No test file anywhere in the repository references `IConversationResolver`.** A repository-wide grep
for the identifier returns exactly six matches, none of which is a consumer:

| Match | Nature |
| --- | --- |
| `QuickFiler/QuickFiler.csproj:348` | the `<Compile Include>` entry |
| `QuickFiler/Helper Classes/IConversationResolver.cs:12` | the declaration itself |
| `QuickFiler/Helper Classes/ConversationResolver.cs:30` | the base list of the sole implementer |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md:281` | this epic's file assignment |
| `docs/features/active/.../research/00-cluster-overview.md:149` | the cluster overview |
| `docs/features/archive/2026-07-18-utilitiescs-nullable-outlook-mailitem-item-371/research/research.2026-07-18T22-15.md:104` | a historical research note |

Consequences established by that grep:

1. **Exactly one implementer** — `ConversationResolver` (`ConversationResolver.cs:30`).
2. **Zero consumers** — no production or test code declares a variable, field, parameter, property,
   return type, generic argument, or `Mock<>` of this interface. Every consumer of the resolver uses
   the **concrete** `ConversationResolver` type; the full inventory of those call sites is in
   `05-ConversationResolver.md` §8.1–8.2.

### 3.1 Why nothing consumes it — the interface is incomplete relative to its consumers

The interface omits members that sibling-owned call sites depend on, so it cannot be substituted for
the concrete type at any existing site without further work:

| Member consumers need | Declared on `IConversationResolver`? | Consumer evidence |
| --- | --- | --- |
| `MailItem Mail { get; }` | **no** | `QuickFiler.Test/Controllers/EfcDataModelTests.cs:80` |
| `MailItemHelper MailHelper { get; set; }` | **no** | `QuickFiler/Controllers/EfcDataModel.cs:232` |
| `internal Pair<DataFrame> LoadDf()` | **no** | `QuickFiler/Controllers/EfcDataModel.cs:67` |
| `internal CancellationToken Token`, `internal CancellationTokenSource TokenSource` | **no** | set from the static factories, `ConversationResolver.cs:96-97, 139-140, 174-175` |
| `static Task<ConversationResolver> LoadAsync(...)` × 3 | **no** (static members cannot appear on a pre-C#-8 interface, and this project targets a legacy non-SDK toolchain) | `QuickFiler/Controllers/EfcDataModel.cs:115, 125`; `QuickFiler/Controllers/QfcItemController.Conversation.cs:85` |
| `Parent` **setter** | **no** — declared get-only at `:20`, but the class exposes `protected internal set` (`ConversationResolver.cs:292`) | `QuickFiler/Controllers/EfcDataModel.cs:121, 132` |

`Count` (`:16`) and `Df` (`:17`) are likewise declared **get-only** on the interface while the class
exposes `internal set` (`ConversationResolver.Loading.cs:270`) and a public setter
(`ConversationResolver.Loading.cs:205`) respectively.

This is a factual observation about why the interface is unused. Widening it is **not** F4 work: F4's
acceptance criteria are coverage and no behaviour change, and every seam this cluster needs is
delivered on the concrete class without touching this interface (`05-ConversationResolver.md` §7,
`06-ConversationResolver.Loading.md` §7).

---

## 4. Per-member coverage gap

| # | Member | Status |
| --- | --- | --- |
| 1–13 | all | **not applicable — no coverable lines exist.** An abstract interface member emits no IL body and therefore no sequence point. The interface's members are indirectly satisfied by `ConversationResolver`'s implementations, whose coverage is measured against `ConversationResolver.cs` and `ConversationResolver.Loading.cs`, not against this file |

There is no coverage gap to close in this file, because there is nothing in it to cover.

---

## 5. Testability classification per member

| # | Member | Classification |
| --- | --- | --- |
| 1–13 | all | **not classifiable** — the three categories (`pure-testable-now`, `needs-seam`, `host-bound-irreducible`) describe executable code. An abstract declaration is none of these. |

For completeness on the Interop question: five member signatures name Outlook Interop types —
`Pair<IList<MailItem>> ConversationItems` (`:15`) references
`Microsoft.Office.Interop.Outlook.MailItem`. **No Interop API is invoked**; the type appears only in a
signature. `MailItem` is a COM interface and is therefore mockable with Moq if a consumer ever needs
it (`00-cluster-overview.md` §3), but no such consumer exists today.

---

## 6. Ordering and async invariants

**This file declares no behaviour and therefore carries no ordering or async invariants of its own.**
It *names* four asynchronous members (`BackgroundInitInfoItemsAsync` `:24`,
`LoadConversationInfoAsync` `:26-29`, `LoadConversationItemsAsync` `:30`, `LoadDfAsync` `:31`) and one
`INotifyPropertyChanged`-shaped pair (`PropertyChanged` `:22`, `Handler_PropertyChanged` `:25`), but
the invariants those members must satisfy live with the implementation and are enumerated in
`05-ConversationResolver.md` §6 (INV-1 … INV-9) and `06-ConversationResolver.Loading.md` §6
(INV-10 … INV-21).

Two shape observations that the atomic plan must respect:

- **`Handler_PropertyChanged` is declared `void` (`:25`), not `Task`.** This is the interface-level
  reason seam S4 (`06-ConversationResolver.Loading.md` §7) extracts an `internal` awaitable core
  rather than changing the handler's return type. Changing `:25` to `Task` would additionally break
  the `PropertyChangedEventHandler` method-group conversions at
  `QuickFiler/Controllers/EfcDataModel.cs:69` and `QuickFiler/Controllers/EfcItemController.cs:667`,
  both sibling-owned.
- **`backgroundLoad` appears on three method signatures (`:29`, `:30`, `:31`)** even though
  `LoadDfAsync`'s implementation never reads it (`ConversationResolver.Loading.cs:231`) and the other
  two only assign an unused local (`:83-85`, `:190-192`). The **parameter** must be retained; only the
  unused locals are removable. See `06-ConversationResolver.Loading.md` §2.

**Banned-API audit: not applicable.** The file contains no statements. A grep of
`QuickFiler/Helper Classes/` for `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, and
`Random.Shared` returns no match. **No banned-API finding.**

---

## 7. Seam proposal

**None. No seam is required or proposed for this file.**

The seams this cluster needs (S1 `IUiDispatcher` injection, S2 `MailItemHelper` factory delegates,
S4 `HandlePropertyChangedCoreAsync` extraction) are all declared on the **concrete** class and are
deliberately kept off this interface. Rationale, in the epic's seam-hierarchy terms:

1. The interface has zero consumers, so adding a member to it enables no test that is not already
   enabled by an `internal` member on the class (reachable via
   `InternalsVisibleTo("QuickFiler.Test")`, `QuickFiler/Properties/AssemblyInfo.cs:5`).
2. Adding a member to a public interface is a public-API change. `.claude/rules/csharp.md` § Public
   surface directs keeping the public surface minimal and preferring `internal`.
3. Every added interface member would have to be implemented by any future implementer, including
   test fakes, for no present benefit.

### Rejected alternatives (brief)

- **Widen `IConversationResolver` to cover `Mail`, `MailHelper`, `Parent` setter, and `LoadDf`, then
  retarget sibling call sites to the interface.** Rejected: it would require edits to
  `QuickFiler/Controllers/EfcDataModel.cs` (F5), `EfcItemController.cs` (F9),
  `QfcItemController.*.cs` (F10), `QfcCollectionController.cs` (F11), and two sibling-owned
  `Interfaces/*.cs` files — a guaranteed multi-child merge conflict, for zero coverage gain on any
  F4 file. If the maintainer wants the interface made usable, that is a separate refactor issue,
  promoted through the lifecycle after epic #136 lands.
- **Delete the file** (see §12 disposition analysis). Rejected as the recommendation, with reasons.

---

## 8. Cross-child conflict analysis

### 8.1 Files outside F4 that call into this file

**None.** The repository-wide grep in §3 is exhaustive: the only code references are the file's own
declaration (`:12`) and the base list of `QuickFiler/Helper Classes/ConversationResolver.cs:30`, which
is itself an F4 file. There is no sibling-owned file to conflict with.

### 8.2 Verdict for the recommended disposition

| Action | Verdict |
| --- | --- |
| **Recommended: retain the file unchanged, classify it in F1's ledger** | **Requires no file change at all** — no production edit, no test edit, no csproj edit. Zero conflict surface. |
| Hypothetical: delete `IConversationResolver.cs` | Would require (a) removing `IConversationResolver` from the base list at `QuickFiler/Helper Classes/ConversationResolver.cs:30` (F4-owned, safe) and (b) **removing `<Compile Include="Helper Classes\IConversationResolver.cs" />` from `QuickFiler/QuickFiler.csproj:348`** — a **shared file** edited by all thirteen sibling children. That csproj edit is the conflict risk. |
| Hypothetical: widen the interface | **Would require editing sibling-owned files in F5, F9, F10, and F11.** PROHIBITED for this child (see §7). |

### 8.3 Intra-F4 coordination

`Pair<T>`, referenced five times in this file's signatures, is declared in
`QuickFiler/Helper Classes/ConversationResolver.cs:18` — the same F4 child, analysed in artifact 05.
No other F4 cluster (theme helpers, viewer queues, `EmailMoveMonitor`) touches this file.

---

## 9. 500-line compliance

| File | Now | After proposed work | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/IConversationResolver.cs` | 33 | 33 (no change proposed) | 500 — **compliant** |

No new production file is proposed, so no `<Compile Include=...>` entry is needed in
`QuickFiler/QuickFiler.csproj` (`Helper Classes\` block, `:342-354`). No new test file is proposed, so
no entry is needed in `QuickFiler.Test/QuickFiler.Test.csproj` (`Helper Classes\` block, `:158-165`).
**This file contributes zero shared-file conflict surface.**

---

## 10. Recommended test cases

**Zero. No test case is recommended for this file.**

This is a reasoned exclusion, not an omission:

1. **There is nothing to cover.** All 13 members are abstract declarations with no IL body and no
   sequence point. No test can raise this file's line coverage above its current value, because the
   denominator is empty.
2. **The interface's contract is already compiler-enforced.** The only assertion a reflection-based
   "shape test" could make — that `ConversationResolver` implements `IConversationResolver` with the
   declared member set — is enforced at compile time by the base list at
   `ConversationResolver.cs:30`. A runtime test such as
   `typeof(IConversationResolver).IsAssignableFrom(typeof(ConversationResolver)).Should().BeTrue()`
   can never fail while the solution builds, so it is a tautology with a maintenance cost.
3. **The behaviour behind each member is tested against the implementation files.** The 71 test cases
   enumerated in `05-ConversationResolver.md` §10 and `06-ConversationResolver.Loading.md` §10 exercise
   every one of the 13 interface members through the concrete type; their coverage is correctly
   attributed to `ConversationResolver.cs` and `ConversationResolver.Loading.cs`.
4. **Repository policy explicitly anticipates this case.** `.claude/rules/general-unit-test.md`
   § Coverage Requirements: *"Type-only / interface-only modules with no executable behavior may be
   omitted from coverage measurement. Examples: … C# interface-only files. Such modules legitimately
   report 0% executable coverage and may be excluded from measurement. This is a clarification only;
   it does not lower any coverage threshold."* Epic.md § Scope likewise records that "~24 [of the 121
   compiled files] are interface-only declarations with no executable behavior".

**Category coverage (AC5).** The issue's acceptance criterion "coverage per file spans the positive
path plus invalid-input, boundary, and error-handling behavior" is **vacuously satisfied** for this
file: there is no path of any category to exercise. The atomic plan should record that explicitly so
the criterion is not read as unmet.

**If the plan nevertheless requires a non-empty test artifact for this file**, the single defensible
option is one guard test asserting that every member declared on `IConversationResolver` is publicly
implemented by `ConversationResolver` with a matching signature (reflection over
`typeof(IConversationResolver).GetMembers()` against
`typeof(ConversationResolver).GetInterfaceMap(typeof(IConversationResolver))`). It would live in
`QuickFiler.Test/Helper Classes/ConversationResolverContractTests.cs`, be categorised `boundary`, and
would still contribute **zero** lines to this file's coverage. It is **not recommended**, for reason
2 above.

---

## 11. STA determination

**No STA test is required for this file, and none is possible.** The file declares no type with an
executable body, constructs nothing, and touches no WinForms or WPF surface. Epic.md Shared Design §3
permits STA only as a last resort for never-shown in-memory controls; there is no control here and no
test to place in an STA file. `QuickFiler.Test` remains free of `[STATestClass]`,
`[STATestMethod]`, and `*.StaTests.cs` (`00-cluster-overview.md` §5), and this artifact proposes no
change to that.

---

## 12. Projected coverage and F1-ledger disposition

### 12.1 Projected coverage

**Undefined / not applicable.** The file's line-coverage denominator is 0. It cannot "reach 80%" and
it cannot "fall short of 80%"; the metric is not defined on an empty set. Any per-file report that
prints `0%` for this file is reporting a division-by-zero artifact of the emitter, not a coverage
deficiency.

### 12.2 How Cobertura represents it

Two emitter behaviours are possible and F1's harness must handle both:

- **Class element absent.** Many Cobertura emitters omit a `<class>` element for a type with no
  method bodies, so the file simply does not appear in the report. F1's per-file harness must then
  treat "file compiled but absent from the report" as *no coverable lines*, **not** as 0%.
- **Class element present with an empty `<lines/>` collection.** Some emitters write the `<class>`
  with `line-rate="0"` or `line-rate="1"` and zero `<line>` children. F1's harness must key on the
  **`<line>` count**, not on `line-rate`, when deciding whether a file has a denominator.

Either way the correct classification is *no coverable lines*, and the file must not be counted in
the numerator or denominator of any aggregate.

### 12.3 Recommended disposition — RETAIN + ledger classification

**Recommendation: leave `QuickFiler/Helper Classes/IConversationResolver.cs` exactly as it is, and
request an F1-ledger classification of `no-coverable-lines`** (an interface-only declaration),
explicitly distinct from `ratified-exempt`, which presumes coverable-but-untestable lines.

Rationale:

1. **Zero conflict surface.** Retaining requires no edit to any file, including the shared
   `QuickFiler/QuickFiler.csproj`. Deleting would require removing `:348` from that shared csproj —
   the single highest-probability merge-conflict surface in this epic
   (`00-cluster-overview.md` §1.3 makes the same point for the test csproj).
2. **Policy already covers it.** `.claude/rules/general-unit-test.md` names C# interface-only files as
   legitimately reporting 0% executable coverage; epic.md § Scope counts ~24 such files across
   QuickFiler. A blanket, consistent ledger treatment for all of them is F1's job, and F4 should
   consume that treatment rather than invent a one-off deletion.
3. **It documents intent.** The interface records the resolver's intended consumer-facing contract.
   Even unused, it is the artifact a future WebView2/Office.js port would start from (epic.md
   § Non-Goals directs preferring host-neutral extraction where a seam choice is open).
4. **Deletion is not behaviour-neutral in the same trivial sense as `cInfoMail.cs`** (artifact 08).
   `cInfoMail.cs` declares no type at all; this file declares a `public` type in a `public` assembly.
   Removing a public type is a public-API break, and `.claude/rules/csharp.md` § Public APIs directs
   calling out breaking changes explicitly rather than folding them into a coverage feature.
5. **No `[ExcludeFromCodeCoverage]`.** Do not add the attribute. Epic.md Shared Design §1 treats the
   attribute on a testable seam as Blocking, and the correct signal here is a ledger classification,
   not an in-source suppression. The file currently has no such attribute and must keep none.

**Rejected disposition — delete the file and its `<Compile Include>` line.** Defensible on
dead-code grounds (zero consumers), but it buys nothing: the coverage metric is unchanged either way
because the denominator is empty in both states, while the cost is a public-API removal plus an edit
to the most conflict-prone shared file in the epic. If the maintainer wants the dead interface
removed, promote it as a separate hygiene issue to be executed after epic #136 fans in, when the
csproj is not being edited concurrently by thirteen children.

**No exemption request against F1's ledger is required in the `ratified-exempt` sense**; what is
required is the distinct `no-coverable-lines` classification described above, cited by file name in
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.
