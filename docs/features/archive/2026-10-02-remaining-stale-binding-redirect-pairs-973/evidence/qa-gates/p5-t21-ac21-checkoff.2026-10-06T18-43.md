# P5-T21 AC21 check-off

Timestamp: 2026-10-06T18-43
Command: Grep tool over spec.md `^- \[x\] AC21 ` and `^- \[[ x]\] AC[0-9]+ ` (count); Read of the named artifacts; live Grep `<Nullable` over glob *.{csproj,props,targets,vbproj,fsproj}; live Grep for the bold lead-in on CLAUDE.md
EXIT_CODE: 0
Output Summary: AC21 met and checked off. The before Greps read A=1 B=0 C=1 D=1 E=0 F=0 G=1 and the after Greps read A=0 B=1 C=1 D=1 E=1 F=1 G=1, every hit at line 211. Numstat is 1/1, the file is 463/463 before and after, and there is no HOOK-BLOCKED line. The `.md` diff is CLAUDE.md plus feature-folder paths, and nothing under `.claude/` is in the diff. The bold lead-in is still at line 211, and `<Nullable` occurs in no project, props or targets file.

Artifacts read:
- evidence/other/claude-md-nullable-bullet-grep.md: EXIT_CODE 0; `BEFORE A=1 B=0 C=1 D=1 E=0 F=0 G=1`; `AFTER A=0 B=1 C=1 D=1 E=1 F=1 G=1`; LINE-NUMBERS all 211; `NUMSTAT: 1	1	CLAUDE.md`; LINECOUNT/CRCOUNT 463/463 before and after; `HOOK-BLOCKED: none`; `AC21: MET (local observation)`.
- evidence/qa-gates/p4-t12-footprint.2026-10-06T18-34.md (EXIT_CODE 0): the quoted `.md` diff lists CLAUDE.md plus feature-folder paths only; `git diff --name-only PLAN-START-HEAD -- .claude` is empty.

Live observations (2026-10-06): Grep `<Nullable` over *.csproj, *.props, *.targets, *.vbproj and *.fsproj finds no match. Grep `**Do not add` with the flag code span at CLAUDE.md:211 finds 1 match.

SPEC-LINE: `- [x] AC21 (CLAUDE.md bullet reworded, conclusion preserved; ...` (criterion text unchanged)
