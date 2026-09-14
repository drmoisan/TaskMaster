using System;
using System.IO;
using System.Reflection;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using UtilitiesCS.Bootstrap;

namespace UtilitiesCS.Test.Bootstrap
{
    /// <summary>
    /// Unit tests for the host-neutral resolution ladder in
    /// <see cref="AssemblyBindingFallback"/>. Ten of these tests drive every ladder rung
    /// through the ladder's injected delegates, so none of those ten touches the assembly
    /// cache, the file system or a real bind. One further test drives the subscribed handler
    /// through the real CLR binder, and is the only test here that does so: it exists to cover
    /// the production entry point and the production ladder factory, neither of which any
    /// delegate-driven test can reach, because both run only when a genuine failed bind raises
    /// the domain's assembly-resolution event.
    /// </summary>
    [TestClass]
    public class AssemblyBindingFallbackTests
    {
        private const string NetstandardToken = "cc7b13ffcd2ddd51";
        private const string MscorlibToken = "b77a5c561934e089";
        private const string FakeRuntimeDirectory = @"X:\fake-runtime\";

        /// <summary>A real assembly object used as a resolution sentinel. Loading none.</summary>
        private static Assembly Sentinel => typeof(Uri).Assembly;

        /// <summary>A strongly named assembly already present in every test host.</summary>
        private static Assembly LoadedMscorlib => typeof(string).Assembly;

        /// <summary>
        /// Restores the two mutable statics the installer tests touch, so tests remain
        /// order-independent.
        /// </summary>
        [TestCleanup]
        public void ResetInstallerSeams()
        {
            DetachProductionHandler();
            InstalledField.SetValue(null, 0);
            ResolvingField.SetValue(null, null);
        }

        /// <summary>
        /// Rung 1 wins over rung 2: when a loaded assembly matches on simple name and token,
        /// the ladder returns it and never asks the load-by-display-name delegate.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenAlreadyLoadedMatches_ReturnsItWithoutCallingLoadByDisplayName()
        {
            // Arrange: a loaded assembly whose token matches but whose version does not.
            var loadByDisplayName = new Mock<Func<string, Assembly>>();
            loadByDisplayName.Setup(f => f(It.IsAny<string>())).Returns(Sentinel);
            var ladder = CreateLadder(
                getLoadedAssemblies: () => new[] { LoadedMscorlib },
                loadByDisplayName: loadByDisplayName.Object
            );

            // Act
            Assembly resolved = ladder.Resolve(NameOf("mscorlib", "99.0.0.0", MscorlibToken));

            // Assert: version is deliberately not compared, so the loaded assembly wins.
            resolved.Should().BeSameAs(LoadedMscorlib);
            loadByDisplayName.Verify(f => f(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Rung 2 asks for the full display name, and pins the netstandard identity to the
        /// only version that exists for .NET Framework.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenNothingLoaded_AsksFullDisplayNameAtFacadeVersion()
        {
            // Arrange
            string observed = null;
            var loadByDisplayName = new Mock<Func<string, Assembly>>();
            loadByDisplayName
                .Setup(f => f(It.IsAny<string>()))
                .Callback<string>(n => observed = n)
                .Returns(Sentinel);
            var ladder = CreateLadder(loadByDisplayName: loadByDisplayName.Object);

            // Act
            Assembly resolved = ladder.Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken));

            // Assert
            resolved.Should().BeSameAs(Sentinel);
            observed.Should().NotBeNull();
            observed.Should().Contain("netstandard");
            observed.Should().Contain("Version=2.0.0.0");
            observed.Should().Contain("PublicKeyToken=" + NetstandardToken);
        }

        /// <summary>
        /// Rung 3 loads the facade from the runtime directory by absolute path, which is the
        /// rung that does not depend on GAC lookup behaviour.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenDisplayNameRungMisses_LoadsFacadeFromRuntimeDirectory()
        {
            // Arrange
            string observedPath = null;
            var ladder = CreateLadder(
                loadByDisplayName: _ => throw new FileNotFoundException("no such assembly"),
                loadFromPath: p =>
                {
                    observedPath = p;
                    return Sentinel;
                },
                fileExists: _ => true
            );

            // Act
            Assembly resolved = ladder.Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken));

