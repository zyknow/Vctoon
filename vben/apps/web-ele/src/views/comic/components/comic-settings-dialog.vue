<script setup lang="ts">
import type { FormInstance, FormRules } from 'element-plus'

import type {
  ComicQualityPreset,
  ComicViewerSettings,
  DisplayMode,
  ReadingDirection,
  ZoomMode,
} from '../types'

import { computed, reactive, ref } from 'vue'

import { useDialogContext } from '#/hooks/useDialogService'
import { $t } from '#/locales'

defineOptions({
  name: 'ComicSettingsDialog',
})

const props = defineProps<{ initialSettings: ComicViewerSettings }>()

const { close, resolve } = useDialogContext<ComicViewerSettings>()
const formRef = ref<FormInstance>()
const form = reactive<ComicViewerSettings>({ ...props.initialSettings })

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

const validateCustomQuality = (
  _rule: unknown,
  value: number,
  callback: (error?: Error) => void,
) => {
  if (!isCustomQuality.value) {
    callback()
    return
  }
  if (typeof value !== 'number' || Number.isNaN(value) || value < 256) {
    callback(new Error($t('page.comic.settings.validation.customQualityWidth')))
    return
  }
  callback()
}

const rules: FormRules<ComicViewerSettings> = {
  customQualityWidth: [
    {
      trigger: 'change',
      validator: validateCustomQuality,
    },
    {
      trigger: 'blur',
      validator: validateCustomQuality,
    },
  ],
}

const handleCancel = () => {
  close()
}

const handleConfirm = async () => {
  const formInstance = formRef.value
  if (formInstance) {
    const valid = await formInstance.validate().catch(() => false)
    if (!valid) {
      return
    }
  }
  const result: ComicViewerSettings = { ...form }
  resolve(result)
}
</script>

<template>
  <div class="flex flex-col gap-6">
    <el-form ref="formRef" :model="form" :rules="rules" label-width="120px">
      <section class="flex flex-col gap-4">
        <h3 class="text-base font-semibold">
          {{ $t('page.comic.settings.sections.readingMode') }}
        </h3>
        <el-form-item
          :label="$t('page.comic.settings.fields.readingDirection')"
        >
          <el-radio-group
            v-model="form.readingDirection"
            class="flex flex-wrap gap-2"
          >
            <el-radio-button
              v-for="item in readingDirectionOptions"
              :key="item.value"
              :label="item.value"
            >
              <span class="flex flex-col items-center gap-1">
                <span>{{ item.label }}</span>
                <span class="text-muted-foreground text-xs">
                  {{ item.description }}
                </span>
              </span>
            </el-radio-button>
          </el-radio-group>
        </el-form-item>
        <el-form-item :label="$t('page.comic.settings.fields.displayMode')">
          <el-radio-group
            v-model="form.displayMode"
            class="flex flex-wrap gap-2"
          >
            <el-radio-button
              v-for="item in displayModeOptions"
              :key="item.value"
              :label="item.value"
            >
              <span class="flex flex-col items-center gap-1">
                <span>{{ item.label }}</span>
                <span class="text-muted-foreground text-xs">
                  {{ item.description }}
                </span>
              </span>
            </el-radio-button>
          </el-radio-group>
        </el-form-item>
      </section>

      <el-divider />

      <section class="flex flex-col gap-4">
        <h3 class="text-base font-semibold">
          {{ $t('page.comic.settings.sections.image') }}
        </h3>
        <el-form-item :label="$t('page.comic.settings.fields.zoomMode')">
          <el-radio-group v-model="form.zoomMode" class="flex flex-wrap gap-2">
            <el-radio-button
              v-for="item in zoomModeOptions"
              :key="item.value"
              :label="item.value"
            >
              <span class="flex flex-col items-center gap-1">
                <span>{{ item.label }}</span>
                <span class="text-muted-foreground text-xs">
                  {{ item.description }}
                </span>
              </span>
            </el-radio-button>
          </el-radio-group>
        </el-form-item>

        <el-form-item :label="$t('page.comic.settings.fields.qualityPreset')">
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
          prop="customQualityWidth"
        >
          <el-input-number
            v-model="form.customQualityWidth"
            :disabled="!isCustomQuality"
            :max="8192"
            :min="256"
            :step="64"
            class="w-full"
          />
        </el-form-item>
      </section>

      <el-divider />

      <section class="flex flex-col gap-4">
        <h3 class="text-base font-semibold">
          {{ $t('page.comic.settings.sections.experience') }}
        </h3>
        <el-form-item :label="$t('page.comic.settings.fields.pageTransition')">
          <el-switch v-model="form.pageTransition" />
        </el-form-item>
        <el-form-item :label="$t('page.comic.settings.fields.backgroundColor')">
          <el-color-picker v-model="form.backgroundColor" :show-alpha="false" />
        </el-form-item>
        <el-form-item
          :label="$t('page.comic.settings.fields.alwaysFullscreen')"
        >
          <el-switch v-model="form.alwaysFullscreen" />
        </el-form-item>
      </section>
    </el-form>

    <div class="flex justify-end gap-3">
      <el-button @click="handleCancel">
        {{ $t('common.cancel') }}
      </el-button>
      <el-button type="primary" @click="handleConfirm">
        {{ $t('common.confirm') }}
      </el-button>
    </div>
  </div>
</template>
