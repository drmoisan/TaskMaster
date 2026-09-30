# P0-T9 Pester 5.6.1 provisioning

Timestamp: 2026-09-29T08-54
Command: pwsh -NoProfile -Command 'if (-not (Get-Module -ListAvailable Pester | Where-Object { $_.Version -eq [version]"5.6.1" })) { Install-Module Pester -RequiredVersion 5.6.1 -Force -SkipPublisherCheck -Scope CurrentUser }; Import-Module Pester -RequiredVersion 5.6.1; "PESTER=" + (Get-Module Pester).Version; "CONFIG=" + [bool](Get-Command New-PesterConfiguration -ErrorAction SilentlyContinue)'
EXIT_CODE: 0
Output Summary:
- PESTER=5.6.1
- CONFIG=True
- No install output was printed (the pinned version was already available).
