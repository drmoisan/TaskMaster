---
name: project-927-r10-ci-session-state-and-folder-level-mcp-seams
description: "#927 round 10 (preflight round 2 over v1.9): a locally-run Pester payload that mirrors a CI callee must also carry the callee's session statements (Set-StrictMode -Version Latest; $ErrorActionPreference = 'Stop'), or a strict-mode-rejected property read passes locally and fails only in the CI context a later task reads; a Phase 0 baseline captured without those statements is not the comparison basis; a folder-scoped read-only MCP analyzer result cannot be attributed to one file (record it, never stop on it); 'every commit sits at a phase boundary' is falsified by loop commits inside a QA phase; run-actionlint.ps1 is 14 lines"
metadata:
  type: project
---

Five narrow defects the executor found on the round-9 revision of plan 927, and the shape of the fixes.

**CI session state in mirrored payloads (R2-01).** The Pester callee (.github/workflows/_pester.yml lines 35 and 36) runs `Set-StrictMode -Version Latest` and `$ErrorActionPreference = 'Stop'` before importing Pester. A plan payload that reproduces the callee's configuration but omits those two statements can pass locally on a property read that strict mode rejects (for example dotted access to a JaCoCo `report` element, which resolves to a two-element array under a DOCTYPE) and fail only in the CI context the final task reads. Fix: prefix the payload with the two statements (double-quoted `"Stop"` inside a single-quoted `-Command` string, per the plan's C2 quoting rule), state in the baseline task that the completed Phase 0 run predates them so the post-merge re-capture is the comparison basis, and give the interim coverage task a branch for a test that fails only under strict mode (fix inside the current batch budget, re-run the green run and the coverage run, record every iteration).

**Folder-level MCP analyzer over a single modified file (R2-02).** When the direct analyzer is pointed at a single modified test file but the read-only PoshQC MCP analyzer accepts folders only, the AC check-off had no MCP observation over that file. Fix: a second MCP call scoped to the file's folder, recorded as `POSHQC MCP HELPER-FOLDER ok=<true|false>` or `POSHQC MCP UNAVAILABLE`; a folder-level `ok=false` cannot be attributed to one file, so it is recorded for the check-off (NOT MET naming the folder-level result), never a loop failure or a stop. Name the folder in plain prose when the plan does not write the folder as a whole (harvester rule).

**Commit-placement claims (R2-04).** "Every commit task sits at a phase boundary" is false for a final QA phase that carries loop commits and mid-phase commits (P7-T17, P7-T18, P7-T38) plus a post-PR commit (P7-T39). State the placement explicitly per phase instead of asserting a uniform property; verify by grepping task lines for "Commit" and comparing against the last task of each phase.

**Stop-list agreement (R2-05).** When C12 (the stop list) gains a stop for a task, every convention paragraph that enumerates the same class of stop (here C17's helper-test stops and the 500-line split stop) must be swept to name the same task set; the two paragraphs drift independently.

**Line counts.** scripts/dev-tools/run-actionlint.ps1 is 14 lines (ripgrep `^` count); the Read tool shows 15 because it numbers the terminal newline. Same class as the round-9 finding.

**How to apply:** when a plan mirrors a CI callee's script in a local payload, copy the callee's session statements too and cite their lines; when a policy names an MCP tool that accepts folders only, add a folder-scoped call and word its result as folder-level; never assert a uniform structural property ("every commit is at a boundary") without enumerating the tasks; sweep every sibling enumeration when a stop list changes.
