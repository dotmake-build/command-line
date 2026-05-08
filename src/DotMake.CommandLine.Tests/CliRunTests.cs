using Xunit.Abstractions;

namespace DotMake.CommandLine.Tests
{
    public class CliRunTests
    {
        private readonly ITestOutputHelper output;

        public CliRunTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData(2, 100)]
        [InlineData(3, 50)]
        [InlineData(3, 10)]
        [InlineData(10, 300)]
        [InlineData(20, 100)]
        public async Task RunAsync_Should_Handle_Parallel(int size, int strength)
        {
            var result = await Cli.RunAsync<MathCommand>(
                ["-s", size.ToString(), "-x", strength.ToString()]
            );

            Assert.Equal(result, size * strength);
        }

        [Theory]
        [InlineData(2, 100, 50)]
        [InlineData(3, 50, 100)]
        public async Task RunAsync_Should_Handle_Parallel2(
            int size,
            int strength,
            int concurrency)
        {
            var tasks = Enumerable.Range(0, concurrency)
                .Select(_ =>
                    Cli.RunAsync<MathCommand>(
                        ["-s", size.ToString(), "-x", strength.ToString()]
                    )
                );

            await Task.WhenAll(tasks);
        }

        [Fact]
        public async Task RunAsync_Should_Handle_Parallel3()
        {
            var gate = new TaskCompletionSource();

            var tasks = Enumerable.Range(0, 500)
                .Select(async _ =>
                {
                    var size = Random.Shared.Next(1, 20);
                    var strength = Random.Shared.Next(1, 500);

                    await gate.Task;

                    var result = await Cli.RunAsync<MathCommand>(
                        ["-s", size.ToString(), "-x", strength.ToString()]
                    );

                    output.WriteLine($"{size} x {strength} = {result}");
                })
                .ToArray();

            gate.SetResult();

            await Task.WhenAll(tasks);
        }

        [CliCommand]
        public class MathCommand : ICliRunAsyncWithContextAndReturn
        {
            [CliOption]
            public int Size { get; set; }

            [CliOption(
                Alias = "x"
            )]
            public int Strength { get; set; }

            public async Task<int> RunAsync(CliContext cliContext)
            {
                //await Task.Delay(2000);

                return Size * Strength;
            }
        }
    }
}
