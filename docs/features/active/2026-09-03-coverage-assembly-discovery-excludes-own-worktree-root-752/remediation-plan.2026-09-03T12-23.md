# Remediation Plan — Issue #752, finding R-1 (from POL-2)

- Timestamp: 2026-09-03T12-23
- Feature folder: `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/`
- Remediation input (authoritative requirements source for this loop):
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md`
- Blocking findings addressed: 1 (R-1)
- Work Mode: `full-bug`, resolved from `issue.md:12` (`- Work Mode: full-bug`) under the
  mode-source-precedence rule in `atomic-plan-contract`. The caller's
  `DIRECTIVE: MINIMAL-AUDIT PLAN REQUIRED` is honoured as a **plan-size** directive (three phases,
  bounded write set), not as a mode reclassification: `issue.md` carries no `## Acceptance Criteria`
  section, so a `minor-audit` plan would fail closed, and the requirements source for a remediation
  loop is the remediation-inputs document rather than `issue.md`.
- Evidence root: `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/`
  (`remediation-baseline/`, `qa-gates/`, `other/`). No `artifacts/` path is used for evidence.

## Path and identifier hygiene rules that bind this plan and every artifact it produces

1. No file this plan creates or edits may contain an absolute host path or a host identifier
   (`.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 8-13). This plan file itself is
   inside the branch diff and is therefore in scope for its own verification sweep.
2. No artifact may quote a removed value. Every sanitisation record describes each substituted token
   **by class only** — "account-name token", "Windows user-profile path prefix", "POSIX user-profile
   path segment", "worktree-parent directory-name token" — and keeps only `AFTER:` text
   (same memory file, lines 88-92, the bullet beginning "A sanitisation record must not quote").
   That citation is verified against this execution worktree's copy of
   `.claude/agent-memory/_shared_no_absolute_host_paths.md`, which is 92 lines long and carries the
   bullet at line 88. It is the same citation `remediation-inputs.2026-09-03T12-23.md` gives at its
   required-remediation step 2 (input line 53). Copies of this memory file in other checkouts on the
   same host differ in length, so any line citation into it must be re-derived against this worktree
   and never carried over from another worktree, from memory, or from a prior revision round.
3. Every path operand in this plan is written repo-relative, or as `<repo-root>/...`. The executor
   resolves `<repo-root>` at run time; it must not be written into any committed file.
4. The five sweep tokens are **never spelled literally** in this plan, in the sweep helper, or in any
   evidence artifact. They are derived at run time from `$env:USERPROFILE`, from the worktree's
   parent directory name, and by character-code composition. Spelling them would make a zero-hit
   gate unsatisfiable against the plan's own text, and this file is covered by `[P2-T3]`'s File-mode
   scan, by `[P2-T5]`'s step-3 `Index` sweep (the plan file is staged at step 2), and by `[P2-T5]`'s
   step-8 `Diff` sweep once step 7 has committed it.
5. Two distinct placeholders are in play and they are **not** interchangeable
   (`.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 17-26). Use `<repo-root>` only when
   the value being replaced is genuinely this repository's own checkout root, or a path rooted at it.
   Use `<user-profile>` when the value is rooted at an operator user-profile directory that is **not**
   this repository — for example a path naming a different checkout's agent worktree. Choosing the
   placeholder per case is mandatory; applying one placeholder uniformly mislabels the value.

## Why this plan is larger than the single mandated line edit

The remediation input's acceptance is "the branch-scoped added-line sweep returns zero matches" for
four patterns. The line-5 edit alone cannot satisfy that, because the audit's own verification
command never measured two of the four patterns. `policy-audit.2026-09-03T12-23.md:185` records the
command as a double-quoted extended-regular-expression alternation. The shell reduces the
doubled backslash in the Windows-prefix alternative to a single backslash before `grep` sees it, and
a lone backslash before an ordinary character is not a defined ERE escape, so that alternative
cannot match the intended text; the POSIX-segment alternative cannot match a backslash-separated
Windows path either. Only the account-name and worktree-parent alternatives were live. That is why
the audit reported exactly one match, and it is a sound derivation that **the account-name token and
the worktree-parent token appear on exactly one added line in the committed branch diff** — the one
the input names.

The residual set for the two path-prefix patterns was therefore never enumerated. Phase 0 enumerates
it. Phase 1 closes it. The table below was derived at planning time by a case-insensitive sweep of
the **committed branch diff** `<merge-base>..HEAD`, restricted to **added lines only** — the same
scope the remediation input's acceptance uses. It is not a read of the whole worktree. On that scope
it finds token-carrying added lines in **four** markdown files under `docs/features/`, at **six**
positions. All four files enter the branch diff with status `A`, so an added-line position is
identical to that file's current worktree line number, which is why the `Line` column below is
directly usable against the worktree:

