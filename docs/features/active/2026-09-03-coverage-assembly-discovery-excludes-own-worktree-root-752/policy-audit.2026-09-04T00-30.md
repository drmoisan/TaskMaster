# Policy Audit — Reaudit After Remediation Cycle 1 (Issue #752)

- Timestamp: 2026-09-04T00-30
- Reviewer: orchestrator (direct verification; the feature-review subagent was unavailable at this
  point in the run due to a subagent-nesting-depth limit, so this reaudit was performed directly by
  the orchestrator with the same evidence-first method the prior feature-review audit used)
- Branch: `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`
- HEAD at time of audit: `36cf73c2` (merge base `87233f86`)
- Scope: full branch diff `87233f86..HEAD`, spanning commits `5375bcc9`, `df81b27e`, `eea3bb9b`,
  `80d07a1c`, `5cd88860`, `8f9f823a`, `36cf73c2`
- Prior audit: `policy-audit.2026-09-03T12-23.md` (verdict PARTIAL, 1 blocking finding POL-2, 3
  non-blocking findings POL-3/POL-4/POL-5/POL-6)

## Purpose of this reaudit

Confirm that remediation cycle 1 (plan `remediation-plan.2026-09-03T12-23.md`, executed at commits
`5cd88860` and `8f9f823a`) genuinely closed POL-2, and that no non-blocking finding from the prior
audit became blocking as a side effect.

## 1. POL-2 closure — verified

Independently re-ran the branch-scoped added-line sweep the prior audit used, over the full current
diff `87233f86..HEAD` (case-insensitive, restricted to `+`-prefixed added lines, for the account
name, `C:\Users`, `/Users/`, and the worktree-parent directory name):

```
git -C <repo-root> diff 87233f86..HEAD | grep -n -i -E "^\+.*(<account>|C:\\Users|/Users/|<worktree-parent>)"
```

Four matches, all in `remediation-plan.2026-09-03T12-23.md` and `remediation-inputs.2026-09-03T12-23.md`,
each either quoting the rule's own `<account>`-placeholder illustrative example or describing the
token classes generically (e.g. "the account name, `C:\Users`, `/Users/`, and the worktree-parent
directory name") without spelling a real value. **Zero matches carry a real host identifier.**

Separately, this reaudit found and closed a defect the round-1 disclosure sweep had flagged but not
localized precisely: `policy-audit.2026-09-03T12-23.md` line 185 (an example verification command)
literally spelled the real account name and the real worktree-parent directory name as alternation
members of a documented `grep` pattern. That is a genuine leak of the same class POL-2 addressed,
independent of the R-1 finding itself (it was introduced by the audit process, not by the fix).
Sanitized in commit `36cf73c2` by describing the token classes generically rather than spelling them,
consistent with the branch's established convention (`.claude/agent-memory/_shared_no_absolute_host_paths.md`
lines 88-92, no-quoting rule). `remediation-inputs.2026-09-03T12-23.md`'s three flagged lines were
independently re-checked and found to carry no real value — the round-1 disclosure sweep's coarse
`C:\Users`/`/Users/` substring pattern over-counted generic pattern-class descriptions as hits; they
were committed unchanged.

**POL-2: RESOLVED.**

## 2. Acceptance criteria — unchanged, still 6 of 6

```
### Acceptance Criteria Status
- Source: docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md
- Total AC items: 6
- Checked off (delivered): 6
- Remaining (unchecked): 0
- Items remaining: none
```

`git diff --name-only 87233f86..HEAD -- docs/.../spec.md` shows exactly one changed line (the
sanitised `## Repro & Evidence` snippet at line 19); the six `- [x] N.` lines at 104-109 are
byte-unchanged.

## 3. Write-set integrity — verified

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1`: unchanged since `eea3bb9b` (2 files, 100
  insertions/1 deletion total across the fix + new test file, confirmed via
  `git diff --stat 87233f86..HEAD -- scripts/vscode/Invoke-MSTestWithCoverage.ps1
  tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`).
- `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1` (out-of-scope, must remain
  byte-identical): `git diff --name-only 87233f86..HEAD -- <that path>` returns empty. Confirmed
  untouched.
- No file outside `docs/features/` and the two source paths above was touched anywhere in the
  branch diff: `git diff --stat 87233f86..HEAD -- . ':(exclude)docs/features/'
  ':(exclude)scripts/vscode/Invoke-MSTestWithCoverage.ps1'
  ':(exclude)tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1'` returns
  empty output.

## 4. Prior non-blocking findings — re-checked, all still non-blocking

| ID | Finding | Status after remediation |
|---|---|---|
| POL-3 | Evidence artifact `Timestamp:` fields postdate their containing commits | Unaffected by a documentation-only remediation; still non-blocking. |
| POL-4 | Stale `artifacts/pr_context.summary.txt` | Unaffected; still non-blocking. |
| POL-5 | Policy-audit template MCP asset / validator not reachable to feature-review's tool profile | Unaffected; this reaudit is hand-authored for the same reason, preserving all required headings. |
| POL-6 | `[P3-T6]` coverage-gain divergence left unchecked | Unaffected; not touched by remediation. |
| (PowerShell coverage) | 78.3313% vs 85% floor, pre-existing shortfall, positive delta | Unaffected; no production or test file changed in this remediation cycle. |

None became blocking.

## Verdict

**PASS.** Blocking findings: **0**. Branch is ready to proceed to PR authoring, subject to the
squash-merge requirement recorded in `evidence/other/r1-squash-merge-note.2026-09-03T12-23.md` and
`orchestrator-state.json`'s `post_pr_merge_note`.
