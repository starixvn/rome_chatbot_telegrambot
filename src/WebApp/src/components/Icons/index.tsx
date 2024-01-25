type IconProps = {
  width?: number;
  height?: number;
  fill?: number;
};

export const CheckMarkIcon = ({ width, height }: IconProps) => {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" width={width} height={height} fill="currentColor" viewBox="0 0 16 16">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M1.33398 8.00016C1.33398 4.32016 4.32065 1.3335 8.00065 1.3335C11.6807 1.3335 14.6673 4.32016 14.6673 8.00016C14.6673 11.6802 11.6807 14.6668 8.00065 14.6668C4.32065 14.6668 1.33398 11.6802 1.33398 8.00016ZM3.33398 8.00016L6.66732 11.3335L12.6673 5.3335L11.7273 4.38683L6.66732 9.44683L4.27398 7.06016L3.33398 8.00016Z"
        fill="#18965B"
      />
    </svg>
  );
};

export const InformationIcon = ({ width, height }: IconProps) => {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" width={width} height={height} fill="currentColor" viewBox="0 0 16 16">
      <path
        d="M6.33398 5H7.66748V3.6665H6.33398V5ZM7.00048 12.3335C4.06048 12.3335 1.66698 9.94 1.66698 7C1.66698 4.06 4.06048 1.6665 7.00048 1.6665C9.94049 1.6665 12.334 4.06 12.334 7C12.334 9.94 9.94049 12.3335 7.00048 12.3335ZM7.00048 0.333496C3.31848 0.333496 0.333984 3.3185 0.333984 7C0.333984 10.6815 3.31898 13.6665 7.00048 13.6665C10.6825 13.6665 13.667 10.6815 13.667 7C13.667 3.3185 10.682 0.333496 7.00048 0.333496ZM6.33398 10.3335H7.66748V6.3335H6.33398V10.3335Z"
        fill="#0B599C"
      />
    </svg>
  );
};

export const EyeIcon = ({ width = 16, height = 16 }: IconProps) => {
  return (
    <svg width={width} height={height} viewBox="0 0 16 16" fill="#444444" xmlns="http://www.w3.org/2000/svg">
      <g clipPath="url(#clip0_2466_18121)">
        <path
          d="M8.00065 3.3335C4.97035 3.3335 2.38247 5.26861 1.33398 8.00016C2.38247 10.7317 4.97035 12.6668 8.00065 12.6668C11.031 12.6668 13.6188 10.7317 14.6673 8.00016C13.6188 5.26861 11.031 3.3335 8.00065 3.3335ZM8.00065 11.1113C6.32792 11.1113 4.97035 9.7175 4.97035 8.00016C4.97035 6.28283 6.32792 4.88905 8.00065 4.88905C9.67338 4.88905 11.031 6.28283 11.031 8.00016C11.031 9.7175 9.67338 11.1113 8.00065 11.1113ZM8.00065 6.1335C6.99459 6.1335 6.18247 6.96727 6.18247 8.00016C6.18247 9.03305 6.99459 9.86683 8.00065 9.86683C9.00671 9.86683 9.81883 9.03305 9.81883 8.00016C9.81883 6.96727 9.00671 6.1335 8.00065 6.1335Z"
          fill="#727272"
        />
      </g>
      <defs>
        <clipPath id="clip0_2466_18121">
          <rect width="16" height="16" fill="white" />
        </clipPath>
      </defs>
    </svg>
  );
};

export const ErrorIcon = ({ width = 20, height = 20 }: IconProps) => {
  return (
    <svg width={width} height={height} viewBox="0 0 18 18" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M0.666016 8.99984C0.666016 4.3915 4.39102 0.666504 8.99935 0.666504C13.6077 0.666504 17.3327 4.3915 17.3327 8.99984C17.3327 13.6082 13.6077 17.3332 8.99935 17.3332C4.39102 17.3332 0.666016 13.6082 0.666016 8.99984ZM11.991 13.1665L13.166 11.9915L10.1743 8.99984L13.166 6.00817L11.991 4.83317L8.99935 7.82484L6.00768 4.83317L4.83268 6.00817L7.82435 8.99984L4.83268 11.9915L6.00768 13.1665L8.99935 10.1748L11.991 13.1665Z"
        fill="#FE0000"
      />
    </svg>
  );
};

