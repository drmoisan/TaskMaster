# User Story: Parallel test suite runs reliably (Issue #1005)

> This item is in `full-bug` work mode. `spec.md` in this folder is the sole authoritative acceptance-criteria source. This file exists only because the prd-feature stop hook requires it; it is narrative context, contains no acceptance criteria, and must not be used for check-off.

## Story

As the maintainer running the full TaskMaster MSTest suite in parallel in Visual Studio Test Explorer, I want every test to pass regardless of execution order and concurrency, so that a red result always indicates a real defect rather than a wall-clock timeout or a race on the shared `user.config` file.

- Given the suite runs with class-level parallelism,
- When `DfDeedle_COM_Tests` and the TaskTree controller move and route-drop tests run alongside the TaskMaster settings tests,
- Then no test waits on a real clock deadline, no test saves application settings to disk, and the run is green on every repetition.
