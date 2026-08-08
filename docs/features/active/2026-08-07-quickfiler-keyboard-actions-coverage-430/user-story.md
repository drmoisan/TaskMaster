# `quickfiler-keyboard-actions-coverage` — User Story

- Issue: #430
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F3, wave 1)
- Owner: drmoisan
- Status: Specified
- Last Updated: 2026-08-07T22-30
- Work Mode: full-feature (AC sources: `spec.md` **and** `user-story.md`)
- Companion document: `spec.md` in this folder carries the technical detail — the K1–K5 seam design,
  the cross-child contract note, the F1 dependencies, and the out-of-scope latent defects.

## Story Statement

- **As the TaskMaster maintainer**, I want the QuickFiler keyboard-handling cluster to be covered by
  deterministic unit tests rather than shielded by an `[ExcludeFromCodeCoverage]` attribute, so that
  a regression in key routing, filter accumulation, or drop-down navigation is caught by the test
  suite instead of by a user pressing a key in Outlook.
- **As a sibling epic child (F6–F11, F15)**, I want F3's testability work to leave
  `IQfcKeyboardHandler` and every construction site untouched, so that my own files continue to
  compile and my `MockBehavior.Strict` mocks continue to pass while F3 executes in the same parallel
  wave.
- **As the F16 capstone reviewer**, I want each of the 11 files in this cluster to carry a numeric,
  reproducible per-file coverage figure — or an explicit `N/A` with a stated reason — so that the
  epic's "all 121 compiled files accounted for" check can actually close.
- **As a future agent maintaining this code**, I want the cluster's surprising behaviors documented
  by executable characterization tests that cite their tracking issues, so that I can tell the
  difference between behavior that is deliberate and behavior that is a known defect awaiting a fix.

## Problem / Why

Epic #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach at
least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child owns the
QuickFiler keyboard-handling and mail-item-action cluster: 11 compiled files totalling roughly 1,025
lines.

The cluster's central file, `QuickFiler/Controllers/KeyboardHandler.cs` (414 lines), carries
`[ExcludeFromCodeCoverage]` at line 22 and **has no tests at all** — no test anywhere in the
repository constructs the type. All 17 test files that reference it use `Mock<IQfcKeyboardHandler>`,
which contributes zero coverage to the concrete class. Every keystroke path in QuickFiler — the
string-filter accumulator, the always-on key registry, the breadcrumb arrow fall-through, and the
folder drop-down routing — is currently unprotected by any test.

The attribute is not justified. The file declares `using Microsoft.Office.Interop.Outlook;` at line
15, but **no member in the file references any Outlook Interop type**; that unused directive is the
most plausible reason the file was ever labelled COM-bound. Under the epic's ratified reconciliation
(`epic.md` Shared Design §1), the `CLAUDE.md` § UT2 qualifier "without an injectable seam" is a live
obligation rather than standing permission, so an `[ExcludeFromCodeCoverage]` attribute on a testable
seam is a Blocking finding.

Separately, `CLAUDE.md` § UT2 names `KbdActions<>` explicitly as a testable seam within an otherwise
COM-bound assembly that is **not** exempt and must meet the coverage floor. (Correction: `issue.md`
lines 31–33 also attribute this clause to `.claude/rules/csharp.md`; verification found no occurrence
of `KbdActions` in that file. The obligation stands; the citation is `CLAUDE.md` § UT2 only.)

## Personas & Scenarios

### Persona — the TaskMaster maintainer

- **Who:** the sole maintainer of a legacy VSTO/.NET Framework 4.8.1 Outlook add-in, working through
  autonomous agents rather than by hand.
- **What they care about:** that agentic changes to QuickFiler are safe. Coverage is not a vanity
  metric here; it is the mechanism that makes autonomous maintenance trustworthy.
- **Constraints:** COM and WinForms make parts of the code genuinely hard to test, and the project
  has a history of exemptions being applied to code that turned out to be testable — the
  `MailItemActionsAdapter` COM-barrier claim was already adjudicated as false once, during the issue
  #227 cycle-2 work.
- **Goals and frustrations:** wants every exemption to be earned and reviewable, and does not want a
  child feature to buy a green number by writing tests that assert nothing.

### Persona — a sibling epic child executing in the same wave

- **Who:** one of the thirteen other wave-1 children (F2, F4–F15), each editing a disjoint set of
  QuickFiler production files against the same integration branch.
- **What they care about:** that F3 does not change a shared contract underneath them.
  `IQfcKeyboardHandler` is consumed by 20 production locations across F6, F7, F8, F9, F10, F11, and
  F15, and by 17 test files.
