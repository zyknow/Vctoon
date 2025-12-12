<script setup lang="ts">
import { computed } from 'vue'

import { libraryApi } from '@/api/http/library'
import AddSeriesModal from '@/components/overlays/AddSeriesModal.vue'
import ConfirmModal from '@/components/overlays/ConfirmModal.vue'
import CreateOrUpdateLibraryModal from '@/components/overlays/CreateOrUpdateLibraryModal.vue'
import LibraryPermissionDialog from '@/components/overlays/LibraryPermissionModal.vue'
import { $t } from '@/locales/i18n'
import { useUserStore } from '@/stores/user'

const props = withDefaults(defineProps<{ menu: MenuRecordRaw }>(), {})
const menu = computed(() => props.menu)
const userStore = useUserStore()
const toast = useToast()
const overlay = useOverlay()

const path = menu.value.to as string

const libraryId = path.trim().replace('/library/', '')

// 添加系列
const addSeriesModal = overlay.create(AddSeriesModal)

async function onAddSeries() {
  const library = userStore.libraries.find((lib) => lib.id === libraryId)
  if (!library) return

  await addSeriesModal.open({
    libraryId: library.id,
    mediumType: library.mediumType,
  })
}

// 编辑资料库
const editLibraryModal = overlay.create(CreateOrUpdateLibraryModal)

async function onEdit() {
  const library = await libraryApi.getById(libraryId)
  const result = await editLibraryModal.open({
    library: library,
  })
  if (result) {
    await userStore.reloadLibraries()
  }
}

// 权限设置
const permissionModal = overlay.create(LibraryPermissionDialog)

async function onPermission() {
  const library = userStore.libraries.find((lib) => lib.id === libraryId)
  if (!library) return

  await permissionModal.open({
    libraryId: library.id,
    libraryName: library.name,
  })
}

// 扫描库文件
async function handleScanFiles() {
  try {
    await libraryApi.scan(libraryId)
    // eslint-disable-next-line @typescript-eslint/no-unused-vars, unused-imports/no-unused-vars
  } catch (error) {
    toast.add({
      title: $t('page.library.actions.scanFailed'),
      color: 'error',
    })
  }
}

// 删除
const confirmModal = overlay.create(ConfirmModal)

async function handleDelete() {
  if (!libraryId) return
  try {
    const confirmed = await confirmModal.open({
      title: $t('page.library.delete.confirmTitle'),
      message: $t('page.library.delete.confirmMessage', [menu.value.label]),
      danger: true,
      confirmText: $t('page.library.delete.confirmButton'),
      cancelText: $t('page.library.delete.cancelButton'),
    })

    if (!confirmed) return

    await libraryApi.delete(libraryId)
    // 由singalR 同步
    // await userStore.reloadLibraries()
  } catch (error) {
    console.error('删除失败:', error)
    toast.add({
      title: $t('page.library.delete.failed'),
      color: 'error',
    })
  }
}

// DropdownMenu items
const menuItems = computed(() => [
  [
    {
      label: $t('page.library.actions.addSeries'),
      icon: 'i-lucide-plus-square',
      onSelect: onAddSeries,
    },
  ],
  [
    {
      label: $t('page.library.actions.edit'),
      icon: 'i-lucide-pencil',
      onSelect: onEdit,
    },
    {
      label: $t('page.library.actions.scanFiles'),
      icon: 'i-lucide-search',
      onSelect: handleScanFiles,
    },
    {
      label: $t('page.library.actions.authorizeAccess'),
      icon: 'i-lucide-lock',
      onSelect: onPermission,
    },
  ],
  [
    {
      label: $t('page.library.actions.delete'),
      icon: 'i-lucide-trash-2',
      color: 'error' as const,
      onSelect: handleDelete,
    },
  ],
])
</script>

<template>
  <div>
    <UDropdownMenu
      :items="menuItems"
      :content="{
        align: 'start',
        side: 'right',
        sideOffset: 8,
      }"
    >
      <UButton
        class="hover:text-primary"
        icon="i-lucide-ellipsis-vertical"
        color="neutral"
        variant="ghost"
        square
        size="xs"
        @click.prevent
      />
    </UDropdownMenu>
  </div>
</template>

<style scoped></style>
