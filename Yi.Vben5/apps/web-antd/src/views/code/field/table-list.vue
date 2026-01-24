<script setup lang="ts">
import type { PropType } from 'vue';

import { onMounted, ref } from 'vue';

import { SyncOutlined } from '@ant-design/icons-vue';
import { Empty, InputSearch, List, ListItem, Skeleton } from 'ant-design-vue';

import { tableSelectList } from '#/api/code/table';

import type { Table } from '#/api/code/table/model';

defineOptions({ inheritAttrs: false });

withDefaults(defineProps<{ showSearch?: boolean }>(), { showSearch: true });

const emit = defineEmits<{
  /**
   * 点击刷新按钮的事件
   */
  reload: [];
  /**
   * 点击节点的事件
   */
  select: [];
}>();

const selectTableId = defineModel('selectTableId', {
  required: true,
  type: String as PropType<string>,
  default: '',
});

const searchValue = defineModel('searchValue', {
  type: String,
  default: '',
});

/** 表数据源 */
const tableArray = ref<Table[]>([]);
/** 骨架屏加载 */
const showTableSkeleton = ref<boolean>(true);

async function loadTable() {
  showTableSkeleton.value = true;
  searchValue.value = '';
  selectTableId.value = '';

  const ret = await tableSelectList();
  tableArray.value = ret;
  showTableSkeleton.value = false;
}

async function handleReload() {
  await loadTable();
  emit('reload');
}

function handleSelectTable(tableId: string) {
  selectTableId.value = tableId;
  emit('select');
}

// 根据搜索值过滤表
const filteredTables = () => {
  if (!searchValue.value) {
    return tableArray.value;
  }
  return tableArray.value.filter(table =>
    table.name.toLowerCase().includes(searchValue.value.toLowerCase())
    || table.description?.toLowerCase().includes(searchValue.value.toLowerCase()),
  );
};

onMounted(loadTable);
</script>

<template>
  <div :class="$attrs.class">
    <Skeleton
      :loading="showTableSkeleton"
      :paragraph="{ rows: 8 }"
      active
      class="p-[8px]"
    >
      <div
        class="bg-background flex h-full flex-col overflow-y-auto rounded-lg"
      >
        <!-- 固定在顶部 必须加上bg-background背景色 否则会产生'穿透'效果 -->
        <div
          v-if="showSearch"
          class="bg-background z-100 sticky left-0 top-0 p-[8px]"
        >
          <InputSearch
            v-model:value="searchValue"
            :placeholder="$t('pages.common.search')"
            size="small"
            allow-clear
          >
            <template #enterButton>
              <a-button @click="handleReload">
                <SyncOutlined class="text-primary" />
              </a-button>
            </template>
          </InputSearch>
        </div>
        <div class="h-full overflow-x-hidden px-[8px] py-[8px]">
          <List
            v-if="filteredTables().length > 0"
            :data-source="filteredTables()"
            size="small"
            :split="false"
          >
            <template #renderItem="{ item }">
              <ListItem
                :class="[
                  'cursor-pointer rounded px-2 py-1',
                  selectTableId === item.id
                    ? 'bg-primary/10 text-primary'
                    : 'hover:bg-gray-100',
                ]"
                @click="handleSelectTable(item.id)"
              >
                <div class="w-full truncate">
                  <div class="font-medium truncate">{{ item.name }}</div>
                  <div
                    v-if="item.description"
                    class="text-xs text-gray-500 truncate"
                  >
                    {{ item.description }}
                  </div>
                </div>
              </ListItem>
            </template>
          </List>
          <!-- 无数据 -->
          <div v-else class="mt-5">
            <Empty
              :image="Empty.PRESENTED_IMAGE_SIMPLE"
              description="无表数据"
            />
          </div>
        </div>
      </div>
    </Skeleton>
  </div>
</template>
