# Scope and Project Rebaseline

Timestamp: 2026-07-21T19-40Z
Command: PowerShell `Get-Content` line counting for the authorized production and protected test files; exact-regex `Select-String` counts for `<Compile Include="Viewers\BreadcrumbDropDownHost.cs" />` and `<Compile Include="Viewers\BreadcrumbWebViewSurfaceFactory.cs" />`; `Test-Path` for the helper; and SHA-256 over each protected test file's ordered exact source lines matching `\.Should\(` or `\bAssert\.`, joined with LF and encoded as UTF-8 without BOM
EXIT_CODE: 0
Output Summary: The current host draft is 625 lines and is unverified and not delivered. The helper file and helper Compile include are absent. The host has exactly one Compile include. The revised plan authorizes only the new internal static helper and one adjacent project include. Ordered assertion inventories and reproducible SHA-256 hashes are recorded for all four protected test files.

## Production and Project State

| Check | Result |
|---|---:|
| `QuickFiler/Viewers/BreadcrumbDropDownHost.cs` current worktree lines | 625 |
| Host draft delivery status | Unverified and not delivered |
| `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` exists | No |
| Exact host Compile include count | 1 |
| Exact helper Compile include count | 0 |

The only authorized scope expansion is:

- New file `QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs` containing internal static type `BreadcrumbWebViewSurfaceFactory`.
- Exactly one `<Compile Include="Viewers\BreadcrumbWebViewSurfaceFactory.cs" />` immediately adjacent to the existing host include in `QuickFiler/QuickFiler.csproj`.
- A final host size of 475–485 lines and an approximately 105-line helper, with both files at or below 500 lines.

No further production or project file is authorized.

## Assertion Hash Method

For each protected test file, source lines were read in file order. Every exact line matching `\.Should\(` or `\bAssert\.` was retained with its original whitespace. Retained line texts were joined with LF, encoded as UTF-8 without a BOM, and hashed with SHA-256. The inventories below show the retained lines in order using their baseline line numbers and trimmed display text. Later comparisons must use the same exact-line hashing method; line-number movement alone does not alter the hash.

## `BreadcrumbDropDownReadinessTests.cs`

- Path: `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs`
- File lines: 307
- Assertion-bearing lines: 51
- SHA-256: `58cff79fb67b5a6d95f60e961adedba7492691fdd9ffe16036ea467417bfda6d`

```text
A035|opening.IsCompleted.Should().BeFalse();
A036|harness.ReadyEventCount.Should().Be(0);
A037|harness.AttachmentCount.Should().Be(0);
A038|harness.PopupMessenger.SubscriberCount.Should().Be(0);
A039|harness.PopupMessenger.Posted.Should().BeEmpty();
A040|harness.ShowCount.Should().Be(0);
A041|harness.FocusPendingCount.Should().Be(0);
A042|harness.Host.PopupMessenger.Should().BeNull();
A049|opened.Should().BeTrue();
A050|harness.FactoryCount.Should().Be(1);
A051|harness.ReadyEventCount.Should().Be(1);
A052|harness.AttachmentCount.Should().Be(1);
A053|harness.PopupMessenger.SubscriberCount.Should().Be(1);
A054|CountType(harness.PopupMessenger.Posted, "render").Should().Be(1);
A055|CountType(harness.PopupMessenger.Posted, "themeChange").Should().Be(1);
A056|CountType(harness.PopupMessenger.Posted, "selectorView").Should().Be(1);
A057|harness.PopupMessenger.Posted.Should().HaveCount(3);
A062|.Should()
A064|harness.ShowCount.Should().Be(1);
A065|harness.FocusPendingCount.Should().Be(1);
A066|harness.Host.PopupMessenger.Should().BeSameAs(harness.PopupMessenger);
A067|harness.Host.IsOpen.Should().BeTrue();
A090|opened.Should().BeFalse();
A091|harness.Coordinator.GetSelectedFolder().Should().Be("A");
A092|harness.Coordinator.CommittedIdentity.Should().Be("A");
A093|harness.Coordinator.PendingIdentity.Should().BeNull();
A094|harness.Coordinator.IsSelectorOpen.Should().BeFalse();
A095|harness.CancelCount.Should().Be(1);
A096|selectionPublications.Should().Be(0);
A097|harness.Surface.DisposeCount.Should().Be(1);
A098|harness.PopupMessenger.DisposeCount.Should().Be(1);
A099|harness.ReadyEventCount.Should().Be(0);
A100|harness.AttachmentCount.Should().Be(0);
A101|harness.PopupMessenger.SubscriberCount.Should().Be(0);
A102|harness.PopupMessenger.Posted.Should().BeEmpty();
A103|harness.ShowCount.Should().Be(0);
A104|harness.FocusPendingCount.Should().Be(0);
A105|harness.FocusAnchorCount.Should().Be(1);
A106|harness.Host.PopupMessenger.Should().BeNull();
A107|harness.Host.IsOpen.Should().BeFalse();
A108|harness.Host.DropDown.Items.Count.Should().Be(0);
A109|harness.Host.LastInitializationException.Should().BeSameAs(failure);
A113|.Should()
A116|harness.CancelCount.Should().Be(1);
A117|harness.FocusAnchorCount.Should().Be(1);
A118|selectionPublications.Should().Be(0);
A144|.Should()
A211|Coordinator.OpenSelector().Should().BeTrue();
A212|Coordinator.HandleSelectorKey(BreadcrumbSelectorKey.Down).Should().BeTrue();
A213|Coordinator.GetSelectedFolder().Should().Be("A");
A214|Coordinator.PendingIdentity.Should().Be("B");
```

