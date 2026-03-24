const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:8000';

export async function apiRequest<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
  const headers = {
    'Content-Type': 'application/json',
    ...options.headers,
  };

  const fetchOptions: RequestInit = {
    ...options,
    headers,
    credentials: 'include',
  };

  let response = await fetch(`${API_BASE_URL}${endpoint}`, fetchOptions);

  if (response.status === 401 && !endpoint.includes('/auth/login') && !endpoint.includes('/auth/refresh')) {
    try {
      const refreshResponse = await fetch(`${API_BASE_URL}/api/auth/refresh`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        credentials: 'include',
      });

      if (refreshResponse.ok) {
        response = await fetch(`${API_BASE_URL}${endpoint}`, fetchOptions);
      }
    } catch (err) {
      console.error('Auto-refresh failed', err);
    }
  }

  if (!response.ok) {
    const errorData = await response.json().catch(() => ({}));
    const message = errorData.message || errorData.Message || `API error: ${response.status}`;
    
    // Import toast dynamically to avoid server-side issues if api.ts is used in SSR
    const { toast } = await import("sonner");
    
    if (response.status >= 400 && response.status < 500) {
      toast.error(message);
    } else if (response.status >= 500) {
      toast.error("A server error occurred. Please try again later.");
    }

    throw new Error(message);
  }


  return response.json();
}

export const api = {
  get: <T>(endpoint: string) => apiRequest<T>(endpoint, { method: 'GET' }),
  post: <T>(endpoint: string, body: any) => 
    apiRequest<T>(endpoint, { 
      method: 'POST', 
      body: JSON.stringify(body) 
    }),
  patch: <T>(endpoint: string, body: any) => 
    apiRequest<T>(endpoint, { 
      method: 'PATCH', 
      body: JSON.stringify(body) 
    }),
  put: <T>(endpoint: string, body: any) => 
    apiRequest<T>(endpoint, { 
      method: 'PUT', 
      body: JSON.stringify(body) 
    }),
};


