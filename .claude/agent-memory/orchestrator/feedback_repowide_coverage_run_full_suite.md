---
name: repowide-coverage-run-full-suite
description: Repo-wide C# coverage must be measured by running ALL test assemblies together; a single-assembly run produces a drastically understated false number
metadata:
  type: feedback
---

Repository-wide C# line coverage MUST be measured by running the FULL multi-assembly
test suite (every built `*.Test.dll`, exactly as `.github/workflows/ci.yml` does), never a
single test assembly. Before ever reporting a repo-wide coverage figure or treating a
coverage shortfall as a blocking finding, run the tooling and verify the number yourself.

**Why:** This mistake has recurred multiple times (most recently issue #248). vstest with
`/EnableCodeCoverage` instruments the whole solution regardless of which assemblies run, but
only the projects whose tests actually execute get hits. Running only
`QuickFiler.Test.dll` reported **20.21%** repo-wide and produced a
`BLOCKED_BY_REPOSITORY_WIDE_COVERAGE_DEBT` disposition and a proposed authority coverage
exception — all false. Running all 7 built `*.Test.dll` together (4,989 tests, all passing)
gave the true figure: **81.19% first-party production** coverage, above the 80% floor. The
blocker never existed.

**How to apply:**
1. Build the solution: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:"Platform=Any CPU"` (use `-` switches, not `/`, under Git Bash to avoid path mangling; also `export MSYS_NO_PATHCONV=1 MSYS2_ARG_CONV_EXCL='*'` before invoking vstest so `/EnableCodeCoverage` is not converted to a path).
2. Enumerate every `*.Test.dll` under `*/bin/Debug/` (regex `/[^/]+\.Test/bin/Debug/[^/]+\.Test\.dll$`) and pass them all to `vstest.console.exe ... /EnableCodeCoverage /InIsolation`.
3. Convert the `.coverage` output to cobertura: `dotnet-coverage merge <file>.coverage --output out.cobertura.xml --output-format cobertura`.
4. Compute the policy denominator = **first-party production only**: exclude `*.Test`/`*.Tests` packages AND bundled third-party libraries (FSharp.Core, Deedle, System.Linq.Async, System.Interactive, log4net, FluentAssertions, Mono.Reflection, Swordfish.NET.*). The raw cobertura `line-rate` (~69% here) still includes test + third-party code and is not the policy number.
5. First-party production assemblies in this repo: UtilitiesCS, QuickFiler, TaskMaster, ToDoModel, Tags, TaskVisualization, SVGControl, VBFunctions.

Do NOT surface a repo-wide coverage authority exception (see [[repowide-coverage-authority-exception]]) until the number is confirmed with a full-suite run. The exception path applies only to a genuine, verified shortfall.