## `BreadcrumbDropDownLifecycleConcurrencyTests.cs`

- Path: `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs`
- File lines: 379
- Assertion-bearing lines: 81
- SHA-256: `a38135b5a39844c4a4f1a420773d54dac6cff6c87c0dcc979a8edd4ebce3e84a`

```text
A037|results.Should().Equal(true, true);
A038|harness.FactoryCount.Should().Be(1);
A039|harness.Attempts.Should().ContainSingle();
A040|harness.Host.DropDown.Items.OfType<ToolStripControlHost>().Should().ContainSingle();
A041|harness.ReadyEventCount.Should().Be(1);
A042|harness.AttachmentCount.Should().Be(1);
A043|harness.ShowCount.Should().Be(1);
A044|harness.FocusPendingCount.Should().Be(1);
A045|harness.FocusAnchorCount.Should().Be(0);
A046|harness.CancelCount.Should().Be(0);
A047|harness.Host.IsOpen.Should().BeTrue();
A048|harness.Host.PopupMessenger.Should().BeSameAs(harness.Attempts.Single().Messenger);
A067|staleOpened.Should().BeFalse();
A068|staleAttempt.Surface.DisposeCount.Should().Be(1);
A069|staleAttempt.Messenger.DisposeCount.Should().Be(1);
A070|staleAttempt.Messenger.Posted.Should().BeEmpty();
A071|harness.ReadyEventCount.Should().Be(0);
A072|harness.AttachmentCount.Should().Be(0);
A073|harness.ShowCount.Should().Be(0);
A074|harness.FocusPendingCount.Should().Be(0);
A075|harness.FocusAnchorCount.Should().Be(0);
A076|harness.CancelCount.Should().Be(0);
A077|harness.Host.IsOpen.Should().BeFalse();
A078|harness.Host.PopupMessenger.Should().BeNull();
A079|harness.Host.LastInitializationException.Should().BeNull();
A083|harness.FactoryCount.Should().Be(2);
A089|freshOpened.Should().BeTrue();
A090|harness.FactoryCount.Should().Be(2);
A091|harness.Host.DropDown.Items.OfType<ToolStripControlHost>().Should().ContainSingle();
A092|harness.ReadyEventCount.Should().Be(1);
A093|harness.AttachmentCount.Should().Be(1);
A094|harness.ShowCount.Should().Be(1);
A095|harness.FocusPendingCount.Should().Be(1);
A096|harness.Host.IsOpen.Should().BeTrue();
A097|harness.Host.PopupMessenger.Should().BeSameAs(freshAttempt.Messenger);
A116|opened.Should().BeFalse();
A117|attempt.Surface.DisposeCount.Should().Be(1);
A118|attempt.Messenger.DisposeCount.Should().Be(1);
A119|attempt.Messenger.Posted.Should().BeEmpty();
A120|harness.ReadyEventCount.Should().Be(0);
A121|harness.AttachmentCount.Should().Be(0);
A122|harness.ShowCount.Should().Be(0);
A123|harness.FocusPendingCount.Should().Be(0);
A124|harness.FocusAnchorCount.Should().Be(0);
A125|harness.CancelCount.Should().Be(0);
A126|harness.Host.IsOpen.Should().BeFalse();
A127|harness.Host.PopupMessenger.Should().BeNull();
A128|harness.Host.LastInitializationException.Should().BeNull();
A148|opened.Should().BeFalse();
A149|harness.ReadyEventCount.Should().Be(0);
A150|harness.AttachmentCount.Should().Be(0);
A151|harness.ShowCount.Should().Be(0);
A152|harness.FocusPendingCount.Should().Be(0);
A153|harness.FocusAnchorCount.Should().Be(0);
A154|harness.CancelCount.Should().Be(0);
A155|harness.Host.IsOpen.Should().BeFalse();
A156|harness.Host.PopupMessenger.Should().BeNull();
A157|harness.Host.LastInitializationException.Should().BeNull();
A173|(await freshOpening).Should().BeTrue();
A181|staleOpened.Should().BeFalse();
A182|harness.Host.LastInitializationException.Should().BeNull();
A183|harness.Host.IsOpen.Should().BeTrue();
A184|harness.Host.PopupMessenger.Should().BeSameAs(freshAttempt.Messenger);
A185|freshAttempt.Surface.DisposeCount.Should().Be(0);
A186|freshAttempt.Messenger.DisposeCount.Should().Be(0);
A187|harness.ReadyEventCount.Should().Be(1);
A188|harness.AttachmentCount.Should().Be(1);
A189|harness.ShowCount.Should().Be(1);
A190|harness.FocusPendingCount.Should().Be(1);
A191|harness.FocusAnchorCount.Should().Be(0);
A192|harness.CancelCount.Should().Be(0);
A211|opened.Should().BeFalse();
A212|harness.Host.LastInitializationException.Should().BeSameAs(failure);
A213|harness.Host.IsOpen.Should().BeFalse();
A214|harness.Host.PopupMessenger.Should().BeNull();
A215|harness.ReadyEventCount.Should().Be(0);
A216|harness.AttachmentCount.Should().Be(0);
A217|harness.ShowCount.Should().Be(0);
A218|harness.FocusPendingCount.Should().Be(0);
A219|harness.FocusAnchorCount.Should().Be(1);
A220|harness.CancelCount.Should().Be(1);
```

