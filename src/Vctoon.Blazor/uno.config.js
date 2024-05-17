// uno.config.ts
import {defineConfig} from 'unocss'
import presetWind from '@unocss/preset-wind'

export default defineConfig({
    presets: [
        presetWind(),
    ],
    theme: {
        colors: {
            'neutral': 'var(--neutral-foreground-rest)',
            'accent': 'var(--accent-fill-rest)',
            'warning': 'var(--warning)',
            'info': 'var(--info)',
            'error': 'var(--error)',
            'success': 'var(--success)',
            'fill': 'var(--neutral-fill-rest)',
            'fillInverse': 'var(--neutral-fill-inverse-rest)',
            'lightweight': 'var(--neutral-layer-1)',
            'disabled': 'var(--neutral-stroke-rest)',
        }
    }
})