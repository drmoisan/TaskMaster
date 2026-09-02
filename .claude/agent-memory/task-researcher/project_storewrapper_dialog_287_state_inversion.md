---
name: storewrapper-dialog-287-state-inversion
description: "#287: the issue text inverts the two readiness states — StoresUnavailable is the transient one, ModelUnavailable is the permanent one; and the permanent/transient split does not map onto the enum at all"
metadata:
  type: project
---

Issue #287 (`storewrapper-dialog-imprecise-for-genuine-failure`) asserts that `StoresUnavailable`
("`StoresWrapper` populated but permanently unable to resolve") is the genuine-failure state. That is
backwards, and the fix the issue proposes cannot fully achieve its own stated goal.

**Verified against the code (research artifact:
`docs/features/active/2026-07-09-storewrapper-dialog-imprecise-for-genuine-failure-287/research/readiness-state-semantics.2026-08-31T21-10.md`):**

- `StoresUnavailable` is unambiguously TRANSIENT. `Stores ??= [];` is the FIRST statement of
  `StoresWrapper.RewireOlObjectsAsync` (`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs:87`), before
  any Stopwatch, COM call, or `await`. Two independent triggers reach it: the fire-and-forget
  `[OnDeserialized] RewireOlObjects` and the awaited `AwaitStoreRewireAsync`. Even a thrown COMException
  during store enumeration leaves `Stores` non-null.
- `ModelUnavailable` is BOTH transient and permanent. The permanent cause is the catch block at
  `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs:66-72` — no retry, no re-entry, `LoadStoresAsync`
  runs exactly once per session from `ThisAddIn.cs:76`. Codified by the existing test
  `LoadStoresAsync_WhenDeserializeThrows_AbsorbsExceptionAndLeavesStoresWrapperNull`
  (`TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:146`).
- Therefore branching the dialog copy on `StoreLaunchReadinessState` alone CANNOT tell a user "retrying
  will not help", because one enum value covers both a startup race and a permanent failure. Achieving
  the issue's literal goal needs a fourth state set inside the catch — larger than the issue scopes.

**Why:** the #262 spec (`docs/features/archive/2026-07-07-folder-settings-store-model-null-262/spec.md:200-203`)
states outright that on the genuine-failure path "`StoresWrapper` remains null, the readiness guard still
reports `ModelUnavailable`". The #240 research
(`docs/features/archive/2026-07-06-store-wrapper-launch-npe-240/research/…:76-88`) designed
`ModelUnavailable` to cover a transient race PLUS two permanent causes from the start. The doc comments on
`StoreWrapperController.cs:97-103` and `StoreLaunchReadinessEvaluator.cs:15-21` call BOTH states transient
and were never updated after #262 introduced the permanent path — so the in-source comments are
incomplete, and reading them is what produced the inverted issue text.

**How to apply:** for any work on #287, do not accept the issue's state attribution. Also: there are TWO
copy sites, not one — `DisabledStoresController.cs:167-168` carries a byte-identical message and title,
deliberately, per #265. Both `Launch()` methods are `[ExcludeFromCodeCoverage]`, so message-selection
logic must be extracted to a non-exempt member (precedent: `MyBoxModeless.BuildMessage`,
`UtilitiesCS/Dialogs/MyBoxModeless.cs:123`). `StoreWrapperController.cs` is 479 lines against the 500 cap;
put the helper in `StoreLaunchReadinessEvaluator.cs` (42 lines, already listed in the `.csproj`) rather
than a new file, since `UtilitiesCS.csproj` needs an explicit `<Compile Include>` for every new `.cs`
(see `UtilitiesCS/UtilitiesCS.csproj:744`).

Related: [[store-runtime-reenable-263]], [[store-lockup-resilience-f4-research]].
