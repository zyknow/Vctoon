using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Zyknow.Abp.Lucene.Analyzers;

internal sealed class IcuGeneralAnalyzer : Analyzer
{
    private readonly LuceneVersion _version;

    public IcuGeneralAnalyzer(LuceneVersion version)
    {
        _version = version;
    }

    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var src = new StandardTokenizer(_version, reader);
        TokenStream ts = new ICUNormalizer2Filter(src);
        ts = new LowerCaseFilter(_version, ts);
        ts = new ICUFoldingFilter(ts);
        return new TokenStreamComponents(src, ts);
    }
}