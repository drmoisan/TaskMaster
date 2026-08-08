---
name: preflight-iterations-need-carried-context
description: Each preflight round is a FRESH atomic-executor (orchestrator has no SendMessage), so carry an established-facts list and the iteration history forward or rounds are wasted re-litigating settled items
metadata:
  type: feedback
---

The orchestrator's toolset has **no `SendMessage`** — only `Agent`. Every preflight round is therefore
a brand-new `atomic-executor` with zero memory of the previous rounds. Two blocks must be rebuilt into
each delegation prompt or iterations burn on already-settled ground:

1. **Established facts — treat as GIVEN, do not raise.** The repo-specific gotchas a fresh reader will
   otherwise flag as defects. For the quickfiler coverage epic: csharpier 1.2.6 needs the
   `format`/`check` subcommand (bare `csharpier .` from CLAUDE.md is stale); the Phase 0 NuGet restore
   is required because `packages/` is gitignored and msbuild does not restore `packages.config`
   projects; CRLF plans validate and must not be normalized; an absent upstream artifact behind an
   execution-time halt gate is expected, not a defect; never read emitted Cobertura
   `line-rate`/`branch-rate`; targets deliberately below 100% under a ratified exclusion table.
2. **Iteration history.** What each prior round found, that you verified it independently, exactly what
   you changed, and — critically — **which items were accepted as non-defects and must not be
   re-raised**. Without this, round N re-reports what round N-1 already dispositioned.

Also state the ALL CLEAR threshold explicitly ("reserve REVISIONS REQUIRED for defects that genuinely
prevent execution; report prose nuance as observations under an ALL CLEAR signal"). Without it a
thorough executor keeps returning REVISIONS REQUIRED for rationale wording no acceptance depends on.

**Why:** On #495 (epic F12) this converged in 4 rounds and each round found real blocking defects —
unsatisfiable acceptance literals that would have failed mid-execution (a `[DataTestMethod]`
declaration-vs-execution miscount, an omitted path-scoped policy file, a touched-file count
contradicting the task's own scope clause, an AC clause contradicting the plan's own Phase 1). Rounds
2-4 each also confirmed the prior delta landed, which is worth the round. The waste would have come
from re-raising CRLF and the absent upstream ledger, which the facts block prevented.

**How to apply:** Verify every blocking finding against the tree yourself before applying it — the
executor was right all four times here, but a "false rationale" claim (e.g. "the csproj block is
alphabetical") is cheap to confirm and occasionally reveals the fix is bigger than proposed. Apply
revisions **in place** in the single canonical plan path, never a timestamped sibling. Edit a CRLF plan
with a byte-exact Python replace script that asserts pure CRLF before and after and requires exactly
one match per edit; re-run the MCP plan validator after every delta. See
[[mcp-plan-validator-editwrite-pervasive-diff]] and [[mcp-plan-validator-defective-em-dash]].
