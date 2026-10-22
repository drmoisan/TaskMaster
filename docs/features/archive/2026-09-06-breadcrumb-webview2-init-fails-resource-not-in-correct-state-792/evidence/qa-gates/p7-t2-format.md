# [P7-T2] Format step (final toolchain loop)

- Issue: #792
- Timestamp: 2026-09-17T21-03
- PASS-NUMBER: 1
- Command: `Get-FileHash -LiteralPath <path> -Algorithm SHA256` over the 29 write-set `.cs` paths and `git status --porcelain --untracked-files=all` (before); `dotnet tool run csharpier format .`; the same hashes and porcelain (after); `dotnet tool run csharpier check .` (run from `coverage/plan792-helper.ps1 -Step format -PassNumber 1` with the item worktree as the working directory; the helper's opening branch assertion `bug/breadcrumb-webview2-init-fails-resource-not-in-correct-state-792` is the worktree proof and passed; HEAD `c9b457bda44ef856306a1bc96c94683dc528993c`; console output of the two csharpier commands captured to the gitignored `coverage/p7-t2-pass1-format.log` and `coverage/p7-t2-pass1-check.log`)
- EXIT_CODE: 0
- Output Summary: format printed `Formatted 1658 files in 4695ms.` (exit 0; this line is recorded and explicitly NOT used as the rewritten count); `REWRITTEN-WRITE-SET-FILES: 0` (no write-set SHA-256 changed); `PORCELAIN-NEW-ENTRIES: 0`; `OUT-OF-SCOPE-REWRITE-COUNT: 0`; check printed `Checked 1658 files in 4924ms.` and exited 0 (the exit-0 branch held); `RESTART-REQUIRED: false`.

## Before

`WRITE-SET-COUNT: 29`, `WRITE-SET-MISSING: 0` (20 production, 9 test; `Test-Path -LiteralPath` over each).

Porcelain before (5 lines, all docs, evidence or agent-memory; convention 9 expected-dirty):

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/plan.2026-09-17T07-30.md
?? .claude/agent-memory/atomic-executor/project_pwsh_param_name_case_collision_flattens_log_array.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/other/p7-t1-outlook-closed.md
?? docs/features/active/2026-09-06-breadcrumb-webview2-init-fails-resource-not-in-correct-state-792/evidence/qa-gates/p6-t6-commit.md
```

SHA-256 before (repository-relative paths):

```
14EFB6386CF4F25B5AAC28C1081212CEE25C6E0987DBE69DE695F4ECA0ED55FB QuickFiler/Viewers/WebView2EnvironmentContract.cs
BD6E5F07AC709BF8C274045C5AC0508E6447A0E686657F742DD0195134BDF0C0 QuickFiler/Viewers/WebView2BreadcrumbHost.cs
AAD50304873955794E69DFE7957B8550D1F1AAF733ACFC37E7A3ABFC4A6F2D1D QuickFiler/Controllers/QfcItemController.ViewerSetup.cs
A2E2FA2B6E154AAC9709D1C7268E1E389C39CAA3BB1C12D3178342C11A059C09 QuickFiler/Controllers/EfcItemController.cs
9E887E98A48FC1DD5AF2043DD6D979864A3807E0020314C6420BC18419BC62E4 QuickFiler/Controllers/EfcItemController.WebViewEnvironment.cs
B5EF211E5A37889A2DBF8B50CB6099A21E6DF2C1CEDEE52BDC2A822DEB786F2B QuickFiler/Controllers/BreadcrumbBridgeRouter.cs
8E9B17C3FDAA4D987C00C6368159D576746172CA82C7F7258F0989B076764347 QuickFiler/Controllers/BreadcrumbOutboundQueue.cs
0B122F9A1ABB9C20C252612A4F616456ADCBEBF27E3603CBFCC2F7EA68316320 QuickFiler/Controllers/EfcFormController.cs
9002A324317539335AA10A33360678662D7C8869B167CBC932DA2A9E19D2F84C QuickFiler/Controllers/EfcFormController.Breadcrumb.cs
24496A45628E979FEED192B2955B0F5B36BF21888AE5CBBD112F2EE49A063733 QuickFiler/Controllers/EfcFormController.SetupAndProperties.cs
5C425D8E9D251B019ABC90DD3FC2FD2B26E62D357E97B3EE0BF46BE572C1F571 QuickFiler/Controllers/EfcFormController.EventHandlers.cs
2519157517921E8BF9E0496E13C306F78BA88F280BA21477DD57F36DB7E0DF9B QuickFiler/Controllers/EfcFormController.Actions.cs
B3CB820EFB1C5F6A000A3E98A3E3BE7D3A2669C576E2CCDA409876B94B6E049A QuickFiler/Controllers/EfcFormController.Helpers.cs
0D5A70322A19516B3A9B1486D613EF571A2C608059C9F5E55E93D6605A9E81A5 QuickFiler/Controllers/QfcCollectionController.cs
C6696DF941B9CF6886F0EE299555E95307ADE2C95203323D9F9D0A9F006D2DE8 QuickFiler/Controllers/QfcCollectionController.PopOut.cs
20FC90DB68FB1CA5199311B2494AD1FB33EF43FEDDB2764BB72359BEF14D64DE QuickFiler/Controllers/EfcHomeController.cs
E40AB978F8E0C7242873F2120F0B27EE1D2F998472C48CE814D6BD6CA571437A QuickFiler/Controllers/EfcDataModel.cs
C0E4085C52DEE82C074BE3EEC0375D7ADB6761E43AF75B90302A823C670B44DB QuickFiler/Controllers/EfcDataModel.Carry.cs
93E666BAE5DFE88AC3A56E5C58DA75B1C2F1A9E7D744BB2B405BBF34A4BE8C8B QuickFiler/Controllers/QfcItemController.cs
39891B2FF0B654CE0BA4CCCE759F5D2FF8155E60055E88A25BCD91DB843020D3 QuickFiler/Helper Classes/EfcViewerQueue.cs
961366098F99D4C8FD0D70D2A6BBEADD9CCDBAFF4380AAF606F7451433045543 QuickFiler.Test/Viewers/WebView2BreadcrumbHostIssue792Tests.cs
31142591645F40ACB833E466035A88D75E2AE46EB9BE117D6822BF21D0C3B7B1 QuickFiler.Test/Viewers/WebView2EnvironmentContractTests.cs
890D1E767BECA732C635A4F81EE8B4D5A78B1CCFE4046FB9D3621255A7F24F4B QuickFiler.Test/Controllers/BreadcrumbBridgeRouterIssue792Tests.cs
9830504A5992DAC793EE2A360B04BB452123E1A811B51B823BF4DFB8E69514B5 QuickFiler.Test/Controllers/BreadcrumbOutboundQueueIssue792Tests.cs
2CBC9ADAA003D60A6970DC3566766A6C14D33F4F1FE5ED0A95AEC021A59E1428 QuickFiler.Test/Controllers/EfcFormControllerIssue792Tests.cs
8BE4C3DFE5BC88CDDAC3CA82C100664329CCFBF8D85C8B75719D396A35A6C43F QuickFiler.Test/Controllers/QfcCollectionControllerIssue792PopOutTests.cs
0C83657C2C70BE669404434EE0DCE153EDC5F72FE94BF5328E07983EC7C42CC5 QuickFiler.Test/Controllers/EfcDataModelIssue792CarryTests.cs
8F319773F59B7CE351C72973E5C396DEDC9078FC9A991728BC5EED128A2C7422 QuickFiler.Test/Helper Classes/EfcViewerQueueIssue792Tests.cs
7CB47A85CF35E8E7DB2CC204C119B409B694EDF1EE53185040535982E98F45DF QuickFiler.Test/Controllers/EfcFormControllerTests.cs
```

## Format run

- FORMAT-EXIT: 0
- Verbatim output (1 line): `Formatted 1658 files in 4695ms.` — recorded per convention 4 and explicitly NOT used as the rewritten count. The count is 1658 = the 1641 files of the [P0-T9] baseline check plus the 17 `.cs` files this change created, so the formatter's sweep reached the new files.

## After

SHA-256 after: identical to the before list for all 29 paths (each `HA:` line of the helper output equals its `HB:` line; the helper compares per path).

- REWRITTEN-WRITE-SET-FILES: 0
- Porcelain after: the same 5 lines as before; `PORCELAIN-NEW-ENTRIES: 0`.
- OUT-OF-SCOPE-REWRITE-RESTORED: (none; no path outside the write set changed, so no `git checkout --` was run and the empty `BASELINE-DRIFT-SET` of [P0-T9] was not consulted)
- Scoped porcelain (`git status --porcelain -- '*.cs' '*.csproj' '*.sln' 'packages.config'`): 0 lines.
- BOM state of the four BOM-bearing files (`EF BB BF` leading bytes), before and after: `BreadcrumbBridgeRouter.cs` true/true, `QfcItemController.ViewerSetup.cs` true/true, `EfcFormController.cs` true/true, `EfcHomeController.cs` true/true — the formatter altered none.

## Check run

- CHECK-EXIT: 0
- Verbatim output (1 line): `Checked 1658 files in 4924ms.`
- Branch held: exit 0 printing `Checked N files in` (the alternative non-zero branch with a reported set was not taken).

## Decision

RESTART-REQUIRED: false

[P7-T3] through [P7-T8] therefore run in this pass (PASS-NUMBER 1).

## Positive control on the hash comparison

The 29 before-hashes are pairwise distinct and the comparison is evaluated per path (`$before[$p] -ne $after[$p]`), so a rewritten file would produce a differing hash and be counted; an unchanged count of 0 is a real observation rather than a constant. The porcelain-difference test was likewise exercised earlier in this plan: the [P4-T11] and [P2-T13] format runs on this same helper pattern reported rewritten files when the tree carried unformatted new files.
