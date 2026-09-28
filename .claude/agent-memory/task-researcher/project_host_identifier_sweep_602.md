---
name: host-identifier-sweep-602
description: "#602 host-identifier sweep research (2026-09-12): Power Query additionalSymbolsDirectories does NO variable substitution (source-verified); ripgrep-vs-git one-file deltas; basename $USERPROFILE is wrong under Git Bash; Grep count-mode trailer with head_limit 1 counts large populations cheaply"
metadata:
  type: project
---

Research for issue #602 (repository-wide account/host/profile-path leak) was completed 2026-09-12 at
`docs/features/active/2026-09-12-host-identifier-leakage-sweep-602/research/2026-09-12T16-25-...`.

Non-derivable findings:

- **Power Query editor setting.** `powerquery.client.additionalSymbolsDirectories` is described in the
  extension manifest as "absolute file system paths"; the client passes the strings through
  `path.normalize` and `vscode.Uri.file` only (WebFetch of `microsoft/vscode-powerquery` master,
  `client/src/extension.ts` and `client/src/librarySymbolManager.ts`). So `${workspaceFolder}` is
  likely inert for that key, and a bare relative path is no better. The maintainer's AC still names
  the environment-reference form; record inertness in the change description rather than choosing
  a relative fallback.
- **Ripgrep vs `git grep` deltas are real but small.** At the same commit every population agreed
  except the profile-path union (rg +1) and an archive/remainder bucket shift (sums identical). Not
  explained by untracked, ignored, or mixed-separator files; likely NUL/UTF-16 binary-detection
  differences. Always let tracked-only `git grep` govern and report the delta rather than resolve it.
- **`basename "$USERPROFILE"` does not yield the account leaf in Git Bash** (backslash path); the
  prior artifact carried that defect. Derive tokens in PowerShell (`Split-Path -Leaf`) or strip
  through the last separator of either kind.
- **Cheap large counts without a shell:** Grep tool `output_mode: count` with `head_limit: 1` still
  prints the trailer "across N files" for the full result set, so a 1,000-file population can be
  counted without dumping paths.
- The stem-only host files (host token minus trailing digits) were exactly three; two derivations
  (set difference and per-file probe) agreed. The stem rule must run AFTER the full-token rule.

**Why:** these are the facts a future sweep or re-measurement will otherwise re-derive expensively or
get wrong (the `basename` defect was already propagated once).

**How to apply:** when asked to re-measure host-identifier populations or to advise on the
`.vscode/settings.json` symbols path, start from these rather than from the older 13-45 artifact.
See [[_shared_no_absolute_host_paths]].
