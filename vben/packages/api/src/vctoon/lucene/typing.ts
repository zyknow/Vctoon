/** Lucene 模块类型（对齐 Zyknow.Abp.Lucene.Dtos） */

export type SearchHit = {
  entityId: string
  highlights: Record<string, string[]>
  payload: Record<string, string>
  score: number
}

export type SearchQueryInput = {
  /** 是否启用模糊查询（可选，默认 false） */
  fuzzy?: boolean
  /** 是否返回高亮片段（预留，默认 false） */
  highlight?: boolean
  /** 是否启用前缀匹配（可选，默认 true） */
  prefix?: boolean
  /** 原始查询字符串 */
  query: string
} & BasePageRequest

export type MultiSearchInput = {
  /** 要搜索的实体类型列表 */
  entities: string[]
} & SearchQueryInput

export type SearchResult = PageResult<SearchHit>
