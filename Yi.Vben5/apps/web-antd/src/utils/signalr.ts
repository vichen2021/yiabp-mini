import * as signalR from '@microsoft/signalr';

import { useAccessStore } from '@vben/stores';

import { Modal } from 'ant-design-vue';

import { useAuthStore } from '#/store';

let connection: signalR.HubConnection | null = null;

export function getSignalRConnection(): signalR.HubConnection | null {
  return connection;
}

export async function startSignalRConnection(): Promise<void> {
  if (connection) {
    return;
  }

  const accessStore = useAccessStore();
  const token = accessStore.accessToken;

  if (!token) {
    console.warn('[SignalR] No token available, skipping connection');
    return;
  }

  const hubBaseUrl = import.meta.env.VITE_GLOB_HUB_URL || '/hub';
  const hubUrl = `${hubBaseUrl}/main?access_token=${encodeURIComponent(token)}`;

  connection = new signalR.HubConnectionBuilder()
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

  connection.on('forceOut', (message: string) => {
    Modal.warning({
      title: '强制退出',
      content: message,
      onOk: async () => {
        const authStore = useAuthStore();
        await authStore.logout();
      },
    });
  });

  connection.on('onlineNum', (count: number) => {
    console.log('[SignalR] Online users:', count);
  });

  connection.onclose((error) => {
    console.log('[SignalR] Connection closed', error);
  });

  connection.onreconnecting((error) => {
    console.log('[SignalR] Reconnecting...', error);
  });

  connection.onreconnected((connectionId) => {
    console.log('[SignalR] Reconnected:', connectionId);
  });

  try {
    await connection.start();
    console.log('[SignalR] Connected successfully');
  } catch (error) {
    console.error('[SignalR] Connection error:', error);
    connection = null;
  }
}

export async function stopSignalRConnection(): Promise<void> {
  if (connection) {
    try {
      await connection.stop();
      console.log('[SignalR] Disconnected');
    } catch (error) {
      console.error('[SignalR] Error stopping connection:', error);
    } finally {
      connection = null;
    }
  }
}
