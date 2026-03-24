import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { api } from '@/lib/api';

export enum ContractStatus {
  Draft,
  Sent,
  Signed,
  Completed,
  Cancelled
}

export interface Contract {
  id: string;
  title: string;
  totalAmount: number;
  status: ContractStatus;
  projectId: string;
  createdAt: string;
}

export const useContracts = () => {
  const queryClient = useQueryClient();

  const contractsQuery = useQuery({
    queryKey: ['contracts'],
    queryFn: () => api.get<Contract[]>('/api/contracts'),
  });

  const createContractMutation = useMutation({
    mutationFn: (newContract: Partial<Contract>) =>
      api.post<Contract>('/api/contracts', newContract),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
    },
  });

  const generateContractMutation = useMutation({
    mutationFn: (projectId: string) =>
      api.post<Contract>(`/api/contracts/generate/${projectId}`, {}),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['contracts'] });
    },
  });

  return {
    contracts: contractsQuery.data ?? [],
    isLoading: contractsQuery.isLoading,
    error: contractsQuery.error,
    createContract: createContractMutation.mutateAsync,
    isCreating: createContractMutation.isPending,
    generateContract: generateContractMutation.mutateAsync,
    isGenerating: generateContractMutation.isPending,
  };
};
