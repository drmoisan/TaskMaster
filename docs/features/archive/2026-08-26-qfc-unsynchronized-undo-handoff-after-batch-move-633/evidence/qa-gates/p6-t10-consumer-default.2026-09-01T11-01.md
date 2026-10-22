# Retained `Consumer` default, test unmodified (P6-T10)

Timestamp: 2026-09-01T11-01
Task: [P6-T10]
Working directory: WORKTREE

## Command 1 — scoped test run

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~FilerQueue_NewInstance_HasCompletedConsumerByDefault" "/Logger:trx;LogFileName=p6-t10.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t10
```

EXIT_CODE: 0

Count of `outcome="Passed"` occurrences in the produced TRX: **1**.
Count of `outcome="Failed"` occurrences: 0.

| Outcome | Test |
|---|---|
| Passed | `FilerQueue_NewInstance_HasCompletedConsumerByDefault` |

## Command 2 — pre-change text

Command:

```
git show 06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72:QuickFiler.Test/Controllers/FilerQueueTests.cs
```

EXIT_CODE: 0

The SHA is the concrete merge-base commit that P0-T3 recorded. Reading the pre-change body from the
working file is not an option: P5-T1 through P5-T8 modified the class comment and added seven tests to
the same file, which shifted the line numbers, so lines 76-87 of the current file no longer name this
method.

Lines 76-87 of that output, quoted verbatim:

```
        [TestMethod]
        public void FilerQueue_NewInstance_HasCompletedConsumerByDefault()
        {
            // Arrange / Act
            var queue = new FilerQueue();

            // Assert
            queue.Consumer.Should().NotBeNull();
            queue
                .Consumer.IsCompleted.Should()
                .BeTrue("a fresh FilerQueue exposes Task.CompletedTask as its consumer");
        }
```

## Byte-identity comparison

In the current file the same method occupies lines 82-93. A case-sensitive line-by-line comparison of
the twelve pre-change lines against the twelve current lines reports **BYTE_IDENTICAL: True**, with no
differing index. The method moved down by exactly six lines because the class comment above it was
expanded from a seven-line summary into a summary plus a remarks block; its own text was not touched.

Output Summary: `FilerQueue.Consumer` retains its declaration, its accessibility, and its
`Task.CompletedTask` default, and the test that pins that contract passes with a body byte-identical to
the pre-change text. The handshake repair removed the `ThreadSafeSingleShotGuard` start gate but left
`Consumer` on the public surface and still assigned by `Enqueue`, so the change is additive on the
assembly's public surface as the specification requires. P3-T7 independently verified that the
declaration line `public Task Consumer { get; private set; } = Task.CompletedTask;` still occurs exactly
once in `QuickFiler/Controllers/FilerQueue.cs`.

This artifact supplies the evidence for the AC11 check-off in P8-T15.
