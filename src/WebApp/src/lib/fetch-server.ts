const get = async (url: string) => {
  const response = await fetch(url);
  return await response.json();
};

const post = async (url: string, body = {}) => {
  const requestOptions = {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body && JSON.stringify(body),
  };

  const response = await fetch(url, requestOptions);
  let data;
  // try catch for success post request but not return data
  try {
    data = await response.json();
  } catch {
    data = {};
  }
  return data;
};

export const fetchServerWrapper = {
  get,
  post,
};
