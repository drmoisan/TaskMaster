# P1-T5 — New production file formatting normalization

Timestamp: 2026-09-01T19-56
Command: `dotnet tool run csharpier format QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, then `dotnet tool run csharpier check QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`, then `git status --porcelain -- QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs`
EXIT_CODE: 0 (the `check` invocation)

## Observation: the format invocation rewrote the file

`csharpier format` is write-mode and exits 0 whether or not it rewrote the file, so its exit code is not the observation. The observation is the SHA-256 of the file on either side of it:

    SHA-256 before format: FDD35C4E6A7A22368CF378DC07A5281F1241145D9EE30B205EA2308059F34D1A
    SHA-256 after format:  D93CF854506FA18AF57FEBB152730CBE86B9176198884340346EB7C9D85C6AC3

The two differ, so the invocation rewrote the file rather than passing it through unchanged. That is the expected outcome for a newly authored file and is why this task exists ahead of the Phase 4 repo-wide format: normalizing here is what leaves P4-T1 with nothing to rewrite under `QuickFiler/`.

`git status --porcelain` output:

    AM QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs

`A` records the staging performed in P1-T2; `M` records the unstaged formatter rewrite. The two-character code is itself corroboration that the file changed after being staged.

## The check gate is discriminating

The read-only `check` invocation was run **before** the format as well as after, so the pass recorded here is demonstrably a real result rather than a gate that cannot fail:

    Before format: EXIT 1 — "Error .\QuickFiler/Controllers/QfcItemController.WebViewFaultBoundary.cs - Was not formatted."
    After format:  EXIT 0 — "Checked 1 files in 431ms."

The before-run additionally printed the specific expected-versus-actual hunk it objected to, at the sink declaration.

## Post-format text of the sink declaration, verbatim

Reproduced from the formatted file so AC2 is checked off against the formatted shape rather than against the pre-format text:

    /// <summary>
    /// #670 fault-boundary sink: an injectable seam over the static log4net logger declared at
    /// QfcItemController.cs:30. Named distinctly from EfcFormController.BoundaryErrorSink so no
    /// shared contract between the two types is implied.
    /// </summary>
    internal System.Action<
        string,
        System.Exception
    > WebViewInitializationErrorSink { get; set; } =
        (message, exception) => logger.Error(message, exception);

The declared type is `System.Action<string, System.Exception>`, the member is `internal`, it carries both a `get` and a `set` accessor, and its default value is `(message, exception) => logger.Error(message, exception)` — the log4net message-first overload `ILog.Error(string, Exception)`, which is the form AC2 requires. The exception-first spelling does not exist on `log4net.ILog` and would not compile, so a successful build is itself corroboration of the overload selected.

## Note on the formatter's actual reshaping

The plan authored the declaration with an explicit `{ get; set; }` accessor block on separate lines, on the reasoning that the single-line form exceeds the print width and its post-format shape would otherwise be unpredictable. The formatter's actual choice differs from that prediction in its details: it collapsed the accessor block back onto one line and instead wrapped the **generic argument list** across three lines, then broke before the initializer. The declaration is semantically identical either way, and no acceptance condition in this plan asserts against the pre-format layout of this declaration, so the difference is recorded rather than remediated. This task exists precisely so the post-format shape is captured from observation instead of predicted.

## The P1-T4 literals survive formatting

All four P1-T4 acceptance literals were re-measured against the formatted file and each still returns exactly one match:

    internal async Task InitializeWebViewGuardedAsync()                        1
    await InitializeWebViewAsync();                                            1
    catch (OperationCanceledException)                                         1
    WebViewInitializationErrorSink("WebView2 initialization failed.", ex);      1

The case-sensitive `throw` search still returns zero. Re-measuring after the rewrite is necessary rather than redundant: a formatter that wrapped any of those four lines would have left the literal present in the file but unmatched by a line-oriented search, which is the wrap-fragility failure mode the plan's authoring rules warn about. The longest of the four sits at 86 columns against the formatter's 100-column print width, so none was a wrapping candidate.

Output Summary: The format invocation rewrote the file (SHA-256 changed). The read-only check invocation exits 1 before the format and 0 after, so the gate discriminates. The post-format sink declaration is reproduced verbatim above and satisfies AC2's stated shape. All four P1-T4 literals survive the rewrite. The file is 41 lines.

Base-ref note: this task states no `git diff` command against a ref. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
