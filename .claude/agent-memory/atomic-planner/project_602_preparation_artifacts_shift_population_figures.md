---
name: project-602-preparation-artifacts-shift-population-figures
description: "#602 R4-R5 (host-identifier sweep): expected population figures measured BEFORE the item's own preparation artifacts are committed go stale by exactly the artifact count; R5: a terminal-zero git-grep gate propagates exit 1 through pwsh (append an explicit zero exit to BOTH pair members); unfiltered porcelain counts staged rows; a 3-field baseline can never contain a 4-field gate record; numstat binary dash rows throw on [int] cast; never probe a write-mode formatter in Phase 0 after the anchor"
metadata:
  type: project
---

Issue 602 round-4 reconciliation (2026-09-12): a plan that enumerates tracked-file and per-class Markdown counts as Phase 0 expectations was measured by the predecessor before the item's own feature documents (plan, issue, spec, user-story, research = 5 under the active features tree) and one recovered agent-memory note were committed. Committing them shifted exactly three figures (tracked total +6, active-features Markdown +5, agent-memory Markdown +1) and nothing else; every identifier-bearing population was unchanged because the preparation artifacts are clean.

**Why:** the tracked-file total and any per-directory enumeration that includes the item's own folder are self-referential: the plan's own commit moves them. The spec, issue and research artifact keep the older figures, so the plan must say in prose why it disagrees with them rather than editing the spec.

**How to apply:**
- When a Phase 0 table includes a tracked-file total or a per-class enumeration covering the feature folder or agent-memory, state in the preamble that the figures include the item's own committed preparation artifacts and name the count, so a preflight reviewer measuring a different tree can reconcile.
- With no shell tool, record caller-supplied figures explicitly as "supplied, not measured" in the SELF-REVIEW enumeration; do not claim a measurement.
- Recording a finding that earlier redaction wrote placeholder tokens INTO XML attribute values: describe the tokens and attributes in prose only (no angle-bracket characters, no attribute-shaped text, no backticks), because the plan's own XML invariant and the blast-radius backtick harvest both apply to the paragraph that documents them.
- Under a "do not add or remove backticks" constraint, carried-forward SELF-REVIEW enumeration lines that contain a backticked token must stay verbatim; move backtick-free lines only.
- Detect CR bytes with a Grep for a carriage return (0 hits = LF-only); the Edit tool preserved LF here.

**Round 5 seams (2026-09-12), all generic:**
- A count gate whose expected terminal state is zero, built as `pwsh -Command '... $n = @(git grep ...); "LABEL=$($n.Count)"'`, exits 1 when git grep selects nothing, because pwsh propagates the last native command's exit code. The evidence row then normalises to a failure with no `ExpectedExitCode:`. Append `; exit 0` before the closing quote. When the gate is a baseline-and-residual pair whose residual says "re-run the identical command from P0-Tn", the residual holds no command text, so the trailing text lands on the baseline only and identity is by construction; say so in the self-review.
- `git status --porcelain -uno` lists staged rows too. An "unstaged = 0" clause run after a staging span and before the commit needs the second-column filter `Where-Object { $_ -match "^.[MD]" }`; counts taken after the commit do not.
- A subset comparison between a baseline record and a gate record requires identical field shape. Invoke-ScriptAnalyzer records printed as rule:severity:line in the baseline and rule:severity:script:line in the gate are never subset-comparable.
- `git diff --numstat` prints a dash for binary-classified rows; `[int]"-"` is a terminating error and the gate prints no line. Guard with `if ($p[0] -eq "-" -or $p[1] -eq "-") { $bin++; continue }` and print the count as informational.
- Never probe a write-mode formatter (PoshQC format, folder-granularity) in Phase 0 after the self-anchor tag exists: a repairing pass enters every anchored diff and no remediation clause exists yet. Probe the read-only analyze tool; let the first real format task carry the hash bracket.
- Tracked-only baselines versus a filesystem enumerator: any reconciliation between a `git grep -l` expectation and a script that walks the disk must name "untracked at the anchor" as an explicit attribution cause and subtract it from anchored-diff modified-file counts.
- A per-row numeric bound that the next sentence says "does not halt" is an expectation, not an acceptance condition; label it `Expected:` so the reviewer does not report an unfailable clause.
