<script setup lang="ts">
import type { SearchHit } from '@vben/api'

import { computed, ref } from 'vue'
import { useRouter } from 'vue-router'

import { luceneApi, mediumResourceApi } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import { CiSearch, MdiComic, MdiVideo } from '@vben/icons'

import { ElMessage } from 'element-plus'

defineOptions({ name: 'MediumSearchInput' })

const queryText = ref('')
const loading = ref(false)

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

const router = useRouter()

const maxResultCount = 6

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
  const q = text.trim()
  if (!q) {
    cb([])
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
      ElMessage.error('搜索失败')
      cb([])
    })
    .finally(() => {
      loading.value = false
    })
}

const handleSelect = (item: any) => {
  const selected = item as SuggestionItem
  const q = queryText.value.trim()
  if (!q) return
  if (selected.kind === 'hit' && selected.id && selected.type) {
    const typeSegment = selected.type === 'video' ? 'video' : 'comic'
    void router.push({
      name: 'MediumDetail',
      params: { type: typeSegment, mediumId: selected.id },
    })
  }
}

const goToSearchAll = () => {
  const q = queryText.value.trim()
  if (!q) return
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

const placeholder = computed(() => '搜索 comic / video...')
</script>

<template>
  <div class="medium-search-input w-80">
    <el-autocomplete
      v-model="queryText"
      :fetch-suggestions="fetchSuggestions"
      :placeholder="placeholder"
      :trigger-on-focus="false"
      class="w-full"
      @select="handleSelect"
    >
      <template #prefix>
        <CiSearch class="text-muted-foreground text-lg" />
      </template>
      <template #default="{ item }">
        <div class="flex items-center justify-between">
          <div class="flex items-center gap-2">
            <MediumCoverCard
              :src="item.cover"
              base-width="3rem"
              base-height="4.5rem"
              class="border"
            />
            <div class="flex flex-col">
              <div class="text-sm font-medium">
                {{ item.title || item.value }}
              </div>
              <div class="text-muted-foreground text-xs">
                <span v-if="item.type === 'comic'">
                  <MdiComic class="inline-block text-xs" /> comic
                </span>
                <span v-else-if="item.type === 'video'">
                  <MdiVideo class="inline-block text-xs" /> video
                </span>
              </div>
            </div>
          </div>
          <el-tag v-if="item.score" size="small" type="info">
            {{ item.score.toFixed(2) }}
          </el-tag>
        </div>
      </template>
      <template #footer>
        <div class="p-2">
          <el-button
            class="w-full"
            type="primary"
            size="small"
            @click="goToSearchAll"
          >
            查看全部搜索结果
          </el-button>
        </div>
      </template>
    </el-autocomplete>
  </div>
</template>

<style scoped>
.medium-search-input :deep(.el-input__wrapper) {
  border-radius: 0.75rem;
}
</style>
