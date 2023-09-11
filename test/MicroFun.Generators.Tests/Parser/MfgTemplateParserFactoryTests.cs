using MicroFun.Generators.Ast;
using MicroFun.Generators.Parser;
using Pidgin;

namespace MicroFun.Generators.Tests.Parser;

public class MfgTemplateParserFactoryTests
{
    [Fact]
    public void ShouldParsePlainText()
    {
        // Given a template
        var template = @"Hello World!";

        // And a template parser
        var parser = new MfgTemplateParserFactory().CreateParser();

        // When the template is parsed
        var result = parser.Parse(template);

        // Then the result should be a success
        Assert.True(result.Success);

        // And the result should not be null
        Assert.NotNull(result.Value);

        // And the result should be the expected template AST
        if (result.Value is not { MainBlock: { Sections: [TextSectionAst { Text: "Hello World!" }] } })
        {
            Assert.Fail($"Unexpected parsed AST: {result.Value}");
        }
    }
}
