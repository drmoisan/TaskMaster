---
name: coverage-runner-throws-before-postprocessing
description: Invoke-MSTestWithCoverage.ps1 throws on ANY non-zero vstest exit before it post-processes the Cobertura XML, so a red suite (or a sub-80% floor trip) leaves a raw, absolute-path, third-party-inclusive document that is not comparable with a processed one
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` reaches its post-processing block (`ConvertTo-KoverageCoberturaXml`
at line 340, `Assert-CoberturaLineCoverageThreshold` at 341, `Set-Content` at 343) only on a fully green run.
Two earlier exits bypass it:

- `Invoke-DotnetCoverageCollection` (same file, ~line 235) throws `MSTest with coverage failed with exit code N`
  whenever the `dotnet-coverage`/vstest child exits non-zero — that is, on **any** failing test.
- `Assert-CoberturaLineCoverageThreshold` throws `Cobertura line coverage <p>% is below the required 80% threshold.`
  at line 341, which is *before* the `Set-Content` at 343.

**Why it matters:** the artifact the runner leaves on disk is then the raw dotnet-coverage document —
absolute host filenames (account name included), third-party `<package>` nodes still present, no
`<sources>` node — whereas a green run leaves the processed document with repo-relative filenames and
third-party packages removed. A baseline captured in one state and a post-change run captured in the
other are **not comparable**: the denominators differ by the whole third-party surface, so any
"post-change covered lines >= baseline covered lines" or narrow percentage-band gate fails spuriously.

The two failure modes are distinguishable from captured stdout by their literals, and only the second
one is a coverage-floor trip rather than a test failure.

**Line numbers as measured 2026-09-13 (they drift; re-derive):** throw at 236, `ConvertTo-KoverageCoberturaXml`
at 341, `Assert-CoberturaLineCoverageThreshold` at 344. The ordering — throw strictly before
post-processing — is the stable fact; the numbers are not.

**The THRESHOLD-ASSERTION detector reports a FALSE PASSED.** The standard CMD-COVERAGE wrapper infers
the branch by searching captured output for `is below the required 80% threshold`. When the line-236
throw fires, the assertion never runs, no such message is emitted, and the wrapper prints
`THRESHOLD-ASSERTION: PASSED` alongside `RUNNER-EXIT: 1`. The detector cannot distinguish "ran and
passed" from "never ran". A plan clause declaring `PASSED` + non-zero exit a failure is therefore
correct, but the executor must report the mechanism rather than a second defect.

**Recognise the raw document in one read.** Root `line-rate` is the unfiltered denominator — measured
0.2014 with 16143/80163 on item-871 — `<package name>` still includes log4net, Mono.Reflection,
SVGControl, Microsoft.IO.RecyclableMemoryStream, System.Linq.Async, System.Interactive, `<sources>` is
empty, and `<class filename>` carries ABSOLUTE host paths. An acceptance condition demanding the
repo-relative backslash form (`QuickFiler\Controllers\QfcQueue.cs`) is therefore *unsatisfiable* from
such a run, not merely off by a margin — say so rather than hand-post-processing, because the figures
would come from a run with failing tests and would understate the affected class.

**How to apply:** in any plan that captures a baseline and a post-change Cobertura from this runner,
(1) record a `POSTPROCESSED: yes|no` flag per artifact (yes iff EXIT_CODE 0), (2) require the two flags
to agree before any delta gate is evaluated, and (3) supply the remedy — dot-source
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and apply
`ConvertTo-KoverageCoberturaXml -XmlContent <raw> -RepoRoot <root>` to the unprocessed one. Also note
that a plan which tolerates a pre-existing baseline failure set (a `BASELINE_FAILURE_SET` / subset-relation
gate) must not simultaneously declare every non-zero stage-4 exit a failure requiring a restart: the
restart cannot clear a pre-existing failure. See [[exact-count-gate-vs-remediation-loop]] and
[[dotnet-coverage-denominator-nondeterminism]].
