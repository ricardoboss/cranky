using Cranky.Commands;
using Spectre.Console.Cli;

var app = new CommandApp<AnalyzeCommand>();

return await app.RunAsync(args);
