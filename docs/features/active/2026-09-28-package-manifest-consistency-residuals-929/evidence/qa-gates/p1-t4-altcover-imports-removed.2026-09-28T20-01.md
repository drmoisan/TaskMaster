# P1-T4 — altcover Import elements removed

Timestamp: 2026-09-30T10-06
Command: Edit QuickFiler.Test/QuickFiler.Test.csproj (delete the two altcover Import lines); pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; "NUMSTAT=" + (git diff --numstat 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- QuickFiler.Test/QuickFiler.Test.csproj); "LINES=" + @(Get-Content QuickFiler.Test/QuickFiler.Test.csproj).Count; "ALTCOVER_LINES=" + @(git grep -i -n altcover -- "*.csproj" "*/packages.config").Count'; git diff 231e1c0b55105aeb626bf5a6e8d0266a567cacad -- QuickFiler.Test/QuickFiler.Test.csproj
EXIT_CODE: 0
Output Summary:
- NUMSTAT=0	2	QuickFiler.Test/QuickFiler.Test.csproj (0 added, 2 deleted)
- LINES=568 (570 minus 2)
- ALTCOVER_LINES=0 (P0-T17 measured 2 over the same pathspec)
- Deleted lines, verbatim from git diff <BASE-SHA> -- QuickFiler.Test/QuickFiler.Test.csproj:
  - `-  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props')" />` (hunk @@ -5,7 +5,6 @@)
  - `-  <Import Project="..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets" Condition="Exists('..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets')" />` (hunk @@ -534,7 +533,6 @@)
- No other change to the file.
