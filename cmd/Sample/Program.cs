using Spectre.Console.Cli;

namespace Sample;

public static class Program
{
    public static int Main(string[] args)
    {
        var app = new CommandApp();

        app.Configure(config =>
        {
            config.AddBranch<AddSettings>("add", add =>
            {
                add.AddCommand<AddPackageCommand>("package");
                add.AddCommand<AddReferenceCommand>("reference");
            });
        });

        return app.Run(args);
    }
}
