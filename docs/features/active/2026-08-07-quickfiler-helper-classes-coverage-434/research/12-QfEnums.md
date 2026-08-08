# F4 per-file research — `QuickFiler/Helper Classes/QfEnums.cs`

Timestamp: 2026-08-07T22-40

Cluster: VIEWER-QUEUE. Companions: `09-ViewerQueueCore.md`, `10-ItemViewerQueue.md`,
`11-EfcViewerQueue.md`. Cross-cutting facts are established in `00-cluster-overview.md`.

Upstream contract: F1 owns the per-file coverage harness and the ratified exemption ledger at
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`; neither exists on disk yet.
This artifact's central deliverable is a **ledger classification request**, not a test plan.

**Headline verdict: this file has ZERO coverable lines. It produces no Cobertura `<class>` element
at all, so it has no coverage denominator, no numerator, and no measurable line rate. It cannot be
brought to 80%, and it does not need to be — there is nothing to cover. The correct F4 action is one
ledger entry and no code change of any kind, in either the production file or the test project.**

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Helper Classes/QfEnums.cs` | — |
| Line count | **16** | file ends at `:16`; matches `epic.md:282` |
| 500-line limit | 16 / 500, **484 lines of headroom** | `.claude/rules/general-code-change.md` § File Size Limit |
| Compiled | yes — `<Compile Include="Helper Classes\QfEnums.cs" />` | `QuickFiler/QuickFiler.csproj:352` |
| `[ExcludeFromCodeCoverage]` | **absent** — confirmed by full read; the file contains no attribute of any kind | `QfEnums.cs:1-16` |
| Type declarations | `public static class QfEnums` (`:3`) containing `public enum InitTypeEnum` (`:5-12`) | `:3-12` |
| Namespace | `QuickFiler` | `:1` |
| Banned APIs | none — the file contains no executable statement at all | `:1-16` |
| Commented-out content | one line: `//public enum ToggleState { Off = 0, On = 1 }` at `:14` | `:14` |

Complete content, for the record (the file is short enough that quoting it removes all ambiguity
about the denominator claim):

```
:1  namespace QuickFiler
:2  {
:3      public static class QfEnums
:4      {
:5          public enum InitTypeEnum
:6          {
:7              Sort = 1,      // 00000000 00000001   2^0
:8              Find = 2,      // 00000000 00000010   2^1
:9              Info = 4,      // 00000000 00000100   2^2
:10             Reminder = 8,  // 00000000 00001000   2^3
:11             SortConv = 16, // 00000000 00010000   2^4
:12         }
:13
:14         //public enum ToggleState { Off = 0, On = 1 }
:15     }
:16 }
```

---

## 2. Member inventory and the coverage denominator

| # | Member | Kind | Lines | Executable? | Decision points |
| --- | --- | --- | --- | --- | --- |
| 1 | `QfEnums` | `public static class` — a pure namespace-like container; **no fields, no properties, no methods, no constructor, no static constructor** | 3-15 | **no** | 0 |
| 2 | `InitTypeEnum` | `public enum` nested in `QfEnums`; underlying type `int` (default) | 5-12 | **no** | 0 |
| 2a | `Sort = 1` | enum literal field | 7 | no | 0 |
| 2b | `Find = 2` | enum literal field | 8 | no | 0 |
| 2c | `Info = 4` | enum literal field | 9 | no | 0 |
| 2d | `Reminder = 8` | enum literal field | 10 | no | 0 |
| 2e | `SortConv = 16` | enum literal field | 11 | no | 0 |

**The coverage denominator for this file is 0 sequence points.** Reasoning, and then direct
evidence:

1. An `enum` declaration compiles to a `System.Enum`-derived type whose only members are the special
   instance field `value__` and one `public static literal` field per member. Literal fields are
   compile-time constants embedded in metadata; they have no IL body and therefore no sequence
   points. Every consumer site (`QfEnums.InitTypeEnum.Sort`) is compiled to an `ldc.i4` at the
   *consumer's* line, not at a line in this file.
