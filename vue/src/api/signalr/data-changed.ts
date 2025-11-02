import { useEnvConfig } from '@/hooks/useEnvConfig'

import { defineHub } from './base'

type EntityName = 'artist' | 'library' | 'tag'

type OnReceive = {
  OnCreated: (entityName: EntityName, items: any[]) => void
  OnDeleted: (entityName: EntityName, items: any[]) => void
  OnUpdated: (entityName: EntityName, items: any[]) => void
}

// eslint-disable-next-line @typescript-eslint/no-empty-object-type
type OnSend = {}

const { apiURL } = useEnvConfig()

export const dataChangedHub = defineHub<OnReceive, OnSend>({
  serverUrl: `${apiURL}/signalr-hubs/data-changed`,
})
