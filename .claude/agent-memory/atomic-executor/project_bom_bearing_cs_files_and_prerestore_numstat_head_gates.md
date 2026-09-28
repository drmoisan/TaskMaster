---
name: bom-bearing-cs-files-and-prerestore-numstat-head-gates
description: Some QuickFiler .cs files carry a UTF-8 BOM (ViewerSetup, BreadcrumbBridgeRouter) so a whole-file normalisation rewrite silently adds a line-1 hunk and breaks tight numstat bounds; a mutation task's "git diff --numstat HEAD shows 0 0 after restore" clause is unsatisfiable while the same phase's earlier edit to that file is still uncommitted; a CSharpier-wrapped const alias defeats a single-line initializer literal
metadata:
  type: project
---

Three Phase 4 (#792, 2026-09-17) findings that cost a re-run each.

1. **BOM state is per-file, not per-repo.** The caller's brief said "C# working copies here are CRLF, no BOM"; in fact `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` carry EF BB BF at HEAD while the other eight Phase 4 targets do not. A normalisation pass that rewrites the whole file with `UTF8Encoding($false)` strips it, producing a `-﻿using System;` / `+using System;` hunk at line 1 and pushing numstat from 3/8 to 4/9 against a "≤4 insertions, 7 or 8 deletions" bound. The Edit tool preserves CRLF and the BOM on its own, so do not normalise at all after Edit-based changes; only Write-created files need a CRLF pass, and check `[System.IO.File]::ReadAllBytes(path)[0] -eq 0xEF` per file before touching encoding.

2. **Restore-proof gates anchored to HEAD are dead before the phase commit.** [P4-T4] asked for `git diff --numstat HEAD -- <site3 file>` to read `0 0` after a temporary mutation was reverted, but HEAD was the Phase 3 commit and [P4-T3]'s rewrite of the same file was still uncommitted, so the command necessarily printed `18 32`. Prove restoration with a SHA-256 of the bytes captured immediately before the mutation plus `git diff --no-index --numstat <snapshot> <file>` (prints nothing for identical files, exit 0), and record the HEAD numstat as observed with the reason. At preflight, flag any "numstat HEAD shows 0 0" clause whose file is edited earlier in the same uncommitted phase.

3. **Const alias initializer wraps.** `internal const string IncognitoArgument = WebView2EnvironmentContract.AdditionalBrowserArguments;` is 105 columns at 8-space indent; CSharpier 1.2.6 breaks after `=`, so a `-SimpleMatch` on the whole `X = Y;` returns 0 whatever is written (`csharpier check` exits 0 on the two-line shape, so it is the formatter's own output). Verify with the two adjacent single-line halves or a `(?s)` regex over the raw text. Same class as [[csharpier-chain-wrap-defeats-singleline-search-gates]] but for a declaration, not a call chain.

Also observed: a plan clause "`catch (OperationCanceledException)` returns 1" undercounted because the moved sibling method already carried one — always take the HEAD count of a literal before asserting the post-edit count ([[project_plan_authoring_time_token_counts_are_undercounts]]).
