namespace MicroFun.Generators.Ast;

public record TemplateAst(SectionBlockAst MainBlock);

public record SectionBlockAst(IReadOnlyList<SectionAst> Sections);

public abstract record SectionAst();

public record TextSectionAst(string Text) : SectionAst();
