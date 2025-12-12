<script setup lang="ts">
import { VueDraggable } from 'vue-draggable-plus'
import { useRouter } from 'vue-router'

import { mediumApi } from '@/api/http/medium'
import type { Medium, MediumGetListOutput } from '@/api/http/medium/typing'
import MediumGridItem from '@/components/mediums/MediumGridItem.vue'
import MediumToolbarFirst from '@/components/mediums/MediumToolbarFirst.vue'
import { useIsMobile } from '@/hooks/useIsMobile'
import { provideMediumItemProvider } from '@/hooks/useMediumProvider'
import { $t } from '@/locales/i18n'
import { formatMediumDateTime } from '@/utils/medium'

const props = defineProps<{
  loading: boolean
  medium: Medium
  mediumId: string
}>()

const { isMobile } = useIsMobile()
const router = useRouter()

const medium = toRef(props, 'medium')
const mediumId = toRef(props, 'mediumId')
const loading = toRef(props, 'loading')

const seriesList = ref<MediumGetListOutput[]>([])
const selectedMediumIds = ref<string[]>([])

provideMediumItemProvider({
  items: seriesList as unknown as Ref<MediumGetListOutput[]>,
  selectedMediumIds,
})

const mediumTitle = computed(() => medium.value.title ?? '')
const coverUrl = computed(() => {
  if (!medium.value.cover) return undefined
  return mediumApi.getCoverUrl(medium.value.cover)
})

const creationTimeText = computed(() => {
  const value = medium.value.creationTime
  if (!value) return '--'
  return formatMediumDateTime(value)
})

const descriptionText = computed(() => medium.value.description?.trim() ?? '')
const hasTags = computed(() => (medium.value.tags?.length ?? 0) > 0)
const hasArtists = computed(() => (medium.value.artists?.length ?? 0) > 0)

const detailTopGridClasses = computed(() => {
  const classes = ['detail-top', 'grid', 'gap-8', 'items-start']
  if (isMobile.value) {
    classes.push('grid-cols-1')
  } else {
    classes.push(
      'lg:grid-cols-[220px_1fr]',
      'xl:grid-cols-[240px_1fr]',
      '2xl:grid-cols-[260px_1fr]',
    )
  }
  return classes
})

const seriesLoading = ref(false)

const loadSeriesList = async () => {
  if (!mediumId.value) return
  seriesLoading.value = true
  try {
    seriesList.value = await mediumApi.getSeriesList(mediumId.value)
  } catch (error) {
    console.error(error)
  } finally {
    seriesLoading.value = false
  }
}

const onSort = async () => {
  const ids = seriesList.value.map((m) => m.id)
  try {
    await mediumApi.seriesSort({
      seriesId: mediumId.value,
      mediumIds: ids,
    })
  } catch (e) {
    console.error(e)
    // Revert or show error
    await loadSeriesList() // reload to reset
  }
}

onMounted(() => {
  void loadSeriesList()
})
</script>

<template>
  <MediumToolbarFirst v-if="!isMobile" :title="mediumTitle">
    <template #left>
      <div class="flex items-center gap-3">
        <UButton size="sm" variant="outline" @click="router.back()">
          <template #leading>
            <UIcon name="i-heroicons-arrow-left" />
          </template>
          {{ $t('page.mediums.detail.back') }}
        </UButton>
      </div>
    </template>
  </MediumToolbarFirst>

  <div class="flex min-h-0 flex-1 flex-col overflow-auto">
    <div v-if="loading" class="text-muted-foreground py-20 text-center">
      {{ $t('common.loading') }}
    </div>
    <div v-else class="flex flex-1 flex-col p-4 md:p-8">
      <!-- Info Section -->
      <div :class="detailTopGridClasses">
        <div
          class="relative flex aspect-[3/4] w-full items-center justify-center overflow-hidden rounded-lg shadow-lg"
          :class="coverUrl ? 'bg-black' : 'bg-muted'"
        >
          <img
            v-if="coverUrl"
            :src="coverUrl"
            alt="cover"
            class="h-full w-full object-cover"
            loading="lazy"
          />
          <div
            v-else
            class="text-muted-foreground flex h-full w-full items-center justify-center text-7xl"
          >
            📚
          </div>
        </div>

        <div class="flex flex-col gap-8">
          <div class="space-y-3">
            <h1 class="text-3xl leading-tight font-bold lg:text-4xl">
              {{ mediumTitle || $t('page.mediums.detail.untitled') }}
            </h1>
            <div class="flex flex-wrap items-center gap-3 text-sm">
              <UBadge color="primary">
                {{ $t('page.mediums.info.series') }}
              </UBadge>
              <span class="text-muted-foreground">
                {{ creationTimeText.split(' ')[0] }}
              </span>
            </div>
          </div>

          <div v-if="descriptionText" class="space-y-2">
            <div
              class="text-muted-foreground text-sm font-medium tracking-wide uppercase"
            >
              {{ $t('page.mediums.info.description') }}
            </div>
            <p class="leading-relaxed">
              {{ descriptionText }}
            </p>
          </div>

          <div class="space-y-4">
            <div v-if="hasTags">
              <div
                class="text-muted-foreground mb-2 text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.tags') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <UBadge v-for="tag in medium.tags" :key="tag.id" size="sm">
                  {{ tag.name }}
                </UBadge>
              </div>
            </div>

            <div v-if="hasArtists">
              <div
                class="text-muted-foreground mb-2 text-sm font-medium tracking-wide uppercase"
              >
                {{ $t('page.mediums.info.artists') }}
              </div>
              <div class="flex flex-wrap gap-2">
                <UBadge
                  v-for="artist in medium.artists"
                  :key="artist.id"
                  size="sm"
                  color="success"
                >
                  {{ artist.name }}
                </UBadge>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Series List Section -->
      <div class="mt-16 flex flex-1 flex-col gap-6">
        <h2 class="text-2xl font-bold">
          {{ $t('page.mediums.detail.seriesContent') }}
          <span class="text-muted-foreground ml-2 text-lg font-normal">
            ({{ seriesList.length }})
          </span>
        </h2>

        <div
          v-if="seriesLoading"
          class="text-muted-foreground py-20 text-center"
        >
          {{ $t('common.loading') }}
        </div>
        <div
          v-else-if="seriesList.length === 0"
          class="text-muted-foreground py-20 text-center"
        >
          {{ $t('common.noData') }}
        </div>
        <VueDraggable
          v-else
          v-model="seriesList"
          :animation="150"
          ghost-class="ghost"
          class="grid grid-cols-2 gap-4 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6"
          @end="onSort"
        >
          <MediumGridItem
            v-for="item in seriesList"
            :key="item.id"
            :model-value="item"
            :fluid="true"
          />
        </VueDraggable>
      </div>
    </div>
  </div>
</template>

<style scoped>
.ghost {
  opacity: 0.5;
  background: hsl(var(--muted));
  border-radius: var(--ui-radius);
}
</style>
