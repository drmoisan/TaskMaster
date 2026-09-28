---
name: re-audit-cycle-review-playbook
description: How to run a post-remediation re-audit - separate remediable blockers from PR-time gates, test "narrower than fixed" labels, census uncommitted compaction claims, and re-anchor a moved merge base
metadata:
  type: project
---

Distilled from #911 cycle 2 (2026-09-20), the first full re-audit after an 80-task remediation plan.

**Separate the blocker taxonomy in the final answer.** The caller's decision rule is usually "if any
blocking findings remain, I open another remediation cycle". A green-run-at-merge-head gate, a
squash-merge obligation, and an AC deferred to a follow-up issue are all Blocking-shaped but none is
remediable by a cycle. Report two counts: **remediable blocking findings** and **gates that only the
pull request / merge can close**. Getting this wrong costs a wasted cycle.

**Test every discharge label, not the finding number.** Executors label some discharges narrower
than "fixed" — "out of scope by decision D<n>", "visibility-only", "working-tree only". Each label
is a claim with its own verification:
- *out of scope* — verify unreachability from BOTH ends (guard and call site), and verify the paired
  obligation the remediation input demanded (usually a spec/doc amendment) actually landed.
- *visibility-only* — verify the diagnostic reaches the log from the DEPLOYED invocation. See
  [[write-verbose-remedy-is-inert-without-a-verbose-call-site]].
- *working-tree only* — verify the residual is zero at head AND that the pre-fix blobs really are
  still reachable (a follow-up commit, not a rewrite), so the squash-merge instruction is warranted.

**A scope decision has doc consequences the remediation input did not enumerate.** When a decision
narrows behaviour, grep every operator-facing document (`README.md`, runbooks) — not only the AC
text. On #911 the spec AC14 note and the workflow comment were amended; the workflow README was not
and still advertised the now-unreachable class.

**Compaction claims about uncommitted intermediates.** "The file hit 510 and I compacted it to 499,
removing only whitespace" cannot be diffed if 510 was never committed. Substitute a whole-cycle
census of `It` / `Should` / `-Because` / Arrange-Act-Assert marker counts per test file at both
heads, and check `Should` count equals `-Because` count. Monotone-nondecreasing counts corroborate;
say explicitly that the intermediate is unverifiable by comparison.

**Re-anchor the merge base every cycle.** On #911 the base advanced from `734112ed2` to
`b5621910c` because a sibling PR landed the same fix on `main` and the branch merged it; 15
`.csproj` silently left the diff. Re-run the whole-tree invariant census anyway, then record the
**attribution shift** — the criteria still hold at head but a reader of the diff alone will not see
the change. Do not downgrade those criteria.

**A plan clause reported unmet is usually the right call.** Verify the clause's *purpose*
independently rather than the count: measure each defective construct at 0 occurrences and each
replacement at its expected count. Then also re-add the evidence artifact's own columns — on #911 a
per-edit deletion table summed to 6 against a measured 5 because one row claimed an unchanged
context line.

**Corrections recorded mid-cycle are a quality signal, and are checkable.** #911 documented three:
a red that arrived as a PowerShell parameter-binding error (`Cannot bind argument to parameter
'Line' because it is an empty string`) rather than an assertion failure, fixed with
`[AllowEmptyString()]`; a non-vacuity grep that read a false zero because a PowerShell
double-quoted `"\\"` keeps both backslashes, fixed by building the needle from `[char]92`; and a
formatter restart after a 9-finding analyzer batch. Each was recorded in the artifact that would
otherwise carry the false result. Re-derive at least one independently (counting `csc.exe` lines in
the msbuild log took one command).
