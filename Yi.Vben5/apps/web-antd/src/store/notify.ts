import type { NotificationItem } from '@vben/layouts';

import { computed, onUnmounted, ref } from 'vue';

import { SvgMessageUrl } from '@vben/icons';
import { $t } from '@vben/locales';
import { useUserStore } from '@vben/stores';

import { notification } from 'ant-design-vue';
import dayjs from 'dayjs';
import { defineStore } from 'pinia';

import { offReceiveNotice, onReceiveNotice } from '#/utils/signalr';

export const useNotifyStore = defineStore(
  'app-notify',
  () => {
    const notificationList = ref<NotificationItem[]>([]);

    const userStore = useUserStore();
    const userId = computed(() => {
      return userStore.userInfo?.userId || '0';
    });

    const notifications = computed(() => {
      return notificationList.value.filter(
        (item) => item.userId === userId.value,
      );
    });

    function startListeningMessage() {
      onReceiveNotice((type: string, title: string, content: string) => {
        console.log(`[Notify] 接收到消息: type=${type}, title=${title}, content=${content}`);

        notification.success({
          description: content,
          duration: 3,
          message: title || $t('component.notice.received'),
        });

        notificationList.value.unshift({
          avatar: SvgMessageUrl,
          date: dayjs().format('YYYY-MM-DD HH:mm:ss'),
          isRead: false,
          message: content,
          title: title || $t('component.notice.title'),
          userId: userId.value,
        });
      });
    }

    function stopListeningMessage() {
      offReceiveNotice();
    }

    function setAllRead() {
      notificationList.value
        .filter((item) => item.userId === userId.value)
        .forEach((item) => {
          item.isRead = true;
        });
    }

    function setRead(item: NotificationItem) {
      !item.isRead && (item.isRead = true);
    }

    function clearAllMessage() {
      notificationList.value = notificationList.value.filter(
        (item) => item.userId !== userId.value,
      );
    }

    function $reset() {
    }

    const showDot = computed(() =>
      notificationList.value
        .filter((item) => item.userId === userId.value)
        .some((item) => !item.isRead),
    );

    return {
      $reset,
      clearAllMessage,
      notificationList,
      notifications,
      setAllRead,
      setRead,
      showDot,
      startListeningMessage,
      stopListeningMessage,
    };
  },
  {
    persist: {
      pick: ['notificationList'],
    },
  },
);
