# P10-T6 — The `QfcItemController.FolderHandling.cs` diff inserts no `ClearFolderItems()` call

Timestamp: 2026-08-28T01-52
Command: git diff cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/Controllers/QfcItemController.FolderHandling.cs | (filter to added lines) | (search for ClearFolderItems)
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Result

The added-line filter over the `FolderHandling.cs` diff yields exactly **one** line:

```
+                _itemViewer.AddFolderItems(_folderHandler.FolderArray);
```

Searching those added lines for `ClearFolderItems` returns **no match**. `git diff --numstat` reports
`1` added and `1` deleted: the one-token, line-neutral `SetFolderItems` to `AddFolderItems` rename at
the single call site inside `AssignFolderComboBox()`, and nothing else.

A whole-file search confirms the same conclusion independently: `ClearFolderItems` does not occur
anywhere in `QuickFiler/Controllers/QfcItemController.FolderHandling.cs`, before or after this
feature.

Acceptance met: the diff contains no added line matching `ClearFolderItems`.

## The deferral

The `ClearFolderItems()` insertion is the **second half of #490 D1** and it is **deferred**.

`FEATURE/spec.md:441` records it as disposition 4 in § Sibling-collision resolution:

> Insert `ClearFolderItems()` before `SetFolderItems` at `AssignFolderComboBox()` —
> `QfcItemController.FolderHandling.cs:182` — **446** — **DEFER.** Recorded as an out-of-scope
> finding. The rename alone closes the contract defect; the clear is a behaviour change in 446's file
> and belongs to 446 or to a follow-up issue.

The rationale has two parts. First, `QfcItemController.FolderHandling.cs` is **446-owned**, and
inserting a clear is a behaviour change rather than the mechanical, compiler-forced rename that
disposition 5 authorises. Second, the insertion is gated on open item **U4** — whether
`AssignFolderComboBox()` can run more than once within a single viewer lifetime — which
`FEATURE/spec.md:603` records as **unverified**. Making a behavioural change in a sibling's file on an
unverified premise is not warranted; the rename alone closes the contract defect the issue reports.

The deferral is carried in `FEATURE/spec.md` § Out-of-Scope Findings as the entry **#490 D1 second
half** at `FEATURE/spec.md:728`, with the evidence pointer `research §5.5.1, §8.2`. It is one of the
eleven entries P10-T12 verifies. `ClearFolderItems()` itself remains declared and uncalled on the
interface; `FEATURE/spec.md:373` records that deleting it is likewise not in scope.

Pointer: `FEATURE/spec.md` § Out-of-Scope Findings, entry "#490 D1 second half" (`spec.md:728`).

Output Summary: The `QfcItemController.FolderHandling.cs` diff contains exactly one added line, the
one-token `SetFolderItems` to `AddFolderItems` rename inside `AssignFolderComboBox()`, and **no added
line matching `ClearFolderItems`**. `git diff --numstat` reports `1` added / `1` deleted, and the
literal `ClearFolderItems` does not occur anywhere in the file. The clear-insertion half of #490 D1 is
deferred to 446 or a follow-up issue because the file is 446-owned, the change is behavioural rather
than compiler-forced, and it is gated on the unverified open item U4; it is recorded in
`FEATURE/spec.md` § Out-of-Scope Findings at `spec.md:728` with an evidence pointer.
