# Readiness-State Semantics Research — Issue #287

Timestamp: 2026-08-31T21-10

- **Issue:** #287 (`storewrapper-dialog-imprecise-for-genuine-failure`)
- **Branch:** `bug/storewrapper-dialog-imprecise-for-genuine-failure-287`
- **Base commit:** `2b85134b42872e405602e6064e02dc9cda6c319b` (from `origin/main`)
- **Scope:** research only. No production, configuration, or test file was modified.

## Tooling constraint affecting this artifact

`Bash` was unavailable in this session (`Error: No such tool available: Bash. Bash is disabled for this
session, in subagents as well as here.`). No `git log`, `git log -S`, `msbuild`, or `vstest.console.exe`
command could be executed. Consequently:

- Section 5 (History) is reconstructed from in-repo feature documents, which are versioned artifacts of
  the same commits, rather than from commit metadata. Commit SHAs and authors are therefore **not**
  recorded and are marked as an open item.
- No claim in this artifact rests on a test run performed during this session. Statements about test
  outcomes are attributed to in-repo evidence artifacts and marked INFERENCE where they extrapolate to
  the current tree.

---

## 1. VERIFIED — Current state of the gate

Both launch paths carry a byte-identical readiness gate and dialog.

| Path | Gate | Message literal | Title literal |
|---|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:120` | `readiness.State != StoreLaunchReadinessState.Ready` | `:123` | `:124` |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:164` | `readiness.State != StoreLaunchReadinessState.Ready` | `:167` | `:168` |

Both messages read: `Store settings are not available yet. Please try again after startup completes.`
Both titles read: `Store Settings Unavailable`.

`StoreLaunchReadinessState` is declared at `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:19-24`
with exactly three members: `Ready`, `ModelUnavailable`, `StoresUnavailable`.

`StoreLaunchReadinessEvaluator.Evaluate` (`UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs:22-39`):

- `globals?.Ol?.StoresWrapper is null` → `ModelUnavailable` (`:27`)
- `model.Stores is null` → `StoresUnavailable` (`:32`)
- otherwise → `Ready` with the model and store display names (`:35-38`)

Both `Launch()` methods carry `[ExcludeFromCodeCoverage]`
(`StoreWrapperController.cs:116`, `DisabledStoresController.cs:160`).

Entry points (VERIFIED): `TaskMaster/Ribbon/RibbonController.cs:259-262` constructs
`StoreWrapperController` and calls `Launch()`; `TaskMaster/Ribbon/RibbonController.cs:267` constructs
`DisabledStoresController` and calls `Launch()`. Both are reached from
`TaskMaster/Ribbon/RibbonViewer.cs:202` and `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs:219`.
Neither entry point performs any readiness check of its own.

---

## 2. VERIFIED — Complete assignment inventory for the two null-able values

### 2.1 Assignments to `IOlObjects.StoresWrapper`

The interface member is declared at `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs:24`
(`public StoresWrapper StoresWrapper { get; set; }`). The only production implementation is the
auto-property at `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:25`, which has no backing-field
declaration and no initializer, so its default value is `null`.

**Production assignments — exactly two, both in `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs`:**

| # | Location | Path | Reachability |
|---|---|---|---|
| 1 | `:47` `StoresWrapper = deserialized;` | valid persisted config, non-null deserialize | followed at `:48` by `await AwaitStoreRewireAsync(StoresWrapper)` |
| 2 | `:64` `StoresWrapper = BuildFreshStoresWrapper();` | config key absent (`:57`) **or** deserialize returned null (`:51-53`) | synchronous `new StoresWrapper(_globals).Init()` (`:32-33`) |

Non-assignments that a name-only search would falsely include:

- `TaskMaster/AppGlobals/AppOlObjects.cs:101` — `var storesWrapper = StoresWrapper ?? new StoresWrapper() { };`
  This is a **local** fallback inside `LoadInboxes()`. It does not write the property. Verified by reading
  `AppOlObjects.cs:99-123`.
- `TaskMaster/AppGlobals/ApplicationGlobals.StoreRehook.cs:46` —
  `store => _olObjects.StoresWrapper?.AddOrRestoreStore(store)`. This is a null-conditional **read**. The
  runtime rehook coordinator never repopulates a null model.
- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs:42` — test only.

SearchScope: whole worktree.
SearchPatterns: `StoresWrapper\s*=[^=]` over `*.cs`; independently `StoresWrapper` over `UtilitiesCS/**/*.cs`
and over the whole worktree.
SearchResult: 3 assignment-shaped hits (2 production + 1 test), listed above. No assignment to
`IOlObjects.StoresWrapper` exists in `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskVisualization`, or `Tags`.
Known-positive control for that absence: the same pattern does return
`TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:47` and `:64`, proving the pattern matches real
assignments; and the un-anchored `StoresWrapper` search does return 30+ hits inside `UtilitiesCS`
(declarations, reads, doc comments), proving the trees were in scope.

### 2.2 Assignments to `StoresWrapper.Stores`

`Stores` is declared at `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:362-363` as
`[JsonProperty] public List<StoreWrapper>? Stores { get; set; }` — nullable, **no initializer**, so the
parameterless constructor leaves it null.

**Production assignments — exactly three, all in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`:**

| # | Location | Member | Shape |
|---|---|---|---|
| 1 | `:49` | `Init()` | `Stores = filteredStores.Select(...).ToList();` — unconditional overwrite |
| 2 | `:87` | `RewireOlObjectsAsync(StreamingContext)` | `this.Stores ??= [];` — **first statement of the method** |
| 3 | `:136` | `AddOrRestoreStore(Outlook.Store)` | `Stores ??= [];` — first statement of the method |

Plus one implicit writer: Newtonsoft deserialization, because the property carries `[JsonProperty]`
(`:362`). A payload without a `Stores` key, or with `"Stores": null`, leaves it null at the end of member
population.

SearchScope: whole worktree.
SearchPatterns: `Stores\s*=[^=]|Stores\s*\?\?=` over `*.cs`.
SearchResult: 3 production hits (above) plus test-only hits in `TaskMaster.Test`, `UtilitiesCS.Test`, and
`ToDoModel.Test`. Known-positive control: the same query returns unrelated but real assignments
(`ExcludeGwsoStores = false`, `CoversAllStores = ...`), confirming the pattern is not over-anchored.

### 2.3 VERIFIED — The rewire ordering that governs `StoresUnavailable`

Two independent triggers set `Stores` non-null after a deserialize:

1. `[OnDeserialized] RewireOlObjects` (`StoresWrapper.cs:62-66`) fires and forgets
   `RewireAfterDeserializeWithLoggingAsync()`, which awaits `RewireAfterDeserializeAsync()` →
   `RewireOlObjectsAsync(default)` (`:68-71`, `:85`).
