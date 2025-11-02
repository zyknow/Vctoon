<script setup lang="ts">
import { memoryStore, Position } from './UScrollbar'

// 自实现的 Scrollbar 组件，遵循 Element Plus Scrollbar 的 API
// 不直接依赖或导入 element-plus 组件

defineOptions({ name: 'UScrollbar' })

interface Props {
  height?: string | number
  maxHeight?: string | number
  native?: boolean
  wrapStyle?: string | Record<string, string | number>
  wrapClass?: string
  viewStyle?: string | Record<string, string | number>
  viewClass?: string
  noresize?: boolean
  tag?: string
  always?: boolean
  minSize?: number
  id?: string
  role?: string
  ariaLabel?: string
  ariaOrientation?: 'horizontal' | 'vertical' | undefined
  tabindex?: number | string
  distance?: number
  /**
   * 是否在 keep-alive 场景记忆并恢复滚动位置（横纵向均记忆）
   */
  remember?: boolean
  /**
   * 位置记忆键，默认优先使用 props.id；未提供时会尝试使用路由 fullPath；
   * 若仍不可用，会退化为组件 uid（仅在同实例 keep-alive 下有效）。
   */
  rememberKey?: string
  /**
   * 记忆存储方式：
   * - memory：仅内存 Map（默认，页面刷新丢失，最快）
   * - session：sessionStorage（刷新保留，会话结束清除）
   * - local：localStorage（长期保留）
   */
  rememberStorage?: 'memory' | 'session' | 'local'
  /**
   * 恢复位置的延迟（毫秒），用于等待复杂布局/图片渲染完成再恢复
   */
  restoreDelay?: number
}

const props = withDefaults(defineProps<Props>(), {
  native: true,
  noresize: false,
  tag: 'div',
  always: false,
  minSize: 20,
  distance: 100,
  remember: false,
  rememberStorage: 'memory',
  restoreDelay: 0,
  // 为可选 props 提供显式默认值以满足 ESLint 规则
  height: undefined,
  maxHeight: undefined,
  wrapStyle: undefined,
  wrapClass: undefined,
  viewStyle: undefined,
  viewClass: undefined,
  id: undefined,
  role: undefined,
  ariaLabel: undefined,
  ariaOrientation: undefined,
  tabindex: undefined,
  rememberKey: undefined,
})

type ScrollbarDirection = 'top' | 'bottom' | 'left' | 'right'
const emit = defineEmits<{
  scroll: [payload: { scrollLeft: number; scrollTop: number }]
  'end-reached': [direction: ScrollbarDirection]
}>()

// refs for wrap and view
const wrapRef = ref<HTMLElement | null>(null)
const viewRef = ref<HTMLElement | null>(null)

// 路由路径（若可用）用于生成默认记忆 key
let routePath: string | undefined
try {
  // 在无路由上下文时使用 useRoute 会抛错，这里用 try/catch 兼容
  const r = useRoute?.()
  if (r && typeof r.fullPath === 'string') routePath = r.fullPath
} catch {
  // ignore
}

// computed styles for wrap (merge height/maxHeight & user styles)
function toPxOrString(v?: string | number): string | undefined {
  if (v === undefined) return undefined
  return typeof v === 'number' ? `${v}px` : v
}

const wrapMergedStyle = computed<Record<string, string | number>>(() => {
  const orientation = props.ariaOrientation
  const style: Record<string, string | number> = {
    // 方向控制规则：
    // - 未设置：同时允许 X/Y 滚动（auto）
    // - horizontal：仅横向滚动（X=auto, Y=hidden）
    // - vertical：仅纵向滚动（Y=auto, X=hidden）
    overflowX:
      orientation === 'horizontal'
        ? 'auto'
        : orientation === 'vertical'
          ? 'hidden'
          : 'auto',
    overflowY:
      orientation === 'horizontal'
        ? 'hidden'
        : orientation === 'vertical'
          ? 'auto'
          : 'auto',
  }
  const h = toPxOrString(props.height)
  const mh = toPxOrString(props.maxHeight)
  if (h) style.height = h
  if (mh) style.maxHeight = mh

  if (typeof props.wrapStyle === 'string') {
    // 当传入字符串样式时，无法与对象合并，交给模板 style 属性处理
    return style
  }
  if (props.wrapStyle && typeof props.wrapStyle === 'object') {
    return { ...style, ...props.wrapStyle }
  }
  return style
})

