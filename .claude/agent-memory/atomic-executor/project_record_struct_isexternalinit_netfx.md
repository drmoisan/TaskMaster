---
name: record-struct-isexternalinit-netfx
description: Positional record/record struct fails CS0518 on this .NET Framework target because init accessors need IsExternalInit; use a constructor-initialized readonly struct
metadata:
  type: project
---

Positional `record` and `record struct` types fail to compile in the first-party
.NET Framework (net48) projects of this repo with **CS0518: Predefined type
'System.Runtime.CompilerServices.IsExternalInit' is not defined or imported**.

**Why:** the compiler-generated `init` accessors of positional records require the
`IsExternalInit` type, which the .NET Framework reference assemblies do not provide
and no polyfill is present in these legacy projects. The CSharpier format step and the
analyzer build both pass, but the analyzer/nullable msbuild step then fails with CS0518
under TreatWarningsAsErrors.

**How to apply:** when a task or plan calls for a small immutable value type ("private
nested readonly record/struct"), implement it as a plain `readonly struct` with an
ordinary constructor and get-only auto-properties instead of a positional `record
struct`. This avoids the `init` accessor entirely. Encountered while instrumenting
`IntelligenceConfig.ReadConfigurationAsync` (Issue #207). Related toolchain quirks:
[[project_build_test_env]].
