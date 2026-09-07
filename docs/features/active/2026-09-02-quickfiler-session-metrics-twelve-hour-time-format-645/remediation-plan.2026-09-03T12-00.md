# Remediation Plan — quickfiler-session-metrics-twelve-hour-time-format-645

- Timestamp: 2026-09-03T12-00
- Source: `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/remediation-inputs.2026-09-03T12-00.md`
- Scope: repository-hygiene remediation only. No production or test source file is touched by
  this plan. Requirements source is the remediation-inputs artifact above (Blocking Finding 1);
  there is no `spec.md` acceptance-criteria section this remediation targets, so the AC inventory
  below is a single synthetic item (`REM1`) per the calling agent's directive.

## Blocking Finding Being Remediated

Two committed Cobertura coverage-evidence XML files each embed 2,007 occurrences of the literal
absolute host path prefix `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\`
inside `<class filename="...">` attributes, disclosing the operator's Windows account name and
local worktree layout:

- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`

Re-confirmed this pass (2026-09-03) by direct inspection of the current tree: both files still
show exactly 2,007 case-insensitive matches of the fixed string `DanMoisan`, and no `<source>`
element carries the leaking path (all 2,007 occurrences per file live one-per-line inside
`<class filename="...">` attributes, so a per-line fixed-string sweep and a per-occurrence count
are the same number for this defect class).

## Out-of-Band Note for the Orchestrator (Not a Plan Task)

The remediation-inputs artifact recommends squash-merging this branch into `main` so the original,
unsanitized blobs added in commits `9cc37d01` and `6c1ac1f1` do not remain permanently reachable
from `main` history after this remediation's sanitizing commit lands. Rewriting git history or
choosing a merge strategy is the parallel-orchestrator's responsibility at merge time, not this
plan's; no task below performs a squash, rebase, or history rewrite.

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read, in this exact order, the repository policy files at their absolute paths
  under the item worktree root:
  1. `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/CLAUDE.md`
  2. `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/general-code-change.md`
  3. `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/general-unit-test.md`
  4. `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/csharp.md`
  5. `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/tonality.md`
  Then write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-00.md`
  containing `Timestamp:`, `Policy Order:` listing the five files above in the stated order, and
  an explicit `Files Read:` enumeration of the same five files.
  Acceptance: the artifact file exists at the stated path and contains `Timestamp:`,
  `Policy Order:` with all five file paths listed in the order above, and a `Files Read:`
  enumeration matching the same five paths.

- [x] [P0-T2] Read `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/remediation-inputs.2026-09-03T12-00.md` in full and write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/phase0-remediation-inputs-read.2026-09-03T12-05.md`
  containing `Timestamp:`, the two exact file paths quoted from the "Files:" list under
  "Blocking Finding 1", and the two commit short-SHAs (`9cc37d01`, `6c1ac1f1`) quoted from the
  same section.
  Acceptance: the artifact file exists at the stated path and contains all four listed fields
  verbatim (both paths, both SHAs).

