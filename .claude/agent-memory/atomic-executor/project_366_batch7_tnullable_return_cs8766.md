---
name: 366-batch7-tnullable-return-cs8766
description: On #366 nullable batches, annotating a generic-class method return as T? when it implements an out-of-scope null-oblivious interface member declared T triggers CS8766; conform to T + justified ! instead
metadata:
  type: project
---

On epic child #366 (utilitiescs-nullable-reusabletypes), Batch 7 (SmartSerializable family), the prior agent annotated `SmartSerializable<T>.DeserializeObject` with a `T?` return. Under the isolated per-file pragma gate (`/t:Rebuild /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`, no `/p:Nullable=enable`) this emitted `error CS8766` (nullability of return type doesn't match implicitly implemented member) against `ISmartSerializable<T>.DeserializeObject` — which is declared `T` and lives in `UtilitiesCS/Interfaces/IReusableTypeClasses/ISmartSerializable.cs`, OUT OF SCOPE (not one of the 7 Batch 7 ReusableTypeClasses files).

**Why:** CS8766 fires even against a null-oblivious interface when the implementing type parameter is `where T : class` constrained (constrained `T` return is treated as non-null in the nullable-enabled implementation, so `T?` is strictly more nullable than the interface's `T`). Method-level unconstrained `<T>` returns (e.g. `SmartSerializableBase.DeserializeObject<T>`, `SmartSerializableNonTyped.DeserializeObject<T>`) do NOT emit CS8766 with `T?` — only the class-level `class`-constrained `T` case does.

**How to apply:** Do NOT edit the interface (out of scope; would be a SCOPE-CHANGE STOP). Conform the implementation return type to the interface's `T` and return the genuinely-nullable local with a justified `!` + `// why` comment noting the oblivious interface contract is preserved (AC5 signature-compatible). Watch for this same pattern on any remaining child (#367 etc.) that annotates a `class`-constrained generic method implementing an oblivious interface member. Baseline whole-assembly gate decomposition for UtilitiesCS.csproj isolated compile is 28 CS0618 + 2 CS0168 (all pre-existing non-cluster files: Triage, SortEmail, ManagerAsyncLazy, etc.) — same as Batch 6.
