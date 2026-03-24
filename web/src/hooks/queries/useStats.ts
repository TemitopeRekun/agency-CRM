import { useQuery } from '@tanstack/react-query';
import { api } from '@/lib/api';

export const useStats = () => {
  return useQuery({
    queryKey: ['stats'],
    queryFn: async () => {
      const [clients, leads, offers, projects] = await Promise.all([
        api.get<any[]>('/api/clients'),
        api.get<any[]>('/api/leads'),
        api.get<any[]>('/api/offers'),
        api.get<any[]>('/api/projects'),
      ]);
      return {
        clients: clients.length,
        leads: leads.length,
        offers: offers.length,
        projects: projects.length,
      };
    },
  });
};
