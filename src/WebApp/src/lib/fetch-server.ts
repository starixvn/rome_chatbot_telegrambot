const get = async (url: string) => {
  const response = await fetch(url);
  let data;
  try {
    data = await response.json();
  } catch {
    data = {};
  }
  return { resStatus: response.status, data: data };
};

const post = async (url: string, body = {}) => {
  const requestOptions = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body && JSON.stringify(body),
  };

  const response = await fetch(url, requestOptions);
  let data;
  try {
    data = await response.json();
  } catch {
    data = {};
  }

  return { resStatus: response.status, data };
};

export const fetchServerWrapper = {
  get,
  post,
};