- **Constraints:** four of those test files use `MockBehavior.Strict`, so even *adding* an interface
  member would break them at run time when the strict mock encountered an unconfigured call.
- **Goals and frustrations:** wants a clean fan-in merge and no rebase conflict caused by F3
  reaching into a file it does not own.

### Scenario 1 — the maintainer reviews the de-exemption

The maintainer opens the F3 pull request. The diff shows `[ExcludeFromCodeCoverage]` removed from
`KeyboardHandler.cs:22` and three unused `using` directives removed alongside it. Two new small files
appear under `QuickFiler/Interfaces/` — an interface and its one-line adapter. The public
constructors gained optional trailing parameters, and both existing two-argument call sites are
untouched in the diff. The evidence folder contains a per-file coverage table produced by F1's
harness showing `KeyboardHandler.cs` moved from unmeasured to a figure above the 80% floor, and the
four interface-only files reported as `N/A` rather than `0%`. The maintainer can see, without running
anything, exactly what was covered and what — if anything — remains exempt and why.

### Scenario 2 — a sibling child rebases after F3 merges

An F10 agent working on `QfcItemController` rebases onto the integration branch after F3 merges. The
`KeyboardHandler` seam work does not appear in any file F10 owns. `IQfcKeyboardHandler` is unchanged,
so F10's `Mock<IQfcKeyboardHandler>(MockBehavior.Strict)` setups compile and pass exactly as before.
The only file F10 and F3 both touch is `QuickFiler.Test/QuickFiler.Test.csproj`, where F3's new
`<Compile Include>` entries were appended adjacent to the existing block, keeping the conflict hunk
small and mechanically resolvable.

### Scenario 3 — a future agent meets a surprising behavior

Six months later an agent reads `KaStringAsync.KeyEquals` and notices that line 72 invokes `Update`
without the `Activated` gate that lines 61 and 74 apply. Rather than guessing, the agent finds a
test named `KeyEquals_MultiCharNonMatchWhileNotActivated_InvokesUpdateButNotToggleControl` whose XML
comment states plainly that this is a characterization test, that the divergence looks unintentional,
and that it is tracked by a specific GitHub issue. The agent now knows the behavior is recorded but
unratified, and that fixing it means updating the test deliberately rather than working around a
mysterious assertion.

### Scenario 4 — a reviewer questions "8 tests, 0% coverage delta"

A reviewer sees eight new tests added to `QfcFormKeyHandlerTests.cs` for a file already at 100% line
coverage and asks why. `spec.md` answers directly: the existing four tests cover positive and
negative flows; the new cases close the **boundary** dimension, and the highest-value one pins that
`Keys.Menu` (the ALT key code, `0x12`) does not satisfy `HasFlag(Keys.Alt)` (the modifier flag,
`0x40000`) — an implicit assumption at three `ProcessCmdKey` call sites that nothing currently
documents. The value is regression protection and scenario completeness, not the percentage.

## What "done" looks like

- Every keystroke path in `KeyboardHandler.cs` that can be reached without a live Outlook host, a
  window handle, or a modal dialog **is** reached by a named, deterministic test.
- The only exemption left in this cluster, if any, is a specific line range with a written reason,
  ratified on F1's ledger — not a file-level attribute and not a decision this child made about
  itself.
- A sibling child can rebase onto the merged result without touching a single line of its own code.
- Every coverage figure in the evidence folder was produced by the same harness every other child
  used, so F16 can add them up.

## Acceptance Criteria

- [ ] **AC1 — Per-file coverage floor.** Every file in the F3 assignment that F1's ledger classifies
      `testable` reaches at least 80% line coverage, verified with F1's per-file harness, with the
      numeric per-file result committed under
      `docs/features/active/2026-08-07-quickfiler-keyboard-actions-coverage-430/evidence/qa-gates/`.
      Files that F1's ledger classifies `interface-only` report `N/A` and are excluded from the
      numeric floor. *Benefit: the maintainer and F16 get one comparable number per file rather than
      an aggregate that hides an untested 414-line file.*
- [ ] **AC2 — `KeyboardHandler.cs` de-exemption.** `[ExcludeFromCodeCoverage]` is removed from
      `QuickFiler/Controllers/KeyboardHandler.cs:22`, the three unused `using` directives (lines 12,
      14, 15) are removed, and the file reaches the floor via the K1–K5 seams — unless F1's ledger
      ratifies a specific irreducible remainder (candidate: lines 35–39 only, the `EfcViewer`
      constructor overload). Any residual exemption is recorded in F1's ledger and is not
      self-granted by this child. *Benefit: the epic's leading indicator — zero
      `[ExcludeFromCodeCoverage]` attributes on testable seams — moves by one file, and the
      unused Outlook `using` that made the file look COM-bound is gone.*
