# Research: Dictionary Lineage Migration (F1, issue #306)

Epic: swordfish-removal — Child feature F1 (Dictionary lineage migration)
Canonical issue: #306
Date: 2026-07-10T20-16
Scope: Replace vendored Swordfish-based `ScoDictionary<TKey,TValue>` with the Swordfish-free
`ScoDictionaryNew<TKey,TValue>` at every production consumer, migrate affected tests, and preserve
on-disk JSON compatibility for persisted dictionaries. Deletion of `UtilitiesSwordfish`,
`ProjectReference` removal, `.sln` edits, and `IScoCollection`/collection/stack/sorted-dictionary
migration are explicitly out of scope (F2/F3/F5).

All file references are relative to the worktree root
`C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a0bb15bdb226acc2c`.

---

## 1. Current-state summary of the two lineages

### Legacy `ScoDictionary<TKey,TValue>`
- File: `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`.
- Base class: Swordfish `ConcurrentObservableDictionary<TKey,TValue>`
  (`using Swordfish.NET.Collections;` at line 13; base clause line 24).
- Implements `IScoDictionary<TKey,TValue>`
  (`UtilitiesCS\Interfaces\IReusableTypeClasses\ISCODictionary.cs`), which itself extends
  `IDictionary<,>`, `ICollection`, `INotifyCollectionChanged`, `IDisposable` and declares the
  serialization surface (`Filename`/`Filepath`/`Folderpath`, `Serialize*`, `Deserialize*`,
  `ToDictionary()`, nested `AltLoader` delegate).

### New `ScoDictionaryNew<TKey,TValue>`
- File: `UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNew.cs`.
- Base class: clean `ConcurrentObservableDictionary<TKey,TValue>` at
  `UtilitiesCS\ReusableTypeClasses\Concurrent\Observable\Dictionary\ConcurrentObservableDictionary.cs`,
  which derives from `System.Collections.Concurrent.ConcurrentDictionary<TKey,TValue>` (line 15-17).
  No Swordfish dependency.
- Implements `IScoDictionaryNew<TKey,TValue>`
  (`UtilitiesCS\Interfaces\IReusableTypeClasses\SerializableNew\Concurrent\Observable\IScoDictionaryNew.cs`),
  which extends `IConcurrentObservableDictionary<TKey,TValue>` + `ISmartSerializable<ScoDictionaryNew<TKey,TValue>>`.
- Serialization is delegated to a composed `SmartSerializable<ScoDictionaryNew<TKey,TValue>>` field
  named `ism` (ScoDictionaryNew.cs lines 107-172) and to the static helper class
  `ScoDictionaryNew<TKey,TValue>.Static` (lines 212-277).

---

## 2. PRIMARY question — type-name-embedding / `TypeNameHandling`

**Finding: on-disk payloads are NOT type-name-embedded, and renaming `ScoDictionary` ->
`ScoDictionaryNew` does NOT break deserialization of existing files, provided the migrated
consumer uses the default (non-globals) serialization path. No `SerializationBinder` and no
custom converter migration is required.**

Evidence, per serializer configuration:

1. Legacy production write path — `SCODictionary.SerializeThreadSafe` (SCODictionary.cs lines
   216-245):
   ```
   var settings = new JsonSerializerSettings();
   //settings.TypeNameHandling = TypeNameHandling.Auto;   // line 227 — COMMENTED OUT
   settings.Formatting = Formatting.Indented;
   var serializer = JsonSerializer.Create(settings);
   serializer.Serialize(sw, this);
   ```
   `TypeNameHandling` is left at its default `None`. The Swordfish base implements
   `IDictionary<,>`, so Newtonsoft uses a dictionary contract and writes a flat
   `{"key": value, ...}` object with **no `$type`, no `$id`, no wrapper**.

