# efc-archiveroot-boundary-sink-defects (Spec)

- **Issue:** #736
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02T13-45
- **Status:** Draft
- **Version:** 0.2

> Work Mode is `full-bug`. Per the acceptance-criteria-tracking skill, this document
> is the **sole authoritative acceptance-criteria source** for issue #736. No user-story document is
> produced for this item.

> **Path-formatting convention (do not "fix" this).** Downstream tooling derives this item's change
> footprint by harvesting backtick-delimited repository-relative path tokens. Therefore **only** the
> `## Write Set` section uses backticks around file paths. Every other file reference in this
> document — context citations, precedents, survey sites, files verified as needing no change, and
> out-of-scope files — is written as bare prose deliberately. Backticks elsewhere are reserved for
> code identifiers, type names, and member names, which are not path tokens.

## Context

Issue #736 consolidates six code-review findings. **This item delivers findings 1, 2, 4, 5, and 6
only.** Finding 3 belongs to a separate item in the same parallel run (see Scope & Non-Goals).

The findings share one boundary: the archive-root value produced by `AppOlObjects.ArchiveRootPath`
and consumed across the Email Folder Chooser (EFC) controller. The getter performs live Outlook COM
reads while composing the arguments it hands to the validation guard, so an undocumented
`COMException` can escape a member whose documented contract admits only `InvalidOperationException`.
Downstream, the EFC controller has a designed fault boundary (`BoundaryErrorSink` plus
`TryReportBoundaryFault`), but the keyboard-dispatch path never reaches it, the breadcrumb bind path
bypasses it, and the default sink implementation only writes a log line.

The research artifact for this item (docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/research/2026-09-02T13-15-efc-archiveroot-boundary-sink-defects-research.md,
"the research") verified every finding at the current worktree HEAD and corrected several claims in
the issue text. Two corrections are load-bearing for this spec and are restated in full below
(findings 2 and 6). All line citations in this document are HEAD line numbers taken from that
research; the issue's own citations are stale because #726 landed unrelated changes to the EFC
controller after the review sweep.

Two file-size facts, reported by research §0 from `Read`-derived line counting (this measurement is
outside the numeric-derivation schema and is therefore used as narrative context only, never as an
acceptance criterion): the EFC controller file is 1216 lines — already far over the repository's
500-line ceiling and declared `internal class`, not `partial` — and AppOlObjects.cs is 494 lines,
leaving roughly six lines of headroom. Both facts constrain **where** the fix may be written, and
are handled in Implementation strategy.

Environment:

- OS/version: Windows 11 Pro (repo default)
- Runtime: C# / .NET Framework 4.8.1 WinForms VSTO add-in with Outlook COM interop
- Command/flags used: n/a — findings originate from code review, verified statically at HEAD
- Data source or fixture: n/a

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Rationale for the severity change from the issue's High: the issue's High rating rests on the claim
that a transient COM failure "can crash the EFC form outright" and that a test "treats the crash as
correct behavior." The research disproved both (see Actual Behavior, findings 2 and 6). The verified
consequence is a **silent, undiagnosed failure**: the exception is absorbed and logged at a
coverage-exempt boundary that reports nothing to the user, and the archive-root getter can emit an
exception type its own contract does not document. That is a real defect in diagnosability and
contract fidelity, and it violates the fail-fast/diagnosable-failure requirements of CLAUDE.md §3 and
the Error Handling and Logging section of the general code-change rule — but it is not a process
crash.

## Repro & Evidence

Steps to Reproduce:

Not reproducible by user action; each finding is a static defect verified against the current
worktree HEAD. See Actual Behavior.

Expected:

Each finding's expected behavior is stated inline below.

Actual:

