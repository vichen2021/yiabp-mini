import type { TableActionProps } from '@vben/common-ui';
import type {
  VxeGridPropTypes,
  VxeTableGridOptions,
} from '@vben/plugins/vxe-table';
import type { Recordable } from '@vben/types';
import type { PropType } from 'vue';

import type { ComponentPropsMap, ComponentType } from './component';

import { computed, defineComponent, h } from 'vue';

import { useAccess } from '@vben/access';
import { VbenTableAction as VbenTableActionCore } from '@vben/common-ui';
import { CircleX, Eye, UserRoundPen } from '@vben/icons';
import { $te } from '@vben/locales';
import {
  setupVbenVxeTable,
  useVbenVxeGrid as useGrid,
} from '@vben/plugins/vxe-table';
import { get, isFunction, isString } from '@vben/utils';

import { objectOmit } from '@vueuse/core';
import { Button, Dropdown, Image, Popconfirm, Switch, Tag } from 'antdv-next';

import { $t } from '#/locales';

import { useVbenForm } from './form';

setupVbenVxeTable({
  configVxeTable: (vxeUI) => {
    vxeUI.setConfig({
      grid: {
        align: 'center',
        border: false,
        minHeight: 180,
        formConfig: {
          // 全局禁用vxe-table的表单配置，使用formOptions
          enabled: false,
        },
        proxyConfig: {
          autoLoad: true,
          response: {
            result: 'items',
            total: 'totalCount',
            list: 'items',
          },
          showActiveMsg: true,
          showResponseMsg: false,
        },
        // 溢出展示形式
        showOverflow: true,
        pagerConfig: {
          // 默认条数
          pageSize: 10,
          // 分页可选条数
          pageSizes: [10, 20, 30, 40, 50],
        },
        rowConfig: {
          // 鼠标移入行显示 hover 样式
          isHover: true,
          // 点击行高亮
          isCurrent: false,
        },
        columnConfig: {
          // 可拖拽列宽
          resizable: true,
        },
        // 右上角工具栏
        toolbarConfig: {
          // 自定义列
          custom: true,
          customOptions: {
            icon: 'vxe-icon-setting',
          },
          // 最大化
          zoom: true,
          // 刷新
          refresh: true,
          refreshOptions: {
            // 默认为reload 修改为在当前页刷新
            code: 'query',
          },
        },
        // 圆角按钮
        round: true,
        // 表格尺寸
        size: 'medium',
        customConfig: {
          // 表格右上角自定义列配置 是否保存到localStorage
          // 必须存在id参数才能使用
          storage: false,
        },
      } as VxeTableGridOptions,
    });

    /**
     * 解决vxeTable在热更新时可能会出错的问题
     */
    vxeUI.renderer.forEach((_item, key) => {
      if (key.startsWith('Cell')) {
        vxeUI.renderer.delete(key);
      }
    });

    // 表格配置项可以用 cellRender: { name: 'CellImage' },
    vxeUI.renderer.add('CellImage', {
      renderTableDefault(renderOpts, params) {
        const { props } = renderOpts;
        const { column, row } = params;
        return h(Image, { src: row[column.field], ...props });
      },
    });

    // 表格配置项可以用 cellRender: { name: 'CellLink' },
    vxeUI.renderer.add('CellLink', {
      renderTableDefault(renderOpts) {
        const { props } = renderOpts;
        return h(
          Button,
          { size: 'small', type: 'link' },
          { default: () => props?.text },
        );
      },
    });

    // 单元格渲染：Tag
    vxeUI.renderer.add('CellTag', {
      renderTableDefault({ options, props }, { column, row }) {
        const value = get(row, column.field);
        const tagOptions = options ?? [
          { color: 'success', label: $t('common.enabled'), value: 1 },
          { color: 'error', label: $t('common.disabled'), value: 0 },
        ];
        const tagItem = tagOptions.find((item) => item.value === value);
        return h(
          Tag,
          {
            ...props,
            ...objectOmit(tagItem ?? {}, ['label']),
          },
          { default: () => tagItem?.label ?? value },
        );
      },
    });

    vxeUI.renderer.add('CellSwitch', {
      renderTableDefault({ attrs, props }, { column, row }) {
        const loadingKey = `__loading_${column.field}`;
        const finallyProps = {
          checkedChildren: $t('common.enabled'),
          checkedValue: 1,
          unCheckedChildren: $t('common.disabled'),
          unCheckedValue: 0,
          ...props,
          checked: row[column.field],
          loading: row[loadingKey] ?? false,
          'onUpdate:checked': onChange,
        };
        async function onChange(newVal: any) {
          row[loadingKey] = true;
          try {
            const result = await attrs?.beforeChange?.(newVal, row);
            if (result !== false) {
              row[column.field] = newVal;
            }
          } finally {
            row[loadingKey] = false;
          }
        }
        return h(Switch, finallyProps);
      },
    });

    /**
     * 注册表格的操作按钮渲染器
     */
    vxeUI.renderer.add('CellOperation', {
      renderTableDefault({ attrs, options, props }, { column, row }) {
        const defaultProps = { ...props };
        let align: TableActionProps['align'];
        switch (column.align) {
          case 'center': {
            align = 'center';
            break;
          }
          case 'left': {
            align = 'start';
            break;
          }
          default: {
            align = 'end';
            break;
          }
        }
        const presets: Recordable<Recordable<any>> = {
          delete: {
            danger: true,
            icon: CircleX,
            tooltip: $t('common.delete'),
          },
          edit: {
            icon: UserRoundPen,
            tooltip: $t('common.edit'),
          },
          detail: {
            icon: Eye,
            tooltip: $t('pages.common.info'),
          },
        };
        const operations = (
          options || ['edit', 'detail', 'delete']
        )
          .map((opt) => {
            if (isString(opt)) {
              return presets[opt]
                ? { code: opt, ...presets[opt], ...defaultProps }
                : {
                    code: opt,
                    text: $te(`common.${opt}`) ? $t(`common.${opt}`) : opt,
                    ...defaultProps,
                  };
            } else {
              return { ...defaultProps, ...presets[opt.code], ...opt };
            }
          })
          .map((opt) => {
            const optBtn: Recordable<any> = {};
            Object.keys(opt).forEach((key) => {
              optBtn[key] = isFunction(opt[key]) ? opt[key](row) : opt[key];
            });
            return optBtn;
          })
          .filter((opt) => opt.show !== false);

        return h(
          VbenTableActionCore,
          {
            actions: operations.map((opt) => {
              const { code, show, type, ...rest } = opt;
              const action = {
                ...rest,
                ifShow: show,
                key: code,
                onClick: () =>
                  attrs?.onClick?.({
                    code,
                    row,
                  }),
              };
              if (code === 'delete') {
                return {
                  ...action,
                  popConfirm: {
                    cancelText: $t('common.cancel'),
                    title: $t('ui.actionTitle.delete', [
                      attrs?.nameTitle || '',
                    ]),
                    okText: $t('common.confirm'),
                    confirm: action.onClick,
                  },
                  tooltip: $t('ui.actionMessage.deleteConfirm', [
                    row[attrs?.nameField || 'name'],
                  ]),
                };
              }
              return action;
            }),
            align,
            class: 'table-operations',
          },
        );
      },
    });

    // 这里可以自行扩展 vxe-table的全局配置，比如自定义格式化
    // vxeUI.formats.add
  },
  useVbenForm,
});

