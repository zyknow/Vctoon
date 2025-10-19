using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
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