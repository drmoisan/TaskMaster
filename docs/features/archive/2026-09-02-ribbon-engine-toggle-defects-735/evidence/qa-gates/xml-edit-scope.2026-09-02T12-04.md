# Finding 1 — CustomUI Edit Scope, Reflow-Independent (P1-T9)

Timestamp: 2026-09-03T01-50
Task: [P1-T9]
Command:

```powershell
$base = git merge-base origin/main HEAD
git show "${base}:TaskMaster/Ribbon/RibbonExplorer.xml"
```

then, for the base text and for the current file, build and compare two sorted multisets:
the element multiset of `<local-name>#<id attribute or "(no id)">` over every descendant element,
and the callback multiset of `<attribute local name>=<value>` over every attribute whose local name
is `onAction`, `onChange` or `onLoad` or begins with `get`.

EXIT_CODE: 0
Merge base re-derived at run time: `a679cd082819af6788cd0fb35f4366786fab87e3`

## Why sets rather than a line diff

A line-count or numstat expectation would be unsatisfiable in principle here, because CSharpier owns
this document's layout and may reflow attributes at any time. Element and attribute multisets are
invariant under reflow, so this comparison measures the semantic edit and nothing else. (In this
particular run the formatter happened to rewrite nothing — see P1-T5 — but the check does not depend
on that.)

## Multiset sizes

| Multiset | Before | After |
|---|---|---|
| Elements (with id) | 98 | 97 |
| Callback attribute bindings | 98 | 97 |

The callback figure of 98 live occurrences before the edit independently reproduces the research
record's Claim B, which derived 98 live callback occurrences by two mutually independent
enumerations.

## Element symmetric difference — exactly one entry

```
REMOVED  button#BtnMigrateIDs
```

Count: **1**. This is the single element deletion, and it is the button whose id is
`BtnMigrateIDs`. No other element was added, removed, or had its id changed.

## Callback symmetric difference — exactly nine entries

```
ADDED    onAction=MoveEntireConversation_Click
ADDED    onAction=SaveAttachments_Click
ADDED    onAction=SaveEmailCopy_Click
ADDED    onAction=SavePictures_Click
REMOVED  onAction=BtnMigrateIDs_Click
REMOVED  onAction=MoveEntireConversation_Clicked
REMOVED  onAction=SaveAttachments_Clicked
REMOVED  onAction=SaveEmailCopy_Clicked
REMOVED  onAction=SavePictures_Clicked
```

Count: **9** — the five removed values and the four added values, exactly as required. Every entry is
an `onAction` binding; no `getPressed`, `getEnabled`, `getLabel`, `getText`, `onChange` or `onLoad`
binding appears in the difference, so no pressed-state or enabled-state callback changed.

## The four-plus-one partition

The nine callback entries decompose into exactly two groups, and the decomposition is the evidence
for F1-AC4:

| Removed value | Added replacement | Classification |
|---|---|---|
| `MoveEntireConversation_Clicked` | `MoveEntireConversation_Click` | rename |
| `SaveAttachments_Clicked` | `SaveAttachments_Click` | rename |
| `SaveEmailCopy_Clicked` | `SaveEmailCopy_Click` | rename |
| `SavePictures_Clicked` | `SavePictures_Click` | rename |
| `BtnMigrateIDs_Click` | none | removal, with the whole element |

Four renames plus one removal, totalling the five defective names that P1-T2 reported. A name was
renamed when a correctly signatured method with the intended spelling already existed on the viewer
type, and removed when no implementation exists anywhere in the solution. That the four renamed
names each have a correctly signatured twin is proven by P1-T7: the check-box arity test passes,
and it resolves every check-box `onAction` and asserts the shape `void (IRibbonControl, bool)`, so
all four post-rename names resolve with the correct signature. That the removed name had no twin is
proven by P1-T2's first failure message, in which `BtnMigrateIDs_Click` appears in the unresolved
list, and by its absence from any check-box in P1-T2's second message.

## No viewer-type method was added, renamed or removed

```
git diff --numstat a679cd082819af6788cd0fb35f4366786fab87e3 -- TaskMaster/Ribbon/RibbonViewer.cs
```

produced NO output line at all, which for `--numstat` means the path is unchanged between the merge
base and the working tree. The bindings were therefore satisfied entirely by editing the CustomUI
document, with no change to the COM-visible viewer type. This is the evidence for the second
sentence of F1-AC2.

Output Summary: The CustomUI edit is exactly one element removal and four attribute-value renames.
The element symmetric difference holds one entry, `button#BtnMigrateIDs`; the callback symmetric
difference holds nine entries, five removed and four added, all `onAction`. `RibbonViewer.cs` is
unchanged against the merge base.
