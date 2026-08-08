# Research — `QuickFiler/Viewers/IItemViewer.cs`

- Feature: `quickfiler-itemviewer-coverage` (issue #456), epic child F14 of `quickfiler-per-file-coverage` (#136)
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Produced: 2026-08-07T21-40
- Scope: one production file — `QuickFiler/Viewers/IItemViewer.cs` (133 lines)

---

## Determination (Q2)

**`QuickFiler/Viewers/IItemViewer.cs` contains zero coverable lines and must be classified
`interface-only / not-measured`.**

Consequent obligations, all verified below:

| Requirement | Determination |
| --- | --- |
| Ledger bucket | `interface-only / not-measured` — **not** `ratified-exempt` |
| `[ExcludeFromCodeCoverage]` | **must not be added**; the file carries none today and none is warranted |
| Reporting | **N/A**, never 0%, never counted as a failure |
| Tests | **none — zero tests.** Shape-assertion tests written to manufacture coverage are prohibited and none is proposed |
| Line / branch target | not applicable (no denominator) |
| 500-line rule | 133 lines — compliant, no action |

The epic's premise is upheld. No executable IL is emitted from this file.

---

## 1. Evidence that the file emits no IL

The file was read in full. Its structure is:

- `IItemViewer.cs:1-11` — eleven `using` directives (`System.Collections.Generic`,
  `System.ComponentModel`, `System.Threading`, `System.Threading.Tasks`, `System.Windows.Forms`,
  `System.Windows.Threading`, `BrightIdeasSoftware`, `Microsoft.Web.WebView2.WinForms`,
  `QuickFiler.Viewers`, `SVGControl`, `UtilitiesCS.Interfaces.IWinForm`). `using` directives are
  compile-time only and emit no IL.
- `IItemViewer.cs:13` — `namespace QuickFiler`.
- `IItemViewer.cs:15` — `public interface IItemViewer : IUserControl, IContainerControlLocal`.
- `IItemViewer.cs:17-131` — **member declarations only**, every one terminated by `;` or by a
  `{ get; }` / `{ get; set; }` accessor list with no bodies.
- `IItemViewer.cs:124` / `:129` — `#pragma warning disable CS0108` / `#pragma warning restore CS0108`.
  Preprocessor directives emit no IL.
- Comment blocks at `:40-42`, `:56-58`, `:76-79`, `:82-85`, `:102-106`, `:115-123`.
- `IItemViewer.cs:132-133` — closing braces.

Explicitly checked for and **absent**:

| Construct that would emit IL | Present? | Check |
| --- | --- | --- |
| Default interface member (a method or accessor with a body) | **No** | every one of the 68 members ends in `;` or a bodiless accessor list; there is no `=>` and no `{` opening a body anywhere between `:17` and `:131` |
| `static` member (including C# 8 static interface members) | **No** | the token `static` does not occur in the file |
| `const` field | **No** | the token `const` does not occur |
| Nested type (`class`, `struct`, `enum`, `record`, nested `interface`) | **No** | the only type-declaring token is the interface at `:15` |
| Field initialiser | **No** | interfaces cannot declare instance fields, and no `static` field is declared |
| Attribute with a constructor argument compiled into the file's IL | **No** | no attribute is applied anywhere in the file |
| Auto-implemented property with a body | **No** | all properties are `{ get; }` / `{ get; set; }` on an interface — declarations, not implementations |

Every member falls into one of four bodiless categories: property declarations (`:17-38`, `:43-52`,
`:66-74`, `:97`, `:125`, `:128`), event declarations (`:53`, `:59-72`, `:95-96`, `:98-99`, `:108`,
`:112`), method declarations (`:54`, `:80`, `:86-94`, `:100`, `:107`, `:109-113`, `:126-127`, `:131`),
and the interface declaration itself.

### 1.1 Why the file is 133 lines despite having no IL

The size is accounted for entirely by non-executable content: 11 `using` lines, 6 explanatory comment
blocks totalling ~24 lines (documenting Seams B, C, and D and the narrowing rationale), 2 `#pragma`
lines, blank separators, and 68 single-line member declarations. The delegation brief's caution that
"133 lines is large for an interface" is resolved: the file is large because it is a *wide* interface
with heavy documentation, not because it contains behavior.

---

## 2. Empirical confirmation with a positive control

Absence from a coverage report is only meaningful if the surrounding code was actually instrumented.
Following the method F7 used (`MailItemActionsAdapter` as positive control), the control here is
stronger because it is in the **same folder**:

**Positive control — `QuickFiler/Viewers/ItemViewerExpanded.cs` is instrumented.** It appears in the
committed report at
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml:5364`
as `<class ... filename="QuickFiler\Viewers\ItemViewerExpanded.cs">` with 106 `<line>` children.
`QuickFiler/Viewers/ItemViewerExpanded.Designer.cs` likewise appears at XML `:4112`. Instrumentation
of `QuickFiler\Viewers\` is therefore proven, and `IItemViewer.cs`'s absence is a true negative rather
than a folder-wide instrumentation gap.

**Negative result — `IItemViewer.cs` produces no `<class>` element.** A search of the report for
`IItemViewer` returns no `<class>` element with that filename. Coverlet emits a `<class>` element per
type that carries sequence points; a declaration-only interface carries none.

**Third confirmation — compilation membership.** The file is compiled, so its absence is not a build
exclusion: `QuickFiler/QuickFiler.csproj:392` contains `<Compile Include="Viewers\IItemViewer.cs" />`.
It is inside the epic's denominator by the dynamic-denominator rule (epic § "Mid-Wave File Creation",
rule 1) and needs a ledger row — just one whose bucket is `interface-only / not-measured`.

---

## 3. Why `ratified-exempt` would be the wrong bucket

`ratified-exempt` denotes production logic that was argued away against the irreducible-remainder
standard — a file with executable behavior that cannot be reached deterministically. `IItemViewer.cs`
has no executable behavior at all, so there is nothing to argue about and no remainder to ratify.
Placing it in `ratified-exempt` would:

1. imply an untested production-logic liability that does not exist;
2. invite an `[ExcludeFromCodeCoverage]` attribute, which per the epic is applied only to exempted
   files and would be inert here (the attribute would suppress nothing, because nothing is
   instrumented) while adding a `System.Diagnostics.CodeAnalysis` `using` and a false signal to
   reviewers;
3. inflate the count of exempted files that F16's capstone must reconcile.

This is the exact situation the epic's third bucket was created for
(epic § "Directives for F1's Ledger and Harness" → "A third ledger bucket:
`interface-only / not-measured`"): "files with **zero coverable lines** — not files that are hard to
test, files with no executable IL at all."

### 3.1 Harness requirement this file exercises

`IItemViewer.cs` is a live test of the epic's second harness-correctness requirement: *decide the
denominator on `<line>` child count, never `line-rate`*. Because the file produces **no `<class>`
element at all**, a harness that keys on filename will find no entry. The harness must report **N/A**
for a compiled file with no `<class>` element, and must not synthesise a `0%` row. A harness that
defaults a missing filename to 0% would report `IItemViewer.cs` as a failing file and would block
F14's acceptance on a file that cannot be covered by construction.

Recommended harness behavior for this file (to be confirmed against F1's deliverable):

- compiled file present in the csproj **and** no matching `<class>` element ⇒ report `N/A`, bucket
  `interface-only / not-measured`, exclude from both the numerator and denominator of any roll-up;
- compiled file present **and** a matching `<class>` element with zero `<line>` children ⇒ same
  treatment (the `<line>` child count, not `line-rate`, is the discriminator).

---

## 4. Tests (Q6 for this file)

**None. Zero test cases are proposed for `IItemViewer.cs`, and none should be planned.**

The epic states plainly: "Shape-assertion tests written purely to manufacture coverage for such a
file are prohibited." There is nothing to assert that the compiler does not already enforce — the
interface's shape is validated at compile time by `ItemViewer`'s implementation
(`QuickFiler/Viewers/ItemViewer.cs:21`, `public partial class ItemViewer : UserControl, IItemViewer,
IContainerControlLocal`) and by every `Mock<IItemViewer>` the existing `QuickFiler.Test` suite
creates. A reflection-based test asserting that `IItemViewer` declares `SenderText` would break on
every legitimate refactor while proving nothing about behavior.

---

## 5. Observations that are in scope for other files, not this one

These are recorded so the planner does not mistake them for `IItemViewer.cs` work.

- **`#pragma warning disable CS0108` at `:124-129`** suppresses "member hides an inherited member" for
  `InvokeRequired`, `Invoke(Delegate)`, `BeginInvoke(Delegate)`, and `Height`, which are re-declared
  over `ISynchronizeInvoke`/`IControl`. The in-file comment (`:115-123`) states the suppression is
  deliberate and narrowly scoped, and that adding `new` or restructuring the hierarchy is an API-shape
  change ruled out of scope by an earlier feature's AC7. That rationale is unchanged by this epic:
  F14's no-behavior-change NFR forbids the restructuring, and the suppression is documented in-code as
  `.claude/rules/general-code-change.md` requires. **No action.**
- **`IItemViewer.cs:131` declares `void RemoveControlsColsRightOf(Control furthestRight)`.** Its only
  implementation is `ItemViewer.cs:77`; `ItemViewerExpanded` does **not** implement `IItemViewer`
  (`ItemViewerExpanded.cs:16` is `: UserControl` only). Its only production call site is
  `QuickFiler/Controllers/EfcItemController.cs:247`. If the F14 plan extracts the shared geometry
  helper recommended in the `ItemViewerExpanded.cs` research, the interface member's **signature does
  not change** — only `ItemViewer`'s implementation body delegates. **No edit to `IItemViewer.cs` is
  required by that refactor**, which is the desired outcome: a change to this file would ripple into
  every `Mock<IItemViewer>` in `QuickFiler.Test` and into F10's `QfcItemController.*` files.
- **`IItemViewer.cs:86` declares `void SetFolderSuggestions(IReadOnlyList<UtilitiesCS.FolderRow> rows)`,**
  added by issue #325 and documented in-file as additive alongside `SetFolderItems(string[])`. This is
  a live contract consumed by F10 territory. F14 must not narrow or remove it.

---

## 6. Cross-child notes

**None.** `IItemViewer.cs` requires no change of any kind, so it generates no cross-child contract
change. It is explicitly recorded that F14 proposes **no edit** to this file — this matters because
the interface is consumed by F10 (`QfcItemController.*`) and F9 (`EfcItemController.cs:247`), and any
edit here would be a cross-child contract change requiring a note. There is none.

---

## 7. Latent defects

None found in this file. It contains no executable code, no dead code, and no policy violation. Its
one suppression is documented in-code with a rationale, as policy requires.

---

## 8. Open-issue scan

Method: GitHub public issue-search UI via WebFetch (no shell tool was available in this session, so
`gh issue list --state open --search ...` could not be run). Terms: `ItemViewer`, `ItemViewerExpanded`,
`expanded`, `designer`, `viewer`, `coverage`.

No open issue concerns `IItemViewer.cs`. The two open issues bearing on its **classification** rather
than its content are:

| Issue | Title | Relevance |
| --- | --- | --- |
| #432 | Feature: quickfiler-coverage-ledger | F1 — owns the three-bucket ledger this file lands in; the `interface-only / not-measured` bucket and the N/A-not-0% reporting rule are F1 deliverables |
| #441 | Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate | confirms the repo's Cobertura post-processing mishandles `<line>` accounting; reinforces § 3.1 — a declaration-only file must be discriminated by `<class>`/`<line>` presence, never by a rate |

---

## 9. Verified vs inferred

**Verified** (direct file read or search with cited evidence):

- Every member between `IItemViewer.cs:17` and `:131` is a bodiless declaration; no `static`, `const`,
  nested type, attribute, field initialiser, or default interface member is present.
- The file is compiled: `QuickFiler/QuickFiler.csproj:392`.
- The file produces no `<class>` element in the committed Cobertura report, while two sibling files in
  the same folder do (report `:4112`, `:5364`) — a true negative with a same-folder positive control.
- The file carries no `[ExcludeFromCodeCoverage]` attribute; the folder's attribute inventory is
  listed in the `ItemViewerExpanded.cs` research artifact and does not include this file.
- `ItemViewerExpanded` does not implement `IItemViewer` (`ItemViewerExpanded.cs:16`).

**Inferred** (stated as reasoning, not measurement):

- That the compiler emits no IL for this file follows from the C# language rules for bodiless
  interface members plus the exhaustive construct check in § 1; it was not confirmed by disassembling
  `QuickFiler.dll` (no shell tool available this session to run `ildasm`/`monodis`). The three
  independent report-based confirmations in § 2 make the conclusion safe, and it is corroborated by
  the epic's own finding that ~24 QuickFiler interface-only files behave identically.
