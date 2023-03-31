import * as Sentry from "@sentry/nextjs";

const get = async <T>(url: string): Promise<T | any> => {
  try {
    const res = await fetch(url);
    return await res.json();
  } catch (error) {
    Sentry.captureException(error);
  }
};

const post = async <T>(url: string, body?: object): Promise<T | any> => {
  const requestOptions = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body && JSON.stringify(body),
  };

  let statusCode;
  try {
    const res = await fetch(url, requestOptions);
    statusCode = res.status;
    return await res.json();
  } catch (error) {
    statusCode !== 200 && Sentry.captureException(error);
  }
};

export const fetchWrapper = {
  get,
  post,
};
