<script setup lang="ts">
import type {
  ComicQualityPreset,
  ComicViewerSettings,
  DisplayMode,
  ReadingDirection,
  ZoomMode,
} from '../types'

import { computed, reactive, watch } from 'vue'

import { useIsMobile } from '@vben/hooks'

import { $t } from '#/locales'

defineOptions({
  name: 'ComicSettingsDrawer',
})

const props = defineProps<{
  modelValue: boolean
  settings: ComicViewerSettings
}>()

const emit = defineEmits<{
  (event: 'update:modelValue', value: boolean): void
  (event: 'update:settings', value: ComicViewerSettings): void
}>()

const { isMobile } = useIsMobile()

const drawerVisible = computed({
  get: () => props.modelValue,
  set: (value: boolean) => emit('update:modelValue', value),
})

const form = reactive<ComicViewerSettings>({ ...props.settings })

let syncingFromParent = false

watch(
  () => props.settings,
  (value) => {
    syncingFromParent = true
    Object.assign(form, value)
    syncingFromParent = false
  },
  { deep: true },
)

watch(
  form,
  (value) => {
    if (syncingFromParent) {
      return
    }
    emit('update:settings', { ...value })
  },
  { deep: true },
)

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

const qualityOptions = computed<OptionItem<ComicQualityPreset>[]>(() => [
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
</script>

<template>
  <el-drawer
    v-model="drawerVisible"
    :append-to-body="true"
    :destroy-on-close="true"
    :modal="true"
    :size="isMobile ? '100%' : '420px'"
    :title="$t('page.comic.settings.dialogTitle')"
    direction="rtl"
  >
    <div class="flex flex-col gap-6">
      <el-form :model="form" label-position="top" class="flex flex-col gap-6">
        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.readingMode') }}
          </h3>
          <div class="grid gap-4 md:grid-cols-2">
            <el-form-item
              :label="$t('page.comic.settings.fields.readingDirection')"
              class="m-0"
            >
              <el-select v-model="form.readingDirection" class="w-full">
                <el-option
                  v-for="item in readingDirectionOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <div class="flex flex-col">
                    <span>{{ item.label }}</span>
                    <span class="text-muted-foreground text-xs">
                      {{ item.description }}
                    </span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>
            <el-form-item
              :label="$t('page.comic.settings.fields.displayMode')"
              class="m-0"
            >
              <el-select v-model="form.displayMode" class="w-full">
                <el-option
                  v-for="item in displayModeOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <div class="flex flex-col">
                    <span>{{ item.label }}</span>
                    <span class="text-muted-foreground text-xs">
                      {{ item.description }}
                    </span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>
          </div>
        </section>

        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.image') }}
          </h3>
          <div class="grid gap-4">
            <el-form-item :label="$t('page.comic.settings.fields.zoomMode')">
              <el-select v-model="form.zoomMode" class="w-full">
                <el-option
                  v-for="item in zoomModeOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <div class="flex flex-col">
                    <span>{{ item.label }}</span>
                    <span class="text-muted-foreground text-xs">
                      {{ item.description }}
                    </span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>

            <el-form-item
              :label="$t('page.comic.settings.fields.qualityPreset')"
            >
              <el-select v-model="form.qualityPreset" class="w-full">
                <el-option
                  v-for="item in qualityOptions"
                  :key="item.value"
                  :label="item.label"
                  :value="item.value"
                >
                  <div class="flex flex-col">
                    <span>{{ item.label }}</span>
                    <span class="text-muted-foreground text-xs">
                      {{ item.description }}
                    </span>
                  </div>
                </el-option>
              </el-select>
            </el-form-item>

            <el-form-item
              :label="$t('page.comic.settings.fields.customQualityWidth')"
            >
              <el-input-number
                v-model="form.customQualityWidth"
                :disabled="!isCustomQuality"
                :max="8192"
                :min="256"
                :step="64"
                class="w-full"
                controls-position="right"
              />
            </el-form-item>

            <el-form-item
              :label="$t('page.comic.settings.fields.imageSpacing')"
              class="mb-0"
            >
              <div class="flex w-full items-center gap-4">
                <el-slider
                  v-model="form.imageSpacing"
                  :min="0"
                  :max="96"
                  :step="2"
                  class="flex-1"
                />
                <el-input-number
                  v-model="form.imageSpacing"
                  :min="0"
                  :max="96"
                  :step="2"
                  size="small"
                />
              </div>
            </el-form-item>
          </div>
        </section>

        <section
          class="border-border/60 bg-background/70 rounded-xl border p-5 shadow-sm backdrop-blur"
        >
          <h3 class="mb-4 text-base font-semibold">
            {{ $t('page.comic.settings.sections.experience') }}
          </h3>
          <div class="grid gap-4 sm:grid-cols-2">
            <el-form-item
              :label="$t('page.comic.settings.fields.pageTransition')"
              class="m-0"
            >
              <el-switch v-model="form.pageTransition" />
            </el-form-item>
            <el-form-item
              :label="$t('page.comic.settings.fields.alwaysFullscreen')"
              class="m-0"
            >
              <el-switch v-model="form.alwaysFullscreen" />
            </el-form-item>
            <el-form-item
              :label="$t('page.comic.settings.fields.backgroundColor')"
              class="m-0"
            >
              <el-color-picker
                v-model="form.backgroundColor"
                :predefine="['#000000', '#0f172a', '#111827', '#1f2937']"
                :show-alpha="false"
              />
            </el-form-item>
          </div>
        </section>
      </el-form>
    </div>
  </el-drawer>
</template>
