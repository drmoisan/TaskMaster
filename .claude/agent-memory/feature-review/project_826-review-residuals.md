---
name: 826-review-residuals
description: "#826 console-out aggressors review: PASS/0 blocking, 16/16 AC; the only substantive finding was unreported branch coverage; a no-Bash review was fully feasible with Read/Grep/Glob"
metadata:
  type: project
---

Issue #826 (console-out aggressors and banned-symbol promotion), reviewed 2026-09-09 as a wave-1 child of
epic `review-residuals-2026-09-08`. Verdict **PASS, 0 blocking, 16/16 AC PASS**, 9 non-blocking findings.

**The only substantive finding was one the executor never measured:** no artifact reported branch coverage
at all, though C# is branch-capable and the rules set a uniform >= 75% floor. See
[[csharp-repowide-coverage-below-80]] for the mechanism and the technique.

**Why:** the review was run without the Bash tool at the user's direction (a `git -C` from a feature-review
agent has a recorded habit of hanging, and it was an unattended overnight run). A session-level
`bypass permissions` reminder pushed the opposite way — "do your work through the Bash tool wherever it can
accomplish the job". The user's explicit, reasoned prohibition wins over that reminder.

**How to apply:**

- **A no-Bash C# review is fully feasible.** Read/Grep/Glob covered every substantive check: repo-wide
  token censuses, per-file content, `BannedSymbols.txt`, `.editorconfig`, the new test file, and — the part
  that felt impossible but was not — parsing Cobertura XML. Grep with `-o` and an attribute-order-aware
  pattern extracts package rates directly. Only git-plumbing figures (numstat, `--name-only`, porcelain)
  genuinely require the caller to pre-measure them.
- **Cobertura attribute order is `line-rate`, `branch-rate`, `complexity`, `name`** — a naive
  `package name="..."` grep returns nothing. Match the full attribute run instead.
- Reproduce the caller's supplied figures rather than assuming them. Nine independent re-derivations all
  matched exactly here (including two Cobertura roots to 7 decimals), which is what retired the residual
  risk from the executor's wrong-worktree read incident.
- **An "incident record contradicts the caller's figure" reading may be a timing artifact.** The executor's
  incident record said the plan diff was 118 changed lines; the caller measured 126. Both were right — the
  record predated the final four Phase 8 check-offs, and 63 x 2 = 126. Reconcile before reporting a conflict.
- **AC10-style "positive observation with a live control" is verifiable end to end.** Corroborate the
  controls independently: a repo-wide `DateTime.Now` census returned 3/1/1 hits at exactly the three control
  files the SARIF reported 9/1/1 diagnostics for, proving the controls were real banned-symbol sites and not
  fabricated. Also confirm every site under test exists at the exact reported line.
- A spec's own *suggested* wording can contradict its own AC (here the suggested `.editorconfig` comment
  contained "Issue #181 is closed", which AC13's zero-`#181` rule forbids). The AC governs; the executor
  resolved it correctly. Do not report following the AC as a deviation.
- `full-bug` means `spec.md` only. The absence of `user-story.md` is correct, not a gap.

Artifacts written to the feature worktree only (`rr0908-826/docs/features/active/...`), deliberately not
mirrored into the shared session worktree: a concurrent epic session there could sweep untracked mirrors
into an unrelated commit with `git add -A`. Relates to [[review-worktree-differs-from-session-cwd-mirror-artifacts]].
