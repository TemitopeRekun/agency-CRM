import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';
import { AdPlatform } from './useAdMetrics';

export interface ProjectAdAccount {
  id: string;
  projectId: string;
  platform: AdPlatform;
  externalAccountId: string;
  isActive: boolean;
  tenantId: string;
}

export const useAdAccounts = (projectId: string) => {
  const queryClient = useQueryClient();

  const accountsQuery = useQuery({
    queryKey: ['adAccounts', projectId],
    queryFn: () => api.get<ProjectAdAccount[]>(`/api/projects/${projectId}/adaccounts`),
    enabled: !!projectId,
  });

  const linkAccountMutation = useMutation({
    mutationFn: (newAccount: Partial<ProjectAdAccount>) =>
      api.post<ProjectAdAccount>(`/api/projects/${projectId}/adaccounts`, newAccount),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adAccounts', projectId] });
      queryClient.invalidateQueries({ queryKey: ['adMetrics', projectId] });
    },
  });

  const unlinkAccountMutation = useMutation({
    mutationFn: (id: string) =>
      api.delete(`/api/projects/${projectId}/adaccounts/${id}`),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adAccounts', projectId] });
    },
  });

  const syncMutation = useMutation({
    mutationFn: () =>
      api.post(`/api/projects/${projectId}/adaccounts/sync`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['adMetrics', projectId] });
    },
  });

  return {
    accounts: accountsQuery.data ?? [],
    isLoading: accountsQuery.isLoading,
    linkAccount: linkAccountMutation.mutateAsync,
    isLinking: linkAccountMutation.isPending,
    unlinkAccount: unlinkAccountMutation.mutateAsync,
    isUnlinking: unlinkAccountMutation.isPending,
    sync: syncMutation.mutateAsync,
    isSyncing: syncMutation.isPending,
  };
};
