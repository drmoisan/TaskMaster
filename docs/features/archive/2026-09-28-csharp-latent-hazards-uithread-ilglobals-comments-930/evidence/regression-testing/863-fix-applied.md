# #863 fix applied ([P1-T10])

Timestamp: 2026-09-29T09-15
Command: Grep tool over UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs (whole-word `\bCache\b`, whole-word `\bmodules\b`, `public static readonly OpCode\[\]`, `using System\.Reflection;`); git diff --numstat ac819907f479ee18026993054e714dc2e056142f -- "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs"; @(Get-Content -LiteralPath "UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs").Count
EXIT_CODE: 0
Output Summary:
- `Cache` whole-word occurrences: 0 (1 before)
- `modules` whole-word occurrences: 0 (1 before)
- `public static readonly OpCode[]` occurrences: 2 (lines 120 and 128)
- `using System.Reflection;` occurrences: 1 (line 4)
- Numstat: `0	3	UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` (0 added, 3 deleted)
- Line count: 225

Edit: deleted `public static Dictionary<int, object> Cache = new Dictionary<int, object>();` (original line 113), the blank line 114 after it, and `public static Module[]? modules = null;` (original line 131). This removes the public members ILGlobals.Cache and ILGlobals.modules (Decision D4, breaking public API change; in-repo consumer check is [P1-T11] and [P1-T13]).