2. Legacy production read path — `SCODictionary.Deserialize(string, bool)` (lines 382-400) and the
   `AltLoader` overload (lines 285-289) read the file with
   `JsonConvert.DeserializeObject<Dictionary<TKey,TValue>>(strObject)`. Deserializing into a plain
   `Dictionary<TKey,TValue>` only succeeds against a flat key/value object, confirming the
   canonical on-disk shape is a flat dictionary with no type discriminator.

3. New default settings — `SmartSerializable<T>.GetDefaultSettings()` (SmartSerializable.cs lines
   442-449) and `SmartSerializableBase.GetDefaultSettings()` (SmartSerializableBase.cs lines
   382-389):
   ```
   new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, Formatting = Indented }
   ```
   No converter, no `SerializationBinder`, no `PreserveReferencesHandling`. On write,
   `SmartSerializable.SerializeToStream` (lines 506-519) calls
   `serializer.Serialize(sw, _parent, _parent.GetType())` when `TypeNameHandling == Auto`. Because
   the declared type equals the runtime type, `Auto` emits no `$type` at the root; `string`/`int`
   values are sealed primitives, so no `$type` on values either. `ScoDictionaryNew` also derives
   from `ConcurrentDictionary<,>` (dictionary contract), so its `[JsonProperty] Config` and `ism`
   properties are ignored on this path. Result: the same flat `{"key": value}` object.

4. On read, `SmartSerializableBase.DeserializeJson` /`SmartSerializable.DeserializeJson` call
   `JsonConvert.DeserializeObject<T>(text, settings)` with those default settings (SmartSerializable.cs
   lines 376-396). A flat legacy file binds directly into `ScoDictionaryNew`'s dictionary entries.

5. Behavioral confirmation in existing tests: both lineages round-trip through identical
   `TypeNameHandling.Auto` + `Formatting.Indented` settings and preserve entries —
   `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs:140-163`
   (`JsonRoundTrip_ScoDictionary_PreservesEntries`) and
   `UtilitiesCS.Test\ReusableTypeClasses\ScoDictionaryNew_Tests.cs:151-174`
   (`JsonRoundTrip_PreservesEntries`). Cross-type binding of the flat shape is exercised by
   `SmartSerializableBase_Tests.cs:47-63`, which serializes a `ScoDictionary<string,int>` and reads
   it back through the SmartSerializable machinery.

**The only way this migration breaks the on-disk format is the "globals" settings path.**
`ScoDictionaryNew<TKey,TValue>.GetSettingsJson<T>(globals)` and `Static.GetSettingsJson<T>(globals)`
(ScoDictionaryNew.cs lines 196-231) register `PreserveReferencesHandling.All` **and** a
`ScoDictionaryConverter<T,TKey,TValue>` (lines 208, 229). That converter
(`UtilitiesCS\NewtonsoftHelpers\ScoDictionaryConverter.cs`) rewrites the payload into the wrapper
object produced by `WrapperScoDictionary<TDerived,TKey,TValue>`
(`UtilitiesCS\NewtonsoftHelpers\WrapperScoDictionary.cs`), i.e.
`{ "$id": "...", "CoDictionary": { ... }, "RemainingObject": { ... } }`. This is a different,
incompatible on-disk shape.

**Directive for the plan:** the four persisted dictionaries (section 4) MUST be re-pointed using the
default `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)` path and plain
`Serialize()` — NOT `GetSettingsJson<T>(globals)`, and the converter/`PreserveReferencesHandling`
must not be registered for them. Under that constraint no binder or converter work is required and
existing on-disk files remain readable.

---

## 3. Construction / deserialize-shape reconciliation (API surfaces)

### Legacy `ScoDictionary` construction & persistence surface
- Constructors used by consumers (SCODictionary.cs):
  - `ScoDictionary(string filename, string folderpath)` (lines 50-56) — sets `Filename`/`Folderpath`
    then calls `Deserialize()` in-place (self-loading constructor).
  - `ScoDictionary(IDictionary<TKey,TValue> dictionary, string filename, string folderpath)`
    (lines 65-75) — copies entries then `Serialize()` immediately.
  - `ScoDictionary(IDictionary<TKey,TValue> source)` (lines 31-32) — in-memory copy.
  - Parameterless `ScoDictionary()` (lines 28-29).
