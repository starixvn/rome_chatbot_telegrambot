import { withIronSessionApiRoute } from 'iron-session/next';
import { NextApiRequest } from 'next';
import { Server } from 'socket.io';
import { getClientIP } from 'src/helpers/ip-helper';
import { TimeTracking } from 'src/hooks/tracking-activity';
import { createCustomAdapter } from 'src/lib/socket-adapter';

import * as Sentry from '@sentry/nextjs';

import { insertElasticDocument, TRACKING_TIME_INDEX_NAME } from '../../lib/elastic';
import { sessionOptions } from '../../lib/session';

export default withIronSessionApiRoute(ioHandler, sessionOptions)

async function ioHandler(req: NextApiRequest, res: any) {
  const { user } = req.session;
  const adapter = await createCustomAdapter();

  if(adapter === undefined) {
    return res.json({ok: false});
  }
  
  if (user?.isLoggedIn && !res?.socket?.server.io) {
    try {
      const httpServer = res.socket.server;
      const io = new Server(httpServer, { adapter });
  
      io.on('connection', async (socket) => {
        socket.on("timeTracking", async (payload) => {
          socket.data.timeTracking = payload;
        })
  
        socket.on('disconnect', async () => {
          const timeTracking = socket.data?.timeTracking as TimeTracking;
          if(timeTracking.timeStart > 0) {
            const currentDate = new Date();
            const dateStr = `${currentDate.getFullYear()}.${currentDate.getMonth() + 1}`
            await insertElasticDocument(`${TRACKING_TIME_INDEX_NAME}-${dateStr}`, {
              url: timeTracking.url,
              parentUrl: timeTracking.parentUrl,
              userId: user.tgChatId,
              site: user.site,
              ip: getClientIP(req),
              usedTime: Date.now() - timeTracking.timeStart,
              '@timestamp': currentDate.toISOString()
            })
          }
        });
      });
  
      res.socket.server.io = io;
    } catch (e) {
      Sentry.captureException(e);
    }
  }

  res.json({ok: true});
}