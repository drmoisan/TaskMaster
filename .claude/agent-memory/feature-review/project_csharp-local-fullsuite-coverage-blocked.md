---
name: csharp-local-fullsuite-coverage-blocked
description: Local full-assembly UtilitiesCS.Test coverage runs fail on a pre-existing Moq binding redirect; per-feature Cobertura evidence is often trimmed to affected classes with no repo-wide root line-rate
metadata:
  type: project
---

Local full-assembly `UtilitiesCS.Test` MSTest-with-coverage runs are environmentally unreliable in this repo due to a pre-existing `System.Threading.Tasks.Extensions, Version=4.2.0.1` Moq binding-redirect `TypeInitializationException` in the local vstest host (`Moq.Async.AwaitableFactory`). It appears identically on baseline and post-change and is independent of any source change.

**Why this matters:** Executors work around it by running a filtered `/TestCaseFilter` coverage pass over only the affected classes, then committing a Cobertura XML that is intentionally trimmed (look for the literal comment `<!-- trimmed to affected classes for issue #NNN -->`). The trimmed file's root `<coverage>` element therefore does NOT carry a meaningful repo-wide aggregate line-rate; the first node is just the first affected class. Do not misread the root/first line-rate as repo-wide.

**How to apply:** For C# reviews here, verify per-file coverage from the affected-class Cobertura class nodes (compare baseline vs postchange `line-rate` on the specific `filename`), and treat the authoritative repo-wide full-suite C# coverage gate as the GitHub Actions CI run on the PR, not a local run. If the only open item is "PR CI green," that is an external verification gate (PARTIAL pending CI), not a code remediation. Confirmed on Issue #176 (PhysicalFileInfoAdapter seam: baseline 0.8909 -> postchange 0.9155, no regression). See [[csharp-coverage-artifact-is-cobertura]].
