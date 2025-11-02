export type FeatureGroup = {
  groups: [
    {
      displayName: string
      features: [
        {
          depth: number
          description: string
          displayName: string
          name: string
          parentName: string
          provider: {
            key: string
            name: string
          }
          value: string
          valueType: {
            name: string
            properties: {
              additionalProp1: string
              additionalProp2: string
              additionalProp3: string
            }
            validator: {
              name: string
              properties: {
                additionalProp1: string
                additionalProp2: string
                additionalProp3: string
              }
            }
          }
        },
      ]
      name: string
    },
  ]
}

export type UpdateFeatureInput = {
  features: [
    {
      name: string
      value: string
    },
  ]
}
