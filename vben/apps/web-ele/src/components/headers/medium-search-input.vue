<script setup lang="ts">
import type { SearchHit } from '@vben/api'

import { computed, onMounted, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { luceneApi, mediumResourceApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import { CiSearch, MdiComic, MdiVideo } from '@vben/icons'

import { ElMessage } from 'element-plus'

import { $t } from '#/locales'

defineOptions({ name: 'MediumSearchInput' })

const queryText = ref('')
const loading = ref(false)
const autocompleteRef = ref<any>(null)

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

type SuggestionKind = 'hit' | 'nav'

type SuggestionItem = {
  cover?: string
  id?: string
  kind: SuggestionKind
  score?: number
  title?: string
  type?: 'comic' | 'video'
  value: string
}

const route = useRoute()
const router = useRouter()

const isSearchRoute = computed(() => route.name === 'Search')

const maxResultCount = 6

// 本地搜索历史
const HISTORY_KEY = 'medium.search.history'
const searchHistory = ref<string[]>([])
const loadHistory = () => {
  try {
    const raw = localStorage.getItem(HISTORY_KEY)
    const arr = raw ? JSON.parse(raw) : []
    searchHistory.value = Array.isArray(arr) ? arr : []
  } catch {
    searchHistory.value = []
  }
}
const saveHistory = () => {
  try {
    const capped = searchHistory.value.slice(0, 12)
    localStorage.setItem(HISTORY_KEY, JSON.stringify(capped))
  } catch {}
}
const addHistory = (q: string) => {
  const v = String(q || '').trim()
  if (!v) return
  const idx = searchHistory.value.indexOf(v)
  if (idx !== -1) searchHistory.value.splice(idx, 1)
  searchHistory.value.unshift(v)
  if (searchHistory.value.length > 20) {
    searchHistory.value = searchHistory.value.slice(0, 20)
  }
  saveHistory()
}
const removeHistory = (q: string) => {
  const v = String(q || '').trim()
  if (!v) return
  searchHistory.value = searchHistory.value.filter((x) => x !== v)
  saveHistory()
}
const clearHistory = () => {
  searchHistory.value = []
  saveHistory()
}

onMounted(() => loadHistory())

const resolveTypeFromPayload = (
  payload: Record<string, any>,
): 'comic' | 'video' | null => {
  const idx = payload?.__IndexName || payload?.IndexName || payload?.indexName
  if (!idx) return null
  const name = String(idx).toLowerCase()
  if (name.includes('comic')) return 'comic'
  if (name.includes('video')) return 'video'
  return null
}

const resolveCoverFromPayload = (payload: Record<string, any>) => {
  const raw = (payload as any).Cover || (payload as any).cover
  if (!raw) return undefined
  const url = mediumResourceApi.url.getCover.format({ cover: raw })
  return `${apiURL}${url}`
}

const toSuggestion = (hit: SearchHit): null | SuggestionItem => {
  const payload = hit.payload || {}
  const id = hit.entityId || (payload as any).Id || (payload as any).id
  if (!id) return null
  const title = (payload as any).Title || (payload as any).title || id
  const type = resolveTypeFromPayload(payload)
  const typeLabel = type === 'video' ? 'video' : 'comic'
  return {
    kind: 'hit',
    id,
    type: type ?? undefined,
    title,
    score: hit.score,
    value: `${title} [${typeLabel}]`,
    cover: resolveCoverFromPayload(payload),
  }
}

const fetchSuggestions = (text: string, cb: (items: any[]) => void) => {
  const q = text?.trim() || queryText.value.trim()
  if (!q) {
    // 返回占位项以打开弹层，结合样式仅显示 header
    cb([{ kind: 'nav', value: '' } as SuggestionItem])
    return
  }
  loading.value = true
  luceneApi
    .searchMany({
      query: q,
      entities: ['comic', 'video'],
      maxResultCount,
      skipCount: 0,
      prefix: true,
      fuzzy: false,
      highlight: false,
    })
    .then((result) => {
      const seen = new Set<string>()
      // 改为显式构建数组，确保类型为 SuggestionItem[]
      const mapped: SuggestionItem[] = []
      const src = result?.items || []
      for (const h of src) {
        const s = toSuggestion(h)
        if (s) mapped.push(s)
      }
      const items = mapped.filter((s) => {
        const id = s.id || ''
        if (!id) return true
        if (seen.has(id)) return false
        seen.add(id)
        return true
      })
      cb(items as any[])
    })
    .catch(() => {
      ElMessage.error($t('page.search.messages.loadError'))
      cb([])
    })
    .finally(() => {
      loading.value = false
    })
}

const emptySuggestions = (_text: string, cb: (items: any[]) => void) => {
  // 在 /search 路由不显示下拉，返回空列表
  cb([])
}

const applyHistory = (q: string) => {
  const v = String(q || '').trim()
  if (!v) return
  queryText.value = v
  addHistory(v)
  const refAny = autocompleteRef.value as any
  // 优先调用组件的 getData 刷新建议；若不存在则回退到 blur/focus
  if (refAny?.getData) {
    try {
      refAny.getData()
      refAny.focus?.()
      return
    } catch {}
  }
  refAny?.blur?.()
  setTimeout(() => refAny?.focus?.(), 0)
}

const navigateOnEnter = () => {
  const q = queryText.value.trim()
  if (!q) return
  addHistory(q)
  if (isSearchRoute.value) {
    void router.replace({
      name: 'Search',
      query: {
        ...route.query,
        q,
        page: '1',
        maxResultCount: String(route.query.maxResultCount || maxResultCount),
        prefix: String(route.query.prefix ?? '1'),
        fuzzy: String(route.query.fuzzy ?? '0'),
        highlight: String(route.query.highlight ?? '0'),
        entities: (route.query.entities as any) || ['comic', 'video'],
      },
    })
  } else {
    void router.push({
      name: 'Search',
      query: {
        q,
        entities: ['comic', 'video'],
        prefix: '1',
        fuzzy: '0',
        highlight: '0',
        page: '1',
        maxResultCount: String(maxResultCount),
      },
    })
  }
}

const handleInput = (value: string) => {
  if (!isSearchRoute.value) return
  const q = String(value || '').trim()
  void router.replace({
    name: 'Search',
    query: {
      ...route.query,
      q,
      page: '1',
      maxResultCount: String(route.query.maxResultCount || maxResultCount),
      prefix: String(route.query.prefix ?? '1'),
      fuzzy: String(route.query.fuzzy ?? '0'),
      highlight: String(route.query.highlight ?? '0'),
      entities: (route.query.entities as any) || ['comic', 'video'],
    },
  })
}

watch(
  () => route.name,
  (name) => {
    if (name === 'Search') {
      const q = (route.query.q as string) || ''
      if (q !== queryText.value) queryText.value = q
    }
  },
  { immediate: true },
)

const handleSelect = (item: any) => {
  const selected = item as SuggestionItem
  const q = queryText.value.trim()
  if (!q) return
  if (selected.kind === 'hit' && selected.id && selected.type) {
    const typeSegment = selected.type === 'video' ? 'video' : 'comic'
    // 选择前清空输入，避免导航后残留内容
    queryText.value = ''
    // 关闭下拉（Element Plus 会在 select 后自动关闭，但确保状态干净）
    ;(autocompleteRef.value as any)?.blur?.()
    void router.push({
      name: 'MediumDetail',
      params: { type: typeSegment, mediumId: selected.id },
    })
  }
}

const goToSearchAll = () => {
  const q = queryText.value.trim()
  if (!q) return
  addHistory(q)
  void router.push({
    name: 'Search',
    query: {
      q,
      entities: ['comic', 'video'],
      prefix: '1',
      fuzzy: '0',
      highlight: '0',
      page: '1',
    },
  })
}
</script>

<template>
  <div class="medium-search-input w-80">
    <el-autocomplete
      clearable
      v-model="queryText"
      :key="isSearchRoute ? 'search' : 'normal'"
      :fetch-suggestions="isSearchRoute ? emptySuggestions : fetchSuggestions"
      :trigger-on-focus="!isSearchRoute"
      :popper-class="
        isSearchRoute
          ? 'medium-search-hide-items'
          : 'medium-search-dropdown-large'
      "
      class="w-full"
      ref="autocompleteRef"
      @select="handleSelect"
      @keyup.enter="navigateOnEnter"
      @input="handleInput"
    >
      <template #prefix>
        <CiSearch class="text-muted-foreground text-lg" />
      </template>

      <!-- 非 /search 路由：显示 header 历史；当输入为空时隐藏列表，仅显示 header -->
      <template v-if="!isSearchRoute" #header>
        <div class="p-3">
          <div class="mb-2 flex items-center justify-between">
            <div class="text-muted-foreground text-xs">
              {{ $t('page.search.history.title') }}
            </div>
            <el-button link size="small" @click="clearHistory">
              {{ $t('page.search.history.clear') }}
            </el-button>
          </div>
          <div v-if="searchHistory.length > 0" class="flex flex-wrap gap-2">
            <el-tag
              v-for="h in searchHistory"
              :key="h"
              class="cursor-pointer"
              type="info"
              size="small"
              @click="applyHistory(h)"
            >
              {{ h }}
              <template #close>
                <span class="ml-1" @click.stop="removeHistory(h)">×</span>
              </template>
            </el-tag>
          </div>
          <div v-else class="text-muted-foreground text-xs">
            {{ $t('page.search.history.empty') }}
          </div>
        </div>
      </template>

      <template
        v-if="!isSearchRoute && queryText.trim().length > 0"
        #default="{ item }"
      >
        <div
          class="flex items-center justify-between"
          @click="addHistory(queryText.trim())"
        >
          <div class="flex items-center gap-3">
            <MediumCoverCard
              :src="item.cover"
              base-width="7rem"
              base-height="10.5rem"
              class="border"
            />
            <div class="flex flex-col">
              <div class="text-base font-medium">
                {{ item.title || item.value }}
              </div>
              <div class="text-muted-foreground text-xs">
                <span v-if="item.type === 'comic'">
                  <MdiComic class="inline-block text-xs" />
                  {{ $t('page.mediums.info.comic') }}
                </span>
                <span v-else-if="item.type === 'video'">
                  <MdiVideo class="inline-block text-xs" />
                  {{ $t('page.mediums.info.video') }}
                </span>
              </div>
            </div>
          </div>
          <!-- <el-tag v-if="item.score" size="small" type="info">
            {{ item.score.toFixed(2) }}
          </el-tag> -->
        </div>
      </template>
      <template v-if="!isSearchRoute && queryText.trim().length > 0" #footer>
        <div class="flex items-center justify-start p-3">
          <el-button type="primary" size="small" @click="goToSearchAll">
            {{ $t('page.search.actions.viewAllResults') }}
          </el-button>
        </div>
      </template>
    </el-autocomplete>
  </div>
</template>

<style scoped></style>

<style>
/* 下拉弹层宽度及高度限制不超过屏幕 */
.medium-search-dropdown-large {
  width: 50vh;
  height: 80vh;
  overflow-y: auto;

  .el-autocomplete-suggestion {
    @apply flex h-full flex-col;

    .el-scrollbar {
      @apply h-full flex-1;

      .el-autocomplete-suggestion__wrap {
        @apply h-full max-h-full;
      }
    }
  }
}

/* 在 /search 路由完全隐藏弹层 */
.medium-search-hide-all,
.medium-search-hide-items {
  display: none !important;
}
</style>
