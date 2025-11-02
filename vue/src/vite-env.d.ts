/// <reference types="vite/client" />

import type { AppConfigRaw } from './types/env-config'

type ImportMetaEnv = AppConfigRaw

interface ImportMeta {
  readonly env: ImportMetaEnv
}
