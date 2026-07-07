# Design Spec — Store Disable Service (F1, Issue #261, Epic #260 Wave 0)

- Date: 2026-07-07
- Author: Dan Moisan (directive), prd-feature (authoring)
- Scope class: full-feature; wave-0 foundation for the store-lockup-resilience epic
- Work Mode: full-feature (AC sources: `spec.md` and `user-story.md`)
- Inputs: `issue.md`; `research/2026-07-07-store-disable-service-research.md`;
  `docs/features/epics/store-lockup-resilience/epic-plan.md`

## 1. Overview

F1 is the wave-0 foundation of the store-lockup-resilience epic. It introduces a single,
testable model of which Outlook stores are disabled and enforces that decision at store-filter
time. Today `StoresWrapper` carries only substring-based exclusion lists
(`ExcludedStoreNameContains`, `ExcludedStoreFilePathContains`, and siblings) with no notion of a
user- or system-disabled store keyed by a stable identity, no runtime service to toggle
disablement, and no distinction between session-only and persisted disablement.

Every other feature in the epic depends on the contracts defined here: F3 (runtime reenable), F4
(lockup detection, auto-disable, notification), and F5 (settings UI) all call the identity
convention and the `IStoreDisableService` contract this feature establishes. Because those
contracts are consumed by dependent features, they are fixed at the epic level and defined here
exactly; F1 must not deviate from them.

F1 delivers: the store-identity convention, the disabled-store data model plus persistence, the
`IStoreDisableService` contract and implementation, and filter integration. It provides only the
seams that later features call; it does not implement detection, rehook mechanics, notification,
or settings UI.

## 2. Scope and Non-Scope

### 2.1 In scope

- A pure store-identity convention: `StoreIdentity.Resolve(displayName, filePathFallback)`, with
  a separate COM-touching overload reserved for filter-time call sites.
- A two-scope disabled-store model on `StoresWrapper`: a persisted future-sessions list and an
  in-memory session-only set.
- `IStoreDisableService`, exposed on `IApplicationGlobals` as the member `StoreDisable`, with the
  five fixed methods in §4.2.
- A staged `ReenableAsync` seam that clears both scopes, persists, and invokes an injected rehook
  collaborator defaulting to a no-op in wave 0.
- Filter integration: a new `Disabled` attribution reason checked last in
  `StoreFilterAttribution.Decide`, applied identically across all three include/exclude surfaces.
- Persistence of future-sessions changes through the existing debounced `Model.Serialize()` path.
- Deterministic MSTest coverage.

### 2.2 Out of scope (explicitly deferred to later features)

