<!-- eslint-disable no-use-before-define -->
<script setup lang="ts">
import type { RadioChangeEvent } from 'antdv-next';

import type { VbenFormProps } from '@vben/common-ui';
import type { Recordable } from '@vben/types';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { computed, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { Button, message, RadioGroup, Space, Switch } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  unPublishList,
  workflowDefinitionActive,
  workflowDefinitionCopy,
  workflowDefinitionDelete,
  workflowDefinitionExport,
  workflowDefinitionList,
  workflowDefinitionPublish,
} from '#/api/workflow/definition';
import { downloadByData } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import CategoryTree from './category-tree.vue';
import { columns, querySchema } from './data';
import processDefinitionDeployModal from './process-definition-deploy-modal.vue';
import processDefinitionModal from './process-definition-modal.vue';

// 左边部门用
const selectedCode = ref<number[] | string[]>([]);

const formOptions: VbenFormProps = {
  schema: querySchema(),
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
  handleReset: async () => {
    selectedCode.value = [];
    const { formApi, reload } = tableApi;
    await formApi.resetForm();
    const formValues = formApi.form.values;
    formApi.setLatestSubmissionValues(formValues);
    await reload(formValues);
  },
};

const gridOptions: VxeGridProps = {
  checkboxConfig: {
    // 高亮
    highlight: true,
    // 翻页时保留选中状态
    reserve: true,
    // 点击行选中
    trigger: 'default',
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        // 部门树选择处理
        if (selectedCode.value.length === 1) {
          formValues.category = selectedCode.value[0];
        } else {
          Reflect.deleteProperty(formValues, 'category');
        }

        return await currentTableApi.value({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  headerCellConfig: {
    height: 44,
  },
  cellConfig: {
    height: 100,
  },
  rowConfig: {
    keyField: 'id',
  },
  id: 'workflow-definition-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

// 左边的切换
const statusOptions = [
  { label: '已发布流程', value: 1 },
  { label: '未发布流程', value: 0 },
];
const currentStatus = ref(1);
const currentTableApi = computed(() => {
  if (currentStatus.value === 1) {
    return workflowDefinitionList;
  }
  return unPublishList;
});
async function handleStatusChange(e: RadioChangeEvent) {
  currentStatus.value = e.target.value as number;
  await tableApi.reload();
}

async function handleDelete(row: Recordable<any>) {
  await workflowDefinitionDelete(row.id);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: any) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await workflowDefinitionDelete(ids);
      await tableApi.query();
    },
  });
}

const router = useRouter();
/**
 * 流程设计/预览
 * @param row row
 * @param disabled true为预览，false为设计
 */
function handleDesign(row: any, disabled: boolean) {
  router.push({
    path: '/workflow/design/index',
    query: { definitionId: row.id, disabled: String(disabled) },
  });
}

/**
 * 激活/挂起流程
 * @param row row
 */
async function handleActive(row: any, status: boolean | number | string) {
  const lastStatus = status === 1 ? 0 : 1;
  try {
    await workflowDefinitionActive(row.id, !!status);
    await tableApi.query();
  } catch (error) {
    row.activityStatus = lastStatus;
    console.error(error);
  }
}

/**
 * 发布流程
 * @param row row
 */
async function handlePublish(row: any) {
  await workflowDefinitionPublish(row.id);
  await tableApi.query();
}

/**
 * 复制流程
 * @param row row
 */
async function handleCopy(row: any) {
  await workflowDefinitionCopy(row.id);
  // 跳转到未发布流程tab
  currentStatus.value = 0;
  await tableApi.reload();
}

const [ProcessDefinitionModal, modalApi] = useVbenModal({
  connectedComponent: processDefinitionModal,
});

/**
 * 新增流程
 */
function handleAdd() {
  modalApi.setData({});
  modalApi.open();
}

/**
 * 编辑流程
 */
function handleEdit(row: any) {
  modalApi.setData({ id: row.id });
  modalApi.open();
}

