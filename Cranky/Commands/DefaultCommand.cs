using Spectre.Console.Cli;

namespace Cranky.Commands;

internal sealed class DefaultCommand : AsyncCommand<DefaultCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var command = new AnalyzeCommand();

        return command.ExecuteAsync(context, new());
    }
}