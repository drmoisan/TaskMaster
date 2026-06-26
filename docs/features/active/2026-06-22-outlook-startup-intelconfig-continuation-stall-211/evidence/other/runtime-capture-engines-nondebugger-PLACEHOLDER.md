# AC9 Runtime Capture — Engines-Phase Attribution (NON-DEBUGGER) — SUPERSEDED

STATUS: SUPERSEDED. The AC9 maintainer non-debugger capture has been recorded at
`runtime-capture-engines-nondebugger-2026-06-23T17-33.md` (same directory).

Result summary: the per-engine instrumentation is validated; in that run the
multi-minute stall did NOT reproduce (TOTAL 0:02.59, Engines 0:01.42), with the
Spam classifier deserialization the dominant TaskMaster engine cost (~1.33 s).
The slow-path (multi-minute) root cause is not yet attributed; a cold/slow-start
capture with this build is needed to attribute it. See the recorded capture file
for the full analysis.