2. The containing `public static class QfEnums` declares nothing else, so Roslyn emits no
   constructor, no static constructor, and no method for it. A class with no method bodies produces
   no sequence points.

**Direct evidence, not inference.** A repository search of both already-committed feature-#424
Cobertura artifacts for `filename="QuickFiler\Helper Classes\QfEnums.cs"` returns **no match**:

- `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
- `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/baseline/coverage-baseline.cobertura.xml`

In those same artifacts the string `QfEnums` appears **only inside method `signature` attributes of
other types**, for example
`coverage-final.cobertura.xml:178` (`name="set_InitType" signature="(QuickFiler.QfEnums.InitTypeEnum)"`),
`:1144`, `:1151`, `:1166`, `:1187`, `:1194`, `:1210`, `:1601`, `:1613`. That is, the coverage tool
sees the *type name* as a parameter type of other classes' methods, and emits no class element for
the file itself. Contrast every other F4 file, each of which does emit one — for example
`coverage-final.cobertura.xml:2213` for `EfcViewerQueue.cs`, `:2394` for `ItemViewerQueue.cs`,
`:2614` for `QfcThemeControlSet.cs`, `:3771` for `ViewerQueueCore.cs`.

Consequence for the F1 harness: any per-file report built by grouping Cobertura `<class>` elements
by `filename` will simply **not contain a row** for `QfEnums.cs`. The harness must therefore
distinguish "0% covered" from "not present / no executable code"; reporting the file as 0% would be
a measurement artefact, not a coverage failure. This is a concrete requirement to feed back to F1.

---

## 3. Existing test inventory

**None, and none is possible in the coverage sense.**

No file in `QuickFiler.Test/Helper Classes/` references `QfEnums` — the eight existing test files
there are inventoried in `00-cluster-overview.md` §2 and none targets this type. Elsewhere in the
test tree the type name appears in seven `QuickFiler.Test/Controllers/` files
(`QfcFormControllerTests.cs`, `QfcFormControllerSeamTests.cs`,
`QfcCollectionControllerDarkModeTests.cs`, `EfcHomeControllerSeamTests.cs`,
`EfcHomeControllerLifecycleTests.cs`, `EfcHomeControllerDependenciesTests.cs`,
`EfcHomeControllerDependenciesProductionFactoryTests.cs`), always as an argument value passed into a
controller under test (for example `QfEnums.InitTypeEnum.Sort`). Those references are compiled into
the *test* assembly as `ldc.i4` constants and contribute **zero** coverage to `QfEnums.cs`, because
there is no sequence point in `QfEnums.cs` for them to attribute to. Those test files are
sibling-owned (F6/F8/F11 territory) and F4 must not modify them.

---

## 4. Per-member coverage gap

| Member | Status |
| --- | --- |
| `QfEnums` (class) | **no coverable lines** — emits no IL body |
| `InitTypeEnum` (enum) | **no coverable lines** — literal fields only |
| `Sort`, `Find`, `Info`, `Reminder`, `SortConv` | **no coverable lines** — compile-time constants |

There is no `covered` / `partially covered` / `uncovered` distinction to draw, because the set of
coverable lines is empty. This is categorically different from a file whose lines exist but are not
reached.

---

## 5. Testability classification per member

Neither `pure-testable-now`, nor `needs-seam`, nor `host-bound-irreducible` applies, because none of
those three categories describes a member with no executable line. The accurate classification is a
fourth one that the F4 plan should name explicitly:

| Member | Classification |
| --- | --- |
| `QfEnums`, `InitTypeEnum` and all five literals | **`no-executable-code`** |

No Outlook Interop type and no WinForms type is touched — the file has no `using` directive at all
(`:1` is the namespace declaration). Nothing here is mockable with Moq because there is nothing to
mock; the type is data, not behaviour.

---

## 6. Ordering, concurrency and static-state invariants

**None. The file declares no state, mutable or otherwise.**

- No static field, no static property, no static constructor, so there is no static mutable state
  and no test-isolation hazard (contrast `10-ItemViewerQueue.md` §1.2 and `11-EfcViewerQueue.md`
  §1.2, where the same `static` keyword *does* carry state).
- No FIFO/LIFO or capacity semantics; not a collection.
- No `lock`, `Interlocked`, or `Concurrent*` usage, and none is needed: enum literals are immutable
  compile-time constants, trivially thread-safe.
- No disposal semantics; the type is never instantiated (a `static class` cannot be).
- "Same item enqueued twice" is not applicable.
- **No banned API.** No `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, or
  `Random.Shared` — the file contains no statement of any kind. No `TimeProvider` seam is required
  (`00-cluster-overview.md` §4 is not engaged).

