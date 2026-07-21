# Maintainer Flags — DvgForm.Designer.cs Scope Conflict (Issue #364, Batch 5)

- Timestamp: 2026-07-19T09-40
- Task: [P5-T6]

## Default handling (applied): DvgForm.Designer.cs left non-opted-in

`UtilitiesCS/HelperClasses/DvgForm.Designer.cs` is a WinForms Designer-generated file. Per the repository "do not touch Designer files" convention, it is left NON-opted-in (no `#nullable enable` pragma, `InitializeComponent`/generated members untouched). Verified byte-unchanged:

- `git status --short UtilitiesCS/HelperClasses/DvgForm.Designer.cs` → empty (no modification).
- `#nullable` pragma count in the file → 0.

Because `#nullable enable` is lexical/per-file, the Designer file's members remain in an oblivious nullable context, emit no CS8618/CS8625, and do NOT cross-block the opted-in hand-written partial `DvgForm.cs` (which received the pragma and the single `object sender` -> `object? sender` annotation). The pragma-only build confirms zero CS86xx for the opted-in `DvgForm.cs` and no CS86xx from the oblivious Designer file.

## Epic-scope conflict (FLAGGED to the maintainer)

The epic lists all 43 files (which explicitly includes `DvgForm.Designer.cs`) as receiving a `#nullable enable` pragma and reaching zero CS86xx. That conflicts with the "do not touch Designer files" convention. This is FLAGGED, not silently resolved:

- Default (applied here): treat `DvgForm.Designer.cs` as a documented exception that stays non-opted-in (oblivious), keeping the file byte-identical. This is durable (Designer regeneration would strip any manually-added pragma) and behavior-safe.
- Maintainer-decision fallback (only if all 43 must be opted-in): the sole permitted change is annotating the generated field as `private IContainer? components = null;` (annotation-only, matches current WinForms templates, changes no behavior), still without touching `InitializeComponent`.

The default is applied; the fallback is surfaced for the maintainer's decision.
