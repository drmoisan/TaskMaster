# Phase 0 — Verbatim Pre-change Text of the Three UT2 Exemption Bullets

Timestamp: 2026-09-14T08-12

Source: CLAUDE.md lines 305, 306 and 307 as returned by the Read tool (offset 305, limit 3). The three lines are reproduced below character for character, including their four-space leading indent and their inline-code backtick formatting, with no paraphrase and no reformatting.

```
    - (a) VSTO add-in lifecycle classes (entry points, ribbon event handlers, COM utility registration) that cannot be unit-tested without a live Outlook process;
    - (b) WinForms form-derived classes and Designer-generated code;
    - (c) Outlook Interop event handler classes in `TaskVisualization`, `QuickFiler`, `TaskMaster`, `ToDoModel`, and `Tags` that directly depend on `Microsoft.Office.Interop.Outlook.Application`, `MailItem`, `Store`, or `MAPIFolder` without an injectable seam.
```
