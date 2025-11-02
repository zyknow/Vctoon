<script setup lang="ts">
import { computed } from 'vue'
import { useColorMode } from '@vueuse/core'
import { storeToRefs } from 'pinia'
import { useI18n } from 'vue-i18n'

import { SUPPORTED_LOCALES } from '@/locales/i18n'
import { usePreferenceStore } from '@/stores/preferences'

interface Props {
  /** 是否为最小化模式，true 时只显示 icon，false 时返回菜单项数组供外部使用 */
  min?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  min: false,
})

const { t } = useI18n()
const preferenceStore = usePreferenceStore()
const colorMode = useColorMode()
const { locale } = storeToRefs(preferenceStore)
const appConfig = useAppConfig()

const primaryColors = [
  'black',
  'red',
  'orange',
  'amber',
  'yellow',
  'lime',
  'green',
  'emerald',
  'teal',
  'cyan',
  'sky',
  'blue',
  'indigo',
  'violet',
  'purple',
  'fuchsia',
  'pink',
  'rose',
]

const neutralColors = ['slate', 'gray', 'zinc', 'neutral', 'stone']
// Primary 颜色配置
const primary = computed({
  get() {
    return appConfig.ui.colors.primary
  },
  set(color: string) {
    appConfig.ui.colors.primary = color
  },
})
// Neutral 颜色配置
const neutral = computed({
  get() {
    return appConfig.ui.colors.neutral
  },
  set(color: string) {
    appConfig.ui.colors.neutral = color
  },
})

// Radius 配置
const radiuses = [0, 0.125, 0.25, 0.375, 0.5]

const radius = computed({
  get() {
    return appConfig.ui.theme.radius
  },
  set(value: number) {
    appConfig.ui.theme.radius = value
  },
})

// 主题模式配置
const modes = [
  { label: 'light', icon: 'i-lucide-sun' },
  { label: 'dark', icon: 'i-lucide-moon' },
  { label: 'auto', icon: 'i-lucide-monitor' },
] as const

const mode = computed({
  get() {
    return colorMode.value
  },
  set(value: 'light' | 'dark' | 'auto') {
    colorMode.value = value
  },
})
</script>

<template>
  <!-- 最小化模式：使用 Popover 显示设置面板 -->
  <div v-if="props.min" class="flex items-center gap-1">
    <!-- 主题设置（包含颜色、Radius、Theme） -->
    <UPopover :ui="{ content: 'w-76 px-6 py-4 flex flex-col gap-4' }">
      <template #default="{ open }">
        <UButton
          icon="i-lucide-palette"
          color="neutral"
          :variant="open ? 'soft' : 'ghost'"
          square
          aria-label="Color picker"
        />
      </template>
      <template #content>
        <!-- Primary 主题色 -->
        <fieldset>
          <legend class="mb-2 text-[11px] leading-none font-semibold">
            {{ t('preferences.userMenu.theme.primary') }}
          </legend>
          <div class="-mx-2 grid grid-cols-3 gap-1">
            <ThemePickerButton
              v-for="color in primaryColors"
              :key="color"
              :label="color"
              :chip="color"
              :selected="primary === color"
              @click="primary = color"
            />
          </div>
        </fieldset>

        <!-- Neutral 中性色 -->
        <fieldset>
          <legend class="mb-2 text-[11px] leading-none font-semibold">
            {{ t('preferences.userMenu.theme.neutral') }}
          </legend>
          <div class="-mx-2 grid grid-cols-3 gap-1">
            <ThemePickerButton
              v-for="color in neutralColors"
              :key="color"
              :label="color"
              :chip="color === 'neutral' ? 'old-neutral' : color"
              :selected="neutral === color"
              @click="neutral = color"
            />
          </div>
        </fieldset>

        <!-- Radius 圆角 -->
        <fieldset>
          <legend class="mb-2 text-[11px] leading-none font-semibold">
            {{ t('preferences.userMenu.theme.radius') }}
          </legend>
          <div class="-mx-2 grid grid-cols-5 gap-1">
            <ThemePickerButton
              v-for="r in radiuses"
              :key="r"
              class="items-center justify-center"
              :label="String(r)"
              :selected="radius === r"
              @click="radius = r"
            />
          </div>
        </fieldset>

        <!-- Theme 主题模式 -->
        <fieldset>
          <legend class="mb-2 text-[11px] leading-none font-semibold">
            {{ t('preferences.userMenu.appearance') }}
          </legend>
          <div class="-mx-2 grid grid-cols-3 gap-1">
            <ThemePickerButton
              :label="t('preferences.themes.light')"
              :icon="modes[0].icon"
              :selected="mode === 'light'"
              @click="mode = 'light'"
            />
            <ThemePickerButton
              :label="t('preferences.themes.dark')"
              :icon="modes[1].icon"
              :selected="mode === 'dark'"
              @click="mode = 'dark'"
            />
            <ThemePickerButton
              :label="t('preferences.themes.system')"
              :icon="modes[2].icon"
              :selected="mode === 'auto'"
              @click="mode = 'auto'"
            />
          </div>
        </fieldset>
      </template>
    </UPopover>

    <!-- 语言切换 -->
    <UDropdownMenu
      :items="[
        SUPPORTED_LOCALES.map((localeOption) => ({
          label: localeOption.label,
          type: 'checkbox',
          checked: locale === localeOption.code,
          onSelect(event: Event) {
            event.preventDefault()
            preferenceStore.setLocale(localeOption.code)
          },
        })),
      ]"
      :content="{ align: 'end', collisionPadding: 12 }"
    >
      <UButton
        icon="i-lucide-languages"
        color="neutral"
        variant="ghost"
        square
        class="data-[state=open]:bg-elevated"
      />
    </UDropdownMenu>
  </div>
</template>
