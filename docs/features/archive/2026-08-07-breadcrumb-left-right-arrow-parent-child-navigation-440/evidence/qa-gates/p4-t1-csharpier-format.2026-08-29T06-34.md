# Phase 4 — Formatting Step (issue #440, plan task P4-T1)

Timestamp: 2026-08-29T06-34

Scoped to the three owned paths per Global rule 10. A repository-wide mutating pass
would rewrite pre-existing formatting drift elsewhere in the tree and break AC-12's
exactly-three-files diff. The unscoped repository-wide verification runs immediately
afterwards at P4-T2.

Command:

```
dotnet tool run csharpier format UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
```

EXIT_CODE: 0 (expected 0)

Tool output:

```
Formatted 3 files in 1680ms.
```

That line reports the number of files **processed**, not the number rewritten, so the
exit code and that line alone cannot distinguish a clean run from a repairing one.
The before-and-after SHA-256 digests below are the observation that does.

## SHA-256 digests before the run

| File | Digest |
| --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | `EE21626D018C8348D461AE61E9DB7B8888B55040E6CBCDEA14E23F8672305EDC` |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | `8871F54BEED14CDA761ADD3A6F1988FDD85A21F83534A1400417E875C448996F` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | `45ECBF941A4C109AE13B8EC7B6B98BA8B6DE565303E95DE339D1B0EA010DFA8F` |

## SHA-256 digests after the run

| File | Digest |
| --- | --- |
| `UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs` | `EE21626D018C8348D461AE61E9DB7B8888B55040E6CBCDEA14E23F8672305EDC` |
| `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs` | `8871F54BEED14CDA761ADD3A6F1988FDD85A21F83534A1400417E875C448996F` |
| `UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs` | `45ECBF941A4C109AE13B8EC7B6B98BA8B6DE565303E95DE339D1B0EA010DFA8F` |

Every digest is unchanged.

## Rewritten-file count

**0**. The formatter rewrote no file, so the Global rule 11 restart condition is not
triggered and the loop proceeds to P4-T2. No restart occurred in this sequence.

## Post-run scoped status

Command: `git status --porcelain -- UtilitiesCS UtilitiesCS.Test`

```
M  UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbStateModelSequenceTests.cs
M  UtilitiesCS.Test/OutlookObjects/Folder/FolderBreadcrumbBridgeRouterTests.cs
M  UtilitiesCS/OutlookObjects/Folder/BreadcrumbStateModel.cs
```

Still exactly three entries, all with the staged-modification status field `M `,
unchanged from the P3-T5 span-2 observation. The formatter introduced no new entry.
