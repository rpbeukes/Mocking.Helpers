﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Completion;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Mocking.Helpers.FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mocking.Helpers.FakeItEasy
{
    [ExportCompletionProvider(nameof(FakeItEasyIgnoredCompletion), LanguageNames.CSharp)]
    public class FakeItEasyIgnoredCompletion : CompletionProvider
    {
        private FakeItEasyProvider _provider;

        public FakeItEasyIgnoredCompletion()
        {
            this._provider = new FakeItEasyProvider();
            IsFakeItEasyCallToMethod = (InvocationExpressionSyntax invocation) => this._provider.MockingMethodNames.Any(methodName => SyntaxHelpers.IsMethodNamed(invocation, methodName));
        }

        internal readonly Func<InvocationExpressionSyntax, bool> IsFakeItEasyCallToMethod;


        public override async Task ProvideCompletionsAsync(CompletionContext context)
        {
            try
            {
                if (!context.Document.SupportsSemanticModel || !context.Document.SupportsSyntaxTree) return;

                var hasFakeItEasyReferenced = PackagesHelper.GetProjectNugetPackages(context.Document.Project).Any(x => x.Equals(this._provider.AssemblyName, StringComparison.InvariantCultureIgnoreCase));
                if (!hasFakeItEasyReferenced) return;

                var syntaxRoot = await context.Document.GetSyntaxRootAsync();
                var token = SyntaxHelpers.GetSelectedTokens(syntaxRoot, context.Position);

                // Not in an opened method
                if (token.Parent == null) return;

                var mockedMethodArgumentList = token.Parent as ArgumentListSyntax;
                var mockedMethodInvocation = mockedMethodArgumentList.Ancestors()
                                                                     .OfType<InvocationExpressionSyntax>()
                                                                     .Where(IsFakeItEasyCallToMethod)
                                                                     .FirstOrDefault();

                if (mockedMethodInvocation == null) return;

                var semanticModel = await context.Document.GetSemanticModelAsync();
                var matchingMockedMethods = SyntaxHelpers.GetCandidatesMockedMethodSignaturesForLambda(semanticModel, mockedMethodInvocation);

                var completionService = new CompletionService(context, token, semanticModel, this._provider);

                foreach (IMethodSymbol matchingMockedMethodSymbol in matchingMockedMethods)
                {
                    completionService.AddSuggestionsForMethod(matchingMockedMethodSymbol, mockedMethodArgumentList);
                }
            }
            catch
            {
            }
        }
    }
}
