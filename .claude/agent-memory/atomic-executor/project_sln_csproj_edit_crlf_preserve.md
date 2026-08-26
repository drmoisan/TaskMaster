---
name: sln-csproj-edit-crlf-preserve
description: git-bash sed strips CRLF when editing TaskMaster.sln/.csproj AND CRLF plan .md files; use perl -0777 with explicit \r\n, or re-add \r after the sed pass
metadata:
  type: project
---

`TaskMaster.sln` is UTF-8 (with BOM) + CRLF. Editing it with git-bash `sed -i` (even a
pure line-delete like `/pattern/d`) rewrites the whole file with LF endings, producing a
massive noisy diff (every line changed CRLF->LF) and losing the BOM/CRLF that VS expects.

**Why:** MSYS sed does newline translation on write. A single `sed -i` delete pass on the
sln converted CRLF->LF across the entire file even though only 28 lines were the target.

**How to apply:** For `.sln`/`.csproj` line-ending-sensitive edits, either (a) use the Edit
tool with exact-string matches (preserves surrounding CRLF), or (b) `git checkout --` the file
and redo with a CRLF-preserving `perl -0777 -i -pe 's/...\r\n//g'` slurp (perl does no newline
translation on unix-like, so `\r` stays). Verify with `file <path>` (expect "with CRLF line
terminators") and `git diff --stat` (expect only the intended N-line delta, not whole-file churn).
Note the many `.csproj` files here are actually LF already, so a simple `sed` start-address using
`.` for the literal backslash works for `<ProjectReference Include=..\X\Y.csproj>` deletions —
but always re-check the `file` type before trusting sed on a CRLF file.

**This also bites plan `.md` files.** Atomic plans written by `atomic-planner` are CRLF, and every
`[P#-T#]` check-off is a `sed -i` pass, so the first check-off silently converts the whole plan to LF and
turns a 9-line change into whole-file churn. Cheapest reliable idiom, run immediately after each check-off
sed: `sed -i 's/$/\r/' <plan>.md`, then confirm with `file <plan>.md` (expect "with CRLF line terminators")
and `git diff --stat` (expect insertions == deletions == number of boxes ticked). Doing this per check-off
rather than once at the end keeps the diff readable if the run is interrupted.
