using MicroFun.Generators.Ast;
using Pidgin;
using static Pidgin.Parser;
using static Pidgin.Parser<char>;

namespace MicroFun.Generators.Parser;

public class MfgTemplateParserFactory
{
    private readonly MfgTemplateParserConfig config;
    private readonly Lazy<Parser<char, TemplateAst>> parser;

    public MfgTemplateParserFactory(MfgTemplateParserConfig? config = null)
    {
        this.config ??= MfgTemplateParserConfig.CreateDefault();

        this.parser = new Lazy<Parser<char, TemplateAst>>(CreateParser);
    }

    public Parser<char, TemplateAst> CreateParser()
    {
        //Parser<char, T> Tok<T>(Parser<char, T> tokenParser) =>
        //    Try(tokenParser).Before(SkipWhitespaces);

        var textParser = Any.AtLeastOnceString().Map(text => new TextSectionAst(text));

        var sectionParser = OneOf(
            textParser.Map(e => (SectionAst)e));

        var sectionBlockParser = sectionParser.Many()
            .Map(e => new SectionBlockAst(e.ToList().AsReadOnly()));

        var templateParser = sectionBlockParser.Map(e =>
            new TemplateAst(MainBlock: e));

        return templateParser;
    }
}
