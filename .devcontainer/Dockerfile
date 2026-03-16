# syntax=docker/dockerfile:1
# Mirrors the environment provisioned by .codex/codex-web-setup.sh.
FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04

# ── Additional system packages ─────────────────────────────────────────────────
# The devcontainers base image already ships ca-certificates, curl, git, and
# lsb-release; add only what it lacks.
RUN apt-get update && apt-get install -y --no-install-recommends \
        jq \
        mono-complete \
        ripgrep \
        unzip \
        zip \
    && rm -rf /var/lib/apt/lists/*

# ── PowerShell ─────────────────────────────────────────────────────────────────
# Mirrors install_powershell() — registers the Microsoft apt repository for the
# current Ubuntu release, then installs the powershell package.
RUN UBUNTU_VERSION="$(lsb_release -rs)" \
    && curl -fsSL \
         "https://packages.microsoft.com/config/ubuntu/${UBUNTU_VERSION}/packages-microsoft-prod.deb" \
         -o /tmp/ms-pkg.deb \
    && dpkg -i /tmp/ms-pkg.deb \
    && rm /tmp/ms-pkg.deb \
    && apt-get update \
    && apt-get install -y --no-install-recommends powershell \
    && rm -rf /var/lib/apt/lists/*

# ── .NET SDK 8.0 (system-wide) ─────────────────────────────────────────────────
# Mirrors install_dotnet_sdk() — uses the official install script, pins channel
# 8.0, and installs into a system path so all users share one SDK copy.
ENV DOTNET_ROOT=/usr/share/dotnet
RUN curl -fsSL https://dot.net/v1/dotnet-install.sh \
    | bash /dev/stdin --channel 8.0 --install-dir "${DOTNET_ROOT}"

# ── NuGet CLI (mono wrapper) ───────────────────────────────────────────────────
# Mirrors install_nuget() — downloads nuget.exe and wraps it in a bash shim so
# it is invocable as `nuget` on Linux via Mono.
RUN curl -fsSL "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" \
         -o /usr/local/lib/nuget.exe \
    && printf '#!/usr/bin/env bash\nexec mono /usr/local/lib/nuget.exe "$@"\n' \
         > /usr/local/bin/nuget \
    && chmod +x /usr/local/bin/nuget

# ── Persistent environment ─────────────────────────────────────────────────────
# Mirrors the env block at the top of main() plus install_dotnet_sdk PATH wiring.
ENV CI=true \
    DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1 \
    NUGET_XMLDOC_MODE=skip

# Expose dotnet, system-level dotnet tools, and the vscode user's global tools.
ENV PATH="${DOTNET_ROOT}:${DOTNET_ROOT}/tools:/home/vscode/.dotnet/tools:${PATH}"

# ── dotnet global tools + git config (as the dev user) ─────────────────────────
# Mirrors install_dotnet_tools() and the git autocrlf line at the end of main().
# Running as vscode ensures global tools land in /home/vscode/.dotnet/tools.
USER vscode
RUN dotnet tool install --global dotnet-coverage \
    && dotnet tool install --global csharpier \
    && git config --global core.autocrlf input

USER root
