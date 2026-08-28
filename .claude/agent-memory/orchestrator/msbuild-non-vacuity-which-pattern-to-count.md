---
name: msbuild-non-vacuity-which-pattern-to-count
description: Resolves a contradiction between two prior memories - `Task "Csc"` counts 0 even on a real compile, but `csc.exe` counts 53 and `CoreCompile:` headers count 15 on the same log; count the latter two
metadata:
  type: project
---

Two earlier memories disagreed about how to prove an msbuild analyzer/nullable gate actually
compiled. [[msbuild-analyzer-gate-vacuous-without-rebuild]] says count `csc.exe` matches;
the MEMORY.md index line for it says the opposite — "assert a ZERO `Skipping target
\"CoreCompile\"` count, NOT a csc.exe count (csc is 0 even on real compiles)".

**Both were describing different regexes.** Measured on one `/t:Rebuild` log
(`/v:normal`, `/flp:Verbosity=normal`), 18-project TaskMaster.sln, exit 0:

| Pattern | Count | Usable as non-vacuity proof? |
| --- | --- | --- |
| `Skipping target "CoreCompile"` | **0** | Yes — the negative signal. Must be 0. |
| `^\s*CoreCompile:` (target header) | **15** | Yes — the positive signal. Must be > 0. |
| `csc\.exe` or `/analyzer:` | **53** | Yes — also positive. |
| `Task "Csc"` | **0** | **No.** Reads 0 on a genuinely non-vacuous compile. |

So the index line's warning was true only of the `Task "Csc"` spelling, not of `csc.exe`.
A gate asserting `Task "Csc" > 0` fails on correct work; a gate asserting `csc.exe > 0`
or `CoreCompile: > 0` passes.

**Why it matters:** an analyzer gate whose only evidence is `EXIT_CODE: 0` cannot fail, because
MSBuild's timestamp-based up-to-date check does not invalidate on a `/p:` change. But picking the
wrong non-vacuity regex converts the fix into a different false result — a gate that always fails.

**How to apply.** Assert BOTH directions, and never use the `Task "Csc"` spelling:

1. `Skipping target "CoreCompile"` count **== 0**.
2. `^\s*CoreCompile:` count **> 0** (record the number).
3. Optionally confirm the analyzers really loaded by grepping an actual
   `/analyzer:...Roslynator.CSharp.Analyzers.dll` argument out of the log — proof the analyzer
   set was passed to the compiler, not merely that a compile happened.

Also note the two gates legitimately produce different `CoreCompile:` counts on the same tree
(15 for the analyzer gate, 11 for the nullable gate) because they run with different properties
and different project subsets rebuild. A mismatch between the two is not a defect.

Related: [[bash-tool-mangles-msbuild-switches]] — run these through `pwsh -NoProfile` with
absolute paths, or `/m` becomes `M:/`.