**Finding 1 — the `ArchiveRootPath` getter performs unguarded live COM reads before validation
(Source: #696). Confirmed as stated.** In AppOlObjects.cs:257-271 the getter evaluates
`Path.Combine(Root.FolderPath, "Archive")` and `ArchiveRoot?.FolderPath` as *arguments* to
`ArchiveRootPathGuard.RequireResolvedArchiveRoot`, so both are evaluated before the guard is entered.
`Root` (AppOlObjects.cs:206-214) is itself a lazy COM read performing `App.Session`, `.DefaultStore`,
`.GetRootFolder()`, then `.FolderPath` — four COM crossings on a cold cache. `ArchiveRoot`
(AppOlObjects.cs:274) resolves through `LoadArchiveRoot` (AppOlObjects.cs:276-280), which constructs
a `FolderPredictor` and calls `GetFolder(Root.Folders, "Archive")`; that method enumerates the root's
child-folder collection with one `.Name` COM read per child. None of this is wrapped in an exception
handler. The getter's XML documentation (AppOlObjects.cs:243-256) declares only
`<exception cref="InvalidOperationException">`; `COMException` is undocumented and unhandled.

Refinement from the research: the interface member declaration in IOlObjects.cs:15 carries **no** XML
documentation at all. The documented `InvalidOperationException` contract exists on the
implementation, not the interface.

**Finding 2 — both `KbdExecuteAsync` overloads lack a try/catch (Source: #695, part A). Structural
claim confirmed; the issue's consequence claim is corrected.** The two overloads are at
EfcFormController.cs:921-925 (`Func<Task>`) and :927-931 (`System.Action`); neither has any local
exception handling. The issue's cited range :894-903 is stale.

The issue escalates this in Impact/Severity to "can crash the EFC form outright." **That is not
supported by the reachable call chain and must not be carried forward.** The research (§1.2) traced
the live wiring: the only `KeyDown` subscription for the EFC form is EfcFormController.cs:435-437,
which targets `KeyboardHandler.KeyboardHandler_KeyDownAsync`. The synchronous
`KeyboardHandler_KeyDown` handler appears in the QuickFiler controllers only inside commented-out
lines; its two live callers are the QFC form viewers, not the EFC viewer.
`KeyboardHandler_KeyDownAsync` (KeyboardHandler.cs:133-148) **does** have a try/catch: it catches
`System.Exception`, writes `logger.Error`, and surfaces nothing to the user. It does not route
through the EFC controller's `BoundaryErrorSink` or `TryReportBoundaryFault`.

**Corrected characterization:** an exception raised by an action dispatched through `KbdExecuteAsync`
is caught and **silently logged at the wrong boundary** — a class decorated
`[ExcludeFromCodeCoverage]` (KeyboardHandler.cs:22), which makes any fix placed there unmeasurable
and untestable in the harness. The defect is an undiagnosed swallow with no user-facing report, not a
crash.

Adjacent finding recorded by the research (§1.2) and deliberately **not** folded into this fix:
`KeyboardHandler.ToggleKeyboardDialogAsync(object, KeyEventArgs)` (KeyboardHandler.cs:238-245) is
`async void` with no try/catch, reached live from the EFC viewer's `ProcessCmdKey` on a bare-Alt
chord. That is a genuine unobserved async-void fault and a separate item (see Rollout & Follow-up).

**Finding 4 — the default `BoundaryErrorSink` is log-only (Source: #697). Substance confirmed; two
supporting details corrected.** EfcFormController.cs:128-129 defaults the sink to
`(message, exception) => logger.Error(message, exception)`. Issue #726 added
`TryReportBoundaryFault` (EfcFormController.cs:138-156), which null-checks the sink and wraps
invocation in its own try/catch; that improved the robustness of sink *invocation* and did not change
the log-only *default*, so finding 4 stands. The issue's claim of "four call sites (lines 456, 473,
491, 553)" is superseded: there are **6** reporter call sites, at lines 483, 500, 518, 580, 595, and
1165 (research §3 N2, primary and cross-check member sets identical).

**Finding 5 — five `_globals.Ol.ArchiveRootPath` reads in the EFC controller (Source: #698). Count
confirmed; the "all five unguarded" characterization is corrected.** There are exactly **5** reads,
at lines 556, 566, 863, 873, and 1014 (research §3 N1, primary and cross-check member sets
identical). The issue's cited lines 529, 539, 836, 846, 987 are stale. Their actual guarding status:

| # | Line | Enclosing member | Local handling |
|---|---|---|---|
| 1 | 556 | `ButtonCreateClickAsync` (:525-582) | `catch (System.Exception)` at :578-581, routed to `TryReportBoundaryFault` |
| 2 | 566 | `ButtonCreateClickAsync` | same catch at :578-581 |
| 3 | 863 | `CreateFolderAsync` (:842-885) | **none** — no try/catch anywhere in the method |
| 4 | 873 | `CreateFolderAsync` | **none** |
| 5 | 1014 | `BindBreadcrumbRowsAsync` (:1007-1024) | `catch (OperationCanceledException)` at :1016 and `catch (System.Exception)` at :1020-1023, `logger.Error` only — bypasses the sink |

Accurate statement: **two of five reads are genuinely unguarded; the other three are guarded only to
the level of a log line, with no user-facing diagnostic.**

**Finding 6 — the success-path test terminates on an incidental crash (Source: #699). The issue's
causal claim is factually wrong and is corrected here.** The issue (#736 finding 6) states that
`MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`
(EfcDataModelArchiveRootTests.cs:172-186) uses a crash tied to an unresolvable archive root as its
pass condition, and that "the `NullReferenceException` it currently expects should no longer occur"
once findings 1 and 5 are fixed.

**Verified state (research §2):** the archive root **resolves successfully** in that test — line 176
is `olObjects.SetupGet(value => value.ArchiveRootPath).Returns(ArchiveRootLiteral);`, which returns a
value and does not throw. The test exercises the *success* path. The `NullReferenceException`
asserted at line 182 is raised several frames downstream, at EmailFiler.cs:133, where
`MailHelpers.FirstOrDefault()!.FolderInfo!.OlFolder!` dereferences a `FolderInfo` that the test's
`TestableEfcDataModel` leaves null. It has nothing to do with archive-root resolution, and it will
**still occur** after findings 1 and 5 land, because those changes touch AppOlObjects.cs and the EFC
controller while this test mocks `IOlObjects` directly and drives `EfcDataModel`.

The now-closed issue **#699** ("efcdatamodel-success-path-test-uses-incidental-crash-as-barrier",
closed as superseded by #736) is the authoritative statement of the real defect: the exception "is
not a property of the unit under test," and once the collaborator stops throwing there, "the test
fails with a message about a missing `NullReferenceException` and points at the wrong subsystem."
#699's expected behavior is that the test "terminates the success path deliberately and asserts only
the invariant it exists to pin, which is
`olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`." #699 also grades the item Low
severity and latent. **This spec adopts #699's framing over #736's literal text.**

Logs / Screenshots:

- [ ] Attached minimal logs or screenshot
- Snippet: n/a. Every citation above was verified at the current worktree HEAD by the research
  artifact; the numeric assertions (5 reads, 6 reporter call sites, 2 overloads, 11 test methods)
  each carry a complete two-derivation record in research §3 (N1-N4).

## Scope & Non-Goals

**In scope:** findings 1, 2, 4, 5, and 6 of issue #736, as corrected in Actual Behavior above,
together with the regression tests that pin them. The complete set of files this change touches is
enumerated in the Write Set section below; that section is the only authoritative footprint.

**Out of scope — finding 3.** Finding 3 of issue #736 (the `ActionOkAsync` hide-before-dispose
ordering in the EFC controller) is **explicitly excluded from this item.** It is owned by a different
item in the same parallel run, covering form-controller disposal ordering. No change in this item may
alter `ActionOkAsync` or any disposal sequencing.

**Binding scope constraints (deliberately written without backticks; see the path-formatting note at
the top).** This item must not touch:

- The Claude runtime tree, the Codex mirror tree, the dot-agents tree, or the two published files in
  the config directory.
- The QuickFiler home-controller metrics files.
- The QuickFiler collection-controller or form-controller disposal files.
- The TaskMaster ribbon surface.

**Must-not-change list (verified files that stay as they are):**

- ArchiveRootPathGuard.cs — its throw contract is frozen. Issue #638's spec excludes it explicitly.
  The fix reuses its rule semantics but does not modify the file.
- AppOlObjectsArchiveRootValidationTests.cs in the TaskMaster test project — must keep passing
  **unmodified**, all six of its test methods.
- The `ArchiveRootPath` member signature declared in IOlObjects.cs — unchanged.
- `EfcDataModel.TryGetArchiveRoot`'s existing `catch (InvalidOperationException)` clause
  (EfcDataModel.cs:280-297) — **must not be widened to `COMException`.** Issue #638 explicitly
  rejected that widening, and the live test
  `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
  (EfcDataModelArchiveRootTests.cs:248-262) pins the opposite behavior by throwing the `COMException`
  from a `Mock<IOlObjects>` — i.e. at the interface seam, above the layer this fix touches. That test
  must stay green with its assertion unchanged.

**File-size non-goal.** The EFC controller file is not split in this item, even though it already
exceeds the repository's 500-line ceiling (1216 lines per research §0) and is declared `internal
class`, not `partial`. That is pre-existing debt tracked separately. This change keeps its addition
as small as possible and calls the pre-existing violation out in the PR description.

**Explicitly excluded systems, integrations, or datasets:** no Outlook COM automation, no live
Microsoft Graph or network access, no data migration, no configuration schema change.

## Root Cause Analysis

Two distinct root causes, not one:

1. **Contract leak at the source (findings 1 and, transitively, 5).** `AppOlObjects.ArchiveRootPath`
   composes its guard arguments from live COM reads inside the getter body. The guard
   `RequireResolvedArchiveRoot` is a pure static helper that cannot defend against a failure occurring
   during *argument evaluation*, because C# evaluates arguments before entering the callee. The
   getter therefore advertises a single failure mode (`InvalidOperationException`) while being able to
   emit a second, undocumented one (`COMException`). Every consumer in the repository was written
   against the documented contract; `EfcDataModel.TryGetArchiveRoot` catches exactly
   `InvalidOperationException` and deliberately lets everything else propagate.

2. **Reporting gap at the boundary (findings 2, 4, and the remaining part of 5).** The EFC controller
   owns a fault-reporting boundary — `BoundaryErrorSink` with the `TryReportBoundaryFault` wrapper —
   which six button/populate paths already use. Three paths do not participate: `CreateFolderAsync`
   has no handler at all and is dispatched through `KbdExecuteAsync`, which also has no handler, so
   its faults land three frames away in a coverage-exempt keyboard class that logs and reports
   nothing; `BindBreadcrumbRowsAsync` has a handler that writes a log line and never reaches the sink;
   and the sink's own default has no user-facing surface, so even the participating paths produce
   nothing a user can act on.

Finding 6 has an unrelated cause: a test whose success-path assertion was anchored to an incidental
collaborator crash rather than to a deliberate stopping point, which makes its future failure message
point at the wrong subsystem.

## Proposed Fix

### Design summary (what changes where)

Guard at the source, in a new sibling partial file, and normalize to the already-documented exception
type; then report at the controller boundary that can actually surface a diagnostic. Concretely:
(1) a COM-guarded, testable archive-root seam added to `AppOlObjects` in a new partial file, with the
getter delegating to it; (2) exception handling on both `KbdExecuteAsync` overloads routed through the
existing `TryReportBoundaryFault`; (3) the breadcrumb bind's general catch rerouted to the same
reporter; (4) a non-blocking user-facing surface for the default sink; (5) a filer-invocation seam on
`EfcDataModel` so the finding-6 test can stop deliberately.

### Boundaries and invariants to preserve

**The invariant this fix establishes, stated as one sentence:**

> `IOlObjects.ArchiveRootPath` either returns a validated, non-null archive-root path, or throws
> exactly `InvalidOperationException` with a redacted message (per issue #602's no-path,
> no-mailbox-address rule) — it never allows an undocumented `COMException` to escape the getter.

Why this specific invariant, and what it buys: every existing consumer already handles
`InvalidOperationException` and only that. `EfcDataModel.TryGetArchiveRoot` (EfcDataModel.cs:280-297)
is the reference case. Normalizing at the source therefore makes every consumer complete **without**
any consumer-side catch being widened. Widening a consumer catch to `COMException` is explicitly out
of bounds: issue #638 rejected it, and
`MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
(EfcDataModelArchiveRootTests.cs:248-262) pins the opposite behavior at the `IOlObjects` mock seam,
which sits **above** the layer this fix changes and is therefore unaffected by it.

**Required trace — one accepted value followed from the guard that accepts it to the boundary that
throws.** The `CreateFolderAsync` / keyboard-'N' path is used because it is the one in-scope path with
no guard anywhere between the accepting checks and the exception's eventual absorption:

1. **Accept point.** The user presses 'N' on the EFC form. `KbdExecuteAsync(CreateFolderAsync)` is
   dispatched (the `'N'` entry is registered at EfcFormController.cs:657 in the async character-action
   map and at :722-726 in the sync map). `CreateFolderAsync` (EfcFormController.cs:842-885) applies
   its own local guards and **accepts**: `IsValidSelection` is checked at :844, and the OneDrive
   `SpecialFolders` lookup at :856-859 returns early only when OneDrive is missing. **Neither guard
   validates the archive root, or touches it at all.** The call proceeds.

2. **Throw point.** With both local guards satisfied, execution reaches
   `_globals.Ol.ArchiveRootPath` at EfcFormController.cs:863 (argument to `FolderHelper.CreateFolder`)
   and again at :873 (argument to `MoveToFolderAsync`). That property is the actual throwing boundary:
   AppOlObjects.cs:257-271 evaluates `Root.FolderPath` and `ArchiveRoot?.FolderPath` across live COM
   before the validation guard is entered, so a transient Outlook failure raises `COMException` here —
   a type the getter's own documentation does not admit.

3. **Current absorption point (wrong boundary).** `CreateFolderAsync` has no local catch.
   `KbdExecuteAsync` (EfcFormController.cs:921-931) has no catch in either overload. The exception
   therefore travels three frames up to `KeyboardHandler.KeyboardHandler_KeyDownAsync`
   (KeyboardHandler.cs:133-148), whose `catch (System.Exception)` writes one `logger.Error` line and
   returns. That class is decorated `[ExcludeFromCodeCoverage]` (KeyboardHandler.cs:22), it is shared
   with the QFC controllers, and it has no access to the EFC controller's sink. Net effect: the user
   sees the folder-creation silently not happen, with no diagnostic and no test able to observe the
   path.

4. **Where the fix moves the catch, and why that boundary can report.** The catch moves into
   `KbdExecuteAsync`'s two overloads, routing through `TryReportBoundaryFault`
   (EfcFormController.cs:138-156) into `BoundaryErrorSink`. That boundary differs from the current one
   in three respects that matter: it is inside `EfcFormController`, which is **not** coverage-exempt
   and is constructible headlessly by the existing `CreateMinimalController()` harness, so the path
   becomes testable; it is the same reporter the six button/populate sites already use, so the
   keyboard path gains parity instead of a parallel mechanism; and finding 4 gives that reporter's
   default a non-blocking user-facing surface, so the fault is reported to the user rather than only
   to a log file.

**Why neither half suffices alone.** Step 2's normalization alone would leave the fault at step 3's
silent boundary, because `CreateFolderAsync` has no consumer-side catch of any exception type — the
exception type is irrelevant when nothing local catches. Step 4's reporting alone would leave the
getter's documented contract wrong for every other consumer in the repository. The prior remediation
failure this trace exists to avoid was precisely the inverse error: two guards were made to agree with
each other while no accepted value was ever followed through to the boundary that throws.

**Other invariants preserved:**

- Failure is **not** cached. The `_archiveRootPath is null` memoization means a failed resolution is
  retried on the next read. A fix that caches a sentinel on failure would be a behavior change and is
  prohibited.
- The redaction rule (#602) holds for the new failure mode: the normalized message names the rule and
  withholds both the path and any mailbox address.
- The diagnostic is logged before the throw, as today.
- Cancellation is not a fault. `catch (OperationCanceledException)` at EfcFormController.cs:1016
  remains unchanged, and `KbdExecuteAsync` adopts the same distinction (see the decision below).

### Dependencies or blocked work

None blocking. This item is downstream of #638 (which froze the guard contract and deferred exactly
these findings), #726 (which introduced `TryReportBoundaryFault`), and #699 (superseded by this
issue). Finding 3 is concurrent in a sibling item; the two must not both edit the same regions of the
EFC controller — this item touches lines around :128-129, :921-931, and :1020-1023 only.

### Implementation strategy (what changes, not sequencing)

1. **Finding 1 — guard the two live COM reads, in a new partial file.** `AppOlObjects` is already
   `public partial class` (AppOlObjects.cs:26) and AppOlObjects.StoreRehook.cs is the existing
   precedent for a sibling partial. Because AppOlObjects.cs has roughly six lines of headroom under
   the 500-line ceiling, the new logic lands in the new partial file and the getter body in
   AppOlObjects.cs changes only to delegate (target: net-neutral or net-negative line count there,
   apart from the XML-doc update). Follow the in-repo idiom already present in the same class:
   `ResolveCurrentUserEmailAddress` (AppOlObjects.cs:360-383) and `TryGetSmtpAddress`
   (AppOlObjects.cs:385-413) — a thin COM-touching wrapper plus a static or delegate-driven core that
   carries the decision logic and is unit-testable without COM. The wrapper catches `COMException`
   from `Root.FolderPath` and from `ArchiveRoot?.FolderPath` and converts it to
   `InvalidOperationException`, preserving the `COMException` as `InnerException` and honoring the
   #602 redaction rule. Update the getter's XML documentation to describe the normalized contract.

2. **Findings 2 and 5 (reads at :863 and :873) — report at the controller boundary.** Add exception
   handling to both `KbdExecuteAsync` overloads (EfcFormController.cs:921-931) routing through the
   existing `TryReportBoundaryFault` (:138-156). This covers `CreateFolderAsync`'s two unguarded reads
   without adding a second, redundant handler inside `CreateFolderAsync`, and it lands in a class that
   is testable and not coverage-exempt.

   **Spec decision (research §5 left this open and required it be decided here):**
   `OperationCanceledException` inside `KbdExecuteAsync` is treated as cancellation, not a fault. Catch
   it first, record it at debug level, and do **not** report it through the sink — matching the
   existing distinction at EfcFormController.cs:1016. All other exceptions route to
   `TryReportBoundaryFault`. In both cases the exception does not propagate out of `KbdExecuteAsync`.
   The handling must also cover a failure raised by `ToggleKeyboardDialogAsync()`, which runs before
   the dispatched action.

3. **Finding 5 (read at :1014) — reroute the breadcrumb catch.** Change
   `BindBreadcrumbRowsAsync`'s `catch (System.Exception ex)` (EfcFormController.cs:1020-1023) to
   report through `TryReportBoundaryFault` instead of a bare `logger.Error`. Leave
   `catch (OperationCanceledException)` at :1016 exactly as it is.

4. **Finding 4 — give the default sink a non-blocking user-facing surface.** Change the default at
   EfcFormController.cs:128-129 so a fault is surfaced to the user, not only logged.
   **A modal `MessageBox.Show` is rejected:** `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`
   (EfcFormControllerTests.cs:282-294) invokes the default delegate directly, in-process, so a modal
   dialog would display in the test host and hang the run. The exact non-blocking mechanism is an
   **implementation decision left to the atomic planner/engineer** — the research does not prescribe an
   API — bounded by two hard constraints: the default must not block the calling thread, and the cited
   test must remain green. Introducing the user-facing surface as a second injectable seam whose
   default stays non-blocking in the test host is an acceptable shape.

5. **Finding 6 — deliberate stopping point via a filer-invocation seam (research §2.4 Option A, which
   is #699's own proposal).** Extract the `new EmailFiler(config)` / `await sorter.SortAsync(mailHelpers)`
   pair (EfcDataModel.cs:343-344) behind a `protected internal virtual` member on `EfcDataModel`, in
   keeping with that class's existing seam style (`UserDiagnosticAction`, and the `protected set`
   accessors on `Globals`, `Token`, `TokenSource`, `FolderHelper`, `ConversationResolver`).
   `TestableEfcDataModel` in the test file overrides it to skip the real filer call. Replace the
   `ThrowAsync<NullReferenceException>` assertion at EfcDataModelArchiveRootTests.cs:182 with a
   deliberate stop, keeping only `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`,
   and update the test's XML summary to describe the deliberate stop rather than the incidental crash.

   **Hard constraint:** the `EmailFilerConfig` object must still be constructed, so that
   EfcDataModel.cs:339 (`OlAncestor = olAncestor,`) **remains covered**. This test is the only one
   reaching that line; #699 records that losing it drops #638's changed-line coverage from 93.10% to
   approximately 89.7%, below the 90% floor for new and changed code in CLAUDE.md's General Unit Test
   Policy UT2. Subclassing `EmailFiler` cannot solve this: `SortAsync(IList<MailItemHelper>)` is not
   virtual, and the null dereference occurs while evaluating the *argument* to `ResolvePaths`, before
   any virtual member is reached.

**Rejected alternatives** (all from research §4.3): guarding at each of the five controller call sites
(duplicates one handler five times, adds roughly forty lines to a file already far over the ceiling,
and leaves every non-EFC consumer of the property unguarded); widening the `EfcDataModel` catch to
`COMException` (rejected by #638 and pinned red by a live test); letting `COMException` propagate to
the `async void` rims (they already catch `System.Exception`, so behavior is unchanged while the
contract stays wrong); fixing finding 2 inside the keyboard handler (coverage-exempt, unmeasurable,
and shared with QFC so the blast radius widens); and for finding 6, supplying a non-null `FolderInfo`
(trades one incidental exception for a different one against a strict mock) or re-documenting without
acting (leaves the misdirecting failure message #699 was raised about).

#### Files/modules to change

See the Write Set section. In prose: the archive-root seam is added to the TaskMaster AppGlobals area
as a new partial file plus a getter delegation; the EFC controller and EFC data model in QuickFiler
are modified; two QuickFiler test files are extended or amended; one new TaskMaster test file is
added; and the two affected legacy project files gain `<Compile Include=...>` entries for the two new
files.

#### Functions/classes/CLI commands impacted

`AppOlObjects.ArchiveRootPath` (getter delegation and XML doc), the new COM-guarded archive-root
members on `AppOlObjects`, `EfcFormController.KbdExecuteAsync` (both overloads),
`EfcFormController.BindBreadcrumbRowsAsync`, `EfcFormController.BoundaryErrorSink` (default value
only), `EfcDataModel.MoveToFolderAsync` (extraction of the filer invocation) plus the new virtual
seam, and `TestableEfcDataModel` in the test file. No CLI surface exists.

#### Data flow and validation changes

The archive-root value flows unchanged on the success path. On the COM-failure path the value is
replaced by a normalized `InvalidOperationException` at the `AppOlObjects` layer instead of a
`COMException`, which routes consumers into their existing failure branches:
`EfcDataModel.MoveToFolderAsync` returns `false`; `OpenOlFolderAsync` and `OpenFsFolderAsync` report
once through `UserDiagnosticAction` and return. Guard ordering is preserved: the COM guard wraps the
*argument evaluation*, so `RequireResolvedArchiveRoot` is never entered with an argument whose
evaluation has already thrown.

#### Error handling and logging updates

The keyboard-dispatch and breadcrumb-bind paths begin reporting through `TryReportBoundaryFault`, so a
fault on those paths is recorded exactly once at the controller boundary. The default sink gains a
non-blocking user-facing report in addition to the existing log call. The normalized
`InvalidOperationException` message names the rule and withholds the path and mailbox address; the
original `COMException` is preserved as `InnerException` for log-level diagnosis.

#### Rollback/feature-flag considerations

None. No feature flag is introduced; the change is small enough to revert as a unit.

### Technical specifications (interfaces/contracts)

#### Inputs/outputs and formats

- `AppOlObjects.ArchiveRootPath` — `string` getter. Returns a validated non-null Outlook folder path;
  otherwise throws `InvalidOperationException` (message names the rule only). The `COMException`, when
  one occurred, is the `InnerException`. Interface signature in IOlObjects.cs is unchanged.
- New archive-root seam — a static or delegate-driven member accepting the resolved inputs (for
  example `Func<string>` accessors for the composed path and the resolved folder path) plus the
  existing diagnostic `Action<string>`, returning the validated path or throwing the normalized
  exception. Shape mirrors `TryGetSmtpAddress` and `EmitPerStoreInboxAttribution`.
- `EfcFormController.KbdExecuteAsync(Func<Task>)` and `KbdExecuteAsync(System.Action)` — return
  `Task`; never throw for a fault raised by the dispatched action or by the keyboard-dialog toggle.
- `EfcDataModel` filer-invocation seam — `protected internal virtual Task<bool>`, returning the
  sorter result; production behavior identical to the current inline call.

#### Required configuration keys and defaults

None. No new configuration keys, resources, or settings entries.

#### Backward-compatibility expectations

No public API signature changes. The only observable behavior change for existing callers is the
exception **type** on the transient-COM path, which moves from an undocumented `COMException` to the
already-documented `InvalidOperationException`. Callers that catch `InvalidOperationException` gain
coverage of a case they previously missed; no caller in the repository catches `COMException` from
this property. Tests that inject a `COMException` at the `IOlObjects` mock seam are unaffected,
because the mock sits above the changed layer.

#### Performance constraints (latency/throughput/memory)

No additional COM round-trips on the success path: the existing `_archiveRootPath` memoization is
preserved unchanged, and the guard still runs once per resolution. Exception handling adds cost only
on the failure path.

## Assumptions, Constraints, Dependencies

- Assumptions: the research artifact's HEAD line citations remain valid for the duration of this item;
  the sibling finding-3 item does not edit the regions listed in Dependencies; the closure of #699 as
  superseded by #736 (reported by the orchestrator, not independently verified in the research
  session) does not change the authority of #699's body as the statement of the finding-6 defect.
- Constraints: MSTest + Moq + FluentAssertions only (CLAUDE.md CUT1-CUT2); no temporary files in
  tests; no live Outlook COM in tests; no new external dependencies; the 500-line file ceiling applies
  to every new file; new logic must not push AppOlObjects.cs over that ceiling; the keyboard handler
  class is coverage-exempt and is therefore not a valid location for the fix.
- External dependencies: none added. Existing log4net, Moq, FluentAssertions, MSTest only.

## Data / API / Config Impact

- User-facing or API changes: EFC faults on the keyboard-dispatch and breadcrumb-bind paths become
  visible to the user through a non-blocking notification instead of being silently logged. No public
  API signature changes.
- Data or migration considerations: none.
- Logging/telemetry updates: fault reports on two additional paths now flow through the controller's
  reporter; the normalized archive-root exception preserves the original `COMException` as
  `InnerException` for log diagnosis; redaction of path and mailbox address is preserved.
- Compatibility notes: no CLI flags, config schemas, or versioned contracts affected. Two legacy
  non-SDK project files require `<Compile Include=...>` entries for the two new source files.

## Test Strategy

Framework is fixed by policy: MSTest with Moq and FluentAssertions, tests mirroring the production
layout, no temporary files, no live COM. Every item below is a defect, so the Bugfix Workflow applies:
**write the failing regression test first**, then the minimal fix, then the full toolchain.

Existing harness that makes this feasible (verified in research §4.1): `CreateMinimalController()`
(EfcFormControllerTests.cs:24-34) constructs the controller through its private no-arg constructor via
reflection, and `SetPrivateField` (:467) injects mocks into private fields.
`AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` (:245-280) and
`PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` (:299-328) already pin all six
reporter call sites, and both inject a custom sink, so they are unaffected by a change to the default.

Regression tests to add or update:

1. **Finding 1 — new test file in the TaskMaster test project's AppGlobals area.** `AppOlObjects`
   cannot be constructed without live Outlook, so the tests target the extracted delegate-driven seam:
   drive it with `Func<string>` delegates that throw `COMException`, and assert that the seam throws
   `InvalidOperationException`, that `InnerException` is the original `COMException`, and that the
   message contains neither a path nor a mailbox address. Add a success case asserting the validated
   path is returned unchanged and no diagnostic is emitted. AppOlObjectsArchiveRootValidationTests.cs
   stays unmodified and must still pass 6/6.
2. **Finding 2 — new tests in the EFC controller test file.** Use `CreateMinimalController()`; the
   all-fields-null state makes the keyboard-dialog toggle throw, which is the same fault-injection
   technique the existing async-void boundary test uses. Assert: does not throw, and the injected sink
   is invoked exactly once. **Cover both overloads** (`Func<Task>` and `System.Action`) — this is a
   two-member family and a single test covers only one of them. Add a cancellation case asserting that
   an `OperationCanceledException` is not reported as a fault.
3. **Finding 4 — regression guard.** Keep `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing`
   (EfcFormControllerTests.cs:282-294) green as the guard against the modal-dialog hazard, and add a
   test asserting the default delegate returns without blocking. Add the two currently untested
   `TryReportBoundaryFault` branches — null sink (:141-145) and throwing sink (:151-155) — since both
   are uncovered today and sit in the neighborhood this change touches, so changed-line coverage will
   require them.
4. **Finding 5 — negative sibling for the breadcrumb path.** Alongside the existing
   `Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` (:61-159), add a test where the
   mocked `ArchiveRootPath` getter throws `InvalidOperationException`; assert the method does not
   throw and the sink is invoked once.
5. **Finding 6 — test rewrite.** Replace the assertion at EfcDataModelArchiveRootTests.cs:182 with the
   deliberate stop from the filer-invocation seam, keeping only the `VerifyGet(..., Times.Once())`
   assertion, and update the test's summary comment. Re-measure coverage and confirm
   EfcDataModel.cs:339 remains covered. All **11** test methods in that class must stay green
   (count derived in research §3 N4).

Edge cases and negative scenarios: null `action` argument to either `KbdExecuteAsync` overload; a null
`BoundaryErrorSink`; a `BoundaryErrorSink` that throws; `OperationCanceledException` inside
`KbdExecuteAsync`; a COM failure on the composed-path read versus on the resolved-folder read
(distinct delegates, both must normalize); repeated reads after a failure (the failure must not be
cached).

Error handling and logging verification: assert the redacted message content directly (no path
substring, no mailbox-address substring), assert `InnerException` identity, and assert the sink is
invoked exactly once per fault (not zero, not twice).

Coverage impact and targets: new and changed code must reach at least 90% line coverage per CLAUDE.md
General Unit Test Policy UT2, and coverage on changed lines must not regress. EfcDataModel.cs:339 must
remain covered. No merge-base coverage baseline exists in this feature folder yet, so the repository
level line-coverage figure is a **record-and-report** obligation for this item — capture it against
the testable denominator defined by UT2's COM/VSTO/WinForms exemption, state whether the change lowers
it, and do not treat the raw uninstrumented figure as a blocking gate.

Toolchain commands to run, in order, restarting from step 1 on any failure or auto-fix:

1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe` against the built test assemblies, with `/EnableCodeCoverage`

Both msbuild steps must be proven non-vacuous: run each with a file logger (the `/fl` switch plus
`/flp` at normal verbosity, writing into this feature folder's evidence subdirectory) and assert
**zero** occurrences of the literal `Skipping target "CoreCompile"` in the log. Exit code 0 alone does
not distinguish a real compile from a skipped one. Local test runs need both the worktree exclusion
filter for the Claude worktrees directory and CI's `/InIsolation`; empty-message sub-millisecond
failures indicate an assembly-load problem, not a regression.

All toolchain, coverage, and regression evidence for this item is written under this feature folder's
evidence subdirectory, partitioned by kind, per the evidence-and-timestamp-conventions skill. No
evidence is written to any top-level artifacts directory.

Manual validation steps: none required; every path in scope is reachable from the headless harness.

## Acceptance Criteria

- [x] **AC1 (finding 1 — guarded seam).** A new archive-root partial file exists in the TaskMaster
      AppGlobals area containing a COM-guarded read plus a delegate-driven testable core following the
      `ResolveCurrentUserEmailAddress` / `TryGetSmtpAddress` shape; `AppOlObjects.ArchiveRootPath`
      delegates to it; AppOlObjects.cs remains under the 500-line ceiling; and the new file is
      registered in the TaskMaster project file.
      (Discharged. Conjunct 1 — the file `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` exists,
      declaring the COM-touching wrapper `internal string ResolveValidatedArchiveRootPath()` and the
      delegate-driven core `internal static string ResolveValidatedArchiveRootPath(Func<string>,
      Func<string>, Action<string>)`. Conjunct 2 — `TaskMaster/AppGlobals/AppOlObjects.cs` contains
      exactly one line `_archiveRootPath = ResolveValidatedArchiveRootPath();` in the
      `ArchiveRootPath` getter, delegating to the new seam. Conjunct 3 — AppOlObjects.cs is 493 lines
      after the formatting pass, under the 500-line ceiling, evidenced by
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t3-file-sizes.md`,
      which also records the new file at 95 lines. Conjunct 4 — `TaskMaster/TaskMaster.csproj`
      contains exactly one element whose `Include` attribute value is
      `AppGlobals\AppOlObjects.ArchiveRoot.cs`.)
- [x] **AC2 (finding 1 — normalization contract).** A `COMException` raised while evaluating either
      archive-root read is converted to `InvalidOperationException` with the `COMException` preserved
      as `InnerException`; the message names the rule and contains neither a filesystem/Outlook path
      nor a mailbox address (#602); the getter's XML documentation states the normalized contract; and
      a failed resolution is not cached (a subsequent read retries).
      (Discharged. Recorded failing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t7-finding1-red.md`
      — total 6, passed 2, failed 4 against the defect-preserving seam. Recorded passing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t9-finding1-green.md`
      — total 6, passed 6, failed 0, which also carries P1-T8's own observations because that task is
      a source edit writing no artifact of its own. The XML-documentation conjunct is delivered by
      P1-T3, likewise a source edit writing no artifact, so its citation is the delivered text:
      `TaskMaster/AppGlobals/AppOlObjects.cs` contains exactly one line whose text after the `/// `
      prefix is, verbatim, `A transient COM failure is normalized to InvalidOperationException
      carrying the original COMException as InnerException.`, inside the XML documentation of the
      `ArchiveRootPath` getter. The retry conjunct is discharged by
      `ResolveValidatedArchiveRootPath_WhenComReadFailsTwice_ReReadsTheComposedPathOnTheSecondCall`,
      which asserts the composed-path delegate was invoked twice across two calls.)
- [x] **AC3 (invariant stated and traced).** This spec's Boundaries and invariants section states the
      `ArchiveRootPath` invariant as a single explicit sentence, and traces one accepted value through
      four numbered steps — accept point (`CreateFolderAsync` local guards at EfcFormController.cs:844
      and :856-859, neither validating the archive root), throw point (the reads at :863 and :873
      resolving to AppOlObjects.cs:257-271), current absorption point
      (`KeyboardHandler.KeyboardHandler_KeyDownAsync`, KeyboardHandler.cs:133-148, a
      `[ExcludeFromCodeCoverage]` class that logs only), and the boundary the fix moves the catch to
      with the reason it can report to the user. The delivered implementation matches that trace.
      (Discharged. The delivered `RunKbdGuardedAsync` handling in
      `QuickFiler/Controllers/EfcFormController.cs` is the boundary named in step 4 of the trace: both
      `KbdExecuteAsync` overloads route their two statements through it, so a fault raised by the
      keyboard-dialog toggle at the reads on :863 and :873 is caught there and reported through
      `TryReportBoundaryFault` rather than travelling three frames up into the
      `[ExcludeFromCodeCoverage]` keyboard handler that logs only. Recorded failing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p2-t8-finding2-red.md`
      — total 6, passed 0, failed 6 against the unguarded seam. Recorded passing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p2-t10-finding2-green.md`
      — total 6, passed 6, failed 0, which also carries P2-T9's own observations because that task is
      a source edit writing no artifact of its own.)
- [x] **AC4 (finding 2 — keyboard boundary).** Both `KbdExecuteAsync` overloads
      (EfcFormController.cs:921-931) handle exceptions locally and route them through
      `TryReportBoundaryFault`; neither propagates a fault raised by the dispatched action or by the
      keyboard-dialog toggle; `OperationCanceledException` is treated as cancellation and is not
      reported as a fault. Exactly **2** overloads exist and both are covered by tests (count derived
      in research §3 N3).
      (Discharged. The two-overload declaration set is established by
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t9-preexisting-counts.md`,
      which records the `KbdExecuteAsync` declaration set as exactly {921, 927}; P2-T5 rewrote both to
      delegate through `RunKbdGuardedAsync` and left the count at 2. Recorded passing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p2-t10-finding2-green.md`
      — total 6, passed 6, failed 0, covering both overloads under a faulting toggle and both
      classification arms, `OperationCanceledException` reporting nothing and every other exception
      reporting exactly once. The two success-path overload tests added by P6-T13 are recorded green
      in
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p6-t13-kbd-success-path.md`.)
- [x] **AC5 (finding 4 — user-facing sink default).** The default `BoundaryErrorSink`
      (EfcFormController.cs:128-129) surfaces a fault to the user through a non-blocking mechanism in
      addition to logging; no modal dialog is invoked from the default;
      `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` passes without hanging; and the null
      sink and throwing sink branches of `TryReportBoundaryFault` are covered by tests.
      (Discharged. Recorded failing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p4-t4-finding4-red.md`
      — total 3, passed 2, failed 1, the single failure being
      `BoundaryErrorSink_DefaultDelegate_RoutesThroughTheUserFaultNotifier` with a capture of 0
      messages where 1 was expected, which is the log-only default finding 4 names. Recorded passing
      run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p4-t6-finding4-green.md`
      — total 3, passed 3, failed 0, recording each method's TRX-reported duration as under one
      second, which is the evidence that the default surface did not block the test host; it also
      carries P4-T5's own observations because that task is a source edit writing no artifact of its
      own, including that the file's `MessageBox` occurrence count is unchanged from the P0-T9
      baseline, so no modal dialog is reachable from the default. AC5's fourth conjunct — that the
      null sink and throwing sink branches of `TryReportBoundaryFault` are covered — is discharged by
      the two tests `KbdExecuteAsync_WhenBoundaryErrorSinkIsNull_DoesNotThrow` and
      `KbdExecuteAsync_WhenBoundaryErrorSinkThrows_DoesNotThrow`, authored in P2-T2 and recorded green
      in
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p2-t10-finding2-green.md`,
      which neither of the two artifacts above records.)
- [x] **AC6 (finding 5 — all five reads accounted for).** All **5** `_globals.Ol.ArchiveRootPath`
      reads in the EFC controller (lines 556, 566, 863, 873, 1014; member set derived in research §3
      N1) are covered by a reporting boundary: 556 and 566 through the pre-existing
      `ButtonCreateClickAsync` catch at :578-581; 863 and 873 through the new `KbdExecuteAsync`
      handling from AC4; and 1014 through `BindBreadcrumbRowsAsync`'s general catch (:1020-1023)
      rerouted to `TryReportBoundaryFault`, with `catch (OperationCanceledException)` at :1016 left
      unchanged.
      (Discharged. The five-read set is established as exactly {556, 566, 863, 873, 1014} by
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/baseline/p0-t9-preexisting-counts.md`.
      Mapping each to the reporting boundary that now covers it: 556 and 566 are covered by the
      pre-existing `ButtonCreateClickAsync` catch, which this item does not change; 863 and 873 are
      covered by the new `RunKbdGuardedAsync` containment both `KbdExecuteAsync` overloads route
      through, recorded green in
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p2-t10-finding2-green.md`;
      and 1014 is covered by `BindBreadcrumbRowsAsync`'s general catch, rerouted from a bare
      `logger.Error` to `TryReportBoundaryFault` by P3-T3 with the
      `catch (OperationCanceledException)` arm above it left byte-identical, recorded green in
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p3-t4-finding5-green.md`
      — total 2, passed 2, failed 0.)
- [x] **AC7 (finding 6 — deliberate stop, restated per #699).** `EfcDataModel`'s filer invocation
      (EfcDataModel.cs:343-344) is extracted behind a `protected internal virtual` seam;
      `TestableEfcDataModel` overrides it to skip the real filer call; the
      `ThrowAsync<NullReferenceException>` assertion at EfcDataModelArchiveRootTests.cs:182 is
      replaced by a deliberate stop retaining only
      `olObjects.VerifyGet(value => value.ArchiveRootPath, Times.Once())`; and the test's summary
      comment no longer describes the incidental crash as the barrier.
      (Discharged. Recorded failing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p5-t2-finding6-red.md`
      — total 1, passed 0, failed 1, the failure naming `NullReferenceException`, which is the
      incidental collaborator crash the rewrite exists to stop depending on. Recorded passing run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p5-t5-finding6-green.md`
      — total 11, passed 11, failed 0 across the whole class. P5-T1, P5-T3 and P5-T4 are source edits
      writing no evidence artifact of their own: P5-T1's observations are carried by the P5-T2
      artifact and P5-T3's and P5-T4's by the P5-T5 artifact, so those two paths carry the evidence of
      all four AC7-delivering tasks. The seam is `protected internal virtual Task<bool>
      InvokeFilerAsync(EmailFilerConfig, IList<MailItemHelper>)` and `TestableEfcDataModel` overrides
      it to return a completed true result.)
- [x] **AC8 (finding 6 — coverage preservation).** EfcDataModel.cs:339 (`OlAncestor = olAncestor,`)
      remains covered by at least one passing test after the rewrite, evidenced by a coverage report
      captured under this feature folder's evidence subdirectory. All **11** test methods in the
      archive-root data-model test class pass (count derived in research §3 N4).
      (Discharged. The coverage evidence is
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t10-olancestor-coverage.md`,
      which records that exactly three lines of `QuickFiler/Controllers/EfcDataModel.cs` match the
      literal — at post-change lines 339, 380 and 404, enclosed in file order by the five-parameter
      `MoveToFolderAsync` overload, `OpenOlFolderAsync` and `OpenFsFolderAsync` — and that the first,
      line 339, carries `hits="1"` in the post-change Cobertura document under the key
      `QuickFiler\Controllers\EfcDataModel.cs`. The single test reaching it is
      `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`. The eleven-method conjunct is
      discharged by
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p5-t5-finding6-green.md`
      — total 11, passed 11, failed 0.)
- [x] **AC9 (frozen contracts hold).** ArchiveRootPathGuard.cs is unmodified;
      AppOlObjectsArchiveRootValidationTests.cs is unmodified and passes 6/6; the `ArchiveRootPath`
      member signature in IOlObjects.cs is unchanged;
      `EfcDataModel.TryGetArchiveRoot`'s `catch (InvalidOperationException)` is **not** widened to
      `COMException`; and `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
      (EfcDataModelArchiveRootTests.cs:248-262) passes with its assertion unchanged.
      (Discharged.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p1-t10-frozen-contracts.md`
      records an empty name-only diff over ArchiveRootPathGuard.cs,
      AppOlObjectsArchiveRootValidationTests.cs and IOlObjects.cs against a non-empty staged index, so
      the three frozen files are provably unmodified rather than merely unreported.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p5-t6-com-propagation-unchanged.md`
      records that no diff hunk intersects the source span of
      `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates` and quotes that method's
      assertion line verbatim.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t11-frozen-contracts.md`
      records the post-change runs: AppOlObjectsArchiveRootValidationTests at 6/6/0 and the
      COM-propagation test passing.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/regression-testing/p5-t5-finding6-green.md`
      is the artifact discharging AC9's remaining conjunct, that
      `EfcDataModel.TryGetArchiveRoot`'s `catch (InvalidOperationException)` is not widened to
      `COMException`: P5-T3 makes that observation — one line matching
      `catch (InvalidOperationException ex)` and zero lines matching `catch (COMException` in
      `QuickFiler/Controllers/EfcDataModel.cs` — and P5-T5 is the artifact recording it, none of the
      other three artifacts speaking to it.)
- [x] **AC10 (regression-first evidence).** For each of findings 1, 2, 4, 5, and 6 a regression test
      is recorded as failing before its fix and passing after, per the Bugfix Workflow; each test's
      file path and method name is listed in the delivery report.
      (Discharged. The delivery report is
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/other/p7-t1-delivery-report.md`,
      which names all five findings and, for each, the regression test file path, every test method
      name, and the evidence artifact paths of the recorded failing and passing runs. The union of the
      method names it lists is nineteen. Three of them — the success-path tests added by P6-T13 — have
      no failing run, because P6-T13 changes no production code and a run that fails first is
      structurally impossible; that artifact records the reason in a `WhyFailingRunImpossible:` field
      per the fail-before exception convention, and no acceptance criterion here is discharged by a
      fail-before observation on those three. Every other test carries both a recorded red and a
      recorded green.)
- [x] **AC11 (scope containment).** No change is made to `ActionOkAsync` or to any disposal ordering
      (finding 3, owned by a sibling item); no change is made to any file listed in the binding scope
      constraints of Scope & Non-Goals; the EFC controller file is not split; and the delivered diff
      touches only the files enumerated in the Write Set section.
      (Discharged. **The delivered footprint is the eleven-path Write Set ratified in this spec at
      2026-09-02T14-10.**
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p7-t3-scope-containment.md`
      records that the anchored merge-base name-only diff, excluding the documentation and Claude
      trees, names exactly those eleven paths sorted and nothing else, and that no line of the
      accompanying `git status --porcelain` span names a path under `TaskMaster/`, `TaskMaster.Test/`,
      `QuickFiler/` or `QuickFiler.Test/`.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p7-t4-exclusions.md`
      records that `ActionOkAsync` occupies lines 838 through 872 and that no hunk of the anchored
      controller diff intersects that span, that the file's single added `.Dispose()` occurrence lies
      inside `ShowModelessFaultNotice` as a self-disposing notification form rather than a
      disposal-ordering change, and that the second anchored diff over the binding-scope exclusions
      printed no lines.
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t3-file-sizes.md`
      records that `QuickFiler/Controllers/EfcFormController.cs` remains a single file of 1320 lines,
      within the D7 budgeted ceiling of 1330, so it was not split.)
- [x] **AC12 (coverage targets).** New and changed code reaches at least 90% line coverage per
      CLAUDE.md General Unit Test Policy UT2, coverage on changed lines does not regress, and the
      repository-level figure is recorded and reported against UT2's testable denominator with a
      statement of whether this change lowers it.
      (Discharged, with the changed-line arithmetic reported explicitly rather than absorbed.
      **New file** —
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t8-newfile-coverage.md`
      records `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` at 18 covered over 18 coverable,
      **100.00%**, against the 90% floor, after removing the mechanically derived lifted-lambda set
      `L` = {89, 90, 91}; its strict figure is 18/21 = 85.71% and both are recorded side by side.
      **Changed lines** —
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t9-changed-line-coverage.md`
      records 59 changed coverable lines across the four production files, 52 covered, a strict
      aggregate of **88.14%**, which is below 90.00%. That is the `10U` escape branch D2 identifies
      and expects, not an implementation defect: the unreachable set `U` has 7 members enumerated in
      advance, `10U` is 70, and the strict denominator of 59 is below it, so the strict quotient
      cannot reach 90.00% whatever the tests do. The escape's precondition is satisfied and recorded —
      **the count of uncovered changed coverable lines lying outside `U` is 0**, with no member to
      name, so every reachable changed line is covered and the lenient figure with exactly those 7
      lines excluded is 100.00%. This condition is reported to the caller per D2 rather than resolved
      by excluding any further line. **No regression on changed lines**: the controller file's 33
      changed coverable lines are all covered, five of them closed by P6-T13, and no changed line that
      was covered before is uncovered now. **Repository level** —
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t7-coverage-delta.md`
      records line coverage moving from 85.43% to 85.46% and branch coverage from 79.53% to 79.52%,
      and states explicitly that this change does not lower the repository-wide line figure.)
- [x] **AC13 (full toolchain pass).** A single final pass of csharpier format/check, the analyzer
      msbuild rebuild, the nullable-warnings-as-errors msbuild rebuild, and vstest with code coverage
      completes with no failures and no auto-fixes, using the exact commands in Test Strategy. Both
      msbuild steps are proven non-vacuous by zero occurrences of `Skipping target "CoreCompile"` in
      their `/fl` logs, with the logs retained as evidence.
      (Discharged by a single final pass, run after the toolchain-loop restart P6-T13 triggered.
      Format:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t1-format.md`
      — exit 0, `Formatted 1580 files in 2390ms.`, with the `git status --porcelain` spans taken
      immediately before and after mechanically identical, so **no file was auto-fixed**. Check:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t2-format-check.md`
      — exit 0, `Checked 1580 files in 5613ms.` Analyzer rebuild:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer-rebuild.md`
      — exit 0, 0 warnings, 0 errors, **0** occurrences of `Skipping target "CoreCompile"` and 18 of
      `Task "Csc"`. Nullable rebuild:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable-rebuild.md`
      — exit 0, 0 warnings, 0 errors, **0** and 18 on the same two literals. Coverage run:
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t6-coverage.md`
      — 7013 total, 7013 passed, **0 failed**. The two retained msbuild logs are
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t4-analyzer.min.log.txt`
      and
      `docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/evidence/qa-gates/p6-t5-nullable.min.log.txt`.
      Those two are the citation for AC13's final conjunct specifically — "with the logs retained as
      evidence" — which none of the five markdown artifacts above discharges, since each of those
      records counts read *from* a log rather than being the retained log itself. Both are tracked by
      git and entered the delivery commit, so retention is established rather than mere production.)

## Write Set

Every file this item's diff creates, modifies, or deletes:

- `TaskMaster/AppGlobals/AppOlObjects.ArchiveRoot.cs` (new)
- `TaskMaster/AppGlobals/AppOlObjects.cs` (modified)
- `TaskMaster/TaskMaster.csproj` (modified — registers the new partial file)
- `QuickFiler/Controllers/EfcFormController.cs` (modified)
- `QuickFiler/Controllers/EfcDataModel.cs` (modified)
- `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` (modified — gains the `partial` keyword; no new test methods land here)
- `QuickFiler.Test/Controllers/EfcFormControllerTests.Part2.cs` (new — sibling partial carrying the new `KbdExecuteAsync`, sink-default, and `TryReportBoundaryFault` branch tests, reusing `CreateMinimalController()` and `SetPrivateField` from the existing file)
- `QuickFiler.Test/QuickFiler.Test.csproj` (modified — registers the new sibling partial test file)
- `QuickFiler.Test/Controllers/EfcDataModelArchiveRootTests.cs` (modified)
- `TaskMaster.Test/AppGlobals/AppOlObjectsArchiveRootComGuardTests.cs` (new)
- `TaskMaster.Test/TaskMaster.Test.csproj` (modified — registers the new test file)

No path in this set contains a space. Both TaskMaster project files and QuickFiler.Test's project file
are legacy non-SDK projects requiring explicit `<Compile Include=...>` entries, verified by their
existing entries for the current archive-root source and test files. This planning document and the
research and evidence artifacts under this feature folder are not part of the code diff and are
deliberately excluded from this set.

**Amendment (2026-09-02T14-10, ratified by the orchestrator from atomic-planner's blocked adversarial
self-review):** `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` is 485 lines against the
500-line ceiling; the nine new test methods required by Test Strategy items 2, 3, and 4 do not fit.
The two rows added above (a new sibling partial file plus the project-file registration) are the
minimal amendment. `EfcFormControllerTests.cs` itself changes only by gaining the `partial` keyword —
no new test method is added to it directly.

## Risks & Mitigations

Technical or operational risks:

1. **The finding-4 surface blocks the test host.** A modal or otherwise blocking default sink hangs
   the vstest run through the test that invokes the default delegate directly. Mitigation: the
   non-blocking constraint is a hard acceptance criterion (AC5), and the existing test is retained as
   the regression guard.
2. **Coverage loss at EfcDataModel.cs:339.** A finding-6 rewrite that bypasses config construction
   silently drops the only test reaching that line, taking #638's changed-line figure below the 90%
   floor. Mitigation: the chosen seam sits after config construction, and AC8 requires positive
   coverage evidence rather than an assertion of intent.
3. **Concurrent edits with the sibling finding-3 item.** Both items modify the same controller file.
   Mitigation: this item's edits are confined to three narrow regions (:128-129, :921-931,
   :1020-1023), none of which is in `ActionOkAsync` or its disposal path; AC11 pins the containment.
4. **Stale line citations.** The controller file changed under #726 after the review sweep, and could
   change again. Mitigation: every citation here is from HEAD-verified research; implementers should
   re-anchor by member name if a line number no longer matches, and report the drift.
5. **Over-correcting the exception contract.** An implementer may be tempted to widen a consumer catch
   to `COMException` to "be safe." Mitigation: AC9 makes that a failing condition, and a live test
   pins the opposite behavior at the mock seam.
6. **Scope creep into the keyboard handler.** The adjacent async-void gap there is tempting to fix in
   passing. Mitigation: it is recorded as a follow-up below, and the coverage exemption on that class
   makes any fix there unverifiable.

Mitigations and rollbacks: the change is revertible as a single commit; no data or configuration
migration is involved, so rollback is a code revert with no cleanup.

## Rollout & Follow-up

Release/rollout steps: standard branch, PR, and merge. The PR description must call out the
pre-existing 500-line-ceiling violation in the EFC controller file, state that this item deliberately
does not split it, and record the exception-type change on the transient-COM path as a behavior
change (from an undocumented `COMException` to the documented `InvalidOperationException`).

Post-fix monitoring or clean-up tasks:

1. **Open a follow-up issue** for the async-void gap at KeyboardHandler.cs:238-245
   (`ToggleKeyboardDialogAsync(object, KeyEventArgs)` has no try/catch, unlike its sibling
   `KeyboardHandler_KeyDownAsync`, and is reached live from the EFC viewer's `ProcessCmdKey` on a
   bare-Alt chord). This is a genuine unobserved async-void fault verified by research §1.2. It is not
   finding 2 and must not be folded into this fix.
2. **Track the EFC controller file split** as separate debt; it is over the ceiling and is not
   `partial`, so relieving it requires a declaration change.
3. **Confirm the other repository consumers** of `ArchiveRootPath` (the folder predictor, folder
   converter, mail-item helper, existing-folder sorter, QFC item controller, sort-email path, and
   meeting-item helper) behave correctly under the normalized exception type; they were surveyed but
   not changed by this item.

Links:

- Issue #736 (this item) — findings 1, 2, 4, 5, 6.
- Issue #699 — authoritative statement of the finding-6 defect; closed as superseded by #736.
- Issue #638 — froze the archive-root guard contract and deferred findings 1, 2, 4, 5 to this issue.
- Issue #726 — introduced `TryReportBoundaryFault`.
- Issue #602 — the redaction rule for archive-root diagnostics.
- Research artifact for this item, under this feature folder's research subdirectory
  (2026-09-02T13-15).
