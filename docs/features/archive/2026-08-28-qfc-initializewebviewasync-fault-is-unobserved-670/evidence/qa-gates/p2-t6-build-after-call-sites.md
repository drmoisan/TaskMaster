# P2-T6 — Build after the three call-site substitutions

Timestamp: 2026-09-01T19-59
Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, using the MSBuild resolution established in P0-T10 (resolved executable `<vs-install>\MSBuild\Current\Bin\MSBuild.exe`)
EXIT_CODE: 0

## Output Summary

    Build succeeded.
        5 Warning(s)
        0 Error(s)

    Time Elapsed 00:00:11.94

**Warning count: 5. Error count: 0.** The five warnings are the pre-existing System.Reactive `packages.config` diagnostic recorded in the P0-T10 baseline, unchanged in count. A search for a coded diagnostic matching `: error [A-Z]+[0-9]+:` returns **zero**, and the same search for `: warning [A-Z]+[0-9]+:` returns zero. The three substituted call sites introduced no compiler or analyzer diagnostic.

The log contains **60** `CoreCompile:` target executions, so `/t:Rebuild` genuinely recompiled and the analyzer set actually ran; a warm `/t:Build` would have skipped compilation entirely and the gate could not have failed.

## What this build establishes

Two of the three sites are ordinary invocations of an `async Task` member and would compile trivially. The substitution worth verifying is site 192:

    _ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewGuardedAsync);

`_itemViewer.UiDispatcher` is `System.Windows.Threading.Dispatcher`. A method group returning `Task` has no method-group conversion to `Action`, so this compiles only by binding `DispatcherOperation<TResult> InvokeAsync<TResult>(Func<TResult>)` with `TResult = Task`. That overload resolution succeeding is what the build confirms; had the substitution produced an ambiguous or absent conversion, this is where it would have surfaced as a CS-coded error rather than as a runtime surprise.

The `AsyncFixer` analyzer is among the five wired into `QuickFiler.csproj` and is active on this build, so a fire-and-forget or async-void defect introduced by the substitution would be reported here rather than passing silently. It reported nothing.

## Plan sequencing note

`spec.md` lists the three call-site substitutions as delivery step 4, after the mutation demonstration at step 3; this plan places them in Phase 2, before Phase 3. The deviation is deliberate and behaviour-neutral: the P3-T5 mutation acts on the guard's `catch (Exception ex)` arm and the discriminating test calls `InitializeWebViewGuardedAsync` directly, so the red/green pair discriminates identically whether or not the call sites have been substituted. Substituting first lets this single analyzer rebuild cover both production edits at once instead of requiring a further rebuild after Phase 3.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
