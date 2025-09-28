<script setup lang="ts">
import type { MenuRecordRaw } from '@vben/types'

import { computed } from 'vue'

import { libraryApi, MediumType } from '@vben/api'
import {
  CiEditPencilLine01,
  CiLock,
  CiMoreVertical,
  CiSearch,
  MdiComic,
  MdiDelete,
  MdiVideo,
} from '@vben/icons'
import { useUserStore } from '@vben/stores'

import { ElMessageBox } from 'element-plus'

import { useLibraryDialogService } from '#/hooks/useLibraryDialogService'
import { $t } from '#/locales'

const props = withDefaults(defineProps<{ menu: MenuRecordRaw }>(), {})
const menu = computed(() => props.menu)
const userStore = useUserStore()
const library = computed(() => {
  const libraryId = menu.value.path.trim().replace('/library/', '')
  const foundLibrary = userStore.libraries.find((item) => item.id === libraryId)
  return foundLibrary || null
})

const mediumTypeIconMap = {
  [MediumType.Comic]: MdiComic,
  [MediumType.Video]: MdiVideo,
}

const dialog = useLibraryDialogService()

async function onEdit() {
  if (!library.value?.id) return
  const updated = await dialog.openEdit(library.value.id)
  if (updated) {
    // TODO: 刷新库信息或触发 store reload
  }
}

async function onPermission() {
  if (!library.value?.id || !library.value?.name) return
  const ok = await dialog.openPermission(library.value.id, library.value.name)
  if (ok) {
    // TODO: 刷新权限信息
  }
}

// 扫描库文件
async function handleScanFiles() {
  if (!library.value?.id) return
  try {
    await libraryApi.scan(library.value.id)
    // TODO: 显示扫描成功消息
  } catch (error) {
    // TODO: 显示扫描失败消息
    console.error('扫描失败:', error)
  }
}

// 删除
async function handleDelete() {
  if (!library.value?.id) return
  try {
    await ElMessageBox.confirm(
      $t('page.library.delete.confirmMessage', [menu.value.name]),
      $t('page.library.delete.confirmTitle'),
      {
        confirmButtonText: $t('page.library.delete.confirmButton'),
        cancelButtonText: $t('page.library.delete.cancelButton'),
        type: 'warning',
        confirmButtonClass: 'el-button--danger',
      },
    )
    await libraryApi.delete(library.value.id)
    // TODO: 刷新库列表，显示删除成功消息
  } catch (error) {
    // 用户取消删除或删除失败
    if (error !== 'cancel') {
      console.error('删除失败:', error)
      // TODO: 显示删除失败消息
    }
  }
}
</script>

<template>
  <div>
    <div class="library-title">
      <div class="flex min-w-20 flex-row items-center gap-2 pl-6">
        <component
          v-if="library"
          :is="mediumTypeIconMap[library!.mediumType]"
        />
        <div>{{ menu.name }}</div>
      </div>
      <el-dropdown trigger="click" placement="right-start">
        <el-button text link @click.stop>
          <CiMoreVertical class="library-action-icon" />
        </el-button>
        <template #dropdown>
          <el-dropdown-menu>
            <el-dropdown-item
              :icon="CiEditPencilLine01"
              class="min-w-[200px]"
              @click="onEdit"
            >
              {{ $t('page.library.actions.edit') }}
            </el-dropdown-item>
            <el-dropdown-item :icon="CiSearch" @click="handleScanFiles">
              {{ $t('page.library.actions.scanFiles') }}
            </el-dropdown-item>
            <el-dropdown-item :icon="CiLock" @click="onPermission">
              {{ $t('page.library.actions.authorizeAccess') }}
            </el-dropdown-item>
            <el-dropdown-item
              :icon="MdiDelete"
              @click="handleDelete"
              class="text-red-500"
            >
              {{ $t('page.library.actions.delete') }}
            </el-dropdown-item>
          </el-dropdown-menu>
        </template>
      </el-dropdown>
    </div>
  </div>
</template>

<style lang="scss" scoped>
.library-title {
  @apply flex flex-row items-center justify-between;
}

.library-action-icon {
  @apply text-xl hover:scale-125;
}
</style>