- Persistence surface (from `IScoDictionary`): `Serialize()`, `Serialize(filepath)`,
  `SerializeAsync()`, `SerializeAsync(filepath)`, `Deserialize()`, `Deserialize(bool)`,
  `Deserialize(filepath, bool)`, `Deserialize(filepath, AltLoader, bool)`, mutable
  `Filename`/`Filepath`/`Folderpath`, and `ToDictionary()` (no-arg). Load is **in-place**:
  deserialize mutates `this`.

### New `ScoDictionaryNew` construction & persistence surface
- Constructors (ScoDictionaryNew.cs lines 31-93): parameterless, `IEnumerable<KeyValuePair<>>`,
  `IEqualityComparer`, collection+comparer, concurrency-level overloads, and a copy constructor
  `ScoDictionaryNew(ScoDictionaryNew<TKey,TValue>)`. **There is no `(filename, folderpath)`
  self-loading constructor.**
- Load is **factory-style, returns a new instance** (does not mutate an existing one):
  - `ScoDictionaryNew<TKey,TValue>.Static.Deserialize(fileName, folderPath)` (lines 233-236).
  - `Static.Deserialize(fileName, folderPath, askUserOnError)` (lines 238-242).
  - Instance `Deserialize(fileName, folderPath[, askUserOnError[, settings]])` (lines 124-138).
- Persistence surface (ISmartSerializable, ScoDictionaryNew.cs lines 109-117): `Serialize()`,
  `Serialize(filePath)`, `SerializeThreadSafe(filePath)`, `SerializeToString()`,
  `SerializeToStream(StreamWriter)`. File path/config is carried by `Config`
  (`NewSmartSerializableConfig`, exposed line 99-104), not by loose `Filename`/`Folderpath`
  properties.
- Note: writes are **deferred** — `SmartSerializable.RequestSerialization` (SmartSerializable.cs
  lines 533-542) schedules the actual write via a 3-second `ITimerWrapper`. Compatibility tests must
  drive `SerializeToString()` / `SerializeThreadSafe(path)` directly (or inject the test
  `TimerFactory` seam) rather than relying on wall-clock elapse; the deterministic-timer seam is
  already established in the SmartSerializable tests (`TimerFactory` property, lines 518/530).

### Per-consumer current construction/persistence today
- `AppToDoObjects._dictRemap` (`TaskMaster\AppGlobals\AppToDoObjects.cs:299-313`):
  `new ScoDictionary<string,string>(filename: FnameDictRemap, folderpath: pythonStaging)` — self-loading ctor. PERSISTED.
- `AppToDoObjects._filteredFolderScraping` (`AppToDoObjects.cs:427-441`):
  `new ScoDictionary<string,int>(_defaults.FileName_FilteredFolderScraping, pythonStaging)` — self-loading. PERSISTED.
- `AppToDoObjects._folderRemap` (`AppToDoObjects.cs:455-469`):
  `new ScoDictionary<string,string>(_defaults.FileName_FolderRemap, pythonStaging)` — self-loading. PERSISTED.
- `SubjectMapEncoder._encoder` (`UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`):
  constructed in ctor (line 22) and in `Encoder` getter (lines 90-94) via
  `new ScoDictionary<string,int>(filename, folderpath)` (self-loading); rebuilt via the
  `(dictionary, filename, folderpath)` ctor at lines 122-126 which serializes immediately;
  re-serialized via `_encoder.Serialize()` (lines 128, 173). Uses no-arg `_encoder.Deserialize()`
  (line 40) and no-arg `_encoder.ToDictionary()` (line 131) — both legacy-only members with no
  `ScoDictionaryNew` equivalent; these call sites must be rewritten (use factory `Static.Deserialize`
  and iterate the dictionary directly / `new Dictionary<>(_encoder)`). PERSISTED (encoder only).
