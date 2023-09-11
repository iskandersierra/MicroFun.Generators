namespace MicroFun.Generators.Parser;

public sealed class MfgTemplateParserConfig : ICloneable
{
    public MfgTemplateParserTokens ExpressionTokens { get; set; }

    #region [ Clone ]

    public MfgTemplateParserConfig Clone()
    {
        var result = (MfgTemplateParserConfig)MemberwiseClone();

        return result;
    }

    object ICloneable.Clone()
    {
        return Clone();
    }

    #endregion

    #region [ Creation ]

    public static MfgTemplateParserConfig CreateDefault(Action<MfgTemplateParserConfig>? configure = null)
    {
        var result = InternalDefault();

        configure?.Invoke(result);

        return result;
    }

    private static MfgTemplateParserConfig InternalDefault() => new()
    {
        ExpressionTokens = MfgTemplateParserTokens.Default,
    };

    #endregion
}

public record struct MfgTemplateParserTokens(string Open, string Closed)
{
    public static readonly MfgTemplateParserTokens Default = new("{{", "}}");
}
