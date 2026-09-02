---
name: coverage-mode-raw-vs-processed-is-flake-sensitive
description: One flaky test terminates Invoke-MSTestWithCoverage before its post-processing step, leaving a RAW Cobertura denominator that is not comparable to a processed one; re-measure cheaply in a detached worktree with packages and SDK copied.
metadata:
  type: project
---

`scripts/vscode/Invoke-MSTestWithCoverage.ps1` asserts its threshold on post-processed content
at `:341` and writes that content back only at `:343`. Any earlier termination leaves the raw
`dotnet-coverage` root attributes on disk. The two states have different denominators: raw
carried 14 `<package>` elements including every `.Test` assembly with `lines-valid=82363`,
while processed carried 9 with `lines-valid=64221`, because the helper strips every
non-allowlisted package and recomputes the root attributes over what remains.

The practical consequence is that a baseline and a post-change figure can be measured in
different modes, and differencing them then measures the denominator rather than the change.
On issue 638 that produced a nonsense `+14.63` delta (70.70 raw to 85.33 processed) and left
AC17 unmet even though the change-scoped figure passed at 93.10 percent.

**Why:** the cause was not the change under test. A single pre-existing wall-clock test,
`QfcDatamodelLivenessTests.RemainingLoadActive_WhenLoaderThrows_IsStillClearedByFinally`,
failed once and terminated the harness at `:236`. It passed on every later run.

**How to apply:**

- Record `COVERAGE_XML_MODE` on every coverage artifact and refuse to difference unequal modes.
- Cross-check the package count. Equal package counts on both sides is the cheap proof that the
  denominators are constructed the same way.
- To re-measure a merge-base baseline, do NOT rebuild a worktree from scratch. Create a detached
  worktree at the base ref and `robocopy` the gitignored `packages/` (about 1.4 GB) and
  `.dotnet-sdk/` (about 750 MB) from the working worktree. That skips the NuGet restore and the
  SDK download entirely, keeps the analyzer set provably identical between the two measurements,
  and the whole re-measurement then costs one rebuild plus one suite run.
- Sanity-check the arithmetic afterwards: on 638 the processed baseline was 85.26 with
  `lines-valid=64195`, and `64221 - 64195 = 26` matched the added executable-line count derived
  independently from the diff.

Note this cuts against a plan clause that requires baseline and post-change modes to be equal:
such a clause is hostage to suite flakiness rather than to the change under test, so expect to
have to remediate it rather than treating it as a code defect.

**Recurrence, 2026-09-01 on issue #287 (found at plan-authoring time, before any run).** Executor
preflight caught the same hazard statically. Current line numbers: `Invoke-DotnetCoverageCollection`
is called at `:326` and the post-processing, `Assert-CoberturaLineCoverageThreshold`, and the
`Set-Content` that persists the processed XML are all downstream at `:333-344`. Re-derive these;
they have moved at least once.

Two ways a plan can guard it, and they are not equivalent:

- **Require a green baseline suite** so both runs are necessarily processed mode. This is what the
  #287 plan adopted. It is correct but it makes the whole plan hostage to any pre-existing flake,
  so it must carry an explicit halt clause ("record as a pre-existing blocker and stop for
  orchestrator direction") rather than an instruction to proceed.
- **Record `COVERAGE_XML_MODE` on every coverage artifact and refuse to difference unequal modes.**
  Cheaper and not flake-hostage, because it degrades to "cannot compare" instead of "cannot
  proceed". Prefer this when the suite has known flakes. Cross-check the `<package>` count as the
  proof the denominators match.

When scheduling a C# item whose plan takes the green-baseline route, warn the parent that the
run can halt on an unrelated flaky test.

**Two more facts from #287, both measured rather than inferred.**

*Cobertura emits every line element TWICE per class.* Each `line` appears once under
`methods/method/lines` and again under the class-level `lines`. Parsing the committed artifact
`docs/features/.../439/evidence/qa-gates/issue-439-final.normalized.cobertura.xml` for the
`StoreLaunchReadinessEvaluator` class element: `.//line` returns **25**, `lines/line` returns
**13**, `methods/method/lines/line` returns **12**, and 13 + 12 = 25. So a PowerShell
`GetElementsByTagName("line")` on a class element — a *descendant* traversal — double-counts, and
any acceptance condition asserting an uncovered-line count from it is inflated. Use
`SelectNodes("lines/line")` to scope to the class-level child. Note the ratio is **not exactly
two**: the class-level block carries one line the method blocks do not, so do not write "twice the
count" in a plan.

*Distinguish a post-processed artifact from a raw one by stdout, not by parsing.* The four root
attributes parse in both states, so reading them proves nothing. `Invoke-MSTestWithCoverage.ps1`
prints `Post-processing coverage XML for Koverage compatibility...` and `Done. Coverage artifact:`
only after the collection exited zero, both downstream of the throw. Requiring both literals in the
log is the cheap, falsifiable discriminator. Re-derive the line numbers; they have moved before.

Related: [[csharp-coverage-denominator-two-figures]],
[[coverage-lines-covered-is-nondeterministic]],
[[feature-review-coverage-85-floor-trap]],
[[project_flaky_ci_physicalfileinfoadapter_test]]
