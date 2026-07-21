# Baseline File Inventory — UtilitiesCS/Extensions/

Timestamp: 2026-07-19T00-05

Command: `for f in *.cs; do wc -l "$f"; grep -q "#nullable enable" "$f"; done` (run in UtilitiesCS/Extensions)

Total .cs files: 25
Already `#nullable enable` (verify-only, no edits expected): 2 — IAsyncEnumerableExtensions.cs, NullExtensions.cs
Remediation targets (23): all others below.

| File | Lines | #nullable enable | Batch |
|---|---|---|---|
| IAsyncEnumerableExtensions.cs | 260 | YES | verify-only |
| NullExtensions.cs | 136 | YES | verify-only |
| ExtToChar.cs | 25 | no | A |
| CompilerServicesExtensions.cs | 17 | no | A |
| DrawingExtensions.cs | 29 | no | A |
| QueueExtensions.cs | 19 | no | A |
| IControlExtensions.cs | 19 | no | A |
| ExceptionExtensions.cs | 24 | no | A |
| StringExtensions.cs | 100 | no | B |
| JsonExtensions.cs | 35 | no | B |
| JsonSerializerExtensions.cs | 126 | no | B |
| ImageExtensions.cs | 59 | no | B |
| StreamExtensions.cs | 41 | no | B |
| LazyExtension.cs | 53 | no | B |
| IEnumerableExtensions.cs | 485 | no | C |
| ArrayExtensions.cs | 544 | no | C (pre-existing >500, annotation-only, NOT split) |
| IListExtensions.cs | 273 | no | C |
| DictionaryExtensions.cs | 280 | no | C |
| EnumExtensions.cs | 198 | no | D |
| TraceExtensions.cs | 108 | no | D |
| WinFormsExtensions.cs | 477 | no | D |
| AsyncSerialization.cs | 330 | no | E |
| DfMLNet.cs | 309 | no | E |
| DfDeedle.cs | 403 | no | E (partial class with FrameUtilities) |
| DfDeedle.FrameUtilities.cs | 274 | no | E (partial class with DfDeedle) |

Confirmation: exactly 2 files (IAsyncEnumerableExtensions.cs, NullExtensions.cs) already carry `#nullable enable` and are verify-only; 23 files are remediation targets. Matches plan Batch A (6) + B (6) + C (4) + D (3) + E (4) = 23.
