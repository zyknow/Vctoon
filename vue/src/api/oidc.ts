import { UserManager } from 'oidc-client-ts'

import { useEnvConfig } from '@/hooks/useEnvConfig'
export function useOidcManager() {
  const host = window.location.origin

  const { apiURL, auth } = useEnvConfig()
  const manager = new UserManager({
    authority: apiURL, // server 地址
    client_id: auth.clientId!, // client id
    post_logout_redirect_uri: `${host}/signout-callback-oidc`, // 退出登录
    redirect_uri: `${host}/signin-callback-oidc`,
    silent_redirect_uri: `${host}/signin-callback-oidc`,
    accessTokenExpiringNotificationTimeInSeconds: 4, // 超时
    silentRequestTimeoutInSeconds: 10000, //
    response_type: 'code',
    scope: auth.scope ?? '',
    filterProtocolClaims: true,
    automaticSilentRenew: true,
  })
  const signCallBack: {
    [key: string]: {
      handler: (to: any) => Promise<any> | undefined
      url: string
    }
  } = {
    signIn: {
      url: '/signin-callback-oidc',
      handler: async (to) => {
        if (to.path === signCallBack.signIn?.url) {
          const user = await manager
            .signinRedirectCallback()
            .catch((error: string) =>
              console.error(`signin-callback-oidc error${error}`),
            )
          if (user) {
            let target: string | undefined
            if ('state' in user) {
              const s = (user as unknown as { state?: unknown }).state
              if (typeof s === 'string') target = s
            }
            if (!target) return { path: '/', replace: true }
            try {
              const url = new URL(target, window.location.origin)
              const query: Record<string, string> = {}
              url.searchParams.forEach((v, k) => {
                query[k] = v
              })
              return {
                path: url.pathname,
                query,
                hash: url.hash || undefined,
                replace: true,
              }
            } catch {
              return { path: target, replace: true }
            }
          }
        }
      },
    },
    signOut: {
      url: '/signout-callback-oidc',
      handler: async (to) => {
        if (to.path === signCallBack.signOut?.url) {
          await manager.signoutRedirectCallback()
          let target: string | undefined
          try {
            const stored = sessionStorage.getItem(POST_LOGOUT_RETURN_KEY)
            if (stored) {
              target = stored
              sessionStorage.removeItem(POST_LOGOUT_RETURN_KEY)
            }
          } catch (e) {
            void e
          }
          if (!target) return { path: '/', replace: true }
          try {
            const url = new URL(target, window.location.origin)
            const query: Record<string, string> = {}
            url.searchParams.forEach((v, k) => {
              query[k] = v
            })
            return {
              path: url.pathname,
              query,
              hash: url.hash || undefined,
              replace: true,
            }
          } catch {
            return { path: target, replace: true }
          }
        }
      },
    },
  }

  const getAccessToken = async () => {
    const user = await manager.getUser()
    return user?.access_token
  }

  async function signinRedirectWithState(target?: string) {
    let desired = target
    if (!desired) {
      const { router } = await import('@/router')
      desired = router.currentRoute.value.fullPath
    }
    await manager.signinRedirect({ state: desired })
  }

  const POST_LOGOUT_RETURN_KEY = 'oidc:postLogoutRedirect'
  async function signoutRedirectWithState(target?: string) {
    let desired = target
    if (!desired) {
      const { router } = await import('@/router')
      desired = router.currentRoute.value.fullPath
    }
    try {
      sessionStorage.setItem(POST_LOGOUT_RETURN_KEY, desired)
    } catch (e) {
      void e
    }
    await manager.signoutRedirect()
  }

  function redirectHandler(to: any) {
    for (const key in signCallBack) {
      const sign = signCallBack[key]
      const res = sign?.handler(to)
      if (res) return res
    }
  }

  manager.events.addUserSignedOut(async () => {
    await manager.removeUser()
    // await manager.signinRedirect()
  })

  return {
    manager,
    redirectHandler,
    getAccessToken,
    signinRedirectWithState,
    signoutRedirectWithState,
  }
}
