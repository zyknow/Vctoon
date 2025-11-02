<script setup lang="ts">
import { computed, onMounted, reactive, ref, watch } from 'vue'

import type { SettingGroup } from '@/api/http/setting'
import { settingUiApi } from '@/api/http/setting'
import { useIsMobile } from '@/hooks/useIsMobile'
import { $t } from '@/locales/i18n'
import { useAbpStore } from '@/stores/abp'

defineOptions({ name: 'SettingPage' })

// 表单名称前缀，所有设置项的key都需要带此前缀
const formNamePrefix = 'Setting_'

// 辅助函数：为设置项名称添加前缀
function addPrefix(name: string): string {
  return `${formNamePrefix}${name}`
}

// 辅助函数：从带前缀的名称中移除前缀
function removePrefix(nameWithPrefix: string): string {
  return nameWithPrefix.startsWith(formNamePrefix)
    ? nameWithPrefix.slice(formNamePrefix.length)
    : nameWithPrefix
}

const abpStore = useAbpStore()
const toast = useToast()
const { isMobile } = useIsMobile()

// 自定义设置项配置
const customSettings = {
  'Abp.Localization.DefaultLanguage': {
    type: 'select',
    options: computed(() => {
      return (
        abpStore.application?.localization?.languages?.map((lang) => ({
          label: lang.displayName,
          value: lang.cultureName,
        })) || []
      )
    }),
  },
  'Abp.Timing.TimeZone': {
    type: 'select',
    options: computed(() => [
      { label: 'UTC (+0:00)', value: 'UTC' },
      { label: 'Asia/Shanghai (中国标准时间 +8:00)', value: 'Asia/Shanghai' },
      { label: 'Asia/Tokyo (日本标准时间 +9:00)', value: 'Asia/Tokyo' },
      { label: 'Asia/Seoul (韩国标准时间 +9:00)', value: 'Asia/Seoul' },
      { label: 'Asia/Hong_Kong (香港时间 +8:00)', value: 'Asia/Hong_Kong' },
      { label: 'Asia/Taipei (台北时间 +8:00)', value: 'Asia/Taipei' },
      { label: 'Asia/Singapore (新加坡时间 +8:00)', value: 'Asia/Singapore' },
      { label: 'Asia/Bangkok (泰国时间 +7:00)', value: 'Asia/Bangkok' },
      { label: 'Asia/Jakarta (印尼时间 +7:00)', value: 'Asia/Jakarta' },
      { label: 'Asia/Kolkata (印度时间 +5:30)', value: 'Asia/Kolkata' },
      { label: 'Asia/Dubai (阿联酋时间 +4:00)', value: 'Asia/Dubai' },
      { label: 'Europe/London (英国时间 +0:00/+1:00)', value: 'Europe/London' },
      { label: 'Europe/Paris (法国时间 +1:00/+2:00)', value: 'Europe/Paris' },
      { label: 'Europe/Berlin (德国时间 +1:00/+2:00)', value: 'Europe/Berlin' },
      { label: 'Europe/Rome (意大利时间 +1:00/+2:00)', value: 'Europe/Rome' },
      { label: 'Europe/Moscow (俄罗斯时间 +3:00)', value: 'Europe/Moscow' },
      {
        label: 'America/New_York (美国东部时间 -5:00/-4:00)',
        value: 'America/New_York',
      },
      {
        label: 'America/Chicago (美国中部时间 -6:00/-5:00)',
        value: 'America/Chicago',
      },
      {
        label: 'America/Denver (美国山地时间 -7:00/-6:00)',
        value: 'America/Denver',
      },
      {
        label: 'America/Los_Angeles (美国西部时间 -8:00/-7:00)',
        value: 'America/Los_Angeles',
      },
      {
        label: 'America/Toronto (加拿大东部时间 -5:00/-4:00)',
        value: 'America/Toronto',
      },
      {
        label: 'America/Vancouver (加拿大西部时间 -8:00/-7:00)',
        value: 'America/Vancouver',
      },
      {
        label: 'America/Sao_Paulo (巴西时间 -3:00)',
        value: 'America/Sao_Paulo',
      },
      {
        label: 'Australia/Sydney (澳大利亚东部时间 +10:00/+11:00)',
        value: 'Australia/Sydney',
      },
      {
        label: 'Australia/Melbourne (澳大利亚东南时间 +10:00/+11:00)',
        value: 'Australia/Melbourne',
      },
      {
        label: 'Australia/Perth (澳大利亚西部时间 +8:00)',
        value: 'Australia/Perth',
      },
      {
        label: 'Pacific/Auckland (新西兰时间 +12:00/+13:00)',
        value: 'Pacific/Auckland',
      },
    ]),
  },
  // 身份认证相关设置
  'Abp.Identity.Password.ForceUsersToPeriodicallyChangePassword': {
    type: 'checkbox',
  },
  'Abp.Identity.Password.PasswordChangePeriodDays': {
    type: 'number',
  },
  'Abp.Identity.SignIn.RequireEmailVerificationToRegister': {
    type: 'checkbox',
  },
}

