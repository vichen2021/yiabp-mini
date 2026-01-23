<script setup lang="ts">
import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Checkbox } from 'ant-design-vue';

import type { Table } from '#/api/code/table/model';
import type { Template } from '#/api/code/template/model';
import { templateList } from '#/api/code/template';
import { postWebBuildCode } from '#/api/code/code-gen';

const emit = defineEmits<{ reload: [] }>();

const selectedTableIds = ref<string[]>([]);
const selectedTables = ref<Table[]>([]);
const templates = ref<Template[]>([]);
const selectedTemplateIds = ref<string[]>([]);
const loading = ref(false);

// 加载模板列表
async function loadTemplates() {
  try {
    const result = await templateList({ MaxResultCount: 1000 });
    templates.value = result.items;
    // 默认选中所有模板（后端目前会使用所有模板，这里只是展示）
    selectedTemplateIds.value = templates.value.map((t) => t.id);
  } catch (error) {
    console.error('加载模板失败:', error);
  }
}

const [BasicModal, modalApi] = useVbenModal({
  onOpenChange: async (isOpen) => {
    if (!isOpen) {
      return null;
    }
    await loadTemplates();
    const data = modalApi.getData() as {
      tableIds: string[];
      tables: Table[];
    };
    if (data) {
      selectedTableIds.value = data.tableIds || [];
      selectedTables.value = data.tables || [];
    }
  },
  onConfirm: handleConfirm,
});

const tableNames = computed(() => {
  return selectedTables.value.map((t) => t.name).join('、');
});

async function handleConfirm() {
  if (selectedTableIds.value.length === 0) {
    return false;
  }

  try {
    loading.value = true;
    // 注意：后端目前会使用所有模板生成代码，不支持选择特定模板
    await postWebBuildCode(selectedTableIds.value);
    emit('reload');
    modalApi.close();
    return true;
  } catch (error) {
    console.error('生成代码失败:', error);
    return false;
  } finally {
    loading.value = false;
  }
}

// 全选/取消全选模板
function handleSelectAllTemplates(checked: boolean) {
  if (checked) {
    selectedTemplateIds.value = templates.value.map((t) => t.id);
  } else {
    selectedTemplateIds.value = [];
  }
}

const allTemplatesSelected = computed(() => {
  return (
    templates.value.length > 0 &&
    selectedTemplateIds.value.length === templates.value.length
  );
});
</script>

<template>
  <BasicModal
    :confirm-loading="loading"
    title="代码生成"
    width="900px"
    @ok="handleConfirm"
  >
    <div class="space-y-4">
      <!-- 选择的表 -->
      <div>
        <div class="mb-2 font-medium">已选择的表：</div>
        <div class="rounded bg-gray-50 p-3">
          <div v-if="selectedTables.length > 0" class="text-sm">
            {{ tableNames }}
            <span class="ml-2 text-gray-500">
              (共 {{ selectedTables.length }} 个表)
            </span>
          </div>
          <div v-else class="text-sm text-gray-400">未选择表</div>
        </div>
      </div>

      <!-- 模板选择 -->
      <div>
        <div class="mb-2 flex items-center justify-between">
          <span class="font-medium">选择模板：</span>
          <Checkbox
            :checked="allTemplatesSelected"
            @change="(e) => handleSelectAllTemplates(e.target.checked)"
          >
            全选
          </Checkbox>
        </div>
        <div
          class="max-h-[300px] space-y-2 overflow-y-auto rounded border p-3"
        >
          <div
            v-for="template in templates"
            :key="template.id"
            class="flex items-start space-x-2"
          >
            <Checkbox
              :checked="selectedTemplateIds.includes(template.id)"
              @change="
                (e) => {
                  if (e.target.checked) {
                    selectedTemplateIds.push(template.id);
                  } else {
                    selectedTemplateIds = selectedTemplateIds.filter(
                      (id) => id !== template.id,
                    );
                  }
                }
              "
            />
            <div class="flex-1">
              <div class="font-medium">{{ template.name }}</div>
              <div class="text-xs text-gray-500">
                {{ template.buildPath }}
              </div>
              <div v-if="template.remarks" class="mt-1 text-xs text-gray-400">
                {{ template.remarks }}
              </div>
            </div>
          </div>
          <div v-if="templates.length === 0" class="text-center text-gray-400">
            暂无模板，请先在模板管理中创建模板
          </div>
        </div>
        <div class="mt-2 text-xs text-gray-500">
          已选择 {{ selectedTemplateIds.length }} / {{ templates.length }} 个模板
        </div>
      </div>

      <!-- 生成说明 -->
      <div class="rounded bg-blue-50 p-3">
        <div class="text-sm text-blue-800">
          <div class="mb-1 font-medium">生成说明：</div>
          <ul class="list-inside list-disc space-y-1">
            <li>
              将为选中的 {{ selectedTables.length }} 个表生成代码
            </li>
            <li>
              将使用所有模板生成代码文件（当前有 {{ templates.length }} 个模板）
            </li>
            <li>生成的文件将保存到模板配置的路径中</li>
            <li>模板中的占位符将被替换为实际的表名和字段信息</li>
          </ul>
        </div>
      </div>
    </div>
  </BasicModal>
</template>
