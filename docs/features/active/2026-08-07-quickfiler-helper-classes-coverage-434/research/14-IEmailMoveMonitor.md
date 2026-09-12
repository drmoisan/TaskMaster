# 14 — `QuickFiler/Interfaces/IEmailMoveMonitor.cs`

Timestamp: 2026-08-07T22-05

Cluster: MOVE-MONITOR (F4, epic `quickfiler-per-file-coverage` #136, child issue #434).
Companion artifacts: `00-cluster-overview.md`, `13-EmailMoveMonitor.md`.

Upstream contract: per-file coverage is measured by F1's harness; classification authority is F1's
`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. Neither exists on disk yet.
No coverage run was executed for this research.

---

## 1. File facts

| Fact | Value | Evidence |
| --- | --- | --- |
| Path | `QuickFiler/Interfaces/IEmailMoveMonitor.cs` | — |
| Exact line count | **39** | line-count of the file; matches epic.md `:283` |
| `<Compile Include>` present | **Yes** — `<Compile Include="Interfaces\IEmailMoveMonitor.cs" />` | `QuickFiler/QuickFiler.csproj:355` |
| `[ExcludeFromCodeCoverage]` | **Absent** (confirmed) — the file contains no attribute of any kind | reading `:1-39` |
| Type declared | 1 — `internal interface IEmailMoveMonitor` | `:13` |
| Namespace | `QuickFiler.Interfaces` | `:4` |
| `using` directives | `System` (`:1`), `Microsoft.Office.Interop.Outlook` (`:2`) | — |
| Sole implementer | `internal class EmailMoveMonitor : IEmailMoveMonitor` | `QuickFiler/Helper Classes/EmailMoveMonitor.cs:18` |
| 500-line limit | Compliant, 461 lines of headroom | — |

---

## 2. Member inventory (the coverage denominator)

The file is 39 lines: 2 `using` directives, a namespace declaration, an 7-line XML summary block
(`:6-12`), the interface declaration (`:13`), three method **declarations** each preceded by an XML
doc block, and closing braces.

| # | Member | Signature | Line span | Body? | Decision points | Executable sequence points |
| --- | --- | --- | --- | --- | --- | --- |
| I1 | `HookItem` | `void HookItem(MailItem mail, Action<MailItem> moveAction);` | declaration `:22`; doc `:15-21` | **No** — terminated by `;` | 0 | **0** |
| I2 | `UnhookItem` | `void UnhookItem(MailItem mail);` | declaration `:31`; doc `:24-30` | **No** | 0 | **0** |
| I3 | `UnhookAll` | `void UnhookAll();` | declaration `:37`; doc `:33-36` | **No** | 0 | **0** |

There are **no** fields, **no** constants, **no** static members, **no** nested types, **no**
properties, **no** events, **no** operators, and **no** attributes.

**Total executable sequence points in this file: 0.**

---

## 3. Existing test inventory

There is **no** test file targeting `IEmailMoveMonitor` as a subject, and none is warranted (§11).

The interface is nevertheless heavily exercised as a **test double** by sibling-owned test files.
Moq can proxy it despite its `internal` accessibility because of
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`
(`QuickFiler/Legacy/IAcceleratorCallbacks.cs:5`, also `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:11`).

| Consumer (test) | Line | Form | Owning sibling |
| --- | --- | --- | --- |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `:333` (doc), `:351` | `new Mock<IEmailMoveMonitor>(MockBehavior.Loose)` | **F11** |
| `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | `:113`, `:140`, `:203` | `new Mock<IEmailMoveMonitor>(MockBehavior.Strict)` | **F2** |
| `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | `:119` | `new Mock<IEmailMoveMonitor>(MockBehavior.Strict)` | **F2** |

The three `MockBehavior.Strict` mocks are the load-bearing constraint: a strict mock fails the test
if the subject calls any member that was not explicitly set up, so **adding a member to this
interface can break sibling-owned tests even if no production call site changes.**

The interface's own behavioral contract — as documented in its XML comments (`:6-12`, `:15-21`,
`:24-30`, `:33-36`) — is verified transitively against the sole implementer:

