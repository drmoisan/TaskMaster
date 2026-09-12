# Research: `QuickFiler/Interfaces/IKbdAction.cs`

Timestamp: 2026-08-07T22-05
Feature: `quickfiler-keyboard-actions-coverage` (issue #430, epic child F3 of #136)
Branch: `feature/quickfiler-keyboard-actions-coverage`
Scope: read-only research. No production or test file was modified.

---

## 1. File Under Research

| Property | Value |
| --- | --- |
| Path | `QuickFiler/Interfaces/IKbdAction.cs` |
| Line count | 18 (file ends at line 19 with the trailing newline) |
| Types declared | One: `public interface IKbdAction<T, U>` (lines 9-17) |
| Compiled by | `QuickFiler/QuickFiler.csproj` line 359 |
| Target framework | `v4.8.1`, `LangVersion=preview` |
| `[ExcludeFromCodeCoverage]` present | **No.** No `System.Diagnostics.CodeAnalysis` using directive and no attribute. |
| Existing tests | **None directly, and none is appropriate.** The interface is exercised transitively through its five implementers and through `KbdActions<TKey, UClass, VDelegate>`, whose generic constraint is `where UClass : IKbdAction<TKey, VDelegate>, new()` (`KbdActions.cs` line 15). |
| Exemption-status authority | **F1's `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`.** This artifact supplies the evidence for an `interface-only / no executable behavior` classification; F1's ledger makes the final call. |

### 1.1 How coverage will be measured

Per-file coverage for this file will be reported by **F1's per-file coverage report harness**, derived from the Cobertura output of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. The expected result is either **0% of 0 executable lines** or **complete absence of a class entry for this filename**, depending on how the instrumenter and the harness treat a type with no method bodies. Section 4 explains why either outcome is correct and how the ledger should record it.

---

## 2. Structural Inventory

Complete file contents by line:

| Lines | Content | Executable? |
| --- | --- | --- |
| 1-5 | `using System;`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `System.Threading.Tasks` | No — using directives emit no IL |
| 6 | blank | No |
| 7 | `namespace QuickFiler.Interfaces` | No |
| 8 | `{` | No |
| 9 | `public interface IKbdAction<T, U>` | No — type declaration |
| 10 | `{` | No |
| 11 | `string SourceId { get; set; }` | **No — abstract property declaration, no body** |
| 12 | `T Key { get; set; }` | **No — abstract property declaration, no body** |
| 13 | `U Delegate { get; set; }` | **No — abstract property declaration, no body** |
| 14 | `bool KeyEquals(T other);` | **No — abstract method declaration, no body** |
| 15 | `//Action<string> Update { get; set; }` | No — comment |
| 16 | `//Type DelegateType { get; }` | No — comment |
| 17 | `}` | No |
| 18 | `}` | No |

**Member surface:** three abstract properties (`SourceId`, `Key`, `Delegate`) and one abstract method (`KeyEquals`). Two further members are present only as comments (lines 15-16).

### 2.1 Executable-behavior determination (the question the delegation brief asked)

Every category of construct that could give an interface file executable IL was checked explicitly:

| Construct that would produce executable IL | Present? | Evidence |
| --- | --- | --- |
| **Default interface member** (body on an interface member) | **No** | Lines 11-14 are all declaration-only; every one terminates in `;` or `{ get; set; }`. Additionally, default interface members require .NET Core 3.0+ / .NET Standard 2.1 runtime support; this project targets `v4.8.1` (`QuickFiler.csproj` line 13), where they cannot execute. `LangVersion=preview` (line 14) would permit the *syntax* but the runtime would not support it — and none is written. |
| **Static member** (static method, static property, static field) | **No** | No `static` keyword appears anywhere in the file. |
| **Static constructor** | **No** | None declared, and none is implied — there is no static field to initialize. |
| **Constant or field with an initializer** | **No** | Interfaces cannot declare instance fields, and no `const` is declared. |
| **Attribute with a constructor invocation** | **No** | No attribute is applied to the interface or to any member. |
| **Nested type with a body** | **No** | No nested type is declared. |
| **Auto-property with an initializer** | **No** | Lines 11-13 are abstract accessor declarations, not auto-properties with backing storage. |
| **Event with accessor bodies** | **No** | No event is declared. |
| **Operator or conversion** | **No** | None declared. |

**Finding, stated plainly: `IKbdAction.cs` contains zero executable statements and produces zero executable IL.** The compiler emits an interface type definition with four abstract member slots and no method bodies. There is nothing for a coverage instrumenter to instrument.

### 2.2 The commented-out members (lines 15-16) explain an anomaly in two sibling files

```csharp
//Action<string> Update { get; set; }
//Type DelegateType { get; }
```

These two members were withdrawn from the contract but left behind on the implementers. That accounts for findings recorded in the sibling artifacts:

- `05-KaChar.md` gaps G1-G3: `KaChar.DelegateType` (`KaChar.cs` lines 43-46), `KaChar.Update` (lines 50-55), and `KaCharAsync.Update` (lines 92-97) are all orphaned public members with no consumer.
- `06-KaKey.md` gaps G1-G3: the same pattern on `KaKey.DelegateType` (`KaKey.cs` lines 43-46), `KaKey.Update` (lines 50-55), and `KaKeyAsync.Update` (lines 92-97).
- `07-KaStringAsync.md` section 2.2: `KaStringAsync.Update` (lines 81-86) is the **only** surviving `Update` with a live consumer — its own `KeyEquals` at lines 62 and 73.

Verified by search: `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'` returns, for `DelegateType`, only the two implementer declarations and this commented-out line; `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'` returns only `KaStringAsync.cs:25`.

`ToggleControl` (`KaStringAsync.cs` lines 88-93) was never on the interface at all and exists on no other implementer — an intentional per-type extension rather than a withdrawal residue.

### 2.3 Implementers and consumers

Verified by `rg 'IKbdAction' --glob '**/*.cs'` (seven matches, all listed):

| Site | Line | Role |
| --- | --- | --- |
| `QuickFiler/Interfaces/IKbdAction.cs` | 9 | declaration |
| `QuickFiler/Controllers/KaChar.cs` | 11 | `KaChar : IKbdAction<char, Action<char>>` |
| `QuickFiler/Controllers/KaChar.cs` | 58 | `KaCharAsync : IKbdAction<char, Func<char, Task>>` |
| `QuickFiler/Controllers/KaKey.cs` | 11 | `KaKey : IKbdAction<Keys, Action<Keys>>` |
| `QuickFiler/Controllers/KaKey.cs` | 58 | `KaKeyAsync : IKbdAction<Keys, Func<Keys, Task>>` |
| `QuickFiler/Controllers/KaStringAsync.cs` | 10 | `KaStringAsync : IKbdAction<string, Func<string, Task>>` |
| `QuickFiler/Controllers/KbdActions.cs` | 15 | generic constraint `where UClass : IKbdAction<TKey, VDelegate>, new()` |

**Five implementers, one constraint consumer.** No variable, parameter, field, or return type anywhere in the repository is declared as `IKbdAction<,>` — the interface is used exclusively as a generic constraint. `KbdActions<TKey, UClass, VDelegate>` reaches every one of the four members through that constraint: `KeyEquals` at lines 49, 51, 55, 73, 80; `SourceId` at lines 92, 100, 110, 115, 125; `Key` at lines 92, 101, 110, 115, 125, 143; `Delegate` at lines 38, 44, 102.

---

## 3. Existing Test Coverage (static analysis)

**Direct tests: none. No file in `QuickFiler.Test/` references `IKbdAction`** (confirmed by the seven-match search in section 2.3, none of which is under `QuickFiler.Test/`).

The contract is nonetheless exercised transitively. The table maps each interface member to the concrete test methods that reach an implementation of it, since that is the only meaningful sense in which an abstract member can be "covered".

| Interface member | Line | Reached through (implementer + test method) |
| --- | --- | --- |
| `string SourceId { get; set; }` | 11 | `KaChar` — `KaCharTests.KaChar_Constructor_StoresSourceIdKeyAndDelegate`, `KaChar_ParameterlessConstructor_LeavesNullDelegate`; `KaCharAsync` — `KaCharTests.KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate`; `KaKey` — `KaKeyTests.KaKey_Constructor_StoresSourceIdKeyAndDelegate`; `KaKeyAsync` — `KaKeyTests.KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate`; `KaStringAsync` — `KaStringAsyncTests.Constructor_LowercasesKeyAndStoresMembers`; via constraint — `KbdActionsRemainingBranchesTests` (all 10 methods), `KbdActionsTests` (all 3) |
| `T Key { get; set; }` | 12 | `KaChar` — `KaCharTests.KaChar_Constructor_StoresSourceIdKeyAndDelegate`, `KaChar_DefaultCharKey_IsSupported`; `KaCharAsync` — `KaCharTests.KaCharAsync_Constructor_StoresSourceIdKeyAndDelegate`; `KaKey` — `KaKeyTests.KaKey_ParameterlessConstructor_LeavesNullDelegateAndNoneKey`; `KaKeyAsync` — `KaKeyTests.KaKeyAsync_Constructor_StoresSourceIdKeyAndDelegate`; `KaStringAsync` — `KaStringAsyncTests.KeySetter_LowercasesValue`; via constraint — `KbdActionsRemainingBranchesTests.Enumeration_YieldsAllRegisteredInstancesAndKeysProjection` (`Keys` projection at `KbdActions.cs` line 143), `KbdActionsTests.Add_WhenSourceAndStoredKeysAreDistinct_DoesNotTreatSubstringAsDuplicate` |
| `U Delegate { get; set; }` | 13 | `KaChar` — `KaCharTests.KaChar_Delegate_DispatchesToSuppliedAction`, `KaChar_Constructor_NullDelegate_IsStoredNotRejected`; `KaCharAsync` — `KaCharTests.KaCharAsync_Delegate_AwaitsAndCompletesSynchronously`; `KaKey` — `KaKeyTests.KaKey_Delegate_DispatchesToSuppliedAction`; `KaKeyAsync` — `KaKeyTests.KaKeyAsync_Delegate_AwaitsAndCompletesSynchronously`; `KaStringAsync` — `KaStringAsyncTests.Delegate_AwaitsAndCompletesSynchronously`; via constraint — `KbdActionsRemainingBranchesTests.Indexer_Get_ReturnsRegisteredDelegate_Set_ReplacesIt` (`KbdActions.cs` lines 38, 44) |
| `bool KeyEquals(T other)` | 14 | `KaChar` — `KaCharTests.KaChar_KeyEquals_MatchesSameCharAndRejectsOther`; `KaCharAsync` — `KaCharTests.KaCharAsync_KeyEquals_MatchesSameCharAndRejectsOther`; `KaKey` — `KaKeyTests.KaKey_KeyEquals_MatchesSameKeyAndRejectsOther`; `KaKeyAsync` — `KaKeyTests.KaKeyAsync_KeyEquals_MatchesSameKeyAndRejectsOther`; `KaStringAsync` — all four `KaStringAsyncTests.KeyEquals_*` methods; via constraint — `KbdActionsTests.FilterKeys_WhenDistinctStoredKeysCoexist_PreservesKeyboardMatchingSemantics`, `KbdActionsRemainingBranchesTests.FilterKeys_ReturnsOnlyMatchingInstances` |

**All four members have at least one implementation reached by a named test on every one of the five implementers.** No member of the contract is unexercised.

---

## 4. Coverage Gaps

**There are no coverage gaps in this file, because there are no executable lines to cover.**

This is a classification outcome, not a deficiency. The governing text is `.claude/rules/general-unit-test.md` § Coverage Requirements:

> Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement. Examples: Python `Protocol`-only modules consumed only under `TYPE_CHECKING`, TypeScript interface/type-only files, and **C# interface-only files. Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold.**

Section 2.1 establishes by exhaustive construct check that `IKbdAction.cs` is exactly such a file.

### 4.1 The distinction that must not be blurred: measurement omission vs. an `exclude` entry

The same rules file carries a § Coverage Exclusion Policy that prohibits excluding production paths:

> No production file may be excluded from coverage measurement. ... **Prohibited `exclude` entries:** Any path under `src/` that contains production runtime code, regardless of whether it is auto-generated, host-bound, or difficult to test. **Enforcement:** Feature-review agents must treat any `exclude` entry that matches a production source path as a **Blocking** finding.

These two clauses are consistent, and the reconciliation matters for how F1 records this file:

- The § Coverage Requirements clarification says a file with **zero executable behavior** legitimately reports 0% and may be omitted from *measurement*. It applies because the denominator is empty, not because the file is hard to test.
- The § Coverage Exclusion Policy prohibits an `exclude` **configuration entry** that removes a production file with real runtime code from the metric.

**Therefore:**

1. **Do not add an `exclude` entry for `IKbdAction.cs` to `coverage.config`.** Independently, this child is forbidden from touching `coverage.config` at all (`issue.md` lines 76-77: "This child must not modify `coverage.config` or any shared build property file; those are owned by F1 and the epic root"). The current `coverage.config` (lines 12-21) excludes only third-party module paths — `Deedle`, `FSharp`, `Castle.Core`, `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest` — and no QuickFiler path. That is correct and must stay correct.
2. **Do not add `[ExcludeFromCodeCoverage]` to the interface.** The attribute would be inert: there are no method bodies for it to suppress. Adding it would be churn on a file consumed by five implementers, would misrepresent an empty-denominator file as a *ratified exemption* (a category `epic.md` Shared Design section 1 reserves for the irreducible COM/WinForms remainder), and would inflate the count that the epic's leading indicator tracks — "the count of QuickFiler files carrying `[ExcludeFromCodeCoverage]` on a testable seam falls to zero" (`epic.md` lines 14-15).
3. **The ledger classification should be a third category, not `testable` and not `ratified-exempt`.** Recommend F1 record `IKbdAction.cs` as **`interface-only / no executable behavior`**, with the § Coverage Requirements clarification as the citation. Forcing it into `testable` would create an unsatisfiable >= 80% obligation against an empty denominator; forcing it into `ratified-exempt` would misfile it alongside the COM/WinForms remainder and distort the exemption audit.

This matters beyond one file: `epic.md` line 112 records that **roughly 24 of the 121 compiled files are interface-only declarations with no executable behavior**. Whatever category F1 assigns here sets the precedent for all of them. Getting the category right once is worth more than the 18 lines at stake.

### 4.2 Non-gaps recorded so the planner does not re-open them

- **No default interface member could be added to increase coverage.** The target framework (`v4.8.1`) does not support them at runtime.
- **The commented-out members at lines 15-16 are not uncovered code.** Comments emit no IL. Restoring them to the contract would be a **breaking** change to five implementers plus `KbdActions` and is out of scope (section 6).
- **No seam, adapter, or refactor could make this file "more covered".** Extracting logic into a host-neutral module — the remedy § Coverage Exclusion Policy prescribes for host-bound files — presupposes that logic exists. There is none.

---

## 5. Seam Requirements

**None required. Recommendation: make zero production changes to `IKbdAction.cs`.**

Assessment against the `.claude/rules/csharp.md` seam hierarchy (lines 49-53):

| Hierarchy level | Determination |
| --- | --- |
| 1. Interface seam | **Already satisfied — this file *is* the interface seam.** `IKbdAction<T, U>` is precisely the narrow, purpose-specific contract the hierarchy's first level prescribes: four members, no boundary calls, no I/O. It is what makes `KbdActions<TKey, UClass, VDelegate>` testable against arbitrary element types, which is why `CLAUDE.md` line 303 can name `KbdActions<>` a non-exempt testable seam. |
| 2. Injectable delegate seam | Not applicable. No call path to isolate. |
| 3. Adapter seam | Not applicable. No static or third-party API is referenced. |

**Boundary dependencies:** none. No COM, no Outlook Interop, no WinForms (the file does not import `System.Windows.Forms` — unlike `KaChar.cs` and `KaKey.cs`, which need it for the `Keys` enum), no filesystem, no network, no clock, no randomness. The five using directives at lines 1-5 are unused by the declared members; `System`, `System.Collections.Generic`, `System.Linq`, and `System.Text` are boilerplate, and `System.Threading.Tasks` was presumably needed when the withdrawn members were live.

**STA last-resort clause (epic.md Shared Design section 3): not applicable.** No WinForms control is constructed. No `*.StaTests.cs` file is warranted.

**Determinism (`.claude/rules/general-unit-test.md` § Determinism Infrastructure):** not applicable. No time, randomness, or async scheduling exists in an abstract declaration.

---

## 6. Cross-Child Contract Impact

**Recommended production change set for this file: empty. Cross-child impact: none.**

Direct consumers (all seven `IKbdAction` matches from section 2.3):

| Consumer | Line | Owning child | Relationship |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/KaChar.cs` | 11, 58 | **F3 (this child)** | implements (2 classes) |
| `QuickFiler/Controllers/KaKey.cs` | 11, 58 | **F3 (this child)** | implements (2 classes) |
| `QuickFiler/Controllers/KaStringAsync.cs` | 10 | **F3 (this child)** | implements |
| `QuickFiler/Controllers/KbdActions.cs` | 15 | **F3 (this child)** | generic constraint |

**Every direct consumer is inside this child's own file set.** No sibling child references `IKbdAction` by name.

Indirect exposure is nonetheless wide: any change to the interface propagates through `KbdActions<>` to `QfcCollectionController.cs` (**F11**), `EfcFormController.cs` (**F9**), `KeyboardHandler.cs` (F3), and `IQfcKeyboardHandler.cs` (F3), plus five `QuickFiler.Test` files under F10/F11 territory (enumerated in `04-KbdActions.md` section 6).

**Additive-vs-breaking determination for changes a future planner might consider:**

| Hypothetical change | Determination | Verdict |
| --- | --- | --- |
| Uncommenting `Action<string> Update { get; set; }` (line 15) | **Breaking.** All five implementers already declare a matching `Update` member, so they would still compile — but the change alters the public contract and any future implementer would be forced to supply it. Additionally `KaStringAsync.Update` is semantically different from the four orphaned ones. | Out of scope. |
| Uncommenting `Type DelegateType { get; }` (line 16) | **Breaking — will not compile.** `KaCharAsync` (`KaChar.cs` line 58) and `KaKeyAsync` (`KaKey.cs` line 58) do **not** declare `DelegateType`; only `KaChar` and `KaKey` do. Restoring the member breaks two of five implementers immediately. | Out of scope. |
| Removing the unused using directives at lines 1-5 | Non-breaking but pointless — no behavior change, no coverage change, pure diff noise on a file five other files depend on. | Not recommended. |
| Adding `[ExcludeFromCodeCoverage]` | Inert on a body-less interface; misclassifies the file and inflates the epic's exemption-count indicator (section 4.1 item 2). | **Do not.** |
| Adding an `exclude` entry to `coverage.config` | Forbidden to this child by `issue.md` lines 76-77; also risks a Blocking feature-review finding under § Coverage Exclusion Policy. | **Do not.** |

**Files this child modifies for `IKbdAction` coverage: none.** No production edit, no new test file, no `QuickFiler.Test.csproj` change. This file contributes zero merge-conflict surface.

---

## 7. Proposed Test Cases

**None. Zero test cases are proposed for this file.**

Justification:

1. **There is no executable behavior to assert against** (section 2.1). A test targeting an abstract declaration can only assert on metadata.
2. **The contract is already fully exercised** through five implementers and the `KbdActions<>` constraint, with every member reached by named tests (section 3).
3. **`.claude/rules/general-unit-test.md` § Coverage Requirements explicitly contemplates this case** and permits omission from measurement without lowering any threshold (section 4).
4. **A reflection-based conformance test would add no coverage and negative value.** A test asserting "all five implementers implement `IKbdAction<,>`" restates what the C# compiler already enforces at build time — the assembly does not compile otherwise. `.claude/rules/csharp.md` line 92 names "introducing heavy generic abstraction frameworks without need" among prohibited behaviors, and the same restraint applies to tests that assert compiler-enforced facts.
5. **A test that constructs a Moq mock of `IKbdAction<,>` would cover the mock, not the interface.** Coverage would be attributed to the dynamically generated proxy assembly, which `coverage.config` line 18 already excludes (`.*Moq.*`).

**If the atomic plan requires a task for this file** so that the per-file mandate has a visible entry, the correct task is a **documentation/classification task**, not a test-authoring task: confirm F1's ledger classifies `IKbdAction.cs` as `interface-only / no executable behavior`, and record the harness's reported figure (0% of 0 lines, or absent) as evidence under `<FEATURE>/evidence/qa-gates/`. That task produces the artifact issue #136 asks for without manufacturing a test.

---

## 8. Risks and Open Questions

1. **Ledger classification is the one real dependency, and it sets a 24-file precedent.** If F1's ledger offers only the binary `testable` / `ratified-exempt`, `IKbdAction.cs` will be misfiled either way (section 4.1 item 3). `epic.md` line 112 counts roughly 24 interface-only files among the 121 compiled. Recommend raising the third category with F1 **before** this child executes, since retrofitting a category across 24 ledger rows after the fact is more expensive than adding it up front.
2. **A feature-review agent may flag a 0% file as a Blocking finding.** § Coverage Exclusion Policy directs reviewers to block on production-path exclusions, and a reviewer scanning for low-coverage files could stop at 0% without reading the § Coverage Requirements clarification. Mitigation: `spec.md` should cite the clarification by name and reference this artifact, so the review has the citation in front of it.
3. **Harness behavior on a body-less type is unverified.** Whether `dotnet-coverage` emits a Cobertura `<class>` entry with `line-rate="0"` and no `<line>` children, or omits the type entirely, has not been observed — F1's harness does not exist yet. Both outcomes are correct; the evidence artifact must state which one occurred so a later reader does not interpret an absent entry as a measurement failure. **Open question for F1.**
4. **Do not "improve" this file to raise its number.** Adding a default interface member to create coverable lines would not run on `v4.8.1` and would be a breaking contract change. Recorded explicitly because an agent optimizing a per-file percentage could reach for it.
5. **The two commented-out members are a real cleanup item, but not here.** They are the root cause of the orphaned `Update` / `DelegateType` members documented in `05-KaChar.md` (G1-G3) and `06-KaKey.md` (G1-G3). Per `promote-latent-defects-to-issues`, the cleanup belongs in **one** GitHub issue covering `KaChar.cs`, `KaKey.cs`, and this file's lines 15-16 together. Note in that issue that restoring `DelegateType` to the contract **will not compile** — `KaCharAsync` and `KaKeyAsync` lack the member (section 6) — so the cleanup direction is removal from the implementers, not restoration to the interface.

---

## 9. Sources

| File | Lines read | Used for |
| --- | --- | --- |
| `QuickFiler/Interfaces/IKbdAction.cs` | 1-19 (whole file) | Complete structural inventory; executable-behavior determination; commented-out members at 15-16 |
| `QuickFiler/Controllers/KaChar.cs` | 1-100 (whole file) | Implementers at lines 11, 58; orphaned `DelegateType` (43-46) and `Update` (50-55, 92-97); `KaCharAsync` lacks `DelegateType` |
| `QuickFiler/Controllers/KaKey.cs` | 1-100 (whole file) | Implementers at lines 11, 58; same orphan pattern; `KaKeyAsync` lacks `DelegateType` |
| `QuickFiler/Controllers/KaStringAsync.cs` | 1-96 (whole file) | Implementer at line 10; the only surviving `Update` consumer (62, 73); `ToggleControl` as a per-type extension (88-93) |
| `QuickFiler/Controllers/KbdActions.cs` | 1-147 (whole file) | Generic constraint (15); all four member dispatch sites (38, 44, 49, 51, 55, 73, 80, 92, 100-102, 110, 115, 125, 143) |
| `QuickFiler.Test/Controllers/KaCharTests.cs` | 1-155 (whole file) | Transitive coverage map |
| `QuickFiler.Test/Controllers/KaKeyTests.cs` | 1-144 (whole file) | Transitive coverage map |
| `QuickFiler.Test/Controllers/KaStringAsyncTests.cs` | 1-168 (whole file) | Transitive coverage map |
| `QuickFiler.Test/Controllers/KbdActionsTests.cs` | 1-88 (whole file) | Transitive coverage via the generic constraint |
| `QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs` | 1-181 (whole file) | Transitive coverage via the generic constraint |
| `QuickFiler/QuickFiler.csproj` | 13-14, 307-310, 359 | Target framework `v4.8.1` (rules out default interface members); compiled-surface confirmation at line 359 |
| `coverage.config` | 1-24 (whole file) | Confirmed only third-party module paths are excluded; no QuickFiler path |
| `.claude/rules/general-unit-test.md` | provided in session context | § Coverage Requirements interface-only clarification; § Coverage Exclusion Policy prohibition and Blocking-finding rule |
| `.claude/rules/csharp.md` | 1-97 (whole file) | Seam hierarchy (49-53); prohibited behaviors (89-96) |
| `CLAUDE.md` | 288-309 (§ UT2) | Exemption categories; testable-seam clause at line 303 |
| `docs/features/epics/quickfiler-per-file-coverage/epic.md` | 1-418 (whole file) | Interface-only file count (112); exemption leading indicator (14-15); Shared Design 1-6; F3 assignment (267-274) |
| `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/issue.md` | 1-95 (whole file) | `coverage.config` prohibition (76-77); F1 ledger authority (78-79) |

**Search commands run:** `rg 'IKbdAction' --glob '**/*.cs'` (7 matches, all enumerated in section 2.3); `rg 'DelegateType|\.Activated|ToggleControl' --glob '**/*.cs'`; `rg 'Update\s*=|\.Update\(' --glob 'QuickFiler/**/*.cs'`.