            // Assert
            resolved.Should().BeSameAs(Sentinel);
            observedPath.Should().NotBeNull();
            observedPath.Should().StartWith(FakeRuntimeDirectory);
            observedPath.Should().EndWith("netstandard.dll");
        }

        /// <summary>
        /// Rung 1 compares the public key token, so a same-named assembly signed with a
        /// different key is not returned.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenLoadedTokenDiffers_DoesNotReturnTheLoadedAssembly()
        {
            // Arrange: same simple name, different token.
            var ladder = CreateLadder(getLoadedAssemblies: () => new[] { LoadedMscorlib });

            // Act
            Assembly resolved = ladder.Resolve(NameOf("mscorlib", "4.0.0.0", NetstandardToken));

            // Assert: rung 1 rejected it and no later rung supplied anything.
            resolved.Should().BeNull();
        }

        /// <summary>
        /// A requested identity carrying no public key token does not match a strongly named
        /// loaded assembly.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenRequestedTokenIsNull_DoesNotMatchStronglyNamedLoadedAssembly()
        {
            // Arrange
            var ladder = CreateLadder(getLoadedAssemblies: () => new[] { LoadedMscorlib });
            var requested = new AssemblyName("mscorlib");

            // Act
            Assembly resolved = ladder.Resolve(requested);

            // Assert
            requested.GetPublicKeyToken().Should().BeNull();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// A requested identity carrying a zero-length public key token does not match a
        /// strongly named loaded assembly either.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenRequestedTokenIsEmpty_DoesNotMatchStronglyNamedLoadedAssembly()
        {
            // Arrange
            var requested = new AssemblyName("mscorlib");
            requested.SetPublicKeyToken(new byte[0]);
            var ladder = CreateLadder(getLoadedAssemblies: () => new[] { LoadedMscorlib });

            // Act
            Assembly resolved = ladder.Resolve(requested);

            // Assert
            requested.GetPublicKeyToken().Should().BeEmpty();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// The thread-static guard stops the handler recursing into itself for the same
        /// simple name.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenAlreadyResolvingTheSameSimpleName_ReturnsNull()
        {
            // Arrange: simulate an outer frame already resolving this simple name.
            ResolvingField.SetValue(null, "netstandard");

            // Act
            Assembly resolved = AssemblyBindingFallback.Resolve(
                NameOf("netstandard", "2.1.0.0", NetstandardToken)
            );

            // Assert: short-circuited before any rung, and the outer frame's guard survives.
            resolved.Should().BeNull();
            ResolvingField.GetValue(null).Should().Be("netstandard");
        }

        /// <summary>
        /// Installing twice attaches exactly one handler, because the counter is exchanged
        /// atomically.
        /// </summary>
        [TestMethod]
        public void Install_CalledTwice_AttachesExactlyOneHandler()
        {
            // Arrange
            InstalledField.SetValue(null, 0);
            int before = CountAssemblyResolveHandlers();

            // Act
            AssemblyBindingFallback.Install();
            AssemblyBindingFallback.Install();

            // Assert
            CountAssemblyResolveHandlers().Should().Be(before + 1);
        }

        /// <summary>
        /// A rung that throws internally is absorbed and the ladder continues to the next
        /// rung rather than propagating to the CLR binder.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenARungThrows_AbsorbsAndContinuesToTheNextRung()
        {
            // Arrange: rung 1 throws; rung 2 supplies the answer.
            var ladder = CreateLadder(
                getLoadedAssemblies: () => throw new InvalidOperationException("rung 1 broke"),
                loadByDisplayName: _ => Sentinel
            );

            // Act
            Action act = () => ladder.Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken));

            // Assert
            act.Should().NotThrow();
            ladder
                .Resolve(NameOf("netstandard", "2.1.0.0", NetstandardToken))
                .Should()
                .BeSameAs(Sentinel);
        }

        /// <summary>
        /// An identity no rung can supply returns null rather than throwing, which is the
        /// contract the CLR binder expects from a resolve handler.
        /// </summary>
        [TestMethod]
        public void Resolve_WhenNoRungApplies_ReturnsNullWithoutThrowing()
        {
            // Arrange: every source misses.
            var ladder = CreateLadder(
                loadByDisplayName: _ => throw new FileNotFoundException("no such assembly"),
                loadFromPath: _ => throw new FileNotFoundException("no such file"),
                fileExists: _ => false
            );

            // Act
            Assembly resolved = null;
            Action act = () =>
                resolved = ladder.Resolve(NameOf("NoSuchAssembly", "1.0.0.0", MscorlibToken));

            // Assert
            act.Should().NotThrow();
            resolved.Should().BeNull();
        }

