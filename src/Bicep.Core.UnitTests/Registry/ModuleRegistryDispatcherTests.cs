// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Bicep.Core.Diagnostics;
using Bicep.Core.Registry;
using Bicep.Core.Syntax;
using Bicep.Core.UnitTests.Assertions;
using Bicep.Core.Workspaces;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace Bicep.Core.UnitTests.Registry
{
    [TestClass]
    public class ModuleRegistryDispatcherTests
    {
        private static readonly MockRepository Repository = new MockRepository(MockBehavior.Strict);

        [TestMethod]
        public void NoRegistries_AvailableSchemes_ShouldReturnEmpty()
        {
            var dispatcher = CreateDispatcher();
            dispatcher.AvailableSchemes.Should().BeEmpty();
        }

        [TestMethod]
        public void NoRegistries_ValidateModuleReference_ShouldReturnError()
        {
            var module = CreateModule("fakeScheme:fakeModule");
            var dispatcher = CreateDispatcher();
            dispatcher.ValidateModuleReference(module, out var failureBuilder).Should().BeFalse();
            failureBuilder!.Should().NotBeNull();

            using (new AssertionScope())
            {
                failureBuilder!.Should().HaveCodeAndSeverity("BCP189", DiagnosticLevel.Error);
                failureBuilder!.Should().HaveMessage("Module references are not supported in this context.");
            }
        }

        [TestMethod]
        public void NoRegistries_NonValidateMethods_ShouldThrow()
        {
            var module = CreateModule("fakeScheme:fakeModule");
            var dispatcher = CreateDispatcher();

            static void ExpectFailure(Action fail) => fail.Should().Throw<InvalidOperationException>().WithMessage($"The specified module is not valid. Call {nameof(IModuleRegistryDispatcher.ValidateModuleReference)}() first.");

            ExpectFailure(() => dispatcher.IsModuleAvailable(module, out _));
            ExpectFailure(() => dispatcher.TryGetLocalModuleEntryPointUri(new Uri("untitled://two"), module, out _));
            ExpectFailure(() => dispatcher.RestoreModules(new[] { module }));
        }

        [TestMethod]
        public void MockRegistries_AvailableSchemes_ShouldReturnedConfiguredSchemes()
        {
            var first = Repository.Create<IModuleRegistry>();
            first.Setup(m => m.Scheme).Returns("first");

            var second = Repository.Create<IModuleRegistry>();
            second.Setup(m => m.Scheme).Returns("second");

            var dispatcher = CreateDispatcher(first.Object, second.Object);
            dispatcher.AvailableSchemes.Should().BeEquivalentTo("first", "second");
        }


        private static IModuleRegistryDispatcher CreateDispatcher(params IModuleRegistry[] registries)
        {
            var provider = Repository.Create<IModuleRegistryProvider>();
            provider.Setup(m => m.Registries).Returns(registries.ToImmutableArray());

            return new ModuleRegistryDispatcher(provider.Object);
        }

        private static ModuleDeclarationSyntax CreateModule(string reference)
        {
            var file = SourceFileFactory.CreateBicepFile(new System.Uri("untitled://hello"), $"module foo '{reference}' = {{}}");
            return file.ProgramSyntax.Declarations.OfType<ModuleDeclarationSyntax>().Single();
        }
    }
}