- **Lockup detection and attribution (F4, #264).** F1 does not detect a UI-thread lockup or decide
  which store to auto-disable; it exposes `DisableSessionOnly`/`ReenableAsync` for F4 to call.
- **Runtime rehook mechanics (F3, #263).** F1's `ReenableAsync` only clears disabled state and
  persists; it does not re-add a `Store` to `Stores` or re-register AppEvents/notification handlers.
  F1 defines the rehook seam (`IStoreRehookService`) and ships a no-op default; F3 supplies the real
  implementation via a small, in-scope edit to F1's service.
- **Modeless notification (F4, #264).** No message box, dispatcher, or notification composition.
- **Settings UI (F5, #265).** No user interface. `GetDisabledStores()` returns the data F5 renders.
- **Filter-duplication cleanup.** The three-way duplication of the include/exclude predicate is
  pre-existing. F1 updates all three surfaces identically but does not collapse them; a follow-up
  refactor is flagged out of scope to keep the wave-0 change surface bounded.

## 3. Store-Identity Convention

Store identity is the stable key by which a store is disabled, tested for disablement, and
reenabled. It is defined by F1 and reused verbatim by F3/F4/F5.

### 3.1 `StoreIdentity` value type

`StoreIdentity` is a small, immutable value type — a plain `public readonly struct` with a private
constructor and a get-only `public string Value { get; }` auto-property wrapping the resolved
identity string. It is created only through the `Resolve` factory (below) so callers cannot
fabricate an identity from an unresolved input. Equality for storage and lookup is performed
case-insensitively by the collections that hold identities (§5); the resolved `Value` preserves
original casing.

> Net48 constraint: declare `StoreIdentity` as a plain `public readonly struct` with an ordinary
> constructor and get-only (`{ get; }`) properties — NOT a `record struct` and NOT any `init`
> accessor. This repository targets .NET Framework 4.8 and ships no `IsExternalInit` polyfill, so
> any `init` accessor (including record-generated ones) fails to compile with CS0518. Mirror the
> `ResourceTimingRow` pattern in `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`.

### 3.2 Pure resolver (the F3/F4/F5 contract)

```csharp
/// <summary>
/// Resolves a stable store identity from already-cached primitives. Performs no COM access and
/// no I/O; safe to call from any thread, including a background monitor.
/// </summary>
/// <param name="displayName">The store DisplayName (the persisted key on StoreWrapper). Primary.</param>
/// <param name="filePathFallback">
/// Optional fallback used only when displayName is null/whitespace. Callers that do not already
/// hold a cheap FilePath (F3/F4/F5, which have only StoreWrapper.DisplayName) pass null.
/// </param>
/// <returns>
/// A StoreIdentity whose Value is displayName when non-null/non-whitespace; otherwise
/// filePathFallback when non-null/non-whitespace; otherwise a documented sentinel value that can
/// never equal any well-formed identity (fail-safe: an unresolvable store is never accidentally
/// disabled and never accidentally reenabled by a stray match).
/// </returns>
public static StoreIdentity Resolve(string displayName, string filePathFallback = null);
```

The sentinel is a deliberate, documented constant (not `string.Empty`, which existing
exclusion-list code treats as a benign no-op token via `IsNullOrWhiteSpace` guards). An
unresolvable store therefore resolves only to the sentinel and can match neither a real disable
request nor a real reenable request.

### 3.3 COM convenience overload (filter call sites only)

```csharp
/// <summary>
/// Convenience overload for filter-time call sites that already read DisplayName and FilePath from
/// a live Outlook.Store in the same pass. Reads store.DisplayName and a guarded store.FilePath
/// (mirroring the existing try/catch in ShouldIncludeStore) and forwards to the pure overload.
/// Not for use by F3/F4/F5, whose contract is the pure string overload.
/// </summary>
public static StoreIdentity Resolve(Outlook.Store store);
```

`FilePath` is read here only because the filter path already reads it for another purpose at no
extra COM cost. It must not be promoted to a value every caller reads on demand: a locked-up
store's `FilePath` read is exactly the blocking COM call the epic prohibits during detection and
attribution.

## 4. Public API Contracts

### 4.1 `DisableScope` and `DisabledStoreEntry`

```csharp
/// <summary>The persistence scope of a disabled-store entry.</summary>
public enum DisableScope
{
    /// <summary>Disabled for the current process only; never persisted.</summary>
    SessionOnly,
    /// <summary>Disabled for the current and all future sessions; persisted.</summary>
    FutureSessions,
}

/// <summary>A disabled store's identity paired with the scope under which it is disabled.</summary>
public readonly struct DisabledStoreEntry
{
    public DisabledStoreEntry(StoreIdentity identity, DisableScope scope)
    {
        Identity = identity;
        Scope = scope;
    }

    public StoreIdentity Identity { get; }
    public DisableScope Scope { get; }
}
```

> Net48 constraint: `DisabledStoreEntry` is a plain `public readonly struct` with an ordinary
> constructor and get-only (`{ get; }`) properties — NOT a `record struct` and NOT any `init`
> accessor. The .NET Framework 4.8 target ships no `IsExternalInit` polyfill, so an `init` accessor
> fails to compile with CS0518. It is constructed via its constructor (no object initializer). Mirror
> the `ResourceTimingRow` pattern in `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs`.

### 4.2 `IStoreDisableService`

Exposed on `IApplicationGlobals` as the read-only member `StoreDisable`. The contract is fixed at
the epic level; F4 and F5 call this service only and do not call F3 directly.

```csharp
public interface IStoreDisableService
{
    /// <summary>
    /// Disables the store for the current session only. Adds the identity to the in-memory
    /// session set. Never persists. Idempotent: disabling an already-session-disabled identity
    /// is a no-op. Throws ArgumentException if identity is unresolved/empty.
    /// </summary>
    void DisableSessionOnly(StoreIdentity identity);

    /// <summary>
    /// Disables the store for the current and future sessions. Adds the identity to the persisted
    /// list and persists via Model.Serialize(). Because filtering unions both scopes, this also
    /// disables the store for the remainder of the current session with no session-set write.
    /// Idempotent: if the identity is already in the persisted list, does not append a duplicate
    /// and does not call Serialize() again. Throws ArgumentException if identity is unresolved/empty.
    /// </summary>
    void DisableForFutureSessions(StoreIdentity identity);

    /// <summary>
    /// Reenables the store by clearing it from BOTH scopes, persisting when the persisted list
    /// changed, then awaiting the injected rehook collaborator (a no-op in wave 0; F3 supplies the
    /// real IStoreRehookService). Idempotent: reenabling a non-disabled identity changes no
    /// collection, calls neither Serialize() nor a state mutation, and still awaits the collaborator.
    /// Throws ArgumentException if identity is unresolved/empty.
    /// </summary>
    Task ReenableAsync(StoreIdentity identity);

    /// <summary>
    /// Returns true when the identity is present in either scope (case-insensitive). Read-only;
    /// never mutates and never persists. Returns false when the store model is not yet populated.
    /// </summary>
    bool IsDisabled(StoreIdentity identity);

    /// <summary>
    /// Returns all currently disabled stores as identity+scope entries. An identity present in both
    /// scopes is reported once with Scope = FutureSessions (the stronger, persisted scope). Returns
    /// an empty collection (never null) when the store model is not yet populated.
    /// </summary>
    IReadOnlyCollection<DisabledStoreEntry> GetDisabledStores();
}
```

### 4.3 Staged rehook seam (`IStoreRehookService`)

F1 defines the seam so `ReenableAsync` can invoke a collaborator without a forward dependency on
F3. F1 ships a no-op default; F3 supplies the real implementation.

```csharp
/// <summary>
/// Collaborator invoked by ReenableAsync after disabled state is cleared, to re-add the Store and
/// re-register its event handlers. Wave 0 uses a no-op implementation; F3 replaces it.
/// </summary>
public interface IStoreRehookService
{
    Task RehookAsync(StoreIdentity identity);
}

/// <summary>Wave-0 default: performs no rehook. Enables F1 to ship without depending on F3.</summary>
internal sealed class NoOpStoreRehookService : IStoreRehookService
{
    public Task RehookAsync(StoreIdentity identity) => Task.CompletedTask;
}
```

`StoreDisableService` accepts an `IStoreRehookService` in its constructor and defaults it to
`NoOpStoreRehookService` when none is supplied. This is the entire F1↔F3 boundary: F3's deliverable
is a small, in-scope edit that constructs `StoreDisableService` with the real collaborator. F1 does
not reference any F3 type.

### 4.4 `IApplicationGlobals` member

```csharp
/// <summary>The store disable service. Constructed in LoadBasicMethod(); reads the store model lazily.</summary>
IStoreDisableService StoreDisable { get; }
```

`StoreDisableService(IApplicationGlobals globals, IStoreRehookService rehook = null)` mirrors
`StoreWrapperController`: it takes the aggregate and reads `Globals.Ol.StoresWrapper` per call,
never caching it, so it can be constructed in `LoadBasicMethod()` before the store model is
populated by the later async `LoadStoresAsync()` phase.

## 5. Data Model and Persistence

Both disabled-scope collections live on `StoresWrapper`, beside the existing exclusion lists. The
service is a thin orchestration layer over this single source of truth.

- **Persisted future-sessions list.** `[JsonProperty] List<string> DisabledStoreIdentities`,
  default `[]`, placed beside `ExcludedStoreFilePathContains`. It round-trips automatically through
  the existing `"StoresWrapper"` key in `IntelligenceConfig`; no new file or config key is added.
- **Session-only set.** `[JsonIgnore] HashSet<string> SessionDisabledStoreIdentities`, comparer
  `StringComparer.OrdinalIgnoreCase`, initialized by a C# field initializer. Newtonsoft invokes the
  parameterless constructor before populating properties, so the field re-initializes to an empty
  set on every deserialize with no extra wiring, and it is absent from emitted JSON.
- **Effective disabled set** (for filtering) = `SessionDisabledStoreIdentities ∪
  DisabledStoreIdentities`, compared case-insensitively against the identity resolved for the store
  under test.
- **Persistence trigger.** `DisableForFutureSessions`, and `ReenableAsync` when the persisted list
  changes, call `Model.Serialize()` — the parameterless `StoresWrapper.Serialize()` that reads
  `Config.Disk.FilePath` and defers to the existing debounced (3-second) write path.
  `DisableSessionOnly` never calls `Serialize()`. This mirrors `StoreWrapperController.SaveChanges()`
  ("mutate the model, then call `Model.Serialize()`").

## 6. Filter-Integration Design

The include/exclude decision is implemented three times with identical short-circuit order:
`StoresWrapper.ShouldIncludeStore` (instance; call sites `AppOlObjects.LoadInboxes` and
`OutlookFolderHierarchyReader`), `StoresWrapper.StoreIsIncluded` (static; only current caller is a
unit test), and `StoresWrapper.ShouldIncludeStoreInstrumented` (delegates to
`StoreFilterAttribution.Decide`, the only path that populates `Stores`). Because `Stores` and the
inbox/folder-tree gating are independently-implemented predicates, the disabled check must be added
to all three identically, or a disabled store could still contribute an inbox subscription or
folder-tree entry.

### 6.1 New attribution reason, checked last

`StoreFilterRule` gains a `Disabled` value inserted immediately before `Included`, preserving the
enum's mirror of evaluation order:

```
PublicFolder, NameContains, GwsoFilePath, FilePathContains, Disabled, Included
```

`Decide` gains a trailing `bool isDisabled` parameter, evaluated **after** the four existing
exclusion checks and immediately before the final `Included` return:

```csharp
public static (bool Included, StoreFilterRule Rule) Decide(
    bool isPublicFolder, string displayName, string filePath,
    bool excludePublicFolderStores, IReadOnlyCollection<string> excludedStoreNameContains,
    IReadOnlyCollection<string> gwsoFilePathContains, IReadOnlyCollection<string> excludedStoreFilePathContains,
    bool isDisabled);
```

`Decide` performs no COM access and does not resolve identity; callers pass an already-resolved
`bool isDisabled` (computed by testing the effective disabled set against `StoreIdentity.Resolve`).

### 6.2 Byte-for-byte preservation of existing attribution

Because `Disabled` is checked only after all existing rules:

- A store already excluded by an existing rule (public folder, name, GWSO path, file path) keeps
  exactly the same attributed reason and `[store-filter]` log line it has today, even if it is also
  in the disabled set.
- Only a store that would otherwise be `Included` can newly become excluded, with the new `Disabled`
  reason. Existing exclusion attribution is unchanged.

## 7. Error Handling

- **Fail fast on writes.** `DisableSessionOnly`, `DisableForFutureSessions`, and `ReenableAsync`
  validate that the supplied identity is resolved and non-empty and throw `ArgumentException`
  otherwise. The sentinel identity is treated as unresolved and rejected, so an unresolvable store
  can never be made a deliberate disable target.
- **Safe-empty on reads.** `IsDisabled` returns `false` and `GetDisabledStores` returns an empty
  (never null) collection when `Globals.Ol.StoresWrapper` is null. The epic confirms the store model
  can legitimately be null for an entire session, so read methods must not throw on a null model.
- **Null model on writes.** A write invoked while the store model is null fails fast (it cannot
  record state that will persist); the exception type and message are defined at implementation time
  consistent with repo fail-fast policy.
- **No broad catch.** The service does not swallow exceptions. The guarded `FilePath` read in the
  COM convenience overload catches only the narrow COM-access failure the existing filter code
  already guards, and returns the DisplayName-or-sentinel result rather than propagating.

## 8. Determinism and Testability

- **Framework/tools.** MSTest (`[TestClass]`/`[TestMethod]`), Moq for `IApplicationGlobals`/
  `IOlObjects`/`StoresWrapper` collaborators (mirroring `StoresWrapperTests.CreateGlobalsWithStores`),
  FluentAssertions for assertions.
- **No live Outlook.** `Outlook.Store`/`NameSpace`/`Stores` are always `Mock<T>`; no live COM
  instantiation. `StoreIdentity.Resolve`'s pure overload is tested with plain strings.
- **No temporary files.** Serialization round-trip tests use the existing temp-file-free
  `SerializeToString()` / `DeserializeObject(json, settings)` pattern used by `SmartSerializable`
  tests. Creation and use of temporary files is prohibited.
- **No banned timing APIs.** No `Thread.Sleep`, `Task.Delay`, or real timers in tests. To observe
  that `Serialize()` was invoked without waiting for the 3-second debounce, use the existing
  `TimerFactory`/`ITimerWrapper` injectable-timer seam in `SmartSerializable<T>.RequestSerialization`
  rather than adding a new seam.
- **Injectable rehook seam.** `ReenableAsync` is tested with a `Mock<IStoreRehookService>` to assert
  the collaborator is awaited after state is cleared; the no-op default is asserted to leave state
  cleared and to complete.
- **Coverage.** New classes/methods (`StoreIdentity`, `StoreDisableService`, and the `Decide`/
  `ShouldIncludeStore`/`StoreIsIncluded` deltas) target the repo's new-code coverage policy.
  `StoreFilterAttribution` remains intentionally coverage-tracked (not `[ExcludeFromCodeCoverage]`).
- **Evidence.** All evidence artifacts (coverage, QA gates, regression results) are written under
  `<FEATURE>/evidence/<kind>/` per the evidence-and-timestamp conventions.

## 9. Acceptance Criteria

Each item is independently testable. Unless stated otherwise, verification uses MSTest + Moq +
FluentAssertions, no live Outlook, and no temporary files.

- [ ] **AC1 — Persisted future-sessions list.** `StoresWrapper` exposes a `[JsonProperty]
      List<string> DisabledStoreIdentities` keyed by resolved identity, defaulting to an empty list.
      A serialize/deserialize round-trip via `SerializeToString()`/`DeserializeObject` preserves its
      contents.
- [ ] **AC2 — Session-only set is in-memory and not persisted.** `StoresWrapper` exposes a
      `[JsonIgnore] HashSet<string> SessionDisabledStoreIdentities` (OrdinalIgnoreCase). After a
      round-trip, the emitted JSON contains no session-set field, and the deserialized set is empty
      (not null).
- [ ] **AC3 — `StoreIdentity.Resolve` (pure).** Returns `displayName` when non-null/non-whitespace;
      returns `filePathFallback` when `displayName` is null/whitespace and the fallback is present;
      returns the documented sentinel when both are absent. Casing of a resolved value is preserved.
      No COM access; callable without Outlook.
- [ ] **AC4 — Service contract on `IApplicationGlobals`.** `IApplicationGlobals.StoreDisable` returns
      an `IStoreDisableService` exposing `DisableSessionOnly`, `DisableForFutureSessions`,
      `ReenableAsync`, `IsDisabled`, and `GetDisabledStores` with the signatures in §4.2, constructed
      in `LoadBasicMethod()` and reading the store model lazily.
- [ ] **AC5 — Disable positive flows.** `DisableSessionOnly(identity)` adds to the session set only;
      `DisableForFutureSessions(identity)` adds to the persisted list and (via the union) also
      renders the store disabled for the current session. After each, `IsDisabled(identity)` is true.
- [ ] **AC6 — Persistence trigger.** `DisableForFutureSessions` invokes `Model.Serialize()` (verified
      through the injectable-timer seam); `DisableSessionOnly` does not.
- [ ] **AC7 — Idempotency.** A second `DisableSessionOnly` for the same identity is a no-op (no
      duplicate, no throw). A second `DisableForFutureSessions` for an already-persisted identity does
      not append a duplicate and does not call `Serialize()` again.
- [ ] **AC8 — `ReenableAsync` clears both scopes and persists conditionally.** Reenabling an identity
      present in both scopes removes it from both and calls `Serialize()` exactly once. Reenabling a
      non-disabled identity changes no collection and calls neither `Serialize()` nor a mutation.
- [ ] **AC9 — Staged rehook seam.** `ReenableAsync` awaits the injected `IStoreRehookService` after
      clearing state; the wave-0 default (`NoOpStoreRehookService`) completes without rehooking and
      leaves state cleared. A `Mock<IStoreRehookService>` confirms invocation ordering (state cleared
      before `RehookAsync` is awaited).
- [ ] **AC10 — `GetDisabledStores` scope and de-duplication.** Returns identity+scope entries; an
      identity in both scopes is reported once as `FutureSessions`. Returns an empty collection when
      the store model is null.
- [ ] **AC11 — Identity validation.** `DisableSessionOnly`, `DisableForFutureSessions`, and
      `ReenableAsync` throw `ArgumentException` for an unresolved/empty identity (including the
      sentinel). Read methods do not throw.
- [ ] **AC12 — Filter attribution: `Disabled` checked last.** `StoreFilterAttribution.Decide` with
      `isDisabled: true` returns `Disabled` only when no earlier exclusion rule matched; a store that
      an existing rule already excludes keeps its original rule even when `isDisabled` is also true
      (existing attribution byte-for-byte unchanged; enum order `..., FilePathContains, Disabled,
      Included`).
- [ ] **AC13 — Filter integration across all three surfaces.** `ShouldIncludeStore`, `StoreIsIncluded`,
      and `ShouldIncludeStoreInstrumented` each exclude a session-disabled store and a
      future-disabled store, using the effective (union) disabled set. Non-disabled stores are
      unaffected.
- [ ] **AC14 — Null-model safety.** With `Globals.Ol.StoresWrapper` null, `IsDisabled` returns false
      and `GetDisabledStores` returns an empty (non-null) collection.
- [ ] **AC15 — Toolchain and coverage.** The full C# toolchain passes in order (csharpier →
      analyzers → nullable/TreatWarningsAsErrors → MSTest with coverage); new-code coverage meets repo
      policy; no repo-wide regression; all touched files remain under 500 lines.

## 10. Cross-Feature Contracts (fixed here, consumed later)

- `StoreIdentity.Resolve(displayName, filePathFallback = null)` (pure overload) is the identity
  contract for F3/F4/F5. The COM overload is for filter call sites only.
- `IStoreDisableService` (member `StoreDisable`) and the `DisabledStoreEntry`/`DisableScope` return
  shape are fixed; changing them after F5 is written would break a dependent feature.
- The `IStoreRehookService` seam and its no-op default are the sole F1↔F3 boundary. F1 takes no
  forward dependency on F3; F3 supplies the real implementation via a small in-scope edit.
