# AC10 verification — build-file discipline (Issue #824, task P4-T7)

Timestamp: 2026-09-09T15-53

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; $b = git merge-base HEAD origin/main; git add -N .; git status --porcelain --untracked-files=all; Write-Output "---"; git diff --stat $b -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj" "UtilitiesCS/UtilitiesCS.csproj"'`, issued with a `HEAD`-anchored companion span per the adaptation recorded in `evidence/other/executor-deviations.2026-09-09T15-28.md`, and with the porcelain listing filtered to entries naming a project file.

EXIT_CODE: 0

## This is the one gate the P0-T15 finding materially affects

Both anchors were run. They disagree, and the disagreement is fully explained.

| Observation | Result |
|---|---|
| `git diff --stat <merge-base> -- <both project files>` | `UtilitiesCS.Test/UtilitiesCS.Test.csproj \| 4 ++++`, `1 file changed, 4 insertions(+)` |
| `git diff --stat HEAD -- <both project files>` | **empty** |
| `git status --porcelain --untracked-files=all`, entries naming a project file | **none** |

### The four inherited insertions, named rather than summarised

`git diff <merge-base> HEAD -- "UtilitiesCS.Test/UtilitiesCS.Test.csproj"` attributes all four added
lines to commits already present at `HEAD` when this run began:

```
+    <Compile Include="OutlookObjects\Folder\FolderPredictorTests.SuggestionsAndRecents.cs" />
+    <Compile Include="OutlookObjects\Folder\FolderPredictorTests.FolderLookupAndUiSeams.cs" />
+    <Compile Include="OutlookObjects\Folder\FolderPredictorTests.CreateFolderWorkflows.cs" />
+    <Compile Include="OutlookObjects\Folder\FolderPredictorTests.TestSupport.cs" />
```

Every one registers a `FolderPredictorTests` partial-class file under `OutlookObjects\Folder\`. That
is the owned file set of a sibling child of this epic, not of #824. This feature adds no test file
and touches neither project file.

### Why the `HEAD`-anchored result is the one this gate is judged on

`HEAD` at the time this task ran is the commit this run started from, and every change this run has
made is uncommitted work on top of it. A `HEAD`-anchored diff therefore shows exactly this run's own
footprint and excludes only commits this run did not make. It is empty, so this feature changed
neither project file.

The merge-base anchor cannot express that distinction on this branch, because the worktree was
fast-forwarded onto the epic integration tip after the plan cleared preflight. Judging the gate on
the merge-base result would attribute a sibling's four `<Compile Include>` additions to #824, which
is precisely the misattribution P0-T15 exists to detect and which its rationale describes.

## Assessment against AC10

AC10 permits either no change at all to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`, which is the
preferred outcome, or exactly one added line being a single `<Compile Include>` entry for this
feature's own new test file. The delivered outcome is **no change at all**, which is the preferred
one. It was achieved by placing all four new tests in `ILGlobals_Tests.cs`, which is already
registered at `UtilitiesCS.Test/UtilitiesCS.Test.csproj:264` and had 367 lines of headroom at
baseline. The one-added-line fallback was not exercised.

`UtilitiesCS/UtilitiesCS.csproj` is unchanged under both anchors: the fix edits an already-registered
production file in place and adds no new production file.

No reordering, reformatting, or whitespace normalisation of any existing item was performed in
either project file, because neither was opened for editing at any point in this run.
