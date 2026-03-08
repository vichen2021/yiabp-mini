import * as signalR from '@microsoft/signalr';

import { useAccessStore } from '@vben/stores';

import { Modal } from 'ant-design-vue';

import { useAuthStore } from '#/store';

let mainHubConnection: signalR.HubConnection | null = null;
let noticeHubConnection: signalR.HubConnection | null = null;

type NoticeCallback = (type: string, title: string, content: string) => void;
let noticeCallback: NoticeCallback | null = null;

export function getMainHubConnection(): signalR.HubConnection | null {
  return mainHubConnection;
}

export function getNoticeHubConnection(): signalR.HubConnection | null {
  return noticeHubConnection;
}

export function onReceiveNotice(callback: NoticeCallback): void {
  noticeCallback = callback;
}

export function offReceiveNotice(): void {
  noticeCallback = null;
}

async function createHubConnection(
  hubPath: string,
  token: string,
): Promise<signalR.HubConnection> {
  const hubBaseUrl = import.meta.env.VITE_GLOB_HUB_URL || '/hub';
  const hubUrl = `${hubBaseUrl}${hubPath}?access_token=${encodeURIComponent(token)}`;

  return new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, {
      skipNegotiation: true,
      transport: signalR.HttpTransportType.WebSockets,
    })
    .withAutomaticReconnect({
      nextRetryDelayInMilliseconds: (retryContext) => {
        if (retryContext.elapsedMilliseconds < 60000) {
          return Math.min(1000 * retryContext.previousRetryCount, 5000);
        }
        return null;
      },
    })
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

export async function startSignalRConnection(): Promise<void> {
  const enableSignalR = import.meta.env.VITE_GLOB_SIGNALR_ENABLE === 'true';
  if (!enableSignalR) {
    console.log('[SignalR] SignalR is disabled');
    return;
  }

  const accessStore = useAccessStore();
  const token = accessStore.accessToken;

  if (!token) {
    console.warn('[SignalR] No token available, skipping connection');
    return;
  }

  if (mainHubConnection || noticeHubConnection) {
    return;
  }

  try {
    mainHubConnection = await createHubConnection('/main', token);

    mainHubConnection.on('forceOut', (message: string) => {
      Modal.warning({
        title: '强制退出',
        content: message,
        onOk: async () => {
          const authStore = useAuthStore();
          await authStore.logout();
        },
      });
    });

    mainHubConnection.on('onlineNum', (count: number) => {
      console.log('[SignalR] Online users:', count);
    });

    mainHubConnection.onclose((error) => {
      console.log('[SignalR] MainHub connection closed', error);
    });

    mainHubConnection.onreconnecting((error) => {
      console.log('[SignalR] MainHub reconnecting...', error);
    });

    mainHubConnection.onreconnected((connectionId) => {
      console.log('[SignalR] MainHub reconnected:', connectionId);
    });

    await mainHubConnection.start();
    console.log('[SignalR] MainHub connected successfully');

    noticeHubConnection = await createHubConnection('/notice', token);

    noticeHubConnection.on('ReceiveNotice', (type: string, title: string, content: string) => {
      console.log('[SignalR] Received notice:', { type, title, content });
      if (noticeCallback) {
        noticeCallback(type, title, content);
      }
    });

    noticeHubConnection.onclose((error) => {
      console.log('[SignalR] NoticeHub connection closed', error);
    });

    noticeHubConnection.onreconnecting((error) => {
      console.log('[SignalR] NoticeHub reconnecting...', error);
    });

    noticeHubConnection.onreconnected((connectionId) => {
      console.log('[SignalR] NoticeHub reconnected:', connectionId);
    });

    await noticeHubConnection.start();
    console.log('[SignalR] NoticeHub connected successfully');
  } catch (error) {
    console.error('[SignalR] Connection error:', error);
    mainHubConnection = null;
    noticeHubConnection = null;
  }
}

export async function stopSignalRConnection(): Promise<void> {
  const connections = [mainHubConnection, noticeHubConnection];

  await Promise.all(
    connections.map(async (connection) => {
      if (connection) {
        try {
          await connection.stop();
          console.log('[SignalR] Disconnected');
        } catch (error) {
          console.error('[SignalR] Error stopping connection:', error);
        }
      }
    }),
  );

  mainHubConnection = null;
  noticeHubConnection = null;
  noticeCallback = null;
}