// 检查是否为自定义设置项
function isCustomSetting(name: string): boolean {
  return name in customSettings
}

// 获取自定义设置项配置
function getCustomSetting(name: string) {
  return customSettings[name as keyof typeof customSettings]
}

const loading = ref(false)
const submitting = ref(false)
const groups = ref<SettingGroup[]>([])
const activeGroupIndex = ref(0)
const activeGroup = computed(() => groups.value[activeGroupIndex.value])
// 移动端视图状态：'list' 显示分组列表，'detail' 显示分组详情
const mobileView = ref<'list' | 'detail'>('list')

// 监听移动端变化，切换到桌面端时重置视图
watch(isMobile, (newVal) => {
  if (!newVal) {
    mobileView.value = 'list'
  }
})

function setActiveGroup(idx: number) {
  if (idx < 0 || idx >= groups.value.length) return
  activeGroupIndex.value = idx
  // 移动端点击后切换到详情视图
  if (isMobile.value) {
    mobileView.value = 'detail'
  }
}

function backToList() {
  mobileView.value = 'list'
}

// 二级分组（Group2）状态：依据 SettingInfo.properties 中的子分组键进行归类
const subgroupKeyOrder = ['Group2', 'SubGroup', 'Category', 'Section'] as const

// 获取当前分组下的所有子分组及其设置项
const groupedInfos = computed(() => {
  const g = activeGroup.value
  const groups = new Map<string, any[]>()

  for (const info of g?.settingInfos ?? []) {
    const props = (info as any)?.properties ?? {}
    let name = ''
    for (const k of subgroupKeyOrder) {
      const v = props?.[k]
      if (typeof v === 'string' && v.trim().length > 0) {
        name = v
        break
      }
    }
    const resolved = name || 'General'

    if (!groups.has(resolved)) {
      groups.set(resolved, [])
    }
    groups.get(resolved)!.push(info)
  }

  return [...groups.entries()].map(([name, infos]) => ({
    name,
    infos,
  }))
})

// 当前值与初始值映射（键为 setting name）
const valuesMap = reactive<Record<string, string>>({})
const initialValuesMap = reactive<Record<string, string>>({})
const changedNames = ref<Set<string>>(new Set())

const totalChangedCount = computed(() => changedNames.value.size)

function markChanged(name: string) {
  const curr = valuesMap[name] ?? ''
  const init = initialValuesMap[name] ?? ''
  if (curr === init) changedNames.value.delete(name)
  else changedNames.value.add(name)
}

async function load() {
  loading.value = true
  try {
    const result = await settingUiApi.get()
    groups.value = Array.isArray(result) ? result : []
    // 初始化映射
    Object.keys(valuesMap).forEach((k) => delete valuesMap[k])
    Object.keys(initialValuesMap).forEach((k) => delete initialValuesMap[k])
    changedNames.value.clear()
    for (const group of groups.value) {
      for (const info of group.settingInfos ?? []) {
        const rawName = info.name ?? ''
        let value = info.value ?? ''
        if (rawName.length === 0) continue

        // 移除可能存在的前缀，确保内部使用统一的无前缀名称
        const name = removePrefix(rawName)
        if (name.length === 0) continue

        const type = getType(info)
        // 规范化复选类型的值为 'True'/'False'
        if (type === 'checkbox') {
          value = normalizeCheckboxValue(value)
        }
        valuesMap[name] = value
        initialValuesMap[name] = value
      }
    }
    // 默认激活第一个分组
    activeGroupIndex.value = groups.value.length > 0 ? 0 : 0
  } catch (error) {
    toast.add({
      title: $t('page.setting.messages.loadError'),
      color: 'error',
    })
    console.error('[Setting] load error', error)
  } finally {
    loading.value = false
  }
}

async function saveChanges() {
  if (changedNames.value.size === 0) {
    toast.add({
      title: $t('page.setting.messages.saveNoChange'),
      color: 'info',
    })
    return
  }
  const input: Record<string, string> = {}
  for (const name of changedNames.value) {
    // 为每个key添加Setting_前缀
    const keyWithPrefix = addPrefix(name)
    input[keyWithPrefix] = valuesMap[name] ?? ''
  }

  submitting.value = true
  try {
    await settingUiApi.setValues(input)
    // 保存成功后更新初始值并清空变更集合
    for (const name of changedNames.value) {
      initialValuesMap[name] = valuesMap[name] ?? ''
    }
    changedNames.value.clear()
    toast.add({
      title: $t('page.setting.messages.saveSuccess'),
      color: 'success',
    })
  } catch (error) {
    toast.add({
      title: $t('page.setting.messages.saveError'),
      color: 'error',
    })
    console.error('[Setting] save error', error)
  } finally {
    await abpStore.loadAbpApplicationConfiguration()
    submitting.value = false
  }
}

