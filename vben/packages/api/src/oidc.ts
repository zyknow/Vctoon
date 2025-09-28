import { useAppConfig } from '@vben/hooks'

import { UserManager } from 'oidc-client-ts'

export function useOidcManager() {
  const host = window.location.origin

  const scopes = ['openid', 'Vctoon', 'offline_access']

  const { apiURL } = useAppConfig(import.meta.env, import.meta.env.PROD)

  const manager = new UserManager({
    authority: apiURL, // server 地址
    client_id: 'Vctoon_Web', // client id
    post_logout_redirect_uri: `${host}/signout-callback-oidc`, // 退出登录
    redirect_uri: `${host}/signin-callback-oidc`,
    silent_redirect_uri: `${host}/signin-callback-oidc`,
    accessTokenExpiringNotificationTimeInSeconds: 4, // 超时
    silentRequestTimeoutInSeconds: 2000, //
    response_type: 'code',
    scope: scopes.join(' '),
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
            return { path: '/', replace: true }
          }
        }
      },
    },
    signOut: {
      url: '/signout-callback-oidc',
      handler: async (to) => {
        if (to.path === signCallBack.signIn?.url) {
          await manager.signoutRedirectCallback()
          return { path: '/', replace: true }
        }
      },
    },
  }

  const getAccessToken = async () => {
    const user = await manager.getUser()
    return user?.access_token
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

  return { manager, redirectHandler, getAccessToken }
}
