---
name: vstest-argument-order-transitive-dep
description: A legacy packages.config test project missing an explicit reference to a transitive dep (e.g. ExCSS via Svg) passes or fails depending on its ordinal position on the vstest.console.exe command line; always run a changed test assembly ALONE
metadata:
  type: project
---

Legacy non-SDK `packages.config` projects **do not flow transitive copy-local**. A test project that references `Svg` but not `ExCSS` gets `Svg.dll` in `bin/Debug` and **no** `ExCSS.dll`, even though the production project it tests references ExCSS explicitly. Any test needing a real parse then throws `FileNotFoundException` for the transitive assembly.

**The failure is argument-order-dependent, not merely co-execution-dependent.** Measured on #418 (`SVGControl.Test`, 2026-08-04), same binaries, same session:

| Command | Result |
|---|---|
| `vstest.console.exe SVGControl.Test.dll` | 75 total, **6 failed** |
| `vstest.console.exe SVGControl.Test.dll VBFunctions.Test.dll` | 76 total, **6 failed** |
| `vstest.console.exe VBFunctions.Test.dll SVGControl.Test.dll` | 76 total, **76 passed** |

The test host's probing path follows the **first** assembly on the command line. All eight sibling `*.Test` projects reference ExCSS explicitly and carry it in their output, so putting any of them first rescues the bind.

**Why a binding redirect does not save it:** redirection presupposes the file is findable. An `app.config` `bindingRedirect` to a correct `newVersion` is inert when the DLL is absent from the probing path, and an `AssemblyResolve` fallback that probes the directory containing the production DLL fails too, because that is the *same* output directory.

**Why:** it violates three quoted policy statements — UT1 Independence ("Tests must be able to run in any order without impacting each other") and the mutable-global-state prohibition in `.claude/rules/general-unit-test.md`, plus "Tests must produce identical results in the IDE test runner and in CLI runs" in `.claude/rules/csharp.md`. Test Explorer runs one assembly, so a developer sees red tests that CI never shows.

**How to apply:**
- When a branch adds tests to a `*.Test` project or adds a `<Reference>` to one, **run that assembly alone** (`vstest.console.exe <One>.Test.dll`) as well as via the mandated 9-assembly `Invoke-MSTestWithCoverage.ps1 -SearchRoot .` wrapper. The wrapper always runs all nine and hides this class of defect completely — a green 6150/6150 proves nothing about isolation.
- An executor's "not a regression, adding one sibling yields N/N passing" disclosure is a signal to reproduce, not to accept. Reverse the argument order; if that flips the outcome, the defect is real and order is the operative variable. Also re-count: a disclosure written mid-plan can predate later tasks that add tests (#418 recorded 5/65, the true figure at head was 6/75).
- Fix is one `<Reference>` with `HintPath` + `<Private>True</Private>` plus one `packages.config` line, copying the `Include` identity string verbatim from the production `.csproj`. Cheap enough that it is worth calling Blocking.
- Check `ls <Proj>/bin/Debug | grep -i <dep>` against a sibling test project's output to confirm the asymmetry before writing the finding.