| Documented contract | Where verified |
| --- | --- |
| "Subscribes to the parent folder's BeforeItemMove event for the first hooked item of that folder" (`:17-18`) | `EmailMoveMonitorTests.cs:88` (E1) |
| "Unsubscribes ... only when the removed item was the last hooked item for that folder" (`:25-27`) | `EmailMoveMonitorTests.cs:108` (E2) |
| "A null argument is a no-op" (`:28`) | `EmailMoveMonitorTests.cs:135` (E3) |
| "Removes all monitored items and unsubscribes every hooked folder's BeforeItemMove event" (`:34-35`) | `EmailMoveMonitorTests.cs:201` (E6) |
| "All Outlook COM member access ... is marshaled to the captured Outlook STA thread, so callers may invoke these members from any thread ... without raising cross-thread COMException" (`:8-11`) | `EmailMoveMonitorTests.cs:177` (E5) and `:267` (E8) |
| Additional contract coverage added by this child | `13-EmailMoveMonitor.md` §11, tests T1–T19 |

---

## 4. Per-member coverage gap

| Member | Status | Notes |
| --- | --- | --- |
| I1 `HookItem` | **not applicable — no executable line** | Declaration only |
| I2 `UnhookItem` | **not applicable — no executable line** | Declaration only |
| I3 `UnhookAll` | **not applicable — no executable line** | Declaration only |

The file has an empty coverage denominator. Any per-file line-coverage figure computed for it is
either `0/0` (undefined) or omitted entirely by the reporting tool, depending on whether AltCover
emits a `<class>` entry for a body-less interface. This is a measurement artifact, not a gap: there
is nothing that a test could execute.

---

## 5. Testability classification per member

| Member | Classification | Interop type / API touched | Mockable with Moq? |
| --- | --- | --- | --- |
| I1 `HookItem` | **not applicable (declaration only)** | Signature references `Microsoft.Office.Interop.Outlook.MailItem` in its parameter list, but the file executes no Interop call | The **interface** is mockable (§3); the declaration itself has nothing to cover |
| I2 `UnhookItem` | **not applicable (declaration only)** | `MailItem` in the parameter list only | as above |
| I3 `UnhookAll` | **not applicable (declaration only)** | none | as above |

The three classifications from the required taxonomy (`pure-testable-now` / `needs-seam` /
`host-bound-irreducible`) all presuppose executable code. None applies. The correct verdict is
**declaration-only / no executable behavior**.

---

## 6. Event-subscription and lifetime invariants

This file declares no events, subscribes to nothing, and holds no state, so it has no
subscription or lifetime invariants of its own. It **documents** the implementer's invariants at
`:17-18`, `:25-27`, `:28`, and `:34-35`; those invariants and their failure modes (leak, double
subscription, double unsubscribe, retention) are enumerated and analysed in `13-EmailMoveMonitor.md`
§6.2 (L1–L7).

