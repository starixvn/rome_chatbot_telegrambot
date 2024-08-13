import { createClient } from 'redis';

import * as Sentry from '@sentry/nextjs';
import { createAdapter } from '@socket.io/redis-adapter';

const pubClient = createClient({
    url: `${process.env.REDIS_HOST}:${Number(process.env.REDIS_PORT)}`,
    password: process.env.REDIS_PASSWORD,
});

const subClient = pubClient.duplicate();

export const createCustomAdapter = async  () => {
  let socketAdapter;
  try {
    if(!pubClient.isReady && !subClient.isReady)
    {
      await Promise.all([
        pubClient.connect(),
        subClient.connect()
      ]);
      socketAdapter = createAdapter(pubClient, subClient);
    } 

    socketAdapter = createAdapter(pubClient, subClient);
  } catch (e) {
    socketAdapter = undefined;
    Sentry.captureException(e);
  }

  return socketAdapter;
}