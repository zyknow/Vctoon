<script setup lang="ts">
import type {
  MediumGetListOutput,
  MultiSearchInput,
  SearchHit,
} from '@vben/api'

import { computed, onMounted, onUnmounted, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { luceneApi, MediumType } from '@vben/api'
import { Page } from '@vben/common-ui'

import { ElMessage } from 'element-plus'

import MediumListItem from '#/components/mediums/medium-list-item.vue'
import MediumToolbarSecondSelect from '#/components/mediums/medium-toolbar-second-select.vue'
import {
  provideMediumAllItemProvider,
  provideMediumItemProvider,
} from '#/hooks/useMediumProvider'
import { $t } from '#/locales'

defineOptions({ name: 'SearchPage' })

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const total = ref(0)
const items = ref<SearchHit[]>([])
const updatingFromRoute = ref(false)

const parseBool = (v: unknown, def = false) => {
  if (Array.isArray(v)) return v.some((x) => x === '1' || x === 'true')
  if (typeof v === 'string') return v === '1' || v === 'true'
  return def
}

const parsePage = (v: unknown, def = 1) => {
  const raw = Array.isArray(v) ? v[v.length - 1] : v
  const n = Number.parseInt(String(raw ?? def), 10)
  return Number.isNaN(n) || n <= 0 ? def : n
}

const queryState = reactive({
  q: (route.query.q as string) || '',
  entities: (route.query.entities as string[] | undefined) || [
    'comic',
    'video',
  ],
  prefix: parseBool(route.query.prefix, true),
  fuzzy: parseBool(route.query.fuzzy, false),
  highlight: parseBool(route.query.highlight, false),
  page: parsePage(route.query.page, 1),
  maxResultCount: Number.parseInt(String(route.query.maxResultCount ?? 10), 10),
})

const requestParams = computed<MultiSearchInput>(() => ({
  query: queryState.q,
  entities: queryState.entities,
  prefix: queryState.prefix,
  fuzzy: queryState.fuzzy,
  highlight: queryState.highlight,
  skipCount: (queryState.page - 1) * queryState.maxResultCount,
  maxResultCount: queryState.maxResultCount,
}))

const syncToUrl = () => {
  void router.replace({
    name: 'Search',
    query: {
      q: queryState.q,
      entities: queryState.entities,
      prefix: queryState.prefix ? '1' : '0',
      fuzzy: queryState.fuzzy ? '1' : '0',
      highlight: queryState.highlight ? '1' : '0',
      page: String(queryState.page),
      maxResultCount: String(queryState.maxResultCount),
    },
  })
}

// 选择支持：与首页相同的 Provider 注入（items + selectedMediumIds）
const mediumItems = ref<MediumGetListOutput[]>([])
const selectedMediumIds = ref<string[]>([])
provideMediumItemProvider({ items: mediumItems, selectedMediumIds })
// 提供 all-item provider（仅主列表映射）以支持批量更新等逻辑
provideMediumAllItemProvider({ itemsMap: { main: mediumItems } })

const load = async () => {
  if (!queryState.q.trim()) {
    items.value = []
    total.value = 0
    mediumItems.value = []
    selectedMediumIds.value = []
    return
  }
  loading.value = true
  try {
    const result = await luceneApi.searchMany(requestParams.value)
    const newHits = result.items || []
    total.value = result.totalCount || 0

    // 根据页码决定是重置还是追加
    items.value = queryState.page <= 1 ? newHits : items.value.concat(newHits)

    const mapped = newHits
      .map((h) => mapHitToMedium(h))
      .filter((m): m is MediumGetListOutput => !!m)

    if (queryState.page <= 1) {
      mediumItems.value = mapped
    } else {
      // 追加时去重（按 id）
      const existIds = new Set(mediumItems.value.map((m) => m.id))
      const appendList = mapped.filter((m) => !existIds.has(m.id))
      mediumItems.value = mediumItems.value.concat(appendList)
    }
    // 过滤无效选中项（仅保留当前结果中的选中）
    const idSet = new Set(mediumItems.value.map((m) => m.id))
    selectedMediumIds.value = selectedMediumIds.value.filter((id) =>
      idSet.has(id),
    )
  } catch {
    ElMessage.error($t('page.search.messages.loadError'))
  } finally {
    loading.value = false
  }
}

// 是否还有更多（根据当前已加载数量与总数）
const hasMore = computed(() => mediumItems.value.length < total.value)

// 触发加载更多的观察器
const infiniteTrigger = ref<HTMLElement | null>(null)
let observer: IntersectionObserver | null = null

const initObserver = () => {
  if (!infiniteTrigger.value) return
  observer = new IntersectionObserver(
    (entries) => {
      const entry = entries[0]
      if (entry?.isIntersecting && !loading.value && hasMore.value) {
        // 触底时翻页，触发请求
        queryState.page += 1
      }
    },
    { root: null, rootMargin: '200px', threshold: 0 },
  )
  observer.observe(infiniteTrigger.value)
}

onMounted(() => {
  initObserver()
})

onUnmounted(() => {
  observer?.disconnect()
  observer = null
})

watch(requestParams, () => {
  if (updatingFromRoute.value) return
  syncToUrl()
  load()
})

watch(
  () => route.query,
  (q) => {
    updatingFromRoute.value = true
    queryState.q = (q.q as string) || ''
    queryState.entities = (q.entities as string[] | undefined) || [
      'comic',
      'video',
    ]
    queryState.prefix = parseBool(q.prefix, true)
    queryState.fuzzy = parseBool(q.fuzzy, false)
    queryState.highlight = parseBool(q.highlight, false)
    queryState.page = parsePage(q.page, 1)
    queryState.maxResultCount = Number.parseInt(
      String(q.maxResultCount ?? 10),
      10,
    )
    load()
    updatingFromRoute.value = false
  },
  { immediate: true },
)

const resolveTypeFromPayload = (payload: Record<string, string>) => {
  const idx = payload?.__IndexName || payload?.IndexName || payload?.indexName
  const name = String(idx || '').toLowerCase()
  if (name.includes('comic')) return 'comic'
  if (name.includes('video')) return 'video'
  return 'comic'
}

// 将 payload 映射到枚举类型
const toMediumTypeEnum = (payload: Record<string, any>): MediumType => {
  const type = resolveTypeFromPayload(payload)
  return type === 'video' ? MediumType.Video : MediumType.Comic
}

// 将 SearchHit 映射为 MediumGetListOutput（仅必要字段）
const mapHitToMedium = (hit: SearchHit): MediumGetListOutput | null => {
  const p = hit.payload || {}
  const id = hit.entityId || (p as any).Id || (p as any).id
  if (!id) return null
  const title = (p as any).Title || (p as any).title || ''
  const cover = (p as any).Cover || (p as any).cover || undefined
  const readingProgressRaw =
    (p as any).ReadingProgress ?? (p as any).readingProgress
  const readingLastTime =
    (p as any).ReadingLastTime ?? (p as any).readingLastTime
  const creationTime = (p as any).CreationTime ?? (p as any).creationTime
  const lastModificationTime =
    (p as any).LastModificationTime ?? (p as any).lastModificationTime
  const libraryId = (p as any).LibraryId ?? (p as any).libraryId
  const medium: Partial<MediumGetListOutput> & { id: string } = {
    id,
    title,
    cover,
    mediumType: toMediumTypeEnum(p),
    readingProgress:
      typeof readingProgressRaw === 'number' ? readingProgressRaw : undefined,
    readingLastTime,
    creationTime,
    lastModificationTime,
    libraryId,
  }
  return medium as MediumGetListOutput
}
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <div class="flex items-center justify-between">
      <div>{{ $t('page.search.total') }}：{{ total }}</div>
      <!-- 移除分页，改为无限滚动加载 -->
    </div>

    <!-- 选择工具栏（当有选中项时显示） -->
    <MediumToolbarSecondSelect v-if="selectedMediumIds.length > 0" />

    <!-- 使用 MediumListItem 渲染并支持选中（与首页一致） -->
    <div class="mt-4 space-y-2">
      <MediumListItem
        v-for="item in mediumItems"
        :key="item.id"
        :model-value="item"
      />
    </div>

    <!-- 底部加载状态与触发器 -->
    <div class="text-muted-foreground py-2 text-center">
      <span v-if="loading">{{ $t('common.loading') }}</span>
      <span v-else-if="!hasMore && mediumItems.length > 0">
        {{ $t('common.noMore') }}
      </span>
    </div>
    <div ref="infiniteTrigger" class="h-6"></div>
  </Page>
</template>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 2;
  line-clamp: 2;
  -webkit-box-orient: vertical;
}
</style>
