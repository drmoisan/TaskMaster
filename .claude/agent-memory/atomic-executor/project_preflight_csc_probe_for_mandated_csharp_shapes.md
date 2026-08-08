---
name: preflight-csc-probe-for-mandated-csharp-shapes
description: During C# plan preflight, prove a mandated code shape compiles with a standalone Roslyn csc probe in the scratchpad instead of reasoning about the spec
metadata:
  type: project
---

When a plan task dictates an exact C# construct (a seam shape, a property/field
initializer, a generic constraint), verify it with a throwaway file compiled by
`csc.exe` in the session scratchpad rather than reasoning from the language spec.

Resolve the compiler with the same vswhere lookup the plans use:

- `vswhere.exe -latest -products * -find 'MSBuild\**\Bin\Roslyn\csc.exe'`
- `csc.exe -nologo -t:library -langversion:preview -out:probe.dll probe.cs`

Probe both directions: the mandated shape must exit 0, and the prohibited shape
must emit the exact diagnostic the plan claims. Confirmed on #433 F7 `[P3-T3]`
(`_metricsAdder ?? DefaultMetricsAdder` method-group lazy default compiles; the
`{ get; set; } = (a,b,c) => _metrics.TryAdd(...)` form is exactly CS0236).

**Why:** preflight bans running msbuild/vstest/csharpier against the worktree, and
a `PREFLIGHT: ALL CLEAR` on a construct that will not compile costs a whole
plan-revision cycle. A standalone csc invocation touches nothing in the worktree,
so it stays inside the preflight constraint while producing a real compiler verdict.

**How to apply:** use it whenever a delta was added specifically to fix a compile
error, or whenever a plan asserts "X is a CSnnnn error" as its justification. Write
the probe under the session scratchpad, never under the worktree. See
[[project_418_plan_rationale_clauses_are_evidence]] for the related rule that a
plan's prose rationale is itself an auditable claim.
