---
name: run-the-failing-class-alone-before-blaming-parallelism
description: A test that passes in a full serial run and fails under MSTest ClassLevel parallelism is NOT necessarily a concurrency bug — run the class ALONE serially first; in TaskMaster issue 877 that one run refuted two successive diagnoses
metadata:
  type: feedback
---

When a test fails only when `/Settings:scripts/vscode/TaskMaster.cli.runsettings` (MSTest
`Workers=0`, `Scope=ClassLevel`) is passed, and passes in the full serial run, the tempting
inference is a concurrency defect. Run the failing class **alone, serially, with no runsettings**
before accepting that. It is one 15-second vstest invocation and it is decisive.

**Why:** on issue 877 (`QuickFiler.Test` / `QfcInitEmailQueueZeroBatchTests`) that single run
failed 3/3. Parallelism was therefore not the variable at all — what varied was *which classes had
already run in the process*. Two successive diagnoses had survived because nobody ran it:

1. "UtilitiesCS.Test's `[AssemblyInitialize]` installs the `AssemblyResolve` fallback that
   QuickFiler.Test silently depends on." Refuted by a prior item's evidence: QuickFiler.Test run
   alone with no runsettings passed 1394/1394, so that sibling initializer never ran and the tests
   still passed.
2. "Deedle's own type initialiser is unsafe against concurrent first touch under ClassLevel
   parallelism." Refuted by running the class alone serially — it fails with no concurrency present.

**The actual mechanism.** `Deedle` references `FSharp.Core 4.5.0.0`; both test app.configs redirect
FSharp.Core to `11.0.0.0`; FSharp.Core 11.0.0.0 references `netstandard, Version=2.1.0.0`. No
netstandard 2.1 exists on the machine (only 2.0.0.0 in the GAC), no app.config redirects it, and
`netstandard.dll` is in neither bin\Debug. The bind can only be satisfied by a process-global
`AppDomain.CurrentDomain.AssemblyResolve` fallback that matches on simple name plus public key
token. The repository has exactly two such handlers, and the one that runs in a QuickFiler.Test-only
process is `SVGControl/SvgAssemblyResolver.cs`, installed lazily from `SvgRenderer`'s **static
constructor** and reached transitively whenever a test constructs `QuickFiler.ItemViewer` (which
hosts SVGControl controls). Serial discovery order happens to put such a class first;
class-level parallelism does not. The CLR then caches the failed `Deedle.Reflection` initialiser for
the process lifetime, which is what makes the failure look deterministic.

**How to apply:** before proposing any fix for an ordering-sensitive test failure, measure the
three-point matrix — class alone / class preceded by one suspect class / full assembly — with the
parallelism switch held constant. Also note the corollary: a fix that "forces the type initialiser
early in `[AssemblyInitialize]`" is *worse* than the bug when the initialiser fails in isolation,
because `[AssemblyInitialize]` runs before every class and so before any accidental rescuer.

Related: [[feedback_verify_repro_before_bugfix_cycle]], [[my-own-negative-claims-need-a-scoped-search]],
[[project_local_vstest_exclude_claude_worktrees]].