// 根据返回的 properties.Type 选择组件类型
function getType(info: any): 'checkbox' | 'number' | 'text' {
  const t = String(info?.properties?.Type ?? '').toLowerCase()
  if (t === 'number' || t === 'checkbox') return t
  return 'text'
}

// 将布尔字符串统一为后端常用的 'True'/'False'
function normalizeCheckboxValue(value: string) {
  const str = String(value ?? '')
  const v = str.trim().toLowerCase()
  if (v === 'true') return 'True'
  if (v === 'false') return 'False'
  return value || 'False'
}

onMounted(() => {
  void load()
})

// 渲染层将字符串安全转换为数字
function toNumber(value: unknown) {
  if (value === null || value === undefined || value === '') return undefined
  const num = Number(value)
  return Number.isNaN(num) ? undefined : num
}

// 统一获取设置项在 map 中的键名
function getSettingName(info: any): string {
  const raw = String(info?.name ?? '')
  return removePrefix(raw)
}

// 自定义下拉选项类型
interface SelectOption {
  label: string
  value: string
}

// 安全获取自定义设置项的下拉选项
function getCustomOptions(nameRaw: string): SelectOption[] {
  const name = removePrefix(String(nameRaw ?? ''))
  const setting = getCustomSetting(name) as
    | undefined
    | { options?: SelectOption[] | { value: SelectOption[] } }
  if (!setting) return []
  const opts = setting.options as any
  if (Array.isArray(opts)) return opts as SelectOption[]
  return Array.isArray(opts?.value) ? (opts.value as SelectOption[]) : []
}

// 统一数字更新
function updateNumberValue(info: any, val: null | number | undefined) {
  const name = getSettingName(info)
  valuesMap[name] = val === null || val === undefined ? '' : String(val)
  markChanged(name)
}

// 判断自定义类型
function isCustomType(
  info: any,
  type: 'checkbox' | 'number' | 'select',
): boolean {
  const t = getCustomSetting(getSettingName(info))?.type
  return t === type
}

// 获取自定义选项
function customOptionsOf(info: any): SelectOption[] {
  return getCustomOptions(getSettingName(info))
}
</script>

