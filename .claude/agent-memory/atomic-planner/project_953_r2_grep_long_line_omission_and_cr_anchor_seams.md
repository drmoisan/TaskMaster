---
name: project-953-r2-grep-long-line-omission-and-cr-anchor-seams
description: "#953 R2 preflight deltas - the Grep tool replaces any output line over about 500 characters with [Omitted long matching line], so a JUnit read via Grep -A 2 cannot transcribe Pester failure messages (use -n for the line number, then Read with offset/limit; Read returns a 3,000-character line in full); ripgrep's $ does not match before a carriage return so end-of-line gates over executor-written files need \\r?$; 'before Phase 1' is wrong when the test is authored inside Phase 1 (anchor to task ids); an expect-fail task needs a wrong-reason branch keyed to the message shape"
metadata:
  type: project
---

Round 2 preflight deltas on the issue #953 plan (worktree `.claude/worktrees/agent-a5292122c820774d3`, 2026-10-02). Four defects plus one advisory, no task-count change (47).

**Why:** each one is a tool-behaviour or wording fact that reading the plan could not reveal; the reviewer found them by running the Grep tool over the plan itself and by tracing where the tests are authored.

**How to apply:** before any plan that transcribes figures from a long-line XML document (JUnit, TRX, Cobertura) with the Grep tool, and before any `$`-anchored gate over a file the executor writes.

1. **The Grep tool omits long lines.** Any output line longer than about 500 characters is replaced with `[Omitted long matching line]` (match) or `[Omitted long context line]` (`-A`/`-B` context). The line number from `-n` survives. A Pester JUnit `testsuite` line carries the absolute test-file path twice and a `failure` line carrying a `-Because` list runs to 1,700 characters, so a Grep-only CMD-JUNIT-READ cannot transcribe them. Pattern: Grep with `-n` for the line number, then the Read tool with `offset` equal to that line number and `limit` 1 (or 3 to span testcase plus failure child); the Read tool returned a 3,000-character plan line in full. Observe it with Grep pattern `^.{500,}` over any file.
2. **ripgrep `$` does not match before a carriage return.** A gate such as `... -Force$` fails on a CRLF file even when the content is right. On a file the executor writes (new .psm1/.Tests.ps1) whose terminator the plan does not pin, write `\r?$`. Check tracked targets instead of changing them blindly: issue.md was LF (Grep `\r$` count 0) so its `^## Acceptance Criteria$` gate was left alone and the check recorded.
3. **Name the task position, not the phase, for fail-before/pass-after prose.** "red before Phase 1" was false because the regression tests are authored at P1-T2 inside Phase 1; the accurate form is "red at P1-T3, before the config edits P1-T4 to P1-T14, and green at P1-T15". Grep the whole plan for the phrase; it sat in section 7 and twice in section 9.
4. **An expect-fail task needs a wrong-reason branch.** A test that fails with an exception or StrictMode error instead of the expected assertion still yields `failures=1`. Gate the message shape (Pester `Should` failures begin `Expected ` and contain `, but got `) and route anything else to "defect in a new file, fix and re-run as `.iter<N>`".
5. **A parenthetical list in a CITATION is itself a count claim.** The VBFunctions.Test citation listed the other 1.16.0.0 ClientModel configs and omitted UtilitiesCS 107-108 while claiming "10 blocks"; re-enumerate every member when the line is touched.
6. **Plan self-references to line numbers go stale.** When citing the long-line observation, name the CMD definitions and section-9 tests the lines hold, plus the Grep that reproduces it, rather than only the line numbers.

Related: [[project-953-r1-pwsh-refused-and-count-recheck-seams]], [[project-953-fizzler-redirect-sweep-and-ratchet-plan-seams]], [[powershell-gate-observables]], [[verify-line-spans-and-computed-literals]].
