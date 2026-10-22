# Remediation cycle 1, P0-T4: CR-1 base-pattern count (read-only)

Timestamp: 2026-10-06T20-27
Command: pwsh -NoProfile -Command '$t = @(git -C "<execution-worktree-root>" show 993fdd01566dee82e5f37acb761a600feaaa1454:UtilitiesCS/UtilitiesCS.csproj); "BASE-LINES=" + $t.Count; "BASE-NARROW=" + @($t | Select-String -Pattern "CategoryClassifierGroup\.ConditionalEngine").Count; "BASE-WIDE=" + @($t | Select-String -Pattern "ConditionalEngine").Count; $h = @(Get-Content -LiteralPath "<execution-worktree-root>/UtilitiesCS/UtilitiesCS.csproj"); "HEAD-NARROW=" + @($h | Select-String -Pattern "CategoryClassifierGroup\.ConditionalEngine").Count; "HEAD-WIDE=" + @($h | Select-String -Pattern "ConditionalEngine").Count'
EXIT_CODE: 0

Printed lines (verbatim):
BASE-LINES=1335
BASE-NARROW=0
BASE-WIDE=1
HEAD-NARROW=1
HEAD-WIDE=2

Output Summary:
- pwsh exit 0; BASE-LINES recorded, not gated.
- BASE-NARROW=0 and BASE-WIDE=1 (the pre-existing Interfaces\IGlobals\IConditionalEngine.cs item) at merge base 993fdd015.
- HEAD-NARROW=1 (the Part H Compile Include) and HEAD-WIDE=2 at head.
- The narrowed pattern of plan revision 1.7 reads 0 at base and discriminates the Part H item from the interface item; no CR1-COUNT-MISMATCH.
