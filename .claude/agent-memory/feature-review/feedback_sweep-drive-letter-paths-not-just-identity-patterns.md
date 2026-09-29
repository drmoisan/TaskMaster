---
name: sweep-drive-letter-paths-not-just-identity-patterns
description: An AC or plan gate phrased "no absolute host path" is not discharged by identity-pattern sanitize checks (account, host, worktree root, <drive>:\Users\); sweep the feature folder for any drive-letter path, because tool banners like "Using vstest.console: C:\Program Files\..." pass those gates verbatim
metadata:
  type: feedback
---

When an acceptance criterion or plan gate says "no committed file contains an absolute host path", run an
independent Grep over the whole feature folder for `[A-Za-z]:[\\/][A-Za-z]` (and `[A-Za-z]:/`) before
crediting the executor's sanitize gate. Do not accept `ACCOUNT_HITS=0 / HOST_HITS=0 / ROOT_HITS=0 /
DRIVE_USERS_HITS=0` as proof of that clause.

**Why:** #930 (2026-09-29). The plan's CMD-SANITIZE matched only the account name, the machine name, the
worktree root and a `<drive>:\Users\` prefix. The coverage runner prints `Using vstest.console:
C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`,
and the executor transcribed that banner verbatim into both the baseline and the final coverage
artifacts. All six sanitize counts read 0, the executor checked AC7 off, and only a drive-letter sweep found
the two lines. The path discloses no identity, so the *policy* purpose (CLAUDE.md Committed Test Evidence
Format) was met, but the AC's literal clause and the plan's own executor note ("use placeholders for any
value outside the repository") were not, so AC7 had to be unchecked (PARTIAL, non-blocking, two-line
placeholder fix).

**How to apply:** on every review with a host-path AC (most TaskMaster items now carry one), Grep the
feature folder for drive-letter paths as a separate step from the identity patterns; the common escapes
are tool banners (`Using vstest.console:`, `Using MSBuild:`), `vswhere` output, and `Program Files`
install paths. Classify identity-free hits as non-blocking but still fail the literal AC clause and name the
exact file:line pairs and the placeholder to substitute (for example `PROGRAM-FILES\...`). Related:
[[_shared_no_absolute_host_paths]] (identity leakage, the higher-severity case) and
[[752-review-residuals]] (an executor scope lock does not discharge the branch-level host-path sweep).
