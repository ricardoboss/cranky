using Cranky.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<DefaultCommand>();
app.Configure(c =>
{
    c.AddCommand<AnalyzeCommand>("analyze");
});

return await app.RunAsync(args);
