# Mock signatures retain production parameter names; unused boundary inputs are explicitly discarded.
BeforeDiscovery {
    $source = Join-Path $PSScriptRoot '../../../.codex/hooks/record-subagent-routing-attestation.ps1'
    $module = New-Module -Name Issue995Attestation -ArgumentList $source -ScriptBlock {
        param($SourcePath); $null = @($SourcePath);
        . $SourcePath
        Export-ModuleMember -Function *
    }
    Import-Module $module -Force
}

AfterAll {
    Remove-Module Issue995Attestation -ErrorAction SilentlyContinue
}

InModuleScope Issue995Attestation {
    Describe 'Issue 995 isolated attestation behavior' {
        BeforeAll {
            function Get-Issue995Payload {
                [pscustomobject]@{
                    agent_id = 'worker'; agent_type = 'atomic-executor-c3'; model = 'gpt-6.1-sol'
                    session_id = 'session'; turn_id = 'turn'; transcript_path = 'transcript'
                }
            }

            function Get-Issue995RootReceipt {
                [pscustomobject]@{
                    schema_version = 1; repository_root = $script:repository; repository_sha256 = ('a' * 64)
                    repository_head_sha = ('b' * 40); session_id = 'session'; turn_id = 'turn'
                    prompt_sha256 = ('c' * 64); requested_persona = 'epic-planner'; entry_kind = 'epic-plan'
                    epic_reference = 'example'; kickoff_path = ''; created_at = $script:now.AddMinutes(-1).ToString('o')
                    expires_at = $script:now.AddMinutes(5).ToString('o'); consumed = $false; consumed_by = $null; consumed_at = $null
                }
            }

            function Invoke-Issue995Attestation {
                Get-CodexSubagentAttestation -Payload $script:payload -RootReceipt $null -Checkpoints @($script:checkpoint) -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now
            }
        }

        BeforeEach {
            $script:repository = Join-Path ([IO.Path]::GetPathRoot($PSScriptRoot)) 'issue-995-workspace'
            $script:now = [datetimeoffset]'2026-10-17T12:00:00Z'
            $script:payload = Get-Issue995Payload
            $script:checkpoint = [pscustomobject]@{ codex_model_routing_receipts = @([pscustomobject]@{ deployment_agent = 'atomic-executor-c3'; model = 'gpt-6.1-sol'; model_reasoning_effort = 'high' }) }
            $script:profile = [pscustomobject]@{
                profile_name = 'atomic-executor-c3'; profile_model = 'gpt-6.1-sol'; profile_reasoning_effort = 'high'
                profile_path = '.codex/agents/atomic-executor-c3.toml'; profile_sha256 = ('d' * 64)
            }
            Mock Get-CodexAgentProfileAttestation { param($RepositoryRoot, $AgentType); $null = @($RepositoryRoot, $AgentType); $script:profile }
            Mock Get-CodexAuthorityRepositoryKey { param($RepositoryRoot); $null = @($RepositoryRoot); 'a' * 64 }
            Mock Get-CodexCanonicalAuthorityPath { param($Path); $null = @($Path); $Path }
            Mock Test-CodexEpicChildRoutingLaunchAuthority { param($RoutingReceipt, $Payload, $RepositoryRoot, $LaunchEnvironment, $LaunchReceiptRaw, $Now); $null = @($RoutingReceipt, $Payload, $RepositoryRoot, $LaunchEnvironment, $LaunchReceiptRaw, $Now); $true }
            Mock Get-CodexAuthorityReceiptPath { param($RepositoryRoot, $SessionId, $TurnId); $null = @($RepositoryRoot, $SessionId, $TurnId); Join-Path $RepositoryRoot 'receipt.json' }
            Mock Get-CodexAuthorityStateRoot { param($RepositoryRoot, $SessionId); $null = @($RepositoryRoot, $SessionId); Join-Path $RepositoryRoot 'authority' }
            Mock Get-CodexAuthorityAttestationPath { param($RepositoryRoot, $SessionId, $AttestationKey); $null = @($RepositoryRoot, $SessionId, $AttestationKey); Join-Path $RepositoryRoot 'attestation.json' }
            Mock Test-Path { param($LiteralPath, $PathType); $null = @($LiteralPath, $PathType); $false }
        }

        Context 'Issue 995 JSON and routing boundaries' {
            It 'rejects empty/malformed required input but tolerates optional absence' {
                { ConvertFrom-SubagentAttestationJson -Raw '' -Name input } | Should -Throw '*is empty*'
                { ConvertFrom-SubagentAttestationJson -Raw '{' -Name input } | Should -Throw '*malformed JSON*'
                ConvertFrom-SubagentAttestationJson -Raw '' -Name optional -Optional | Should -BeNullOrEmpty
                ConvertFrom-SubagentAttestationJson -Raw '{' -Name optional -Optional | Should -BeNullOrEmpty
                (ConvertFrom-SubagentAttestationJson -Raw '{"value":1}' -Name input).value | Should -Be 1
            }

            It 'accepts the native model and records the profile identity' {
                $actual = Invoke-Issue995Attestation
                $actual.routing_valid | Should -BeTrue
                $actual.launch_authority_valid | Should -BeTrue
                $actual.provenance_valid | Should -BeTrue
                $actual.actual_model | Should -BeExactly 'gpt-6.1-sol'
                $actual.actual_reasoning_effort | Should -BeExactly 'high'
                $actual.profile_sha256 | Should -BeExactly ('d' * 64)
                $actual.attestation_key | Should -Match '^[a-f0-9]{64}$'
            }

            It 'rejects an incorrect actual model or profile effort' {
                $script:payload.model = 'gpt-6-astra'
                (Invoke-Issue995Attestation).routing_valid | Should -BeFalse
                $script:payload.model = 'gpt-6.1-sol'
                $script:profile.profile_reasoning_effort = 'medium'
                (Invoke-Issue995Attestation).routing_valid | Should -BeFalse
            }

            It 'preserves explicit helper path/hash rejection separately from hook routing' {
                $arguments = @{ AgentProfile = $script:profile; AgentType = 'atomic-executor-c3'; ActualModel = 'gpt-6.1-sol'; ExpectedModel = 'gpt-6.1-sol'; ExpectedReasoningEffort = 'high'; ExpectedProfilePath = '.codex/agents/atomic-executor-c3.toml'; ExpectedProfileSha256 = ('d' * 64) }
                Test-CodexAgentProfileBinding @arguments | Should -BeTrue
                $arguments.ExpectedProfileSha256 = 'e' * 64
                Test-CodexAgentProfileBinding @arguments | Should -BeFalse
                $arguments.ExpectedProfileSha256 = 'd' * 64
                $arguments.ExpectedProfilePath = 'incorrect'
                Test-CodexAgentProfileBinding @arguments | Should -BeFalse
            }

            It 'fails closed for absent receipts, incomplete receipt fields or profile read failures' {
                $script:checkpoint.codex_model_routing_receipts = @()
                (Invoke-Issue995Attestation).routing_valid | Should -BeFalse
                $script:checkpoint.codex_model_routing_receipts = @([pscustomobject]@{ deployment_agent = 'atomic-executor-c3' })
                (Invoke-Issue995Attestation).routing_valid | Should -BeFalse
                Mock Get-CodexAgentProfileAttestation { param($RepositoryRoot, $AgentType); $null = @($RepositoryRoot, $AgentType); throw 'profile unavailable' }
                (Invoke-Issue995Attestation).profile_validation_error | Should -Match 'profile unavailable'
            }

            It 'rejects missing startup identity and failed launch authority' {
                $script:payload.agent_id = ''
                { Invoke-Issue995Attestation } | Should -Throw '*requires agent_id*'
                $script:payload.agent_id = 'worker'
                Mock Test-CodexEpicChildRoutingLaunchAuthority { param($RoutingReceipt, $Payload, $RepositoryRoot, $LaunchEnvironment, $LaunchReceiptRaw, $Now); $null = @($RoutingReceipt, $Payload, $RepositoryRoot, $LaunchEnvironment, $LaunchReceiptRaw, $Now); $false }
                (Invoke-Issue995Attestation).routing_valid | Should -BeFalse
            }

            It 'handles an unrouted agent and hashes fallback identity deterministically' {
                $script:payload.agent_type = 'explorer'
                (Invoke-Issue995Attestation).routing_valid | Should -BeTrue
                $first = Get-SubagentAttestationKey -TranscriptPath '' -AgentId worker
                $first | Should -BeExactly (Get-SubagentAttestationKey -TranscriptPath $null -AgentId worker)
                $first | Should -Not -BeExactly (Get-SubagentAttestationKey -TranscriptPath transcript -AgentId worker)
            }
        }

        Context 'Issue 995 epic root authority' {
            BeforeEach {
                $script:payload.agent_type = 'epic-planner'
                $script:payload.model = 'gpt-6-astra'
                $script:profile.profile_name = 'epic-planner'
                $script:profile.profile_model = 'gpt-6-astra'
                $script:profile.profile_reasoning_effort = 'ultra'
                $script:receipt = Get-Issue995RootReceipt
            }

            It 'accepts valid root authority and preserves forced model identity' {
                $actual = Get-CodexSubagentAttestation -Payload $script:payload -RootReceipt $script:receipt -Checkpoints @() -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now
                $actual.provenance_valid | Should -BeTrue
                $actual.root_authorized | Should -BeTrue
                $actual.root_entry_kind | Should -BeExactly 'epic-plan'
                $actual.expected_model | Should -BeExactly 'gpt-6-astra'
                $actual.expected_reasoning_effort | Should -BeExactly 'ultra'
            }

            It 'rejects invalid receipt field <Field>' -TestCases @(
                @{ Field = 'schema_version'; Value = 2 }
                @{ Field = 'repository_root'; Value = 'wrong' }
                @{ Field = 'repository_sha256'; Value = 'wrong' }
                @{ Field = 'repository_head_sha'; Value = 'wrong' }
                @{ Field = 'session_id'; Value = 'wrong' }
                @{ Field = 'turn_id'; Value = 'wrong' }
                @{ Field = 'requested_persona'; Value = 'wrong' }
                @{ Field = 'prompt_sha256'; Value = 'wrong' }
                @{ Field = 'consumed'; Value = $true }
                @{ Field = 'consumed_by'; Value = 'worker' }
                @{ Field = 'consumed_at'; Value = 'earlier' }
                @{ Field = 'entry_kind'; Value = 'wrong' }
                @{ Field = 'epic_reference'; Value = '' }
                @{ Field = 'kickoff_path'; Value = 'unexpected' }
                @{ Field = 'created_at'; Value = 'invalid' }
                @{ Field = 'expires_at'; Value = 'invalid' }
            ) {
                param($Field, $Value); $null = @($Field, $Value);
                $script:receipt.$Field = $Value
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
            }

            It 'rejects null, extra fields, expired and overly long receipt lifetime' {
                Test-RootEpicReceipt -Receipt $null -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
                $script:receipt | Add-Member unexpected 1
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
                $script:receipt = Get-Issue995RootReceipt
                $script:receipt.expires_at = $script:now.AddMinutes(-1).ToString('o')
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
                $script:receipt.expires_at = $script:now.AddHours(2).ToString('o')
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
            }

            It 'validates epic-run reference and kickoff path' {
                $script:payload.agent_type = 'epic-orchestrator'
                $script:receipt.requested_persona = 'epic-orchestrator'
                $script:receipt.entry_kind = 'epic-run'
                $script:receipt.epic_reference = 'docs/features/epics/example/epic.md'
                $script:receipt.kickoff_path = 'docs/features/epics/example/epic-kickoff.md'
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeTrue
                $script:receipt.epic_reference = 'example'
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeTrue
                $script:receipt.kickoff_path = 'wrong'
                Test-RootEpicReceipt -Receipt $script:receipt -Payload $script:payload -RepositoryRoot $script:repository -CurrentHeadSha ('b' * 40) -Now $script:now | Should -BeFalse
            }

            It 'consumes a root receipt once with exclusive stream access and durable flush' {
                Mock Test-Path { param($LiteralPath, $PathType); $null = @($LiteralPath, $PathType); $true }
                $script:memory = [IO.MemoryStream]::new()
                $bytes = [Text.Encoding]::UTF8.GetBytes(($script:receipt | ConvertTo-Json))
                $script:memory.Write($bytes, 0, $bytes.Length)
                $script:memory.Position = 0
                $script:flushes = 0
                $arguments = @{ Payload = $script:payload; ReceiptPath = 'receipt'; Checkpoints = @(); RepositoryRoot = $script:repository; CurrentHeadSha = ('b' * 40); Now = $script:now; OpenStream = { param($Path, $Mode, $Access, $Share); $null = @($Path, $Mode, $Access, $Share); $Mode | Should -Be ([IO.FileMode]::Open); $Access | Should -Be ([IO.FileAccess]::ReadWrite); $Share | Should -Be ([IO.FileShare]::None); $script:memory }; FlushStream = { param($Stream); $null = @($Stream); $script:flushes++; $Stream | Should -Be $script:memory } }
                (Get-CodexSubagentAttestationFromAuthority @arguments).root_authorized | Should -BeTrue
                $script:memory.CanRead | Should -BeFalse
                $script:flushes | Should -Be 1
                $consumedBytes = $script:memory.ToArray()
                $consumed = [Text.Encoding]::UTF8.GetString($consumedBytes) | ConvertFrom-Json
                $consumed.consumed | Should -BeTrue
                $consumed.consumed_by | Should -BeExactly 'worker'
                $script:memory = [IO.MemoryStream]::new($consumedBytes)
                (Get-CodexSubagentAttestationFromAuthority @arguments).root_authorized | Should -BeFalse
                $script:flushes | Should -Be 1
                $script:memory.CanRead | Should -BeFalse
            }
        }

        Context 'Issue 995 attestation output and command wrapper' {
            It 'writes JSON through exclusive creation and disposes its stream' {
                $script:memory = [IO.MemoryStream]::new()
                $script:directories = 0
                $attestation = Invoke-Issue995Attestation
                $path = Write-CodexSubagentAttestation -RepositoryRoot $script:repository -Attestation $attestation -EnsureDirectory { param($Path); $null = @($Path); $script:directories++ } -OpenStream {
                    param($Path, $Mode, $Access, $Share); $null = @($Path, $Mode, $Access, $Share);
                    $Mode | Should -Be ([IO.FileMode]::CreateNew)
                    $Access | Should -Be ([IO.FileAccess]::Write)
                    $Share | Should -Be ([IO.FileShare]::None)
                    $script:memory
                }
                $path | Should -Match 'attestation.json$'
                $script:directories | Should -Be 1
                $script:memory.CanWrite | Should -BeFalse
                ([Text.Encoding]::UTF8.GetString($script:memory.ToArray()) | ConvertFrom-Json).routing_valid | Should -BeTrue
            }

        }

        Context 'Issue 995 command wrapper' {

            BeforeEach {
                Mock Write-CodexSubagentAttestation { param($RepositoryRoot, $Attestation, $OpenStream, $EnsureDirectory); $null = @($RepositoryRoot, $Attestation, $OpenStream, $EnsureDirectory); 'attestation.json' }
                $script:command = @{ RepositoryRoot = $script:repository; ReadInput = { $script:payload | ConvertTo-Json }; ReadCheckpoint = { param($Path); $null = @($Path); '{' }; ReadHead = { param($Root); $null = @($Root); @{ sha = ('b' * 40); exit_code = 0 } }; GetNow = { $script:now } }
            }

            It 'returns success without output for correctly routed input' {
                $script:command.ReadCheckpoint = { param($Path); $null = @($Path); $script:checkpoint | ConvertTo-Json -Depth 5 }
                Mock Test-Path { param($LiteralPath, $PathType); $null = @($LiteralPath, $PathType); $LiteralPath -like '*orchestrator-state.json' }
                $actual = Invoke-CodexSubagentAttestationCommand @script:command
                $actual.exit_code | Should -Be 0
                $actual.stdout | Should -BeNullOrEmpty
                $actual.stderr | Should -BeNullOrEmpty
            }

            It 'resolves the default repository root through mocked authority boundaries' {
                $script:command.Remove('RepositoryRoot')

                $actual = Invoke-CodexSubagentAttestationCommand @script:command

                $actual.exit_code | Should -Be 0
                $actual.stdout | Should -Match 'MODEL_ROUTING_ATTESTATION_BLOCKED'
                Should -Invoke Get-CodexAuthorityReceiptPath -Exactly -Times 1 -ParameterFilter {
                    -not [string]::IsNullOrWhiteSpace($RepositoryRoot)
                }
            }

            It 'returns the routing marker for malformed optional checkpoint data' {
                Mock Test-Path { param($LiteralPath, $PathType); $null = @($LiteralPath, $PathType); $true }
                $actual = Invoke-CodexSubagentAttestationCommand @script:command
                $actual.exit_code | Should -Be 0
                $actual.stdout | Should -Match 'MODEL_ROUTING_ATTESTATION_BLOCKED'
            }

            It 'returns the provenance marker when epic root authority is absent' {
                $script:payload.agent_type = 'epic-planner'
                $script:payload.model = 'gpt-6-astra'
                $actual = Invoke-CodexSubagentAttestationCommand @script:command
                $actual.exit_code | Should -Be 0
                $actual.stdout | Should -Match 'EPIC_INVOCATION_ORIGIN_BLOCKED'
            }

            It 'returns exit two for invalid HEAD or malformed startup input' {
                $script:command.ReadHead = { param($Root); $null = @($Root); @{ sha = 'invalid'; exit_code = 1 } }
                $actual = Invoke-CodexSubagentAttestationCommand @script:command
                $actual.exit_code | Should -Be 2
                $actual.stderr | Should -Match 'HEAD could not be resolved'
                $script:command.ReadInput = { '{' }
                (Invoke-CodexSubagentAttestationCommand @script:command).stderr | Should -Match 'malformed JSON'
            }
        }
    }
}
