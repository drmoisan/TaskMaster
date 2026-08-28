# quickfiler-search-box-loses-focus-on-dropdown-expand — Remediation Plan (Issue #680)

- **Issue:** #680
- **Trigger:** feature-review REMEDIATION REQUIRED, review cycle `2026-08-28T17-48`, source:
  `remediation-inputs.2026-08-28T17-48.md`
- **Owner:** drmoisan
- **Created:** 2026-08-28T18-05
- **Revision:** 2 (preflight round 1 deltas applied — see Preflight Revision Log below)
- **Status:** Draft
- **Work Mode:** full-bug (unchanged from `issue.md`; `spec.md` remains the sole feature-AC source —
  this remediation closes review findings, not spec ACs)
- **Feature folder (`<FEATURE>`):** `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680`
- **Branch:** `bug/quickfiler-search-box-loses-focus-on-dropdown-expand-680` (review cycle head:
  `c4e96b72`)

All evidence artifacts go to `<FEATURE>/evidence/<kind>/` (canonical scheme; `artifacts/*` evidence
paths are forbidden). `<ts>` in artifact names denotes the ISO-8601 `yyyy-MM-ddTHH-mm` stamp taken at
task execution time. This is a new plan file for a new remediation pass; it does not reuse or amend
`remediation-plan.2026-08-28T17-15.md`, which is already fully executed. This plan does not include a
`git commit` task — the orchestrator performs the commit separately after Phase 5 completes.

`EVIDENCE_LOCATION_OVERRIDE_REJECTED`: not applicable — no non-canonical evidence path was supplied by
the calling instructions for this plan.

## Preflight Revision Log (Round 1)

- **Delta 1 (P2-T3 `outcome="Failed"` count):** preflight counted the actual `72b4b7ed` blob and found
  3 literal `outcome="Failed"` occurrences, not 2 — 2 `<UnitTestResult>`-level failures plus 1
  `<ResultSummary outcome="Failed">` run-level rollup that the plan's grep pattern also matches. P2-T3's
  acceptance condition and the structural self-check restatement are corrected from `2` to `3`.
- **Delta 2 (self-leak in this plan's own prose):** the R2 finding description previously quoted the raw
  `runUser` value and a raw absolute storage path verbatim. Rewritten to describe the leak without
  quoting it, matching the AFTER-values-only convention in
  `evidence/other/trx-sanitisation.2026-08-28T16-45.md`.
- **Delta 3 (Phase 4 sweep too narrow):** preflight found four already-leaking review/memory artifacts
  that Phase 4's original two tasks did not sanitize. Added `P4-T2` to sanitize those four files (with
  its own before/after hit-count acceptance), renumbered the former `P4-T2`/`P4-T3` to `P4-T3`/`P4-T4`,
  and added the four files to P5-T6's allowed commit-readiness list.

**Preflight note:** this planning session has no `mcp__drm-copilot__validate_orchestration_artifacts`
tool in its surface. `VALIDATOR NOT RUN`. A structural self-check against the `atomic-plan-contract`
skill is recorded at the end of this document in place of a validator signal.

## Remediation Findings Addressed

- **R2 (Blocking):** five committed TRX evidence files carry real host identity — a `runUser` attribute
  exposing the real machine and account name, and in `p4-t4.trx` additionally raw absolute storage
  paths — introduced solely by commit `c4e96b72`, regressing the sanitization standard this branch's own
  commit `72b4b7ed` established. **Fixed by Phase 1 (four files) and Phase 2 (the fifth, `p2-t3.trx`,
  which requires a content restore rather than a placeholder substitution — see below).** Phase 4
  additionally re-runs the wide, whole-feature-folder sweep this branch used to certify the `72b4b7ed`
  fix, because this feature has a documented history of sanitization fixes that turned out to be too
  narrowly scoped, and (per preflight round 1) `P4-T2` now also sanitizes four review-artifact files that
  quoted the leak verbatim while reporting on it.
