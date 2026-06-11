<script setup lang="ts">
import type { PropType } from 'vue';

import type { Dept } from '#/api/system/dept/model';

import { onMounted, ref } from 'vue';

import { SyncOutlined } from '@antdv-next/icons';
import { Button, Empty, Input, Skeleton, SpaceCompact, Tree } from 'antdv-next';

import { listToTree } from '@vben/utils';

import { deptSelectList } from '#/api/system/dept';

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

const selectDeptId = defineModel('selectDeptId', {
  required: true,
  type: Array as PropType<string[]>,
});

const searchValue = defineModel('searchValue', {
  type: String,
  default: '',
});

/** 部门数据源 */
const deptTreeArray = ref<Dept[]>([]);
/** 骨架屏加载 */
const showTreeSkeleton = ref<boolean>(true);

async function loadTree() {
  showTreeSkeleton.value = true;
  searchValue.value = '';
  selectDeptId.value = [];

  const ret = await deptSelectList();
  deptTreeArray.value = listToTree(ret, { id: 'id', pid: 'parentId' });
  showTreeSkeleton.value = false;
}

async function handleReload() {
  await loadTree();
  emit('reload');
}

onMounted(loadTree);
</script>

<template>
  <div :class="$attrs.class">
    <Skeleton
      :loading="showTreeSkeleton"
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
          <SpaceCompact class="w-full">
            <Input
              v-model:value="searchValue"
              :placeholder="$t('pages.common.search')"
              size="small"
              allow-clear
            />
            <Button size="small" @click="handleReload">
              <SyncOutlined class="text-primary" />
            </Button>
          </SpaceCompact>
        </div>
        <div class="h-full overflow-x-hidden px-[8px]">
          <Tree
            v-bind="$attrs"
            v-if="deptTreeArray.length > 0"
            v-model:selected-keys="selectDeptId"
            :class="$attrs.class"
            :field-names="{
              title: 'deptName',
              key: 'id',
              children: 'children',
            }"
            :show-line="{ showLeafIcon: false }"
            :tree-data="deptTreeArray as any"
            :virtual="false"
            default-expand-all
            @select="$emit('select')"
          >
            <template #titleRender="{ deptName }">
              <span v-if="deptName.includes(searchValue)">
                {{ deptName.substring(0, deptName.indexOf(searchValue)) }}
                <span class="text-primary">{{ searchValue }}</span>
                {{
                  deptName.substring(
                    deptName.indexOf(searchValue) + searchValue.length,
                  )
                }}
              </span>
              <span v-else>{{ deptName }}</span>
            </template>
          </Tree>
          <!-- 仅本人数据权限 可以考虑直接不显示 -->
          <div v-else class="mt-5">
            <Empty
              :image="Empty.PRESENTED_IMAGE_SIMPLE"
              description="无部门数据"
            />
          </div>
        </div>
      </div>
    </Skeleton>
  </div>
</template>
