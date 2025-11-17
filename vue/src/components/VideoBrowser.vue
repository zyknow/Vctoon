<template>
  <div class="relative" style="width: 100%; height: 100%">
    <div
      ref="container"
      :style="{
        width: '100%',
        height: '100%',
        '--player-primary':
          'var(--ui-primary, var(--color-primary, var(--primary, #3b82f6)))',
      }"
    ></div>
  </div>
</template>

<script setup lang="ts">
import { computed, onBeforeUnmount, onMounted, ref, watch } from 'vue'
import { useEventListener } from '@vueuse/core'
import DPlayer, { DPlayerEvents } from 'dplayer'
import { useI18n } from 'vue-i18n'

import { MediumType } from '@/api/http/library'
import { mediumResourceApi } from '@/api/http/medium-resource'
import { Video, videoApi, VideoGetListOutput } from '@/api/http/video'

const props = defineProps<{
  video: VideoGetListOutput | Video
  autoplay?: boolean
  theme?: string
  loop?: boolean
  lang?: 'zh-cn' | 'en' | string
  type?: 'auto' | 'hls' | 'flv' | 'dash'
  preferWeb?: boolean
  onToggleWide?: (wide: boolean) => void
}>()

const { locale } = useI18n()
const dpLang = computed(
  () =>
    props.lang ??
    (String(locale.value).toLowerCase().startsWith('zh') ? 'zh-cn' : 'en'),
)

const url = computed(() => videoApi.getVideoUrl(props.video.id))

const dpType = computed<'auto' | 'hls' | 'flv' | 'dash'>(() => {
  return 'hls'
  // if (props.type) return props.type
  // const u = (url.value || '').toLowerCase()
  // if (u.endsWith('.m3u8')) return 'hls'
  // if (u.endsWith('.flv')) return 'flv'
  // if (u.endsWith('.mpd')) return 'dash'
  // return 'auto'
})

const container = ref<HTMLDivElement | null>(null)
let dp: DPlayer | null = null
let wideBtnInjected = false
let lastSavedProgress = -1
let progressDirty = false

const getProgress = () => {
  const current = (dp?.video?.currentTime ?? 0) as number
  const duration = (dp?.video?.duration ?? 0) as number
  if (
    !Number.isFinite(current) ||
    !Number.isFinite(duration) ||
    duration <= 0
  ) {
    return 0
  }
  const value = Math.min(Math.max(current / duration, 0), 1)
  return Number.parseFloat(value.toFixed(4))
}

const saveProgress = async () => {
  if (!progressDirty) return
  const id = props.video.id
  if (!id) return
  const progress = getProgress()
  if (!Number.isFinite(progress)) return
  if (
    progress >= 0 &&
    lastSavedProgress >= 0 &&
    Math.abs(progress - lastSavedProgress) < 0.001
  ) {
    return
  }
  try {
    await mediumResourceApi.updateReadingProcess([
      {
        mediumId: id,
        mediumType: MediumType.Video,
        progress,
        readingLastTime: new Date().toISOString(),
      },
    ])
    lastSavedProgress = progress
    progressDirty = false
  } catch {
    // ignore
  }
}

defineExpose({ saveProgress })

const resumePlayback = () => {
  const player = dp
  const saved = lastSavedProgress
  const video = player?.video as HTMLVideoElement | undefined
  if (!player || !video) return
  const duration = video.duration
  if (!Number.isFinite(duration) || duration <= 0) return
  const p = Math.min(Math.max(saved, 0), 1)
  if (p <= 0) return
  const target = Math.min(Math.max(duration * p, 0), duration * 0.995)
  if (!Number.isFinite(target) || target <= 0) return
  try {
    player.seek(target)
  } catch {
    video.currentTime = target
  }
}

