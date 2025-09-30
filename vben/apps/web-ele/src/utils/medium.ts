import { $t } from '#/locales'

export const formatMediumDateTime = (date?: Date | string): string => {
  if (!date) return ''
  const value = typeof date === 'string' ? new Date(date) : date
  if (Number.isNaN(value.getTime())) return ''
  return value.toLocaleString('zh-CN', {
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    month: '2-digit',
    year: 'numeric',
  })
}

export const formatMediumProgress = (progress?: number): string => {
  if (typeof progress !== 'number') {
    return $t('page.mediums.info.notStarted')
  }
  if (progress >= 1) {
    return $t('page.mediums.info.completed')
  }
  if (progress > 0) {
    return $t('page.mediums.info.inProgress', {
      progress: Math.round(progress * 100),
    })
  }
  return $t('page.mediums.info.notStarted')
}

export const formatMediumDuration = (duration?: string): string => {
  if (!duration) return ''
  const seconds = Number.parseInt(duration, 10)
  if (Number.isNaN(seconds)) return duration
  const hours = Math.floor(seconds / 3600)
  const minutes = Math.floor((seconds % 3600) / 60)
  const remainSeconds = seconds % 60
  if (hours > 0) {
    return `${hours}:${minutes.toString().padStart(2, '0')}:${remainSeconds
      .toString()
      .padStart(2, '0')}`
  }
  return `${minutes}:${remainSeconds.toString().padStart(2, '0')}`
}
