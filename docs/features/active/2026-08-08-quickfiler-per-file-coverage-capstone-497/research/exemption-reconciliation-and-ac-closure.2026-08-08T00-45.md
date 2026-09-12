# F16 Capstone Research — Exemption/Ledger Reconciliation, AC Closure Mapping, Defect Trail

- **Feature:** `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497` (issue **#497**, epic child **F16**)
- **Epic:** `quickfiler-per-file-coverage` (parent issue **#136**), integration branch `epic/quickfiler-per-file-coverage-integration`
- **Branch under analysis:** `feature/quickfiler-per-file-coverage-capstone`
- **Worktree:** `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a8b4d64f3ad6053b3`
- **Timestamp:** 2026-08-08T00-45
- **Scope boundary:** This artifact covers the exemption/ledger reconciliation surface, the acceptance-criteria closure mapping, and the defect trail. The measurement harness, csproj denominator derivation, Cobertura parsing mechanics, and toolchain command forms are covered by a parallel researcher and are **not** duplicated here.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED:` none — no non-canonical evidence path was supplied.

**Tooling constraint on this session (affects reproducibility of some checks).** No Bash/shell tool was available. All source facts were derived with `Grep`/`Glob`/`Read` against the checkout; all GitHub facts were derived with `WebFetch` against `github.com/drmoisan/TaskMaster`, not `gh`. Where a `gh` invocation was requested in the brief, the equivalent web fetch is recorded and the substitution is called out.

---

## 0. Verified prerequisites and the state of F1's deliverables

Two facts change how F16 must be planned and are stated up front.

1. **F1's ledger and harness do not exist on this branch.** `docs/features/epics/quickfiler-per-file-coverage/` contains exactly one file, `epic.md`. There is no `coverage-ledger.md` and no harness script under the epic root. F7's approved plan already anticipates this and records it explicitly at `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/plan.2026-08-07T20-41.md:68`:

   > "F1 (`quickfiler-coverage-ledger`, wave 0) delivers the ratified ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and the per-file coverage harness. **Neither artifact exists on disk today**"

   Every F16 acceptance condition that reads the ledger is therefore an **execution-time** dependency, exactly as F7 framed its own. F16 must not carry a planning-time or preflight-time assertion that `coverage-ledger.md` exists.

2. **Issue #497's body is an unpopulated template, and so is the capstone's `spec.md`.** `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497/spec.md` lines 10-66 are all placeholder prompts ("What need or gap does this idea address?", "- Criterion 1"). The authoritative acceptance-criteria source for F16 is therefore **issue #136's eight criteria plus the epic brief**, not #497's own body. #497's body must be populated before it can function as an AC source.

3. **Five of the fourteen sibling feature folders are not on this branch.** Present under `docs/features/active/`: `...-coverage-ledger-432` (F1), `quickfiler-queue-admission-coverage` (F2), `...-keyboard-actions-coverage-430` (F3), `...-helper-classes-coverage-434` (F4), `...-datamodel-coverage-436` (F5), `...-qfc-form-explorer-controller-coverage-435` (F6), `...-qfc-home-controller-coverage-433` (F7), `...-efc-home-controller-coverage-437` (F8), `...-collection-controller-coverage-454` (F11), `...-breadcrumb-dropdown-webview-coverage-455` (F13), plus the capstone. **Absent: F9 (452), F10 (453), F12 (495), F14 (456), F15 (496).** Any artifact-existence check F16 runs across siblings must be run on the *integration branch after fan-in*, not on this branch.

---

## Q1 — Independent re-derivation of the live `[ExcludeFromCodeCoverage]` census

### Q1.1 Result: F1's census is confirmed exactly. Zero numeric discrepancies.

| Figure | F1 recorded | Independently re-derived (this branch) | Match |
| --- | --- | --- | --- |
| Compiled files declaring a real attribute | 21 | **21** | yes |
| Total attribute usages in compiled files | 40 | **40** | yes |
| Type-level usages | 14 | **14** | yes |
| Member-level usages | 26 | **26** | yes |
| Compiled files fully coverage-suppressed | 24 | **24** | yes |
| Compiled files mentioning the attribute only in a comment | 5 | **5** | yes |
| Compiled files (`<Compile Include=…>`) | 121 | **121** | yes |

Raw totals behind those numbers: a literal-string grep for `ExcludeFromCodeCoverage` across `QuickFiler/` returns **54 lines**. Of those, **7 are prose/doc-comment mentions**, leaving **47 real attribute applications**. **7 of the 47 are in files that are not `<Compile Include=…>`** (uncompiled orphans), leaving **40 in the compiled set across 21 files**.

The 121-file count was re-derived by counting `<Compile Include="` entries in `QuickFiler/QuickFiler.csproj`: Controllers lines 290-341 (52), Helper Classes 342-354 (13), Interfaces 355-368 (14), `Properties\AssemblyInfo.cs` 369 (1), `Properties\Resources.Designer.cs` 370 + `Properties\Settings.Designer.cs` 375 (2), Viewers 380-459 (39). **52+13+14+1+2+39 = 121.**

### Q1.2 The complete per-file inventory (21 declaring compiled files, 40 usages)

**Type-level (14 usages, 14 files):**

| File | Line | Type annotated | `partial`? | Files suppressed |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/EfcFormController.cs` | 27 | `internal class EfcFormController` (:28) | no | 1 |
| `QuickFiler/Controllers/EfcItemController.cs` | 25 | `internal class EfcItemController` (:26) | no | 1 |
| `QuickFiler/Controllers/KeyboardHandler.cs` | 22 | `internal class KeyboardHandler` (:23) | no | 1 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | 21 | `public class QfcCollectionController` (:22) | no | 1 |
| `QuickFiler/Controllers/QfcExplorerController.cs` | 20 | `internal class QfcExplorerController` (:21) | no | 1 |
| `QuickFiler/Controllers/QfcDatamodel.cs` | 25 | `public partial class QfcDatamodel` (:26) | **yes** | 3 |
| `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` | 166 | `internal sealed class FolderScoringService` (:167) | no | **0** — see Q1.4 |
| `QuickFiler/Viewers/ItemViewer.cs` | 20 | `public partial class ItemViewer` (:21) | **yes** | 7 |
| `QuickFiler/Viewers/EfcViewer.cs` | 20 | `public partial class EfcViewer : Form` (:21) | **yes** | 2 |
| `QuickFiler/Viewers/QfcFormViewer.cs` | 17 | `public partial class QfcFormViewer : Form` (:18) | **yes** | 2 |
| `QuickFiler/Viewers/QfcItemViewerExpanded.cs` | 18 | `public partial class QfcItemViewerExpanded` (:19) | **yes** | 2 |
| `QuickFiler/Viewers/WebView2Messenger.cs` | 20 | `public sealed class WebView2Messenger` (:21) | no | 1 |
| `QuickFiler/Viewers/WebView2CoreInitializer.cs` | 15 | `public sealed class WebView2CoreInitializer` (:16) | no | 1 |
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 29 | `public sealed class WebView2BreadcrumbHost` (:30) | no | 1 |

Suppressed-file total: `1+1+1+1+1+3+0+7+2+2+2+1+1+1` = **24**.

**Member-level (26 usages, 7 files):**

| File | Lines | Count |
| --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` | 105, 380, 383, 390, 394, 412, 457 | 7 |
| `QuickFiler/Controllers/QfcItemController.Initialization.cs` | 138, 168, 200, 260, 291, 403, 436 | 7 |
| `QuickFiler/Controllers/QfcItemController.EventHandlers.cs` | 60, 83, 97, 111, 125 | 5 |
| `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` | 38, 132, 253 | 3 |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` | 173, 191 | 2 |
| `QuickFiler/Controllers/QfcItemController.Conversation.cs` | 79 | 1 |
| `QuickFiler/Controllers/QfcItemController.EventWiring.cs` | 99 | 1 |

**The 7 uncompiled orphans carrying a real attribute** (present in the working tree, absent from `QuickFiler.csproj`, therefore outside the denominator and outside every child's mandate): `Viewers/EfcViewer3.cs:17`, `Viewers/QfcFormViewerDark.cs:16`, `Viewers/QfcFormViewerExpanded.cs:16`, `Viewers/QfcItemViewer.cs:18`, `Viewers/QfcItemViewerExpandedLight.cs:14`, `Viewers/QfcItemViewerV1.cs:14`, `Viewers/QfcItemViewerLightSelected.cs:15`.

**The 5 doc-comment-only files are confirmed exactly as the epic states** — `Controllers/QfcScanProgressBandMapper.cs:12`, `Viewers/ItemViewer.Commands.cs:10`, `Viewers/ItemViewer.DisplayState.cs:9`, `Viewers/ItemViewer.FolderSearch.cs:17`, `Viewers/ItemViewer.WebViewThread.cs:12`. Two further files carry *both* a doc-comment `<see cref="ExcludeFromCodeCoverage"/>` **and** a real attribute — `WebView2Messenger.cs` (comment :17, attribute :20) and `WebView2CoreInitializer.cs` (comment :12, attribute :15). A naive "first match per file" classifier would mis-file both as comment-only; the classifier must evaluate every occurrence in a file, not the first.

Note that four of the five comment-only files (the `ItemViewer.*` partials) **are** fully suppressed — by inheritance from `ItemViewer.cs:20`. Comment-only and suppressed are orthogonal properties.

### Q1.3 The concrete detection recipe

This is the recipe F16 should implement. It is deliberately three-pass; a single regex cannot answer (a), (b), and (c).

**Pass 0 — build the denominator.** Parse `<Compile Include="([^"]+)"` out of `QuickFiler/QuickFiler.csproj` at evaluation time (the epic mandates a dynamic denominator, § *Mid-Wave File Creation and the Ledger Denominator*, rule 1). Normalize `\` to `/`. Anything not in this set is out of scope even if it carries an attribute — this is what removes the 7 orphans.