## `BreadcrumbDropDownHostTests.cs`

- Path: `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs`
- File lines: 499
- Assertion-bearing lines: 52
- SHA-256: `8d9b16ed5d5e2ca21217e4e4c6653415f7fb7c13c105119f7a2182cac418f3dc`

```text
A029|dropDown.AutoClose.Should().BeTrue();
A030|Property<Control>(harness.Host, "Anchor").Should().BeSameAs(harness.Anchor);
A034|.Should()
A050|.Should()
A055|dropDown.Items.Count.Should().Be(1);
A056|dropDown.Items[0].Should().BeOfType<ToolStripControlHost>();
A057|harness.ShownOwner.Should().BeSameAs(harness.Anchor);
A058|harness.ShownLocation.Should().Be(new Point(500, 125));
A059|((ToolStripControlHost)dropDown.Items[0]).Size.Should().Be(new Size(300, 200));
A071|Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();
A074|Close(harness.Host, "ExplicitCommit").Should().BeTrue();
A075|harness.CancelCount.Should().Be(0);
A076|harness.FocusAnchorCount.Should().Be(1);
A079|Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();
A080|Close(harness.Host, "Uncommitted").Should().BeTrue();
A081|harness.CancelCount.Should().Be(1);
A082|harness.FocusAnchorCount.Should().Be(2);
A099|.Should()
A103|harness.FocusPendingCount.Should().Be(1);
A104|Property<bool>(harness.Host, "IsOpen").Should().BeTrue();
A107|Close(harness.Host, "Uncommitted").Should().BeTrue();
A108|harness.FocusAnchorCount.Should().Be(1);
A109|Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
A124|Property<string>(harness.Host, "Theme").Should().Be("light");
A139|.Should()
A153|Open(harness.Host, anchor, work, new Size(300, 200)).Should().BeTrue();
A159|reopened.Should().BeTrue();
A160|harness.FactoryCount.Should().Be(1);
A161|harness.ShowCount.Should().Be(1);
A162|harness.FocusPendingCount.Should().Be(2);
A181|opened.Should().BeFalse();
A182|harness.CancelCount.Should().Be(1);
A183|harness.FocusAnchorCount.Should().Be(1);
A185|.Should()
A209|opened.Should().BeFalse();
A210|harness.CancelCount.Should().Be(1);
A211|harness.FocusAnchorCount.Should().Be(1);
A212|Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
A214|.Message.Should()
A231|.Should()
A243|harness.CancelCount.Should().Be(1);
A244|harness.FocusAnchorCount.Should().Be(1);
A245|Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
A260|.Should()
A270|harness.CancelCount.Should().Be(1);
A271|harness.FocusAnchorCount.Should().Be(1);
A273|.Should()
A294|control.WasDisposed.Should().BeTrue();
A295|messenger.WasDisposed.Should().BeTrue();
A325|missingInitializer.Should().Throw<ArgumentNullException>();
A326|missingHtml.Should().Throw<ArgumentNullException>();
A336|type.Should().NotBeNull("issue #400 requires an owned native drop-down host");
```