- [ ] **AC3 — Additive cross-child contract.** `QuickFiler/Interfaces/IQfcKeyboardHandler.cs` has no
      member added, removed, renamed, or re-typed, and both existing two-argument construction sites
      — `QuickFiler/Controllers/QfcHomeController.cs:184–189` (F7) and
      `QuickFiler/Controllers/EfcHomeControllerDependencyFactories.cs:141–147` (F8) — compile
      unmodified. *Benefit: the thirteen sibling children in the same wave are unaffected, including
      the four test files using `MockBehavior.Strict`.*
- [ ] **AC4 — File size.** No production file in scope exceeds 500 lines. `KeyboardHandler.cs` is
      measured after the refactor; the documented contingency split at line 262 is applied only if
      the measured count exceeds 500. *Benefit: the epic NFR holds without a speculative split that
      would enlarge the diff.*
- [ ] **AC5 — Test framework and determinism.** Every new or modified test uses MSTest, Moq, and
      FluentAssertions in Arrange–Act–Assert form, and is deterministic and isolated: no temporary
      files, no external services, no live forms, no popups, no UI-thread dependency, and no
      `Thread.Sleep`, `Task.Delay`, `.Wait()`/`.Result`, or wall-clock wait. `async void` paths use
      the `InlineSynchronizationContext` precedent, and every test touching
      `SynchronizationContext.Current` restores it in a disposable scope. *Benefit: the suite gives
      the same answer in the IDE runner and in CI, and cannot contaminate a sibling test class under
      `ClassLevel` parallelization.*
- [ ] **AC6 — Scenario completeness per file.** For each in-scope file with executable behavior,
      coverage spans the positive path plus invalid-input, boundary, and error-handling behavior.
      Where a category is structurally inapplicable — for example `QfcFormKeyHandler.IsAltKeyCommand`
      takes a non-nullable enum and cannot throw — that fact is recorded rather than a test being
      manufactured to satisfy the form. *Benefit: a reviewer can distinguish an honest "not
      applicable" from an omission.*
- [ ] **AC7 — Toolchain.** The full C# toolchain passes in final form in one uninterrupted pass, and
      the commands run are stated: `csharpier .`; the analyzer build
      (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`); the nullable build
      (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`); and coverage-enabled
      `vstest.console.exe ... /EnableCodeCoverage`. *Benefit: the merge is safe for the integration
      branch that thirteen siblings share.*
- [ ] **AC8 — No behavior change.** No observable QuickFiler keyboard flow changes. The six latent
      defects identified in research (L1–L6 in `spec.md`) are characterized, not fixed, and every
      characterization test carries an XML comment naming it as such and citing its promoted issue
      number. *Benefit: a user's keystrokes behave exactly as before, and a future agent can tell
      recorded-but-unratified behavior from intended behavior.*
- [ ] **AC9 — File-boundary isolation.** No file outside the F3 assignment is modified —
      specifically not `coverage.config`, not `UtilitiesCS/Properties/AssemblyInfo.cs`, not any
      shared build property file, and not any sibling-owned production or test file. Exactly two
      edits are permitted outside the F3 production set, both limited to adding `<Compile Include>`
      entries adjacent to the existing block: new test files in
      `QuickFiler.Test/QuickFiler.Test.csproj`, and the two new F3-authored production files in
      `QuickFiler/QuickFiler.csproj`. The latter is unavoidable because the legacy non-SDK project
      uses no globbing, so an unlisted production file does not compile; a `.csproj` is the project's
      own file, not a shared build property file such as `Directory.Build.props`. *Benefit: fan-in
      stays a clean merge; F1 keeps sole ownership of `coverage.config`; no unrelated assembly's
      internals are leaked to widen a test's reach.*
- [ ] **AC10 — `MailItemActionsAdapter` guard is atomic with its test.** The `ArgumentNullException`
      constructor guard and its test
      (`Constructor_WithNullMailItem_ThrowsArgumentNullException`) ship together, or both are
      deferred and the deferral is recorded as an explicit decision. Neither ships alone. *Benefit:
      the file stays at 100% branch coverage either way; shipping the guard alone would be a coverage
      regression on changed lines.*