// resize observer to emit update when size changes
let resizeObserver: ResizeObserver | null = null
function setupResizeObserver() {
  if (props.noresize) return
  const el = wrapRef.value
  if (!el || typeof ResizeObserver === 'undefined') return
  resizeObserver = new ResizeObserver(() => {
    // 调用 update 让使用方可以在外部重新计算
    update()
  })
  resizeObserver.observe(el)
}

onMounted(() => {
  setupResizeObserver()
})

onUnmounted(() => {
  resizeObserver?.disconnect()
  resizeObserver = null
})

// expose methods (对齐 Element Plus Scrollbar)
type ScrollToOptions = { left?: number; top?: number }
function scrollTo(x: number | ScrollToOptions, y?: number) {
  const el = wrapRef.value
  if (!el) return
  if (typeof x === 'number') {
    el.scrollTo?.(x, y ?? 0)
    return
  }
  const left = x.left ?? el.scrollLeft
  const top = x.top ?? el.scrollTop
  el.scrollTo?.({ left, top })
}

function setScrollTop(top: number) {
  const el = wrapRef.value
  if (!el) return
  el.scrollTop = top
}

function setScrollLeft(left: number) {
  const el = wrapRef.value
  if (!el) return
  el.scrollLeft = left
}

function update() {
  // 原 Element Plus 会重新计算滚动条尺寸，这里通过强制触发一次 scroll 事件并刷新内部状态
  requestAnimationFrame(() => {
    const el = wrapRef.value
    if (!el) return
    emit('scroll', { scrollLeft: el.scrollLeft, scrollTop: el.scrollTop })
    checkEndReached(el)
  })
}

function getWrapRef(): HTMLElement | null {
  return wrapRef.value
}

defineExpose({
  scrollTo,
  setScrollTop,
  setScrollLeft,
  update,
  wrapRef: getWrapRef,
})

// scroll & end-reached handling
const endState = reactive({
  top: false,
  bottom: false,
  left: false,
  right: false,
})

function maybeEmit(direction: ScrollbarDirection, active: boolean) {
  if (active && !endState[direction]) {
    endState[direction] = true
    emit('end-reached', direction)
  }
  if (!active && endState[direction]) {
    // 离开端点阈值时重置，以便再次触发
    endState[direction] = false
  }
}

function checkEndReached(el: HTMLElement) {
  const {
    scrollTop,
    scrollLeft,
    scrollHeight,
    scrollWidth,
    clientHeight,
    clientWidth,
  } = el
  const d = props.distance ?? 0

  const atTop = scrollTop <= d
  const atBottom = scrollHeight - clientHeight - scrollTop <= d
  const atLeft = scrollLeft <= d
  const atRight = scrollWidth - clientWidth - scrollLeft <= d

  maybeEmit('top', atTop)
  maybeEmit('bottom', atBottom)
  maybeEmit('left', atLeft)
  maybeEmit('right', atRight)
}

function onScroll() {
  const el = wrapRef.value
  if (!el) return
  emit('scroll', { scrollLeft: el.scrollLeft, scrollTop: el.scrollTop })
  checkEndReached(el)
  // 滚动中按需记录位置（节流）
  // if (props.remember) scheduleSavePosition()
}

// --- 位置记忆/恢复 ---------------------------------------------------------

function getPersistKey(): string | null {
  // 优先级：rememberKey > id > route.fullPath > 组件 uid
  const instance = getCurrentInstance()
  const base = props.rememberKey || props.id || routePath
  if (base) return `UScrollbar:${base}`
  return instance ? `UScrollbar:uid:${instance.uid}` : null
}

function loadPosition(key: string): Position | undefined {
  switch (props.rememberStorage) {
    case 'session': {
      try {
        const raw = sessionStorage.getItem(key)
        if (!raw) return undefined
        const parsed = JSON.parse(raw) as Position
        if (typeof parsed?.left === 'number' && typeof parsed?.top === 'number')
          return parsed
      } catch {
        // ignore
      }
      return undefined
    }
    case 'local': {
      try {
        const raw = localStorage.getItem(key)
        if (!raw) return undefined
        const parsed = JSON.parse(raw) as Position
        if (typeof parsed?.left === 'number' && typeof parsed?.top === 'number')
          return parsed
      } catch {
        // ignore
      }
      return undefined
    }
    case 'memory':
    default:
      return memoryStore.get(key)
  }
}