**Banned-API audit: CLEAN.** A full read of all 39 lines finds no `Task.Delay`, `Thread.Sleep`,
`DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, or any other banned symbol — the file contains
no executable statement at all.

---

## 7. Interface-file coverage semantics — the disposition for this file

**Verified content check.** Reading `:1-39` in full confirms:

- Every one of the three members is terminated with `;` and has **no** body (`:22`, `:31`, `:37`).
- There is **no default interface member (DIM)**. This is also structurally impossible here:
  `QuickFiler` targets .NET Framework 4.8.1, and DIMs require .NET Core 3.0+ runtime support.
- There is **no** static member, constant, nested type, property, event, or field.
- Everything else in the file is `using` directives, the namespace, XML documentation comments, and
  braces.

**Governing rule.** `.claude/rules/general-unit-test.md` (§ Coverage Requirements) states:

> Type-only / interface-only modules with no executable behavior may be omitted from coverage
> measurement. Examples: ... and C# interface-only files. Such modules legitimately report 0%
> executable coverage and may be excluded from measurement. This is a clarification only; it does
> not lower any coverage threshold.

**Correct disposition: `interface-only — omitted from measurement (zero denominator)`.**

Three consequences the plan must honour:

1. **This is NOT `ratified-exempt`.** The `ratified-exempt` classification in epic.md Shared Design
   §1 and `CLAUDE.md` UT2 is the COM/VSTO/WinForms exemption for code that *has* executable lines
   which cannot be reached without a live host. `IEmailMoveMonitor.cs` has no executable lines at
   all, so it never enters the numerator or the denominator. Recording it as `ratified-exempt` would
   overstate the exemption ledger and imply an irreducible-remainder argument that does not exist.
2. **Do NOT add `[ExcludeFromCodeCoverage]`.** It is unnecessary (nothing to exclude), it would be a
   new attribute in a file the epic is trying to keep attribute-free, and epic.md Shared Design §1
   treats such attributes as suspect until justified.
3. **How to record it in F1's ledger.** Recommended entry for
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`:

   | File | Lines | Classification | Denominator | Rationale |
   | --- | --- | --- | --- | --- |
   | `QuickFiler/Interfaces/IEmailMoveMonitor.cs` | 39 | `interface-only` | 0 sequence points | Three body-less method declarations (`:22`, `:31`, `:37`); no default interface member (impossible on net481); no field, constant, or nested type. Omitted from measurement per `.claude/rules/general-unit-test.md` § Coverage Requirements. Not an exemption and carries no `[ExcludeFromCodeCoverage]`. Behavioral contract verified against the sole implementer `EmailMoveMonitor` (`QuickFiler/Helper Classes/EmailMoveMonitor.cs:18`). |

   epic.md `:112` already anticipates this class of file ("~24 are interface-only declarations with
   no executable behavior"), so F1's ledger schema should have a slot for it distinct from
   `ratified-exempt`. If F1's ledger provides only `testable` / `ratified-exempt`, this child should
   request the third classification rather than force-fit either one; that request is the only
   dependency this file places on F1.

---

## 8. Seam proposal

**None. No seam is proposed, required, or permitted for this file.**

Ranked against the epic's hierarchy (interface seam > injectable delegate > adapter):
`IEmailMoveMonitor` **is** the interface seam. It already isolates the three production consumers
(`QfcQueue`, `QfcDatamodel`, `QfcCollectionController`) from the concrete `EmailMoveMonitor`, and
those consumers already exploit it by injecting `Mock<IEmailMoveMonitor>` in their own tests (§3).
Adding a seam to a seam has no meaning.

Explicitly rejected changes, with rationale:

| Candidate change | Verdict |
| --- | --- |
| Add `UnhookItemAsync` to the interface | **Rejected.** It would break the three `MockBehavior.Strict` mocks in F2- and F11-owned test files (§9) and buys no coverage — `13-EmailMoveMonitor.md` §11 T5–T12 call the member directly on the concrete class via `InternalsVisibleTo`. |
| Add `IDisposable` to the interface | **Rejected.** `UnhookAll` already serves as the teardown contract and is invoked by both production owners (`QfcDatamodel.cs:80`, `QfcCollectionController.cs:1007`). Adding `IDisposable` would force `using`/`Dispose` changes in three sibling-owned files and would trip strict mocks. |
| Widen the interface to `public` | **Rejected.** `internal` is correct per `CLAUDE.md` C#5.2, and Moq already proxies it via `InternalsVisibleTo("DynamicProxyGenAssembly2")`. |
| Split the file or relocate it under `Helper Classes/` | **Rejected.** It would require an edit to `QuickFiler/QuickFiler.csproj:355` and would churn the `using QuickFiler.Interfaces;` line in `QuickFiler/Helper Classes/EmailMoveMonitor.cs:12` and in sibling-owned consumers. No benefit. |

---

## 9. CRITICAL — cross-child conflict analysis

Every file outside F4 scope that references `IEmailMoveMonitor`:

| # | File | Line(s) | Use | Owning sibling |
| --- | --- | --- | --- | --- |
| P1 | `QuickFiler/Controllers/QfcQueue.cs` | `:40` | field type `IEmailMoveMonitor` | **F2** `quickfiler-queue-admission-coverage` (epic.md `:262`) |
| P2 | `QuickFiler/Controllers/QfcQueue.cs` | `:76`, `:130` | `UnhookItem` | **F2** |
| P3 | `QuickFiler/Controllers/QfcQueue.cs` | `:230` | `HookItem` | **F2** |
| P4 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:103` | field type | **F5** `quickfiler-datamodel-coverage` (epic.md `:287`) |
| P5 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:80` | `UnhookAll` | **F5** |
| P6 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:81` | assignment to `null` | **F5** |
| P7 | `QuickFiler/Controllers/QfcDatamodel.cs` | `:357`, `:400`, `:452` | `HookItem` (method group at `:357`) | **F5** |
| P8 | `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` | `:44` | `UnhookItem` | **F5** |
| P9 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:78` | field type | **F11** `quickfiler-collection-controller-coverage` (epic.md `:332`) |
| P10 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:256`, `:284`, `:364`, `:451`, `:1942` | `HookItem` | **F11** |
| P11 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:1007` | `UnhookAll` | **F11** |
| P12 | `QuickFiler/Controllers/QfcCollectionController.cs` | `:1124`, `:1187` | `UnhookItem` | **F11** |
| P13 | `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | `:333`, `:351` | `Mock<IEmailMoveMonitor>` loose | **F11** |
| P14 | `QuickFiler.Test/Controllers/QfcQueueCoverageExpansionTests.cs` | `:113`, `:140`, `:203` | `Mock<IEmailMoveMonitor>` **strict** | **F2** |
| P15 | `QuickFiler.Test/Controllers/QfcQueuePurePathsTests.cs` | `:119` | `Mock<IEmailMoveMonitor>` **strict** | **F2** |

In-scope reference: `QuickFiler/Helper Classes/EmailMoveMonitor.cs:12` (`using QuickFiler.Interfaces;`)
and `:18` (the implements clause) — both F4-owned.

### Conflict statement

**The recommended action for this file is: make no change of any kind.**

- "Requires no sibling-owned file change" — trivially satisfied, because the recommendation is a
  zero-diff disposition plus a ledger entry authored in F4's own feature folder.
- Any additive change to the interface's member set **would** require editing sibling-owned files:
  adding a member forces new `Setup` calls in P14 and P15 (both `MockBehavior.Strict`) and may force
  implementation changes wherever a second implementer is later introduced. There is no alternative
  design that avoids this, which is precisely why the recommendation is to change nothing.
- No `<Compile Include>` change is needed: the entry already exists at
  `QuickFiler/QuickFiler.csproj:355`.
- No `QuickFiler.Test.csproj` change is needed for this file, because no new test file is proposed
  for it (§11).

---

## 10. 500-line compliance

| File | Lines | Limit | Verdict |
| --- | --- | --- | --- |
| `QuickFiler/Interfaces/IEmailMoveMonitor.cs` | 39 | 500 | Compliant; 461 lines of headroom. No split needed, and none is proposed. |

No new production file is proposed for this cluster, so **no `<Compile Include>` line needs to be
added to `QuickFiler/QuickFiler.csproj`** (its `Helper Classes\` block is `:342-354`; the
`Interfaces\IEmailMoveMonitor.cs` entry is `:355`). Were a new production file ever required, that
csproj edit would be a shared-file conflict risk of the same class as the test project's
`<Compile Include>` list.

---

## 11. Recommended test cases

**Enumerated count: 0. No test is recommended for this file.**

Rationale, stated explicitly because the artifact convention asks for coverage across four
categories:

1. The file contains **zero executable sequence points** (§2, §4, §7). There is no line a test could
   execute and therefore no positive, invalid-input, boundary, or error-handling behavior to
   exercise. **The four-category requirement is not applicable to a declaration-only file** — this
   is the same clarification `.claude/rules/general-unit-test.md` makes when it permits such files to
   be omitted from measurement.
2. A reflection-based "the interface declares exactly these three members" test was considered and
   is **rejected**. It would assert a tautology about the compiler's own output, cover no production
   line, and turn every legitimate future interface change into a two-file edit — increasing, not
   reducing, cross-child conflict surface.
3. A "`EmailMoveMonitor` implements `IEmailMoveMonitor`" test is likewise **rejected**: the compiler
   already enforces it at `QuickFiler/Helper Classes/EmailMoveMonitor.cs:18`, and the assertion
   would fail only in a build that does not compile.
4. The interface's **behavioral** contract is fully covered through its sole implementer. The
   existing mapping is in §3; the additional coverage this child adds is
   `13-EmailMoveMonitor.md` §11 T1–T19. Specifically: `HookItem` by E1, E5, E7, T16; `UnhookItem` by
   E2, E3, E4, E5, E7, E8, T14; `UnhookAll` by E5, E6, T13.

If the F4 plan wants a per-file task for this file for bookkeeping symmetry, the appropriate atomic
task is **"record the `interface-only` classification for `QuickFiler/Interfaces/IEmailMoveMonitor.cs`
in the F1 ledger and in `<FEATURE>/evidence/qa-gates/`"** — a documentation task, not a test task.

---

## 12. Projected coverage

**Not applicable: the file has an empty coverage denominator (0 executable sequence points).**

- No test set can raise a `0/0` ratio, and none needs to.
- The file is **not** an 80%-threshold failure and does **not** consume any exemption budget. It is
  omitted from measurement per `.claude/rules/general-unit-test.md` § Coverage Requirements, which
  is a clarification of the measurement denominator and expressly *not* a threshold reduction.
- **Irreducible fraction: not applicable** — there is no reducible or irreducible executable code.
- **No exemption request is raised against F1's ledger.** What is requested from F1 is the
  `interface-only` classification slot described in §7.3, so the capstone F16 can close issue #136's
  "every one of the 121 compiled files is either at >= 80% line coverage or on the ratified
  exemption ledger" criterion for this file without misclassifying it as an exemption.

Confirmation that AltCover emits no non-empty `<class>` entry for
`filename="QuickFiler\Interfaces\IEmailMoveMonitor.cs"` is produced at execution time by F1's
per-file coverage harness and recorded under `<FEATURE>/evidence/qa-gates/`.
