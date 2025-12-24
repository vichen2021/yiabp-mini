<script setup lang="ts">
// @ts-nocheck
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Menu } from '#/api/system/menu/model';

import { computed, nextTick, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Fallback, Page, useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { eachTree, getVxePopupContainer, treeToList } from '@vben/utils';

import { Popconfirm, Space, Switch, Tooltip } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { menuCascadeRemove, menuList, menuRemove } from '#/api/system/menu';

import { columns, querySchema } from './data';
import menuDrawer from './menu-drawer.vue';

function normalizeId(id: Menu['id'] | Menu['menuId'] | null | undefined) {
  if (id === undefined || id === null) {
    return undefined;
  }
  if (typeof id === 'string' && id.trim() === '') {
    return undefined;
  }
  return String(id);
}

function extractMenuList(resp: Menu[] | Record<string, any> | undefined) {
  if (!resp) {
    return [];
  }
  if (Array.isArray(resp)) {
    return resp;
  }
  if (Array.isArray(resp.data)) {
    return resp.data;
  }
  if (Array.isArray(resp.items)) {
    return resp.items;
  }
  return [];
}

interface MenuRow extends Record<string, unknown> {
  menuId: string;
  parentId?: string;
  menuName: string;
  menuType: string;
  icon?: string;
  orderNum?: number;
  perms?: string;
  component?: string;
  status: string;
  visible: string;
  createTime?: string;
  children?: any[];
}

function transformMenuData(raw: Menu): MenuRow | null {
  const menuId = normalizeId(raw.menuId ?? raw.id);
  if (!menuId) {
    return null;
  }
  const parentId = normalizeId(raw.parentId);
  let status = raw.status ?? '0';
  if (typeof raw.state === 'boolean') {
    status = raw.state ? '0' : '1';
  }
  let visible = raw.visible ?? '0';
  if (typeof raw.isShow === 'boolean') {
    visible = raw.isShow ? '0' : '1';
  }
  return {
    ...raw,
    menuId,
    parentId,
    menuType: raw.menuType ?? '',
    icon: raw.icon ?? raw.menuIcon ?? '#',
    perms: raw.perms ?? raw.permissionCode ?? '',
    status,
    visible,
    createTime: raw.createTime ?? raw.creationTime ?? '',
    children: undefined,
  } as MenuRow;
}

function buildTree(data: MenuRow[]) {
  const map = new Map<string, Menu>();
  const roots: Menu[] = [];
  data.forEach((item) => {
    const key = String(item.menuId);
    map.set(key, item);
  });
  data.forEach((item) => {
    const menuIdKey = String(item.menuId);
    const parentKey = item.parentId ? String(item.parentId) : undefined;
    if (parentKey && parentKey !== menuIdKey && map.has(parentKey)) {
      const parent = map.get(parentKey);
      if (parent) {
        parent.children = parent.children || [];
        parent.children.push(item);
        return;
      }
    }
    roots.push(item);
  });
  return roots;
}

/**
 * 不要问为什么有两个根节点 v-if会控制只会渲染一个
 */

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
};

const gridOptions: VxeGridProps<Record<string, any>> = {
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const resp = await menuList({
          ...formValues,
        });
        const flatList = extractMenuList(resp);
        const transformedData = flatList
          .map((item) => transformMenuData(item))
          .filter(Boolean) as MenuRow[];
        const treeData = buildTree(transformedData);
        const payload =
          resp && !Array.isArray(resp) && typeof resp === 'object' ? resp : {};
        return {
          ...payload,
          items: treeData,
          rows: treeData,
          data: treeData,
        };
      },
      querySuccess: () => {
        eachTree(tableApi.grid.getData(), (item) => (item.expand = true));
        nextTick(() => {
          setExpandOrCollapse(true);
        });
      },
    },
  },
  rowConfig: {
    keyField: 'menuId',
  },
  /**
   * 开启虚拟滚动
   * 数据量小可以选择关闭
   * 如果遇到样式问题(空白、错位 滚动等)可以选择关闭虚拟滚动
   */
  scrollY: {
    enabled: true,
    gt: 0,
  },
  treeConfig: {
    childrenField: 'children',
    parentField: 'parentId',
    rowField: 'menuId',
    // 刷新接口后 记录展开行的情况
    transform: false,
    reserve: true,
  },
  id: 'system-menu-index',
};