**Pass (a) — real attribute vs. doc-comment mention.** For each denominator file, scan line by line and classify each occurrence of the literal `ExcludeFromCodeCoverage`:

- Strip the line's leading whitespace. If the remainder starts with `//` or `///` or `*`, or the occurrence is inside a `<see cref="…"/>` / `<c>…</c>` construct, it is a **mention**.
- Otherwise, require the occurrence to sit inside a bracketed attribute list: the nearest preceding non-whitespace character on the logical line is `[` or `,`, and a `]` follows on the same logical line. This admits `[ExcludeFromCodeCoverage]`, the fully-qualified `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` (present at 20 of the 40 compiled usages), the `Attribute`-suffixed spelling, and multi-attribute brackets `[Foo, ExcludeFromCodeCoverage]` / `[ExcludeFromCodeCoverage, Foo]`.
- **Both prefixed and unprefixed spellings occur in this codebase.** All 19 `QfcItemController.*` member-level usages and the `QfcHighConfidencePreFilter.cs:166` type-level usage use the fully-qualified form; the other 20 use the short form. A pattern anchored on `^\s*\[ExcludeFromCodeCoverage\]$` misses exactly half the census.
- No multi-attribute bracket currently exists in `QuickFiler/`, but the recipe must tolerate one because children are adding attributes during execution.

**Pass (b) — type-level vs. member-level.** Do not use indentation depth; it is a proxy that happens to work today and will break on a nested type or a file-scoped namespace. Use the **next declaration** instead: from the attribute line, skip forward over further attribute lines, blank lines, comment lines, and modifier-only lines, and read the first declaration token sequence. If it matches `(class|struct|record|interface|enum)\s+\w+`, the attribute is **type-level**; anything else (a method signature, a property, an event, a constructor) is **member-level**. Applying this rule to the 40 compiled usages reproduces 14/26 exactly.

**Pass (c) — suppressed-by-propagation vs. self-declared.** This is a **per-type** computation, never per-file, and it has two halves:

1. Build a map `type fully-qualified name -> set of declaring files` by scanning every denominator file for `(class|struct|record|interface)\s+(\w+)` declarations together with the enclosing `namespace`. A type is *exempt* if **any** of its declaring files carries a type-level attribute on it. Because a partial type may be annotated only once (a second annotation is CS0579), exactly one file declares it.
2. A **file** is *fully suppressed* iff **every type declared in that file** is exempt. A file is *partially suppressed* iff some but not all are.

The second half is the part a naive implementation gets wrong, and this repository contains a live counterexample (Q1.4).

**Cross-check.** The recipe's output must satisfy the identity `declaring files (21) + comment-only files (5) + orphan files (7) = 33`, which is precisely the "33 exemptions" figure the original manifest survey reported. Reproducing 33 from the three disjoint parts is a cheap proof that the classifier is partitioning correctly rather than coincidentally landing on 21.

### Q1.4 The one narrative discrepancy against F1's record (numbers agree; the stated mechanism does not)

The epic explains the 24-vs-21 gap solely by partial-class propagation:

> "**24 files are fully suppressed**, because a type-level attribute on a partial type propagates to every partial of that type."

That is true but incomplete, and the omission is load-bearing for a capstone reconciliation. `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs` declares **four** types — `internal static class QfcHighConfidencePreFilter` (:23), `public readonly struct QfcPreScoredItem` (:98), `internal interface IFolderScoringService` (:130), and `internal sealed class FolderScoringService` (:167). The file's single type-level attribute sits at line **166**, on `FolderScoringService` only. The file is therefore **partially** suppressed: the primary type is fully instrumented, one secondary type is not.

Consequences the capstone must encode:

- **"Has a type-level attribute" does not imply "the file is exempt."** A ledger row that classifies `QfcHighConfidencePreFilter.cs` as `ratified-exempt` on the strength of a type-level attribute is wrong; the file is `testable` and is F2's assignment, with one exempt adapter type inside it.
- **Its measured line rate is computed against a reduced denominator.** The `FolderScoringService.ScoreAsync` body (lines 170-189) is absent from the report. A capstone that recomputes the file's rate from source line counts rather than from the Cobertura `<lines>` block will disagree with the harness.
- The count reconciles because this file contributes **0** suppressed files, not 1: `1+1+1+1+1+3+0+7+2+2+2+1+1+1 = 24`. An implementation that assumes one type-level attribute suppresses one file would compute 25 and would then "correct" the epic's 24 in the wrong direction.

Two further recorded-figure notes:

- **The epic's `[X]` markers now number exactly 21 and agree with the declaring-file count.** Counting `[X]` in § *Feature File Assignments*: F2 1, F3 1, F5 1, F6 1, F9 3, F10 6, F11 1, F13 4, F14 1, F15 2 = **21**. An earlier revision of this epic reportedly carried 26 markers agreeing with neither figure; on this branch that inconsistency is gone. The `[X]` set is now usable as a cross-check on the declaring-file set.
- **`[ExcludeFromCodeCoverage]` is not the only attribute that removes code from the Cobertura report, and the capstone must not conflate them.** `QuickFiler/Properties/Resources.Designer.cs:23` carries a **type-level** `[global::System.Diagnostics.DebuggerNonUserCodeAttribute()]`, which is why the file is absent from the report entirely; `QuickFiler/Properties/Settings.Designer.cs` carries the same attribute at **member** level on seven members (lines 27, 39, 48, 60, 72, 84, 96) while its type carries only `CompilerGenerated` (:14) and `GeneratedCode` (:15), which is why a 4-line residue is still instrumented and reports 0.0%. `DebuggerNonUserCode` is **not** the ratified exemption mechanism named in `CLAUDE.md:303`, and a file suppressed by it has no ledger disposition at all today. F16 should treat "absent from the report" as having three possible causes — `[ExcludeFromCodeCoverage]`, `DebuggerNonUserCode`, and zero coverable lines — and require the ledger to name which.

---

## Q2 — The four ratified exemption grounds, verbatim, with acceptance and rejection tests

### Grounds 1-3 — `CLAUDE.md` § UT2, lines 298-303 (verbatim)

> `CLAUDE.md:298`
> ```
>   - **COM/VSTO/WinForms coverage exemption (testable denominator).** The 80% floor applies to the **testable denominator** — production-only first-party code, after excluding:
> ```
> `CLAUDE.md:299`
> ```
>     - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility registration) that cannot be unit-tested without a live Outlook process;
> ```
> `CLAUDE.md:300`
> ```
>     - (b) WinForms form-derived classes and Designer-generated code;
> ```
> `CLAUDE.md:301`
> ```
>     - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.
> ```
> `CLAUDE.md:303`
> ```
>     These classes are formally exempted from the 80% floor. Exemption is applied via `[ExcludeFromCodeCoverage]` attributes in source code (reviewable in PRs) or via `coverage.config` assembly-level excludes for near-wholly-untestable assemblies. **Authority**: This exemption must be ratified by the project maintainer and is tracked in `feature/csharp-coverage-uplift`. Testable seams within otherwise-COM-bound assemblies (e.g., `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, path/settings helpers) are explicitly NOT exempt and must meet the `>= 80%` floor.
> ```

### Ground 4 — the epic's own ratification (verbatim from `epic.md`)

Heading: `## Epic Ruling: a fourth exemption ground for prohibited-to-execute adapters (F13)`.

> "**Ruling: ratify a fourth ground — irreducible adapters whose execution is *prohibited* by test policy.**"

> "A file qualifies under this fourth ground only when **all** of the following hold. Any failure means extract a seam and cover the code instead:
>
> 1. Every member is a **pure 1:1 forward** to a third-party or host API — no branching, no computation, no state, nothing a test could meaningfully assert beyond the forward itself.
> 2. Executing any member would require an external process, an external runtime, or a filesystem side effect that `.claude/rules/general-unit-test.md` prohibits in a unit test.
> 3. A **seam interface exists** and the consuming code is tested against that interface, so the untested surface is the adapter alone and not the logic behind it.
> 4. The type is `sealed` and **not `partial`** (see the `#457` trap below), and the attribute is applied at **type level**."

> "This ground is ratified **for this epic only** and recorded here rather than in `CLAUDE.md`, which this epic does not amend. Extending it repository-wide requires maintainer ratification, exactly as the existing §UT2 exemption did."

The epic also fixes ground 4's scope by worked example: of the three WebView2 files, **only `WebView2CoreInitializer` survives**; `WebView2BreadcrumbHost.InitializeAsync` "is already testable behind a seam its own constructor injects, so its exemption must be removed and the code covered."

### Q2.1 The competing policy the capstone must reconcile against

`.claude/rules/general-unit-test.md:33` (verbatim):

> "No production file may be excluded from coverage measurement. Every production source file is in the denominator of the coverage metric, regardless of whether its lines are reachable in the test environment."

`.claude/rules/general-unit-test.md:46` (verbatim):

> "**Enforcement:** Feature-review agents must treat any `exclude` entry that matches a production source path as a **Blocking** finding."

The epic settles the collision at § *Policy reconciliation — the load-bearing epic-level decision*:

> "**refactor first, exempt only the irreducible remainder.** The qualifier "without an injectable seam" in the CLAUDE.md exemption is read as a live obligation, not a standing permission — if a seam can be introduced, the exemption does not apply and the file must be covered. `[ExcludeFromCodeCoverage]` on a *testable* seam is a Blocking finding."

### Q2.2 What a capstone reviewer must see to ACCEPT an `exempt-with-ground` ledger row

These are the per-ground acceptance tests. All rows additionally require the four **universal** conditions listed after the table.