- `SubjectMapEncoder._decoder`: built only from an in-memory `IEnumerable<KeyValuePair<>>`
  (lines 49, 129-134); never assigned a filename/folderpath and never serialized. IN-MEMORY ONLY.
- `FolderScorer._folderNameScores` (`UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs:27`):
  `= new()` (parameterless). No filename, no `Serialize`/`Deserialize` anywhere in the file
  (grep of the file shows only `.Clear()`, `.Count`, `.TryAdd`, indexer, LINQ). IN-MEMORY ONLY.

---

## 4. Persisted vs in-memory-only (determines which need a compatibility test)

| Consumer field | Type | Persisted? | On-disk compat test needed |
|---|---|---|---|
| `AppToDoObjects.DictRemap` (`_dictRemap`) | `ScoDictionary<string,string>` | YES (PythonStaging, `FileName_DictRemap`) | YES |
| `AppToDoObjects.FilteredFolderScraping` | `ScoDictionary<string,int>` | YES (PythonStaging) | YES |
| `AppToDoObjects.FolderRemap` | `ScoDictionary<string,string>` | YES (PythonStaging) | YES |
| `SubjectMapEncoder.Encoder` (`_encoder`) | `ScoDictionary<string,int>` | YES (ctor filename/folderpath) | YES |
| `SubjectMapEncoder.Decoder` (`_decoder`) | `ScoDictionary<int,string>` | NO (derived in-memory) | no |
| `FolderScorer._folderNameScores` | `ScoDictionary<string,long>` | NO (`new()`, never serialized) | no |

**Discrepancy to flag:** the epic lists "FolderScorer scores" among persisted dictionaries. The
current code does not persist `_folderNameScores` (no filename, no serialize/deserialize). It is a
transient per-mail scoring buffer that is `Clear()`-ed on each load. No on-disk compatibility test is
warranted for it; the migration there is a pure type swap. Likewise the epic does not list
`DictRemap`, but `DictRemap` IS persisted and does need a compatibility test.

Net: four persisted dictionaries require on-disk round-trip compatibility tests — `DictRemap`,
`FilteredFolderScraping`, `FolderRemap`, and the SubjectMap `Encoder`.

---

## 5. Test surface

### Direct legacy-`ScoDictionary` tests (migrate or delete with the class)
- `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs` — constructs `ScoDictionary<...>`
  throughout; includes the JSON round-trip test (lines 140-163).
- `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Additional_Tests.cs` — defines
  `RecordingScoDictionary : ScoDictionary<string,int>` (line 272) exercising the legacy
  serialize/deserialize/dialog seams (`DirectoryExists`, `CreateText`, `ShowMyBoxDialog`, etc.).
  These seams exist only on the legacy class; there is no equivalent to migrate them onto — they are
  deleted together with the legacy class.

### Consumer-coupled test fixtures (must switch type when consumers switch)
These construct legacy `ScoDictionary` (or `IScoDictionary`) to satisfy the consumer contracts and
must move to `ScoDictionaryNew`/`IScoDictionaryNew` when the production interfaces change:
- `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs` (lines 28, 71, 90, 114,
  134, 151, 172, 226) — `new ScoDictionary<string,int>()` for FilteredFolderScraping.
- `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Orchestration_Tests.cs` (lines 272, 279, 285) —
  FolderRemap / FilteredFolderScraping fixtures.
- `UtilitiesCS.Test\EmailIntelligence\OlFolderClassifierGroup_Tests.cs:93`.
- `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs:352` (`IScoDictionary<string,int> Encoder`).
- `UtilitiesCS.Test\EmailIntelligence\SubjectMapEncoder_Tests.cs` (comments referencing the
  legacy null-folderpath throw behavior at lines 91, 102 — behavioral expectations to re-verify).
