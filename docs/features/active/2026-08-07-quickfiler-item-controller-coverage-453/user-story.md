# `quickfiler-item-controller-coverage` — User Story

- Issue: #453 (https://github.com/drmoisan/TaskMaster/issues/453)
- Parent epic: #136 QuickFiler Per-File 80% Coverage — child F10 (wave 1, band C3)
- Owner: drmoisan
- Work Mode: `full-feature` (this file and `spec.md` are the authoritative acceptance-criteria sources)
- Status: Ready for Planning
- Last Updated: 2026-08-07

---

## Story Statement

- **As the maintainer of QuickFiler**, I want the `QfcItemController` family to be covered by tests
  dense enough in both lines and branches that an autonomous agent changing any of its ten partials
  gets a failing test rather than a silent regression, **so that** I can delegate work on this
  3,180-line type without personally reading every diff.

- **As the maintainer who personally ratified this family's coverage-exemption boundary on
  2026-07-02 after denying three weaker versions of it**, I want each of the 18 ratified exemptions
  re-verified against today's source and each retention traceable back to my decision record, **so
  that** the boundary neither silently rots as the code moves nor gets quietly re-litigated by an
  agent that lacks the authority to overturn it.

- **As a reviewer of this epic's child pull requests**, I want a per-file line **and** branch figure
  computed by a method I can reproduce, **so that** I am not asked to accept a pass on a number the
  tooling is known to compute wrongly.

---

## Problem / Why

The `QfcItemController` family is the largest single cluster in epic #136's denominator: ten
production partials plus one interface file, 3,180 lines, all declaring one type. It is also the
family the project has already invested the most governance effort in — five remediation cycles
under issue #227 reduced its coverage-exemption boundary from 103 members to 19, and the maintainer
ratified that 19-member boundary on 2026-07-02.

That history creates the specific problem this child solves. The family sits in an awkward middle
state:

