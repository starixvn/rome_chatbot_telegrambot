export interface CustomWindow extends Window {
  Telegram: {
    WebApp: {
      initData: string;
      onEvent: (eventType: string, eventHandler: () => any) => void;
    };
  };
}