- `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsTests.cs:144` and
  `EmailDetailsWrapperTests.cs:152` — `new ScoDictionary<string,string>(...)` dictRemap fixtures
  (see ripple consumers, section 7).
- `TaskMaster.Test\AppGlobals\AppToDoObjectsTests.cs` / `AppToDoObjectsCoverageTests.cs` — currently
  reference `PeopleScoDictionaryNew`/`IPeopleScoDictionaryNew` only; verify DictRemap/FolderRemap
  assertions after the type change.

### SmartSerializable negative-sample tests (use `ScoDictionary` as a convenient non-ISmartSerializable type)
- `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableStatic_Tests.cs:25-28`
  (`IsSmartSerializable_ScoDictionary_ReturnsFalse`).
- `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableNonTyped_Tests.cs:21-96`.
- `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableBase_Tests.cs:47-80`.
  These do not test the migrated consumers; they use `ScoDictionary<string,int>` as a sample type
  that does not implement `ISmartSerializable<>`. If `SCODictionary.cs` is deleted, these tests need
  a substitute non-`ISmartSerializable` type (e.g. a plain `Dictionary<string,int>` or a tiny local
  stub). This is a cost that gates deletion (section 6).

### Reference patterns for the new compatibility round-trip tests
- `UtilitiesCS.Test\ReusableTypeClasses\ScoDictionaryNew_Tests.cs` — round-trip (151-174),
  serialize/deserialize with the SmartSerializable loader (`CreateLoader`, lines 537-560),
  `GetSettingsJson` converter-registration assertion (498-512).
- `UtilitiesCS.Test\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNewTests.cs`.
- `UtilitiesCS.Test\NewtonsoftHelpers\ScoDictionaryConverterTests.cs` and `WrapperScoDictionaryTest.cs`
  — show the wrapper/globals path (the format to AVOID for these consumers), useful as a negative
  reference to assert the migrated dictionaries do NOT emit the wrapper shape.

Recommended new tests (no test code here, per policy): for each of the four persisted dictionaries,
a deterministic round-trip that (a) writes a flat `{"key":value}` string via a legacy-equivalent
serialize, (b) loads it through the new `Static.Deserialize` path via an injected `ReadAllText`
seam, asserts entries; and (c) re-serializes via `SerializeToString()` and asserts the output is a
flat object with no `$type`/`$id`/`CoDictionary`/`RemainingObject` tokens. Use the injectable
`ReadAllText`/`DiskExists`/`CreateStreamWriter`/`TimerFactory` seams already present on
`SmartSerializable`/`SmartSerializableBase` to avoid any temporary files (temp files are prohibited
by policy).

---

## 6. Delete-eligibility of `SCODictionary.cs`

After re-pointing the enumerated production consumers (AppToDoObjects x3, SubjectMapEncoder,
FolderScorer) and the interface `IToDoObjects`/`ISubjectMapEncoder`, plus the ripple consumers in
section 7, the concrete legacy `ScoDictionary<>` class becomes **production-unreferenced**. It
remains referenced by test code:

- Direct legacy tests: `SCODictionary_Tests.cs`, `SCODictionary_Additional_Tests.cs` — deletable
  together with the class.
- Consumer-coupled fixtures (section 5) — migrated to `ScoDictionaryNew`.
- SmartSerializable negative-sample tests (section 5) — require a substitute non-`ISmartSerializable`
  sample type; this is the main friction point for deletion.

The legacy interface `IScoDictionary<TKey,TValue>` (`ISCODictionary.cs`) is a separate type. It stays
referenced by the ripple production consumers (EmailDetails/EmailDetailsWrapper, section 7) unless
those signatures are also changed, and by `SubjectMapSco_Tests.cs:352`. Deleting the class does not
require deleting the interface.

