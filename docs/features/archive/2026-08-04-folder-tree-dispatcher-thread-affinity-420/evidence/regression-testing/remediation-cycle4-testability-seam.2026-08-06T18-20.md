# P5-T42 refreshed testability-seam evidence

Timestamp: 2026-08-06T18-20

The preserved public `FilterOlFoldersController(IApplicationGlobals)` constructor previously created a concrete viewer directly. A deterministic test of that path could not substitute a viewer without a real form/message loop, reflection, or global state. The required minimal seam is the protected-internal instance-local `CreateViewerFactory` provider. Its base implementation remains the concrete viewer factory; the public signature and callers remain unchanged.

`TryAttachSnapshotSubscription` is internal so the deterministic disposal test can assert its terminal false result directly. `OnFolderTreeViewCommitted` is an instance-local post-commit hook used only to causally dispose test instances between a successful view commit and the existing initialization/refresh disposal checks. It covers no new public behavior and does not add a global hook.

Red proof: the prior direct-construction path could not use a fake viewer under the no-real-viewer/no-reflection constraints. Green proof: the controller fixture passed 25/25, including public-constructor factory substitution, pending snapshot disposal, subscription attachment, and queued disposal-before-initialization/refresh cases. The current changed controller partial is 499 lines, `LifecycleRaces.cs` is 296 lines, and `FilterOlFoldersController.Lifecycle.cs` is 498 lines. The authorized coverage partial has exactly one adjacent `Compile` entry. The exact P5-T46 report measures the changed controller at 101/102 and lifecycle at 334/335 covered lines; the only unhit controller/lifecycle lines are unchanged line 81 in each source file.