- [ ] **AC11 — No timer or clock seam.** No `TimeProvider`, `FakeTimeProvider`, fake-timer facility,
      or injected clock is introduced in any production or test file in this child. The `issue.md`
      lines 73–74 expectation that `KaStringAsync` needs one is recorded as corrected — verification
      found zero `async`, `await`, `Task.Delay`, `Thread.Sleep`, timer, `DateTime`, or `TimeProvider`
      occurrences in all 95 lines — and the existing `KaStringAsyncTests.cs` is confirmed free of
      wall-clock waits with no remediation performed. *Benefit: no seam is added to a five-parameter
      constructor consumed by F11 for a dependency that does not exist.*
- [ ] **AC12 — Evidence.** Baseline and final per-file coverage figures are written under
      `<FEATURE>/evidence/qa-gates/` per
      `.claude/skills/evidence-and-timestamp-conventions/SKILL.md`, and the evidence states whether
      the harness aggregates Cobertura entries by `filename` or by class — which determines whether
      `KaChar.cs` and `KaKey.cs`, each declaring two classes, report as one figure or two — and how
      0/0 files are reported. *Benefit: the recorded numbers are unambiguous to F16 and to a reader
      months later.*
- [ ] **AC13 — F1 ledger consumed, not presumed.** F1's ledger classification is read and cited for
      each of the 11 in-scope files. No file is assumed `testable` or `ratified-exempt` by this
      child. If F1's classification conflicts with the evidence recorded in `spec.md`, the conflict
      is escalated to the epic orchestrator rather than resolved by fabricating tests or
      self-granting an exemption. *Benefit: exemptions stay reviewable and centralized, which is the
      reason F1 is a real dependency rather than stylistic ordering.*
- [ ] **AC14 — Repository-wide coverage recorded, not regressed.** Repository-wide line and branch
      coverage figures are recorded before and after this child's work as a record-and-report
      obligation, and this child does not lower them. The repository-wide floor is not a blocking
      gate for this child; the change-scoped obligations are AC1, AC2, and the `>= 90%` new-code
      floor on the two new files `IQfcDialogPrompt.cs` and `MyBoxDialogPrompt.cs` per
      `.claude/rules/csharp.md:40`. *Benefit: the epic's third leading indicator — repository-wide
      coverage retained or improved at each child merge — is evidenced without importing an
      unsatisfiable gate into a child that controls only 11 files.*

## Non-Goals

Explicitly excluded from this feature:

- **Fixing the latent defects.** L1 (`KbdActions(IEnumerable)` duplicate-guard bypass), L2
  (`KaChar.DelegateType` type mismatch), L3 (orphaned `Update` / `DelegateType` members), L4
  (`KaStringAsync.KeyEquals` input validation), L5 (the `Update` gate asymmetry at line 72), and L6
  (the `MailItemActionsAdapterTests.cs` layout deviation) are report-only. Each is being promoted to
  its own GitHub issue by the epic orchestrator. Fixing any of them here would violate AC8.
- **Any change to `IQfcKeyboardHandler`, `IItemControler`, `IKbdAction`, or `IMailItemActions`.**
  All four stay byte-identical. The two tempting-but-breaking changes — widening
  `BreadcrumbArrowFallThrough(ItemViewer, …)` to `IItemViewer`, and adding an
  `IKeyboardHandlerHost` to F9-owned `EfcViewer.cs` — are rejected in `spec.md` with their minimum
  breaking deltas recorded for any future issue that pursues them.
- **Renaming the misspelled `IItemControler`.** It requires edits to five sibling-owned files.
- **Any change to `coverage.config`, `UtilitiesCS/Properties/AssemblyInfo.cs`, or any shared build
  property file.** `coverage.config` is F1-owned; adding `InternalsVisibleTo("QuickFiler.Test")` to
  `UtilitiesCS` would modify a shared assembly's public-surface policy for one child's convenience,
  which the K1 seam makes unnecessary.
- **Deleting dead members.** `ClearFilter()`, `KeyboardHandler_PreviewKeyDown`, and `GetItemViewer`
  have no callers, but deletion is a public-surface change. They are covered, and removal is proposed
  as a follow-up issue.
- **Any STA test file.** No `*.StaTests.cs` file exists in `QuickFiler.Test` and none is warranted
  here; every WinForms object this child touches constructs headlessly.
- **Converting QuickFiler away from VSTO/WinForms.** That is the separate long-term migration effort
  (`epic.md` § Non-Goals). Where a seam choice was open, the host-neutral option was preferred.
- **Coverage work on `QuickFiler/Legacy/**`, `QuickFiler/Notes/**`, or the three non-compiled viewer
  files (`QfcFormViewerExpanded.cs`, `QfcFormViewerDark.cs`, `EfcViewer3.cs`).** None appears in
  `QuickFiler.csproj`'s 121 `<Compile Include>` entries, so all are outside the coverage denominator
  and outside the epic.
