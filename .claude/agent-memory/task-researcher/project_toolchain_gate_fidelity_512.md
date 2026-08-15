---
name: toolchain-gate-fidelity-512
description: "#512/#492/#509/#522 research: AGENTS.md/.agents are externally-owned mirrors (generator absent), Invoke-VSBuild.ps1 is an unenumerated executable carrier, and ~1.2s vs ~17s elapsed time is a reusable vacuity tell"
metadata:
  type: project
---

Research for feature `csharp-toolchain-gate-fidelity-512` (epic `build-ci-coverage-gate-fidelity`,
Wave 0), completed 2026-08-10. Three findings that are not obvious from reading any single file.

**1. The `AGENTS.md` / `.agents/` / `.github/instructions/` tree is externally owned — do not treat
it as a first-party edit surface.** `AGENTS.md` line 3 declares itself generated from sixteen
`.github/instructions/*.instructions.md` files, and names the generator
`scripts/dev-tools/sync-agents-from-instructions.ps1` — **which does not exist in this repository**
(`scripts/dev-tools/` holds only `run-actionlint.ps1`), and there is no CI drift check. `.agents/skills/*/SKILL.md`
self-describe as "Legacy C# variant resource for Codex push-down" and are installed by the external
`drm-copilot` MCP tool `push_down_codex_and_agents_customizations`, so local edits are reverted on
the next push-down. Separately, `policy-compliance-order` names `.github/instructions/` in the SAME
hard-constraint clause as `.claude/rules/`, and epic authorizations that suspend the constraint have
so far named only `.claude/rules/` — so `.github/instructions/` stays blocked even when `.claude/rules/`
is opened.

**Why:** the correct disposition for a governance defect that also appears in these mirrors is a
follow-up issue naming `drm-copilot` as owner, not a wider diff. This is the same reasoning the epic
used to exclude issue #513.

**How to apply:** before proposing an edit to `AGENTS.md`, `.agents/`, `.codex/` or
`.github/instructions/`, check ownership first. Recommend exclusion plus an MCP-promoted follow-up
issue. See [[push-down-claude-dir-149]] for the push-down direction (bundled resources flow inbound
from the extension).

**2. Governance defects often have an unenumerated EXECUTABLE carrier — grep `scripts/` and
`.vscode/tasks.json`, not just the docs.** For #512/#522 the issues enumerated only prose sites, but
`scripts/vscode/Invoke-VSBuild.ps1` hardcodes `/t:Build` (in `Get-MSBuildBuildArguments`) and maps
`-EnableNullable` to `/p:Nullable=enable`; `.vscode/tasks.json` wires both into the `type-check`
task; and `tests/scripts/vscode/Invoke-VSBuild.Tests.ps1` **pins the defect as asserted
expectations**. That test file is the ready-made home for a red-to-green regression test, and
`Invoke-VSBuild.ps1` already has a pure/impure split plus a `-NoExecute` seam, so no new wrapper
seams are needed.

**Why:** `policy-compliance-order` tells agents to "prefer repo-defined tasks/commands", so fixing
only the prose leaves the *preferred* execution path still unable to fail.

**3. Reusable vacuity tell for the MSBuild nullable gate: elapsed time.** Across ~12 committed
evidence artifacts the documented `/t:Build` nullable step records **0.89-1.51 s** while the analyzer
step over the same 18-project solution records **6.29-17.02 s**. An 18-project solution cannot
compile in 1.2 s. A second independent signal: the `CS2002` warning is emitted by `CoreCompile`, so
its presence in one log and absence in another proves which run actually compiled.

**How to apply:** when auditing any committed MSBuild gate evidence, read `Time Elapsed` before
believing `EXIT_CODE: 0`. Note that two repo artifacts directly contradict each other on this
(2026-08-06T22-23 claims 18 CoreCompile executions / 0 skips; 2026-08-08T16-19 refutes it), and the
memory entry `project_nullable_pragma_gate_mechanics.md` propagates the stale claim — re-measure
rather than trusting either.

Research artifact:
`docs/features/active/2026-08-10-csharp-toolchain-gate-fidelity-512/research/toolchain-gate-fidelity.2026-08-10T14-40.md`
