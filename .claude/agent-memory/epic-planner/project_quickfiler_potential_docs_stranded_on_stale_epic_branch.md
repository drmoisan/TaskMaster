---
name: quickfiler-potential-docs-stranded-on-stale-epic-branch
description: 41 QuickFiler bug issues' authoritative potential documents exist only on the stale epic/quickfiler-per-file-coverage-integration branch, never on main, and the GitHub issue bodies are near-empty without them
metadata:
  type: project
---

The QuickFiler open-bug corpus (48 validated items as of 2026-08-21) was spilled out of epic #136
(`quickfiler-per-file-coverage`) during its children's research. Their promoted potential documents
were committed to that epic's integration branch and **never reached `main`**.

Measured 2026-08-21: of 49 candidate issues, 41 promoted potential docs exist only at
`origin/epic/quickfiler-per-file-coverage-integration:docs/features/potential/promoted/<file>`,
7 exist on both, and 1 (#571, newer) only on `main`.

**Why this matters more than it sounds:** those documents are the *only* real requirements source.
The GitHub issue bodies for the terse items are near-empty — every section below `## Summary`
literally reads `(not provided in potential file)`. The potential doc, by contrast, carries the
exact `file:line`, the offending code block, the root cause, a suggested fix, severity, and a
detection note. A preparation child pointed only at `gh issue view` will plan from ~700 bytes
instead of ~5 KB.

**How to apply:** before preparing any child of this corpus, restore its potential doc onto the
epic integration branch:
`git checkout origin/epic/quickfiler-per-file-coverage-integration -- docs/features/potential/promoted/<file>.md`
Then point the child at the restored path explicitly, not at the issue body.

Two related facts about that stale branch:
- **It is not a collision hazard.** It differs from `origin/main` by zero files under `QuickFiler/`
  and `QuickFiler.Test/`; epic #136's code already landed on `main` via the child PRs. Its 70
  commits ahead are docs/`.claude` churn only, and it is ~1.18M lines behind `main`. Do not merge
  it wholesale to recover the docs — cherry-pick the individual files.
- **Its bookkeeping is all open.** Epic #136 and all 15 child issues (430-437, 452-456, 495-497)
  are still `OPEN` although their code is on `main`. Do not read an open child issue as unlanded
  work; check the file content on `main` instead.

Related: [[check-inflight-branches-before-decomposition]],
[[preexisting-issues-skip-potential-to-issue]], [[quickfiler-csproj-universal-contention]].
