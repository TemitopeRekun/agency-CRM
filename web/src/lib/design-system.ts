export const designSystem = {
  colors: {
    primary: {
      main: '#4f46e5', // Indigo 600
      hover: '#4338ca', // Indigo 700
      text: '#ffffff',
    },
    secondary: {
      main: '#64748b', // Slate 500
      hover: '#475569', // Slate 600
      text: '#ffffff',
    },
    background: {
      default: '#f8fafc', // Slate 50
      paper: '#ffffff',
    },
    text: {
      primary: '#0f172a', // Slate 900
      secondary: '#64748b', // Slate 500
      muted: '#94a3b8', // Slate 400
    },
    border: '#e2e8f0', // Slate 200
    status: {
      success: '#10b981', // Emerald 500
      error: '#ef4444', // Red 500
      warning: '#f59e0b', // Amber 500
      info: '#3b82f6', // Blue 500
    }
  },
  typography: {
    fontFamily: {
      sans: 'var(--font-geist-sans), sans-serif',
      mono: 'var(--font-geist-mono), monospace',
    },
    sizes: {
      xs: '0.75rem',
      sm: '0.875rem',
      base: '1rem',
      lg: '1.125rem',
      xl: '1.25rem',
      '2xl': '1.5rem',
      '3xl': '1.875rem',
    }
  },
  spacing: {
    xxs: '0.25rem',
    xs: '0.5rem',
    sm: '0.75rem',
    md: '1rem',
    lg: '1.5rem',
    xl: '2rem',
    xxl: '3rem',
  },
  borderRadius: {
    sm: '0.25rem',
    md: '0.5rem',
    lg: '0.75rem',
    full: '9999px',
  },
  shadows: {
    sm: '0 1px 2px 0 rgb(0 0 0 / 0.05)',
    md: '0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1)',
    lg: '0 10px 15px -3px rgb(0 0 0 / 0.1), 0 4px 6px -4px rgb(0 0 0 / 0.1)',
  }
};
