# Dispatcher SynchronizationContext probe (issue #781)

- Timestamp: 2026-09-05T10-40 (local)
- Host: Windows PowerShell 5.1, .NET Framework 4.8 (CLR 4.0.30319.42000), STA thread
- Purpose: confirm that a callback executed through a WPF Dispatcher operation observes a `DispatcherSynchronizationContext` that is not reference-equal to the thread ambient `WindowsFormsSynchronizationContext`, which is the mechanism behind the `ItemViewer.ThrowIfOffUiBoundary` failure.

## Script

```powershell
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Windows.Forms
"CLR: $([System.Environment]::Version)  ApartmentState: $([System.Threading.Thread]::CurrentThread.GetApartmentState())"
"ReuseDispatcherSynchronizationContextInstance: $([System.Windows.BaseCompatibilityPreferences]::ReuseDispatcherSynchronizationContextInstance)"
$wf = New-Object System.Windows.Forms.WindowsFormsSynchronizationContext
[System.Threading.SynchronizationContext]::SetSynchronizationContext($wf)
$outer = [System.Threading.SynchronizationContext]::Current
$d = [System.Windows.Threading.Dispatcher]::CurrentDispatcher
$script:c1 = $null; $script:c2 = $null; $script:c3 = $null
$d.Invoke([Action]{ $script:c1 = [System.Threading.SynchronizationContext]::Current }, [System.Windows.Threading.DispatcherPriority]::Render)
$d.Invoke([Action]{ $script:c2 = [System.Threading.SynchronizationContext]::Current }, [System.Windows.Threading.DispatcherPriority]::Render)
$op = $d.InvokeAsync([Action]{ $script:c3 = [System.Threading.SynchronizationContext]::Current }, [System.Windows.Threading.DispatcherPriority]::ContextIdle)
$op.Wait() | Out-Null
"outer ambient type            : $($outer.GetType().FullName)"
"inside Dispatcher.Invoke type : $($script:c1.GetType().FullName)"
"inside InvokeAsync type       : $($script:c3.GetType().FullName)"
"Invoke ctx == outer ambient   : $([object]::ReferenceEquals($script:c1, $outer))"
"Invoke#1 ctx == Invoke#2 ctx  : $([object]::ReferenceEquals($script:c1, $script:c2))"
"InvokeAsync ctx == Invoke#1   : $([object]::ReferenceEquals($script:c3, $script:c1))"
"ambient after ops == outer    : $([object]::ReferenceEquals([System.Threading.SynchronizationContext]::Current, $outer))"
"WinForms CreateCopy == self   : $([object]::ReferenceEquals($wf.CreateCopy(), $wf))"
```

## Output

```
CLR: 4.0.30319.42000  ApartmentState: STA
ReuseDispatcherSynchronizationContextInstance: True
outer ambient type            : System.Windows.Forms.WindowsFormsSynchronizationContext
inside Dispatcher.Invoke type : System.Windows.Threading.DispatcherSynchronizationContext
inside InvokeAsync type       : System.Windows.Threading.DispatcherSynchronizationContext
Invoke ctx == outer ambient   : False
Invoke#1 ctx == Invoke#2 ctx  : True
InvokeAsync ctx == Invoke#1   : True
ambient after ops == outer    : True
WinForms CreateCopy == self   : False
```

## Reading

`Invoke ctx == outer ambient : False` is the decisive line: a viewer constructed inside `UiThread.Dispatcher.Invoke(...)` captures the dispatcher context, so a later reference comparison against the ambient WinForms context on the same UI thread fails. `ReuseDispatcherSynchronizationContextInstance` follows the host AppDomain target framework and only decides whether successive dispatcher operations share one instance; it does not make the captured context equal to the ambient one.
