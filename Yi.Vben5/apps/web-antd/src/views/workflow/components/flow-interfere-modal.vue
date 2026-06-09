<script setup lang="ts">
import type { User } from '#/api/system/user/model';
import type { TaskInfo } from '#/api/workflow/task/model';

import { computed, ref } from 'vue';

import {
  useVbenModal,
  VbenDescriptions,
  VbenDescriptionsItem,
} from '@vben/common-ui';

import { Button } from 'antdv-next';

import {
  getTaskByTaskId,
  taskOperation,
  terminationTask,
} from '#/api/workflow/task';
import { confirmDangerAction } from '#/utils/modal';

import { userSelectModal } from '.';

const emit = defineEmits<{ complete: [] }>();

const taskInfo = ref<TaskInfo>();

/**
 * 是否显示 加签/减签操作
 */
const showMultiActions = computed(() => {
  if (!taskInfo.value) {
    return false;
  }
  if (Number(taskInfo.value.nodeRatio) > 0) {
    return true;
  }
  return false;
});

const [BasicModal, modalApi] = useVbenModal({
  title: '流程干预',
  class: 'w-[800px]',
  fullscreenButton: false,
  async onOpenChange(isOpen) {
    if (!isOpen) {
      return null;
    }
    const { taskId } = modalApi.getData() as { taskId: string };
    taskInfo.value = await getTaskByTaskId(taskId);
  },
});

/**
 * 转办
 */
const [TransferModal, transferModalApi] = useVbenModal({
  connectedComponent: userSelectModal,
});
function handleTransfer(userList: User[]) {
  if (userList.length === 0 || !taskInfo.value) return;
  const current = userList[0];
  confirmDangerAction({
    title: '转办',
    content: `确定转办给${current?.nick}吗?`,
    onConfirmed: async () => {
      await taskOperation(
        { taskId: taskInfo.value!.id, userId: current!.id },
        'transferTask',
      );
      emit('complete');
    },
  });
}

/**
 * 审批终止
 */
function handleTermination() {
  if (!taskInfo.value) {
    return;
  }
  confirmDangerAction({
    title: '审批终止',
    content: '确定终止当前审批流程吗？',
    onConfirmed: async () => {
      await terminationTask({ taskId: taskInfo.value!.id });
      emit('complete');
    },
  });
}

const [AddSignatureModal, addSignatureModalApi] = useVbenModal({
  connectedComponent: userSelectModal,
});
function handleAddSignature(userList: User[]) {
  if (userList.length === 0 || !taskInfo.value) return;
  const userIds = userList.map((user) => user.id);
  confirmDangerAction({
    content: '确认加签吗?',
    onConfirmed: async () => {
      await taskOperation(
        { taskId: taskInfo.value!.id, userIds },
        'addSignature',
      );
      emit('complete');
    },
  });
}

const [ReductionSignatureModal, reductionSignatureModalApi] = useVbenModal({
  connectedComponent: userSelectModal,
});
function handleReductionSignature(userList: User[]) {
  if (userList.length === 0 || !taskInfo.value) return;
  const userIds = userList.map((user) => user.id);
  confirmDangerAction({
    content: '确认减签吗?',
    onConfirmed: async () => {
      await taskOperation(
        { taskId: taskInfo.value!.id, userIds },
        'reductionSignature',
      );
      emit('complete');
    },
  });
}
</script>

<template>
  <BasicModal>
    <VbenDescriptions v-if="taskInfo" :column="2" bordered size="small">
      <VbenDescriptionsItem label="任务名称">
        {{ taskInfo.nodeName }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="节点编码">
        {{ taskInfo.nodeCode }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="开始时间">
        {{ taskInfo.createTime }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="流程实例ID">
        {{ taskInfo.instanceId }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="版本号">
        {{ taskInfo.version }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="业务ID">
        {{ taskInfo.businessId }}
      </VbenDescriptionsItem>
    </VbenDescriptions>
    <TransferModal mode="single" @finish="handleTransfer" />
    <AddSignatureModal mode="multiple" @finish="handleAddSignature" />
    <ReductionSignatureModal
      mode="multiple"
      @finish="handleReductionSignature"
    />
    <template #footer>
      <template v-if="showMultiActions">
        <Button @click="() => addSignatureModalApi.open()">加签</Button>
        <Button @click="() => reductionSignatureModalApi.open()">
          减签
        </Button>
      </template>
      <Button @click="() => transferModalApi.open()">转办</Button>
      <Button danger type="primary" @click="handleTermination">终止</Button>
    </template>
  </BasicModal>
</template>
