---
name: project-927-evidence-hygiene-sweep-plan-seams
description: Issue #927 round-6 seam — the parallel-run blast-radius extractor harvests path tokens from INLINE backtick spans only (including multi-word command spans), so a `./scripts/vscode/X.ps1` invocation inside a command span is a write claim that defeats the scripts/vscode/** mandate-read glob; fix by moving such payloads into fenced text blocks, which the per-line span regex never sees
metadata:
  type: project
---

Round 6 of the #927 plan (2026-09-29) was a scheduling defect, not a preflight defect: the
parallel-run scheduler derived a radius from the plan and made the item contend with siblings
that never touch its Write Set.

**Mechanism (verified in .claude/lib/blast-radius/BlastRadiusExtraction.psm1).** `Get-PlanPaths`
matches one inline span per backtick pair per line (line 69), splits each span on whitespace
(lines 224 to 236) and classifies every token. A fence line (three backticks plus a language) has no
closing backtick on its own line, so a fenced payload contributes NO token. Acceptance rules that
matter for command payloads:

- a token whose leading segment carries a colon is rejected, so a `":!.claude/"` pathspec exclusion
  is never harvested (lines 315 to 321);
- a wildcard-free token needs a recognized extension on its final component (closed list, lines 88
  to 94: ps1 sln xml yml md json toml txt cs ... but NOT log, exe, runsettings, config), so a
  quoted token like `"coverage/x.xml"` (ext `xml"`) and a directory token like `scripts/vscode` are
  dropped;
- `./scripts/vscode/Invoke-Restore.ps1` IS accepted (leading `.` segment has no colon, ext ps1) and
  is recorded VERBATIM with its `./`, so the `scripts/vscode/**` mandate-read glob in
  config/blast-radius.json (full-match containment, BlastRadiusGlob.psm1 line 30) does not remove
  it. Every `& ./scripts/...` invocation in a command span is therefore a write claim.

**How to apply:** when a plan's command spans invoke or read repository scripts, solution files,
runsettings or checkpoints the plan does not write, carry those payloads in fenced `text` blocks
placed in the task's attribution window (after the task line, before the next task line or
heading). Leave inline: spans naming only Write Set paths, evidence paths, or ignored coverage
outputs; colon-bang exclusion pathspecs; separator-free scope roots (`scripts tests .github`).
State the rule once in the plan's formatting-contract note so a later editor does not "fix" it.

**Editing technique that kept the payloads byte-identical:** never retype a long payload. Edit the
prefix (`- Command: \`pwsh ... '$p = ` becomes `- Command: the pwsh payload fenced below ...\n\n\`\`\`text\npwsh ... '$p = `)
and the suffix (`...Count'\`.\n  - Acceptance:` becomes `...Count'\n\`\`\`\n\n  - Acceptance:`) as two
separate Edit calls so the body between them is untouched. When an explanatory parenthetical
followed the span on the same line, move it to its own sub-bullet (`- Reading rule:`) instead of
duplicating it before the fence.

**Checks that closed the pass:** grep `^\`\`\`` count is even; grep `^#{1,6} ` lists only real
headings; task-line count unchanged; no fenced line contains a backtick or starts with `#` or the
task prefix; `\r$` count 0 (the plan is LF); the Write Set section byte-identical (P6-T14's
scope gate reads only that section).

**Round 7 (R5-01, blocking): a final-QA gate compared against a baseline figure the baseline task
never recorded.** P6-T7 said "warning count not greater than the P0-T12 figure" but P0-T12's
acceptance named no warning row, and the Phase 6 msbuild run overwrites the Phase 0 log
(`Tee-Object -FilePath`, no append), so the figure had no surviving source. Fix shape: the baseline
task transcribes the whole-line `N Warning(s)` summary and records its integer as a labelled artifact
row (`BASELINE-WARNINGS:`), stating WHY the row is the only surviving copy; the final gate names that
row. Corollary: do not call a whole comparison "baseline-relative" when only one clause reads the
baseline; state which clauses stay absolute (exit code, `ZERO-ERRORS=1`). Inheritance via "as P0-T12"
/ "as P6-T7 against the P0-T13 figures" propagates the new row without a text change, but the
self-review must re-derive and state the inherited reading explicitly. Console-logger shape verified
in tracked archived logs (issue-164 evidence): `Build succeeded.`, blank, `    0 Warning(s)`,
`    0 Error(s)`.
