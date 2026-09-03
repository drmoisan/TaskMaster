Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $helperScriptPath = Join-Path $repoRoot 'scripts\vscode\Invoke-MSTestWithCoverage.Helpers.ps1'
    . $helperScriptPath
}

Describe 'Remove-CoberturaExemptClosureCoverage' {
    It 'drops closure lines whose declaring member is absent from the instrumented method set' {
        # Regression case 1 (Issue #457, required direction 1): a lambda hoisted into
        # <>c__DisplayClass41_0 belongs to the exempt member 'Exempt', which emits no plain
        # <method> element. Its lines must leave the denominator; 'Visible' must survive.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass41_0" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        # The recomputed summary is read from Get-CoberturaCoverageSummary, never from the
        # document-level attributes: the filter mutates the tree but does not rewrite them.
        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        @($doc.SelectNodes('//line[@number="406"]')).Count | Should -Be 0
        @($doc.SelectNodes('//line[@number="409"]')).Count | Should -Be 0
        $summary.LinesValid | Should -Be '2'
        $summary.LinesCovered | Should -Be '2'
    }

    It 'keeps closure lines whose declaring member is present in the instrumented method set' {
        # Regression case 2 (Issue #457, required direction 2): the same shape, but the lambda
        # belongs to 'Visible', which IS instrumented. Deleting these lines would be
        # over-exclusion, which the fail-safe invariant forbids.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass41_0" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Visible&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="406" hits="0" branch="False" /><line number="409" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        # Scoped to the closure class's own rollup: every fixture carries each line twice (once
        # under its <method>, once in the class-level <lines>), so an unscoped //line count would
        # be 2 and would not identify WHERE the line survived.
        $closureLines = '//class[@name="Ns.T.<>c__DisplayClass41_0"]/lines/line'
        @($doc.SelectNodes("$closureLines[@number=`"406`"]")).Count | Should -Be 1
        @($doc.SelectNodes("$closureLines[@number=`"409`"]")).Count | Should -Be 1
        $summary.LinesValid | Should -Be '4'
    }

    It 'keeps closure lines whose declaring member exists only as an async state-machine class' {
        # Regression case 3 (Issue #457, async guard): an async member emits no plain <method>
        # element; its whole body moves to Ns.T.<Async>d__33. Presence-set source (2) must admit
        # that d__ class name, or lambdas inside non-exempt async members are wrongly deleted.
        # Modelled on the verified live counter-example
        # BreadcrumbPopupUiOperations.<>c__DisplayClass33_1 inside CreateAndInstallSurfaceAsync.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T.&lt;Async&gt;d__33" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="MoveNext" signature="()" line-rate="1" branch-rate="1"><lines><line number="60" hits="1" branch="False" /><line number="61" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="60" hits="1" branch="False" /><line number="61" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass33_1" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="&lt;Async&gt;b__0" signature="()" line-rate="1" branch-rate="1"><lines><line number="50" hits="1" branch="False" /><line number="51" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="50" hits="1" branch="False" /><line number="51" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        # XPath predicates compare against PARSED attribute values, so the unescaped '<' and '>'
        # characters are used here even though the fixture text carries the escaped entities.
        # The line assertions are scoped to the closure class's own rollup, because each fixture
        # line appears twice (once under its <method>, once in the class-level <lines>).
        $closureLines = '//class[@name="Ns.T.<>c__DisplayClass33_1"]/lines/line'
        @($doc.SelectNodes("$closureLines[@number=`"50`"]")).Count | Should -Be 1
        @($doc.SelectNodes("$closureLines[@number=`"51`"]")).Count | Should -Be 1
        @($doc.SelectNodes('//class[@name="Ns.T.<>c__DisplayClass33_1"]')).Count | Should -Be 1
        $summary.LinesValid | Should -Be '4'
    }

    It 'drops only the exempt method from a mixed closure class and retains an underivable method' {
        # Regression case 4 (Issue #457): one <>c class carries an exempt lambda, a visible
        # lambda, and a '.ctor' whose declaring member can be derived from neither the method
        # name nor the class name. '.ctor' must be RETAINED (fail-safe), which is the only
        # orchestrator-level exercise of that path and the evidence for spec AC 12.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="3">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method><method name="&lt;Visible&gt;b__1_0" signature="()" line-rate="1" branch-rate="0"><lines><line number="21" hits="1" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="1" branch-rate="0"><lines><line number="22" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="20" hits="0" branch="False" /><line number="21" hits="1" branch="False" /><line number="22" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        # XPath predicates and name comparisons use PARSED attribute values, so the unescaped
        # '<' and '>' characters appear here even though the fixture text carries the entities.
        $closure = $doc.SelectSingleNode('//class[@name="Ns.T.<>c"]')
        $retainedLines = @($closure.SelectNodes('./lines/line')) | ForEach-Object { $_.number }
        $retainedMethods = @($closure.SelectNodes('./methods/method')) | ForEach-Object { $_.name }
        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        # The class survives, having lost only the exempt method.
        $closure | Should -Not -BeNullOrEmpty
        $retainedMethods | Should -Not -Contain '<Exempt>b__0_0'
        $retainedMethods | Should -Contain '<Visible>b__1_0'
        # Fail-safe retention: '.ctor' is underivable, so it is kept, not removed.
        $retainedMethods | Should -Contain '.ctor'
        # <lines> is rebuilt as the de-duplicated union of the RETAINED methods' lines.
        ($retainedLines -join ',') | Should -Be '21,22'
        # line-rate is recomputed against the rebuilt set (2 of 2 covered).
        $closure.'line-rate' | Should -Be '1'
        $summary.LinesValid | Should -Be '3'
        $summary.LinesCovered | Should -Be '3'
    }

    It 'removes a closure class outright when every method resolves to an absent member' {
        # Regression case 5 (Issue #457), both parts in one It so the ten-case count is preserved.
        # Part A (Ns\A.cs): no declaring-type class exists at all, so the closure class is removed
        # entirely and the filename disappears from the report.
        # Part B (Ns\B.cs): the declaring type's only method is the local-function shape
        # <Exempt>g__Local|7_0. g__ methods are deliberately NOT admitted to the presence set, so
        # they cannot mask an otherwise-absent declaring member and the closure is still removed.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.A.&lt;&gt;c__DisplayClass1_0" filename="Ns\A.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Gone&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="30" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="30" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.B" filename="Ns\B.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;g__Local|7_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="40" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="40" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.B.&lt;&gt;c__DisplayClass2_0" filename="Ns\B.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Exempt&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="41" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="41" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@
        $declaringBeforeXml = $doc.SelectSingleNode('//class[@name="Ns.B"]').OuterXml

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        # XPath predicates compare against PARSED attribute values, hence the unescaped '<>'.
        # Part A: the class element is gone and its filename no longer appears anywhere.
        @($doc.SelectNodes('//class[@name="Ns.A.<>c__DisplayClass1_0"]')).Count | Should -Be 0
        @($doc.SelectNodes('//class[@filename="Ns\A.cs"]')).Count | Should -Be 0
        # Part B: a g__ local function on the declaring type does not admit 'Exempt'.
        @($doc.SelectNodes('//class[@name="Ns.B.<>c__DisplayClass2_0"]')).Count | Should -Be 0
        # Part B: the declaring type itself carries no '.<>c' marker and must not be mutated.
        $doc.SelectSingleNode('//class[@name="Ns.B"]').OuterXml | Should -Be $declaringBeforeXml
        @($doc.SelectNodes('//class[@filename="Ns\B.cs"]')).Count | Should -Be 1
    }

    It 'leaves an async state-machine class untouched even when its member has no plain method' {
        # Regression case 7 (Issue #457): Ns.T.<Foo>d__1 is NOT a closure class, so
        # Test-CoberturaClosureClassName rejects it before any derivation is attempted. This pins
        # the documented async residual so it cannot regress silently in either direction.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T.&lt;Foo&gt;d__1" filename="Ns\T.cs" line-rate="0.5" branch-rate="0.25" complexity="2">
      <methods><method name="MoveNext" signature="()" line-rate="0.5" branch-rate="0.25"><lines><line number="70" hits="1" branch="False" /><line number="71" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="70" hits="1" branch="False" /><line number="71" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@
        # XPath predicates compare against PARSED attribute values, hence the unescaped '<Foo>'.
        $beforeXml = $doc.SelectSingleNode('//class[@name="Ns.T.<Foo>d__1"]').OuterXml

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $after = $doc.SelectSingleNode('//class[@name="Ns.T.<Foo>d__1"]')

        # Retained unchanged: the <lines> set and both rate attributes survive byte-for-byte.
        $after | Should -Not -BeNullOrEmpty
        $after.OuterXml | Should -Be $beforeXml
        $after.'line-rate' | Should -Be '0.5'
        $after.'branch-rate' | Should -Be '0.25'
        @($after.SelectNodes('./lines/line')).Count | Should -Be 2
    }

    It 'removes covered closure lines from both the numerator and the denominator' {
        # Regression case 8 (Issue #457): the <>c__DisplayClass42_0 / DisposeProductionSurface
        # shape, whose lines are COVERED. Removing them must reduce lines-covered as well as
        # lines-valid, so the corrected rate is not covered / (valid - n). The declaring class
        # keeps the post-filter denominator non-zero so the recomputed rate is meaningful.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="0.5" branch-rate="0"><lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /><line number="11" hits="0" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass42_0" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="&lt;DisposeProductionSurface&gt;b__0" signature="()" line-rate="1" branch-rate="1"><lines><line number="80" hits="1" branch="False" /><line number="81" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="80" hits="1" branch="False" /><line number="81" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@
        $before = Get-CoberturaCoverageSummary -XmlDocument $doc

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $after = Get-CoberturaCoverageSummary -XmlDocument $doc

        # Before the filter the covered closure lines inflate both totals.
        $before.LinesValid | Should -Be '4'
        $before.LinesCovered | Should -Be '3'
        # After the filter they have left BOTH the numerator and the denominator.
        $after.LinesValid | Should -Be '2'
        $after.LinesCovered | Should -Be '1'
        # The recomputed rate is consistent with the reduced numerator AND denominator.
        $after.LineRate | Should -Be '0.5'
    }

    It 'creates a missing rollup and merges a line number shared by two retained methods' {
        # Coverage of the rebuild path's remaining branches: the closure class carries NO
        # class-level <lines> element, so the rollup must be created rather than emptied; and
        # line 21 appears in two RETAINED methods with differing hits, branch and
        # condition-coverage, so the de-duplication precedence rules must all fire.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="3">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method><method name="&lt;Visible&gt;b__1_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="21" hits="0" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="0" branch-rate="0"><lines><line number="21" hits="1" branch="True" condition-coverage="50% (1/2)" /><line number="22" hits="0" branch="False" /></lines></method></methods>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $closure = $doc.SelectSingleNode('//class[@name="Ns.T.<>c"]')
        $rollup = @($closure.SelectNodes('./lines/line'))
        $line21 = $closure.SelectSingleNode('./lines/line[@number="21"]')

        # The rollup element did not exist and was created from the retained methods.
        (($rollup | ForEach-Object { $_.number }) -join ',') | Should -Be '21,22'
        # Maximum hits wins, the branch flag is promoted, and the richer condition-coverage is kept.
        $line21.hits | Should -Be '1'
        $line21.branch | Should -Be 'True'
        $line21.'condition-coverage' | Should -Be '50% (1/2)'
        # Both rates are recomputed against the rebuilt rollup: 1 of 2 lines, 1 of 2 branches.
        $closure.'line-rate' | Should -Be '0.5'
        $closure.'branch-rate' | Should -Be '0.5'
    }

    It 'emits a zero rate when every retained method contributes no line' {
        # Coverage of the zero-denominator fallback: one method is dropped, so the rebuild runs,
        # but the sole retained method carries an empty <lines> element, leaving the rollup empty.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0.5" branch-rate="0.5" complexity="2">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="30" hits="0" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="0" branch-rate="0"><lines /></method></methods>
      <lines><line number="30" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        $closure = $doc.SelectSingleNode('//class[@name="Ns.T.<>c"]')
        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        # The class survives on the fail-safe retention of '.ctor', with an emptied rollup.
        $closure | Should -Not -BeNullOrEmpty
        @($closure.SelectNodes('./lines/line')).Count | Should -Be 0
        # Zero-denominator fallback, matching Get-CoberturaCoverageSummary's own '0' convention.
        $closure.'line-rate' | Should -Be '0'
        $closure.'branch-rate' | Should -Be '0'
        # Only the declaring class's line remains in the document totals.
        $summary.LinesValid | Should -Be '1'
    }

    It 'retains a closure whose bare member name collides with a non-exempt overload' {
        # Issue #733 finding 6: the presence set is keyed by BARE member name, so the exempt and
        # the non-exempt 'Overloaded' overloads share one entry. Only the non-exempt overload
        # emits a plain <method> element (the exempt one emits none), and that element admits the
        # shared name, so the exempt overload's closure resolves as present and survives.
        # This pins the CURRENT behaviour, which fails in the SAFE under-exclusion direction: the
        # exempt overload's lambda lines stay in the denominator permanently uncovered, so the
        # file measures no better than it truly is. The forbidden over-exclusion direction, in
        # which those lines would be deleted, is what a signature-based re-key would risk; that
        # re-key was evaluated and rejected as infeasible, per the P3-T2 addendum on
        # Get-CoberturaInstrumentedMemberName.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Overloaded" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c__DisplayClass1_0" filename="Ns\T.cs" line-rate="0" branch-rate="0" complexity="1">
      <methods><method name="&lt;Overloaded&gt;b__0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method></methods>
      <lines><line number="20" hits="0" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@

        Remove-CoberturaExemptClosureCoverage -XmlDocument $doc

        # XPath predicates compare against PARSED attribute values, hence the unescaped '<>'.
        $closure = $doc.SelectSingleNode('//class[@name="Ns.T.<>c__DisplayClass1_0"]')
        $summary = Get-CoberturaCoverageSummary -XmlDocument $doc

        $closure | Should -Not -BeNullOrEmpty
        # Scoped to the closure class's own rollup: each fixture line appears twice (once under
        # its <method>, once in the class-level <lines>), so an unscoped count would not identify
        # WHERE the line survived.
        @($closure.SelectNodes('./lines/line[@number="20"]')).Count | Should -Be 1
        @($closure.SelectNodes('./methods/method')).Count | Should -Be 1
        # Both lines remain in the denominator; only the declaring class's line is covered.
        $summary.LinesValid | Should -Be '2'
        $summary.LinesCovered | Should -Be '1'
    }
}

Describe 'Cobertura closure name derivation' {
    It 'derives declaring member, declaring type and closure classification purely from names' {
        # Regression case 9 (Issue #457): unit purity of the three pure name helpers. Every
        # assertion here is a direct call with a literal name; nothing touches a document.
        # These are the sole discharging verification for [P2-T1], [P2-T2] and [P2-T3].
        $expectedMembers = [ordered]@{
            '<M>b__0'                              = 'M'
            '<M>b__1_2'                            = 'M'
            '<M>g__L|3_0'                          = 'M'
            'Ns.T.<M>d__4'                         = 'M'
            'Ns.T.<>c__DisplayClass5_0.<<M>b__0>d' = 'M'
            'MoveNext'                             = $null
            '.ctor'                                = $null
        }

        foreach ($name in $expectedMembers.Keys) {
            $because = "input '$name'"
            { Get-CoberturaClosureDeclaringMemberName -Name $name } | Should -Not -Throw -Because $because

            $errorRecords = $null
            $warningRecords = $null
            $informationRecords = $null
            # Verbose (stream 4) is merged into success output and then partitioned by type, so
            # both the success-object count and the verbose-record count can be asserted.
            $rawOutput = @(
                Get-CoberturaClosureDeclaringMemberName -Name $name `
                    -ErrorVariable errorRecords `
                    -WarningVariable warningRecords `
                    -InformationVariable informationRecords 4>&1
            )

            $successObjects = [System.Collections.ArrayList]::new()
            $verboseCount = 0
            foreach ($item in $rawOutput) {
                if ($item -is [System.Management.Automation.VerboseRecord]) { $verboseCount++ }
                else { $null = $successObjects.Add($item) }
            }

            # Exactly one object on the success stream (its return value, which may be $null).
            $successObjects.Count | Should -Be 1 -Because $because
            $successObjects[0] | Should -Be $expectedMembers[$name] -Because $because
            # Nothing on the error, warning, verbose or information streams.
            @($errorRecords).Count | Should -Be 0 -Because $because
            @($warningRecords).Count | Should -Be 0 -Because $because
            @($informationRecords).Count | Should -Be 0 -Because $because
            $verboseCount | Should -Be 0 -Because $because
        }

        # Get-CoberturaDeclaringTypeName truncates at the first '.<'; a name with no '.<' is
        # returned unchanged.
        Get-CoberturaDeclaringTypeName -Name 'Ns.T.<>c' | Should -Be 'Ns.T'
        Get-CoberturaDeclaringTypeName -Name 'Ns.T.<>c__DisplayClass5_0' | Should -Be 'Ns.T'
        Get-CoberturaDeclaringTypeName -Name 'Ns.T.<M>d__4' | Should -Be 'Ns.T'
        Get-CoberturaDeclaringTypeName -Name 'Ns.T' | Should -Be 'Ns.T'
        Get-CoberturaDeclaringTypeName -Name 'Ns.Outer.Inner' | Should -Be 'Ns.Outer.Inner'

        # Test-CoberturaClosureClassName is true for the '.<>c' marker in every shape and
        # deliberately false for a Type.<Member>d__N state machine and for a plain type.
        Test-CoberturaClosureClassName -Name 'Ns.T.<>c' | Should -BeTrue
        Test-CoberturaClosureClassName -Name 'Ns.T.<>c__DisplayClass5_0' | Should -BeTrue
        Test-CoberturaClosureClassName -Name 'Ns.T.<>c__DisplayClass5_0.<<M>b__0>d' | Should -BeTrue
        Test-CoberturaClosureClassName -Name 'Ns.T.<M>d__4' | Should -BeFalse
        Test-CoberturaClosureClassName -Name 'Ns.T' | Should -BeFalse
    }

    It 'is idempotent and silent when applied twice to the same document' {
        # Regression case 10 (Issue #457): reuses the case-4 shape, which the filter does modify,
        # so idempotence is a real property here rather than a vacuous one over an untouched
        # document. Also pins the 'no output stream content' requirement of spec AC 11.
        [xml]$doc = @'
<coverage line-rate="0" branch-rate="0" lines-covered="0" lines-valid="0" branches-covered="0" branches-valid="0">
  <packages><package name="Ns" line-rate="0" branch-rate="0" complexity="1"><classes>
    <class name="Ns.T" filename="Ns\T.cs" line-rate="1" branch-rate="1" complexity="1">
      <methods><method name="Visible" signature="()" line-rate="1" branch-rate="1"><lines><line number="10" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="10" hits="1" branch="False" /></lines>
    </class>
    <class name="Ns.T.&lt;&gt;c" filename="Ns\T.cs" line-rate="0.5" branch-rate="0" complexity="3">
      <methods><method name="&lt;Exempt&gt;b__0_0" signature="()" line-rate="0" branch-rate="0"><lines><line number="20" hits="0" branch="False" /></lines></method><method name="&lt;Visible&gt;b__1_0" signature="()" line-rate="1" branch-rate="0"><lines><line number="21" hits="1" branch="False" /></lines></method><method name=".ctor" signature="()" line-rate="1" branch-rate="0"><lines><line number="22" hits="1" branch="False" /></lines></method></methods>
      <lines><line number="20" hits="0" branch="False" /><line number="21" hits="1" branch="False" /><line number="22" hits="1" branch="False" /></lines>
    </class>
  </classes></package></packages>
</coverage>
'@
        $originalXml = $doc.OuterXml
        $firstErrors = $null
        $firstWarnings = $null
        $firstInformation = $null
        $secondErrors = $null
        $secondWarnings = $null
        $secondInformation = $null

        $firstOutput = @(
            Remove-CoberturaExemptClosureCoverage -XmlDocument $doc `
                -ErrorVariable firstErrors `
                -WarningVariable firstWarnings `
                -InformationVariable firstInformation 4>&1
        )
        $afterFirstPass = $doc.OuterXml

        $secondOutput = @(
            Remove-CoberturaExemptClosureCoverage -XmlDocument $doc `
                -ErrorVariable secondErrors `
                -WarningVariable secondWarnings `
                -InformationVariable secondInformation 4>&1
        )
        $afterSecondPass = $doc.OuterXml

        # The first pass really does change the document, so idempotence is non-vacuous.
        $afterFirstPass | Should -Not -Be $originalXml
        # The second pass produces no further change.
        $afterSecondPass | Should -Be $afterFirstPass
        # Both invocations together emit nothing on any stream (verbose is merged into $*Output).
        ($firstOutput.Count + $secondOutput.Count) | Should -Be 0
        (@($firstErrors).Count + @($secondErrors).Count) | Should -Be 0
        (@($firstWarnings).Count + @($secondWarnings).Count) | Should -Be 0
        (@($firstInformation).Count + @($secondInformation).Count) | Should -Be 0
    }
}
