---
name: 418-500line-gate-vs-plan-content
description: Issue #418 P1-T19 — plan mandated ~193 net new lines in a 354-line file with 146 lines of headroom, so the 500-line gate was unsatisfiable; per-block logging acceptance clauses blocked centralization
metadata:
  type: project
---

An atomic plan can mandate more new code than its target file's 500-line headroom allows, making its
own "tighten until compliant" task unsatisfiable. Check the arithmetic during preflight: sum the
estimated line cost of every production task and compare against `500 - <current file length>`.

**Why:** Issue #418 `[P1-T19]` required `SVGControl/SvgRenderer.cs` <= 500 lines. The file was 354
lines (146 headroom), but `[P1-T10]`–`[P1-T18]` mandated a parse-failure boundary, three public
parse members, two rewritten constructors, two pure probe helpers, and an `AssemblyResolve`
strategy-3 block. First pass landed at 603; a full tightening pass reached only 547. csharpier
reformats from the AST, so the post-format count is stable — hand-compressing further does not help.

Three tightening levers were blocked by the plan's own acceptance clauses:
- `[P1-T11]` required its `catch (Exception` block to literally contain both `logger.Error` and
  `Trace.TraceError`, and `[P1-T14]` required *both* byte-array constructors to contain both calls.
  Routing all four sites through one helper saves only ~4 lines and breaks three clauses.
- Chaining the 4-arg constructor to the 3-arg one saves ~16 lines but breaks `[P1-T14]` and makes the
  log record name the wrong signature.
- Deleting pre-existing members (unreferenced private `AddMargins`, 19 lines) exceeds a mandate to
  "tighten the added code".

**How to apply:** When the gate is unreachable, report `SCOPE_EXCEEDED` with the measured count and a
concrete plan delta rather than bending an acceptance clause. Here the clean delta was extracting the
two *pure* helpers (`TryGetDirectoryFromCodeBase`, `GetProbeDirectories` — 50 lines) into a new
`SVGControl/SvgAssemblyProbe.cs`, projecting 497 lines. That needs the Scope Lock amended to add the
new file *plus* `SVGControl/SVGControl.csproj` for its `<Compile Include>` (legacy non-SDK, no glob;
see [[project_legacy_csproj_no_transitive_compile_refs]]). Stop before the test-authoring tasks: the
delta moves which type the helpers live on, so tests written first would bind to a surface that moves.

Related: [[project_csharpier_pipefiles_nonenforcing_gate]] (size new files AFTER formatting).
