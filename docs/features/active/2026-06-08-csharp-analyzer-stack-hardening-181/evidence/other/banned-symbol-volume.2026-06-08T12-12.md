# Banned-Symbol Volume in First-Party Source (Issue #181)

Timestamp: 2026-06-08T12-27
Method: read-only ripgrep (Grep tool) across all *.cs files in the repo working tree (first-party project directories; vendored SVGControl / UtilitiesSwordfish have no matches for these symbols and are excluded from the rollout regardless).

## Per-symbol occurrence counts
| Banned symbol | Occurrences | Files |
|---|---|---|
| System.DateTime.Now | 55 | 18 |
| System.DateTime.UtcNow | 15 | 5 |
| System.Random.Shared | 0 | 0 |
| System.Threading.Thread.Sleep | 21 | 13 |
| System.Threading.Tasks.Task.Delay | 52 | 24 |
| TOTAL | 143 | — |

Notes:
- Counts include production and test files. The dominant hosts are UtilitiesCS / UtilitiesCS.Test, QuickFiler, ToDoModel, TaskMaster.
- Random.Shared has zero current usages.

## Conclusion
- The aggregate volume (143 existing usages across first-party code) confirms that RS0030 MUST be configured at `severity = suggestion` at initial rollout. Setting RS0030 to `warning` would, under the nullable `TreatWarningsAsErrors=true` CI step, be promoted to errors and break the build on every one of these ~143 call sites.
- Legacy banned-symbol cleanup (migrating these call sites to TimeProvider / injected abstractions and then promoting RS0030 to `warning`) is documented as OUT-OF-SCOPE / FOLLOW-UP work for this feature, per the plan Open Questions and the RS0030 .editorconfig comment to be added in P2-T7.