export const EyeCloseIcon = ({ width = 16, height = 16 }: IconProps) => {
  return (
    <svg width={width} height={height} viewBox="0 0 24 24" fill="#444444" xmlns="http://www.w3.org/2000/svg">
      <g clipPath="url(#clip0_4381_26896)">
        <path
          d="M12 7C14.76 7 17 9.24 17 12C17 12.65 16.87 13.26 16.64 13.83L19.56 16.75C21.07 15.49 22.26 13.86 22.99 12C21.26 7.61 16.99 4.5 11.99 4.5C10.59 4.5 9.25 4.75 8.01 5.2L10.17 7.36C10.74 7.13 11.35 7 12 7ZM2 4.27L4.28 6.55L4.74 7.01C3.08 8.3 1.78 10.02 1 12C2.73 16.39 7 19.5 12 19.5C13.55 19.5 15.03 19.2 16.38 18.66L16.8 19.08L19.73 22L21 20.73L3.27 3L2 4.27ZM7.53 9.8L9.08 11.35C9.03 11.56 9 11.78 9 12C9 13.66 10.34 15 12 15C12.22 15 12.44 14.97 12.65 14.92L14.2 16.47C13.53 16.8 12.79 17 12 17C9.24 17 7 14.76 7 12C7 11.21 7.2 10.47 7.53 9.8ZM11.84 9.02L14.99 12.17L15.01 12.01C15.01 10.35 13.67 9.01 12.01 9.01L11.84 9.02Z"
          fill="#727272"
        />
      </g>
      <defs>
        <clipPath id="clip0_4381_26896">
          <rect width="24" height="24" fill="white" />
        </clipPath>
      </defs>
    </svg>
  );
};

export const CopyICon = ({ width = 16, height = 16 }: IconProps) => {
  return (
    <svg width={width} height={height} viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M16 1H4C2.9 1 2 1.9 2 3V17H4V3H16V1ZM19 5H8C6.9 5 6 5.9 6 7V21C6 22.1 6.9 23 8 23H19C20.1 23 21 22.1 21 21V7C21 5.9 20.1 5 19 5ZM8 21H19V7H8V21Z"
        fill="#0B599C"
      />
    </svg>
  );
};

export const CopiedICon = ({ width = 16, height = 16 }: IconProps) => {
  return (
    <svg width={width} height={height} viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
      <path
        fillRule="evenodd"
        clipRule="evenodd"
        d="M16 1H4C2.9 1 2 1.9 2 3V17H4V3H16V1ZM19 5H8C6.9 5 6 5.9 6 7V21C6 22.1 6.9 23 8 23H12V21H8V7H19V12H21V7C21 5.9 20.1 5 19 5ZM19 14C16.24 14 14 16.24 14 19C14 21.76 16.24 24 19 24C21.76 24 24 21.76 24 19C24 16.24 21.76 14 19 14ZM18 21.5L15.5 19L16.205 18.295L18 20.085L21.795 16.29L22.5 17L18 21.5Z"
        fill="#0B599C"
      />
    </svg>
  );
};

export const FirstPageIcon = ({ width, height }: IconProps) => {
  return (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="20" height="20" rx="3" fill="#DDDDDD" />
      <path d="M6 14.0938V6.09375H7.3335V14.0938H6ZM8.3335 10.0938L14 6.09375V14.0938L8.3335 10.0938Z" fill="#333333" />
    </svg>
  );
};

export const PrevPageIcon = ({ width, height }: IconProps) => {
  return (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="20" height="20" rx="3" fill="#DDDDDD" />
      <path d="M9.33333 10L12 7.21739L10.8333 6L7 10L10.8333 14L12 12.7826L9.33333 10Z" fill="#333333" />
    </svg>
  );
};

export const NextPageIcon = ({ width, height }: IconProps) => {
  return (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="20" height="20" rx="3" fill="#DDDDDD" />
      <path d="M10.6667 10L8 12.7826L9.16667 14L13 10L9.16667 6L8 7.21739L10.6667 10Z" fill="#333333" />
    </svg>
  );
};

export const LastPageIcon = ({ width, height }: IconProps) => {
  return (
    <svg width="20" height="20" viewBox="0 0 20 20" fill="none" xmlns="http://www.w3.org/2000/svg">
      <rect width="20" height="20" rx="3" fill="#DDDDDD" />
      <path d="M12.6665 14.0938H14V6.09375H12.6665V14.0938ZM6 14.0938L11.6665 10.0938L6 6.09375V14.0938V14.0938Z" fill="#333333" />
    </svg>
  );
};

export const CalendarIcon = ({ width, height }: IconProps) => {
  return (
    <svg xmlns="http://www.w3.org/2000/svg" width={width} height={height} fill="currentColor" viewBox="0 0 1024 1024">
      <path
        d="M810.666667 810.666667 213.333333 810.666667 213.333333 341.333333 810.666667 341.333333M682.666667 42.666667 682.666667 128 341.333333 128 341.333333 42.666667 256 42.666667 256 128 213.333333 128C165.973333 128 128 165.973333 128 213.333333L128 810.666667C128 857.6 166.4 896 213.333333 896L810.666667 896C857.6 896 896 857.6 896 810.666667L896 213.333333C896 165.973333 857.6 128 810.666667 128L768 128 768 42.666667M725.333333 512 512 512 512 725.333333 725.333333 725.333333 725.333333 512Z"
        fill="#000000"
      />
    </svg>
  );
};