2. `AppOlObjects.LoadStoresAsync` explicitly awaits `AwaitStoreRewireAsync(StoresWrapper)`
   (`AppOlObjects.StoreLoading.cs:48`), which forwards to the same `RewireAfterDeserializeAsync()`
   (`:27-30`).

`RewireOlObjectsAsync` performs `this.Stores ??= [];` at `StoresWrapper.cs:87` — before the first
`Stopwatch`, before `MaterializeFilteredStores()` (`:90`), and before the first `await Task.Yield()`
(`:100`). Therefore **any** entry into the rewire, even one that subsequently throws a `COMException`
while enumerating `Namespace.Stores`, leaves `Stores` non-null (at minimum an empty list).

`Init()` (`:37-51`) assigns `Stores` at `:49`. If `MaterializeFilteredStores()` (`:45`) throws first, the
assignment never happens and the exception propagates out of `Init()`, out of `BuildFreshStoresWrapper()`,
and into the `LoadStoresAsync` catch block — leaving the **property** null rather than leaving a
partially-built model in place.

---

## 3. VERIFIED — Answer to the central research question

### 3.1 Can `ModelUnavailable` be permanent? **Yes.**

`TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:35-73` wraps the entire store-load body in a single
`try`/`catch (Exception)`. The catch (`:66-72`) logs and returns:

```
"Failed to load StoresWrapper; store settings will remain unavailable until this is resolved. {e.Message}"
```

There is no retry, no fallback assignment inside the catch, and no re-entry point. `LoadStoresAsync` is
called exactly once per session, from `LoadAsync()` (`:19-23`), which is invoked once from
`TaskMaster/ThisAddIn.cs:76` (`await _globals.LoadAsync(false)`) inside a single
`IdleAsyncQueue.AddEntry` continuation (`ThisAddIn.cs:71-88`). No production code re-invokes it.

Reachable throw sites inside the `try` that leave the property null:

- `_globals.IntelRes.Config.TryGetValue(...)` (`:39`) — throws if `IntelRes` or `Config` is null.
- `SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config)` (`:41-44`).
- `BuildFreshStoresWrapper()` (`:64`) → `new StoresWrapper(_globals).Init()` (`:32-33`) → a COM failure
  enumerating `Globals.Ol.NamespaceMAPI.Stores` (`StoresWrapper.cs:188-193`).

This behaviour is **codified in an existing regression test**:
`TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:146-184`,
`LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull`, whose comment reads
"Path 3 - genuine failure" and whose assertions are `sut.StoresWrapper.Should().BeNull()` and
`BuildFreshStoresWrapperInvocationCount.Should().Be(0, "there is no fresh-build retry after a
mid-deserialize exception.")`.

`ModelUnavailable` is therefore **both** transient and permanent depending on cause:

- Transient: the ribbon is clicked before the `IdleAsyncQueue` continuation has run `LoadStoresAsync`.
  This window is real and can be long on a cold start.
- Permanent: `LoadStoresAsync` completed via its catch block. A user retry cannot change the outcome for
  the remainder of the Outlook session.

The state alone does not distinguish which of the two occurred.

### 3.2 Can `StoresUnavailable` be permanent? **No path was found that makes it permanent.**

To observe `StoresUnavailable`, `Globals.Ol.StoresWrapper` must be non-null while its `Stores` is null.
The only production writer of the property is `LoadStoresAsync`, and both of its assignments close that
window:

- Assignment 1 (`:47`) is immediately followed by an awaited rewire (`:48`) whose very first statement is
  `Stores ??= []`. Additionally, the `[OnDeserialized]` callback has already started the same rewire
  during `Deserialize` itself.
- Assignment 2 (`:64`) assigns the result of `Init()`, which has already set `Stores` at
  `StoresWrapper.cs:49`; if `Init()` throws before that line, no assignment occurs at all and the state is
  `ModelUnavailable`, not `StoresUnavailable`.

The residual `StoresUnavailable` window is the interval between the property write at `:47` and the
`Stores ??= []` at `StoresWrapper.cs:87`. That is a race window measured in the code path between two
adjacent statements, closed by an awaited call in the same method. It is genuinely transient, and it
closes without user action.

**No production mechanism was found that leaves `Stores` null indefinitely on a non-null model.**

SearchScope: whole worktree.
SearchPatterns: `Stores\s*=[^=]|Stores\s*\?\?=` over `*.cs`; `new StoresWrapper|override.*Init\(\)` over
`*.cs`.
SearchResult: no production override of `StoresWrapper.Init()` exists; the only production
`new StoresWrapper(...)` outside tests is `AppOlObjects.StoreLoading.cs:33` (which calls `.Init()`) and the
non-assigning local at `AppOlObjects.cs:101`. Known-positive control: the same `new StoresWrapper` query
returns 25+ test-side constructions, confirming the pattern matches.

### 3.3 Can BOTH be permanent? Can NEITHER?

- **Both permanent: no.** Only `ModelUnavailable` has a demonstrated permanent cause.
- **Neither permanent: no.** `ModelUnavailable` has a demonstrated, test-codified permanent cause.

**Direct answer to the framing in the issue:** the transient/permanent split does **not** align with the
`ModelUnavailable` / `StoresUnavailable` boundary. It aligns with the *cause* of `ModelUnavailable`, which
the enum does not record. `StoresUnavailable` — the state the issue text identifies as the genuine-failure
case — is the one state that is unambiguously transient.

### 3.4 VERIFIED — Are the two states distinguishable in a way that should change the user action?

They are distinguishable at the code level, and the distinction is meaningful, but not in the direction the
issue assumes.

| Observed state | What it means about the model | Correct user guidance |
|---|---|---|
| `StoresUnavailable` | The model object exists and deserialized; only the store list has not been filled in yet by the rewire that is already running | Retry shortly. Retrying is the correct advice, and it will succeed. |
| `ModelUnavailable` | The model object was never assigned. Either startup has not reached the store-load phase, or the store-load phase completed through its catch block. | Ambiguous with the state alone. Retry may work (startup still running) or may never work (load already failed). |

The one place the two states genuinely differ for the user is: `StoresUnavailable` guarantees the load
succeeded far enough to produce a model, so the current "try again after startup completes" wording is
*accurate* for that state. `ModelUnavailable` is the state whose wording is imprecise.

Two supporting observations relevant to the choice of copy:

- The permanent `ModelUnavailable` case always writes an `Error`-level log line with the exception attached
  (`AppOlObjects.StoreLoading.cs:68-71`). Pointing the user at the log is actionable for that case and only
  that case.
