# P2-T10 footprint and scope boundary (AC8)

Timestamp: 2026-09-30T12-40
Command: git diff --name-only 039cf779110df3313b3324299d019cabfccce980 with git status --porcelain --untracked-files=all; the scoped pair over UtilitiesCS UtilitiesCS.Test; the third pair over TaskMaster.runsettings scripts config coverage.config
EXIT_CODE: 0

Output Summary:
Full pair, `git diff --name-only MERGE-BASE` (MERGE-BASE 039cf779110df3313b3324299d019cabfccce980), verbatim:
.claude/agent-memory/atomic-planner/MEMORY.md
.claude/agent-memory/atomic-planner/project_945_sortemail_trysave_directory_seam_plan_seams.md
.claude/agent-memory/orchestrator/MEMORY.md
.claude/agent-memory/orchestrator/planner-prompt-needs-issue-and-branch-lines-every-round.md
.claude/agent-memory/orchestrator/read-the-clock-with-git-var-when-pwsh-is-refused.md
UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/other/preflight-clearance.2026-09-30T08-28.md
docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/issue.md
docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/plan.2026-09-30T07-20.md
docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/research/2026-09-30T07-30-sort-email-attachment-test-creates-directory-at-repository-root-research.md
docs/features/potential/promoted/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root.md

Full pair, `git status --porcelain --untracked-files=all`: two modified source files (SortEmail_Tests.cs, SortEmail.cs), two modified feature files (issue.md, plan.2026-09-30T07-20.md) and 27 untracked artifacts, all under docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/ (baseline/, other/, qa-gates/, regression-testing/); no untracked path outside that folder.

Scoped pair over UtilitiesCS UtilitiesCS.Test:
`git diff --name-only MERGE-BASE -- UtilitiesCS UtilitiesCS.Test`:
UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs
`git status --porcelain --untracked-files=all -- UtilitiesCS UtilitiesCS.Test`:
 M UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs
 M UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs

Third pair over TaskMaster.runsettings scripts config coverage.config: `git diff --name-only MERGE-BASE -- ...` printed nothing; `git status --porcelain --untracked-files=all -- ...` printed nothing.

INHERITED-AND-EXCLUDED:
- Clause A (P0-T3): the preflight-clearance artifact, issue.md, the plan file, the research file, docs/features/potential/promoted/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root.md, and the evidence artifacts written by P0-T1 and P0-T2 before P0-T3 captured the set
- Clause B: every path under .claude/agent-memory/ (the five listed above)
THIS-ITEM-FOOTPRINT: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs; UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs; and evidence artifacts under docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/evidence/ (every other entry begins docs/features/active/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root-945/)
SCOPED-SOURCE-PATHS: UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs; UtilitiesCS.Test/EmailIntelligence/SortEmail_Tests.cs (exactly these two)
TOOLING-CONFIG-PATHS: NONE
INHERITED-OUTSIDE-FEATURE: docs/features/potential/promoted/2026-09-30-sort-email-attachment-test-creates-directory-at-repository-root.md
Every path beginning .claude/ in the recorded lists begins .claude/agent-memory/ (Clause B).

Hashes:
SRC SHA256: 195BABDB966DFB24CEF1C8F7681B59B8CDBA84C1C045B57FB8F8060FE048708B (equals FIX-HASH-SORTEMAIL of P1-T7)
TST SHA256: 791E2B9E00C565C428939F0A3A26B8DE4A613AC492B5D16F80A2CD9627F2957E (equals the P2-T1 after-format hash)
RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57 (equals RUNSETTINGS-HASH of P0-T4)
