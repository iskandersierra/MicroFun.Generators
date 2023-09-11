using MicroFun.Generators.Ast;
using MicroFun.Generators.DataModel;
using MicroFun.Generators.Templates;

namespace MicroFun.Generators.Interpreter;

public class InterpretedTemplate : ITemplate
{
    private readonly TemplateAst ast;

    public InterpretedTemplate(TemplateAst ast)
    {
        this.ast = ast ?? throw new ArgumentNullException(nameof(ast));
    }

    public string Generate(IDataModel rootModel, CancellationToken cancel = default)
    {
        using var writer = new StringWriter();

        var context = new GenerateContext(writer, rootModel, cancel);

        Generate(ast, context);

        return writer.ToString();
    }

    private void Generate(TemplateAst ast, GenerateContext context)
    {
        Generate(ast.MainBlock, context);
    }

    private void Generate(SectionBlockAst ast, GenerateContext context)
    {
        foreach (var section in ast.Sections)
        {
            Generate(section, context);
        }
    }

    private void Generate(SectionAst ast, GenerateContext context)
    {
        switch (ast)
        {
            case TextSectionAst textSection:
                Generate(textSection, context);
                break;
            default:
                throw new NotSupportedException($"Unsupported section type: {ast.GetType().Name}");
        }
    }

    private void Generate(TextSectionAst ast, GenerateContext context)
    {
        context.Writer.Write(ast.Text);
    }

    private record GenerateContext(
        TextWriter Writer,
        IDataModel CurrentModel,
        CancellationToken Cancel,
        GenerateContext? ParentContext = null);
}
