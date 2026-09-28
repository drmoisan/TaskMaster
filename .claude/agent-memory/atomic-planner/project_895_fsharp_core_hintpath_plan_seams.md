---
name: project-895-fsharp-core-hintpath-plan-seams
description: "#895 (FSharp.Core HintPath netstandard2.1 skew) R0 planning seams - spec's 'no 879 folder' claim false; AC5 grep token occurs twice; DataRow DisplayName bracket token for TRX row identity; Rebuild keeps copied-reference timestamps; Meziantou 3.0.203 skew still needs a nuget bootstrap; runner document-state from console literals; build-lock dir parametrised; 8 of 48 first-draft task lines lacked a path token"
metadata:
  type: project
---

Authored 2026-09-17 for `docs/features/active/2026-09-14-fsharp-core-hintpath-netstandard21-skew-895/plan.2026-09-16T23-27.md` (48 tasks, 10/8/4/2/15/9). No preflight round had run when this was written.

**Seams found while authoring:**

- **A spec "verified: no folder matching N exists" claim is a citation; Glob it.** `spec.md` 261-263 said no `879` folder exists under `docs/features`; `docs/features/active/2026-09-13-...-879/` exists with a fully checked plan (0 unchecked task lines) and a preflight-clearance artifact. The ownership conclusion survived; the claim did not. Recorded as an observed correction, spec not amended.
- **An AC that says "grep for X returns the stale sentence at line N" must be re-counted.** `unsatisfiable` occurs at `NetstandardBindChildDomainTests.cs` 281 (retained, still true) and 366 (stale). Author the replacement remark WITHOUT the word so the gate is a 2-to-1 transition, and add a fresh single-line token (`display-name tests`, 0 before) for the presence half. `display-name creation` at 443 is the near-miss.
- **TRX row identity for `[DataRow]` tests: set `DisplayName` to the method name plus a bracketed argument** (`... [QuickFiler]`), so ordinal `Contains("[QuickFiler]")` cannot match the `[QuickFiler.Test]` row. `[DataTestMethod]` is still the in-repo form under MSTest 4.4 (21 files).
- **`/t:Rebuild` re-copies references with their SOURCE timestamps.** A freshness gate on `FSharp.Core.dll` LastWriteTime fails on a genuine rebuild; assert freshness on the project's own compiled `<P>.dll` plus the echoed `/out:obj\Debug\<P>.dll` csc line per project.
- **Meziantou `3.0.203` `<Analyzer Include>` skew persists on 15 of 16 first-party csproj (TaskMaster.csproj is already 3.0.235); packages.config pins 3.0.235 in a multi-line element** (`id=` and `version=` on separate lines, so a one-line regex finds nothing). A fresh worktree restore leaves CS0006 until `nuget install Meziantou.Analyzer -Version 3.0.203 -OutputDirectory packages`; that is issue #898 and must stay a `packages/`-only bootstrap, never a csproj edit.
- **Shape-A csproj walk must skip every dot-prefixed directory, not only `.git`/`.claude`:** Phase 0 installs `.dotnet-sdk` under the root. The AC's literal list is a subset; the superset reading is the only one under which "exactly six" stays satisfiable.
- **Coverage-runner document state: read the runner's console literals.** `MSTest with coverage failed with exit code` (line 262, throws before post-processing) means raw; `is below the required` (Threshold.ps1 52/122, after post-processing) means processed with a threshold breach. The `<sources>` test may not discriminate (879 preflight O1) and a drive-letter scan is always true on processed docs (#731 R7).
- **Build-lock scripts exist at `<TaskMaster-wt>/parallel-build-lock/{acquire,release}.txt`** (`-Item`, print `ACQUIRED <item>` / `TIMEOUT`, 60-minute wait). Reference them through a caller-supplied `BUILD-LOCK-DIR` token, not the absolute path the 879 plan hard-coded.
- **`PEReader` needs `System.Collections.Immutable` referenced** (overload resolution over the `ImmutableArray<byte>` ctor); `TaskMaster.Test.csproj` 204-205 already has it with an app.config redirect at 47. No prior `PEReader` use in the repo.
- **The planner-output hook's path check reads the task's FIRST line only:** 8 of 48 first-draft tasks (every commit task whose line ended `Commands:`, and payload-opening lines) failed it. Verify with a positive `[\\/]` count against the task-line count before ending the turn.
- **Green scoped runs need `COUNTERS_` from the TRX**, and the whole-namespace Bootstrap run totals 29 (9 + 2 + 2 + 16); `AfterInstall_DeedleTypeInitializerSucceeds` stops discriminating after the fix and is recorded, not fixed.

Related: [[project-816-iscompleted-branch2-ac5-plan-seams]], [[project-824-ilglobals-static-publication-plan-seams]], [[project-731-r6-coverage-runner-bypass-seams]], [[validate-planner-output-hook-line-anchored-gotchas]], [[project-planner-mcp-validator-not-in-tool-surface]].
