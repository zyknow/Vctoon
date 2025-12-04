<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { onClickOutside, useDebounceFn } from '@vueuse/core'
import { useRoute, useRouter } from 'vue-router'

import type { SearchHit } from '@/api/http/lucene'
import { luceneApi } from '@/api/http/lucene'
import { mediumResourceApi } from '@/api/http/medium-resource'
import { $t } from '@/locales/i18n'

defineOptions({ name: 'MediumSearchInput' })

const searchTerm = ref('')
const loading = ref(false)
const dropdownVisible = ref(false)
const inputRef = ref<HTMLElement | null>(null)
const dropdownRef = ref<HTMLElement | null>(null)

const toast = useToast()

// 点击外部关闭下拉框
onClickOutside(
  dropdownRef,
  () => {
    dropdownVisible.value = false
  },
  { ignore: [inputRef] },
)

type SuggestionItem = {
  label: string
  value: string
  cover?: string
  id?: string
  type?: 'comic' | 'video'
  score?: number
  isHistory?: boolean
}

const route = useRoute()
const router = useRouter()

const isSearchRoute = computed(() => route.name === 'Search')

const maxResultCount = 10

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
  } catch {
    // Ignore localStorage errors
  }
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
  return mediumResourceApi.getCoverUrl(raw)
}

const toSuggestion = (hit: SearchHit): null | SuggestionItem => {
  const payload = hit.payload || {}
  const id = hit.entityId || (payload as any).Id || (payload as any).id
  if (!id) return null
  const title = (payload as any).Title || (payload as any).title || id
  const type = resolveTypeFromPayload(payload)
  return {
    label: title,
    value: id,
    id,
    type: type ?? undefined,
    score: hit.score,
    cover: resolveCoverFromPayload(payload),
  }
}

const searchSuggestions = ref<SuggestionItem[]>([])

const fetchSuggestions = async (q: string) => {
  const trimmed = q.trim()

  if (!trimmed) {
    searchSuggestions.value = []
    loading.value = false
    return
  }

  if (isSearchRoute.value) {
    searchSuggestions.value = []
    loading.value = false
    return
  }

  console.log('[Search] 开始搜索请求，关键词:', trimmed)
  loading.value = true
  try {
    const result = await luceneApi.searchMany({
      query: trimmed,
      entities: ['comic', 'video'],
      maxResultCount,
      skipCount: 0,
      prefix: true,
      fuzzy: false,
      highlight: false,
    })

    console.log('[Search] 搜索结果返回，数量:', result?.items?.length || 0)

    const seen = new Set<string>()
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
    searchSuggestions.value = items
  } catch {
    toast.add({
      title: $t('page.search.messages.loadError'),
      color: 'error',
    })
    searchSuggestions.value = []
  } finally {
    loading.value = false
  }
}

// 标准防抖函数
const fetchSuggestionsDebounced = useDebounceFn(fetchSuggestions, 200)

// 追踪上一次的搜索词长度，用于判断是否是第一个字符
let lastSearchLength = 0

// 智能搜索：从空到第一个字符时立即执行，否则使用防抖
const smartFetch = (query: string) => {
  const currentLength = query.trim().length

  // 从空到第一个字符（0 -> 1）：立即执行
  if (lastSearchLength === 0 && currentLength === 1) {
    console.log('[Search] 第一个字符，立即执行搜索:', query)
    lastSearchLength = currentLength
    void fetchSuggestions(query)
  }
  // 其他情况：使用防抖
  else {
    console.log('[Search] 使用防抖，当前长度:', currentLength)
    lastSearchLength = currentLength
    void fetchSuggestionsDebounced(query)
  }

  // 如果清空了，重置长度
  if (currentLength === 0) {
    lastSearchLength = 0
  }
}

const navigateToSearch = (query: string) => {
  if (!query) return
  addHistory(query)
  void router.push({
    name: 'Search',
    query: {
      q: query,
      entities: ['comic', 'video'],
      prefix: '1',
      fuzzy: '0',
      highlight: '0',
      page: '1',
      maxResultCount: String(maxResultCount),
    },
  })
}

