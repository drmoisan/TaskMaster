using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TaskMaster.Test.Bootstrap
{
    /// <summary>
    /// Child-<see cref="AppDomain"/> acceptance harness for the production binding fallback,
    /// with a negative control.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every observation is made inside a domain created by
    /// <see cref="AppDomain.CreateDomain(string, System.Security.Policy.Evidence, AppDomainSetup)"/>,
    /// because a parent domain's resolve handler does not propagate into a child domain.
    /// That is what makes the result attributable to the installer under test rather than to
    /// a handler an unrelated component happened to install first.
    /// </para>
    /// <para>
    /// The class is pinned with <c>DoNotParallelize</c>. The runsettings in force set
    /// class-level parallelisation, so sibling classes in this assembly otherwise run
    /// concurrently in the host process, and <see cref="AppDomain.Unload"/> in cleanup is the
    /// one harness operation that concurrency reaches: it suspends the runtime and aborts the
    /// threads executing in the target domain. Every observation the probe makes is
    /// per-domain state read through the child domain's own instance and needs no pinning.
    /// </para>
    /// </remarks>
    [DoNotParallelize]
    [TestClass]
    public class NetstandardBindChildDomainTests
    {
        private const string Netstandard21DisplayName =
            "netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

        private const string Netstandard20DisplayName =
            "netstandard, Version=2.0.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51";

        /// <summary>
        /// The configuration file supplied to every child domain. This is the test
        /// assembly's own deployed configuration, which carries no <c>netstandard</c> entry
        /// and is out of scope for this work to change. The add-in's deployed configuration
        /// image, which does gain a <c>netstandard</c> entry, is deliberately not named here:
        /// selecting it would void the negative control.
        /// </summary>
        private const string HostConfigFileName = "TaskMaster.Test.dll.config";

        private const string DeedleFileName = "Deedle.dll";

        private readonly List<AppDomain> _createdDomains = new List<AppDomain>();

        /// <summary>Supplied by MSTest; used to record the open `2.0.0.0` observation.</summary>
        public TestContext TestContext { get; set; }

        /// <summary>Unloads every domain this test created.</summary>
        [TestCleanup]
        public void UnloadChildDomains()
        {
            foreach (AppDomain domain in _createdDomains)
            {
                AppDomain.Unload(domain);
            }

            _createdDomains.Clear();
        }

        /// <summary>
        /// Proves no SVG rendering occurred in the positive domain: the type initializer that
        /// installs the unrelated fallback cannot have run if its assembly is not loaded.
        /// The observation is taken after the installer call, which is the state the bind
        /// assertions measure in.
        /// </summary>
        [TestMethod]
        public void ChildDomain_HasNoSvgControlAssemblyLoaded()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-no-svg");

            // Act
            probe.InstallProductionFallback();
            int svgControlCount = probe.CountLoadedAssembliesNamed("SVGControl");
            int utilitiesCount = probe.CountLoadedAssembliesNamed("UtilitiesCS");

            // Assert
            svgControlCount
                .Should()
                .Be(
                    0,
                    "SvgRenderer's type initializer cannot have run if SVGControl is not loaded"
                );
            utilitiesCount
                .Should()
                .BeGreaterThan(
                    0,
                    "installing the fallback cannot be JIT-compiled without loading UtilitiesCS, "
                        + "so a non-zero count here is the positive control on the counting mechanism"
                );
        }

        /// <summary>
        /// Proves the positive domain starts with an empty assembly-resolution invocation
        /// list, and that the counting mechanism can report a non-zero value, so the first
        /// reading is an observation rather than a constant.
        /// </summary>
        [TestMethod]
        public void ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-handler-count");

            // Act
            int before = probe.CountAssemblyResolveHandlers();
            probe.InstallProductionFallback();
            int after = probe.CountAssemblyResolveHandlers();

            // Assert
            before.Should().Be(0, "a fresh child domain inherits no handler from its parent");
            after
                .Should()
                .BeGreaterThan(
                    0,
                    "without this second reading a field that is null in every state would "
                        + "report an empty list unconditionally and the first reading would "
                        + "prove nothing"
                );
        }

        /// <summary>
        /// Proves the configuration file supplied to the child domains declares no
        /// <c>netstandard</c> redirect, so a positive bind result is attributable to the
        /// installer rather than to the declarative hardening. Position relative to the
        /// installer call is unconstrained: the installer neither reads nor writes this file.
        /// </summary>
        [TestMethod]
        public void ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-config-check");

            // Act
            int entries = probe.ConfigurationFileNetstandardEntryCount(HostConfigPath);

            // Assert
            entries
                .Should()
                .Be(
                    0,
                    "the harness must measure the resolver, not a binding redirect it did not install"
                );
        }

        /// <summary>
        /// The two-version bind assertion. The reported production trace falls back to
        /// <c>2.0.0.0</c> and fails there too, so a remedy that resolved only <c>2.1.0.0</c>
        /// would not be sufficient.
        /// </summary>
        [TestMethod]
        public void AfterInstall_BothNetstandardVersionsBind()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-both-versions");

            // Act
            probe.InstallProductionFallback();
            string outcome21 = probe.TryLoadDisplayName(Netstandard21DisplayName);
            string outcome20 = probe.TryLoadDisplayName(Netstandard20DisplayName);

            // Assert
            outcome21
                .Should()
                .Be(ChildDomainBindProbe.LoadedOutcome, "the 2.1.0.0 identity must bind");
            outcome20
                .Should()
                .Be(ChildDomainBindProbe.LoadedOutcome, "the 2.0.0.0 identity must bind too");
        }

        /// <summary>
        /// The end-to-end assertion: the Deedle type initializer reported in the production
        /// failure runs without throwing once the fallback is installed.
        /// </summary>
        [TestMethod]
        public void AfterInstall_DeedleTypeInitializerSucceeds()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-deedle");

            // Act
            probe.InstallProductionFallback();
            string outcome = probe.DeedleTypeInitializerOutcome(DeedlePath);

            // Assert
            outcome
                .Should()
                .Be(
                    ChildDomainBindProbe.OkOutcome,
                    "the reported production failure is a TypeInitializationException here"
                );
        }

        /// <summary>
        /// The load-bearing criterion. Without the installer, the <c>2.1.0.0</c> identity is
        /// unsatisfiable, which is what distinguishes a fixed build from an unfixed one.
        /// </summary>
        [TestMethod]
        // ISOLATION-LOST-INVARIANT
        // If the load in this negative-control domain ever succeeds without a code change,
        // that is, if this test fails because no FileNotFoundException was raised, then
        // isolation has been lost, every positive assertion in this class is vacuous, and no
        // positive result from this harness may be trusted. Investigate the harness before
        // investigating the bind.
        public void NegativeControl_WithoutInstall_Netstandard21Throws()
        {
            // Arrange: a second child domain in which the installer is never run.
            ChildDomainBindProbe probe = CreateChildDomainProbe("negative-2-1-0-0");

            // Act
            string outcome = probe.TryLoadDisplayName(Netstandard21DisplayName);

            // Assert
            Netstandard21DisplayName
                .Should()
                .StartWith("netstandard", "the identity under test must be the facade identity");
            outcome
                .Should()
                .Be(
                    nameof(FileNotFoundException),
                    "an unfixed build cannot satisfy this identity; see the invariant comment above"
                );
        }

        /// <summary>
        /// Records, rather than gates, the outcome of a <c>2.0.0.0</c> load in the
        /// installer-free domain. Why the <c>2.0.0.0</c> leg also failed in the reported
        /// production trace is unexplained, so asserting a particular value here would be
        /// asserting something this work does not know.
        /// </summary>
        [TestMethod]
        public void NegativeControl_Netstandard20Observation_IsRecorded()
        {
            // Arrange: the same installer-free role as the negative control above.
            ChildDomainBindProbe probe = CreateChildDomainProbe("negative-2-0-0-0");

            // Act
            string outcome = probe.TryLoadDisplayName(Netstandard20DisplayName);
            TestContext.WriteLine("NETSTANDARD_2_0_0_0_NEGATIVE_DOMAIN_RESULT=" + outcome);

            // Assert
            outcome
                .Should()
                .NotBeNullOrEmpty(
                    "the probe must report some outcome for the open 2.0.0.0 question"
                );
        }

        /// <summary>
        /// Checkable form of the design claim that keeping the installer call in its own
        /// probe method prevents <c>UtilitiesCS</c> from being JIT-resolved in the
        /// installer-free domain. The bind attempt is performed first so the count is taken
        /// after the domain has done real work, not on an untouched domain.
        /// </summary>
        [TestMethod]
        public void NegativeControl_HasNoUtilitiesCsAssemblyLoaded()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("negative-no-utilities");

            // Act
            probe.TryLoadDisplayName(Netstandard21DisplayName);
            int utilitiesCount = probe.CountLoadedAssembliesNamed("UtilitiesCS");

            // Assert
            utilitiesCount
                .Should()
                .Be(
                    0,
                    "the same helper returns a non-zero count in the positive domain, so this "
                        + "zero is an observation rather than a constant"
                );
        }

        private static string HostConfigPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, HostConfigFileName);

        private static string DeedlePath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DeedleFileName);

        /// <summary>
        /// Creates a child domain rooted at this test assembly's output directory and
        /// unwraps the cross-domain probe inside it.
        /// </summary>
        private ChildDomainBindProbe CreateChildDomainProbe(string friendlyName)
        {
            var setup = new AppDomainSetup
            {
                ApplicationBase = AppDomain.CurrentDomain.BaseDirectory,
                ConfigurationFile = HostConfigPath,
            };

            AppDomain domain = AppDomain.CreateDomain(friendlyName, null, setup);
            _createdDomains.Add(domain);

            return (ChildDomainBindProbe)
                domain.CreateInstanceAndUnwrap(
                    typeof(ChildDomainBindProbe).Assembly.FullName,
                    typeof(ChildDomainBindProbe).FullName
                );
        }
    }
}