- [x] [P0-T3] Capture the pre-remediation fixed-string sweep count for
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
  by running:
  `pwsh -NoProfile -Command '$m = Select-String -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml" -Pattern "DanMoisan","agent-a6cd1c774527c71c3" -SimpleMatch; Write-Output ("MATCH_COUNT=" + $m.Count)'`
  and write `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/baseline-sweep-coverage-baseline.2026-09-03T12-05.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `MATCH_COUNT=2007`.

- [x] [P0-T4] Capture the pre-remediation fixed-string sweep count for
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  using the same command form as P0-T3 (absolute `-LiteralPath`, single-quoted outer wrapper)
  with the path substituted, and write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/baseline-sweep-coverage-final.2026-09-03T12-05.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `MATCH_COUNT=2007`.

---

### Phase 1 — Sanitize `evidence/baseline/coverage-baseline.cobertura.xml`

- [x] [P1-T1] Run the redaction substitution against
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`:
  `pwsh -NoProfile -Command '$p = "C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml"; $c = [System.IO.File]::ReadAllText($p); $pat1 = [regex]::Escape("C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\"); $pat2 = [regex]::Escape("C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/"); $n1 = ([regex]::Matches($c, $pat1, "IgnoreCase")).Count; $n2 = ([regex]::Matches($c, $pat2, "IgnoreCase")).Count; $c = [regex]::Replace($c, $pat1, "", "IgnoreCase"); $c = [regex]::Replace($c, $pat2, "", "IgnoreCase"); [System.IO.File]::WriteAllText($p, $c, (New-Object System.Text.UTF8Encoding($false))); Write-Output ("REPLACED_BACKSLASH=" + $n1 + " REPLACED_FORWARDSLASH=" + $n2)'`
  This substitutes the absolute worktree-root prefix (both backslash and forward-slash
  path-separator variants, matched case-insensitively) with an empty string, leaving each
  `filename="..."` attribute as a repository-relative path (e.g.
  `QuickFiler\Controllers\EfcHomeController.cs`). Write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/sanitize-coverage-baseline.2026-09-03T12-06.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly
  `REPLACED_BACKSLASH=2007 REPLACED_FORWARDSLASH=0`.

- [x] [P1-T2] Verify the rewritten file still parses as well-formed XML:
  `pwsh -NoProfile -Command 'try { [xml](Get-Content -Raw -LiteralPath "C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml") | Out-Null; Write-Output "XML_WELL_FORMED=True" } catch { Write-Output ("XML_WELL_FORMED=False: " + $_.Exception.Message); exit 1 }'`
  Write `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/xml-wellformed-coverage-baseline.2026-09-03T12-06.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `XML_WELL_FORMED=True`.

- [x] [P1-T3] Re-run the fixed-string sweep (same command form as P0-T3) against
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
  and write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/post-sweep-coverage-baseline.2026-09-03T12-07.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `MATCH_COUNT=0`.

---

### Phase 2 — Sanitize `evidence/qa-gates/coverage-final.cobertura.xml`

- [x] [P2-T1] Run the redaction substitution against
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  using the same command form as P1-T1 with the path substituted. Write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/sanitize-coverage-final.2026-09-03T12-08.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly
  `REPLACED_BACKSLASH=2007 REPLACED_FORWARDSLASH=0`.

- [x] [P2-T2] Verify the rewritten file still parses as well-formed XML using the same command
  form as P1-T2 with the path substituted. Write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/xml-wellformed-coverage-final.2026-09-03T12-08.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `XML_WELL_FORMED=True`.

- [x] [P2-T3] Re-run the fixed-string sweep (same command form as P0-T3) against
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  and write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/post-sweep-coverage-final.2026-09-03T12-09.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: `EXIT_CODE: 0` and the printed line reads exactly `MATCH_COUNT=0`.

---

### Phase 3 — Commit Sanitized Evidence

- [x] [P3-T1] Stage exactly the two sanitized files with an explicit pathspec (never `git add -A`
  or `git add .`):
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 add docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  then
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 status --porcelain -- docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`
  Write `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/git-stage-verification.2026-09-03T12-10.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: the porcelain output shows exactly two lines, each beginning with `M ` in the
  staged (index) column, one per sanitized path, with no unstaged (`M` in the second column)
  marker on either line.

- [x] [P3-T2] Commit the staged files:
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 commit -m "fix(evidence): redact absolute worktree host path from Cobertura coverage evidence (issue #645)"`
  then record the resulting commit via
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 rev-parse --short HEAD`. Write
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/git-commit-record.2026-09-03T12-10.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (the printed short SHA).
  Acceptance: both commands exit `0` and the artifact records a non-empty 7+ character short SHA.

- [x] [P3-T3] Verify the new commit touches exactly the two sanitized paths and nothing else:
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 diff --name-only HEAD~1..HEAD`
  Write `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/git-commit-scope-verification.2026-09-03T12-10.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`.
  Acceptance: the output contains exactly two lines, matching
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml`
  and
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml`,
  and no other path.