<template>
  <div :class="isMobile ? 'p-4' : 'flex h-[calc(100vh-64px)] flex-col p-4'">
    <!-- 顶部操作栏 -->
    <UCard :class="isMobile ? 'mb-4' : 'mb-4 shrink-0'">
      <div class="flex flex-wrap items-center gap-2">
        <!-- 移动端返回按钮 -->
        <UButton
          v-if="isMobile && mobileView === 'detail'"
          variant="ghost"
          icon="i-lucide-arrow-left"
          size="sm"
          @click="backToList"
        >
          {{ $t('common.back') }}
        </UButton>
        <h2 class="text-foreground text-xl font-semibold">
          {{ $t('page.setting.title') }}
        </h2>
        <div class="flex-1"></div>
        <UButton
          :loading="loading"
          :disabled="submitting"
          variant="ghost"
          icon="i-lucide-refresh-cw"
          size="sm"
          @click="load"
        >
          <span v-if="!isMobile">{{ $t('page.setting.actions.refresh') }}</span>
        </UButton>
        <UButton
          :loading="submitting"
          :disabled="totalChangedCount === 0"
          icon="i-lucide-save"
          size="sm"
          @click="saveChanges"
        >
          <span v-if="!isMobile">{{ $t('page.setting.actions.save') }}</span>
          <span v-if="totalChangedCount > 0" class="ml-1">
            ({{ totalChangedCount }})
          </span>
        </UButton>
      </div>
    </UCard>

    <!-- 主要内容区域 -->
    <div
      class="flex gap-4"
      :class="isMobile ? 'flex-col' : 'min-h-0 flex-1 overflow-hidden'"
    >
      <!-- 左侧导航 -->
      <div
        v-show="!isMobile || mobileView === 'list'"
        :class="isMobile ? 'w-full' : 'w-80 shrink-0 overflow-y-auto'"
      >
        <UCard>
          <div
            v-if="groups.length === 0 && !loading"
            class="text-muted-foreground py-8 text-center"
          >
            {{ $t('page.setting.empty') }}
          </div>
          <div v-else class="space-y-2">
            <div v-if="loading" class="flex items-center justify-center py-8">
              <UIcon
                name="i-lucide-loader-2"
                class="text-primary h-6 w-6 animate-spin"
              />
            </div>
            <template v-else>
              <UButton
                v-for="(group, idx) in groups"
                :key="group.groupName || idx"
                :variant="idx === activeGroupIndex ? 'solid' : 'ghost'"
                class="w-full justify-start"
                @click="setActiveGroup(idx)"
              >
                {{ group.groupDisplayName || group.groupName }}
              </UButton>
            </template>
          </div>
        </UCard>
      </div>

      <!-- 右侧内容区域 -->
      <div
        v-show="!isMobile || mobileView === 'detail'"
        :class="isMobile ? 'w-full' : 'min-w-0 flex-1 overflow-y-auto'"
      >
        <div v-if="activeGroup" class="space-y-6 p-1">
          <!-- 按子分组显示内容 -->
          <div
            v-for="group in groupedInfos"
            :key="group.name"
            class="space-y-4"
          >
            <!-- 子分组标题 -->
            <div
              class="bg-primary/10 border-primary/20 rounded-lg border px-4 py-3"
            >
              <h3
                class="text-primary flex items-center gap-2 text-lg font-semibold"
              >
                <span class="bg-primary/20 h-2 w-2 rounded-full"></span>
                {{ group.name }}
              </h3>
            </div>

            <!-- 子分组内容 -->
            <UCard>
              <div class="space-y-6">
                <div
                  v-for="(info, index) in group.infos"
                  :key="info.name"
                  class="space-y-3"
                >
                  <!-- 标签 -->
                  <div class="flex items-start justify-between gap-4">
                    <div class="min-w-0 flex-1 space-y-1">
                      <label class="text-foreground text-sm font-medium">
                        {{ info.displayName || info.name }}
                      </label>
                      <p
                        v-if="info.description"
                        class="text-muted-foreground text-sm leading-relaxed"
                      >
                        {{ info.description }}
                      </p>
                    </div>

                    <!-- 控件区域 -->
                    <div class="flex items-center gap-2">
                      <!-- 自定义组件渲染 -->
                      <template v-if="isCustomSetting(info.name ?? '')">
                        <!-- 自定义 Select 下拉选择器 -->
                        <USelectMenu
                          v-if="isCustomType(info, 'select')"
                          v-model="valuesMap[getSettingName(info)]"
                          :items="customOptionsOf(info)"
                          value-key="value"
                          label-key="label"
                          class="w-64"
                          @update:model-value="
                            markChanged(getSettingName(info))
                          "
                        />

                        <!-- 自定义 Checkbox -->
                        <USwitch
                          v-else-if="isCustomType(info, 'checkbox')"
                          :model-value="
                            valuesMap[getSettingName(info)] === 'True'
                          "
                          @update:model-value="
                            (value: boolean) => {
                              const key = getSettingName(info)
                              valuesMap[key] = value ? 'True' : 'False'
                              markChanged(key)
                            }
                          "
                        />

                        <!-- 自定义 Number 数字输入 -->
                        <UInput
                          v-else-if="isCustomType(info, 'number')"
                          :model-value="
                            toNumber(valuesMap[getSettingName(info)])
                          "
                          type="number"
                          class="w-32"
                          @update:model-value="
                            (value: any) => {
                              const key = getSettingName(info)
                              valuesMap[key] =
                                value === null || value === undefined
                                  ? ''
                                  : String(value)
                              markChanged(key)
                            }
                          "
                        />
                      </template>

                      <!-- 标准组件渲染 -->
                      <template v-else>
                        <!-- 复选框/开关 -->
                        <USwitch
                          v-if="getType(info) === 'checkbox'"
                          :model-value="
                            valuesMap[getSettingName(info)] === 'True'
                          "
                          @update:model-value="
                            (value: boolean) => {
                              const key = getSettingName(info)
                              valuesMap[key] = value ? 'True' : 'False'
                              markChanged(key)
                            }
                          "
                        />

                        <!-- 数字输入 -->
                        <UInput
                          v-else-if="getType(info) === 'number'"
                          :model-value="
                            toNumber(valuesMap[getSettingName(info)])
                          "
                          type="number"
                          class="w-32"
                          @update:model-value="updateNumberValue(info, $event)"
                        />

                        <!-- 文本输入 -->
                        <UInput
                          v-else
                          v-model="valuesMap[getSettingName(info)]"
                          class="w-64"
                          @input="markChanged(getSettingName(info))"
                        />
                      </template>
                    </div>
                  </div>
                  <USeparator v-if="index < group.infos.length - 1" />
                </div>
              </div>
            </UCard>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
