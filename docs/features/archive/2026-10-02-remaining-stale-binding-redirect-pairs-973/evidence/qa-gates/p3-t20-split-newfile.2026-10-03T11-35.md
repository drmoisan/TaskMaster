# P3-T20 create CategoryClassifierGroup.ConditionalEngine.cs (issue #973; Part H)

Timestamp: 2026-10-03T11-35
Command: CMD-SPLIT-NEWFILE (pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; ... locate the region by its two marker lines with [Array]::IndexOf, copy the lines as read, prepend #nullable enable, the five using directives, namespace and partial class opening, append the two closing braces, write CRLF with the original BOM state ...'); Glob Categories/*; CMD-LINECOUNT; CMD-CRCOUNT; CMD-BOM; Greps for `#nullable enable`, `^using `, Graph usings, namespace, partial declaration, base list and region markers; CMD-MEMBER-CENSUS
EXIT_CODE: 0
Output Summary: the new partial file was created by copying the 93 marker-bounded region lines verbatim; it is 106 lines, CRLF with a terminated last line (106/106) and carries the original's UTF-8 BOM; layout and member census exactly as D14 states.

REGION-START-LINE: 443
REGION-LINES: 93
NEWFILE-LINES: 106
BOM: True

Glob Categories/*: CategoryClassifierGroup.cs, CategoryClassifierGroup.ConditionalEngine.cs
LINECOUNT 106; CRCOUNT 106
BOM-BYTES (new file)=239,187,191 (equals the P3-T19 BOM-BYTES)
1 `#nullable enable`
2 `using System;`
3 `using System.Threading.Tasks;`
4 `using Microsoft.Office.Interop.Outlook;`
5 `using UtilitiesCS.OutlookExtensions;`
6 `using UtilitiesCS.ReusableTypeClasses;`
(`^using ` count 5, lines 2-6)
`^using Microsoft\.Graph`: 0
8 `namespace UtilitiesCS.EmailIntelligence.ClassifierGroups.Categories`
10 `    public partial class CategoryClassifierGroup`
`class CategoryClassifierGroup : ` count: 0 (no base list in this file)
12 `        #region IConditionalEngine Implementation`
104 `        #endregion IConditionalEngine Implementation`

CMD-MEMBER-CENSUS (MEMBER-COUNT: 12): Config (14), void IConditionalEngine<MailItemHelper>.Serialize() (22), AsyncAction (27), AsyncCondition (36), Condition (39), ConditionLog (53), GetOlItemString (78), Engine (93), EngineInitializer (95), EngineName (98), Message (100), TypedItem (102) - the twelve fact 11 region members.