| File (repo-relative) | Line | Token class present | Placeholder that applies |
|---|---|---|---|
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md` | 5 | account-name, worktree-parent, Windows user-profile prefix | `<repo-root>` (the value is this checkout's own root) |
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md` | 19 | Windows user-profile prefix | `<user-profile>` |
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` | 22 | Windows user-profile prefix | `<user-profile>` |
| `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md` | 42 | Windows user-profile prefix | `<user-profile>` |
| `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` | 20 | Windows user-profile prefix | `<user-profile>` |
| `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md` | 40 | Windows user-profile prefix | `<user-profile>` |

The promoted file is the pre-promotion copy of `issue.md`; its two matching lines were read at
planning time and are byte-identical to `issue.md` lines 22 and 42 respectively.

Two limits on this table are load-bearing. First, it is a **planning-time** derivation, and `[P0-T4]`
re-derives the same scope at execution time against the merge base resolved in `[P0-T3]`; `[P0-T4]`'s
enumeration — not this table — is what `[P1-T2]` consumes. The table exists so that a divergence
between it and the enumeration is visible and must be recorded rather than absorbed. Second, the
diff-and-added-lines scope is what makes the result four files rather than a
repository-scale number. A whole-tree case-insensitive read of `docs/features/` for the same five
token classes is more than two orders of magnitude larger: measured at preflight against the
current branch tip, by counting distinct markdown files under that tree carrying any of the five
token classes, it matched **956** files. Essentially all of that residue is pre-existing on
`main`, sits in other feature folders, and is out of bounds for this remediation; the memory
record's own recurrence section already tracks it as separate work. No acceptance condition in
this plan is stated against a whole-tree count, and the 956 figure is recorded here as a scale
observation only — nothing in Phase 0, 1, or 2 measures it or gates on it. Because it describes a
scope this plan does not act on, later drift in that figure is not a plan defect.

The four `*.2026-09-03T12-23.md` audit artifacts of this loop (policy-audit, code-review,
feature-audit, remediation-inputs) sit in the feature folder. A Diff-mode enumeration cannot observe
a file that is not committed, so if any of them is untracked it will not appear in `[P0-T4]`'s
output at all. `[P2-T5]` therefore scans all four directly in File mode and discloses the result,
so that a later commit cannot stage one of them un-sanitised without the disclosure having been made.
A planning-time worktree read of those four files — a file read, deliberately outside the diff scope
used for the table above — shows that `remediation-inputs.2026-09-03T12-23.md` and
`policy-audit.2026-09-03T12-23.md` each carry token-bearing lines, while
`code-review.2026-09-03T12-23.md` and `feature-audit.2026-09-03T12-23.md` carry none. That is a
disclosure expectation for `[P2-T5]`, not an acceptance value: `[P2-T5]`'s disclosure is explicitly
not a failure gate, so whatever counts those four invocations report, they do not block the task.

## Scope boundary

In bounds:
`docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`
line 5; **any markdown file under `docs/features/` that is present in the branch diff and that the
Phase 0 enumeration proves carries an added-line token match**; this plan file; new evidence
artifacts under the feature folder's `evidence/` tree; one gitignored throwaway sweep helper.

Out of bounds, and asserted unmodified in `[P1-T3]` and `[P2-T4]`:
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`; `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`;
`tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`; every acceptance-criteria checkbox line in
`spec.md` (all six must remain checked); every file outside `docs/features/`.

One narrow carve-out applies to that last clause, and it is not a widening of this plan's write set.
`.claude/agent-memory/` is a tracked path in this repository (`.gitignore:351` keeps `.claude/`
tracked, ignoring only `.claude/settings.local.json` and `.claude/state/`), and the agent executing
this plan carries a standing, plan-independent obligation to persist memory files there during its
run. Those writes are not this plan's writes and are never staged by it: no task here creates,
edits, or commits a file under `.claude/agent-memory/`, and the explicit pathspec in `[P2-T2]`
excludes the path. They are excluded from the out-of-bounds assertion solely so that the executor's
own mandated behaviour cannot force a false BLOCKED in `[P1-T3]`, `[P2-T1]`, or `[P2-T5]`. Every task
that inspects porcelain lists them by path and treats them as pre-existing-equivalent tree state.

No non-markdown file is rewritten, and no markdown file outside `docs/features/` is rewritten. If the
Phase 0 enumeration reports any matching path that is not a `.md` file under `docs/features/`, the
executor stops and reports BLOCKED at `[P0-T4]` rather than proceeding: this plan's write set cannot
repair such a file, and the terminal zero-match gate in `[P2-T5]` would then be unsatisfiable.
Rewriting a committed Cobertura/JaCoCo XML is specifically not an option, because it would invalidate
the coverage evidence the six verified acceptance criteria rest on.

## Acceptance criteria for this remediation

- AC-R1. `research/research-findings.2026-09-03T00-00.md` line 5 carries the `<repo-root>` placeholder
  in place of the drive-rooted value, the rest of the line is unchanged, the file's line count is
  unchanged at **184**, and a file-scoped scan of that file for all five tokens returns zero. The
  line count is measured as `@(Get-Content -LiteralPath <path>).Count`. It must **not** be derived by
  splitting the raw file text on a newline character: the file is LF-terminated, so a raw split
  produces a trailing empty element and reports 185, which is one greater than the true count.
- AC-R2. No artifact this remediation produces quotes a removed value; every sanitisation record
  describes substituted tokens by class only.
- AC-R3. The branch-scoped added-line sweep reports `COUNT: 0` for the account-name token and
  `COUNT: 0` for the worktree-parent directory-name token. AC-R3 is **measured** by the `[P2-T5]`
  step-3 `Index` sweep, which compares the merge base against the staged index — the tree the
  step-7 commit creates — and that result is recorded in the committed artifact
  `evidence/qa-gates/r1-final-gate.2026-09-03T12-23.md`. The `[P2-T5]` step-8 `Diff` sweep over
  `<merge-base>..HEAD` is a **confirming** re-run; disagreement between the two is a BLOCKED
  outcome.
- AC-R4. The same sweep reports zero added-line matches, for all five tokens, in every markdown file
  under `docs/features/`, and reports no matching path of any other kind — that is, `TOTAL: 0` with no
  `MATCHFILE:` line, in both the `[P2-T5]` step-3 and step-8 runs, with step 3 recorded in
  `evidence/qa-gates/r1-final-gate.2026-09-03T12-23.md`.
- AC-R5. The out-of-bounds files listed above are absent from this remediation's changed-file set, and
  `spec.md` still carries exactly six checked acceptance-criteria lines.
