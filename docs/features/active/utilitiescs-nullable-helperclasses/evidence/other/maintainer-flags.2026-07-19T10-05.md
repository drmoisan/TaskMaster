# Maintainer Flags — File-Size Pre-Existing/Annotation-Driven Breaches (Issue #364, Batch 8)

- Timestamp: 2026-07-19T10-05
- Task: [P8-T4]

## (a) PrettyPrint.cs exceeds the 500-line limit — PRE-EXISTING

`UtilitiesCS/HelperClasses/PrettyPrint.cs` is 680 lines (was 677 before this feature; the +3 is the `#nullable enable` pragma and two `// why` comments). It exceeded the repo 500-line limit BEFORE this feature. Annotation-only work cannot bring the file under 500 without a refactor/split, which is outside the annotation-only scope of issue #364. Per the spec Constraints & Risks item (4) and the Non-Goals, the file is NOT split. This is FLAGGED as a known pre-existing policy exception, not fixed here.

## (b) FilePathHelper.cs crossed 500 lines — ANNOTATION-DRIVEN breach, FLAGGED

`UtilitiesCS/HelperClasses/FileSystem/FilePathHelper.cs` was 494 lines (near the limit) before this feature and is now 505 lines. The +11 lines are the `#nullable enable` pragma plus the annotation/`// why` comments required to document the behavior-preserving `!` operators (adapter-style nullable boundaries, the `_filePath = null!` transient assignment, and the `Path.GetDirectoryName(...)!` boundary) and the string-property contract split. Per the research findings item (2) and spec Constraints & Risks item (4), an annotation-driven crossing of the 500-line limit is FLAGGED rather than triggering a refactor. The file is NOT split; the crossing is a direct consequence of the mandated per-file `#nullable enable` + `// why` documentation convention.

Both breaches are flags for the maintainer, not fixes. Neither `PrettyPrint.cs` nor `FilePathHelper.cs` was refactored or split within this annotation-only feature.
