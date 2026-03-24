'use client';

import { createContext, useContext, useEffect, useState } from 'react';
import { useRouter } from 'next/navigation';
import { api } from '@/lib/api';

interface User {
  email: string;
  fullName: string;
}

interface AuthContextType {
  user: User | null;
  loading: boolean;
  login: (email: string, password: string) => Promise<void>;
  logout: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);
  const router = useRouter();

  useEffect(() => {
    // Initial check - we can't see the cookie, but we can try to fetch the dashboard or a dedicated /me endpoint
    // For now, if we get a 401 on any initial fetch, we know we are logged out.
    setLoading(false);
  }, []);

  const login = async (email: string, password: string) => {
    const data = await api.post<User>('/api/auth/login', { email, password });
    setUser(data); // data now only contains email and fullName
    router.push('/dashboard');
  };


  const logout = async () => {
    await api.post('/api/auth/logout', {});
    setUser(null);
    router.push('/login');
  };

  return (
    <AuthContext.Provider value={{ user, loading, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};