### 6.1 One substantive observation — `InitTypeEnum` is bit-flag-shaped but is not `[Flags]`

The member values are `1, 2, 4, 8, 16` with explanatory bit-pattern comments (`:7-11`), and the type
is consumed with bitwise composition and `HasFlag` from sibling-owned files:

- Bitwise composition: `QuickFiler/Controllers/EfcHomeController.cs:75`
  (`InitType = QfEnums.InitTypeEnum.Sort | QfEnums.InitTypeEnum.SortConv;`) and `:134`.
- `HasFlag` on the resulting value: `QuickFiler/Controllers/EfcHomeController.cs:310`, `:327`;
  `QuickFiler/Controllers/EfcFormController.cs:200`, `:476`, `:716`, `:720`, `:758`;
  `QuickFiler/Controllers/QfcFormController.EventHandlers.cs:114`;
  `QuickFiler/Controllers/QfcExplorerController.cs:151`, `:179`.

Yet `QfEnums.cs:5` carries **no `[System.Flags]` attribute**. Two consequences: `ToString()` on a
composed value such as `Sort | SortConv` renders the numeric `17` rather than `"Sort, SortConv"`
(affecting any log or diagnostic that formats it); and repository analyzers that target this pattern
(the Sonar/Meziantou "non-flags enum used in bitwise operation / with `HasFlag`" family, part of the
five-package analyzer stack wired at `.claude/rules/csharp.md:67-79`) can be expected to fire on the
consumer sites once severities are promoted from `suggestion`.

**F4 must not add `[Flags]`.** Three reasons: (a) it changes observable `ToString`/`Parse`
behaviour, which `issue.md:69` forbids; (b) the affected consumers are all sibling-owned (F6, F8,
F9, F11 — see §9), so validating the change would require reading and possibly editing their files;
(c) it would add **zero** coverable lines, so it does nothing for issue #136's objective.

**Recommended action: promote a separate issue** through the promotion lifecycle, titled along the
lines of `qfenums-inittypeenum-missing-flags-attribute`, scoped to add `[Flags]` and verify the
formatting/analyzer consequences across the ten consumer files. Recording this only in a research
artifact would lose it at merge.

---

## 7. Static-state test-isolation analysis

Not applicable. The file declares no static mutable state (§6), so no test can leave residue through
it and no reset seam, `[TestInitialize]` hook, or `[DoNotParallelize]` attribute is warranted. The
static-state analysis that matters for this cluster is in `10-ItemViewerQueue.md` §7 and
`11-EfcViewerQueue.md` §7.

---

## 8. Seam proposal

**No seam is proposed, and none is possible.** A seam exists to make behaviour observable or
substitutable; this file declares no behaviour. Every tier of the hierarchy in
`.claude/rules/csharp.md` § DI Seams (interface seam > injectable delegate > adapter) is inapplicable
because there is no boundary call, no collaborator, and no method.

For completeness, the alternatives considered and rejected:

1. **Add `[ExcludeFromCodeCoverage]` to `QfEnums`.** Rejected on two independent grounds. First, it
   is a production edit with zero benefit — the file already contributes nothing to the coverage
   metric, so suppressing it changes no number. Second,
   `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy states that no production file
   may be excluded from coverage measurement, and `epic.md` Shared Design §1 reads the CLAUDE.md
   exemption as a live obligation rather than a standing permission; adding a new exclusion attribute
   would run against the epic's stated direction of driving the count of such attributes toward zero
   (`epic.md:14`).
2. **Add an entry to the repository-root `coverage.config`.** Rejected: `coverage.config` is a
   **shared file this child must not modify** (`issue.md:75`), and the exclusion would be
   unnecessary for the reason in item 1.
3. **Split the enum into its own file / move it out of the `QfEnums` container class.** Rejected:
   it is a pure churn refactor that would touch the 52 consumer references in ten sibling-owned
   files (§9) and add zero coverage.
4. **Add `[Flags]`.** Rejected for F4 and promoted as a separate issue — see §6.1.

**The recommended F4 change to this file is: none.**

---

## 9. Cross-child conflict analysis

F4's production file set is the 13 files under `QuickFiler/Helper Classes/` plus
`QuickFiler/Interfaces/IEmailMoveMonitor.cs` (`epic.md:276-283`). Every other QuickFiler file belongs
to a sibling child running in parallel. A repository-wide grep for `QfEnums` finds **52 occurrences
across 10 compiled production files**, every one of them outside F4's set, plus 3 non-compiled
`Legacy` files.

| Consumer file | Sibling owner | Line numbers |
| --- | --- | --- |
| `QuickFiler/Controllers/EfcHomeController.cs` | **F8** (`epic.md:309`) | 75 (×2), 81, 134 (×2), 165, 173, 206, 278, 279, 310, 327 |
| `QuickFiler/Controllers/EfcHomeControllerDependencies.cs` | **F8** (`epic.md:312`) | 21, 30, 53, 107, 210, 224, 228, 257, 279, 317, 337 |
| `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs` | **F8** (`epic.md:313`) | 56, 63, 150, 164, 184, 210, 224, 245 |
| `QuickFiler/Controllers/EfcFormController.cs` | **F9** (`epic.md:317`) | 38, 58, 149, 200, 476, 716, 720, 758 |
| `QuickFiler/Controllers/QfcExplorerController.cs` | **F6** (`epic.md:295`) | 28, 39, 151, 179 |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | **F6** (`epic.md:294`) | 52, 86, 142 |
| `QuickFiler/Controllers/QfcFormController.cs` | **F6** (`epic.md:293`) | 31, 77 |
| `QuickFiler/Controllers/QfcFormController.EventHandlers.cs` | **F6** (`epic.md:294`) | 114 |
| `QuickFiler/Controllers/QfcCollectionController.cs` | **F11** (`epic.md:332`) | 33, 61 |
| `QuickFiler/Controllers/QfcHomeController.cs` | **F7** (`epic.md:302`) | 16 (`using static QuickFiler.QfEnums;`), plus the unqualified uses that directive enables at 92, 102, 137, 144 |

Non-compiled, outside the epic (`epic.md:108-110` — `QuickFiler/Legacy/**` is not listed as
`<Compile Include>`): `QuickFiler/Legacy/QfcController.cs` (many, e.g. 430, 873, 964, 1016, 1092,
1143, 1294, 1313, 1448, 1454, 1460, 1466), `QuickFiler/Legacy/QuickFileController.cs`,
`QuickFiler/Legacy/QfcGroupOperationsLegacy.cs`.

Sibling-owned **test** files that reference the type (F4 must not modify them):
`QuickFiler.Test/Controllers/QfcFormControllerTests.cs`, `QfcFormControllerSeamTests.cs`,
`QfcCollectionControllerDarkModeTests.cs`, `EfcHomeControllerSeamTests.cs`,
`EfcHomeControllerLifecycleTests.cs`, `EfcHomeControllerDependenciesTests.cs`,
`EfcHomeControllerDependenciesProductionFactoryTests.cs`.

Build reference: `QuickFiler/QuickFiler.csproj:352` — existing line, shared file, **no edit needed**.

**Explicit per-proposal statement:**

- Recommended proposal (**no change to the file**): **requires no sibling-owned file change** —
  trivially, since there is no change at all.
- Rejected proposal 1 (`[ExcludeFromCodeCoverage]`): would require no sibling change, but is
  rejected on the policy grounds in §8.
- Rejected proposal 2 (`coverage.config`): **requires editing the repository-root `coverage.config`,
  a shared file explicitly out of bounds for this child** (`issue.md:75`) and contended by all
  fourteen siblings. Rejected; the alternative that does not is the ledger entry in §13.
- Rejected proposal 3 (split/move the enum): **would require editing all ten sibling-owned consumer
  files listed above** (namespace/type-name changes). Rejected; the alternative that does not is to
  leave the declaration exactly where it is.
- Rejected proposal 4 (`[Flags]`): would require no *compile-breaking* sibling edit, but its
  behavioural blast radius covers ten sibling-owned files and its verification would need their test
  suites. Deferred to a separate promoted issue (§6.1).

**This file is the lowest-risk file in the F4 set: the recommended production diff is empty, so its
merge-conflict probability against every sibling is zero.**

---

## 10. 500-line compliance

- `QfEnums.cs` — **16 lines**, compliant with the 500-line hard limit
  (`.claude/rules/general-code-change.md` § File Size Limit), with 484 lines of headroom. No partial
  split is needed or proposed, and the recommended change adds zero lines.
- **No new production file is proposed.** Recorded for completeness because the epic's shared-file
  risk applies to any child that adds one: a new production file would require a
  `<Compile Include="Helper Classes\<name>.cs" />` line in `QuickFiler/QuickFiler.csproj` inside the
  contiguous `Helper Classes\` block at `:342-354` — a file shared with all fourteen siblings and
  therefore a merge-conflict surface (`00-cluster-overview.md` §7.3). Avoided here.
- **No new test file is proposed** (§11), so no `<Compile Include>` line is needed in
  `QuickFiler.Test/QuickFiler.Test.csproj` for this file either. This file contributes **zero** to
  the cluster's shared-csproj footprint; the three test-file additions the cluster does need come
  from `09-ViewerQueueCore.md` §10, `10-ItemViewerQueue.md` §10, and `11-EfcViewerQueue.md` §10.

---

## 11. Recommended test cases

**Zero. No test is recommended for this file.**

Justification: no test can raise this file's coverage, because it has no coverable line (§2). Writing
a test would therefore satisfy none of the acceptance criteria at `issue.md:58-66` while adding a
new test file, a new shared-csproj line (§10), and permanent maintenance surface.

One candidate was considered and is **rejected**, recorded here so the planner does not re-derive it:

| Candidate | Scenario | Category | Why rejected |
| --- | --- | --- | --- |
| `InitTypeEnum_MemberValues_AreStableBitFlagConstants` | Assert `(int)QfEnums.InitTypeEnum.Sort == 1`, `Find == 2`, `Info == 4`, `Reminder == 8`, `SortConv == 16` — a regression guard against accidental renumbering of values that sibling controllers compose bitwise (§6.1) | boundary | **Contributes zero coverage to `QfEnums.cs`**: the compiler folds each member access into an `ldc.i4` at the *test's* line, and the production file has no sequence point to attribute a hit to. It would require a new test file plus a `<Compile Include>` line in the shared `QuickFiler.Test.csproj` for a guard whose real subject is the consumer semantics, not this file. If the renumbering risk is judged material, it belongs with the `[Flags]` issue promoted in §6.1, not in F4. |

Consistent with the sibling finding for `cInfoMail.cs` (`00-cluster-overview.md` §2 finding 2), the
correct F4 disposition for a file with no executable content is a **ledger classification, not test
authoring**.

---

## 12. STA determination

**No member of this file requires an STA thread, and no `*.StaTests.cs` file is proposed.**

The STA last-resort clause (`epic.md` Shared Design §3) permits never-shown in-memory WinForms
controls on an STA thread only where no seam can isolate the logic under test. It is not reached
here for the most basic reason available: there is no logic, no control, no thread affinity, and no
test. The seam hierarchy is not "exhausted" so much as inapplicable (§8). `QuickFiler.Test` has no
STA infrastructure today (`00-cluster-overview.md` §5) and this file gives no reason to introduce
any.

---

## 13. Projected coverage and the ledger request

**Projected line coverage: undefined — there are no coverable lines, before or after.** The file
cannot clear 80% because the ratio has a zero denominator; equally, it cannot fail 80%. The
irreducible fraction is 100% of a set of size zero, which is to say the file imposes no coverage
obligation at all.

**Request to F1's ledger** (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`):

| Field | Value |
| --- | --- |
| File | `QuickFiler/Helper Classes/QfEnums.cs` |
| Requested classification | **`no-executable-code`** — a "no coverable lines" request, **not** `ratified-exempt` and **not** `testable` |
| Owning child | F4 `quickfiler-helper-classes-coverage` (#434) |
| Basis | The file declares one `static class` with no members other than a nested `enum` whose members are compile-time literal fields; Roslyn emits no method body, so no sequence point exists. Verified empirically: neither feature-#424 Cobertura artifact contains a `<class>` element with `filename="QuickFiler\Helper Classes\QfEnums.cs"`, while every other F4 file does (§2). |
| Exemption attribute required | **none** — no `[ExcludeFromCodeCoverage]` is to be added, and `coverage.config` is not to be touched (§8, §9) |
| Policy anchor | `.claude/rules/general-unit-test.md` § Coverage Requirements already recognises this category: "Type-only / interface-only modules with no executable behavior may be omitted from coverage measurement… Such modules legitimately report 0% executable coverage and may be excluded from measurement. This is a clarification only; it does not lower any coverage threshold." An enum-only declaration file is the same class of artifact as an interface-only file. |
| Consequence for F16 (capstone) | `QfEnums.cs` is one of the ~24 declaration-only files the epic already anticipates (`epic.md:112`). It must appear in the ledger so the capstone can account for all 121 compiled files, but it contributes no numerator or denominator to the aggregate. |

**Harness requirement to feed back to F1:** the per-file report must distinguish
*absent from the Cobertura output* (no executable code) from *present with a 0% line rate* (real
uncovered code). If the harness defaults a missing file to 0%, `QfEnums.cs` and the other
declaration-only files will appear as failures that no amount of test authoring can fix.

---

## 14. Findings to carry into the F4 plan

1. **Zero coverable lines, proven empirically** — no Cobertura `<class>` element exists for this file
   in either committed #424 artifact, while every other F4 file has one (§2).
2. **Recommended production diff: empty.** No `[Flags]`, no `[ExcludeFromCodeCoverage]`, no
   `coverage.config` entry, no file split (§8).
3. **Recommended test count: zero**, with the one candidate explicitly considered and rejected
   (§11).
4. **Ledger request: `no-executable-code`**, not `ratified-exempt` (§13).
5. **F1 harness requirement**: "absent from the report" must not be rendered as "0% covered" (§13).
6. **Latent defect to promote separately**: `InitTypeEnum` is bit-flag-shaped (`1, 2, 4, 8, 16`) and
   is consumed with `|` and `HasFlag` from ten sibling-owned files, but carries no `[Flags]`
   attribute (§6.1). Out of F4 scope; promote as its own issue rather than leaving it in an artifact
   that disappears at merge.
7. **Zero merge-conflict probability** — an empty production diff and no shared-csproj line (§9,
   §10). This is the safest file in the cluster.
