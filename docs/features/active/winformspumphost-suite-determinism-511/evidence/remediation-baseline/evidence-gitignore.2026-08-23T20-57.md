# Remediation Baseline — Evidence `.gitignore` Verification and Append

Timestamp: 2026-08-23T18-59

Command:
```bash
G=docs/features/active/winformspumphost-suite-determinism-511/evidence/.gitignore
# verify the five dictated lines each exist as their own non-comment line
for L in '*.trx' '*.coverage' '*.coveragexml' 'Deploy_*/' '20[0-9][0-9]-[0-9][0-9]-[0-9][0-9]_*/'; do grep -cxF "$L" $G; done
# append the Phase 3 scratch-directory pattern only if absent
grep -qxF 'r1-p*-t*/' $G || printf 'r1-p*-t*/\n' >> $G
git diff --stat -- $G
git check-ignore -q docs/features/active/winformspumphost-suite-determinism-511/evidence/qa-gates/r1-p3-t6/probe.trx
git check-ignore -q $G
```

EXIT_CODE: 0

Output Summary:

The file already existed at 929 bytes, created alongside the raw-artifact disposition recorded in
`docs/features/active/winformspumphost-suite-determinism-511/evidence/other/raw-vstest-artifact-disposition.2026-08-23T21-40.md`.
It was not rewritten. Verification of the five dictated lines, each measured as an exact whole-line
match against a non-comment line:

| Line | Whole-line match count | Verified |
| --- | --- | --- |
| `*.trx` | 1 | yes |
| `*.coverage` | 1 | yes |
| `*.coveragexml` | 1 | yes |
| `Deploy_*/` | 1 | yes |
| `20[0-9][0-9]-[0-9][0-9]-[0-9][0-9]_*/` | 1 | yes |

Append performed: **yes**. The single line `r1-p*-t*/` was absent and was appended at the end of the
file, covering the Phase 3 per-run scratch directory `evidence/qa-gates/r1-p3-t6/`. Whole-line match
count after the append: 1. File size after the append: 939 bytes (929 + 10).

No pre-existing line was removed, reordered, or altered. `git diff --stat` on the file reports
`1 file changed, 1 insertion(+)` with zero deletions, and the diff hunk is the single added line
`+r1-p*-t*/` at the end of the file.

`Deploy_*/` was retained deliberately: it is the vstest deployment scratch directory whose default
name embeds the account and host, so removing that line would reintroduce a host-identifier leak.

Ignore-behaviour verification:

| Check | Exit code | Meaning |
| --- | --- | --- |
| `git check-ignore -q .../evidence/qa-gates/r1-p3-t6/probe.trx` | 0 | the new Phase 3 TRX path is ignored, closing the `git add -A` hazard |
| `git check-ignore -q .../evidence/.gitignore` | 1 | the `.gitignore` itself is not ignored and remains committable |
