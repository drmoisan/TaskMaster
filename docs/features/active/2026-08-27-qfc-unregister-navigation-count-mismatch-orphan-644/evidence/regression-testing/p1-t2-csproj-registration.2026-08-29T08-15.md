# Regression testing — Project registration of the new test file ([P1-T2])

- Issue: #644
- Task: `[P1-T2]`
- Timestamp: 2026-08-29T08-15
- File modified: `QuickFiler.Test/QuickFiler.Test.csproj`

## Base-commit substitution applied to this task

The plan anchors this task's diff to `ecdb1c84ba8541ab67042985919cfed4df768c01`. Per the
orchestrator-authorized substitution recorded in
`evidence/baseline/phase0-instructions-read.2026-08-29T08-15.md`, the ref operand used here is
`e968a1a8804b7641380d4489c496662824d45767`, the merge commit that brought `origin/main` tip
`fa2ddefacf2c08abe18f3e3250d77da804534637` — carrying the merged #638 fix from PR #700 — onto
this branch.

**Rationale.** The #638 fix also edits `QuickFiler.Test/QuickFiler.Test.csproj`. Anchoring to the
original base would show #638's registrations alongside this plan's single added line, so the
"exactly one added line and zero removed lines" clause would fail for a reason unrelated to this
work. The substitution narrows the diff to this run's own change and widens no acceptance clause:
the clause still demands exactly one addition and zero removals.

## Line-number citation drift, recorded

The plan directs the insertion "immediately after the existing
`Controllers\QfcCollectionControllerNavigationDigitsTests.cs` entry on line 131". At the
substituted base that entry sits on **line 132**, not 131, because the #638 merge added a
registration earlier in the same `ItemGroup`. The instruction is positional and unambiguous — the
new item is inserted immediately after that entry — so it was followed as written; only the
absolute line number recorded in the plan has drifted. The inserted line landed on **line 133**.

## Edit made

```
     <Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
```

The inserted item uses the same four-space indentation and the same backslash-separated relative
form as its neighbours. The project is legacy non-SDK style, so an unlisted `.cs` file is silently
not compiled; this item is what makes the six ledger tests reachable by the test runner and is
what AC-8 verifies.

## Acceptance verification

Command: `git grep -F -n 'Controllers\QfcCollectionControllerNavigationLedgerTests.cs' -- QuickFiler.Test/QuickFiler.Test.csproj`
EXIT_CODE: 0

```
QuickFiler.Test/QuickFiler.Test.csproj:133:    <Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />
```

Exactly **one** line printed, and the command exited 0.

Command: `git diff e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler.Test/QuickFiler.Test.csproj`

```
@@ -130,6 +130,7 @@
     <Compile Include="Controllers\EfcHomeControllerSeamTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerNavigationDigitsTests.cs" />
+    <Compile Include="Controllers\QfcCollectionControllerNavigationLedgerTests.cs" />
     <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
     <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
     <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
```

Command: `git diff --numstat e968a1a8804b7641380d4489c496662824d45767 -- QuickFiler.Test/QuickFiler.Test.csproj`

```
1	0	QuickFiler.Test/QuickFiler.Test.csproj
```

**Exactly one added line and zero removed lines.**

## Line-ending integrity

`.csharpierignore` excludes `*.csproj`, so CSharpier will not normalise this file and a
line-ending regression here would not be repaired by `[P4-T1]`. Verified by byte scan:

```
crlf=511 bare-lf=0
```

The file remains uniformly CRLF; the inserted line introduced no bare LF.

EXIT_CODE: 0

Output Summary: One `Compile Include` item for
`Controllers\QfcCollectionControllerNavigationLedgerTests.cs` was inserted immediately after the
`NavigationDigitsTests.cs` entry, landing on line 133. The fixed-string search exits 0 and prints
exactly one line; the anchored diff against the substituted base
`e968a1a8804b7641380d4489c496662824d45767` shows **1 insertion, 0 deletions**. CRLF line endings
are intact. Both `[P1-T2]` acceptance clauses hold.