- **RC-2 (non-blocking, riding this commit per the reviewer's handoff):** the remediation execution's
  own Phase 2 green-test-run TRX write collided with, and overwrote, the original feature plan's AC-3
  fail-before red-run TRX at `evidence/regression-testing/p2-t3/p2-t3.trx`. **Fixed by Phase 2**:
  restore the original red-run TRX from commit `72b4b7ed` (which already carries the escaped-placeholder
  sanitization treatment) to its original path, relocate the remediation's own green-run TRX to a
  non-colliding directory, sanitize the relocated copy, and update the path reference in
  `evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md`.
- **RC-3 (non-blocking, riding this commit per the reviewer's handoff):** the remediation execution's
  evidence artifacts are self-stamped `2026-08-28T18-16` through `2026-08-28T20-12`, ahead of both the
  commit that introduced them (`c4e96b72`) and the wall-clock time of the review that flagged this
  (`2026-08-28T17-48`). **Addressed by Phase 3** with a documentation note only (per the calling
  instruction, this is treated as a sandbox-clock artifact with no functional effect and does not
  warrant renaming or rewriting any committed artifact).
- Items outside this plan's scope (per the calling instruction, which named only R2/RC-2/RC-3): the
  `spec.md` AC-6 stale-enumeration note and the owner's live-Outlook HV runbook action remain exactly as
  recorded in `remediation-inputs.2026-08-28T17-48.md` and are not touched here.

## Decisions Record

- **D1 (shared TRX sanitization routine — matches `72b4b7ed`'s treatment exactly).** Every sanitizing
  task in this plan applies this PowerShell routine, which reads and writes bytes 1:1 through
  ISO-8859-1 (so no byte is altered by an encoding round-trip), applies the three substitutions
  case-insensitively, worktree-root prefix first (because that prefix itself contains the account
  name), and always substitutes to the **XML-escaped** placeholder form — never the raw
  `<repo-root>`/`<user>`/`<host>` spelling, which would break XML well-formedness inside an attribute
  value (confirmed against this branch's own `72b4b7ed`-sanitized survivors, e.g.
  `evidence/regression-testing/p3-t6/p3-t6.trx`, which reads `storage="&lt;repo-root&gt;\..."` and
  `evidence/qa-gates/p4-t2/p4-t2.trx`, which reads `runUser="&lt;host&gt;\&lt;user&gt;"`):
  ```powershell
  function Set-TrxSanitized {
      param([Parameter(Mandatory)][string]$Path)
      $enc = [System.Text.Encoding]::GetEncoding('ISO-8859-1')
      $text = [System.IO.File]::ReadAllText($Path, $enc)
      $repoRoot = (git rev-parse --show-toplevel).Trim().Replace('/', '\')
      $before = $text.Length
      $text = [System.Text.RegularExpressions.Regex]::Replace(
          $text, [System.Text.RegularExpressions.Regex]::Escape($repoRoot),
          '&lt;repo-root&gt;', 'IgnoreCase')
      $text = [System.Text.RegularExpressions.Regex]::Replace(
          $text, [System.Text.RegularExpressions.Regex]::Escape($env:USERNAME),
          '&lt;user&gt;', 'IgnoreCase')
      $text = [System.Text.RegularExpressions.Regex]::Replace(
          $text, [System.Text.RegularExpressions.Regex]::Escape($env:COMPUTERNAME),
          '&lt;host&gt;', 'IgnoreCase')
      [System.IO.File]::WriteAllText($Path, $text, $enc)
      [PSCustomObject]@{ Path = $Path; BytesBefore = $before; BytesAfter = $text.Length }
  }
  ```
  The worktree-root prefix, `$env:USERNAME`, and `$env:COMPUTERNAME` are resolved fresh at execution
  time in every task that calls this routine; none of the three literal values is hardcoded anywhere in
  this plan document, so no absolute host path, account name, or machine name appears in this plan's own
  text.
- **D2 (verification sweep must use `-a`, never `-I` — this feature's own established lesson).** Every
  content-search verification task in this plan uses `rg -a -i -F -- '<literal>' <path>` (ripgrep's
  `--text`/`-a` forces a binary-classified file to be scanned as text). No verification task in this
  plan uses a flag or tool behavior that silently skips a binary-classified file, because that would let
  a match through undetected — exactly the failure mode `evidence/other/trx-sanitisation.2026-08-28T16-45.md`
  records guarding against for the `72b4b7ed` sweep.
- **D3 (vstest resolution, resolved fresh in every scoped-run task).**
  ```powershell
  $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
  $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
  ```
  Every scoped run in this plan passes `/InIsolation`, `/Settings:scripts\vscode\TaskMaster.cli.runsettings`,
  a named `/Logger:"trx;LogFileName=<task-id>.trx"`, and a task-private
  `/ResultsDirectory:<FEATURE>/evidence/<kind>/<task-id>` (one `p#-t#` subdirectory per run task, holding
  exactly one file named exactly `<task-id>.trx`).
- **D4 (host-path hygiene).** No `Command:` or `Output Summary:` field in any artifact this plan
  produces may contain an absolute host path; substitute `<repo-root>` / `<user>` / `<host>` or use
  repo-relative paths.
- **D5 (no `git commit` task).** The orchestrator performs the commit after Phase 5 completes.
- **D6 (`git checkout <commit> -- <path>` is a single-pathspec, precise restore).** Restoring
  `evidence/regression-testing/p2-t3/p2-t3.trx` from `72b4b7ed` with exactly one pathspec updates only
  that one file in the working tree and the index; no other tracked path is touched. The restored blob
  is verified byte-for-byte against the commit's own blob hash (P2-T3), not merely assumed correct from
  the command's exit code.
- **D7 (no full-repo coverage re-run in Phase 5, and why).** This remediation's only file changes are to
  evidence artifacts under `<FEATURE>/evidence/` plus this plan file and two append-only documentation
  edits (`p2-t3-new-test-green.2026-08-28T19-27.md`, `delivery-report.2026-08-28T16-40.md`); Phase 4 and
  P5-T6 both verify no `.cs`/`.csproj`/`.props`/`.targets` path appears in the change set. A change set
  containing zero production or test source lines cannot alter measured coverage, so re-running the
  ~20-minute full-repo coverage pass would re-confirm a number that cannot have moved; the most recent
  coverage evidence, `evidence/qa-gates/p4-t5-coverage-final.2026-08-28T20-05.md`, remains current for
  this branch state. The calling instruction's toolchain list for this pass (format/check → analyzer
  rebuild → nullable rebuild → pinned suites + full `QuickFiler.Test` suite) likewise does not name a
  coverage step. Phase 5 runs exactly that four-part toolchain.
- **D8 (no vacuous gates).** Every acceptance condition in Phases 1, 2, and 4 that claims "zero
  host-identity hits" is paired with a same-task or same-phase positive control proving the search
  mechanism actually finds the literal when it is present (Phase 0's before-sweep, and — for the
  restored `p2-t3.trx` — the `outcome="Failed"` count distinguishing the restored red run from the
  green run it replaces), so no zero-hit result can be produced by a scope or tooling defect rather than
  a real fix.

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read the policy documents in the `policy-compliance-order` sequence: `CLAUDE.md`,
  `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md`,
  `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`. Write
  `<FEATURE>/evidence/remediation-baseline/r2-phase0-instructions-read.<ts>.md` containing `Timestamp:`,
  `Policy Order:`, and the explicit list of the six files read. Acceptance: the artifact exists and
  lists all six files.
- [x] [P0-T2] Record remediation-pass context: `git rev-parse HEAD`, `git status --porcelain`.
  Acceptance: `git status --porcelain` output is empty at the moment this task runs (the orchestrator is
  expected to have committed any outstanding review artifacts before this plan's Phase 0 starts); the
  observed `HEAD` is recorded as `R2_BASE_COMMIT` (informational only, not asserted against a fixed
  hash). Artifact: `<FEATURE>/evidence/remediation-baseline/r2-p0-t2-context.<ts>.md` with `Timestamp:`,
  `Command:`, `EXIT_CODE:`, `Output Summary:`.
- [x] [P0-T3] Baseline (fail-before) host-identity sweep of the five named leaking files — for each of
  `evidence/remediation-baseline/p0-t6/p0-t6.trx`, `evidence/remediation-baseline/p0-t7/p0-t7.trx`,
  `evidence/regression-testing/p1-t3/p1-t3.trx`, `evidence/regression-testing/p2-t3/p2-t3.trx`,
  `evidence/qa-gates/p4-t4/p4-t4.trx`, run `rg -a -i -F -- '<literal>' <path>` for each of the three
  literal classes in D1 (worktree-root prefix, `$env:USERNAME` value, `$env:COMPUTERNAME` value; the
  literal values are resolved per D1, never hardcoded here). Acceptance: at least one of the three
  literal classes returns a nonzero hit count for every one of the five files (proves the leak is real
  and the search mechanism can detect it — the positive control required by D8); record the full 5×3
  hit-count matrix as `R2_BEFORE_MATRIX`. Artifact:
  `<FEATURE>/evidence/remediation-baseline/r2-p0-t3-before-sweep.<ts>.md` with the four schema fields.
- [x] [P0-T4] Baseline XML well-formedness check of the same five files: for each, run
  `[xml](Get-Content -Raw -Path <path>)` and record whether it throws. Acceptance: record the per-file
  PASS/FAIL result as `R2_BEFORE_XML_STATUS` (informational baseline; the leak does not itself break XML
  well-formedness, so this is expected to be PASS for all five and is recorded to give Phase 1/2 a
  same-file non-regression comparison point). Artifact:
  `<FEATURE>/evidence/remediation-baseline/r2-p0-t4-before-xml-check.<ts>.md` with the four schema
  fields.
- [x] [P0-T5] Distinguishing baseline for the RC-2 restore: count `outcome="Failed"` occurrences in the
  current `evidence/regression-testing/p2-t3/p2-t3.trx` via
  `(Select-String -Path <path> -Pattern 'outcome="Failed"' -AllMatches).Matches.Count`. Acceptance: the
  count is `0` (the file currently holds the remediation's 36/36 green run, per
  `evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md`); record as
  `R2_BEFORE_FAILED_COUNT`. Artifact:
  `<FEATURE>/evidence/remediation-baseline/r2-p0-t5-p2t3-before-state.<ts>.md` with the four schema
  fields.

### Phase 1 — R2: Sanitize the Four Directly-Leaking TRX Files

`evidence/regression-testing/p2-t3/p2-t3.trx` (the fifth leaking file) is deliberately excluded from
this phase; it is fixed by content restoration in Phase 2, not by placeholder substitution, because
restoring it also closes RC-2.

- [x] [P1-T1] Apply the D1 `Set-TrxSanitized` routine to each of these four files, in one script
  invocation that loops over the four paths and fails on the first exception:
  `evidence/remediation-baseline/p0-t6/p0-t6.trx`, `evidence/remediation-baseline/p0-t7/p0-t7.trx`,
  `evidence/regression-testing/p1-t3/p1-t3.trx`, `evidence/qa-gates/p4-t4/p4-t4.trx`. Acceptance: the
  routine completes for all four files with no exception; for every one of the four, the reported
  `BytesAfter` is strictly less than `BytesBefore` (the absolute path/account/machine literals being
  replaced are longer than the `&lt;repo-root&gt;`/`&lt;user&gt;`/`&lt;host&gt;` placeholders that
  replace them). Artifact: `<FEATURE>/evidence/qa-gates/r2-p1-t1-sanitize.<ts>.md` with the four schema
  fields and a before/after byte-count table for all four files.
- [x] [P1-T2] Re-run the same 4×3 sweep from P0-T3 (excluding `p2-t3.trx`) against the now-sanitized
  files: `rg -a -i -F -- '<literal>' <path>` for each of the three D1 literal classes across the four
  files. Acceptance: total hit count across all twelve sub-checks is `0`; zero files produced a read
  error. Artifact: `<FEATURE>/evidence/qa-gates/r2-p1-t2-sweep.<ts>.md` with the four schema fields and
  the 4×3 hit-count matrix (all zero), quoted against P0-T3's nonzero matrix for the same four files.
- [x] [P1-T3] Verify well-formedness and escaped-only placeholder form for the same four files: (a)
  `[xml](Get-Content -Raw -Path <path>)` throws no exception for any of the four; (b)
  `(Select-String -Path <path> -SimpleMatch -Pattern '<repo-root>','<user>','<host>' -AllMatches)` (the
  raw, unescaped spellings) returns `0` combined matches across all four files; (c)
  `(Select-String -Path <path> -SimpleMatch -Pattern '&lt;repo-root&gt;','&lt;user&gt;','&lt;host&gt;' -AllMatches)`
  returns at least `4` combined matches (at least one escaped placeholder landed in each file).
  Acceptance: all three conditions hold. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p1-t3-xml-and-escaping-check.<ts>.md` with the four schema fields.

### Phase 2 — RC-2: Restore the Original `p2-t3.trx` and De-Collide/Sanitize the Remediation's Green TRX

This phase also closes R2 for the fifth file, because the commit being restored from (`72b4b7ed`)
already carries the escaped-placeholder sanitization treatment.

- [x] [P2-T1] Verify the recovery source exists and is unambiguous: `git cat-file -e 72b4b7ed^{commit}`
  and `git cat-file -e 72b4b7ed:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3/p2-t3.trx`.
  Acceptance: both commands exit `0`. Artifact:
  `<FEATURE>/evidence/regression-testing/r2-p2-t1-source-verified.<ts>.md` with the four schema fields.
- [x] [P2-T2] Preserve the remediation's own green-run TRX before it is overwritten: create directory
  `evidence/regression-testing/r-p2-t3/` and `Copy-Item evidence/regression-testing/p2-t3/p2-t3.trx evidence/regression-testing/r-p2-t3/p2-t3.trx`.
  Acceptance: the destination file exists and `(Get-FileHash <source>).Hash -eq (Get-FileHash <dest>).Hash`
  immediately after the copy. Artifact:
  `<FEATURE>/evidence/regression-testing/r2-p2-t2-green-preserved.<ts>.md` with the four schema fields.
- [x] [P2-T3] Restore the original fail-before red-run TRX:
  `git checkout 72b4b7ed -- docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3/p2-t3.trx`.
  Acceptance: `git hash-object evidence/regression-testing/p2-t3/p2-t3.trx` equals
  `git rev-parse 72b4b7ed:docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3/p2-t3.trx`
  (exact blob-hash equality); AND
  `(Select-String -Path evidence/regression-testing/p2-t3/p2-t3.trx -Pattern 'outcome="Failed"' -AllMatches).Matches.Count`
  equals `3` (2 `<UnitTestResult>`-level AC-3 failures plus 1 `<ResultSummary outcome="Failed">`
  run-level rollup that the same pattern also matches — confirmed by extracting the `72b4b7ed` blob and
  counting literal occurrences directly), strictly greater than `R2_BEFORE_FAILED_COUNT` (`0`,
  P0-T5) — the false-before/true-after pair required by D8. Artifact:
  `<FEATURE>/evidence/regression-testing/r2-p2-t3-restored.<ts>.md` with the four schema fields.
- [x] [P2-T4] Apply the D1 `Set-TrxSanitized` routine to the relocated green-run copy
  `evidence/regression-testing/r-p2-t3/p2-t3.trx`. Acceptance: the routine completes with no exception;
  `BytesAfter` is strictly less than `BytesBefore`; a follow-up `rg -a -i -F -- '<literal>' <path>` sweep
  for all three D1 literal classes returns `0` combined hits; `[xml](Get-Content -Raw -Path <path>)`
  throws no exception; a `Select-String -SimpleMatch` check for the raw unescaped forms
  (`<repo-root>`, `<user>`, `<host>`) returns `0` matches, and for the escaped forms
  (`&lt;repo-root&gt;`, `&lt;user&gt;`, `&lt;host&gt;`) returns at least `1` match. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p2-t4-relocated-sanitized.<ts>.md` with the four schema fields.
- [x] [P2-T5] Append a relocation addendum to
  `evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md` (append-only; do not edit or
  delete any existing line), containing a new `## Relocation Addendum — <ts>` heading followed by
  prose stating, verbatim on its own line: `- Relocated TRX: evidence/regression-testing/r-p2-t3/p2-t3.trx`
  and explaining that the original `evidence/regression-testing/p2-t3/p2-t3.trx` path has been restored
  to the feature plan's original AC-3 fail-before red run (from commit `72b4b7ed`) because this file's
  own green-run TRX write had collided with and overwritten it, per RC-2 of
  `remediation-inputs.2026-08-28T17-48.md`. Acceptance:
  `git diff HEAD -- docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md | Select-String -Pattern '^-[^-]'`
  returns `0` matches (proves the edit is append-only); `Select-String -SimpleMatch -Pattern "Relocation Addendum"`
  on the file returns exactly `1` hit; `Select-String -SimpleMatch -Pattern "evidence/regression-testing/r-p2-t3/p2-t3.trx"`
  on the file returns at least `1` hit. Artifact:
  `<FEATURE>/evidence/other/r2-p2-t5-addendum-verified.<ts>.md` with the four schema fields.

### Phase 3 — RC-3: Timestamp-Accuracy Documentation Note

- [x] [P3-T1] Append a new `## Remediation-Cycle Timestamp Accuracy Note — <ts>` section to
  `delivery-report.2026-08-28T16-40.md` (append-only; do not edit or delete any existing line,
  including the existing "Post-Rebase Addendum" section from the prior remediation cycle), stating,
  verbatim on its own line:
  `- Note: the 2026-08-28T17-15 remediation cycle's evidence artifacts are self-stamped 2026-08-28T18-16 through 2026-08-28T20-12, which postdate both the commit that introduced them (c4e96b72) and the wall-clock time of the review that flagged this (2026-08-28T17-48).`
  followed by one explanatory sentence attributing this to a sandbox/session clock offset with no
  functional effect on evidence validity, ordering, or content, and stating that no committed artifact
  is renamed or rewritten to correct it. Acceptance:
  `git diff HEAD -- docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/delivery-report.2026-08-28T16-40.md | Select-String -Pattern '^-[^-]'`
  returns `0` matches; `Select-String -SimpleMatch -Pattern "Timestamp Accuracy Note"` on the file
  returns exactly `1` hit; `Select-String -SimpleMatch -Pattern "sandbox"` on the file returns at least
  `1` hit. Artifact: `<FEATURE>/evidence/other/r2-p3-t1-timestamp-note-verified.<ts>.md` with the four
  schema fields.

### Phase 4 — Full-Scope Host-Identity Sweep (Confirms No Other Leak Exists)

This phase covers the whole feature folder and the actual working-tree change set, not only the five
named files, because this feature has a documented history of sanitization fixes that turned out to be
too narrowly scoped. Preflight round 1 found four already-leaking review/memory artifacts that the
original two tasks of this phase did not sanitize; `P4-T2` sanitizes those four files before `P4-T3`'s
content sweep runs, so `P4-T3`'s own zero-hit acceptance condition holds against the real tree.

- [x] [P4-T1] Enumerate the sweep scope as the de-duplicated union of (a) every file under
  `docs/features/active/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand-680/` via
  `Get-ChildItem -Recurse -File`, and (b) every path reported by `git status --porcelain` at this
  moment (directory entries, if any, expanded to their contained files). Acceptance: the union count is
  recorded as `R2_SWEEP_SCOPE_COUNT` and is strictly greater than `0`; the full path list is persisted
  in the artifact. Artifact: `<FEATURE>/evidence/qa-gates/r2-p4-t1-sweep-scope.<ts>.md` with the four
  schema fields.
- [x] [P4-T2] Sanitize four review/memory artifacts that quoted the R2 leak verbatim while reporting on
  it — found by preflight round 1, not by the five-file list Phase 1-2 already cover:
  `code-review.2026-08-28T17-48.md`, `policy-audit.2026-08-28T17-48.md`, and
  `remediation-inputs.2026-08-28T17-48.md` (all three under `<FEATURE>/`), plus
  `.claude/agent-memory/feature-review/project_680-review-residuals.md`. For each of the four files: (a)
  record the before combined hit count via `rg -a -i -F -c -- '<literal>' <path>` for each of the three
  D1 literal classes; (b) replace every case-insensitive occurrence of the `$env:USERNAME` and
  `$env:COMPUTERNAME` literal values with the raw (not XML-escaped) placeholder spellings `<user>` and
  `<host>` — these are Markdown files, not TRX, so D1's XML-escaping requirement does not apply; (c)
  re-run the same sweep and record the after combined hit count. Acceptance: for every one of the four
  files, the before combined hit count is strictly greater than `0` (the leak is real — the D8 positive
  control) and the after combined hit count is exactly `0`. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p4-t2-review-artifact-sanitize.<ts>.md` with the four schema fields and
  a before/after hit-count table for all four files.
- [x] [P4-T3] Content sweep (runs after `P4-T2` sanitizes the four review/memory artifacts it found): for
  every file in the P4-T1 union, run `rg -a -i -F -- '<literal>' <path>` for each of the three D1 literal
  classes (worktree-root prefix, `$env:USERNAME` value, `$env:COMPUTERNAME` value). Acceptance: the
  combined hit count for each of the three literal classes is exactly `0` across the entire union
  (`R2_WIDE_REPOROOT_HITS` = `R2_WIDE_USER_HITS` = `R2_WIDE_HOST_HITS` = `0`); `0` files produced a read
  error. Artifact: `<FEATURE>/evidence/qa-gates/r2-p4-t3-content-sweep.<ts>.md` with the four schema
  fields.
- [x] [P4-T4] Path-name sweep: for every file's repo-relative path in the P4-T1 union, case-insensitively
  test for the `$env:USERNAME` and `$env:COMPUTERNAME` literal values (the worktree-root prefix is not
  part of any committed repo-relative path by construction and is excluded from this check, matching
  the reasoning already recorded in `evidence/other/trx-sanitisation.2026-08-28T16-45.md`). Acceptance:
  combined path-name hit count is `0`. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p4-t4-pathname-sweep.<ts>.md` with the four schema fields.

### Phase 5 — Final QA Loop (format → analyzer → nullable → pinned suites → full suite)

Run this loop in order. If any step fails or the format step changes any tracked file, restart the
entire Phase 5 loop from P5-T1; a restarted pass does not count as final.

- [x] [P5-T1] Format gate (verify-only; per D7 this plan makes no `.cs`/`.xml`/`packages.config` change,
  so no `format` write step is required — only the read-only check): `dotnet tool run csharpier check .`.
  Acceptance: `EXIT_CODE: 0`. Artifact: `<FEATURE>/evidence/qa-gates/r2-p5-t1-format-check.<ts>.md`
  with the four schema fields.
- [x] [P5-T2] Analyzer rebuild:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
  Acceptance: `EXIT_CODE: 0`. Artifact: `<FEATURE>/evidence/qa-gates/r2-p5-t2-analyzers.<ts>.md` with
  the four schema fields.
- [x] [P5-T3] Nullable/type-check rebuild:
  `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`.
  Acceptance: `EXIT_CODE: 0`. Artifact: `<FEATURE>/evidence/qa-gates/r2-p5-t3-nullable.<ts>.md` with the
  four schema fields.
- [x] [P5-T4] Pinned-suite green run (spec AC-5), using D3's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~QfcItemController_SearchFocusRegressionTests|FullyQualifiedName~BreadcrumbDropDownSearchIntegrationTests|FullyQualifiedName~BreadcrumbDropDownOpenCoordinatorTests|FullyQualifiedName~BreadcrumbItemViewerLifecycleCoordinatorTests|FullyQualifiedName~ItemViewerBreadcrumbDropDownContractTests" /Logger:"trx;LogFileName=r2-p5-t4.trx" "/ResultsDirectory:<FEATURE>/evidence/qa-gates/r2-p5-t4"`.
  Acceptance: `EXIT_CODE: 0`; total equals `75` and failed equals `0` (matching the total recorded in
  `evidence/qa-gates/p4-t2-pinned-suites.2026-08-28T16-07.md`); the `r2-p5-t4` subdirectory holds
  exactly one file, named exactly `r2-p5-t4.trx`. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p5-t4-pinned-suites.<ts>.md` with the four schema fields.
- [x] [P5-T5] Full `QuickFiler.Test` assembly run, using D3's resolution:
  `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" /Logger:"trx;LogFileName=r2-p5-t5.trx" "/ResultsDirectory:<FEATURE>/evidence/qa-gates/r2-p5-t5"`.
  Acceptance: `EXIT_CODE: 0`; total equals `1237` and failed equals `0` (matching the total recorded in
  `evidence/qa-gates/p4-t4-qft-full-run.2026-08-28T19-50.md`); the `r2-p5-t5` subdirectory holds exactly
  one file, named exactly `r2-p5-t5.trx`. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p5-t5-full-suite.<ts>.md` with the four schema fields.
- [x] [P5-T6] Commit-readiness check (no `git commit` is performed by this task): `git status
  --porcelain`. Acceptance: every listed entry is one of: the five originally-leaking TRX paths (or
  their containing `p0-t6/`, `p0-t7/`, `p1-t3/`, `p2-t3/`, `p4-t4/` directories),
  `evidence/regression-testing/r-p2-t3/p2-t3.trx`,
  `evidence/regression-testing/p2-t3-new-test-green.2026-08-28T19-27.md`,
  `delivery-report.2026-08-28T16-40.md`, the four `P4-T2` review/memory sanitization targets
  (`code-review.2026-08-28T17-48.md`, `policy-audit.2026-08-28T17-48.md`,
  `remediation-inputs.2026-08-28T17-48.md`, and
  `.claude/agent-memory/feature-review/project_680-review-residuals.md`), this remediation plan file
  itself, or a path under `<FEATURE>/evidence/` created by this plan's own tasks (all
  `r2-p#-t#-*.<ts>.md` artifacts); zero entries match none of these; zero entries have a `.cs`,
  `.csproj`, `.props`, or `.targets` extension. Artifact:
  `<FEATURE>/evidence/qa-gates/r2-p5-t6-commit-readiness.<ts>.md` with the four schema fields.

## Structural Self-Check (validator not run)

- Phase headings match `### Phase N — <Title>` exactly. Task IDs are `[P#-T#]`, sequential within each
  phase, all starting `- [ ]`, no phase-number gaps (0–5).
- Every task names explicit file paths. Every acceptance condition is a real, executable check with a
  measurable pass/fail outcome (blob-hash equality, exact match/hit counts, exception/no-exception on a
  real XML parse, append-only diff plus exact-phrase presence for text this plan's own tasks create).
- Every claimed "zero hits" condition is paired with a positive control per D8: Phase 0 establishes a
  nonzero before-state for the five leaking files and a `0` `outcome="Failed"` count for the
  not-yet-restored `p2-t3.trx`; Phase 1/2/4 then assert `0` after remediation, Phase 2 additionally
  asserts the restored file's `outcome="Failed"` count becomes `3`, strictly greater than its own
  before-value, and Phase 4's `P4-T2` records its own before/after hit-count pair for the four
  review-artifact files preflight round 1 found leaking.
- No absolute host path, account name, or machine name is hardcoded anywhere in this plan's own text;
  the three literal values are always resolved dynamically per D1 at task-execution time.
- Every `git diff` acceptance condition in this plan supplies a `HEAD` ref operand (never left
  unanchored) and is paired with a `Select-String` companion reading the same diff's own output, not a
  bare `--name-only`/`--name-status` form, so no separate staging/porcelain companion is required.
- No task hardcodes a specific commit hash as an expected `HEAD` value; `72b4b7ed` is used only as a
  restore *source*, and its existence and the target path's presence at that commit are verified by
  P2-T1 before any restore is attempted.
- Coverage evidence: per D7, this plan intentionally omits a full-repo coverage re-run because the
  change set contains zero production/test source lines (verified by Phase 4 and P5-T6), so no coverage
  delta is possible; the most recent coverage evidence
  (`evidence/qa-gates/p4-t5-coverage-final.2026-08-28T20-05.md`) remains the current, valid figure for
  this branch state. This matches the calling instruction's explicit four-part toolchain for this pass.
- No `git commit` task is present, per the calling agent's standing instruction (D5).
- `EVIDENCE_LOCATION_OVERRIDE_REJECTED`: not applicable — no non-canonical evidence path was supplied;
  all evidence paths in this plan resolve under `<FEATURE>/evidence/<kind>/` using only canonical
  sub-paths (`remediation-baseline/`, `regression-testing/`, `other/`, `qa-gates/`).

**PREFLIGHT: VALIDATOR NOT RUN** — the `mcp__drm-copilot__validate_orchestration_artifacts` tool is not
present in this planning session's tool surface. The structural self-check above is a substitute
review, not a validator pass, and must not be reported as `PREFLIGHT: ALL CLEAR`.
