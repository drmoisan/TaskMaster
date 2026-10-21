# Phase 7 — Project-File and New-Source Gate

Timestamp: 2026-08-26T11-34
Task: [P7-T7]
Command: three commands, listed individually below
EXIT_CODE: 0

`363bfcdd4da5a24743ee665ea9fd124bc42239ff` is `BASELINE_SHA`, recorded by [P0-T2].

## Output Summary

**All three commands produced no output lines.** The acceptance condition holds. This artifact
carries acceptance criterion AC-20.

### 1. No project, props, or targets file was edited

```
git diff --name-only 363bfcdd4da5a24743ee665ea9fd124bc42239ff -- "*.csproj" "*.props" "*.targets"
```

No output.

Both `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj` are legacy non-SDK
projects with explicit `Compile Include` entries, and both are unowned. No new `.cs` file was
created, so no `Compile Include` entry needed adding. The two owned test files were already
registered in `QuickFiler.Test.csproj`.

Note that the pre-existing analyzer version skew described in
`evidence/baseline/msbuild-analyzers.2026-08-26T10-42.md` sits in `UtilitiesCS.csproj` and other
first-party project files. It was **not** corrected, precisely because correcting it would edit a
project file. It was worked around by installing the two missing analyzer package versions into the
git-ignored `packages/` folder, which this gate correctly does not see.

### 2. No `.cs` file was added

```
git diff --name-only --diff-filter=A 363bfcdd4da5a24743ee665ea9fd124bc42239ff -- "*.cs"
```

No output.

### 3. No untracked `.cs` file exists

```
git ls-files --others --exclude-standard -- "*.cs"
```

No output.

This third command is required because `git diff` never lists untracked files, and this plan's only
commit ([P7-T35]) runs after this task. A forbidden newly created `.cs` file would still be
untracked at this point and therefore invisible to the first two commands. It is not present.

## Related observation

While running the [P7-T8] inventory, one untracked path outside the owned surface was found:
`.claude/state/powershell-batch-budget.default.json`. It is not a `.cs` file, so it did not affect
this gate. It was transient bookkeeping written by a repository hook in response to the two
PowerShell coverage-analysis scripts created in the session scratchpad, and its contents were
nothing but a cap count and the two scratchpad paths. It was deleted rather than committed, because
`.claude/**` is outside this feature's owned surface. Details are in
`evidence/qa-gates/changed-file-inventory.2026-08-26T11-34.md`.