---

### Phase 4 — Final QC (Language-Scope Determination and Evidence Close-Out)

- [x] [P4-T1] Using the
  `git -C C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3 diff --name-only HEAD~1..HEAD`
  output captured in P3-T3, confirm the
  commit changed zero `*.cs`, zero `*.csproj`, zero `*.ps1`, and zero `*.py` paths (the only two
  changed paths are the sanitized `.xml` evidence files). Record the determination in
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/final-qa-scope-determination.2026-09-03T12-11.md`
  with `Timestamp:`, `Command:` (the P3-T3 command, re-cited), `EXIT_CODE:`, and
  `Output Summary: C#_TOOLCHAIN: NOT APPLICABLE (0 .cs/.csproj/.ps1/.py files changed by this commit)`.
  Acceptance: the artifact records a `0` count for each of the four extensions and the
  `C#_TOOLCHAIN: NOT APPLICABLE` line verbatim. This satisfies the "Final QA Loop" requirement of
  the atomic-plan-contract for this cycle: no language-specific toolchain applies because no
  source file in any covered language was changed.

- [x] [P4-T2] Close out acceptance criterion `REM1` by confirming, from the artifacts already
  written, that both post-sanitization sweeps (P1-T3, P2-T3) record `MATCH_COUNT=0` and both
  well-formedness checks (P1-T2, P2-T2) record `XML_WELL_FORMED=True`. Record the confirmation in
  `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/remediation-baseline/rem1-ac-closeout.2026-09-03T12-11.md`
  with `Timestamp:` and an `Output Summary:` line citing all four source artifact filenames and
  their recorded values, then check off `[P1-T1]` through `[P4-T1]` above as complete once each
  task's own artifact and acceptance criterion has been independently confirmed on disk.
  Acceptance: the artifact exists, cites all four source artifacts by filename, and each cited
  value matches what P1-T2/P1-T3/P2-T2/P2-T3 actually recorded.

---

## Planner Adversarial Self-Review

SELF-REVIEW: RE-DERIVED THIS PASS

- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/remediation-inputs.2026-09-03T12-00.md` — lines 7-50 (Blocking Finding 1: file list, defect description, confirmed 2,007-per-file count, required remediation steps 1-4) — read in full this pass.
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml` — line 6, `<class filename="C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\QuickFiler\Controllers\EfcHomeController.cs">` — read this pass to confirm the exact literal prefix and attribute format the substitution task targets.
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml` — fixed-string count of `DanMoisan` re-derived this pass via direct search: 2,007 matches (matches the remediation-inputs citation).
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml` — fixed-string count of `DanMoisan` re-derived this pass via direct search: 2,007 matches (matches the remediation-inputs citation).
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml` — searched for a `<source>` element this pass: none found, confirming all 2,007 occurrences live one-per-line inside `<class filename="...">` attributes only (no second leak surface in this file).
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/` (directory listing) — re-derived this pass: no pre-existing `evidence/remediation-baseline/` subfolder, so Phase 0-4 artifact paths in this plan create that folder rather than colliding with prior content.
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/` (directory listing) — re-derived this pass: no pre-existing `remediation-plan*.md` file, confirming this is first authoring of this plan, not a revision round.

### Revision Round 2 (2026-09-03) — Preflight Defects A–D

SELF-REVIEW: RE-DERIVED THIS PASS

- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml` — fixed-string count of `DanMoisan` re-derived this pass via direct case-insensitive search: 2,007 matches, confirming the `MATCH_COUNT=2007` acceptance value in P0-T3 still holds against current tree state after the outer-quote and absolute-path rewrite of the sweep command.
- `docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml` — fixed-string count of `DanMoisan` re-derived this pass via direct case-insensitive search: 2,007 matches, confirming the `MATCH_COUNT=2007` acceptance value in P0-T4 still holds.
- `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/CLAUDE.md` — existence re-derived this pass (glob match at item worktree root), confirming the absolute path cited in new task P0-T1 resolves.
- `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/general-code-change.md` — existence re-derived this pass, confirming the absolute path cited in P0-T1 resolves.
- `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/general-unit-test.md` — existence re-derived this pass, confirming the absolute path cited in P0-T1 resolves.
- `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/csharp.md` — existence re-derived this pass, confirming the absolute path cited in P0-T1 resolves.
- `C:/Users/DanMoisan/repos/TaskMaster/.claude/worktrees/agent-a6cd1c774527c71c3/.claude/rules/tonality.md` — existence re-derived this pass, confirming the absolute path cited in P0-T1 resolves.
- `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a6cd1c774527c71c3\.git` — read this pass: a gitdir pointer file resolving to `C:/Users/DanMoisan/repos/TaskMaster/.git/worktrees/agent-a6cd1c774527c71c3`, confirming the item worktree root used in every `git -C <root>` rewrite (P3-T1, P3-T2, P3-T3, P4-T1) is a valid worktree, not a bare or missing directory.
- Sibling-region check on Phase 0 (P0-T3, P0-T4): re-read both tasks after inserting the new P0-T1 and renumbering; confirmed P0-T4's "same command form as P0-T3" cross-reference and P1-T3/P2-T3's "same command form as P0-T3" cross-references were updated in the same edit and no other task in the plan still cites the retired ID `P0-T2` in the sweep-count-command sense (the surviving `P0-T2` id now names the unrelated "read remediation-inputs" task and is not cross-referenced from any sweep-command task).
- Sibling-region check on Phase 3 (P3-T1, P3-T2, P3-T3): re-read all three tasks after adding `-C <root>` to every `git` invocation; confirmed no bare `git ` invocation (without a preceding `-C <root>`) remains in Phase 3 or in P4-T1's re-citation of the P3-T3 command.
- Sibling-region check on Phase 1/2 command payloads (P1-T1, P1-T2, P2-T1, P2-T2): re-read all four after converting P1-T1 and P1-T2 to single-quoted outer wrappers with double-quoted inner literals; confirmed neither payload contains an embedded literal single-quote character (all inner string literals — paths, regex patterns, `IgnoreCase`, empty-string replacements, status labels — use double quotes only) and that P2-T1/P2-T2, which reference "same command form as P1-T1"/"same command form as P1-T2" without repeating the literal, inherit the corrected form without requiring their own text to change.
- AC-MAPPING/CITATION re-check: confirmed none of the three `CITATION:` lines or the `AC-MAPPING:` line in the Planner Internal Review Record below name a task ID or file path altered by this revision round (they cite `remediation-inputs.2026-09-03T12-00.md` lines 7-50, the coverage-baseline sample filename attribute, and the coverage-final match count — none of which changed), so those lines are carried forward unchanged rather than restated.

## Planner Internal Review Record

PLANNER-INTERNAL-REVIEW: PASS
CITATION-TO-TREE: PASS
AC-TRACEABILITY: PASS
SCOPE-BOUNDARY: PASS
CITATION: docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/remediation-inputs.2026-09-03T12-00.md | lines 7-50, Blocking Finding 1
CITATION: docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/baseline/coverage-baseline.cobertura.xml | line 6, sample filename attribute
CITATION: docs/features/active/quickfiler-session-metrics-twelve-hour-time-format-645/evidence/qa-gates/coverage-final.cobertura.xml | fixed-string DanMoisan count = 2007
AC-INVENTORY: REM1
AC-MAPPING: REM1 | IMPLEMENTATION: P1-T1, P2-T1 | TESTS: P1-T2, P1-T3, P2-T2, P2-T3 | EVIDENCE: P4-T2
UNRESOLVED-GAPS: NONE

DIRECTIVE: PREFLIGHT VALIDATION ONLY
