# Phase 4 — Formatter Pass Over This Change's Own Paths (P4-T1)

Timestamp: 2026-09-03T02-56
Task: [P4-T1]
Command: `Get-FileHash -Algorithm SHA256` for each of the eight paths, then `dotnet tool run csharpier format <the eight paths>`, then the same eight hashes again.
EXIT_CODE: 0
Pass number: 1

The two project files are excluded from the formatter by `.csharpierignore` and are deliberately not
passed to it.

## Sixteen hashes — eight before, eight after

| # | Path | SHA-256 before | SHA-256 after | Rewritten |
|---|---|---|---|---|
| 1 | `TaskMaster/Ribbon/RibbonExplorer.xml` | `6C2673485DCBC716E1DAD38803A8A1AAC91F918F943DD0C417796388732326C9` | `6C2673485DCBC716E1DAD38803A8A1AAC91F918F943DD0C417796388732326C9` | No |
| 2 | `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | `31CA5A054B1E4FB0649BE08FEF0FD25582CE75A9D4625A9C367F2BD0CD10BE33` | `31CA5A054B1E4FB0649BE08FEF0FD25582CE75A9D4625A9C367F2BD0CD10BE33` | No |
| 3 | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `0BBE904E0E1DB4A86FF0106F26220CD213B1DB6D560F49C565DC5023FCCCA5E5` | `0BBE904E0E1DB4A86FF0106F26220CD213B1DB6D560F49C565DC5023FCCCA5E5` | No |
| 4 | `TaskMaster/Ribbon/SpamManagerResetGate.cs` | `F7411151BB149D75922187F5F929797116AAEC19C9A1D205C9E62FF1B8B47D4C` | `87730FADCE25A67FC07F25B6117252AC03D4FFC9C4EA96CE9891D81E32324755` | **Yes** |
| 5 | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | `2C1C6DF540044F52C13495C27243A0106DEAFA6668C886F61023DE4ECA88C44E` | `9C77B35867FD5C702EFD18D3A3C80DE9C2FA7F19282A22995D8B559D829A363A` | **Yes** |
| 6 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | `61F4EB0FCC001C43A8F0F2F4C95EBDC6C0B1D2310700C0B79CF3F5805C84D487` | `61F4EB0FCC001C43A8F0F2F4C95EBDC6C0B1D2310700C0B79CF3F5805C84D487` | No |
| 7 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | `29BD55822B6983188C251DBAD25C843542FFF6882839A1C5AAF0B6F951B6E6BD` | `CA98BB732B3686268C12B1C8FA5EC4A0E02D6EC57E0A1025429F19626953011C` | **Yes** |
| 8 | `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | `FF58BC8895A08E6713430D498D0ABF1672657E32F0275A39DF8A249A1B35198E` | `F0F8D8A2EA05A4C878B138B154DFD23A2A346DA40727C6ECECE75FFBA284D93C` | **Yes** |

**Rewritten-file count: 4** — defined as the number of paths whose two hashes differ.

The console line `Formatted 8 files in 4099ms.` is NOT used as the rewritten count. CSharpier reports
the number of files it PROCESSED, not the number it CHANGED, so an eight-path run always prints 8
and a restart rule keyed on it could never terminate. The hash pairs above are the observation of
record, and they show 4 of the 8 were actually rewritten.

A rewritten count greater than zero does not by itself trigger a restart: the restart obligation is
triggered by a later failing step, so execution continues to P4-T2.

## Sibling-invalidation check

The rule is that for every path whose two hashes differ, the earlier scope gate that measured that
path must be re-run and its artifact replaced.

| Named trigger | Hash changed | Required action | Outcome |
|---|---|---|---|
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | **No** | re-run the P3-T13 numstat check and re-confirm F3-AC6 | **Not required.** The hash is byte-identical, so the one-word `partial` edit measured by P3-T13 is untouched and F3-AC6 stands. |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | **No** | re-run the P2-T11 region check and re-confirm F2-AC4 | **Not required.** The hash is byte-identical, so the call-site region check and F2-AC4 stand. |
| `TaskMaster/Ribbon/RibbonExplorer.xml` | No | none in any case | Not required. The P1-T9 check compares element and attribute multisets rather than lines and is reflow-independent, so it would have stood even had the hash changed. |

Additional re-run performed under the general rule, beyond the three named cases:

- `TaskMaster/Ribbon/SpamManagerResetGate.cs` was rewritten, and it is the path measured by the
  P2-T4 host-neutrality gate. That gate was re-run against the post-format file. All four required
  counts are still zero (coverage-attribute form 0, `^using Microsoft\.Office` 0,
  `^using System\.Windows\.Forms` 0, `log4net` 0), the four using directives are unchanged, exactly
  one type is still declared, and the file is still 141 lines. The P2-T4 artifact therefore remains
  accurate and needed no replacement.

The other three rewritten paths — `RibbonExplorerXmlTests.cs`,
`EngineToggleStateCoordinatorTests.Race.cs` and `SpamManagerResetGateTests.cs` — are measured by no
scope gate. Their earlier acceptance conditions are test-method-name presence checks, which
reformatting cannot invalidate, and all of their tests are re-executed by the P4-T7 coverage run.

`TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` was NOT rewritten, so the P3-T6 through P3-T9
structural checks recorded in `race-fix-structure.2026-09-02T12-04.md`, including the quoted line
numbers, remain exact.

## Post-format line counts, carried to P4-T2

| Path | Lines |
|---|---|
| `TaskMaster/Ribbon/RibbonExplorer.xml` | 544 |
| `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | 444 |
| `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | **515** |
| `TaskMaster/Ribbon/SpamManagerResetGate.cs` | 141 |
| `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | 496 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | 459 |
| `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | 277 |
| `TaskMaster.Test/Ribbon/SpamManagerResetGateTests.cs` | 326 |

## Pass 2 — the branch B re-run required by P4-T3

P4-T3 resolved on branch B and extracted the versioned cache, which added two new formatter-visible
paths and changed the coordinator. Branch B requires P4-T1 to be re-run, and it was.

Command: the same shape as pass 1, over TEN paths — the original eight plus
`TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` and
`TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs`.
EXIT_CODE: 0

| # | Path | SHA-256 before | SHA-256 after | Rewritten |
|---|---|---|---|---|
| 1 | `TaskMaster/Ribbon/RibbonExplorer.xml` | `6C2673485DCBC716E1DAD38803A8A1AAC91F918F943DD0C417796388732326C9` | unchanged | No |
| 2 | `TaskMaster/Ribbon/RibbonController.Intelligence.cs` | `31CA5A054B1E4FB0649BE08FEF0FD25582CE75A9D4625A9C367F2BD0CD10BE33` | unchanged | No |
| 3 | `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` | `F2A961DD50F2E4678B5CF8B7FAA3F0316AA22D2FB8FE904AE5D08057F26ACEF0` | unchanged | No |
| 4 | `TaskMaster/Ribbon/EngineTogglePressedStateCache.cs` | `8AF4D6AC6C615AC51BA52459B90B171A7B315040ECFE85DBE3023FEC9676A7C2` | `5085EC6E2FBC501027BF3127F3DA80CEA0C64A6366BEF97B40DB8CA770831D03` | **Yes** |
| 5 | `TaskMaster/Ribbon/SpamManagerResetGate.cs` | `87730FADCE25A67FC07F25B6117252AC03D4FFC9C4EA96CE9891D81E32324755` | unchanged | No |
| 6 | `TaskMaster.Test/Ribbon/RibbonExplorerXmlTests.cs` | `9C77B35867FD5C702EFD18D3A3C80DE9C2FA7F19282A22995D8B559D829A363A` | unchanged | No |
| 7 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.cs` | `61F4EB0FCC001C43A8F0F2F4C95EBDC6C0B1D2310700C0B79CF3F5805C84D487` | unchanged | No |
| 8 | `TaskMaster.Test/Ribbon/EngineToggleStateCoordinatorTests.Race.cs` | `CA98BB732B3686268C12B1C8FA5EC4A0E02D6EC57E0A1025429F19626953011C` | unchanged | No |
| 9 | `TaskMaster.Test/Ribbon/EngineTogglePressedStateCacheTests.cs` | `4B6519D91378177FF57BCD58280F1ED9BFBDE7BC9696FBA0F6A0D1F81851F9F0` | `705521DC9A540C3BF157DF98D14CAF9CDAAF685A6CC6E6B85E4AB89DD625AEF2` | **Yes** |
| 10 | `TaskMaster/…/SpamManagerResetGateTests.cs` | `F0F8D8A2EA05A4C878B138B154DFD23A2A346DA40727C6ECECE75FFBA284D93C` | unchanged | No |

**Rewritten-file count on pass 2: 2** — both are the newly authored branch B files. Every path that
existed before branch B is byte-identical across pass 2, including the coordinator, whose hand-edit
for the extraction was already CSharpier-clean.

Sibling-invalidation check on pass 2: none of the three named triggers fired, because
`EngineToggleStateCoordinatorTests.cs`, `RibbonController.Intelligence.cs` and `RibbonExplorer.xml`
are all unchanged. The two rewritten paths are new files that no earlier scope gate measured.

Because `TaskMaster/Ribbon/EngineToggleStateCoordinator.cs` is unchanged by pass 2, the P3-T6 through
P3-T9 structural checks in `race-fix-structure.2026-09-02T12-04.md` still describe the file
accurately in substance, although its line numbers shifted when the extracted members were removed;
the members themselves and their ordering are unchanged, and the post-extraction ribbon test run
confirms the behavior.

Output Summary: The formatter ran with EXIT_CODE 0 over the eight formatter-visible in-scope paths
and rewrote 4 of them, measured by differing SHA-256 pairs rather than by the processed-file console
line. None of the three named sibling-invalidation triggers fired. The one additional rewritten path
covered by a scope gate, the new gate class, had that gate re-run with identical results.
`EngineToggleStateCoordinator.cs` measures 515 lines after formatting, above the 500-line ceiling,
so the P4-T3 contingency is live.
