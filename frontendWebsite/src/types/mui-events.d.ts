// Type declarations for MUI event handlers
import { SyntheticEvent } from 'react';

declare module '@mui/material' {
  interface SelectChangeEvent<T = string> {
    target: {
      value: T;
      name?: string;
    };
  }
}

declare global {
  namespace React {
    interface ChangeEvent<T = Element> extends SyntheticEvent<T> {
      target: EventTarget & T;
    }
  }
}

export {};
