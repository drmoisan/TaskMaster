# Final 01: CSharpier format, write mode ([P2-T1])

Timestamp: 2026-09-29T09-18
Command: pwsh -NoProfile -Command '$before = (git diff ac819907f479ee18026993054e714dc2e056142f -- UtilitiesCS UtilitiesCS.Test QuickFiler | Out-String); $namesBefore = @(git diff --name-only ac819907f479ee18026993054e714dc2e056142f -- . ":(exclude)docs" ":(exclude).claude"); dotnet tool run csharpier format . 2>&1 | Tee-Object -FilePath coverage/930-format.log; $fmt = $LASTEXITCODE; $after = (git diff ac819907f479ee18026993054e714dc2e056142f -- UtilitiesCS UtilitiesCS.Test QuickFiler | Out-String); $namesAfter = @(git diff --name-only ac819907f479ee18026993054e714dc2e056142f -- . ":(exclude)docs" ":(exclude).claude"); "FORMAT_EXIT=$fmt"; "FORMAT_CHANGED_OWNED_PATCH=$($before -ne $after)"; "FORMAT_NEW_PATHS=$(@(Compare-Object $namesBefore $namesAfter | Where-Object { $_.SideIndicator -eq "=>" } | ForEach-Object { $_.InputObject }) -join ";")"; git status --porcelain --untracked-files=all -- . ":(exclude)docs" ":(exclude).claude"'
EXIT_CODE: 0
Iteration: 1
Output Summary:
- Formatter summary line: Formatted 1623 files in 6569ms. (processed count, not a rewrite count)
- FORMAT_EXIT=0
- FORMAT_CHANGED_OWNED_PATCH=False (the anchored patch over UtilitiesCS, UtilitiesCS.Test and QuickFiler is identical before and after the format run)
- FORMAT_NEW_PATHS= (empty)
- Porcelain outside docs and .claude: no lines (nothing under UtilitiesCS, UtilitiesCS.Test or QuickFiler).
