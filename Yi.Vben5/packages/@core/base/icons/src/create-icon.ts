import { defineComponent, h } from 'vue';

import { addIcon, Icon } from '@iconify/vue';
import type { IconifyIcon } from '@iconify/vue';

function createIconifyIcon(icon: string) {
  return defineComponent({
    name: `Icon-${icon}`,
    setup(props, { attrs }) {
      return () => h(Icon, { icon, ...props, ...attrs });
    },
  });
}

function createIconifyOfflineIcon(icon: string, data: IconifyIcon) {
  addIcon(icon, data);
  return createIconifyIcon(icon);
}

export { createIconifyIcon, createIconifyOfflineIcon };
