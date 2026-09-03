Set-StrictMode -Version Latest

function Test-CoberturaClosureClassName {
    <#
        .SYNOPSIS
        Reports whether a Cobertura <class> name denotes a compiler-generated closure type.

        .DESCRIPTION
        The C# compiler hoists lambdas declared inside a member into a closure type whose name
        carries the '.<>c' marker: '<>c' for a stateless lambda cache, '<>c__DisplayClass<N>_<M>'
        for a capturing closure, either of them optionally carrying a generic suffix, and
        '<>c...<<Member>b__K>d' for an async lambda nested inside a closure.

        A 'Type.<Member>d__<N>' async or iterator state machine is deliberately NOT a closure
        class: it is the whole body of the declaring member rather than a lambda declared inside
        it, and it is the only trace an async member leaves in the report. Classifying it as a
        closure would delete the coverage of every async member.

        The function is pure: it performs no I/O and mutates nothing.

        .PARAMETER Name
        A Cobertura <class> name attribute value.

        .OUTPUTS
        [bool] $true when the name carries the '.<>c' marker.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Name
    )

    return $Name.Contains('.<>c')
}

function Get-CoberturaClosureDeclaringMemberName {
    <#
        .SYNOPSIS
        Derives the declaring member token from a synthesized class or method name.

        .DESCRIPTION
        Recognizes the four Roslyn name shapes that carry a declaring-member token:

          '<Member>b__...'        a lambda hoisted into a closure type
          '<Member>g__Local|N_M'  a local function
          'Type.<Member>d__<N>'   an async or iterator state machine (last such segment)
          '...<<Member>b__K>d'    an async lambda nested inside a closure type

        Any other shape - 'MoveNext', '.ctor', a plain member name - yields $null. That is the
        fail-safe direction: the caller retains a method whose declaring member could not be
        derived, so an unrecognized or drifted name shape can never cause coverage to be removed.

        The function is pure: it performs no I/O, mutates nothing, and writes nothing to any
        stream other than its single return value.

        .PARAMETER Name
        A Cobertura <method> name or <class> name attribute value.

        .OUTPUTS
        [string] the declaring member token, or $null when no shape matches.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Name
    )

    $lambda = [regex]::Match($Name, '^<(?<m>[^<>]+)>b__')
    if ($lambda.Success) {
        return $lambda.Groups['m'].Value
    }

    $localFunction = [regex]::Match($Name, '^<(?<m>[^<>]+)>g__')
    if ($localFunction.Success) {
        return $localFunction.Groups['m'].Value
    }

    # Checked before the plain d__ shape: the inner token of a nested async lambda is the
    # declaring member, whereas the outer '>d' suffix carries no digits and would not match.
    $nestedAsyncLambda = [regex]::Match($Name, '<<(?<m>[^<>]+)>b__\d+>d')
    if ($nestedAsyncLambda.Success) {
        return $nestedAsyncLambda.Groups['m'].Value
    }

    # The LAST such segment: a state machine nested inside another synthesized type carries the
    # innermost declaring member last.
    $stateMachines = [regex]::Matches($Name, '<(?<m>[^<>]+)>d__\d+')
    if ($stateMachines.Count -gt 0) {
        return $stateMachines[$stateMachines.Count - 1].Groups['m'].Value
    }

    return $null
}

function Get-CoberturaDeclaringTypeName {
    <#
        .SYNOPSIS
        Returns the declaring type of a Cobertura <class> name.

        .DESCRIPTION
        Every synthesized nested type - closure, display class or state machine - is named
        '<DeclaringType>.<' followed by the synthesized part. Truncating at the first '.<'
        therefore recovers the declaring type. A name carrying no '.<' is already a declaring
        type and is returned unchanged, including a nested type such as 'Ns.Outer.Inner'.

        The function is pure: it performs no I/O and mutates nothing.

        .PARAMETER Name
        A Cobertura <class> name attribute value.

        .OUTPUTS
        [string] the declaring type name.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Name
    )

    $markerIndex = $Name.IndexOf('.<')
    if ($markerIndex -lt 0) {
        return $Name
    }

    return $Name.Substring(0, $markerIndex)
}

