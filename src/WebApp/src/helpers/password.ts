export const validatePassword = (password: string) => {
  const whiteSpace = /^\S*$/;
  const whiteSpaceCase = password.search(whiteSpace) != -1;

  const regex1 = /^(?=.{8,100}$).*\d/i;
  const regex2 = /^(?=.{8,100}$).*[a-z]/;
  const regex3 = /^(?=.{8,100}$).*[A-Z]/;
  const regex4 = /^(?=.{8,100}$).*[\[\]\^\$\.\|\?\*\+\(\)\\~`\!@#%&\-_+={}'""<>:;,]/i;

  const case1 = password.search(regex1) != -1 ? 1 : 0;
  const case2 = password.search(regex2) != -1 ? 1 : 0;
  const case3 = password.search(regex3) != -1 ? 1 : 0;
  const case4 = password.search(regex4) != -1 ? 1 : 0;

  return case1 + case2 + case3 + case4 >= 3 && whiteSpaceCase;
};

export const validateMemberPassword = (password: string) => {
  const regex = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z]).{6,10}$/;
  const whiteSpace = /^\S*$/;

  const case5 = password.search(whiteSpace) !== -1;

  return password.search(regex) !== -1 && case5;
};