- AC-R6. The squash-merge requirement is recorded in a committed evidence artifact, and this
  remediation's changed-file set is markdown-only under `docs/`, which is the recorded basis for
  classifying every language toolchain gate NOT APPLICABLE. "Changed-file set" here means exactly the
  paths this plan stages and commits in `[P2-T2]`, `[P2-T4]`, and `[P2-T5]` step 7. It excludes any
  unstaged `.claude/agent-memory/` entry, which is the executing agent's standing memory-persistence
  write rather than a change this remediation makes, and which `[P2-T1]` records separately under
  `AGENT_MEMORY_WRITES:`.
- AC-R7. No `spec.md` acceptance-criteria checkbox is modified by this remediation. This is a
  `full-bug` item (`issue.md:12`), so `spec.md` is the acceptance-criteria source; its six criteria
  at `spec.md:104-109` are already checked and verified, and this loop neither adds, removes, nor
  re-checks any of them. The execution summary must report an AC Status Summary reading
  `6 of 6 acceptance criteria checked (unchanged by this remediation)`.

### Phase 0 — Baseline capture

- [x] [P0-T1] Read, in the order given, `CLAUDE.md`, `.claude/rules/general-code-change.md`,
  `.claude/rules/general-unit-test.md`, `.claude/rules/quality-tiers.md`, `.claude/rules/tonality.md`,
  `.claude/rules/powershell.md`, `.claude/agent-memory/_shared_no_absolute_host_paths.md`, and
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-inputs.2026-09-03T12-23.md`.
  Write `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md`
  containing `Timestamp:`, `Policy Order:`, and one bullet per file read.
  Acceptance: the artifact exists and lists all eight paths.

- [x] [P0-T2] Create the gitignored throwaway sweep helper at `<repo-root>/coverage/r1-host-path-sweep.ps1`
  with exactly the content below. The `coverage/` directory already exists and `.gitignore` line 144
  (`coverage/*`, with only `!coverage/.gitkeep` re-included) makes any new file in it ignored. The helper
  derives the worktree root from `$PSScriptRoot`, so it is independent of the process working directory
  and no absolute path is passed to it, and it composes the five tokens at run time so that none is
  spelled literally. It prints only paths, token class names, line numbers, and counts — never matched text.

  **Invocation warning, binding on every task in this plan that runs the helper:** `pwsh -File`
  passes each argument through as a plain string and does not bind several space-separated operands
  into one array parameter. Passing more than one path in a single `-Path` operand list therefore
  scans only the first and silently discards the rest, with no binding error and no warning — a
  zero-hit result on an unscanned file reads identically to a clean file. To make that failure mode
  structurally impossible, `-Path` below is declared as a single `[string]`, File mode throws when it
  is empty, and **every File-mode task in this plan issues one invocation per path**.

  ```powershell
  [CmdletBinding()]
  param(
      [Parameter(Mandatory = $true)][ValidateSet('Diff', 'Index', 'File')][string] $Mode,
      [string] $BaseSha = '',
      [string] $Path = ''
  )
  Set-StrictMode -Version Latest
  $ErrorActionPreference = 'Stop'
  $worktree = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
  $bs = [string][char]92
  $fs = [string][char]47
  $profileRoot = $env:USERPROFILE
  if ([string]::IsNullOrWhiteSpace($profileRoot)) { throw 'USERPROFILE is not set; token derivation would silently disable a gate.' }
  $parentPath = Split-Path -Parent $worktree
  if ([string]::IsNullOrWhiteSpace($parentPath)) { throw 'Worktree has no parent directory; token derivation would silently disable a gate.' }
  $tokens = [ordered]@{
      account      = (Split-Path -Leaf $profileRoot)
      parentdir    = (Split-Path -Leaf $parentPath)
      winprofile   = 'C:' + $bs + 'Users'
      winprofilefs = 'C:' + $fs + 'Users'
      posixprofile = $fs + 'Users' + $fs
  }
  foreach ($key in $tokens.Keys) {
      if ([string]::IsNullOrWhiteSpace([string]$tokens[$key])) { throw ('Token ' + $key + ' derived empty.') }
  }
  function Get-TokenHit {
      param([string] $Text)
      $found = @()
      $lower = $Text.ToLowerInvariant()
      foreach ($key in $tokens.Keys) {
          if ($lower.Contains(([string]$tokens[$key]).ToLowerInvariant())) { $found += $key }
      }
      return $found
  }
  if ($Mode -eq 'File') {
      if ([string]::IsNullOrWhiteSpace($Path)) { throw 'File mode requires exactly one -Path value.' }
      $count = 0
      $number = 0
      foreach ($line in (Get-Content -LiteralPath (Join-Path $worktree $Path))) {
          $number++
          $hit = @(Get-TokenHit -Text $line)
          if ($hit.Count -gt 0) {
              Write-Output ('FILEMATCH: ' + $Path + ' | LINE: ' + $number + ' | TOKENS: ' + ($hit -join ','))
              $count++
          }
      }
      Write-Output ('FILECOUNT: ' + $Path + ' | COUNT: ' + $count)
      Write-Output 'TOTAL: DONE'
      exit 0
  }
  $gitArgs = if ($Mode -eq 'Index') { @('diff', '--cached', $BaseSha) } else { @('diff', $BaseSha, 'HEAD') }
  $current = '(unknown)'
  $perFile = [ordered]@{}
  $perToken = [ordered]@{}
  foreach ($key in $tokens.Keys) { $perToken[$key] = 0 }
  foreach ($line in (& git -C $worktree @gitArgs)) {
      if ($line -like '+++ *') {
          $current = $line.Substring(4)
          if ($current.StartsWith('b' + $fs)) { $current = $current.Substring(2) }
          continue
      }
      if (-not $line.StartsWith('+')) { continue }
      $hit = @(Get-TokenHit -Text $line)
      if ($hit.Count -eq 0) { continue }
      if (-not $perFile.Contains($current)) { $perFile[$current] = 0 }
      $perFile[$current] = $perFile[$current] + 1
      foreach ($key in $hit) { $perToken[$key] = $perToken[$key] + 1 }
  }
  $total = 0
  foreach ($key in $perFile.Keys) {
      Write-Output ('MATCHFILE: ' + $key + ' | COUNT: ' + $perFile[$key])
      $total = $total + $perFile[$key]
  }
  foreach ($key in $perToken.Keys) { Write-Output ('TOKENCOUNT: ' + $key + ' | COUNT: ' + $perToken[$key]) }
  Write-Output ('TOTAL: ' + $total)
  exit 0
  ```

  Then run these three commands, in order:
  `git -C <repo-root> check-ignore -q coverage/r1-host-path-sweep.ps1`,
  `git -C <repo-root> status --porcelain -uall -- coverage`, and
  `git -C <repo-root> status --porcelain -uall` (whole tree).
  Write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:` (all three commands, with the repo root written as `<repo-root>`),
  `EXIT_CODE:` (one per command), and an `Output Summary:` recording that the helper file now exists
  at the stated repo-relative path, the exit code of `check-ignore`, the literal text the second
  command printed (recorded as `PORCELAIN_COVERAGE: <empty>` when it printed nothing), and the
  complete verbatim output of the third command under the label `PORCELAIN_BASELINE:` (recorded as
  `PORCELAIN_BASELINE: <empty>` when it printed nothing).
  `PORCELAIN_BASELINE:` is the pre-remediation working-tree state and is consumed by `[P1-T3]`,
  `[P2-T1]`, and `[P2-T5]`. Its purpose is to make "entry this plan created" mechanically derivable
  by set difference rather than by executor judgement, so that an untracked file that already existed
  outside `docs/features/` before this plan ran cannot be mistaken for a scope-lock violation.
  Acceptance, all four required: the artifact exists and carries all four schema fields;
  `check-ignore` exits 0 (the helper is ignored and can never enter the diff); the recorded
  `PORCELAIN_COVERAGE:` value is `<empty>`; and a `PORCELAIN_BASELINE:` label is present, carrying
  either the verbatim third-command output or the literal `<empty>`.

- [x] [P0-T3] Resolve the merge base without fetching: run
  `git -C <repo-root> merge-base origin/main HEAD` and `git -C <repo-root> rev-parse HEAD`.
  Write `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` recording the merge-base SHA
  as `MERGE_BASE:` and the pre-remediation head SHA as `PRE_REMEDIATION_HEAD:`.
  Acceptance: both commands exit 0, and both SHAs are 40 hexadecimal characters. Neither SHA is
  compared against a literal pinned in this plan; both are recorded for later reuse. Commit SHAs are
  not host identifiers and may be recorded verbatim.

- [x] [P0-T4] Run `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Diff -BaseSha <MERGE_BASE>`
  and write its full stdout to
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:` (with the repo root written as `<repo-root>`), `EXIT_CODE:`, and an
  `Output Summary:` giving `TOTAL:`, every `TOKENCOUNT:` line, and every `MATCHFILE:` line.
  A successful run always prints a `TOTAL:` line; absence of that line is a failed run, not a zero result.
  Acceptance, all five required: `EXIT_CODE: 0`; the output contains a `TOTAL:` line; `TOKENCOUNT: account`
  is at least 1 and `TOKENCOUNT: parentdir` is at least 1, and the `MATCHFILE:` list includes
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`
  (this is the fail-before proof for R-1); **every** `MATCHFILE:` path ends in `.md` and begins with
  `docs/features/` — if any does not, stop and report BLOCKED, because such a file is outside this
  plan's write set and `[P2-T5]`'s terminal gate could not then be satisfied; and the `MATCHFILE:` list
  is recorded verbatim as the enumeration that drives `[P1-T2]`. A lower bound is used for the two
  `TOKENCOUNT:` values rather than an equality because the audit's command measured only two of its four
  alternatives, and because any commit made on this branch after the audit ran can have added further
  matching lines; the enumeration, not a pinned count, is what `[P1-T2]` consumes. Record explicitly, as
  `TABLE_RECONCILIATION:`, whether the enumeration agrees with the six-position planning-time table in
  "Why this plan is larger"; any divergence is recorded, and the enumeration governs.
  This artifact records only paths, class names, and counts, so it does not quote a removed value.

- [x] [P0-T5] Record the line-5 baseline **by class only**, without quoting it. Read
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`
  and write `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` containing: the total line count
  of the file, measured as `@(Get-Content -LiteralPath <path>).Count` and recorded as `LINE_COUNT:`;
  the fact that line 5 begins with the literal prefix `- Worktree: `; the SHA-256 of the raw
  bytes of line 5 as `LINE5_SHA256:`; and the per-token hit classes for line 5 taken from a single
  File-mode invocation,
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`.
  Acceptance: the recorded `LINE_COUNT:` is **184**; exactly one `FILECOUNT:` line is printed by that
  invocation and it reports `COUNT: 1`; the single `FILEMATCH:` line reports `LINE: 5`; `LINE5_SHA256:`
  is a 64-character hexadecimal digest; and the artifact contains no reproduction of the line's value.
  Do not obtain the line count by splitting the raw file text on a newline character — the file is
  LF-terminated and that method reports 185. A cryptographic digest is a one-way transform and is not
  a quoted value under the no-quoting rule.

### Phase 1 — Sanitisation

- [x] [P1-T1] In
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`,
  replace the backtick-quoted value that sits between the literal prefix `- Worktree: ` and the literal
  segment ` (branch ` on line 5 with the **repository-root** placeholder — the value being replaced is
  this checkout's own root, which is the one case where `<repo-root>` is the correct placeholder under
  hygiene rule 5 — leaving the rest of the line byte-identical.
  The resulting line 5 must read exactly:
  `- Worktree: ` then a backtick, then the repository-root placeholder token written with its angle
  brackets, then a backtick, then ` (branch ` then a backtick, then
  `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`, then a backtick, then `)`.
  Acceptance, all five required: the file still has **184** lines, measured as
  `@(Get-Content -LiteralPath <path>).Count` and not by splitting the raw text on a newline; line 5
  starts with `- Worktree: `; line 5 contains the placeholder-free substring `repo-root`; line 5
  contains the substring `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`; and the
  single File-mode invocation
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/research/research-findings.2026-09-03T00-00.md`
  prints exactly one `FILECOUNT:` line reading `COUNT: 0` and no `FILEMATCH:` line.
  Do not record the replaced value anywhere.

- [x] [P1-T2] Close the enumerated residual set. Take the `MATCHFILE:` list recorded in `[P0-T4]` and select
  every entry that is a `.md` file under `docs/features/`, excluding the research file already handled in
  `[P1-T1]`. The selection rule is deliberately the whole `docs/features/` tree and not the feature folder
  alone, because the pre-promotion copy of `issue.md` lives under `docs/features/potential/promoted/` and
  carries the same lines. For each selected file, run the helper in `File` mode — **one invocation per
  file, never several paths in one invocation** — to get its matching line numbers, and on each matching
  line apply exactly one of these two substitutions and nothing else.

  (a) Where the matched token is part of a path, replace the rooted prefix with the placeholder that
  matches the value's actual role, per hygiene rule 5 and
  `.claude/agent-memory/_shared_no_absolute_host_paths.md` lines 17-26, and compose the remainder of the
  path from placeholders as that table prescribes. Select per case, not uniformly: use the
  user-profile placeholder when the path names a directory rooted at an operator user profile that is
  not this repository — which is the case for every path in this plan's expected-members list below,
  since each of them names a **different** checkout's agent-worktree root belonging to item #735 — and
  use the repository-root placeholder only when the path is genuinely this repository's own root, which
  is the case only for the line already handled in `[P1-T1]`. Worked shape for the expected members: a
  value of the form "Windows user-profile prefix, then an account segment (already elided or already
  placeholdered), then `\repos\TaskMaster\.claude\worktrees\agent-...`" becomes the user-profile
  placeholder followed by `\repos\TaskMaster\.claude\worktrees\agent-...`, with the trailing portion
  byte-identical to what was there before.

  (b) Where the matched token appears as a search-pattern fragment or a quoted rule excerpt rather than
  as part of a path, replace the fragment with its class name (`the account-name token`,
  `the Windows user-profile path prefix`, `the POSIX user-profile path segment`, `the worktree-parent
  directory-name token`) so the surrounding sentence still reads correctly.

  Never modify a line that begins with `- [x] ` or `- [ ] `. If the `[P0-T4]` enumeration selects any of
  this loop's four audit artifacts — `remediation-inputs.2026-09-03T12-23.md`,
  `policy-audit.2026-09-03T12-23.md`, `code-review.2026-09-03T12-23.md`,
  `feature-audit.2026-09-03T12-23.md` — which happens only when that artifact is tracked and committed on
  this branch, then substitution (b) is the only permitted change to it: do not alter any finding,
  severity, disposition, or acceptance statement it carries. When the enumeration does not select them
  they are untracked, no change is made here, and `[P2-T5]` discloses them instead; record in the
  artifact which of the two branches applied, per file. Write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` listing, per file, the path, the line
  numbers changed, the class of each substituted token, and which placeholder was selected — no quoted values.

  Expected members, read from the current tree at planning time and to be reconciled against the
  `[P0-T4]` enumeration rather than assumed:
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md`
  line 19 (inside the quoted error snippet under `## Repro & Evidence`);
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/issue.md`
  line 22 (the `Command/flags used` bullet) and line 42 (the `Snippet:` bullet); and
  `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md`
  line 20 and line 40 (byte-identical twins of `issue.md` lines 22 and 42). If the enumeration selects a
  different set, follow the enumeration and record the difference in the artifact.
  Acceptance: for every selected file, a `File`-mode run afterwards prints exactly one `FILECOUNT:` line
  reading `COUNT: 0`; the artifact lists one entry per selected file; and if the selection is empty the
  artifact records `SELECTED: 0` and `RESULT: NOT APPLICABLE`, with the `[P0-T4]` enumeration cited as
  the basis.

- [x] [P1-T3] Assert the out-of-bounds set is untouched at this point. Run
  `git -C <repo-root> status --porcelain -uall` and
  `Select-String -Path <repo-root>/docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md -Pattern '^- \[x\] [1-6]\.'`.
  Acceptance, all three required: the porcelain output contains no entry for
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, none for
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1`, and none for
  `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`; the `Select-String` call returns exactly six
  matches; and every porcelain entry that is **absent from `PORCELAIN_BASELINE:`** (recorded in `[P0-T2]`)
  lies under `docs/features/` **or under `.claude/agent-memory/`**. Entries that are present in
  `PORCELAIN_BASELINE:` are pre-existing tree state that this plan did not create; list them in the
  execution notes and do not treat them as violations. The sweep helper cannot appear in that output
  because `[P0-T2]` proved it is ignored.

  The `.claude/agent-memory/` allowance is not a scope loophole and is required for the gate to be
  satisfiable. `.gitignore:351` records that `.claude/` is deliberately tracked so it materialises in
  git worktrees, and only `.claude/settings.local.json` and `.claude/state/` are ignored; therefore a
  file written under `.claude/agent-memory/` is a tracked-tree change and appears in porcelain. The
  agent executing this plan carries a standing, plan-independent obligation to persist memory files
  there during its run, and any such write necessarily happens after `PORCELAIN_BASELINE:` was
  captured in `[P0-T2]`, so it is absent from the baseline and is not under `docs/features/`. Without
  this allowance the executor's own mandated behaviour would force a false BLOCKED. Treat such
  entries as follows, in every task of this plan that inspects porcelain: list each one by path in
  the execution notes, never stage it (the explicit pathspec in `[P2-T2]` excludes it and `git add -A`
  remains prohibited), and do not count it as a scope-lock violation.

- [x] [P1-T4] Write the merge-mode record at
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/other/r1-squash-merge-note.2026-09-03T12-23.md`
  with `Timestamp:`, `Command: N/A (documentation record)`, `EXIT_CODE: 0`, and an `Output Summary:` stating:
  the pull request for this branch must be merged with a squash merge; the reason is that a sanitising commit
  removes the value from the branch tip while the pre-sanitisation blob remains reachable through an earlier
  commit on this branch, and squashing is what keeps the identifier out of the history of `main`; and that
  performing the merge is the orchestrator's or maintainer's action, not an executable step of this plan.
  Acceptance: the artifact exists, carries the three schema fields, states the squash requirement and its
  reason, and quotes no removed value.

- [x] [P1-T5] Verify sanitisation-record hygiene for every artifact written so far. Run the helper in `File`
  mode **once per path — seven separate invocations, each supplying exactly one `-Path` operand**, because
  `pwsh -File` does not bind several space-separated operands into one array parameter and would silently
  scan only the first. The seven repo-relative paths, all under
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/`, are:
  `evidence/remediation-baseline/phase0-instructions-read.2026-09-03T12-23.md`,
  `evidence/remediation-baseline/r1-sweep-helper-bootstrap.2026-09-03T12-23.md`,
  `evidence/remediation-baseline/r1-mergebase.2026-09-03T12-23.md`,
  `evidence/remediation-baseline/r1-sweep-baseline.2026-09-03T12-23.md`,
  `evidence/remediation-baseline/r1-line5-baseline.2026-09-03T12-23.md`,
  `evidence/other/r1-secondary-sanitisation.2026-09-03T12-23.md`, and
  `evidence/other/r1-squash-merge-note.2026-09-03T12-23.md`.
  Write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-artifact-hygiene.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:` (all seven invocations, with the repo root written as `<repo-root>`),
  `EXIT_CODE:` (one per invocation), and an `Output Summary:` reproducing the seven `FILECOUNT:` lines
  verbatim. This artifact records only paths and counts, so it does not quote a removed value.
  Acceptance, all four required: seven invocations are made; each prints exactly one `FILECOUNT:` line, so
  seven `FILECOUNT:` lines are produced in total; every one reports `COUNT: 0` and no `FILEMATCH:` line is
  printed by any invocation; and the artifact exists carrying all four schema fields and all seven
  `FILECOUNT:` lines. Any non-zero count means an artifact quoted a removed value; repair that artifact by
  replacing the quotation with a class description and re-run its invocation before proceeding.

### Phase 2 — Verification, classification, and commit

- [x] [P2-T1] Classify the change set. Run `git -C <repo-root> status --porcelain -uall` and write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-changed-file-class.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` listing every changed and added path
  and its file extension. The `-uall` flag is required and must not be dropped: with the default `-unormal`,
  git collapses a newly created untracked directory into a single entry naming the directory rather than its
  files, and `[P0-T1]` creates `evidence/remediation-baseline/`, which does not exist in the tree before this
  plan runs. A collapsed directory entry does not end in `.md` and would trip this task's own stop-and-report
  clause below even though every file inside it is markdown.
  Acceptance: every listed path that is **absent from `PORCELAIN_BASELINE:`** (recorded in `[P0-T2]`) ends in
  `.md` and either lies under `docs/` **or lies under `.claude/agent-memory/`**; consequently no `.ps1`,
  `.cs`, `.csproj`, `.props`, or `.targets` file is in this remediation's change set. Paths that are present
  in `PORCELAIN_BASELINE:` are pre-existing tree state that this plan did not create; record them verbatim
  in the artifact under the label `PREEXISTING:` and exclude them from the classification.
  Paths under `.claude/agent-memory/` are the executing agent's standing, plan-independent memory-persistence
  writes, described in full in `[P1-T3]`. Record them verbatim in the artifact under the label
  `AGENT_MEMORY_WRITES:`, exclude them from the classification, and do not stage them — the explicit pathspec
  in `[P2-T2]` excludes them and `git add -A` remains prohibited. They do not break the NOT APPLICABLE
  classification because they are markdown, which is not a coverage-bearing language. A path under
  `.claude/agent-memory/` that does **not** end in `.md` is a stop-and-report BLOCKED condition, because a
  source file there would give a language toolchain a non-empty input set and falsify the classification below.
  Record on that basis
  `TOOLCHAIN: NOT APPLICABLE — no source file in any coverage-bearing language is modified, so the PowerShell
  (PoshQC format, PoshQC analyze, Pester with coverage) and C# (csharpier, msbuild analyzers, msbuild nullable,
  vstest) gates have an empty input set and the Coverage Evidence Contract is not triggered.` If any path
  absent from `PORCELAIN_BASELINE:` is neither a `.md` file under `docs/` nor a `.md` file under
  `.claude/agent-memory/`, stop and report BLOCKED rather than recording NOT APPLICABLE.
  The sweep helper is not a counter-example to that classification, and its absence from the porcelain output
  is not the reason. It is a `.ps1` file, but it is (i) gitignored, so it can never enter the change set, and
  (ii) a temporary throwaway script created and deleted within this agent session, which is the first named
  exception in the File Size Limit section of `.claude/rules/general-code-change.md` (lines 47-50, exception at
  line 50). `[P2-T5]` performs the deletion that makes that exemption true. Record both grounds in the artifact
  as `HELPER_EXEMPTION:`, and base the NOT APPLICABLE classification on the rule exemption rather than on
  porcelain invisibility alone.

- [x] [P2-T2] Stage the remediation write set with an explicit pathspec list — the research file, every file
  changed by `[P1-T2]` (which may include
  `docs/features/potential/promoted/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root.md`,
  outside the feature folder), this plan file, and the feature folder's `evidence/` tree — using
  `git -C <repo-root> add -- <paths>`. Do not use `git add -A` or `git add .`, which would sweep in any
  untracked audit artifact of this loop and any memory file the executing agent wrote under
  `.claude/agent-memory/`. No path under `.claude/agent-memory/` may appear in the pathspec list, in this
  task or in `[P2-T4]` or `[P2-T5]`: those writes are the executing agent's standing obligation and are
  outside this remediation's change set. Then run
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Index -BaseSha <MERGE_BASE>`
  and write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:`, `EXIT_CODE:`, and an `Output Summary:` giving `TOTAL:`, every `TOKENCOUNT:`
  line, and every remaining `MATCHFILE:` line.
  Acceptance, all four required: the output contains a `TOTAL:` line; `TOKENCOUNT: account` is `COUNT: 0`;
  `TOKENCOUNT: parentdir` is `COUNT: 0`; and no remaining `MATCHFILE:` entry names a `.md` file under
  `docs/features/`. Any other `MATCHFILE:` entry is a stop-and-report BLOCKED condition, because `[P0-T4]`
  established that no such path exists in the enumeration.

- [x] [P2-T3] Scan the two files that carry this task's remaining File-mode hygiene obligation. The first is the
  sweep artifact just written, which is the only file whose content enters the `[P2-T4]` commit
  without having been present in the `[P2-T2]` staged snapshot. The second is this plan file, which
  `[P2-T2]` did stage and which is re-scanned here because `[P2-T5]` step 1 modifies it again after
  `[P2-T4]` commits it. First append a short
  `## Post-artifact scan` section to
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md`
  stating that the re-runs are performed in `[P2-T5]` — step 3 in `Index` mode, which compares the merge
  base against the staged index, and step 8 in `Diff` mode over `<MERGE_BASE>..HEAD` after the step-7
  commit — and that their input differs from the run recorded above only by this
  artifact, by the checkbox characters of the plan file, and — for step 8 only — by the final-gate artifact
  `[P2-T5]` step 5 writes, which `[P2-T5]` step 6 proves token-free before it is committed.
  Then make **two separate File-mode invocations, one path each** — passing both paths to a single
  invocation would scan only the first, silently, per the `[P0-T2]` invocation warning:
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md`
  and
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md`.
  Acceptance: two invocations are made; each prints exactly one `FILECOUNT:` line; both report `COUNT: 0`;
  and no `FILEMATCH:` line is printed by either invocation.

- [x] [P2-T4] Stage the sweep artifact from `[P2-T3]` with
  `git -C <repo-root> add -- docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-host-path-sweep.2026-09-03T12-23.md`
  and commit the remediation with
  `git -C <repo-root> commit -m "docs(752): sanitise absolute host path in research findings (R-1)"`.
  The commit message must contain none of the five tokens. Then run
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Diff -BaseSha <MERGE_BASE>`.
  Acceptance, all six required: the `git add` span above exits 0 and the artifact path it names appears in
  the `git -C <repo-root> diff --name-only <PRE_REMEDIATION_HEAD> HEAD` output below (the explicit staging
  span is what makes that name-listing diff able to observe a file this plan created); the commit exits 0;
  the sweep prints a `TOTAL:` line; `TOKENCOUNT: account` is `COUNT: 0`; `TOKENCOUNT: parentdir` is
  `COUNT: 0`; and no `MATCHFILE:` entry names a `.md` file under `docs/features/`. Additionally re-run the
  `[P1-T3]` assertions against `git -C <repo-root> diff --name-only <PRE_REMEDIATION_HEAD> HEAD`: none of the
  three out-of-bounds paths may appear, and
  `Select-String -Path <repo-root>/docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/spec.md -Pattern '^- \[x\] [1-6]\.'`
  must still return exactly six matches.

- [x] [P2-T5] Close out, in this exact nine-step order. The ordering is load-bearing: AC-R3 and AC-R4 are
  worded against this task's sweeps, so the sweep results must be committed rather than left only in an
  execution summary. Steps 1 to 7 end with a commit that carries both the completed plan file and the
  final-gate artifact; steps 8 and 9 are confirming gates that deliberately write no file.

  **Step 1.** Mark every checkbox in this plan, including this one — this task's remaining actions are
  verification-only, and if any assertion below fails the executor must unmark this box and report BLOCKED.

  **Step 2.** Stage the plan file and nothing else:
  `git -C <repo-root> add -- docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/remediation-plan.2026-09-03T12-23.md`.

  **Step 3.** Run the helper in `Index` mode:
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Index -BaseSha <MERGE_BASE>`.
  Because the plan file is staged, this run covers the tree of the commit that step 7 will create, minus
  only the artifact step 5 writes — which step 6 proves token-free.

  **Step 4.** Perform the untracked-artifact disclosure plus a whole-tree porcelain snapshot. Run the helper
  in `File` mode **once per path — four separate invocations, each supplying exactly one `-Path` operand** —
  over this loop's four audit artifacts, all under
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/`:
  `policy-audit.2026-09-03T12-23.md`, `code-review.2026-09-03T12-23.md`,
  `feature-audit.2026-09-03T12-23.md`, and `remediation-inputs.2026-09-03T12-23.md`. These files are outside
  this plan's write set except for any one of them that `[P0-T4]`'s enumeration selected for `[P1-T2]`, so
  this is a **disclosure, not a failure gate**: whatever counts they report, they do not block this task. A
  file `[P1-T2]` did sanitise will report `COUNT: 0` here; a file that is untracked may report any count.
  Then run `git -C <repo-root> status --porcelain -uall` (whole tree). This porcelain run is a recorded
  snapshot only; it will show the plan file staged from step 2, and the tracked-clean assertion is made at
  step 9, not here.

  **Step 5.** Write
  `docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-final-gate.2026-09-03T12-23.md`
  with `Timestamp:`, `Command:` (all six commands run in steps 3 and 4 — the Index-mode sweep, the four
  File-mode disclosure invocations, and the porcelain command — with the repo root written as `<repo-root>`),
  `EXIT_CODE:` (one per command, six values), and an `Output Summary:` recording: the step-3 Index-mode
  `TOTAL:` line and every `TOKENCOUNT:` line it printed; the four disclosure `FILECOUNT:` lines, each with
  the token classes named on any `FILEMATCH:` line stated **by class only and never by quoting the matched
  value**; and the step-4 porcelain result, verbatim, or the literal `<empty>` when it printed nothing.
  Entries under `.claude/agent-memory/` in that porcelain output are the executing agent's standing
  memory-persistence writes described in `[P1-T3]`: list them by path under the label `AGENT_MEMORY_WRITES:`,
  do not stage them, and do not treat them as a scope-lock violation.

  **Step 6.** Scan the artifact just written with one `File`-mode invocation:
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode File -Path docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-final-gate.2026-09-03T12-23.md`.
  It must print exactly one `FILECOUNT:` line reading `COUNT: 0` and no `FILEMATCH:` line. If it does not,
  repair the artifact by replacing the quotation with a class description and re-run this step before
  proceeding.

  **Step 7.** Stage the artifact with
  `git -C <repo-root> add -- docs/features/active/2026-09-03-coverage-assembly-discovery-excludes-own-worktree-root-752/evidence/qa-gates/r1-final-gate.2026-09-03T12-23.md`
  and commit the plan file and the artifact together with
  `git -C <repo-root> commit -m "docs(752): mark remediation plan tasks complete and record final gate"`.
  The commit message contains none of the five tokens. **Write no further file inside the feature folder
  after step 7** — such a file would be uncommitted content inside the feature folder and would reopen the
  gate step 8 closes. This prohibition does not extend to the executing agent's standing
  `.claude/agent-memory/` writes: those lie outside the feature folder, are never staged by this plan, and
  are explicitly tolerated by step 9's acceptance.

  **Step 8.** Run the confirming post-commit gate:
  `pwsh -NoProfile -File <repo-root>/coverage/r1-host-path-sweep.ps1 -Mode Diff -BaseSha <MERGE_BASE>`.
  Its input differs from step 3's only by the now-committed final-gate artifact, which step 6 proved
  token-free, so agreement between the two runs is the expected result and a disagreement is a BLOCKED
  outcome. This run is the **confirming** gate for AC-R3 and AC-R4; the measured values those
  criteria cite are the step-3 values recorded in the step-5 artifact.

  **Step 9.** Delete `<repo-root>/coverage/r1-host-path-sweep.ps1`, then run
  `git -C <repo-root> status --porcelain -uall`.

  Acceptance, all twelve required: the step-3 `Index` sweep prints a `TOTAL:` line reading `TOTAL: 0`, prints
  **no** `MATCHFILE:` line, and prints every `TOKENCOUNT:` line at `COUNT: 0`, including `TOKENCOUNT: account`
  and `TOKENCOUNT: parentdir`; four disclosure invocations were made in step 4 and four `FILECOUNT:` lines
  recorded; the step-6 scan prints exactly one `FILECOUNT:` line reading `COUNT: 0` and no `FILEMATCH:` line;
  the step-5 artifact exists at the stated path and carries all four schema fields (`Timestamp:`, `Command:`,
  `EXIT_CODE:`, `Output Summary:`); the step-7 `git add` and `git commit` each exit 0; the step-8 `Diff` sweep
  prints a `TOTAL:` line reading `TOTAL: 0`, prints **no** `MATCHFILE:` line, and prints every `TOKENCOUNT:`
  line at `COUNT: 0`, including `TOKENCOUNT: account` and `TOKENCOUNT: parentdir`; the helper file no longer
  exists after step 9; the step-9 porcelain output contains no modified, added, or deleted **tracked** entry
  under `docs/features/` — only `??` untracked entries under `docs/features/` are permitted, and each must be
  listed in the execution summary by path so the orchestrator can sanitise it under the same rule before any
  later commit stages it; and every step-9 porcelain entry outside `docs/features/` is either present in
  `PORCELAIN_BASELINE:` (recorded in `[P0-T2]`) or lies under `.claude/agent-memory/`, so this plan created
  none. There is no accepted-residual escape for the sweeps: a surviving `MATCHFILE:` line or a non-zero
  `TOTAL:` in step 3 or step 8 is a BLOCKED outcome, not a disclosable one.

  Report in the execution summary: the step-3 and step-8 `TOTAL:` values and every `TOKENCOUNT:` value from
  both; the four disclosure counts with their token classes named by class only, together with the explicit
  statement that each such file must be sanitised under the same hygiene rules before any later commit stages
  it; the step-9 porcelain result including any `.claude/agent-memory/` and `??` entries by path; the AC
  Status Summary required by AC-R7 (`6 of 6 acceptance criteria checked (unchanged by this remediation)`);
  and the squash-merge requirement from `[P1-T4]`. Steps 8 and 9 write no file inside the feature folder by
  design, because an artifact written there would violate the step-7 write prohibition above.
