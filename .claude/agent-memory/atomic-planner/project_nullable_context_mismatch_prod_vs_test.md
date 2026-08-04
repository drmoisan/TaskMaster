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
