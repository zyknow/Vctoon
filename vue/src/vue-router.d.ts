import 'vue-router'

import type { RouteMeta as IRouteMeta } from './types/vue-router'

declare module 'vue-router' {
  // eslint-disable-next-line @typescript-eslint/no-empty-object-type
  interface RouteMeta extends IRouteMeta {}
}