function Get-CoberturaInstrumentedMemberName {
    <#
        .SYNOPSIS
        Builds the set of instrumented member names for one Cobertura <package>.

        .DESCRIPTION
        Returns a hashtable keyed by "$declaringType|$filename" whose values are ordinal string
        sets of member names present in the report for that declaring type and source file. A
        member is admitted from exactly two sources:

          1. a plain <method name="X"> on a class whose name contains no '.<', where X does not
             begin with '<' - the ordinary case of an instrumented member;
          2. the <Member> token of a class named 'Type.<Member>d__<N>' - an async or iterator
             member, whose whole body moves to a state machine and which therefore emits no plain
             <method> element of its own.

        Source 2 is mandatory rather than optional. Without it, a lambda declared inside a
        non-exempt async member would resolve to an apparently absent declaring member and its
        coverage would be wrongly deleted.

        '<Member>g__Local|N_M' local functions are deliberately NOT admitted. A local function is
        emitted inside the declaring type's own class and does not inherit the member's
        attribute, so admitting it would let an exempt member's local function mask the member's
        absence and keep its lambdas in the denominator.

        That non-admission is an asserted design choice rather than a measured one (issue #733
        finding 5). Issue #733's research ratified it because no over-exclusion counter-example
        was found or could be constructed: every candidate reduced to a member that also emits a
        plain <method> element or a state-machine class, and is therefore already admitted by
        source 1 or source 2. Revisit this choice if a genuine case is ever observed in which a
        non-exempt member's only entry in the report is a local-function entry; that member would
        resolve as absent and its lambdas would be removed, which is the forbidden over-exclusion
        direction.

        The keys are per (declaring type, filename) rather than per declaring type alone, so a
        partial type spanning files errs toward under-exclusion rather than over-exclusion.

        Known limitation, bare-name overload collision (issue #733 finding 6): the members inside
        each key are stored by bare member name with no parameter signature, so two overloads
        sharing a name under the same declaring type and source file occupy one entry. If one
        overload is exempt and the other is not, the non-exempt overload's plain <method> element
        admits the shared name, and the exempt overload's closures then resolve as present and are
        retained. The resulting failure direction is the safe one, under-exclusion: the exempt
        overload's lambdas stay in the coverage denominator permanently uncovered, so the file
        measures no better than it truly is. It is not the forbidden direction, over-exclusion,
        in which coverage for a member the filter failed to resolve would be deleted. A
        signature-based re-key was evaluated and rejected as infeasible in this item's Root Cause
        Analysis: Get-CoberturaClosureDeclaringMemberName can never recover a parameter signature
        from Roslyn's closure-naming convention, which encodes only the bare member name, so
        forcing a signature key would flip the failure direction from safe under-exclusion to
        forbidden over-exclusion.

        The function is pure: it reads the supplied node and mutates nothing.

        .PARAMETER PackageNode
        A Cobertura <package> element.

        .OUTPUTS
        [hashtable] keyed by "$declaringType|$filename", each value a
        [System.Collections.Generic.HashSet[string]] of member names.
    #>
    [CmdletBinding()]
    [OutputType([hashtable])]
    param(
        [Parameter(Mandatory = $true)]
        [System.Xml.XmlElement]$PackageNode
    )

    $presence = @{}

    foreach ($classNode in @($PackageNode.SelectNodes('.//class[@filename]'))) {
        $className = $classNode.GetAttribute('name')
        # GetAttribute is used rather than bare property access because Set-StrictMode -Version
        # Latest makes a missing XML attribute throw instead of returning $null.
        $key = '{0}|{1}' -f (Get-CoberturaDeclaringTypeName -Name $className), $classNode.GetAttribute('filename')

        if (-not $presence.Contains($key)) {
            $presence[$key] = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::Ordinal)
        }

        # Source 1: plain methods on a non-synthesized class.
        if (-not $className.Contains('.<')) {
            foreach ($methodNode in @($classNode.SelectNodes('./methods/method'))) {
                $methodName = $methodNode.GetAttribute('name')
                if (-not $methodName.StartsWith('<')) {
                    $null = $presence[$key].Add($methodName)
                }
            }
        }

        # Source 2: an async or iterator state-machine class name. Anchored at the end so a
        # closure-nested state machine ('...<<M>b__0>d') does not admit a spurious member.
        $stateMachine = [regex]::Match($className, '<(?<m>[^<>]+)>d__\d+$')
        if ($stateMachine.Success) {
            $null = $presence[$key].Add($stateMachine.Groups['m'].Value)
        }
    }

    return $presence
}

