// String.prototype 扩展的运行时代码，多次加载时保持幂等

if (typeof String.prototype.format !== 'function') {
  String.prototype.format = function (
    args: any[] | Record<string, any>,
  ): string {
    let result: string = this as string
    if (args === null) return result

    if (Array.isArray(args)) {
      for (const [index, value] of args.entries()) {
        const regexp = new RegExp(`\\{${index}\\}`, 'gi')
        result = result.replace(regexp, String(value))
      }
      return result
    }

    if (typeof args === 'object') {
      const keys = Object.keys(args)
      if (keys.length === 0) return result
      for (const key of keys) {
        const regexp = new RegExp(`\\{${key}\\}`, 'gi')
        result = result.replace(regexp, String(args[key]))
      }
      return result
    }

    return result
  }
}

if (typeof String.prototype.toUpperFirst !== 'function') {
  String.prototype.toUpperFirst = function (): string {
    const self = String(this)
    if (self.trim().length === 0) return self
    return self.charAt(0).toUpperCase() + self.slice(1)
  }
}