function init() {
  if (!container.value || !url.value) return
  dp = new DPlayer({
    container: container.value,
    autoplay: props.autoplay ?? true,
    theme: props.theme,
    loop: props.loop,
    hotkey: false,
    lang: dpLang.value as any,
    video: {
      url: url.value,
      pic: mediumResourceApi.getCoverUrl(props.video.cover!),
      type: dpType.value,
    },
    customType: {
      hls: async (video: HTMLVideoElement) => {
        try {
          const mod = await import('hls.js')
          const Hls = (mod as any).default || (mod as any)
          if (Hls.isSupported()) {
            const hls = new Hls()
            const src = (video as any).src || url.value
            hls.loadSource(src)
            hls.attachMedia(video)
          } else {
            const src = (video as any).src || url.value
            ;(video as any).src = src
          }
        } catch {
          const src = (video as any).src || url.value
          ;(video as any).src = src
        }
      },
      flv: async (video: HTMLVideoElement) => {
        try {
          const mod = await import('flv.js')
          const flvjs = (mod as any).default || (mod as any)
          if (flvjs.isSupported()) {
            const player = flvjs.createPlayer({ type: 'flv', url: url.value })
            player.attachMediaElement(video)
            player.load()
          } else {
            ;(video as any).src = url.value
          }
        } catch {
          ;(video as any).src = url.value
        }
      },
      dash: async (video: HTMLVideoElement) => {
        try {
          const mod = await import('dashjs')
          const dashjs = (mod as any).default || (mod as any)
          const player = dashjs.MediaPlayer().create()
          player.initialize(video, url.value, true)
        } catch {
          ;(video as any).src = url.value
        }
      },
    },
  })
  injectWideButton()
  lastSavedProgress = props.video.readingProgress ?? 0
  progressDirty = false
  dp.on(DPlayerEvents.timeupdate, () => {
    progressDirty = true
  })
  dp.on(DPlayerEvents.pause, () => {
    void saveProgress()
  })
  dp.on(DPlayerEvents.ended, () => {
    progressDirty = true
    void saveProgress()
  })
  if (dp?.video) {
    if (Number.isFinite(dp.video.duration) && dp.video.duration > 0) {
      resumePlayback()
    } else {
      dp.video.addEventListener(
        'loadedmetadata',
        () => {
          resumePlayback()
        },
        { once: true },
      )
    }
  }
}

function injectWideButton() {
  if (wideBtnInjected || !container.value) return
  const right = container.value.querySelector(
    '.dplayer-icons.dplayer-icons-right',
  ) as HTMLElement | null
  if (!right) return
  const btn = document.createElement('button')
  btn.className = 'dplayer-icon dplayer-wide-toggle'
  btn.setAttribute('data-balloon', '宽屏')
  btn.setAttribute('data-balloon-pos', 'up')
  const span = document.createElement('span')
  span.className = 'dplayer-icon-content'
  span.innerHTML =
    '<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 32 32"><path d="M12 4H4v8h2V6h6V4zm16 0h-8v2h6v6h2V4zM6 18H4v10h10v-2H6V18zm22 0h-2v8h-8v2h10V18z"/></svg>'
  btn.appendChild(span)
  let wide = false
  const update = () => {
    btn.setAttribute('data-balloon', wide ? '退出宽屏' : '宽屏')
  }
  btn.addEventListener('click', () => {
    wide = !wide
    update()
    if (props.onToggleWide) props.onToggleWide(wide)
  })
  right.appendChild(btn)
  wideBtnInjected = true
  update()
}

onMounted(() => {
  init()
})

watch(
  () => props.video.id,
  () => {
    if (!url.value) return
    if (!dp && container.value) {
      init()
      return
    }
    if (dp) {
      dp.switchVideo({ url: url.value }, undefined as any)
      if (props.autoplay ?? true) {
        dp.play()
      }
      lastSavedProgress = props.video.readingProgress ?? 0
      progressDirty = false
      if (dp.video) {
        dp.video.addEventListener(
          'loadedmetadata',
          () => {
            resumePlayback()
          },
          { once: true },
        )
      }
    }
  },
)

onBeforeUnmount(() => {
  void saveProgress()
  if (dp) {
    dp.destroy()
    dp = null
  }
})

useEventListener(window, 'visibilitychange', () => {
  if (document.visibilityState === 'hidden') {
    void saveProgress()
  }
})

useEventListener(window, 'pagehide', () => {
  void saveProgress()
})

useEventListener(window, 'beforeunload', () => {
  void saveProgress()
})

useEventListener(window, 'blur', () => {
  void saveProgress()
})
</script>
<style scoped>
:deep(.dplayer-bezel) {
  display: none;
}
:deep(.dplayer-notice-list) {
  display: none !important;
}
:deep(.dplayer-bar-time) {
  display: none !important;
}
:deep(.dplayer-played) {
  background: var(--player-primary, #22c55e) !important;
}
:deep(.dplayer-thumb) {
  display: block !important;
  background: var(--player-primary, #22c55e) !important;
  width: 12px;
  height: 12px;
  border-radius: 9999px;
}
</style>
