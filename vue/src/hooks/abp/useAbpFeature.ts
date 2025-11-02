import { useAbpStore } from '@/stores'

type FeatureValue = {
  name: string
  value: string
}

export function useFeatures() {
  const abpStore = useAbpStore()

  const features = Object.keys(abpStore.application.features.values).map(
    (name) => ({
      name,
      value: abpStore.application.features.values[name] ?? '',
    }),
  )

  function get(name: string): FeatureValue | undefined {
    return features.find((feature) => feature.name === name)
  }

  function _isEnabled(name: string): boolean {
    const setting = get(name)
    return setting?.value.toLowerCase() === 'true'
  }

  const featureChecker = {
    getOrEmpty(name: string) {
      return get(name)?.value ?? ''
    },

    isEnabled(featureNames: string | string[], requiresAll?: boolean) {
      if (Array.isArray(featureNames)) {
        if (featureNames.length === 0) return true
        if (requiresAll === undefined || requiresAll === true) {
          return featureNames.every(_isEnabled)
        }
        return featureNames.some(_isEnabled)
      }
      return _isEnabled(featureNames)
    },
  }

  return featureChecker
}
