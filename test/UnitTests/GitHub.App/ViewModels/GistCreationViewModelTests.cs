﻿using System;
using System.Linq;
using System.Reactive.Linq;
using GitHub.Api;
using GitHub.Factories;
using GitHub.Models;
using GitHub.SampleData;
using GitHub.Services;
using GitHub.ViewModels;
using NSubstitute;
using Octokit;
using ReactiveUI;
using UnitTests;
using Xunit;
using IConnection = GitHub.Models.IConnection;

public class GistCreationViewModelTests
{
    static IGistCreationViewModel CreateViewModel(IServiceProvider provider, string selectedText = "", string fileName = "", bool isPrivate = false)
    {
        var selectedTextProvider = Substitute.For<ISelectedTextProvider>();
        selectedTextProvider.GetSelectedText().Returns(selectedText);

        var accounts = new ReactiveList<IAccount>() { Substitute.For<IAccount>(), Substitute.For<IAccount>() };
        var modelService = Substitute.For<IModelService>();
        modelService.GetAccounts().Returns(Observable.Return(accounts));

        var modelServiceFactory = Substitute.For<IModelServiceFactory>();
        modelServiceFactory.CreateAsync(null).ReturnsForAnyArgs(modelService);
        modelServiceFactory.CreateBlocking(null).ReturnsForAnyArgs(modelService);

        var gistPublishService = provider.GetGistPublishService();
        var connection = Substitute.For<IConnection>();

        return new GistCreationViewModel(connection, modelServiceFactory, selectedTextProvider, gistPublishService, Substitute.For<IUsageTracker>())
        {
            FileName = fileName,
            IsPrivate = isPrivate
        };
    }

    public class TheCreateGistCommand : TestBaseClass
    {
        [Theory]
        [InlineData("Console.WriteLine", "Gist.cs", true)]
        [InlineData("Console.WriteLine", "Gist.cs", false)]
        public void CreatesAGistUsingTheApiClient(string selectedText, string fileName, bool isPrivate)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, selectedText, fileName, isPrivate);
            var gistPublishService = provider.GetGistPublishService();
            vm.CreateGist.Execute(null);

            gistPublishService
                .Received()
                .PublishGist(
                    Arg.Any<IApiClient>(),
                    Arg.Is<NewGist>(g => g.Public == !isPrivate
                        && g.Files.First().Key == fileName
                        && g.Files.First().Value == selectedText));
        }

        [Theory]
        [InlineData(null, false)]
        [InlineData("", false)]
        [InlineData("Gist.cs", true)]
        public void CannotCreateGistIfFileNameIsMissing(string fileName, bool expected)
        {
            var provider = Substitutes.ServiceProvider;
            var vm = CreateViewModel(provider, fileName: fileName);

            var actual = vm.CreateGist.CanExecute(null);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Foo()
        {
            var x = new PullRequestDetailViewModelDesigner();
        }
    }
}
