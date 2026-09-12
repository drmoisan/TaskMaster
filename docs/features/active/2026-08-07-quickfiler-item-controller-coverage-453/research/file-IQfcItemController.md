# Per-File Research — `QuickFiler/Interfaces/IQfcItemController.cs`

- Feature: `quickfiler-item-controller-coverage` (issue #453), epic child F10 of epic #136
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Interfaces/IQfcItemController.cs` (107 lines)
- Coverage report examined:
  `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`

---

## Recommendation (headline)

**Ledger classification: `interface-only / not-measured`.**

Reported **N/A**, never 0%, never a failure. **No `[ExcludeFromCodeCoverage]` attribute is to be
added.** **No tests are to be written for this file**, and the atomic plan must not contain a task
that proposes any.

---

## 1. Coverage status

The file is **absent** from the Cobertura report. A grep for
`filename="QuickFiler\Interfaces\IQfcItemController.cs"` across
`coverage-final.cobertura.xml` returns zero matches. There is no `<class>` element, no `<lines>`
element, and therefore no denominator.

Per epic.md §"Directives for F1's Ledger and Harness", the harness must decide the denominator on
`<line>` child count, never on `line-rate`. This file contributes zero `<line>` children. Keying on
`line-rate` — which does not exist for this file — would mis-report it as a 0% failure, which is
exactly the false alarm the third bucket exists to prevent.

---

## 2. Evidence that "absent" means "zero coverable lines", not "not instrumented"

Three independent lines of evidence.

### 2.1 Positive control — the folder IS instrumented

`QuickFiler/Interfaces/MailItemActionsAdapter.cs` is a sibling file in the **same directory**. It
appears in the report at line 14448:

```xml
<class line-rate="1" branch-rate="1" complexity="1"
       name="QuickFiler.Interfaces.MailItemActionsAdapter"
       filename="QuickFiler\Interfaces\MailItemActionsAdapter.cs">
```

Instrumentation therefore reached `QuickFiler\Interfaces\`. The absence of `IQfcItemController.cs` is
a property of the file, not of the coverage configuration. This is the same technique sibling child F7
used for its two interface files.

### 2.2 The file is in the compile set

`QuickFiler/QuickFiler.csproj:365` contains
`<Compile Include="Interfaces\IQfcItemController.cs" />`. The file is compiled, so it is inside the
epic's dynamic denominator (epic.md §"Mid-Wave File Creation" rule 1) and must carry a ledger row. It
is not excluded by any `coverage.config` assembly rule — if it were, the sibling
`MailItemActionsAdapter.cs` in the same folder would be excluded too, and it is not.

### 2.3 Source inspection — every member is a bodiless declaration

Full read of all 107 lines. The file contains:

- `using` directives (lines 1-12) — no IL.
- `namespace QuickFiler.Interfaces` (line 14) and `public interface IQfcItemController` (line 16) — no
  IL.
- Lines 18-105: **57 interface member declarations**, all bodiless — 24 methods, 22 properties, and 11
  overloads across them. Examples: `void AssignFolderComboBox();` (line 18),
  `Task InitializeAsync();` (line 23), `MailItem Mail { get; set; }` (line 42),
  `long TopFolderScore { get; }` (line 105).
- XML doc comments (lines 20-22, 100-104) and `//` comments (lines 31, 48, 67, 80-84) — no IL.

Explicitly checked for, and **not present**:

| Construct that would emit IL | Present? |
| --- | --- |
| Default interface implementation (a member with a body or an expression body) | **No** — every declaration terminates in `;` |
| `static` member (including `static abstract`) | **No** |
| Constant with an initializer (`const`) | **No** |
| Field | **No** — interfaces cannot declare instance fields, and no static field is declared |
| Nested type with a body | **No** |
| Attribute usage (an attribute constructor call is data, not executable IL, but is checked anyway) | **No** — the file declares no attributes at all |
| Static constructor | **No** |
| Event with accessor bodies | **No** — no event is declared |

Two members carry an explicit `public` accessibility modifier —
`public CancellationToken Token { get; set; }` (line 95) and
`public Dictionary<string, System.Action> RightKeyActions { get; }` (line 97). This is C# 8+ syntax for
explicit interface-member accessibility and does **not** create a body or emit IL. The remaining 55
members omit the modifier. The inconsistency is cosmetic; see §5.

### 2.4 Corroborating platform constraint

`QuickFiler/QuickFiler.csproj:13` sets `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` and
`:14` sets `<LangVersion>preview</LangVersion>`. Default interface implementations require runtime
support that .NET Framework 4.8.1 does not provide, so a DIM could not compile in this project even if
one were added. This corroborates §2.3 but is not the primary evidence — the source inspection is.

**Conclusion: the file has zero coverable lines.** Absence from the report is correct and expected.

---

## 3. Ledger classification and rationale

| Bucket | Applies? | Rationale |
| --- | --- | --- |
| `testable` (>= 80% line) | **No** | There is no denominator. A file with zero coverable lines cannot be measured against a percentage floor. |
| `ratified-exempt` | **No** | This bucket implies untestable *production logic* argued away against the irreducible-remainder standard. There is no logic here at all. Classifying it `ratified-exempt` would misrepresent the epic's exemption inventory and inflate the count of files whose exemption a reviewer must audit. |
| **`interface-only / not-measured`** | **Yes** | Zero executable IL. Reported N/A. Never counts as a failure. Receives no `[ExcludeFromCodeCoverage]`. |

This matches the four files F4 identified (`IConversationResolver.cs`, `IEmailMoveMonitor.cs`,
`QfEnums.cs`, `cInfoMail.cs`) and both files F7 evidenced.

### Prohibited actions, stated explicitly for the plan

1. **Do not add `[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`** to this file or to the
   interface declaration. epic.md §"A third ledger bucket" is explicit: none of the third-bucket files
   receives the attribute. Adding it would also be a behavior-neutral but reviewable diff that
   contradicts the epic's own directive.
2. **Do not write shape-assertion tests** — reflection tests asserting that `IQfcItemController`
   declares N members, that a member has a given signature, or that `QfcItemController` implements the
   interface. Such tests manufacture no coverage for this file (they would attribute to the test
   assembly and to `QfcItemController.*.cs`), and epic.md prohibits them outright.
3. **Do not delete or trim the file** to raise a percentage. It is a live production contract.

### Ledger row (proposed text for F1's ledger)

| File | Lines | Bucket | Line % | Branch % | Rationale |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Interfaces/IQfcItemController.cs` | 107 | `interface-only / not-measured` | N/A | N/A | 57 bodiless interface member declarations; no DIM, no static member, no const, no nested type, no attribute. Absent from Cobertura; folder instrumentation proven by the sibling `QuickFiler/Interfaces/MailItemActionsAdapter.cs` (report line 14448, `line-rate="1"`). |

---

## 4. Member inventory (for the record)

Declared surface, all bodiless, grouped by concern. Line references are to
`QuickFiler/Interfaces/IQfcItemController.cs`.

| Concern | Members | Lines |
| --- | --- | --- |
| Lifecycle / initialization | `InitializeAsync()`, `InitializeSequentialAsync()`, `Initialize(bool async)`, `InitializeGraphicsAsync()`, `Cleanup()` | 23, 24, 25, 98, 77 |
| Conversation | `LoadConversationResolverAsync(CancellationTokenSource, CancellationToken, bool)`, `PopulateConversation()`, `PopulateConversationAsync(CancellationTokenSource, CancellationToken, bool)`, `PopulateConversation(int countOnly)`, `PopulateConversation(ConversationResolver resolver)`, `RenderConversationCount(int)`, `RenderConversationCount()`, `ToggleConversationCheckbox()`, `ToggleConversationCheckbox(Enums.ToggleState)`, `ConvOriginID` | 26-30, 60, 61-65, 68, 69, 73, 74, 57, 58, 39 |
| Folder handling | `AssignFolderComboBox()`, `PopulateFolderComboBox(object varList = null)`, `PopulateFolderComboBoxAsync(CancellationToken, object = null)`, `LoadFolderHandlerAsync(CancellationToken, object = null)`, `SelectedFolder`, `TopFolderScore` | 18, 48, 70, 71, 44, 105 |
| Focus / theme / tips | `ToggleFocus()`, `ToggleFocus(Enums.ToggleState)`, `ToggleFocusAsync()`, `ToggleFocusAsync(Enums.ToggleState off)`, `SetThemeDark(bool)`, `SetThemeLight(bool)`, `ToggleTips(bool, Enums.ToggleState)`, `ListTipsDetails`, `ListTipsExpanded` | 31, 32, 33, 94, 75, 76, 92, 87, 88 |
| Navigation / expansion | `ToggleNavigation(bool)`, `ToggleNavigation(bool, Enums.ToggleState)`, `ToggleNavigationAsync(Enums.ToggleState)`, `ToggleExpansion()`, `ToggleExpansionAsync()`, `IsExpanded`, `JumpToSearchTextbox()`, `JumpToFolderDropDown()`, `RightKeyActions` | 89, 90, 91, 43, 93, 36, 53, 54, 97 |
| Mail actions | `ApplyReadEmailFormat(object state)`, `FlagAsTask()`, `MarkItemForDeletion()`, `ToggleSaveCopyOfMail()`, `ToggleSaveAttachments()`, `MoveMailAsync()`, `Mail`, `ItemHelper` | 50, 51, 52, 55, 56, 78, 42, 41 |
| Layout / identity | `PopulateControls(MailItemHelper, int)`, `Height`, `ItemNumber`, `ItemIndex`, `ItemNumberDigits`, `TableLayoutPanels`, `Buttons`, `Parent`, `IsChild`, `IsActiveUI`, `SuppressEvents`, `CounterEnter`, `CounterComboRight`, `Token` | 72, 40, 45, 46, 47, 85, 86, 59, 37, 38, 49, 34, 35, 95 |

Commented-out declarations at lines 67 (`//void PopulateConversation(DataFrame df);`) and 80-84
(`//string Subject;` etc.) are dead comments with no IL.

**Consistency note relevant to F10's two production files:**
`PopulateConversationAsync(ConversationResolver, CancellationToken, bool)` — implemented at
`QuickFiler/Controllers/QfcItemController.Conversation.cs:125-139` and entirely uncovered — is **not**
declared on this interface. That is one of the two facts establishing it as dead production code (see
`file-QfcItemController.Conversation.md` §4 Gap A and LD-1). Likewise
`RenderConversationCountAsync` and `SetTopicThread` are public on the concrete class but absent from
the interface, and `LoadFolderHandler` / `PopulateAndSelectFolder` are internal and correctly absent.

---

## 5. Observations (not defects, not in scope)

| ID | Location | Observation | Severity |
| --- | --- | --- | --- |
| **O-1** | `IQfcItemController.cs:95, 97` | Two members carry an explicit `public` modifier while the other 55 omit it. Cosmetic inconsistency; no behavioral or coverage effect. Fixing it is a whitespace-class diff on a shared interface file during a 14-way parallel wave and is **not** recommended. | Informational |
| **O-2** | `IQfcItemController.cs:9` | `using Microsoft.Data.Analysis;` appears to exist solely for the commented-out `PopulateConversation(DataFrame df)` declaration at line 67. Likely an unused using. Removing it is a real (if trivial) diff on a shared file; not recommended during the wave. | Informational |
| **O-3** | `IQfcItemController.cs:11` | `using QuickFiler.Helper_Classes;` is required by line 69 (`PopulateConversation(ConversationResolver resolver)`), which means this **interface** binds to the concrete F4-owned `ConversationResolver` type rather than to `IConversationResolver`. This is the interface-level expression of the cross-child contract recorded in `file-QfcItemController.Conversation.md` §5.5: **if F4 were to rename or retype `ConversationResolver`, this interface would not compile.** F4 may only append parameters with defaults; it must not rename or retype the class. | Contract note |

None of these is a latent production defect requiring MCP promotion. O-3 is a cross-child contract
note, recorded here and mirrored in the Conversation artifact so both agree.

**Ratification-context check (2026-08-07, cycle 2).** This file declares no member bodies, so none of
the 19 issue-#227-ratified `[ExcludeFromCodeCoverage]` sites (see
`file-QfcItemController.Conversation.md` §7) or the one unratified site
(`EnsureBreadcrumbPipeline`, `ViewerSetup.cs:132`, not in this file set) can apply here. No exemption
reconciliation is needed for this artifact.

