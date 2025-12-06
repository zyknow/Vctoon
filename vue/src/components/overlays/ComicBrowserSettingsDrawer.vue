<script setup lang="ts">
import { computed, reactive, watch } from 'vue'

import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'
import { useComicStore } from '@/stores/comic'

import type {
  ComicQualityPreset,
  ComicViewerSettings,
  DisplayMode,
  ReadingDirection,
  ZoomMode,
} from './types'
import { DEFAULT_COMIC_PRELOAD_COUNTS, isVerticalDirection } from './types'

defineOptions({
  name: 'ComicSettingsDrawer',
})

const props = withDefaults(
  defineProps<{
    modelValue: boolean
    portal?: boolean | string
  }>(),
  {
    portal: true,
  },
)

const emit = defineEmits<{
  (event: 'update:modelValue', value: boolean): void
}>()

const { isMobile } = useIsMobile()

const comicStore = useComicStore()
const settings = comicStore.settings

const drawerVisible = computed({
  get: () => props.modelValue,
  set: (value: boolean) => emit('update:modelValue', value),
})

const form = reactive<ComicViewerSettings>({ ...settings })

let syncingFromStore = false

watch(
  settings,
  (value) => {
    syncingFromStore = true
    Object.assign(form, value)
    syncingFromStore = false
  },
  { deep: true },
)

watch(
  form,
  (value) => {
    if (syncingFromStore) {
      return
    }
    comicStore.setSettings({ ...value })
  },
  { deep: true },
)

const drawerDirection = computed(() => (isMobile.value ? 'bottom' : 'right'))

type OptionItem<T extends string> = {
  description: string
  label: string
  value: T
}

const createOption = <T extends string>(
  value: T,
  labelKey: string,
  descriptionKey: string,
): OptionItem<T> => ({
  value,
  label: $t(labelKey),
  description: $t(descriptionKey),
})

const readingDirectionOptions = computed<OptionItem<ReadingDirection>[]>(() => [
  createOption(
    'ltr',
    'page.comic.settings.options.direction.ltr',
    'page.comic.settings.options.direction.ltrDescription',
  ),
  createOption(
    'rtl',
    'page.comic.settings.options.direction.rtl',
    'page.comic.settings.options.direction.rtlDescription',
  ),
  createOption(
    'ttb',
    'page.comic.settings.options.direction.ttb',
    'page.comic.settings.options.direction.ttbDescription',
  ),
  createOption(
    'btt',
    'page.comic.settings.options.direction.btt',
    'page.comic.settings.options.direction.bttDescription',
  ),
])

const displayModeOptions = computed<OptionItem<DisplayMode>[]>(() => [
  createOption(
    'single',
    'page.comic.settings.options.display.single',
    'page.comic.settings.options.display.singleDescription',
  ),
  createOption(
    'double',
    'page.comic.settings.options.display.double',
    'page.comic.settings.options.display.doubleDescription',
  ),
  createOption(
    'scroll',
    'page.comic.settings.options.display.scroll',
    'page.comic.settings.options.display.scrollDescription',
  ),
])

const zoomModeOptions = computed<OptionItem<ZoomMode>[]>(() => [
  createOption(
    'fit-screen',
    'page.comic.settings.options.zoom.fitScreen',
    'page.comic.settings.options.zoom.fitScreenDescription',
  ),
  createOption(
    'fit-height',
    'page.comic.settings.options.zoom.fitHeight',
    'page.comic.settings.options.zoom.fitHeightDescription',
  ),
  createOption(
    'fit-width',
    'page.comic.settings.options.zoom.fitWidth',
    'page.comic.settings.options.zoom.fitWidthDescription',
  ),
  createOption(
    'original',
    'page.comic.settings.options.zoom.original',
    'page.comic.settings.options.zoom.originalDescription',
  ),
])

const isScrollMode = computed(() => form.displayMode === 'scroll')
const isHorizontalOrientation = computed(
  () => !isVerticalDirection(form.readingDirection),
)

const availableZoomOptions = computed(() => {
  if (isScrollMode.value && isHorizontalOrientation.value) {
    return zoomModeOptions.value.filter((item) => item.value === 'fit-height')
  }
  return zoomModeOptions.value
})

watch(
  [isScrollMode, isHorizontalOrientation],
  ([scroll, horizontal]) => {
    if (scroll && horizontal && form.zoomMode !== 'fit-height') {
      form.zoomMode = 'fit-height'
    }
  },
  { immediate: true },
)