- The repository already models a transient/permanent outcome split explicitly elsewhere:
  `TaskMaster/AppGlobals/StoreRehookCoordinator.cs` has `StoreRehookOutcome.TransientTimeout` (`:176`) and
  `StoreRehookOutcome.PermanentError` (`:186`) with distinct log copy per outcome (`LogOutcome`,
  `:208-233`). That is in-repo precedent for naming the distinction rather than inferring it from a
  null-shape.

---

## 4. INFERENCE — Design implication (not a decision)

If the goal is "do not tell a user to retry when retrying cannot help", then branching the message on
`StoreLaunchReadinessState` alone **cannot fully achieve it**, because the permanent case and one transient
case both surface as `ModelUnavailable`. Three options follow, in increasing scope:

- **(A) Copy-only, state-branched.** Give `StoresUnavailable` a "finishing loading, try again shortly"
  message and `ModelUnavailable` a message that covers both sub-cases without asserting either
  (for example, one that mentions the log). Smallest change; fully satisfied by a pure helper. Does not
  distinguish the two `ModelUnavailable` sub-causes.
- **(B) Copy-only, single message.** Rewrite the one message so it is accurate for all three non-`Ready`
  cases. Smallest possible diff; loses the accurate "retry shortly" guidance that `StoresUnavailable`
  legitimately deserves.
- **(C) Add a fourth state.** Record load failure at the point it happens (a flag or a
  `LoadFailed` state set inside the `LoadStoresAsync` catch) so the evaluator can return a distinct
  permanent state. This is the only option that fully removes the ambiguity, and it requires editing
  `AppOlObjects.StoreLoading.cs` and the `IOlObjects`/evaluator contract — materially larger than a copy
  fix and beyond what the issue scopes.

Option (A) is the smallest change that improves accuracy without asserting anything unverified. Option (C)
is recorded here because the issue's stated goal is only fully reachable through it; whether to take it is
a scope decision for the spec, not a research conclusion.

---

## 5. History and original author intent

Commit metadata could not be retrieved (see the tooling-constraint note). The following quotations come
from in-repo, version-controlled feature documents.

### 5.1 Origin of the enum, the evaluator's predecessor, and the dialog copy — issue #240

`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/research/2026-07-06T00-00-store-wrapper-launch-npe-240-research.md`
is the design document for the guard. It ranks four causes (`:76-88`):

> 1. **(a) Ribbon invoked before `IdleAsyncQueue` `LoadStoresAsync` completes — most likely, operative.**
> 2. **(b) `LoadStoresAsync` config-missing branch leaves `StoresWrapper` null — likely in misconfigured
>    sessions, deterministic (not a race).** ... "`StoresWrapper` stays null for the entire session. ...
>    and is permanent, so the dialog can never open in such a session."
> 3. **(c) Deserialization returning null — possible edge, reachable.**
> 4. **(d) Non-null `StoresWrapper` with transiently-null `Stores` — latent secondary, narrower race.**

The same document supplies the exact dialog wording as an example, at `:104`:

> present a clear user-facing message via the `MyBox` surface (e.g., "Store settings are not available yet.
> Please try again after startup completes.")

And it states the rewire ordering that this research independently re-verified, at `:70`:

> once `LoadStoresAsync` completes, `Stores` is non-null. The transient window is: `StoresWrapper` assigned
> ... but the awaited rewire ... has not yet run and the `[OnDeserialized]` fire-and-forget has not yet
> completed.

**Author intent, as written:** `ModelUnavailable` was designed to cover causes (a), (b), and (c) —
explicitly a mix of one transient and two permanent causes. `StoresUnavailable` was designed to cover cause
(d) only, explicitly described as a "narrower race". The dialog copy was written as one message for all
non-`Ready` states because at the time the guard was purely about not crashing:
`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/issue.md:76` states AC2 as
"handles a non-null `Model` whose `Stores` list is null (transient post-deserialize state) without
throwing."

The #240 research also records why the readiness decision was extracted out of `Launch()` at all
(`:114-120`):

> `Launch()` is `[ExcludeFromCodeCoverage]` ... A guard written inline in `Launch()` would be excluded from
> coverage ... Extract the readiness decision into a small non-exempt, testable member ... Keep `Launch()`
> as the thin, coverage-exempt shell.

That constraint is unchanged today and governs where any new message-selection logic must live.

### 5.2 Why causes (b) and (c) stopped being permanent — issue #262

`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md` restructured
`LoadStoresAsync`. It names three null paths (`:77-95`) and classifies them:

> 1. **Path 1 — config missing.** ... `StoresWrapper` ... is never assigned and stays null for the session.
> 2. **Path 2 — null deserialize.** ... `StoresWrapper` remains null.
> 3. **Path 3 — exception during load.** ... This is the bare, unattributed failure surface AC3 targets.

Paths 1 and 2 were converted to a fresh-build fallback (AC1, AC2, `:297-309`). Path 3 was deliberately left
permanent (`:200-203`):

> There is **no retry** inside the catch — if `BuildFreshStoresWrapper()` itself throws, the same catch
> reports it once; no second fallback attempt. On the genuine-failure path `StoresWrapper` remains null, the
> readiness guard still reports `ModelUnavailable`, and the existing dialog remains the single user-facing
> surface (AC3).

That sentence is the authoritative statement of which state the permanent case surfaces as:
**`ModelUnavailable`, not `StoresUnavailable`.**

### 5.3 The deferred follow-up that became #287

The same spec scopes the copy change out, twice, in identical terms.

`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md:60-62` (Non-Goals):

> - Changing the `StoreWrapperController` "not available yet" dialog copy for the genuine-failure case
>   (imprecise but not required by any AC; documented follow-up only).

`:360-361` (Rollout & Follow-up):

> Follow-up (not required by any AC): consider revising the `StoreWrapperController` "not available yet"
> dialog copy for the genuine-failure case, whose current wording implies a timing issue.

`docs/features/epics/store-lockup-resilience/epic-plan.md:57-63` records the epic-level root-cause note:

> The reported symptom ("the settings table is never notified that startup completed") does not match the
> code. ... The dialog appears because `Globals.Ol.StoresWrapper` is genuinely null for the whole session,
> caused upstream in `AppOlObjects.LoadStoresAsync`.

### 5.4 Origin of the second copy site — issue #265

`docs/features/archive/2026-07-07-disabled-stores-settings-ui-265/plan.2026-07-07T18-00.md:68` contains
both literals, and `DisabledStoresController.cs:154-158` documents the intent:

> Applies the shared readiness gate; when the model is not ready it shows the same warning as the
> single-store editor.

This is the reason there are two copy sites, and it means any copy change must be applied to both or the
two settings dialogs will diverge.

**Open item:** commit SHAs, dates, and authors for the introduction of `StoreLaunchReadinessState`,
`StoreLaunchReadinessEvaluator.cs`, and the dialog literals were not retrieved. Recover with
`git log -S "StoreLaunchReadinessState"`, `git log -S "StoreLaunchReadinessEvaluator"`, and
`git log -S "Store settings are not available yet"` in a session where a shell is available.

---

## 6. Exhaustive occurrence inventory

All counts below were produced with two differently-shaped searches. Scope for every count is the entire
worktree unless a narrower scope is stated.

### 6.1 Literal `Store settings are not available yet`

Primary search — exact literal, all file types, whole worktree.
SearchScope: worktree root.
SearchPatterns: `Store settings are not available yet`.
SearchResult: **20 matching lines across 14 files.**

| # | Path:line | Kind |
|---|---|---|
| 1 | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:123` | production |
| 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:167` | production |
| 3 | `docs/features/epics/store-lockup-resilience/epic-plan.md:46` | docs |
| 4 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/spec.md:31` | docs |
| 5 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/spec.md:39` | docs |
| 6 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/issue.md:28` | docs |
| 7 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/issue.md:38` | docs |
| 8 | `docs/features/potential/promoted/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure.md:26` | docs |
| 9 | `docs/features/potential/promoted/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure.md:36` | docs |
| 10 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md:12` | docs |
| 11 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md:35` | docs |
| 12 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/research/2026-07-07-folder-settings-store-model-null-research.md:222` | docs |
| 13 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/issue.md:17` | docs |
| 14 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/issue.md:33` | docs |
| 15 | `docs/features/archive/2026-07-07-folder-settings-store-model-null-262/evidence/other/ac4-controller-unchanged.md:14` | docs |
| 16 | `docs/features/archive/2026-07-07-disabled-stores-settings-ui-265/plan.2026-07-07T18-00.md:68` | docs |
| 17 | `docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/research/2026-07-06T00-00-store-wrapper-launch-npe-240-research.md:104` | docs |
| 18 | `docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/plan.2026-07-06T06-41.md:46` | docs |
| 19 | `docs/features/archive/2026-07-07-store-lockup-resilience-260/issue.md:25` | docs |
| 20 | `docs/features/archive/2026-07-07-store-lockup-resilience-260/issue.md:52` | docs |

Production `*.cs` subtotal: **2** (rows 1-2). Test `*.cs` subtotal: **0**.

Cross-check search — different shape (case-insensitive substring, narrower `*.cs` scope, shorter phrase).
SearchScope: worktree root, `*.cs` only.
SearchPatterns: `not available yet` with `-i`.
SearchResult: **4 lines**, of which exactly **2** are the store copy (`StoreWrapperController.cs:123`,
`DisabledStoresController.cs:167`).

Known-positive control for the "0 test occurrences" absence claim: the same case-insensitive `*.cs` search
returns two unrelated true positives — `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:346` and
`TaskMaster/Ribbon/EngineGatedCommandRunner.cs:132` — proving the query matched real message literals
outside the store files and did not silently fail.

**Member-set comparison:** the two production members from the primary search
(`StoreWrapperController.cs:123`, `DisabledStoresController.cs:167`) are set-identical to the two
store-copy members from the cross-check search. Counts agree at 2 in `*.cs`.

### 6.2 Literal `Store Settings Unavailable`

Primary search — exact literal, all file types, whole worktree.
SearchScope: worktree root.
SearchPatterns: `Store Settings Unavailable`.
SearchResult: **7 matching lines across 7 files.**

| # | Path:line | Kind |
|---|---|---|
| 1 | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:124` | production |
| 2 | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:168` | production |
| 3 | `docs/features/potential/promoted/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure.md:26` | docs |
| 4 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/spec.md:31` | docs |
| 5 | `docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/issue.md:28` | docs |
| 6 | `docs/features/archive/2026-07-07-disabled-stores-settings-ui-265/plan.2026-07-07T18-00.md:68` | docs |
| 7 | `docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/plan.2026-07-06T06-41.md:46` | docs |

Production `*.cs` subtotal: **2** (rows 1-2). Test `*.cs` subtotal: **0**.

Cross-check search — different shape (case-insensitive, `*.cs` only, dropped leading word).
SearchScope: worktree root, `*.cs` only.
SearchPatterns: `settings unavailable` with `-i`.
SearchResult: **2 lines** — `StoreWrapperController.cs:124`, `DisabledStoresController.cs:168`.

**Member-set comparison:** identical two-member sets. Counts agree at 2 in `*.cs`.

Known-positive control for the "not in any `.resx`" claim below: the `*.resx` inventory query does return
24 files under `UtilitiesCS/`, so the resource tree was in scope.

### 6.3 `StoreLaunchReadinessEvaluator.Evaluate` call sites

Primary search — type-name, code scope.
SearchScope: worktree root, `*.cs` and `*.csproj`.
SearchPatterns: `StoreLaunchReadinessEvaluator`.
SearchResult: **4 lines across 4 files.**

| Path:line | Role |
|---|---|
| `UtilitiesCS/UtilitiesCS.csproj:744` | `<Compile Include="OutlookObjects\Store\StoreLaunchReadinessEvaluator.cs" />` |
| `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs:13` | class declaration |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:111` | **call site** (inside `EvaluateLaunchReadiness()`) |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:163` | **call site** (inside `Launch()`) |

Call sites of `StoreLaunchReadinessEvaluator.Evaluate`: **2**, both production. Test projects call the
controller wrapper, never the static directly.

Cross-check search — different shape (case-insensitive, unrestricted file types).
SearchScope: worktree root, all file types.
SearchPatterns: `storelaunchreadinessevaluator` with `-i`.
SearchResult: the same 4 code lines, plus `<class ... name="UtilitiesCS.OutlookObjects.Store.StoreLaunchReadinessEvaluator" ...>`
rows inside archived Cobertura XML evidence files under
`docs/features/archive/2026-06-19-tesseract-engine-initialization-failure-209/evidence/`. Those are
coverage artifacts, not references. Excluding artifact XML, the member sets are identical.

**Member-set comparison:** normalized code-scope member sets are identical (4 lines / 2 call sites). The
count difference between the two searches is fully explained by archived coverage XML and does not affect
the call-site count.

### 6.4 `EvaluateLaunchReadiness` references

SearchScope: worktree root, `*.cs`.
SearchPatterns: `EvaluateLaunchReadiness`.
SearchResult: **14 lines across 2 files.**

Production (`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`) — 3:
- `:27` — doc-comment `<see cref="StoreWrapperController.EvaluateLaunchReadiness"/>`
- `:109` — declaration `internal StoreLaunchReadiness EvaluateLaunchReadiness()`
- `:119` — **the only production call site**

Test (`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs`) — 11:
- `:102` region comment
- `:110`, `:127`, `:146`, `:168`, `:189` — five test-method names
- `:116`, `:135`, `:156`, `:178`, `:205` — five invocations

Cross-check search — different shape (local-variable binding rather than method name).
SearchScope: worktree root, `*.cs`.
SearchPatterns: `var readiness =`.
SearchResult: **36 lines**, of which **7** bind a `StoreLaunchReadiness`: production
`StoreWrapperController.cs:119` and `DisabledStoresController.cs:163`; test `…Launch.cs:116,135,156,178,205`.
The other 29 bind unrelated `BreadcrumbNavigationReadiness`, `TaskCompletionSource<bool>`, and
`HookReadinessCoordinator` values in `QuickFiler.Test` and `TaskMaster/AppGlobals/StoreRehookCoordinator.cs:152`.

**Member-set comparison:** the five test invocation lines are set-identical across both searches
(`:116,135,156,178,205`). The production call-site counts differ by construction and reconcile exactly:
the method-name search finds 1 (`StoreWrapperController.cs:119`, the wrapper call) while the
variable-binding search finds 2 (that line plus `DisabledStoresController.cs:163`, which calls the
evaluator static directly and therefore never mentions `EvaluateLaunchReadiness`). Both searches agree
that there are exactly **2 production readiness evaluations** in the codebase.

### 6.5 `StoreLaunchReadinessState` and its three members

Primary search — the shared type-name prefix, code scope.
SearchScope: worktree root, `*.cs`.
SearchPatterns: `StoreLaunchReadiness` (matches the enum, the struct, and the evaluator).
SearchResult: **28 lines** total; of these, **18 lines mention `StoreLaunchReadinessState`.**

`StoreLaunchReadinessState` references in `*.cs` — 18:

| File | Lines | Count |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 19, 34, 44, 50, 64, 103, 120 | 7 |
| `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs` | 17, 18, 19, 27, 32 | 5 |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | 164 | 1 |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` | 119, 138, 159, 181, 208 | 5 |

Cross-check search — three independent per-member queries, all file types, whole worktree. This is the
exhaustive-family check: the enum has exactly three members
(`StoreWrapperController.cs:21-23`), and each is queried separately.

| Member | SearchPattern | Repo-wide occurrences / files | `*.cs` members |
|---|---|---|---|
| `Ready` | `StoreLaunchReadinessState\.Ready` | 11 / 9 | 6 — `StoreWrapperController.cs:64,103,120`; `StoreLaunchReadinessEvaluator.cs:19`; `DisabledStoresController.cs:164`; `…Launch.cs:208` |
| `ModelUnavailable` | `StoreLaunchReadinessState\.ModelUnavailable` | 7 / 4 | 5 — `StoreLaunchReadinessEvaluator.cs:17,27`; `…Launch.cs:119,138,159` |
| `StoresUnavailable` | `StoreLaunchReadinessState\.StoresUnavailable` | 5 / 4 | 3 — `StoreLaunchReadinessEvaluator.cs:18,32`; `…Launch.cs:181` |

Repo-wide totals across the complete three-member family: **23 occurrences**. The 5 non-`*.cs`
occurrences are in `docs/` (the #287 issue/spec/promoted entries and the archived #240 / #265 plans).

**Member-set comparison:** member-qualified `*.cs` lines sum to 6 + 5 + 3 = **14**. The primary search's
18 `*.cs` lines minus those 14 leaves exactly **4** bare type-name uses, which are enumerable and account
for the difference: `StoreWrapperController.cs:19` (enum declaration), `:34` (constructor parameter),
`:44` (`State` property type), `:50` (`NotReady` parameter). 14 + 4 = 18. The two independently-derived
sets reconcile exactly with no residue.

### 6.6 Methods that read `readiness.State`

SearchScope: worktree root, all file types.
SearchPatterns: `readiness\.State`.
SearchResult: **12 lines**, of which 5 are docs prose and 7 are `*.cs`.

Production methods that read `readiness.State`: **2.**

| Method | Location |
|---|---|
| `StoreWrapperController.Launch()` | `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs:120` |
| `DisabledStoresController.Launch()` | `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:164` |

Test methods that read `readiness.State`: **5**, all in
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` at `:119, :138, :159, :181, :208`.

Cross-check — the declaration side. `StoreLaunchReadiness.State` is declared exactly once
(`StoreWrapperController.cs:44`) and there is exactly one other way to reach it: through a local of that
struct type. The `var readiness =` search (6.4) finds exactly 7 such locals in `*.cs` (2 production,
5 test), matching the 7 `*.cs` reads one-for-one.

**Member-set comparison:** the 7-line member sets from `readiness\.State` and from
`var readiness =` (filtered to `StoreLaunchReadiness` bindings) are identical. Counts agree at 2 production
methods and 5 test methods.

---

## 7. Testability precedent (item 7)

### 7.1 VERIFIED — what a test can observe from the invoked viewer

`UtilitiesCS/Dialogs/MyBox.cs:41-45` declares the seam:

```
internal static Func<MyBoxViewer, DialogResult> DialogInvoker
```

backed by an `AsyncLocal<Func<MyBoxViewer, DialogResult>>` (`:30-31`) with `RealDialogInvoker` as the
fallback (`:33-34`). The in-source comment (`:26-29`) states the storage is per-async-flow specifically so
that `ClassLevel` parallel test classes do not race on the seam.

The overload that both controllers call is
`MyBox.ShowDialog(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)`
(`:129-139`). It constructs `using MyBoxViewer viewer = new();` (`:137`) and delegates to the
viewer-accepting overload at `:112-127`, which performs, in order:

1. `ReplaceButtons(viewer, actionButtons)` (`:120`)
2. `viewer.Text = Title;` (`:121`) — **the title**
3. `viewer.TextMessage.Text = Message;` (`:122`) — **the message**
4. `viewer.SetDialogIcon(icon);` (`:123`)
5. `viewer.TopMost = true;` (`:124`)
6. `DialogResult result = DialogInvoker(viewer);` (`:125`)

**Confirmed:** the hypothesis in the delegation prompt is correct. `viewer.Text` is the title and
`viewer.TextMessage.Text` is the message, and both are assigned *before* `DialogInvoker` is called, so a
capturing invoker observes the final values. `TextMessage` is declared at
`UtilitiesCS/Dialogs/MyBoxViewer.Designer.cs:175` as `internal System.Windows.Forms.TextBox TextMessage;`
and is reachable from `UtilitiesCS.Test` via `InternalsVisibleTo`.

**A test can therefore assert the exact dialog copy end-to-end through `Launch()`.** The established
capturing pattern already exists in-repo at
`UtilitiesCS.Test/EmailIntelligence/AutoFile_Tests.cs:264` (`MyBox.DialogInvoker = viewer => { ... }`) and
`UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/ClassifierGroups_Tests.cs:905`.

### 7.2 VERIFIED — what gets constructed when it does

`new MyBoxViewer()` runs `InitializeComponent()` (`UtilitiesCS/Dialogs/MyBoxViewer.cs:20-23`), building the
Designer control tree (a `Form`, nested `TableLayoutPanel`s including `L2Bottom`, the `TextMessage`
`TextBox`, `SvgIcon`, and `Button1`/`Button2`).

Additional construction/mutation on the assertion path:

- `MyBox.ReplaceButtons` (`MyBox.cs:165-181`) reads `viewer.L2Bottom.ColumnStyles[1].Width`, calls
  `viewer.RemoveStandardButtons()`, reads and writes `viewer.MinimumSize`, and adds a new
  `ActionButton.Button` per entry. The in-source comment (`:166-169`) explicitly notes the column width is
  read from the *style* rather than a measured layout so the form does **not** need to be shown.
- `MyBoxViewer.RemoveStandardButtons` (`MyBoxViewer.cs:50-60`) disposes `Button1`/`Button2` and is guarded
  by an `ableToRemoveStandard` flag, so it is idempotent per viewer instance.
- Assigning `TextMessage.Text` fires `TextMessage_TextChanged` (`MyBoxViewer.Designer.cs:137`) →
  `GrowTextbox()` (`MyBoxViewer.cs:97-127`), which calls
  `TextRenderer.MeasureText(TextMessage.Text, TextMessage.Font, TextMessage.Size, TextFormatFlags.WordBreak)`
  and, when the measured height exceeds the control height, mutates `this.Size`, `this.MinimumSize`, and
  `TextMessage.Size`. **This is a real side effect of a longer message** and is worth knowing for any test
  that asserts a substantially longer copy string.
- `SetDialogIcon(MessageBoxIcon.Warning)` (`MyBox.cs:326-329`) calls `SystemIcons.Warning.ToBitmap()`.

INFERENCE on window handles and apartment state: no `CreateHandle`, `Show`, or `ShowDialog` call occurs on
this path once `DialogInvoker` is replaced, and `MyBox.ShowDialog` disposes the viewer via `using`. The
load-bearing empirical fact is stronger than reasoning about WinForms internals:
`UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs:13-15` declares the class as
`[TestClass]` + `[DoNotParallelize]` — **not** `[STATestClass]` — and
`StoreWrapperController_Tests.Launch.cs:46` and `:87` already invoke `controller.Launch()` through this
exact code path. So whatever construction occurs is already tolerated in this suite's default apartment.
By contrast, `UtilitiesCS.Test/Dialogs/MyBox_ShowDialog_Tests.cs:30` and
`UtilitiesCS.Test/Dialogs/MyBox_Tests.cs:25` are both `[STATestClass]`, with the stated reason
"every test creates WinForms controls". A new test may follow either precedent; the existing
`StoreWrapperController_Tests` precedent is the closer match and requires no attribute change.

INFERENCE on current green status: the existing `Launch()` tests are recorded as passing in
`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/evidence/regression-testing/pass-after-240.md:16-19`
and re-confirmed in `docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/feature-audit.2026-07-06T13-00.md:35`
("`Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer` was independently re-run this
cycle and passed"). **This was not re-verified in this session** because no shell was available. Treat it
as documentary, not as a live baseline; the plan should include a Phase 0 task that runs the class and
records the result.

### 7.3 VERIFIED — parallelization and apartment configuration

- `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` —
  `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`.
- `UtilitiesCS.Test/test.runsettings` is `<RunSettings />` with a comment at `:2-5`:
  "Global STA execution is intentionally disabled. Tests that require an STA apartment must opt in with
  MSTest's STATestMethod or STATestClass attributes so the rest of the suite can run under the default
  threading model and participate in parallel execution."
- `StoreWrapperController_Tests` is `[DoNotParallelize]` (`StoreWrapperController_Tests.cs:14`), so its
  class runs alone even under class-level parallelism.

Bearing on the seam: because `MyBox.DialogInvoker` is `AsyncLocal`-backed, a swap inside a test does not
leak to a concurrently running class. The repository shows two established restore idioms — `try/finally`
(`StoreWrapperController_Tests.Launch.cs:34-56`) and a `[TestCleanup]` reset
(`MyBox_ShowDialog_Tests.cs:42-46`, `AutoFile_Tests.cs:41-44`). Either satisfies the isolation policy; the
`try/finally` idiom is already used in the very file a new test would extend.

---

## 8. Extraction precedent and project mechanics (item 8)

### 8.1 VERIFIED — Existing pure, unit-tested user-facing-copy helpers

There is no existing helper in this repository that maps an *enum* to *user-facing dialog copy*. The
nearest equivalents, in decreasing similarity, are:

| Precedent | Shape | Tested | Notes |
|---|---|---|---|
| `UtilitiesCS/Dialogs/MyBoxModeless.cs:123-127` `BuildMessage(string identity)` | `internal static` method on an `internal static class`, pure, returns `string` | Yes — the XML doc at `:120` and the sibling `BuildButtons` doc at `:87-90` state it is "Exposed for unit testing so … can be invoked without a real window" | **Closest precedent.** Same assembly, same dialog surface, same problem shape (compose user copy outside the coverage-exempt shell). Consumed at `:80` `viewer.TextMessage.Text = BuildMessage(identity);` |
| `TaskMaster/Ribbon/EngineGatedCommandRunner.cs:117-137` `BuildNotReadyMessage(string controlId)` | `private static`, pure, `string.Format` with `CultureInfo.CurrentCulture` | Yes, indirectly, via `EngineGatedCommandRunnerTests` | Establishes the `Build<X>Message` naming and the `CultureInfo.CurrentCulture` formatting convention |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs:342-370` `BuildUnavailableMessage` / `BuildToggleFailedMessage` / `BuildPrimeFailedMessage` | three `private static` pure builders plus a shared `RenderEngineName` null-render helper (`:334-337`) | Yes | Establishes multiple sibling builders in one type, one per condition |
| `TaskMaster/AppGlobals/StoreRehookCoordinator.cs:208-233` `LogOutcome(StoreRehookResult)` | `private void`, `switch` over `StoreRehookOutcome` producing per-outcome text | Yes, via `StoreRehookCoordinatorTests` | **The only enum→text switch in the store area.** It is log copy, not dialog copy, but it is the structural template for a state-driven message selector |

Naming convention across all four: `Build<Something>Message`, `static`, pure, no I/O.

### 8.2 VERIFIED — User-facing strings are source literals, not resources

SearchScope: `UtilitiesCS/**/*.resx`.
SearchPatterns: `<data name=` over `UtilitiesCS/Properties/Resources.resx` and `UtilitiesCS/ManagerResources.resx`;
plus a full `*.resx` inventory of `UtilitiesCS/`.
SearchResult: 24 `.resx` files under `UtilitiesCS/`. Twenty-one are per-Form Designer resources
(`MyBoxViewer.resx`, `StoreWrapperViewer.resx`, `DisabledStoresViewer.resx`, …). The three non-Designer
resource files contain no UI message copy:
- `UtilitiesCS/Properties/Resources.resx` — image/icon `ResXFileRef` entries plus the Visual Studio
  boilerplate sample entries (`Name1`, `Color1`, `Bitmap1`, `Icon1`).
- `UtilitiesCS/ManagerResources.resx` — six taxonomy entries (`Project`, `Folder`, `Spam`, `Triage`,
  `Context`, `Actionable`).
- `UtilitiesCS/IntelligenceResources.resx` — JSON configuration templates
  (see `UtilitiesCS/IntelligenceResources.Designer.cs:124-142`).

Known-positive control: the `<data name=` query does return 29 real entries from `Resources.resx` and 10
from `ManagerResources.resx`, confirming the query works and the files were parsed.

**Conclusion:** every user-facing string in `UtilitiesCS` lives as a source literal. Introducing a `.resx`
message table would be new, unmatched convention; a source-literal helper matches local practice.

### 8.3 VERIFIED — `UtilitiesCS.csproj` requires an explicit `<Compile Include>` for any new `.cs`

`UtilitiesCS/UtilitiesCS.csproj:744` contains:

```
<Compile Include="OutlookObjects\Store\StoreLaunchReadinessEvaluator.cs" />
```

This is a non-SDK, `packages.config`-era project with explicit per-file `<Compile Include>` items. A new
source file **will not build** unless it is added to the `.csproj`. Note also that `.csproj` files are
excluded from CSharpier by `.csharpierignore` per CLAUDE.md, so a `.csproj` edit does not participate in
the formatter gate but does need to be hand-formatted consistently with its neighbours.

### 8.4 VERIFIED — File-size headroom (500-line cap)

| File | Current lines | Headroom |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | 479 (last line 478 + trailing newline) | ~21 lines |
| `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs` | 181 | ample |
| `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessEvaluator.cs` | 42 | ample |
| `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` | 235 | ample |

INFERENCE: `StoreWrapperController.cs` has only ~21 lines of headroom, which is not enough for a documented
message-selection helper plus its XML doc. Two placements avoid both the cap and a `.csproj` edit:

- **Preferred:** add the helper to the existing `StoreLaunchReadinessEvaluator.cs` (42 lines). It is
  already the shared, non-exempt home of readiness semantics, it is already registered in the `.csproj`,
  and both `Launch()` methods already reference that file's type. No new file, no `.csproj` change, no
  file-size risk.
- **Alternative:** a new `UtilitiesCS/OutlookObjects/Store/StoreLaunchReadinessMessages.cs` following the
  `MyBoxModeless` precedent, which requires the `<Compile Include>` entry described in 8.3.

The helper must be a **non-exempt** member because both `Launch()` methods carry
`[ExcludeFromCodeCoverage]`; logic placed inline in `Launch()` is removed from the coverage denominator,
which is the exact constraint recorded in the #240 research at `:114-120`.

---

## 9. Candidate wordings (item 9 — options only, no recommendation)

Constraints applied: `.claude/rules/tonality.md` (read at
`.claude/rules/tonality.md:1-30` in this worktree) — neutral, factual, no hyperbole, no dramatized urgency,
no metaphor. Additional constraint from Section 7.2: a longer message triggers `GrowTextbox()` and enlarges
the dialog, so brevity has a real UI effect.

Both call sites (`StoreWrapperController.cs:123-124` and `DisabledStoresController.cs:167-168`) must be
updated together, or the two settings dialogs will present different copy for the same condition.

### Case T — `StoresUnavailable` (verified transient; retry is correct advice)

| Option | Title | Message | Tradeoff |
|---|---|---|---|
| T1 | `Store Settings Unavailable` | `Store settings are still loading. Please try again in a moment.` | Shortest; keeps the existing title so no title-assertion churn. Does not say what is loading. |
| T2 | `Store Settings Loading` | `The store list has not finished loading. Please try again shortly.` | Names the specific thing that is incomplete, which matches the actual state (`Stores` is null on a loaded model). Changes the title, so any future title assertion must track both cases. |
| T3 | `Store Settings Unavailable` | `Store settings are not available yet because the store list is still being populated. Please try again shortly.` | Most precise. Longest — most likely to trigger the `GrowTextbox()` resize branch. |

### Case M — `ModelUnavailable` (may be transient or permanent; state alone cannot tell)

Because this state covers both a startup race and a completed-but-failed load, wording that asserts either
one would be unsupported. The options below either stay neutral or offer both possibilities.

| Option | Title | Message | Tradeoff |
|---|---|---|---|
| M1 | `Store Settings Unavailable` | `Store settings could not be opened. Startup may still be in progress, or the store settings failed to load. Check the application log for a store-load error before retrying.` | Accurate for both sub-causes and gives one actionable next step. Longest of the three; asks the user to read a log. |
| M2 | `Store Settings Unavailable` | `Store settings are not available. If startup has completed, the store settings failed to load and retrying will not help; see the application log for details.` | States the permanent case conditionally rather than unconditionally, so it never misleads. Requires the user to judge whether startup completed. |
| M3 | `Store Settings Unavailable` | `Store settings are not available. Retry once startup has completed; if the message persists, the store settings failed to load and the application log records the cause.` | Preserves the existing retry guidance while bounding it. Least disruptive to users familiar with the current copy. Implicitly asks for a second attempt before pointing at the log. |

### Case S — single message for all non-`Ready` states (Option B in Section 4)

| Option | Title | Message | Tradeoff |
|---|---|---|---|
| S1 | `Store Settings Unavailable` | `Store settings are not available. If startup has completed, check the application log for a store-load error.` | Smallest possible diff; one literal per call site. Loses the accurate "try again shortly" guidance that `StoresUnavailable` legitimately warrants. |
| S2 | `Store Settings Unavailable` | `Store settings are not available yet. Retry after startup completes; if the message persists, the application log records why the store settings failed to load.` | Backward-compatible with the current copy while removing the implication that retrying always works. Slightly long. |

### Open question for the maintainer

The issue's own checklist (`docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/issue.md:63`)
already asks: "Confirm with the maintainer what the genuine-failure copy should say (e.g. pointing at logs
or a support path) before implementing." Every Case M option above assumes "point at the application log"
is acceptable guidance. If a different support path is preferred, all three Case M options need
re-wording. This is not a research question.

---

## 10. Testing implications (no test code written)

Framework: MSTest + Moq + FluentAssertions, per CLAUDE.md CUT1/CUT2.

Proposed strategy, following existing precedent and the coverage constraint in Section 8.4:

1. **Pure-helper unit tests (primary coverage vehicle).** Test the extracted message selector directly for
   each of the three `StoreLaunchReadinessState` values. This is the `MyBoxModeless.BuildMessage` /
   `EngineToggleStateCoordinator.Build*Message` precedent and is where the >= 90% new-code target is met,
   because the helper is not coverage-exempt.
2. **One end-to-end dialog-copy assertion per state, through `Launch()`.** Extend
   `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.Launch.cs` using the existing
   `try/finally` seam swap, capturing `viewer.Text` and `viewer.TextMessage.Text` in the invoker. This
   proves the wiring, not the logic. The existing arrange blocks at `:30` and `:71` already produce the two
   non-`Ready` states.
3. **A differing-copy assertion.** Assert that the `ModelUnavailable` message and the `StoresUnavailable`
   message are not equal, which is the issue's own stated test in
   `issue.md:62`. Note this test is only meaningful under Option A/C, not Option B.
4. **Parity assertion for the second call site.** Assert `DisabledStoresController.Launch()` produces the
   same copy for the same state, so the two dialogs cannot silently diverge. `DisabledStoresController` has
   no existing `Launch()` test file; a new one would be needed, and it must not construct
   `DisabledStoresViewer` (the `Ready` path is out of scope for a copy fix).
5. **No new determinism risk.** No timers, no `Thread.Sleep`/`Task.Delay`, no temp files, no live Outlook.
   All state is produced by `Mock<IApplicationGlobals>` / `Mock<IOlObjects>` `SetupGet`. Per the Moq caveat
   recorded in the #240 research (`:131`), set only `Ol` and `StoresWrapper` on the `IOlObjects` mock and
   never force a setup on the Task-returning `LoadAsync`.
6. **Phase 0 obligation.** Because no test run was possible in this session, the plan must record a
   baseline run of `StoreWrapperController_Tests` before any edit, so "fail-before" is measured against an
   observed baseline rather than a documentary one.

---

## 11. Contradictions with the issue text

Five items in the issue and its promoted spec are contradicted by the code and by the prior feature
documents.

1. **The genuine-failure state is misidentified.**
   `issue.md:29` (mirrored at `spec.md:32`) says the non-`Ready` set "includes both a transient 'not yet
   loaded' state and a genuine-failure state (`Globals.Ol.StoresWrapper` populated but permanently unable
   to resolve, vs. still loading)". "Populated but unable to resolve" is `StoresUnavailable`. That state is
   the one this research found to be unambiguously **transient** (Section 3.2): `Stores ??= []` is the
   first statement of `RewireOlObjectsAsync` (`StoresWrapper.cs:87`), reached on both the fire-and-forget
   `[OnDeserialized]` path and the awaited `AwaitStoreRewireAsync` path. The permanent case is
   `ModelUnavailable`, produced by the catch block at
   `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72`. The issue has the two states inverted.

2. **The cited doc comment does not say what the issue says it says.**
   `issue.md:29` attributes the transient/genuine split to "the addressed-issue comment at lines 96-100".
   The comment at `StoreWrapperController.cs:97-103` describes **both** non-`Ready` conditions as transient:
   "populated asynchronously during startup and **can be null (load not yet complete)**, or non-null with a
   **transiently null `Stores` list** (post-deserialize, before the async rewire populates it)".
   `StoreLaunchReadinessEvaluator.cs:15-21` repeats "list is transiently null". Neither comment describes a
   permanent state. The doc comments are themselves incomplete — they omit the permanent
   `ModelUnavailable` cause that #262 later introduced at `AppOlObjects.StoreLoading.cs:66-72` — but they do
   not support the issue's reading.

3. **The stated goal is not fully reachable by branching on state alone.**
   `issue.md:34` expects the copy to "distinguish a transient 'still starting up, try again shortly' case
   from a genuine/permanent failure case", and `issue.md:56` asserts the states "already model distinct
   readiness states; the fix is likely to branch the dialog message on the specific non-`Ready` state
   rather than introducing new state-detection logic". `ModelUnavailable` conflates a transient startup
   race with a permanent load failure (Section 3.1), so a three-way branch on the existing enum cannot tell
   a user "retrying will not help" without asserting something the state does not know. Achieving the
   stated goal exactly requires new state detection (Section 4, Option C), which is larger than the issue
   scopes.

4. **The issue names only one call site; there are two.**
   `issue.md:16` and every repro step reference `StoreWrapperController.Launch` only.
   `UtilitiesCS/OutlookObjects/Store/DisabledStoresController.cs:161-178` carries a byte-identical gate and
   an identical message and title, introduced deliberately by issue #265 ("shows the same warning as the
   single-store editor", `DisabledStoresController.cs:154-158`). A fix applied to one file only would make
   the two settings dialogs disagree.

5. **Line references in the issue are off by one to three lines.**
   `issue.md:16` cites `StoreWrapperController.cs:119-127`; `issue.md:27` cites `:119` for the `if`;
   `issue.md:28` cites "lines 121-126" for the `MyBox.ShowDialog` call; `issue.md:29` cites "lines 96-100"
   for the doc comment. On the current tree the `if` is at `:120`, `var readiness = …` is at `:119`, the
   `MyBox.ShowDialog` call spans `:122-127`, and the doc comment spans `:97-103`. The drift is minor and
   the references are still resolvable, but any plan task that quotes exact line numbers should re-derive
   them from the branch rather than copying them from the issue.

### Points where the issue is confirmed correct

- One dialog fires for every non-`Ready` state (`StoreWrapperController.cs:120-129`). Confirmed.
- The copy is imprecise for at least one reachable permanent case. Confirmed — though the case is
  `ModelUnavailable`, not the state the issue names.
- The change was a deliberately deferred follow-up from #262, not an oversight
  (`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md:60-62` and `:360-361`).
  Confirmed verbatim.
- Severity Low, no functional or data-integrity impact (`issue.md:50-52`). Confirmed: the gate still
  prevents entry into a non-ready dialog in every non-`Ready` state.