/**
 * 导出xml
 * @param row row
 */
async function handleExportXml(row: any) {
  const hideLoading = message.loading($t('pages.common.downloadLoading'), 0);
  try {
    const blob = await workflowDefinitionExport(row.id);
    downloadByData(blob, `${row.flowName}-${Date.now()}.json`);
  } catch (error) {
    console.error(error);
  } finally {
    hideLoading();
  }
}

const [ProcessDefinitionDeployModal, deployModalApi] = useVbenModal({
  connectedComponent: processDefinitionDeployModal,
});

/**
 * 部署流程xml
 */
function handleDeploy() {
  if (selectedCode.value.length === 0) {
    message.warning('请先选择流程分类');
    return;
  }
  const selectedCategory = selectedCode.value[0];
  if (selectedCategory === 0) {
    message.warning('不可选择根目录进行部署, 请选择子分类');
    return;
  }
  deployModalApi.setData({ category: selectedCategory });
  deployModalApi.open();
}

// 部署流程json
async function handleDeploySuccess() {
  // 跳转到未发布
  currentStatus.value = 0;
  await tableApi.reload();
}

// 新增完成需要跳转到未发布
async function handleReload(type: 'add' | 'update') {
  if (type === 'add') {
    currentStatus.value = 0;
  }
  await tableApi.reload();
}
</script>

<template>
  <Page :auto-content-height="true">
    <div class="flex h-full gap-[8px]">
      <CategoryTree
        v-model:select-code="selectedCode"
        class="w-[260px]"
        @reload="() => tableApi.reload()"
        @select="() => tableApi.reload()"
      />
      <BasicTable class="flex-1 overflow-hidden">
        <template #toolbar-actions>
          <RadioGroup
            v-model:value="currentStatus"
            :options="statusOptions"
            button-style="solid"
            option-type="button"
            @change="handleStatusChange"
          />
        </template>
        <template #toolbar-tools>
          <Space>
            <Button
              :disabled="!vxeCheckboxChecked(tableApi)"
              danger
              type="primary"
              v-access:code="['system:user:remove']"
              @click="handleMultiDelete"
            >
              {{ $t('pages.common.delete') }}
            </Button>
            <Button v-access:code="['system:user:add']" @click="handleDeploy">
              部署
            </Button>
            <Button
              type="primary"
              v-access:code="['system:user:add']"
              @click="handleAdd"
            >
              {{ $t('pages.common.add') }}
            </Button>
          </Space>
        </template>
        <template #activityStatus="{ row }">
          <Switch
            v-model:checked="row.activityStatus"
            :checked-value="1"
            :unchecked-value="0"
            checked-children="激活"
            un-checked-children="挂起"
            @change="(status) => handleActive(row, status)"
          />
        </template>
        <template #action="{ row }">
          <VbenTableAction
            :actions="[
              {
                onClick: () => handleEdit(row),
                text: '编辑信息',
              },
              {
                danger: true,
                popConfirm: {
                  title: '确认删除？',
                  confirm: () => handleDelete(row),
                },
                text: '删除流程',
              },
              {
                onClick: () => handleDesign(row, !!row.isPublish),
                text: row.isPublish ? '查看流程' : '设计流程',
              },
              {
                ifShow: !row.isPublish,
                popConfirm: {
                  title: `确认发布流程[${row.flowName}]?`,
                  confirm: () => handlePublish(row),
                },
                text: '发布流程',
              },
              {
                popConfirm: {
                  title: `确认复制流程[${row.flowName}]?`,
                  confirm: () => handleCopy(row),
                },
                text: '复制流程',
              },
              {
                onClick: () => handleExportXml(row),
                text: '导出流程',
              },
            ]"
            align="center"
          />
        </template>
      </BasicTable>
    </div>
    <ProcessDefinitionModal @reload="handleReload" />
    <ProcessDefinitionDeployModal @reload="handleDeploySuccess" />
  </Page>
</template>
