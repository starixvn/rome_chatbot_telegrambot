export const Member = 1;
export const Agent = 2;
export const Master = 3;
export const Super = 4;

export const isDirectDownLine = (downLineRole: number, upLineRole: number): boolean => {
  if (downLineRole > Super || downLineRole < Member || upLineRole > Super || upLineRole < Agent) {
    return false;
  }

  return upLineRole - downLineRole === 1;
};
