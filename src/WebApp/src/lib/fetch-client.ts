import * as Sentry from "@sentry/nextjs";
import { NextRouter } from "next/router";

const get = async <T>(url: string, router?: NextRouter): Promise<T> => {
  const res = await fetch(url);
  let data;
  try {
    data = await res.json();
  } catch {
    data = {} as T;
  }

  if (res.status !== 200 && router) {
    handleRedirect(router, res.status, data);
    return {} as T;
  }

  return data;
};

const post = async <T>(url: string, body = {}, router?: NextRouter): Promise<T> => {
  const requestOptions = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body && JSON.stringify(body),
  };

  const res = await fetch(url, requestOptions);
  let data;
  try {
    data = await res.json();
  } catch {
    data = {} as T;
  }
  if (res.status !== 200 && router) {
    handleRedirect(router, res.status, data);
    return {} as T;
  }

  return data;
};

const handleRedirect = (router: NextRouter, resStatus: number, data: any) => {
  if (resStatus === 500) {
    redirectToUrl(router, "/500");
  }
  if (resStatus === 503) {
    redirectToUrl(router, `/um?isSpecialSite=${data.isSpecialSite}&from=${data.umInfo.from}&to=${data.umInfo.to}`);
  }
  if (resStatus === 401) {
    redirectToUrl(router, "/unauthorized");
  }
  if (resStatus === 403) {
    redirectToUrl(router, `/kickout?isActive=${data.isActive}&isProtectedMode=${data.isProtectedMode}&isLocked=${data.isLocked}`);
  }
};

const redirectToUrl = (router: NextRouter, url: string) => {
  router && router.isReady && router.push(url);
};

export const fetchWrapper = {
  get,
  post,
};
