import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';

// Public API client for portal (doesn't need JWT in header as it uses Token in URL)
const portalApi = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_BASE_URL + '/portal',
});

export interface PortalContract {
  id: string;
  title: string;
  totalAmount: number;
  terms: string;
  status: string;
  signedAt?: string;
  projectId: string;
  createdAt: string;
}

export function useContractPortal(token: string) {
  const queryClient = useQueryClient();

  const contractQuery = useQuery({
    queryKey: ['portal-contract', token],
    queryFn: async () => {
      const response = await portalApi.get<PortalContract>(`/contracts/${token}`);
      return response.data;
    },
    enabled: !!token,
  });

  const signMutation = useMutation({
    mutationFn: async (digitalSignature: string) => {
      const response = await portalApi.post<PortalContract>(`/contracts/${token}/sign`, {
        digitalSignature,
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portal-contract', token] });
    },
  });

  const viewMutation = useMutation({
    mutationFn: async () => {
      await portalApi.post(`/contracts/${token}/view`);
    },
  });

  return {
    contract: contractQuery.data,
    isLoading: contractQuery.isLoading,
    error: contractQuery.error,
    sign: signMutation.mutateAsync,
    isSigning: signMutation.isPending,
    markViewed: viewMutation.mutateAsync,
  };
}
