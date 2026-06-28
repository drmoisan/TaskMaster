# Banned-API Baseline (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command: `Select-String -Path 'QuickFiler/Controllers/QfcDatamodel.cs','QuickFiler/Controllers/QfcHomeController.cs' -Pattern 'DateTime\.Now','DateTime\.UtcNow','Random\.Shared','Thread\.Sleep','Task\.Delay'`

EXIT_CODE: 0

Output Summary:
- Banned set scanned: `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep`, `Task.Delay`.
- No matches for `DateTime.UtcNow`, `Random.Shared`, or `Thread.Sleep`.
- `Task.Delay` and `DateTime.Now` matches found (some active, some inside `//`-commented logger lines). These are pre-existing; RS0030 (BannedApiAnalyzers) is held at `suggestion` severity per `.claude/rules/csharp.md`, so they do not break the analyzer/nullable builds. Phase 4 (P4-T1) determines per-match disposition (removed-with-seam or deferred-finding).

## All matches (file:line)

### QuickFiler/Controllers/QfcDatamodel.cs
| Line | Text | Active/Commented | Banned token |
|---:|---|---|---|
| 58 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Creating new ...` | Commented | DateTime.Now |
| 65 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling InitDfAsync ...` | Commented | DateTime.Now |
| 434 | `await Task.Delay(5);` | Active | Task.Delay |
| 445 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Filtering df ...` | Commented | DateTime.Now |
| 452 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Sorting df ...` | Commented | DateTime.Now |
| 467 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Toggle offline mode")` | Commented | DateTime.Now |
| 470 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling ...GetEmailDataInViewAsync ...` | Commented | DateTime.Now |
| 679 | `await Task.Delay(200);` | Active | Task.Delay |

### QuickFiler/Controllers/QfcHomeController.cs
| Line | Text | Active/Commented | Banned token |
|---:|---|---|---|
| 43 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} ...LaunchAsync is beginning")` | Commented | DateTime.Now |
| 75 | `$"{DateTime.Now.ToString("mm:ss.fff")} "` | Active | DateTime.Now |
| 262 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling ...InitEmailQueueAsync ...` | Commented | DateTime.Now |
| 276 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Calling ...LoadItemsAsync ...` | Commented | DateTime.Now |
| 281 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} Showing and Refreshing ...` | Commented | DateTime.Now |
| 287 | `//logger.Debug($"{DateTime.Now.ToString("mm:ss.fff")} ...RunAsync is complete")` | Commented | DateTime.Now |
| 399 | `var now = DateTime.Now;` | Active | DateTime.Now |
| 400 | `//var curDateText = DateTime.Now.ToString("MM/dd/yyyy");` | Commented | DateTime.Now |
| 401 | `//var curTimeText = DateTime.Now.ToString("hh:mm");` | Commented | DateTime.Now |
| 479 | `curDateText = DateTime.Now.ToString("MM/dd/yyyy");` | Active | DateTime.Now |
| 481 | `curTimeText = DateTime.Now.ToString("hh:mm");` | Active | DateTime.Now |
| 493 | `OlEndTime = DateTime.Now;` | Active | DateTime.Now |
| 602 | `await Task.Delay(20);` | Active | Task.Delay |

Active banned-API call sites (pre-existing): QfcDatamodel.cs lines 434, 679 (`Task.Delay`); QfcHomeController.cs lines 75, 399, 479, 481, 493 (`DateTime.Now`), 602 (`Task.Delay`). Disposition is determined in Phase 4 (P4-T1).
