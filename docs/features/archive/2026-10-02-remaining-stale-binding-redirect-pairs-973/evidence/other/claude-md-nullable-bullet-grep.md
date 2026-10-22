# P3-T26 Part G: CLAUDE.md nullable bullet correction (issue #973; AC21)

Timestamp: 2026-10-06T18-21
Command: git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- CLAUDE.md (plus the EDIT-CLAUDE-BULLET Edit-tool replacement on line 211 and the Grep-tool observations A to G with -n, before and after, and CMD-LINECOUNT / CMD-CRCOUNT on CLAUDE.md)
EXIT_CODE: 0
Output Summary: The single Edit-tool replacement of the first sentence of line 211 applied (one match, no hook block). Greps before: A=1 B=0 C=1 D=1 E=0 F=0 G=1; after: A=0 B=1 C=1 D=1 E=1 F=1 G=1; every hit at line 211. CLAUDE.md 463 lines / 463 CR before and after. Numstat `1	1	CLAUDE.md`. Porcelain under `.claude` empty.

## Grep patterns (Grep tool, path CLAUDE.md, -n)

- A `there is no \x60Directory\.Build\.props\x60`
- B `neither root build file sets one`
- C `195 errors in \x60UtilitiesCS\.csproj\x60 on 2026-08-10`
- D `CI omits it deliberately`
- E `RxUseUnsupportedPackagesConfig`
- F `toggles VSTO signing for the TaskMaster project`
- G `Removing it loses no enforcement over any file that has opted in`

## Before the edit

BEFORE A=1 B=0 C=1 D=1 E=0 F=0 G=1
LINE-NUMBERS-BEFORE: A=211 C=211 D=211 G=211
LINECOUNT-BEFORE: 463
CRCOUNT-BEFORE: 463

## Edit

EDIT-CLAUDE-BULLET: old_string = the plan section 7 sentence beginning "No project in this repository carries a `<Nullable>` element and there is no ..."; new_string = the plan section 7 replacement sentence (neither root build file sets one; Directory.Build.props sets only RxUseUnsupportedPackagesConfig, issue #730; Directory.Build.targets only toggles VSTO signing for the TaskMaster project). Result: applied, one match. HOOK-BLOCKED: none.

## After the edit

AFTER A=0 B=1 C=1 D=1 E=1 F=1 G=1
LINE-NUMBERS-AFTER: B=211 C=211 D=211 E=211 F=211 G=211
LINECOUNT-AFTER: 463
CRCOUNT-AFTER: 463
NUMSTAT: 1	1	CLAUDE.md
PORCELAIN-.claude (git -C <execution-worktree-root> status --porcelain -- .claude): (empty)
AC21: MET (local observation)
