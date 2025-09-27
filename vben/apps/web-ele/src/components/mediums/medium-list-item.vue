<script setup lang="ts">
import type { MediumGetListOutput } from '@vben/api'

import { computed } from 'vue'

import { mediumResourceApi, MediumType } from '@vben/api'
import { useAppConfig } from '@vben/hooks'
import { CiEditPencilLine01, CiMoreVertical, MdiPlayCircle } from '@vben/icons'
import { formatDate } from '@vben/utils'

const props = defineProps<{
  modelValue: MediumGetListOutput
}>()

const emit = defineEmits<{
  edit: [medium: MediumGetListOutput]
  updated: [medium: MediumGetListOutput]
}>()

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

const mediumType = computed(() => props.modelValue?.mediumType)

// 标题、年份、相对时间
const title = computed(() => props.modelValue?.title ?? '')
const year = computed(() => {
  const time = props.modelValue?.creationTime
  if (!time) return ''
  try {
    return formatDate(time, 'YYYY')
  } catch {
    return ''
  }
})
const timeAgo = computed(() => {
  const time =
    props.modelValue?.lastModificationTime || props.modelValue?.creationTime
  if (!time) return ''
  try {
    return formatDate(time, 'YYYY-MM-DD')
  } catch {
    return ''
  }
})

const cover = computed(() => {
  let mediumUrl

  switch (mediumType.value) {
    case MediumType.Comic: {
      mediumUrl = mediumResourceApi.url.getCover.format({
        cover: props.modelValue?.cover,
      })
      break
    }
    case MediumType.Video: {
      mediumUrl = mediumResourceApi.url.getCover.format({
        cover: props.modelValue?.cover,
      })
      break
    }
    default: {
      break
    }
  }

  return `${apiURL}${mediumUrl}`
})

// 编辑功能相关
const handleEdit = (event: MouseEvent) => {
  event.stopPropagation()
  emit('edit', props.modelValue)
}
</script>

<template>
  <!-- 列表项：横向布局，封面固定高，右侧信息区自适应 -->
  <div
    class="text-foreground hover:bg-muted/60 group relative flex w-full items-start gap-4 rounded-xl p-2 transition-colors"
  >
    <!-- Cover -->
    <div
      class="group-hover:border-primary relative overflow-hidden rounded-lg border border-transparent transition-colors"
    >
      <img
        v-if="cover"
        loading="lazy"
        :src="cover"
        alt="cover"
        class="h-24 w-16 rounded-lg object-cover"
      />
      <div v-else class="bg-muted h-24 w-16 rounded-lg"></div>

      <!-- Hover overlay on cover only -->
      <div
        class="pointer-events-none absolute inset-0 hidden items-center justify-center rounded-lg bg-black/40 group-hover:flex"
      >
        <MdiPlayCircle class="h-10 w-10 text-white drop-shadow" />
      </div>
    </div>

    <!-- Meta -->
    <div class="min-w-0 flex-1">
      <div class="line-clamp-1 text-base font-medium leading-tight">
        {{ title }}
      </div>
      <div
        class="text-muted-foreground mt-1 flex flex-wrap items-center gap-x-4 gap-y-1 text-xs"
      >
        <span>{{ year }}</span>
        <span>{{ timeAgo }}</span>
      </div>

      <!-- 行内操作 -->
      <div class="text-muted-foreground mt-2 flex items-center gap-3">
        <CiEditPencilLine01
          class="hover:text-primary h-4 w-4 cursor-pointer"
          @click="handleEdit"
        />
        <CiMoreVertical class="hover:text-primary h-4 w-4 cursor-pointer" />
      </div>
    </div>
  </div>
</template>

<style scoped>
.line-clamp-1 {
  display: -webkit-box;
  overflow: hidden;
  -webkit-line-clamp: 1;
  line-clamp: 1;
  -webkit-box-orient: vertical;
}
</style>
