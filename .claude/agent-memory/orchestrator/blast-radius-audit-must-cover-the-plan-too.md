---
name: blast-radius-audit-must-cover-the-plan-too
description: "Auditing only spec.md for stray backticked paths is incomplete — the blast-radius extractor reads the plan task bodies too; and a BACKSLASH path is dropped by it, which is the escape hatch for command spans"
metadata:
  type: reference
---

The blast-radius extractor harvests backticked paths from **both** `spec.md` **and the plan task
bodies**. Auditing only the spec passes a clean bill on a plan that still over-schedules.

**Why:** on 2026-09-12 (issue #743) I audited every backticked forward-slash token in `spec.md`,
confirmed it resolved to exactly the declared seven-path Write Set, and recorded the radius as clean.
Preflight then found five out-of-Write-Set paths backticked in the **plan's** prose — the UtilitiesCS
threading and extension files and the test-support file — each of which would have serialized the item
against siblings that had no real conflict with it.

**How to apply:**

- Audit both files. Extract with the extractor's own rule rather than by eye: a backtick span, a
  whitespace-free token inside it, containing `/`, ending in a recognised extension. Intersect the
  result with tracked paths and diff against the declared Write Set.
- **A backslash path is DROPPED.** `.claude/lib/blast-radius/BlastRadiusExtraction.psm1` tests for `/`
  only, so `scripts\vscode\Invoke-MSTestWithCoverage.ps1` is not harvested while
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is. That is the fix for a path that must stay inside a
  backticked **command span**, where plain prose is not an option: write it with backslashes, which
  pwsh accepts anyway. For ordinary prose, name the file in words with no backticks at all.
- The extractor has no notion of polarity, so a path in a sentence promising *not* to touch the file is
  scheduled exactly like one you will edit. See [[spec-backticks-widen-blast-radius]].
- A plan that carries its own "excluded files are named in plain prose" rule can still violate it in
  the Decisions Record and the citation table, which is where the #743 violations were. Check those
  two regions specifically; they are prose-heavy and easy to skip.

Related: [[get-blastradius-overincludes-citations-omits-gitignored-writes]]