        /// <summary>
        /// The subscribed production handler declines an identity no rung can supply, and
        /// declines by returning null rather than by letting an exception reach the CLR
        /// binder. This is the only test in this class that drives a real bind, and it is the
        /// only one that executes
        /// <c>AssemblyBindingFallback.OnAssemblyResolve</c> and
        /// <c>AssemblyBindingFallback.CreateProductionLadder</c>, because both are reachable
        /// only through a failed bind raising the domain's assembly-resolution event. The CLR
        /// consumes the handler's return value, so the observable outcome is the load failing
        /// rather than a value this test can inspect.
        /// </summary>
        [TestMethod]
        public void Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing()
        {
            // Arrange: a fixed display name matching no assembly in the repository, in the
            // assembly cache or beside the test assembly, so the outcome is deterministic.
            InstalledField.SetValue(null, 0);
            AssemblyBindingFallback.Install();
            const string Unresolvable =
                "UtilitiesCS.Test.NoSuchAssembly.7f3a2b1c, Version=1.0.0.0, "
                + "Culture=neutral, PublicKeyToken=b77a5c561934e089";

            // Act
            Action act = () => Assembly.Load(Unresolvable);

            // Assert: the binder reports the miss. Had the handler propagated an exception
            // instead of returning null, the thrown type would not be this one.
            act.Should()
                .Throw<FileNotFoundException>(
                    "the handler declines an unresolvable identity by returning null"
                );
        }

        /// <summary>
        /// Builds a ladder whose five sources default to "miss", overriding only what a test
        /// needs.
        /// </summary>
        private static AssemblyBindingFallback.AssemblyBindingLadder CreateLadder(
            Func<Assembly[]> getLoadedAssemblies = null,
            Func<string, Assembly> loadByDisplayName = null,
            Func<string, Assembly> loadFromPath = null,
            Func<string, bool> fileExists = null,
            Func<string> getRuntimeDirectory = null
        )
        {
            return new AssemblyBindingFallback.AssemblyBindingLadder(
                getLoadedAssemblies ?? (() => new Assembly[0]),
                loadByDisplayName ?? (_ => null),
                loadFromPath ?? (_ => null),
                fileExists ?? (_ => false),
                getRuntimeDirectory ?? (() => FakeRuntimeDirectory)
            );
        }

        /// <summary>Builds a fully specified assembly identity.</summary>
        private static AssemblyName NameOf(string simpleName, string version, string token)
        {
            return new AssemblyName(
                simpleName + ", Version=" + version + ", Culture=neutral, PublicKeyToken=" + token
            );
        }

        private static FieldInfo InstalledField =>
            typeof(AssemblyBindingFallback).GetField(
                "_installed",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        private static FieldInfo ResolvingField =>
            typeof(AssemblyBindingFallback).GetField(
                "_resolvingSimpleName",
                BindingFlags.NonPublic | BindingFlags.Static
            );

        /// <summary>
        /// Reads the length of the current domain's assembly-resolution invocation list
        /// through the private field, which is the only accessor net481 offers.
        /// </summary>
        private static int CountAssemblyResolveHandlers()
        {
            FieldInfo field = typeof(AppDomain).GetField(
                "_AssemblyResolve",
                BindingFlags.Instance | BindingFlags.NonPublic
            );
            if (field == null)
            {
                throw new InvalidOperationException(
                    "AppDomain field _AssemblyResolve was not found."
                );
            }

            var handler = field.GetValue(AppDomain.CurrentDomain) as Delegate;
            return handler == null ? 0 : handler.GetInvocationList().Length;
        }

        /// <summary>
        /// Detaches the production handler if a test attached it, so the test host is left
        /// as it was found.
        /// </summary>
        private static void DetachProductionHandler()
        {
            MethodInfo method = typeof(AssemblyBindingFallback).GetMethod(
                "OnAssemblyResolve",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            if (method == null)
            {
                return;
            }

            var handler = (ResolveEventHandler)
                Delegate.CreateDelegate(typeof(ResolveEventHandler), method);
            AppDomain.CurrentDomain.AssemblyResolve -= handler;
        }
    }
}
