---
name: project-985-r1-scratchpad-identity-and-double-write-seams
description: "#985 R1 remediation seams - the session scratchpad's encoded directory name carries the account name, so a plan must never spell it (define SCRATCH as 'the scratchpad named in the executor's environment'); a pre-commit identity sweep must read diff-added lines AND untracked files with run-time-derived tokens plus synthetic and .git-pointer controls; a reflowed assemblyBinding start tag makes the sync and normalisation passes write one app.config twice"
metadata:
  type: project
---

Remediation cycle 1 of #985 (2026-10-09) was driven by review B-1: the round-0 plan wrote the encoded session-scratchpad folder through `$env:LOCALAPPDATA`, which removed the drive-rooted path but kept the account name inside the encoded segment (`C--Users-<user>-...`).

**Why:** the caller-supplied scratchpad path always embeds the account name in its encoded worktree segment, so any plan that copies it leaks the identifier even when it avoids a drive-rooted path; the CI hygiene pattern does not catch it (no colon).

**How to apply:**
- Define `SCRATCH` as "the session scratchpad named in the executing agent's own environment block"; never spell it, not even via an environment variable. Have Phase 0 rewrite helper scripts into it from the prior plan's cited line spans.
- An identity sweep that runs before the commit must read `git diff <base>` added lines (merge base vs working tree) plus every `git ls-files --others --exclude-standard` file; after the commit run the same script over `<base>...HEAD` and report it in the return message (no file, so the tree stays clean). Tokens: leaf of `$env:USERPROFILE`, 8.3 short form from `$env:TEMP`, `$env:COMPUTERNAME`, `git config user.email`; controls: synthetic `x<token>y` per token and the worktree `.git` pointer file.
- To reproduce a duplicate `WrittenPath` in `Repair-PackageManifestConsistency.ps1`, split the `<assemblyBinding` start tag across two lines in a stale app.config fixture: the sync pass writes the redirect, then `ConvertTo-AppConfigText` collapses the tag and writes again. Guard with a sibling `It` proving both rewrites landed.
- `Invoke-BindingRedirectReconciliation` already matches names case-insensitively, so a case-variant duplicate block only produced a redundant repair record; the regression test asserts the repair count, not the text.
- Related: [[project-985-r0-rehearsal-fidelity-and-hygiene-path-seams]], [[runtime-derived-account-token-pattern]].
