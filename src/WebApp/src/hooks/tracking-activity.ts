import { useEffect } from 'react';
import { io, Socket } from 'socket.io-client';

import * as Sentry from '@sentry/nextjs';

export type TimeTracking = {
  url: string,
  parentUrl: string,
  timeStart: number
}

let socket: Socket;

export function useTracking(parentUrl?: string) {
  useEffect(() => {
    const initSocket = async () => {
      try { 
        const result = await fetch('/api/socket');
        const isOk = (await result.json()).ok;

        if(isOk) {
          socket = io({ autoConnect: false, retries: 2});
          socket.connect();
        }

        socket.on("connect", () => {
            socket.emit("timeTracking", {
              url: document.location.pathname,
              parentUrl: parentUrl ?? "",
              timeStart: Date.now(),
          })
        });
      } catch (e) {
        Sentry.captureException(e);
      }
    }

    if(process.env.NEXT_PUBLIC_TRACKING_TIME_URLS?.includes(document.location.pathname)) {
      initSocket();
    }

    return () => {
      socket?.disconnect();
    }
  }, [])
}