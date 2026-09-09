# 2026-09-08-quickfiler-teardown-review-residuals (Spec)

- **Issue:** #823
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/823
- **Epic:** review-residuals-2026-09-08 (wave 0)
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-09T00-15
- **Status:** Approved
- **Version:** 1.0
- **Work Mode:** full-bug

> `full-bug` work mode. This `spec.md` is the sole acceptance-criteria source for this feature.
> `user-story.md` is deliberately absent and must not be created.

---

## Context

Issue #823 is a standing residuals record. It batches six low-severity findings raised by the
`bugs-2026-09-06` run's reviews of items 810 and 812, none of which individually warranted its own
issue. The findings are labelled R1 through R6 in the issue body.

Environment:

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Toolchain: CSharpier 1.2.6 via `dotnet tool run`, MSBuild, `vstest.console.exe`
- Data source or fixture: not applicable; all findings are static review findings, not runtime
  reproductions

Impact / Severity:

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low individually. Recorded together so they are not rediscovered by every subsequent review of this
subsystem, and so R5's intermittent failure has a place to accumulate observations before anyone
diagnoses it.

### Disposition of the six entries

| Entry | Disposition | Produces |
| --- | --- | --- |
| R1 — per-controller SMTP retry latch is shared across stores | In scope, code change | Production change plus two new tests |
| R2 — the issue-818 throw relocated rather than disappeared | In scope, decision only | Recorded decision; no code change |
| R3 — `Register` signature and contract disagree | In scope, contract reconciliation | Production change plus one rewritten test |
| R4 — stale line-count comment | In scope, comment correction | One comment token |
| R5 — intermittent dispatcher-transaction test | In scope, observation only | Evidence artifact plus an XML-doc pointer |
| R6 — plan-authoring guidance amendment | Out of scope, deferred upstream | Nothing in this repository |

### Evidence basis

Every location cited below was read in the current tree during research
(`research/research.2026-09-08T23-50.md`) and spot-checked by the orchestrator, which found no false
citation. Statements derived by reasoning rather than by direct reading are marked INFERRED. No
build and no test run was performed while authoring this specification, so no toolchain evidence is
asserted here; the toolchain acceptance criteria below are the confirming gates.

---

## Write Set

The complete set of files this feature may modify or create. Any file not listed here is out of
scope.

- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs`
- `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`
- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs`
- `QuickFiler/Viewers/QfcFormViewer.cs`
- `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`
- `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs`
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`
- `docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md`

Plus this feature's own folder artifacts (`spec.md`, the plan, and `evidence/**`).

No `.csproj` change is required. Both target test classes already exist and both already carry
`Compile Include` entries: `UtilitiesCS.Test/UtilitiesCS.Test.csproj:542` lists
`OutlookObjects\Store\StoreWrapperController_Tests.Display.cs` and
`QuickFiler.Test/QuickFiler.Test.csproj:84` lists `Viewers\BreadcrumbPopupOwnerRegistryTests.cs`.
This feature adds no new source file, so it makes no contribution to the epic's project-file
contention.

---

## Scope & Non-Goals

**In scope:** R1 (code), R2 (decision), R3 (code), R4 (comment), R5 (observation record).

**Out of scope / non-goals:**

1. **R6.** The plan-authoring guidance amendment in `atomic-plan-contract` belongs upstream.
   Everything under `.claude/` in this repository is pushed down from the separate `drm-copilot`
   governance repository with no templating, so a fix applied here is overwritten by the next
   push-down. No file under `.claude/` may be changed by this feature. This matches the epic's
   Non-Goals item 2.
2. **The relocated throw in QuickFiler/Controllers/QfcItemController.FolderHandling.cs.** Owned by
   issue #813, a wave-0 sibling in the same epic. See the R2 decision below. That file is off
   limits.
3. **Stabilizing the R5 test.** No sleep, no retry, and no timing tolerance is introduced. No
   executable statement in that test changes.
4. **The three sibling stale line-count comments** listed under "Reported-only observations". R4
   names one file; correcting the others would be unrequested scope.
5. **Any change to a coverage threshold, an analyzer severity, or a policy requirement.** None is
   lowered, weakened, or deleted.

**Off-limits files** (owned by concurrently running sibling features or by governance push-down):
QuickFiler/Controllers/QfcItemController.FolderHandling.cs; QuickFiler/Controllers/QfcHomeController.cs;
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs; UtilitiesCS/Threading/ProgressViewer.cs;
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs; UtilitiesCS/NewtonsoftHelpers/SDIL Reader/\*\*;
UtilitiesCS.Test/Properties/AssemblyInfo.cs; UtilitiesCS/OutlookObjects/Table/OlTableExtensions.\*;
UtilitiesCS/Threading/TimeOutTask.cs; UtilitiesCS/Extensions/DfDeedle.cs;
UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs; .editorconfig; BannedSymbols.txt;
every `Console.SetOut` restore in test classes; anything under .claude/, CLAUDE.md, .claude/rules/,
.github/instructions/, and docs/features/epics/.

---

## R1 — Rescope the SMTP retry budget from per-controller to per-store

### R1.1 Current state (verified)

| Fact | Location |
| --- | --- |
| `private bool _userEmailRetryAttempted;` | `StoreWrapperController.cs:105` |
| Field XML doc stating "one attempt per controller instance (issue #812)", never reset | `StoreWrapperController.cs:96-104` |
| Latch read, third conjunct `&& !_userEmailRetryAttempted` | `StoreWrapperController.Display.cs:51` |
| Latch set `_userEmailRetryAttempted = true;` | `StoreWrapperController.Display.cs:54` |
| First two conjuncts `Current is not null && Current.UserEmailAddress is null` | `StoreWrapperController.Display.cs:49-50` |
| Retried call `Current.RefreshUserEmailAddress();` | `StoreWrapperController.Display.cs:55` |
| Retry-gate comment stating the superseded bound | `StoreWrapperController.Display.cs:40-47` |

A repo-wide search for `_userEmailRetryAttempted` returns exactly those three sites. There is no
other reader and no reset anywhere.

The multi-store path is real. `DisplayName_SelectedValueChanged` (`StoreWrapperController.cs:163`)
assigns `Current` from `Model.Stores` at `:179` and then calls `PopulateWithCurrent()` at `:180`, so
one controller observes many distinct `StoreWrapper` instances while the single `bool` is consumed
by whichever store happens to be selected first with a null address. R1's premise is correct.

### R1.2 Approved change

Replace the `bool` with an instance-field, reference-identity set of the stores already attempted,
declared on `StoreWrapperController.cs`:

```csharp
private readonly HashSet<StoreWrapper> _userEmailRetryAttemptedStores =
    new HashSet<StoreWrapper>();
```

At `StoreWrapperController.Display.cs:48-56`, the third conjunct becomes a membership test against
that set and the set-line becomes an `Add` of `Current`. The first two conjuncts and the statement
order are unchanged. `System.Collections.Generic` is already imported at
`StoreWrapperController.cs:3`; the display partial needs no new import because the field lives in
the other partial part.

The field must not be `static`.

### R1.3 Why reference identity and not `StoreId`

`StoreWrapper.StoreId` (`StoreWrapper.cs:162`) is declared `public string? StoreId { get; set; }`
and is documented and coded as possibly unreadable at four existing fail-safe sites
(`StoreWrapper.cs:46-55`, `StoreWrapperController.cs:262`, `StoreWrapperController.cs:312`,
`StoreWrapperController.Display.cs:116`). Keying the set on it would collapse every store with an
unreadable id onto a single sentinel key, so exactly the stores whose COM reads are already failing
would go on sharing one budget. That is R1's defect, reintroduced in the worst place.

`StoreWrapper` declares no `Equals` or `GetHashCode` override anywhere in the type (whole file read,
306 lines), so a `HashSet<StoreWrapper>` uses the default reference equality. The key is the object
itself, is available without any COM read, and therefore cannot itself throw. `Current is not null`
is already established by the first conjunct before the set is consulted, so the key can never be
null.

### R1.4 Alternatives considered and rejected

- **Reset the `bool` in `DisplayName_SelectedValueChanged`.** Rejected. This is precisely the
  unbounded per-re-selection retry that issue #812 removed: re-selecting one failing store
  repeatedly would run the blocking COM chain on every gesture.
- **Make the latch `static`.** Rejected. It would silently reduce the bound to once per process,
  and it would fail the existing test
  `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`
  (`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:200`), whose
  assertion at `:222` is `exchangeUser.VerifyGet(x => x.PrimarySmtpAddress, Times.Exactly(2));`.
  A process-wide per-store latch would produce 1 there.

### R1.5 Ordering constraints that must be preserved

1. **The latch is recorded before the lookup runs.** The `Add` must precede the
   `Current.RefreshUserEmailAddress()` call, so an exception escaping the lookup still consumes that
   store's single attempt. This ordering was reviewed and endorsed in the issue-812 code review.
2. **The key derivation sits after the existing `Current is not null` conjunct.**
   `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow`
   (`StoreWrapperController_Tests.Display.cs:333`) drives the method with `Current` set to null at
   `:338`, so any dereference of `Current` placed before that conjunct fails the test with a null
   reference.
3. **The set is never reset.** A fresh dialog open still yields a fresh budget because
   `TaskMaster/Ribbon/RibbonController.cs:261` constructs a fresh controller per open. That is the
   sole production construction site; every other construction is in `UtilitiesCS.Test`.

### R1.6 Invariant that must not regress

The number of SMTP lookup attempts a single controller instance performs is bounded by the number of
distinct stores it has been given, and is independent of how many times the user re-selects any
store. No sequence of user gestures can produce an unbounded series of blocking UI-thread COM
lookups.

### R1.7 Accepted cost

The worst case rises from one blocking SMTP lookup per dialog open to N, where N is the number of
stores in `Model.Stores` whose address is null. This is stated explicitly and accepted: the resulting
bound is set by configuration rather than by user gestures, which is the distinction issue #812 drew.

### R1.8 Superseded prose that must be corrected

Changing the bound invalidates prose at five living sites, all of which are inside this change's own
file set:

1. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:96-104` — the field XML doc.
2. `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs:40-47` — the retry-gate
   comment.
3. `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs:196-201` — the `RefreshUserEmailAddress`
   comment, which currently states that the caller "attempts this at most once per controller
   instance".
4. `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:118-121` — the
   Arrange comment naming the bound.
5. `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs:193-198` — the
   XML doc that currently asserts in prose that the bound is "per controller instance, not per
   store".

Each must be rewritten to state the new bound: at most one attempt per controller instance **per
store**.

### R1.9 Approved documentation correction outside this feature's own folder

Append a single dated correction block to the living `spec.md` of the merged issue-812 feature
folder, at
`docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/spec.md`,
recording that issue #823 changed the retry bound from per-controller to per-controller-per-store.

Nothing else in the issue-812 folder may be modified: its plan, `code-review.2026-09-08T17-00.md`,
`policy-audit.2026-09-08T17-00.md`, `feature-audit.2026-09-08T17-00.md`, `research/**` and
`evidence/**` must be left byte-identical. This follows that feature's own precedent, which amended
the merged issue-797 living spec with dated corrections while leaving its other artifacts untouched.

---

## R2 — Decision only, no code change

**Decision: intermediate state, deferred to issue #813.**

The issue-818 change relocated the throw from the `FolderArray` consumption at
QuickFiler/Controllers/QfcItemController.FolderHandling.cs:212 to the direct archive-root read at
:233, within the same unguarded method `AssignFolderComboBox` (declared at :191). The exception type
is unchanged — `InvalidOperationException`, raised by
`TaskMaster/AppGlobals/ArchiveRootPathGuard.RequireResolvedArchiveRoot` — and the throw still
reaches a UI-dispatcher boundary unhandled: `AssignFolderComboBox` contains no `try`, and none of its
callers wraps the call.

The relocation is **not** one of the five functional `FolderPredictor` reads that issue #818
deliberately left unguarded. Those five are inside UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs.
The relocated site is a sixth, distinct site in a different file and a different project, and it is a
consumer rather than a provider.

The relocation was disclosed at the time, in the issue-812 specification's Non-Goals item 2 and
Risk 3, both of which committed to filing a follow-up. That follow-up exists: **issue #813**,
severity Medium, promoted at
docs/features/potential/promoted/2026-09-08-assignfoldercombobox-unguarded-archiverootpath-read.md,
and it is a wave-0 sibling of this same epic that owns the file. QfcItemController.FolderHandling.cs
is therefore off limits to this feature. No code change is made under #823 and the scope is not
widened.

**Correction supplied by the research.** The first read of `FolderArray` in that method occurs at
:200, not :212, so :212 is the second read rather than the unique pre-change throw site. The
conclusion is unaffected, because both reads are in the same method and the same call frame. This
correction is recorded so that a later reader does not quote :212 as the unique site.

---

## R3 — Reconcile the `Register` contract with its signature

### R3.1 Current state (verified)

- `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs:1` carries `#nullable enable`.
- `internal void Register(Control itemViewer, Func<bool> popupIsOpen)` is declared at `:42` with
  **non-nullable** parameters.
- The body silently returns at `:44-47` when either argument is null.
- The XML doc at `:30-36` documents both parameters as null-tolerant.
- `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs:154`
  (`Register_NullControlOrNullPredicate_IsIgnored`) passes literal nulls at `:161-162` and asserts
  that neither throws.

### R3.2 The documented rationale is false

The doc at `BreadcrumbPopupOwnerRegistry.cs:31-33` justifies null tolerance with the claim that "the
registration hop runs from a form lookup that can legitimately find no form". That claim is false.

The sole production call site is `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:216`:

```csharp
(FindForm() as QfcFormViewer)?.SetBreadcrumbPopupOwner(this, () => host.IsOpen);
```

The null-conditional operator consumes the form lookup. A failed lookup — or a form that is not a
`QfcFormViewer` — produces a null **receiver** and skips the invocation entirely; it can never
produce a null **argument**. The two arguments are `this` and a lambda literal, neither of which can
be null. The forwarder is `QfcFormViewer.SetBreadcrumbPopupOwner` at `QfcFormViewer.cs:227`, whose
body at `:228` forwards both arguments unchanged, and whose own doc at `:220-221` mirrors the same
false claim ("Ignored when null").

**Therefore: the signature is right and the contract is wrong.** The non-nullable parameters
correctly describe every reachable call; the doc and the test encode a condition that the only call
site provably cannot produce.

### R3.3 Approved change

1. Keep the non-nullable signature exactly as declared.
2. Replace the silent-return guard at `:44-47` with explicit `ArgumentNullException` throws naming
   `itemViewer` and `popupIsOpen`. net48 has no `ArgumentNullException.ThrowIfNull`, so the throws
   are written out longhand with `nameof(...)`.
3. Rewrite the XML doc at `BreadcrumbPopupOwnerRegistry.cs:30-36` and at `QfcFormViewer.cs:220-221`
   to state rejection rather than tolerance.
4. Rewrite `Register_NullControlOrNullPredicate_IsIgnored` into a rejection test.

### R3.4 Considered and rejected: annotate the parameters nullable and keep the guard

Annotating `Control?` / `Func<bool>?` would make the signature agree with the doc and the test, and
would change nothing else. It is rejected because it ratifies a written contract asserting a
null-producing mechanism that does not exist, and it makes permanent a defensive branch that no
production path can reach. It also conflicts with the fail-fast requirement in
`.claude/rules/general-code-change.md` ("Do not silently ignore errors") and with `CLAUDE.md`
section C#4.1.

### R3.5 Why deleting the guard outright is not acceptable

The explicit throw is the point of the change, not an incidental detail:

- A **null control** with no guard reaches `_owners[itemViewer] = popupIsOpen;` at `:49`. The
  `Dictionary<Control, Func<bool>>` indexer setter does throw `ArgumentNullException` on a null key,
  but its message names the framework's own parameter rather than `itemViewer`.
- A **null predicate** with no guard is stored successfully. The failure is then deferred to
  `AnyOpen` at `:59`, where it surfaces as a `NullReferenceException` on a different member, on the
  issue-677 deactivation path.

Both deferrals are strictly worse than the present silent-return behaviour. The explicit throw at
the boundary converts an unreachable silent branch into an unreachable loud branch, which is the
direction the repository's error-handling policy prefers.

### R3.6 Deliberate constraint on the implementation

**Do not add `#nullable enable` to `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs`.**

With the non-nullable parameters retained, the two literal null arguments would raise CS8625
("Cannot convert null literal to non-nullable reference type"), which
`msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
promotes to a build error in an opted-in file. The nullable gate would fail. Nullable annotations are
erased at runtime, so the rejection behaviour is observable from an oblivious test file exactly as
well as from an annotated one. Working around the diagnostic with `null!` or a
`#pragma warning disable CS8625` block would add noise for no gain.

### R3.7 Predicted diagnostic impact

**None.** The complete set of files that mention the type or the member is
`BreadcrumbPopupOwnerRegistry.cs`, `QfcFormViewer.cs`, `ItemViewer.Breadcrumb.cs` and
`BreadcrumbPopupOwnerRegistryTests.cs`. Of those four, only `BreadcrumbPopupOwnerRegistry.cs`
carries `#nullable enable`; the other three are in an oblivious context and can raise no CS86xx
regardless. The orchestrator independently confirmed the absence of those three from the
`#nullable enable` set. No `.csproj` in the repository declares a `<Nullable>` property, so nullable
participation is per-file opt-in, matching `CLAUDE.md` section C#1.3.

This prediction is a static reading of the nullable rules, not a compiler run. The confirming gate is
toolchain step 3.

---

## R4 — Correct the stale line-count comment

`QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs:11` currently reads:

```
/// Held on a second partial-class part so <c>BreadcrumbDropDownHost.cs</c> (480 lines) stays
```

The figure 480 is stale. The measured current line count of `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`
is **459**, confirmed twice independently: once during research (a line-count measurement plus a
read of the file tail confirming that line 459 is the closing brace), and once again while
authoring this specification. Correct 480 to 459.

No test accompanies this change; a comment correction has no observable behaviour. Verification is
the toolchain plus a reviewer re-measuring the referenced file and reading the corrected figure
against that measurement.

---

## R5 — Observation record only

### R5.1 The test

- **File:** `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- **Class:** `QfcItemController_UiThreadDispatcherFixtureTests`, declared at `:31`
- **Method:** `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, declared at `:197`,
  XML doc at `:190-194`, `[TestMethod]` at `:195`, `[Timeout(GateTimeoutMs)]` at `:196` where
  `GateTimeoutMs` is 60000 (`:33`)

The class doc at `:13-22` already records that this test is probabilistic by construction: nothing
can force the second caller to reach its acquisition point while the first still holds the gate, and
there is no deterministic way to prove the second caller is currently blocked without a timed wait,
which the repository's determinism rules forbid. The observation record references that existing
acknowledgement rather than restating it. The class doc at `:23-28` additionally records that the
fixture uses no sleep, no delay, no wall-clock wait and no temporary file, so the issue's prohibition
is already satisfied by the current code and nothing needs removing.

### R5.2 Approved treatment — two parts and no more

1. **An append-only observation log** at
   `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`.
   The path is fixed by this specification so that the pointer added in part 2 and the acceptance
   criterion below name the same artifact. Seed it with the one known observation set: during the
   issue-810 run the test failed once and passed three times, on the same tree and on an adjacent
   unmodified tree. The per-row schema carries date, command, assembly set, parallelism setting, and
   the failure text.
2. **A two-or-three-line addition to the existing XML doc** on the test method at `:190-194`, naming
   issue #823 and the artifact path, so an engineer who sees the failure finds the log from the
   failure.

**No executable statement in the test may change.** No sleep, no retry and no timing tolerance is
introduced.

### R5.3 Caveat, recorded honestly

A log inside a feature folder is durable in version control but is not an open register once issue
#823 closes. The orchestrator has decided **not** to promote R5 to its own potential entry on this
branch, because promoting a follow-up from inside this delivery would contradict this feature's own
footprint accounting. If observations continue to accumulate after #823 closes, the next observer
appends to the same artifact path — which is stable after merge — or promotes a fresh record then.

---

## R6 — Deferred upstream, out of scope

The plan-authoring guidance amendment in `.claude/skills/atomic-plan-contract/SKILL.md` belongs
upstream. Everything under `.claude/` in this repository is pushed down from the separate
`drm-copilot` governance repository with no templating, so a fix applied here is overwritten by the
next push-down. R6 is recorded as deferred for that reason. No file under `.claude/` may be changed
by this feature.

---

## Reported-only observations (no change, no promotion on this branch)

Three sibling doc comments in QuickFiler/Viewers/ carry the same class of stale line-count figure
that R4 corrects. They are outside R4's named scope and none of them is changed:

| Comment site | Claimed | Measured |
| --- | --- | --- |
| QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs:11, for BreadcrumbBridgeCoordinator.cs | 487 | 437 |
| QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs:10, for BreadcrumbItemViewerLifecycleCoordinator.cs | 481 | 497 |
| QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs:8 | 477 | not measured |

The second is the one worth a later follow-up, because it **understates** a file that now measures
497 against the repository's 500-line ceiling. It is recorded here and left unchanged.

---

## Root Cause Analysis

Each entry has its own cause; there is no shared root.

- **R1.** The issue-812 fix chose the controller as the scope of the retry budget because the
  observed defect was unbounded retry across repeated re-selections of one store. That scope is
  correct for bounding gestures and incorrect for multiple stores: the budget is a property of the
  store whose lookup failed, not of the controller that happened to display it. Every quantity the
  retry touches — `UserEmailAddress` (`StoreWrapper.cs:174`), `LastSmtpLookupError`
  (`StoreWrapper.cs:184`), and the lookup `GetSmtpAddressFromStore` (`StoreWrapper.cs:207`) — is
  per-store instance state.
- **R2.** Not a defect introduced by this feature. The issue-818 change moved a throw within a method
  it did not guard, disclosed the move, and filed the follow-up.
- **R3.** The XML doc and the test were written from an incorrect reading of the single call site: a
  null receiver consumed by `?.` was mistaken for a null argument.
- **R4.** A line count recorded in prose was not updated when the referenced file shrank during the
  issue-810 work.
- **R5.** Unknown, and deliberately not diagnosed here. The test is probabilistic by construction,
  as its own class doc already records.

---

## Proposed Fix

### Design summary (what changes where)

| Entry | File | Change |
| --- | --- | --- |
| R1 | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | Replace the `bool` at `:105` with a non-static `HashSet<StoreWrapper>` instance field; rewrite the XML doc at `:96-104` |
| R1 | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` | Third conjunct at `:51` becomes a membership test; `:54` becomes an `Add`; rewrite the comment at `:40-47` |
| R1 | `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` | Comment only, `:196-201` |
| R1 | `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` | Add two tests; reword the docs at `:118-121` and `:193-198`; leave the assertions at `:190` and `:222` unchanged |
| R1 | issue-812 living `spec.md` | Append one dated correction block |
| R3 | `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` | Guard at `:44-47` becomes explicit throws; XML doc at `:30-36` rewritten |
| R3 | `QuickFiler/Viewers/QfcFormViewer.cs` | XML doc at `:220-221` only |
| R3 | `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` | Rewrite `Register_NullControlOrNullPredicate_IsIgnored` (`:147-173`) into a rejection test |
| R4 | `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` | One token at `:11`: `480` becomes `459` |
| R5 | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | XML doc at `:190-194` only |
| R5 | new evidence artifact | Seeded observation log |

### Boundaries and invariants to preserve

1. The R1 invariant in section R1.6.
2. The R1 statement ordering in section R1.5: latch before lookup; key derivation after the null
   check; no reset.
3. `Register`'s signature stays non-nullable, and the test file stays nullable-oblivious (R3.6).
4. `AnyOpen` continues to report false when no owner is registered, which is the issue-677 contract.
5. No executable statement changes in the R5 test.
6. No file on the off-limits list is touched.

### Error handling and logging updates

R3 converts one silent-return branch into two explicit `ArgumentNullException` throws. No logging is
added: the registry has no logger and the throw is the diagnostic. R1 adds no error handling; the
existing behaviour of recording the attempt before the lookup runs means a throwing lookup still
consumes that store's attempt, which is preserved deliberately.

### Rollback considerations

No feature flag. Each of R1, R3, R4 and R5 is independently revertible by file.

### Backward-compatibility expectations

`Register` is `internal` on an `internal sealed` class (`BreadcrumbPopupOwnerRegistry.cs:21`),
visible to `QuickFiler.Test` through `QuickFiler/Properties/AssemblyInfo.cs:5`. Converting silent
tolerance into an `ArgumentNullException` is a behavioural change to an internal API with exactly one
production call site, which provably cannot pass null. No public API changes.

### Performance constraints

R1's accepted cost is stated in section R1.7: worst case rises from one blocking SMTP lookup per
dialog open to N, bounded by the number of null-address stores in `Model.Stores`. No other entry has
a performance dimension.

---

## Test Strategy

All tests are MSTest with Moq and FluentAssertions, per `CLAUDE.md` sections CUT1 and CUT2. Every
test is headless: no live Outlook, no window handle, no file, no temporary file.

**R1 — two tests added to `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`.**

1. `PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce` — the fail-before
   test. One controller; two distinct `StoreWrapper` instances, each with its own failing SMTP chain
   built by the existing helper `CreateDisplayFailingSmtpRootFolderWithUser` (`:72-91`). Set
   `Current` to store A, call `PopulateWithCurrent()`, set `Current` to store B, call
   `PopulateWithCurrent()`. Assert `Times.Once()` on **each** store's own
   `ExchangeUser.PrimarySmtpAddress` getter. Against the pre-change tree this fails, with store B's
   getter observed `Times.Never()`. That is the deterministic fail-before proof the repository's
   bugfix workflow requires.
2. `PopulateWithCurrent_OnOneFailingStoreReselectedThreeTimes_RetriesLookupOnlyOnce` — the
   no-regression test for the issue-812 bound. One controller, one store, three
   `PopulateWithCurrent()` calls, asserting `Times.Once()`.

**R1 — four existing tests must remain green with their assertions unchanged.**
`PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce` (`:168`, assertion `Times.Once()`
at `:190`); `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`
(`:200`, assertion `Times.Exactly(2)` at `:222`);
`PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` (`:231`); and
`PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` (`:333`).

**R3 — one test rewritten in place.** `Register_NullControlOrNullPredicate_IsIgnored` (`:154`)
becomes `Register_NullControlOrNullPredicate_IsRejected`, asserting
`.Should().Throw<ArgumentNullException>().WithParameterName("itemViewer")` for the null-control call
and `.WithParameterName("popupIsOpen")` for the null-predicate call, and additionally asserting that
`AnyOpen` is still false afterwards so the registry is provably unchanged by the rejected calls. The
test file does not gain `#nullable enable` (R3.6).

**R4 — no test.** Verification is the toolchain plus the re-measurement named in the acceptance
criterion.

**R5 — no test change.** Observation record and one XML-doc pointer only.

**Coverage.** R3 converts one covered silent branch into one covered throwing branch. R1 adds one
collection-membership branch, covered by both new tests. Neither is expected to move the repository
coverage figure measurably. No production file is added to any coverage exclusion list, and no
threshold is changed.

**Toolchain.** The four steps from `CLAUDE.md` section "C# Toolchain", in that exact order, restarting
from step 1 on any failure or any file rewrite.

---

## Assumptions, Constraints, Dependencies

**Assumptions.**

- The base anchor for this feature's footprint is captured at execution time, not fixed as a commit
  literal in this document. The implementation plan's first phase runs `git rev-parse HEAD` before
  any task has edited a file and records the printed 40-character SHA as the `BASE-SHA` field of its
  Phase 0 branch-and-base baseline artifact. Every acceptance criterion below that compares against
  a base ref writes the token `BASE-SHA` as the diff's left ref operand; the executor substitutes
  the recorded 40-character SHA for that token before running the command. A command left carrying
  the token addresses no commit and fails, which is the intended failure mode rather than a silent
  pass.

  This replaces an earlier fixed-literal anchor, and the reason is load-bearing rather than
  cosmetic. A three-dot diff degenerates into a two-dot diff whenever its left operand is an
  ancestor of its right, because the merge base is then the left operand itself, so a literal anchor
  bills this feature for every commit that lands between that literal and the executor's own work.
  That degeneration was observed here. The anchor was first fixed to the head of
  `epic/review-residuals-2026-09-08-integration` as it stood when this document was drafted, against
  which the three-dot diff then listed one path, this feature's own `issue.md`. The epic-planner
  subsequently fanned sibling preparation work into the same integration branch, and the same
  command on the same branch now lists twenty-one paths: ten under `.claude/agent-memory/`, eight
  owned by the sibling feature folders
  `docs/features/active/2026-09-08-coverage-aggregation-double-counts-method-rows-815/` and
  `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/`, and
  this feature's own three documents. `epic-orchestrator` branches this feature's execution worktree
  from that integration branch at execution time, after further sibling fan-ins, so any literal
  fixed now would be an ancestor of that future head and the same degeneration would recur. A
  self-anchor cannot degenerate: the recorded HEAD is read before this feature's first commit, so it
  is by construction not an ancestor of any commit that is not this feature's own. Under a
  degenerated anchor the footprint criteria AC26, AC27 and AC28 would report sibling-owned and
  epic-owned files against this delivery.
- The line count of `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` does not change before delivery.
  If it does, R4's corrected figure is the count measured at delivery, not the literal 459.

**Constraints.**

- No coverage threshold, analyzer severity, or policy requirement may be lowered, weakened, or
  deleted (epic NFR).
- No `.claude/` file may be changed (R6).
- No off-limits file may be changed.
- No temporary file may be created by any test.

**Known local-environment constraints (environmental, not defects of this change).** Prior runs on
this machine recorded two constraints on `vstest.console.exe` that reproduce on `main` and are not
caused by this feature: assemblies under `.claude\worktrees\` must be excluded from the discovered
set, and CI's `/InIsolation` is required locally; and several `UtilitiesCS.Test` shell-icon test
classes stall the runner because of a `SHGetFileInfo` interaction, so they are excluded by
`TestCaseFilter` locally and are covered by CI. These are recorded as execution notes. They do not
waive any acceptance criterion below, and no test in this feature's own scope is excluded by them.
This paragraph is INFERRED from prior-run records rather than re-verified in this session.

**Dependencies.** None. This feature is wave 0 in its epic with an empty `depends_on`, and its file
set is disjoint from every sibling's.

---

## Data / API / Config Impact

- User-facing changes: none. The address label in the store-settings dialog may now be populated for
  a second store whose lookup previously never ran, which is the intended fix.
- API changes: one internal method's null behaviour (R3.3). No public API changes.
- Data or migration considerations: none. `UserEmailAddress` carries `[JsonIgnore]`, so no persisted
  shape changes.
- Logging/telemetry updates: none.
- Configuration: none. No new setting, no new default.

---

## Acceptance Criteria

All criteria are unchecked. Each is independently verifiable by a named test, a named toolchain
command, or a stated file-and-line observation. Where a criterion carries a numeric value, that value
is either a named test's assertion argument or is re-measurable at check-off by the command named in
the criterion; no criterion asserts a population count that is not enumerated in full in this
document.

Base ref for every diff-based criterion: the token `BASE-SHA`, standing for the 40-character SHA that
the implementation plan's Phase 0 branch-and-base task recorded from `git rev-parse HEAD` before any
task had edited a file. Substitute that literal SHA for the token before running any command below.
See the anchor bullet under "Assumptions" for why the anchor is captured at execution time rather
than fixed as a literal in this document.

### R1 — per-store retry budget

- [ ] **AC1.** A new test `PopulateWithCurrent_OnTwoFailingStoresInOneController_RetriesEachStoreOnce`
      exists in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs`, uses
      one controller and two distinct `StoreWrapper` instances each built with
      `CreateDisplayFailingSmtpRootFolderWithUser`, and passes with `Times.Once()` asserted on each
      store's own `ExchangeUser.PrimarySmtpAddress` getter.
- [ ] **AC2.** Fail-before proof for AC1 is recorded under
      `evidence/regression-testing/` in this feature folder: the same test, run against the tree at
      base ref `BASE-SHA` with only the test added, fails, and the recorded failure text shows store
      B's `PrimarySmtpAddress` getter observed as `Times.Never()`. The artifact carries `Timestamp`,
      `Command`, and `EXIT_CODE` per the evidence conventions.
- [ ] **AC3.** A new test
      `PopulateWithCurrent_OnOneFailingStoreReselectedThreeTimes_RetriesLookupOnlyOnce` exists in the
      same file, calls `PopulateWithCurrent()` three times on one controller with one store, and
      passes with `Times.Once()`.
- [ ] **AC4.** `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore`
      passes, and its assertion argument at
      `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` remains
      `Times.Exactly(2)`. This is the criterion that forbids a `static` latch.
- [ ] **AC5.** `PopulateWithCurrent_WithNullCurrent_RendersPlaceholdersAndDoesNotThrow` passes
      unchanged. This is the criterion that forbids deriving the set key before the
      `Current is not null` conjunct.
- [ ] **AC6.** `PopulateWithCurrent_CalledTwiceOnOneController_RetriesLookupOnlyOnce` and
      `PopulateWithCurrent_WhenUserEmailIsAlreadyPopulated_NeverInvokesExchangeUserLookup` both pass
      unchanged.
- [ ] **AC7.** In `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, the retry state is a
      `HashSet<StoreWrapper>` instance field whose declaration does not contain the `static` keyword,
      and a repo-wide search for the token `_userEmailRetryAttempted;` returns no match.
- [ ] **AC8.** Reading `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.Display.cs` at the
      retry gate confirms both ordering constraints: the membership test is the third conjunct,
      following `Current is not null` and `Current.UserEmailAddress is null` in that order; and the
      `Add` statement precedes the `Current.RefreshUserEmailAddress()` call.
- [ ] **AC9.** Each of the five superseded prose sites listed in section R1.8 has been rewritten to
      state the per-controller-per-store bound, verified by reading each site; and a search scoped to
      the three files `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`,
      `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs` and
      `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Display.cs` for the token
      `not per store` returns no match.
- [ ] **AC10.** The living `spec.md` of the issue-812 feature folder carries one appended dated
      correction block naming issue #823 and the change from a per-controller bound to a
      per-controller-per-store bound; and
      `git diff --name-only BASE-SHA...HEAD -- docs/features/active/2026-09-07-utilitiescs-archive-root-read-and-user-email-retry-801-805-812/`
      lists that `spec.md` and nothing else, with `git status --porcelain` confirming no untracked
      addition under the same path.

### R2 — decision recorded, no code change

- [ ] **AC11.** This specification's R2 section records the intermediate-state decision, the
      unchanged exception type, the deferral to issue #813, and the `:200` correction; and
      `git diff --name-only BASE-SHA...HEAD` does not list
      QuickFiler/Controllers/QfcItemController.FolderHandling.cs, with `git status --porcelain`
      confirming no untracked file at that path.

### R3 — contract reconciliation

- [ ] **AC12.** A test `Register_NullControlOrNullPredicate_IsRejected` exists in
      `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` and passes, asserting
      `ArgumentNullException` with parameter name `itemViewer` for the null-control call, parameter
      name `popupIsOpen` for the null-predicate call, and `AnyOpen` still false after both rejected
      calls.
- [ ] **AC13.** A search scoped to `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` for
      the token `Register_NullControlOrNullPredicate_IsIgnored` returns no match.
- [ ] **AC14.** A search scoped to `QuickFiler.Test/Viewers/BreadcrumbPopupOwnerRegistryTests.cs` for
      the token `#nullable enable` returns no match.
- [ ] **AC15.** Reading `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` and
      `QuickFiler/Viewers/QfcFormViewer.cs` confirms that neither XML doc states that a null argument
      is ignored or tolerated, and neither repeats the form-lookup rationale; a search scoped to those
      two files for the token `Ignored when null` returns no match.
- [ ] **AC16.** `Register` in `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` still declares
      `Control itemViewer` and `Func<bool> popupIsOpen` with no `?` on either type, verified by
      reading the declaration; and the file still carries `#nullable enable` on line 1.

### R4 — line-count comment

- [ ] **AC17.** The parenthetical figure in the comment at
      `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` equals the line count of
      `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` measured at delivery with a whole-file line
      count. The measurement recorded during research and re-measured while authoring this
      specification is 459, so the expected corrected token is `(459 lines)`; a search scoped to
      `QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs` for the token `(480 lines)` returns no
      match, and the delivery measurement is recorded alongside the criterion.
- [ ] **AC18.** `git diff --name-only BASE-SHA...HEAD` lists none of
      QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Search.cs,
      QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs, or
      QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.Focus.cs.

### R5 — observation record

- [ ] **AC19.** The file
      `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`
      exists, names the test
      `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`, declares the per-row schema of
      date, command, assembly set, parallelism setting and failure text, and carries the seeded
      observation row for the issue-810 run recording one failure and three passes.
- [ ] **AC20.** The XML doc on
      `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` at the test
      method names issue #823 and the artifact path from AC19; and
      `git diff BASE-SHA...HEAD -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
      shows added lines that all begin with `///` and shows no removed or added executable statement.
- [ ] **AC21.** A search scoped to
      `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` for each of
      the tokens `Thread.Sleep`, `Task.Delay` and `RetryAttribute` returns no match, confirming that
      no sleep, retry or timing tolerance was introduced.

### Toolchain (run in `CLAUDE.md` order; restart from step 1 on any failure or rewrite)

- [ ] **AC22.** Step 1 — `dotnet tool run csharpier format .` followed by
      `dotnet tool run csharpier check .`, with the check run producing exit code 0 and its output
      recorded showing that it reported no file needing formatting. Evidence under
      `evidence/qa-gates/`.
- [ ] **AC23.** Step 2 —
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
      completes with exit code 0 and 0 errors, and the recorded log contains zero occurrences of
      `Skipping target "CoreCompile"`, proving the rebuild was not vacuous. Evidence under
      `evidence/qa-gates/`.
- [ ] **AC24.** Step 3 —
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
      completes with exit code 0 and 0 errors, with the recorded log containing zero occurrences of
      `Skipping target "CoreCompile"`. This is the confirming gate for the R3.7 prediction of no new
      CS86xx diagnostic and for the R3.6 constraint on the test file. Evidence under
      `evidence/qa-gates/`.
- [ ] **AC25.** Step 4 — `vstest.console.exe` with `/EnableCodeCoverage` over the
      `UtilitiesCS.Test` and `QuickFiler.Test` assemblies completes with zero failed tests, and the
      evidence artifact records the numeric total passed count, the numeric failed count, and the
      numeric line-coverage and branch-coverage percentages read from the generated coverage report.
      Evidence under `evidence/qa-gates/`.

### Footprint

- [ ] **AC26.** `git diff --name-only BASE-SHA...HEAD` lists no path beginning with `.claude/`, and
      does not list `CLAUDE.md`, and lists no path beginning with `docs/features/epics/`;
      `git status --porcelain` confirms no untracked addition under any of those paths.
- [ ] **AC27.** `git diff --name-only BASE-SHA...HEAD` lists no file from the off-limits list in the
      Scope & Non-Goals section, and lists no `.csproj` file; `git status --porcelain` confirms no
      untracked addition at any of those paths.
- [ ] **AC28.** Every path listed by `git diff --name-only BASE-SHA...HEAD`, together with every
      untracked path reported by `git status --porcelain`, appears in the Write Set section of this
      specification or is a file inside
      `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/`.
- [ ] **AC29.** `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/user-story.md`
      does not exist, confirmed by a directory listing of the feature folder. `full-bug` work mode
      requires its absence.

---

## Risks & Mitigations

| # | Risk | Mitigation |
| --- | --- | --- |
| 1 | R1 raises the worst-case number of blocking UI-thread SMTP lookups per dialog open from one to N. | Accepted and stated in R1.7. The bound is set by configuration, not by user gestures, which is the property issue #812 was defending. The latch is still recorded before the lookup, so a throwing lookup cannot be retried. |
| 2 | A future maintainer converts the set to `static` to "reduce" the cost, silently reducing the bound to once per process. | AC4 fails in that case, because `PopulateWithCurrent_OnASecondControllerOverTheSameFailingStore_RetriesOnceMore` would observe 1 rather than 2. |
| 3 | A future maintainer keys the set on `StoreId`, reintroducing the shared-budget defect for stores with unreadable ids. | Rationale recorded in R1.3; AC7 pins the field type to `HashSet<StoreWrapper>`. |
| 4 | R3's explicit throw reaches production and crashes the breadcrumb configuration path. | Unreachable: the sole call site passes `this` and a lambda literal, both provably non-null (R3.2). If a null ever did arrive, the throw is a louder and earlier failure than the `NullReferenceException` in `AnyOpen` that today's silent store would produce (R3.5). |
| 5 | Adding `#nullable enable` to the rewritten test file breaks the nullable gate with CS8625. | Prohibited by R3.6 and pinned by AC14; AC24 is the confirming gate. |
| 6 | The R5 log stops being an open register once issue #823 closes. | Acknowledged in R5.3. The artifact path is stable after merge and the test's XML doc points at it, so the next observer can append or promote a fresh record. |
| 7 | The R4 figure goes stale again if `BreadcrumbDropDownHost.cs` changes before merge. | AC17 asserts equality against a measurement taken at delivery, not against a literal fixed at authoring time. |

---

## Rollout & Follow-up

- Release: one pull request from this feature branch into the epic integration branch
  `epic/review-residuals-2026-09-08-integration`. No staged rollout and no feature flag.
- Post-fix monitoring: none required. R5's observation log is the only ongoing record, and it is
  passive.
- Follow-ups deliberately not filed on this branch: R6 (belongs in `drm-copilot`); the stale
  line-count figure in QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.Search.cs; and a
  standing register for the R5 intermittent failure. Each is recorded above with its reason.
- Links: issue #823 (https://github.com/drmoisan/TaskMaster/issues/823); sibling issue #813; epic
  `review-residuals-2026-09-08`; research record `research/research.2026-09-08T23-50.md`.