const handleUpdateSearchTerm = (value: string) => {
  console.log('[Search] handleUpdateSearchTerm 接收到值:', value)

  if (!isSearchRoute.value) {
    const currentTerm = value

    // 使用智能搜索：第一次立即执行，后续防抖
    smartFetch(currentTerm)

    // 有输入时显示下拉框
    if (currentTerm.trim() || searchHistory.value.length > 0) {
      dropdownVisible.value = true
    }
    return
  }

  // 搜索路由,更新 URL
  void router.replace({
    name: 'Search',
    query: {
      ...route.query,
      q: value,
      page: '1',
      maxResultCount: String(route.query.maxResultCount || maxResultCount),
      prefix: String(route.query.prefix ?? '1'),
      fuzzy: String(route.query.fuzzy ?? '0'),
      highlight: String(route.query.highlight ?? '0'),
      entities: (route.query.entities as any) || ['comic', 'video'],
    },
  })
}

const handleInputFocus = () => {
  // 非搜索页：无条件展开（展示历史或空态），更贴合用户预期
  if (!isSearchRoute.value) {
    dropdownVisible.value = true
  }
}

watch(
  () => route.name,
  (name) => {
    if (name === 'Search') {
      const q = (route.query.q as string) || ''
      if (q !== searchTerm.value) {
        searchTerm.value = q
      }
    }
  },
  { immediate: true },
)

const handleSelect = (item: SuggestionItem) => {
  // 如果是历史记录项，直接搜索
  if (item.isHistory) {
    navigateToSearch(item.value)
    dropdownVisible.value = false
    return
  }

  // 如果是搜索结果，跳转到详情页
  if (item.id && item.type) {
    const typeSegment = item.type === 'video' ? 'video' : 'comic'
    searchTerm.value = ''
    lastSearchLength = 0 // 重置，下次输入立即触发
    dropdownVisible.value = false
    void router.push({
      name: 'MediumDetail',
      params: { type: typeSegment, mediumId: item.id },
    })
  }
}

// 监听搜索词，清空时重置长度
watch(searchTerm, (newVal) => {
  if (!newVal) {
    lastSearchLength = 0
  }
})

const handleKeyEnter = () => {
  const q = searchTerm.value.trim()
  if (!q) return
  dropdownVisible.value = false
  navigateToSearch(q)
}

const handleClearHistory = () => {
  clearHistory()
  if (searchHistory.value.length === 0 && !searchTerm.value.trim()) {
    dropdownVisible.value = false
  }
}
</script>

