---
name: nullable-context-mismatch-prod-vs-test
description: When planning C# signatures, check the target file's #nullable state AND the test project's LangVersion — prod files under #nullable enable need `?` annotations, C# 7.3 test projects must not have them
metadata:
  type: project
---

When a plan mandates new member signatures in a C# file, it must first check two independent facts and encode both as a Design Decision:

1. **Is the production file under `#nullable enable`?** If yes, every new `out` parameter that receives `null` on a failure path and every new return that can be `null` MUST be declared with `?`. net481 has no `[NotNullWhen]` post-condition attribute, so annotation cannot relieve the diagnostic. An unannotated `out SvgDocument` emits `CS8625`; an unannotated nullable return emits `CS8603`.
2. **Does the test project set `<LangVersion>`?** Legacy `packages.config` test projects often do not, so they compile as **C# 7.3**, where a `?` annotation emits `CS8370`/`CS8630`. A test-side `Mock<Func<byte[], SvgDocument>>` binds fine to a `Func<byte[], SvgDocument?>` parameter because nullability is metadata-only and both are the same CLR type.

**Why:** #418 preflight pass 3 blocked on this. The plan mandated non-nullable signatures in `SVGControl/SvgRenderer.cs`, which is `#nullable enable` at line 1 and already fully annotated. Those signatures would have introduced `CS8625`/`CS8603` — codes absent from the nullable baseline — which the plan's own no-new-diagnostics gate tasks and the Phase 2 `Output Summary: 0 errors` acceptance were required to reject. The plan contradicted itself.

**How to apply:** Before writing any task that states a literal C# signature, grep the target file for `^#nullable` and grep the consuming test `.csproj` for `LangVersion`. If the prod file is nullable-enabled and the test project is not, the plan must say explicitly that annotations are mandatory on the prod side and forbidden on the test side. Also verify the existing declaration: if the member already carries `?` (e.g. `public static SvgDocument? GetSvgDocument`), the task must say "preserve the annotation", not restate a bare type. Related: [[project_legacy_csproj_explicit_compile_include]], [[project_csharp_phase0_toolchain_bootstrap]].

## Adding `<LangVersion>latest</LangVersion>` to such a project is never "one property"

`CS8630` ("Invalid 'nullable' value: 'Enable' for C# 7.3") means the compiler **rejected the property and never ran nullable analysis**. Adding `<LangVersion>latest</LangVersion>` does not remove a diagnostic; it *enables* the whole nullable analysis pass over every file in the project. In a legacy WinForms test project that reliably surfaces a new set of `CS86xx` in auto-generated files nobody may edit:

- `private System.ComponentModel.IContainer components = null;` in each `*.Designer.cs` → `CS8625`
- uninitialized `resourceMan` / `resourceCulture` statics in each `Resources.Designer.cs` → `CS8618`
- `return resourceCulture;` → `CS8603`; `object obj = ResourceManager.GetObject(...)` → `CS8600`

Two rulings the orchestrator ratified on #418 that constrain the remedy: (i) `#nullable disable` / `#nullable restore` islands inside `*.Designer.cs` / `Resources.Designer.cs` are **not** a durable fix — `ResXFileCodeGenerator` and the WinForms designer erase them on the next regeneration, so the fix reverts itself with no signal, which is worse than a recorded measurement. An older plan's Scope Lock ratifying that route does not survive this objection. (ii) `CS8630` in one test project is a repo-wide condition, not a branch defect: nine test projects exist and only `TaskMaster.Test`, `UtilitiesCS.Test`, `VBFunctions.Test` set `LangVersion`; of the other six only `SVGControl.Test` reaches its own `CoreCompile` in a cold solution-wide nullable build, because the rest cascade-fail from `UtilitiesCS` first. Scope the follow-up entry repo-wide (`Directory.Build.props` or generator-aware exclusion), never to the one project that happened to expose it.

**How to apply:** never plan the property edit as a single atomic fix. Plan measure → gate → revert: (1) forced project-scope `MSBuild <proj>.csproj /t:Rebuild /p:Nullable=enable /p:TreatWarningsAsErrors=true` **before** the edit, (2) the edit, (3) the same rebuild after, enumerating every diagnostic by file, (4) a gate task that partitions diagnostics into in-scope vs out-of-scope files and reverts the property byte-identically if the out-of-scope set is non-empty, (5) the in-scope fixes *after* the gate so a revert only ever touches the `.csproj`. Also note that the mandated solution-level `Invoke-VSBuild.ps1 -EnableNullable` gate cannot detect any of this: the preceding analyzer `/t:Build` leaves every project up to date and legacy non-SDK up-to-date checks are timestamp-based, so the nullable build recompiles nothing.