const qualityOptions = computed<OptionItem<ComicQualityPreset>[]>(() => [
  createOption(
    '480p',
    'page.comic.settings.options.quality.quality480p',
    'page.comic.settings.options.quality.quality480pDescription',
  ),
  createOption(
    '720p',
    'page.comic.settings.options.quality.quality720p',
    'page.comic.settings.options.quality.quality720pDescription',
  ),
  createOption(
    '1080p',
    'page.comic.settings.options.quality.quality1080p',
    'page.comic.settings.options.quality.quality1080pDescription',
  ),
  createOption(
    'original',
    'page.comic.settings.options.quality.original',
    'page.comic.settings.options.quality.originalDescription',
  ),
  createOption(
    'custom',
    'page.comic.settings.options.quality.custom',
    'page.comic.settings.options.quality.customDescription',
  ),
])

const isCustomQuality = computed(() => form.qualityPreset === 'custom')

const preloadCountOptions = computed(() =>
  DEFAULT_COMIC_PRELOAD_COUNTS.map((count) => ({
    label: $t('page.comic.settings.options.preloadCount', { count }),
    value: count,
  })),
)
</script>

<template>
  <UDrawer
    v-model:open="drawerVisible"
    :direction="drawerDirection"
    :handle="false"
    :portal="portal"
    :ui="{ overlay: 'z-50', content: 'z-50' }"
  >
    <template #title>
      {{ $t('page.comic.settings.dialogTitle') }}
    </template>
    <template #description>
      {{ $t('page.comic.settings.dialogDescription') }}
    </template>

    <template #body>
      <div class="flex flex-col gap-6 p-4">
        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.readingMode') }}
          </h3>
          <div class="grid gap-4 md:grid-cols-2">
            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.readingDirection') }}
              </label>
              <USelect
                v-model="form.readingDirection"
                :items="readingDirectionOptions"
                value-key="value"
                class="w-full"
              />
            </div>

            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.displayMode') }}
              </label>
              <USelect
                v-model="form.displayMode"
                :items="displayModeOptions"
                value-key="value"
                class="w-full"
              />
            </div>
          </div>
        </section>

        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.image') }}
          </h3>
          <div class="grid gap-4">
            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.zoomMode') }}
              </label>
              <USelect
                v-model="form.zoomMode"
                :items="availableZoomOptions"
                value-key="value"
                class="w-full"
              />
            </div>

            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.qualityPreset') }}
              </label>
              <USelect
                v-model="form.qualityPreset"
                :items="qualityOptions"
                value-key="value"
                class="w-full"
              />
            </div>

            <div v-if="isCustomQuality" class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.customQualityWidth') }}
              </label>
              <UInput
                v-model.number="form.customQualityWidth"
                type="number"
                :min="256"
                :max="8192"
                :step="64"
                class="w-full"
              />
            </div>

            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.imageSpacing') }}
              </label>
              <div class="flex w-full items-center gap-4">
                <USlider
                  v-model="form.imageSpacing"
                  :min="0"
                  :max="96"
                  :step="2"
                  class="flex-1"
                />
                <UInput
                  v-model.number="form.imageSpacing"
                  type="number"
                  :min="0"
                  :max="96"
                  :step="2"
                  class="w-20"
                />
              </div>
            </div>

            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.preloadCount') }}
              </label>
              <USelect
                v-model="form.preloadCount"
                :items="preloadCountOptions"
                value-key="value"
                class="w-full"
              />
            </div>
          </div>
        </section>

        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.experience') }}
          </h3>
          <div class="grid gap-4 sm:grid-cols-2">
            <USwitch
              v-model="form.pageTransition"
              :label="$t('page.comic.settings.fields.pageTransition')"
            />

            <USwitch
              v-model="form.alwaysFullscreen"
              :label="$t('page.comic.settings.fields.alwaysFullscreen')"
            />

            <div class="flex flex-col gap-2">
              <label class="text-sm font-medium">
                {{ $t('page.comic.settings.fields.backgroundColor') }}
              </label>
              <input
                v-model="form.backgroundColor"
                type="color"
                class="border-border h-10 w-20 cursor-pointer rounded-md border"
              />
            </div>
          </div>
        </section>
      </div>
    </template>
  </UDrawer>
</template>
