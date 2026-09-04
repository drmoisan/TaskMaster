# Sibling-Defect Sweep ([P3-T8])

Timestamp: 2026-09-03T12-18

Command: `git -C <repo-root> grep -n -F -e ".claude" -- scripts/`

EXIT_CODE: 0

## Returned lines, verbatim (exactly 5)

```
scripts/bash/shell_qc_lib.sh:76:	# Discover shell scripts under tools/, scripts/, and .claude/lib/bash/ relative to
scripts/bash/shell_qc_lib.sh:85:	for root in tools scripts .claude/lib/bash; do
scripts/bash/shell_qc_lib.sh:335:	local include_pattern="$repo_root/tools,$repo_root/scripts,$repo_root/.claude/lib/bash"
scripts/vscode/Invoke-MSTest.ps1:142:        Coverage Exclusion Policy in .claude/rules/general-unit-test.md, which requires logic
scripts/vscode/Invoke-MSTestWithCoverage.ps1:301:                ([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^|\\)\.claude\\'
```

## Classification, one line per returned line

1. `scripts/bash/shell_qc_lib.sh:76` — NOT A PREDICATE. A comment describing the discovery roots of the shell QC library; `.claude/lib/bash/` is named as a directory to **include** in discovery, and the line performs no matching at all.
2. `scripts/bash/shell_qc_lib.sh:85` — NOT A PREDICATE. A `for` loop whose root list includes `.claude/lib/bash`; this is a discovery **inclusion** root for a different tool, the opposite of an exclusion clause.
3. `scripts/bash/shell_qc_lib.sh:335` — NOT A PREDICATE. A coverage `include_pattern` that adds `$repo_root/.claude/lib/bash` to the measured set; again an inclusion root for a different tool.
4. `scripts/vscode/Invoke-MSTest.ps1:142` — NOT A PREDICATE. A documentation comment citing the rules file `.claude/rules/general-unit-test.md` by path; it is prose inside a comment block and performs no matching.
5. `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` — EXCLUSION PREDICATE. This is the sole `.claude` exclusion clause in `scripts/`, and it is the line this item fixed: it now matches the anchored pattern against the candidate path computed relative to `$resolvedSearchRoot` rather than against the absolute `FullName`.

Output Summary: The sweep returns exactly 5 lines. Exactly one is classified `EXCLUSION PREDICATE`, and it is in `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, the file this item fixed. The other four are the three `shell_qc_lib.sh` discovery and coverage **inclusion** roots for a different tool and one documentation comment naming a rules file; none of them is an exclusion predicate and none carries the defect this item fixes. The claim is re-derived here against the post-change tree rather than carried forward from the research artifact.
