---
name: csharp-repowide-coverage-below-80
description: A real multi-assembly Cobertura run yields ~59% repo-wide C# coverage, below the 80% gate; distinct from the misleading 8% single-assembly aggregate
metadata:
  type: project
---

When a genuine repository-wide C# coverage run is produced (all 7 first-party `*.Test.dll` in one `vstest /EnableCodeCoverage` pass, ~4068 tests, merged to Cobertura via `dotnet-coverage merge -f cobertura`), the canonical `artifacts/csharp/coverage.xml` root `line-rate` is ~0.589 (58.9%) repo-wide. First-party-only is ~77.6% (incl. test assemblies) and ~60.5% (first-party production only). All are below the mandatory >= 80% gate in `.claude/rules/csharp.md`.

**Why:** This is genuinely below threshold, not an instrumentation artifact. It is depressed by under-covered first-party production assemblies (TaskVisualization ~0.4%, ToDoModel ~11%, QuickFiler/TaskMaster ~25%, Tags ~31%) plus bundled third-party DLLs (Deedle, log4net, FluentAssertions, Swordfish, etc.) and vendored projects (SVGControl). It is distinct from the ~8.4% single-assembly aggregate that earlier cycles recorded from a TaskMaster.Test-only run — that 8% figure is a misleading aggregate dominated by unexercised third-party DLLs; the 59% is the real repo-wide number.

**How to apply:** When the executor produces a real repo-wide Cobertura artifact for a C#-touching feature, the repo-wide >= 80% gate is a FAIL/blocking finding on its literal value (~59%), even though a trivial XML/test-only change cannot have caused it. Parse the root line-rate yourself (the hook parses Cobertura as JaCoCo and gets $null — see [[csharp-coverage-artifact-is-cobertura]]). Record it as FAIL with the pre-existing-condition context, and offer two remediation paths in remediation-inputs: raise coverage, or an authority-recorded exception scoping the gate to changed/new code. Do not silently reinterpret the threshold to changed-code-only. Relates to [[csharp-local-fullsuite-coverage-blocked]] (that note's "no repo-wide root" applies to the per-feature trimmed Cobertura; the /EnableCodeCoverage path here avoids the Moq binding-redirect and does produce a repo-wide root).
