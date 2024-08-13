import { ClientOptions } from '@elastic/elasticsearch';
import * as Sentry from '@sentry/nextjs';

export const TRACKING_TIME_INDEX_NAME = 'rome-telegram-webapp-logs'

const DEFAULTS: ClientOptions = {
  node: process.env.ELK_URL,
};

function getElasticOptions(): ClientOptions {
  return {
    ...DEFAULTS,
    auth: {
      username: process.env.ELK_USERNAME ?? '',
      password: process.env.ELK_PASSWORD ?? '',
    },
  };
}

async function createElasticSearchClient(options: ClientOptions) {
  const { Client } = await import('@elastic/elasticsearch'); 
  return new Client(options);
}

export async function getElasticClient() {
  return await createElasticSearchClient(getElasticOptions());
}

export async function insertElasticDocument(index: string, document: object) {
  const elasticClient = await getElasticClient();

  let result;
  try {
    result = await elasticClient.index({index, document})
  } catch (e) {
    Sentry.captureException(e);
  }

  return result;
}

export async function updateElasticDocument(index: string, id: string, doc: object) {
  const elasticClient = await getElasticClient();

  let result;
  try {
    result = await elasticClient.update({index, id, doc})
  } catch (e) {
    Sentry.captureException(e);
  }

  return result;
}