import * as Sentry from "@sentry/nextjs";
import { NextRouter } from "next/router";

const get = async <T>(url: string, router?: NextRouter): Promise<T> => {
  const res = await fetch(url);
  if (res.status !== 200) {
    router && router.isReady && router.push("/500");
    return {} as T;
  }

  return await res.json();
};

const post = async <T>(url: string, body = {}, router?: NextRouter): Promise<T> => {
  const requestOptions = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body && JSON.stringify(body),
  };

  const res = await fetch(url, requestOptions);
  if (res.status !== 200) {
    router && router.isReady && router.push("/500");
    return {} as T;
  }

  return await res.json();
};

export const fetchWrapper = {
  get,
  post,
};
