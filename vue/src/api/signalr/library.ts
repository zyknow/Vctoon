import { useEnvConfig } from '@/hooks/useEnvConfig'

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

const { apiURL } = useEnvConfig()

export const libraryHub = defineHub<OnReceive, OnSend>({
  serverUrl: `${apiURL}/signalr-hubs/library`,
})