// @ts-expect-error TS2589: MenuRow + proxyConfig causes deep instantiation; generics are manageable at runtime.
const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
  gridEvents: {
    cellDblclick: (e) => {
      const row = (e.row ?? {}) as Record<string, any>;
      if (!row?.children) {
        return;
      }
      const isExpanded = row?.expand;
      tableApi.grid.setTreeExpand(row, !isExpanded);
      row.expand = !isExpanded;
    },
    // 需要监听使用箭头展开的情况 否则展开/折叠的数据不一致
    toggleTreeExpand: (e) => {
      const row = (e.row ?? {}) as Record<string, any>;
      const { expanded } = e;
      row.expand = expanded;
    },
  },
});
const [MenuDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: menuDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

function handleSubAdd(row: MenuRow) {
  const { menuId } = row;
  drawerApi.setData({ id: menuId, update: false });
  drawerApi.open();
}

async function handleEdit(record: MenuRow) {
  drawerApi.setData({ id: record.menuId, update: true });
  drawerApi.open();
}

/**
 * 是否级联删除
 */
const cascadingDeletion = ref(false);
async function handleDelete(row: MenuRow) {
  if (cascadingDeletion.value) {
    // 级联删除
    const menuAndChildren = treeToList<MenuRow[]>([row], { id: 'menuId' });
    const menuIds = menuAndChildren.map((item) => String(item.menuId));
    await menuCascadeRemove(menuIds);
  } else {
    // 单删除
    await menuRemove([String(row.menuId)]);
  }
  await tableApi.query();
}

function removeConfirmTitle(row: MenuRow) {
  const menuName = $t(row.menuName);
  if (!cascadingDeletion.value) {
    return `是否确认删除 [${menuName}] ?`;
  }
  const menuAndChildren = treeToList<MenuRow[]>([row], { id: 'menuId' });
  if (menuAndChildren.length === 1) {
    return `是否确认删除 [${menuName}] ?`;
  }
  return `是否确认删除 [${menuName}] 及 [${menuAndChildren.length - 1}]个子项目 ?`;
}

/**
 * 编辑/添加成功后刷新表格
 */
async function afterEditOrAdd() {
  tableApi.query();
}

/**
 * 全部展开/折叠
 * @param expand 是否展开
 */
function setExpandOrCollapse(expand: boolean) {
  eachTree(tableApi.grid.getData(), (item) => (item.expand = expand));
  tableApi.grid?.setAllTreeExpand(expand);
}

/**
 * 与后台逻辑相同
 * 只有租户管理和超级管理能访问菜单管理
 * 注意: 只有超管才能对菜单进行`增删改`操作
 * 注意: 只有超管才能对菜单进行`增删改`操作
 * 注意: 只有超管才能对菜单进行`增删改`操作
 */
const { hasAccessByRoles } = useAccess();
const isAdmin = computed(() => {
  return hasAccessByRoles(['admin', 'superadmin']);
});
</script>

<template>
  <Page v-if="isAdmin" :auto-content-height="true">
    <BasicTable table-title="菜单列表" table-title-help="双击展开/收起子菜单">
      <template #toolbar-tools>
        <Space>
          <Tooltip title="删除菜单以及子菜单">
            <div
              v-access:role="['superadmin']"
              v-access:code="['system:menu:remove']"
              class="flex items-center"
            >
              <span class="mr-2 text-sm text-[#666666]">级联删除</span>
              <Switch v-model:checked="cascadingDeletion" />
            </div>
          </Tooltip>
          <a-button @click="setExpandOrCollapse(false)">
            {{ $t('pages.common.collapse') }}
          </a-button>
          <a-button @click="setExpandOrCollapse(true)">
            {{ $t('pages.common.expand') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['system:menu:add']"
            v-access:role="['superadmin']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['system:menu:edit']"
            v-access:role="['superadmin']"
            @click="handleEdit(row)"
          >
            {{ $t('pages.common.edit') }}
          </ghost-button>
          <!-- '按钮类型'无法再添加子菜单 -->
          <ghost-button
            v-if="row.menuType !== 'F'"
            class="btn-success"
            v-access:code="['system:menu:add']"
            v-access:role="['superadmin']"
            @click="handleSubAdd(row)"
          >
            {{ $t('pages.common.add') }}
          </ghost-button>
          <Popconfirm
            :get-popup-container="getVxePopupContainer"
            placement="left"
            :title="removeConfirmTitle(row)"
            @confirm="handleDelete(row)"
          >
            <ghost-button
              danger
              v-access:code="['system:menu:remove']"
              v-access:role="['superadmin']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <MenuDrawer @reload="afterEditOrAdd" />
  </Page>
  <Fallback v-else description="您没有菜单管理的访问权限" status="403" />
</template>
