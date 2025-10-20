using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.Icu;
using Lucene.Net.Analysis.NGram;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Util;

namespace Zyknow.Abp.Lucene.Analyzers;

internal class EdgeNGramAnalyzer(LuceneVersion version, int minGram, int maxGram) : Analyzer
{
    protected override TokenStreamComponents CreateComponents(string fieldName, TextReader reader)
    {
        var src = new StandardTokenizer(version, reader);
        TokenStream ts = new ICUNormalizer2Filter(src);
        ts = new LowerCaseFilter(version, ts);
        ts = new ICUFoldingFilter(ts);
        ts = new EdgeNGramTokenFilter(version, ts, minGram, maxGram);
        return new TokenStreamComponents(src, ts);
    }
}