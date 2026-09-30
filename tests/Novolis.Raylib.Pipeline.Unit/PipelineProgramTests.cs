using Novolis.Raylib.CodeGen;
using Novolis.Raylib.Pipeline;
using Novolis.Raylib.Pipeline.Steps;

namespace Novolis.Raylib.Pipeline.Unit;

public sealed class PipelineProgramTests
{
    [Test]
    public async Task Main_list_returns_zero()
    {
        var code = await Program.Main(["list"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_help_flag_returns_zero()
    {
        var code = await Program.Main(["-h"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_no_args_returns_one()
    {
        var code = await Program.Main([]);
        await Assert.That(code).IsEqualTo(1);
    }

    [Test]
    public async Task Main_explain_known_step_returns_zero()
    {
        var code = await Program.Main(["explain", "step_06_codegen"]);
        await Assert.That(code).IsEqualTo(0);
    }

    [Test]
    public async Task Main_explain_unknown_step_returns_one()
    {
        var code = await Program.Main(["explain", "step_not_real"]);
        await Assert.That(code).IsEqualTo(1);
    }

    [Test]
    public async Task Main_unknown_command_returns_one()
    {
        var code = await Program.Main(["not-a-command"]);
        await Assert.That(code).IsEqualTo(1);
    }
}
