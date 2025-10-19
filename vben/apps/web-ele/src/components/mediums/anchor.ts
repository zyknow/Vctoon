export const MEDIUM_ANCHOR_PREFIX = 'medium-anchor-'

export const getMediumAnchorId = (mediumId: string) => {
  return `${MEDIUM_ANCHOR_PREFIX}${mediumId}`
}
