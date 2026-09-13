# P0-T15 — Analyzer include-path existence re-measurement after containment

Timestamp: 2026-09-13T02-20

Command: the identical pwsh payload P0-T13 used, with the same enumeration from `git -C . ls-files -- "*.csproj"`, the same extraction of every `Analyzer` element's `Include` attribute, and the same resolution of each include against its own declaring project file's directory, re-run after the P0-T14 containment.

EXIT_CODE: 0

PROJECT_COUNT=18
ANALYZER_ITEM_TOTAL=162
UNRESOLVED_COUNT=0

Output Summary: all 162 analyzer includes across the eighteen tracked project files now resolve on disk. The fifteen that were unresolved before the containment resolve through the back-filled package version directory, and no include was introduced or lost by the containment: the analyzer item total is unchanged at 162. The acceptance clause `UNRESOLVED_COUNT=0` holds, so the build gates acceptance criteria 7, 11 and 14 demand are not blocked by a missing analyzer assembly, which is a compiler error rather than a warning and would be an error in any case under the warnings-as-errors gate.
