<script setup lang="ts">
import type { ServerInfo } from '#/api/service/model';

import { onMounted, ref } from 'vue';

import {
  Page,
  VbenDescriptions,
  VbenDescriptionsItem,
} from '@vben/common-ui';

import { Button, Card, Col, Row, Spin, Table } from 'antdv-next';

import { getServerInfo } from '#/api/service';

const server = ref<ServerInfo>();
const loading = ref(false);

const diskColumns = [
  {
    title: '盘符路径',
    dataIndex: 'diskName',
    key: 'diskName',
  },
  {
    title: '盘符类型',
    dataIndex: 'typeName',
    key: 'typeName',
  },
  {
    title: '总大小',
    dataIndex: 'totalSize',
    key: 'totalSize',
  },
  {
    title: '可用大小',
    dataIndex: 'availableFreeSpace',
    key: 'availableFreeSpace',
  },
  {
    title: '已用大小',
    dataIndex: 'used',
    key: 'used',
  },
  {
    title: '已用百分比',
    dataIndex: 'availablePercent',
    key: 'availablePercent',
  },
];

async function loadData() {
  loading.value = true;
  try {
    server.value = await getServerInfo();
  } catch (error) {
    console.warn(error);
  } finally {
    loading.value = false;
  }
}

onMounted(() => {
  loadData();
});
</script>

<template>
  <Page>
    <Spin :spinning="loading" tip="正在加载服务监控数据，请稍候...">
      <Row :gutter="[15, 15]">
      <Col :lg="12" :md="24" :sm="24" :xl="12" :xs="24">
        <Card size="small">
          <template #title>
            <span>CPU</span>
          </template>
          <template #extra>
            <Button size="small" @click="loadData">
              <div class="flex">
                <span class="icon-[charm--refresh]"></span>
              </div>
            </Button>
          </template>
          <VbenDescriptions
            v-if="server?.cpu"
            bordered
            :column="1"
            size="small"
          >
            <VbenDescriptionsItem label="核心数">
              {{ server.cpu.coreTotal }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="逻辑处理器">
              {{ server.cpu.logicalProcessors }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="系统使用率">
              {{ server.cpu.cpuRate }}%
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="当前空闲率">
              {{ 100 - server.cpu.cpuRate }}%
            </VbenDescriptionsItem>
          </VbenDescriptions>
        </Card>
      </Col>

      <Col :lg="12" :md="24" :sm="24" :xl="12" :xs="24">
        <Card size="small">
          <template #title>
            <span>内存</span>
          </template>
          <VbenDescriptions
            v-if="server?.memory"
            bordered
            :column="1"
            size="small"
          >
            <VbenDescriptionsItem label="总内存">
              {{ server.memory.totalRAM }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="已用内存">
              {{ server.memory.usedRam }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="剩余内存">
              {{ server.memory.freeRam }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="使用率">
              <span :class="{ 'text-danger': server.memory.ramRate > 80 }">
                {{ server.memory.ramRate }}%
              </span>
            </VbenDescriptionsItem>
          </VbenDescriptions>
        </Card>
      </Col>

      <Col :span="24">
        <Card size="small">
          <template #title>
            <span>服务器信息</span>
          </template>
          <VbenDescriptions
            v-if="server?.sys"
            bordered
            :column="2"
            size="small"
          >
            <VbenDescriptionsItem label="服务器名称">
              {{ server.sys.computerName }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="操作系统">
              {{ server.sys.osName }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="服务器IP">
              {{ server.sys.serverIP }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="系统架构">
              {{ server.sys.osArch }}
            </VbenDescriptionsItem>
          </VbenDescriptions>
        </Card>
      </Col>

      <Col :span="24">
        <Card size="small">
          <template #title>
            <span>应用信息</span>
          </template>
          <VbenDescriptions
            v-if="server?.app"
            bordered
            :column="2"
            size="small"
          >
            <VbenDescriptionsItem label="应用环境">
              {{ server.app.name }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="应用版本">
              {{ server.app.version }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="启动时间">
              {{ server.app.startTime }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="运行时长">
              {{ server.app.runTime }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="安装路径" :span="2">
              {{ server.app.rootPath }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="项目路径" :span="2">
              {{ server.app.webRootPath }}
            </VbenDescriptionsItem>
            <VbenDescriptionsItem label="运行参数" :span="2">
              {{ server.app.name }}
            </VbenDescriptionsItem>
          </VbenDescriptions>
        </Card>
      </Col>

      <Col :span="24">
        <Card size="small">
          <template #title>
            <span>磁盘状态</span>
          </template>
          <Table
            v-if="server?.disk"
            :columns="diskColumns"
            :data-source="server.disk"
            :pagination="false"
            size="small"
            bordered
          >
            <template #bodyCell="{ column, record }">
              <template v-if="column.key === 'availablePercent'">
                <span :class="{ 'text-danger': record.availablePercent > 80 }">
                  {{ record.availablePercent }}%
                </span>
              </template>
            </template>
          </Table>
        </Card>
      </Col>
      </Row>
    </Spin>
  </Page>
</template>

<style scoped>
.text-danger {
  color: #ff4d4f;
}
</style>
