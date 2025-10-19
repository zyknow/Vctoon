using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Zyknow.Abp.Lucene.Analyzers;

public static class AnalyzerFactories
{
    public static Analyzer IcuGeneral()
    {
        return new IcuGeneralAnalyzer(LuceneVersion.LUCENE_48);
    }

    public static Analyzer Keyword()
    {
        return new KeywordAnalyzer();
    }

    public static Analyzer EdgeNGram(int minGram = 1, int maxGram = 20)
    {
        return new EdgeNGramAnalyzer(LuceneVersion.LUCENE_48, minGram, maxGram);
    }
}

internal sealed class IcuGeneralAnalyzer : Analyzer
{
    private readonly LuceneVersion _version;
    public IcuGeneralAnalyzer(LuceneVersion version) => _version = version;

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var src = new StandardTokenizer(_version, reader);
        TokenStream ts = new ICUNormalizer2Filter(src);
        ts = new LowerCaseFilter(_version, ts);
        ts = new ICUFoldingFilter(ts);
        return new TokenStreamComponents(src, ts);
    }
}

internal sealed class EdgeNGramAnalyzer : Analyzer
{
    private readonly LuceneVersion _version;
    private readonly int _min;
    private readonly int _max;
    public EdgeNGramAnalyzer(LuceneVersion version, int minGram, int maxGram)
    {
        _version = version;
        _min = minGram;
        _max = maxGram;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var src = new StandardTokenizer(_version, reader);
        TokenStream ts = new ICUNormalizer2Filter(src);
        ts = new LowerCaseFilter(_version, ts);
        ts = new ICUFoldingFilter(ts);
        ts = new EdgeNGramTokenFilter(_version, ts, _min, _max);
        return new TokenStreamComponents(src, ts);
    }
}