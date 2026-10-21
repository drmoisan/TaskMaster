# P0-T18 — Baseline InternalsVisibleTo Occurrence Count

Timestamp: 2026-08-31T19-12
Command: enumerate `git ls-files -- "*.cs"`, read each file's raw content, and sum `[regex]::Matches($content, 'InternalsVisibleTo').Count`
EXIT_CODE: 0

BASELINE_IVT_COUNT: 37

Supporting figures: 1604 tracked `*.cs` files were enumerated; 35 of them carry at least one occurrence; the total across all of them is 37. The file carrying more than one is `UtilitiesCS/Properties/AssemblyInfo.cs`, which carries 3 — one of which is the `[assembly: InternalsVisibleTo("UtilitiesCS.Test")]` declaration at line 19 that the seam this change adds depends on. Every other matching file carries exactly 1.

DRIFT: the plan's P0-T18 records 36 as the count observed while the plan was authored. The measured count on this branch head is 37, one higher. The plan's acceptance for this task is that the artifact records an integer under the named field, which it does; the recorded value governs. The later gate that reads this field, P7-T21's sibling criterion P7-T11, is a comparison against this recorded 37, so the drift does not make any later gate unsatisfiable — it only means the comparison baseline is 37 rather than 36. The difference is consistent with this branch having been reconciled against `origin/main` after the plan was authored.

Counting-method note, recorded so P7-T11 reproduces it exactly: the count is over raw file content, so it includes occurrences inside comments and XML documentation as well as real attribute declarations. A line-oriented tool such as `grep -c` reports a different figure because it counts matching lines rather than matches and, when driven through `xargs`, silently skips tracked paths containing a space. P7-T11 must use the PowerShell regex form recorded above.

Output Summary: Baseline repository-wide occurrence count of `InternalsVisibleTo` across tracked C# sources is 37. AC11 requires this change to add no new `InternalsVisibleTo` attribute anywhere, so the post-change count must still equal 37.
