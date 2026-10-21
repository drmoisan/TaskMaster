# Artifact Hygiene Sweep — Phase 1 Commit (P1-T11)

Timestamp: 2026-09-01T15-58

Command:

```powershell
$feature = 'docs/features/active/efcselectionguard-banner-prefix-arity-and-stale-comment-662'
$plan = Join-Path $feature 'plan.2026-08-31T20-11.md'
$root = (Resolve-Path .).Path
$profileDir = $env:USERPROFILE
$account = Split-Path -Leaf $env:USERPROFILE
$machine = $env:COMPUTERNAME
$pairs = @(@($root, '<repo-root>'), @($profileDir, '<user-profile>'), @($machine, '<host>'), @($account, '<user>'))
$opts = [System.Text.RegularExpressions.RegexOptions]::IgnoreCase
$rewritten = 0
Get-ChildItem -LiteralPath (Join-Path $feature 'evidence') -Recurse -File | ForEach-Object {
    git check-ignore -q -- $_.FullName
    if ($LASTEXITCODE -eq 0) { return }
    $orig = [System.IO.File]::ReadAllText($_.FullName)
    $text = $orig
    foreach ($p in $pairs) { $text = [regex]::Replace($text, [regex]::Escape($p[0]), $p[1], $opts) }
    if ($text -ne $orig) { [System.IO.File]::WriteAllText($_.FullName, $text); $rewritten++ }
}
$hits = Get-ChildItem -LiteralPath $feature -Recurse -File | Where-Object { $_.FullName -ne (Resolve-Path $plan).Path } | Where-Object {
    $t = [System.IO.File]::ReadAllText($_.FullName)
    $t.IndexOf($account, [System.StringComparison]::OrdinalIgnoreCase) -ge 0 -or $t.IndexOf($machine, [System.StringComparison]::OrdinalIgnoreCase) -ge 0
}
"FilesRewritten=$rewritten"
"ResidualMatchCount=$(@($hits).Count)"
```

The span is recorded as written, with its variable names unsubstituted.

EXIT_CODE: 0

Output Summary:

```
FilesRewritten=0
ResidualMatchCount=0
```

- `FilesRewritten=` 0
- `ResidualMatchCount=` 0

Token classes substituted: worktree-root prefix, user-profile path,
`computerName`, `runUser`, `storage`, Cobertura `filename`.

`FilesRewritten=` is 0 because the P0-T18 sweep already cleaned every file that
carried a host token, and the Phase 1 artifacts written since then were authored
without one. `ResidualMatchCount=` is 0, so the BLOCKED branch does not arise
and the sweep clears the `git add` that follows.
