# Log Sanitization — Absolute Host-Path Removal from Raw MSBuild Logs ([P2-T10])

Timestamp: 2026-09-02T23-31

## Scope

Four raw MSBuild file-logger artifacts sanitized in place, before any of them is committed:

- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-analyzers-pre.log`
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/baseline/msbuild-nullable-pre.log`
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-analyzers-post.log`
- `docs/features/active/2026-09-02-ci-build-infra-debt-730/evidence/qa-gates/msbuild-nullable-post.log`

Two distinct absolute-path leak classes were substituted, each in both backslash and
forward-slash form:

1. **Worktree-root path prefix** — emitted by MSBuild's normal-verbosity logger on
   effectively every diagnostic line (full absolute path to `TaskMaster.sln` and to every
   project file). Replaced with the placeholder `<repo-root>`.
2. **Main-checkout-root path prefix** — an ancestor of the worktree root, embedded by every
   `csc.exe` compiler-invocation line via an `/analyzerconfig:` argument pointing at the
   main-checkout `.editorconfig`. Because it is an ancestor, it is not itself prefixed by the
   worktree-root string, so the worktree-root substitution never matched it. Replaced with the
   distinct placeholder `<main-checkout-root>`.

Both prefixes were derived at run time (`git rev-parse --show-toplevel`;
`git rev-parse --path-format=absolute --git-common-dir` with its parent taken), and the
residual-sweep account token was derived at run time from `Split-Path -Leaf $env:USERPROFILE`.
Neither raw path nor the account name is quoted as a literal anywhere in this artifact.

Ordering is load-bearing: Step 2 ran strictly after Step 1 completed for all four files,
because the main-checkout-root string is itself a prefix of the worktree-root string and
applying it first would truncate-match and corrupt every worktree-root occurrence. The two
placeholders are kept distinct because the two roots appear juxtaposed on the same `csc.exe`
line; collapsing them would destroy the log's own record that MSBuild emits two distinct
`/analyzerconfig:` arguments for the same file.

## Command

Executed as a single `pwsh -NoProfile -Command` payload (line breaks added below for
readability; no literal path or account name is spelled out — all three are derived at run
time):

```powershell
$ErrorActionPreference = "Stop"
$files = @(
  <baseline/msbuild-analyzers-pre.log>,
  <baseline/msbuild-nullable-pre.log>,
  <qa-gates/msbuild-analyzers-post.log>,
  <qa-gates/msbuild-nullable-post.log>
)

# Line-count invariant: recorded immediately before Step 1
$repoRoot    = (git rev-parse --show-toplevel).Trim() -replace "/","\"
$repoRootFwd = $repoRoot -replace "\\","/"
$userToken   = [regex]::Escape((Split-Path -Leaf $env:USERPROFILE))
$before = @{}; foreach ($f in $files) { $before[$f] = (Get-Content -LiteralPath $f).Count }

# Step 1 — worktree-root substitution (runs first)
foreach ($f in $files) {
  $c = Get-Content -Raw -LiteralPath $f
  $c = $c -ireplace [regex]::Escape("$repoRoot\"), "<repo-root>\"
  $c = $c -ireplace [regex]::Escape("$repoRootFwd/"), "<repo-root>/"
  Set-Content -LiteralPath $f -Value $c -NoNewline
}

# Step 2 — main-checkout-root substitution (strictly after Step 1, all four files)
$gitCommonDir        = (git -C $repoRoot rev-parse --path-format=absolute --git-common-dir).Trim() -replace "/","\"
$mainCheckoutRoot    = Split-Path -Parent $gitCommonDir
$mainCheckoutRootFwd = $mainCheckoutRoot -replace "\\","/"
foreach ($f in $files) {
  $c = Get-Content -Raw -LiteralPath $f
  $c = $c -ireplace [regex]::Escape("$mainCheckoutRoot\"), "<main-checkout-root>\"
  $c = $c -ireplace [regex]::Escape("$mainCheckoutRootFwd/"), "<main-checkout-root>/"
  Set-Content -LiteralPath $f -Value $c -NoNewline
}

# Line-count invariant: recorded immediately after Step 2
$after = @{}; foreach ($f in $files) { $after[$f] = (Get-Content -LiteralPath $f).Count }

# Residual verification — single case-insensitive sweep, root-agnostic
$residual = @(Get-ChildItem -LiteralPath $files | Select-String -Pattern "(?i)$userToken").Count
if ($residual -ne 0) { exit 1 } else { exit 0 }
```

EXIT_CODE: 0

## Output Summary

### (a) Line-count invariant — before Step 1 vs. after Step 2

Both steps are pure substring substitutions performed within each existing line; neither
inserts, deletes, reorders, nor merges any line. Every line-number citation already recorded
against these files (P0-T3, P0-T4, P2-T6, P2-T7, and the plan's "Warning-count measurement
method" Framing bullet) therefore remains valid unchanged.

| File | Lines before Step 1 | Lines after Step 2 | Identical |
|---|---|---|---|
| `evidence/baseline/msbuild-analyzers-pre.log` | 11878 | 11878 | yes |
| `evidence/baseline/msbuild-nullable-pre.log` | 12030 | 12030 | yes |
| `evidence/qa-gates/msbuild-analyzers-post.log` | 11906 | 11906 | yes |
| `evidence/qa-gates/msbuild-nullable-post.log` | 11742 | 11742 | yes |

All four before/after pairs are identical.

### (b) Residual-match count — case-insensitive sweep for the run-time-derived account token

| File | Residual matches |
|---|---|
| `evidence/baseline/msbuild-analyzers-pre.log` | 0 |
| `evidence/baseline/msbuild-nullable-pre.log` | 0 |
| `evidence/qa-gates/msbuild-analyzers-post.log` | 0 |
| `evidence/qa-gates/msbuild-nullable-post.log` | 0 |
| **Total across all four files** | **0** |

The sweep is agnostic to which root the token was embedded in, so this single sweep covers
both the worktree-root and the main-checkout-root leak classes.

### Spot check

The `csc.exe` invocation at `evidence/baseline/msbuild-analyzers-pre.log:1139` — the line the
plan's round-8 correction cites as carrying both leak classes back to back — now reads its two
`/analyzerconfig:` arguments as `/analyzerconfig:<main-checkout-root>\.editorconfig` followed
by `/analyzerconfig:<repo-root>\.editorconfig`. Both placeholders are present and remain
distinct, preserving the log's record that MSBuild emits two separate `/analyzerconfig:`
arguments for the same file.
