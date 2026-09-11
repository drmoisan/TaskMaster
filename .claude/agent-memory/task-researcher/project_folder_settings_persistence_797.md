---
name: folder-settings-persistence-797
description: "#797: live StoresWrapper deserialize path is SmartSerializable<T>, NOT SmartSerializableBase; the null-return contract is pinned by LCPPN fail-soft, so AC1 must land in AppOlObjects.StoreLoading.cs; ThisAddIn_Shutdown is never raised"
metadata:
  type: project
---

Research for issue #797 (folder settings never persist; User Email "Error Loading"), completed
2026-09-06 at base `c431dc32`. Findings that are not re-derivable by a quick grep:

1. **The live deserialize path is `SmartSerializable<T>`, not `SmartSerializableBase`.**
   `AppOlObjects.SmartSerializable` is an `ISmartSerializableNonTyped`; `SmartSerializableNonTyped.Deserialize<T,U>`
   calls `GetInstance<T>()` which returns `SmartSerializable<T>`, so overload resolution binds
   `SmartSerializable.cs` `Deserialize<U>(SmartSerializable<U> loader)`, not the base-class twin.
   The defect shape is identical in both files, which is why the issue text mis-attributed it.
   **Why:** any regression-surface enumeration read off `SmartSerializableBase` is the wrong set.
   **How to apply:** when tracing a `SmartSerializable.Deserialize<T,U>(config)` call in TaskMaster,
   follow the NonTyped forwarder to the generic class first.

2. **The file-absent path returns null and CANNOT be fixed by "copy the loader config onto the
   returned instance" — there is no instance.** Worse, the null return is a documented, load-bearing
   contract for a second production caller: `AppAutoFileObjects.FolderPredictorLoad.cs` relies on
   "returns null when the dedicated file is absent (fail-soft)" to fall back to the flat classifier.
   **Why:** constructing an instance there would silently disable the LCPPN fallback.
   **How to apply:** bootstrap-gap fixes of this shape belong at the CALLER (the fresh-build branch),
   not in the shared serializer.

3. **`ThisAddIn_Shutdown` is dead.** `TaskMaster\ThisAddIn.cs` still carries the stock VSTO comment
   "Outlook no longer raises this event". Any "flush on shutdown" design in this repo is unimplementable
   through that hook.
   **Why:** a plan that hosts a flush there would ship a no-op.
   **How to apply:** for deferred-write flush requirements, use a synchronous write on the explicit
   user action instead; `SmartSerializable<T>.SerializeThreadSafe(string)` is already public and
   re-arms the single-shot guard in its `finally`.

4. **`RequestSerialization` coalesces, it does not reset.** `ThreadSafeSingleShotGuard.CheckAndSetFirstCall`
   means the FIRST caller's `filePath` wins for the whole 3-second window; a second `Serialize(otherPath)`
   inside the window silently writes to the first path. The Elapsed callback runs on a background
   ThreadPool thread, so a pending write is lost at process exit with no log line.

5. **`StoreWrapperController.cs` is 478/500 and four ACs land in it** — a partial split is mandatory.
   `SmartSerializable.cs` (613) and `SmartSerializableBase.cs` (545) are ALREADY over the 500-line cap.
   `AppOlObjects.cs` is 493. `AppOlObjects.StoreLoading.cs` is 75 (ample room).

6. **`PersistJunkFolderSelections`'s reflection lookup SUCCEEDS in production.** `AppOlObjects.ApplyJunkFolderSelections(string,string)`
   exists with a matching signature; the "method not found" warn branch is reachable only by test doubles.
   The real defect is the double persistence: per-store JSON stores a path relative to the SELECTED
   store's root, while `Properties.Settings.Default.OlJunkCertain/JunkPotential` is a single global pair
   resolved against `AppOlObjects.Root` = the DEFAULT store's root.

7. **An existing test codifies the AC8 bug.** `StoreWrapperController_Tests.ButtonAndPopulate.cs`
   `PopulateWithCurrent_NullCurrent_SetsErrorLoadingText` asserts `Throw<NullReferenceException>()`
   despite its name. Fixing AC8 requires inverting it — call that out explicitly.

8. **The `&` vs `&&` in `GetRelativeFsPath` is inert.** Both operands call the null-tolerant
   `StringExtensions.IsNullOrEmpty(this string?)` extension and are side-effect free, so the
   non-short-circuit form cannot throw. Do not claim the fix repairs a fault. Separately, the
   condition is effectively dead because `FilePathHelperConverter.GetSerializablePath` never returns
   an empty name (it returns "Not Found").

9. **The recurring `_ExchangeUser.get_PrimarySmtpAddress()` UI-thread stalls in the runtime log are
   NOT `StoreWrapper.GetSmtpAddressFromStore`.** They come from `RecipientStatic.GetRecipientAddress`
   in the QuickFiler mail-load path. Out of scope for #797.

10. **`TimeOutTask.RunWithTimeout` is unusable for Outlook COM.** Every overload runs the work via
    `Task.Run` on an MTA ThreadPool thread; an STA-bound Outlook interop call marshals straight back
    to the STA, so the UI thread still blocks and the timeout only abandons the caller's wait.
    `AppOlObjects.ResolveCurrentUserEmailAddress` documents the same STA constraint in-code.

11. **`AppOlObjects.TryGetSmtpAddress(AddressEntry)` is the in-repo precedent for the AC6 fallback
    chain** (PrimarySmtpAddress in its own try/catch, then `AddressEntry.Address` when it contains "@").
    It lives in TaskMaster, so `StoreWrapper` (UtilitiesCS) cannot call it — replicate the shape.
    `StoreWrapper.RestoreGlobalAddresses`/`GlobalAddressBook` is dead (no caller anywhere).

12. **Project reference direction is one-way `TaskMaster -> UtilitiesCS`.** `UtilitiesCS.csproj` has
    exactly one ProjectReference, to SVGControl. `IApplicationGlobals.cs` carrying `using TaskMaster;`
    is NOT a cross-reference — `UtilitiesCS\Interfaces\IGlobals\IAppEvents.cs` declares
    `namespace TaskMaster` inside UtilitiesCS itself.

Related: [[storewrapper-dialog-287-state-inversion]], [[store-runtime-reenable-263]].
