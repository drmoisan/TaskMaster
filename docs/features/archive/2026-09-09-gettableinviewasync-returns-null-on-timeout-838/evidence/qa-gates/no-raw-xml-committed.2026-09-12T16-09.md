# P4-T20 — No raw test-result or coverage XML was committed or left untracked

Timestamp: 2026-09-13T03-25

Command: a single pwsh payload that runs `git -C . diff --name-status --diff-filter=A 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD` together with `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"`, forms the union of the added paths from the diff and the untracked paths from the status, and counts three properties of that union.

EXIT_CODE: 0

```
ADDED_COUNT=48
UNTRACKED_COUNT=3
UNION_COUNT=51
TRX_COUNT=0
COBERTURA_COUNT=0
EVIDENCE_MEMBERS=44
EVIDENCE_OUTSIDE_THREE=0
```

Output Summary: all three acceptance clauses hold. No union member's path ends with `.trx` and none ends with `.cobertura.xml`, so no raw test-result file and no raw Cobertura file was added to the repository or left sitting untracked in the worktree. All 44 evidence members of the union lie under one of the three permitted subdirectories `evidence/baseline/`, `evidence/regression-testing/` or `evidence/qa-gates/`, so `EVIDENCE_OUTSIDE_THREE` is 0 and no fourth evidence subdirectory was created.

The union is non-empty at 51 members, so none of the three counts is zero merely because the listing it reads was empty. The status span is paired with the name-listing diff because a name-listing diff cannot see an untracked path, and the all-untracked-files option is used because the default collapses an untracked directory to a single entry, which would hide a stray file inside it.

This is the evidence-hygiene outcome the plan's scratch design produces: every raw test-result and raw coverage document written during this run was directed to an out-of-repository scratch root, so only Markdown projections of those documents were ever candidates for commit. This decides acceptance criterion 16.
