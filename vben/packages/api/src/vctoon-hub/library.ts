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

type OnSend = {
  SendMessage: (message: string) => void
  SendMessageToGroup: (groupName: string, message: string) => void
}

const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

export const libraryHub = defineHub<OnReceive, OnSend>({
  serverUrl: `${apiURL}/signalr-hubs/library`,
})
