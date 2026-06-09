import type { Plugin } from 'vite';

const REFERENCE_LINE = '@reference "@vben/tailwind-config/theme";\n';

/**
 * Auto-inject @reference into Vue SFC <style> blocks that use @apply.
 */
export function viteTailwindReferencePlugin(): Plugin {
  return {
    enforce: 'pre',
    name: 'vite:tailwind-reference',
    transform(code, id) {
      if (!id.includes('.vue')) {
        return null;
      }
      if (!id.includes('type=style')) {
        return null;
      }
      if (code.includes('@reference')) {
        return null;
      }
      if (!code.includes('@apply')) {
        return null;
      }
      return {
        code: REFERENCE_LINE + code,
        map: null,
      };
    },
  };
}