<template>
  <div ref="dropdownRef" class="medium-search-input relative w-full">
    <!-- 搜索输入框 -->
    <UInput
      ref="inputRef"
      v-model="searchTerm"
      leading-icon="i-lucide-search"
      :loading="loading"
      :placeholder="$t('page.search.placeholder')"
      class="w-full"
      @update:model-value="handleUpdateSearchTerm"
      @focus="handleInputFocus"
      @click="handleInputFocus"
      @keyup.enter="handleKeyEnter"
    />

    <!-- 自定义下拉面板 -->
    <Transition
      enter-active-class="transition duration-100 ease-out"
      enter-from-class="transform scale-95 opacity-0"
      enter-to-class="transform scale-100 opacity-100"
      leave-active-class="transition duration-75 ease-in"
      leave-from-class="transform scale-100 opacity-100"
      leave-to-class="transform scale-95 opacity-0"
    >
      <div
        v-if="dropdownVisible && !isSearchRoute"
        class="bg-elevated absolute top-full left-0 z-50 mt-2 max-h-[85vh] min-w-160 overflow-hidden rounded-xl shadow-2xl"
      >
        <!-- 标题栏 -->
        <div
          v-if="searchTerm.trim() && searchSuggestions.length > 0"
          class="px-6 py-4"
        >
          <h3 class="text-lg font-semibold">
            {{ $t('page.search.title') }}
            <span class="text-muted-foreground ml-2 text-base font-normal"
              >"{{ searchTerm }}"</span
            >
          </h3>
        </div>

        <div
          v-else-if="!searchTerm.trim() && searchHistory.length > 0"
          class="flex items-center justify-between px-6 py-4"
        >
          <h3 class="text-base font-semibold">
            {{ $t('page.search.history.title') }}
          </h3>
          <UButton
            variant="ghost"
            size="xs"
            color="neutral"
            @click="handleClearHistory"
          >
            {{ $t('page.search.history.clear') }}
          </UButton>
        </div>

        <!-- 内容区域 -->
        <div class="max-h-[70vh] overflow-y-auto">
          <!-- 搜索历史列表 -->
          <div
            v-if="!searchTerm.trim() && searchHistory.length > 0"
            class="p-2"
          >
            <button
              v-for="h in searchHistory"
              :key="h"
              class="group hover:bg-primary/10 dark:hover:bg-primary/25 flex w-full items-center gap-4 rounded-lg px-4 py-3 text-left transition-colors duration-150"
              @click="handleSelect({ label: h, value: h, isHistory: true })"
            >
              <div
                class="bg-primary/10 flex size-10 shrink-0 items-center justify-center rounded-lg"
              >
                <UIcon name="i-lucide-clock" class="text-primary size-5" />
              </div>
              <span class="flex-1 truncate font-medium">{{ h }}</span>
              <UIcon
                name="i-lucide-arrow-up-left"
                class="text-muted-foreground size-4 opacity-0 transition-opacity"
              />
            </button>
          </div>

          <!-- 搜索结果列表 -->
          <div v-else-if="searchTerm.trim()">
            <!-- 有结果 -->
            <div v-if="searchSuggestions.length > 0">
              <button
                v-for="(item, index) in searchSuggestions"
                :key="index"
                class="group hover:bg-primary/10 dark:hover:bg-primary/25 flex w-full items-center gap-6 rounded-lg px-6 py-5 text-left transition-colors duration-150"
                @click="handleSelect(item)"
              >
                <!-- 封面 -->
                <div class="relative shrink-0">
                  <MediumCoverCard
                    v-if="item.cover"
                    :src="item.cover"
                    base-width="5rem"
                    base-height="7.5rem"
                    class="rounded-lg shadow-lg"
                  />
                  <div
                    v-else
                    class="bg-muted flex h-30 w-20 shrink-0 items-center justify-center rounded-lg shadow-lg"
                  >
                    <UIcon
                      :name="
                        item.type === 'video'
                          ? 'i-lucide-video'
                          : 'i-lucide-book-open'
                      "
                      class="text-muted-foreground size-10"
                    />
                  </div>
                  <!-- 播放图标覆盖层 -->
                  <div
                    class="absolute inset-0 flex items-center justify-center rounded-lg bg-black/40 opacity-0 transition-opacity"
                  >
                    <div
                      class="bg-primary flex size-12 items-center justify-center rounded-full"
                    >
                      <UIcon
                        name="i-lucide-play"
                        class="ml-0.5 size-6 text-white"
                      />
                    </div>
                  </div>
                </div>

                <!-- 信息 -->
                <div class="flex min-w-0 flex-1 flex-col gap-2">
                  <h4 class="text-lg leading-tight font-semibold">
                    {{ item.label }}
                  </h4>
                  <div
                    class="text-muted-foreground flex items-center gap-2 text-sm"
                  >
                    <UIcon
                      :name="
                        item.type === 'comic'
                          ? 'i-lucide-book-open'
                          : 'i-lucide-video'
                      "
                      class="size-4"
                    />
                    <span>{{
                      item.type === 'comic'
                        ? $t('page.mediums.info.comic')
                        : $t('page.mediums.info.video')
                    }}</span>
                  </div>
                </div>

                <!-- 右侧操作区域 -->
                <div class="flex shrink-0 items-center gap-3">
                  <div
                    class="bg-primary/10 text-primary flex size-12 items-center justify-center rounded-full transition-all"
                  >
                    <UIcon name="i-lucide-play" class="ml-0.5 size-5" />
                  </div>
                </div>
              </button>
            </div>

            <!-- 无结果 -->
            <div v-else-if="!loading" class="px-6 py-16 text-center">
              <UIcon
                name="i-lucide-search-x"
                class="text-muted-foreground mx-auto mb-4 size-16"
              />
              <div class="text-muted-foreground text-base font-medium">
                {{ $t('page.search.messages.noResults') }}
              </div>
            </div>
          </div>

          <!-- 空状态 -->
          <div
            v-else-if="!searchTerm.trim() && searchHistory.length === 0"
            class="px-6 py-16 text-center"
          >
            <UIcon
              name="i-lucide-search"
              class="text-muted-foreground mx-auto mb-4 size-16"
            />
            <div class="text-muted-foreground text-base font-medium">
              {{ $t('page.search.history.empty') }}
            </div>
          </div>
        </div>

        <!-- 查看全部结果按钮 -->
        <div v-if="searchTerm.trim().length > 0" class="p-4">
          <UButton
            color="primary"
            size="lg"
            variant="solid"
            class="w-full font-semibold"
            @click="handleKeyEnter"
          >
            {{ $t('page.search.actions.viewAllResults') }}
            <template #trailing>
              <UIcon name="i-lucide-arrow-right" class="size-5" />
            </template>
          </UButton>
        </div>
      </div>
    </Transition>
  </div>
</template>
