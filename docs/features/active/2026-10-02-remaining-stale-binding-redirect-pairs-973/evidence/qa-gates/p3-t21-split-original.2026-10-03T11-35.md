# P3-T21 remove the region from CategoryClassifierGroup.cs and add partial (issue #973; Part H)

Timestamp: 2026-10-03T11-35
Command: CMD-SPLIT-ORIGINAL (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; ... locate the region markers and the single declaration line, cut the blank lines directly above the #region line, drop the region, replace "public class CategoryClassifierGroup" with "public partial class CategoryClassifierGroup", write CRLF with the original BOM state ...'); CMD-LINECOUNT; CMD-CRCOUNT; CMD-BOM; Greps for the partial declaration, `partial`, `IConditionalEngine Implementation`, `#endregion Public Properties`; Read tool offset 440 limit 3; CMD-MEMBER-CENSUS; git -C <execution-worktree-root> diff --numstat a6915d62fe9d85218e5453fc5ac5cd5674b04984 -- UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
EXIT_CODE: 0
Output Summary: the region and the two blank lines above it were removed and the declaration gained `partial`; the file is 442 lines with 442 carriage returns and keeps its BOM; it now ends `#endregion Public Properties`, `    }`, `}`; 24 member declarations remain; numstat 1/98.

DECLARATION-LINE: 23
BLANK-LINES-REMOVED-BEFORE-REGION: 2
REMOVED-LINES: 95
LINES-AFTER: 442
BOM: True

LINECOUNT 442; CRCOUNT 442
BOM-BYTES=239,187,191 (equals P3-T19)
23 `    public partial class CategoryClassifierGroup : IConditionalEngine<MailItemHelper>`
`partial` count: 1
`IConditionalEngine Implementation` count: 0
Read 440 `        #endregion Public Properties`, 441 `    }`, 442 `}`
MEMBER-COUNT: 24 (the fact 11 outside-region members)
NUMSTAT: 1	98	UtilitiesCS/EmailIntelligence/ClassifierGroups/Categories/CategoryClassifierGroup.cs
