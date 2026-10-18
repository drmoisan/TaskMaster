BeforeAll {
    $source = Join-Path $PSScriptRoot '../../../.codex/lib/codex-routing/CodexDeployment.psm1'
    Import-Module $source -Force
}

AfterAll {
    Remove-Module CodexDeployment -ErrorAction SilentlyContinue
}

Describe 'Issue 995 deterministic Codex deployment' {
    It 'resolves <Band> to its configured model and effort' -TestCases @(
        @{ Band = 'C1'; Model = 'gpt-6-luna'; Effort = 'low'; Suffix = 'c1' }
        @{ Band = 'C2'; Model = 'gpt-6.1-sol'; Effort = 'medium'; Suffix = 'c2' }
        @{ Band = 'C3'; Model = 'gpt-6.1-sol'; Effort = 'high'; Suffix = 'c3' }
        @{ Band = 'C4'; Model = 'gpt-6-astra'; Effort = 'max'; Suffix = 'c4' }
    ) {
        param($Band, $Model, $Effort, $Suffix); $null = @($Band, $Model, $Effort, $Suffix);
        $actual = Resolve-CodexDeployment -LogicalAgent atomic-executor -ComplexityBand $Band -ExecutionContext standalone -OrchestrationComplexityCeiling $Band

        $actual.deployment_agent | Should -BeExactly "atomic-executor-$Suffix"
        $actual.model | Should -BeExactly $Model
        $actual.model_reasoning_effort | Should -BeExactly $Effort
        $actual.c3_overlay_applied | Should -BeFalse
        $actual.c3_overlay_reason | Should -BeNullOrEmpty
        $actual.logical_agent | Should -BeExactly 'atomic-executor'
        $actual.complexity_band | Should -BeExactly $Band
        $actual.execution_context | Should -BeExactly 'standalone'
        $actual.orchestration_complexity_ceiling | Should -BeExactly $Band
        @($actual.Keys).Count | Should -Be 9
    }

    It 'elevates C3 for <Context> and ceiling <Ceiling>' -TestCases @(
        @{ Context = 'epic_preparation_child'; Ceiling = 'C3'; Reason = 'epic_context' }
        @{ Context = 'epic_execution_child'; Ceiling = 'C3'; Reason = 'epic_context' }
        @{ Context = 'standalone'; Ceiling = 'C4'; Reason = 'c4_orchestration_ceiling' }
        @{ Context = 'epic_execution_child'; Ceiling = 'C4'; Reason = 'epic_context_and_c4_ceiling' }
    ) {
        param($Context, $Ceiling, $Reason); $null = @($Context, $Ceiling, $Reason);
        $actual = Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C3 -ExecutionContext $Context -OrchestrationComplexityCeiling $Ceiling

        $actual.deployment_agent | Should -BeExactly 'orchestrator-c3-elevated'
        $actual.model | Should -BeExactly 'gpt-6-astra'
        $actual.model_reasoning_effort | Should -BeExactly 'high'
        $actual.c3_overlay_applied | Should -BeTrue
        $actual.c3_overlay_reason | Should -BeExactly $Reason
    }

    It 'forces <Persona> to Astra ultra regardless of assessed band' -TestCases @(
        @{ Persona = 'epic-planner' }
        @{ Persona = 'epic-orchestrator' }
    ) {
        param($Persona); $null = @($Persona);
        $actual = Resolve-CodexDeployment -LogicalAgent $Persona -ComplexityBand C1 -ExecutionContext standalone -OrchestrationComplexityCeiling C4

        $actual.deployment_agent | Should -BeExactly $Persona
        $actual.model | Should -BeExactly 'gpt-6-astra'
        $actual.model_reasoning_effort | Should -BeExactly 'ultra'
        $actual.c3_overlay_applied | Should -BeFalse
    }

    It 'normalizes the feature-review alias without changing its logical identity' {
        $actual = Resolve-CodexDeployment -LogicalAgent feature-review -ComplexityBand C2 -ExecutionContext standalone -OrchestrationComplexityCeiling C3

        $actual.logical_agent | Should -BeExactly 'feature-review'
        $actual.deployment_agent | Should -BeExactly 'feature-reviewer-c2'
    }

    It 'rejects invalid band <Value>' -TestCases @(@{ Value = 'C0' }, @{ Value = '' }, @{ Value = 'c3' }) {
        param($Value); $null = @($Value);
        { Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand $Value -ExecutionContext standalone -OrchestrationComplexityCeiling C4 } | Should -Throw '*complexity_band must be one of*'
    }

    It 'rejects an invalid ceiling and a ceiling below the assessment' {
        { Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C2 -ExecutionContext standalone -OrchestrationComplexityCeiling invalid } | Should -Throw '*orchestration_complexity_ceiling must be one of*'
        { Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C3 -ExecutionContext standalone -OrchestrationComplexityCeiling C2 } | Should -Throw '*below C3*'
    }

    It 'rejects unsupported execution context and logical agent' {
        { Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C2 -ExecutionContext invalid -OrchestrationComplexityCeiling C2 } | Should -Throw '*execution_context must be one of*'
        { Resolve-CodexDeployment -LogicalAgent unsupported -ComplexityBand C2 -ExecutionContext standalone -OrchestrationComplexityCeiling C2 } | Should -Throw '*Unsupported Codex logical agent*'
    }

    It 'fails closed when the required model is unavailable' {
        { Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C3 -ExecutionContext standalone -OrchestrationComplexityCeiling C3 -AvailableModel @('gpt-6-astra') } | Should -Throw '*model_unavailable*'
        $actual = Resolve-CodexDeployment -LogicalAgent orchestrator -ComplexityBand C3 -ExecutionContext standalone -OrchestrationComplexityCeiling C3 -AvailableModel @('gpt-6.1-sol')

        $actual.model | Should -BeExactly 'gpt-6.1-sol'
    }
}
