<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Dept } from '#/api/system/dept/model';

import { nextTick } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { eachTree, getVxePopupContainer } from '@vben/utils';

import { Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { deptList, deptRemove } from '#/api/system/dept';

import { columns, querySchema } from './data';
import deptDrawer from './dept-drawer.vue';

// 构建树形结构的辅助函数
function buildTree(data: any[], parentId: string = '00000000-0000-0000-0000-000000000000') {
  return data
    .filter((item) => item.parentId === parentId)
    .map((item) => {
      const children = buildTree(data, item.id);
      return {
        ...item,
        children: children.length > 0 ? children : undefined,
      };
    });
}

// 根据实际后端返回数据调整字段名
function transformDeptData(dept: any) {
  return {
    ...dept,
    // 映射后端字段名到表格需要的字段名
    id: dept.id,
    deptId: dept.id,
    deptName: dept.deptName,
    deptCode: dept.deptCode,
    leader: dept.leader,
    parentId: dept.parentId,
    remark: dept.remark,
    orderNum: dept.orderNum,
    createTime: dept.creationTime,
    status: dept.state ? '0' : '1', // 假设 state=true 表示正常状态
  };
}

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

const gridOptions: VxeGridProps = {
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const resp = await deptList({
          ...formValues,
        });
        // 将扁平数据转换为树形结构
        const flatData = resp.items || [];
        // 转换数据字段以匹配表格需要的字段名
        const transformedData = flatData.map(transformDeptData);
        const treeData = buildTree(transformedData);
        return {
          ...resp,
          items: treeData,
        };
      },
      // 默认请求接口后展开全部 不需要可以删除这段
      querySuccess: () => {
        // 默认展开 需要加上标记
        // eslint-disable-next-line no-use-before-define
        eachTree(tableApi.grid.getData(), (item) => (item.expand = true));
        nextTick(() => {
          setExpandOrCollapse(true);
        });
      },
    },
  },
  /**
   * 虚拟滚动  默认关闭
   */
  scrollY: {
    enabled: false,
    gt: 0,
  },
  rowConfig: {
    keyField: 'deptId',
  },
  treeConfig: {
    childrenField: 'children',
    parentField: 'parentId',
    rowField: 'deptId',
    transform: false, // 前端已处理为树形结构，不需要表格转换
  },
  id: 'system-dept-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
  gridEvents: {
    cellDblclick: (e) => {
      const { row = {} } = e;
      if (!row?.children) {
        return;
      }
      const isExpanded = row?.expand;
      tableApi.grid.setTreeExpand(row, !isExpanded);
      row.expand = !isExpanded;
    },
    // 需要监听使用箭头展开的情况 否则展开/折叠的数据不一致
    toggleTreeExpand: (e) => {
      const { row = {}, expanded } = e;
      row.expand = expanded;
    },
  },
});
const [DeptDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: deptDrawer,
});

function handleAdd() {
  drawerApi.setData({ update: false });
  drawerApi.open();
}

function handleSubAdd(row: Dept) {
  const { deptId } = row;
  drawerApi.setData({ id: deptId, update: false });
  drawerApi.open();
}

async function handleEdit(record: Dept) {
  drawerApi.setData({ id: record.deptId, update: true });
  drawerApi.open();
}

async function handleDelete(row: Dept) {
  await deptRemove(row.deptId);
  await tableApi.query();
}

/**
 * 全部展开/折叠
 * @param expand 是否展开
 */
function setExpandOrCollapse(expand: boolean) {
  eachTree(tableApi.grid.getData(), (item) => (item.expand = expand));
  tableApi.grid?.setAllTreeExpand(expand);
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="部门列表" table-title-help="双击展开/收起子菜单">
      <template #toolbar-tools>
        <Space>
          <a-button @click="setExpandOrCollapse(false)">
            {{ $t('pages.common.collapse') }}
          </a-button>
          <a-button @click="setExpandOrCollapse(true)">
            {{ $t('pages.common.expand') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['system:dept:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['system:dept:edit']"
            @click="handleEdit(row)"
          >
            {{ $t('pages.common.edit') }}
          </ghost-button>
          <ghost-button
            class="btn-success"
            v-access:code="['system:dept:add']"
            @click="handleSubAdd(row)"
          >
            {{ $t('pages.common.add') }}
          </ghost-button>
          <Popconfirm
            :get-popup-container="getVxePopupContainer"
            placement="left"
            title="确认删除？"
            @confirm="handleDelete(row)"
          >
            <ghost-button
              danger
              v-access:code="['system:dept:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <DeptDrawer @reload="tableApi.query()" />
  </Page>
</template>
