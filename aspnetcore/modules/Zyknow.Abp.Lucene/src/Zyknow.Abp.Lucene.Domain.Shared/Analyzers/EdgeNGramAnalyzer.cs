using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Zyknow.Abp.Lucene.Analyzers;

internal sealed class EdgeNGramAnalyzer : Analyzer
{
    private readonly int _max;
    private readonly int _min;
    private readonly LuceneVersion _version;

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