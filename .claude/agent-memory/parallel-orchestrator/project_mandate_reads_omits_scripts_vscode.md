---
name: mandate-reads-omits-scripts-vscode
description: config/blast-radius.json mandate_reads excludes .claude/rules/** and artifacts/** but NOT scripts/vscode/**, so every C# plan's mandated coverage-command citation becomes a real conflict edge and serializes the parallel surface
metadata:
  type: project
---

`mandate_reads` in `config/blast-radius.json` is a DERIVATION-time citation filter, not a
conflict-time one: `Get-BlastRadius` drops matching citations before the feature folder is added
(`BlastRadius.psm1` line 179 comment; the reader is `config_mandate_reads` ported at
`BlastRadiusConfig.psm1:44`, key `mandate_reads`). A path it does not list stays in `paths` and
produces a real `path_overlap` edge.

The list covers `.claude/rules/**`, `.claude/skills/{atomic-plan-contract,evidence-and-timestamp-conventions,acceptance-criteria-tracking,policy-compliance-order}/SKILL.md`,
`.github/instructions/**`, `artifacts/**`, `quality-tiers.yml`, `.claude/agent-memory/**`, and
`.agents/skills/**`. It does **not** cover `scripts/vscode/**`.

**Why this matters:** every C# plan in this repository cites
`scripts/vscode/Invoke-MSTestWithCoverage.ps1` (and often `Invoke-MSTestWithCoverage.Helpers.ps1`,
`Invoke-Restore.ps1`, `Invoke-VSBuild.ps1`, `Install-RepoDotNetSdk.ps1`) as the MANDATED coverage
command it will RUN, not as a file it will WRITE. Verified 2026-09-01 on run `bugs-638-644-647`:
all five items — 638, 644, 647, 637, 646 — carry `Invoke-MSTestWithCoverage.ps1` in their declared
paths, and it is the sole recorded `path_overlap` detail for the 644~647 edge. That one citation is
sufficient to make any two C# items contend, which pushes a surface designed for concurrency toward
serial execution for a reason unrelated to real contention. It is the same defect class
`mandate_reads` exists to correct.

`QuickFiler.Test/QuickFiler.Test.csproj` is a second, weaker instance: it is genuinely written
(every new test file adds a `<Compile Include>` row), so its edges are real, but it makes every pair
of QuickFiler items contend by construction.

**How to apply:** Expect a `scripts/vscode/**` overlap on nearly every C# pair and do not read it as
evidence of real contention when reporting a verdict — say which path drove the edge. Do NOT narrow
a radius, drop the citation, or reinterpret the edge to merge cohorts: the
`parallel-orchestrate` skill forbids exactly that, and the relation is designed to fail closed. Do
NOT edit `config/blast-radius.json` here either — it is push-down-owned from drm-copilot and is
overwritten wholesale, so the fix is an upstream `mandate_reads` addition of `scripts/vscode/**`.
See [[claude-files-are-pushdown-owned-fix-upstream]] and
[[blast-radius-powershell-calling-convention]] for the read-the-`conflict`-key harness discipline
that produced this observation.
