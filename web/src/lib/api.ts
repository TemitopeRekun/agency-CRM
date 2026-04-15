import { signals } from './signals';

export class ApiError extends Error {
  constructor(public readonly status: number, message: string) {
    super(message);
    this.name = 'ApiError';
  }
}

export function isApiError(err: unknown): err is ApiError {
  return err instanceof ApiError;
}

const rawBase = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:8000';

// In the browser, always use relative paths (/api/...) so requests flow through
// the Next.js rewrite in next.config.ts, which proxies to the backend server-side.
// This keeps cookies on the app's own domain (agency-ccrm.netlify.app) where
// proxy.ts can read them for server-side route protection.
// In SSR context (no window), use the absolute URL for direct backend access.
const API_BASE_URL = typeof window !== 'undefined' ? '' : rawBase.replace(/\/$/, '');

export async function apiRequest<T>(
  endpoint: string,
  options: RequestInit = {},
): Promise<T> {
  const headers: HeadersInit = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  };

  const fetchOptions: RequestInit = {
    ...options,
    headers: {
      ...headers,
      'Authorization': typeof window !== 'undefined' ? `Bearer ${localStorage.getItem('access_token')}` : '',
    },
    credentials: 'include',
  };

  const normalizedEndpoint = endpoint.startsWith('/') ? endpoint : `/${endpoint}`;
  let response = await fetch(`${API_BASE_URL}${normalizedEndpoint}`, fetchOptions);

  // --- 401 Auto-refresh ---
  if (
    response.status === 401 &&
    !endpoint.includes('/auth/login') &&
    !endpoint.includes('/auth/refresh')
  ) {
    try {
      const refreshResponse = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (refreshResponse.ok) {
        // Persist the new access token so the retried request sends a fresh Bearer header.
        const refreshData = await refreshResponse.json().catch(() => ({}));
        if ((refreshData as { accessToken?: string }).accessToken) {
          localStorage.setItem('access_token', (refreshData as { accessToken: string }).accessToken);
        }
        // Rebuild fetch options with the updated token.
        const retryOptions: RequestInit = {
          ...fetchOptions,
          headers: {
            ...(fetchOptions.headers as Record<string, string>),
            Authorization: `Bearer ${localStorage.getItem('access_token')}`,
          },
        };
        response = await fetch(`${API_BASE_URL}${normalizedEndpoint}`, retryOptions);
      } else {
        // Refresh token failed - fallback to signaling an expired session
        localStorage.removeItem('access_token');
        throw new Error('Session expired');
      }
    } catch (err) {
      if (err instanceof Error && err.message === 'Session expired. Please log in again.') {
        throw err;
      }
      console.error('Auto-refresh failed', err);
    }
  }

  // --- Error handling with user-visible states ---
  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    const message =
      (errorData as { message?: string; Message?: string }).message ||
      (errorData as { message?: string; Message?: string }).Message ||
      `API error: ${response.status}`;

    // --- Global Signals for Failsafe UI ---
    if (response.status === 401 && !endpoint.includes('/auth/login')) {
      signals.emit('401', message);
    } else if (response.status === 403) {
      signals.emit('403', message);
    } else if (response.status >= 500) {
      signals.emit('500', 'A server error occurred.');
    }

    // Show a toast only for actionable client errors (e.g. 400 Bad Request,
    // 409 Conflict, 422 Unprocessable). Skip 401/403 (handled by signals),
    // and 404 (handled by each component's own empty/error state — no toast
    // needed, the resource simply doesn't exist).
    const shouldToast =
      response.status >= 400 &&
      response.status < 500 &&
      response.status !== 401 &&
      response.status !== 403 &&
      response.status !== 404;

    if (shouldToast) {
      const { toast } = await import('sonner');
      toast.error(message);
    }

    throw new ApiError(response.status, message);
  }

  // --- Guard 204 No Content / 205 Reset Content (no body to parse) ---
  if (response.status === 204 || response.status === 205) {
    return null as unknown as T;
  }

  return response.json() as Promise<T>;
}

export const api = {
  get: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body?: unknown) =>
    apiRequest<T>(endpoint, {
      method: 'POST',
      body: JSON.stringify(body ?? {}),
    }),
  patch: <T>(endpoint: string, body: unknown) =>
    apiRequest<T>(endpoint, {
      method: 'PATCH',
      body: JSON.stringify(body),
    }),
  put: <T>(endpoint: string, body: unknown) =>
    apiRequest<T>(endpoint, {
      method: 'PUT',
      body: JSON.stringify(body),
    }),
  delete: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'DELETE' }),
};
