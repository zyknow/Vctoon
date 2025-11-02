import { useAbpStore } from '@/stores'

type SettingValue = {
  name: string
  value: string
}

export function useAbpSettings() {
  const abpStore = useAbpStore()

  const settings = Object.keys(abpStore.application.setting.values).map(
    (key) => ({
      name: key,
      value: abpStore.application.setting.values[key] ?? '',
    }),
  )

  function get(name: string): SettingValue | undefined {
    return settings.find((setting) => name === setting.name)
  }

  function getAll(...names: string[]): SettingValue[] {
    if (names.length > 0) {
      return settings.filter((setting) => names.includes(setting.name))
    }
    return settings
  }

  function getOrDefault<T>(name: string, defaultValue: T): string | T {
    const setting = get(name)
    if (!setting) {
      return defaultValue
    }
    return setting.value
  }

  const settingProvider = {
    getAll(...names: string[]) {
      return getAll(...names)
    },
    getNumber(name: string, defaultValue = 0) {
      const value = getOrDefault(name, defaultValue)
      const num = Number(value)
      return Number.isNaN(num) ? defaultValue : num
    },
    getOrEmpty(name: string) {
      return getOrDefault(name, '')
    },
    isTrue(name: string) {
      const value = getOrDefault(name, 'false')
      return typeof value === 'string' && value.toLowerCase() === 'true'
    },
  }

  return settingProvider
}
