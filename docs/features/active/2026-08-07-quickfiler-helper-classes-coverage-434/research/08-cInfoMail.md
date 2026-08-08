# F4 per-file research — `QuickFiler/Helper Classes/cInfoMail.cs`

Timestamp: 2026-08-07T22-40

Cluster: CONVERSATION-RESOLUTION (artifacts 05–08). Cross-cutting facts are in
`research/00-cluster-overview.md`.

Upstream contract: child F1 owns the per-file line-coverage harness and the ratified exemption ledger
at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
This artifact defines no alternative coverage measurement. `coverage.config` is a shared file this
child must not modify.

**Headline finding — the orchestrator's preliminary inspection is CONFIRMED.** `cInfoMail.cs` is 231
lines of which the only non-comment, non-blank content is 8 `using` directives. The file **declares
no namespace and no type**, contains **zero executable statements**, and therefore has an **empty
coverage denominator**. It cannot be covered by any test. The issue-#434 test condition *"`cInfoMail`
construction from a mocked `MailItem` including missing-property paths"* (`issue.md:99`) is
**unsatisfiable as written** and must be struck from the plan.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/cInfoMail.cs` | — |
| Line count | 231 content lines; the file terminates with a newline so a `wc -l`-style count reads 231 and an editor shows an empty line 232 | read of `:225-232` |
| Compiled | yes | `QuickFiler/QuickFiler.csproj:342` — `<Compile Include="Helper Classes\cInfoMail.cs" />` |
| `[ExcludeFromCodeCoverage]` | **absent** | grep across `QuickFiler/Helper Classes/` returns no match |
| Types declared | **none** | `:13` is `//namespace QuickFiler`, `:16` is `//    public class cInfoMail` — both comments |
| Namespace declared | **none** | `:13` commented |
| Executable statements | **0** | `:13-231` are all `//`-prefixed comment lines |
| 500-line limit | 231 / 500 — nominally compliant, though the metric is meaningless for a file with no code | — |

### 1.1 Line-by-line verification of the "only content is usings" claim

| Lines | Content | Active? |
| --- | --- | --- |
| `:1` | `using System;` | yes |
| `:2` | `using System.Collections.Generic;` | yes |
| `:3` | `using System.Diagnostics;` | yes |
| `:4` | `using System.Linq;` | yes |
| `:5` | `using System.Windows.Forms;` | yes |
| `:6` | `using Microsoft.Office.Interop.Outlook;` | yes |
| `:7` | `using ToDoModel;` | yes |
| `:8` | `//using Microsoft.VisualBasic;` | **commented** |
| `:9` | `//using Microsoft.VisualBasic.CompilerServices;` | **commented** |
| `:10` | `using UtilitiesCS;` | yes |
| `:11-12` | blank | — |
| `:13-231` | every line begins with `//` | **all commented** |

**8 active `using` directives** (`:1-7`, `:10`), two commented-out ones (`:8-9`), and 219 consecutive
comment lines. Confirmed exactly as the orchestrator reported.

### 1.2 What the commented block contains (for disposition reasoning only)

The commented body is a VB6-era translation carrying `//    [Obsolete]` at `:15` and
`//    public class cInfoMail` at `:16`, with: public fields `Subject`/`StartDate`/`SentTo`/`SentCC`/
`SentFrom`/`Body`/`Importance`/`Categories`/`Action`/`ProcName` (`:18-29`); a private
`Dictionary<string,long> _dict` (`:31`); `ReverseSortDictionary` (`:36-39`); `dict_new` (`:41-44`);
`dict_add` (`:45-49`); `dict_ct` (`:51-57`); `dict_strSum` (`:59-90`); `dict_upORadd` (`:92-102`);
`internal object Init(...)` (`:103-135`); `internal bool Init_wMail(MailItem, ...)` (`:137-166`);
`EndDate` (`:168-179`); `DurationSec` (`:181-192`); and `public new string ToString` (`:194-228`).

Two properties of that dead body are relevant to the disposition:

1. `Init_wMail` (`:159`) contains a `MessageBox.Show(...)` in its catch block. Reviving this class
   would introduce a **modal dialog into a code path**, which epic.md Shared Design §2 forbids in any
   testable unit ("never show popups"). The class is not a candidate for revival.
2. `Init` (`:127`) calls `lcl_Categories.ToString()` on a parameter defaulted to `null`, and
   `dict_strSum` (`:63`) declares `string dict_strSumRet = default;`. The code would not compile
   cleanly as written; `ReverseSortDictionary` (`:38`) uses `ToDictionary()` with no arguments, an
   overload that does not exist on .NET Framework 4.8.1's `Enumerable`. **The commented code is not
   merely disabled — it is not compilable in this solution.**

---

## 2. Member inventory (coverage denominator for THIS file)

| # | Type | Member | Lines | Decision points |
| --- | --- | --- | --- | --- |
| — | — | **none** | — | — |

**The inventory is empty. The coverage denominator for this file is 0 executable lines.**

A C# compilation unit consisting solely of `using` directives is legal and produces no metadata and
no IL. The file contributes nothing to `QuickFiler.dll` beyond compilation time.

**Analyzer note (not verified by a build; msbuild was deliberately not run per this research task's
constraints).** All 8 active `using` directives are unused, which is the IDE0005 condition. A grep of
`.editorconfig` for `IDE0005` returns **no match**, so the rule runs at its default severity and is
not promoted to `warning`; there is therefore no evidence that it would break the
`/p:EnforceCodeStyleInBuild=true` or `/p:TreatWarningsAsErrors=true` toolchain stages. The file
builds today on `main`, which is the strongest available evidence that it is diagnostic-clean under
the current configuration.

---

## 3. Existing test inventory — and the `MailItemInfoTests.cs` question

### 3.1 No test references `cInfoMail` anywhere

A repository-wide grep for the identifier `cInfoMail` returns these matches and no others:

| Match | Nature |
| --- | --- |
| `QuickFiler/QuickFiler.csproj:342` | the live `<Compile Include>` entry |
| `QuickFiler/QuickFiler.csproj.bak:248` | the same entry in the **stale** backup csproj |
| `QuickFiler/Helper Classes/cInfoMail.cs:16` | the commented class declaration |
| `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1277` | `var infoMail = new cInfoMail();` — **live code in a NON-COMPILED file** (see §3.3) |
| `QuickFiler/Legacy/QuickFileController.cs:1058` | a reference inside a commented-out method signature |
| `docs/**` | epic, issue, spec, user-story, and cluster-overview prose |

**Zero test files reference it.**

### 3.2 `MailItemInfoTests.cs` has NO bearing on `cInfoMail.cs` — verified

The orchestrator's suspicion is confirmed on three independent grounds.

**(a) The type under test is `UtilitiesCS.MailItemHelper`, not `cInfoMail`.** The factory method is:

```
private MailItemHelper CreateMailItemInfo()
{
    return new MailItemHelper(this.mockMailItem.Object, this.mockApplicationGlobals.Object);
}
```

— `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:120-123`. `MailItemHelper` is declared at
`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs` (public partial class; the two-argument
constructor is at `:86-90`).

**(b) No type named `MailItemInfo` exists anywhere in the repository.** A grep for
`class MailItemInfo|struct MailItemInfo|record MailItemInfo` returns exactly one match —
`QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:16`, the **test class** itself. The test file is
named after a production type that no longer exists; it was evidently renamed to `MailItemHelper` at
some point and the test file name was never updated. The commented-out body at
`MailItemInfoTests.cs:147` still writes `new MailItemInfo() { ... }`, which is why the body cannot
compile and is commented out.

**(c) Both of its test methods have fully commented-out bodies and assert nothing.**
`SenderName_Get_StateUnderTest_ExpectedBehavior` (`:125-138`) and
`ExtractBasics_StateUnderTest_ExpectedBehavior` (`:140-168`) each contain only
`//TODO: Incomplete. Need to finish setting up the mail item mock` (`:128`, `:143`) followed by
commented Arrange/Act/Assert. They execute zero production statements and pass unconditionally.

