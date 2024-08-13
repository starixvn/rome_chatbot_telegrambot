FROM hub.nexdev.net/base/node:20.9.0-alpine AS deps
RUN apk add --no-cache libc6-compat
WORKDIR /app

# Install dependencies based on the preferred package manager
COPY src/WebApp/package.json ./
COPY src/WebApp/yarn.lock ./
RUN --mount=type=cache,target=/root/.yarn yarn --frozen-lockfile

# 2. Rebuild the source code only when needed
FROM hub.nexdev.net/base/node:20.9.0-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY src/WebApp .

# Register TRANSFORM_CONFIG argument for transforming by environment
ARG TRANSFORM_CONFIG
RUN echo $TRANSFORM_CONFIG

# Next.js collects completely anonymous telemetry data about general usage.
# Learn more here: https://nextjs.org/telemetry
# Uncomment the following line in case you want to disable telemetry during the build.
# ENV NEXT_TELEMETRY_DISABLED 1
COPY "src/WebApp/.env.$TRANSFORM_CONFIG" .env.production
RUN yarn build

# If using npm comment out above and use below instead
# RUN npm run build

# Production image, copy all the files and run next
FROM hub.nexdev.net/base/node:20.9.0-alpine AS runner
WORKDIR /app

ENV NODE_ENV production
# Uncomment the following line in case you want to disable telemetry during runtime.
# ENV NEXT_TELEMETRY_DISABLED 1

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public

# Automatically leverage output traces to reduce image size
# https://nextjs.org/docs/advanced-features/output-file-tracing
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 5000

ENV PORT 5000

CMD ["node", "server.js"]