| Ground | Required, individually checkable evidence in the ledger row |
| --- | --- |
| **1 — VSTO add-in lifecycle** | (i) The named type is an add-in entry point, ribbon event handler, or COM utility-registration class — cited by file path and type name. (ii) A named symbol in the file that cannot resolve without a live Outlook process. (iii) A statement that no logic remains in the file beyond host wiring, with the extracted host-neutral module named. **`QuickFiler.csproj` contains no such class today**, so any ground-1 row in a QuickFiler ledger is itself suspicious and must be challenged. |
| **2 — WinForms form-derived / Designer-generated** | Either (i) the type declaration line, quoted, showing a base of `Form`, `UserControl`, `ToolStripMenuItem`, or another `System.Windows.Forms` type; or (ii) the file is `*.Designer.cs` **and** carries `GeneratedCodeAttribute` or is the `InitializeComponent` partial. The row must name the *type* and list *every* file that type is declared in, because the attribute is per-type (Q1.3 pass (c)). |
| **3 — Outlook Interop event handler without an injectable seam** | (i) The quoted `using`/type reference showing a direct dependency on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder`. (ii) The event-handler member(s) by name — the ground is written for *event handler classes*, not for any COM-touching class. (iii) **A written, file-specific argument that no injectable seam is feasible**, naming the seam that was considered and the concrete obstacle. (iv) A statement that no logic behind the COM call remains in the file. |
| **4 — irreducible prohibited-to-execute adapter** | All four conjunctive conditions, each separately evidenced: (1) an enumeration of **every** member with its body quoted, each a 1:1 forward with no branch/computation/state; (2) the specific `.claude/rules/general-unit-test.md` clause the execution would violate — external process, external runtime, or filesystem side effect — named, not gestured at; (3) the seam interface named by file path **and** a named test that exercises a consumer against that interface; (4) the type declaration quoted showing `sealed` and the absence of `partial`, plus the attribute line showing type-level placement. |

**Universal conditions on every `exempt-with-ground` row, regardless of ground:**

- **U1 — Named ground.** The row states exactly one of grounds 1-4. A row with no ground, or with "COM/WinForms" as an unresolved disjunction, is rejectable.
- **U2 — Type-scoped disposition with the full file list.** Because a type-level attribute suppresses every partial (`epic.md`: confirmed on `QfcDatamodel.cs:25`; `ItemViewer.cs:20` suppresses 7 files), the row must be keyed on the type and must enumerate the suppressed files. `CLAUDE.md:303` itself says "**These classes** are formally exempted", not these files.
- **U3 — Attribute placement recorded.** `file:line` of the attribute and whether it is type-level or member-level.
- **U4 — Disposition count reconciles to 40, not 21.** The epic states the AC requiring "a disposition for every existing attribute is satisfied by **40 dispositions**, not 21." F16 must count dispositions, not files.

### Q2.3 What makes a row REJECTABLE

Each of the following is independently sufficient for a **Blocking** finding naming the owning child. None may be waived by F16, and F16 may never itself grant an exemption to close a gap.

1. **A seam exists or is feasible.** This is the single most important test, and it follows from the epic reading "without an injectable seam" as a live obligation. Concretely rejectable: the row asserts ground 3 but the file already takes an injected dependency, or a sibling has already demonstrated the seam. `WebView2BreadcrumbHost` is the epic's own worked instance — an attribute that survives on it after F13 executes is Blocking against F13, not a ledger row F16 may accept.
2. **The ground does not textually cover the file.** The epic's own finding: "`CLAUDE.md` §UT2 enumerates three exemption grounds … and **none of them textually covers the WebView2 files**. … All three existing attributes therefore rest on a ground that does not exist." Ground 4 now covers exactly one of the three. A row asserting ground 1, 2, or 3 for a non-VSTO, non-form-derived, non-Outlook-Interop file is rejectable on its face.
3. **A named non-exempt seam type.** `CLAUDE.md:303` names `ToDoLoader`, `IDList` arithmetic, `KbdActions<>`, and path/settings helpers as "explicitly NOT exempt". `QuickFiler/Controllers/KbdActions.cs:14` declares `public class KbdActions<TKey, UClass, VDelegate>`; any exemption reaching it is rejectable by direct citation.
4. **Ground-4 conjunct failure.** Any one of the four fails: a member contains a branch or computation; the execution is merely *inconvenient* rather than *prohibited*; no seam interface exists or no consumer test exercises it; the type is `partial`, or unsealed, or the attribute is member-level. Note conjunct 4 is not decorative — a `partial` type silently exempts every partial, and a member-level attribute triggers #457 (Q4).
5. **Member-level attribute used to hide a residual.** A member-level attribute is admissible only where a whole-type ground-4 adapter is impossible. Where it is used, #457 applies and the row must carry the lambda analysis of Q4. A member-level attribute asserted under ground 4 contradicts conjunct 4 outright.
6. **A `ratified-exempt` classification on a zero-coverable-lines file.** That is the third bucket, which explicitly "must **not** be classified `ratified-exempt`" and "**none receives `[ExcludeFromCodeCoverage]`**" (Q3).
7. **An exemption used to close a coverage gap discovered during execution.** The epic's rejection of the "exempt rather than fix" pattern is explicit in the `QfcExplorerController.cs` ruling: "The alternative — ratifying an exemption for code everyone agrees should be deleted — is exactly the 'exempt rather than fix' pattern this epic's policy reconciliation rejects".
8. **A `coverage.config` assembly-level exclude touching QuickFiler.** `CLAUDE.md:303` permits assembly-level excludes only for "near-wholly-untestable assemblies", and `.claude/rules/general-unit-test.md:46` makes a production-path exclude Blocking. F1's research recorded that no `coverage.config` or `.runsettings` entry excludes QuickFiler today; F16 should re-verify that this is still true at fan-in, because a child could regress it.
9. **A new file claiming exemption without a ground.** Per the epic, "**New files default to `testable` at >= 90%.** … Claiming `ratified-exempt` for a newly created file requires a rationale meeting one of the three grounds."

---

## Q3 — The third ledger bucket (`interface-only / not-measured`)

### Q3.1 The deciding test

The epic's rule and the harness rule together give a two-sided test, and the textual authority is `.claude/rules/general-unit-test.md:29` (verbatim):

> "Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only files, and C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold."

**Authoritative (measurement-side) test.** A denominator file belongs in the third bucket iff, after unioning all Cobertura `<class>` elements whose `filename` equals that file, the union's class-level `<lines>` block contains **zero `<line>` children**. This is the epic's own harness rule 2: "**Decide the denominator on `<line>` child count, never `line-rate`.** A declaration-only file reports `line-rate="0"` because it has no lines, not because it is uncovered."

**Necessary disambiguation.** Zero `<line>` children has three causes, and only the first is the third bucket:
1. the file emits no executable IL (third bucket);
2. every type in the file is `[ExcludeFromCodeCoverage]` (`ratified-exempt`, 24 files today);
3. every type in the file carries type-level `DebuggerNonUserCode` (`Resources.Designer.cs` today — currently unclassified by any bucket).

So the measurement test must be conjoined with the Q1 attribute census: **third bucket = zero `<line>` children AND no `[ExcludeFromCodeCoverage]` anywhere in the file AND no type-level `DebuggerNonUserCode`.**

**Positive control (mandatory).** The epic records that F7 "evidenced the same for both its interface files, three ways, using `MailItemActionsAdapter` as a positive control to prove the folder was instrumented." `QuickFiler/Interfaces/MailItemActionsAdapter.cs:12` is a real `public sealed class` with nine expression-bodied members (lines 23, 26, 29, 32, 37, 38, 42, 45). If it is absent from the report, the report did not instrument `Interfaces/` and every "zero lines" verdict in that folder is a false positive. F16 must re-run this control rather than inherit F7's result.

**Source-side screen (fast, pre-measurement).** A denominator file is a third-bucket candidate if, outside comments and `using`/`namespace`/assembly-attribute lines, it declares only `interface`, `enum`, and/or `delegate` types — i.e. it contains none of `=>`, `static`, `const`, `class`, `struct`, `record`, and no `{ get; …} =` initializer and no constructor. This screen is a filter, not the verdict; the Cobertura test decides. On this branch the screen and the epic's own claims agree.

### Q3.2 Enumeration — 26 compiled files qualify today

All 26 were screened directly. `Grep` for `=>|\bstatic\b|\bconst\b|\bclass\b|\bstruct\b` returned **no matches** in each interface file listed, and none of the 26 appears in the 21-file declaring set of Q1.2.

**Interface/enum declaration-only (23):**

| Folder | Files |
| --- | --- |
| `Controllers/` (4) | `IQfcFormController.cs`, `IQfcHomeController.cs`, `IQfcQueue.cs`, `IQfcQueue1.cs` |
| `Helper Classes/` (1) | `IConversationResolver.cs` |
| `Interfaces/` (13) | `IEmailMoveMonitor.cs`, `IFilerFormController.cs`, `IFilerHomeController.cs`, `IItemControler.cs`, `IKbdAction.cs`, `IQfcCollectionController.cs`, `IQfcDatamodel.cs`, `IQfcExplorerController.cs`, `IQfcFormController.cs`, `IQfcFormViewer.cs`, `IQfcItemController.cs`, `IQfcKeyboardHandler.cs`, `IMailItemActions.cs` |
| `Viewers/` (5) | `IItemViewer.cs`, `IBreadcrumbDropDownHost.cs`, `IBreadcrumbWebHost.cs`, `IWebViewCoreInitializer.cs`, `IWebViewMessenger.cs` |

Two of the 23 also declare a bare `enum` alongside the interface — `Interfaces/IQfcDatamodel.cs:13 public enum SortOptionsEnum` and `Viewers/IBreadcrumbDropDownHost.cs:9 public enum BreadcrumbDropDownCloseReason`. An `enum` emits no method bodies, so this does not move them out of the bucket. `Interfaces/MailItemActionsAdapter.cs` is the one file in `Interfaces/` that is **not** in the bucket.

**Non-interface files with zero coverable lines (3):**

| File | Basis |
| --- | --- |
| `Helper Classes/QfEnums.cs` | 16 lines total; `public static class QfEnums` (:3) whose entire body is the nested `public enum InitTypeEnum` (:5-12) plus one commented-out enum (:14). No methods, no fields, no static constructor. |
| `Helper Classes/cInfoMail.cs` | **Verified directly, see Q3.3.** |
| `Properties/AssemblyInfo.cs` | 39 lines, all `using` directives (1-3) and assembly-level attributes (5, 10-17, 22, 25, 37, 38). No type is declared, so no IL is emitted from this file. Note line 5 is `[assembly: InternalsVisibleTo("QuickFiler.Test")]` — deleting or "cleaning up" this file would break every internal-facing test in the epic. |

The epic's own § *A third ledger bucket* names four files (`IConversationResolver.cs`, `IEmailMoveMonitor.cs`, `QfEnums.cs`, `cInfoMail.cs`) from F4 and both of F7's interface files. All six are in the 26 above; the epic's ~24 estimate in § *Scope* ("~24 are interface-only declarations") is close to the 23 interface-only count and does not include `QfEnums.cs`/`cInfoMail.cs`/`AssemblyInfo.cs`.

### Q3.3 `cInfoMail.cs` — claim verified directly

**VERIFIED as stated.** `QuickFiler/Helper Classes/cInfoMail.cs` is 231 content lines (the file's last content line is 231, `//}`). Live content is lines 1-11 only:

```
1  using System;
2  using System.Collections.Generic;
3  using System.Diagnostics;
4  using System.Linq;
5  using System.Windows.Forms;
6  using Microsoft.Office.Interop.Outlook;
7  using ToDoModel;
8  //using Microsoft.VisualBasic;
9  //using Microsoft.VisualBasic.CompilerServices;
10 using UtilitiesCS;
```

That is **eight live `using` directives** (lines 1-7 and 10) plus two commented-out ones (8-9) — the claim's "eight `using` directives" is exact. Lines 13-231 are entirely comment-prefixed, beginning `//namespace QuickFiler` at line 13 and ending `//}` at line 231; the tail (lines 200-231) was read directly and is comment-prefixed throughout. **No type is declared, so the file emits no IL and cannot appear in any Cobertura report.** It is a correct third-bucket member.

Secondary observation for whoever owns it (F4): the commented-out class at line 15-16 is annotated `//    [Obsolete]`. The file is dead in the strongest sense — deletion, rather than bucket classification, is the arguably correct disposition. That is an F4 scope call, not an F16 one; F16 reports, it does not remediate.

### Q3.4 Files wrongly carrying `[ExcludeFromCodeCoverage]`

**None.** Cross-referencing the 26-file third-bucket set against the 21-file declaring set of Q1.2 yields an empty intersection. No third-bucket file carries the attribute today, on this branch.

This is a state F16 must **re-verify at fan-in, not inherit**, because it is exactly the failure mode the third bucket exists to prevent: a child looking at a 0% line-rate on an interface file and "fixing" it with an attribute. The check is one set intersection and costs nothing.

Related prohibition the capstone must also police: "Shape-assertion tests written purely to manufacture coverage for such a file are prohibited." F7's plan already binds itself to this at `plan.2026-08-07T20-41.md:44` (decision D-2, "**Interface shape-assertion and reflection-shape tests are PROHIBITED anywhere in this child's diff.**"). The mechanical detection is a grep over each child's new test files for reflection-shape idioms — `typeof(I…)`, `GetMethods()`, `GetProperties()`, `GetInterfaces()`, `Should().Implement`, `Should().BeAssignableTo<I…>` — cross-referenced against whether the asserted-about type lives in a third-bucket file. It is a screen with false positives (`BeAssignableTo` is legitimate in a factory test), so hits require a read, not an automatic finding.

---

## Q4 — The #457 lambda trap: detection recipe and current exposure

### Q4.1 Issue #457 as it actually reads

**#457 — "Bug: excludefromcodecoverage-does-not-suppress-nested-lambdas" — OPEN.** Fetched from `github.com/drmoisan/TaskMaster/issues/457` (no `gh` available this session). Body, as returned:

> "A method-level `[ExcludeFromCodeCoverage]` attribute fails to suppress instrumentation of lambdas declared within that method. The C# compiler hoists lambdas into separate compiler-generated closure types that don't inherit the attribute, leaving lambda bodies permanently uncovered and depressing line-coverage metrics."

> "The repository's preferred 'thin exempt production forwarder' pattern—where decision logic sits in a testable member and production wiring uses exempt forwarders with lambda event handlers—directly triggers this defect. This creates an invisible ceiling on achievable coverage: `BreadcrumbPopupUiOperations.cs` cannot exceed approximately **91.5%** coverage regardless of testing efforts."

Reproduction step 4 states the observable signature precisely: "Lambda body line numbers appear with `hits="0"` while method lines correctly absent."

### Q4.2 The concrete detection the capstone can run

**Stage 1 — source-side candidate identification (no coverage run needed).**

For each denominator file, for each **member-level** attribute located by Q1.3 pass (b):
1. Determine the member's line span. For an expression-bodied member, from the declaration line to the terminating `;`. For a block-bodied member, from the declaration line to the matching `}` at the member's brace depth.
2. Within that span, search for lambda-producing tokens: `=>` occurrences beyond the member's own expression-body arrow, `delegate` followed by `{` or `(`, and local-function declarations. Also flag `async` lambdas (`async …=>`) and query expressions.
3. Any hit makes the member a **#457 candidate**. Record `file:line` of the attribute, the member name, the span, and the lambda line numbers.

Stage 1 is cheap and is the check F16 should run over the whole compiled set. It is a *candidate* screen: a lambda that is also reachable from a non-exempt caller may be covered anyway.

**Stage 2 — Cobertura-side confirmation (the definitive test).**

