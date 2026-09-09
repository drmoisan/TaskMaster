---
name: noexecute-early-return-premise-hides-reachable-entry-point-tests
description: A plan claiming an entry-point block is unreachable because tests only use a -NoExecute early return is often false; verify by searching for calls that omit the switch before accepting an uncovered-by-construction argument.
metadata:
  type: project
---

A plan premise of the form "the existing tests exercise this entry point only through its `-NoExecute`
early return, so no unit test can reach the block below it" must be verified before it is relied on.
In `scripts/vscode/Invoke-MSTestWithCoverage.ps1` it was **false**: six pre-existing tests call
`Invoke-MSTestWithCoverageMain` **without** `-NoExecute` and do reach the post-processing block, with
`ConvertTo-KoverageCoberturaXml` mocked.

**Why:** issue #815's plan decision D3 built two things on that premise — an argument that the one
added wiring line is "uncovered by construction", and an AC10 allowance letting the entry point's
missed LINE count rise by 1. Both were wrong in the safe direction (the line turned out covered;
missed stayed 10), but the premise also hid a real regression: the mocked document those six tests
supply is the minimal stub `<coverage line-rate="0.8" />`, which carries no `<packages>` node.
`Assert-CoberturaLineCoverageThreshold` tolerates it because it reads only the root attribute; any
new call that parses deeper throws, and the whole PoshQC test stage went red at P5-T6.

**How to apply:** before accepting an unreachability or uncovered-by-construction claim about an
entry-point block, run `git grep -n` for calls to the function and check which ones omit the guarding
switch. When adding a call into such a block, check what the existing mocks return: a stub that
satisfies the *previous* consumer can be structurally invalid for a new one. The right remedy is to
make the stub structurally realistic (here, adding an empty `<packages />`), not to relax the new
function's validation — that keeps every assertion exact and keeps fail-fast behaviour intact. Note
the collateral: the two test files needing the fixture fix were outside the spec's Write Set, and one
of them sits at 496 of 500 lines, so the edit had to be a same-line replacement with zero net lines.
Related: [[project-plan-checkoff-fixpoint-breaks-terminal-clean-tree-gate]].
