import { useAppConfig } from '@vben/hooks'

import { defineHub } from './base'

type OnReceive = {
  OnScanned: (libraryId: string) => void
  OnScanning: (item: {
    libraryId: string
    message: string
    title: string
  }) => void
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
type OnSend = {}

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

export const libraryHub = defineHub<OnReceive, OnSend>({
  serverUrl: `${apiURL}/signalr-hubs/library`,
})
