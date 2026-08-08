---
name: feedback-wiring-gates-must-be-wiring-sensitive
description: Executed-test-count floors cannot detect unwired test files (both sides deflate equally); gate wiring via static [TestMethod] enumeration vs vstest /ListTests discovery instead
metadata:
  type: feedback
---

Never gate csproj test-file wiring on an executed-test-count floor. Prove wiring with two wiring-sensitive checks: (1) enumerate feature-added `.cs` files from `git diff --name-only` and match each against `<Compile Include>` entries; (2) statically enumerate the new `[TestMethod]` names from source and confirm each appears in `vstest.console.exe <assembly> /ListTests` output. Source enumeration does not shrink when a file is unwired; `/ListTests` reflects only what compiled — a name in source but absent from discovery is the silent-failure condition.

**Why:** #230 preflight revision 3 (2026-08-07). The count floor failed three ways: (a) an unwired `*.Part2.cs` deflates the same-phase filtered run that feeds the floor, so floor and full-run figures drop identically and the comparison passes — the gate is degraded by exactly the condition it exists to detect; (b) incommensurable units — `Invoke-MSTestWithCoverage.ps1` counts repo-wide (thousands) vs per-assembly filtered runs (tens), making "full >= floor" vacuous; (c) overlapping `/TestCaseFilter` values across phases (three tasks shared `FullyQualifiedName~InitializationTests`) triple-count shared classes, so the sum is neither a measurement nor commensurable.

**How to apply:** In legacy packages.config projects (see [[project-legacy-csproj-explicit-compile-include]]), put the two-check wiring task before final QC; keep executed/passed counts in run artifacts only as `Output Summary:` audit-trail data, never as a wiring gate. Generalization: before pinning any numeric floor as a gate, ask whether the failure mode it guards against also deflates the floor's own measurement source.
