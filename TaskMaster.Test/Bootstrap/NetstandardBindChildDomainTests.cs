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
        /// The configuration file supplied to every child domain. It is the deployed
        /// configuration of the assembly whose output directory the child domains are rooted
        /// at; it carries no <c>netstandard</c> entry and is out of scope for this work to
        /// change. The add-in's deployed configuration image, which does gain a
        /// <c>netstandard</c> entry, is deliberately not named here: selecting it would void
        /// the negative control. The name is stated explicitly rather than derived from the
        /// host assembly's own name, so that the prohibition on naming the add-in's image
        /// cannot be defeated by a change of host.
        /// </summary>
        private const string HostConfigFileName = "QuickFiler.Test.dll.config";

        private const string DeedleFileName = "Deedle.dll";

        private const string FSharpCoreFileName = "FSharp.Core.dll";

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
        /// Proves the child domains are rooted where this harness intends, rather than at this
        /// test assembly's own output directory.
        /// </summary>
        /// <remarks>
        /// <para>
        /// It runs in the positive domain and deliberately does not install the fallback. An
        /// application base is fixed at domain creation and no later call can change it, so
        /// ordering relative to the installer is not merely unconstrained here but meaningless,
        /// and omitting the call keeps this domain's loaded set at its minimum for the
        /// observation.
        /// </para>
        /// <para>
        /// The two file checks are made in the parent domain rather than in the child, so no
        /// filesystem helper is added to the child domain's loaded set. Without this method a
        /// silent regression of the application base back to the host assembly's own directory
        /// would make every positive result in this class vacuous, which is the failure this
        /// harness has already suffered twice.
        /// </para>
        /// </remarks>
        [TestMethod]
        public void ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-application-base");
            string expectedBase = ProbeApplicationBase;

            // Act
            string observedBase = probe.ApplicationBaseDirectory();

            // Assert: paths are compared case-insensitively because Windows paths are.
            string.Equals(
                    TrimSeparator(observedBase),
                    TrimSeparator(expectedBase),
                    StringComparison.OrdinalIgnoreCase
                )
                .Should()
                .BeTrue(
                    "the child domain must be rooted at {0} but reported {1}",
                    expectedBase,
                    observedBase
                );
            File.Exists(Path.Combine(expectedBase, DeedleFileName))
                .Should()
                .BeTrue("the harness loads Deedle from the directory the domain is rooted at");
            File.Exists(Path.Combine(expectedBase, FSharpCoreFileName))
                .Should()
                .BeTrue(
                    "the flavour of FSharp.Core deployed beside Deedle is what determines "
                        + "whether the identity under test is requested at all"
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
        /// The end-to-end assertion: the Deedle member invocation at the deepest caller frame
        /// of the reported production trace completes once the fallback is installed.
        /// </summary>
        /// <remarks>
        /// The observation is a member invocation rather than a forced class-constructor run.
        /// Forcing the class constructor returned a success token against a build carrying no
        /// fix, so it could not distinguish a fixed build from a broken one; invoking the
        /// member closed over a record type reaches the binding failure the production trace
        /// reports.
        /// </remarks>
        [TestMethod]
        public void AfterInstall_DeedleTypeInitializerSucceeds()
        {
            // Arrange
            ChildDomainBindProbe probe = CreateChildDomainProbe("positive-deedle");

            // Act
            probe.InstallProductionFallback();
            string outcome = probe.DeedleRecordConversionOutcome(DeedlePath);

            // Record the observed value before asserting, so it is recoverable from the TRX
            // whether this test passes or fails.
            TestContext.WriteLine("DEEDLE_RECORD_CONVERSION_OUTCOME=" + outcome);

            // Assert: the class assertion first, then the completion assertion. FluentAssertions
            // reports the first failing assertion, so a bind failure names the bind class while
            // an unrelated exception passes the first and fails the second carrying its own type
            // name. Neither failure can be mistaken for the other.
            outcome
                .Should()
                .NotStartWith(
                    ChildDomainBindProbe.BindFailurePrefix,
                    "a netstandard identity must be bindable once the fallback is installed"
                );
            outcome
                .Should()
                .Be(
                    ChildDomainBindProbe.InvokedOutcome,
                    "the record conversion must complete without throwing"
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

        /// <summary>
        /// The directory every child domain is rooted at.
        /// </summary>
        /// <remarks>
        /// It is deliberately not this test assembly's own output directory. This directory is
        /// retained as the historically failing root: before issue 895 aligned every
        /// <c>FSharp.Core</c> HintPath on the netstandard2.0 flavour, it deployed the flavour
        /// whose own reference is <c>netstandard 2.1.0.0</c>, while this test assembly's own
        /// output directory did not, so a probe rooted here could observe the bind failure and a
        /// probe rooted there could not. After that alignment every deployed copy references
        /// <c>netstandard 2.0.0.0</c>, so the Deedle invocation no longer requests the
        /// <c>2.1.0.0</c> identity from any directory, and the discriminating power of this class
        /// rests with the display-name tests, which bind that identity directly. The host base
        /// directory is this assembly's <c>bin\Debug</c>, so three parent steps reach the
        /// repository root.
        /// </remarks>
        private static string ProbeApplicationBase =>
            Path.GetFullPath(
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "..",
                    "..",
                    "..",
                    "QuickFiler.Test",
                    "bin",
                    "Debug"
                )
            );

        private static string HostConfigPath =>
            Path.Combine(ProbeApplicationBase, HostConfigFileName);

        private static string DeedlePath => Path.Combine(ProbeApplicationBase, DeedleFileName);

        /// <summary>
        /// Trims any trailing directory separator, so a path that reports one and a path that
        /// does not can be compared.
        /// </summary>
        private static string TrimSeparator(string path)
        {
            return path == null
                ? null
                : path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        /// <summary>
        /// Verifies that the directory the child domains are rooted at exists and deploys every
        /// file this harness depends on.
        /// </summary>
        /// <remarks>
        /// It throws rather than skipping. A skipped precondition here would leave every result
        /// in this class vacuous while still reporting as a pass.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// The directory, or one of the three required files in it, is missing.
        /// </exception>
        private static void EnsureProbeApplicationBaseIsComplete()
        {
            string applicationBase = ProbeApplicationBase;
            if (!Directory.Exists(applicationBase))
            {
                throw new InvalidOperationException(
                    "The directory the child domains must be rooted at was not found: "
                        + applicationBase
                );
            }

            string[] required = new[] { HostConfigFileName, DeedleFileName, FSharpCoreFileName };
            foreach (string fileName in required)
            {
                string candidate = Path.Combine(applicationBase, fileName);
                if (!File.Exists(candidate))
                {
                    throw new InvalidOperationException(
                        "A file this harness depends on was not found: " + candidate
                    );
                }
            }
        }

        /// <summary>
        /// Creates a child domain rooted at the directory named by
        /// <see cref="ProbeApplicationBase"/> and unwraps the cross-domain probe inside it.
        /// </summary>
        /// <remarks>
        /// The probe is created from its assembly's file location rather than from its display
        /// name, because this test assembly is not deployed to the directory the domain is
        /// rooted at and a display-name creation would therefore fail before any observation
        /// could be taken.
        /// </remarks>
        private ChildDomainBindProbe CreateChildDomainProbe(string friendlyName)
        {
            EnsureProbeApplicationBaseIsComplete();

            var setup = new AppDomainSetup
            {
                ApplicationBase = ProbeApplicationBase,
                ConfigurationFile = HostConfigPath,
            };

            AppDomain domain = AppDomain.CreateDomain(friendlyName, null, setup);
            _createdDomains.Add(domain);

            return (ChildDomainBindProbe)
                domain.CreateInstanceFromAndUnwrap(
                    typeof(ChildDomainBindProbe).Assembly.Location,
                    typeof(ChildDomainBindProbe).FullName
                );
        }
    }
}