export const useVbenVxeGrid = <T extends Record<string, any>>(
  ...rest: Parameters<typeof useGrid<T, ComponentType, ComponentPropsMap>>
) => useGrid<T, ComponentType, ComponentPropsMap>(...rest);

/**
 * 表格操作按钮组件
 *
 * 在适配器内部统一注入权限判断（hasPermission），使用方无需再传入 `:has-permission`。
 * 通过 action 的 `auth` 字段声明权限码，结合 `useAccess().hasAccessByCodes` 判断是否展示。
 * 如需自定义权限逻辑，仍可显式传入 `:has-permission` 覆盖默认行为。
 */
export const VbenTableAction = defineComponent({
  inheritAttrs: false,
  name: 'VbenTableAction',
  props: {
    actions: {
      default: () => [],
      type: Array as PropType<TableActionProps['actions']>,
    },
    align: {
      default: 'end',
      type: String as PropType<TableActionProps['align']>,
    },
    class: {
      default: undefined,
      type: [Array, Object, String] as PropType<TableActionProps['class']>,
    },
    divider: {
      default: false,
      type: Boolean,
    },
    dropdownActions: {
      default: () => [],
      type: Array as PropType<TableActionProps['dropdownActions']>,
    },
    moreText: {
      default: undefined,
      type: String,
    },
  },
  setup(props: TableActionProps, { attrs }) {
    const { hasAccessByCodes } = useAccess();
    function hasPermission(auth?: string | string[]) {
      if (!auth) return true;
      return hasAccessByCodes(Array.isArray(auth) ? auth : [auth]);
    }
    function checkVisible(item: NonNullable<TableActionProps['actions']>[number]) {
      if (item.auth && !hasPermission(item.auth)) return false;
      if (typeof item.ifShow === 'boolean') return item.ifShow;
      if (typeof item.ifShow === 'function') return item.ifShow();
      return true;
    }
    const visibleActions = computed(() =>
      (props.actions ?? []).filter((item) => checkVisible(item)),
    );
    const visibleDropdownActions = computed(() =>
      (props.dropdownActions ?? []).filter((item) => checkVisible(item)),
    );
    const justifyContent = computed(() => {
      switch (props.align) {
        case 'center': {
          return 'center';
        }
        case 'start': {
          return 'flex-start';
        }
        default: {
          return 'flex-end';
        }
      }
    });
    function runAction(action: NonNullable<TableActionProps['actions']>[number]) {
      if (action.disabled || action.loading) return;
      action.onClick?.();
    }
    function getButtonSize(
      size: NonNullable<TableActionProps['actions']>[number]['size'],
    ) {
      if (size === 'sm' || size === 'xs') return 'small';
      if (size === 'lg') return 'large';
      return undefined;
    }
    function renderAction(action: NonNullable<TableActionProps['actions']>[number]) {
      const button = h(
        Button,
        {
          class: ['px-1', action.class],
          danger: action.danger,
          disabled: action.disabled,
          loading: action.loading,
          size: getButtonSize(action.size) ?? 'small',
          type: 'link',
          onClick: (event: MouseEvent) => {
            event.stopPropagation();
            if (!action.popConfirm) {
              runAction(action);
            }
          },
        },
        { default: () => action.text },
      );

      if (!action.popConfirm) {
        return button;
      }

      const popConfirm = action.popConfirm;
      return h(
        Popconfirm,
        {
          cancelText: popConfirm.cancelText,
          okText: popConfirm.okText,
          placement: 'left',
          title: popConfirm.title,
          onConfirm: () => {
            popConfirm.confirm ? popConfirm.confirm() : runAction(action);
          },
        },
        { default: () => button },
      );
    }
    return () =>
      h('div', {
        class: ['flex items-center', props.class, attrs.class],
        style: { justifyContent: justifyContent.value },
      }, [
        ...visibleActions.value.map((action) => renderAction(action)),
        visibleDropdownActions.value.length > 0
          ? h(
              Dropdown,
              {
                placement: 'bottomRight',
                menu: {
                  items: visibleDropdownActions.value.map((action, index) => ({
                    danger: action.danger,
                    disabled: action.disabled,
                    key: action.key ?? index,
                    label: action.text,
                  })),
                  onClick: ({ key }: { key: number | string }) => {
                    const action = visibleDropdownActions.value.find(
                      (item, index) => (item.key ?? index) === key,
                    );
                    if (action) runAction(action);
                  },
                },
              },
              {
                default: () =>
                  h(
                    Button,
                    {
                      class: 'px-1',
                      size: 'small',
                      type: 'link',
                    },
                    {
                      default: () => props.moreText ?? $t('pages.common.more'),
                    },
                  ),
              },
            )
          : null,
      ]);
  },
});

export type OnActionClickParams<T = Recordable<any>> = {
  code: string;
  row: T;
};

export type OnActionClickFn<T = Recordable<any>> = (
  params: OnActionClickParams<T>,
) => void;

export type * from '@vben/plugins/vxe-table';

/**
 * 判断vxe-table的复选框是否选中
 * @param tableApi api
 * @returns boolean
 */
export function vxeCheckboxChecked(
  tableApi: ReturnType<typeof useVbenVxeGrid>[1],
) {
  return tableApi?.grid?.getCheckboxRecords?.()?.length > 0;
}

/**
 * 通用的 排序参数添加到请求参数中
 * @param params 请求参数
 * @param sortList vxe-table的排序参数
 */
export function addSortParams(
  params: Record<string, any>,
  sortList: VxeGridPropTypes.ProxyAjaxQuerySortCheckedParams[],
) {
  // 这里是排序取消 length为0 就不添加参数了
  if (sortList.length === 0) {
    return;
  }
  // 支持单/多字段排序
  const orderByColumn = sortList.map((item) => item.field).join(',');
  const isAsc = sortList.map((item) => item.order).join(',');
  params.orderByColumn = orderByColumn;
  params.isAsc = isAsc;
}