function Remove-CoberturaExemptClosureCoverage {
    <#
        .SYNOPSIS
        Removes closure-type coverage whose declaring member is absent from the report.

        .DESCRIPTION
        A method-level [ExcludeFromCodeCoverage] attribute suppresses the attributed member but
        not the lambdas declared inside it: the compiler hoists those into a closure type that
        does not inherit the attribute, so their lines stay in the denominator permanently
        uncovered (issue #457). This filter removes them by inferring exemption from the
        declaring member's absence from the instrumented method set of the same declaring type
        and source file.

        For each <package>: build the presence set, then for each closure class derive each
        <method>'s declaring member, falling back to a class-name-derived token when the method
        name yields none (for example MoveNext on a nested async-lambda state machine). Drop
        methods whose declaring member is absent from the presence set for that
        (declaringType, filename) key. Then, when at least one method was dropped, rebuild the
        class-level <lines> from the retained methods and recompute the rates; when zero methods
        are retained, remove the <class> element outright.

        Fail-safe invariant, non-negotiable: a method whose declaring member could not be derived
        is RETAINED. No code path removes coverage for a member the filter failed to resolve.
        Over-exclusion is not an acceptable failure mode; every failure mode of the key is in the
        under-exclusion direction, so a file measures no better than it truly is.

        A <class> whose name carries no '.<>c' marker is never mutated. A closure class with no
        <methods> element, or an empty one, is left untouched: "zero methods present" is not
        "zero methods retained", and treating it as such would delete coverage the filter never
        resolved.

        The transform is pure with respect to the outside world - no file, process, clock or
        network access - and is idempotent: a second pass finds nothing further to drop.

        Ordering constraint: this function MUST run before Merge-CoberturaClassesByFilename. A
        closure type always shares its declaring type's filename, so the merge collapses it and
        the surviving node is named for the declaring type, carries no '.<>c' marker, and no
        longer holds the '<Member>b__...' method names this function resolves against. Running it
        after the merge does not degrade the result, it makes the result unobtainable.

        .PARAMETER XmlDocument
        A Cobertura document, mutated in place. Matches the existing mutation convention of
        Merge-CoberturaClassesByFilename.

        .OUTPUTS
        None. The document is mutated in place.
    #>
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [Parameter(Mandatory = $true)]
        [xml]$XmlDocument
    )

    if (-not $PSCmdlet.ShouldProcess('Cobertura document', 'Remove exempt closure coverage')) {
        return
    }

    foreach ($packageNode in @($XmlDocument.SelectNodes('//package'))) {
        $presence = Get-CoberturaInstrumentedMemberName -PackageNode $packageNode

        foreach ($classNode in @($packageNode.SelectNodes('.//class[@filename]'))) {
            $className = $classNode.GetAttribute('name')
            if (-not (Test-CoberturaClosureClassName -Name $className)) {
                continue
            }

            $methodNodes = @($classNode.SelectNodes('./methods/method'))
            if ($methodNodes.Count -eq 0) {
                continue
            }

            # The presence set is built over the same class node set this loop walks, so every
            # closure class's own key is guaranteed to exist; an else branch here would be dead.
            $key = '{0}|{1}' -f (Get-CoberturaDeclaringTypeName -Name $className), $classNode.GetAttribute('filename')
            $presentMembers = $presence[$key]

            $retainedMethods = [System.Collections.ArrayList]::new()
            $droppedMethods = [System.Collections.ArrayList]::new()

            foreach ($methodNode in $methodNodes) {
                $declaringMember = Get-CoberturaClosureDeclaringMemberName -Name $methodNode.GetAttribute('name')
                if ($null -eq $declaringMember) {
                    $declaringMember = Get-CoberturaClosureDeclaringMemberName -Name $className
                }

                if ($null -eq $declaringMember) {
                    # Fail-safe: an unresolved declaring member is always retained.
                    $null = $retainedMethods.Add($methodNode)
                    continue
                }

                if ($presentMembers.Contains($declaringMember)) {
                    $null = $retainedMethods.Add($methodNode)
                }
                else {
                    $null = $droppedMethods.Add($methodNode)
                }
            }

            if ($droppedMethods.Count -eq 0) {
                continue
            }

            foreach ($methodNode in $droppedMethods) {
                $null = $methodNode.ParentNode.RemoveChild($methodNode)
            }

            if ($retainedMethods.Count -eq 0) {
                $null = $classNode.ParentNode.RemoveChild($classNode)
                continue
            }

            # Rebuild the class-level rollup as the de-duplicated union of the RETAINED methods'
            # lines, keyed by line number, taking the maximum hits and the richest
            # condition-coverage. This mirrors Merge-CoberturaClassesByFilename's own rebuild and
            # reuses Get-CoberturaLineConditionCoverageParts for the precedence rule rather than
            # re-deriving it.
            $linesNode = $classNode.SelectSingleNode('./lines')
            if ($linesNode) {
                $linesNode.RemoveAll()
            }
            else {
                $linesNode = $XmlDocument.CreateElement('lines')
                $null = $classNode.AppendChild($linesNode)
            }

            $lineMap = @{}
            foreach ($methodNode in $retainedMethods) {
                foreach ($lineNode in @($methodNode.SelectNodes('./lines/line'))) {
                    $lineNumber = [int]$lineNode.GetAttribute('number')
                    $candidateCoverage = Get-CoberturaLineConditionCoverageParts -LineNode $lineNode

                    if (-not $lineMap.Contains($lineNumber)) {
                        $lineMap[$lineNumber] = [pscustomobject]@{
                            Node    = $lineNode.CloneNode($true)
                            Covered = $candidateCoverage.Covered
                            Total   = $candidateCoverage.Total
                        }
                        continue
                    }

                    $existing = $lineMap[$lineNumber]
                    $existingNode = $existing.Node
                    $existingNode.SetAttribute('hits', [string]([math]::Max([int]$existingNode.GetAttribute('hits'), [int]$lineNode.GetAttribute('hits'))))

                    if ($existingNode.GetAttribute('branch') -ne 'True' -and $lineNode.GetAttribute('branch') -eq 'True') {
                        $existingNode.SetAttribute('branch', 'True')
                    }

                    if (
                        $candidateCoverage.Total -gt $existing.Total -or
                        ($candidateCoverage.Total -eq $existing.Total -and $candidateCoverage.Covered -gt $existing.Covered)
                    ) {
                        $existing.Covered = $candidateCoverage.Covered
                        $existing.Total = $candidateCoverage.Total

                        if ($lineNode.HasAttribute('condition-coverage')) {
                            $existingNode.SetAttribute('condition-coverage', $lineNode.GetAttribute('condition-coverage'))
                        }
                    }
                }
            }

            foreach ($lineNumber in ($lineMap.Keys | Sort-Object)) {
                $null = $linesNode.AppendChild($lineMap[$lineNumber].Node)
            }

            # The rate expressions match Get-CoberturaCoverageSummary and the merge path exactly,
            # including the '0' zero-denominator fallback, so a consumer that reads the attribute
            # and one that recomputes from <lines> see the same document.
            $retainedSummary = Get-CoberturaClassLineSummary -ClassNode $classNode
            $retainedLineRate = if ($retainedSummary.TotalLines -gt 0) { [string]([math]::Round($retainedSummary.CoveredLines / $retainedSummary.TotalLines, 6)) } else { '0' }
            $retainedBranchRate = if ($retainedSummary.TotalBranches -gt 0) { [string]([math]::Round($retainedSummary.CoveredBranches / $retainedSummary.TotalBranches, 6)) } else { '0' }

            $classNode.SetAttribute('line-rate', $retainedLineRate)
            $classNode.SetAttribute('branch-rate', $retainedBranchRate)
        }
    }
}
