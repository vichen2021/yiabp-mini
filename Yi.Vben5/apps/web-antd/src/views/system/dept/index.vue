<script setup lang="ts">
  import type { VbenFormProps } from '@vben/common-ui';
  
  import type { VxeGridProps } from '#/adapter/vxe-table';
  import type { Dept } from '#/api/system/dept/model';
  
  import { nextTick } from 'vue';
  
  import { Page, useVbenDrawer } from '@vben/common-ui';
  import { eachTree } from '@vben/utils';
  
  import { Button, Space } from 'antdv-next';
  
  import { useVbenVxeGrid, VbenTableAction } from '#/adapter/vxe-table';
  import { deptList, deptRemove } from '#/api/system/dept';
  import { emptyGuidToNull } from '#/utils/guid';
  
  import { columns, querySchema } from './data';
  import deptDrawer from './dept-drawer.vue';
  
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
          // 将根节点的 parentId 置为 null，以便 vxe-table 正确识别根节点
          const items = resp.map((item) => ({
            ...item,
            parentId: emptyGuidToNull(item.parentId),
          }));
          return { items };
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
    rowConfig: {
      keyField: 'id',
    },
    treeConfig: {
      parentField: 'parentId',
      rowField: 'id',
      transform: true,
    },
    id: 'system-dept-index',
  };
  
  const [BasicTable, tableApi] = useVbenVxeGrid({
    formOptions,
    gridOptions,
    gridEvents: {
      cellDblclick: (e: any) => {
        const { row = {} } = e;
        if (!row?.children) {
          return;
        }
        const isExpanded = row?.expand;
        tableApi.grid.setTreeExpand(row, !isExpanded);
        row.expand = !isExpanded;
      },
      // 需要监听使用箭头展开的情况 否则展开/折叠的数据不一致
      toggleTreeExpand: (e: any) => {
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
  const { id } = row;
  drawerApi.setData({ id, update: false });
  drawerApi.open();
}
  
async function handleEdit(record: Dept) {
  drawerApi.setData({ id: record.id, update: true });
  drawerApi.open();
}
  
async function handleDelete(row: Dept) {
  await deptRemove(row.id);
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
            <Button @click="setExpandOrCollapse(false)">
              {{ $t('pages.common.collapse') }}
            </Button>
            <Button @click="setExpandOrCollapse(true)">
              {{ $t('pages.common.expand') }}
            </Button>
            <Button
              type="primary"
              v-access:code="['system:dept:add']"
              @click="handleAdd"
            >
              {{ $t('pages.common.add') }}
            </Button>
          </Space>
        </template>
        <template #action="{ row }">
          <VbenTableAction
            :actions="[
              {
                auth: 'system:dept:edit',
                onClick: () => handleEdit(row),
                text: $t('pages.common.edit'),
              },
              {
                auth: 'system:dept:add',
                class: 'text-green-600 hover:text-green-700',
                onClick: () => handleSubAdd(row),
                text: $t('pages.common.add'),
              },
              {
                auth: 'system:dept:remove',
                danger: true,
                popConfirm: {
                  title: '确认删除？',
                  confirm: () => handleDelete(row),
                },
                text: $t('pages.common.delete'),
              },
            ]"
            align="center"
          />
        </template>
      </BasicTable>
      <DeptDrawer @reload="tableApi.query()" />
    </Page>
  </template>
  
