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

**How to apply:** in any plan that captures a baseline and a post-change Cobertura from this runner,
(1) record a `POSTPROCESSED: yes|no` flag per artifact (yes iff EXIT_CODE 0), (2) require the two flags
to agree before any delta gate is evaluated, and (3) supply the remedy — dot-source
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` and apply
`ConvertTo-KoverageCoberturaXml -XmlContent <raw> -RepoRoot <root>` to the unprocessed one. Also note
that a plan which tolerates a pre-existing baseline failure set (a `BASELINE_FAILURE_SET` / subset-relation
gate) must not simultaneously declare every non-zero stage-4 exit a failure requiring a restart: the
restart cannot clear a pre-existing failure. See [[exact-count-gate-vs-remediation-loop]] and
[[dotnet-coverage-denominator-nondeterminism]].
