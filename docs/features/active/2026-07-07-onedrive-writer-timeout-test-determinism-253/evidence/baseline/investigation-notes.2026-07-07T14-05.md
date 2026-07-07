# Investigation Notes — Production Call Chain and Seam Pattern (Issue #253)

Timestamp: 2026-07-07T16-28

## (a) Current call to `RunWithTimeout`

`UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:88-97`, method `TryGetFileStreamWriter` (`public virtual async Task<Stream>`):

```csharp
try
{
    var stream = await GetFileStreamWriter.RunWithTimeout(
        destinationPath,
        cancel,
        timeoutMs,
        3,
        false
    );
    return stream;
}
catch (Exception)
{
    return null;
}
```

Confirmed by direct read of the file (lines 82-103 for the full method, catch clause returns `null`).

## (b) Existing virtual-delegate-property seam pattern to mirror

Two existing properties in `OneDriveDownloader.cs` follow the same pattern (backing field + public virtual getter + protected setter), confirmed by direct read:

- `ClientGetAsync` (`OneDriveDownloader.cs:33-38`):
  ```csharp
  protected Func<string, CancellationToken, Task<HttpResponseMessage>> _clientGetAsync;
  public virtual Func<string, CancellationToken, Task<HttpResponseMessage>> ClientGetAsync
  {
      get => _clientGetAsync;
      protected set => _clientGetAsync = value;
  }
  ```
- `GetFileStreamWriter` (`OneDriveDownloader.cs:105-118`):
  ```csharp
  public virtual Func<string, Stream> GetFileStreamWriter
  {
      get => _getFileStreamWriter;
      protected set => _getFileStreamWriter = value;
  }
  protected Func<string, Stream> _getFileStreamWriter = (string destinationPath) =>
      new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
  ```

`TestableOneDriveDownloader` (`UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs:14-27`) exposes `SetClientGetAsync` and `SetFileStreamWriter` public setter methods that assign these protected-settable virtual properties, confirmed by direct read.

The new `WriterTimeoutRunner` seam (Phase 1) will mirror this exact pattern: a `protected` backing field, a `public virtual` getter/protected-setter property, and a `SetWriterTimeoutRunner` method on `TestableOneDriveDownloader`.

## (c) `RunWithTimeout<T1, TResult>` signature that the default seam implementation must call unchanged

`UtilitiesCS/Threading/TimeOutTask.cs:164-174`:

```csharp
public static async Task<TResult> RunWithTimeout<T1, TResult>(
    this Func<T1, TResult> function,
    T1 arg1,
    CancellationToken token,
    int milliseconds,
    int maxAttempts,
    bool strict
)
{
    return await function.RunWithTimeout(arg1, token, milliseconds, maxAttempts, strict, 0);
}
```

Confirmed by direct read. This is the exact public extension-method overload invoked at `OneDriveDownloader.cs:90`. The default `WriterTimeoutRunner` delegate body must call this overload with the same argument order and the same literal `3` (maxAttempts) and `false` (strict) values, per plan task P1-T1.

Research artifact cross-reference: `docs/features/active/2026-07-07-onedrive-writer-timeout-test-determinism-253/research/2026-07-07T13-00-onedrive-writer-timeout-research.md`, Section 1.1, corroborates (a) and (c) with identical line citations.

## (d) `TimeOutTask.cs` out-of-scope confirmation

`UtilitiesCS/Threading/TimeOutTask.cs` (private implementation at lines 176-229, including the `catch (TimeoutException)` clause at line 199 flagged by research Section 1.2 as a distinct, separately-tracked defect) is explicitly OUT OF SCOPE for issue #253. It will not be modified by this plan. The plan's adopted fix (research Section 3, Option (a)) operates entirely within `OneDriveDownloader.cs` and the test file, routing production code around the nondeterministic boundary rather than modifying `TimeOutTask.cs` itself.

## Output Summary

Confirmed via direct source inspection: (a) current `RunWithTimeout` call site at `OneDriveDownloader.cs:88-97`; (b) existing `ClientGetAsync`/`GetFileStreamWriter` virtual-delegate-property seam pattern to mirror; (c) `RunWithTimeout<T1, TResult>` extension signature at `TimeOutTask.cs:164-174` that the new seam's default implementation must call unchanged; (d) `TimeOutTask.cs` confirmed out of scope and will not be modified. All citations cross-checked against the research artifact and match.