---

## 6. File-size and creation impact

107 lines, far below the 500-line limit. **No change is proposed to this file**, so:

- no `QuickFiler/QuickFiler.csproj` edit (the `<Compile Include>` at line 365 already exists),
- no `QuickFiler.Test/QuickFiler.Test.csproj` edit,
- no new test file,
- no new ledger row beyond the classification row in §3.

The only F10 deliverable touching this file is the ledger row.

---

## 7. Sibling boundaries

| Dependency | Owner | F10 action |
| --- | --- | --- |
| `QuickFiler.Helper_Classes.ConversationResolver` (via line 69) | **F4 (#434)** | Read-only. Contract note O-3. |
| `QuickFiler.Interfaces.IQfcCollectionController` (line 59) | **F11** | Read-only. |
| `QuickFiler.Interfaces.IQfcTipsDetails` (lines 87, 88) | F10 / F14 boundary | Read-only. |
| `UtilitiesCS.MailItemHelper` (lines 41, 72), `UtilitiesCS.Enums.ToggleState` (lines 32, 58, 90, 91, 94) | UtilitiesCS, outside the epic | Read-only. |
| `Microsoft.Office.Interop.Outlook.MailItem` (line 42) | Interop | Read-only. Note: the interface itself carries an Outlook Interop type, which is why every implementer inherits a COM dependency — but the interface file emits no IL and is not itself exempt-eligible. |

No boundary crossing is proposed for this file.