## `BreadcrumbDropDownLifecycleTests.cs`

- Path: `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleTests.cs`
- File lines: 277
- Assertion-bearing lines: 34
- SHA-256: `fc9370c70b339dd99251e43385d82e7c04c2ac779a17546c3ae64e0a7c4fd5ce`

```text
A027|harness.FactoryCount.Should().Be(0);
A030|harness.Open().Should().BeTrue();
A032|harness.Close("ExplicitCommit").Should().BeTrue();
A033|harness.Open().Should().BeTrue();
A036|harness.FactoryCount.Should().Be(1);
A037|harness.SuppliedEnvironment.Should().BeSameAs(harness.Environment);
A038|Property<object>(harness.Host, "PopupMessenger").Should().BeSameAs(firstMessenger);
A039|ready.Should().Be(1);
A049|harness.Open().Should().BeTrue();
A051|harness.Close("ExplicitCommit").Should().BeTrue();
A057|first.WasDisposed.Should().BeTrue();
A058|Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
A059|Property<object>(harness.Host, "PopupMessenger").Should().BeNull();
A062|harness.Open().Should().BeTrue();
A063|harness.FactoryCount.Should().Be(2);
A081|opened.Should().BeFalse();
A082|partial.WasDisposed.Should().BeTrue();
A083|harness.CancelCount.Should().Be(1);
A084|harness.FocusAnchorCount.Should().Be(1);
A085|Property<bool>(harness.Host, "IsOpen").Should().BeFalse();
A086|Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
A095|harness.Open().Should().BeTrue();
A105|control.WasDisposed.Should().BeTrue();
A106|cancelAfterDispose.Should().Be(1);
A107|focusAfterDispose.Should().Be(1);
A108|closedAgain.Should().BeFalse();
A109|harness.CancelCount.Should().Be(cancelAfterDispose);
A110|harness.FocusAnchorCount.Should().Be(focusAfterDispose);
A129|opened.Should().BeFalse();
A130|harness.CancelCount.Should().Be(1);
A131|harness.FocusAnchorCount.Should().Be(1);
A133|.Message.Should()
A135|Property<ToolStripDropDown>(harness.Host, "DropDown").Items.Count.Should().Be(0);
A164|type.Should()
```

P0-T9 result: PASS. The authorized Phase 2 boundary is sufficient; no further production or project file is required before delegation.
