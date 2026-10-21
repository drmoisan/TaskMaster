# Code Review — Issue #752

- Timestamp: 2026-09-03T12-23
- Branch: `bug/coverage-assembly-discovery-excludes-own-worktree-root-752`
- Head: `80d07a1c26122a5cede04edc5833c964d663d8b7`
- Base (merge base with `origin/main`): `87233f867ad60c0a5c0d19b09cc121ae536d7ba1`
- Source files reviewed: `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (+1/-1),
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` (+99/-0)

Blocking findings in this artifact: **0**.

## 1. The production change

Before:

```powershell
$_.FullName -notmatch '\\\.claude\\'
```

After (`scripts/vscode/Invoke-MSTestWithCoverage.ps1:301`, 16 spaces of indentation):

```powershell
([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'
```

### CR-1 — PASS — The fix matches the documented root cause and is correct

`spec.md` line 31 states the root cause as: the clause tests the absolute `FullName`, and
`Get-ChildItem -Path $resolvedSearchRoot -Recurse` guarantees every candidate is prefixed by the
search root, so a search root that itself contains `.claude\worktrees\agent-<id>\` makes the
exclusion fire on every candidate. The change addresses exactly that by changing the match target
to the path relative to `$resolvedSearchRoot`, which is in scope by closure from its assignment at
line 272 — the same mechanism the sibling clause at line 298 already uses for `$Configuration`.

The anchor `(^|\\)` is load-bearing and not incidental. `GetRelativePath` returns a descendant path
with no leading separator, so `.claude\worktrees\agent-1\...` has no backslash before `.claude`; a
literal substitution that kept the original `\\\.claude\\` pattern would have stopped matching
nested sibling worktrees and silently broken the preserved regression test at
`Invoke-MSTest.RunSettings.Tests.ps1:416-442`. The three measured relative paths in
`evidence/regression-testing/getrelativepath-probe.2026-09-03T07-23.md` show this directly:
all three produce `OLD_REGEX_MATCH=False`, while the anchored pattern yields `False`, `True`, `True`.

Behaviour re-derived independently by this reviewer for the four reachable shapes:

| Search root | Candidate | Relative path | Old | New |
|---|---|---|---|---|
| `C:\repo\.` (default `-SearchRoot '.'`) | `C:\repo\QuickFiler.Test\bin\Debug\x.Test.dll` | `QuickFiler.Test\bin\Debug\x.Test.dll` | keep | keep |
| `C:\repo\.` | `C:\repo\.claude\worktrees\agent-1\...\x.Test.dll` | `.claude\worktrees\agent-1\...` | drop | drop |
| `C:\repo\.claude\worktrees\agent-7\.` | `...agent-7\QuickFiler.Test\bin\Debug\x.Test.dll` | `QuickFiler.Test\bin\Debug\x.Test.dll` | drop (the defect) | keep (the fix) |
| `C:\repo\.claude\worktrees\agent-7\.` | `...agent-7\.claude\worktrees\agent-9\...` | `.claude\worktrees\agent-9\...` | drop | drop |

Rows 1, 2 and 4 are unchanged by the fix; only row 3 changes, and it changes in the direction the
issue requires. The change is therefore a strict, minimal widening, matching the
backward-compatibility statement at `spec.md` line 76.

Two further correctness properties worth recording:

- `[System.IO.Path]::GetRelativePath` normalises both arguments internally, so the un-normalised
  trailing `\.` that `Join-Path $repoRoot '.'` produces at line 272 needs no pre-stripping. The
  probe artifact exercises exactly that shape (`C:\repo\.`) rather than a cleaned one.
- The new clause sits last in the `-and` chain. PowerShell's `-and` short-circuits, so the
  `GetRelativePath` call is evaluated only for candidates that already matched
  `\\bin\\$Configuration\\` and cleared the `obj` and `ref` clauses. Placing the only clause with a
  method call at the end of the chain is the right ordering and should be preserved if the chain is
  ever reordered.

### CR-2 — Observation — non-blocking — Test-fixture duplication

`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:1-18` reproduces the
import block of `Invoke-MSTest.RunSettings.Tests.ps1:1-26`, and lines 21-42 reproduce that file's
`BeforeEach` mock set at lines 347-373. The second `It` (lines 59-72) is a near-verbatim restatement
of the preserved test at lines 416-442 of the same file, with a different name.

- Violated principle: `.claude/rules/general-code-change.md`, Design Principles item 2
  ("Reusability — Factor out logic that is clearly reusable. Avoid copy-paste").
- Mitigating context, verified: `plan.2026-09-03T07-23.md` line 53 records that
  `Invoke-MSTest.RunSettings.Tests.ps1` stands at 488 of the repository's 500-line cap
  (`evidence/baseline/pre-change-tree-state.2026-09-03T07-23.md`, `LINECOUNT ... 488`), so growing
  it was not available. Creating a sibling file was the correct call. The duplication is a
  consequence of that constraint, not of carelessness, and the plan documents the second `It` as a
  deliberate "symmetry twin".
- Suggested follow-up, not required for this merge: extract the ten-seam `BeforeEach` and the AST
  import block into a dot-sourced helper under `tests/scripts/vscode/` so a future third file does
  not triple the copy. That refactor touches a file this item is not permitted to modify, so it
  belongs in its own item.

### CR-3 — Observation — non-blocking — `Should` assertion inside `BeforeAll`

`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1:17` places
`$parseErrors | Should -BeNullOrEmpty` inside `BeforeAll`. In Pester 5 a failed assertion in a
container setup block surfaces as a container-level error rather than as a named test failure,
which reads less clearly in a failure report than a dedicated `It` would.

This is inherited verbatim from `Invoke-MSTest.RunSettings.Tests.ps1:21`, so repository consistency
argues for keeping it as written. Recorded only so the pattern is not mistaken for a new choice
made by this item.

### CR-4 — Observation — non-blocking — Explanatory comments dropped in the copy

The source block at `Invoke-MSTest.RunSettings.Tests.ps1:9-10` and `:13-14` carries two comments
explaining *why* dot-sourcing the script and the parsed `ScriptBlock` is safe — the production
entry points are guarded by an `InvocationName` check (`Invoke-MSTestWithCoverage.ps1:348-349`), so
only definitions are imported. The new file reproduces the mechanics at lines 4-18 without those
two comments.

- Violated guidance: `.claude/rules/general-code-change.md`, Naming/Docs — comment *why*, not what.
  A reader of the new file alone cannot tell why dot-sourcing a whole script inside `BeforeAll` does
  not execute it.
- Cost to remedy: two comment lines. Recommended but not required.

### CR-5 — Observation — non-blocking — The empty-discovery error path has no permanent test

`scripts/vscode/Invoke-MSTestWithCoverage.ps1:305-307` throws
`No test assemblies found under '<root>' for configuration '<config>'. Build first.` when the
discovery set is empty. That path is exercised only transiently, by the pre-fix run recorded in
`evidence/regression-testing/pre-fix-new-suite.2026-09-03T07-23.md`; once the fix is in, no test in
either file drives the discovery set to empty on purpose.

`.claude/rules/general-unit-test.md`, Scenario Completeness, lists error-handling behaviour among
the scenarios each unit should cover. An `It` that mocks `Get-ChildItem` to return an empty array
and asserts `Should -Throw -ExpectedMessage 'No test assemblies found*'` would close this in about
five lines and would also pin the message the issue report identifies as misleading. Recommended as
a follow-up; the gap predates this item and this item's minimal-fix scope does not require it.

### CR-6 — PASS — Test determinism and purity

Checked against `.claude/rules/general-unit-test.md` Determinism Infrastructure and the
`check-powershell-test-purity.ps1` forbidden-pattern list (lines 99-117):

- No `Start-Sleep`, `Start-Process`, `Invoke-WebRequest`, `Invoke-RestMethod`, `System.Net.*`.
- No `New-TemporaryFile`, `GetTempPath`, `GetTempFileName`, `$env:TEMP`, `$env:TMP`; no filesystem
  write of any kind.
- No direct executable mocking; the vswhere seam is mocked through `Invoke-VsWhereExe` with a
  parameter block matching the production call at lines 284-286.
- No wall-clock or RNG read.
- All fixtures are literal in-memory `[pscustomobject]` records.

### CR-7 — PASS — Readability and failure diagnostics

Each `It` name states the behaviour in full, and each carries a comment naming the originating
issue and, for the third case, why it is not redundant with the other two ("a fix that simply
disables the exclusion whenever the root is under `.claude` cannot pass"). Assertions compare the
captured array against an explicit expected array, so a Pester failure prints both sides.

### CR-8 — PASS — File size, structure, and public surface

- Production file: 350 lines, unchanged in length (the edit is in place).
- New test file: 99 lines, well under the 500-line cap in `.claude/rules/general-code-change.md`
  and `.claude/rules/powershell.md` line 35.
- No function signature, parameter, or return shape changed; `$testAssemblies` remains an array of
  absolute path strings.

## 2. Items explicitly checked and found clean

| Check | Result |
|---|---|
| Fix confined to the plan's Write Set | Yes — only the two source files plus feature-folder documents appear in the branch diff |
| `Invoke-MSTest.RunSettings.Tests.ps1` byte-identical to its pre-change blob | Yes — absent from `git diff --numstat <base> HEAD`; blob `4b168b07967b692fdb0574aefd7a5734dfeb0d9c` unchanged |
| Sibling clauses preserved | Yes — lines 298, 299, 300, the `@(...)` wrapper at 296/303, and the throw at 306 are untouched |
| Working tree clean against `HEAD` | Yes — `git -C <repo-root> diff --stat HEAD` returns empty |
| Analyzer debt not increased | Yes — 16 diagnostics before and after, identical set, none in either changed file |
| Same defect class elsewhere in the repository | No — a repo-wide grep across `*.ps1`, `*.psm1`, `*.psd1` for a comparison operator followed by a quoted `.claude` returns exactly one hit, the fixed line itself |

## 3. Summary

| Severity | Count | IDs |
|---|---|---|
| Blocking | 0 | — |
| Non-blocking observation | 4 | CR-2, CR-3, CR-4, CR-5 |
| Pass | 4 | CR-1, CR-6, CR-7, CR-8 |

The production change is minimal, correct, well-targeted, and does not regress any behaviour the
prior item established. The test file is deterministic, dependency-free, and covers the positive,
negative, and double-nested edge cases. The four observations are quality suggestions, none of which
should hold the merge.
