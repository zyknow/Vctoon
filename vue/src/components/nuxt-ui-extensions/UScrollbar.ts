export type Position = Readonly<{ left: number; top: number }>

export const memoryStore = new Map<string, Position>()