- **It looks safer than it is.** 17 test files and 166 test methods already exist, and eight of the
  ten partials report a passing line-coverage figure. But branch coverage — the gate that actually
  catches a missed conditional — is below 75% on **seven of the ten**, including two files the epic
  listed as compliant. And the reported figures themselves are produced by a defective post-processor
  (open issue #441) that inflates some files and deflates others, so at least one file
  (`MailActions.cs`) **falsely passes** its branch gate on the emitted number.
- **It looks less compliant than it is.** Nineteen `[ExcludeFromCodeCoverage]` attributes read, to a
  fresh pair of eyes or an automated gate, as nineteen unexamined holes. Eighteen of them are the
  documented residue of an audit the maintainer already ran to exhaustion. Treating them as
  violations would waste a cycle re-deriving a conclusion that is already on file — and would
  contradict a decision the child has no authority to overturn.
- **Three of the nineteen are on code nothing calls.** `Initialize(9-arg)`, `CreateAsync`, and
  `CreateSequentialAsync` have zero call sites anywhere in the solution. They carry exemptions on
  code that cannot run.
- **One of the nineteen is drift.** `EnsureBreadcrumbPipeline` entered the file after the
  ratification, via breadcrumb work, and never went through the boundary process at all.

The value of this child is therefore not "more coverage". It is **making this family's safety
legible**: a real branch-coverage floor on every file, an exemption boundary whose every member is
either freshly re-verified or gone, evidence a reviewer can reproduce, and a written record of which
sibling contracts this family is standing on.

---

## Personas & Scenarios

### Persona — Dan, maintainer of QuickFiler

- **Who:** The project maintainer and sole reviewer of merges into `main`. Wrote most of
  `QfcItemController` originally; ratified its exemption boundary under #227.
- **Cares about:** Whether a change to this family is safe to accept without reading it line by
  line. Whether a coverage number means what it says.
- **Constraints:** VSTO/WinForms host means a real message pump, a real Outlook, and a real WebView2
  runtime are all off-limits inside a unit test. Review time is the scarce resource; the whole point
  of the epic is to spend engineering effort now to buy review confidence later.
- **Frustrations:** Having previously been given exemption boundaries of 103, then 41, then 24, each
  presented as the floor. Being asked to accept a metric that is arithmetically wrong on its face
  (a 326-line file reporting 373 coverable lines).
- **Goals:** Delegate QuickFiler maintenance to agents. Keep the long-term option of migrating off
  VSTO open by preferring host-neutral extractions over new WinForms investment.

### Persona — an autonomous agent assigned future work in this family

- **Who:** A future `csharp-atomic-executor` or `feature-reviewer` run, weeks from now, with no
  memory of this child.
- **Cares about:** Being told by a failing test when it breaks something; not being blocked by an
  exemption whose reason is unrecorded; not proposing an edit to a file another child owns.
- **Constraints:** Reads only the repository. Cannot ask the maintainer a question mid-run.
- **Frustrations:** In-code justification comments that were true when written and are false now —
  this family has several (`Navigation.cs:171-172` and `:189-190` cite a control-tree barrier
  defeated by a later retrofit; all seven `Initialization.cs` comments cite "requires a live
  ItemViewer" when headless construction is proven twice in the existing test project).

### Scenario 1 — accepting an agent-authored change to expansion behaviour

**Trigger.** An agent is assigned issue #482 (the cross-variant expansion-registry divergence) and
proposes a change to `ToggleExpansion(ToggleState)` in `Navigation.cs`.

**Today.** That method carries `[ExcludeFromCodeCoverage]`, so its body is outside the denominator.
The agent can rewrite it, coverage does not move, and no test fails. Dan has to read the diff and
reason about the sync/async registry split himself.

**After this child.** Either the attribute is gone and the method is covered by ordering,
re-entrancy, and cross-variant characterisation tests — so a semantic change breaks a test — or the
attribute is retained with a re-verified, member-specific rationale in an artifact Dan can read in
the PR diff. Either outcome tells him something. Neither leaves him guessing.

### Scenario 2 — reviewing the coverage claim on the pull request

**Trigger.** The PR body claims every file clears 80% line and 75% branch.

**Today.** `MailActions.cs` emits `branch-rate="0.75"` — exactly on the floor — while its true
de-duplicated rate is 72.7%. A reviewer trusting the emitted attribute accepts a file that fails.

**After this child.** The evidence under `<FEATURE>/evidence/qa-gates/` carries **both** figures side
by side, the recomputation method is stated, and the discrepancy is attributed to open issue #441 by
number. Dan can reproduce the class-level-union arithmetic from the committed Cobertura file in a
few minutes and confirm the claim, rather than taking it on trust.

### Scenario 3 — the F16 capstone reconciles the ledger

**Trigger.** The epic's capstone recomputes the denominator from `QuickFiler.csproj` and checks every
compiled file against the ledger.

**Today.** Nineteen attributes in this family would each surface as an unexplained exemption, and
epic AC2 ("the count ... falls to zero") would fail with no recorded reason.

**After this child.** Each retained attribute's ledger row cites `maintainer-decision.2026-07-02.md`
and the fresh re-verification artifact; each removed attribute's row records the current-code
evidence that its rationale lapsed; the three deletions are recorded as removals of dead members
rather than as de-exemptions; and the `IQfcItemController.cs` row reads `interface-only /
not-measured`, N/A, with the positive-control proof attached. The capstone closes on evidence
instead of on an unmeetable count.

### Scenario 4 — a sibling child changes `ConversationResolver`

**Trigger.** F4 (#434) is working on `QuickFiler/Helper Classes/ConversationResolver.cs` in the same
wave and considers retyping the public surface to `IConversationResolver`.

**Today.** Nothing in the repository tells F4 that three positional call sites in F10's files bind
the concrete type, that `IQfcItemController.cs:69` declares
`PopulateConversation(ConversationResolver)`, or that F11 receives the same concrete type through
`ToggleUnGroupConv`. The change lands and three children break at fan-in.

**After this child.** `spec.md` §10.1 states the exact shapes F10 depends on, states that appending
defaulted parameters is safe while retyping is not, and states that a retype is a **three-child**
break. F4 can check its proposed change against a written contract before making it.

---

## Acceptance Criteria

These complement `spec.md`'s criteria rather than repeating them. `spec.md` AC-1 through AC-20
govern the mechanics — coverage floors, the exemption arithmetic, seam discipline, csproj and
evidence obligations. The criteria below govern whether the **maintainer-facing value** described
above was actually delivered.

### Legibility of the exemption boundary

- [ ] **US-1.** A reader who opens only the fresh exemption-boundary artifact under
      `<FEATURE>/evidence/other/` can answer, for **every** attribute remaining in the F10 file set:
      which member it is (`file:line`), whether it is covered by the #227 ratification or asserted on
      F10's own authority, what the current barrier is, and what would remove it. No entry says only
      "retained" or points at a category without naming the member.

- [ ] **US-2.** Every in-code justification comment left standing next to a retained attribute is
      **true of the current source**. Comments whose stated barrier has been defeated are rewritten
      to describe the real residual barrier — specifically, the seven `Initialization.cs` comments
      claiming "not unit-reachable without a live `ItemViewer`" (headless construction is proven at
      `ViewerSetupTests.cs:379` and `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`), the
      `ViewerSetup.cs:30-37` comment citing the concrete `L0v2h2_WebView2` cast rather than the
      operative `.CoreWebView2` external-runtime dependency at `:76`, and the `Navigation.cs`
      `:171-172` / `:189-190` comments citing a `TlpCellSnapShot` barrier that the
      `IContainerControlLocal` retrofit removed (these two attributes are retained, so correcting
      their comments is mandatory rather than conditional — see US-3).

- [ ] **US-3.** The `Navigation.cs:173` / `:191` pair is **retained**, its two stale comments are
      corrected, and the reasoning is legible to a future reader. The exemption-boundary artifact
      distinguishes the two claims that were previously conflated: the *in-code comment* cited a
      `TlpCellSnapShot` barrier that the `IContainerControlLocal` retrofit removed (stale — correct
      it), whereas the *ratified* rationale in `exemption-boundary.2026-07-02T17-00.md` §3 is
      "deliberate `virtual` test seam" and was written with that retrofit explicitly in view ("now
      de-exempted at the leaf via R2"), so it has not lapsed. The artifact also records that the
      deliberate-seam argument is weaker post-R2 for this pair than for
      `DoLoadConversationResolverCoreAsync`, and refers that to the maintainer. Silence on this pair
      is not acceptable, and neither is removing it on F10's own authority.

### Safety for autonomous modification

- [ ] **US-4.** The family's load-bearing state-transition invariants are pinned by named tests, so
      that a future agent breaking one gets a failure rather than a silent behaviour change. At
      minimum: initialization **ordering** (control groups before themes; populate before
      tips/navigation; wiring last); **re-entrancy** (a second `WireEvents()` double-dispatches; a
      second focus registration throws from `KbdActions.Add`); and **dispose-before-setup** (after
      `Cleanup()`, `EnsureBreadcrumbPipeline` is a safe no-op, and the handlers that survive
      `Cleanup()` fail in the way #481 describes).

- [ ] **US-5.** Each such test **characterises** current behaviour and is written so that the
      corresponding defect (#480-#485) remains straightforward to fix later. No assertion is added
      that would have to be inverted rather than tightened when the defect is fixed. Where an
      existing test masks a defect with a loose assertion — `FocusAndThemeTests.cs:310` uses
      `Times.AtLeastOnce()`, which hides #480's double toggle — the assertion is tightened to an
      exact count so the defect becomes visible without being fixed.

- [ ] **US-6.** No change in this child makes an open defect harder to fix. Specifically:
      `TextBoxSearch_TextChanged` behaviour is untouched and no new test pins
      `SetFolderDroppedDown(true)` (#438 must stay fixable); no new test constrains
      `BreadcrumbArrowFallThrough` semantics beyond the routing already asserted (#440); and nothing
      pre-empts or partially implements the `LoadFolderHandlerAsync` short-circuit that #427 will
      need.

### Trustworthy evidence

- [ ] **US-7.** A reviewer can reproduce the per-file coverage claim from the committed Cobertura
      artifact without re-running the suite: the evidence states the recomputation method (unique
      class-level `<line>` children; summed `condition-coverage` numerators and denominators), shows
      both the harness figure and the recomputed figure, and names open issue **#441** as the reason
      the two differ.

- [ ] **US-8.** Line and branch are reported as **independent** results for every file. No file is
      described as passing on the strength of its line figure alone, and the summary states plainly
      that branch was the binding gate on seven of the ten partials.

- [ ] **US-9.** `QuickFiler/Interfaces/IQfcItemController.cs` is reported as **N/A**, never as 0% and
      never as a failure, and the summary states that no test was written for it and why (57 bodiless
      declarations; zero coverable lines; instrumentation reach proven by the sibling
      `MailItemActionsAdapter.cs` at `line-rate="1"`).

### Host-neutrality and the migration path

- [ ] **US-10.** The one new production file, `QfcCidImageResolver`, is **host-neutral** — a pure
      static resolver over a URI string and an `IAttachment[]`, with no WinForms, WebView2, or
      Outlook Interop dependency in its own surface — so that a future WebView2/Office.js port can
      reuse it verbatim. No new WinForms or WPF dependency is introduced anywhere in the change set.

- [ ] **US-11.** No `*.StaTests.cs` file is created, and no unit test constructs a real WinForms
      **form**, shows a dialog, or depends on a message pump. Research established that the existing
      harness plus headless `UserControl` construction suffices everywhere in this family; if
      execution finds an exception, it is justified per-member against epic.md's STA last-resort
      clause and called out explicitly rather than adopted quietly.

### Reviewability of the change set

- [ ] **US-12.** The `public static` deletion (`CreateAsync`, `CreateSequentialAsync`) appears in the
      change description as its **own** called-out item, not folded into an exemption count, with the
      mitigating facts stated so a reviewer can judge it in one reading.

- [ ] **US-13.** The cross-child contract notes in `spec.md` §10 are delivered to F4 (#434), F14
      (#456), F3 (#430), and F11 (#454) by the mechanism the epic uses for cross-child
      communication, so a sibling can check a proposed contract change against them **before**
      making it rather than discovering the break at fan-in.

- [ ] **US-14.** The documented deviations that affect other children are propagated, not just
      recorded here: the `QuickFiler.Test/QuickFiler.Test.csproj` shared-file omission (a
      higher-conflict surface than the production csproj, which epic.md does not mention) and the
      member-level-not-file-level nature of this family's exemptions reach F1 and the epic manifest.

---

## Non-Goals

Explicitly excluded from this child. Each is excluded for a stated reason, not by omission.

- **Reducing this family's exemption count to zero.** 18 of 19 attributes are maintainer-ratified;
  neither F1's ledger nor F10 has the authority to overturn that decision. Epic AC2 as literally
  written is reconciled in `spec.md` §3.2, not pursued.
- **Building the issue-#230 WinForms message-pump test seam.** It is materially larger, distinct
  test infrastructure living outside F10's file assignment; the #227 decision record states
  explicitly that it is not a merge condition. Attempting it is a scope breach.
- **Fixing any defect.** The epic's no-behavior-change NFR binds. #480-#485 are promoted and left
  alone; new findings are promoted, not fixed.
- **Re-filing already-filed issues.** #441, #457, #463, #444, #450, #230, #427, #438, #440 exist.
- **Editing any sibling-owned file.** `QuickFiler/Helper Classes/**` (F4), `KeyboardHandler.cs` and
  the keyboard-action types (F3), `IQfcDatamodel` (F5 — and unreferenced by this family in any
  case), `QfcCollectionController.cs` (F11), `ItemViewer*.cs` / `IItemViewer.cs` (F14),
  `WebView2CoreInitializer.cs` (F13), and all of `UtilitiesCS`.
- **Widening `IQfcItemController` or `IItemViewer`.** A hand-written full implementation of
  `IQfcItemController` lives in `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:340-460`, an
  F4-adjacent file; adding an interface member breaks its compilation and guarantees a fan-in
  conflict.
- **Widening the `UtilitiesCS` `InternalsVisibleTo` grant.** F10 does not hit that wall; every
  `UtilitiesCS` surface it touches is public. F3's precedent (build a local seam) is followed.
- **Testing or exempting dead code.** Deletion is the disposition for members with zero call sites.
- **Writing shape-assertion tests for `IQfcItemController.cs`.** Prohibited by epic.md; they
  manufacture no coverage for the file they claim to cover.
- **Converting QuickFiler away from VSTO/WinForms.** That is the separate long-term migration
  effort. Where a seam choice is open, this child prefers host-neutral extraction that the migration
  can reuse.
