# P4-T18 — The two consumer files are absent from the change

Timestamp: 2026-09-13T03-23

Command: a single pwsh payload that runs `git -C . diff --name-only 2405a829d6afd3b12eb7c228d57158a97cb4e2ca HEAD` together with `git -C . status --porcelain --untracked-files=all -- . ":(exclude).claude/agent-memory" ":(exclude)docs/features/potential"`, then counts entries across both listings whose path ends with each consumer file name.

EXIT_CODE: 0

```
DIFF_ENTRIES=52
STATUS_ENTRIES=2
FRAMEBUILDING_COUNT=0
DFDEEDLE_COUNT=0
```

Output Summary: both acceptance clauses hold, both counts being exactly 0. Neither consumer file appears anywhere in the anchored diff from the merge base to the current head, nor among the working-tree entries the porcelain span reports. The two names are written without backticks because neither file is touched by this change.

The status span is paired with the name-listing diff because a name-listing diff enumerates tracked changes only and cannot report an untracked addition, and the all-untracked-files option is used because the default collapses an untracked directory to a single entry. Both listings are non-empty — 52 diff entries and 2 status entries — so neither count is zero because its listing was empty.

The design significance is that the fix changes what the method raises, not what any caller does about it. The two consumer files are the sites that would have needed adjusting had the fix instead changed the method's signature or return shape; leaving them untouched is what confines the change to the failure contract. Together with P4-T11 this decides acceptance criterion 6.