**Conclusion: `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` contributes exactly zero coverage
to `QuickFiler/Helper Classes/cInfoMail.cs`, and to any other F4 file.** Its only residual value is
the ~85-line Moq arrangement at `:34-118` (`PropertyAccessor`, `AddressEntry`, `Recipient`,
`Recipients`, `UserProperty`, `UserProperties`, `Folder`), which is a reusable reference for other F4
clusters (`00-cluster-overview.md` §2 finding 1).

### 3.3 The `QuickFiler/Legacy/` reference is inert

`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1277-1289` contains live, non-commented code that
constructs `cInfoMail` and calls `infoMail.Init_wMail(...)` and `infoMail.ToString`. It is inert for
three compounding reasons:

1. **`QuickFiler/Legacy/**` is not compiled.** A grep of `QuickFiler/QuickFiler.csproj` for `Legacy\`
   and `Notes\` returns **no match**. Epic.md § Scope states the same. The Legacy tree is present in
   the working tree but outside the compiled surface and outside this epic.
2. **It could not compile even if re-added**, because `cInfoMail` is commented out — the very
   symbol it constructs does not exist.
3. **The stale `QuickFiler/QuickFiler.csproj.bak` proves the historical state**: it lists
   `Legacy\QfcGroupOperationsLegacy.cs` at `:264` and `Legacy\QuickFileController.cs` at `:284`
   alongside `Helper Classes\cInfoMail.cs` at `:248`. The Legacy files and `cInfoMail` were removed
   from compilation together. Per `00-cluster-overview.md` §6, **the `.bak` files must not be
   modified, deleted, or referenced** by this child.

---

## 4. Per-member coverage gap

| # | Member | Status |
| --- | --- | --- |
| — | — | **not applicable — the file declares no member.** There is no line to be `covered`, `partially covered`, or `uncovered` |

---

## 5. Testability classification per member

| # | Member | Classification |
| --- | --- | --- |
| — | — | **not classifiable.** `pure-testable-now`, `needs-seam`, and `host-bound-irreducible` all describe executable code. This file has none |

For completeness on the Interop question: `using Microsoft.Office.Interop.Outlook;` at `:6` is an
**unused** import. No Interop type is named in any active declaration and no Interop API is invoked.
The commented `Init_wMail` signature at `:137` names `MailItem`, and `MailItem` is a COM interface
that Moq can proxy (`00-cluster-overview.md` §3), but that is irrelevant because the method does not
exist in the compiled assembly.

---

## 6. Ordering and async invariants

**None. The file declares no behaviour, no state, no ordering, and no asynchrony.** There is no
`INotifyPropertyChanged` implementation, no `Task`, no `await`, no event, and no state transition to
enumerate.

**Banned-API audit of this file: CLEAN.** A grep of `QuickFiler/Helper Classes/` for `DateTime.Now`,
`DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`, and `Random.Shared` returns **no match** in
`cInfoMail.cs`. **No banned-API finding in production code for this file.** (The commented body at
`:177` and `:190` uses `DateTime`/`TimeSpan` arithmetic, and `:205-209` reads `TimeSpan` members, but
none of it is compiled and none of it calls a banned symbol.)

**Banned-API finding in an ADJACENT F4 test file — reported, not fixed here.**
`QuickFiler.Test/Helper Classes/MailItemInfoTests.cs:25` declares
`private DateTime now = DateTime.Now;`. `System.DateTime.Now` is banned by `BannedSymbols.txt:1`
("Do not use DateTime.Now. Inject System.TimeProvider and call GetLocalNow() …"), and
`00-cluster-overview.md` §1.2 confirms the banned-symbol set applies to test code as well as
production code. `RS0030` is held at `severity = suggestion` (`.editorconfig:546-548`), so this does
not break the build today. The field is consumed only by the commented-out body at `:154`, so it is
dead. Disposition guidance is in §12.4.

---

## 7. Seam proposal

**None. No seam is required, and none is possible.** A seam is an injection point in executable code;
this file has no executable code. The proposal for this file is a **disposition decision** (§12), not
a refactor.

### Rejected alternatives (brief)

- **Uncomment and revive `cInfoMail`, then cover it.** Rejected on four independent grounds: (i) it
  would be a behaviour change, violating the issue's acceptance criterion "No behavior change to
  observable QuickFiler flows" (`issue.md:69`); (ii) the code does not compile against .NET Framework
  4.8.1 as written (`ToDictionary()` with no arguments at `:38`); (iii) `Init_wMail` shows a modal
  `MessageBox` (`:159`), forbidden by epic.md Shared Design §2; (iv) its only consumer,
  `QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1277`, is not compiled and is outside this epic's
  scope entirely.
- **Rewrite the dead class as a clean, testable helper.** Rejected: that is new feature work, not
  coverage work, and no caller exists to justify it.

---

## 8. Cross-child conflict analysis

### 8.1 Files outside F4 that call into this file

**None that are compiled.** The exhaustive grep in §3.1 finds exactly two code references outside
this file, both in `QuickFiler/Legacy/`, which is not in `QuickFiler.csproj` and is explicitly out of
epic scope (epic.md § Scope, § Non-Goals). `QuickFiler/Legacy/**` is assigned to **no child** of epic
#136, so there is no sibling owner to conflict with.

There is therefore **zero cross-child call-site conflict surface for `cInfoMail.cs`**.

### 8.2 Conflict verdict per candidate disposition

| Disposition | File changes required | Verdict |
| --- | --- | --- |
| **(b) RECOMMENDED — retain the file, request an F1-ledger `no-coverable-lines` classification** | none | **Requires no sibling-owned file change. Requires no file change at all.** Zero conflict surface. |
| **(a) delete `cInfoMail.cs` and remove its `<Compile Include>` line** | (i) delete `QuickFiler/Helper Classes/cInfoMail.cs` (F4-owned, safe); (ii) **delete line `342` of `QuickFiler/QuickFiler.csproj`** | **Requires editing the shared `QuickFiler/QuickFiler.csproj`, which all thirteen sibling children also edit.** The deletion is a single-line removal in the middle of the contiguous `Helper Classes\` block (`:342-354`); siblings add and remove lines in the `Controllers\` (`:320-341`) and `Viewers\`/`Interfaces\` regions. A three-way merge of a one-line deletion in a textually distinct region usually resolves cleanly, but it is a non-zero risk that disposition (b) avoids entirely. |
| add `[ExcludeFromCodeCoverage]` | would require adding a type to attach it to | **Impossible and prohibited.** There is no type. Epic.md Shared Design §1 also treats the attribute as a Blocking finding when applied to avoid coverage work. |

### 8.3 Intra-F4 coordination

No other F4 cluster (conversation resolution, theme helpers, viewer queues, `EmailMoveMonitor`)
references `cInfoMail`. `QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` sits in the F4 test
directory; §12.4 recommends leaving it alone in this child.

---

## 9. 500-line compliance

| File | Now | After recommended disposition (b) | Limit |
| --- | --- | --- | --- |
| `QuickFiler/Helper Classes/cInfoMail.cs` | 231 | 231 (unchanged) | 500 — compliant |
| Under rejected disposition (a) | 231 | file deleted | n/a |

The 500-line rule is a code-size control; applying it to a file containing 219 comment lines and no
code is formally satisfied but substantively meaningless. Recorded for completeness.

No new production file is proposed, so no `<Compile Include=...>` addition to
`QuickFiler/QuickFiler.csproj` (`Helper Classes\` block, `:342-354`) is needed. No new test file is
proposed, so no addition to `QuickFiler.Test/QuickFiler.Test.csproj` (`Helper Classes\` block,
`:158-165`) is needed. **This file contributes zero shared-file conflict surface under the
recommended disposition.**

---

## 10. Recommended test cases

**Zero. No test case is recommended, and none is possible.**

This is a hard impossibility, not a judgement call:

1. There is no type to instantiate, no method to invoke, and no property to read. A test file
   referencing `cInfoMail` would fail to compile with CS0246 (type or namespace not found).
2. The issue's test condition *"`cInfoMail` construction from a mocked `MailItem` including
   missing-property paths"* (`issue.md:99`, mirrored at `spec.md:119`) presupposes a constructible
   `cInfoMail` type. **That presupposition is false.** The atomic plan must strike this line item and
   record why, so the acceptance-criteria tracker does not carry an unsatisfiable condition.
3. The acceptance criterion "Coverage per file spans the positive path plus invalid-input, boundary,
   and error-handling behavior" (`issue.md:65-66`) is **vacuously satisfied** for this file: there is
   no path of any category. The plan should record that explicitly rather than leaving the criterion
   apparently unmet.

**Category coverage table for this file:** positive 0, invalid-input 0, boundary 0, error-handling 0
— vacuous, by construction.

---

## 11. STA determination

**No STA test is required, and none is possible.** The file declares no type, constructs no WinForms
control, and has no test to place anywhere. The `using System.Windows.Forms;` directive at `:5` is
unused; the only WinForms reference in the file is the commented `MessageBox.Show` at `:159`.
`QuickFiler.Test` remains free of `[STATestClass]`, `[STATestMethod]`, and `*.StaTests.cs`
(`00-cluster-overview.md` §5), and this artifact proposes no change to that.

---

## 12. Projected coverage and recommended disposition

### 12.1 Projected coverage

**Undefined.** The line-coverage denominator is 0. The file cannot reach 80% and cannot fall below
80%; the ratio is not defined on an empty set. Any per-file report printing `0%` for this file is
reporting a division artifact, not a coverage deficiency.

### 12.2 How Cobertura represents a file with zero coverable lines

`Invoke-MSTestWithCoverage.ps1`'s Cobertura output is F1's input. Two emitter behaviours are
possible and F1's per-file harness must tolerate both:

- **The file is absent from the report.** A compilation unit that produces no metadata yields no
  `<class>` element, so `cInfoMail.cs` simply never appears under `<packages>/<package>/<classes>`.
  The harness must treat "listed as `<Compile Include>` in the csproj but absent from the Cobertura
  report" as *no coverable lines*, **not** as 0%.
- **A `<class>` element is emitted with an empty `<lines/>` collection.** Some emitters write the
  element with `line-rate="0"` (or `"1"`) and zero `<line>` children. The harness must key its
  has-a-denominator decision on the **`<line>` child count**, never on `line-rate`, or it will
  mis-report this file as a 0% failure and block F4's acceptance criteria.

Either representation must map to the same ledger classification.

### 12.3 Recommended disposition: **(b) retain the file and request an F1-ledger classification of `no-coverable-lines`**

The classification must be explicitly **distinct from `ratified-exempt`**, which per epic.md Shared
Design §1 presumes *coverable-but-untestable* lines and requires an irreducible-remainder
justification. `cInfoMail.cs` has no coverable lines at all, so the exemption test does not apply to
it; asserting `ratified-exempt` would misrepresent the file and would import an
irreducible-remainder argument that has no subject.

Rationale for preferring (b) over (a):

1. **Zero conflict surface.** (b) requires no edit to any file. (a) requires deleting line `342` of
   `QuickFiler/QuickFiler.csproj`, the shared project file that all thirteen sibling children of epic
   #136 are editing concurrently. `00-cluster-overview.md` §1.3 identifies exactly this class of edit
   as the epic's highest-probability merge-conflict surface.
2. **The coverage outcome is identical.** Under (a) the file disappears from the denominator; under
   (b) it is classified as having an empty denominator. Neither changes any aggregate or per-file
   percentage. (a) buys **no** coverage benefit for its conflict cost.
3. **A consistent, epic-wide treatment is F1's job, not F4's.** Epic.md § Scope records that ~24 of
   the 121 compiled files are declaration-only with no executable behaviour. A single ledger rule
   applied by F1 across all of them is more durable than one child unilaterally deleting one file.
   F4 should consume F1's classification, per `issue.md:76-78` ("it does not define its own coverage
   measurement mechanism and does not unilaterally decide exemptions").
4. **Deletion is safe but is dead-code hygiene, not coverage work.** (a) *is* behaviour-neutral —
   verified: the file declares nothing, so removing it cannot change any compiled symbol; its only
   code reference (`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs:1277`) is in a non-compiled file
   that already cannot compile against the commented-out class. But behaviour-neutral is not the same
   as in-scope. Deleting it belongs in a repository-hygiene issue promoted through the lifecycle,
   executed after epic #136 fans in and the csproj is no longer under thirteen-way concurrent edit.
   That hygiene issue should bundle `cInfoMail.cs` with the other dead artefacts identified in this
   cluster: `QuickFiler/QuickFiler.csproj.bak`, `QuickFiler.Test/QuickFiler.Test.csproj.bak`
   (`00-cluster-overview.md` §6), and the two `[Obsolete(…, true)]` methods at
   `QuickFiler/Helper Classes/ConversationResolver.cs:299-356` (`05-ConversationResolver.md` §12).
5. **No `[ExcludeFromCodeCoverage]`.** Not merely inadvisable — impossible, since there is no type to
   attach the attribute to. The file carries none today and must keep none.

**Disposition (c), considered and rejected: retain the file but strip the 219 comment lines, leaving
only the 8 `using` directives.** This would shrink the file to ~10 lines while remaining
behaviour-neutral, but it destroys the historical record for no measurable benefit, still leaves a
zero-denominator file in the compiled surface, and still requires an F4 commit touching a file whose
coverage cannot change. Rejected.

### 12.4 Companion recommendation for `MailItemInfoTests.cs` — report, do not fix in this child

`QuickFiler.Test/Helper Classes/MailItemInfoTests.cs` is misleadingly named (no `MailItemInfo` type
exists), has two assertion-free test methods (`:125-138`, `:140-168`), and contains a banned-API call
site at `:25` (`DateTime.Now`). It is nonetheless **not** a `cInfoMail` concern and should be left
untouched by the `cInfoMail` phase of the plan, for two reasons:

- Its ~85-line Moq arrangement (`:34-118`) is cited by `00-cluster-overview.md` §2 as a reusable
  reference for other F4 clusters; deleting or rewriting it during the `cInfoMail` phase would create
  an avoidable intra-F4 sequencing dependency.
- Any edit to it also requires touching `QuickFiler.Test/QuickFiler.Test.csproj:160`, the shared test
  project file, if the file is removed or renamed.

Recommended handling: record the three defects (misleading name, assertion-free tests, `DateTime.Now`
at `:25`) as a promotion-lifecycle issue so they survive the feature-folder merge, per the repository
practice of converting out-of-scope defects into real issues rather than leaving them as prose in a
feature folder. If the F4 plan chooses to act within this child instead, the correct owner is the
theme/helper phase that already touches `QuickFiler.Test/Helper Classes/`, not the `cInfoMail` phase,
which produces no test artefact at all.

### 12.5 Summary verdict

| Question | Answer |
| --- | --- |
| Does `cInfoMail.cs` declare a type? | No |
| Does it contain any executable statement? | No |
| Can any test cover it? | No — the denominator is empty |
| Does `MailItemInfoTests.cs` bear on it? | No — it targets `UtilitiesCS.MailItemHelper`, and both of its test bodies are commented out |
| Is it `ratified-exempt`? | No — that classification presumes coverable lines |
| Recommended disposition | **(b)** retain + F1-ledger `no-coverable-lines` classification |
| Does the recommendation touch a shared file? | No |
| Test cases recommended | 0 |
| Is `issue.md:99` satisfiable? | **No — strike it from the plan** |