Against the merged per-file class union (all `<class>` elements sharing the `filename`, unioned with max hits per line, class-level `<lines>` block only — the epic's harness rules 1, 2, 3 and the #478 correction):

1. Collect the `<class name>` values contributing to that filename. A file exhibiting the trap has at least one class whose name contains a compiler-generated marker — `<>c`, `<>c__DisplayClass`, or `<…>d__` for async state machines. (`<…>d__` is an async state machine rather than a closure and must be distinguished; it appears for `async` members and is not by itself evidence of #457.)
2. Intersect the union's line numbers with each Stage-1 candidate's member span.
3. **The signature is an asymmetry inside one brace span:** the member's *own* statement lines are **absent** from the union (the attribute did suppress them), while lines lying *inside the same span* are **present with `hits="0"`**. Present lines interleaved within an otherwise-absent member range are lifted closures and nothing else. Ordinary uncovered code produces a contiguous absent-or-present block, not an interleaving.
4. Confirm the cap: sum the `hits="0"` lines matched in step 3 across the file. That count is the file's **irreducible residual under the current attribute placement**. `residual / total-lines-in-union` is the coverage the file can never reach. If `1 - residual/total < 0.80`, the file cannot meet AC1 without changing the attribute placement — that is a Blocking finding against the owning child, not a ledger exemption.

**Stage 3 — the disposition, which F16 reports but does not perform.** The epic's fix is "a class-level-exempt adapter **type** — which carries its own trap: **that type must not be `partial`**." F16's output is the finding plus the owning child; the refactor belongs to that child.

### Q4.3 Current exposure on this branch: one file, confirmed

**`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` is already in this state.** It is the file #457 names, and Stage 1 confirms it directly. The type is `internal sealed class BreadcrumbPopupUiOperations` (:29) — not `partial`, so a type-level attribute *would* be admissible — and it carries **seven member-level attributes**. Four of the seven enclose lambdas:

| Attribute | Member | Lambdas inside the span | Verdict |
| --- | --- | --- | --- |
| :105 | `ShowOwnedPopup` (:106-110) | none — single expression-bodied forward `dropDown.Show(anchor, anchor.PointToClient(screenLocation))` | clean |
| :380 | `CreateProductionControl` (:381) | none | clean |
| :383 | `BeginProductionInitialization` (:384-388) | none | clean |
| :390 | `ReadProductionCore` (:391-392) | none | clean |
| **:394** | `BeginProductionNavigation` (:395-410) | **2** — `() => ((WebView2)control).NavigateToString(html)` (:406), `() => new WebView2Messenger(core, dispatcher)` (:409) | **#457 candidate** |
| **:412** | `DisposeProductionSurface` (:413-417) | **2** — `() => (messenger as IDisposable)?.Dispose()` (:415), `() => control?.Dispose()` (:416) | **#457 candidate** |
| **:457** | `BindProductionNavigation` (:458-…) | **≥5** — the multi-parameter lambda at :470, `starting` at :472-473, `completed` at :474-479, `disposed` at :480, and the subscription-disposal lambda beginning :484 | **#457 candidate** |

Fifteen or more lambda-body lines sit inside three method-level-exempted members, and their only call sites are inside those exempted production forwarders — so no policy-compliant test can reach them. This is consistent with #457's stated ≈91.5% ceiling. F13 owns the file.

**No other compiled file is currently exposed**, because the only other member-level attributes are the 19 in the `QfcItemController.*` family, and #453 (F10) records that those 19 are slated for removal or ratification rather than retention. F16 must nonetheless re-run Stage 1 over the *whole* compiled set at fan-in, because the epic warns the pattern has "epic-wide reach because several children plan to adopt the thin-forwarder seam pattern" — the exposure set at fan-in is expected to be larger than the exposure set today.

**One nuance that makes Stage 2 essential.** Not every lambda inside an exempt member is unreachable. `BreadcrumbPopupUiOperations` also exposes a testable `NavigateToDocumentCore(…, NavigationBinder bindNavigation)` overload at :441-455 that accepts an injected binder — that is precisely the seam that makes the *logic* testable while leaving `BindProductionNavigation` as the untested adapter. A test that supplies its own binder covers `NavigateToDocumentCore` but cannot cover `BindProductionNavigation`'s lifted lambdas. Stage 1 alone would flag the file; Stage 2 is what proves the residual is irreducible rather than merely untested.

---

## Q5 — Issue #136 acceptance-criteria closure mapping

### Q5.0 The evidence-path convention (verbatim from `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`)

Line 12:

> "This skill is the single source of truth for evidence paths. All agents, plans, and hooks MUST use the canonical scheme `<FEATURE>/evidence/<kind>/` without exception."

Lines 15-20, the valid sub-paths:

> ```
> - `<FEATURE>/evidence/baseline/`
> - `<FEATURE>/evidence/regression-testing/`
> - `<FEATURE>/evidence/qa-gates/`
> - `<FEATURE>/evidence/issue-updates/`
> - `<FEATURE>/evidence/other/`
> - `<FEATURE>/evidence/remediation-baseline/`
> ```

Lines 22-30, forbidden for evidence output: `artifacts/baselines/`, `artifacts/baseline/`, `artifacts/qa/`, `artifacts/qa-gates/`, `artifacts/evidence/`, `artifacts/coverage/`, `artifacts/regression-testing/`, `artifacts/post-change/`. Only `artifacts/orchestration/` is allowed, and only for non-evidence orchestration use.

Timestamp format (line 46): `yyyy-MM-ddTHH-mm`, e.g. `2026-02-06T14-30`.

Machine-checkable schema every command-bearing artifact must carry (lines 109-111): `Timestamp: <ISO-8601>`, `Command: <exact command>`, `EXIT_CODE: <int>`; baseline artifacts additionally require `Output Summary:` (line 117).

Negative claims (lines 134-138) must record `SearchScope:`, `SearchPatterns:`, `SearchResult:`. **This binds several F16 checks below**, because most of F16's AC2-AC6 verification consists of asserting that something is *absent* across fourteen sibling folders. Every such assertion must carry the three fields or it is not auditable.

For F16, `<FEATURE>` = `docs/features/active/2026-08-08-quickfiler-per-file-coverage-capstone-497`.

### Q5.1 The eight criteria, verbatim

Fetched from `github.com/drmoisan/TaskMaster/issues/136` (title "Feature: quickfiler-80-per-file-coverage", state **Open**):

> - All production `.cs` files in QuickFiler.csproj reach minimum 80% line coverage
> - Coverage research and planning happens per-file with separate research artifacts
> - Each test case executes as an atomic step within per-file phases
> - Tests follow MSTest conventions, use Moq for mocking, and FluentAssertions for assertions where practical
> - All tests remain deterministic, isolated, and independent of external dependencies or temp files
> - Coverage includes positive paths, invalid inputs, boundary conditions, and error handling
> - C# validation passes (formatting, analyzers, nullable safety, coverage execution)
> - Repository-wide coverage expectations are maintained or improved

### Q5.2 Closure mapping

Only **AC1**, **AC7**, and **AC8** are numeric-coverage or toolchain criteria. **AC2, AC3, AC4, AC5, AC6** are process/convention criteria that need entirely different evidence, and they are the ones that must be verified across fourteen siblings without re-reading every test file.

---

#### AC1 — "All production `.cs` files in QuickFiler.csproj reach minimum 80% line coverage"

**Closing evidence:** a per-file reconciliation table with one row per compiled file at evaluation time, each row carrying `file`, `bucket` (`testable` / `ratified-exempt` / `interface-only`), `line %`, `branch %`, `owning child`, `pass|fail|N/A`; plus the assertion that `denominator(csproj) \ ledger = ∅` (the epic's rule 5: "**F16 re-derives and reconciles.** The capstone recomputes the denominator from the csproj and fails if any compiled file lacks a ledger row").

**Path:** `<FEATURE>/evidence/qa-gates/per-file-coverage-reconciliation.<TS>.md` plus the raw `<FEATURE>/evidence/qa-gates/coverage-final.<TS>.cobertura.xml`.

**Non-obvious closure conditions.**
- **Branch is a second, independent gate.** The epic's reconciliation table sets per-file branch at **>= 75%** and warns "the 80% per-file line figure and the 75% branch figure are independent gates." The epic identifies **twelve files that pass on line and fail on branch** — a reconciliation table with only a line column closes AC1 while silently failing the epic.
- **`interface-only` rows are `N/A`, never 0% and never a failure.** A harness keyed on `line-rate` reports all 26 as 0% failures.
- **Absence is not coverage.** Roughly 51 compiled files are absent from the pre-epic report. F16 must confirm, per file, which of the three absence causes (Q3.1) applies.
- **The `[ExcludeFromCodeCoverage]`-on-a-testable-seam count must be zero.** This is a leading indicator in the epic front-matter ("The count of QuickFiler files carrying [ExcludeFromCodeCoverage] on a testable seam falls to zero") and is checked by re-running Q1's census and comparing every surviving attribute against Q2.2/Q2.3.

---

#### AC2 — "Coverage research and planning happens per-file with separate research artifacts"

Two distinct obligations: (i) a per-file **research artifact**, (ii) a per-file **atomic-plan phase**.

**Mechanical check (i) — per-file research artifact existence over sibling folders.**

For each child `C` and each production file `X.cs` assigned to `C`, assert at least one file under `docs/features/active/<C>/research/` whose *normalized stem* equals `normalize(X)`.

`normalize(name)`: lowercase; drop directory; drop trailing `.md`; strip a leading ordinal `^\d{2}-`; strip a leading ISO timestamp `^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-`; strip a trailing `.research` and any trailing `.<timestamp>`; strip `.cs`; strip a leading folder qualifier `^(controllers|interfaces|viewers|helper ?classes)\.`; delete `.` and `-`.

**This normalization is required because the naming convention is not uniform.** Eight distinct schemes are in use on this branch, all verified by `Glob docs/features/active/*/research/*`:

| Child | Scheme | Example |
| --- | --- | --- |
| F2 (431) | `<File>.cs.research.md` | `QfcQueue.cs.research.md` |
| F3 (430), F4 (434), F13 (455) | `NN-<TypeName>.md` | `01-KeyboardHandler.md` |
| F5 (436) | `<ISO-TS>-<lowercased>.md` | `2026-08-08T00-43-qfcdatamodel.md` |
| F6 (435) | `<Folder>.<File>.cs.md` / `<File>.cs.md` | `Interfaces.IQfcFormViewer.cs.md` |
| F7 (433) | `<File>.cs.research.<ISO-TS>.md` | `QfcHomeController.cs.research.2026-08-07T20-50.md` |
| F8 (437) | `<TypeName>.research.md` | `EfcHomeController.Timing.research.md` |
| F11 (454) | `<kebab-name>.md` | `qfc-collection-controller.md` |
| F1 (432) | single non-per-file artifact (F1 owns no production files) | `2026-08-07T22-15-quickfiler-coverage-ledger-research.md` |

**Two collision hazards the normalization must handle.**
- **Substring matching is unsafe.** `ItemViewer` is a proper substring of `ItemViewerExpanded`, `QfcItemViewerExpanded`, and `ItemViewer.Designer`; a `contains` check would satisfy `ItemViewer.cs` with `ItemViewerExpanded.md`. Use **stem equality**, never `contains`.
- **Two compiled files share a stem.** `QuickFiler.csproj:303` compiles `Controllers\IQfcFormController.cs` and `:363` compiles `Interfaces\IQfcFormController.cs`. Both normalize to `iqfcformcontroller`. Where >1 compiled file shares a stem, the check must additionally require the artifact name to carry the folder segment — F6 already does exactly this (`Controllers.IQfcFormController.cs.md` and `Interfaces.IQfcFormController.cs.md` both exist).

**Honest limits of check (i).** It proves an artifact with the right *name* exists. It does not prove the artifact is about that file, is non-empty, or contains analysis. A cheap strengthening that stays mechanical: assert the artifact is >= N bytes and that its body contains the production file's path at least once. Anything beyond that requires reading, and F16 should read a **sample**, disclosing the sample size rather than implying full review.

**A real gap this check already finds on this branch.** F13 (`...-breadcrumb-dropdown-webview-coverage-455`) has **11 per-file research artifacts** (`01-`…`11-`) plus `00-cross-cutting-context.md`, against **15 assigned compiled files**. The four with no artifact are `Viewers/IBreadcrumbDropDownHost.cs`, `Viewers/IBreadcrumbWebHost.cs`, `Viewers/IWebViewCoreInitializer.cs`, `Viewers/IWebViewMessenger.cs` — all four are third-bucket interface-only files. Every other present child has a 1:1 artifact-to-file correspondence, including for their interface files (F2 has `IQfcQueue.cs.research.md` and `IQfcQueue1.cs.research.md`; F6 has four `Interfaces.*` artifacts).

This forces a decision F16 cannot make unilaterally and must escalate to the epic: **does AC2's "per-file" obligation extend to third-bucket files?** F13's practice says no; F2/F3/F6/F7's practice says yes. The consistent reading is that it does — a file needs a research artifact precisely to *establish* that it belongs in the third bucket, which is a research finding. Under that reading F13 has a four-file AC2 shortfall and F16 reports it as a Blocking finding **naming F13**, without writing the four artifacts itself.

**Mechanical check (ii) — per-file atomic-plan phase.**

Grep each child's `plan.<TS>.md` for `^### Phase \d+ — ` headings and assert that, for each assigned production file, some phase heading names it. F7's plan is the reference shape:

```
### Phase 1 — Mandatory Partial Split of QfcHomeController.cs
### Phase 2 — QfcHomeController.cs Seams and Coverage
### Phase 3 — QfcHomeController.Metrics.cs Seams and Coverage
### Phase 4 — QfcHomeController.Iteration.cs Coverage
### Phase 5 — IQfcHomeController.cs Ledger Classification and Evidence
### Phase 6 — IFilerHomeController.cs Ledger Classification and Evidence
### Phase 7 — Final C# Toolchain QA Loop and Coverage Verification
```

Note F7 gives its two third-bucket interface files their **own phases** (5 and 6), consistent with the "yes" reading above. Phase 0 (baseline) and the final QA phase are structural and are correctly excluded from the per-file mapping. **Limit:** the check confirms a phase is *named* for the file, not that the phase's tasks actually address it.

**Path:** `<FEATURE>/evidence/qa-gates/ac2-per-file-artifact-audit.<TS>.md`, carrying the per-child matrix, the `SearchScope:`/`SearchPatterns:`/`SearchResult:` triple for every absent artifact, and the normalization rule used.

---

#### AC3 — "Each test case executes as an atomic step within per-file phases"

**Mechanical check.** Within each child's plan, count task bullets matching `^- \[[ x]\] \[P\d+-T\d+\]` and, among those, count the ones that introduce exactly one named test. F7's convention is unambiguous and greppable:

```
- [ ] [P2-T8] Add **TC1** `PrivateParameterlessConstructor_LeavesEveryLoaderSeamAtItsNonNullDefault` to `QfcHomeControllerLifecycleTests.cs`.
  - Acceptance: the single named test exists and passes.
```

Two signatures make this checkable: the task text begins `Add **TC<n>** ` followed by a single backticked identifier, and the acceptance line contains the literal `the single named test`. Assert: **no task adds more than one backticked test identifier**, and every test-adding task carries a singular acceptance phrase.

**Cross-check against the delivered diff (this is what makes AC3 more than a documentation audit).** Count `[TestMethod]` occurrences added across the epic's diff and compare against the count of test-adding plan tasks across all fourteen plans. The two need not match exactly — a scaffold task legitimately creates a `[TestClass]` with zero `[TestMethod]` members (F7 `[P2-T4]`…`[P2-T7]` do exactly this) — but a large excess of `[TestMethod]`s over planned test tasks means tests were written outside the atomic-step discipline. Baseline for the delta: **101 `[TestClass]` occurrences across 100 files** in `QuickFiler.Test/` today, and 107 `.cs` files total.

**Limit, stated honestly:** the signature depends on every child's planner having adopted F7's phrasing. Only F7's plan was read in full this session; the other nine present children's plans were not. A child using different phrasing produces a false negative, so the check must first *discover* each plan's task convention and only then count. That discovery step is manual, once per child.

**Path:** `<FEATURE>/evidence/qa-gates/ac3-atomic-test-step-audit.<TS>.md`.

---

#### AC4 — "Tests follow MSTest conventions, use Moq for mocking, and FluentAssertions for assertions where practical"

**Mechanical check — a grep signature over `QuickFiler.Test/**/*.cs` at fan-in, expressed as ratios rather than absolutes:**

| Signal | Pattern | Assertion |
| --- | --- | --- |
| MSTest framework | `using Microsoft.VisualStudio.TestTools.UnitTesting;` | present in every file declaring `[TestClass]` |
| Foreign frameworks | `using Xunit`, `using NUnit`, `[Fact]`, `[Theory]`, `[Test]\b` | **count must be 0** — this is the only absolute, and it is the CUT1 prohibition ("Do not introduce xUnit or NUnit into existing test projects") |
| Moq | `using Moq;`, `new Mock<` | present in files that mock |
| FluentAssertions | `using FluentAssertions;`, `.Should()` | present |
| MSTest `Assert` fallback | `Assert\.` | permitted but must be a minority; AC4 says "where practical" |

The *only* absolute is the foreign-framework count of zero. Everything else is a ratio, because "where practical" is a judgement AC4 explicitly delegates. F16 should report the `.Should()`-to-`Assert.`-call ratio for files added by this epic and compare against the pre-epic ratio; a material regression is a finding, a small residue is not.

**Limit:** the grep proves the libraries are referenced, not that they are used idiomatically. A test using `Assert.IsTrue(x.Should() != null)` passes every pattern and is nonsense. F16 reports the numbers and a sampled read.

**Path:** `<FEATURE>/evidence/qa-gates/ac4-test-convention-scan.<TS>.md`.

---

#### AC5 — "All tests remain deterministic, isolated, and independent of external dependencies or temp files"

**This is the AC with the strongest mechanical check**, because `.claude/rules/general-unit-test.md` § *Determinism Infrastructure* names banned APIs explicitly and F7's plan already encodes the full ban list at `plan.2026-08-07T20-41.md:18`.

**Banned-symbol grep over `QuickFiler.Test/**/*.cs`:** `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `Path.GetTempPath`, `Path.GetTempFileName`, `File.WriteAllText`, `File.Create`, `Directory.CreateDirectory`, `Process.Start`, `new HttpClient`, `MessageBox`, `.Show()`, `UiThread.Init`, `SynchronizationContext.SetSynchronizationContext`.

**Baseline measured on this branch (run this session).** The scan returns exactly **four hits**, of which **one is a real violation** and three are prose in comments:

| Hit | Verdict |
| --- | --- |
| `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25` — `private DateTime now = DateTime.Now;` | **Real violation.** This is precisely F4's recorded defect "`MailItemInfoTests.cs:25` uses banned `DateTime.Now`", confirmed still present. |
| `QuickFiler.Test/Controllers/KaCharTests.cs:113` — `// Arrange: the delegate completes synchronously (no Task.Delay / Sleep).` | comment, not a call |
| `QuickFiler.Test/Controllers/KaKeyTests.cs:104` — `// Arrange: synchronously-completing delegate (no Task.Delay / Sleep).` | comment, not a call |
| `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:245` — doc comment referencing `<c>Task.Delay</c>` | comment, not a call |

**This is the single most valuable number in this report for F16**: the pre-fan-in banned-API baseline is **1**. Any post-fan-in count above 1 is a new violation introduced by a child and is attributable by `git blame` to a specific child's diff. Below 1 means F4 fixed it (the epic assigns it to F4's own execution: "The last two are test-policy violations in existing tests … and are in-scope for F4's own execution rather than deferral").

**Necessary refinement:** the raw grep must exclude comment lines, or it produces the three false positives above. Strip `//`-prefixed and `///`-prefixed lines and the content of `/* */` blocks before matching.

**Second AC5 signal — the 500-line file limit, which is also a `.claude/rules/general-code-change.md` requirement.** Measured this session across `QuickFiler.Test/`: **two files exceed 500 lines**, and both already have owners:

| File | Lines | Disposition |
| --- | --- | --- |
| `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` | **578** | F4's recorded defect, in-scope for F4's own execution per the epic. **Not promoted to an issue** (see Q6). |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | **827** | **Already promoted — issue #450** "Refactor: quickfiler-formcontroller-tests-file-size-split", open, describing "827 lines across 42 test methods". |

Four further files sit at exactly 500 (`QfcCollectionControllerTests.cs`, `BreadcrumbDropDownIntegrationTests.cs`) or 498-499 (`QfcItemController.FolderHandlingTests.cs`, `BreadcrumbDropDownHostTests.cs`, `BreadcrumbDropDownReadinessTests.cs`) — at the limit, not over it, but with zero headroom. A child appending a single test method to any of them breaches the limit. F16 should report the headroom distribution, not just the breach count.

**Third AC5 signal — STA scoping.** The epic's STA last-resort clause requires STA-bound tests to "live in dedicated `*.StaTests.cs` files with `[STATestClass]`/`[STATestMethod]` or equivalent runsettings scoping". Measured baseline: **zero `[STATestClass]` occurrences and zero `*.StaTests.cs` files** exist today. Any at fan-in must satisfy the file-name and attribute conditions together.

**Limits, stated honestly:** the grep cannot detect a non-deterministic test that uses no banned symbol — an unseeded `Random`, a dictionary-ordering dependence, a shared static mutated by two `[TestClass]`es. The only mechanical proxy for those is a repeated shuffled-order test run, and the durable signal is the pass/fail delta between two runs, not a grep. F16 should run the suite at least twice and record both results.

**Path:** `<FEATURE>/evidence/qa-gates/ac5-determinism-and-isolation-scan.<TS>.md`, plus the two run artifacts under `<FEATURE>/evidence/qa-gates/`.

---

#### AC6 — "Coverage includes positive paths, invalid inputs, boundary conditions, and error handling"

**This is the weakest AC to verify mechanically, and F16 should say so rather than manufacture a proxy.**

**Best available mechanical check — plan-side, per production file.** For each per-file phase in each child's plan, assert the phase's task set contains at least one task per scenario class. The signature is more tractable than it looks because planners consistently encode the scenario in the test *name*, and the four classes map to recognizable name fragments and assertion shapes:

| Scenario class | Name/assertion fragments |
| --- | --- |
| positive | no negative fragment; assertion is a value/state assertion |
| invalid input | `_Null`, `_Empty`, `_Invalid`, `ThrowsExactly<ArgumentNullException>`, `ThrowsExactly<ArgumentException>` |
| boundary | `_Zero`, `_Negative`, `_Max`, `_AtThreshold`, `_Boundary`, `_Twice` |
| error handling | `_Throws`, `_Rethrows`, `_DoesNotThrow`, `_Cancel`, `OperationCanceledException`, `_Failure` |

F7's plan demonstrates the shape: `[P2-T10]` adds `CreateCancellationToken_CalledTwice_ReplacesSourceWithoutCancellingThePrevious` with the explicit rationale "State-transition scenario required by AC23; distinct post-condition set from TC2."

**A second, sharper signal that costs nothing:** per-file **branch** coverage. Invalid-input, boundary, and error-handling scenarios are exactly what exercise the second arm of a conditional. A file at >= 80% line but < 75% branch is *prima facie* evidence that AC6's negative scenarios are thin, and the epic already identifies twelve such files. Reporting AC6 alongside the branch column is more informative than any name-fragment count, and it is derived from the same measurement AC1 already requires.

**Honest limits.** The name-fragment check counts *labels*, not *behaviors*; a test named `_Null_Throws` that asserts nothing satisfies it. It cannot detect a missing scenario class the planner never considered. And the per-child variation in plan phrasing seen under AC3 applies here too. **F16 should close AC6 as a documented, sampled, partly-manual review — stating the sample size and the branch-coverage corroboration — and must not present a name-fragment count as proof of scenario completeness.**

**Path:** `<FEATURE>/evidence/qa-gates/ac6-scenario-completeness-review.<TS>.md`.

---

#### AC7 — "C# validation passes (formatting, analyzers, nullable safety, coverage execution)"

**Closing evidence:** one artifact per toolchain stage, each with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`, from a **single uninterrupted pass** in the mandated order (format → analyze → type-check → test). If any stage mutates a file, the pass restarts and only the final clean pass counts.

**Path:** `<FEATURE>/evidence/qa-gates/toolchain-{format,analyze,nullable,test}.<TS>.md`.

**Note carried from the epic (verified there, not re-verified here):** `dotnet-tools.json` pins csharpier 1.2.6, whose v1 CLI requires a subcommand, so `dotnet tool run csharpier format .` / `check .` is the working form and the bare `csharpier .` in `CLAUDE.md` §C#1/§CUT3 fails. The epic's instruction is to "apply the working command form and record the deviation in their own evidence rather than editing policy." F16 must record the deviation in its own artifact; it must not amend `CLAUDE.md`. Command forms are the parallel researcher's scope.

---

#### AC8 — "Repository-wide coverage expectations are maintained or improved"

**Closing evidence:** a self-consistent before/after pair captured on F16's own branch with the identical command and identical post-processing.

**Path:** `<FEATURE>/evidence/baseline/repo-coverage-before.<TS>.md` (+ the `.cobertura.xml`) and `<FEATURE>/evidence/qa-gates/repo-coverage-after.<TS>.md` (+ the `.cobertura.xml`), plus a comparison artifact citing both figures and the command.

**The trap, verbatim from the epic:** "**Rule: the repository-wide criterion is satisfied by a self-consistent before/after pair, never by comparison against an imported number.** … Do not carry a repository-wide figure across branches, across tools, or between raw and post-processed artifacts." The withdrawn 70.19% merge-base figure is not a valid reference; the epic shows the raw and post-processed artifacts of feature #424 differ by 15 points for two independent reasons (different package sets, and `lines-valid` rising from 79,957 to 110,849).

**Also load-bearing:** the epic reads AC8 as "retained or improved", not "meets an absolute floor", and says the absolute repository-wide floors in `CLAUDE.md` (80%) and `.claude/rules/general-unit-test.md` (85%) "remain the standing repository aspiration and are untouched by this epic". F16 must not convert AC8 into an absolute-floor gate — and equally must not silently drop the absolute floors from its report. Report both: the before/after delta (the gate) and the absolute figure against each policy floor (informational). Issue **#494** "Bug: conflicting-coverage-thresholds-across-policy-docs" is open against precisely this inconsistency, and F16 should cite it rather than re-adjudicate it.

---

## Q6 — Defect-trail coherence

All GitHub state below was fetched from `github.com/drmoisan/TaskMaster` this session (no `gh` available). Every issue named by the epic **exists and is open**.

### Q6.1 The named issues — all verified present and open

| # | Title | State | Verified role |
| --- | --- | --- | --- |
| 441 | Cobertura post-processing double-counts `<line>` nodes, inflating lines-valid and every coverage rate | Open | Harness axis defect; corrupted the epic's first baseline table |
| 442 | Bug: qfc-home-controller-metrics-never-flushed | Open | F7 promotion |
| 443 | Bug: qfc-home-controller-metrics-duration-misread | Open | F7 promotion |
| 444 | Bug: kbdactions-enumerable-ctor-bypasses-duplicate-guard | Open | F3 promotion |
| 445 | Bug: quickfiler-keyboard-action-contract-defects | Open | F3 promotion |
| 446 | Bug: iteratequeueasync-deadline-closes-queue-early | Open | F7 promotion |
| 447 | Refactor: qfc-home-controller-dead-iterate-paths | Open | F7 promotion; F7 plan D-5 sequences it after F6 |
| 451 | Bug: efc-home-controller-metrics-inert-duration | Open | F8 promotion; body records **six** latent defects |
| 426 | Bug: emailmovemonitor-rejected-item-hook-retention | Open | The promoted-but-not-active issue the initial survey missed |
| 449 | Bug: quickfiler-explorer-controller-latent-defects | Open | F6 promotion; narrowed to two findings by the epic's deletion ruling |
| 457 | Bug: excludefromcodecoverage-does-not-suppress-nested-lambdas | Open | F13 promotion; the Q4 trap |
| 464 | Bug: efc-controllers-null-guard-and-async-void-boundary-defects | Open | F9-territory promotion |
| 478 | Bug: merge-cobertura-classes-blends-union-with-primary-methods | Open | F11 promotion; the second, distinct harness defect |
| 494 | Bug: conflicting-coverage-thresholds-across-policy-docs | Open | Policy-conflict promotion. **Body is an unpopulated template** — every substantive section reads "(not provided in potential file)". It exists but carries no content, so it cannot be closed or acted on as written. F16 should report this as an incomplete promotion. |

The epic's claim that "F3, F7, and F8 did this, producing issues **#442-#447 and #451**" is confirmed: 442, 443, 444, 445, 446, 447, and 451 all exist and are open. **The defect trail for the promoting children is coherent and complete.**

Additional QuickFiler-coverage-adjacent issues discovered this session that the epic does **not** name (all open), showing the trail is broader than the epic records: **#448** `quickfiler-undoconsumer-nonterminating-loop`, **#450** `quickfiler-formcontroller-tests-file-size-split`, **#458** `webview2breadcrumbhost-handler-retention-pooled-viewer`, **#459** `efc-item-controller-keyboard-registration-defects`, **#460**-**#463**, **#465**-**#471**, **#486**-**#493**. F16's reconciliation should treat the epic's list as a lower bound, not an inventory.

### Q6.2 F4's eight unpromoted defects — verbatim from the epic

> "F4 recorded eight defects as plan follow-ups without promoting them. Its execution run must promote them via the MCP promotion surface before it completes:
>
> - Leaked `BeforeItemMove` subscription when a mail's parent folder changes.
> - Handler predicate reading live COM instead of the cached ID.
> - Unsynchronised `Queue<T>` across the dispatcher boundary.
> - `Reset` double-dispose.
> - `DequeueChunk` unbounded regrowth.
> - Missing `[Flags]` on `QfEnums.InitTypeEnum`.
> - `MailItemInfoTests.cs:25` uses banned `DateTime.Now`.
> - `ConversationResolverTests.cs` at 578 lines, breaching the 500-line limit.
>
> The last two are test-policy violations in existing tests, not production defects, and are
> in-scope for F4's own execution rather than deferral."

### Q6.3 Do any of the eight already have an issue today? — **No.**

Two keyword searches were run against the issue index:

- `is:issue beforeitemmove OR dequeuechunk OR "double-dispose" OR conversationresolver OR qfenums OR emailmovemonitor` → returned #461, #453, #437, #435, #434, **#426**, #228 (closed). None is a match for any of the eight except by adjacency.
- `is:issue viewerqueue OR dequeuechunk OR inittypeenum OR "DateTime.Now" OR "500-line"` → returned #454, #436, #434, #432, #431, #371, #369, #297, #296, #295, #233. All are feature/epic issues; none is a match.

`SearchScope:` the GitHub issue index for `drmoisan/TaskMaster`, all states.
`SearchPatterns:` the two queries above.
`SearchResult:` no issue exists for any of the eight.

**Per-defect determination:**

| # | F4 defect | Issue today | Note |
| --- | --- | --- | --- |
| 1 | Leaked `BeforeItemMove` subscription on parent-folder change | **none** | Adjacent to **#426**; the epic itself says defects 1 and 2 "are plausibly the same underlying defect" as #426. F4 must determine whether to promote separately or fold into #426 — **it must not silently drop them on the assumption that #426 covers them.** |
| 2 | Handler predicate reading live COM instead of the cached ID | **none** | same |
| 3 | Unsynchronised `Queue<T>` across the dispatcher boundary | **none** | |
| 4 | `Reset` double-dispose | **none** | |
| 5 | `DequeueChunk` unbounded regrowth | **none** | |
| 6 | Missing `[Flags]` on `QfEnums.InitTypeEnum` | **none** | **Verified still present in source.** `QuickFiler/Helper Classes/QfEnums.cs:5-12` declares `public enum InitTypeEnum` with power-of-two values `1, 2, 4, 8, 16` and explicit bit-pattern comments (`// 00000000 00000001   2^0`), and carries **no `[Flags]`**. The intent is unambiguous from the comments. |
| 7 | `MailItemInfoTests.cs:25` banned `DateTime.Now` | **none** | **Verified still present**, `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25` — `private DateTime now = DateTime.Now;`. In-scope for F4's own execution per the epic, so no issue is expected. |
| 8 | `ConversationResolverTests.cs` at 578 lines | **none** | **Verified still 578 lines.** In-scope for F4's own execution, so no issue is expected. |

**Capstone obligation, precisely stated.** Items 1-6 are production defects the epic *promises* will be promoted; none has an issue today. Items 7-8 are in-scope for F4's execution, so their correct terminal state is *fixed in F4's diff*, not *promoted*. F16's check is therefore asymmetric:

- **For items 1-6:** assert an open issue exists whose body names the defect, and that F4's evidence contains the promotion receipt. Absence at fan-in is a Blocking finding **naming F4**. F16 must not promote them on F4's behalf — promotion is the owning child's obligation and F16's mandate is verification.
- **For items 7-8:** assert the code is fixed. The mechanical checks are already defined under AC5: banned-API count returns to 0 for `MailItemInfoTests.cs:25`, and `ConversationResolverTests.cs` line count drops to <= 500. If instead an *issue* appears for either, that is a deferral of in-scope work and is itself a finding.

**Useful precedent for how this should look when done right:** the 827-line `QfcFormControllerTests.cs` — a structurally identical 500-line breach — **was** promoted, as **#450**. F4's 578-line breach was not. The asymmetry between #450 and item 8 is the clearest evidence that F4's follow-ups are genuinely unpromoted rather than promoted-under-a-name-I-failed-to-search.

---

## Q7 — Sibling issue-number reconciliation

All six mappings were checked against the real issue bodies. **All six are correct.**

| Child | Manifest placeholder | Real issue | Title | State | Body evidence confirming the mapping |
| --- | --- | --- | --- | --- | --- |
| F9 `quickfiler-efc-form-item-controller-coverage` | 1009 | **452** | Feature: quickfiler-efc-form-item-controller-coverage | Open | Title matches the feature slug exactly. Body is an unpopulated template citing `docs/features/potential/2026-08-07-quickfiler-efc-form-item-controller-coverage.md`. **Confirmed by slug only, not by file list** — see caveat below. |
| F10 `quickfiler-item-controller-coverage` | 1010 | **453** | Feature: quickfiler-item-controller-coverage | Open | Body names the `QfcItemController` partial family, "10 production partial classes", "1 interface file", "3,180 lines", four partials below 80% (`ViewerSetup`, `FocusAndTheme`, `MailActions`, `EventHandlers`), and "19 `[ExcludeFromCodeCoverage]` attributes across six partials". Matches the epic's F10 row (11 files / ~3,180 lines / six exempted partials) and matches this report's independent count of **19** member-level attributes in the `QfcItemController.*` family. **Strong confirmation.** |
| F12 `quickfiler-breadcrumb-bridge-coverage` | 1012 | **495** | Feature: quickfiler-breadcrumb-bridge-coverage | Open | Body names all five F12 files — `BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbBridgeCoordinator.cs`, `BreadcrumbMessengerHub.cs`, `BreadcrumbCoordinatorUpgradeLifetime.cs`, `BreadcrumbBridgeRouter.cs` — and cites the 66.4% branch figure the epic records. **Strong confirmation.** |
| F14 `quickfiler-itemviewer-coverage` | 1014 | **456** | Feature: quickfiler-itemviewer-coverage | Open | Body names `ItemViewer.cs` (with its `[ExcludeFromCodeCoverage]`), `ItemViewerExpanded.cs` (39.0%/8.3%), `IItemViewer.cs`, `ItemViewer.WebViewThread.cs`, `ItemViewer.Breadcrumb.cs`, two `*.Designer.cs`, and lists F10/F12/F13 as not-to-be-edited. **Strong confirmation.** |
| F15 `quickfiler-form-viewers-bayesian-coverage` | 1015 | **496** | Feature: quickfiler-form-viewers-bayesian-coverage | Open | Body names all five testable F15 files with the epic's exact baseline figures (`BayesianPerformanceController.cs` 66.0%/57.1%, `BayesianPerformanceViewer.cs` 54.3%/12.5%, `ToolStripMenuItemCb.cs` 61.5%/50.0%) plus "seven generated `*.Designer.cs` and `Properties/` files". **Strong confirmation.** |
| F16 `quickfiler-per-file-coverage-capstone` | 1016 | **497** | Feature: quickfiler-per-file-coverage-capstone | Open | Title matches. Body is an unpopulated template (`Criterion 1` / `Criterion 2`) citing `docs/features/potential/2026-08-08-quickfiler-per-file-coverage-capstone.md`. **Confirmed by slug only.** |

**Caveat on #452 and #497.** Both have unpopulated template bodies, so the confirmation rests on the issue title matching the feature slug plus the `docs/features/potential/…` source path in the body. That is strong but weaker than the file-list confirmation available for 453, 495, 456, and 496. Neither is ambiguous — no competing issue carries either slug — but the mapping for these two should be recorded as slug-confirmed rather than content-confirmed.

### Q7.1 The `depends_on` list for F16 WILL be stale at execution time — confirmed

`epic.md` lines 67-83 currently read:

```yaml
  - issue_num: 1016
    feature_folder: quickfiler-per-file-coverage-capstone
    depends_on:
      - 431
      - 430
      - 434
      - 436
      - 435
      - 433
      - 437
      - 1009
      - 1010
      - 454
      - 1012
      - 455
      - 1014
      - 1015
```

**Five of the fourteen `depends_on` entries are placeholders that resolve to real issues, and F16's own `issue_num` is a sixth placeholder:**

| Manifest value | Correct value |
| --- | --- |
| `1009` | **452** |
| `1010` | **453** |
| `1012` | **495** |
| `1014` | **456** |
| `1015` | **496** |
| `issue_num: 1016` | **497** |

Nine `depends_on` entries are already correct (431, 430, 434, 436, 435, 433, 437, 454, 455), and the five `feature_folder` values for the unresolved children are correct slugs — only the numbers are wrong.

**The manifest also contradicts itself on this branch.** The back-fill note at `epic.md:92-98` says:

> "Ten are now resolved — **432** (F1), **430**, **431**, **433**, **434**, **435**, **436**, **437**, **454**, **455**. Six remain placeholders in the `1009`-`1016` range and belong to children still in preparation. … The manifest is committed in final resolved form, with no placeholder remaining, before the kickoff artifact is written."

The count is accurate (ten resolved, six placeholders) but the closing sentence states a precondition that is **not yet satisfied**: six placeholders remain in the committed manifest on this branch. All six are now resolvable from the verified table above.

**Two consequences for F16 specifically.**

1. **Any epic-orchestrator dependency-gate keyed on `depends_on` will fail to find issues 1009, 1010, 1012, 1014, 1015** and will either error or, worse, treat the dependency as unsatisfiable and skip it. This is a real execution-time blocker, not a documentation nit.
2. **Fixing it is an edit to `epic.md`, which is not a per-child owned file.** The correct sequencing is that the manifest is repaired *before* F16 executes, by whoever owns the epic manifest — F16 verifies the repair rather than performing it. If F16 finds the placeholders still present at execution time, that is an epic-sequencing finding, consistent with the epic's own framing ("If F1's ledger or harness is genuinely absent when execution begins, that is an epic-orchestrator sequencing failure to be raised at that moment").

---

## Section: what could NOT be verified

Recorded explicitly so nothing here is mistaken for a checked fact.

1. **No shell/`gh` access this session.** Every GitHub fact came from `WebFetch` against rendered issue pages, summarized by a secondary model. Titles, numbers, and states are reliable (they were returned consistently across independent fetches); **body quotations from issues are paraphrases unless shown in a fenced block**. The `gh issue view 457` and `gh issue view 136` invocations in the brief were substituted with web fetches. Anyone re-running this should confirm the #457 and #136 bodies with `gh`.

2. **The GitHub issue-index pages were truncated by the fetch summarizer.** Each request returned roughly 12 rows where GitHub renders 25. Issue numbers **472-485**, **447-459** (partially closed by individual fetches of 447-452, 454-459), and **395-434** (partially closed) were not fully enumerated. **The Q6 conclusion that F4's eight defects have no issue rests on two keyword searches, not on a complete index walk.** The searches are targeted and returned coherent result sets, but a defect promoted under wording that misses both queries would be missed. F16 should re-run this with `gh issue list --limit 500 --state all --json number,title,state`.

3. **Only F7's plan was read in full.** The AC2 phase-heading check and the AC3 task-signature check were validated against `docs/features/active/2026-08-07-quickfiler-qfc-home-controller-coverage-433/plan.2026-08-07T20-41.md` only. The other nine present children's plans were not opened, and five children's folders are not on this branch at all. Statements about "each child's plan convention" are extrapolations from one instance and are flagged as such in Q5.

4. **No coverage run was performed.** Every Cobertura-side rule in Q3 and Q4 (the `<line>`-child-count test, the `<>c` class-name intersection, the present/absent interleaving signature) is derived from the epic's recorded harness rules, from #441/#457/#478, and from source structure — **not** from a report generated on this branch. The three Q4 candidate members in `BreadcrumbPopupUiOperations.cs` are confirmed at Stage 1 (source) only; **Stage 2 confirmation against an actual Cobertura report was not performed.** The ≈91.5% ceiling is #457's number, not one this report reproduced.

5. **The 24-file suppression set was computed, not observed.** It follows from the type-declaration map plus the partial-file map, both derived by grep. It was not cross-checked against a Cobertura report showing those 24 files absent. The one place this could go wrong is a partial declared in a file whose declaration line my grep pattern missed; the pattern required a modifier keyword before `class`, and a bare `partial class X` at the start of a line **is** matched (all seven `*.Designer.cs` partials were found this way), but an unmodified `class X` with no modifier at all would not be.

6. **`coverage.config` / `.runsettings` were not re-inspected this session.** F1's research recorded that no entry excludes QuickFiler. That is a memory-sourced claim carried forward, not re-verified here, and Q2.3 item 8 asks F16 to re-verify it at fan-in.

7. **`QuickFiler.Test.csproj`'s 107 `<Compile Include>` entries were not counted directly.** The epic states 107; `Glob` found 107 `.cs` files under `QuickFiler.Test/`, which is consistent but is not the same measurement.

8. **Line counts were derived by `Grep '^' --count`**, which counts matching lines. For a file lacking a trailing newline this equals the line count; for files with unusual line endings it could differ by one. The three figures that matter (578, 827, 231) are each consistent with the independently-recorded figures in the epic and in issue #450, so the risk is low but not zero.

9. **AC6's scenario-completeness check has no sound mechanical form**, and this report deliberately does not propose one that would over-claim. The name-fragment screen and the branch-coverage corroboration are the honest maximum; closing AC6 requires a sampled manual review whose sample size must be disclosed.
