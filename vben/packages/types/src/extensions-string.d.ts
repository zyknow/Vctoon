declare global {
  interface String {
    /**
     * 格式化字符串
     * @param args 参数列表，可以是数组，也可以是对象
     * @returns 返回格式化的字符串
     * @example format('Hello, {0}!', ['World'])
     * @example format('Hello, {name}!', {name: 'World'})
     */
    format(args: any[] | Record<string, any>): string

    /** 首字母大写 */
    toUpperFirst(): string
  }
}

export {}
