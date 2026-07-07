---
name: record-struct-isexternalinit-netfx
description: Any init accessor (positional record, record struct, or explicit { get; init; }) fails CS0518 on this net48 target because IsExternalInit is absent; use a constructor-initialized readonly struct with get-only props
metadata:
  type: project
---

Any use of the C# `init` accessor fails to compile in the first-party .NET Framework
(net48) projects of this repo with **CS0518: Predefined type
'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported**. This is
NOT limited to positional records — it applies equally to:
- positional `record` / `record struct` (compiler-generated `init` accessors),
- an explicit `public T Prop { get; init; }` on any type (including a non-positional
  `readonly record struct` with an "explicit body"), which was mistakenly believed safe.

**Why:** the `init` accessor is lowered with a `modreq(IsExternalInit)`; the .NET
Framework reference assemblies do not provide `IsExternalInit` and no polyfill exists in
the repo (no `class IsExternalInit` anywhere; no production `.cs` uses `{ get; init; }`).
CSharpier passes, but the analyzer/nullable msbuild step fails with CS0518 under
TreatWarningsAsErrors (and the plain compile fails too).

**How to apply:** implement small immutable value types as a plain `public readonly
struct` with an ordinary constructor and get-only auto-properties (`{ get; }`), never
`record struct`/positional/`init`. Repo precedent that documents this in-code:
`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` `ResourceTimingRow` (~line 165) and
`TaskMaster/AppGlobals/HookReadinessCoordinator.cs` (line 12). Encountered again on the
F1 store-disable-service (#261) atomic plan, whose `StoreIdentity`/`DisabledStoreEntry`
were spec'd as `readonly record struct` with `{ get; init; }` (would not compile).
Related toolchain quirks: [[project_build_test_env]].
