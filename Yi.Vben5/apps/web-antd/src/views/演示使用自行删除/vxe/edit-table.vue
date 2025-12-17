<script setup lang="tsx">
import type { VxeGridProps } from '#/adapter/vxe-table';

import { nextTick, onMounted } from 'vue';

import { JsonPreview } from '@vben/common-ui';
import { getPopupContainer } from '@vben/utils';

import {
  Button,
  Input,
  InputNumber,
  message,
  Modal,
  Select,
  Space,
} from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';

const gridOptions: VxeGridProps = {
  editConfig: {
    // 触发编辑的方式
    trigger: 'click',
    // 触发编辑的模式
    mode: 'row',
    showStatus: true,
  },
  border: true,
  rowConfig: {
    drag: true,
  },
  checkboxConfig: {},
  editRules: {
    name: [{ required: true, message: '请输入姓名', trigger: 'blur' }],
    age: [
      { required: true, message: '请输入年龄', trigger: 'blur' },
      { min: 0, max: 200, message: '年龄必须为1-200' },
    ],
    job: [{ required: true, message: '请选择工作', trigger: 'blur' }],
  },
  columns: [
    {
      type: 'checkbox',
      width: 60,
    },
    {
      dragSort: true,
      title: '排序',
      width: 60,
    },
    {
      field: 'name',
      title: '姓名',
      align: 'left',
      editRender: {},
      slots: {
        default: ({ row }) => {
          if (!row.name) {
            return <span class="text-red-500">未填写</span>;
          }
          return <span>{row.name}</span>;
        },
        edit: ({ row }) => {
          return <Input placeholder={'请输入'} v-model:value={row.name} />;
        },
      },
    },
    {
      field: 'age',
      title: '年龄',
      align: 'left',
      editRender: {},
      slots: {
        default: ({ row }) => {
          if (!row.age) {
            return <span class="text-red-500">未填写</span>;
          }
          return <span>{row.age}</span>;
        },
        edit: ({ row }) => {
          return (
            <InputNumber
              class="w-full"
              placeholder={'请输入'}
              v-model:value={row.age}
            />
          );
        },
      },
    },
    {
      field: '工作',
      title: 'job',
      align: 'left',
      editRender: {},
      slots: {
        default: ({ row }) => {
          if (!row.job) {
            return <span class="text-red-500">未选择</span>;
          }
          return <span>{row.job}</span>;
        },
        edit: ({ row }) => {
          const options = ['前端佬', '后端佬', '组长'].map((item) => ({
            label: item,
            value: item,
          }));
          return (
            <Select
              class="w-full"
              getPopupContainer={getPopupContainer}
              options={options}
              placeholder={'请选择'}
              v-model:value={row.job}
            />
          );
        },
      },
    },
    {
      field: 'action',
      title: '操作',
      width: 100,
      slots: {
        default: ({ $table, row }) => {
          function handleDelete() {
            $table.remove(row);
          }
          return (
            <Button danger={true} onClick={handleDelete} size={'small'}>
              删除
            </Button>
          );
        },
      },
    },
  ],
  height: 'auto',
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    enabled: false,
  },
  toolbarConfig: {
    enabled: false,
  },
  showOverflow: false,
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  gridOptions,
});

onMounted(async () => {
  const data = [
    {
      name: '张三',
      age: 18,
      job: '前端佬',
    },
    {
      name: '李四',
      age: 19,
      job: '后端佬',
    },
    {
      name: '王五',
      age: 20,
      job: '组长',
    },
  ];
  await nextTick();
  await tableApi.grid.loadData(data);
});
async function handleAdd() {
  const record = { name: '', age: undefined, job: undefined };
  const { row: newRow } = await tableApi.grid.insert(record);
  await tableApi.grid.setEditCell(newRow, 'name');
}

async function handleRemove() {
  await tableApi.grid.removeCheckboxRow();
}

async function handleValidate() {
  const result = await tableApi.grid.validate(true);
  if (result) {
    message.error('校验失败');
  } else {
    message.success('校验成功');
  }
}

function getData() {
  const data = tableApi.grid.getTableData();
  const { fullData } = data;
  console.log(fullData);
  Modal.info({
    title: '提示',
    content: (
      <div class="max-h-[350px] overflow-y-auto">
        <JsonPreview data={fullData} />
      </div>
    ),
  });
}
</script>

<template>
  <BasicTable>
    <template #toolbar-tools>
      <Space>
        <a-button @click="getData">获取表格数据</a-button>
        <a-button @click="handleValidate">校验</a-button>
        <a-button danger @click="handleRemove"> 删除勾选 </a-button>
        <a-button
          type="primary"
          v-access:code="['system:config:add']"
          @click="handleAdd"
        >
          {{ $t('pages.common.add') }}
        </a-button>
      </Space>
    </template>
  </BasicTable>
</template>
