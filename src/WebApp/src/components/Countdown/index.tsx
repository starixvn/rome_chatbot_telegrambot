import React, { useEffect, useState } from "react";

type Props = {
  time: number;
  onFinishCount: () => void;
};

export const Countdown = (props: Props) => {
  const [time, setTime] = useState(props.time);

  useEffect(() => {
    const timer = setInterval(() => {
      const newTime = time - 1;
      if (newTime === 0) {
        clearInterval(timer);
        props.onFinishCount();
        return;
      }
      setTime(newTime);
    }, 1000);

    return () => clearInterval(timer);
  });

  return (
    <>
      ({`${Math.floor(time / 60)}`.padStart(2, "0")}:{`${time % 60}`.padStart(2, "0")} )
    </>
  );
};
