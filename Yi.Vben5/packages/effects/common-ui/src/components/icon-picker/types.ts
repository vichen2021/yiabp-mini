import type { VNode } from 'vue';

export interface IconPickerProps {
  MaxResultCount?: number;
  pageSize?: number;
  prefix?: string;
  autoFetchApi?: boolean;
  icons?: string[];
  inputComponent?: VNode;
  iconSlot?: string;
  modelValueProp?: string;
  iconClass?: string;
  type?: 'icon' | 'input';
}