**Conclusion:** `SCODictionary.cs` deletion is achievable within F1 only if every test reference
(including the three SmartSerializable negative-sample tests) is migrated in the same change.
Because the task states deletion is optional, the recommended posture is: re-point all consumers and
migrate consumer-coupled fixtures; treat `SCODictionary.cs` (and its two direct test files)
deletion as an optional final step, executed only if the SmartSerializable negative-sample tests are
also updated within budget. If the interface `IScoDictionary` must survive for the ripple consumers,
that alone does not block class deletion, but it does mean the Swordfish base type is still reachable
through the class until deletion completes (final Swordfish removal is F5 regardless).

---

## 7. Ripple consumers beyond the enumerated list (in dictionary-lineage scope)

`DictRemap` is exposed as `IScoDictionary<string,string>` on `IToDoObjects` (line 19) and flows into
production methods that accept the legacy interface directly:
- `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs` — parameters
  `IScoDictionary<string,string> dictRemap` at lines 32, 71, 308.
- `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs` — parameter at line 17.

Re-pointing `DictRemap` to `IScoDictionaryNew<string,string>` requires changing these signatures (and
their test fixtures, `EmailDetailsTests.cs:144`, `EmailDetailsWrapperTests.cs:152`). This widens F1's
blast radius beyond the five files named in the task; all of it is dictionary-lineage and therefore
in scope. `CaptureEmailAddressesModule2.cs:15` and `ToDoModel\Email Utilities\CaptureEmailAddressesModule.cs:11`
contain only commented-out `IScoDictionary` references (inert). `AutoFile.cs` already consumes
`IScoDictionaryNew<string,string>` (test refs `AutoFile_Tests.cs:173/213/257`), so it is already on
the new lineage and needs no change.

---

## 8. PeopleScoDictionary status (Q6)

`ToDoModel\Data Model\People\PeopleScoDictionary.cs` is **entirely commented out** — the whole file
(lines 1-213, including `public class PeopleScoDictionary : ScoDictionary<string,string>`) is inside
a block comment. It contains no live `ScoDictionary` reference. The live People type is
`PeopleScoDictionaryNew : ScoDictionaryNew<string,string>`
(`ToDoModel\Data Model\People\PeopleScoDictionaryNewBackup.cs:19`, consumed in
`AppToDoObjects.cs:176`). The commented `PeopleScoDictionary` file requires no code change for F1
(optionally deletable as dead code, but that is outside the stated re-point work).

---

## 9. Candidate approaches

### Recommended: default-path re-point (flat-format preservation)
Re-point each consumer field/property/interface member to `ScoDictionaryNew<>`/`IScoDictionaryNew<>`,
construct persisted instances via `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)`,
persist via plain `Serialize()`/`SerializeToString()`, and never register the globals-based
`GetSettingsJson`/`ScoDictionaryConverter`/`PreserveReferencesHandling` for these dictionaries. This
preserves the flat `{"key":value}` on-disk shape (section 2) so existing files remain readable with
zero data-migration work. In-memory-only fields (`Decoder`, `_folderNameScores`) become a pure type
swap. Rationale: satisfies the compatibility requirement with the least mechanism, matches the
established default-path usage already proven by `ScoDictionaryNew_Tests`, and confines new machinery
to test coverage.

### Rejected alternatives (brief)
- **Wrapper/globals settings path** (register `ScoDictionaryConverter` + `PreserveReferencesHandling.All`
  via `GetSettingsJson<T>(globals)`): rejected — changes the on-disk shape to
  `{"$id","CoDictionary","RemainingObject"}`, breaking every existing persisted file and violating
  the F1 compatibility requirement.
- **Serialization binder / dual-format converter to map the old type token**: rejected as
  unnecessary — the production legacy files carry no `$type` token (TypeNameHandling was disabled on
  write, SCODictionary.cs:227), so there is no old type token to map; a binder would add complexity
  with no compatibility benefit.

---

## 10. Requirements mapping (design deltas)

