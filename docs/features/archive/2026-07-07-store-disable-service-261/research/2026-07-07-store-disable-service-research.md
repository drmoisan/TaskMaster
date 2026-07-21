# Research: store-disable-service (Issue #261, Epic #260 Wave 0)

- Date: 2026-07-07
- Scope: disabled-store model (session-only + persisted future-sessions), `IStoreDisableService`,
  store-identity convention, filter integration, persistence. Detection (F4), runtime rehook (F3),
  notification (F4), and settings UI (F5) are explicitly out of scope; this research defines only
  the seams those features will call.
- Sources read: `docs/features/active/2026-07-07-store-disable-service-261/{issue.md,spec.md,user-story.md,plan.2026-07-07T17-41.md}`,
  `docs/features/epics/store-lockup-resilience/epic-plan.md`,
  `UtilitiesCS/OutlookObjects/Store/{StoresWrapper.cs,StoreFilterAttribution.cs,StoreWrapper.cs,StoreWrapperController.cs}`,
  `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs`,
  `UtilitiesCS/Interfaces/IGlobals/{IApplicationGlobals.cs,IOlObjects.cs,IAppEvents.cs}`,
  `TaskMaster/AppGlobals/{AppOlObjects.cs,ApplicationGlobals.cs}`,
  `UtilitiesCS.Test/OutlookObjects/Store/{StoresWrapperTests.cs,StoreFilterAttributionTests.cs}`.

## 1. Current State Analysis

### 1.1 Store model and persistence

