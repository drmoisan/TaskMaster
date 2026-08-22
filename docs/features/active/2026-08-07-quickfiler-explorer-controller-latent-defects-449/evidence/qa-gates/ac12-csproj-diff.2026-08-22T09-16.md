# AC-12 — Shared-Surface Project-File Diff (Issue #449, [P7-T14])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`
Merge-base SHA (from [P0-T7]): `c551eabab0aa0a6b1a284252811a2e1de819634e`
HEAD: `05156a3adca741bb3cdfa4d92da836f87814e600`

Command:
```
git diff c551eabab0aa0a6b1a284252811a2e1de819634e..HEAD -- QuickFiler.Test/QuickFiler.Test.csproj
```
EXIT_CODE: 0

## Full diff (verbatim, complete — nothing elided)

```diff
diff --git a/QuickFiler.Test/QuickFiler.Test.csproj b/QuickFiler.Test/QuickFiler.Test.csproj
index 13e522e1..8d3eb5aa 100644
--- a/QuickFiler.Test/QuickFiler.Test.csproj
+++ b/QuickFiler.Test/QuickFiler.Test.csproj
@@ -117,6 +117,8 @@
     <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
     <Compile Include="Controllers\QfcDatamodelTests.cs" />
     <Compile Include="Controllers\QfcDatamodelLivenessTests.cs" />
+    <Compile Include="Controllers\QfcExplorerController.ConversationViewTests.cs" />
+    <Compile Include="Controllers\QfcExplorerControllerTests.cs" />
     <Compile Include="Controllers\QfcInitEmailQueueZeroBatchTests.cs" />
     <Compile Include="Controllers\QfcFormControllerTests.cs" />
     <Compile Include="Controllers\QfcFormControllerSeamTests.cs" />
```

## The diff contains ONLY added `Compile Include` lines

**One hunk. Two added lines. Zero removed lines. Zero modified lines.**

Both added lines are `<Compile Include>` entries, and both sit in the `Controllers` item group
**adjacent to the `QfcDatamodelLivenessTests` entry**, exactly where [P1-T2] specifies. The hunk spans
source lines 117-123 — the three context lines above, the two additions, and the three context lines
below. Nothing else in the file changed.

Both lines use **CRLF**, matching the rest of the file. The file remains `XML 1.0 document, ASCII
text, with CRLF line terminators`, and it retains its lack of a terminating newline, so its true line
count is **486** (`grep -c ''`); `wc -l` reports 485.

`*.csproj` is listed in `.csharpierignore`, so CSharpier never processes this file and could not have
reformatted it as a side effect of the repository-wide format pass in [P7-T2].

## The `Form1` regions owned by sibling child #491 are UNTOUCHED

The hunk covers pre-change lines 117-123 only. The regions owned exclusively by sibling child #491
are at pre-change lines **161-166** (the `Form1` compile region) and **180-182** (the `Form1.resx`
`EmbeddedResource`). **The diff touches no line in either region** — the nearest changed line is 38
lines above the start of the first one, far outside git's three-line merge context.

Post-change, those regions are intact and merely shifted down by the two added lines:

```
   163	    <Compile Include="Form1.cs">
   164	      <SubType>Form</SubType>
   165	    </Compile>
   166	    <Compile Include="Form1.Designer.cs">
   167	      <DependentUpon>Form1.cs</DependentUpon>
   168	    </Compile>
...
   181	  <ItemGroup>
   182	    <EmbeddedResource Include="Form1.resx">
   183	      <DependentUpon>Form1.cs</DependentUpon>
   184	    </EmbeddedResource>
   185	  </ItemGroup>
```

Their content is byte-identical to the merge base; only their line numbers moved, which is not a
textual change and produces no merge conflict.

### Why line 119 rather than a tail append

The placement is deliberate merge-conflict avoidance, per [P1-T2]. A tail append after pre-change line
158 would have sat **within git's three-line merge context** of the `Form1` region beginning at line
161, so sibling #491's removal of the `Form1` entries would have conflicted with this child's
addition. Line 119 is 42 lines clear of that region, so the two children's edits to this shared
surface are textually independent and merge cleanly.

## AC-12 reconciliation — two lines, not one

**AC-12's "exactly one appended line" is SUPERSEDED by two appended lines.** The cause is the [P6-T14]
500-line cap split: `QfcExplorerControllerTests.cs` reached 569 lines, so its conversation-view tests
moved into `QfcExplorerController.ConversationViewTests.cs`, and a second file requires a second
compile entry. [P6-T14] is the plan's own provision for exactly this case and it directs that the
reconciliation be recorded in the [P7-T27] and [P7-T31] check-off notes, which it is.

Correspondingly, **AC-16's project-file figure of 485 is SUPERSEDED by 486**. The progression is
484 pre-change -> 485 after [P1-T2] -> **486** after the [P6-T14] second append.

Both entries satisfy every substantive constraint AC-12 imposes: they are `<Compile Include>` lines
and nothing else, they sit in the `Controllers` item group adjacent to the `QfcDatamodelLivenessTests`
entry, they are CRLF, and they leave the `Form1` regions untouched. The full reconciliation is
recorded in `../other/test-file-size.2026-08-22T09-16.md`.

## Output Summary

`git diff <merge-base>..HEAD -- QuickFiler.Test/QuickFiler.Test.csproj` shows **one hunk containing
exactly two added lines and no other change**. Both are `<Compile Include>` entries in the
`Controllers` item group adjacent to the `QfcDatamodelLivenessTests` entry, in CRLF. **The diff
touches no line of the `Form1` compile region (pre-change `:161-166`) or the `Form1.resx`
`EmbeddedResource` region (pre-change `:180-182`)**, both owned exclusively by sibling child #491;
those regions are byte-identical to the merge base and merely shifted by two lines. AC-12's "exactly
one appended line" is superseded by two, and AC-16's figure of 485 by **486**, because the [P6-T14]
file-size split required a second test file.