- `TaskMaster\AppGlobals\AppToDoObjects.cs`: change `_dictRemap`, `_filteredFolderScraping`,
  `_folderRemap` and their properties/loaders from `ScoDictionary<>` to `ScoDictionaryNew<>`; replace
  self-loading `new ScoDictionary<>(filename, folderpath)` with
  `ScoDictionaryNew<...>.Static.Deserialize(filename, folderpath)`. Preserve the existing
  `Initialized`/`Initializer.GetOrLoad` lazy pattern.
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`: change `FilteredFolderScraping` ->
  `ScoDictionaryNew<string,int>`, `FolderRemap` -> `ScoDictionaryNew<string,string>`, `DictRemap` ->
  `IScoDictionaryNew<string,string>`. Cross-module contract change.
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`: change `_encoder`/`_decoder` to
  `IScoDictionaryNew<string,int>`/`IScoDictionaryNew<int,string>`; replace self-loading ctor with
  `ScoDictionaryNew<...>.Static.Deserialize`; replace no-arg `Deserialize()` (line 40) and no-arg
  `ToDictionary()` (line 131) with new-lineage equivalents (factory load; iterate the dictionary or
  `new Dictionary<>(_encoder)`); keep `Serialize()` calls. Update `ISubjectMapEncoder.Encoder` return
  type to `IScoDictionaryNew<string,int>`.
- `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`: change `_folderNameScores` to
  `ScoDictionaryNew<string,long>` (pure type swap; in-memory).
- Ripple (section 7): `EmailDetails.cs`/`EmailDetailsWrapper.cs` `dictRemap` parameter type ->
  `IScoDictionaryNew<string,string>`.
- Tests: migrate consumer-coupled fixtures to `ScoDictionaryNew`; add four on-disk compatibility
  round-trip tests; substitute the SmartSerializable negative-sample type only if pursuing deletion.

No state machine is involved; this is a type/lineage swap with a serialization-format invariant. The
one behavioral state to preserve is the load-then-serialize-on-miss semantics: the legacy
self-loading ctor and the new `Static.Deserialize` both create-and-write an empty instance when the
file is absent (`SmartSerializable.Deserialize` -> `CreateEmpty` -> `Serialize`, SmartSerializable.cs
lines 304-352). Confirm the deferred-write timer does not change observable startup ordering for the
`LoadParallelAsync` tasks.

---

## 11. Testing implications (strategy, no test code)

- Use MSTest + Moq + FluentAssertions (repo policy). Cover, for each of the four persisted
  dictionaries: legacy-flat-file load into the new type, entry fidelity, and re-serialize-to-flat
  (assert absence of `$type`/`$id`/`CoDictionary`/`RemainingObject`).
- Drive serialization deterministically via the injectable seams (`ReadAllText`, `DiskExists`,
  `CreateStreamWriter`, `TimerFactory`) — no temporary files, no wall-clock waits (both prohibited).
- Preserve the existing negative-path behavior expectations (missing-file create-empty-and-write;
  `SubjectMapEncoder` duplicate-key rebuild path, SubjectMapEncoder.cs lines 47-81).
- Keep coverage on changed lines from regressing; new type-swap lines are low-risk but the SubjectMap
  encoder rewrite (Deserialize/ToDictionary reconciliation) is the highest-risk edit and needs
  focused positive/negative/edge tests.

---

## Automation Feasibility

No human-interaction or manual step is required. F1 is an entirely code-only C# migration: source
edits across the enumerated production files, interface contract changes, test migration/authoring,
and the standard toolchain runs (csharpier -> msbuild analyzers -> msbuild nullable/TreatWarnings ->
vstest with coverage). There is no third-party UI, no external service, and no interactive prompt on
the migration path — the only runtime dialogs (`MyBox`/`MessageBox`) live behind injectable seams
that tests stub. The autonomous-execution mandate is satisfied. No unavoidable manual step was
discovered during research.