- `StoresWrapper` (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`) extends
  `SmartSerializable<StoresWrapper>` and is the single persisted model for store configuration. It
  is deserialized in `AppOlObjects.LoadStoresAsync()` (`TaskMaster/AppGlobals/AppOlObjects.cs:251`)
  from the `"StoresWrapper"` key already registered in `IntelligenceConfig`; no new config key is
  required to add fields to this class — any new `[JsonProperty]` on `StoresWrapper` round-trips
  automatically through the existing key.
- Existing persisted exclusion state is five sibling `[JsonProperty]` members (:319-333):
  `ExcludePublicFolderStores` (bool), `ExcludeGwsoStores` (bool), `GwsoFilePathContains`,
  `ExcludedStoreNameContains`, `ExcludedStoreFilePathContains` (all `List<string>`). A new
  persisted disabled-store list belongs beside these, in the same style.
- `StoreWrapper` (`StoreWrapper.cs:129`) persists only `DisplayName` (`public string DisplayName`,
  no `[JsonIgnore]`); every other live-Outlook-derived field (`InnerStore`, `Inbox`, `RootFolder`,
  `UserEmailAddress`, `GlobalAddressBook`) is `[JsonIgnore]`. `DisplayName` is therefore already the
  established persisted identity for a store in this codebase — this is the anchor for the identity
  convention below, not a new design choice.
- `SmartSerializable<T>.Serialize()` (`SmartSerializable.cs:426`) is a debounced (3-second timer),
  thread-safe write triggered by `RequestSerialization`; it is a no-op if `Config.Disk.FilePath ==
  ""`. `StoreWrapperController.SaveChanges()` (`StoreWrapperController.cs:306`) is the existing
  precedent for "mutate the model, then call `Model.Serialize()`" — the disable service should
  follow the same shape. `SmartSerializable` also exposes `SerializeToString()` /
  `DeserializeObject(json, settings)`, which is the mechanism the existing test suite uses for
  temp-file-free round-trip tests (`SmartSerializable_Tests.cs`).
- `[OnDeserialized] RewireOlObjects` (`StoresWrapper.cs:60`) fires after every deserialize and
  fires-and-forgets `RewireAfterDeserializeAsync()`, which repopulates `Stores` from live COM via
  `GetFilteredStores()`. Newtonsoft invokes the parameterless constructor before populating
  properties (confirmed by the existing `Config` field, which has a non-serialized initializer that
  survives deserialization), so a `[JsonIgnore]` field with a C# field initializer (e.g., a
  session-only `HashSet<string>`) will be correctly re-initialized on every deserialize without any
  extra wiring.

### 1.2 Store-filter predicate: three independent implementations

The include/exclude decision is implemented **three times** with the same short-circuit order, and
all three currently omit any notion of "disabled":

1. `StoresWrapper.ShouldIncludeStore(Outlook.Store store)` — instance method (:255-309). Call sites:
   `AppOlObjects.LoadInboxes()` (`TaskMaster/AppGlobals/AppOlObjects.cs:140`) and
   `OutlookFolderHierarchyReader` (`UtilitiesCS/OutlookObjects/Folder/OutlookFolderHierarchyReader.cs:245`).
2. `StoresWrapper.StoreIsIncluded(...)` — `public static`, explicit-parameter mirror of #1
   (:192-253). Confirmed by repo-wide grep: the only call site in first-party code is the existing
   unit test (`StoresWrapperTests.AssertInclusionDecision`); no production caller depends on its
   signature today, so it is low-blast-radius to extend.
3. `StoresWrapper.ShouldIncludeStoreInstrumented(Outlook.Store store)` (:145-190), the issue #211
   Phase-3.4 diagnosis wrapper that reads `DisplayName`/`FilePath` once, times the reads, and
   delegates the actual decision to the pure `StoreFilterAttribution.Decide(...)` helper. This is
   the only call site of `GetFilteredStores()` (:129), which is what actually populates
   `StoresWrapper.Stores` during `Init()`/`RewireOlObjectsAsync()`.

**Implication:** because `Stores` (populated via path #3) and `Inboxes`/folder-tree store gating
(populated via path #1) are two independently-implemented predicates, disabled-store filtering must
be added to **all three** surfaces identically, or a disabled store could still contribute an inbox
subscription (`LoadInboxes`) or folder-tree entry (`OutlookFolderHierarchyReader`) even though it is
absent from `Stores`. This is pre-existing duplication, not something this feature should refactor
away (that would widen scope beyond F1), but it is the single largest correctness risk in this
change and must be called out to whoever implements it.

### 1.3 `StoreFilterAttribution` (pure decision helper)

`Decide(...)` (`StoreFilterAttribution.cs:55`) takes only pre-read primitives (`isPublicFolder`,
`displayName`, `filePath`, and the four exclusion-list/flag parameters) and returns
`(bool Included, StoreFilterRule Rule)`. It performs no COM access and is explicitly documented as
intentionally **not** `[ExcludeFromCodeCoverage]`. `StoreFilterRule` (:14-30) is ordered to mirror
the short-circuit evaluation order, with `Included` last. This is the natural home for a new
`Disabled` reason and the natural pattern to mirror for the new parameter: pass in an
already-resolved `bool isDisabled`, do not resolve identity or touch COM inside `Decide`.

### 1.4 DI aggregate and sub-service wiring convention

- `IApplicationGlobals` (`UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`) exposes `Ol`,
  `TD`, `AF`, `Events`, `QfSettings`, `Engines`, `IntelRes` as read-only properties; `StoresWrapper`
  itself lives one level down, on `IOlObjects.StoresWrapper` (`IOlObjects.cs:24`, `{ get; set; }`).
- `ApplicationGlobals.LoadBasicMethod()` (`TaskMaster/AppGlobals/ApplicationGlobals.cs:99-117`)
  synchronously constructs every sub-service (`_olObjects = new AppOlObjects(_outlookApp, this)`,
  `_events = new AppEvents(this)`, etc.) by passing `this` (the globals aggregate) into each
  sub-service's constructor. None of these sub-services require their dependencies to already be
  loaded at construction time — they resolve state lazily through the `IApplicationGlobals`
  reference at call time. This is directly reusable for `IStoreDisableService`: it can be
  constructed in `LoadBasicMethod()` even though `Ol.StoresWrapper` is not populated until the later
  async `LoadStoresAsync()` phase, because the service reads `Globals.Ol.StoresWrapper` per call,
  never caching it.
- `StoreWrapperController(IApplicationGlobals globals)` (`StoreWrapperController.cs:77-82`) is the
  existing precedent for a plain (non-`IApplicationGlobals`-registered) class that takes the
  aggregate in its constructor and reads `Globals.Ol.StoresWrapper` defensively
  (`EvaluateLaunchReadiness()`, :108-125, treats a null model and a null `Stores` list as distinct
  non-ready states rather than throwing). The epic's F2 root-cause note confirms `Ol.StoresWrapper`
  can legitimately be null for an entire session, so `IStoreDisableService` must not assume it is
  always populated.

## 2. Candidate Approaches

### 2.1 Where the two disabled-scope collections live

**Approach A (recommended): both collections live on `StoresWrapper`.** Add
`[JsonProperty] List<string> DisabledStoreIdentities` (persisted, future-sessions) and a
`[JsonIgnore] HashSet<string> SessionDisabledStoreIdentities` (session-only, `OrdinalIgnoreCase`
comparer, defaulted via field initializer) directly on `StoresWrapper`, beside the existing
exclusion lists. `ShouldIncludeStore`, `StoreIsIncluded`, and `ShouldIncludeStoreInstrumented` all
read from this single source of truth. `IStoreDisableService` becomes a thin orchestration layer
that mutates these two `StoresWrapper` collections and calls `Serialize()`.
- Advantages: matches the existing pattern exactly (all filter state already lives on
  `StoresWrapper`); `ShouldIncludeStore(store)` keeps its current zero-extra-parameter signature, so
  its two existing call sites (`LoadInboxes`, `OutlookFolderHierarchyReader`) need no signature
  change; serialization is automatic because both collections travel with the object that already
  round-trips through `SmartSerializable`.
- Limitations: `StoresWrapper` gains two more fields, growing an already-large class (present:
  ~335 lines) — still comfortably under the 500-line file limit.

**Approach B (rejected): both collections live inside `IStoreDisableService`, decoupled from
`StoresWrapper`.** The service holds its own in-memory session set and reads/writes an opaque
persisted blob it manages independently.
- Rejected because it violates the explicit constraint "reuse `StoresWrapper`/`SmartSerializable`;
  do not add a new settings file or config key" — an independently-owned persisted collection would
  either need its own `SmartSerializable` instance (a new file/config key) or would have to be
  smuggled onto `StoresWrapper` anyway, at which point it is Approach A with extra indirection. It
  would also force `ShouldIncludeStore`/`StoreIsIncluded`/`Decide` to take a new "effective disabled
  set" parameter supplied by the service, changing the public signature of a method with two
  existing production call sites for no behavioral benefit.

**Recommendation: Approach A.** `IStoreDisableService` is the *behavior* (identity resolution
delegated to callers, idempotency, persistence triggering); `StoresWrapper` remains the *state*
(the two disabled-identity collections), exactly mirroring how it already owns
`ExcludedStoreNameContains` etc.

### 2.2 Store identity resolution

**Approach A (rejected): resolve identity by accepting a live `Outlook.Store` inside the shared
identity helper.** Convenient for filter-time call sites but unusable by F4 (lockup detection),
whose cross-cutting constraint is "no new blocking COM reads" and which the epic plan says must
attribute a lockup "using only cheap, already-cached identity." A cached `StoreWrapper` has no live
`Outlook.Store` reference available to a background/monitor thread, and touching the *offending*
store's own COM members is precisely what a lockup-detection path must avoid.

**Approach B (recommended): a pure, string-in/string-out resolver, with a COM-touching convenience
overload for filter call sites only.**
- Core: `StoreIdentity.Resolve(string displayName, string filePathFallback = null)` — returns
  `displayName` when non-null/non-whitespace; otherwise `filePathFallback` when non-null/non-
  whitespace; otherwise a documented sentinel identity that can never match a real disable/reenable
  request (fail-safe: an unresolvable store is never accidentally disabled or accidentally
  reenabled by a stray match). No COM parameter, no I/O, unit-testable without Outlook.
- Convenience overload for filter call sites (which already read both values from a live
  `Outlook.Store` in the same pass, at no extra COM cost): `StoreIdentity.Resolve(Outlook.Store
  store)`, internally reading `store.DisplayName` and a guarded `store.FilePath` (mirroring the
  existing try/catch pattern already present in `ShouldIncludeStore`/`ShouldIncludeStoreInstrumented`)
  and forwarding to the pure overload.
- F3/F4/F5 call the pure string overload with `storeWrapper.DisplayName` and no fallback (since
  `StoreWrapper` does not cache `FilePath`), which is consistent with the epic's "single stable
  identity convention... defined by F1 and reused by F3/F4/F5."

**Recommendation: Approach B**, placed in a new small pure class,
`UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`. `FilePath` is a legitimate fallback only where
it is *already being read* for another purpose in the same call (the filter path); it must not be
promoted to a value every caller reads on demand, since a locked-up store's `FilePath` read is
exactly the kind of blocking COM call the epic wants to avoid during detection/attribution.

### 2.3 `IStoreDisableService.GetDisabledStores()` return shape

**Approach A (rejected): `IReadOnlyCollection<string>`** (a flat union of both scopes). Simplest,
matches the issue text literally, but forces F5 (settings UI, which needs to show scope so a user
can tell "this session only" from "until re-enabled") to re-derive scope by cross-checking the
returned identity against a second call or an internal implementation detail.

**Approach B (recommended): `IReadOnlyCollection<DisabledStoreEntry>`**, where
`DisabledStoreEntry` is a small `readonly record struct(string StoreIdentity, DisableScope Scope)`
and `DisableScope` is `{ SessionOnly, FutureSessions }`. Still satisfies the issue's literal method
name (`GetDisabledStores()`); gives F5 the scope it needs without a second method or reflection into
`StoresWrapper` internals. If a store identity is present in both collections (disabled for future
sessions, which — per the persistence rule below — also disables it for the remainder of the
current session), it should be reported once, with `Scope = FutureSessions` (the stronger/persisted
scope), not twice.

**Recommendation: Approach B.** This is a small, backward-compatible enrichment of the literal
issue text that avoids a signature change later when F5 is implemented.

## 3. Behavior Semantics

### 3.1 Two scopes, one persisted representation

- **Session-only** (`DisableSessionOnly`): adds the identity to `StoresWrapper.
  SessionDisabledStoreIdentities` (in-memory `HashSet<string>`, `OrdinalIgnoreCase`). Never
  persisted. Cleared naturally on process restart (field re-initializes on deserialize, or simply
  never existed if the object was newly constructed).
- **Future-sessions** (`DisableForFutureSessions`): adds the identity to `StoresWrapper.
  DisabledStoreIdentities` (persisted `List<string>`), and — because the filter check below is a
  union of both collections — this also disables the store for the remainder of the *current*
  session without needing a duplicate write to the session set.
- **Effective disabled set** for filtering purposes = `SessionDisabledStoreIdentities ∪
  DisabledStoreIdentities`, compared case-insensitively against the identity resolved for the store
  under test.

### 3.2 Idempotency

- `DisableSessionOnly(identity)` on an already-session-disabled identity: no-op (HashSet `Add`
  naturally returns `false`; no exception, no duplicate).
- `DisableForFutureSessions(identity)` on an already-future-disabled identity: no-op with respect to
  the persisted list (must not append a duplicate string) and must **not** call `Serialize()` again
  (avoids resetting/spamming the debounce timer for no state change). If the identity was previously
  only session-disabled and is now also future-disabled, the persisted list changes (grows) and
  `Serialize()` **is** called.
- `Reenable(identity)` on a non-disabled identity: no-op in both collections, no exception, and — per
  the acceptance criterion "persists when it affects the future-sessions list" — no `Serialize()`
  call, since nothing in the persisted list changed.
- `Reenable(identity)` on an identity present in both scopes: removes from both; persists once
  (because the persisted list changed), not twice.

### 3.3 Identity fallback and unresolvable stores

- When `DisplayName` is null/empty and no fallback is available (or the fallback is also
  unavailable), `StoreIdentity.Resolve` returns a sentinel value that cannot equal any real,
  well-formed identity ever passed to `DisableSessionOnly`/`DisableForFutureSessions`. This must be
  a deliberate, documented constant (not `string.Empty`, which existing exclusion-list code treats
  as a benign no-op token via `IsNullOrWhiteSpace` guards elsewhere in the same file) so a store with
  no resolvable identity can never be silently matched by an unrelated disable request and can never
  itself be disabled (its `DisableXxx` call would target the sentinel and only ever match other
  unresolvable stores).
- `IStoreDisableService`'s public methods should validate their `storeIdentity` parameter is
  non-null/non-whitespace and throw `ArgumentException` otherwise (fail fast, per repo policy),
  rather than silently accepting the sentinel as a legitimate disable target.

### 3.4 Filter-decision ordering (must not change existing attribution)

The disabled-store check must be evaluated **after** the four existing exclusion checks in
`Decide`/`ShouldIncludeStore`/`StoreIsIncluded`, immediately before the final `Included` return.
This guarantees:
- A store that is already excluded by an existing rule (public folder, name, GWSO path, file path)
  keeps exactly the same attributed reason it has today, even if it also happens to be in the
  disabled set — existing exclusion behavior and existing `[store-filter]` log lines for those
  stores are byte-for-byte unchanged.
- Only a store that would otherwise be `Included` can newly become excluded, with the new
  `StoreFilterRule.Disabled` reason, inserted in the enum immediately before `Included`
  (`PublicFolder, NameContains, GwsoFilePath, FilePathContains, Disabled, Included`).

### 3.5 Persistence trigger

`DisableForFutureSessions` and `Reenable` (when the persisted list changes) call
`Model.Serialize()` — i.e., `StoresWrapper.Serialize()`, the parameterless overload that reads
`Config.Disk.FilePath` and defers to the existing debounced write path. `DisableSessionOnly` never
calls `Serialize()`.

## 4. Requirements Mapping

| Acceptance criterion (issue #261) | Design element |
|---|---|
| Persisted future-sessions list, new `[JsonProperty]`, round-trips via `SmartSerializable` | `StoresWrapper.DisabledStoreIdentities : List<string>`, `[JsonProperty]`, default `[]`, placed beside `ExcludedStoreFilePathContains` |
| Session-only disabled set, in-memory, not persisted | `StoresWrapper.SessionDisabledStoreIdentities : HashSet<string>(OrdinalIgnoreCase)`, `[JsonIgnore]` |
| `IStoreDisableService` with the five named methods, reachable via `IApplicationGlobals` | New interface `UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs`; new property `IApplicationGlobals.StoreDisable { get; }`; concrete `StoreDisableService` constructed in `ApplicationGlobals.LoadBasicMethod()` |
| Filter excludes both scopes, distinct `StoreFilterAttribution` reason, existing behavior unchanged | New `StoreFilterRule.Disabled` inserted before `Included`; `Decide` gains a trailing `bool isDisabled` parameter, checked last; all three predicate surfaces (`ShouldIncludeStore`, `StoreIsIncluded`, `ShouldIncludeStoreInstrumented`) updated identically |
| `DisableForFutureSessions` triggers `Model.Serialize()`; `Reenable` removes from both scopes and persists when it affects the future-sessions list | Service-level idempotency + conditional `Serialize()` logic described in §3.2/§3.5 |
| Deterministic MSTest/Moq/FluentAssertions coverage, no live Outlook, no temp files | See §5 |
| Identity: `DisplayName` primary, documented fallback, cheap | `StoreIdentity.Resolve` (pure overload for F3/F4/F5, COM-touching convenience overload for filter call sites only) — see §2.2 |

## 5. Testing Implications

- **Framework/tools**: MSTest (`[TestClass]`/`[TestMethod]`), Moq for `IApplicationGlobals`/
  `IOlObjects` mocks (mirrors `StoresWrapperTests.CreateGlobalsWithStores`), FluentAssertions for
  assertions. No live Outlook COM instantiation (`Outlook.Store`/`NameSpace`/`Stores` are always
  `Mock<T>` objects, per the existing `StoresWrapperTests` pattern using `Mock<Stores>().As<IEnumerable>()`).
- **New test files** (mirroring production 1:1, per repo test-location convention):
  - `UtilitiesCS.Test/OutlookObjects/Store/StoreIdentityTests.cs` — pure function tests: DisplayName
    present; DisplayName null/whitespace with FilePath fallback present; both absent falls back to
    the documented sentinel; case is preserved (comparison casing is the caller's/service's
    responsibility, not the resolver's).
  - `UtilitiesCS.Test/OutlookObjects/Store/StoreDisableServiceTests.cs` — one test per method
    covering: positive disable/reenable flows for each scope; idempotent double-disable (session and
    future); idempotent reenable of a non-disabled identity (no `Serialize()` call — verify via a
    `Mock<StoresWrapper>`-observable `Config.Disk.FilePath`/timer seam, or a lightweight injectable
    `Action` hook if a full mock of `Serialize()` proves impractical because `StoresWrapper` is a
    concrete class, not an interface, so Moq would need `virtual` on `Serialize()`/`Config` to
    intercept it — confirm this seam exists before finalizing the test approach, since
    `SmartSerializable<T>.Serialize()` is not currently `virtual`); `IsDisabled`/`GetDisabledStores`
    reflect both scopes and de-duplicate an identity present in both; `ArgumentException` for
    null/whitespace identity inputs; behavior when `Globals.Ol.StoresWrapper` is null (fail-fast on
    writes per §3.3, safe-empty on reads).
- **Extended existing test files**:
  - `StoreFilterAttributionTests.cs`: add `Decide(..., isDisabled: true)` cases proving `Disabled`
    wins only when no earlier rule matches, and that an already-excluded store's rule is unchanged
    when `isDisabled` is also `true` (proves the ordering in §3.4).
  - `StoresWrapperTests.cs`: add cases for `ShouldIncludeStore`/`StoreIsIncluded` covering a
    session-disabled and a future-disabled store each being excluded, and — importantly — a
    round-trip serialization test using the existing temp-file-free pattern
    (`SerializeToString()` / `StoresWrapper.Static.Deserialize`-equivalent `DeserializeObject`) that
    proves `DisabledStoreIdentities` survives serialize/deserialize while
    `SessionDisabledStoreIdentities` does not appear in the emitted JSON and is empty (not null)
    immediately after deserialization.
- **Coverage target**: new classes/methods (`StoreIdentity`, `StoreDisableService`, the `Decide`/
  `ShouldIncludeStore`/`StoreIsIncluded` deltas) must reach the repo's >= 90% new-code target;
  `StoreFilterAttribution` remains intentionally coverage-tracked (not `[ExcludeFromCodeCoverage]`).
- **Determinism**: no `Thread.Sleep`/`Task.Delay`/real timers. If a test needs to observe that
  `Serialize()` was invoked without waiting for the 3-second debounce, follow the existing
  `TimerFactory`/`ITimerWrapper` injectable-timer seam already present in `SmartSerializable<T>`
  (`RequestSerialization`, :533) rather than adding a new one — this seam is designed exactly for
  deterministic, manually-fired serialization tests.

## 6. File-by-File Change List

**Production files: 7 total (3 new, 4 modified).**

New:
1. `UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs` — pure identity resolver + COM convenience
   overload (§2.2).
2. `UtilitiesCS/Interfaces/IGlobals/IStoreDisableService.cs` — interface, `DisableScope` enum,
   `DisabledStoreEntry` record (§2.3).
3. `UtilitiesCS/OutlookObjects/Store/StoreDisableService.cs` — concrete implementation, takes
   `IApplicationGlobals` in its constructor (mirrors `StoreWrapperController`).

Modified:
4. `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` — add `DisabledStoreIdentities`
   (`[JsonProperty]`) and `SessionDisabledStoreIdentities` (`[JsonIgnore]`); update
   `ShouldIncludeStore`, static `StoreIsIncluded` (new trailing parameter — its only current caller
   is a unit test, so this is a controlled, low-blast-radius signature change), and
   `ShouldIncludeStoreInstrumented`'s call into `Decide`.
5. `UtilitiesCS/OutlookObjects/Store/StoreFilterAttribution.cs` — add `StoreFilterRule.Disabled`
   (inserted before `Included`) and the new `isDisabled` parameter/branch on `Decide`.
6. `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs` — add `IStoreDisableService StoreDisable
   { get; }`.
7. `TaskMaster/AppGlobals/ApplicationGlobals.cs` — construct `_storeDisableService` in
   `LoadBasicMethod()` alongside the other sub-services and expose it through the new interface
   property.

**Test files** (new unless noted): `StoreIdentityTests.cs` (new),
`StoreDisableServiceTests.cs` (new), `StoreFilterAttributionTests.cs` (extend),
`StoresWrapperTests.cs` (extend). No changes anticipated to `TaskMaster.Test/AppGlobals/
ApplicationGlobalsTests.cs` unless that suite asserts an exhaustive list of `IApplicationGlobals`
members; verify at implementation time.

## 7. Cross-Feature Impacts to Flag

- **Identity convention is a hard dependency for F3/F4/F5.** `StoreIdentity.Resolve(string
  displayName, string filePathFallback = null)` (the pure, string-only overload) is the contract
  those features must call — not the COM-touching convenience overload, which only filter-time call
  sites should use. If F4's detection path is later found to need a different cached field than
  `StoreWrapper.DisplayName`, that is a signal the identity convention itself needs revisiting before
  F4 starts, not a local workaround inside F4.
- **`IStoreDisableService` contract shape is a hard dependency for F4 and F5.** F4 will call
  `DisableSessionOnly`/`Reenable`; F5 will call `GetDisabledStores()`/`Reenable`. The
  `DisabledStoreEntry`/`DisableScope` return-shape recommendation in §2.3 should be settled during
  F1's implementation, since changing `GetDisabledStores()`'s return type after F5 is written would
  be a breaking change to a dependent feature.
- **Reenable's COM rehook is explicitly F3's responsibility, not F1's.** F1's `Reenable` only
  updates the disabled-identity collections and persists; it must not attempt to re-add a `Store` to
  `Stores` or re-register AppEvents/folder-notification handlers (the epic plan states the original
  hookup method has already terminated and a shared rehook helper is F3's extraction). F1's
  `IStoreDisableService.Reenable` is therefore a pure state-removal operation; F3/F4 compose it with
  the rehook helper rather than F1 doing both.
- **Triple-implementation filter duplication (§1.2) is pre-existing, not introduced here, but this
  feature is the first to require all three surfaces to stay behaviorally identical.** A future
  cleanup (out of scope for F1) could collapse `ShouldIncludeStore`/`StoreIsIncluded` to call
  `StoreFilterAttribution.Decide` directly, the way `ShouldIncludeStoreInstrumented` already does;
  flagging this as a candidate follow-up issue rather than doing it inside F1, since it would widen
  the change surface for a wave-0 foundation feature.
- **`SmartSerializable<T>.Serialize()`/`SerializeThreadSafe` are not `virtual`.** If
  `StoreDisableServiceTests` needs to assert "`Serialize()` was/was not called" without waiting on
  the real debounce timer, confirm during implementation whether the existing `TimerFactory`
  injectable-timer seam is sufficient (it should be, per §5) before considering any change to
  `SmartSerializable` itself; widening that class's testability surface is out of scope for F1.
