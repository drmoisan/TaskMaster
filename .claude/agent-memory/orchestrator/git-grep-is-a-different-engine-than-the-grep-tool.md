---
name: git-grep-is-a-different-engine-than-the-grep-tool
description: A baseline measured with the Grep tool can make a plan acceptance condition unfalsifiable, because git grep defaults to BRE where a bare ? is literal; git grep is also blind to untracked files and exits 1 silently.
metadata:
  type: feedback
---

When a plan states an acceptance condition as a `git grep` command, measure the baseline with
`git grep` itself. Never hand the planner a count measured with the Grep tool.

**Why.** On issue 742 (2026-09-12) I verified a residual-sweep baseline of 14 lines using the Grep
tool and passed that figure into the planning prompt as a verified fact. The Grep tool is ripgrep, an
extended-regex engine where `?` is a quantifier. `git grep` defaults to POSIX **basic** regular
expressions, where a bare `?` is a **literal character**. The pattern
`[.]ToString[(]@?"[^"]*[:/.\-][^"]*"[)]` therefore matched 14 lines under the Grep tool and **zero**
lines under `git grep` on the same unfixed tree. Preflight round 1 caught it. Five acceptance
conditions were built on that pattern: two demanded a specific non-zero result the command could never
print (permanently unsatisfiable) and one would have passed vacuously whether or not the phase fixed
anything. The GNU BRE spelling `@\?` reproduces the intended 4/4/1/3/2 distribution and exits 0.

**The second trap, same command, same run.** `git grep` does not search untracked files. It prints
nothing and exits 1, which is byte-identical to a genuine zero result. A freshly created feature
folder — plan, spec, research, evidence — is entirely untracked until the first commit, and so is any
test file the plan creates. `git grep --untracked` fixes it. This bit three times in one run: once in
the plan (an assertion on the new test file's method count), once in my own verification of the fix,
and it was pre-emptively flagged to the round-2 reviewer to stop it biting a fourth time.

**How to apply.**

- Pick the instrument first, then measure. If the acceptance condition will run `git grep`, the
  baseline command is `git grep`. Cross-engine figures are not interchangeable evidence.
- Suspect any `git grep` that exits 1 before believing the zero. Run a **positive discovery control**
  with the same command form against something you know is present. A zero result with no control is
  not an observation. I applied this to my own negative result and the control returned 8 and 9 hits,
  which is what made the zero trustworthy.
- Escape `?`, `+`, `|`, `(`, `)`, `{`, `}` for `git grep` BRE, or pass `-E`. Prefer `-F` when the
  pattern is a fixed string.
- Add `--untracked` for anything not yet committed.

Related: [[absence-from-failure-list-is-not-a-pass-gate]],
[[preflight-catches-vacuous-gates]], [[my-own-negative-claims-need-a-scoped-search]],
[[piped-command-exit-code-is-the-last-segment]].
