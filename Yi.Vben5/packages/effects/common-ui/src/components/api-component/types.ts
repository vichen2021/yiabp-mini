import type { Component } from 'vue';

import type { AnyPromiseFunction } from '@vben/types';

export type ApiComponentOptionsItem = {
  [name: string]: any;
  children?: ApiComponentOptionsItem[];
  disabled?: boolean;
  label?: string;
  value?: number | string;
};

export type ApiComponentLabelFn = (item: ApiComponentOptionsItem) => string;

export interface ApiComponentProps {
  component: Component;
  numberToString?: boolean;
  api?: (arg?: any) => Promise<ApiComponentOptionsItem[] | Record<string, any>>;
  params?: Record<string, any>;
  resultField?: string;
  labelField?: string;
  labelFn?: ApiComponentLabelFn;
  childrenField?: string;
  valueField?: string;
  disabledField?: string;
  optionsPropName?: string;
  immediate?: boolean;
  alwaysLoad?: boolean;
  beforeFetch?: AnyPromiseFunction<any, any>;
  shouldFetch?: AnyPromiseFunction<any, boolean>;
  afterFetch?: AnyPromiseFunction<any, any>;
  options?: ApiComponentOptionsItem[];
  loadingSlot?: string;
  visibleEvent?: string;
  modelPropName?: string;
  autoSelect?:
    | 'first'
    | 'last'
    | 'one'
    | ((item: ApiComponentOptionsItem[]) => ApiComponentOptionsItem)
    | false;
}

export type ApiComponentSharedProps = Omit<ApiComponentProps, 'component'>;
