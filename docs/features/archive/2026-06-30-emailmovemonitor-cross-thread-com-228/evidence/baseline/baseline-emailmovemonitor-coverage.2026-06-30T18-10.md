# Baseline — EmailMoveMonitor.cs Coverage Denominator (Issue #228)

Timestamp: 2026-06-30T22-21
Source path: QuickFiler\Helper Classes\EmailMoveMonitor.cs
Coverage source: P0-T5 binary .coverage converted to Cobertura (scratchpad baseline.cobertura.xml)

Pre-change EmailMoveMonitor.cs file-level line coverage: 8.15%
- Total instrumented lines (all classes in the file: EmailMoveMonitor, EmailMoveAction, compiler-generated display/state-machine classes): 135
- Covered lines: 11

Per-class baseline line-rate:
- QuickFiler.Helper_Classes.EmailMoveMonitor (primary): 17.74% (only the parameterless constructor and SetupBeforeItemMove delegate assignment are hit incidentally when consumers construct the type under other tests; HookItem/UnhookItem/UnhookAll bodies are not exercised)
- QuickFiler.Helper_Classes.EmailMoveAction: 0%
- Compiler-generated lambda/display/async-state-machine classes (<>c__DisplayClass*, <GetParentFolderAsync>d__6, <UnhookItemAsync>d__5): 0%

This confirms research §6: there are zero existing direct unit tests for EmailMoveMonitor bookkeeping. The near-zero baseline is the denominator against which the >=90% changed/new-code target (AC5) is measured in P10-T1.