function savePositionNow() {
  const el = wrapRef.value
  if (!el) return
  const p: Position = { left: el.scrollLeft, top: el.scrollTop }
  const key = getPersistKey()
  if (!key) return
  switch (props.rememberStorage) {
    case 'session':
      try {
        sessionStorage.setItem(key, JSON.stringify(p))
      } catch {
        // ignore
      }
      break
    case 'local':
      try {
        localStorage.setItem(key, JSON.stringify(p))
      } catch {
        // ignore
      }
      break
    case 'memory':
    default:
      memoryStore.set(key, p)
  }
}

// let saveTimer: number | null = null
// function scheduleSavePosition() {
//   if (saveTimer !== null) return
//   // 使用 rAF 节流，足够轻量
//   saveTimer = requestAnimationFrame(() => {
//     saveTimer = null
//     savePositionNow()
//   }) as unknown as number
// }

function restorePosition() {
  const key = getPersistKey()
  if (!key) return
  const p = loadPosition(key)
  if (!p) return
  const el = wrapRef.value
  if (!el) return
  // 等待下一帧，确保内容尺寸稳定后再恢复
  const delay = Math.max(0, props.restoreDelay || 0)
  const doRestore = () => {
    requestAnimationFrame(() => {
      el.scrollLeft = p.left
      el.scrollTop = p.top
      // 触发更新与 end-reached 检查
      update()
    })
  }
  if (delay > 0) {
    setTimeout(doRestore, delay)
  } else {
    // nextTick + rAF，兼顾同步布局与异步渲染
    nextTick().then(doRestore)
  }
}

onMounted(() => {
  if (props.remember) restorePosition()
})

// keep-alive 场景下的恢复/保存
onActivated?.(() => {
  if (props.remember) restorePosition()
})
onDeactivated?.(() => {
  if (props.remember) savePositionNow()
})
onBeforeUnmount(() => {
  if (props.remember) savePositionNow()
})
</script>

<template>
  <!-- 自定义实现：wrap 为滚动容器，view 为内容容器 -->
  <div class="uscrollbar" style="height: 100%">
    <div
      :id="props.id"
      ref="wrapRef"
      :role="props.role"
      :aria-label="props.ariaLabel"
      :aria-orientation="props.ariaOrientation"
      :tabindex="props.tabindex"
      :class="[
        'uscrollbar__wrap',
        props.wrapClass,
        props.native ? 'uscrollbar--native' : 'uscrollbar--custom',
      ]"
      :style="
        typeof props.wrapStyle === 'string' ? props.wrapStyle : wrapMergedStyle
      "
      @scroll="onScroll"
    >
      <component
        :is="props.tag"
        ref="viewRef"
        :class="['uscrollbar__view', props.viewClass]"
        :style="props.viewStyle"
      >
        <slot />
      </component>
    </div>
  </div>
</template>

<style scoped>
.uscrollbar__wrap {
  width: 100%;
  height: 100%;
  /* 与 Element Plus 行为保持一致，容器控制滚动 */
  overflow: auto;
}
/* 默认不强制撑宽内容区域，避免意外出现横向滚动。
   如需横向滚动，请在使用处通过 viewStyle/viewClass 指定宽度或布局。 */
.uscrollbar__view {
  /* 保持为块级，宽度随内容；横向滚动由使用方控制 viewClass/viewStyle */
  display: block;
}
/* 当 native=false 时，使用更细的滚动条以接近 Element Plus 外观（非强制一致） */
.uscrollbar--custom::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}
.uscrollbar--custom::-webkit-scrollbar-thumb {
  background-color: rgba(0, 0, 0, 0.2);
  border-radius: 4px;
}
.uscrollbar--custom {
  scrollbar-width: thin;
  scrollbar-color: rgba(0, 0, 0, 0.2) transparent;
}
</style>
