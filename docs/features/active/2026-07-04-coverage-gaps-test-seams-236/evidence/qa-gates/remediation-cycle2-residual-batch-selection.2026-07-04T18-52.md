# Remediation Cycle 2 Residual Batch Selection

Timestamp: 2026-07-04T18:52:00-04:00
Command: Read docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\remediation-cycle2-normalized-coverage-threshold-preview.2026-07-04T18-52.md and parse docs\features\active\2026-07-04-coverage-gaps-test-seams-236\evidence\qa-gates\remediation-cycle2-normalized-coverage.cobertura.xml for residual candidate uncovered executable lines.
EXIT_CODE: 0
Output Summary:
- Repository-wide line coverage after normalization is 79.47%, below 80.00%, so residual C# tests are required.
- Residual covered-line gap is 525 lines.
- Required selected uncovered executable lines including 250-line buffer: 775.
- Selected residual batch uncovered executable lines: 2219.
- Selection status: PASS.

Selected Residual Batch:
| File | Covered Lines | Valid Lines | Uncovered Executable Lines | Coverage |
| --- | ---: | ---: | ---: | ---: |
| `SVGControl\RelativePath.cs` | 147 | 774 | 627 | 18.99% |
| `ToDoModel\Data Model\ToDo\ToDoItem.cs` | 284 | 820 | 536 | 34.63% |
| `QuickFiler\Controllers\QfcQueue.cs` | 47 | 386 | 339 | 12.18% |
| `Tags\TagController.cs` | 249 | 578 | 329 | 43.08% |
| `ToDoModel\Data Model\Project\ProjectData.cs` | 7 | 216 | 209 | 3.24% |
| `TaskMaster\AppGlobals\AppAutoFileObjects.cs` | 224 | 403 | 179 | 55.58% |

Phase 2 Task Decision:
- P2-T2 through P2-T7 are required.
