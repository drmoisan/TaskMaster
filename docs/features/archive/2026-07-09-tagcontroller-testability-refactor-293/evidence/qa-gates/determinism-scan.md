# Determinism / STA-Confinement Scan (P7-T6)

Timestamp: 2026-07-09T22-42

Command: `grep -rnE "new TagViewer\(|new Form\(|\.ShowDialog\(|\.Show\(|MessageBox\.|InputBox\.|\[STAThread\]|DoEvents|Thread\.Sleep|Task\.Delay|GetTempPath|GetTempFileName" Tags.Test --include=*.cs`
EXIT_CODE: 1 (no matches)

Command: `grep -rlE "STATestClass|STATestMethod|STAThread" Tags.Test --include=*.cs`
Command: `grep -rnE "new CheckBox\(|new Form\(|new Button\(|new Panel\(|new TextBox\(" Tags.Test --include=*.cs`

Output Summary:
1. No `new TagViewer(`, `new Form(`, `.Show(`/`.ShowDialog(`, `MessageBox`/`InputBox`,
   `[STAThread]`, `DoEvents`, `Thread.Sleep`/`Task.Delay`, or temp-file API remains anywhere in
   `Tags.Test`. (Earlier comment text referencing those tokens was reworded so the textual scan is
   clean.)
2. The ONLY files using `[STATestClass]`/`[STATestMethod]` are the two sanctioned dedicated files:
   - `Tags.Test/TagControllerRendering.StaTests.cs`
   - `Tags.Test/CheckBoxControllerWiring.StaTests.cs`
3. Live-control construction (`new CheckBox()`) occurs ONLY in those two STA files. Each constructs
   only unshown `CheckBox` controls (never a `Form`-derived type), never calls `Show()`/`ShowDialog()`,
   uses no message pump/timer/sleep, and disposes every control via `using`. The migrated non-STA
   tests obtain their option checkboxes from the fake viewer's tracked list rather than constructing
   controls.

Result: **PASS** — determinism and STA-confinement invariants satisfied.
