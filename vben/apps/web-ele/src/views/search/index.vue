<script setup lang="ts">
import type { MultiSearchInput, SearchHit } from '@vben/api'

import { computed, reactive, ref, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { luceneApi, mediumResourceApi } from '@vben/api'
import { Page } from '@vben/common-ui'
import { CiSearch, MdiRefresh } from '@vben/icons'

import { ElMessage } from 'element-plus'
import { useAppConfig } from '@vben/hooks'

import { $t } from '#/locales'

defineOptions({ name: 'SearchPage' })

const route = useRoute()
const router = useRouter()

const loading = ref(false)
const total = ref(0)
const items = ref<SearchHit[]>([])

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

const load = async () => {
  if (!queryState.q.trim()) {
    items.value = []
    total.value = 0
    return
  }
  loading.value = true
  try {
    const result = await luceneApi.searchMany(requestParams.value)
    items.value = result.items || []
    total.value = result.totalCount || 0
  } catch {
    ElMessage.error('搜索失败')
  } finally {
    loading.value = false
  }
}

watch(requestParams, () => {
  syncToUrl()
  load()
})

// 初始化
load()

const resolveTypeFromPayload = (payload: Record<string, string>) => {
  const idx = payload?.__IndexName || payload?.IndexName || payload?.indexName
  const name = String(idx || '').toLowerCase()
  if (name.includes('comic')) return 'comic'
  if (name.includes('video')) return 'video'
  return 'comic'
}

const goToDetail = (hit: SearchHit) => {
  const type = resolveTypeFromPayload(hit.payload || {})
  const id = hit.entityId
  if (!id) return
  void router.push({ name: 'MediumDetail', params: { type, mediumId: id } })
}

const onSubmit = () => {
  queryState.page = 1
}

const onReset = () => {
  queryState.q = ''
  queryState.entities = ['comic', 'video']
  queryState.prefix = true
  queryState.fuzzy = false
  queryState.highlight = false
  queryState.page = 1
  queryState.maxResultCount = 10
}

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

const resolveCoverUrl = (hit: SearchHit) => {
  const payload = hit.payload || {}
  const raw = (payload as any).Cover || (payload as any).cover
  if (!raw) return ''
  const url = mediumResourceApi.url.getCover.format({ cover: raw })
  return `${apiURL}${url}`
}
</script>

<template>
  <Page content-class="flex flex-col gap-4">
    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div class="flex items-center justify-between">
        <div class="text-muted-foreground flex items-center gap-2 text-sm">
          <CiSearch class="text-lg" />
          {{ $t('page.search.title') }}
        </div>
        <el-button :loading="loading" @click="load">
          <MdiRefresh class="mr-1" />
          刷新
        </el-button>
      </div>
    </div>

    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div class="grid grid-cols-1 gap-4 md:grid-cols-4">
        <div class="flex items-center gap-2 md:col-span-2">
          <el-input
            v-model="queryState.q"
            placeholder="搜索 comic / video..."
            clearable
            @keyup.enter="onSubmit"
          />
        </div>
        <div class="flex items-center gap-2 md:col-span-2">
          <el-checkbox-group v-model="queryState.entities" class="flex gap-2">
            <el-checkbox label="comic">comic</el-checkbox>
            <el-checkbox label="video">video</el-checkbox>
          </el-checkbox-group>
          <el-checkbox v-model="queryState.prefix">前缀</el-checkbox>
          <el-checkbox v-model="queryState.fuzzy">模糊</el-checkbox>
          <el-checkbox v-model="queryState.highlight">高亮</el-checkbox>
          <el-button type="primary" @click="onSubmit">搜索</el-button>
          <el-button @click="onReset">重置</el-button>
        </div>
      </div>
    </div>

    <div class="border-border bg-card rounded-xl border px-6 py-4 shadow-sm">
      <div class="flex items-center justify-between">
        <div>合计：{{ total }}</div>
        <el-pagination
          v-model:current-page="queryState.page"
          v-model:page-size="queryState.maxResultCount"
          :page-sizes="[10, 20, 50]"
          layout="sizes, prev, pager, next"
          :total="total"
        />
      </div>
      <div class="mt-4 grid grid-cols-1 gap-3">
        <el-card
          v-for="hit in items"
          :key="hit.entityId + hit.score"
          shadow="hover"
          class="cursor-pointer"
          @click="goToDetail(hit)"
        >
          <div class="flex items-center justify-between">
            <div class="flex items-center gap-3">
              <MediumCoverCard
                :src="resolveCoverUrl(hit)"
                base-width="4rem"
                base-height="6rem"
                class="border"
              />
              <div class="flex flex-col">
                <div class="text-lg font-medium">
                  {{ hit.payload?.Title || hit.entityId }}
                </div>
                <div class="text-muted-foreground text-xs">
                  ID: {{ hit.entityId }}
                </div>
              </div>
            </div>
            <el-tag size="small" type="info">
              {{ resolveTypeFromPayload(hit.payload || {}) }}
            </el-tag>
          </div>
        </el-card>
      </div>
    </div>
  </Page>
</template>
