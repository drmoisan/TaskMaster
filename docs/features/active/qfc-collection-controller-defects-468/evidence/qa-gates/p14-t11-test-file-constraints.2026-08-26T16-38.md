# [P14-T11] Test-file constraints (AC-22)

Timestamp: 2026-08-26T16-38

Command:

```
grep -c '\[TestMethod\]' QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
git show 61edc19b:QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs | grep -c '\[TestMethod\]'
wc -l QuickFiler.Test/Controllers/QfcCollectionController*.cs
grep -n 'Compile Include' QuickFiler.Test/QuickFiler.Test.csproj
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

All three constraints hold. The `[TestMethod]` count of the existing
`QfcCollectionControllerTests.cs` is **unchanged at 13**; every new test file is **under 500 lines**;
and the five `Compile Include` entries sit **contiguously between the dark-mode entry at line 120 and
the datamodel entry at line 126**.

---

## 1. `[TestMethod]` count of the existing test file

| Measurement | Value |
|---|---|
| P0-T15 baseline (`evidence/baseline/p0-t15-source-facts.2026-08-26T08-25.md`, section 5) | **13** |
| Current tree | **13** |
| Delta | **0** |

Decision D12 required this. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is exactly
500 lines — at the repository cap — so it could not receive a new test method without first being
split, which is out of scope. Its only change in this feature is the `_itemGroupsToMove` injection
type at its field-setup site, made as part of the `#469` defect 3 fix; that edit replaces a type name
and adds no method.

The count is a direct re-measurement of the same command the baseline used, against the same file, so
the two figures are comparable without adjustment.

## 2. Line counts of the five new test files

The 500-line cap in `.claude/rules/general-code-change.md` applies to test code as well as production
code.

| File | Lines | Under 500? |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionController.TestSupport.cs` | 154 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` | **494** | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | **497** | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs` | 432 | yes |
| `QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs` | 183 | yes |

All five are under the cap. Two of them — 494 and 497 — sit within 6 and 3 lines of it respectively.
That headroom is worth recording plainly: neither file can absorb another test method, and a future
change to either must extract before it adds. This is the same condition that forced D12's
five-file distribution in the first place.

These figures are pre-final-format. P15-T7 re-measures them **after** the final CSharpier pass,
because formatting can change line counts, and that re-measurement is the one that governs.

### The two changed existing test files, for completeness

| File | Lines | Under 500? |
|---|---|---|
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | **500** | at the cap, unchanged from the P0-T15 baseline of 500 |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDarkModeTests.cs` | 155 | yes |

`QfcCollectionControllerTests.cs` is at exactly 500 and was at exactly 500 at the base commit. This
feature did not push it over and did not bring it under; the condition is pre-existing and is claimed
by open issue #623.

## 3. The five `Compile Include` entries and their surrounding entries

`QuickFiler.Test/QuickFiler.Test.csproj`, lines 119 through 126, verbatim:

```
119    <Compile Include="Controllers\QfcCollectionControllerTests.cs" />
120    <Compile Include="Controllers\QfcCollectionControllerDarkModeTests.cs" />
121    <Compile Include="Controllers\QfcCollectionController.TestSupport.cs" />
122    <Compile Include="Controllers\QfcCollectionControllerDefects468Tests.cs" />
123    <Compile Include="Controllers\QfcCollectionControllerDefects468MoveTests.cs" />
124    <Compile Include="Controllers\QfcCollectionControllerDefects468ConversationTests.cs" />
125    <Compile Include="Controllers\QfcCollectionControllerLayout.StaTests.cs" />
126    <Compile Include="Controllers\QfcDatamodelTests.cs" />
```

| Requirement from D13 | Observed |
|---|---|
| the five entries are consecutive | lines **121-125**, no intervening line |
| immediately after the dark-mode entry | dark-mode is line **120**; the first new entry is **121** |
| immediately before the datamodel entry | the last new entry is **125**; datamodel is **126** |
| in the D12 order | TestSupport, Defects468, Defects468Move, Defects468Conversation, Layout.StaTests — matches |

The insertion point is exact because that item group is shared with sibling epic children. Keeping the
five entries contiguous and pinned between two stable neighbours minimises the merge-conflict surface.
Both merges of `origin/epic/quickfiler-bug-family-integration` into this branch were clean at this
file, which is the practical confirmation that the choice worked.

The diff at this file is five added lines and zero removed lines. No `PackageReference`, `Reference`,
`Analyzer`, or property element was touched.

## Acceptance verification

- The artifact exists.
- The `[TestMethod]` count is unchanged: 13 at baseline, 13 now.
- Every new test file is under 500 lines: 154, 494, 497, 432, 183.
- The five entries sit between the dark-mode entry (line 120) and the datamodel entry (line 126).
