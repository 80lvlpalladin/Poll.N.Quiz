﻿
using Assembly = System.Reflection.Assembly;

namespace Poll.N.Quiz.Settings.ArchitectureTests;

public class LayerIsolationTests
{
    [Test]
    public async Task Queries_DoesNotReference_CommandsOrSynchronizer()
    {
        var queriesAssembly = Assembly.Load("Poll.N.Quiz.Settings.Queries");
        var referencedAssemblies = queriesAssembly.GetReferencedAssemblies();

        foreach (var assembly in referencedAssemblies)
        {
            await Assert.That(assembly.Name)
                .IsNotEqualTo("Poll.N.Quiz.Settings.Commands").Or
                .IsNotEqualTo("Poll.N.Quiz.Settings.Synchronizer");
        }
    }

    [Test]
    public async Task Commands_DoesNotReference_QueriesOrSynchronizer()
    {
        var commandsAssembly = Assembly.Load("Poll.N.Quiz.Settings.Commands");
        var referencedAssemblies = commandsAssembly.GetReferencedAssemblies();

        foreach (var assembly in referencedAssemblies)
        {
            await Assert.That(assembly.Name)
                .IsNotEqualTo("Poll.N.Quiz.Settings.Queries").Or
                .IsNotEqualTo("Poll.N.Quiz.Settings.Synchronizer");
        }
    }

    [Test]
    public async Task Synchronizer_DoesNotReference_QueriesOrCommands()
    {
        var synchronizerAssembly = Assembly.Load("Poll.N.Quiz.Settings.Synchronizer");
        var referencedAssemblies = synchronizerAssembly.GetReferencedAssemblies();

        foreach (var assembly in referencedAssemblies)
        {
            await Assert.That(assembly.Name)
                .IsNotEqualTo("Poll.N.Quiz.Settings.Queries").Or
                .IsNotEqualTo("Poll.N.Quiz.Settings.Commands");
        }
    }

    [Test]
    public async Task Api_References_QueriesCommandsAndSynchronizer()
    {
        var apiAssembly = Assembly.Load("Poll.N.Quiz.Settings.API");
        var referencedAssemblies =
            apiAssembly.GetReferencedAssemblies();

        List<string> searchedAssemblies =
        [
            "Poll.N.Quiz.Settings.Queries",
            "Poll.N.Quiz.Settings.Commands",
            "Poll.N.Quiz.Settings.Synchronizer"
        ];

        foreach (var assembly in referencedAssemblies)
        {
            if (assembly.Name is null)
                continue;

            if (searchedAssemblies.Contains(assembly.Name))
                searchedAssemblies.Remove(assembly.Name);
        }

        await Assert.That(searchedAssemblies).IsEmpty();
    }

}
