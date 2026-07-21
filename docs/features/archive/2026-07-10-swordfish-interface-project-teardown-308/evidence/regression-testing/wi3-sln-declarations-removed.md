# WI-3 — Solution Project Declarations Removed (P4-T1)

- **Timestamp:** 2026-07-11T13-25
- **Feature:** swordfish-interface-project-teardown (#308), F5

## Action

Removed from `TaskMaster.sln` the `Project(...)`/`EndProject` declaration blocks for both GUIDs:

- `{F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF}` (`UtilitiesSwordfish.NET.General`)
- `{9A04D222-2B52-4E93-9B92-CC6EF54D5848}` (`UtilitiesSwordfish.NET.Test`)

Edit performed with a CRLF-preserving perl slurp (`perl -0777`); the file remains UTF-8 (with BOM),
CRLF line terminators. (An initial `sed -i` pass was reverted via `git checkout` because it converted
CRLF to LF; the perl slurp preserves the original line endings.)

## Verification

- **Command:** `grep -nE "F2E1680E-1B15-4CF2-BAB0-54B8C8F6ABDF|9A04D222-2B52-4E93-9B92-CC6EF54D5848" TaskMaster.sln`
- After the declaration removal, no `Project(...)` line for either GUID remains (see P4-T2 for the
  final zero across declarations + config rows).
- **Output Summary:** both `Project(...)="UtilitiesSwordfish.NET.General"` and `...Test` declarations
  and their `EndProject` lines removed. `git diff --stat`: 28 deletions total (declarations + config
  rows), zero additions, no other lines changed. Delivers AC-8.
