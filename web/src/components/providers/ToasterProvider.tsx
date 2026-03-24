"use client";

import { Toaster as SonnerToaster } from "sonner";

export function ToasterProvider() {
  return (
    <SonnerToaster 
      position="top-right"
      expand={true}
      richColors
      closeButton
    />
  );
}
